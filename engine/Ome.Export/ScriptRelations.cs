using System.Text.Json;
using Ome.Engine.Model;

namespace Ome.Export;

/// <summary>One script relation (game data export spec §6.7, plan §4.5) as <c>entities.json</c> gives it: the calls to
/// <see cref="Call"/> that set <see cref="Key"/> (an entity id of <see cref="Kind"/>) and <see cref="Count"/> from
/// constants, and whose <see cref="Where"/> globals hold their values, outside the <see cref="Exclude"/> scripts.
/// </summary>
public sealed record RelationDeclaration(
    string Id,
    string? Negative,
    string Call,
    string Kind,
    string Key,
    string? Count,
    IReadOnlyList<(string Global, long Value)> Where,
    IReadOnlyList<string> Exclude)
{
    internal static RelationDeclaration Parse(JsonElement relation)
        => new(
            relation.GetProperty("id").GetString()!,
            Optional(relation, "negative"),
            relation.GetProperty("call").GetString()!,
            relation.GetProperty("kind").GetString()!,
            relation.GetProperty("key").GetString()!,
            Optional(relation, "count"),
            relation.TryGetProperty("where", out JsonElement where)
                ? where.EnumerateObject().Select(pair => (pair.Name, pair.Value.GetInt64())).ToList()
                : [],
            ExportDeclarations.Strings(relation, "exclude"));

    private static string? Optional(JsonElement parent, string name)
        => parent.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

/// <summary>A call a relation resolved, on its entity: the relation's name (its id, or its negative for a negative
/// count), the calling script as the index names it, the call's offset, and the count as the script sets it, signed,
/// or null for a relation with no count.</summary>
public sealed record ExportRelation(string Relation, string Script, int Offset, long? Count);

/// <summary>What the scan of one relation found, for the report: its calls (in excluded scripts too) and the scripts
/// that make them, the rows written, the calls in excluded scripts, the calls left unresolved with the reason, and the
/// resolved calls whose key no entity has.</summary>
public sealed class RelationScan(RelationDeclaration declaration)
{
    public RelationDeclaration Declaration { get; } = declaration;
    public int Calls { get; internal set; }
    public int Scripts { get; internal set; }
    public int Rows { get; internal set; }
    public int Excluded { get; internal set; }
    public List<(string Script, int Offset, string Reason)> Unresolved { get; } = [];
    public List<(string Script, int Offset, long Key)> Keyless { get; } = [];
}

/// <summary>
/// Finds a relation's calls in every script of the install (spec §6.7, plan §4.5). Each argument global's value at a
/// call is the last write before it in its block, a block starting at each code offset an operand names: <c>mov G
/// N</c> gives N, <c>sub G 0 N</c> gives -N, any other write is computed. An operand that takes the global's address, an
/// opcode that calls other code (its <c>leaves</c> hold <c>calls</c>) and a call of a subroutine make each value
/// unknown, since that code may write it.
/// </summary>
public static class ScriptRelations
{
    /// <summary>The relation's resolved calls, each with its key, and the scan's counts but its rows and keyless calls,
    /// which the caller counts as it puts each call on its entity. <paramref name="nameOfId"/> names the script a
    /// call-script id calls, as the engine resolves it, or gives null.</summary>
    public static (RelationScan Scan, List<(long Key, ExportRelation Relation)> Found) Find(
        RelationDeclaration relation, ScriptIndex index, OpcodeTable table, RegistryView registry,
        Func<long, string?> nameOfId)
    {
        int callScript = table.ByLabel("call-script") ?? throw new ExportFailure("the opcode table has no call-script");
        int mov = table.ByLabel("mov") ?? throw new ExportFailure("the opcode table has no mov");
        int sub = table.ByLabel("sub") ?? throw new ExportFailure("the opcode table has no sub");
        var globals = new List<(string Name, int Cell)> { (relation.Key, registry.IntegerCell(relation.Key)) };
        if (relation.Count is { } count) globals.Add((count, registry.IntegerGlobal(count)));
        globals.AddRange(relation.Where.Select(where => (where.Global, registry.IntegerGlobal(where.Global))));
        var tracked = new Dictionary<int, int>();
        foreach ((string name, int cell) in globals)
            if (!tracked.TryAdd(cell, tracked.Count))
                throw new ExportFailure($"script relation {relation.Id}: {name} is a cell another of its globals names");
        string target = ScriptNames.Base(relation.Call);
        var names = new Dictionary<long, bool>();
        bool Calls(long id)
        {
            if (!names.TryGetValue(id, out bool hit))
                names[id] = hit = nameOfId(id) is { } name && ScriptNames.Base(name) == target;
            return hit;
        }

        var scan = new RelationScan(relation);
        var found = new List<(long, ExportRelation)>();
        ScriptReading reading = index.Reading;
        foreach (ScriptIndexEntry entry in index.Entries)
        {
            IReadOnlyList<Instruction> code = entry.Script.Script.Instructions;
            string script = entry.Script.Name;
            bool excluded = relation.Exclude.Any(pattern => ScriptNames.Matches(pattern, ScriptNames.Base(script)));
            var starts = new HashSet<long>();
            bool any = false;
            foreach (Instruction instruction in code)
            {
                for (int k = 0; k < instruction.Args.Count; k++)
                    if (instruction.Args[k].Type == 0 && reading.IsCodeOffset(instruction.Opcode, k + 1))
                        starts.Add(instruction.Args[k].Value);
                any |= instruction.Opcode == callScript && instruction.Args is [{ Type: 0 } id, ..] && Calls(id.Value);
            }
            if (!any) continue;

            scan.Scripts++;
            var values = new Value[globals.Count];
            Array.Fill(values, Value.Unwritten);
            foreach (Instruction instruction in code)
            {
                int op = instruction.Opcode;
                if (starts.Contains(instruction.Offset)) Array.Fill(values, Value.Unwritten);
                if (op == callScript && instruction.Args is [{ Type: 0 } id, ..] && Calls(id.Value))
                {
                    scan.Calls++;
                    if (excluded) scan.Excluded++;
                    else Resolve(relation, globals, values, script, instruction.Offset, scan, found);
                }
                for (int k = 0; k < instruction.Args.Count; k++)
                {
                    Operand operand = instruction.Args[k];
                    if (operand.Type != 3 || !tracked.TryGetValue(unchecked((int)operand.Value), out int slot)
                        || reading.Access(op, k + 1) is not { } kinds)
                        continue;
                    if (kinds.Contains("address"))
                        values[slot] = Value.Unknown($"its address is taken at 0x{instruction.Offset:x}");
                    else if (kinds.Any(kind => kind.StartsWith("write", StringComparison.Ordinal)))
                        values[slot] = k == 0 && Constant(instruction, mov, sub) is { } constant
                            ? Value.Of(constant)
                            : Value.Unknown($"computed at 0x{instruction.Offset:x}");
                }
                if ((table.TryGetDefinition(op, out OpcodeDefinition? definition)
                     && definition.Leaves.HasFlag(OpcodeLeaves.Calls))
                    || reading.IsLocalCall(op))
                    Array.Fill(values, Value.Unknown($"the call at 0x{instruction.Offset:x} may write it"));
            }
        }
        return (scan, found);
    }

    /// <summary>The constant a write gives its first operand: N for <c>mov G N</c>, -N for <c>sub G 0 N</c>; null for any
    /// other write.</summary>
    private static long? Constant(Instruction instruction, int mov, int sub)
        => instruction.Opcode == mov && instruction.Args is [_, { Type: 0 } value]
            ? unchecked((int)value.Value)
            : instruction.Opcode == sub && instruction.Args is [_, { Type: 0, Value: 0 }, { Type: 0 } negated]
                ? -(long)unchecked((int)negated.Value)
                : null;

    private static void Resolve(RelationDeclaration relation, List<(string Name, int Cell)> globals, Value[] values,
                                string script, int offset, RelationScan scan, List<(long, ExportRelation)> found)
    {
        var reasons = new List<string>();
        long? Known(int slot)
        {
            if (values[slot].Constant is { } constant) return constant;
            reasons.Add($"{globals[slot].Name}: {values[slot].Why}");
            return null;
        }

        long? key = Known(0);
        long? count = null;
        string name = relation.Id;
        if (relation.Count != null && (count = Known(1)) is { } known)
        {
            if (known == 0) reasons.Add($"{relation.Count} is 0");
            else if (known < 0 && relation.Negative is null)
                reasons.Add($"{relation.Count} is {known}, and the relation has no negative");
            else if (known < 0) name = relation.Negative!;
        }
        int first = relation.Count != null ? 2 : 1;
        for (int k = 0; k < relation.Where.Count; k++)
            if (Known(first + k) is { } value && value != relation.Where[k].Value)
                reasons.Add($"{relation.Where[k].Global} is {value}, not {relation.Where[k].Value}");
        if (reasons.Count > 0) scan.Unresolved.Add((script, offset, string.Join("; ", reasons)));
        else found.Add((key!.Value, new ExportRelation(name, script, offset, count)));
    }

    /// <summary>A tracked global's value in the block so far: a constant, or why it is not known.</summary>
    private readonly record struct Value(long? Constant, string Why)
    {
        public static Value Unwritten => new(null, "no write before the call in its block");
        public static Value Of(long constant) => new(constant, "");
        public static Value Unknown(string why) => new(null, why);
    }
}
