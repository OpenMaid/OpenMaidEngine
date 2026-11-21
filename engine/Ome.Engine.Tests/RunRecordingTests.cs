using Ome.Engine.Hosting;
using Ome.Engine.Localization;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Persistence;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

namespace Ome.Engine.Tests;

using Script = Ome.Engine.Model.Script;

/// <summary>The run options the game data export needs of the engine (game data export spec §6.2), on synthetic
/// scripts: the writer record and the string origins, carried by a session and its snapshots, and a stop at a site.
/// </summary>
public class RunRecordingTests
{
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);
    private const int OpExit = 0x2, OpCallScript = 0x3, OpMov = 0x55, OpSetString = 0x192;
    private static readonly VmOptions Recording = new() { RecordsWrites = true };

    private static Operand I(long value) => new(0, value);
    private static Operand S(int index) => new(2, index);
    private static Operand G(int cell) => new(3, cell);
    private static Operand GS(int cell) => new(5, cell);

    private static Script Asm(string name, IReadOnlyList<(int, Operand[])> code, params string[] strings)
        => ScriptAssembler.Assemble(Table, name, code, strings);

    private sealed class Scripts(Dictionary<long, Script> byId) : IScriptProvider
    {
        public Script? GetById(long id) => byId.GetValueOrDefault(id);
    }

    /// <summary>CALLER writes G[0x10], calls $1$CALLEE (id 5), which writes G[0x11] at 0x0 and G[0x13] at 0x5, then
    /// writes G[0x12] at 0xa.</summary>
    private static (Script Caller, Scripts Provider) CallerAndCallee()
    {
        Script callee = Asm("$1$CALLEE.BIN", [(OpMov, [G(0x11), I(2)]), (OpMov, [G(0x13), I(4)]), (OpExit, [])]);
        Script caller = Asm("CALLER.BIN",
            [(OpMov, [G(0x10), I(1)]), (OpCallScript, [I(5)]), (OpMov, [G(0x12), I(3)]), (OpExit, [])]);
        return (caller, new Scripts(new() { [5] = callee }));
    }

    private static GlobalCell Int(int address) => new(GlobalBank.Integer, address);
    private static GlobalCell Str(int address) => new(GlobalBank.String, address);

    [Fact]
    public void ARecordingRunNamesTheScriptOfEachWrite()
    {
        var (caller, provider) = CallerAndCallee();
        var session = new GameSession();
        SceneResult result = session.RunScene(caller, Table, new HeadlessWaits(), Recording, provider);

        Assert.Equal("exit", result.Halt);
        Assert.Equal(new Dictionary<GlobalCell, string>
        {
            [Int(0x10)] = "CALLER.BIN", [Int(0x11)] = "$1$CALLEE.BIN", [Int(0x13)] = "$1$CALLEE.BIN",
            [Int(0x12)] = "CALLER.BIN",
        }, session.Writers);
    }

    [Fact]
    public void ARunThatDoesNotRecordLeavesNoRecord()
    {
        var (caller, provider) = CallerAndCallee();
        var session = new GameSession();
        session.RunScene(caller, Table, new HeadlessWaits(), Recording, provider);
        Assert.NotEmpty(session.Writers);

        // The run may have written any cell, so the earlier records no longer hold.
        session.RunScene(Asm("PLAIN.BIN", [(OpSetString, [GS(5), S(0)]), (OpExit, [])], "剣"), Table, new HeadlessWaits());
        Assert.Empty(session.Writers);
        Assert.Empty(session.StringStores);
        Assert.Equal("剣", session.GlobalStrings[5]);
    }

    /// <summary>A stores a string and an integer; B copies the string and writes another integer.</summary>
    private static GameSession TwoRuns()
    {
        var session = new GameSession();
        session.RunScene(Asm("A.BIN", [(OpSetString, [GS(5), S(0)]), (OpMov, [G(0x10), I(1)]), (OpExit, [])], "剣"),
                         Table, new HeadlessWaits(), Recording);
        session.RunScene(Asm("B.BIN", [(OpMov, [GS(6), GS(5)]), (OpMov, [G(0x11), I(2)]), (OpExit, [])]),
                         Table, new HeadlessWaits(), Recording);
        return session;
    }

    [Fact]
    public void TheSessionCarriesBothRecordsFromRunToRun()
    {
        GameSession session = TwoRuns();

        Assert.Equal(new Dictionary<GlobalCell, string>
        {
            [Str(5)] = "A.BIN", [Int(0x10)] = "A.BIN", [Str(6)] = "B.BIN", [Int(0x11)] = "B.BIN",
        }, session.Writers);
        // The copy in B keeps the literal's origin, stored in A's run.
        Assert.Equal(new TextManager.StringStore(new TextSite("A.BIN", 0x0), "剣"), session.StringStores[5]);
        Assert.Equal(new TextManager.StringStore(new TextSite("A.BIN", 0x0), "剣"), session.StringStores[6]);
    }

    [Fact]
    public void AnOverwriteTakesTheNewWriterAndOrigin()
    {
        GameSession session = TwoRuns();
        session.RunScene(Asm("C.BIN", [(OpMov, [G(0x10), I(9)]), (OpSetString, [GS(6), S(0)]), (OpExit, [])], "盾"),
                         Table, new HeadlessWaits(), Recording);

        Assert.Equal("C.BIN", session.Writers[Int(0x10)]);
        Assert.Equal("C.BIN", session.Writers[Str(6)]);
        Assert.Equal(new TextManager.StringStore(new TextSite("C.BIN", 0x5), "盾"), session.StringStores[6]);
        Assert.Equal("A.BIN", session.Writers[Str(5)]);
    }

    [Fact]
    public void ASnapshotAndACopyCarryTheRecords()
    {
        GameSession session = TwoRuns();

        foreach (GameSession restored in new[] { GameSession.FromJson(session.ToJson()), session.CopyState() })
        {
            Assert.Equal(session.Globals, restored.Globals);
            Assert.Equal(session.GlobalStrings, restored.GlobalStrings);
            Assert.Equal(session.Writers, restored.Writers);
            Assert.Equal(session.StringStores, restored.StringStores);
        }
        // A copy is the session's state, not the session: a run on it leaves the original as it was.
        GameSession copy = session.CopyState();
        copy.RunScene(Asm("D.BIN", [(OpMov, [G(0x10), I(7)]), (OpExit, [])]), Table, new HeadlessWaits(), Recording);
        Assert.Equal(1, session.Globals[0x10]);
        Assert.Equal("A.BIN", session.Writers[Int(0x10)]);
    }

    [Fact]
    public void ASnapshotWithoutRecordsReadsAsBefore()
    {
        var session = new GameSession();
        session.Seed(0x10, 1);
        string json = session.ToJson();

        Assert.DoesNotContain("Writers", json);
        Assert.DoesNotContain("Origins", json);
        GameSession restored = GameSession.FromJson(json);
        Assert.Empty(restored.Writers);
        Assert.Empty(restored.StringStores);
    }

    [Fact]
    public void EveryBankRecordsItsWritesAndASeedIsNone()
    {
        var memory = new ScriptMemory();
        memory.SetInteger(1, 5);   // not recording yet
        memory.RecordWriters(() => "X.BIN");
        memory.SetInteger(2, 5);
        memory.SetFloat(3, 5);
        memory.SetString(4, "a");
        memory.SetPointer(5, 6);
        memory.SetStringPointer(7, 8);
        memory.SeedInteger(9, 1);

        Assert.Equal(new Dictionary<GlobalCell, string>
        {
            [Int(2)] = "X.BIN", [new GlobalCell(GlobalBank.Float, 3)] = "X.BIN", [Str(4)] = "X.BIN",
            [new GlobalCell(GlobalBank.Pointer, 5)] = "X.BIN", [new GlobalCell(GlobalBank.StringPointer, 7)] = "X.BIN",
        }, memory.Writers);

        // Off again, a write is not recorded, and the record stays.
        memory.RecordWriters(null);
        memory.SetInteger(10, 1);
        Assert.False(memory.Writers.ContainsKey(Int(10)));
        Assert.Equal(5, memory.Writers.Count);
    }

    [Fact]
    public void AResetAndALoadDropTheRecord()
    {
        var memory = new ScriptMemory();
        memory.RecordWriters(() => "X.BIN");
        memory.SetInteger(2, 5);
        memory.ResetScene();
        Assert.Empty(memory.Writers);
        Assert.True(memory.RecordsWriters);

        memory.SetInteger(2, 5);
        var dimensions = new NativeSaveBankDimensions(4, 0, 0, 0, 0, 0);
        NativeNumberedSaveState saved = memory.WriteSaveRecords(NativeNumberedSaveCodec.Empty([]), dimensions,
                                                                keepsFloats: false);
        memory.ReadSaveRecords(saved, keepsFloats: false);
        Assert.Equal(5, memory.Integer(2));
        Assert.Empty(memory.Writers);
    }

    [Fact]
    public void ARunStopsAtItsSiteBeforeTheInstruction()
    {
        var session = new GameSession();
        Script script = Asm("STOPPER.BIN", [(OpMov, [G(1), I(1)]), (OpMov, [G(2), I(2)]), (OpExit, [])]);
        SceneResult result = session.RunScene(script, Table, new HeadlessWaits(),
                                              Recording with { StopAt = new RunStop("stopper", 0x5) });

        Assert.Equal("stop:STOPPER@0x5", result.Halt);
        // The banks merge as at any end of a run.
        Assert.Equal(1, session.Globals[1]);
        Assert.False(session.Globals.ContainsKey(2));
        Assert.Equal("STOPPER.BIN", session.Writers[Int(1)]);
    }

    [Fact]
    public void AStopInACalledScriptEndsTheWholeRun()
    {
        var (caller, provider) = CallerAndCallee();
        var session = new GameSession();
        SceneResult result = session.RunScene(caller, Table, new HeadlessWaits(),
                                              new VmOptions { StopAt = new RunStop("$1$CALLEE.BIN", 0x5) }, provider);

        Assert.Equal("stop:$1$CALLEE@0x5", result.Halt);
        Assert.Equal(new[] { 0x10, 0x11 }, session.Globals.Keys.Order());
    }

    [Fact]
    public void ASiteTheRunNeverReachesChangesNothing()
    {
        var (caller, provider) = CallerAndCallee();
        var session = new GameSession();
        SceneResult result = session.RunScene(caller, Table, new HeadlessWaits(),
                                              new VmOptions { StopAt = new RunStop("CALLER", 0x6) }, provider);

        Assert.Equal("exit", result.Halt);
        Assert.Equal(new[] { 0x10, 0x11, 0x12, 0x13 }, session.Globals.Keys.Order());
    }

    private sealed class ShownText : ITextListener
    {
        public List<string> Heard { get; } = [];
        public void RunShown(Ome.Engine.Model.AdvLiveTextRun run, int glyphDelayMilliseconds)
            => Heard.Add($"{run.ScriptStack[^1]}:{run.Text}" + (run.SourceRuby is { } ruby ? $"({ruby})" : ""));
        public void LineEnded(int layoutSlot) => Heard.Add($"line {layoutSlot}");
    }

    [Fact]
    public void ARunsTextListenerHearsItsRunsTheirRubyAndTheLineEnds()
    {
        Script other = Asm("OTHER.BIN", [(0x6e, [I(0), S(0)]), (OpExit, [])], "他");
        Script shows = Asm("DESC.BIN",
            [(0x6e, [I(0), S(0)]), (0x196, [I(0), S(1), S(2)]), (0x6f, [I(0)]), (OpCallScript, [I(5)]), (OpExit, [])],
            "【", "銅", "どう");
        var heard = new ShownText();
        SceneResult result = new GameSession().RunScene(shows, Table, new HeadlessWaits(), provider: new Scripts(new() { [5] = other }),
                                                        textListener: heard);

        Assert.Equal("exit", result.Halt);
        Assert.Equal(["DESC.BIN:【", "DESC.BIN:銅(どう)", "line 0", "OTHER.BIN:他"], heard.Heard);
        Assert.Equal(["【", "銅", "他"], result.Emitted.Select(piece => piece.Text));
    }

    [Fact]
    public void AStopSiteNamesItsScriptAsPatchPointsDo()
    {
        Assert.Equal("INIT2", new RunStop("init2.bin", 0x2cf).Script);
        Assert.Equal("$1$ITINIT", new RunStop("$1$ITINIT.BIN", 0).Script);
        Assert.Equal("stop:INIT2@0x2cf", new RunStop("INIT2", 0x2cf).HaltReason);
    }
}
