namespace Ome.Engine.Natives;

/// <summary>
/// The audio rules an engine build gives the audio native (docs/superpowers/specs/2026-11-12-engine-layering-design.md
/// §7.3). The builds that differ are declared in <c>Ome.Engine.Vm/EngineBuilds.cs</c>.
/// </summary>
/// <param name="PlayComparesStartMode">Whether op 0xbf keeps the retained track's stream only when the retained start
/// mode is also its own (1). 4.33G's <c>bgm_play_track@0x469e90</c> and 4.38D's <c>@0x4763e0</c> compare the track
/// and the start mode; 4.24J's <c>@0x464750</c> compares the track alone (docs/engine-re.md, "The retained start mode,
/// the scene reset and the Music route"; <see cref="Audio.PlayBgm"/>).</param>
/// <param name="ZeroFadeClearsTrack">Whether an op 0xc2 fade to zero clears the retained track when it is armed, while
/// the music still fades out. 4.24N's <c>bgm_fade_arm@0x465220</c>, 4.33G's <c>@0x469f80</c> and 4.38D's
/// <c>@0x4764d0</c> do; 4.24J's <c>@0x464830</c> leaves it to the fade's end (docs/engine-re.md, "BGM fades run in the
/// background"; <see cref="Audio.FadeBgm"/>).</param>
/// <param name="Voices">How many voices the build plays, each on its own channel from 12, so its sound manager has
/// 12 + <paramref name="Voices"/> channels. AGE 4.45D has three (ctx+0x14c68[3], the voice facade's per-voice arrays,
/// ops 0x2f4 to 0x2f8); the builds before it one (docs/engine-re.md, "魔導巧殻 against 天秤のLa DEA。, triaged";
/// <see cref="Audio.Voices"/>).</param>
/// <param name="BackgroundVoices">Whether a delayed voice whose playback variant has bits 0 and 1 set is a background
/// voice: AGE 4.45D's tick (0x4a9dc0) loops it, pauses it while another voice plays and starts it again its delay
/// after they end, and voice_any_playing@0x404c70 does not count it. The tick of 4.43B and before (4.43B 0x4a9110,
/// 4.24J voice_tick_delayed_playback@0x4884d0) starts it as any other (<see cref="Audio.BackgroundVoices"/>).</param>
public sealed record AudioRules(bool PlayComparesStartMode, bool ZeroFadeClearsTrack = false, int Voices = 1,
                                bool BackgroundVoices = false);
