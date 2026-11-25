using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// The AGERC.DLL code scripts call (docs/engine-re.md, "Comparing engine builds", Verita): the map functions of
/// <see cref="AgercMap"/> on geometry worked out by hand from the DLL's code, ops 0x14b to 0x14d in synthetic scripts,
/// and Verita's FIELD.BIN, the only script that calls them.
/// </summary>
public sealed class AgercOpsTests
{
    private const int Immediate = 0, InlineString = 2, GlobalInt = 3;
    private const int PointCells = AgercMap.Polygons * AgercMap.PointsPerPolygon;

    private static Operand I(long value) => new(Immediate, value);
    private static Operand G(int address) => new(GlobalInt, address);

    // ---- the map functions ----

    /// <summary>A map with one polygon in a set: <paramref name="points"/> as (x, y), set and boxed as FIELD does
    /// (the setters, _SetMapParams, _InitRect).</summary>
    private static AgercMap MapWith(string set, (int X, int Y)[] points, int wallCollides = 1, int landIndex = 0)
    {
        var map = new AgercMap();
        var x = new int[PointCells];
        var y = new int[PointCells];
        for (int k = 0; k < points.Length; k++) (x[k], y[k]) = points[k];
        var counts = new int[AgercMap.Polygons];
        counts[0] = points.Length;
        Done(map.Call($"_SetMap{set}PX@20", x));
        Done(map.Call($"_SetMap{set}PY@20", y));
        Done(map.Call($"_SetMap{set}PN@20", counts));
        if (set == "Wall") Done(map.Call("_SetMapWallCr@20", OneHundred(wallCollides)));
        if (set == "Land") Done(map.Call("_SetMapLandIdx@20", OneHundred(landIndex)));
        Done(map.Call("_SetMapParams@20", set switch
        {
            "Wall" => [1, 0, 0],
            "Land" => [0, 1, 0],
            _ => [0, 0, 1],
        }));
        Done(map.Call("_InitRect@20", []));
        return map;
    }

    private static int[] OneHundred(int first)
    {
        var cells = new int[AgercMap.Polygons];
        cells[0] = first;
        return cells;
    }

    private static int Done(AgercCallResult result)
    {
        Assert.Equal(AgercCallStatus.Done, result.Status);
        return result.Value;
    }

    private static readonly (int X, int Y)[] Square = [(0, 0), (10, 0), (10, 10), (0, 10)];

    [Theory]
    [InlineData(6, 4, 7)]     // inside: the segment to (-1, -1) crosses the bottom edge once
    [InlineData(5, 5, 7)]     // through the corner (0, 0): a point on a line counts as the line's other side
    [InlineData(20, 5, 0)]    // outside the box
    [InlineData(10, 10, 0)]   // in the box, but the segment crosses two edges
    public void GetLandReturnsTheLandIndexOfThePolygonHoldingThePoint(int x, int y, int expected)
    {
        AgercMap map = MapWith("Land", Square, landIndex: 7);

        Assert.Equal(expected, Done(map.Call("_GetLand@20", [x, y])));
    }

    [Fact]
    public void GetEvReturnsThePolygonsNumberSoPolygonZeroReadsAsNone()
    {
        AgercMap map = MapWith("Ev", Square);

        Assert.Equal(0, Done(map.Call("_GetEv@20", [6, 4])));
        Assert.Equal(0, Done(map.Call("_GetEv@20", [20, 5])));
    }

    [Fact]
    public void TheQueriesReadTheOutlinesInitRectCopiedNotThePointsSetSince()
    {
        AgercMap map = MapWith("Land", Square, landIndex: 3);
        // The same land shrunk to (0, 0)-(4, 4). (8, 2) is in the old square, not in the new one.
        var x = new int[PointCells];
        var y = new int[PointCells];
        (x[1], x[2], y[2], y[3]) = (4, 4, 4, 4);
        Done(map.Call("_SetMapLandPX@20", x));
        Done(map.Call("_SetMapLandPY@20", y));
        Assert.Equal(3, Done(map.Call("_GetLand@20", [8, 2])));

        Done(map.Call("_InitRect@20", []));

        Assert.Equal(0, Done(map.Call("_GetLand@20", [8, 2])));
        Assert.Equal(3, Done(map.Call("_GetLand@20", [2, 1])));
    }

    private static readonly (int X, int Y)[] Wall = [(10, -10), (10, 10)];

    [Fact]
    public void FindClashStopsAMoveThatCrossesAWallAtTheContactPoint()
    {
        AgercMap map = MapWith("Wall", Wall);
        Done(map.Call("_SetChParams@20", [0, 0, 20, 0, 1]));
        int[] cells = [0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(1, Done(map.Call("_FindClash@20", cells)));
        // Polygon 0, edge 0, the contact point, the move's new end and 0 (not at an end); the eighth cell is untouched.
        Assert.Equal([0, 0, 10, 0, 10, 0, 0, 0], cells);
    }

    [Theory]
    [InlineData(3, 1)]   // the nearest point (10, 0) is 2 from the move's end (8, 0): within the radius
    [InlineData(2, 1)]
    [InlineData(1, 0)]   // the box grown by the radius does not reach the wall
    public void FindClashFindsAWallWithinTheRadiusOfTheMovesEnd(int radius, int expected)
    {
        AgercMap map = MapWith("Wall", Wall);
        Done(map.Call("_SetChParams@20", [0, 0, 8, 0, radius]));
        int[] cells = [0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, Done(map.Call("_FindClash@20", cells)));
        if (expected == 1) Assert.Equal([0, 0, 10, 0, 8, 0, 0], cells);
    }

    [Theory]
    [InlineData(7, 1)]   // the end (10, 5) is 26 squared from (9, 0): within 49
    [InlineData(5, 0)]   // not within 25
    public void FindClashTakesAnEdgesEndWhenTheMovesEndProjectsPastIt(int radius, int expected)
    {
        AgercMap map = MapWith("Wall", [(10, 5), (10, 20)]);
        Done(map.Call("_SetChParams@20", [0, 0, 9, 0, radius]));
        int[] cells = [0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, Done(map.Call("_FindClash@20", cells)));
        if (expected == 1) Assert.Equal([0, 0, 10, 5, 9, 0, 1], cells);
    }

    [Fact]
    public void FindClashRoundsTheNearestPointAsTheX87DoesAtSinglePrecision()
    {
        // x1 = 2^24 + 1 is no float: x1 - dx*r rounds to 24 bits, 2^24 (to even), where exact arithmetic gives x1.
        const int x = (1 << 24) + 1;
        AgercMap map = MapWith("Wall", [(x, -10), (x, 10)]);
        Done(map.Call("_SetChParams@20", [x - 7, 0, x - 2, 0, 3]));
        int[] cells = [0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(1, Done(map.Call("_FindClash@20", cells)));
        Assert.Equal([0, 0, 1 << 24, 0, x - 2, 0, 0], cells);
    }

    [Fact]
    public void FindClashNeverTestsAPolygonsClosingEdge()
    {
        // A triangle whose only edge the move crosses is the closing one, (0, 10) back to (0, -10); the nearest points
        // of the other two, (2, 5) truncated from (2.2, 5.6) and (5, 0), are not the move's end.
        AgercMap map = MapWith("Wall", [(0, -10), (5, 0), (0, 10)]);
        Done(map.Call("_SetChParams@20", [-5, 5, 1, 5, 0]));

        Assert.Equal(0, Done(map.Call("_FindClash@20", [0, 0, 0, 0, 0, 0, 0])));
    }

    [Fact]
    public void FindClashSkipsAWallWhoseCrEntryIsNotOne()
    {
        AgercMap map = MapWith("Wall", Wall, wallCollides: 2);
        Done(map.Call("_SetChParams@20", [0, 0, 20, 0, 1]));

        Assert.Equal(0, Done(map.Call("_FindClash@20", [0, 0, 0, 0, 0, 0, 0])));
    }

    [Fact]
    public void AnExportThatRunsPastTheCellsOrItsTablesIsRefused()
    {
        var map = new AgercMap();
        Assert.Equal(AgercCallStatus.CellsTooShort, map.Call("_SetMapWallPX@20", new int[PointCells - 1]).Status);
        Assert.Equal(AgercCallStatus.CellsTooShort, map.Call("_SetChParams@20", [1, 2, 3, 4]).Status);
        // 101 land polygons: _InitRect reads the point count of polygon 100, past its table.
        Done(map.Call("_SetMapParams@20", [0, 101, 0]));
        Assert.Equal(AgercCallStatus.OutsideTable, map.Call("_InitRect@20", []).Status);
        Assert.Equal(AgercCallStatus.ShellExport, map.Call("_ShowDialog@12", []).Status);
    }

    [Fact]
    public void FindClashFromANegativeEdgeOfAnEmptyWallFaultsOnItsDivision()
    {
        // Point count 0 and edge -5: the edge loop runs (-5 < -1) and takes (k + 1) % 0, a divide error in native. A
        // wall with no points has the box (2^31 - 1, 2^31 - 1, 0, 0), which a move from (2^31 - 1, 2^31 - 1) to (0, 0)
        // reaches.
        var map = new AgercMap();
        Done(map.Call("_SetMapWallCr@20", OneHundred(1)));
        Done(map.Call("_SetMapParams@20", [1, 0, 0]));
        Done(map.Call("_InitRect@20", []));
        Done(map.Call("_SetChParams@20", [int.MaxValue, int.MaxValue, 0, 0, 0]));

        Assert.Equal(AgercCallStatus.DivideFault, map.Call("_FindClash@20", [0, -5, 0, 0, 0, 0, 0]).Status);
    }

    // ---- the shell native's module and slots ----

    [Fact]
    public void TheSceneResetDropsTheModuleAndTheSlotsButNotTheMap()
    {
        var shell = ShellServices.Headless();
        Assert.Equal(AgercBindStatus.NoModule, shell.BindAgercExport(1, "_GetLand@20"));
        Assert.False(shell.LoadAgercModule("KERNEL32.DLL"));
        Assert.True(shell.LoadAgercModule("agerc.dll"));
        Assert.Equal(AgercBindStatus.Bound, shell.BindAgercExport(1, "_GetLand@20"));
        Assert.Equal(AgercBindStatus.NoSuchExport, shell.BindAgercExport(100, "_getland@20"));
        Assert.Equal(AgercBindStatus.SlotOutOfRange, shell.BindAgercExport(100, "_GetLand@20"));
        AgercMap map = shell.AgercMap;

        shell.ResetScene();

        Assert.Null(shell.AgercExport(1));
        Assert.Equal(AgercBindStatus.NoModule, shell.BindAgercExport(1, "_GetLand@20"));
        Assert.Same(map, shell.AgercMap);
    }

    // ---- the ops ----

    private sealed class Catalog(IReadOnlyDictionary<long, string> names) : IScriptProvider
    {
        public Script? GetById(long id) => null;
        public string? EntryName(long id) => names.GetValueOrDefault(id);
    }

    private static (VirtualMachine Vm, RecordingWaits Host) Run(List<(int, Operand[])> code, string[] strings,
                                                                string module = "AGERC.DLL",
                                                                ScriptErrorChoice? answer = null)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        code.Add((0x2, []));
        Script script = ScriptAssembler.Assemble(table, "AGERC", code, strings);
        var host = new RecordingWaits();
        if (answer is { } choice) host.ScriptErrorAnswers.Enqueue(choice);
        var vm = new VirtualMachine(script, table, host, provider: new Catalog(new Dictionary<long, string> { [5] = module }));
        vm.Run();
        return (vm, host);
    }

    [Fact]
    public void TheOpsLoadBindAndCallTheMapFunctionsOverTheScriptsCells()
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        int mov = table.ByLabel("mov")!.Value;
        string[] exports = ["_SetMapLandPX@20", "_SetMapLandPY@20", "_SetMapLandPN@20", "_SetMapLandIdx@20",
                            "_SetMapParams@20", "_InitRect@20", "_GetLand@20"];
        var code = new List<(int, Operand[])> { (0x14b, [I(5)]) };
        for (int i = 0; i < exports.Length; i++) code.Add((0x14c, [I(i + 1), new Operand(InlineString, i)]));
        // A square land (0, 0)-(10, 10) with index 7, and the point (6, 4).
        foreach ((int address, int value) in new[] { (0x1001, 10), (0x1002, 10), (0x4002, 10), (0x4003, 10),
                                                     (0x8000, 4), (0x8100, 7), (0x8201, 1), (0x8300, 6), (0x8301, 4) })
            code.Add((mov, [G(address), I(value)]));
        foreach ((int slot, int cells, int count) in new[] { (1, 0x1000, PointCells), (2, 0x4000, PointCells),
                                                             (3, 0x8000, 100), (4, 0x8100, 100), (5, 0x8200, 3),
                                                             (6, 0x8300, 0), (7, 0x8300, 2) })
            code.Add((0x14d, [I(slot), G(0x9000 + slot), G(cells), I(count), G(0x9100), I(0)]));

        (VirtualMachine vm, RecordingWaits host) = Run(code, exports);

        Assert.Equal("exit", vm.HaltReason);
        Assert.Empty(host.Diagnostics);
        Assert.Equal(7, vm.Globals[0x9007]);
        Assert.Equal(0, vm.Globals[0x9001]);
        // The setters change no cell, so none is written: the cells they copied stay unset.
        Assert.False(vm.Globals.ContainsKey(0x1003));
    }

    [Fact]
    public void FindClashWritesItsAnswerIntoTheScriptsCells()
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        int mov = table.ByLabel("mov")!.Value;
        string[] exports = ["_SetMapWallPX@20", "_SetMapWallPY@20", "_SetMapWallPN@20", "_SetMapWallCr@20",
                            "_SetMapParams@20", "_InitRect@20", "_SetChParams@20", "_FindClash@20"];
        var code = new List<(int, Operand[])> { (0x14b, [I(5)]) };
        for (int i = 0; i < exports.Length; i++) code.Add((0x14c, [I(i + 1), new Operand(InlineString, i)]));
        // The wall (10, -10)-(10, 10) and the move (0, 0) to (20, 0) with radius 1.
        foreach ((int address, int value) in new[] { (0x1000, 10), (0x1001, 10), (0x4000, -10), (0x4001, 10),
                                                     (0x8000, 2), (0x8100, 1), (0x8200, 1), (0x8302, 20),
                                                     (0x8304, 1) })
            code.Add((mov, [G(address), I(value)]));
        foreach ((int slot, int cells, int count) in new[] { (1, 0x1000, PointCells), (2, 0x4000, PointCells),
                                                             (3, 0x8000, 100), (4, 0x8100, 100), (5, 0x8200, 3),
                                                             (6, 0x8300, 0), (7, 0x8300, 5), (8, 0x8400, 8) })
            code.Add((0x14d, [I(slot), G(0x9000 + slot), G(cells), I(count), G(0x8400), I(0)]));

        (VirtualMachine vm, _) = Run(code, exports);

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(1, vm.Globals[0x9008]);
        Assert.Equal([0, 0, 10, 0, 10, 0, 0, 0],
                     Enumerable.Range(0x8400, 8).Select(address => vm.Globals.GetValueOrDefault(address)));
    }

    [Fact]
    public void AMissingExportRaisesNativesBoxAndIgnoreGoesOn()
    {
        var code = new List<(int, Operand[])>
        {
            (0x14b, [I(5)]),
            (0x14c, [I(1), new Operand(InlineString, 0)]),
            (0x55, [G(0x200), I(1)]),
        };

        (VirtualMachine vm, RecordingWaits host) = Run(code, ["_Missing@20"], answer: ScriptErrorChoice.Ignore);

        Assert.Equal("exit", vm.HaltReason);
        DiagnosticMessage box = Assert.Single(host.Diagnostics);
        Assert.Equal(AgeBoxButtons.AbortRetryIgnore, box.Parts!.Buttons);
        Assert.Contains("_Missing@20のアドレス取得に失敗しました．", box.Text);
        Assert.Contains("ERRORCODE = 127", box.Text);
        Assert.Equal(1, vm.Globals[0x200]);
    }

    [Theory]
    [InlineData("AGERC.DLL", 3, "agerc-export-unbound: AGERC@0x")]       // slot 3 holds no export
    [InlineData("OTHER.DLL", 1, "agerc-module-unsupported: AGERC@0x0 ")]  // the port has no module for the name
    public void ACallTheModuleOrSlotCannotServeHalts(string module, int slot, string reason)
    {
        var code = new List<(int, Operand[])>
        {
            (0x14b, [I(5)]),
            (0x14d, [I(slot), G(0x9000), G(0x8000), I(2), G(0x9100), I(0)]),
        };

        (VirtualMachine vm, _) = Run(code, [], module);

        Assert.StartsWith(reason, vm.HaltReason);
    }

    // ---- Verita's FIELD.BIN ----

    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "verita")]
    public void VeritasFieldBindsTheMapExportsAndPassesEachTheCellsItReads()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("verita");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("verita")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(Path.Combine(gameRoot, "SYS4INI.BIN"));
        var scripts = new Sys4ScriptProvider(table, catalog, new Sys4AssetStore(catalog, gameRoot, [gameRoot]));
        Script field = scripts.GetByName("FIELD.BIN")!;

        // FIELD@0x0 loads entry 0x5118, AGERC.DLL, and FIELD@0x3 to @0x53 bind slots 1 to 17.
        Instruction load = field.Instructions[0];
        Assert.Equal(0x14b, load.Opcode);
        Assert.Equal(ShellServices.AgercModuleName, scripts.EntryName(load.Args[0].Value));
        Dictionary<long, string> slots = field.Instructions.Where(ins => ins.Opcode == 0x14c)
            .ToDictionary(ins => ins.Args[0].Value, ins => field.GetString((int)ins.Args[1].Value));
        Assert.Equal(Enumerable.Range(1, 17).Select(slot => (long)slot), slots.Keys.Order());
        Assert.Equal(AgercMap.MapExportNames.Order(), slots.Values.Order());

        // Each call names a bound slot and passes at least the cells its export reads.
        List<Instruction> calls = field.Instructions.Where(ins => ins.Opcode == 0x14d).ToList();
        Assert.Equal(19, calls.Count);
        foreach (Instruction call in calls)
        {
            string export = slots[call.Args[0].Value];
            Assert.Equal(Immediate, call.Args[3].Type);
            var cells = new int[call.Args[3].Value];
            AgercCallResult result = new AgercMap().Call(export, cells);
            Assert.True(result.Status == AgercCallStatus.Done, $"FIELD@0x{call.Offset:x} {export}: {result.Detail}");
        }
    }
}
