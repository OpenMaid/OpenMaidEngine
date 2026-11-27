using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Persistence;
using Ome.Engine.Profiles;
using Ome.Engine.Text;

namespace Ome.Engine.Vm;

/// <summary>Every rule an engine build gives the natives (docs/superpowers/specs/2026-11-12-engine-layering-design.md
/// §7.3). The rule types live beside the natives that read them.</summary>
public sealed record EngineRules(TextRules Text, GfxRules Gfx, AudioRules Audio, SettingsRules Settings,
                                 SaveRules Saves, ShellRules Shell, InputRules Input);

/// <summary>
/// One build of native AGE whose behaviour the port reproduces (spec §7.1). <see cref="Id"/> is the version the build
/// names itself by, and <see cref="AbiId"/> the opcode table it runs. Both are read only where the engine is put
/// together: code below that point takes <see cref="Rules"/> (spec §7.6).
/// </summary>
public sealed record EngineBuild(string Id, string AbiId, EngineRules Rules);

/// <summary>
/// The engine builds the port reproduces, each declared as its parent plus changes (spec §7.4). A change names the
/// behaviour, never the build, and its comment points to the evidence. Adding a game: spec §7.8.
/// </summary>
public static class EngineBuilds
{
    /// <summary>AGE 4.24J, Himegari's (its version resource and the <c>424J</c> stamp in every save it writes;
    /// docs/engine-re.md, "Comparing engine builds").</summary>
    public static EngineBuild Age424J { get; } = new(
        Id: "4.24J",
        AbiId: "SYS4422",
        Rules: new EngineRules(
            Text: new TextRules(
                GlyphCoverage: GlyphCoverageAlpha.Sixteenths,
                Ruby: RubyRule.FixedOffsets,
                FuriganaArmsGlyphTimer: false,
                HistoryLookupTestsSign: false,
                LineEnd: LineEndRule.FixedThree,
                ExtendedGlyphFace: null,
                AspectCalibration: AspectCalibration.BlackBoxOverCellIncrement,
                PublisherGrowsGlyphCells: false,
                PublisherMovesAliasedEffectGlyphs: false),
            // gfx_object_blit_d3d9@0x4774c0 never sets D3DRS_ALPHATESTENABLE, so the sprite's alpha test holds.
            Gfx: new GfxRules(UnboundObjectReadsAbsent: false, SpriteAlphaTest: true),
            Audio: new AudioRules(PlayComparesStartMode: false),
            Settings: SettingsRules.LoaderKeys,
            Saves: new SaveRules(versionStamp: "424J", numberedGfxRecordSize: NativeGfxSaveLayout.BaseRecordSize),
            Shell: new ShellRules(AutoMessagePage: false),
            Input: InputRules.AxesOnly));

    /// <summary>AGE 4.15D, 戦女神ZERO's (its version resource; docs/engine-re.md, "Comparing engine builds"). Its
    /// changes are the real rows of the revision diff's triage (docs/engine-re.md, "Himegari against 戦女神ZERO,
    /// triaged") and its settings ("戦女神ZERO's settings and 設定").</summary>
    public static EngineBuild Age415D { get; } = Age424J with
    {
        Id = "4.15D",
        AbiId = "SYS4415",
        Rules = Age424J.Rules with
        {
            // engine_settings_save@0x45fbb0 writes 4.24J's keys but these seven, and none of their names is a string
            // in the image: no virtual fullscreen, no ForceScreen, and no wheel-down advance (adv_input_service_poll
            // @0x40ae90 has no such branch). A SYS4REG.INI native 4.15D wrote has exactly the rest.
            Settings = Age424J.Rules.Settings
                .Without("display", "ForceScreen").Without("display", "VirtualFullScreen")
                .Without("display", "AspectMode").Without("display", "FullScreenWidth")
                .Without("display", "FullScreenHeight").Without("display", "LimitAero")
                .Without("message", "AdvanceMesOnWheel"),
            // The stamp follows the version, and a numbered save has 4.24J's layout, its 0x2d4-byte graphics records
            // and its six counts: native 4.15D's saves of SC0000's fourth page read and re-encode byte for byte, and
            // the port's equal them (docs/sys4-format-notes.md; 2026-11-25). The in-memory frame record is 8 bytes
            // shorter, but the saved frame is not.
            Saves = new SaveRules(versionStamp: "415D", numberedGfxRecordSize: NativeGfxSaveLayout.BaseRecordSize),
        },
    };

    /// <summary>AGE 4.24N, 戦女神VERITA's (its version resource; docs/engine-re.md, "Comparing engine builds"). Its
    /// changes are the real rows of the revision diff's triage (docs/engine-re.md, "Himegari against Verita,
    /// triaged").</summary>
    public static EngineBuild Age424N { get; } = Age424J with
    {
        Id = "4.24N",
        AbiId = "SYS4424",
        Rules = Age424J.Rules with
        {
            // Its op 0x196 handler (0x41fe20) arms the glyph timer as op 0x6e does, as 4.33G's does.
            Text = Age424J.Rules.Text with { FuriganaArmsGlyphTimer = true },
            // bgm_fade_arm@0x465220 clears the retained track for a target of zero, as 4.33G's does; its bgm_play_track
            // compares the track alone, as 4.24J's does.
            Audio = Age424J.Rules.Audio with { ZeroFadeClearsTrack = true },
            // Start-up (0x47c880) guards slot 38 from the release worker as well as 36 and 37, as 4.33G's does.
            Gfx = Age424J.Rules.Gfx with { GuardsFrameSurface = true },
            // The SYS4REG.INI loader 0x46c1a0 binds display:VirtualFullScreenType after LimitAero, registered as 0;
            // it binds no AntiFontLevel.
            Settings = Age424J.Rules.Settings
                .BindingAfter("LimitAero", new Sys4RegIniKey("display", "VirtualFullScreenType", "0")),
            // The stamp follows the version, as 4.24J's and 4.33G's do, and the record size is 4.24J's: both read
            // from a native save on 2026-11-25 (sys4-format-notes.md, "Observed profile identities").
            Saves = new SaveRules(versionStamp: "424N", numberedGfxRecordSize: NativeGfxSaveLayout.BaseRecordSize),
        },
    };

    /// <summary>AGE 4.33G, Kamidori's clean install (docs/engine-re.md, "Kamidori engine builds"). The profile runs this
    /// build whichever executable the player has (spec §7.5).</summary>
    public static EngineBuild Age433G { get; } = Age424J with
    {
        Id = "4.33G",
        AbiId = "SYS4433",
        Rules = Age424J.Rules with
        {
            Text = Age424J.Rules.Text with
            {
                // text_blit_glyph_bitmap_sys4433@0x45cff0 divides coverage by 17.
                GlyphCoverage = GlyphCoverageAlpha.Seventeenths,
                // adv_text_draw_ruby_over_row_sys4433@0x4590d0 reads message:RubyShiftMode and set:DrawMode.
                Ruby = RubyRule.ShiftModes,
                // Its op 0x196 handler re-arms the glyph timer as op 0x6e does.
                FuriganaArmsGlyphTimer = true,
                // text_history_find_typed_value@0x4501b0 and text_history_find_pair@0x450270 return for a negative
                // record index.
                HistoryLookupTestsSign = true,
                // adv_text_publish_next_glyph@0x4509f0 and adv_text_publish_layout@0x4503b0 add the effect's vertical
                // offset to the bottom of the rectangle they bind.
                PublisherGrowsGlyphCells = true,
                // ...and in effect mode 3 with message:UseAntiFont 0 (text_set_use_anti_font@0x413de0) move the object
                // back by the effect offsets.
                PublisherMovesAliasedEffectGlyphs = true,
                // text_set_use_anti_font and display_toggle_screen_mode_and_menu_bar@0x4130f0 (while
                // display:VirtualFullScreenType is 2) draw windows 1 to 9 again in each window's style
                // (adv_text_redraw_live_records@0x4596f0).
                LiveTextRedraw = LiveTextRedraw.WindowStyle,
                LiveTextRedrawFirstLayout = 1,
            },
            // Worker 0x486120 tests the record's bound bit before it returns the slot.
            Gfx = Age424J.Rules.Gfx with { UnboundObjectReadsAbsent = true, GuardsFrameSurface = true },
            // bgm_play_track@0x469e90 returns early only for the retained track with the retained start mode, and
            // bgm_fade_arm@0x469f80 clears the retained track for a target of zero.
            Audio = Age424J.Rules.Audio with { PlayComparesStartMode = true, ZeroFadeClearsTrack = true },
            // The SYS4REG.INI loader 0x472530 binds two more keys, and a native exit writes both as 0.
            Settings = Age424J.Rules.Settings
                .BindingAfter("LimitAero", new Sys4RegIniKey("display", "VirtualFullScreenType", "0"))
                .BindingAfter("UseAntiFont", new Sys4RegIniKey("message", "AntiFontLevel", "0")),
            // Numbered saves append a rotation cache to every graphics record. context_state_serialize@0x40d1e0 and
            // save_data_deserialize_and_begin_restore@0x40faa0 copy the float globals and the float pointers when the
            // save's minor version is above 0x13; 4.24J's have no such branch.
            Saves = new SaveRules(versionStamp: "433G", numberedGfxRecordSize: NativeGfxSaveLayout.RotationCacheRecordSize,
                                  floatSectionsFromMinor: 20),
        },
    };

    /// <summary>AGE 4.38D, 創刻のアテリアル's (its version resource; docs/engine-re.md, "Comparing engine builds"). Its
    /// changes are the real rows of the revision diff's triage (docs/engine-re.md, "Kamidori against Arterial,
    /// triaged"); the ones the port does not have yet are in the game's item in docs/backlog.md.</summary>
    public static EngineBuild Age438D { get; } = Age433G with
    {
        Id = "4.38D",
        AbiId = "SYS4438",
        Rules = Age433G.Rules with
        {
            Text = Age433G.Rules.Text with
            {
                // adv_text_append_horizontal_glyph_records@0x466000 asks text_is_justify_character@0x457950, whose
                // list is the catalog trailer's JUSTIFYCHAR values.
                LineEnd = LineEndRule.CatalogList,
                // text_draw_glyph_string_plain@0x4692f0 and text_draw_glyph_string_effect@0x46b1d0 select the font
                // text_rebuild_primary_font_resources@0x456800 creates with this face for codes 0xF040 to 0xF9FC.
                ExtendedGlyphFace = "AGE Extend",
                // text_calibrate_mode1_glyph_font@0x454ab0 divides the two black box widths.
                AspectCalibration = AspectCalibration.BlackBoxRatio,
                // op_0x2c7_byte_substring@0x42fe10 is SBSubstr; 4.33G's op 0x2c7 (@0x42d220) is std::string's substr.
                SubstringKeepsDoubleByteCharacters = true,
                // Its records carry their style, and adv_text_redraw_live_records@0x463630 draws each in its own, from
                // window 0, clearing a window with no records too.
                LiveTextRedraw = LiveTextRedraw.RecordStyle,
                LiveTextRedrawFirstLayout = 0,
                LiveTextRedrawClearsEmptyLayouts = true,
            },
            // engine_settings_save@0x47d1d0 writes four more keys; a SYS4REG.INI native 4.38D wrote has them in
            // these places.
            Settings = Age433G.Rules.Settings
                .BindingAfter("VirtualFullScreenType", new Sys4RegIniKey("display", "MultiSample", "0"))
                .BindingAfter("AutoMessageTime1", new Sys4RegIniKey("message", "AutoMessagePitch0", "0"))
                .BindingAfter("AutoMessagePitch0", new Sys4RegIniKey("message", "AutoMessagePitch1", "0"))
                .BindingAfter("AutoMessagePitch1", new Sys4RegIniKey("message", "AutoMessageOption", "0")),
            // The stamp follows the version, as 4.24J's and 4.33G's do; no 4.38D save has been read. The record size
            // is 4.33G's, unproven: the profile's writes stay off until a native save is read.
            // The float sections are 4.33G's, read in 0x40d5a0 and 0x40fe90.
            Saves = new SaveRules(versionStamp: "438D", numberedGfxRecordSize: NativeGfxSaveLayout.RotationCacheRecordSize,
                                  floatSectionsFromMinor: 20),
            // Its AGERC menu 110 and 設定 dialog, read from the running game.
            Shell = new ShellRules(AutoMessagePage: true),
            // gfx_draw_surface_through_sprite@0x48c6d0 sets D3DRS_ALPHATESTENABLE to 0 after the sprite's Begin;
            // 4.33G's (0x47dac0) does not.
            Gfx = Age433G.Rules.Gfx with { SpriteAlphaTest = false },
        },
    };

    /// <summary>AGE 4.43B, 魔導巧殻's (its version resource; docs/engine-re.md, "Comparing engine builds"). Its changes
    /// are the real rows of the revision diff's triage (docs/engine-re.md, "Arterial against 魔導巧殻, triaged"); each
    /// joins this definition as it is ported, and the ones the port does not have yet are in the game's item in
    /// docs/backlog.md. The opcodes it adds need no rule: no older build's script runs them.</summary>
    public static EngineBuild Age443B { get; } = Age438D with
    {
        Id = "4.43B",
        AbiId = "SYS4443",
        Rules = Age438D.Rules with
        {
            Text = Age438D.Rules.Text with
            {
                // adv_text_publish_next_glyph@0x459440, adv_text_publish_layout@0x4583e0,
                // text_redraw_retained_group@0x463310 and text_history_render_records@0x4646d0 raise a ruby character
                // by op 0x260's top padding in RubyShiftMode 2, the catalog trailer's RUBYSHIFTMODE.
                Ruby = RubyRule.ShiftModesRaisedByTopPadding,
                // The last three add the effect's offset under a cell once in mode 3 (0x45867a, the redraw and
                // History's 0x465a94), where 4.38D's add it twice.
                LayoutPublishersGrowOnce = true,
            },
            // gfx_set_view_look_at@0x48a050 passes the projection, the view and the world to
            // display_scene_effects_set_transforms@0x450a80 when the display exists (manager +0xc600), as the new op
            // 0x33d does; 4.38D's (0x486440) leaves that to the next device setup.
            Gfx = Age438D.Rules.Gfx with { CameraHandsEffectsTheView = true },
            // engine_settings_load@0x480c50 binds one key more than 4.38D's, message:MessageFade after MessageSpeed
            // (0x481308), which engine_settings_register_defaults@0x481ef0 registers with 0 (0x4823da) and
            // engine_initialize (0x4174e4) gives the text manager.
            Settings = Age438D.Rules.Settings
                .BindingAfter("MessageSpeed", new Sys4RegIniKey("message", "MessageFade", "0")),
            // The stamp follows the version, as the other builds' do; no 4.43B save has been read. The record size
            // and the float sections are 4.38D's, unproven: the profile's writes stay off until a native save is read.
            Saves = new SaveRules(versionStamp: "443B", numberedGfxRecordSize: NativeGfxSaveLayout.RotationCacheRecordSize,
                                  floatSectionsFromMinor: 20),
        },
    };

    /// <summary>AGE 4.45D, 天秤のLa DEA。's (its version resource; docs/engine-re.md, "Comparing engine builds"). Its
    /// changes are the real rows of the revision diff against 4.43B (docs/engine-re.md, "魔導巧殻 against 天秤のLa DEA。");
    /// each joins this definition as it is ported, and the ones the port does not have yet are in the game's item in
    /// docs/backlog.md. The opcodes it adds need no rule: no older build's script runs them.</summary>
    public static EngineBuild Age445D { get; } = Age443B with
    {
        Id = "4.45D",
        AbiId = "SYS4445",
        Rules = Age443B.Rules with
        {
            // text_draw_glyph_string_plain@0x46c4b0 (the join at 0x46ca70) and text_draw_glyph_string_effect@0x46e360
            // (0x46ede8) take the dash strip's rows from the glyph's black box, the effect drawer's widened by the
            // effect's vertical offset; 4.43B's take the cell's (docs/engine-re.md, "The dash join").
            //
            // adv_text_redraw_live_records@0x466a20 draws every record in the window's style, as 4.33G's does; 4.43B's
            // (0x466100) applies each record's own (docs/engine-re.md, "The settings redraw").
            Text = Age443B.Rules.Text with
            {
                DashStrip = DashStripRows.BlackBox,
                LiveTextRedraw = LiveTextRedraw.WindowStyle,
            },
            // 4.43B's record size and float sections with its own stamp: native's 4.45D save decodes so, and the
            // port's saves load on native and native's on the port (docs/engine-re.md, "魔導巧殻 against 天秤のLa DEA。,
            // triaged", step 7).
            Saves = new SaveRules(versionStamp: "445D", numberedGfxRecordSize: NativeGfxSaveLayout.RotationCacheRecordSize,
                                  floatSectionsFromMinor: 20),
            // Three voices on channels 12 to 14, and the background voice of 4.45D's tick (docs/engine-re.md,
            // "魔導巧殻 against 天秤のLa DEA。, triaged": voice_play_indexed_asset@0x4a9b90 takes a voice index,
            // voice_any_playing@0x404c70, the facade's tick 0x4a9dc0).
            Audio = Age443B.Rules.Audio with { Voices = 3, BackgroundVoices = true },
            // The joystick's POV makes the directions (docs/engine-re.md, the same triage:
            // input_poll_joystick_action_bits@0x474450 and input_poll_joystick_action_edges@0x474660 read dwPOV).
            Input = Age443B.Rules.Input with { JoystickPovDirections = true },
            // 音声終了まで待つ leaves the voice group's 文字単位時間 enabled: AGERC's ｵｰﾄﾒｯｾｰｼﾞ page procedure
            // (0x6fea4d60 in the running game) handles only WM_INITDIALOG and WM_HSCROLL (docs/engine-re.md, the same
            // triage, the shell).
            Shell = Age443B.Rules.Shell with { VoiceEndDisablesVoicePitch = false },
        },
    };

    /// <summary>Every build, parents first.</summary>
    public static IReadOnlyList<EngineBuild> All { get; } = [Age424J, Age415D, Age424N, Age433G, Age438D, Age443B, Age445D];

    public static EngineBuild? Find(string id)
        => All.FirstOrDefault(build => string.Equals(build.Id, id, StringComparison.Ordinal));

    /// <summary>The build a profile names. Its manifest also names the ABI, for the tools that read manifests; the
    /// two must agree.</summary>
    public static EngineBuild ForProfile(GameProfileManifest profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        EngineBuild build = Find(profile.EngineBuildId)
            ?? throw new InvalidDataException(
                $"profile '{profile.Id}' names engine build '{profile.EngineBuildId}', which is not declared; "
                + $"builds: {string.Join(", ", All.Select(known => known.Id))}");
        if (!string.Equals(build.AbiId, profile.EngineAbiId, StringComparison.Ordinal))
            throw new InvalidDataException(
                $"profile '{profile.Id}' names ABI '{profile.EngineAbiId}', but its engine build {build.Id} runs "
                + $"'{build.AbiId}'");
        return build;
    }

    /// <summary>
    /// The build a VM runs over <paramref name="table"/>: <paramref name="build"/> when the composition named one (it
    /// must run that table's ABI), else, for tests and tools that run a bare table, the first build declared for the
    /// table's ABI, or <see cref="Age424J"/> for a table of no build's ABI (the opcode catalog, a test's own table).
    /// </summary>
    public static EngineBuild ForVm(EngineBuild? build, OpcodeTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        if (build == null)
            return All.FirstOrDefault(known => string.Equals(known.AbiId, table.AbiId, StringComparison.Ordinal))
                   ?? Age424J;
        if (!string.Equals(build.AbiId, table.AbiId, StringComparison.Ordinal))
            throw new ArgumentException(
                $"engine build {build.Id} runs ABI '{build.AbiId}', not the opcode table's '{table.AbiId}'",
                nameof(build));
        return build;
    }
}
