using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

public class IndexedSortOpsTests
{
    private const int Immediate = 0, GlobalInt = 3, LocalInt = 9;

    [Fact]
    public void SortIndicesByKeySum_IsStableAndUsesSignedInt32Overflow()
    {
        var table = OpcodeTableJson.Load(Paths.OpcodesJson);
        int move = table.ByLabel("mov")!.Value;
        var ops = new List<(int, Operand[])>();

        long[] secondaryKeys = { -1, 1, -1, -2, 1 };
        for (int i = 0; i < secondaryKeys.Length; i++)
            ops.Add((move, new[] { new Operand(LocalInt, 20 + i), new Operand(Immediate, secondaryKeys[i]) }));

        ops.Add((0x12f, new[]
        {
            new Operand(LocalInt, 0),
            new Operand(GlobalInt, 0x100),
            new Operand(LocalInt, 20),
            new Operand(Immediate, secondaryKeys.Length),
        }));
        for (int i = 0; i < secondaryKeys.Length; i++)
            ops.Add((move, new[] { new Operand(GlobalInt, 0x200 + i), new Operand(LocalInt, i) }));
        ops.Add((0x2, Array.Empty<Operand>()));

        var vm = new VirtualMachine(
            ScriptAssembler.Assemble(table, "INDEX_SORT", ops, Array.Empty<string>()),
            table, new RecordingWaits());
        long[] primaryKeys = { 4, -2, 4, 1, int.MaxValue };
        for (int i = 0; i < primaryKeys.Length; i++) vm.SeedGlobal(0x100 + i, primaryKeys[i]);

        vm.Run();

        // Sums are 3, -1, 3, -1, and int.MinValue after native signed 32-bit overflow.
        // Equal -1 and 3 pairs retain their original relative order.
        Assert.Equal(new long[] { 4, 1, 3, 0, 2 },
            Enumerable.Range(0, 5).Select(i => vm.Globals[0x200 + i]).ToArray());
    }

    [Fact]
    public void SortIndicesByKeySum_MatchesNativeInsertionSort()
    {
        // Native's handler (op_0x12f_sort_indices_by_key_sum): an insertion sort that shifts an index only past a
        // strictly greater signed 32-bit key sum. The port sorts by (sum, index) instead; over many ties, both
        // extremes and sums that overflow, the two orders must be the same.
        const int count = 3000;
        var random = new Random(12);
        long[] primary = new long[count], secondary = new long[count];
        long[] extremes = { int.MinValue, int.MaxValue, -1, 0, 1 };
        for (int i = 0; i < count; i++)
        {
            primary[i] = random.Next(8) == 0 ? extremes[random.Next(extremes.Length)] : random.Next(-6, 7);
            secondary[i] = random.Next(8) == 0 ? extremes[random.Next(extremes.Length)] : random.Next(-3, 4) << 24;
        }

        var expected = new int[count];
        for (int source = 1; source < count; source++)
        {
            int key = unchecked((int)primary[source] + (int)secondary[source]);
            int position = source;
            while (position > 0)
            {
                int previous = expected[position - 1];
                if (key >= unchecked((int)primary[previous] + (int)secondary[previous])) break;
                expected[position] = previous;
                position--;
            }
            expected[position] = source;
        }

        var table = OpcodeTableJson.Load(Paths.OpcodesJson);
        int move = table.ByLabel("mov")!.Value;
        var ops = new List<(int, Operand[])>
        {
            (0x12f, new[]
            {
                new Operand(LocalInt, 0),
                new Operand(GlobalInt, 0x1000),
                new Operand(GlobalInt, 0x2000),
                new Operand(Immediate, count),
            }),
        };
        for (int i = 0; i < count; i++)
            ops.Add((move, new[] { new Operand(GlobalInt, 0x4000 + i), new Operand(LocalInt, i) }));
        ops.Add((0x2, Array.Empty<Operand>()));
        var vm = new VirtualMachine(
            ScriptAssembler.Assemble(table, "INDEX_SORT_NATIVE", ops, Array.Empty<string>()),
            table, new RecordingWaits(), new VmOptions(MaxSteps: 10_000));
        for (int i = 0; i < count; i++)
        {
            vm.SeedGlobal(0x1000 + i, primary[i]);
            vm.SeedGlobal(0x2000 + i, secondary[i]);
        }

        vm.Run();

        Assert.Equal(expected.Select(index => (long)index).ToArray(),
            Enumerable.Range(0, count).Select(i => vm.Globals[0x4000 + i]).ToArray());
    }

    [Fact]
    public void SortIndicesByKeySum_WritesInitialIndexWhenCountIsZero()
    {
        var table = OpcodeTableJson.Load(Paths.OpcodesJson);
        int move = table.ByLabel("mov")!.Value;
        var script = ScriptAssembler.Assemble(table, "EMPTY_INDEX_SORT", new List<(int, Operand[])>
        {
            (move, new[] { new Operand(LocalInt, 0), new Operand(Immediate, 99) }),
            (0x12f, new[]
            {
                new Operand(LocalInt, 0), new Operand(LocalInt, 10),
                new Operand(LocalInt, 20), new Operand(Immediate, 0),
            }),
            (move, new[] { new Operand(GlobalInt, 0x200), new Operand(LocalInt, 0) }),
            (0x2, Array.Empty<Operand>()),
        }, Array.Empty<string>());
        var vm = new VirtualMachine(script, table, new RecordingWaits());

        vm.Run();

        Assert.Equal(0, vm.Globals[0x200]);
    }
}
