namespace Ome.Engine.Vm;

/// <param name="EmitCap">Headless runaway-line guard: the VM halts (<c>LOOP:line@…</c>) when one frame shows the same
/// authored inline string more than this many times. Native AGE has no such limit; the interactive session passes
/// <see cref="int.MaxValue"/> (GodotVmOptions).</param>
/// <param name="HaltAtWaitForInput">When true, the VM halts (reason "wait-for-input") at op 0x72, and at op 0xfa
/// when it would wait, instead of calling the waits and continuing. This is the FAITHFUL headless semantics: with no player to click,
/// "the scene is waiting for input" means stop here — not pretend the click already happened and plow on
/// through every prompt into code no real playthrough reaches (which is what made a headless SC0000 spin
/// 493k× in the name-entry poll loop). Leave false for interactive frontends that really block on input
/// (Godot) and for the dialogue-coverage sweep that deliberately walks every page.</param>
/// <param name="IgnoreExitRequests">Debug-only divergence: treat op 0x1 as a no-op so unreachable
/// post-exit bytecode can be explored. Leave false for native-faithful execution.</param>
/// <param name="NativeStringCodePage">Encoding used when an opcode measures the engine's byte-string
/// representation. SYS4 defaults to CP932; another container frontend can select its own code page.</param>
/// <param name="CreateObject">Native set:CreateObject (SYS4INI CREATEOBJECT): the renderer backends AGE creates, one bit
/// per DrawMode (<see cref="Sys4.Sys4StartupSettings.CreateObject"/>); the default, 2, is both shipped games' object
/// backend. Bit 1 arms <paramref name="AutoFreeTex"/>.</param>
/// <param name="AutoFreeTex">Native set:AutoFreeTex (SYS4INI AUTOFREETEX, registered default 0). Under CreateObject bit 1,
/// bit 1 releases all 1000 surfaces before a numbered load restores its flagged records, so only those come back; bit 0
/// releases them at op 0x9's root reload (the scene reset itself keeps every surface). Kamidori sets 3;
/// Himegari has none, so its initialized system textures survive a numbered load.</param>
/// <param name="WheelKeyUpAction">Native set:WheelKeyUp (SYS4INI WHEELKEYUP, registered default 3): the logical action
/// a wheel-up raises while an ADV text wait polls (<see cref="VirtualMachine.ApplyMouseWheel"/>).</param>
/// <param name="WheelKeyDownAction">Native set:WheelKeyDown (SYS4INI WHEELKEYDOWN, registered default 1).</param>
/// <param name="GameVersion">Native set:GameVersion, the game's version text: SYS4INI GAMEVERSION, or with VERREGPOS
/// the installer's version from the registry (<c>Ome.Engine.Frontend.Shell.GameVersionRule</c>, which the front end
/// applies). AGE 4.38D's op 0x2eb reads it; 創刻のアテリアル's title shows it.</param>
/// <param name="HWheelKeyUpAction">Native set:HWheelKeyUp (SYS4INI HWHEELKEYUP, registered default -1; AGE 4.38D):
/// the logical action a horizontal wheel turn to the right raises while an ADV text wait polls
/// (<see cref="VirtualMachine.ApplyMouseHorizontalWheel"/>); negative raises none.</param>
/// <param name="HWheelKeyDownAction">Native set:HWheelKeyDown (SYS4INI HWHEELKEYDOWN, registered default -1).</param>
/// <param name="TransferMusicVolume">Native set:TransferMusicVolume (SYS4INI TRANSFERMUSICVOLUME, registered default 0;
/// AGE 4.43B): what op 0xc2 does with a fade that is still running (<c>Ome.Engine.Natives.Audio.FadeBgm</c>). 魔導巧殻
/// sets 1.</param>
/// <remarks>CancelMesSkipOnClick, CoexistMesSkip and the fast-forward pace are the skip and auto native's
/// (<c>Ome.Engine.Natives.SkipAndAutoSettings</c>), and NoSaveDat is the save and settings native's
/// (<c>Ome.Engine.Natives.SavesAndSettings</c>).</remarks>
public sealed record VmOptions(int EmitCap = 2, long MaxSteps = 2_000_000, int CallDepthCap = 64,
                               bool HaltAtWaitForInput = false, bool IgnoreExitRequests = false,
                               int NativeStringCodePage = 932, int CreateObject = 2,
                               int AutoFreeTex = 0,
                               int WheelKeyUpAction = 3, int WheelKeyDownAction = 1,
                               string GameVersion = "",
                               int HWheelKeyUpAction = -1, int HWheelKeyDownAction = -1,
                               int TransferMusicVolume = 0)
{
    /// <summary>The selected profile's id, which labels an unsupported-opcode diagnostic
    /// (<see cref="UnsupportedOpcodeDiagnostic.ProfileId"/>). No behaviour reads it.</summary>
    public string ProfileId { get; init; } = "unselected";

    /// <summary>Probe mode (<c>--probe</c>): an unsupported opcode is reported and skipped instead of halting the
    /// run.</summary>
    public bool ProbeMode { get; init; }

    /// <summary>Whether the run records its writes (game data export spec §6.2): each global cell's writer
    /// (<see cref="Ome.Engine.Natives.ScriptMemory.Writers"/>) and each global string cell's origin
    /// (<see cref="Ome.Engine.Natives.TextManager.StringStores"/>). A <see cref="GameSession"/> carries both records
    /// from one recording run to the next.</summary>
    public bool RecordsWrites { get; init; }

    /// <summary>A site the run stops at, before its instruction runs, the first time any frame reaches it (game data
    /// export spec §6.2); null for none. The run ends as a stop request ends it, with
    /// <see cref="RunStop.HaltReason"/>.</summary>
    public RunStop? StopAt { get; init; }
}

/// <summary>A site a run stops at (<see cref="VmOptions.StopAt"/>): a script, named as text catalogs and patch points
/// name it (upper case, no <c>.BIN</c>, its <c>$n$</c> prefix kept), and an instruction's offset in it.</summary>
public sealed record RunStop
{
    public RunStop(string script, int offset)
    {
        Script = Ome.Engine.Localization.TextCatalog.NormalizeScriptName(script);
        Offset = offset;
    }

    public string Script { get; }
    public int Offset { get; }

    /// <summary>The halt reason of a run that stopped here.</summary>
    public string HaltReason => $"stop:{Script}@0x{Offset:x}";
}
