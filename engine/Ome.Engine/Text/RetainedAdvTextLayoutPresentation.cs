using Ome.Engine.Model;

namespace Ome.Engine.Text;

/// <summary>
/// Transient live-layout presentation state. History persistence owns neither this list nor its Gfx
/// bindings; saved-frame script reconstruction rebuilds both.
/// </summary>
public sealed class RetainedAdvTextLayoutPresentation
{
    private readonly List<AdvRetainedGlyphPlacement> _glyphs = new();
    // The glyphs each run's source text has in the source layout (localization spec §8), in drawing order, and for
    // each run the drawn glyph count after it and its source glyphs' end.
    private readonly List<AdvRetainedGlyphPlacement> _sourceGlyphs = new();
    private readonly List<(int DrawnEnd, int SourceEnd)> _sourceRuns = new();
    // For each run, what the publisher last did to its glyphs (the rows under their cells, the move), and whether to
    // every record alike or to ruby characters apart: its source glyphs are published as its drawn ones were.
    private readonly List<(GlyphPublications Publications, bool EveryRecord)> _sourceRunGrowth = new();

    public RetainedAdvTextLayoutPresentation(AdvTextLayoutPresentationBinding binding)
    {
        if (binding.LayoutSlot <= 0)
            throw new ArgumentOutOfRangeException(nameof(binding));
        if (binding.SourceSurfaceSlot < 0)
            throw new ArgumentOutOfRangeException(nameof(binding));
        if (binding.FirstObjectHandle < 0 || binding.ObjectCapacity <= 0)
            throw new ArgumentException(
                "A retained ADV layout requires a nonnegative first handle and positive capacity.",
                nameof(binding));
        Binding = binding;
    }

    public AdvTextLayoutPresentationBinding Binding { get; }
    public IReadOnlyList<AdvRetainedGlyphPlacement> Glyphs => _glyphs;
    public int PublishedGlyphCount { get; private set; }
    public int PublishableGlyphCount
        => (int)Math.Min(_glyphs.Count, Binding.ObjectCapacity);

    /// <summary>Whether a run drew something other than its source layout (localization spec §8), so the glyphs the
    /// source-language run would have published (<see cref="PublishedSourceGlyphs"/>) are not <see cref="Glyphs"/>.</summary>
    public bool SourceDiffers { get; private set; }

    public int Append(
        IReadOnlyList<AdvRetainedGlyphRecord> records,
        int layoutOriginX,
        int layoutOriginY)
    {
        ArgumentNullException.ThrowIfNull(records);
        int first = _glyphs.Count;
        foreach (AdvRetainedGlyphRecord record in records)
            _glyphs.Add(new AdvRetainedGlyphPlacement(record, layoutOriginX, layoutOriginY));
        if (records.Count > 0) _lastAppendedIsRuby = false;
        AddSourceRun(records, layoutOriginX, layoutOriginY);
        return first;
    }

    /// <summary>
    /// Append one run's records in drawing order, marking which are ruby characters. AGE's builders always draw into
    /// the layout's last (pending) record and then push the next one. In SYS4422 the base builder pushes chain flag 0,
    /// the ruby builder (adv_text_draw_ruby_over_row@0x456b40) chain flag 1, and a layout reset starts over with 0, so a
    /// record's chain flag is 1 exactly when the record drawn before it, in this run or an earlier one, is a ruby
    /// character; the publisher then shows it in the same tick as that character's successor. In SYS4433
    /// (<paramref name="chainEveryRubyRecord"/>; adv_text_draw_ruby_over_row_sys4433@0x4590d0) every ruby record has
    /// chain flag 1 and the ruby builder resets the pending record to 0, so a record's flag is 1 exactly when it is a
    /// ruby character.
    /// <paramref name="sourceRecords"/> are the source layout's records when the run drew something else (localization
    /// spec §8), in the same drawing order; null when the run drew its source layout. <paramref name="rows"/> gives
    /// each record its text row since the layout's reset (<see cref="AdvRetainedGlyphPlacement.Row"/>), when the host
    /// counted them.
    /// </summary>
    public int AppendRun(
        IReadOnlyList<AdvRetainedGlyphRecord> records,
        IReadOnlyList<bool> isRuby,
        int layoutOriginX,
        int layoutOriginY,
        bool chainEveryRubyRecord = false,
        IReadOnlyList<AdvRetainedGlyphRecord>? sourceRecords = null,
        IReadOnlyList<int>? rows = null)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(isRuby);
        if (isRuby.Count != records.Count)
            throw new ArgumentException("Each record needs a ruby marker.", nameof(isRuby));
        if (rows != null && rows.Count != records.Count)
            throw new ArgumentException("Each record needs a row.", nameof(rows));
        int first = _glyphs.Count;
        var flagged = new AdvRetainedGlyphRecord[records.Count];
        for (int index = 0; index < records.Count; index++)
        {
            bool chained = chainEveryRubyRecord ? isRuby[index] : _lastAppendedIsRuby;
            AdvRetainedGlyphRecord record = records[index] with { PublicationChainFlag = chained ? 1 : 0 };
            flagged[index] = record;
            _glyphs.Add(new AdvRetainedGlyphPlacement(record, layoutOriginX, layoutOriginY) { Row = rows?[index] ?? 0 });
            _lastAppendedIsRuby = !chainEveryRubyRecord && isRuby[index];
        }
        AddSourceRun(sourceRecords ?? flagged, layoutOriginX, layoutOriginY);
        SourceDiffers |= sourceRecords != null;
        return first;
    }

    /// <summary>A run that drew nothing (a hidden run of a presented message, localization spec §8): only its source
    /// layout's records, which the source-language run would have drawn.</summary>
    public void AppendSourceOnly(IReadOnlyList<AdvRetainedGlyphRecord> sourceRecords, int layoutOriginX, int layoutOriginY)
    {
        ArgumentNullException.ThrowIfNull(sourceRecords);
        AddSourceRun(sourceRecords, layoutOriginX, layoutOriginY);
        SourceDiffers |= sourceRecords.Count > 0;
    }

    private void AddSourceRun(IReadOnlyList<AdvRetainedGlyphRecord> records, int layoutOriginX, int layoutOriginY)
    {
        foreach (AdvRetainedGlyphRecord record in records)
            _sourceGlyphs.Add(new AdvRetainedGlyphPlacement(record, layoutOriginX, layoutOriginY));
        _sourceRuns.Add((_glyphs.Count, _sourceGlyphs.Count));
        _sourceRunGrowth.Add((default, false));
    }

    /// <summary>The glyphs the source-language run would have published by now (localization spec §8), up to the
    /// layout's capacity: all of every run whose drawn glyphs are published, and of a run still revealing, as many as it
    /// has published.</summary>
    public IReadOnlyList<AdvRetainedGlyphPlacement> PublishedSourceGlyphs()
    {
        int published = 0, drawnStart = 0, sourceStart = 0;
        var grown = new List<AdvRetainedGlyphPlacement>();
        for (int run = 0; run < _sourceRuns.Count; run++)
        {
            (int drawnEnd, int sourceEnd) = _sourceRuns[run];
            bool whole = PublishedGlyphCount >= drawnEnd;
            published = whole
                ? sourceEnd
                : Math.Min(sourceEnd, sourceStart + Math.Max(0, PublishedGlyphCount - drawnStart));
            (GlyphPublications publications, bool everyRecord) = _sourceRunGrowth[run];
            for (int index = sourceStart; index < published; index++)
            {
                AdvRetainedGlyphPlacement glyph = _sourceGlyphs[index];
                grown.Add(Published(glyph, publications, everyRecord));
            }
            if (!whole) break;
            (drawnStart, sourceStart) = (drawnEnd, sourceEnd);
        }
        return grown.Take((int)Math.Min(published, Binding.ObjectCapacity)).ToArray();
    }

    private bool _lastAppendedIsRuby;

    /// <summary>
    /// The glyph count published after <paramref name="ticks"/> publication ticks of a run's records
    /// [<paramref name="firstGlyph"/>, +<paramref name="glyphCount"/>). Each tick of
    /// adv_text_publish_next_glyph@0x451220 publishes one record and keeps going while the record it just published has
    /// chain flag 1, stopping at the run's end.
    /// </summary>
    public int PublicationTarget(int firstGlyph, int glyphCount, int ticks)
    {
        int index = firstGlyph;
        int end = Math.Min(_glyphs.Count, checked(firstGlyph + glyphCount));
        for (int tick = 0; tick < ticks && index < end; tick++)
        {
            bool chained;
            do
            {
                chained = _glyphs[index].Record.PublishesNextInSameTick;
                index++;
            }
            while (chained && index < end);
        }
        return index;
    }

    /// <summary>The number of publication ticks a run's records take (<see cref="PublicationTarget"/>).</summary>
    public int PublicationTicks(int firstGlyph, int glyphCount)
    {
        int ticks = 0;
        int end = Math.Min(_glyphs.Count, checked(firstGlyph + glyphCount));
        for (int index = firstGlyph; index < end; ticks++)
            index = PublicationTarget(index, end - index, 1);
        return ticks;
    }

    /// <summary>The text rows the published glyphs reach: the row of the last one published, 0 with none. AGE 4.43B
    /// shows that many of a window's line images (op 0x25c): its publishers read the row count of the record after the
    /// last one published, which the record took from the window's count when it was pushed.</summary>
    public int PublishedRowCount => PublishedGlyphCount == 0 ? 0 : _glyphs[PublishedGlyphCount - 1].Row;

    /// <summary>
    /// Publish newly revealed glyphs through a layout-global target count, as adv_text_publish_next_glyph does.
    /// <paramref name="publications"/> is what the build's publisher does to each glyph as it binds it: to a record that
    /// is not ruby (chain flag 0), the rows it adds under the cell while a text effect is on
    /// (<c>TextRules.PublisherGrowsGlyphCells</c>), the paddings and the object's move
    /// (<c>TextRules.PublisherMovesAliasedEffectGlyphs</c>); to a ruby character, its raise; and AGE 4.43B's fade of each
    /// glyph it binds. The default does nothing.
    /// </summary>
    public int PublishThrough(GfxState gfx, int targetGlyphCount, GlyphPublications publications = default)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        int target = Math.Clamp(targetGlyphCount, 0, PublishableGlyphCount);
        for (int index = PublishedGlyphCount; index < target; index++)
        {
            Grow(index, publications, everyRecord: false);
            if (Bind(gfx, index) && publications.FadeMs > 0)
                FadeIn(gfx, Binding.FirstObjectHandle + index, publications.FadeMs);
        }
        PublishedGlyphCount = Math.Max(PublishedGlyphCount, target);
        return PublishedGlyphCount;
    }

    /// <summary>AGE 4.43B's fade of a glyph its timed publisher binds (adv_text_publish_next_glyph@0x459440): the object's
    /// colour to white at alpha 0 (gfx_object_set_blend_and_color@0x49bba0, op 0x203's worker), then op 0x202's animation
    /// to opaque white over <paramref name="durationMs"/>.</summary>
    public static void FadeIn(GfxState gfx, long handle, int durationMs)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        gfx.SetStaticObjectColorResolved(handle, 0, 0, 0xffffff);
        gfx.SetAnimatedObjectColorResolved(handle, 0, durationMs, 0xff, 0xffffff);
    }

    /// <summary>
    /// Recreate every currently revealed binding. With <paramref name="layout"/> it is op 0x20a's
    /// adv_text_publish_layout, which does one thing to every record, chosen by the chain flag of the record at the
    /// reveal index, not of the record it binds: <paramref name="layout"/>'s primary publication when that record is not
    /// ruby (the rows under the cell, the paddings, the move), else its ruby one. A ruby publication that changes nothing
    /// leaves each cell with what it had: AGE 4.33G's and 4.38D's chained case moves cells in effect mode 3 with
    /// RubyShiftMode 0, which the port does not model. Without it (a layout coming back from a suspension, a redraw)
    /// each glyph keeps what it was published with.
    /// </summary>
    public int Republish(GfxState gfx, GlyphPublications? layout = null)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        GlyphPublication? every = layout is { } publications ? EveryRecordPublication(publications) : null;
        for (int index = 0; index < PublishedGlyphCount; index++)
        {
            if (every is { } applied) Grow(index, new GlyphPublications(applied), everyRecord: true);
            Bind(gfx, index);
        }
        return PublishedGlyphCount;
    }

    // What op 0x20a does to every record: chosen by the chain flag of the record at the reveal index.
    private GlyphPublication? EveryRecordPublication(GlyphPublications layout)
    {
        bool chained = PublishedGlyphCount < _glyphs.Count
                       && _glyphs[PublishedGlyphCount].Record.PublicationChainFlag != 0;
        if (!chained) return layout.Primary;
        return layout.Ruby == default ? null : layout.Ruby;
    }

    // What op 0x20a last bound past the glyphs: the pending record's publication, while the layout still has the glyph
    // count it had then (a glyph drawn since fills that record).
    private (int GlyphCount, GlyphPublication Publication)? _pendingBound;

    /// <summary>
    /// Op 0x20a (adv_text_publish_layout; 4.24J @0x450c80, 4.33G @0x4503b0, 4.38D @0x453990, 4.43B @0x4583e0): the
    /// layout's objects are erased, then <see cref="Republish"/> with <paramref name="layout"/>, and the record at the
    /// reveal index too, since native binds records 0 to that index inclusive (docs/engine-re.md, "The glyph publisher
    /// grows a cell"). While a run reveals, that record is the next glyph. Once every glyph is shown it is the pending
    /// record, the cell the next glyph is drawn into: left and top <paramref name="cursor"/>, right and bottom 0, so it
    /// draws nothing, but a save keeps it. With no glyph published nothing is erased or bound, as native's publisher
    /// does nothing at reveal index 0.
    /// </summary>
    public int PublishLayout(GfxState gfx, GlyphPublications? layout, (int X, int Y) cursor)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        if (PublishedGlyphCount == 0) return 0;
        gfx.EraseRange(Binding.FirstObjectHandle, Binding.ObjectCapacity);
        GlyphPublication? every = layout is { } publications ? EveryRecordPublication(publications) : null;
        int published = Republish(gfx, layout);
        if (published < _glyphs.Count)
        {
            if (every is { } applied) Grow(published, new GlyphPublications(applied), everyRecord: true);
            Bind(gfx, published);
            return published;
        }
        GlyphPublication pendingPublication = every ?? default;
        BindPending(gfx, Binding.FirstObjectHandle + published, Binding.SourceSurfaceSlot,
                    Pending(cursor, pendingPublication));
        _pendingBound = (_glyphs.Count, pendingPublication);
        return published;
    }

    /// <summary>The pending record a source-language run would have had op 0x20a bind (localization spec §8), at the
    /// source layout's <paramref name="sourceCursor"/>: null unless op 0x20a bound the drawn one and no glyph was drawn
    /// since.</summary>
    public AdvRetainedGlyphPlacement? PendingSource((int X, int Y) sourceCursor)
        => _pendingBound is { } bound && bound.GlyphCount == _glyphs.Count
            ? Pending(sourceCursor, bound.Publication)
            : null;

    private AdvRetainedGlyphPlacement Pending((int X, int Y) cursor, GlyphPublication publication)
    {
        AdvRetainedGlyphPlacement last = _glyphs[^1];
        return WithPublication(
            new AdvRetainedGlyphPlacement(new AdvRetainedGlyphRecord(0, cursor.X, cursor.Y, 0, 0),
                                          last.LayoutOriginX, last.LayoutOriginY) { Row = last.Row },
            publication);
    }

    /// <summary>Bind a pending record as native does: its rectangle runs back from the cursor to 0, so it is empty.
    /// </summary>
    public static void BindPending(GfxState gfx, long handle, int surfaceSlot, AdvRetainedGlyphPlacement pending)
    {
        (int x, int y, int width, int height) = pending.SourceRect;
        gfx.BindDraw(handle, surfaceSlot, x, y, width, height, pending.ObjectPosition.X, pending.ObjectPosition.Y);
    }

    /// <summary>
    /// Recreate every currently revealed binding as op 0x82's redraw does on 4.33G, 4.38D and 4.43B
    /// (text_redraw_retained_group, @0x4553c0, @0x460cd0 and @0x463310): it binds each record again, doing to each
    /// what <paramref name="publications"/> says for its own chain flag, as <see cref="PublishThrough"/> does, unlike the
    /// layout publisher, which chooses by the record at the reveal index. It does not fade.
    /// </summary>
    public int RepublishEach(GfxState gfx, GlyphPublications publications)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        for (int index = 0; index < PublishedGlyphCount; index++)
        {
            Grow(index, publications with { FadeMs = 0 }, everyRecord: false);
            Bind(gfx, index);
        }
        return PublishedGlyphCount;
    }

    // Remember what the publisher does to glyph index, and give its run's source glyphs the same.
    private void Grow(int index, GlyphPublications publications, bool everyRecord)
    {
        _glyphs[index] = Published(_glyphs[index], publications, everyRecord);
        for (int run = 0; run < _sourceRuns.Count; run++)
            if (index < _sourceRuns[run].DrawnEnd)
            {
                _sourceRunGrowth[run] = (publications, everyRecord);
                break;
            }
    }

    // A glyph as the publisher binds it: the primary publication for a record that is not ruby (chain flag 0) or when
    // the publisher does the same to every record, else the ruby one.
    private static AdvRetainedGlyphPlacement Published(AdvRetainedGlyphPlacement glyph, GlyphPublications publications,
                                                       bool everyRecord)
        => WithPublication(glyph, everyRecord || glyph.Record.PublicationChainFlag == 0
                                      ? publications.Primary
                                      : publications.Ruby);

    private static AdvRetainedGlyphPlacement WithPublication(AdvRetainedGlyphPlacement glyph, GlyphPublication applied)
        => glyph with
        {
            CellGrowth = applied.CellGrowth, GrowLeft = applied.GrowLeft, GrowTop = applied.GrowTop,
            GrowRight = applied.GrowRight, ObjectShiftX = applied.ShiftX, ObjectShiftY = applied.ShiftY,
        };

    public void ErasePublished(GfxState gfx)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        gfx.EraseRange(Binding.FirstObjectHandle, Binding.ObjectCapacity);
        _pendingBound = null;
    }

    // Bind glyph index's object; false for an empty cell, which the port leaves unbound.
    private bool Bind(GfxState gfx, int index)
    {
        AdvRetainedGlyphPlacement placement = _glyphs[index];
        AdvRetainedGlyphRecord cell = placement.Record;
        if (cell.Width <= 0 || cell.Height <= 0) return false;
        (int x, int y, int width, int height) = placement.SourceRect;
        gfx.BindDraw(
            checked(Binding.FirstObjectHandle + index),
            Binding.SourceSurfaceSlot,
            x,
            y,
            width,
            height,
            placement.ObjectPosition.X,
            placement.ObjectPosition.Y);
        return true;
    }
}
