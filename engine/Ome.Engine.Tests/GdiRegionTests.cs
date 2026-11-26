using Ome.Engine.Model;
using Xunit;

namespace Ome.Engine.Tests;

/// <summary>
/// GdiRegion against Windows 10's gdi32: each expected row list is what GetRegionData returned for the same call on
/// 2026-11-26 (CreatePolygonRgn in winding mode, CreateEllipticRgnIndirect).
/// </summary>
public class GdiRegionTests
{
    private static string Rows(GdiRegion region)
    {
        var parts = new List<string>();
        var spans = new List<(long Left, long Right)>();
        for (long y = region.Top; y < region.Bottom; y++)
        {
            spans.Clear();
            region.Spans(y, spans);
            if (spans.Count > 0) parts.Add($"{y}:" + string.Join(",", spans.Select(span => $"{span.Left}-{span.Right}")));
        }
        return string.Join(" ", parts);
    }

    private static GdiRegion Polygon(params (int X, int Y)[] points) => GdiRegion.Polygon(points)!;

    [Theory]
    [InlineData(new[] { 2, 8, 3, 15, 14, 15, 12, 6 }, "7:7-13 8:2-13 9:3-13 10:3-13 11:3-14 12:3-14 13:3-14 14:3-14")]
    [InlineData(new[] { 0, 0, 10, 10, 0, 10, 10, 0 }, "0:0-10 1:1-9 2:2-8 3:3-7 4:4-6 6:4-6 7:3-7 8:2-8 9:1-9")]
    [InlineData(new[] { 5, 0, 10, 10, 0, 10 }, "1:5-6 2:4-6 3:4-7 4:3-7 5:3-8 6:2-8 7:2-9 8:1-9 9:1-10")]
    [InlineData(new[] { 0, 0, 5, 5 }, "")]
    [InlineData(new[] { -7, -3, 4, -9, 6, 5, -2, 2, 0, -1 },
                "-8:3-5 -7:1-5 -6:-1-5 -5:-3-5 -4:-5-5 -3:-7-5 -2:-3-5 -1:0-6 0:0-6 1:-1-6 2:-2-6 3:1-6 4:4-6")]
    [InlineData(new[] { 0, 0, 12, 3, 2, 9, 9, -2, 4, 11 },
                "0:8-9 1:1-4 2:1-8 3:2-12 4:2-11 5:2-9 6:3-7 7:3-6 8:3-6 9:4-5 10:4-5")]
    public void APolygonHoldsTheSamePixelsAsGdisWindingRegion(int[] coordinates, string gdiRows)
    {
        var points = new (int X, int Y)[coordinates.Length / 2];
        for (int index = 0; index < points.Length; index++) points[index] = (coordinates[2 * index], coordinates[2 * index + 1]);
        Assert.Equal(gdiRows, Rows(GdiRegion.Polygon(points)!));
    }

    [Fact]
    public void GdiRefusesAPolygonOfFewerThanTwoPoints()
    {
        Assert.Null(GdiRegion.Polygon([]));
        Assert.Null(GdiRegion.Polygon([(1, 1)]));
        Assert.NotNull(GdiRegion.Polygon([(1, 1), (4, 4)]));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, "-1:-1-0")]          // an empty rectangle: the pixel at (left - 1, top - 1)
    [InlineData(0, 0, 2, 5, "0:0-1 1:0-1 2:0-1 3:0-1")]
    [InlineData(0, 0, 5, 2, "0:0-4")]
    [InlineData(0, 0, 1, 4, "")]
    [InlineData(0, 0, 3, 3, "")]
    [InlineData(3, 4, 9, 9, "4:5-6 5:4-7 6:4-7 7:5-6")]
    [InlineData(0, 0, 10, 10, "0:4-5 1:1-7 2:1-8 3:1-8 4:0-9 5:1-8 6:1-8 7:1-7 8:4-5")]   // not symmetric: row 1
    [InlineData(0, 0, 20, 9, "0:8-11 1:3-16 2:1-18 3:1-18 4:1-18 5:1-18 6:3-16 7:8-11")]
    [InlineData(12, 20, -7, 3, "3:1-3 4:-2-6 5:-4-8 6:-5-9 7:-6-10 8:-6-10 9:-6-10 10:-7-11 11:-7-11 12:-6-10 " +
                               "13:-6-10 14:-6-10 15:-5-9 16:-4-8 17:-2-6 18:1-3")]
    [InlineData(100, 50, 131, 64, "50:112-118 51:107-123 52:104-126 53:102-128 54:101-129 55:101-129 56:100-130 " +
                                  "57:101-129 58:101-129 59:102-128 60:104-126 61:107-123 62:112-118")]
    public void AnEllipseHoldsTheSamePixelsAsGdisEllipticRegion(int left, int top, int right, int bottom, string gdiRows)
    {
        GdiRegion ellipse = GdiRegion.Ellipse(left, top, right, bottom)!;
        Assert.Equal(gdiRows, Rows(ellipse));
        Assert.False(ellipse.Approximate);
    }

    [Fact]
    public void TheFlattenerGivesGdisPointsForAQuarterOfAnEllipse()
    {
        // GDI's FlattenPath of the ellipse path with the box (-4, -4) to (132, 132) in 28.4, relative to the box:
        // (136, 68), (116, 20), (68, 0) for the first quarter, the midpoint step rounded to the nearest unit.
        var quarters = GdiCurveFlattener.EllipseQuarters(0, 0, 136, 136);
        Assert.Equal([(136L, 68L), (136L, 31L), (106L, 0L), (68L, 0L)], quarters[0]);
        var points = new List<(long X, long Y)>();
        Assert.True(GdiCurveFlattener.Flatten(quarters[0], points));
        Assert.Equal([(116L, 20L), (68L, 0L)], points);

        // 201 by 133 pixels: steps of 1/16, then 1/8 from a quarter on.
        quarters = GdiCurveFlattener.EllipseQuarters(0, 0, 3192, 2104);
        points.Clear();
        Assert.True(GdiCurveFlattener.Flatten(quarters[0], points));
        Assert.Equal([(3184L, 944L), (3160L, 840L), (3067L, 643L), (2920L, 464L), (2725L, 308L)], points.Take(5));
    }

    [Fact]
    public void AnEllipseWithAQuarterTooWideForGdisThirtyTwoBitFlattenerIsMarkedApproximate()
    {
        Assert.False(GdiRegion.Ellipse(0, 0, 2048, 2048)!.Approximate);
        Assert.True(GdiRegion.Ellipse(0, 0, 2060, 50)!.Approximate);
    }

    [Fact]
    public void GdiRefusesAnEllipseOutsideItsCoordinateRange()
    {
        Assert.NotNull(GdiRegion.Ellipse(GdiRegion.SmallestCoordinate, 0, GdiRegion.LargestCoordinate, 10));
        Assert.Null(GdiRegion.Ellipse(GdiRegion.SmallestCoordinate - 1, 0, 0, 10));
        Assert.Null(GdiRegion.Ellipse(0, 0, 10, GdiRegion.LargestCoordinate + 1));
    }

    [Fact]
    public void RegionsThatOnlyTouchShareNoPixelAndAPointOnTheRightOrBottomEdgeIsOutside()
    {
        // GDI: two rectangles side by side give NULLREGION; a region's right and bottom edges are not in it.
        GdiRegion left = Polygon((0, 0), (10, 0), (10, 10), (0, 10)), right = Polygon((10, 0), (20, 0), (20, 10), (10, 10));
        Assert.False(left.Intersects(right));
        Assert.True(left.Intersects(Polygon((9, 9), (20, 9), (20, 20), (9, 20))));
        Assert.True(left.Contains(0, 0));
        Assert.True(left.Contains(9, 9));
        Assert.False(left.Contains(10, 5));
        Assert.False(left.Contains(5, 10));

        GdiRegion ellipse = GdiRegion.Ellipse(0, 0, 10, 10)!;
        Assert.True(ellipse.Contains(4, 0));
        Assert.False(ellipse.Contains(0, 0));
        Assert.True(ellipse.Intersects(Polygon((0, 4), (3, 4), (3, 5), (0, 5))));
        Assert.False(ellipse.Intersects(Polygon((-5, -5), (0, -5), (0, 0), (-5, 0))));
    }

    [Fact]
    public void CoordinatesAtTheEdgesOfTheIntRangeDoNotOverflow()
    {
        GdiRegion wide = Polygon((int.MinValue, 0), (int.MaxValue, 0), (int.MaxValue, 2), (int.MinValue, 2));
        var spans = new List<(long Left, long Right)>();
        wide.Spans(1, spans);
        Assert.Equal([((long)int.MinValue, (long)int.MaxValue)], spans);
    }
}
