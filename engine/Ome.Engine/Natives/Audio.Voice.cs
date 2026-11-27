using Ome.Engine.Devices;
using Ome.Engine.Model;

namespace Ome.Engine.Natives;

public sealed partial class Audio
{
    // voice_mute_next_flags (ctx+0x1493c), one per voice in AGE 4.45D (ctx+0x14c68[N]): op 0x1d5 or 0x2f7 requests
    // (bit 0), the voice's next play (op 0xc4 or 0x2f4) promotes it (0x10001) and plays muted, and a promoted request
    // belongs to that earlier voice, so the voice after it clears it.
    private const int VoiceMuteRequested = 0x1;
    private const int VoiceMutePromoted = 0x10000;

    private sealed record VoiceStart(AudioClip Clip, int PlaybackVariant, bool Muted);

    // A voice's delayed start (ops 0x2c0 and 0x2f5): the voice facade's +0x41c..+0x42c, in AGE 4.45D its per-voice
    // arrays +0x424..+0x454. The start disarms it but keeps it: its variant stays the voice's until op 0x2f6 stops the
    // voice or another request replaces it, and 4.45D's background cycle arms it again.
    private sealed class ScheduledVoice(AudioClip clip, int playbackVariant, uint delayMs)
    {
        public AudioClip Clip { get; } = clip;
        public int PlaybackVariant { get; } = playbackVariant;
        public uint DelayMs { get; } = delayMs;
        public bool Armed = true;
        public uint? StartMs;
    }

    // Kept in each voice channel's sound-buffer state by voice_play_indexed_asset.
    private readonly int[] _voicePlaybackVariants = new int[MaxVoices];
    // Each voice channel holds its last voice: voice_play_indexed_asset decodes each one there, and only a decode or op
    // 0x2f6 replaces it (op 0x1bc and the Voice route only stop it). The level is the one the voice was given: 0 when
    // muted.
    private readonly bool[] _voiceLoaded = new bool[MaxVoices];
    private readonly int[] _voiceChannelLevels = [FullLevel, FullLevel, FullLevel];

    /// <summary>The playback variant voice 0's channel last started with: 0 for a line (op 0xc4), 1 for a History replay
    /// (op 0x1bd). Its audible meaning is unproven; the port only keeps it.</summary>
    public int VoicePlaybackVariant
    {
        get { return _voicePlaybackVariants[0]; }
    }

    /// <summary>Op 0xc4 (voice 0, variant 0) and op 0x2f4 (any voice and variant): a line's voice. A pending mute request
    /// of the voice is promoted and the voice plays muted, at volume 0, so its completion keeps Auto's timing.</summary>
    public void PlayVoice(long voiceId, int playbackVariant = 0, int voice = 0)
    {
        int flags = _scene.VoiceMuteFlags[voice];
        if ((flags & VoiceMutePromoted) != 0) flags = 0;
        else if ((flags & VoiceMuteRequested) != 0) flags |= VoiceMutePromoted;
        _scene.VoiceMuteFlags[voice] = flags;
        StartVoice(voice, voiceId, playbackVariant, muted: (flags & VoiceMuteRequested) != 0);
        UpdateTimedWork();
    }

    /// <summary>Op 0x1bd: a voice replayed from History, on voice 0 with playback variant 1. It plays audibly and
    /// consumes a pending mute request.</summary>
    public void PlayHistoryVoice(long voiceId)
    {
        if ((_scene.VoiceMuteFlags[0] & (VoiceMuteRequested | VoiceMutePromoted)) != 0) _scene.VoiceMuteFlags[0] = 0;
        StartVoice(0, voiceId, 1, muted: false);
        UpdateTimedWork();
    }

    /// <summary>Op 0x1d5 (voice 0) and op 0x2f7: the voice's next play (op 0xc4 or 0x2f4) is at volume 0.</summary>
    public void MuteNextVoice(int voice = 0)
    {
        _scene.VoiceMuteFlags[voice] = VoiceMuteRequested;
    }

    /// <summary>Op 0x1bc's audio part: stop every voice, drop the voices fast-forward held back and clear the mute
    /// requests.</summary>
    public void ResetMessageVoiceState()
    {
        _timeline?.Event("voice-stop", new());
        for (int voice = 0; voice < _voices; voice++)
        {
            _scene.HeldVoices[voice] = null;
            _scene.VoiceMuteFlags[voice] = 0;
            StopChannel(VoiceChannel + voice);
        }
        UpdateTimedWork();
    }

    /// <summary>Op 0x2f6: stop a voice and forget it (voice_stop_n@0x4a9d00): its channel releases the sound, and its
    /// delayed start, held voice and mute request go.</summary>
    public void StopVoice(int voice)
    {
        _timeline?.Event("voice-stop", new() { ["voice"] = voice });
        _scene.ScheduledVoices[voice] = null;
        _scene.HeldVoices[voice] = null;
        _scene.VoiceMuteFlags[voice] = 0;
        _voiceLoaded[voice] = false;
        _voicePlaybackVariants[voice] = 0;
        UnloadChannel(VoiceChannel + voice);
        UpdateTimedWork();
    }

    /// <summary>Op 0x2f8: a voice's pan in hundredths of a decibel, -10000 (left) to 10000 (right)
    /// (sound_set_channel_level@0x4a4e60, which stores it and calls IDirectSoundBuffer::SetPan on the voice's
    /// buffer). The port's channels have no pan, so it is only recorded: every site of 天秤のLa DEA。 passes 0, the
    /// centre, and the next play sets the pan of ctx+0x4e8c, which nothing writes.</summary>
    public void SetVoicePan(int voice, long pan)
    {
        _timeline?.Event("voice-pan", new() { ["voice"] = voice, ["pan"] = Math.Clamp(pan, -10_000, 10_000) });
    }

    /// <summary>Op 0x2c0 (voice 0) and op 0x2f5: replace the voice's delayed start. The delay counts from the first
    /// tick outside a statement block (voice_tick_delayed_playback@0x4884d0), and the start arms the duck release as
    /// op 0x1bf does.</summary>
    public void ScheduleVoicePlayback(long voiceId, int playbackVariant, long delayMs, int voice = 0)
    {
        AudioClip? clip = _assets.Voice(voiceId);
        _timeline?.Event("voice-scheduled", new()
        {
            ["id"] = voiceId, ["file"] = clip?.Name, ["playback_variant"] = playbackVariant,
            ["delay_ms"] = unchecked((uint)delayMs), ["voice"] = voice,
        });
        _scene.ScheduledVoices[voice] =
            clip == null ? null : new ScheduledVoice(clip, playbackVariant, unchecked((uint)delayMs));
        UpdateTimedWork();
    }

    /// <summary>Fast-forward ended: play each voice it held back (<see cref="IFastForwardListener"/>; op 0xfa plays
    /// them in voice order).</summary>
    void IFastForwardListener.FastForwardChannelSet(FastForwardChannel channel, bool value, bool active, bool changed)
    {
        if (!changed || active) return;
        for (int voice = 0; voice < _voices; voice++)
        {
            if (_scene.HeldVoices[voice] is not { } held) continue;
            _scene.HeldVoices[voice] = null;
            DispatchVoice(voice, held.Clip, held.PlaybackVariant, held.Muted, duck: true);
        }
        UpdateTimedWork();
    }

    private void StartVoice(int voice, long voiceId, int playbackVariant, bool muted)
    {
        AudioClip? clip = _assets.Voice(voiceId);
        _timeline?.Event("voice", new() { ["id"] = voiceId, ["file"] = clip?.Name,
            ["playback_variant"] = playbackVariant, ["muted"] = muted, ["voice"] = voice });
        if (clip == null) return;
        var start = new VoiceStart(clip, playbackVariant, muted);
        // A voice requested while fast-forward runs is held and played by the notice of its end; otherwise it plays
        // now.
        bool first = false;
        if (_skip.WhileFastForward(() =>
            {
                first = _scene.HeldVoices[voice] == null;
                _scene.HeldVoices[voice] = start;
            }))
        {
            if (first) StopChannel(VoiceChannel + voice);
            return;
        }
        DispatchVoice(voice, clip, playbackVariant, muted, duck: true);
    }

    private void DispatchVoice(int voice, AudioClip clip, int playbackVariant, bool muted, bool duck)
    {
        // voice_bgm_duck_begin reads sound:MusicFadeOnVoicePlaying and its volume (percent) at each voice start; the
        // 設定 dialog's ｻｳﾝﾄﾞ page sets both. It ducks whenever op 0x1cf bit 0 is clear, muted voices included; a
        // suppressed start leaves an earlier duck in place. AGE 4.45D's background voice starts without it.
        if (duck)
        {
            (bool duckOnVoice, int duckPercent) = DuckSettings();
            if (duckOnVoice && (_scene.DuckControl & 1) == 0) BeginDuck(duckPercent);
        }
        _voicePlaybackVariants[voice] = playbackVariant;
        _voiceLoaded[voice] = true;
        _voiceChannelLevels[voice] = muted ? 0 : FullLevel;
        _device.Load(VoiceChannel + voice, AudioBus.Voice, clip);
        PlayChannel(VoiceChannel + voice, loop: false, NativeAudioVolume.ToDecibels(_voiceChannelLevels[voice]));
    }

    // Ops 0xb5/0xba on a voice channel: the voice the channel holds plays again from its start at its level, so a muted
    // one stays muted. No duck begins: voice_bgm_duck_begin belongs to a voice op. The start mode goes to the buffer's
    // field voice_play_indexed_asset writes the playback variant to, so it becomes the variant; the port plays it once,
    // as it plays every voice.
    private void StartVoiceChannel(int voice, int startMode)
    {
        if (!_voiceLoaded[voice]) return;
        _timeline?.Event("sfx-start", new() { ["channel"] = VoiceChannel + voice, ["start_mode"] = startMode });
        _voicePlaybackVariants[voice] = startMode;
        PlayChannel(VoiceChannel + voice, loop: false, NativeAudioVolume.ToDecibels(_voiceChannelLevels[voice]));
    }

    /// <summary>sound:MusicFadeOnVoicePlaying (registered default 1) and sound:MusicFadeOnVoicePlayingVolume (50).</summary>
    private (bool Enabled, int Percent) DuckSettings()
    {
        if (_settings == null) return (true, 50);
        try
        {
            return (_settings.LoadInt("sound", "MusicFadeOnVoicePlaying", 1) != 0,
                    _settings.LoadInt("sound", "MusicFadeOnVoicePlayingVolume", 50));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return (true, 50);
        }
    }

    // A background voice: a delayed start whose variant has bits 0 and 1 set, under BackgroundVoices.
    private bool IsBackgroundVoice(int voice)
        => BackgroundVoices && _scene.ScheduledVoices[voice] is { } request && IsBackgroundVariant(request.PlaybackVariant);

    private static bool IsBackgroundVariant(int playbackVariant) => (playbackVariant & 3) == 3;

    private bool HasScheduledVoice()
    {
        for (int voice = 0; voice < _voices; voice++)
            if (_scene.ScheduledVoices[voice] is { } request && (request.Armed || IsBackgroundVoice(voice))) return true;
        return false;
    }

    // The facade's tick (voice_tick_delayed_playback@0x4884d0; AGE 4.45D's 0x4a9dc0 for each voice in turn). A start
    // takes the request's first tick, and the voice starts once the unsigned time since reaches the delay, at once for
    // no delay. The start clears the voice's mute request, promoted or not, so the voice is heard. After a start that is
    // not a background voice's, the engine tick (4.24J 0x411a72..0x411aae, 4.45D 0x4125ea..0x412626) begins the duck,
    // marks a voice pending for Auto while the Voice route is on, and arms the duck release as op 0x1bf does.
    private void ServiceDelayedVoices(long now)
    {
        uint now32 = unchecked((uint)now);
        bool started = false;
        for (int voice = 0; voice < _voices; voice++)
        {
            if (_scene.ScheduledVoices[voice] is not { } request) continue;
            bool background = IsBackgroundVoice(voice);
            if (background && !CycleBackgroundVoice(voice, request)) continue;
            if (!request.Armed) continue;
            request.StartMs ??= now32;
            if (unchecked(now32 - request.StartMs.Value) < request.DelayMs) continue;
            request.Armed = false;
            _scene.VoiceMuteFlags[voice] = 0;
            // 4.45D starts its delayed voices with the variant's bit 0 (so a background voice loops); the builds before
            // pass the variant as it was given.
            int playbackVariant = BackgroundVoices ? request.PlaybackVariant & 1 : request.PlaybackVariant;
            _timeline?.Event("voice-scheduled-start", new()
            {
                ["file"] = request.Clip.Name, ["playback_variant"] = playbackVariant, ["voice"] = voice,
            });
            DispatchVoice(voice, request.Clip, playbackVariant, muted: false, duck: !background);
            if (!background) started = true;
        }
        if (!started) return;
        if (VoiceRouteEnabled) _skip.NoteVoiceStarted();
        ArmDuckRelease();
    }

    // AGE 4.45D's background cycle for one voice (0x4a9de7..0x4a9eb4). While another voice plays, a playing background
    // voice pauses and arms its start again, and one that is not playing disarms. When the others are silent, one
    // that is not playing arms again. False when the tick goes on to the next voice.
    private bool CycleBackgroundVoice(int voice, ScheduledVoice request)
    {
        bool othersPlay = false;
        for (int other = 1; other < MaxVoices; other++)
            if (_playing[VoiceChannel + (voice + other) % MaxVoices]) othersPlay = true;
        if (_playing[VoiceChannel + voice])
        {
            if (!othersPlay) return true;
            request.Armed = true;
            request.StartMs = null;
            StopChannel(VoiceChannel + voice);
            return true;
        }
        if (othersPlay)
        {
            request.Armed = false;
            return false;
        }
        if (!request.Armed)
        {
            request.Armed = true;
            request.StartMs = null;
        }
        return true;
    }
}
