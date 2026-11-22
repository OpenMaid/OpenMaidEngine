using System.Text.Json;

namespace Ome.Export;

/// <summary>
/// A game's entity declarations as <c>tools/entities_build.py</c> writes them, <c>build/games/&lt;game&gt;/entities.json</c>
/// (game data export spec §5). The builder has linted every name against the registry; the exporter checks again only
/// what it needs to run.
/// </summary>
public sealed record ExportDeclarations(
    string Profile,
    string SourceSha256,
    string BootScript,
    string ReadAt,
    IReadOnlyList<string> Builders,
    IReadOnlyList<string> ExcludedBuilders,
    IReadOnlyList<string> TextScripts,
    IReadOnlyList<KindDeclaration> Kinds)
{
    /// <summary>The pictures the site shows blurred until a click, by catalog name without the <c>$n$</c> prefix and the
    /// extension, a <c>*</c> matching any run of characters (<c>[export] explicit_pictures</c>).</summary>
    public IReadOnlyList<string> ExplicitPictures { get; init; } = [];

    /// <summary>The loaders (spec §6.3, export plan §4.2): the scripts the export runs once for each key entity, and the
    /// tables of their scope it publishes under that entity.</summary>
    public IReadOnlyList<LoaderDeclaration> Loaders { get; init; } = [];

    /// <summary>The script relations (spec §6.7, export plan §4.5): the calls of a script whose arguments name an
    /// entity.</summary>
    public IReadOnlyList<RelationDeclaration> Relations { get; init; } = [];

    public static ExportDeclarations Load(string path) => Parse(File.ReadAllText(path));

    public static ExportDeclarations Parse(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement export = root.GetProperty("export");
        JsonElement boot = export.GetProperty("boot");
        return new ExportDeclarations(
            root.GetProperty("profile").GetString()!,
            root.GetProperty("source_sha256").GetString()!,
            boot.GetProperty("script").GetString()!,
            boot.GetProperty("read_at").GetString()!,
            Strings(export, "builders"),
            Strings(export, "excluded_builders"),
            Strings(export, "text_scripts"),
            root.GetProperty("kinds").EnumerateArray().Select(KindDeclaration.Parse).ToList())
        {
            ExplicitPictures = Strings(export, "explicit_pictures"),
            Loaders = root.TryGetProperty("loaders", out JsonElement loaders)
                ? loaders.EnumerateArray().Select(LoaderDeclaration.Parse).ToList()
                : [],
            Relations = root.TryGetProperty("script_relations", out JsonElement relations)
                ? relations.EnumerateArray().Select(RelationDeclaration.Parse).ToList()
                : [],
        };
    }

    internal static IReadOnlyList<string> Strings(JsonElement parent, string name)
        => parent.TryGetProperty(name, out JsonElement list)
            ? list.EnumerateArray().Select(item => item.GetString()!).ToList()
            : [];
}

/// <summary>One kind (spec §5.2): a registry table, which of its records are entities, and what is published. Its
/// <see cref="Groups"/> (export plan §5) are published rows of cells, which the site shows as one table: the narrowest
/// field's width is its number of rows, and each field's width a multiple of it.</summary>
public sealed record KindDeclaration(
    string Id,
    IReadOnlyList<string> Aliases,
    string Table,
    string Exists,
    IReadOnlyList<(string Field, long Value)> Skip,
    string? Title,
    IReadOnlyList<string> Text,
    IReadOnlyList<string> Fields,
    IReadOnlyList<string> Filters,
    IReadOnlyList<(string Name, string Field)> Pictures,
    IReadOnlyList<DescriptionDeclaration> Descriptions)
{
    public IReadOnlyList<GroupDeclaration> Groups { get; init; } = [];

    internal static KindDeclaration Parse(JsonElement kind)
        => new(
            kind.GetProperty("id").GetString()!,
            ExportDeclarations.Strings(kind, "aliases"),
            kind.GetProperty("table").GetString()!,
            kind.GetProperty("exists").GetString()!,
            kind.TryGetProperty("skip", out JsonElement skip)
                ? skip.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetInt64())).ToList()
                : [],
            kind.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String
                ? title.GetString()
                : null,
            ExportDeclarations.Strings(kind, "text"),
            ExportDeclarations.Strings(kind, "fields"),
            ExportDeclarations.Strings(kind, "filters"),
            kind.TryGetProperty("pictures", out JsonElement pictures)
                ? pictures.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetString()!)).ToList()
                : [],
            kind.TryGetProperty("descriptions", out JsonElement descriptions)
                ? descriptions.EnumerateArray().Select(DescriptionDeclaration.Parse).ToList()
                : [])
        {
            Groups = kind.TryGetProperty("groups", out JsonElement groups)
                ? groups.EnumerateArray().Select(group => new GroupDeclaration(
                    group.GetProperty("name").GetString()!, ExportDeclarations.Strings(group, "fields"))).ToList()
                : [],
        };
}

/// <summary>A group of a kind's published fields (export plan §5), in its order: rows of cells, each field's width a
/// multiple of the narrowest's.</summary>
public sealed record GroupDeclaration(string Name, IReadOnlyList<string> Fields);

/// <summary>A description (spec §5.7): text a script shows for a record, run with its seeds.</summary>
/// <param name="ScriptField">The field holding the packed id of the script the game calls for the record, or null.</param>
/// <param name="ScriptName">The script to run, named without its extension, or null.</param>
/// <param name="Seed">Each global set before the run: the record's id (<see cref="SeedValue.IsKey"/>) or a number.</param>
public sealed record DescriptionDeclaration(
    string Name,
    string? ScriptField,
    string? ScriptName,
    IReadOnlyList<(string Global, SeedValue Value)> Seed)
{
    internal static DescriptionDeclaration Parse(JsonElement description)
    {
        JsonElement script = description.GetProperty("script");
        return new DescriptionDeclaration(
            description.GetProperty("name").GetString()!,
            script.TryGetProperty("field", out JsonElement field) ? field.GetString() : null,
            script.TryGetProperty("name", out JsonElement name) ? name.GetString() : null,
            description.TryGetProperty("seed", out JsonElement seed)
                ? seed.EnumerateObject().Select(pair => (pair.Name, SeedValue.Parse(pair.Value))).ToList()
                : []);
    }
}

/// <summary>A loader (spec §6.3): for each entity of the kind <see cref="Keys"/> not skipped, <see cref="Script"/> run
/// from the booted session with the seeds to the <see cref="ReadAt"/> point, then the published tables of
/// <see cref="Scope"/> read (export plan §4.2).</summary>
public sealed record LoaderDeclaration(
    string Scope,
    string Keys,
    IReadOnlyList<(string Field, long Value)> Skip,
    string Script,
    IReadOnlyList<(string Global, SeedValue Value)> Seed,
    string ReadAt,
    IReadOnlyList<string> Builders,
    IReadOnlyList<LoadedTableDeclaration> Tables)
{
    internal static LoaderDeclaration Parse(JsonElement loader)
        => new(
            loader.GetProperty("scope").GetString()!,
            loader.GetProperty("keys").GetString()!,
            loader.TryGetProperty("skip", out JsonElement skip)
                ? skip.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetInt64())).ToList()
                : [],
            loader.GetProperty("script").GetString()!,
            loader.TryGetProperty("seed", out JsonElement seed)
                ? seed.EnumerateObject().Select(pair => (pair.Name, SeedValue.Parse(pair.Value))).ToList()
                : [],
            loader.GetProperty("read_at").GetString()!,
            ExportDeclarations.Strings(loader, "builders"),
            loader.TryGetProperty("tables", out JsonElement tables)
                ? tables.EnumerateObject().Select(pair => new LoadedTableDeclaration(
                    pair.Name, pair.Value.GetProperty("table").GetString()!, pair.Value.GetProperty("exists").GetString()!,
                    ExportDeclarations.Strings(pair.Value, "fields"))).ToList()
                : []);
}

/// <summary>A table a loader publishes (export plan §4.2): its short name in the scope, its registry table, which of its
/// records exist, and its published fields.</summary>
public sealed record LoadedTableDeclaration(string Name, string Table, string Exists, IReadOnlyList<string> Fields);

/// <summary>A seed's value: the record's id (<c>"key"</c>) or a number.</summary>
public readonly record struct SeedValue(bool IsKey, long Value)
{
    public static SeedValue Key => new(true, 0);

    public long For(int record) => IsKey ? record : Value;

    internal static SeedValue Parse(JsonElement value)
        => value.ValueKind == JsonValueKind.String
            ? value.GetString() == "key" ? Key : throw new InvalidDataException($"a seed is \"key\" or a number, not {value}")
            : new SeedValue(false, value.GetInt64());
}
