using Ome.Script.Ir;

namespace Ome.Script.Compile;

/// <summary>
/// A lowered expression (spec §7.2): the shape both the compiler and the decompiler agree on. A leaf is an operand
/// as written; a binary node computes into an integer temporary; an element node binds a reference temporary to
/// <c>base[index]</c> (op 0x61) or <c>base[row * stride + col]</c> (op 0x12c). <see cref="Origin"/> is the original
/// instruction index of a decompiled node and takes no part in lowering.
/// </summary>
public abstract record LExpr
{
    public int Origin { get; init; } = -1;
}

public sealed record LLeaf(IrOperand Operand) : LExpr;

public sealed record LBinary(int Opcode, LExpr Left, LExpr Right) : LExpr;

/// <summary>Indices are <c>[index]</c> or <c>[row, stride, col]</c>; the temporary has
/// <see cref="PointerType"/> (0xc integer, 0xd float or 0xe string reference).</summary>
public sealed record LElement(IrOperand Base, IReadOnlyList<LExpr> Indices, int PointerType) : LExpr
{
    public int Opcode => Indices.Count == 1 ? ExpressionLowering.LookupArray : ExpressionLowering.LookupArray2D;
}

/// <summary>One lowered statement: the instructions of one source statement end with its root instruction.</summary>
public abstract record LStatement;

/// <summary><c>target = value</c>, or <c>target op= right</c> when <see cref="Compound"/> (then the value is a binary
/// node whose left operand re-reads the target).</summary>
public sealed record LAssign(LExpr Target, LExpr Value, bool Compound) : LStatement;

/// <summary>Any other root instruction; each operand is evaluated in full, in operand order.</summary>
public sealed record LInstruction(int Opcode, IReadOnlyList<LExpr> Operands) : LStatement;

/// <summary>
/// The canonical lowering order (spec §7.2), shared by the compiler and the decompiler's fold verification.
/// <list type="bullet">
/// <item>A binary node prepares both operands, then materializes both, then computes into a fresh temporary.
/// Preparing an operand computes its nested binary nodes; materializing emits its element lookups, so lookups
/// land as late as possible, just before the instruction that consumes them.</item>
/// <item><c>x = a op b</c> writes straight into <c>x</c>: the operands first, then the target's lookup.
/// <c>x op= b</c> looks the target up first and re-reads it as the left operand.</item>
/// <item><c>x = y</c> evaluates <c>y</c>, then the target, then moves.</item>
/// <item>Other root instructions evaluate each operand in full, one operand after another.</item>
/// </list>
/// Temporaries are taken from <c>allocate(node, type)</c> in emission order: type 9 for integer temporaries and the
/// node's pointer type for element lookups.
/// </summary>
public sealed class ExpressionLowering
{
    public const int LookupArray = 0x61, LookupArray2D = 0x12C, Mov = 0x55, IntTemp = 0x9;

    private readonly Func<LExpr, int, uint> _allocate;
    private readonly Action<IrInstruction> _emit;
    private readonly Dictionary<LExpr, IrOperand> _values = new(ReferenceEqualityComparer.Instance);

    private ExpressionLowering(Func<LExpr, int, uint> allocate, Action<IrInstruction> emit)
    {
        _allocate = allocate;
        _emit = emit;
    }

    public static void Lower(LStatement statement, Func<LExpr, int, uint> allocate, Action<IrInstruction> emit)
        => new ExpressionLowering(allocate, emit).Statement(statement);

    /// <summary>The instructions a statement lowers to, with temporaries from <paramref name="allocate"/>.</summary>
    public static List<IrInstruction> Instructions(LStatement statement, Func<LExpr, int, uint> allocate)
    {
        var code = new List<IrInstruction>();
        Lower(statement, allocate, code.Add);
        return code;
    }

    private void Statement(LStatement statement)
    {
        switch (statement)
        {
            case LAssign { Value: LBinary binary } assign when assign.Compound:
            {
                IrOperand target = Value(assign.Target);
                Prepare(binary.Left);
                Prepare(binary.Right);
                IrOperand left = Materialize(binary.Left), right = Materialize(binary.Right);
                _emit(new IrInstruction(binary.Opcode, [target, left, right]));
                break;
            }
            case LAssign { Value: LBinary binary } assign:
            {
                Prepare(binary.Left);
                Prepare(binary.Right);
                IrOperand left = Materialize(binary.Left), right = Materialize(binary.Right);
                IrOperand target = Value(assign.Target);
                _emit(new IrInstruction(binary.Opcode, [target, left, right]));
                break;
            }
            case LAssign assign:
            {
                if (assign.Compound) throw new ArgumentException("a compound assignment needs a binary value");
                IrOperand value = Value(assign.Value);
                IrOperand target = Value(assign.Target);
                _emit(new IrInstruction(Mov, [target, value]));
                break;
            }
            case LInstruction instruction:
            {
                var operands = new IrOperand[instruction.Operands.Count];
                for (int k = 0; k < operands.Length; k++) operands[k] = Value(instruction.Operands[k]);
                _emit(new IrInstruction(instruction.Opcode, operands));
                break;
            }
            default:
                throw new ArgumentException($"unknown statement {statement.GetType().Name}");
        }
    }

    private IrOperand Value(LExpr expression)
    {
        Prepare(expression);
        return Materialize(expression);
    }

    private void Prepare(LExpr expression)
    {
        switch (expression)
        {
            case LBinary binary:
            {
                Prepare(binary.Left);
                Prepare(binary.Right);
                IrOperand left = Materialize(binary.Left), right = Materialize(binary.Right);
                var temp = IrOperand.Literal(IntTemp, _allocate(binary, IntTemp));
                _emit(new IrInstruction(binary.Opcode, [temp, left, right]));
                _values[binary] = temp;
                break;
            }
            case LElement element:
                foreach (LExpr index in element.Indices) Prepare(index);
                break;
        }
    }

    private IrOperand Materialize(LExpr expression)
    {
        switch (expression)
        {
            case LLeaf leaf:
                return leaf.Operand;
            case LBinary binary:
                return _values[binary];
            case LElement element:
            {
                var operands = new IrOperand[element.Indices.Count + 2];
                for (int k = 0; k < element.Indices.Count; k++) operands[k + 2] = Materialize(element.Indices[k]);
                var temp = IrOperand.Literal(element.PointerType, _allocate(element, element.PointerType));
                operands[0] = temp;
                operands[1] = element.Base;
                _emit(new IrInstruction(element.Opcode, operands));
                return temp;
            }
            default:
                throw new ArgumentException($"unknown expression {expression.GetType().Name}");
        }
    }
}

/// <summary>How a script numbers its integer-reference temporaries (bank type <c>0xC</c>), by statement label
/// (spec §7.2). Each game's compiler keeps one order, and a script's sidecar says which one it has.</summary>
public enum RefOrder
{
    /// <summary>Label k is slot k.</summary>
    Plain,
    /// <summary>The first two labels are exchanged: LP1, LP0, then slot k for label k.</summary>
    Swap,
    /// <summary>The first three labels are rotated by one: LP1, LP2, LP0, then slot k for label k. Verita's scenes number
    /// their references so: a page's voice check reads <c>G[0x30a0][G[0x268f74][G[0x268b8c][G[0x3489]]]]</c> through
    /// LP0, LP2 and LP1, in that order of emission.</summary>
    Rotate,
}

/// <summary>
/// The canonical temporary plan of one statement (spec §7.2), measured on both corpora. The original compiler numbers
/// a statement's temporaries in a labelling pass separate from emission, with one counter shared by every bank:
/// <list type="bullet">
/// <item>operands in operand order; an assignment labels its value before its target, a compound assignment its
/// target first;</item>
/// <item>a binary node labels its binary operands first (post-order), then itself, then its element operands;</item>
/// <item>an element labels itself, then its indices.</item>
/// </list>
/// Label k is integer slot <c>intBase + k</c> or reference slot k, the integer references in the script's
/// <see cref="RefOrder"/> (<paramref name="refOrder"/>). A reference temporary never takes a slot of its bank that the
/// statement itself names: the k-th reference slot is then the k-th slot the statement leaves free (Verita's
/// SORTSALLY writes through LP1 a value it reads through two temporaries, which slots 0 and 1 would clobber). A sidecar
/// override replaces either sequence outright, listing slots in emission order.
/// </summary>
public sealed class CanonicalTemps
{
    private readonly uint _intBase;
    private readonly RefOrder _refOrder;
    private readonly IReadOnlyList<uint>? _intOverride, _refOverride;
    private readonly Dictionary<LExpr, int> _labels = new(ReferenceEqualityComparer.Instance);
    private int _ints, _refs;

    private readonly IReadOnlyDictionary<int, uint>? _refBase;

    /// <summary>The slots the statement names, by operand type: a reference temporary of that type skips them.</summary>
    private readonly Dictionary<int, SortedSet<uint>> _named = new();

    /// <param name="refBase">The first reference slot per reference bank type, when a fragment's temporaries must stay
    /// above the base script's (spec §10.2); otherwise reference temporaries start at slot 0.</param>
    public CanonicalTemps(LStatement statement, uint intBase, RefOrder refOrder = RefOrder.Plain,
                          IReadOnlyList<uint>? intOverride = null, IReadOnlyList<uint>? refOverride = null,
                          IReadOnlyDictionary<int, uint>? refBase = null)
    {
        _intBase = intBase;
        _refBase = refBase;
        _refOrder = refOrder;
        _intOverride = intOverride;
        _refOverride = refOverride;
        switch (statement)
        {
            case LAssign { Compound: true, Value: LBinary value } assign:
                Label(assign.Target);
                LabelOperands(value, root: true);
                break;
            case LAssign { Value: LBinary value } assign:
                LabelOperands(value, root: true);
                Label(assign.Target);
                break;
            case LAssign assign:
                Label(assign.Value);
                Label(assign.Target);
                break;
            case LInstruction instruction:
                foreach (LExpr operand in instruction.Operands) Label(operand);
                break;
        }
        switch (statement)
        {
            case LAssign assign:
                Name(assign.Target);
                Name(assign.Value);
                break;
            case LInstruction instruction:
                foreach (LExpr operand in instruction.Operands) Name(operand);
                break;
        }
    }

    private void Name(LExpr node)
    {
        switch (node)
        {
            case LLeaf leaf when leaf.Operand.Kind == IrOperandKind.Literal:
                Named(leaf.Operand);
                break;
            case LElement element:
                Named(element.Base);
                foreach (LExpr index in element.Indices) Name(index);
                break;
            case LBinary binary:
                Name(binary.Left);
                Name(binary.Right);
                break;
        }
    }

    private void Named(IrOperand operand)
    {
        if (operand.Kind != IrOperandKind.Literal) return;
        if (!_named.TryGetValue(operand.Type, out SortedSet<uint>? slots)) _named[operand.Type] = slots = new SortedSet<uint>();
        slots.Add(operand.Value);
    }

    /// <summary>The <paramref name="index"/>-th slot from <paramref name="first"/> that the statement does not name.</summary>
    private uint FreeSlot(int type, uint first, uint index)
    {
        if (!_named.TryGetValue(type, out SortedSet<uint>? named)) return first + index;
        uint slot = first;
        for (uint free = 0; ; slot++)
        {
            if (named.Contains(slot)) continue;
            if (free++ == index) return slot;
        }
    }

    public int IntCount => _ints;
    public int RefCount => _refs;

    private void Label(LExpr node)
    {
        switch (node)
        {
            case LElement element:
                _labels[element] = _labels.Count;
                foreach (LExpr index in element.Indices) Label(index);
                break;
            case LBinary binary:
                LabelOperands(binary, root: false);
                break;
        }
    }

    private void LabelOperands(LBinary binary, bool root)
    {
        if (binary.Left is LBinary) Label(binary.Left);
        if (binary.Right is LBinary) Label(binary.Right);
        if (!root) _labels[binary] = _labels.Count;
        if (binary.Left is LElement) Label(binary.Left);
        if (binary.Right is LElement) Label(binary.Right);
    }

    public uint Allocate(LExpr node, int type)
    {
        if (type == ExpressionLowering.IntTemp)
        {
            int k = _ints++;
            if (_intOverride != null)
                return k < _intOverride.Count ? _intOverride[k] : throw new InvalidDataException(
                    $"the temporary override lists {_intOverride.Count} integer slots but the statement needs more");
            return _intBase + (uint)_labels[node];
        }
        int r = _refs++;
        if (_refOverride != null)
            return r < _refOverride.Count ? _refOverride[r] : throw new InvalidDataException(
                $"the temporary override lists {_refOverride.Count} reference slots but the statement needs more");
        uint label = (uint)_labels[node];
        return FreeSlot(type, _refBase?.GetValueOrDefault(type) ?? 0, type == 0xC ? Ordered(label) : label);
    }

    /// <summary>The place of <paramref name="label"/> among the integer-reference slots in the script's order.</summary>
    private uint Ordered(uint label) => _refOrder switch
    {
        RefOrder.Swap when label < 2 => 1 - label,
        RefOrder.Rotate when label < 3 => (label + 1) % 3,
        _ => label,
    };
}
