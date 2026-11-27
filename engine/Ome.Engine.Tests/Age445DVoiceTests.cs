using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Devices;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Persistence;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// AGE 4.45D's three voices (docs/engine-re.md, "魔導巧殻 against 天秤のLa DEA。, triaged"): each voice on its own
/// channel with its own mute request, delayed start and History records, the ops 0x2f3 to 0x2f8 that name a voice, and
/// the background voice of the facade's tick (0x4a9dc0), each as /ladea/4.45D's handlers and workers leave it.
/// </summary>
public class Age445DVoiceTests
{
    private const int T_IMM = 0, T_GINT = 3;
    private const int Exit = 0x2, PlayVoice = 0xc4, FindVoicePair = 0x2f3, PlayVoiceN = 0x2f4,
                      ScheduleVoiceN = 0x2f5, StopVoice = 0x2f6, MuteNextVoiceN = 0x2f7, SetVoicePan = 0x2f8,
                      VoiceIsPlaying = 0x1c4;

    private static readonly AudioRules Rules = EngineBuilds.Age445D.Rules.Audio;
    private static readonly string Muted = RecordingAudioDevice.Db(NativeAudioVolume.ToDecibels(0));

    private static Operand I(long value) => new(T_IMM, value);
    private static Operand G(int address) => new(T_GINT, address);

    private static (VirtualMachine Vm, AudioRig Rig) Run(AdvTextHistory? history, params (int, Operand[])[] ops)
        => Run(history, new RecordingWaits(), ops);

    private static (VirtualMachine Vm, AudioRig Rig) Run(AdvTextHistory? history, RecordingWaits waits,
                                                         params (int, Operand[])[] ops)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
        var rig = new AudioRig(rules: Rules);
        var all = ops.Append((Exit, Array.Empty<Operand>())).ToList();
        VirtualMachine vm = rig.Vm(ScriptAssembler.Assemble(table, "VOICES.BIN", all, []), table, waits,
                                   textHistory: history);
        vm.Run();
        return (vm, rig);
    }

    [Fact]
    public void FourFortyFiveDIsFourFortyThreeBPlusItsChanges()
    {
        EngineRules parent = EngineBuilds.Age443B.Rules, child = EngineBuilds.Age445D.Rules;
        Assert.Equal(parent.Text with
        {
            DashStrip = Ome.Engine.Text.DashStripRows.BlackBox,
            LiveTextRedraw = Ome.Engine.Text.LiveTextRedraw.WindowStyle,
        }, child.Text);
        Assert.Equal(parent.Gfx, child.Gfx);
        Assert.Equal(parent.Audio with { Voices = 3, BackgroundVoices = true }, child.Audio);
        Assert.Equal(parent.Settings.BoundKeys, child.Settings.BoundKeys);
        Assert.Equal(parent.Shell with { VoiceEndDisablesVoicePitch = false }, child.Shell);
        Assert.Equal(parent.Input with { JoystickPovDirections = true }, child.Input);
        Assert.Equal("445D", child.Saves.VersionStamp);
    }

    [Fact]
    public void TheVmRefusesAnAudioNativeWithAnotherBuildsVoices()
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
        Script script = ScriptAssembler.Assemble(table, "VOICES.BIN", [(Exit, Array.Empty<Operand>())], []);
        var oneVoice = new AudioRig();
        Assert.Throws<ArgumentException>(() => new VirtualMachine(script, table, new RecordingWaits(),
            build: EngineBuilds.Age445D, skipAndAuto: oneVoice.Skip, audio: oneVoice.Audio, tick: oneVoice.Tick));
        // Without an audio native the VM's silent one has the build's voices.
        var vm = new VirtualMachine(script, table, new RecordingWaits(), build: EngineBuilds.Age445D);
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
    }

    [Fact]
    public void EachVoicePlaysOnItsOwnChannelWithItsOwnMuteRequest()
    {
        var (vm, rig) = Run(null,
            (MuteNextVoiceN, [I(1)]),
            (PlayVoice, [I(10)]),                   // voice 0, audible: the request is voice 1's
            (PlayVoiceN, [I(11), I(0), I(1)]),      // voice 1, muted
            (PlayVoiceN, [I(12), I(0), I(2)]),
            (VoiceIsPlaying, [G(0x100)]));

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(15, rig.Audio.ChannelCount);
        Assert.Equal(["play 12 voice:10 once 0.0dB", $"play 13 voice:11 once {Muted}", "play 14 voice:12 once 0.0dB"],
            rig.Device.CommandsStartingWith("play"));
        Assert.Equal(1, vm.Globals[0x100]);
    }

    [Fact]
    public void StoppingAVoiceReleasesItsChannelAndLeavesTheOthers()
    {
        var (vm, rig) = Run(null,
            (PlayVoiceN, [I(10), I(0), I(0)]),
            (PlayVoiceN, [I(11), I(0), I(1)]),
            (StopVoice, [I(0)]),
            (VoiceIsPlaying, [G(0x100)]),
            (StopVoice, [I(1)]),
            (VoiceIsPlaying, [G(0x101)]),
            (StopVoice, [I(2)]));                   // a voice that holds nothing

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(["unload 12", "unload 13", "unload 14"], rig.Device.CommandsStartingWith("unload"));
        Assert.Equal(1, vm.Globals[0x100]);
        Assert.Equal(0, vm.Globals[0x101]);
        Assert.False(rig.Audio.ChannelHoldsSound(12));
    }

    [Fact]
    public void AVoiceOutsideTheThreeHaltsAndAPanPastTheChannelsIsSkipped()
    {
        var (stop, _) = Run(null, (StopVoice, [I(3)]));
        Assert.StartsWith("index-out-of-range", stop.HaltReason);

        // sound_set_channel_pan traces a channel past 14 and returns; voice 2's pan is clamped and set.
        var waits = new RecordingWaits();
        var (pan, _) = Run(null, waits, (SetVoicePan, [I(3), I(0)]), (SetVoicePan, [I(2), I(20000)]));
        Assert.Equal("exit", pan.HaltReason);
        Assert.Contains("関数：dsSetVolume エラー：不正なSound番号です", Assert.Single(waits.Warnings));

        var (negative, _) = Run(null, (SetVoicePan, [I(-1), I(0)]));   // channel 11: the BGM's, outside the voices
        Assert.StartsWith("index-out-of-range", negative.HaltReason);
    }

    [Fact]
    public void HistoryKeepsEachRecordsVoiceAndFindsThemByVoice()
    {
        var history = new AdvTextHistory();
        history.ResetLayout(1);
        var (vm, _) = Run(history,
            (PlayVoice, [I(10)]),
            (PlayVoiceN, [I(11), I(1), I(2)]),      // History's argument is 0 whatever the variant
            (FindVoicePair, [G(0x100), G(0x101), G(0x102), I(0), I(0), I(0)]),
            (FindVoicePair, [G(0x110), G(0x111), G(0x112), I(0), I(0), I(2)]),
            (FindVoicePair, [G(0x120), G(0x121), G(0x122), I(0), I(0), I(1)]));

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal([0, 2], history.Records.Select(record => record.VoiceIndex));
        Assert.Equal([10, 0, 0], new[] { vm.Globals[0x100], vm.Globals[0x101], vm.Globals[0x102] });
        // Op 0x1d4 finds voice 0's, as the builds before 4.45D do (no script of 天秤のLa DEA。 has it).
        Assert.True(history.TryFindVoicePair(0, out long firstId, out _));
        Assert.Equal(10, firstId);
        Assert.Equal([11, 0, 0], new[] { vm.Globals[0x110], vm.Globals[0x111], vm.Globals[0x112] });
        Assert.Equal([-1, -1, 0], new[] { vm.Globals[0x120], vm.Globals[0x121], vm.Globals[0x122] });

        var restored = new AdvTextHistory();
        NativeTextHistoryCodec.DecodeInto(NativeTextHistoryCodec.Encode(history), restored);
        Assert.Equal([0, 2], restored.Records.Select(record => record.VoiceIndex));
        Assert.True(restored.TryFindVoicePair(0, 2, out long voiceId, out _, out _));
        Assert.Equal(11, voiceId);
    }

    [Fact]
    public void ABackgroundVoiceLoopsAroundTheOtherVoicesAndIsNotCounted()
    {
        var (vm, rig) = Run(null, (ScheduleVoiceN, [I(20), I(3), I(300), I(2)]));   // as the SP scripts pass it
        Assert.Equal("exit", vm.HaltReason);
        rig.Advance(0);                             // the request takes this tick
        rig.Advance(300);
        Assert.Equal(["play 14 voice:20 once 0.0dB"], rig.Device.CommandsStartingWith("play"));
        Assert.True(rig.Audio.IsChannelPlaying(14));
        Assert.False(rig.Audio.IsVoicePlaying);     // voice_any_playing skips it

        rig.Audio.PlayVoice(10);
        rig.Advance(0);                             // another voice plays: the background voice pauses
        Assert.Equal(["stop 14"], rig.Device.CommandsStartingWith("stop 14"));
        rig.Device.End(12);
        rig.Advance(0);                             // the others are silent: it waits its delay again
        rig.Advance(299);
        Assert.Single(rig.Device.PlayedOn(14));
        rig.Advance(1);
        Assert.Equal(["voice:20", "voice:20"], rig.Device.PlayedOn(14));

        rig.Audio.StopVoice(2);                     // op 0x2f6 ends the cycle
        rig.Advance(1000);
        Assert.Equal(2, rig.Device.PlayedOn(14).Count);
    }

    [Fact]
    public void ADelayedStartIsDueOnItsFirstTickAndClearsTheMuteRequest()
    {
        // voice_tick_delayed_playback@0x4884d0 (4.24J; 4.43B and 4.45D alike) takes the first tick and compares in it,
        // so no delay plays at once; and the start clears the mute request, so it is heard and the next line too. The
        // engine tick then marks the voice pending for Auto (4.24J 0x411a89).
        var rig = new AudioRig();
        rig.Audio.MuteNextVoice();
        rig.Audio.ScheduleVoicePlayback(10, 3, 0);
        Assert.False(rig.Skip.AutoWaitStateFor(0).VoicePending);
        rig.Advance(0);
        Assert.Equal([(10L, false)], rig.Device.Voices);
        Assert.True(rig.Audio.IsVoicePlaying);      // one voice: no background voice
        Assert.True(rig.Skip.AutoWaitStateFor(0).VoicePending);
        rig.Audio.PlayVoice(11);
        Assert.Equal([(10L, false), (11L, false)], rig.Device.Voices);
    }

    [Fact]
    public void AVoiceIsPendingForAutoOnlyWhileTheVoiceRouteIsOn()
    {
        // op_0xc4_play_voice@0x420610 and the tick's delayed start set adv_auto_voice_pending only when the Voice route
        // (ctx+0x1491c) is on; a background voice's start never does.
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
        var rig = new AudioRig(rules: Rules);
        Assert.True(rig.Audio.TrySetRouteEnabled((int)AudioMixerCategory.Voice, false));
        (int, Operand[])[] ops = [(PlayVoice, [I(10)]), (PlayVoiceN, [I(11), I(0), I(1)]), (Exit, [])];
        VirtualMachine off = rig.Vm(ScriptAssembler.Assemble(table, "VOICES.BIN", ops.ToList(), []), table,
                                    new RecordingWaits());
        off.Run();
        rig.Audio.ScheduleVoicePlayback(12, 0, 0);
        rig.Advance(0);
        Assert.False(rig.Skip.AutoWaitStateFor(0).VoicePending);

        Assert.True(rig.Audio.TrySetRouteEnabled((int)AudioMixerCategory.Voice, true));
        rig.Device.End(12);                         // a background voice waits while another voice plays
        rig.Device.End(13);
        rig.Audio.ScheduleVoicePlayback(20, 3, 0, voice: 2);
        rig.Advance(0);
        Assert.Single(rig.Device.PlayedOn(14));
        Assert.False(rig.Skip.AutoWaitStateFor(0).VoicePending);
        VirtualMachine on = rig.Vm(ScriptAssembler.Assemble(table, "VOICES.BIN", ops.ToList(), []), table,
                                   new RecordingWaits());
        on.Run();
        Assert.True(rig.Skip.AutoWaitStateFor(0).VoicePending);
    }
}
