using System.Security.Cryptography;
using Ome.Engine.Hosting;
using Ome.Engine.Localization;
using Ome.Engine.Model;
using Ome.Engine.Modding;
using Ome.Engine.Natives;
using Ome.Engine.Vm;

namespace Ome.Export;

using Script = Ome.Engine.Model.Script;

/// <summary>A decoded picture as the export writes it (game data export spec §6.5): its catalog name without the
/// extension, its PNG bytes and size.</summary>
public sealed record ExportPicture(string Name, byte[] Png, int Width, int Height)
{
    /// <summary>The SHA-256 of the PNG bytes, in lower-case hex.</summary>
    public string Hash { get; } = Convert.ToHexStringLower(SHA256.HashData(Png));

    /// <summary>Whether the declarations name it explicit, for the site to blur until a click.</summary>
    public bool Explicit { get; init; }

    /// <summary>Whether the game draws it with op 0x203's additive glow (export plan §5): light added to what is under
    /// it, which a light page would wash out, so the site shows it on a dark tile.</summary>
    public bool Additive { get; init; }
}

/// <summary>Where the export's pictures come from: the game's catalog, through the engine's resource map, or a test's.
/// </summary>
public interface IPictureSource
{
    /// <summary>The picture a packed resource id names, or null when it names none.</summary>
    ExportPicture? Resolve(long packedId);

    /// <summary>The picture a packed resource id names with every pixel of the colour <paramref name="key"/> (0xRRGGBB)
    /// transparent, as the game draws it when it loads the picture with that colour key (export plan §5: Himegari's map
    /// sprites, DRAWCH@0x9c), named by its catalog name and <c>-key-RRGGBB</c>; or null when it names none.</summary>
    ExportPicture? ResolveKeyed(long packedId, int key) => Resolve(packedId);

    /// <summary>A cell cut from the catalog picture <paramref name="sheet"/> (export plan §5), with every pixel of the
    /// colour <paramref name="key"/> (0xRRGGBB) transparent where one is given, as <see cref="ResolveKeyed"/> makes a
    /// picture; or null when there is no such picture or the cell lies outside it.</summary>
    ExportPicture? Cell(string sheet, int x, int y, int width, int height, int? key) => null;

    /// <summary>The picture a packed resource id names, or its first frame where <paramref name="frame"/> gives one: its
    /// top left corner of that size (export plan §5); with the colour <paramref name="key"/> (0xRRGGBB) transparent
    /// where one is given, as <see cref="ResolveKeyed"/> makes a picture; and where <paramref name="additive"/>, as op
    /// 0x203's mode 1 draws it over black: each pixel adds its colour, so its brightest channel is its alpha (a healing
    /// feather's glow). A frame the size of the whole picture, not additive, is the picture as <see cref="Resolve"/>
    /// or <see cref="ResolveKeyed"/> gives it. Null when the id names no picture or the frame is larger than it.
    /// </summary>
    ExportPicture? Frame(long packedId, (int Width, int Height)? frame, int? key, bool additive) => null;
}

/// <summary>A package a language of the export comes from (spec §6.4): what the site credits.</summary>
public sealed record ExportLanguagePackage(string Id, string Name, string Version, IReadOnlyList<string> Authors);

/// <summary>A language the export translates its text into (spec §6.4, plan §4.6): its catalogs, loaded from the
/// packages that declare it, as the port loads them.</summary>
public sealed record ExportLanguage(string Tag, TextLanguage Text, IReadOnlyList<ExportLanguagePackage> Packages);

/// <summary>What one export reads (game data export spec §6.1): the installed game the caller loaded, the
/// declarations and the registry.</summary>
public sealed class ExportInputs
{
    public required string GameId { get; init; }
    public required OpcodeTable Table { get; init; }
    /// <summary>The install's engine build, or null for a bare opcode table's (a test's).</summary>
    public EngineBuild? Build { get; init; }
    public required IScriptProvider Scripts { get; init; }
    /// <summary>A script by its name without the extension (<c>INIT2</c>, <c>FAMES</c>), or null.</summary>
    public required Func<string, Script?> ScriptByName { get; init; }
    /// <summary>The site of the declarations' <c>read_at</c> patch point in this install.</summary>
    public required RunStop BootStop { get; init; }
    /// <summary>The site of a patch point in this install, by its name, for a loader's <c>read_at</c> (spec §6.3); it
    /// throws <see cref="ExportFailure"/> for a point with no site. Null when the caller resolves none.</summary>
    public Func<string, RunStop>? StopOf { get; init; }
    public required ExportDeclarations Declarations { get; init; }
    public required RegistryView Registry { get; init; }
    public IPictureSource? Pictures { get; init; }
    public long BootMaxSteps { get; init; } = 200_000_000;
    public long DescriptionMaxSteps { get; init; } = 5_000_000;
    public long LoaderMaxSteps { get; init; } = 50_000_000;
    /// <summary>The base mod's shape of its data kind (<c>mods/&lt;game&gt;-base/shapes/tables.schema.json</c>), which
    /// every snippet is checked against (plan §4.3); null checks only that a snippet reads back.</summary>
    public Ome.Mods.Data.DataShape? Shape { get; init; }
    /// <summary>The install's script index (plan §4.3), which <c>script_use</c> counts from and the script relations
    /// are found in (plan §4.5); null leaves <c>script_use</c> empty, and fails an export that declares relations.
    /// </summary>
    public ScriptIndex? ScriptIndex { get; init; }
    /// <summary>The languages to translate the text into, besides the source (spec §6.4, plan §4.6), in order.</summary>
    public IReadOnlyList<ExportLanguage> Languages { get; init; } = [];
    /// <summary>The message tables a language's description text resolves by (localization spec §6.2), as the port's;
    /// null resolves each run on its own.</summary>
    public ITextMessageIndex? Messages { get; init; }
    /// <summary>The registry's names for global cells, which the report's unnamed cells are counted by (plan §4.3);
    /// null counts none.</summary>
    public GlobalNames? Names { get; init; }
    /// <summary>The <c>build_info</c> rows the caller knows (spec §7.2): the revision, the fingerprint, OME's version.
    /// </summary>
    public IReadOnlyList<(string Key, string Value)> BuildInfo { get; init; } = [];
}

/// <summary>One cell of a record, as the boot left it, with the script that wrote it last.</summary>
public sealed record ExportCell(string Field, int Index, GlobalBank Bank, int Address, long Number, string? Text,
                                string? Writer)
{
    public bool IsDefault => Text is null ? Number == 0 : Text.Length == 0;
}

/// <summary>A value left out of the export (spec §5.5), with its cell and why.</summary>
public sealed record Refusal(string Kind, int Id, string Field, int Index, GlobalBank Bank, int Address, string Reason);

/// <summary>A published string or description in a language: the source's (<see cref="ExportWriter.SourceLanguage"/>)
/// or one the export translates into (spec §6.4).</summary>
public sealed record ExportText(string Field, int Index, string Value, string Language = ExportWriter.SourceLanguage);

/// <summary>A reference (spec §5.3): element <see cref="Index"/> of <see cref="Field"/> names entity
/// <see cref="ToId"/> of <see cref="ToKind"/>.</summary>
public sealed record ExportRef(string Field, int Index, string ToKind, long ToId);

/// <summary>A cell as the modder data shows it (spec §6.6): its raw value, unless it is a string the kind does not
/// publish or a refused value.</summary>
public sealed record ExportModderCell(string Field, int Index, GlobalBank Bank, int Address, string? Raw, string? Writer);

/// <summary>One entity: a record that exists and is not skipped.</summary>
public sealed class ExportEntity(int id)
{
    public int Id { get; } = id;
    /// <summary>Each published field's values, in the declarations' order; null for a refused element.</summary>
    public List<(string Field, long?[] Values)> Values { get; } = [];
    public List<ExportText> Texts { get; } = [];
    public List<ExportRef> Refs { get; } = [];
    /// <summary>The records its published two-field references name (export plan §5), by field; null for an element
    /// either cell of which is withheld.</summary>
    public Dictionary<string, long?[]> KeyedRefs { get; } = new(StringComparer.Ordinal);
    /// <summary>The rows its row references name (export plan §5), by field: for an element whose plus is the
    /// reference's <c>any</c>, the row, every record of which it names; else null.</summary>
    public Dictionary<string, long?[]> KeyedRows { get; } = new(StringComparer.Ordinal);
    public List<(string Name, ExportPicture Picture)> Pictures { get; } = [];
    public List<ExportModderCell> Cells { get; } = [];
    /// <summary>The rows its loaders built (export plan §4.2), by table and record.</summary>
    public List<ExportLoadedRow> Loaded { get; } = [];
    /// <summary>The calls the script relations resolved to this entity (spec §6.7, plan §4.5), by relation, script and
    /// offset.</summary>
    public List<ExportRelation> Relations { get; } = [];
    /// <summary>The snippet's record (spec §6.6, plan §4.3): each published, text and picture field at its value.</summary>
    internal List<KeyValuePair<string, DataValue>> SnippetRecord { get; } = [];
    /// <summary>The snippet's records of the tables its kind joins: the table, the record and its fields.</summary>
    internal List<(string Table, int Record, List<KeyValuePair<string, DataValue>> Fields)> SnippetJoined { get; } = [];
    /// <summary>The snippet's loaded rows: scope, table name, record and the row's fields.</summary>
    internal List<(string Scope, string Table, int Record, List<KeyValuePair<string, DataValue>> Fields)> SnippetRows { get; } = [];
    /// <summary>A data file that sets the entity's published fields to their values, ready to edit (spec §6.6).</summary>
    public string? Snippet { get; internal set; }
}

/// <summary>One record of a loaded table: its published fields' values, in the declaration's order; null for a refused
/// element.</summary>
public sealed record ExportLoadedRow(string Table, int Record, IReadOnlyList<(string Field, long?[] Values)> Values)
{
    /// <summary>The part of the table it is in (export plan §5), or null for the table's own rows.</summary>
    public string? Part { get; init; }

    /// <summary>The records its published two-field references name (export plan §5), by field.</summary>
    public IReadOnlyDictionary<string, long?[]> KeyedRefs { get; init; } = new Dictionary<string, long?[]>();

    /// <summary>The rows its published row references name (export plan §5), by field.</summary>
    public IReadOnlyDictionary<string, long?[]> KeyedRows { get; init; } = new Dictionary<string, long?[]>();
}

/// <summary>A table a loader publishes under its key kind (export plan §4.2), with the layouts the writer needs.
/// </summary>
public sealed record ExportLoadedTable(LoadedTableDeclaration Declaration, string? Scope, TableLayout Table,
                                       IReadOnlyList<FieldLayout> TableFields, IReadOnlyList<FieldLayout> Published,
                                       FieldLayout Exists)
{
    /// <summary>Its published text fields (export plan §5), which the entity's text holds as <c>table.field</c>.</summary>
    public IReadOnlyList<FieldLayout> Text { get; init; } = [];

    /// <summary>Its parts (export plan §5), each with the row's field it reads and the global that field indexes.</summary>
    public IReadOnlyList<(PartDeclaration Declaration, FieldLayout Field, ArrayLayout Global)> Parts { get; init; } = [];

    /// <summary>The values a record's field must hold to be a row (export plan §5).</summary>
    public IReadOnlyList<(FieldLayout Field, IReadOnlySet<long> Values)> Where { get; init; } = [];

    /// <summary>The tables a row's reference reaches (export plan §5), by name, with the referring field.</summary>
    public IReadOnlyDictionary<string, (TableLayout Table, FieldLayout Through)> Joins { get; init; }
        = new Dictionary<string, (TableLayout, FieldLayout)>();

    /// <summary>Every field of the tables it joins, each named <c>table.field</c>.</summary>
    public IReadOnlyList<FieldLayout> JoinedFields { get; init; } = [];

    /// <summary>The registry table a field is of: a joined table's, or its own.</summary>
    public TableLayout TableOf(FieldLayout field) => field.Joined is { } joined ? Joins[joined].Table : Table;
}

/// <summary>One kind's entities, with the layouts the writer needs: every field of the table, the published ones,
/// and every field or column the declaration names.</summary>
public sealed record ExportKind(KindDeclaration Declaration, TableLayout Table, IReadOnlyList<FieldLayout> TableFields,
                                IReadOnlyList<FieldLayout> Published, IReadOnlyList<FieldLayout> Declared,
                                IReadOnlyList<ExportEntity> Entities)
{
    /// <summary>The tables loaders publish under this kind's entities (export plan §4.2).</summary>
    public List<ExportLoadedTable> Loaded { get; } = [];
    /// <summary>The tables it joins (export plan §5, <c>joins</c>), by name: record r of each is entity r.</summary>
    public IReadOnlyDictionary<string, TableLayout> Joins { get; init; } = new Dictionary<string, TableLayout>();
    /// <summary>Every field of its joined tables, each written <c>table.field</c>.</summary>
    public IReadOnlyList<FieldLayout> JoinedFields { get; init; } = [];

    /// <summary>The table a field of the kind is of: a joined table's, or its own.</summary>
    public TableLayout TableOf(FieldLayout field) => field.Joined is { } joined ? Joins[joined] : Table;
    /// <summary>The scripts that read, write or index a cell of its table or of its loaded tables, with their counts
    /// (plan §4.3), by script name.</summary>
    public List<(string Script, GlobalUse Use)> ScriptUse { get; } = [];
    /// <summary>The words and names of its shown fields' values (plan §5), its loaded tables' written table.field.</summary>
    public List<ExportValueName> ValueNames { get; } = [];
    /// <summary>The game's names for its shown fields' columns (<c>columns_from</c>), its loaded tables' written
    /// table.field.</summary>
    public List<ExportColumnName> ColumnNames { get; } = [];
    /// <summary>Its categories (export plan §5), whose values each entity's <see cref="ExportEntity.Values"/> holds after
    /// its published fields', in order.</summary>
    public IReadOnlyList<ExportCategory> Categories { get; init; } = [];
}

/// <summary>A kind's category as the export works it out (export plan §5): its rules, each with the published field it
/// reads and the first of the mask's bits it places an entity in.</summary>
public sealed record ExportCategory(string Name, IReadOnlyList<ExportCategoryRule> Rules);

/// <summary>A rule of an <see cref="ExportCategory"/>: its field, the word of its one category or null, the mask's bit
/// its first category is, and how many categories it has (1 for a word, else as many as the game's names of the
/// field's bits, <see cref="BitsOf"/>).</summary>
public sealed record ExportCategoryRule(FieldLayout Field, string? Word, int Offset, int Bits, string? BitsOf);

/// <summary>What a value of a shown field means to a reader (plan §5): OME's word for it, in no language
/// (<see cref="Language"/> empty), or the game's name for it, in the source language or a translation's. A mask's
/// values are its bits.</summary>
public sealed record ExportValueName(string Field, long Value, string Language, string Text);

/// <summary>The game's name for a column of a shown field (<c>columns_from</c>): a string of the game's table of names,
/// in the source language or a translation's.</summary>
public sealed record ExportColumnName(string Field, int Column, string Language, string Text);

/// <summary>What the export found worth a look (spec §6.1): refused values end the command with exit code 1, the rest
/// is reported.</summary>
public sealed class ExportReport
{
    public string BootHalt { get; internal set; } = "";
    public List<Refusal> Refusals { get; } = [];
    /// <summary>The command's exit code for this report: 1 when a value was refused, else 0.</summary>
    public int ExitCode => Refusals.Count > 0 || SnippetProblems.Count > 0 ? 1 : 0;
    /// <summary>Problems of the snippets (plan §4.3): one that does not read back or that the base mod's shape refuses,
    /// each with its kind, id and the problem as the link would print it. Like a refusal, it makes the exit code 1.
    /// </summary>
    public List<(string Kind, int Id, string Problem)> SnippetProblems { get; } = [];
    /// <summary>For each kind, the integer cells its builders wrote that no global names (spec §6.6, plan §4.3), by
    /// address: arrays the registry has yet to name.</summary>
    public SortedDictionary<string, int[]> UnnamedCells { get; } = new(StringComparer.Ordinal);
    /// <summary>References whose target exists in no kind: (kind, field, target kind) and how many.</summary>
    public SortedDictionary<(string Kind, string Field, string ToKind), int> MissingRefs { get; } = new();
    /// <summary>Fields whose references name a table no kind publishes, and how many such values.</summary>
    public SortedDictionary<(string Kind, string Field, string Table), int> UnpublishedRefs { get; } = new();
    public List<(string Kind, int Id, string Picture, long PackedId)> UnresolvedPictures { get; } = [];
    /// <summary>Entries of <c>explicit_pictures</c> that match no exported picture: a name to correct or drop.</summary>
    public List<string> UnmatchedExplicitPictures { get; } = [];
    public int DescriptionRuns { get; internal set; }
    /// <summary>Pieces another script than the description's showed, dropped (spec §5.7).</summary>
    public int DroppedPieces { get; internal set; }
    /// <summary>Description runs that showed nothing.</summary>
    public int EmptyDescriptions { get; internal set; }
    /// <summary>Description runs that did not end at their script's exit, with their halt.</summary>
    public List<(string Kind, int Id, string Description, string Halt)> DescriptionHalts { get; } = [];
    /// <summary>Loader runs (spec §6.3): one for each key entity not skipped.</summary>
    public int LoaderRuns { get; internal set; }
    /// <summary>Loader runs that ended before their point, with their key and halt; their entity has no loaded rows.
    /// </summary>
    public List<(string Scope, string Kind, int Id, string Halt)> LoaderHalts { get; } = [];
    /// <summary>Records whose description field names no script.</summary>
    public List<(string Kind, int Id, string Description, long ScriptId)> MissingScripts { get; } = [];
    /// <summary>What each script relation's scan found (spec §6.7, plan §4.5), in the declarations' order.</summary>
    public List<RelationScan> Relations { get; } = [];
    /// <summary>What each language's translation found (spec §6.4, plan §4.6), in the inputs' order.</summary>
    public List<LanguageReport> Languages { get; } = [];
}

/// <summary>What one language's translation of the export found (spec §6.4, plan §4.6): its strings and
/// descriptions, and the misses of the text it exports, each as the port's miss log gives it.</summary>
public sealed class LanguageReport(ExportLanguage language, IEnumerable<string>? overrides = null)
{
    private readonly HashSet<(string Source, string Script, int Offset)> _misses = [];
    private readonly HashSet<string> _unusedOverrides = [.. overrides ?? []];

    public ExportLanguage Language { get; } = language;
    /// <summary>Published table strings, and those an entry translated.</summary>
    public int Strings { get; internal set; }
    public int StringsTranslated { get; internal set; }
    /// <summary>Published table strings the declarations' overrides gave their text (<c>[export] text_overrides</c>).
    /// </summary>
    public int StringsOverridden { get; private set; }
    /// <summary>The overrides no published string used, in order: one the declarations should drop or correct.</summary>
    public IReadOnlyList<string> UnusedOverrides => [.. _unusedOverrides.Order(StringComparer.Ordinal)];

    internal void Overridden(string source)
    {
        StringsOverridden++;
        _unusedOverrides.Remove(source);
    }
    /// <summary>Descriptions with text, those whose every run was presented, and those with a run presented and a run
    /// missed.</summary>
    public int Descriptions { get; internal set; }
    public int DescriptionsTranslated { get; internal set; }
    public int DescriptionsPartly { get; internal set; }
    /// <summary>Description runs in the language that did not end at their script's exit, with their halt.</summary>
    public List<(string Kind, int Id, string Description, string Halt)> DescriptionHalts { get; } = [];
    /// <summary>The strings the export's text missed (localization spec §14): a table string at its origin, a
    /// description's message or run at its site in the description's script; each once, by script, offset and source.
    /// </summary>
    public IReadOnlyList<(string Source, string Script, int Offset)> Misses
        => [.. _misses.OrderBy(miss => miss.Script, StringComparer.Ordinal).ThenBy(miss => miss.Offset)
                      .ThenBy(miss => miss.Source, StringComparer.Ordinal)];

    internal void Missed(string source, string script, int offset)
        => _misses.Add((source, TextCatalog.NormalizeScriptName(script), offset));
}

/// <summary>An export read from the game, ready to be written.</summary>
public sealed record ExportResult(ExportDeclarations Declarations, IReadOnlyList<ExportKind> Kinds, ExportReport Report,
                                  IReadOnlyList<(string Key, string Value)> BuildInfo)
{
    /// <summary>The languages its text is translated into, besides the source (spec §6.4).</summary>
    public IReadOnlyList<ExportLanguage> Languages { get; init; } = [];
}

/// <summary>Why an export could not run at all (exit code 2): a boot that ended elsewhere, a missing script.</summary>
public sealed class ExportFailure(string message) : Exception(message);
