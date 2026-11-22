using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Script.Abi;
using Ome.Script.Patch;

namespace Ome.Export;

using Script = Ome.Engine.Model.Script;

/// <summary>A script as the index reads it: its name as the catalog mounts it, its packed id, its fingerprint and its
/// parsed code.</summary>
public sealed record IndexedScript(string Name, long PackedId, string? Fingerprint, Script Script);

/// <summary>How often a script's operands read, write or index (take as an array's base) one global cell.</summary>
public sealed record GlobalUse(int Reads, int Writes, int Addresses)
{
    public GlobalUse Plus(GlobalUse other) => new(Reads + other.Reads, Writes + other.Writes, Addresses + other.Addresses);
}

/// <summary>What the index holds of one script (export plan §4.3): its calls, the global cells its operands use and the
/// waiting opcodes in it. Names come later, when the index is written (<see cref="ScriptIndex.ToJson"/>).</summary>
public sealed class ScriptIndexEntry
{
    public required IndexedScript Script { get; init; }
    /// <summary>The scripts it calls by a constant id (call-script), with how many sites each.</summary>
    public SortedDictionary<long, int> Calls { get; } = new();
    /// <summary>Its call-script sites whose id is not a constant.</summary>
    public int ComputedCalls { get; internal set; }
    /// <summary>The scripts it preloads into a frame slot by a constant id (preload-script-slot), with how many sites.
    /// </summary>
    public SortedDictionary<long, int> Preloads { get; } = new();
    /// <summary>Each global cell an operand of a global bank names (types 3, 4 and 5), as the operand contracts say it is
    /// used.</summary>
    public Dictionary<GlobalCell, GlobalUse> Uses { get; } = new();
    /// <summary>Its opcodes whose handler waits or may wait (the registry's <c>waits</c>), with how many sites each.
    /// </summary>
    public SortedDictionary<string, int> Waits { get; } = new(StringComparer.Ordinal);
}

/// <summary>A named global of the registry view: its bank, base address and how many cells it covers, or null where its
/// type does not say (<c>int[]</c>, <c>int[unit][30]</c>).</summary>
public sealed record NamedGlobal(string Name, GlobalBank Bank, int Address, int? Extent);

/// <summary>
/// The registry's names for global cells (<c>build/games/&lt;game&gt;/globals.json</c>, curated and inferred alike): a
/// cell is named for the narrowest named global that holds it. A global whose type gives no length holds its cells up to
/// the next named global of its bank (the last such holds its base only).
/// </summary>
public sealed class GlobalNames
{
    private static readonly Regex TypeShape = new(@"^(int|string|float)((?:\[[^\]]*\])*)$", RegexOptions.CultureInvariant);

    private readonly Dictionary<GlobalBank, (int Start, int End, string Name)[]> _ranges = new();
    private readonly Dictionary<GlobalCell, string?> _known = new();

    public GlobalNames(IEnumerable<NamedGlobal> globals)
    {
        foreach (IGrouping<GlobalBank, NamedGlobal> bank in globals.GroupBy(global => global.Bank))
        {
            NamedGlobal[] sorted = bank.OrderBy(global => global.Address).ThenBy(global => global.Name, StringComparer.Ordinal)
                .ToArray();
            _ranges[bank.Key] = sorted.Select((global, k) =>
            {
                int end = global.Extent is { } extent
                    ? global.Address + Math.Max(extent, 1)
                    : sorted.Skip(k + 1).FirstOrDefault(next => next.Address > global.Address)?.Address ?? global.Address + 1;
                return (global.Address, end, global.Name);
            }).ToArray();
        }
    }

    public static GlobalNames Load(string globalsJson)
    {
        using FileStream stream = File.OpenRead(globalsJson);
        using JsonDocument document = JsonDocument.Parse(stream);
        var globals = new List<NamedGlobal>();
        foreach (JsonProperty cell in document.RootElement.GetProperty("cells").EnumerateObject())
        {
            JsonElement entry = cell.Value;
            if (entry.GetProperty("name").GetString() is not { Length: > 0 } name) continue;
            GlobalBank? bank = entry.GetProperty("bank").GetString() switch
            {
                "G" => GlobalBank.Integer,
                "GS" => GlobalBank.String,
                "GF" => GlobalBank.Float,
                _ => null,
            };
            if (bank is null) continue;
            globals.Add(new NamedGlobal(name, bank.Value, Convert.ToInt32(entry.GetProperty("address").GetString(), 16),
                                        ExtentOf(entry.GetProperty("type").GetString() ?? "")));
        }
        return new GlobalNames(globals);
    }

    /// <summary>The cells a registry type covers: 1 for a scalar, the product of its dimensions (<c>[1000][50]</c>,
    /// <c>[1000x50]</c>), null when one is not a number.</summary>
    public static int? ExtentOf(string type)
    {
        Match shape = TypeShape.Match(type);
        if (!shape.Success) return null;
        long extent = 1;
        foreach (Match dimension in Regex.Matches(shape.Groups[2].Value, @"\[([^\]]*)\]"))
            foreach (string part in dimension.Groups[1].Value.Split('x'))
            {
                if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int length)) return null;
                extent *= length;
                if (extent > int.MaxValue) return null;
            }
        return (int)extent;
    }

    /// <summary>The narrowest named global that holds the cell, or null.</summary>
    public string? NameOf(GlobalCell cell)
    {
        if (_known.TryGetValue(cell, out string? known)) return known;
        string? best = null;
        int width = int.MaxValue;
        if (_ranges.TryGetValue(cell.Bank, out (int Start, int End, string Name)[]? ranges))
            foreach ((int start, int end, string name) in ranges)
            {
                if (start > cell.Address) break;
                if (cell.Address < end && end - start < width) (best, width) = (name, end - start);
            }
        return _known[cell] = best;
    }
}

/// <summary>
/// The script index (game data export spec §8.2, plan §4.3): for each script the catalog mounts, the scripts it calls
/// and preloads by a constant id, its computed calls, the global cells its operands read, write or index, and the
/// waiting opcodes in it. <c>Ome.Cli script-index</c> writes it as <c>docs/script-index-&lt;game&gt;.json</c>, with the
/// registry's names, the callers and the patch points; the exporter's <c>script_use</c> counts from it.
/// </summary>
public sealed class ScriptIndex
{
    public const int FormatVersion = 1;

    public IReadOnlyList<ScriptIndexEntry> Entries { get; }

    private ScriptIndex(IReadOnlyList<ScriptIndexEntry> entries) => Entries = entries;

    /// <summary>Reads each script's instructions. An operand counts when its type is a global bank's (3 integer,
    /// 4 float, 5 string): as a read for a <c>read</c> kind of its contract, a write for a <c>write</c> kind, an index for
    /// <c>address</c>; an operand with no contract is not counted. <paramref name="operandAccess"/> gives an opcode's
    /// one-based operand's contract (<see cref="OpcodeContracts.Access"/> under the install's ABI, which composition
    /// binds: engine layering spec §7.6).</summary>
    public static ScriptIndex Build(IEnumerable<IndexedScript> scripts, OpcodeTable table,
                                    Func<int, int, IReadOnlyList<string>?> operandAccess)
    {
        int? callScript = table.ByLabel("call-script"), preload = table.ByLabel("preload-script-slot");
        var entries = new List<ScriptIndexEntry>();
        foreach (IndexedScript script in scripts)
        {
            var entry = new ScriptIndexEntry { Script = script };
            foreach (Instruction instruction in script.Script.Instructions)
            {
                int op = instruction.Opcode;
                if (op == callScript || op == preload)
                {
                    SortedDictionary<long, int> calls = op == callScript ? entry.Calls : entry.Preloads;
                    if (instruction.Args is [{ Type: 0 } id, ..]) calls[id.Value] = calls.GetValueOrDefault(id.Value) + 1;
                    else if (op == callScript) entry.ComputedCalls++;
                }
                if (table.TryGetDefinition(op, out OpcodeDefinition? definition)
                    && definition.Waits is OpcodeWaits.May or OpcodeWaits.Always)
                    entry.Waits[definition.CanonicalLabel] = entry.Waits.GetValueOrDefault(definition.CanonicalLabel) + 1;
                for (int k = 0; k < instruction.Args.Count; k++)
                {
                    Operand operand = instruction.Args[k];
                    GlobalBank? bank = operand.Type switch
                    {
                        3 => GlobalBank.Integer,
                        4 => GlobalBank.Float,
                        5 => GlobalBank.String,
                        _ => null,
                    };
                    if (bank is null || operandAccess(op, k + 1) is not { } kinds) continue;
                    var use = new GlobalUse(kinds.Any(kind => kind.StartsWith("read", StringComparison.Ordinal)) ? 1 : 0,
                                            kinds.Any(kind => kind.StartsWith("write", StringComparison.Ordinal)) ? 1 : 0,
                                            kinds.Contains("address") ? 1 : 0);
                    if (use is { Reads: 0, Writes: 0, Addresses: 0 }) continue;
                    var cell = new GlobalCell(bank.Value, unchecked((int)operand.Value));
                    entry.Uses[cell] = entry.Uses.TryGetValue(cell, out GlobalUse? sum) ? sum.Plus(use) : use;
                }
            }
            entries.Add(entry);
        }
        return new ScriptIndex(entries);
    }

    /// <summary>The index as <c>docs/script-index-&lt;game&gt;.json</c> holds it: per script, by name, its id,
    /// fingerprint and size, its calls and preloads by name (a constant id no script has as <c>#&lt;id&gt;</c>), its
    /// computed calls, its callers, its named globals with their counts and the counts of the cells no global names,
    /// the patch points in its revision and its waiting opcodes. It holds no text of the game.</summary>
    public string ToJson(string game, GlobalNames names, OmsPatchPoints? points, Func<long, string?> nameOfId)
    {
        string Target(long id) => nameOfId(id) ?? "#" + id.ToString(CultureInfo.InvariantCulture);
        var callers = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (ScriptIndexEntry entry in Entries)
            foreach (long id in entry.Calls.Keys)
            {
                string callee = Target(id);
                if (!callers.TryGetValue(callee, out SortedSet<string>? set))
                    callers[callee] = set = new SortedSet<string>(StringComparer.Ordinal);
                set.Add(entry.Script.Name);
            }

        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions
                   { Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            json.WriteStartObject();
            json.WriteString("generated", $"by Ome.Cli script-index --game {game} from the installed game; do not edit "
                                          + "(game data export spec §8.2, plan §4.3)");
            json.WriteNumber("format", FormatVersion);
            json.WriteString("game", game);
            json.WriteStartObject("scripts");
            foreach (ScriptIndexEntry entry in Entries.OrderBy(entry => entry.Script.Name, StringComparer.Ordinal))
            {
                IndexedScript script = entry.Script;
                json.WriteStartObject(script.Name);
                json.WriteNumber("id", script.PackedId);
                if (script.Fingerprint != null) json.WriteString("fingerprint", script.Fingerprint);
                json.WriteNumber("instructions", script.Script.Instructions.Count);
                WriteCounts(json, "calls", entry.Calls.GroupBy(pair => Target(pair.Key), pair => pair.Value)
                                                .ToDictionary(group => group.Key, group => group.Sum()));
                if (entry.ComputedCalls > 0) json.WriteNumber("computed_calls", entry.ComputedCalls);
                WriteCounts(json, "preloads", entry.Preloads.GroupBy(pair => Target(pair.Key), pair => pair.Value)
                                                    .ToDictionary(group => group.Key, group => group.Sum()));
                if (callers.TryGetValue(script.Name, out SortedSet<string>? by))
                {
                    json.WriteStartArray("called_by");
                    foreach (string caller in by) json.WriteStringValue(caller);
                    json.WriteEndArray();
                }
                var named = new SortedDictionary<string, GlobalUse>(StringComparer.Ordinal);
                var unnamed = new GlobalUse(0, 0, 0);
                foreach ((GlobalCell cell, GlobalUse use) in entry.Uses)
                    if (names.NameOf(cell) is { } name)
                        named[name] = named.TryGetValue(name, out GlobalUse? sum) ? sum.Plus(use) : use;
                    else unnamed = unnamed.Plus(use);
                if (named.Count > 0)
                {
                    json.WriteStartObject("globals");
                    foreach ((string name, GlobalUse use) in named)
                    {
                        json.WritePropertyName(name);
                        WriteUse(json, use);
                    }
                    json.WriteEndObject();
                }
                if (unnamed != new GlobalUse(0, 0, 0))
                {
                    json.WritePropertyName("unnamed");
                    WriteUse(json, unnamed);
                }
                if (points != null && script.Fingerprint != null)
                {
                    var here = points.Points
                        .Where(point => ScriptNames.Normalized(point.Script) == ScriptNames.Normalized(script.Name))
                        .SelectMany(point => point.At.Where(site => site.Fingerprint == script.Fingerprint)
                                                .Select(site => (point.Name, site.Site)))
                        .OrderBy(point => point.Site).ThenBy(point => point.Name, StringComparer.Ordinal).ToList();
                    if (here.Count > 0)
                    {
                        json.WriteStartObject("patch_points");
                        foreach ((string name, int site) in here) json.WriteString(name, "0x" + site.ToString("x", CultureInfo.InvariantCulture));
                        json.WriteEndObject();
                    }
                }
                WriteCounts(json, "waits", entry.Waits);
                json.WriteEndObject();
            }
            json.WriteEndObject();
            json.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.ToArray()).ReplaceLineEndings("\n") + "\n";
    }

    private static void WriteCounts(Utf8JsonWriter json, string name, IReadOnlyDictionary<string, int> counts)
    {
        if (counts.Count == 0) return;
        json.WriteStartObject(name);
        foreach ((string key, int count) in counts.OrderBy(pair => pair.Key, StringComparer.Ordinal)) json.WriteNumber(key, count);
        json.WriteEndObject();
    }

    /// <summary>A use as <c>[reads, writes, addresses]</c>, which keeps the index a line per global.</summary>
    private static void WriteUse(Utf8JsonWriter json, GlobalUse use)
    {
        json.WriteRawValue($"[{use.Reads}, {use.Writes}, {use.Addresses}]");
    }

    /// <summary>The scripts that use any cell of <paramref name="ranges"/>, with their summed counts (the export's
    /// <c>script_use</c>, plan §4.3).</summary>
    public IEnumerable<(string Script, GlobalUse Use)> UsesOf(IReadOnlyList<(GlobalBank Bank, int Start, int End)> ranges)
    {
        foreach (ScriptIndexEntry entry in Entries)
        {
            var sum = new GlobalUse(0, 0, 0);
            foreach ((GlobalCell cell, GlobalUse use) in entry.Uses)
                if (ranges.Any(range => range.Bank == cell.Bank && cell.Address >= range.Start && cell.Address < range.End))
                    sum = sum.Plus(use);
            if (sum != new GlobalUse(0, 0, 0)) yield return (entry.Script.Name, sum);
        }
    }
}
