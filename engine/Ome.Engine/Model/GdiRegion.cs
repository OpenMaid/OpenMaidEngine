namespace Ome.Engine.Model;

/// <summary>
/// A GDI region as AGE 4.43B's region tests build one (ops 0x2ef to 0x2f2; docs/engine-re.md, "The region tests"):
/// CreatePolygonRgn's in winding mode and CreateEllipticRgnIndirect's, pixel for pixel. A region is a polygon whose
/// corners are in whole pixels (a polygon) or in 28.4 fixed point (an ellipse, GDI's flattened path); a pixel (x, y) is
/// in when the point (x, y) has a winding number other than 0, where an edge counts on the rows from its upper end to
/// before its lower one and its crossing of row y counts when it is at or left of x. That rule matched gdi32 on 708
/// random integer polygons and 120 fixed-point ones (2026-11-26).
/// </summary>
public sealed class GdiRegion
{
    /// <summary>CreateEllipticRgnIndirect refuses a coordinate outside this range, measured on Windows 10's gdi32: in
    /// 28.4 fixed point it would not fit 32 bits.</summary>
    public const int SmallestCoordinate = -(1 << 27), LargestCoordinate = (1 << 27) - 1;

    private readonly (long X, long Y)[] _corners;
    private readonly int _fractionBits;   // 0: whole pixels; 4: 28.4 fixed point

    private GdiRegion((long X, long Y)[] corners, int fractionBits)
    {
        _corners = corners;
        _fractionBits = fractionBits;
        long top = long.MaxValue, bottom = long.MinValue;
        foreach ((_, long y) in corners)
        {
            top = Math.Min(top, y);
            bottom = Math.Max(bottom, y);
        }
        Top = CeilingShift(top, fractionBits);
        Bottom = CeilingShift(bottom, fractionBits);
    }

    /// <summary>The first row that can hold a pixel.</summary>
    public long Top { get; }

    /// <summary>The row past the last that can hold one.</summary>
    public long Bottom { get; }

    /// <summary>The ellipse needed GDI's 64-bit flattener, which the port does not have: a quarter of it is 1024
    /// pixels or more across. Its points are the 32-bit flattener's, worked in 64 bits (docs/backlog.md, "GDI's
    /// 64-bit curve flattener").</summary>
    public bool Approximate { get; private init; }

    /// <summary>CreatePolygonRgn(points, count, WINDING). Null where GDI fails: fewer than two points.</summary>
    public static GdiRegion? Polygon(IReadOnlyList<(int X, int Y)> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 2) return null;
        var corners = new (long X, long Y)[points.Count];
        for (int index = 0; index < corners.Length; index++) corners[index] = (points[index].X, points[index].Y);
        return new GdiRegion(corners, fractionBits: 0);
    }

    /// <summary>
    /// CreateEllipticRgnIndirect({left, top, right, bottom}), which is the region of an ellipse path (measured against
    /// gdi32 on 1420 sizes, 2026-11-26): GDI normalizes the rectangle, takes the box from 28.4 point (16 left - 4,
    /// 16 top - 4) to (16 right - 28, 16 bottom - 28), normalized again, draws it as four Bézier quarters
    /// (<see cref="GdiCurveFlattener.EllipseQuarters"/>) and flattens them (<see cref="GdiCurveFlattener.Flatten"/>).
    /// Null where GDI fails: a coordinate outside <see cref="SmallestCoordinate"/> to <see cref="LargestCoordinate"/>.
    /// </summary>
    public static GdiRegion? Ellipse(int left, int top, int right, int bottom)
    {
        if (!InRange(left) || !InRange(top) || !InRange(right) || !InRange(bottom)) return null;
        if (left > right) (left, right) = (right, left);
        if (top > bottom) (top, bottom) = (bottom, top);
        long x0 = 16L * left - 4, x1 = 16L * right - 28, y0 = 16L * top - 4, y1 = 16L * bottom - 28;
        if (x0 > x1) (x0, x1) = (x1, x0);
        if (y0 > y1) (y0, y1) = (y1, y0);
        var quarters = GdiCurveFlattener.EllipseQuarters(x0, y0, x1, y1);
        var corners = new List<(long X, long Y)> { quarters[0][0] };
        bool approximate = false;
        foreach (var quarter in quarters) approximate |= !GdiCurveFlattener.Flatten(quarter, corners);
        return new GdiRegion([.. corners], fractionBits: 4) { Approximate = approximate };

        static bool InRange(int value) => value is >= SmallestCoordinate and <= LargestCoordinate;
    }

    /// <summary>PtInRegion.</summary>
    public bool Contains(int x, int y)
    {
        if (y < Top || y >= Bottom) return false;
        var spans = new List<(long Left, long Right)>();
        Spans(y, spans);
        foreach ((long left, long right) in spans)
            if (x >= left && x < right) return true;
        return false;
    }

    /// <summary>The rows both regions can hold a pixel in: the most rows <see cref="Intersects"/> reads.</summary>
    public static long SharedRows(GdiRegion a, GdiRegion b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));
    }

    /// <summary>CombineRgn(RGN_AND) leaves a region that is not NULLREGION: the two share a pixel.</summary>
    public bool Intersects(GdiRegion other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var mine = new List<(long Left, long Right)>();
        var theirs = new List<(long Left, long Right)>();
        for (long y = Math.Max(Top, other.Top); y < Math.Min(Bottom, other.Bottom); y++)
        {
            mine.Clear();
            Spans(y, mine);
            if (mine.Count == 0) continue;
            theirs.Clear();
            other.Spans(y, theirs);
            int i = 0, j = 0;
            while (i < mine.Count && j < theirs.Count)
            {
                if (mine[i].Left < theirs[j].Right && theirs[j].Left < mine[i].Right) return true;
                if (mine[i].Right <= theirs[j].Right) i++;
                else j++;
            }
        }
        return false;
    }

    /// <summary>Row <paramref name="y"/>'s pixels, as spans [left, right) from left to right, none touching.</summary>
    public void Spans(long y, List<(long Left, long Right)> spans)
    {
        ArgumentNullException.ThrowIfNull(spans);
        if (y < Top || y >= Bottom) return;
        long sample = y << _fractionBits;
        var crossings = new List<(long X, int Direction)>();
        for (int index = 0; index < _corners.Length; index++)
        {
            (long x0, long y0) = _corners[index];
            (long x1, long y1) = _corners[(index + 1) % _corners.Length];
            if (y0 == y1 || sample < Math.Min(y0, y1) || sample >= Math.Max(y0, y1)) continue;
            // The crossing x0 + (x1 - x0)(sample - y0)/(y1 - y0), in pixels: the first whole x at or right of it.
            Int128 numerator = (Int128)x0 * (y1 - y0) + (Int128)(x1 - x0) * (sample - y0);
            Int128 denominator = (Int128)(y1 - y0) << _fractionBits;
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            crossings.Add(((long)CeilingDivide(numerator, denominator), y1 > y0 ? 1 : -1));
        }
        crossings.Sort((a, b) => a.X.CompareTo(b.X));
        int winding = 0;
        long start = 0;
        for (int index = 0; index < crossings.Count;)
        {
            long x = crossings[index].X;
            int before = winding;
            for (; index < crossings.Count && crossings[index].X == x; index++) winding += crossings[index].Direction;
            if (before == 0 && winding != 0) start = x;
            else if (before != 0 && winding == 0 && x > start) spans.Add((start, x));
        }
    }

    private static long CeilingShift(long value, int bits) => -((-value) >> bits);

    private static Int128 CeilingDivide(Int128 numerator, Int128 denominator)
    {
        Int128 quotient = Int128.DivRem(numerator, denominator).Quotient;
        return quotient * denominator < numerator ? quotient + 1 : quotient;
    }
}

/// <summary>
/// GDI's flattening of a path's Bézier curves into lines, as Windows 10's kernel GDI does it for a region
/// (win32kbase.sys 10.0.19041: the 32-bit forward-differencing flattener, its setup at RVA 0x83060 and its step at
/// RVA 0x82dd0; docs/engine-re.md, "The region tests"). Coordinates are 28.4 fixed point.
/// </summary>
public static class GdiCurveFlattener
{
    /// <summary>The ellipse's Bézier constant, 4(√2 - 1)/3: GDI's offsets fit it to within 2e-7 (2026-11-26).</summary>
    private static readonly double Kappa = 4 * (Math.Sqrt(2) - 1) / 3;

    /// <summary>
    /// The four Bézier quarters of the ellipse path in the box (x0, y0) to (x1, y1), anticlockwise on screen from the
    /// right end: the control points sit ceil(rx κ) across and floor(ry κ) up or down from the ends, matched against
    /// GDI's path on every width from 0 to 2059 pixels.
    /// </summary>
    public static (long X, long Y)[][] EllipseQuarters(long x0, long y0, long x1, long y1)
    {
        long rx = (x1 - x0) / 2, ry = (y1 - y0) / 2;
        long ox = (long)Math.Ceiling(rx * Kappa), oy = (long)Math.Floor(ry * Kappa);
        long cx = x0 + rx, cy = y0 + ry;
        return
        [
            [(x1, cy), (x1, cy - oy), (cx + ox, y0), (cx, y0)],
            [(cx, y0), (cx - ox, y0), (x0, cy - oy), (x0, cy)],
            [(x0, cy), (x0, cy + oy), (cx - ox, y1), (cx, y1)],
            [(cx, y1), (cx + ox, y1), (x1, cy + oy), (x1, cy)],
        ];
    }

    /// <summary>
    /// Adds the curve's points after its start to <paramref name="points"/>. The flattener works relative to the
    /// curve's bounds; it refuses a curve whose bounds are 0x4000 or more across (GDI then uses its 64-bit flattener),
    /// and for that the port runs the same steps in 64 bits and returns false.
    /// <para>Setup, in 18.14: e0 = p0, e1 = p3 - p0, e2 = 6(p1 - 2p2 + p3), e3 = 6(p0 - 2p1 + p2). While the larger
    /// |e2| or |e3| of either axis is over 0xffc0 shifted left by the count of halvings so far times two, it halves the
    /// step lazily, leaving e3 and the scale of e2 where they are. It then moves to 15.17 and takes the first step.
    /// Each later step: give the point, (e0 + 0x1000) >> 13; stop when no step is left; halve once when an error is
    /// over 0x7fe00; double while the steps left are even and neither axis's max(|e3|, |2 e2 - e3|) is over 0x1ff80;
    /// step.</para>
    /// </summary>
    public static bool Flatten(IReadOnlyList<(long X, long Y)> curve, List<(long X, long Y)> points)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(points);
        long originX = long.MaxValue, originY = long.MaxValue;
        foreach ((long x, long y) in curve)
        {
            originX = Math.Min(originX, x);
            originY = Math.Min(originY, y);
        }
        bool fits = true;
        foreach ((long x, long y) in curve) fits &= x - originX < 0x4000 && y - originY < 0x4000;

        var bx = new Basis(curve[0].X - originX, curve[1].X - originX, curve[2].X - originX, curve[3].X - originX);
        var by = new Basis(curve[0].Y - originY, curve[1].Y - originY, curve[2].Y - originY, curve[3].Y - originY);
        long steps = 1;
        int shift = 0;
        while (bx.Error > (0xffc0L << shift) || by.Error > (0xffc0L << shift))
        {
            shift += 2;
            bx.LazyHalve(shift);
            by.LazyHalve(shift);
            steps <<= 1;
        }
        bx.SteadyState(shift);
        by.SteadyState(shift);
        bx.Step();
        by.Step();
        steps--;
        while (true)
        {
            points.Add((bx.Value + originX, by.Value + originY));
            if (steps == 0) return fits;
            if (Math.Max(bx.Error, by.Error) > 0x7fe00)
            {
                bx.Halve();
                by.Halve();
                steps <<= 1;
            }
            while ((steps & 1) == 0 && bx.ParentErrorQuarter <= 0x1ff80 && by.ParentErrorQuarter <= 0x1ff80)
            {
                bx.Double();
                by.Double();
                steps >>= 1;
            }
            bx.Step();
            by.Step();
            steps--;
        }
    }

    /// <summary>One axis's forward differences: e0 the value, e1 the step, e2 and e3 the second differences at the
    /// step's end and start (times 6 at setup).</summary>
    private sealed class Basis(long p0, long p1, long p2, long p3)
    {
        private long _e0 = p0 << 10, _e1 = (p3 - p0) << 10, _e2 = 3 * (p1 - 2 * p2 + p3) << 11, _e3 = 3 * (p0 - 2 * p1 + p2) << 11;

        public long Error => Math.Max(Math.Abs(_e2), Math.Abs(_e3));

        public long ParentErrorQuarter => Math.Max(Math.Abs(_e3), Math.Abs(2 * _e2 - _e3));

        public long Value => (_e0 + 0x1000) >> 13;

        public void LazyHalve(int shift)
        {
            _e2 = (_e3 + _e2) >> 1;
            _e1 = (_e1 - (_e2 >> shift)) >> 1;
        }

        public void SteadyState(int shift)
        {
            _e0 <<= 3;
            _e1 <<= 3;
            int by = shift - 3;
            if (by < 0)
            {
                _e2 <<= -by;
                _e3 <<= -by;
            }
            else
            {
                _e2 >>= by;
                _e3 >>= by;
            }
        }

        public void Halve()
        {
            _e2 = (_e2 + _e3) >> 3;
            _e1 = (_e1 - _e2) >> 1;
            _e3 >>= 2;
        }

        public void Double()
        {
            _e3 <<= 2;
            _e1 = _e2 + 2 * _e1;
            _e2 = (_e2 << 3) - _e3;
        }

        public void Step()
        {
            _e0 += _e1;
            long previous = _e2;
            _e1 += previous;
            _e2 = previous - _e3 + previous;
            _e3 = previous;
        }
    }
}
