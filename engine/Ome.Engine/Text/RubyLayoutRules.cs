using Ome.Engine.Sys4;

namespace Ome.Engine.Text;

/// <summary>
/// How an engine build chooses its furigana rules (op 0x196; docs/engine-re.md, "Furigana"): the algorithm differs
/// between builds, so each is one implementation and the build names it (docs/superpowers/specs/2026-11-12-engine-layering-design.md
/// §7.3, form 3).
/// </summary>
public abstract class RubyRule
{
    private RubyRule() { }

    /// <summary>The same offsets whatever the settings: 4.24J's adv_text_draw_ruby_over_row@0x456b40 and History's
    /// text_history_render_records@0x4526c0.</summary>
    public static RubyRule FixedOffsets { get; } = new Fixed();

    /// <summary>Offsets chosen by <c>message:RubyShiftMode</c> and <c>set:DrawMode</c>: 4.33G's
    /// adv_text_draw_ruby_over_row_sys4433@0x4590d0 and its History renderer @0x456490.</summary>
    public static RubyRule ShiftModes { get; } = new ShiftMode();

    /// <summary><see cref="ShiftModes"/>, and in RubyShiftMode 2 the publishers raise a ruby character by op 0x260's top
    /// padding: AGE 4.43B's adv_text_publish_next_glyph@0x459440, adv_text_publish_layout@0x4583e0,
    /// text_redraw_retained_group@0x463310 and text_history_render_records@0x4646d0. Its drawer,
    /// adv_text_draw_ruby_over_row@0x462d70, places the ruby's cells as 4.33G's does and draws their ink that much
    /// higher (0x4630c1; <see cref="RubyLayoutRules.InkRaise"/>).</summary>
    public static RubyRule ShiftModesRaisedByTopPadding { get; } = new ShiftMode(raisesByTopPadding: true);

    /// <summary>The rules for a RubyShiftMode and a DrawMode.</summary>
    public abstract RubyLayoutRules For(int rubyShiftMode, int drawMode);

    /// <summary>The rules for SYS4INI's settings (DRAWMODE, RUBYSHIFTMODE). <paramref name="rubyShiftMode"/> is
    /// message:RubyShiftMode as loaded, when a settings file is in use
    /// (<see cref="Persistence.Sys4RegIniStore.RubyShiftMode"/>).</summary>
    public RubyLayoutRules For(Sys4StartupSettings settings, int? rubyShiftMode = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return For(rubyShiftMode ?? settings.RubyShiftMode, settings.DrawMode);
    }

    private sealed class Fixed : RubyRule
    {
        public override RubyLayoutRules For(int rubyShiftMode, int drawMode) => RubyLayoutRules.FixedOffsets;
    }

    private sealed class ShiftMode(bool raisesByTopPadding = false) : RubyRule
    {
        public override RubyLayoutRules For(int rubyShiftMode, int drawMode)
            => RubyLayoutRules.ShiftModes(rubyShiftMode, drawMode) with
            {
                PublisherRaisesByTopPadding = raisesByTopPadding && rubyShiftMode == 2,
            };
    }
}

/// <summary>
/// The furigana rules the text natives apply (op 0x196; docs/engine-re.md, "Furigana"). A build picks them through its
/// <see cref="RubyRule"/>.
/// </summary>
public sealed record RubyLayoutRules
{
    /// <summary><see cref="RubyRule.FixedOffsets"/>'s rules (4.24J).</summary>
    public static RubyLayoutRules FixedOffsets { get; } = new()
    {
        HalfWidthAwareSpacing = false,
        LiveTopOffset = 1,
        HistoryTopOffset = 1,
        RaiseByEffectInMode3 = true,
        ChainEveryRubyRecord = false,
    };

    /// <summary>
    /// <see cref="RubyRule.ShiftModes"/>'s rules (4.33G) for its <c>message:RubyShiftMode</c> and <c>set:DrawMode</c>:
    /// the live ruby starts one pixel lower only when DrawMode is not 1 and RubyShiftMode is 0; History never adds the
    /// pixel; a mode-3 effect raises the ruby by its y offset only when RubyShiftMode is 0. (With RubyShiftMode 0 and
    /// DrawMode 1, History's sprite publication also samples its source one pixel lower, which the port does not
    /// model: no shipped game on this build runs that way.)
    /// </summary>
    public static RubyLayoutRules ShiftModes(int rubyShiftMode, int drawMode) => new()
    {
        HalfWidthAwareSpacing = true,
        LiveTopOffset = drawMode != 1 && rubyShiftMode == 0 ? 1 : 0,
        HistoryTopOffset = 0,
        RaiseByEffectInMode3 = rubyShiftMode == 0,
        ChainEveryRubyRecord = true,
    };

    /// <summary>The gap divides by <c>(bytes + 3) >> 1</c> instead of <c>bytes / 2 + 1</c>, a single-byte ruby
    /// character is followed by half the gap, and a control character draws as a space (4.33G).</summary>
    public bool HalfWidthAwareSpacing { get; init; }

    /// <summary>Pixels added to <c>baseTop + lfHeight</c> for the live ruby.</summary>
    public int LiveTopOffset { get; init; }

    /// <summary>Pixels added to <c>rowTop + lfHeight</c> for History's ruby.</summary>
    public int HistoryTopOffset { get; init; }

    /// <summary>Whether a mode-3 effect raises the ruby by its y offset.</summary>
    public bool RaiseByEffectInMode3 { get; init; }

    /// <summary>Whether the publishers raise a ruby character's cell and object by op 0x260's top padding (AGE 4.43B in
    /// RubyShiftMode 2; <see cref="RubyRule.ShiftModesRaisedByTopPadding"/>). The cell keeps its height: the object
    /// shows the rows above it and loses as many at its bottom.</summary>
    public bool PublisherRaisesByTopPadding { get; init; }

    /// <summary>Pixels the drawer draws the ruby's ink above its cells, which the records keep. AGE 4.43B's
    /// adv_text_draw_ruby_over_row@0x462d70 computes the cell, then draws at its top less op 0x260's top padding in
    /// RubyShiftMode 2 (0x4630bc): the text manager sets it to that padding where
    /// <see cref="PublisherRaisesByTopPadding"/> holds, and the publisher's raise shows the rows the ink moved
    /// into.</summary>
    public int InkRaise { get; init; }

    /// <summary>Every ruby record gets chain flag 1 and the pending record resets to 0, so a run's whole ruby
    /// publishes in one tick and the next run starts its own (4.33G). Otherwise (4.24J) the record chains after each
    /// ruby character.</summary>
    public bool ChainEveryRubyRecord { get; init; }
}
