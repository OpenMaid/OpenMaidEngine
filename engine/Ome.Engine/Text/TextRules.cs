namespace Ome.Engine.Text;

/// <summary>
/// The text rules an engine build gives the text natives (docs/superpowers/specs/2026-11-12-engine-layering-design.md
/// §7.3). Each member is one behaviour, named for what it does; the builds that differ are declared in
/// <c>Ome.Engine.Vm/EngineBuilds.cs</c>.
/// </summary>
/// <param name="GlyphCoverage">How the glyph blit turns a gray-4 coverage value into alpha: in sixteenths
/// (text_blit_glyph_bitmap@0x458c80 in 4.24J) or seventeenths (text_blit_glyph_bitmap_sys4433@0x45cff0 in 4.33G);
/// <see cref="GlyphCoverageAlpha"/>.</param>
/// <param name="Ruby">Where furigana are placed, how they are spaced and how they publish
/// (<see cref="RubyRule"/>; docs/engine-re.md, "Furigana").</param>
/// <param name="FuriganaArmsGlyphTimer">Whether op 0x196 re-arms the glyph timer (<c>ctx+0x5f630</c>) as op 0x6e
/// does. 4.33G's handler does; 4.24J's op_0x196_handler@0x41f2a0 does not, so its ruby run keeps the previous run's
/// reveal phase (docs/engine-re.md, "The reveal clock").</param>
/// <param name="HistoryLookupTestsSign">Whether ops 0x1d3 and 0x1d4 refuse a negative record index as they refuse one
/// past the last record, finding nothing. 4.33G's text_history_find_typed_value@0x4501b0 and
/// text_history_find_pair@0x450270 do; 4.24J's @0x450840 and @0x4509f0 test only the upper end, and the negative index
/// fails a checked subscript, which ends the process (docs/engine-re.md, "Indices outside their tables").</param>
/// <param name="LineEnd">Which characters a horizontal ADV layout keeps on the line they overflow: a fixed three
/// (4.24J, 4.33G) or the catalog's <c>JUSTIFYCHAR</c> list (4.38D); <see cref="LineEndRule"/>.</param>
/// <param name="ExtendedGlyphFace">The face that draws a character whose Shift-JIS code is 0xF040 to 0xF9FC, the
/// user-defined area, whatever face the text is in (<see cref="ExtendedFaceGlyphRasterizer"/>). 4.38D's glyph
/// drawers select <c>AGE Extend</c>, a font the game ships in its catalog; null draws such a character in the
/// text's own face (4.24J, 4.33G).</param>
/// <param name="AspectCalibration">What aspect mode 1 divides a font's narrower black box by to set its width
/// (<see cref="Text.AspectCalibration"/>). 4.24J has no aspect mode and never reads it.</param>
/// <param name="PublisherGrowsGlyphCells">Whether the glyph publisher makes a glyph object's source rectangle taller
/// than the glyph's record when a text effect is on, by the effect's vertical offset as the text manager holds it at
/// that moment. 4.33G's and 4.38D's publishers do (adv_text_publish_next_glyph@0x4509f0 and @0x4541b0 once for a
/// record that is not ruby; adv_text_publish_layout@0x4503b0 and @0x453990 once, and once more in effect mode 3; op
/// 0x82's redraw as the layout publisher, to each record that is not ruby; History's renderer once more than the record
/// in effect mode 3). 4.24J's (@0x451220, @0x450c80) bind the record's rectangle as it is (docs/engine-re.md, "The
/// glyph publisher grows a cell").</param>
/// <param name="PublisherMovesAliasedEffectGlyphs">Whether the glyph publishers move a glyph object that is not ruby
/// left and up by the effect offsets in effect mode 3 while glyphs are not antialiased (<c>message:UseAntiFont</c> 0,
/// the text manager's +0x548 in these builds). 4.33G's and 4.38D's do, all four: the timed and layout publishers, op
/// 0x82's redraw and History's renderer; 4.24J's do not, so its text drawn with AntiFont off lands one pixel right and
/// down (docs/engine-re.md, "AntiFont off in 4.33G and 4.38D").</param>
/// <param name="SubstringKeepsDoubleByteCharacters">Whether op 0x2c7 keeps a double-byte character whole when the
/// byte interval cuts it. 4.38D's handler is SBSubstr (op_0x2c7_byte_substring@0x42fe10). 4.24J's (@0x42c2f0) and
/// 4.33G's (@0x42d220) are std::string's substr of the bytes, with no care for such a character, and they throw
/// out_of_range for a start past the end, which ends the program (docs/engine-re.md, "The substring").</param>
/// <param name="LayoutPublishersGrowOnce">Whether the layout publisher (op 0x20a), op 0x82's redraw and History's
/// renderer (op 0x1d1) add the effect's vertical offset under a cell once in effect mode 3, as the timed publisher does
/// in every effect mode. AGE 4.43B's do (adv_text_publish_layout@0x4583e0, text_redraw_retained_group@0x463310,
/// text_history_render_records@0x4646d0, each one add); 4.33G's and 4.38D's add it a second time in mode 3, so their
/// History rows are one row taller than the record there (docs/engine-re.md, "The glyph publisher grows a cell" and
/// "Arterial against 魔導巧殻, triaged").</param>
/// <param name="DashStrip">Which rows the join of the dashes 0xF042 and 0xF043 copies, in a build that joins them
/// (<see cref="ExtendedGlyphFace"/>): the cell's (4.38D, 4.43B) or the glyph's black box (4.45D's two drawers;
/// <see cref="DashStripRows"/>, docs/engine-re.md, "The dash join").</param>
/// <param name="LiveTextRedraw">What a change of <c>message:UseAntiFont</c> does to the text already in the windows, and
/// a screen-mode switch while <c>display:VirtualFullScreenType</c> is 2: nothing (4.15D, 4.24J, 4.24N), or each
/// window's live records drawn again, in its style (4.33G, 4.45D) or in each record's own (4.38D, 4.43B;
/// <see cref="Text.LiveTextRedraw"/>, docs/engine-re.md, "The settings redraw").</param>
/// <param name="LiveTextRedrawFirstLayout">The first window that redraw draws: 1 in 4.33G (its loop at 0x4596f0 starts
/// there), 0 after it.</param>
/// <param name="LiveTextRedrawClearsEmptyLayouts">Whether that redraw clears a window that has no live records too:
/// 4.38D's loop (0x463630) and 4.45D's (0x466a20) clear each defined window's surface before they look at its records;
/// 4.33G's (0x4596f0) skips a window without any.</param>
public sealed record TextRules(GlyphCoverageAlpha GlyphCoverage, RubyRule Ruby, bool FuriganaArmsGlyphTimer,
                               bool HistoryLookupTestsSign, LineEndRule LineEnd, string? ExtendedGlyphFace,
                               AspectCalibration AspectCalibration, bool PublisherGrowsGlyphCells,
                               bool PublisherMovesAliasedEffectGlyphs = false,
                               bool SubstringKeepsDoubleByteCharacters = false,
                               bool LayoutPublishersGrowOnce = false,
                               DashStripRows DashStrip = DashStripRows.Cell,
                               LiveTextRedraw LiveTextRedraw = LiveTextRedraw.None,
                               int LiveTextRedrawFirstLayout = 0,
                               bool LiveTextRedrawClearsEmptyLayouts = false);

/// <summary>
/// What a build draws again when <c>message:UseAntiFont</c> changes (text_set_use_anti_font) and when the screen mode
/// switches while <c>display:VirtualFullScreenType</c> is 2 (display_toggle_screen_mode_and_menu_bar): each window's
/// live records, those drawn there since its last op 0x71, at their rectangles into its cleared surface, in the new
/// setting (docs/engine-re.md, "The settings redraw").
/// </summary>
public enum LiveTextRedraw
{
    /// <summary>Nothing is drawn again: the text keeps the glyphs it was drawn with (4.24J's
    /// text_set_antialias_enabled@0x4137e0; 4.15D's @0x411ff0 and 4.24N's @0x414100 likewise).</summary>
    None,

    /// <summary>Every record in the window's style, which is the last record's: the appender
    /// (adv_text_append_segment_record) copies a record's style into the window (its <c>+0xa8</c> to <c>+0xc4</c>,
    /// a ruby record's size and weight into <c>+0xc8</c> and <c>+0xcc</c>). 4.33G's records carry no style
    /// (<c>0x4596f0</c>); 4.45D's do, and its redraw (<c>0x466a20</c>) does not read it.</summary>
    WindowStyle,

    /// <summary>Each record in its own style, on top of the window's: face, size, colors, weight, render mode and
    /// effect offsets from the record's <c>+0x4c</c> to <c>+0x68</c> (4.38D <c>0x463630</c>, 4.43B
    /// <c>0x466100</c>).</summary>
    RecordStyle,
}
