using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// AGE 4.43B's region tests, ops 0x2ef to 0x2f2 (docs/engine-re.md, "The region tests"). Each expected answer is what
/// gdi32 gave for the same regions on 2026-11-26.
/// </summary>
public class RegionOpsTests
{
    private const int Imm = 0, GlobalInt = 3;
    private const int Exit = 0x2, Mov = 0x55;
    private const int PolygonsIntersect = 0x2ef, EllipsePolygonIntersect = 0x2f0, EllipsesIntersect = 0x2f1,
                      PointInEllipse = 0x2f2;
    private const int Result = 0x10, Ellipse = 0x100, Ellipse2 = 0x110, Xs = 0x200, Ys = 0x210, Xs2 = 0x220, Ys2 = 0x230;

    private static Operand I(long value) => new(Imm, value);
    private static Operand G(int address) => new(GlobalInt, address);

    // An ellipse's four cells, read left, right, top, bottom.
    private static IEnumerable<(int, Operand[])> Box(int at, int left, int top, int right, int bottom)
        => [(Mov, [G(at), I(left)]), (Mov, [G(at + 1), I(right)]), (Mov, [G(at + 2), I(top)]), (Mov, [G(at + 3), I(bottom)])];

    private static IEnumerable<(int, Operand[])> Points(int xs, int ys, params (int X, int Y)[] points)
        => points.SelectMany((point, index) => new (int, Operand[])[]
        {
            (Mov, [G(xs + index), I(point.X)]), (Mov, [G(ys + index), I(point.Y)]),
        });

    private static (int, Operand[]) Op(int op, params Operand[] operands) => (op, operands);

    private static VirtualMachine Run(RecordingWaits waits, params IEnumerable<(int, Operand[])>[] parts)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4443");
        var ops = parts.SelectMany(part => part).Append((Exit, Array.Empty<Operand>())).ToList();
        var vm = new VirtualMachine(ScriptAssembler.Assemble(table, "REGIONS.BIN", ops, []), table, waits);
        vm.Run();
        return vm;
    }

    private static long EllipsesAt(int apart)
    {
        VirtualMachine vm = Run(new RecordingWaits(),
            Box(Ellipse, -10, -10, 10, 10), Box(Ellipse2, -10, -10, 10, 10),
            [Op(EllipsesIntersect, G(Result), G(Ellipse), I(0), I(0), G(Ellipse2), I(apart), I(0))]);
        Assert.Equal("exit", vm.HaltReason);
        return vm.Globals[Result];
    }

    [Fact]
    public void TwoCirclesShareAPixelUpToEighteenApartAndNotFromNineteen()
    {
        // Two 20-pixel circles: GDI's regions overlap up to 18 pixels apart, the second moved by its x offset.
        Assert.Equal(1, EllipsesAt(18));
        Assert.Equal(0, EllipsesAt(19));
        Assert.Equal(0, EllipsesAt(20));
    }

    [Theory]
    [InlineData(-10, 0, 1)]
    [InlineData(8, 0, 1)]
    [InlineData(9, 0, 0)]       // the right edge is outside
    [InlineData(-11, 0, 0)]
    [InlineData(0, 8, 1)]
    [InlineData(0, 9, 0)]
    public void APointIsInTheCircleAsPtInRegionHasIt(int x, int y, long inside)
    {
        VirtualMachine vm = Run(new RecordingWaits(),
            Box(Ellipse, -20, -10, 0, 10),
            [Op(PointInEllipse, G(Result), I(x), I(y), G(Ellipse), I(10), I(0))]);
        Assert.Equal(inside, vm.Globals[Result]);
    }

    [Theory]
    [InlineData(8, 1)]
    [InlineData(9, 0)]
    public void AnEllipseAndAPolygonShareAPixelAsGdisRegionsDo(int squareLeft, long shared)
    {
        // The circle (-10, -10) to (10, 10) against a 5 by 4 square, its points moved by the polygon's x offset.
        VirtualMachine vm = Run(new RecordingWaits(),
            Box(Ellipse, -10, -10, 10, 10),
            Points(Xs, Ys, (0, -2), (5, -2), (5, 2), (0, 2)),
            [Op(EllipsePolygonIntersect, G(Result), G(Ellipse), I(0), I(0), G(Xs), G(Ys), I(4), I(squareLeft), I(0))]);
        Assert.Equal(shared, vm.Globals[Result]);
    }

    [Fact]
    public void PolygonsThatOnlyTouchShareNoPixel()
    {
        IEnumerable<(int, Operand[])> square = Points(Xs, Ys, (0, 0), (10, 0), (10, 10), (0, 10));
        VirtualMachine touching = Run(new RecordingWaits(), square, Points(Xs2, Ys2, (0, 0), (10, 0), (10, 10), (0, 10)),
            [Op(PolygonsIntersect, G(Result), G(Xs), G(Ys), I(4), I(0), I(0), G(Xs2), G(Ys2), I(4), I(10), I(0))]);
        Assert.Equal(0, touching.Globals[Result]);

        VirtualMachine overlapping = Run(new RecordingWaits(), square, Points(Xs2, Ys2, (0, 0), (11, 0), (11, 11), (0, 11)),
            [Op(PolygonsIntersect, G(Result), G(Xs), G(Ys), I(4), I(0), I(0), G(Xs2), G(Ys2), I(4), I(9), I(9))]);
        Assert.Equal(1, overlapping.Globals[Result]);
    }

    [Fact]
    public void ARegionGdiRefusesTracesNativesErrorAndWritesZero()
    {
        var waits = new RecordingWaits();
        VirtualMachine vm = Run(waits,
            [(Mov, [G(Result), I(7)]), (Mov, [G(Result + 1), I(7)])],
            Box(Ellipse, 0, 0, 1 << 27, 10),
            Points(Xs, Ys, (0, 0)),
            [
                Op(PointInEllipse, G(Result), I(0), I(0), G(Ellipse), I(0), I(0)),
                Op(PolygonsIntersect, G(Result + 1), G(Xs), G(Ys), I(1), I(0), I(0), G(Xs), G(Ys), I(1), I(0), I(0)),
            ]);
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(0, vm.Globals[Result]);
        Assert.Equal(0, vm.Globals[Result + 1]);
        Assert.Contains(waits.Warnings, warning => warning.Contains("楕円形リージョン作成に失敗しました．"));
        Assert.Contains(waits.Warnings, warning => warning.Contains("多角形リージョン作成に失敗しました．"));
    }

    [Fact]
    public void ANegativePointCountStopsTheRunWhereNativeAsksForMoreThanFourGigabytes()
    {
        VirtualMachine vm = Run(new RecordingWaits(),
            Box(Ellipse, -10, -10, 10, 10),
            [Op(EllipsePolygonIntersect, G(Result), G(Ellipse), I(0), I(0), G(Xs), G(Ys), I(-1), I(0), I(0))]);
        Assert.Equal("region-polygon-count-out-of-range:-1", vm.HaltReason);
    }
}
