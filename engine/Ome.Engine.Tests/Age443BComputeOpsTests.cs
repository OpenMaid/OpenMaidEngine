using System;
using System.Collections.Generic;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>AGE 4.43B's compute ops that changed from 4.38D's (docs/engine-re.md, "Arterial against 魔導巧殻,
/// triaged").</summary>
public class Age443BComputeOpsTests
{
    private const int Imm = 0, Float = 1, GlobalInt = 3, GlobalFloat = 4;
    private const int Exit = 0x2, AbsoluteValue = 0x191;

    private static Operand I(long value) => new(Imm, value);
    private static Operand F(float value) => new(Float, unchecked((uint)BitConverter.SingleToInt32Bits(value)));
    private static Operand G(int address) => new(GlobalInt, address);
    private static Operand GF(int address) => new(GlobalFloat, address);

    private static VirtualMachine Run(string abi, params (int, Operand[])[] instructions)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, abi);
        var ops = new List<(int, Operand[])>(instructions) { (Exit, Array.Empty<Operand>()) };
        var vm = new VirtualMachine(ScriptAssembler.Assemble(table, "OPS.BIN", ops, []), table, new RecordingWaits());
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        return vm;
    }

    private static float FloatCell(VirtualMachine vm, int address)
        => BitConverter.Int32BitsToSingle(unchecked((int)vm.GlobalFloats[address]));

    [Fact]
    public void FourFortyThreeBsAbsoluteValueIsAFloatsFabs()
    {
        // op_0x191_absolute_value@0x42b140: the float read, rounded to a float, its fabs through the float accessor.
        VirtualMachine vm = Run("SYS4443", (AbsoluteValue, [GF(1), F(-2.75f)]), (AbsoluteValue, [GF(2), I(-7)]),
                                (AbsoluteValue, [G(0x300), F(-3.5f)]));
        Assert.Equal(2.75f, FloatCell(vm, 1));
        Assert.Equal(7f, FloatCell(vm, 2));
        Assert.Equal(3L, vm.Globals[0x300]);                // an integer destination takes it truncated

        // 4.38D's works on integers: int.MinValue stays itself.
        vm = Run("SYS4438", (AbsoluteValue, [G(0x300), I(-9)]), (AbsoluteValue, [G(0x301), I(int.MinValue)]));
        Assert.Equal(9L, vm.Globals[0x300]);
        Assert.Equal((long)int.MinValue, vm.Globals[0x301]);
    }
}
