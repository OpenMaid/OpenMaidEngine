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

    /// <summary>The derived scripts (<c>[export] derived</c>, export plan §5): what they write during the boot is play
    /// state worked out from the tables, withheld without a refusal.</summary>
    public IReadOnlyList<string> Derived { get; init; } = [];

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
            Derived = Strings(export, "derived"),
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

    /// <summary>A declaration's <c>notes</c>, name to line, in order.</summary>
    internal static IReadOnlyList<(string Name, string Note)> Notes(JsonElement parent) => Pairs(parent, "notes");

    /// <summary>A declaration's table of name to text, in order: its <c>notes</c> or its <c>labels</c>.</summary>
    internal static IReadOnlyList<(string Name, string Text)> Pairs(JsonElement parent, string key)
        => parent.TryGetProperty(key, out JsonElement pairs) && pairs.ValueKind == JsonValueKind.Object
            ? pairs.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetString()!)).ToList()
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

    /// <summary>The references a title falls back on, in order, when <see cref="Title"/>, a reference too, names no
    /// record (export plan §5): an Arterial recipe makes an item or a card, and is named by the one it makes.</summary>
    public IReadOnlyList<string> TitleFallbacks { get; init; } = [];

    /// <summary>The pictures cut from a sheet (export plan §5): a cell a field numbers.</summary>
    public IReadOnlyList<SheetPictureDeclaration> Sheets { get; init; } = [];

    /// <summary>A line for a reader for each field or group that needs one (export plan §5), by its name.</summary>
    public IReadOnlyList<(string Name, string Note)> Notes { get; init; } = [];

    /// <summary>A reader's name for a field or group whose own name reads as the game stores it (export plan §5):
    /// <c>min_range_encoded</c> is "Min range".</summary>
    public IReadOnlyList<(string Name, string Label)> Labels { get; init; } = [];

    /// <summary>The blocks a text field's lines are drawn in (export plan §5), by the field, in the order the game
    /// draws them.</summary>
    public IReadOnlyList<(string Field, IReadOnlyList<TextBlockDeclaration> Blocks)> TextBlocks { get; init; } = [];

    /// <summary>The fields that tell apart entities of the kind with one name (export plan §5), in order: the site adds
    /// those that differ to a repeated name ("Wilfred Dion (Novice Alchemist)").</summary>
    public IReadOnlyList<string> Qualifiers { get; init; } = [];

    /// <summary>How the kind's references read on their targets' pages (export plan §5), by the referring field.</summary>
    public IReadOnlyList<ReferenceHeadingDeclaration> ReferencedAs { get; init; } = [];

    /// <summary>What its entities are, in one line, for the site's card and list of the kind (site plan §10.3), or
    /// null.</summary>
    public string? About { get; init; }

    internal static KindDeclaration Parse(JsonElement kind)
        => new(
            kind.GetProperty("id").GetString()!,
            ExportDeclarations.Strings(kind, "aliases"),
            kind.GetProperty("table").GetString()!,
            kind.GetProperty("exists").GetString()!,
            kind.TryGetProperty("skip", out JsonElement skip)
                ? skip.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetInt64())).ToList()
                : [],
            !kind.TryGetProperty("title", out JsonElement title) ? null
                : title.ValueKind == JsonValueKind.String ? title.GetString()
                : title.ValueKind == JsonValueKind.Array ? title.EnumerateArray().Select(t => t.GetString()).FirstOrDefault()
                : null,
            ExportDeclarations.Strings(kind, "text"),
            ExportDeclarations.Strings(kind, "fields"),
            ExportDeclarations.Strings(kind, "filters"),
            kind.TryGetProperty("pictures", out JsonElement pictures)
                ? pictures.EnumerateObject().Where(pair => pair.Value.ValueKind == JsonValueKind.String)
                    .Select(pair => (pair.Name, pair.Value.GetString()!)).ToList()
                : [],
            kind.TryGetProperty("descriptions", out JsonElement descriptions)
                ? descriptions.EnumerateArray().Select(DescriptionDeclaration.Parse).ToList()
                : [])
        {
            Groups = kind.TryGetProperty("groups", out JsonElement groups)
                ? groups.EnumerateArray().Select(group => new GroupDeclaration(
                    group.GetProperty("name").GetString()!, ExportDeclarations.Strings(group, "fields"))
                    {
                        Labels = group.TryGetProperty("labels", out JsonElement labels)
                                 && labels.ValueKind == JsonValueKind.String ? labels.GetString() : null,
                    }).ToList()
                : [],
            Sheets = kind.TryGetProperty("pictures", out JsonElement sheets)
                ? sheets.EnumerateObject().Where(pair => pair.Value.ValueKind == JsonValueKind.Object)
                    .Select(pair => SheetPictureDeclaration.Parse(pair.Name, pair.Value)).ToList()
                : [],
            Notes = ExportDeclarations.Notes(kind),
            Labels = ExportDeclarations.Pairs(kind, "labels"),
            TextBlocks = kind.TryGetProperty("text_blocks", out JsonElement blocks)
                ? blocks.EnumerateObject().Select(pair => (pair.Name, (IReadOnlyList<TextBlockDeclaration>)pair.Value
                    .EnumerateArray().Select(block => new TextBlockDeclaration(block.GetProperty("name").GetString()!,
                        block.GetProperty("first").GetInt32(), block.GetProperty("last").GetInt32())).ToList())).ToList()
                : [],
            Qualifiers = ExportDeclarations.Strings(kind, "qualifiers"),
            TitleFallbacks = kind.TryGetProperty("title", out JsonElement titles) && titles.ValueKind == JsonValueKind.Array
                ? titles.EnumerateArray().Skip(1).Select(t => t.GetString()!).ToList()
                : [],
            About = kind.TryGetProperty("about", out JsonElement about) && about.ValueKind == JsonValueKind.String
                ? about.GetString()
                : null,
            ReferencedAs = kind.TryGetProperty("referenced_as", out JsonElement referenced)
                ? referenced.EnumerateArray().Select(entry => new ReferenceHeadingDeclaration(
                    entry.GetProperty("field").GetString()!, entry.GetProperty("heading").GetString()!,
                    ExportDeclarations.Strings(entry, "details"), ExportDeclarations.Pairs(entry, "parts"))).ToList()
                : [],
        };
}

/// <summary>How a reference reads on its target's page (export plan §5): the heading of the list of entities whose
/// <see cref="Field"/> names the target ("Dropped by"), the fields shown beside each of them (a drop's chance), and a
/// loaded table's part's own heading ("Ally on stages"). A loaded table's field is written <c>table.field</c>.</summary>
public sealed record ReferenceHeadingDeclaration(string Field, string Heading, IReadOnlyList<string> Details,
                                                 IReadOnlyList<(string Part, string Heading)> Parts);

/// <summary>A block of a text field's row of lines (export plan §5): lines <see cref="First"/> to <see cref="Last"/>,
/// which the game draws together (a quest's clear condition).</summary>
public sealed record TextBlockDeclaration(string Name, int First, int Last);

/// <summary>A group of a kind's published fields (export plan §5), in its order: rows of cells, each field's width a
/// multiple of the narrowest's. <see cref="Labels"/> names a text field of a table the kind's loaders publish
/// (<c>table.field</c>), whose record r labels row r.</summary>
public sealed record GroupDeclaration(string Name, IReadOnlyList<string> Fields)
{
    public string? Labels { get; init; }
}

/// <summary>A picture cut from a sheet (export plan §5): <see cref="Field"/> numbers a cell of the catalog picture
/// <see cref="Sheet"/>, <see cref="First"/> being the first, <see cref="Columns"/> to a row from the origin, a step
/// apart, each of the size; a number below the first is no picture.</summary>
public sealed record SheetPictureDeclaration(string Name, string Field, string Sheet, int First, int Columns,
                                             (int X, int Y) Origin, (int X, int Y) Step, (int Width, int Height) Size)
{
    /// <summary>The cell's left and top in the sheet for number <paramref name="number"/>, or null for none.</summary>
    public (int X, int Y)? Cell(long number)
    {
        if (number < First) return null;
        long index = number - First;
        return ((int)(Origin.X + index % Columns * Step.X), (int)(Origin.Y + index / Columns * Step.Y));
    }

    internal static SheetPictureDeclaration Parse(string name, JsonElement sheet)
    {
        (int, int) Pair(string key)
        {
            JsonElement[] pair = [.. sheet.GetProperty(key).EnumerateArray()];
            return (pair[0].GetInt32(), pair[1].GetInt32());
        }
        return new SheetPictureDeclaration(name, sheet.GetProperty("field").GetString()!,
                                           sheet.GetProperty("sheet").GetString()!, sheet.GetProperty("first").GetInt32(),
                                           sheet.GetProperty("columns").GetInt32(), Pair("origin"), Pair("step"),
                                           Pair("size"));
    }
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

/// <summary>A loader (spec §6.3): for each entity of the kind <see cref="Keys"/> in <see cref="KeyRange"/> and not
/// skipped, <see cref="Script"/> run from the booted session <see cref="Steps"/> times, each with the seeds, to the
/// <see cref="ReadAt"/> point; then its published tables read (export plan §4.2): the tables of <see cref="Scope"/>, or,
/// for a loader with none, registry tables that no data file edits (export plan §5). Run k's records are numbered k
/// times the table's records plus the record.</summary>
public sealed record LoaderDeclaration(
    string? Scope,
    string Keys,
    IReadOnlyList<(string Field, long Value)> Skip,
    string Script,
    IReadOnlyList<SeedCell> Seed,
    string ReadAt,
    IReadOnlyList<string> Builders,
    IReadOnlyList<LoadedTableDeclaration> Tables)
{
    /// <summary>The first and last key it runs for, or null for every entity of the kind.</summary>
    public (int First, int Last)? KeyRange { get; init; }

    /// <summary>The runs for each key, from step 0.</summary>
    public int Steps { get; init; } = 1;

    /// <summary>How messages and the report name it: by its scope, or by its script when it has none.</summary>
    public string Name => Scope ?? Script;

    internal static LoaderDeclaration Parse(JsonElement loader)
        => new(
            loader.TryGetProperty("scope", out JsonElement scope) && scope.ValueKind == JsonValueKind.String
                ? scope.GetString()
                : null,
            loader.GetProperty("keys").GetString()!,
            loader.TryGetProperty("skip", out JsonElement skip)
                ? skip.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetInt64())).ToList()
                : [],
            loader.GetProperty("script").GetString()!,
            loader.TryGetProperty("seed", out JsonElement seed)
                ? seed.ValueKind == JsonValueKind.Object
                    ? seed.EnumerateObject().Select(pair => new SeedCell(pair.Name, [], SeedValue.Parse(pair.Value))).ToList()
                    : seed.EnumerateArray().Select(SeedCell.Parse).ToList()
                : [],
            loader.GetProperty("read_at").GetString()!,
            ExportDeclarations.Strings(loader, "builders"),
            loader.TryGetProperty("tables", out JsonElement tables)
                ? tables.EnumerateObject().Select(pair => LoadedTableDeclaration.Parse(pair.Name, pair.Value)).ToList()
                : [])
        {
            KeyRange = loader.TryGetProperty("key_range", out JsonElement range) && range.ValueKind == JsonValueKind.Array
                ? (range[0].GetInt32(), range[1].GetInt32())
                : null,
            Steps = loader.TryGetProperty("steps", out JsonElement steps) && steps.ValueKind == JsonValueKind.Number
                ? steps.GetInt32()
                : 1,
        };
}

/// <summary>A cell a loader's seed writes before each run (export plan §5): a named integer global, at an index of a
/// term for each of its dimensions (none for one cell): a number, the key, the step, or every index of the dimension.
/// Seeds are written in order.</summary>
public sealed record SeedCell(string Global, IReadOnlyList<SeedIndex> Index, SeedValue Value)
{
    internal static SeedCell Parse(JsonElement entry)
        => new(entry.GetProperty("global").GetString()!,
               entry.TryGetProperty("index", out JsonElement index)
                   ? index.EnumerateArray().Select(SeedIndex.Parse).ToList()
                   : [],
               SeedValue.Parse(entry.GetProperty("value")));
}

/// <summary>One index term of a <see cref="SeedCell"/>.</summary>
public readonly record struct SeedIndex(SeedIndexKind Kind, int Value)
{
    internal static SeedIndex Parse(JsonElement term)
        => term.ValueKind == JsonValueKind.Number
            ? new SeedIndex(SeedIndexKind.Number, term.GetInt32())
            : term.GetString() switch
            {
                "key" => new SeedIndex(SeedIndexKind.Key, 0),
                "step" => new SeedIndex(SeedIndexKind.Step, 0),
                "*" => new SeedIndex(SeedIndexKind.Every, 0),
                _ => throw new InvalidDataException($"a seed index term is a number, \"key\", \"step\" or \"*\", not {term}"),
            };
}

public enum SeedIndexKind { Number, Key, Step, Every }

/// <summary>A table a loader publishes (export plan §4.2): its short name, its registry table, which of its records
/// exist, its published fields and text (export plan §5), the literals a record shows in place of its text, the
/// notes for its fields, and the parts the site shows apart.</summary>
public sealed record LoadedTableDeclaration(string Name, string Table, string Exists, IReadOnlyList<string> Fields)
{
    public IReadOnlyList<string> Text { get; init; } = [];
    public IReadOnlyList<LiteralDeclaration> Literals { get; init; } = [];
    public IReadOnlyList<(string Name, string Note)> Notes { get; init; } = [];
    public IReadOnlyList<(string Name, string Label)> Labels { get; init; } = [];
    public IReadOnlyList<PartDeclaration> Parts { get; init; } = [];

    internal static LoadedTableDeclaration Parse(string name, JsonElement table)
        => new(name, table.GetProperty("table").GetString()!, table.GetProperty("exists").GetString()!,
               ExportDeclarations.Strings(table, "fields"))
        {
            Text = ExportDeclarations.Strings(table, "text"),
            Literals = table.TryGetProperty("literals", out JsonElement literals)
                ? literals.EnumerateArray().Select(literal => new LiteralDeclaration(
                    literal.GetProperty("record").GetInt32(), literal.GetProperty("field").GetString()!,
                    literal.GetProperty("script").GetString()!, literal.GetProperty("offset").GetInt32())).ToList()
                : [],
            Notes = ExportDeclarations.Notes(table),
            Labels = ExportDeclarations.Pairs(table, "labels"),
            Parts = table.TryGetProperty("parts", out JsonElement parts)
                ? parts.EnumerateArray().Select(part => new PartDeclaration(
                    part.GetProperty("name").GetString()!, part.GetProperty("field").GetString()!,
                    part.GetProperty("global").GetString()!, part.GetProperty("same_as").GetInt32())
                {
                    Note = part.TryGetProperty("note", out JsonElement note) && note.ValueKind == JsonValueKind.String
                        ? note.GetString()
                        : null,
                }).ToList()
                : [],
        };
}

/// <summary>A part of a loaded table that the site shows apart (export plan §5), such as a stage's allies among its
/// enemy slots: the rows whose <see cref="Field"/> indexes a cell of the one-dimensional integer global
/// <see cref="Global"/> that is not 0 and equals its cell <see cref="SameAs"/>, as the run left them. The first part a
/// row matches is its part; the rest are the table's own. A data file still edits all of them as one table.</summary>
public sealed record PartDeclaration(string Name, string Field, string Global, int SameAs)
{
    public string? Note { get; init; }
}

/// <summary>A record whose text field the game shows a script's string in place of (export plan §5): the string operand
/// of the instruction at <see cref="Script"/>@<see cref="Offset"/> (a dword offset), translated at that site.</summary>
public sealed record LiteralDeclaration(int Record, string Field, string Script, int Offset);

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
