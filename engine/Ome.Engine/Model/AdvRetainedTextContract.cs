namespace Ome.Engine.Model;

[Flags]
public enum AdvTextOverflowFlags
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
}

/// <summary>
/// Native AGE's 20-byte live ADV glyph record. Rectangle values are edges, matching the
/// retained-object bind ABI; they are not x/y/width/height values.
/// </summary>
public readonly record struct AdvRetainedGlyphRecord(
    int PublicationChainFlag,
    int Left,
    int Top,
    int Right,
    int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public bool PublishesNextInSameTick => PublicationChainFlag == 1;
}

/// <summary>The script-owned retained-object binding associated with one live ADV layout.</summary>
public readonly record struct AdvTextLayoutPresentationBinding(
    int LayoutSlot,
    int SourceSurfaceSlot,
    long FirstObjectHandle,
    long ObjectCapacity,
    long WaitIndicatorObjectHandle,
    int ResetCursorX,
    int ResetCursorY);

/// <summary>
/// A glyph's retained record and the layout origin it is published at. Like
/// adv_text_publish_next_glyph@0x451220, publication crops the layout surface to the record's measured
/// cell; ink placed at the font's ascent stays inside it.
/// </summary>
public readonly record struct AdvRetainedGlyphPlacement(
    AdvRetainedGlyphRecord Record,
    int LayoutOriginX,
    int LayoutOriginY)
{
    /// <summary>The rows the glyph's object shows below its record's cell: what the build's publisher added to the
    /// bottom of the source rectangle when it bound the object (<c>TextRules.PublisherGrowsGlyphCells</c>, or op
    /// 0x260's bottom padding); 0 before it is published and on a build whose publisher binds the cell as it is. A
    /// raised ruby character has a negative growth here and the same rows at <see cref="GrowTop"/>.</summary>
    public int CellGrowth { get; init; }

    /// <summary>The columns and rows the object shows left of, above and right of its record's cell (op 0x260's
    /// paddings, and the raise of a ruby character in RubyShiftMode 2); 0 before it is published.</summary>
    public int GrowLeft { get; init; }

    /// <inheritdoc cref="GrowLeft"/>
    public int GrowTop { get; init; }

    /// <inheritdoc cref="GrowLeft"/>
    public int GrowRight { get; init; }

    /// <summary>How far left and up the build's publisher moved the glyph's object from its cell when it bound it
    /// (<c>TextRules.PublisherMovesAliasedEffectGlyphs</c>, and op 0x260's left and top paddings); 0 before it is
    /// published.</summary>
    public int ObjectShiftX { get; init; }

    /// <inheritdoc cref="ObjectShiftX"/>
    public int ObjectShiftY { get; init; }

    /// <summary>The text row the glyph is on, counted from 1 since its layout's reset as AGE 4.43B's window counts them
    /// (window <c>+0x11c</c>, which a record's sixth dword keeps): each wrap and each op 0x6f starts the next. It says how
    /// many line images op 0x25c's window shows; 0 where the host did not count rows.</summary>
    public int Row { get; init; }

    /// <summary>The surface rectangle the object shows: the cell, grown by what the publisher added on each side.</summary>
    public (int X, int Y, int Width, int Height) SourceRect
        => (checked(Record.Left - GrowLeft), checked(Record.Top - GrowTop),
            checked(Record.Width + GrowLeft + GrowRight), checked(Record.Height + GrowTop + CellGrowth));

    /// <summary>The object's place: the layout origin plus the cell's corner, less the publisher's move.</summary>
    public (int X, int Y) ObjectPosition
        => (checked(LayoutOriginX + Record.Left - ObjectShiftX), checked(LayoutOriginY + Record.Top - ObjectShiftY));
}

/// <summary>What a build's publisher changes as it binds a glyph: the rows it adds under the cell
/// (<c>TextRules.PublisherGrowsGlyphCells</c>, or op 0x260's bottom padding), the columns and rows it adds on the other
/// sides (<see cref="GrowLeft"/>), and how far it moves the object left and up
/// (<c>TextRules.PublisherMovesAliasedEffectGlyphs</c>, op 0x260's left and top paddings). The default changes
/// nothing.</summary>
public readonly record struct GlyphPublication(int CellGrowth, int ShiftX = 0, int ShiftY = 0)
{
    /// <summary>The columns added left of the cell; with <see cref="GrowTop"/> and <see cref="GrowRight"/>, AGE 4.43B's
    /// paddings (op 0x260 while op 0x261's bit 0 is set). A ruby character AGE 4.43B raises in RubyShiftMode 2 has
    /// the top padding here, the same rows taken from <see cref="CellGrowth"/>, and its object moved up by them.</summary>
    public int GrowLeft { get; init; }

    /// <inheritdoc cref="GrowLeft"/>
    public int GrowTop { get; init; }

    /// <inheritdoc cref="GrowLeft"/>
    public int GrowRight { get; init; }
}

/// <summary>One object History's renderer binds (text_history_render_records: 4.24J @0x4526c0 to 4.45D @0x464fa0): over
/// glyphs [<see cref="FirstGlyph"/>, + <see cref="GlyphCount"/>) of its run, a text record's (from its first glyph's left
/// to its last glyph's right, and from its glyphs' top to their bottom) or a ruby character's; or, with no glyphs, over
/// <see cref="Empty"/>, an empty record's rectangle in the layout.</summary>
public readonly record struct AdvTextRecordObject(int FirstGlyph, int GlyphCount)
{
    public AdvRetainedGlyphRecord? Empty { get; init; }
}

/// <summary>What a publisher does to the glyphs it binds: <see cref="Primary"/> to a record that is not ruby (chain flag
/// 0), <see cref="Ruby"/> to a ruby character, and with <see cref="FadeMs"/> above 0 it fades each in over that many
/// milliseconds (AGE 4.43B's timed publisher: op 0x2ee's MessageFade percent of the glyph delay).</summary>
public readonly record struct GlyphPublications(GlyphPublication Primary, GlyphPublication Ruby = default, int FadeMs = 0)
{
    public static implicit operator GlyphPublications(GlyphPublication primary) => new(primary);
}

public readonly record struct AdvRetainedTextRunResult(
    int LayoutSlot,
    int FirstGlyphIndex,
    int GlyphCount,
    int CursorX,
    int CursorY)
{
    /// <summary>The run's drawn rows; empty when the host did not lay the text out.</summary>
    public IReadOnlyList<AdvTextRow> Rows { get; init; } = [];

    /// <summary>Op 0x196's ruby characters in drawing order, each with the row it was drawn over.</summary>
    public IReadOnlyList<AdvRubyGlyph> Ruby { get; init; } = [];

    /// <summary>The run's source text laid out where native AGE would draw it, when the host drew something else
    /// (localization spec §8): presented text, another rasterizer, or from the presented cursor. Null when the drawn
    /// layout is the source layout. History records these rows and the layout's source cursor follows them.</summary>
    public AdvTextSourceLayout? Source { get; init; }

    /// <summary>The rows of presented text the run did not draw because they fell below its layout's bottom
    /// (localization spec §7.3); 0 when nothing was cut.</summary>
    public int CutRows { get; init; }

    /// <summary>The glyph width, in percent, presented text was narrowed to so that it fitted its layout instead of
    /// being cut (the language's <c>overflow = "shrink"</c>, spec §7.3); 0 when the run was not narrowed for that.</summary>
    public int NarrowedToFitPercent { get; init; }
}

/// <summary>
/// The glyph objects one ADV layout would have bound with no language selected (localization spec §8): its object range
/// and surface, and the source glyphs published so far, glyph i bound at <see cref="FirstHandle"/> + i. A numbered save
/// writes these in place of the drawn glyphs, so it holds what the source-language run would have.
/// </summary>
public sealed record AdvTextSourceObjects(
    long FirstHandle, long Capacity, int SurfaceSlot, IReadOnlyList<AdvRetainedGlyphPlacement> Glyphs,
    AdvRetainedGlyphPlacement? Pending = null);

/// <summary>A source layout pass (localization spec §8): the rows and ruby native AGE would record for a run's
/// source text, and where its cursor would end. Nothing is drawn.</summary>
public sealed record AdvTextSourceLayout(
    IReadOnlyList<AdvTextRow> Rows, IReadOnlyList<AdvRubyGlyph> Ruby, int CursorX, int CursorY);

/// <summary>One drawn ruby character: its text, the base row it sits over, and its retained record.</summary>
public readonly record struct AdvRubyGlyph(int Row, string Text, AdvRetainedGlyphRecord Record);

/// <summary>
/// One drawn row of a show-text run: <see cref="GlyphCount"/> glyphs (one per Unicode scalar of the run's
/// text) from <see cref="FirstGlyph"/>, with rectangle edges from the first glyph's left/top to the last
/// glyph's right/bottom. <see cref="WrapsAfter"/> marks a horizontal wrap, which AGE records as a line
/// break (adv_text_build_glyph_records@0x4576c0).
/// </summary>
public readonly record struct AdvTextRow(
    int FirstGlyph, int GlyphCount, int Left, int Top, int Right, int Bottom, bool WrapsAfter);

/// <summary>Platform-neutral rules established from AGE's native retained-glyph workers.</summary>
public static class AdvRetainedTextContract
{
    public static AdvTextOverflowFlags CheckOverflow(
        int rightBound, int bottomBound, int glyphRight, int glyphBottom)
    {
        var result = AdvTextOverflowFlags.None;
        if (rightBound < glyphRight) result |= AdvTextOverflowFlags.Horizontal;
        if (bottomBound < glyphBottom) result |= AdvTextOverflowFlags.Vertical;
        return result;
    }

    public static bool TimedPublicationHandleExists(int revealIndex, long objectCapacity)
        => revealIndex >= 0 && revealIndex < objectCapacity;
}
