using System;
using Ome.Engine.Model;
using Ome.Engine.Text;

namespace Ome.Engine.Natives;

// AGE 4.43B's line images (ops 0x25c to 0x25f; docs/engine-re.md, "Arterial against 魔導巧殻, triaged"): a layout can
// show an image under each text row its published glyphs reach. 魔導巧殻's SYSTEM4 gives layout 1 a dotted line from
// surface 17 under each of its first three rows.
public sealed partial class TextManager
{
    /// <summary>
    /// Bind the layout's line images its published glyphs reach and that do not exist yet, as the publishers do after
    /// each publication: image <c>i</c> shows the images' surface rectangle at the layout's origin plus the offsets,
    /// <c>i</c> row pitches lower (the font size and the line spacing, text manager +0x313d4 and +0x564), with op 0x25f's
    /// blend mode and colour. The timed publisher (<paramref name="animate"/>; adv_text_publish_next_glyph@0x459440) then
    /// starts op 0x25e's animation when its duration is set; the layout publisher (adv_text_publish_layout@0x4583e0)
    /// binds the animation's end colour instead.
    /// </summary>
    private void BindLineImages(GfxState gfx, RetainedAdvTextLayoutPresentation presentation, bool animate)
    {
        int slot = presentation.Binding.LayoutSlot;
        AdvTextLineImages images = History.GetLineImages(slot);
        if (!images.Enabled || images.Count <= 0) return;
        long count = Math.Min(presentation.PublishedRowCount, images.Count);
        if (count <= 0) return;
        AdvTextLayoutSnapshot layout = History.GetLayoutObjectSnapshot(slot);
        int pitch = checked((Style.PrimaryFontSize > 0 ? Style.PrimaryFontSize : 24) + Style.LineSpacing);
        uint color = animate || images.FadeDurationMs == 0 ? images.Color : images.FadeColor;
        for (int row = 0; row < count; row++)
        {
            long handle = checked(images.FirstHandle + row);
            if (gfx.TryGet(handle) != null) continue;
            gfx.BindDraw(handle, images.Surface, images.SourceLeft, images.SourceTop,
                         unchecked(images.SourceRight - images.SourceLeft),
                         unchecked(images.SourceBottom - images.SourceTop),
                         checked(layout.OriginX + images.OffsetX), checked(layout.OriginY + pitch * row + images.OffsetY));
            gfx.SetStaticObjectColorResolved(handle, images.BlendMode, color >> 24, color & 0xffffff);
            if (animate && images.FadeDurationMs != 0)
                gfx.SetAnimatedObjectColorResolved(handle, images.FadeDelayMs, images.FadeDurationMs,
                                                   images.FadeColor >> 24, images.FadeColor & 0xffffff);
        }
        _graphics.RequestPresentation();
    }

    /// <summary>Erase a layout's line images, as its reset (adv_text_layout_reset@0x45c1d0) and the layout publisher do:
    /// the handles op 0x25d gave it, whatever op 0x25c set.</summary>
    private static void EraseLineImages(GfxState gfx, AdvTextLineImages images)
    {
        if (images.Count > 0) gfx.EraseRange(images.FirstHandle, images.Count);
    }
}
