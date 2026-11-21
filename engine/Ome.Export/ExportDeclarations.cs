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
    public static ExportDeclarations Load(string path) => Parse(File.ReadAllText(path), path);

    public static ExportDeclarations Parse(string json, string source = "entities.json")
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        // Loaders (milestone 3) and script relations (milestone 6) are not exported yet; declarations that have them
        // are refused rather than half exported.
        foreach (string later in new[] { "loaders", "script_relations" })
            if (root.TryGetProperty(later, out JsonElement list) && list.GetArrayLength() > 0)
                throw new InvalidDataException($"{source}: {later} are declared, which this exporter does not export yet");
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
            root.GetProperty("kinds").EnumerateArray().Select(KindDeclaration.Parse).ToList());
    }

    internal static IReadOnlyList<string> Strings(JsonElement parent, string name)
        => parent.TryGetProperty(name, out JsonElement list)
            ? list.EnumerateArray().Select(item => item.GetString()!).ToList()
            : [];
}

/// <summary>One kind (spec §5.2): a registry table, which of its records are entities, and what is published.</summary>
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
                : []);
}

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
