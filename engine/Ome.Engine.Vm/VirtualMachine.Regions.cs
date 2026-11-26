using Ome.Engine.Model;

namespace Ome.Engine.Vm;

// AGE 4.43B's region tests, ops 0x2ef to 0x2f2 (docs/engine-re.md, "The region tests"). Each builds GDI regions from
// cells, as native does through gdi32, and writes 1 when two share a pixel or the region holds the point: a polygon
// from an x array and a y array, each cell plus an offset (CreatePolygonRgn, winding); an ellipse from four cells read
// as (left, right, top, bottom), the first two plus an x offset and the last two plus a y one (CreateEllipticRgnIndirect).
// Native reads every operand first; a region GDI refuses traces an error and writes 0.
public sealed partial class VirtualMachine
{
    // More shared rows than this would take the port seconds to test; native's GDI has its own limit, not measured.
    private const long MaxRegionTestRows = 1 << 24;

    private const string PolygonRegionTrace = "多角形リージョン作成に失敗しました．";
    private const string EllipseRegionTrace = "楕円形リージョン作成に失敗しました．";

    private readonly record struct PolygonOperands(VmAddress Xs, VmAddress Ys, int Count, int Dx, int Dy);

    private readonly record struct EllipseOperands(VmAddress Cells, int Dx, int Dy);

    // 0x2ef (out)(xs)(ys)(count)(dx)(dy)(xs2)(ys2)(count2)(dx2)(dy2): two polygons share a pixel
    // (op_0x2ef_polygons_intersect@0x42f530).
    private StepResult OpPolygonsIntersect(Instruction ins, int pc)
    {
        var a = ins.Args;
        PolygonOperands first = ReadPolygonOperands(a, 1), second = ReadPolygonOperands(a, 6);
        if (BuildPolygonRegion(ins, first, out GdiRegion? one) is { } haltOne) return haltOne;
        if (one == null) return RegionRefused(ins, PolygonRegionTrace);
        if (BuildPolygonRegion(ins, second, out GdiRegion? two) is { } haltTwo) return haltTwo;
        if (two == null) return RegionRefused(ins, PolygonRegionTrace);
        return WriteRegionsShareAPixel(ins, one, two);
    }

    // 0x2f0 (out)(rectangle cells)(dx)(dy)(xs)(ys)(count)(px)(py): an ellipse and a polygon share a pixel
    // (op_0x2f0_ellipse_polygon_intersect@0x42f720). The ellipse is made first.
    private StepResult OpEllipsePolygonIntersect(Instruction ins, int pc)
    {
        var a = ins.Args;
        EllipseOperands ellipse = ReadEllipseOperands(a, 1);
        PolygonOperands polygon = ReadPolygonOperands(a, 4);
        GdiRegion? one = BuildEllipseRegion(ellipse);
        if (one == null) return RegionRefused(ins, EllipseRegionTrace);
        if (BuildPolygonRegion(ins, polygon, out GdiRegion? two) is { } halt) return halt;
        if (two == null) return RegionRefused(ins, PolygonRegionTrace);
        return WriteRegionsShareAPixel(ins, one, two);
    }

    // 0x2f1 (out)(rectangle cells)(dx)(dy)(rectangle cells 2)(dx2)(dy2): two ellipses share a pixel
    // (op_0x2f1_ellipses_intersect@0x42f9a0).
    private StepResult OpEllipsesIntersect(Instruction ins, int pc)
    {
        var a = ins.Args;
        EllipseOperands first = ReadEllipseOperands(a, 1), second = ReadEllipseOperands(a, 4);
        GdiRegion? one = BuildEllipseRegion(first);
        if (one == null) return RegionRefused(ins, EllipseRegionTrace);
        GdiRegion? two = BuildEllipseRegion(second);
        if (two == null) return RegionRefused(ins, EllipseRegionTrace);
        return WriteRegionsShareAPixel(ins, one, two);
    }

    // 0x2f2 (out)(x)(y)(rectangle cells)(dx)(dy): the ellipse holds the pixel (x, y) (op_0x2f2_point_in_ellipse@0x42fb60,
    // PtInRegion).
    private StepResult OpPointInEllipse(Instruction ins, int pc)
    {
        var a = ins.Args;
        int x = unchecked((int)Read(a[1])), y = unchecked((int)Read(a[2]));
        GdiRegion? region = BuildEllipseRegion(ReadEllipseOperands(a, 3));
        if (region == null) return RegionRefused(ins, EllipseRegionTrace);
        Write(a[0], region.Contains(x, y) ? 1 : 0);
        return StepResult.Next;
    }

    private PolygonOperands ReadPolygonOperands(IReadOnlyList<Operand> a, int first)
        => new(BaseAddr(a[first]), BaseAddr(a[first + 1]), unchecked((int)Read(a[first + 2])),
               unchecked((int)Read(a[first + 3])), unchecked((int)Read(a[first + 4])));

    private EllipseOperands ReadEllipseOperands(IReadOnlyList<Operand> a, int first)
        => new(BaseAddr(a[first]), unchecked((int)Read(a[first + 1])), unchecked((int)Read(a[first + 2])));

    // Native allocates count points before CreatePolygonRgn, which refuses fewer than two. A negative count asks
    // operator new for more than 4 GiB, which the port does not follow.
    private StepResult? BuildPolygonRegion(Instruction ins, PolygonOperands polygon, out GdiRegion? region)
    {
        region = null;
        if (polygon.Count < 0 || polygon.Count > MaxScriptRunLength)
        {
            HaltReason ??= $"region-polygon-count-out-of-range:{polygon.Count}";
            return StepResult.Halt;
        }
        var points = new (int X, int Y)[polygon.Count];
        for (int index = 0; index < points.Length; index++)
            points[index] = (unchecked((int)ReadIntCell(polygon.Xs.Offset(index)) + polygon.Dx),
                             unchecked((int)ReadIntCell(polygon.Ys.Offset(index)) + polygon.Dy));
        region = GdiRegion.Polygon(points);
        return null;
    }

    private GdiRegion? BuildEllipseRegion(EllipseOperands ellipse)
    {
        int Cell(int index) => unchecked((int)ReadIntCell(ellipse.Cells.Offset(index)));
        return GdiRegion.Ellipse(unchecked(Cell(0) + ellipse.Dx), unchecked(Cell(2) + ellipse.Dy),
                                 unchecked(Cell(1) + ellipse.Dx), unchecked(Cell(3) + ellipse.Dy));
    }

    private StepResult RegionRefused(Instruction ins, string trace)
    {
        _waits.ReportWarning($"{At(ins)}: GDI refuses the region; 0, as AGE writes it with the trace 「{trace}」");
        Write(ins.Args[0], 0);
        return StepResult.Next;
    }

    private StepResult WriteRegionsShareAPixel(Instruction ins, GdiRegion one, GdiRegion two)
    {
        long rows = GdiRegion.SharedRows(one, two);
        if (rows > MaxRegionTestRows)
        {
            HaltReason ??= $"region-test-too-large:{rows}-rows";
            return StepResult.Halt;
        }
        Write(ins.Args[0], one.Intersects(two) ? 1 : 0);
        return StepResult.Next;
    }
}
