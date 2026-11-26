using Ome.Engine.Devices;
using Ome.Engine.Model;

namespace Ome.Engine.Natives;

public sealed partial class Audio
{
    private const int FullLevel = AudioMixerSettings.MaximumVolume;
    // The voice duck release: bgm_fade_arm(10000, 1) on the 10 ms period timer, one progress point per period, so the
    // level runs from the ducked level to full in 100 steps over one second.
    private const long DuckReleaseMs = 1000;
    private const int DuckReleaseSteps = 100;

    /// <summary>Run-state 0x200: a fade of the BGM envelope. <see cref="Script"/> marks an op 0xc2 fade, which a BGM op
    /// ends first and whose zero target releases the source and clears the retained track; the duck release is the
    /// other kind.</summary>
    private sealed record BgmFade(int From, int Target, int Steps, long StartMs, long DurationMs, bool Script);

    // The retained track, pause flag and start mode are the scene's (_scene.BgmTrack, BgmPaused, BgmStartMode).
    private bool _bgmLoaded;                 // a track is live on channel 11
    // The track last decoded on channel 11, and whether the device holds it. The DirectSound backend's stop
    // (vtable 0x57977c +4, 0x468980) only stops the channel's buffer, and only a decode replaces it, so native's channel
    // keeps the track after a stop; the port unloads the device and keeps the clip.
    private AudioClip? _bgmChannelClip;
    private bool _bgmChannelLoaded;
    private int _bgmLevel = FullLevel;       // bgm object +0x420: the committed envelope level, basis points
    private int _bgmChannelLevel = FullLevel;   // the level channel 11 was last given (a fade step is not committed)
    // Op 0xc1 stopped channel 11's buffer where it is (sound_channel_pause, sound_buffer_stop), which clears the buffer's
    // playing flag (+0x2460): the BGM backend's is-playing (4.43B 0x47de00) then reports no music.
    private bool _bgmChannelHeld;
    private BgmFade? _fade;
    private bool _duckActive;                // run-state 0x4000
    private int _duckRestoreLevel = FullLevel;
    private bool _duckReleaseArmed;          // voice_bgm_duck_release_armed (op 0x1bf, a delayed voice start)

    /// <summary>
    /// Op 0xbf (bgm_play_track, start mode 1). The retained track keeps its stream and position: FIELD reasserts the
    /// stage BGM after every unit's action (docs/engine-re.md, "Forced BGM lifecycle siblings"). Where
    /// <paramref name="comparesStartMode"/> (the build's <see cref="AudioRules.PlayComparesStartMode"/>) it does so only
    /// with the retained start mode 1, so a track op 0xb9 played once, or one a scene reset left, starts again. Another
    /// track starts; zero starts the retained track again, and with none releases the source.
    /// </summary>
    public void PlayBgm(long trackId, bool comparesStartMode = false)
    {
        const int loop = 1;
        CompleteScriptFade();
        bool keeps = trackId != 0 && trackId == _scene.BgmTrack
            && (!comparesStartMode || _scene.BgmStartMode == loop);
        // Every path leaves the start mode at 1: 4.24J stores it before its check, and 4.33G's check keeps only a 1.
        _scene.BgmStartMode = loop;
        if (!keeps) StartOrRelease(trackId, loop, forced: false);
        UpdateTimedWork();
    }

    /// <summary>Ops 0xb7 (start mode 1, loop) and 0xb9 (0, once), bgm_force_play_track: store the start mode and start a
    /// track even if it is the current one; zero restarts the retained track, and with none stops.</summary>
    public void RestartBgm(long trackId, int startMode)
    {
        CompleteScriptFade();
        _scene.BgmStartMode = startMode;
        StartOrRelease(trackId, startMode, forced: true);
        UpdateTimedWork();
    }

    // A nonzero track is retained and started; zero starts the retained track, and with none releases the source.
    private void StartOrRelease(long trackId, int startMode, bool forced)
    {
        if (trackId != 0) _scene.BgmTrack = trackId;
        if (_scene.BgmTrack == 0) StopBgmSource();
        else StartBgm(_scene.BgmTrack, startMode, forced);
    }

    /// <summary>Op 0xc3: end a running op 0xc2 fade, then set the retained track without starting it
    /// (op_0xc3_complete_fade_set_bgm_track, 4.43B 0x420140; 4.33G's and 4.24J's the same). The next fade's arm starts
    /// it from silence (<see cref="RestartRetainedTrackForFade"/>), and a play of track 0 starts it at once.</summary>
    public void SetBgmTrack(long trackId)
    {
        CompleteScriptFade();
        _scene.BgmTrack = trackId;
        _timeline?.Event("bgm-track", new() { ["id"] = trackId });
        UpdateTimedWork();
    }

    /// <summary>Op 0xb8: release the source and clear the retained track (bgm_stop_release@0x464690).</summary>
    public void StopBgm()
    {
        CompleteScriptFade();
        _scene.BgmTrack = 0;
        StopBgmSource();
        UpdateTimedWork();
    }

    /// <summary>
    /// Op 0xc1: flip the BGM's pause (bgm object +0x410) and tell the backend. The DirectSound backend stops channel
    /// 11's buffer where it is, or plays it on from there; with no track on the channel it does nothing, and the flag
    /// still flips. Starting a track and releasing the source clear the flag; an identical play request, which keeps
    /// the stream, does not.
    /// </summary>
    public void ToggleBgmPause()
    {
        _scene.BgmPaused = !_scene.BgmPaused;
        _timeline?.Event("bgm-pause", new() { ["paused"] = _scene.BgmPaused });
        if (_bgmLoaded && _bgmChannelLoaded)
        {
            _device.SetPaused(BgmChannel, _scene.BgmPaused);
            _bgmChannelHeld = _scene.BgmPaused;
        }
        UpdateTimedWork();
    }

    /// <summary>The BGM object's pause flag (+0x410), which op 0xc1 flips. A scene reset zeroes it and leaves the
    /// stream, so a held stream stays held until a start, and the next op 0xc1 holds it again.</summary>
    public bool BgmPaused => _scene.BgmPaused;

    /// <summary>Op 0xc0: the retained track. A fade to zero that has run its course has cleared it, and on the builds
    /// whose arm clears it (<see cref="AudioRules.ZeroFadeClearsTrack"/>) one that has only begun.</summary>
    public long CurrentBgmTrack
    {
        get
        {
            ServiceFade(_clock.NowMs);
            UpdateTimedWork();
            return _scene.BgmTrack;
        }
    }

    /// <summary>Op 0xc2: fade the BGM envelope to a level in basis points (CONFIG passes the setting × 100) and return.
    /// The tick runs the fade while the script goes on; at its end the level is committed, and a zero target releases
    /// the source and clears the retained track (bgm_fade_tick@0x464960). Under ADV fast-forward native applies the
    /// endpoint at once; an advance click sets no fast-forward, so it keeps the authored length. A run on a standing
    /// clock applies it at once too (settleFadesAtOnce). Where <paramref name="zeroTargetClearsTrack"/> (the build's
    /// <see cref="AudioRules.ZeroFadeClearsTrack"/>) the arm clears the retained track for a target of exactly zero, and the
    /// music fades on: op 0xc0 and a numbered save made during the fade read no track. <paramref name="transferMode"/> is
    /// set:TransferMusicVolume, for a fade still running (<see cref="TransferFadeVolume"/>).</summary>
    public void FadeBgm(int targetBasisPoints, long durationMs, bool zeroTargetClearsTrack = false, int transferMode = 0)
    {
        if (!_skip.FastForwardActive) TransferFadeVolume(transferMode);
        long ms = _skip.FastForwardActive || _settleFadesAtOnce ? 0 : Math.Clamp(durationMs, 0, 60_000);
        _timeline?.Event("bgm-fade", new() { ["target_basis_points"] = targetBasisPoints, ["duration_ms"] = ms });
        RestoreDuck();
        _fade = null;
        RestartRetainedTrackForFade();
        var fade = new BgmFade(_bgmLevel, Math.Clamp(targetBasisPoints, 0, FullLevel),
            NativeAudioVolume.FadeSteps(ms), _clock.NowMs, ms, Script: true);
        if (ms == 0) CommitFade(fade, targetBasisPoints);
        else _fade = fade;
        // bgm_fade_arm tests the target it stores, unclamped, after its two restarts of the retained track.
        if (zeroTargetClearsTrack && targetBasisPoints == 0) _scene.BgmTrack = 0;
        UpdateTimedWork();
    }

    /// <summary>Op 0x1bf, and the tick after a delayed voice start: bit 0x10000 makes the op 0x1cf mask one-shot, and
    /// unless bit 0 suppresses it the duck release is armed for when the voice ends.</summary>
    public void ArmVoiceBgmDuckRelease()
    {
        ArmDuckRelease();
        UpdateTimedWork();
    }

    /// <summary>Op 0x1cf: the transient control mask. Bit 0 suppresses the duck at a voice start and the tick's
    /// release.</summary>
    public void SetVoiceBgmDuckControl(long flags)
    {
        _scene.DuckControl = unchecked((int)flags);
        _timeline?.State("voice-bgm-duck-control", new() { ["flags"] = flags });
        UpdateTimedWork();
    }

    // The start (bgm_play_track and bgm_force_play_track) clears the pause flag before it calls the backend, whose play
    // decodes the track into channel 11 and starts it, looping by the start mode. A start ends a fade, except the Music
    // route's restart (endsFade false), which leaves a running op 0xc2 fade to the tick.
    private void StartBgm(long trackId, int startMode, bool forced, bool endsFade = true)
    {
        AudioClip? clip = _assets.Bgm(trackId);
        _timeline?.Event("bgm", new()
        {
            ["id"] = trackId,
            ["file"] = clip?.Name,
            ["start_mode"] = startMode,
            ["force_restart"] = forced,
        });
        _scene.BgmPaused = false;
        if (clip == null) return;   // the track that played goes on
        if (endsFade) _fade = null;
        _bgmLevel = FullLevel;
        _bgmChannelLevel = FullLevel;
        _bgmLoaded = true;
        _bgmChannelClip = clip;
        _bgmChannelLoaded = true;
        _bgmChannelHeld = false;
        _device.Load(BgmChannel, AudioBus.Music, clip);
        PlayChannel(BgmChannel, loop: startMode != 0, NativeAudioVolume.ToDecibels(FullLevel));
    }

    /// <summary>
    /// bgm_fade_arm's two restarts (4.24J 0x464830, 4.38D 0x4764d0, 4.43B 0x47a0d0, alike), before it arms a fade: with
    /// a retained track, it plays the track again when the committed level is 0, and when channel 11 is not playing
    /// (released, run out, or held by op 0xc1), which also makes the committed level 0. The backend plays it silent: it
    /// sets channel 11's volume to 0 around the play and puts the stored volume back without applying it. The fade then
    /// rises from silence. 魔導巧殻 sets a track with op 0xc3 and fades it in this way (SC0000@0x50ae). The pause flag
    /// stays as it was.
    /// </summary>
    private void RestartRetainedTrackForFade()
    {
        if (_scene.BgmTrack == 0) return;
        if (_bgmLevel == 0) StartRetainedTrackSilently();
        if (_bgmChannelLoaded && _playing[BgmChannel] && !_bgmChannelHeld) return;
        StartRetainedTrackSilently();
        _bgmLevel = 0;
    }

    private void StartRetainedTrackSilently()
    {
        AudioClip? clip = _assets.Bgm(_scene.BgmTrack);
        _timeline?.Event("bgm-fade-restart", new()
        {
            ["id"] = _scene.BgmTrack,
            ["file"] = clip?.Name,
            ["start_mode"] = _scene.BgmStartMode,
        });
        if (clip == null) return;
        _bgmLoaded = true;
        _bgmChannelClip = clip;
        _bgmChannelLoaded = true;
        _bgmChannelHeld = false;
        _bgmChannelLevel = 0;
        _device.Load(BgmChannel, AudioBus.Music, clip);
        PlayChannel(BgmChannel, loop: _scene.BgmStartMode != 0, NativeAudioVolume.ToDecibels(0));
    }

    private void StopBgmSource()
    {
        _timeline?.Event("bgm-stop", new());
        ReleaseBgmSource();
    }

    // bgm_stop_release: the source goes, and the envelope starts again at full.
    private void ReleaseBgmSource()
    {
        RestoreDuck();
        _fade = null;
        ReleaseBgmStream();
    }

    private void ReleaseBgmStream()
    {
        _scene.BgmPaused = false;
        _bgmLoaded = false;
        _bgmChannelLoaded = false;
        _bgmChannelHeld = false;
        UnloadChannel(BgmChannel);
        _bgmLevel = FullLevel;
        _bgmChannelLevel = FullLevel;
    }

    // sound_set_music_route_enabled (4.38D 0x408460, 4.24J 0x407f80) releases the source, puts the retained track back
    // and calls bgm_force_play_track(0, +0x414) on the new route. The retained track starts again from its start at
    // full level, with the retained start mode and the pause cleared; the off route sounds nothing. With no retained
    // track (after a stop, a fade to zero or a scene reset) the source goes. A running op 0xc2 fade goes on: native's
    // tick advances it whatever the route, so a fade to zero still releases the source when it ends.
    private void RestartBgmForRoute(bool enabled)
    {
        if (_scene.BgmTrack == 0) ReleaseBgmStream();
        else if (enabled) StartBgm(_scene.BgmTrack, _scene.BgmStartMode, forced: true, endsFade: false);
        else
        {
            StopChannel(BgmChannel);
            _scene.BgmPaused = false;
            _bgmLevel = FullLevel;
        }
    }

    // Ops 0xb7/0xb8/0xb9/0xbf, and a numbered load, first end a running op 0xc2 fade: bgm_fade_tick(..., 100) applies
    // its endpoint. A fade that has run its course ends as the tick would end it.
    private void CompleteScriptFade()
    {
        if (_fade is not { Script: true } fade) return;
        if (_clock.NowMs - fade.StartMs < fade.DurationMs)
            _timeline?.Event("bgm-fade-complete", new());
        _fade = null;
        CommitFade(fade, fade.Target);
    }

    // bgm_fade_tick: the level at progress p is (from × (100 − p) + target × p) / 100, applied in whole steps and
    // committed only at 100, so a fade armed again part-way starts from the last committed level.
    private void ServiceFade(long now)
    {
        if (_fade is not { } fade) return;
        long elapsed = now - fade.StartMs;
        if (elapsed >= fade.DurationMs)
        {
            _fade = null;
            CommitFade(fade, fade.Target);
            return;
        }
        ApplyBgmChannelLevel(NativeAudioVolume.InterpolateFade(fade.From, fade.Target, FadeProgress(fade, now)));
    }

    /// <summary>
    /// AGE 4.43B's op 0xc2, outside fast-forward, with a fade running (run-state 0x200, an op 0xc2 fade or the duck's
    /// release) calls bgm_fade_transfer_volume@0x417b90 with set:TransferMusicVolume before it arms the new fade, which
    /// starts from the committed level (+0x420). Mode 1 makes that the level mixed so far, start × (100 − p) / 100 +
    /// target × p / 100 with the progress p below 100; mode 2 makes it the old target (+0x424); any other value keeps it,
    /// as older builds' op 0xc2, which has no such call, does. Native's tick has run before the op, so a fade whose time
    /// is up has ended.
    /// </summary>
    private void TransferFadeVolume(int mode)
    {
        if (mode is not (1 or 2)) return;
        ServiceFade(_clock.NowMs);
        if (_fade is not { } fade) return;
        if (mode == 2)
        {
            _bgmLevel = fade.Target;
            return;
        }
        int points = FadeProgress(fade, _clock.NowMs);
        if (points < 100) _bgmLevel = NativeAudioVolume.InterpolateFade(fade.From, fade.Target, points);
    }

    // The progress points native's tick has reached: one per period of a 100-step fade, ten per period of a 10-step one.
    private static int FadeProgress(BgmFade fade, long now)
        => (int)(Math.Max(0, now - fade.StartMs) * fade.Steps / fade.DurationMs) * (100 / fade.Steps);

    private void CommitFade(BgmFade fade, int target)
    {
        if (fade.Script && target <= 0)
        {
            _timeline?.Event("bgm-fade-end", new() { ["target_basis_points"] = target, ["released"] = true });
            ReleaseBgmSource();
            _scene.BgmTrack = 0;
            return;
        }
        if (fade.Script)
            _timeline?.Event("bgm-fade-end", new() { ["target_basis_points"] = target, ["released"] = false });
        SetBgmLevel(target);
    }

    private void SetBgmLevel(int basisPoints)
    {
        _bgmLevel = Math.Clamp(basisPoints, 0, FullLevel);
        ApplyBgmChannelLevel(_bgmLevel);
    }

    private void ApplyBgmChannelLevel(int basisPoints)
    {
        if (basisPoints == _bgmChannelLevel) return;
        _bgmChannelLevel = basisPoints;
        if (_bgmChannelLoaded) _device.SetVolume(BgmChannel, NativeAudioVolume.ToDecibels(basisPoints));
    }

    // Ops 0xb5/0xba on channel 11 (sound_buffer_start rewinds a buffer that has played): the track the channel holds
    // plays again from its start, once or looping by the op's start mode, at the channel's level, also after a BGM stop.
    // The retained start mode, which the Music route restarts the track with, stays.
    private void StartBgmChannel(int startMode)
    {
        if (_bgmChannelClip is not { } clip) return;
        _timeline?.Event("sfx-start", new() { ["channel"] = BgmChannel, ["start_mode"] = startMode,
            ["file"] = clip.Name });
        if (!_bgmChannelLoaded)
        {
            _device.Load(BgmChannel, AudioBus.Music, clip);
            _bgmChannelLoaded = true;
        }
        _bgmChannelHeld = false;
        PlayChannel(BgmChannel, loop: startMode != 0, NativeAudioVolume.ToDecibels(_bgmChannelLevel));
    }

    // voice_bgm_duck_begin@0x406de0 (4.43B 0x406e70 the same): at a voice start, with a retained track and unless a
    // fade owns the envelope, keep the level to restore and drop to sound:MusicFadeOnVoicePlayingVolume × 100 basis
    // points (0 reads as 100) at once. It arms a fade and ticks it to its end, so the arm's restarts come first: a
    // track that ran out, or that op 0xc1 holds, plays again from its start at the duck level.
    private void BeginDuck(int percent)
    {
        if (_scene.BgmTrack == 0 || _fade != null) return;
        if (!_duckActive) _duckRestoreLevel = _bgmLevel;
        _duckActive = true;
        _duckReleaseArmed = false;
        RestartRetainedTrackForFade();
        int target = percent * 100;
        SetBgmLevel(target == 0 ? 100 : target);
        _timeline?.Event("bgm-duck", new() { ["level"] = _bgmLevel });
    }

    // Leaving the duck at once (a BGM stop, an op 0xc2 fade, the Music route): the saved level comes back.
    private void RestoreDuck()
    {
        if (!_duckActive) return;
        SetBgmLevel(_duckRestoreLevel);
        _duckActive = false;
        _duckReleaseArmed = false;
    }

    private void ArmDuckRelease()
    {
        if ((_scene.DuckControl & 0x10000) != 0) _scene.DuckControl = 0;
        if ((_scene.DuckControl & 1) != 0) return;
        _duckReleaseArmed = true;
    }

    // engine_main_tick@0x411e12: with BGM ducked, once the voice has ended, the release is armed, no fade owns the
    // envelope and op 0x1cf bit 0 is clear. So BGM stays ducked between lines until the next line arms the release.
    private bool DuckReleaseDue()
        => _duckActive && _duckReleaseArmed && _fade == null && (_scene.DuckControl & 1) == 0 && !_playing[VoiceChannel];

    // The release swaps run-state 0x4000 for a 0x200 fade to full (10000, not the level saved at the duck), which a
    // new voice does not duck while it runs.
    private void ServiceDuckRelease()
    {
        if (!DuckReleaseDue()) return;
        _duckActive = false;
        _duckReleaseArmed = false;
        _timeline?.Event("bgm-duck-release", new() { ["level"] = _bgmLevel });
        RestartRetainedTrackForFade();
        _fade = new BgmFade(_bgmLevel, FullLevel, DuckReleaseSteps, _clock.NowMs, DuckReleaseMs, Script: false);
    }
}
