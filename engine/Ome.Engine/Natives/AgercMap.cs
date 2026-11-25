using System.Collections.Frozen;
using Ome.Engine.Model;

namespace Ome.Engine.Natives;

/// <summary>What a call of an AGERC.DLL export came to (<see cref="AgercMap.Call"/>).</summary>
public enum AgercCallStatus
{
    /// <summary>The export ran; <see cref="AgercCallResult.Value"/> is what it returned.</summary>
    Done,
    /// <summary>The export reads or writes a cell past the ones the script passed. Native hands it a heap copy of that
    /// many cells (none for a count of 0 or less), and the export runs past it.</summary>
    CellsTooShort,
    /// <summary>The export indexes past one of the DLL's tables, where native reads or writes the statics beside it.
    /// </summary>
    OutsideTable,
    /// <summary>An integer division by zero, or of -2^31 by -1, where native's <c>idiv</c> raises a divide error.</summary>
    DivideFault,
    /// <summary>One of the shell's exports (<c>_OperateMenu</c>, <c>_ShowDialog</c>, <c>_GetInstance</c>), which the port's
    /// menus and dialogs stand for; the port does not run one as a script's call.</summary>
    ShellExport,
}

/// <summary>A call's status, its return when it ran, and words for a halt when it did not.</summary>
public readonly record struct AgercCallResult(AgercCallStatus Status, int Value = 0, string Detail = "");

/// <summary>
/// The map functions of AGERC.DLL, which op 0x14d calls (docs/engine-re.md, "Comparing engine builds", Verita): one map
/// in the DLL's statics, up to 100 wall, 100 land and 100 event polygons of up to 100 points each, a move with a
/// radius, and the 17 exports that set and query them. Himegari's and Verita's DLLs have the same code for them,
/// instruction for instruction. Every table here is its native size, so an index past one is native reading or writing
/// the statics beside it, which the port refuses (<see cref="AgercCallStatus.OutsideTable"/>).
/// </summary>
/// <remarks>
/// <para>AGE loads AGERC.DLL for its menus at start, so op 0x14b's FreeLibrary never unloads it: the map lives as long as
/// the program, through op 0x9's scene reset and a numbered load, and no save keeps it. Its statics start at zero.</para>
/// <para>The arithmetic is the DLL's: integers wrap at 32 bits, divisions truncate, and <c>_FindClash</c>'s float steps
/// round as AGE's x87 does at single precision (<see cref="SinglePrecision"/>), with <c>_ftol2</c>'s truncation.</para>
/// <para>Threads: the engine thread only.</para>
/// </remarks>
public sealed class AgercMap
{
    /// <summary>The polygons each set holds.</summary>
    public const int Polygons = 100;

    /// <summary>The points of a polygon: point k of polygon p is at 100p + k in the point tables.</summary>
    public const int PointsPerPolygon = 100;

    private const int PointCells = Polygons * PointsPerPolygon;

    private delegate int Export(AgercMap map, Cells cells);

    // GetProcAddress matches a name exactly.
    private static readonly FrozenDictionary<string, Export> MapExports = new Dictionary<string, Export>(StringComparer.Ordinal)
    {
        ["_FindClash@20"] = static (map, cells) => map.FindClash(cells),
        ["_GetLand@20"] = static (map, cells) => map.GetLand(cells),
        ["_GetEv@20"] = static (map, cells) => map.GetEvent(cells),
        ["_InitRect@20"] = static (map, _) => map.InitRect(),
        ["_SetMapParams@20"] = static (map, cells) => map.SetMapParams(cells),
        ["_SetMapWallPX@20"] = static (map, cells) => Copy(cells, map._walls.X),
        ["_SetMapWallPY@20"] = static (map, cells) => Copy(cells, map._walls.Y),
        ["_SetMapWallPN@20"] = static (map, cells) => Copy(cells, map._walls.PointCounts),
        ["_SetMapWallCr@20"] = static (map, cells) => Copy(cells, map._wallCollides),
        ["_SetMapLandPX@20"] = static (map, cells) => Copy(cells, map._lands.X),
        ["_SetMapLandPY@20"] = static (map, cells) => Copy(cells, map._lands.Y),
        ["_SetMapLandPN@20"] = static (map, cells) => Copy(cells, map._lands.PointCounts),
        ["_SetMapLandIdx@20"] = static (map, cells) => Copy(cells, map._landIndices),
        ["_SetMapEvPX@20"] = static (map, cells) => Copy(cells, map._events.X),
        ["_SetMapEvPY@20"] = static (map, cells) => Copy(cells, map._events.Y),
        ["_SetMapEvPN@20"] = static (map, cells) => Copy(cells, map._events.PointCounts),
        ["_SetChParams@20"] = static (map, cells) => map.SetChParams(cells),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenSet<string> ShellExports =
        new[] { "_GetInstance@0", "_OperateMenu@16", "_ShowDialog@12" }.ToFrozenSet(StringComparer.Ordinal);

    // One polygon set's statics: its points as the setters copy them, InitRect's interleaved copy of them (x, y of
    // point k of polygon p at 2(100p + k)), and its boxes (min x, min y, max x, max y).
    private sealed class PolygonSet
    {
        public readonly int[] X = new int[PointCells];
        public readonly int[] Y = new int[PointCells];
        public readonly int[] PointCounts = new int[Polygons];
        public readonly int[] Points = new int[2 * PointCells];
        public readonly int[] Boxes = new int[4 * Polygons];
        public int Count;

        public bool BoxHolds(int polygon, int x, int y)
        {
            int box = 4 * polygon;
            return Boxes[box] <= x && x <= Boxes[box + 2] && Boxes[box + 1] <= y && y <= Boxes[box + 3];
        }
    }

    private readonly PolygonSet _walls = new();
    private readonly PolygonSet _lands = new();
    private readonly PolygonSet _events = new();
    private readonly int[] _wallCollides = new int[Polygons];   // _SetMapWallCr: a wall stops a move when its entry is 1
    private readonly int[] _landIndices = new int[Polygons];    // _SetMapLandIdx: what _GetLand returns for each land
    private int _fromX, _fromY, _toX, _toY, _radius, _radiusSquared;   // _SetChParams

    /// <summary>Whether AGERC.DLL exports <paramref name="name"/>, as GetProcAddress finds it: the map functions and the
    /// shell's three.</summary>
    public static bool HasExport(string name) => MapExports.ContainsKey(name) || ShellExports.Contains(name);

    /// <summary>The map exports' names, as a script binds them (FIELD.BIN binds all 17).</summary>
    public static IEnumerable<string> MapExportNames => MapExports.Keys;

    /// <summary>Call an export with the script's cells, as op 0x14d does: the export reads them and may change them in
    /// place. Native also passes the owner window, the count, a cell's address and a value, which no map function reads.
    /// </summary>
    public AgercCallResult Call(string export, int[] cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        if (ShellExports.Contains(export))
            return new(AgercCallStatus.ShellExport, Detail: $"{export} is the shell's, which the port's menus stand for");
        if (!MapExports.TryGetValue(export, out Export? run))
            throw new ArgumentException($"AGERC has no export {export}.", nameof(export));
        try
        {
            return new(AgercCallStatus.Done, run(this, new Cells(cells)));
        }
        catch (CellPastEnd past)
        {
            return new(AgercCallStatus.CellsTooShort,
                       Detail: $"{export} reads cell {past.Index}, and the script passed {cells.Length}");
        }
        catch (IndexOutOfRangeException)
        {
            return new(AgercCallStatus.OutsideTable, Detail: $"{export} indexes past one of the DLL's tables");
        }
        catch (ArithmeticException)
        {
            return new(AgercCallStatus.DivideFault, Detail: $"{export} divides by zero, or -2^31 by -1");
        }
    }

    // ---- the setters ----

    // Each copies its table's whole size from the cells, whatever count the script passed, and returns 0.
    private static int Copy(Cells cells, int[] table)
    {
        if (cells.Length < table.Length) throw new CellPastEnd(cells.Length);
        for (int i = 0; i < table.Length; i++) table[i] = cells[i];
        return 0;
    }

    // _SetMapParams: the wall, land and event polygon counts.
    private int SetMapParams(Cells cells)
    {
        _walls.Count = cells[0];
        _lands.Count = cells[1];
        _events.Count = cells[2];
        return 0;
    }

    // _SetChParams: the move (from x, from y, to x, to y) and the radius, whose square the DLL keeps.
    private int SetChParams(Cells cells)
    {
        _fromX = cells[0];
        _fromY = cells[1];
        _toX = cells[2];
        _toY = cells[3];
        _radius = cells[4];
        _radiusSquared = unchecked(_radius * _radius);
        return 0;
    }

    // ---- _InitRect ----

    // Every polygon's interleaved points and box, wall, land and event sets in turn. It resets each box's minimums, but
    // its maximums start from the box's last values, so a box only grows; one with no points keeps only the reset.
    private int InitRect()
    {
        InitRects(_walls);
        InitRects(_lands);
        InitRects(_events);
        return 0;
    }

    private static void InitRects(PolygonSet set)
    {
        for (int p = 0; p < set.Count; p++)
        {
            int count = set.PointCounts[p];
            int box = 4 * p;
            set.Boxes[box] = int.MaxValue;
            set.Boxes[box + 1] = int.MaxValue;
            if (count <= 0) continue;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = set.Boxes[box + 2], maxY = set.Boxes[box + 3];
            for (int k = 0; k < count; k++)
            {
                int at = unchecked(PointsPerPolygon * p + k);
                int x = set.X[at], y = set.Y[at];
                set.Points[2 * at] = x;
                set.Points[2 * at + 1] = y;
                if (x < minX) minX = x;
                if (maxX < x) maxX = x;
                if (y < minY) minY = y;
                if (maxY < y) maxY = y;
            }
            set.Boxes[box] = minX;
            set.Boxes[box + 1] = minY;
            set.Boxes[box + 2] = maxX;
            set.Boxes[box + 3] = maxY;
        }
    }

    // ---- the queries ----

    // _GetLand (x, y): the land index of the first land polygon whose box and outline hold the point; 0 for none.
    private int GetLand(Cells cells)
    {
        int x = cells[0], y = cells[1];
        for (int i = 0; i < _lands.Count; i++)
            if (_lands.BoxHolds(i, x, y) && PointInPolygon(_lands, i, x, y)) return _landIndices[i];
        return 0;
    }

    // _GetEv (x, y): the number of the first event polygon whose box and outline hold the point; 0 for none, so
    // polygon 0 cannot be told from none.
    private int GetEvent(Cells cells)
    {
        int x = cells[0], y = cells[1];
        for (int i = 0; i < _events.Count; i++)
            if (_events.BoxHolds(i, x, y) && PointInPolygon(_events, i, x, y)) return i;
        return 0;
    }

    // _FindClash (polygon, edge; out: polygon, edge, contact x, y, move end x, y, at an end): from the polygon and the
    // edge the cells name, the first wall edge the move crosses, or whose distance from the move's end is at most the
    // radius. A wall is tested when its Cr entry is 1 and the move's box, grown by the radius, reaches its box. Every
    // polygon's edges start at the cells' edge, not only the first's, and its closing edge (last point to first) is never
    // tested. A crossing moves the move's end to the contact point and writes 0 for "at an end"; a near edge writes the
    // nearest point and 1 when that is one of the edge's ends.
    private int FindClash(Cells cells)
    {
        unchecked
        {
            // The segment test reads the move as the call found it; the arithmetic reads the statics.
            int fromX = _fromX, fromY = _fromY, toX = _toX, toY = _toY;
            for (int p = cells[0]; p < _walls.Count; p++)
            {
                if (_wallCollides[p] != 1) continue;
                int box = 4 * p;
                int r = _radius;
                if (!((_fromX + r >= _walls.Boxes[box] || _toX + r >= _walls.Boxes[box])
                      && (_fromX - r <= _walls.Boxes[box + 2] || _toX - r <= _walls.Boxes[box + 2])
                      && (_fromY + r >= _walls.Boxes[box + 1] || _toY + r >= _walls.Boxes[box + 1])
                      && (_fromY - r <= _walls.Boxes[box + 3] || _toY - r <= _walls.Boxes[box + 3])))
                    continue;
                int count = _walls.PointCounts[p];
                for (int k = cells[1]; k < count - 1; k++)
                {
                    int j = (k + 1) % count;
                    int first = PointsPerPolygon * p + k, second = PointsPerPolygon * p + j;
                    int x1 = _walls.X[first], y1 = _walls.Y[first], x2 = _walls.X[second], y2 = _walls.Y[second];
                    int dx = x2 - x1, dy = y2 - y1;
                    int denominator = (_toX - _fromX) * dy - (_toY - _fromY) * dx;
                    if (denominator == 0) continue;
                    if (SegmentsCross(fromX, fromY, toX, toY,
                                      _walls.Points[2 * first], _walls.Points[2 * first + 1],
                                      _walls.Points[2 * second], _walls.Points[2 * second + 1]))
                    {
                        int numerator = (_fromY - y1) * dx - (_fromX - x1) * dy;
                        int contactX = (_toX - _fromX) * numerator / denominator + _fromX;
                        int contactY = (_toY - _fromY) * numerator / denominator + _fromY;
                        _toX = contactX;
                        _toY = contactY;
                        WriteClash(cells, p, k, contactX, contactY, atEnd: 0);
                        return 1;
                    }

                    // The point of the edge nearest the move's end. The DLL keeps the squared length and the end's
                    // projection as floats, each rounded once from its integer.
                    float lengthSquared = dy * dy + dx * dx;
                    float along = (y1 - _toY) * dy + (x1 - _toX) * dx;   // minus the projection
                    float projection = -along;
                    int nearX, nearY, atEnd = 1;
                    if (projection < 0)
                    {
                        nearX = x1;
                        nearY = y1;
                    }
                    else if (lengthSquared < projection)
                    {
                        nearX = x2;
                        nearY = y2;
                    }
                    else
                    {
                        float ratio = SinglePrecision.Divide(along, lengthSquared);
                        nearX = TruncateToInt(SinglePrecision.Subtract(x1, SinglePrecision.Multiply(dx, ratio)));
                        nearY = TruncateToInt(SinglePrecision.Subtract(y1, SinglePrecision.Multiply(ratio, dy)));
                        atEnd = 0;
                    }
                    int offX = nearX - _toX, offY = nearY - _toY;
                    if (offX * offX + offY * offY <= _radiusSquared)
                    {
                        WriteClash(cells, p, k, nearX, nearY, atEnd);
                        return 1;
                    }
                }
            }
            return 0;
        }
    }

    private void WriteClash(Cells cells, int polygon, int edge, int x, int y, int atEnd)
    {
        cells[0] = polygon;
        cells[1] = edge;
        cells[2] = x;
        cells[3] = y;
        cells[4] = _toX;
        cells[5] = _toY;
        cells[6] = atEnd;
    }

    // ---- the geometry helpers (0x10004530, 0x10004570, 0x10004620) ----

    // Which side of the line through (x1, y1) and (x2, y2) the point (x3, y3) is on: 1 for one side, -1 for the
    // other and for the line itself. The products wrap at 32 bits.
    private static int Side(int x1, int y1, int x2, int y2, int x3, int y3)
        => unchecked((y3 - y1) * (x2 - x1) > (y2 - y1) * (x3 - x1)) ? 1 : -1;

    // Whether the segments (x1, y1)-(x2, y2) and (x3, y3)-(x4, y4) cross: neither's ends are both on one side of the
    // other's line.
    private static bool SegmentsCross(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        => Side(x1, y1, x2, y2, x3, y3) * Side(x1, y1, x2, y2, x4, y4) < 1
           && Side(x3, y3, x4, y4, x1, y1) * Side(x3, y3, x4, y4, x2, y2) < 1;

    // Whether the polygon's outline holds the point: the segment from the point to (-1, -1) crosses an odd number of
    // its edges, the closing one included. The point count is read as 16 bits.
    private static bool PointInPolygon(PolygonSet set, int polygon, int x, int y)
    {
        int count = unchecked((ushort)set.PointCounts[polygon]);
        int start = 2 * PointsPerPolygon * polygon;
        int at = start;
        int crossings = 0;
        for (int edge = 0; edge < count - 1; edge++, at += 2)
            if (SegmentsCross(x, y, -1, -1, set.Points[at], set.Points[at + 1], set.Points[at + 2], set.Points[at + 3]))
                crossings++;
        if (SegmentsCross(x, y, -1, -1, set.Points[at], set.Points[at + 1], set.Points[start], set.Points[start + 1]))
            crossings++;
        return (crossings & 1) != 0;
    }

    // _ftol2 on an SSE2 processor: cvttsd2si, which truncates, and gives -2^31 for a value outside the int range.
    private static int TruncateToInt(float value)
    {
        double truncated = Math.Truncate(value);
        return truncated >= int.MinValue && truncated <= int.MaxValue ? (int)truncated : int.MinValue;
    }

    // ---- the cells ----

    // The script's cells as the export sees them: a read or write past them is native's export running past its copy.
    private sealed class Cells(int[] values)
    {
        public int Length => values.Length;

        public int this[int index]
        {
            get => index < values.Length ? values[index] : throw new CellPastEnd(index);
            set
            {
                if (index >= values.Length) throw new CellPastEnd(index);
                values[index] = value;
            }
        }
    }

    private sealed class CellPastEnd(int index) : Exception
    {
        public int Index { get; } = index;
    }
}
