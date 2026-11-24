using System.Globalization;
using Ome.Engine.Model;
using Ome.Engine.Modding;
using Ome.Script.Abi;

namespace Ome.Commands;

/// <summary>
/// A script's instructions as text (modding from a release spec §7.3, plan §3.3), in the form
/// <c>tools/sys4load.py</c>'s <c>render_listing</c> gives them, which <c>tools/locate_page.py</c> prints:
/// <list type="bullet">
/// <item>the mnemonic is the opcode's label, or its semantic name where the label is unnamed (<c>u00…</c>,
/// <c>dev_ukn…</c>, or the opcode in hex); an unnamed label is followed by <c>   ; op 0x…</c>, and by
/// <c> investigated</c> when the opcode has a semantic name;</item>
/// <item>a string operand is <c>"text"</c>, a code target <c>label_&lt;hex&gt;</c>, an immediate <c>0x…</c>, a float
/// <c>(float 0x…)</c>, a typed operand <c>(&lt;type&gt; 0x…)</c> with the registry's type names, and a type the
/// registry does not name <c>&lt;t0x… 0x…&gt;</c>.</item>
/// </list>
/// The names are what a release has, so they differ from the Python loader's, which reads the checkout's registry
/// JSON: <c>call-script</c>'s target is the catalog's name for the id (<c>0x5 =SC0000.BIN</c>), and a global the base
/// mod's layout name, with a table's row (<c>(global-int 0x66713 =acting_entity_index)</c>).
/// </summary>
public sealed class ScriptListing
{
    public const int CallScriptOpcode = 0x03;
    private const int StringType = 2, FloatType = 1, ImmediateType = 0;
    private const long NoTarget = 0xFFFFFFFF;

    private readonly OpcodeTable _table;
    private readonly OpcodeContracts _contracts;
    private readonly Func<long, string?>? _scriptName;
    private readonly Func<CellKind, int, string?>? _globalName;

    /// <param name="scriptName">The script a <c>call-script</c> id names; null for no names.</param>
    /// <param name="globalName">The name of a global cell; null for no names.</param>
    public ScriptListing(OpcodeTable table, OpcodeContracts contracts, Func<long, string?>? scriptName = null,
                         Func<CellKind, int, string?>? globalName = null)
    {
        _table = table;
        _contracts = contracts;
        _scriptName = scriptName;
        _globalName = globalName;
    }

    /// <summary>The cell kind of a global operand type (3 global-int, 4 global-float, 5 global-string, 6 global-ptr,
    /// 8 global-string-ptr), or null for another type.</summary>
    public static CellKind? GlobalKind(int type) => type switch
    {
        3 or 6 => CellKind.Integer,
        4 => CellKind.Float,
        5 or 8 => CellKind.String,
        _ => null,
    };

    /// <summary>The instruction's text, without its offset.</summary>
    public string Line(Engine.Model.Script script, Instruction instruction)
    {
        int op = instruction.Opcode;
        string label = _table.Label(op);
        string operands = string.Join(' ', instruction.Args.Select((operand, index) => Operand(script, op, index, operand)));
        bool semantic = _contracts.SemanticNames.TryGetValue(op, out string? semanticName);
        // sys4load.display_label and render_listing test "unnamed" two ways: the mnemonic also takes a label that is
        // the opcode in hex, and the suffix also takes any label that starts with a digit.
        bool unnamedMnemonic = label.StartsWith("u00", StringComparison.Ordinal)
                               || label.StartsWith("dev_ukn", StringComparison.Ordinal)
                               || label.Equals(op.ToString("x", CultureInfo.InvariantCulture),
                                               StringComparison.OrdinalIgnoreCase);
        bool unnamedSuffix = label.StartsWith("u00", StringComparison.Ordinal)
                             || label.StartsWith("dev_ukn", StringComparison.Ordinal)
                             || (label.Length > 0 && char.IsAsciiDigit(label[0]));
        string mnemonic = unnamedMnemonic && semantic ? semanticName! : label;
        string suffix = unnamedSuffix ? $"   ; op 0x{op:x}" + (semantic ? " investigated" : "") : "";
        return mnemonic + (operands.Length > 0 ? " " + operands : "") + suffix;
    }

    /// <summary>One operand's text; <paramref name="index"/> counts from 0.</summary>
    public string Operand(Engine.Model.Script script, int op, int index, Operand operand)
    {
        long value = operand.Value;
        if (operand.Type == StringType)
            return script.Strings.TryGetValue((int)value, out string? text) ? $"\"{Escape(text)}\"" : "\"?\"";
        if (operand.Type == ImmediateType && value != NoTarget && _contracts.IsCodeTarget(op, index + 1))
            return $"label_{value:x}";
        if (operand.Type == ImmediateType)
            return op == CallScriptOpcode && index == 0 && _scriptName?.Invoke(value) is { } name
                ? $"0x{value:x} ={name}"
                : $"0x{value:x}";
        if (!_contracts.OperandTypeNames.TryGetValue(operand.Type, out string? typeName))
            return $"<t0x{operand.Type:x} 0x{value:x}>";
        if (operand.Type == FloatType) return $"(float 0x{value:x})";
        string annotation = GlobalKind(operand.Type) is { } kind && value <= int.MaxValue
                            && _globalName?.Invoke(kind, (int)value) is { } global
            ? " =" + global
            : "";
        return $"({typeName} 0x{value:x}{annotation})";
    }

    /// <summary>A line break in a string would end the listing's line; none of the three games' scripts has one.</summary>
    private static string Escape(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n");
}
