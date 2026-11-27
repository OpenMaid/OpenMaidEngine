using Ome.Engine.Model;
using Ome.Engine.Sys4;

namespace Ome.Engine.Text;

/// <summary>
/// How AGE 4.38D joins consecutive dashes (docs/engine-re.md, "Kamidori against Arterial, triaged"). After its glyph
/// drawers (text_draw_glyph_string_plain@0x4692f0, text_draw_glyph_string_effect@0x46b1d0) draw a character whose
/// Shift-JIS code is 0xF042 or 0xF043, they copy a strip of the text surface onto the same surface with
/// gfx_copy_surface_rect@0x48d2f0, op 0x207's worker. With <c>x</c> the glyph's draw position, <c>y</c> the string's
/// top, <c>bbw</c> the glyph's black box width, <c>(ex, ey)</c> the style's effect offset, <c>size</c> the primary
/// font size and <c>next</c> the pen after the glyph's advance:
/// <list type="bullet">
/// <item>the strip is <c>[x + ex, x + bbw - 2)</c> wide and <c>[y, y + size + 2 * ey)</c> tall: the dash's ink
/// without its last two columns;</item>
/// <item>both codes copy it so that it ends at <c>x + 1</c>, which draws the dash again to the left, over the gap
/// before it;</item>
/// <item>0xF042 then copies it so that it ends at <c>next + ex</c>, which carries the dash to where the next glyph
/// starts.</item>
/// </list>
/// The worker clips the destination rectangle to the surface and moves the source by as much, then the source
/// likewise, and copies nothing when what is left is empty. It changes the caller's rectangles, and the second copy
/// starts from the first copy's clipped source, as here. A build that draws the user-defined area with its own face
/// (<see cref="TextRules.ExtendedGlyphFace"/>) joins; the others have no such code. Which rows the strip takes is the
/// build's <see cref="TextRules.DashStrip"/>: 4.38D's and 4.43B's are the cell's, as above; 4.45D's are the glyph's
/// black box (<see cref="DashStripRows.BlackBox"/>).
/// </summary>
public static class ExtendedDashJoin
{
    /// <summary>The dash that joins the glyph before it and the glyph after it.</summary>
    public const ushort BothSides = 0xf042;

    /// <summary>The dash that joins the glyph before it.</summary>
    public const ushort LeftOnly = 0xf043;

    public static bool Applies(ushort? code) => code is BothSides or LeftOnly;

    private struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    /// <summary>Join <paramref name="request"/>'s glyph, just drawn into <paramref name="surface"/> at
    /// (<paramref name="drawX"/>, <paramref name="top"/>) in <paramref name="style"/> with its baseline
    /// <paramref name="topToBaseline"/> below <paramref name="top"/>, when it is one of the two dashes; the strip's rows
    /// follow <paramref name="rows"/>.</summary>
    public static void AfterGlyph(RgbaImage surface, GlyphRasterRequest request, GlyphMask mask, int drawX, int top,
                                  int topToBaseline, int nextPenX, AdvTextStyle style, DashStripRows rows)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(mask);
        if (request.Cp932Code is not { } code || !Applies(code)) return;
        if (rows == DashStripRows.BlackBox)
            ApplyBlackBox(surface, code, drawX, checked(top + topToBaseline), mask.OriginY, mask.Height,
                          effectMode: style.RenderMode != 0, nextPenX, mask.Width, style.EffectOffsetX,
                          style.EffectOffsetY);
        else
            Apply(surface, code, drawX, top, nextPenX, mask.Width,
                  style.PrimaryFontSize > 0 ? style.PrimaryFontSize : request.PixelHeight,
                  style.EffectOffsetX, style.EffectOffsetY);
    }

    /// <summary>Join the dash just drawn into <paramref name="surface"/>, with the strip's rows the cell's
    /// (<see cref="DashStripRows.Cell"/>).</summary>
    /// <param name="drawX">The x the glyph was drawn at, before its own origin.</param>
    /// <param name="top">The string's y: the top of the glyph's cell.</param>
    /// <param name="nextPenX">The pen after the glyph's advance.</param>
    /// <param name="blackBoxWidth">The glyph's <c>gmBlackBoxX</c>.</param>
    /// <param name="fontSize">The primary font size (text manager +0x313d4).</param>
    public static void Apply(RgbaImage surface, ushort code, int drawX, int top, int nextPenX, int blackBoxWidth,
                             int fontSize, int effectOffsetX, int effectOffsetY)
        => Join(surface, code, drawX, top, unchecked(top + fontSize + effectOffsetY * 2), nextPenX, blackBoxWidth,
                effectOffsetX);

    /// <summary>Join the dash just drawn into <paramref name="surface"/>, with the strip's rows the glyph's black box
    /// (<see cref="DashStripRows.BlackBox"/>): from the baseline less <c>gmptGlyphOrigin.y</c>, <c>gmBlackBoxY</c>
    /// rows, and in an effect mode also <paramref name="effectOffsetY"/> rows above and below.</summary>
    /// <param name="baselineY">The glyph's baseline: the string's y plus the ascent it was drawn with.</param>
    /// <param name="originY">The glyph's <c>gmptGlyphOrigin.y</c>, upward from the baseline.</param>
    /// <param name="blackBoxHeight">The glyph's <c>gmBlackBoxY</c>.</param>
    /// <param name="effectMode">Whether the text effect is on (render mode 1 to 3): native draws the string with
    /// text_draw_glyph_string_effect then, and with text_draw_glyph_string_plain in mode 0
    /// (text_draw_string_primary_or_ruby@0x454850).</param>
    public static void ApplyBlackBox(RgbaImage surface, ushort code, int drawX, int baselineY, int originY,
                                     int blackBoxHeight, bool effectMode, int nextPenX, int blackBoxWidth,
                                     int effectOffsetX, int effectOffsetY)
    {
        unchecked
        {
            int grow = effectMode ? effectOffsetY : 0;
            int top = baselineY - originY - grow;
            Join(surface, code, drawX, top, top + blackBoxHeight + grow * 2, nextPenX, blackBoxWidth, effectOffsetX);
        }
    }

    private static void Join(RgbaImage surface, ushort code, int drawX, int top, int bottom, int nextPenX,
                             int blackBoxWidth, int effectOffsetX)
    {
        ArgumentNullException.ThrowIfNull(surface);
        if (!Applies(code)) return;
        unchecked
        {
            var source = new Rect
            {
                Left = drawX + effectOffsetX,
                Top = top,
                Right = drawX + blackBoxWidth - 2,
                Bottom = bottom,
            };
            var destination = new Rect
            {
                Left = drawX + 1 - (source.Right - source.Left),
                Top = top,
                Right = drawX + 1,
                Bottom = source.Bottom,
            };
            Copy(surface, ref source, ref destination);
            if (code != BothSides) return;
            destination.Right = nextPenX + effectOffsetX;
            destination.Left = source.Left + (destination.Right - source.Right);
            Copy(surface, ref source, ref destination);
        }
    }

    // gfx_copy_surface_rect's clip and copy for one surface whose bounds are (0, 0, width, height).
    private static void Copy(RgbaImage surface, ref Rect source, ref Rect destination)
    {
        unchecked
        {
            Clip(surface, ref destination, ref source);
            Clip(surface, ref source, ref destination);
        }
        if (source.Right <= source.Left || source.Bottom <= source.Top) return;
        RgbaSurfaceOps.CopyRect(surface, surface, source.Left, source.Top, source.Right - source.Left,
                                source.Bottom - source.Top, destination.Left, destination.Top);
    }

    private static void Clip(RgbaImage surface, ref Rect clipped, ref Rect other)
    {
        if (clipped.Left < 0)
        {
            other.Left += -clipped.Left;
            clipped.Left = 0;
        }
        if (clipped.Top < 0)
        {
            other.Top += -clipped.Top;
            clipped.Top = 0;
        }
        if (surface.Width < clipped.Right)
        {
            other.Right += surface.Width - clipped.Right;
            clipped.Right = surface.Width;
        }
        if (surface.Height < clipped.Bottom)
        {
            other.Bottom += surface.Height - clipped.Bottom;
            clipped.Bottom = surface.Height;
        }
    }
}

/// <summary>Which rows of the text surface the dash join copies (<see cref="ExtendedDashJoin"/>; docs/engine-re.md, "The
/// dash join").</summary>
public enum DashStripRows
{
    /// <summary>The cell's: from the string's top, the primary font size plus twice the effect's vertical offset
    /// (4.38D's text_draw_glyph_string_plain@0x4692f0 and text_draw_glyph_string_effect@0x46b1d0, and 4.43B's).</summary>
    Cell,

    /// <summary>The glyph's black box: from the baseline less <c>gmptGlyphOrigin.y</c>, <c>gmBlackBoxY</c> rows,
    /// widened by the effect's vertical offset above and below when the effect is on (4.45D's
    /// text_draw_glyph_string_plain@0x46c4b0, the join at 0x46ca70, and text_draw_glyph_string_effect@0x46e360, the
    /// join at 0x46ede8).</summary>
    BlackBox,
}
