using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Diagnostics;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// The last ops of 天秤のLa DEA。's scripts, each as /ladea/4.45D's handler leaves it (docs/engine-re.md, "魔導巧殻 against
/// 天秤のLa DEA。, triaged"): the joystick's POV, six axes and buttons (ops 0x2f9 to 0x2fb), the box overlap test
/// (0x12d), and the timed catch-up callback (0xce, 0xcf), which LAUNCH's battle cut-in runs on.
/// </summary>
public class Age445DInputAndCallbackOpsTests
{
    private const int Imm = 0, GlobalInt = 3, GlobalFloat = 4;
    private const int Exit = 0x2, CallScript = 0x3, Ret = 0x5, Add = 0x50, Mov = 0x55, Sleep = 0xc8,
                      RegisterCatchUp = 0xce, DispatchCatchUp = 0xcf, BoxesOverlap = 0x12d, ReadAxes = 0x2f9,
                      SetAxesIgnored = 0x2fa, ReadButtons = 0x2fb;
    private const int Up = 1 << 0, Right = 1 << 1, Down = 1 << 2, Left = 1 << 3;

    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");

    private static Operand I(long value) => new(Imm, value);
    private static Operand G(int address) => new(GlobalInt, address);
    private static Operand GF(int address) => new(GlobalFloat, address);

    /// <summary>A host whose clock moves only by op 0xc8's sleeps.</summary>
    private sealed class TimedHost : RecordingWaits
    {
        public long Now { get; set; } = 1000;
        public override long InputClockMilliseconds => Now;

        public override void Sleep(long duration)
        {
            base.Sleep(duration);
            Now += duration;
        }
    }

    private static Script Assemble(string name, params (int, Operand[])[] ops)
        => ScriptAssembler.Assemble(Table, name, ops.ToList(), []);

    private static VirtualMachine Run(Script script, RecordingWaits? host = null, SkipAndAuto? skipAndAuto = null)
    {
        var vm = new VirtualMachine(script, Table, host ?? new TimedHost(), build: EngineBuilds.Age445D,
                                    skipAndAuto: skipAndAuto);
        vm.Run();
        return vm;
    }

    private static long Global(VirtualMachine vm, int address) => vm.Globals.GetValueOrDefault(address);

    private static float FloatCell(VirtualMachine vm, int address)
        => BitConverter.Int32BitsToSingle(unchecked((int)vm.GlobalFloats.GetValueOrDefault(address)));

    // ---- the joystick ----

    [Fact]
    public void FourFortyFiveDTakesTheDirectionsFromAPressedPov()
    {
        Assert.False(EngineBuilds.Age443B.Rules.Input.JoystickPovDirections);
        Assert.True(EngineBuilds.Age445D.Rules.Input.JoystickPovDirections);

        // input_poll_joystick_action_bits@0x474450: up past 27000 or below 9000, right 1 to 17999, down 9001 to 26999,
        // left past 18000, in hundredths of a degree clockwise from up.
        var bindings = new InputBindings(EngineBuilds.Age445D.Rules.Input);
        foreach ((int pov, int directions) in new[]
                 {
                     (0, Up), (4500, Up | Right), (9000, Right), (13500, Right | Down), (18000, Down),
                     (22500, Down | Left), (27000, Left), (31500, Left | Up), (8999, Up | Right), (9001, Right | Down),
                 })
        {
            bindings.UpdateJoystickPov(pov);
            Assert.Equal(directions, bindings.PollActionMask() & 0xf);
        }

        // While the POV is pressed the axes are not read; centred, they are.
        bindings.UpdateJoystickAxes(-1.0, 1.0);                       // left and down
        bindings.UpdateJoystickPov(0);
        Assert.Equal(Up, bindings.PollActionMask() & 0xf);
        bindings.UpdateJoystickPov(InputBindings.JoystickPovCentered);
        Assert.Equal(Left | Down, bindings.PollActionMask() & 0xf);

        // 4.43B's polls (0x473ed0) read no POV.
        var older = new InputBindings(EngineBuilds.Age443B.Rules.Input);
        older.UpdateJoystickPov(9000);
        Assert.Equal(0, older.PollActionMask() & 0xf);
    }

    [Fact]
    public void OpTwoFaStopsTheAxesMakingDirectionsButNotThePov()
    {
        VirtualMachine vm = Run(Assemble("FIELD.BIN", (SetAxesIgnored, [I(1)]), (Exit, [])));
        InputBindings bindings = vm.InputBindings;
        Assert.Equal(1, bindings.JoystickAxesFlags);
        bindings.UpdateJoystickAxes(1.0, 0.0);
        Assert.Equal(0, bindings.PollActionMask() & 0xf);
        bindings.UpdateJoystickPov(18000);
        Assert.Equal(Down, bindings.PollActionMask() & 0xf);
        bindings.JoystickAxesFlags = 0;
        bindings.UpdateJoystickPov(InputBindings.JoystickPovCentered);
        Assert.Equal(Right, bindings.PollActionMask() & 0xf);
    }

    [Fact]
    public void EachJoystickReaderPollsAgainOnlyAfterTwentyMilliseconds()
    {
        var bindings = new InputBindings(EngineBuilds.Age445D.Rules.Input);
        Span<float> axes = stackalloc float[InputBindings.JoystickAxisCount];
        bindings.UpdateJoystickAxis(5, 1.0);
        bindings.ReadJoystickAxes(1000, axes);
        Assert.Equal(1f, axes[5]);
        Assert.Equal(32768f / 65535f, axes[0]);                       // at rest, the middle of WinMM's range
        bindings.UpdateJoystickAxis(5, -1.0);
        bindings.ReadJoystickAxes(1020, axes);                        // within 20 ms: the last poll's positions
        Assert.Equal(1f, axes[5]);
        bindings.ReadJoystickAxes(1021, axes);
        Assert.Equal(0f, axes[5]);

        // The buttons reader gives the buttons only from the call that polls (input_read_joystick_buttons@0x4745e0).
        bindings.UpdateJoystickButton(3, true);
        Assert.Equal(1u << 3, bindings.ReadJoystickButtons(1000));
        Assert.Equal(0u, bindings.ReadJoystickButtons(1020));
        Assert.Equal(1u << 3, bindings.ReadJoystickButtons(1041));
    }

    [Fact]
    public void OpsTwoF9AndTwoFbReadJoystickZeroOnlyWhenTheHostHasOne()
    {
        Script script = Assemble("SETJOY.BIN",
            (ReadAxes, [G(0x100), GF(1), GF(2), GF(3), GF(4), GF(5), GF(6)]),
            (ReadButtons, [G(0x101)]),
            (ReadButtons, [G(0x102)]),
            (Exit, []));

        var host = new TimedHost { JoystickButtonCount = 10 };
        var vm = new VirtualMachine(script, Table, host, build: EngineBuilds.Age445D);
        vm.InputBindings.UpdateJoystickAxis(0, 1.0);
        vm.InputBindings.UpdateJoystickAxis(1, -1.0);
        vm.InputBindings.UpdateJoystickButton(3, true);
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(1, Global(vm, 0x100));
        Assert.Equal(1f, FloatCell(vm, 1));
        Assert.Equal(0f, FloatCell(vm, 2));
        Assert.Equal(32768f / 65535f, FloatCell(vm, 3));
        Assert.Equal(8, Global(vm, 0x101));
        Assert.Equal(0, Global(vm, 0x102));                            // the same millisecond: no fresh poll

        // Without a joystick (op 0x106's count 0) native writes 0; the port writes 0 for the six floats too.
        vm = new VirtualMachine(script, Table, new TimedHost(), build: EngineBuilds.Age445D);
        vm.InputBindings.UpdateJoystickAxis(0, 1.0);
        vm.InputBindings.UpdateJoystickButton(3, true);
        vm.Run();
        Assert.Equal(0, Global(vm, 0x100));
        Assert.Equal(0f, FloatCell(vm, 1));
        Assert.Equal(0, Global(vm, 0x101));
    }

    // ---- op 0x12d ----

    [Fact]
    public void OpTwelveDTestsTwoMovedBoxesForOverlapWithTheirEdges()
    {
        // Boxes are four cells: left, right, top, bottom. A is (0..10, 0..10); B is (10..20, 5..6).
        var ops = new List<(int, Operand[])>
        {
            (Mov, [G(0x200), I(0)]), (Mov, [G(0x201), I(10)]), (Mov, [G(0x202), I(0)]), (Mov, [G(0x203), I(10)]),
            (Mov, [G(0x210), I(10)]), (Mov, [G(0x211), I(20)]), (Mov, [G(0x212), I(5)]), (Mov, [G(0x213), I(6)]),
            (BoxesOverlap, [G(0x300), G(0x200), I(0), I(0), G(0x210), I(0), I(0)]),     // touching edges overlap
            (BoxesOverlap, [G(0x301), G(0x200), I(0), I(0), G(0x210), I(1), I(0)]),     // B moved right by 1
            (BoxesOverlap, [G(0x302), G(0x200), I(1), I(0), G(0x210), I(1), I(0)]),     // both moved: again
            (BoxesOverlap, [G(0x303), G(0x200), I(0), I(-5), G(0x210), I(0), I(0)]),    // A up by 5: bottom 5
            (BoxesOverlap, [G(0x304), G(0x200), I(0), I(-6), G(0x210), I(0), I(0)]),    // A up by 6: apart
            (Exit, []),
        };
        VirtualMachine vm = Run(ScriptAssembler.Assemble(Table, "CHMENU.BIN", ops, []));
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(1, Global(vm, 0x300));
        Assert.Equal(0, Global(vm, 0x301));
        Assert.Equal(1, Global(vm, 0x302));
        Assert.Equal(1, Global(vm, 0x303));
        Assert.Equal(0, Global(vm, 0x304));
    }

    // ---- ops 0xce and 0xcf ----

    [Fact]
    public void OpCfRunsTheOnTimeTargetAndCatchesUpOncePerMissedInterval()
    {
        // On-time (20): count 0x100 and keep the catch-ups so far in 0x102; catch-up (33): count 0x101.
        VirtualMachine vm = Run(Assemble("LAUNCH.BIN",
            (RegisterCatchUp, [I(10), I(20), I(33)]),        // 0
            (Sleep, [I(5)]),                                  // 7
            (DispatchCatchUp, []),                            // 10: 5 ms, not due
            (Sleep, [I(5)]),                                  // 11
            (DispatchCatchUp, []),                            // 14: 10 ms, on time
            (Sleep, [I(45)]),                                 // 15
            (DispatchCatchUp, []),                            // 18: 45 ms, late by three more intervals
            (Exit, []),                                       // 19
            (Add, [G(0x100), G(0x100), I(1)]),                // 20
            (Mov, [G(0x102), G(0x101)]),                      // 27
            (Ret, []),                                        // 32
            (Add, [G(0x101), G(0x101), I(1)]),                // 33
            (Ret, [])));                                      // 40
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(2, Global(vm, 0x100));
        // 45 ms is four whole intervals and 5 ms: four catch-up runs, then the on-time target after them.
        Assert.Equal(4, Global(vm, 0x101));
        Assert.Equal(4, Global(vm, 0x102));
    }

    [Fact]
    public void OpCfRunsAtMost128CatchUps()
    {
        VirtualMachine vm = Run(Assemble("LAUNCH.BIN",
            (RegisterCatchUp, [I(1), I(12), I(20)]),          // 0
            (Sleep, [I(1000)]),                               // 7
            (DispatchCatchUp, []),                            // 10
            (Exit, []),                                       // 11
            (Add, [G(0x100), G(0x100), I(1)]),                // 12
            (Ret, []),                                        // 19
            (Add, [G(0x101), G(0x101), I(1)]),                // 20
            (Ret, [])));                                      // 27
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(1, Global(vm, 0x100));
        Assert.Equal(128, Global(vm, 0x101));
    }

    [Fact]
    public void ALateDispatchWithNoCatchUpTargetLeavesItsReturnPcPushed()
    {
        // Native pops what it queued but the first push and falls through, so the frame's next ret returns to the
        // instruction after op 0xcf and runs it again.
        VirtualMachine vm = Run(Assemble("LAUNCH.BIN",
            (RegisterCatchUp, [I(10), I(19), I(-1)]),         // 0
            (Sleep, [I(30)]),                                 // 7
            (DispatchCatchUp, []),                            // 10
            (Add, [G(0x102), G(0x102), I(1)]),                // 11
            (Ret, []),                                        // 18
            (Add, [G(0x100), G(0x100), I(1)]),                // 19: on time, not run
            (Ret, [])));                                      // 26
        Assert.Equal(2, Global(vm, 0x102));
        Assert.Equal(0, Global(vm, 0x100));
        Assert.DoesNotContain("timed-catchup", vm.HaltReason ?? "");
    }

    [Fact]
    public void AnOnTimeDispatchWithNoOnTimeTargetPopsItsReturnPc()
    {
        VirtualMachine vm = Run(Assemble("LAUNCH.BIN",
            (RegisterCatchUp, [I(10), I(-1), I(19)]),         // 0
            (Sleep, [I(10)]),                                 // 7
            (DispatchCatchUp, []),                            // 10
            (Add, [G(0x102), G(0x102), I(1)]),                // 11
            (Ret, []),                                        // 18: the frame returns
            (Add, [G(0x101), G(0x101), I(1)]),                // 19: late, not run
            (Ret, [])));                                      // 26
        Assert.Equal(1, Global(vm, 0x102));
        Assert.Equal(0, Global(vm, 0x101));
    }

    [Fact]
    public void FastForwardDispatchesAtOnce()
    {
        VirtualMachine vm = Run(Assemble("LAUNCH.BIN",
            (RegisterCatchUp, [I(1000), I(9), I(17)]),        // 0
            (DispatchCatchUp, []),                            // 7: 0 ms
            (Exit, []),                                       // 8
            (Add, [G(0x100), G(0x100), I(1)]),                // 9: on time
            (Ret, []),                                        // 16
            (Add, [G(0x101), G(0x101), I(1)]),                // 17: late
            (Ret, [])), skipAndAuto: TestSkipAndAuto.HoldingFastForward());
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(1, Global(vm, 0x100));
        Assert.Equal(0, Global(vm, 0x101));
    }

    private static Script WithPackedId(Script source, uint packedId)
        => new()
        {
            Name = source.Name,
            PackedId = packedId,
            Header = source.Header,
            Instructions = source.Instructions,
            IndexByOffset = source.IndexByOffset,
            Strings = source.Strings,
            BodyDwords = source.BodyDwords,
            ReadMessageOffsets = source.ReadMessageOffsets,
            ScriptCallOffsets = source.ScriptCallOffsets,
            LocalCallOffsets = source.LocalCallOffsets,
        };

    [Theory]
    [InlineData(10, "Depth が不正です 16777222 != 16777221")]   // on time: the raw ids (0x616a38)
    [InlineData(25, "Depth が不正です INNER != OUTER")]         // late: the names (0x6157ec)
    public void ACallbackRegisteredByTheCallerIsADepthErrorAtTheCalleesDispatch(int sleep, string message)
    {
        Script outer = WithPackedId(Assemble("OUTER",
            (RegisterCatchUp, [I(10), I(11), I(11)]),         // 0
            (CallScript, [I(5)]),                             // 7
            (Exit, []),                                       // 10
            (Mov, [G(0x120), I(1)]),                          // 11: the callback
            (Ret, [])), 0x01000005);
        Script inner = WithPackedId(Assemble("INNER",
            (Sleep, [I(sleep)]),                              // 0
            (DispatchCatchUp, []),                            // 3
            (Ret, [])), 0x01000006);
        var host = new TimedHost();
        var vm = new VirtualMachine(outer, Table, host, provider: new MapProvider(new() { [5] = inner }),
                                    build: EngineBuilds.Age445D);
        vm.Run();
        Assert.Equal("script-error: " + message, vm.HaltReason);
        Assert.Single(host.Diagnostics);
        Assert.Equal(0, Global(vm, 0x120));
    }
}
