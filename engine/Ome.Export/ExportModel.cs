using System.Security.Cryptography;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Vm;

namespace Ome.Export;

/// <summary>A decoded picture as the export writes it (game data export spec §6.5): its catalog name without the
/// extension, its PNG bytes and size.</summary>
public sealed record ExportPicture(string Name, byte[] Png, int Width, int Height)
{
    /// <summary>The SHA-256 of the PNG bytes, in lower-case hex.</summary>
    public string Hash { get; } = Convert.ToHexStringLower(SHA256.HashData(Png));
}

/// <summary>Where the export's pictures come from: the game's catalog, through the engine's resource map, or a test's.
/// </summary>
public interface IPictureSource
{
    /// <summary>The picture a packed resource id names, or null when it names none.</summary>
    ExportPicture? Resolve(long packedId);
}

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
    public required ExportDeclarations Declarations { get; init; }
    public required RegistryView Registry { get; init; }
    public IPictureSource? Pictures { get; init; }
    public long BootMaxSteps { get; init; } = 200_000_000;
    public long DescriptionMaxSteps { get; init; } = 5_000_000;
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

/// <summary>A published string or description, in the source language.</summary>
public sealed record ExportText(string Field, int Index, string Value);

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
    public List<(string Name, ExportPicture Picture)> Pictures { get; } = [];
    public List<ExportModderCell> Cells { get; } = [];
}

/// <summary>One kind's entities, with the layouts the writer needs: every field of the table, the published ones,
/// and every field or column the declaration names.</summary>
public sealed record ExportKind(KindDeclaration Declaration, TableLayout Table, IReadOnlyList<FieldLayout> TableFields,
                                IReadOnlyList<FieldLayout> Published, IReadOnlyList<FieldLayout> Declared,
                                IReadOnlyList<ExportEntity> Entities);

/// <summary>What the export found worth a look (spec §6.1): refused values end the command with exit code 1, the rest
/// is reported.</summary>
public sealed class ExportReport
{
    public string BootHalt { get; internal set; } = "";
    public List<Refusal> Refusals { get; } = [];
    /// <summary>The command's exit code for this report: 1 when a value was refused, else 0.</summary>
    public int ExitCode => Refusals.Count > 0 ? 1 : 0;
    /// <summary>References whose target exists in no kind: (kind, field, target kind) and how many.</summary>
    public SortedDictionary<(string Kind, string Field, string ToKind), int> MissingRefs { get; } = new();
    /// <summary>Fields whose references name a table no kind publishes, and how many such values.</summary>
    public SortedDictionary<(string Kind, string Field, string Table), int> UnpublishedRefs { get; } = new();
    public List<(string Kind, int Id, string Picture, long PackedId)> UnresolvedPictures { get; } = [];
    public int DescriptionRuns { get; internal set; }
    /// <summary>Pieces another script than the description's showed, dropped (spec §5.7).</summary>
    public int DroppedPieces { get; internal set; }
    /// <summary>Description runs that showed nothing.</summary>
    public int EmptyDescriptions { get; internal set; }
    /// <summary>Description runs that did not end at their script's exit, with their halt.</summary>
    public List<(string Kind, int Id, string Description, string Halt)> DescriptionHalts { get; } = [];
    /// <summary>Records whose description field names no script.</summary>
    public List<(string Kind, int Id, string Description, long ScriptId)> MissingScripts { get; } = [];
}

/// <summary>An export read from the game, ready to be written.</summary>
public sealed record ExportResult(ExportDeclarations Declarations, IReadOnlyList<ExportKind> Kinds, ExportReport Report,
                                  IReadOnlyList<(string Key, string Value)> BuildInfo);

/// <summary>Why an export could not run at all (exit code 2): a boot that ended elsewhere, a missing script.</summary>
public sealed class ExportFailure(string message) : Exception(message);
