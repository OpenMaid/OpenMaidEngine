using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Diagnostics;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Text;

namespace Ome.Engine.Natives;

// Laying runs out and drawing them: a show-text run into its layout's surface, a hidden run's source pass, op 0x204's
// strings, op 0x1d1's History groups and op 0x82's redraws. Without devices nothing is drawn.
public sealed partial class TextManager
{
    // The layout the last run was shown in: the page a hide-window callback suspends outside an input wait. Not
    // native's current layout, which slot 0 selects (AdvTextHistory.CurrentLayoutSlot, op 0x80's).
    private int _lastRunLayout = AdvTextHistory.InitialCurrentLayoutSlot;

    /// <summary>The page map's view of the run being shown (<see cref="PageLocatorState"/>): its source layout's
    /// geometry, set by the pass that laid the source text out, and whether the drawn run stopped at its layout's
    /// bottom. The engine thread sets and reads them around one run.</summary>
    private PageTextGeometry? _runGeometry;
    private bool _runCut;

    /// <summary>
    /// A later run of a presented message (localization spec §6.2): the message's first run drew the whole presented
    /// sentence, so this run draws nothing. The page map and the locator keep its source text (§14), and its source
    /// layout (§8) is what history records and the source cursor follows. Null without devices: history then records
    /// one row at the cursor.
    /// </summary>
    private AdvTextSourceLayout? LayoutHiddenTextRun(AdvTextLayoutPresentationBinding binding, AdvLiveTextRun run)
    {
        if (_sourceText == null) return null;
        // A hidden run draws nothing: its message's first run drew the presented text.
        if (run.Layout.Width <= 0 || run.Layout.Height <= 0)
        {
            _locator?.Text(run.SourceOffset, run.Text, "", null);
            return null;
        }
        _runGeometry = null;
        AdvTextSourceLayout source = LayoutSourceText(binding, run, out IReadOnlyList<AdvRetainedGlyphRecord> glyphs, out _);
        _locator?.Text(run.SourceOffset, run.Text, "", _runGeometry);
        if (binding.LayoutSlot <= 0 || binding.LayoutSlot != run.Layout.Slot || binding.SourceSurfaceSlot < 0
            || binding.FirstObjectHandle < 0 || binding.ObjectCapacity <= 0)
            return source;
        if (!_scene.RetainedTextLayouts.TryGetValue(binding.LayoutSlot, out RetainedAdvTextLayoutPresentation? presentation))
        {
            presentation = new RetainedAdvTextLayoutPresentation(binding);
            _scene.RetainedTextLayouts.Add(binding.LayoutSlot, presentation);
        }
        // Drawn by the message's first run; a save binds what the source-language run would have drawn.
        if (presentation.Binding == binding)
            presentation.AppendSourceOnly(glyphs, run.Layout.OriginX, run.Layout.OriginY);
        return source;
    }

    /// <summary>The layouts whose bound glyphs are not what the source-language run would have bound, with its glyphs
    /// (localization spec §8). A numbered save writes those in their place.</summary>
    public IReadOnlyList<AdvTextSourceObjects> SourceTextObjects
    {
        get
        {
            return _scene.RetainedTextLayouts.Values
                .Where(presentation => presentation.SourceDiffers)
                .Select(presentation =>
                {
                    IReadOnlyList<AdvRetainedGlyphPlacement> glyphs = presentation.PublishedSourceGlyphs();
                    AdvTextLayoutSnapshot layout = History.GetLayoutSnapshot(presentation.Binding.LayoutSlot);
                    return new AdvTextSourceObjects(
                        presentation.Binding.FirstObjectHandle, presentation.Binding.ObjectCapacity,
                        presentation.Binding.SourceSurfaceSlot, glyphs,
                        glyphs.Count > 0 ? presentation.PendingSource((layout.CursorX, layout.CursorY)) : null);
                })
                .ToArray();
        }
    }

    /// <summary>
    /// The source layout pass (localization spec §8): <paramref name="run"/>'s source text and ruby laid out with the
    /// gameplay rasterizer from the source cursor, as the run would draw with no language selected, without drawing.
    /// History records its rows, so a save holds what the source-language run would have recorded, and
    /// <paramref name="glyphs"/> are its records in drawing order, which a save binds in place of the drawn glyphs.
    /// <paramref name="extent"/> is where its ink runs, which an entry's <c>align</c> and <c>fit</c> place presented
    /// text against (§7.2).
    /// </summary>
    private AdvTextSourceLayout LayoutSourceText(AdvTextLayoutPresentationBinding binding, AdvLiveTextRun run,
                                                 out IReadOnlyList<AdvRetainedGlyphRecord> glyphs, out TextExtent? extent)
    {
        TextPipeline text = _sourceText!;
        AspectFontRealization? realization = text.Realization;
        (int x, int y) = run.SourceCursor ?? (run.Layout.CursorX, run.Layout.CursorY);
        GlyphTextLayoutResult laid = text.Layout.Render(
            null,
            new GlyphTextLayoutOptions(
                x, y, binding.ResetCursorX, run.Layout.Right, run.Layout.Bottom, WrapHorizontally: true, run.Style,
                FontRealization: realization),
            ImmediateSurfaceTextRenderer.CreateRequests(run.Text, run.Style, text.Policy, _textAntialias, realization));
        IReadOnlyList<AdvRubyGlyph> ruby = !string.IsNullOrEmpty(run.SourceRuby) && laid.Rows.Count > 0
            ? text.Layout.RenderRuby(
                null, run.Text, laid.Rows, run.SourceRuby, run.Style, text.Policy, _textAntialias, LiveRubyRules)
            : [];
        // A ruby record carries chain flag 1 where the build's ruby builder gives every one of them the flag, so the
        // publisher's cell growth tells the two apart in the source layout as it does in the drawn one.
        glyphs = OrderedGlyphs(laid, ruby, byRow: !string.IsNullOrEmpty(run.SourceRuby) && laid.Rows.Count > 0)
            .Select(item => item.Record with
            {
                PublicationChainFlag = item.IsRuby && _rubyRules.ChainEveryRubyRecord ? 1 : 0,
            }).ToArray();
        extent = PresentedTextPlacement.Extent(run.Text, laid.Records);
        _runGeometry = PageGeometry(run, laid, text, realization);
        return new AdvTextSourceLayout(laid.Rows, ruby, laid.CursorX, laid.CursorY);
    }

    /// <summary>The page map's geometry of <paramref name="run"/>'s source text laid out as <paramref name="laid"/>
    /// with <paramref name="text"/>'s rasterizer; null unless the locator writes a map.</summary>
    private PageTextGeometry? PageGeometry(AdvLiveTextRun run, GlyphTextLayoutResult laid, TextPipeline text,
                                           AspectFontRealization? realization)
    {
        if (_locator is not { WritesMap: true } || laid.Rows.Count == 0) return null;
        int space = text.Layout.Render(
            null,
            new GlyphTextLayoutOptions(0, 0, 0, int.MaxValue, int.MaxValue, WrapHorizontally: false, run.Style,
                                       FontRealization: realization),
            ImmediateSurfaceTextRenderer.CreateRequests(" ", run.Style, text.Policy, _textAntialias, realization)).CursorX;
        int height = run.Style.PrimaryFontSize > 0 ? run.Style.PrimaryFontSize : laid.Rows[0].Bottom - laid.Rows[0].Top;
        return PageTextGeometry.Measure(run.Text, laid.Rows, laid.Records, run.Layout.Slot, run.Layout.Right,
                                        height + run.Style.LineSpacing, space);
    }

    /// <summary>A run's records in AGE's drawing order: with ruby (<paramref name="byRow"/>), op 0x196's ruby follows
    /// each base row.</summary>
    private static IEnumerable<(AdvRetainedGlyphRecord Record, bool IsRuby)> OrderedGlyphs(
        GlyphTextLayoutResult rendered, IReadOnlyList<AdvRubyGlyph> ruby, bool byRow)
    {
        if (!byRow)
        {
            foreach (AdvRetainedGlyphRecord record in rendered.Records) yield return (record, false);
            yield break;
        }
        for (int row = 0; row < rendered.Rows.Count; row++)
        {
            AdvTextRow drawn = rendered.Rows[row];
            for (int glyph = drawn.FirstGlyph; glyph < drawn.FirstGlyph + drawn.GlyphCount; glyph++)
                yield return (rendered.Records[glyph], false);
            foreach (AdvRubyGlyph character in ruby)
                if (character.Row == row) yield return (character.Record, true);
        }
    }

    /// <param name="recordedLineBreaks">History's retained rows (op 0x1d1): break only at these glyph
    /// indices, never at the layout's edges. Native text_history_render_records@0x4526c0 checks no bounds;
    /// the surface clips what falls outside.</param>
    /// <param name="historyRuby">History's ruby runs (op 0x1d1), each drawn over its base glyphs.</param>
    /// <param name="history">Op 0x1d1's group: no bottom bound, as native checks none, and no source layout pass.</param>
    private void PrepareRetainedTextRun(
        GfxState gfx,
        AdvTextLayoutPresentationBinding binding,
        AdvLiveTextRun run,
        out RetainedAdvTextLayoutPresentation presentation,
        out AdvRetainedTextRunResult result,
        IReadOnlyList<int>? recordedLineBreaks = null,
        IReadOnlyList<AdvTextRubyAnnotation>? historyRuby = null,
        bool history = false)
    {
        presentation = null!;
        result = default;
        if (run.Layout.Width <= 0 || run.Layout.Height <= 0)
            throw new InvalidOperationException(
                $"ADV layout {run.Layout.Slot} has invalid dimensions " +
                $"{run.Layout.Width}x{run.Layout.Height}.");

        RgbaImage destination = _graphics.SurfacePixels(binding.SourceSurfaceSlot)
            ?? new RgbaImage(
                run.Layout.Width,
                run.Layout.Height,
                new byte[checked(run.Layout.Width * run.Layout.Height * 4)]);
        if (destination.Width != run.Layout.Width
            || destination.Height != run.Layout.Height)
            throw new InvalidOperationException(
                $"ADV layout {run.Layout.Slot} surface {binding.SourceSurfaceSlot} is " +
                $"{destination.Width}x{destination.Height}; expected " +
                $"{run.Layout.Width}x{run.Layout.Height}.");

        var updated = new RgbaImage(
            destination.Width, destination.Height, (byte[])destination.Pixels.Clone());
        // A presented run always draws with the Unicode rasterizer, even if the language changed meanwhile. It is laid out
        // as its source text would be (localization spec §7): the same style, aspect mode and rules.
        TextPipeline text = run.Presented != null ? _presentedText!.Value : ActiveText;
        string drawnText = run.Presented?.Text ?? run.Text;
        AdvTextStyle style = run.Style;
        AspectFontRealization? realization = text.Realization;
        IReadOnlyList<GlyphRasterRequest> requests = Requests(drawnText, style, text);
        IReadOnlyList<int>? breaks = recordedLineBreaks;
        int rightBound = recordedLineBreaks == null ? run.Layout.Right : int.MaxValue;
        bool wrap = recordedLineBreaks == null;

        // What history records and a save binds (spec §8): the drawn rows when the run drew its source text with the
        // gameplay rasterizer from the source cursor, else a separate source layout pass, laid out first: presented text
        // aligns and fits against its extent (§7.2).
        IReadOnlyList<AdvRetainedGlyphRecord>? sourceGlyphs = null;
        TextExtent? sourceExtent = null;
        AdvTextSourceLayout? source = !history
                                      && (run.Presented != null || run.SourceCursor != null
                                          || !ReferenceEquals(text, _sourceText))
            ? LayoutSourceText(binding, run, out sourceGlyphs, out sourceExtent)
            : null;

        GlyphTextLayoutOptions LayoutOptions(int right, bool wrapRows, IReadOnlyList<int>? lineBreaks,
                                             IReadOnlyList<int>? rowShifts)
            => new(run.Layout.CursorX, run.Layout.CursorY, binding.ResetCursorX, right,
                   history ? int.MaxValue : run.Layout.Bottom, WrapHorizontally: wrapRows, style, lineBreaks, realization,
                   RowShiftsX: rowShifts);

        PresentedRunPlacement? placement = null;
        if (run.Presented is { } presented && recordedLineBreaks == null)
        {
            // Presented text breaks where the presenter allows (§7.1), planned before drawing, and is narrowed and
            // aligned as its entry asks (§7.2); history has no source pass and draws it as written. A language that
            // shrinks overflow tries each width with the layout that draws it, without drawing (§7.3).
            Func<PresentedRunPlacement, bool>? fitsWindow =
                presented.Overflow == Ome.Engine.Localization.TextOverflow.Shrink && !history
                    ? candidate => !text.Layout.Render(
                        null, LayoutOptions(int.MaxValue, false, candidate.Plan.BreaksBefore, candidate.RowShiftsX),
                        candidate.Plan.Glyphs).StoppedOnVerticalOverflow
                    : null;
            placement = PresentedTextPlacement.Place(
                drawnText, Ome.Engine.Localization.PresentedLineBreaks.Opportunities(drawnText, presented.LineBreak), requests,
                text.Layout.Room(style, text.Policy, _textAntialias), run.Layout.CursorX, binding.ResetCursorX,
                run.Layout.Right,
                presented.Entry?.Align ?? Ome.Engine.Localization.TextAlign.Start,
                presented.Entry?.Fit ?? Ome.Engine.Localization.TextFit.None, sourceExtent, source?.Rows.Count ?? 0,
                presented.MinFitScale, fitsWindow);
            requests = placement.Plan.Glyphs;
            breaks = placement.Plan.BreaksBefore;
            rightBound = int.MaxValue;
            wrap = false;
        }
        GlyphTextLayoutResult rendered = text.Layout.Render(
            updated, LayoutOptions(rightBound, wrap, breaks, placement?.RowShiftsX), requests);
        // Presented rows below the layout's bottom are cut and reported (§7.3); history has no bottom.
        int cutRows = placement != null && !history && rendered.StoppedOnVerticalOverflow
            ? PresentedTextPlacement.CutRows(placement.Plan, rendered.ConsumedGlyphs)
            : 0;

        // Op 0x196's ruby follows each base row in AGE's drawing order; History draws its ruby runs after the text.
        IReadOnlyList<AdvRubyGlyph> ruby = [];
        var ordered = new List<AdvRetainedGlyphRecord>(rendered.Records.Count);
        var orderedIsRuby = new List<bool>(rendered.Records.Count);
        // Each record's row in the run (AGE 4.43B's line images count the rows the published glyphs reach).
        var orderedRows = new List<int>(rendered.Records.Count);
        void Add(AdvRetainedGlyphRecord record, bool isRuby, int row)
        {
            ordered.Add(record);
            orderedIsRuby.Add(isRuby);
            orderedRows.Add(row);
        }
        int[] rowOf = new int[rendered.Records.Count];
        for (int row = 0; row < rendered.Rows.Count; row++)
            for (int glyph = rendered.Rows[row].FirstGlyph;
                 glyph < rendered.Rows[row].FirstGlyph + rendered.Rows[row].GlyphCount && glyph < rowOf.Length;
                 glyph++)
                rowOf[glyph] = row;
        if (!string.IsNullOrEmpty(run.Ruby) && rendered.Rows.Count > 0)
        {
            ruby = text.Layout.RenderRuby(
                updated, run.Text, rendered.Rows, run.Ruby, run.Style,
                text.Policy, _textAntialias, LiveRubyRules);
            // AGE's drawing order (OrderedGlyphs): each row's base glyphs, then that row's ruby.
            for (int row = 0; row < rendered.Rows.Count; row++)
            {
                AdvTextRow drawn = rendered.Rows[row];
                for (int glyph = drawn.FirstGlyph; glyph < drawn.FirstGlyph + drawn.GlyphCount; glyph++)
                    Add(rendered.Records[glyph], false, row);
                foreach (AdvRubyGlyph character in ruby)
                    if (character.Row == row) Add(character.Record, true, row);
            }
        }
        else
        {
            for (int index = 0; index < rendered.Records.Count; index++) Add(rendered.Records[index], false, rowOf[index]);
            foreach (AdvTextRubyAnnotation annotation in historyRuby ?? [])
            {
                int firstBase = annotation.BaseFirstGlyph;
                int lastBase = firstBase + annotation.BaseGlyphCount - 1;
                if (annotation.BaseGlyphCount <= 0 || lastBase >= rendered.Records.Count) continue;
                AdvRetainedGlyphRecord left = rendered.Records[firstBase], right = rendered.Records[lastBase];
                foreach (AdvRetainedGlyphRecord record in text.Layout.RenderRubyOver(
                             updated, left.Left, left.Top, right.Right - left.Left, annotation.Text, run.Style,
                             text.Policy, _textAntialias, LiveRubyRules, history: true))
                    Add(record, true, rowOf[firstBase]);
            }
        }

        if (!history)
        {
            if (source == null) _runGeometry = PageGeometry(run, rendered, text, realization);
            _runCut = rendered.StoppedOnVerticalOverflow;
        }

        _timeline?.Event("adv-text-layout-result", new()
        {
            ["layout"] = binding.LayoutSlot,
            ["text"] = run.Text,
            ["presented"] = run.Presented?.Text,
            ["backend"] = text.Info.Id,
            ["cursor_x"] = run.Layout.CursorX,
            ["cursor_y"] = run.Layout.CursorY,
            ["right"] = run.Layout.Right,
            ["bottom"] = run.Layout.Bottom,
            ["font"] = run.Style.FontFace,
            ["font_size"] = run.Style.PrimaryFontSize,
            ["line_spacing"] = run.Style.LineSpacing,
            ["requested_glyphs"] = requests.Count,
            ["rendered_glyphs"] = rendered.ConsumedGlyphs,
            ["ruby"] = run.Ruby ?? "",
            ["ruby_glyphs"] = orderedIsRuby.Count(isRuby => isRuby),
            ["wrapped_lines"] = rendered.WrappedLines,
            ["vertical_overflow"] = rendered.StoppedOnVerticalOverflow,
            ["result_cursor_x"] = rendered.CursorX,
            ["result_cursor_y"] = rendered.CursorY,
            ["source_cursor_x"] = source?.CursorX,
            ["source_cursor_y"] = source?.CursorY,
            ["width_percent"] = placement?.WidthPercent,
            ["narrowed_to_fit"] = placement?.NarrowedToFit,
            ["row_shifts"] = placement?.RowShiftsX is { } shifts ? string.Join(",", shifts) : null,
            ["cut_rows"] = cutRows,
        });

        if (!_scene.RetainedTextLayouts.TryGetValue(
                binding.LayoutSlot, out presentation!))
        {
            presentation = new RetainedAdvTextLayoutPresentation(binding);
            _scene.RetainedTextLayouts.Add(binding.LayoutSlot, presentation);
        }
        else if (presentation.Binding != binding)
            throw new InvalidOperationException(
                $"ADV layout {binding.LayoutSlot} changed its retained binding without reset.");
        // The rows count from the layout's reset; History's groups are not counted (its renderer draws no line images
        // without op 0x1d1's flag 0x20, which no supported game passes).
        int firstRow = history ? 0 : LayoutRow(binding.LayoutSlot);
        int first = presentation.AppendRun(
            ordered,
            orderedIsRuby,
            run.Layout.OriginX,
            run.Layout.OriginY,
            _rubyRules.ChainEveryRubyRecord,
            sourceGlyphs,
            history ? null : orderedRows.Select(row => firstRow + row).ToArray());
        if (!history && rendered.Rows.Count > 0)
            _scene.LayoutRows[binding.LayoutSlot] =
                firstRow + rendered.Rows.Count - 1 + (rendered.Rows[^1].WrapsAfter ? 1 : 0);
        _scene.LayoutCursors[binding.LayoutSlot] = (rendered.CursorX, rendered.CursorY);
        RebuildRetainedWaitIndicator(binding.LayoutSlot);
        result = new AdvRetainedTextRunResult(
            binding.LayoutSlot,
            first,
            ordered.Count,
            rendered.CursorX,
            rendered.CursorY)
        {
            Rows = rendered.Rows, Ruby = ruby, Source = source, CutRows = cutRows,
            NarrowedToFitPercent = placement is { NarrowedToFit: true } ? placement.WidthPercent : 0,
        };

        _graphics.PublishSurfacePixels(binding.SourceSurfaceSlot, updated);
        gfx.CreateSurface(binding.SourceSurfaceSlot);
    }

    /// <summary>Ops 0x7a, 0x6f and 0x71 move a layout's pending record; slot 0 is the current layout. The
    /// follow-last-glyph wait indicator anchors there.</summary>
    public void SetAdvTextCursor(int layoutSlot, int x, int y)
    {
        int slot = layoutSlot != 0 ? layoutSlot : History.CurrentLayoutSlot;
        _scene.LayoutCursors[slot] = (x, y);
        RebuildRetainedWaitIndicator(slot);
        _timeline?.Event("text-cursor", new() { ["slot"] = layoutSlot, ["x"] = x, ["y"] = y });
        foreach (ITextListener listener in _listeners) listener.CursorMoved(layoutSlot, x, y);
    }

    /// <summary>Op 0x204/0x205 (adv_text_draw_string_to_surface): rasterize <paramref name="text"/> into a numbered
    /// surface before op 0x1fb binds it. A string the presenter replaced draws <paramref name="presented"/>'s text, a
    /// "\n" in it starting a row at the same x (localization spec §5.2, §7.1).</summary>
    public void DrawStringToSurface(int surfaceSlot, int x, int y, string text, AdvTextStyle style,
                                    Ome.Engine.Localization.PresentedText? presented = null)
    {
        foreach (ITextListener listener in _listeners) listener.StringDrawn(surfaceSlot, x, y, text, style, presented);
        if (_sourceText == null) return;
        TextPipeline pipeline = presented != null ? _presentedText!.Value : ActiveText;
        string? placed = DrawStringPixels(surfaceSlot, x, y, text, style, pipeline, presented);
        _timeline?.Event("draw-string", new()
        {
            ["surface"] = surfaceSlot,
            ["x"] = x,
            ["y"] = y,
            ["text"] = text,
            ["presented"] = presented?.Text,
            ["placement"] = placed,
            ["presentation"] = "rgba-glyph-mask",
            ["backend"] = pipeline.Info.Id,
        });
    }

    /// <summary>The gap between the rows of a presented <c>draw-string</c> when its entry sets no <c>row_gap</c>
    /// (localization spec §7.1).</summary>
    private const int PresentedRowGap = 2;

    /// <returns>For an entry that aligns or fits, how its rows were placed (§7.2): the source string's extent, the
    /// glyph width in percent and each row's shift; else null.</returns>
    private string? DrawStringPixels(
        int surfaceSlot, int x, int y, string text, AdvTextStyle style, TextPipeline pipeline,
        Ome.Engine.Localization.PresentedText? presented)
    {
        RgbaImage? destination = _graphics.SurfacePixels(surfaceSlot);
        if (destination == null
            && _graphics.TryGetSurfaceSize(surfaceSlot, out var dimensions)
            && dimensions.W > 0
            && dimensions.H > 0)
            destination = new RgbaImage(
                dimensions.W, dimensions.H,
                new byte[checked(dimensions.W * dimensions.H * 4)]);
        if (destination == null || destination.Width <= 0 || destination.Height <= 0)
            throw new InvalidOperationException(
                $"Surface {surfaceSlot} has no rasterizable pixel allocation.");

        var updated = new RgbaImage(
            destination.Width, destination.Height, (byte[])destination.Pixels.Clone());
        string? placed = null;
        if (presented == null)
            pipeline.Pixels.Render(updated, x, y, text, style, _textAntialias, pipeline.Realization, _blankExtentMode);
        else
        {
            // Each row draws as its source string would (localization spec §7): the same style, aspect mode and rules. A
            // "\n" of the presented text starts a row at the same x, lower by the font's height and a gap (§7.1).
            AspectFontRealization? realization = pipeline.Realization;
            int height = style.PrimaryFontSize > 0 ? style.PrimaryFontSize : 24;
            int step = height + (presented.Entry?.RowGap ?? PresentedRowGap);
            string[] rows = presented.Text.Split('\n');
            ImmediateSurfaceTextResult Draw(RgbaImage? into, int at, int top, string row, int percent)
                => pipeline.Pixels.Render(into, at, top, row, style, _textAntialias, realization, _blankExtentMode, percent);
            // An entry's align and fit place its rows against what the source string would have covered (§7.2).
            Ome.Engine.Localization.TextAlign align = presented.Entry?.Align ?? Ome.Engine.Localization.TextAlign.Start;
            bool fit = presented.Entry?.Fit == Ome.Engine.Localization.TextFit.Shrink;
            TextExtent? source = align != Ome.Engine.Localization.TextAlign.Start || fit
                ? _sourceText!.Pixels.Render(null, x, y, text, style, _textAntialias, _sourceText!.Realization,
                                             _blankExtentMode).Ink
                : null;
            int percent = fit && source is { } fitted
                ? PresentedTextPlacement.FitPercent(
                    at => rows.Max(row => Draw(null, 0, y, row, at).Ink?.Width ?? 0), fitted.Width, presented.MinFitScale)
                : 100;
            var shifts = new int[rows.Length];
            for (int row = 0; row < rows.Length; row++)
            {
                if (rows[row].Length == 0) continue;
                shifts[row] = source is { } target && align != Ome.Engine.Localization.TextAlign.Start
                              && Draw(null, x, y, rows[row], percent).Ink is { } extent
                    ? PresentedTextPlacement.Shift(align, target, extent, 0, updated.Width)
                    : 0;
                Draw(updated, checked(x + shifts[row]), checked(y + row * step), rows[row], percent);
            }
            if (source is { } placedAgainst)
                placed = $"source {placedAgainst.Left}..{placedAgainst.Right}, width {percent}%, shifts {string.Join(",", shifts)}";
        }

        _graphics.PublishSurfacePixels(surfaceSlot, updated, updateSize: false);
        _graphics.RequestPresentation();
        return placed;
    }

    /// <summary>Op 0x1fb's release of a layout's live text (no retained presentation): the run being revealed there
    /// stops.</summary>
    public void ClearRenderedAdvTextLayout(int layoutSlot)
    {
        int slot = layoutSlot == 0 ? History.CurrentLayoutSlot : layoutSlot;
        if (!_scene.RetainedTextLayouts.ContainsKey(slot) && _scene.ActiveRunLayout == slot)
        {
            _scene.ActiveRunLayout = 0;
            _scene.AdvTextForceComplete = false;
            IsRevealing = false;
        }
        foreach (ITextListener listener in _listeners) listener.LayoutCleared(layoutSlot);
    }

    /// <summary>Op 0x71: a layout's retained glyphs and History binding go, and its surface is cleared. Its line images
    /// (AGE 4.43B) are erased and its rows count from 1 again.</summary>
    public void ResetRenderedAdvTextLayout(GfxState gfx, AdvTextLayoutPresentationBinding binding)
    {
        _scene.LayoutRows.Remove(binding.LayoutSlot);
        EraseLineImages(gfx, History.GetLineImages(binding.LayoutSlot));
        _scene.RetainedHistoryLayouts.Remove(binding.LayoutSlot);
        _scene.RetainedTextLayouts.Remove(binding.LayoutSlot);
        if (_scene.ActiveRunLayout == binding.LayoutSlot)
        {
            _scene.ActiveRunLayout = 0;
            _scene.AdvTextForceComplete = false;
            IsRevealing = false;
        }
        if (_scene.SuspendedRetainedTextLayoutSlot == binding.LayoutSlot)
            _scene.SuspendedRetainedTextLayoutSlot = 0;
        foreach (ITextListener listener in _listeners) listener.LayoutCleared(binding.LayoutSlot);
        if (_sourceText == null) return;

        if (_graphics.TryGetSurfaceSize(
                binding.SourceSurfaceSlot, out var dimensions)
            && dimensions.W > 0
            && dimensions.H > 0)
        {
            _graphics.PublishSurfacePixels(
                binding.SourceSurfaceSlot,
                new RgbaImage(dimensions.W, dimensions.H, new byte[checked(dimensions.W * dimensions.H * 4)]),
                updateSize: false);
        }
        _graphics.RequestPresentation();
        _timeline?.Event("adv-text-layout-reset", new()
        {
            ["layout"] = binding.LayoutSlot,
            ["surface"] = binding.SourceSurfaceSlot,
            ["first_handle"] = binding.FirstObjectHandle,
            ["capacity"] = binding.ObjectCapacity,
        });
    }

    /// <summary>Op 0x1d1: draw one History group into its target layout and bind it. False without devices. Public for
    /// the Godot self-test, which draws a batch it builds; the op goes through <see cref="RenderHistoryGroup"/>.</summary>
    public bool RenderTextHistory(
        GfxState gfx,
        AdvTextLayoutPresentationBinding binding,
        AdvTextHistoryRenderBatch batch)
    {
        foreach (ITextListener listener in _listeners) listener.HistoryGroupDrawn(binding, batch);
        if (_sourceText == null) return false;
        // A presented group (localization spec §8) wraps in the target layout as live presented text does.
        var run = new AdvLiveTextRun(
            batch.FirstRecordIndex,
            batch.Layout,
            batch.Style,
            batch.Text,
            Array.Empty<string>()) { Presented = batch.Presented };
        if (binding.LayoutSlot != batch.LayoutSlot
            || binding.FirstObjectHandle < 0
            || binding.ObjectCapacity <= 0)
            throw new InvalidOperationException(
                $"History layout {batch.LayoutSlot} has no retained presentation binding.");
        PrepareRetainedTextRun(
            gfx, binding, run,
            out RetainedAdvTextLayoutPresentation? presentation,
            out AdvRetainedTextRunResult result,
            batch.Presented == null ? batch.RecordedLineBreaks : null,
            batch.Presented == null ? batch.Ruby : null,
            history: true);

        presentation.PublishThrough(
            gfx,
            checked(result.FirstGlyphIndex + result.GlyphCount),
            HistoryPublication(batch.Style));
        // text_history_render_records appends each row it draws to the window's live records, which the settings
        // redraw draws again; a presented group's rows are not the source's, so it appends none.
        if (batch.Presented == null)
            History.AppendLiveRows(batch.LayoutSlot, batch.Text, batch.Style, result.Rows, result.Ruby);
        _scene.RetainedHistoryLayouts.Add(batch.LayoutSlot);
        _graphics.RequestPresentation();
        _timeline?.Event("history-render", new()
        {
            ["layout"] = batch.LayoutSlot,
            ["record"] = batch.FirstRecordIndex,
            ["x"] = batch.Layout.OriginX + batch.Layout.CursorX,
            ["y"] = batch.Layout.OriginY + batch.Layout.CursorY,
            ["text"] = batch.Text,
            ["presented"] = batch.Presented?.Text,
            ["glyphs"] = result.GlyphCount,
            ["presentation"] = "retained-glyphs",
        });
        return true;
    }

    /// <summary>
    /// Op 0x82 (text_redraw_retained_group@0x451530; SYS4433 @0x4553c0): release the layout's objects, clear its surface
    /// and draw each record at its rectangle, writing each text record's drawn right and bottom back
    /// (<see cref="RedrawnRecords"/>). Unless flag 4 is set, it binds one object per text record over its rectangle, as
    /// native does, handles counting up from the layout's first. A group presented again in the selected language
    /// (localization spec §8) has no records to its glyphs, so its published glyphs are bound again instead. The glyph
    /// list and the reveal index stay, so the wait indicator keeps its anchor and later text continues the page. This is
    /// not a History presentation, which History's exit would erase.
    /// </summary>
    private void RedrawRetainedTextGroup(
        GfxState gfx,
        AdvTextLayoutPresentationBinding binding,
        AdvTextRedrawBatch batch)
    {
        foreach (ITextListener listener in _listeners) listener.GroupRedrawn(binding, batch);
        if (_sourceText == null) return;
        if (binding.LayoutSlot != batch.LayoutSlot
            || binding.FirstObjectHandle < 0
            || binding.ObjectCapacity <= 0)
            return;
        (int width, int height) = _graphics.TryGetSurfaceSize(binding.SourceSurfaceSlot, out var dimensions)
                                  && dimensions.W > 0 && dimensions.H > 0
            ? (dimensions.W, dimensions.H)
            : (batch.Layout.Width, batch.Layout.Height);
        if (width <= 0 || height <= 0) return;

        var updated = new RgbaImage(width, height, new byte[checked(width * height * 4)]);
        TextPipeline text = batch.Presented != null ? _presentedText!.Value : ActiveText;
        List<int>? penRights = batch.Presented == null ? new List<int>(batch.Records.Count) : null;
        IReadOnlyList<(AdvRetainedGlyphRecord Cell, bool IsRuby)> cells = batch.Presented is { } presented
            ? RedrawPresented(updated, binding, batch, presented, text)
            : text.Layout.RenderRedraw(
                updated, batch.Records, text.Policy, _textAntialias, LiveRubyRules, text.Realization, penRights);
        IReadOnlyList<(AdvRetainedGlyphRecord Record, GlyphPublication Publication)>? redrawnRecords =
            penRights != null ? RedrawnRecords(batch, penRights) : null;

        bool bind = (batch.Flags & 4) == 0;
        RetainedAdvTextLayoutPresentation? presentation;
        // A layout with no live text yet takes the drawn cells as its glyphs.
        if (!_scene.RetainedTextLayouts.TryGetValue(binding.LayoutSlot, out presentation) && bind)
        {
            presentation = new RetainedAdvTextLayoutPresentation(binding);
            _scene.RetainedTextLayouts.Add(binding.LayoutSlot, presentation);
            presentation.AppendRun(
                cells.Select(cell => cell.Cell).ToArray(),
                cells.Select(cell => cell.IsRuby).ToArray(),
                batch.Layout.OriginX,
                batch.Layout.OriginY,
                _rubyRules.ChainEveryRubyRecord);
            RebuildRetainedWaitIndicator(binding.LayoutSlot);
        }

        _graphics.PublishSurfacePixels(binding.SourceSurfaceSlot, updated);
        gfx.CreateSurface(binding.SourceSurfaceSlot);
        gfx.EraseRange(binding.FirstObjectHandle, binding.ObjectCapacity);
        if (bind && presentation != null)
        {
            // 4.33G's, 4.38D's, 4.43B's and 4.45D's redraw binds each record as the layout publisher does
            // (LayoutPublication), by its own ruby bit; 4.24J's binds the cells as they are.
            GlyphPublications? redrawn = LayoutPublication;
            if (redrawnRecords != null)
            {
                presentation.RedrawnAsRecords(redrawn);
                int bound = (int)Math.Min(redrawnRecords.Count, binding.ObjectCapacity);
                for (int index = 0; index < bound; index++)
                    RetainedAdvTextLayoutPresentation.BindRecord(
                        gfx, checked(binding.FirstObjectHandle + index), binding.SourceSurfaceSlot,
                        redrawnRecords[index].Record, batch.Layout.OriginX, batch.Layout.OriginY,
                        redrawnRecords[index].Publication);
            }
            else if (presentation.PublishedGlyphCount == 0)
                presentation.PublishThrough(gfx, presentation.Glyphs.Count, redrawn ?? default);
            else if (redrawn is { } publication) presentation.RepublishEach(gfx, publication);
            else presentation.Republish(gfx);
        }
        _graphics.RequestPresentation();
        _timeline?.Event("text-redraw", new()
        {
            ["layout"] = batch.LayoutSlot,
            ["record"] = batch.FirstRecordIndex,
            ["flags"] = batch.Flags,
            ["records"] = batch.Records.Count,
            ["cells"] = cells.Count,
        });
    }

    /// <summary>
    /// What op 0x82 leaves in each text record of <paramref name="batch"/> and binds over it (text_redraw_retained_group:
    /// 4.33G @0x4553c0, 4.38D @0x460cd0, 4.43B @0x463310, 4.45D @0x463b40, all alike). The walk keeps a record's left and
    /// top and writes back its right, the pen after its last character (<paramref name="penRights"/>), and its bottom:
    /// the top plus the record's pixel height (+0x24, which the walk makes the primary size +0x313d4), plus the effect's
    /// y offset (+0x56c) when the text manager's render mode (+0x55c) is not 0 at that character and
    /// message:UseAntiFont (+0x548) is set. The mode is the one at entry until a ruby character sets it to 0, and a
    /// character that is not ruby puts it back, each after its bottom is taken; so a ruby record right after text gets
    /// the offset and the ruby records after it do not. A record with no text, a line break, keeps its bottom, and its
    /// right becomes its left. The history keeps what is written back (a later redraw, History and a save read it).
    /// Each record is then bound with what the layout publisher does to a record of its kind in the mode the walk
    /// left: a ruby record its raise, any other the rows under the cell, the paddings and the move.
    /// </summary>
    private List<(AdvRetainedGlyphRecord Record, GlyphPublication Publication)> RedrawnRecords(
        AdvTextRedrawBatch batch, IReadOnlyList<int> penRights)
    {
        int entryMode = Style.RenderMode;
        int mode = entryMode;
        var redrawn = new List<(AdvRetainedGlyphRecord, GlyphPublication)>(batch.Records.Count);
        for (int index = 0; index < batch.Records.Count; index++)
        {
            AdvTextRedrawRecord record = batch.Records[index];
            AdvTextHistoryRect rect = record.Rect;
            int characters = 0;
            foreach (System.Text.Rune _ in record.Text.EnumerateRunes()) characters++;
            if (characters > 0)
            {
                int modeAtLast = characters == 1 ? mode : record.IsRuby ? 0 : entryMode;
                int height = ImmediateSurfaceTextRenderer.NativePixelHeight(record.Style.PrimaryFontSize);
                int offset = modeAtLast != 0 && _textAntialias ? Style.EffectOffsetY : 0;
                rect = rect with { Right = penRights[index], Bottom = checked(rect.Top + height + offset) };
                mode = record.IsRuby ? 0 : entryMode;
            }
            else
                rect = rect with { Right = rect.Left };
            if (record.HistoryIndex >= 0) History.SetRedrawnRect(record.HistoryIndex, rect);
            GlyphPublication publication = !_publisherGrowsGlyphCells ? default
                : record.IsRuby ? RubyPublication
                : PrimaryPublication(Style with { RenderMode = mode }, extraModeThreeGrowth: !_layoutPublishersGrowOnce);
            redrawn.Add((new AdvRetainedGlyphRecord(record.IsRuby ? 1 : 0, rect.Left, rect.Top, rect.Right, rect.Bottom),
                         publication));
        }
        return redrawn;
    }

    /// <summary>Op 0x82 for a presented group (localization spec §8): the presented text from the first text record's
    /// top-left in its style, wrapped in the layout as live presented text is.</summary>
    private IReadOnlyList<(AdvRetainedGlyphRecord Cell, bool IsRuby)> RedrawPresented(
        RgbaImage destination, AdvTextLayoutPresentationBinding binding, AdvTextRedrawBatch batch,
        Ome.Engine.Localization.PresentedText presented, TextPipeline text)
    {
        AdvTextRedrawRecord first = batch.Records.First(record => !record.IsRuby && record.Text.Length > 0);
        AdvTextStyle style = first.Style;
        PresentedTextPlan plan = PresentedTextLayout.Plan(
            presented.Text, Ome.Engine.Localization.PresentedLineBreaks.Opportunities(presented.Text, presented.LineBreak),
            Requests(presented.Text, style, text), text.Layout.Room(style, text.Policy, _textAntialias),
            first.Rect.Left, binding.ResetCursorX, batch.Layout.Right);
        GlyphTextLayoutResult drawn = text.Layout.Render(
            destination,
            new GlyphTextLayoutOptions(
                first.Rect.Left, first.Rect.Top, binding.ResetCursorX, int.MaxValue, batch.Layout.Bottom,
                WrapHorizontally: false, style, plan.BreaksBefore, text.Realization),
            plan.Glyphs);
        return drawn.Records.Select(record => (record, false)).ToArray();
    }

    /// <summary>
    /// The settings redraw (adv_text_redraw_live_records: 4.33G @0x4596f0, 4.38D @0x463630, 4.43B @0x466100, 4.45D
    /// @0x466a20; <see cref="LiveTextRedraw"/>), which text_set_use_anti_font runs as message:UseAntiFont changes and
    /// display_toggle_screen_mode_and_menu_bar after a switch while display:VirtualFullScreenType is 2: each layout from
    /// the build's first that has a surface is drawn again from its live records
    /// (<see cref="AdvTextHistory.GetLiveRecords"/>, <see cref="RedrawLiveLayout"/>); 4.33G's skips one with none.
    /// </summary>
    private void RedrawLiveText()
    {
        if (_liveTextRedraw == LiveTextRedraw.None || _sourceText == null) return;
        GfxState gfx = _graphics.State;
        bool redrawn = false;
        for (int slot = _liveTextRedrawFirstLayout; slot < AdvTextHistory.LayoutCount; slot++)
            redrawn |= RedrawLiveLayout(gfx, slot);
        if (redrawn) _graphics.RequestPresentation();
    }

    /// <summary>
    /// One layout of the settings redraw: its surface cleared and its live records drawn again at their rectangles in the
    /// current setting (<see cref="LiveRedrawRecords"/>), and, while its first object shows its surface, its objects
    /// erased and bound again. Native counts the records' characters against the layout's published glyphs (window
    /// record <c>+0x84</c>, else <c>+0x88</c>): a record whose first character is published is bound to the pen after
    /// its last published one (<see cref="LiveRedrawPublication"/>), a ruby record per character, so a page mid-reveal
    /// stays so. Nothing is written back, and the glyph list, the reveal and the cursors stay. A group History presents
    /// in the selected language (localization spec §8) is drawn as op 0x82 draws one, and its published glyphs are bound
    /// again.
    /// </summary>
    private bool RedrawLiveLayout(GfxState gfx, int slot)
    {
        // Native takes each window whose surface (0x14 on) exists: the loop reads the graphics native's surface table
        // (gfx+0xa5c8, so +0xa618 for window 0), not the window's records.
        if (!_graphics.TryGetSurfaceSize(checked(slot + 0x14), out var dimensions)
            || dimensions.W <= 0 || dimensions.H <= 0)
            return false;
        IReadOnlyList<AdvTextLiveRecord> live = History.GetLiveRecords(slot);
        if (live.Count == 0 && !_liveTextRedrawClearsEmptyLayouts) return false;
        // A layout History has no state for has nothing to bind: its binding is not created for the asking.
        AdvTextLayoutPresentationBinding binding = History.HasLayout(slot)
            ? History.GetLayoutObjectBinding(slot)
            : new AdvTextLayoutPresentationBinding(slot, checked(slot + 0x14), -1, 0, -1, 0, 0);

        IReadOnlyList<AdvTextRedrawRecord> records = LiveRedrawRecords(live);
        var updated = new RgbaImage(dimensions.W, dimensions.H, new byte[checked(dimensions.W * dimensions.H * 4)]);
        _scene.RetainedTextLayouts.TryGetValue(slot, out RetainedAdvTextLayoutPresentation? presentation);
        // gfx_object_query_bound_source_slot: native binds nothing unless the first object shows the layout's surface.
        int published = presentation != null && binding.FirstObjectHandle >= 0 && binding.ObjectCapacity > 0
                        && gfx.QuerySlot(binding.FirstObjectHandle, unboundReadsAbsent: true) == binding.SourceSurfaceSlot
            ? presentation.PublishedGlyphCount
            : 0;

        int bound = 0;
        if (LivePresentation(live, records) is { } presented && _presentedText is { } presentedText)
        {
            AdvTextRedrawBatch batch = new(slot, live[0].HistoryIndex, 0, History.GetLayoutObjectSnapshot(slot), records)
            {
                Presented = presented,
            };
            RedrawPresented(updated, binding, batch, presented, presentedText.Value);
            _graphics.PublishSurfacePixels(binding.SourceSurfaceSlot, updated);
            gfx.CreateSurface(binding.SourceSurfaceSlot);
            if (published > 0 && presentation != null)
            {
                gfx.EraseRange(binding.FirstObjectHandle, binding.ObjectCapacity);
                if (LayoutPublication is { } publication) presentation.RepublishEach(gfx, publication);
                else presentation.Republish(gfx);
                bound = presentation.PublishedGlyphCount;
            }
        }
        else
        {
            TextPipeline text = ActiveText;
            var shown = new List<(AdvRetainedGlyphRecord Record, GlyphPublication Publication)>();
            int drawn = 0;
            var pen = new List<int>(1);
            foreach (AdvTextRedrawRecord record in records)
            {
                pen.Clear();
                IReadOnlyList<(AdvRetainedGlyphRecord Cell, bool IsRuby)> cells = text.Layout.RenderRedraw(
                    updated, [record], text.Policy, _textAntialias, LiveRubyRules, text.Realization, pen);
                int characters = 0;
                foreach (System.Text.Rune _ in record.Text.EnumerateRunes()) characters++;
                AdvTextHistoryRect rect = record.Rect;
                if (record.IsRuby)
                {
                    // One object per character, each over its own cell.
                    for (int index = 0; index < characters; index++, drawn++)
                    {
                        if (drawn >= published) continue;
                        int right = index + 1 < cells.Count ? cells[index + 1].Cell.Left : pen[0];
                        int left = index < cells.Count ? cells[index].Cell.Left : rect.Left;
                        shown.Add((new AdvRetainedGlyphRecord(1, left, rect.Top, right, rect.Bottom), RubyPublication));
                    }
                    continue;
                }
                if (drawn < published)
                {
                    int count = Math.Min(characters, published - drawn);
                    int right = count >= characters ? pen[0] : count < cells.Count ? cells[count].Cell.Left : pen[0];
                    shown.Add((new AdvRetainedGlyphRecord(0, rect.Left, rect.Top, right, rect.Bottom),
                               LiveRedrawPublication(record.Style)));
                }
                drawn += characters;
            }
            _graphics.PublishSurfacePixels(binding.SourceSurfaceSlot, updated);
            gfx.CreateSurface(binding.SourceSurfaceSlot);
            if (published > 0)
            {
                gfx.EraseRange(binding.FirstObjectHandle, binding.ObjectCapacity);
                bound = (int)Math.Min(shown.Count, binding.ObjectCapacity);
                AdvTextLayoutSnapshot layout = History.GetLayoutObjectSnapshot(slot);
                for (int index = 0; index < bound; index++)
                    RetainedAdvTextLayoutPresentation.BindRecord(
                        gfx, checked(binding.FirstObjectHandle + index), binding.SourceSurfaceSlot, shown[index].Record,
                        layout.OriginX, layout.OriginY, shown[index].Publication);
            }
        }
        _timeline?.Event("text-live-redraw", new()
        {
            ["layout"] = slot,
            ["records"] = records.Count,
            ["published"] = published,
            ["bound"] = bound,
            ["antialias"] = _textAntialias,
        });
        return true;
    }

    /// <summary>
    /// The text records the settings redraw draws, walked as op 0x82 walks a group: a navigation-filtered record ends
    /// the walk, so does the next group's start, and metadata and voice records draw nothing. A text record draws in the
    /// window's style, which is the last text record's that is not ruby (<see cref="LiveTextRedraw.WindowStyle"/>), or in
    /// its own (<see cref="LiveTextRedraw.RecordStyle"/>). A ruby record draws in the ruby font at its own size in mode
    /// 0, with the colors of the window, or of the text record before it: 4.38D's redraw sets the ruby font once, from
    /// the window.
    /// </summary>
    private IReadOnlyList<AdvTextRedrawRecord> LiveRedrawRecords(IReadOnlyList<AdvTextLiveRecord> live)
    {
        AdvTextStyle? window = null;
        foreach (AdvTextLiveRecord entry in live)
            if (entry.Record.Kind == AdvTextHistoryRecordKind.Text
                && !entry.Record.Flags.HasFlag(AdvTextHistoryRecordFlags.Ruby))
                window = entry.Record.Style;

        var records = new List<AdvTextRedrawRecord>();
        AdvTextStyle? before = window;
        for (int index = 0; index < live.Count; index++)
        {
            AdvTextHistoryRecord record = live[index].Record;
            if (record.Flags.HasFlag(AdvTextHistoryRecordFlags.NavigationFiltered)) break;
            if (record.Kind == AdvTextHistoryRecordKind.Text)
            {
                bool ruby = record.Flags.HasFlag(AdvTextHistoryRecordFlags.Ruby);
                AdvTextStyle style;
                if (!ruby)
                {
                    style = _liveTextRedraw == LiveTextRedraw.WindowStyle ? window ?? record.Style : record.Style;
                    before = style;
                }
                else
                    style = (before ?? record.Style) with
                    {
                        PrimaryFontSize = record.Style.PrimaryFontSize,
                        RenderMode = 0,
                    };
                records.Add(new AdvTextRedrawRecord(record.Text, record.Rect, style, ruby));
            }
            if (index + 1 < live.Count && live[index + 1].Record.Flags.HasFlag(AdvTextHistoryRecordFlags.GroupStart))
                break;
        }
        return records;
    }

    /// <summary>The layout's live text presented in the selected language (localization spec §8), from History's
    /// lookups of the group its first live record starts (<see cref="PresentHistoryGroup"/>); null when nothing is
    /// presented, or History did not record the records.</summary>
    private Ome.Engine.Localization.PresentedText? LivePresentation(
        IReadOnlyList<AdvTextLiveRecord> live, IReadOnlyList<AdvTextRedrawRecord> records)
    {
        if (_presenter == null || !records.Any(record => !record.IsRuby && record.Text.Length > 0)) return null;
        int first = live[0].HistoryIndex;
        if ((uint)first >= (uint)History.Records.Count || History.Records[first].Text != live[0].Record.Text)
            return null;
        return PresentHistoryGroup(first, 0);
    }

    /// <summary>The settings redraw's bind of a record that is not ruby (4.45D adv_text_redraw_live_records@0x466a20 at
    /// 0x467e4e; 4.38D's @0x463630 alike): the rectangle grown in an effect mode by the effect's x offset at the right
    /// and twice its y offset under it, or by op 0x260's paddings while op 0x261's bit 0 is set, and moved back by the
    /// offsets in mode 3 while message:UseAntiFont is 0.</summary>
    private GlyphPublication LiveRedrawPublication(AdvTextStyle style)
    {
        (int shiftX, int shiftY) = AliasedEffectShiftOf(style);
        if (PaddingsOn) return PaddedPublication(shiftX, shiftY);
        if (style.RenderMode == 0) return new GlyphPublication(0, shiftX, shiftY);
        return new GlyphPublication(checked(style.EffectOffsetY * 2), shiftX, shiftY) { GrowRight = style.EffectOffsetX };
    }

    /// <summary>HISTORY.BIN's recording re-enable at exit (op 0x1bb 1) ends its presentation: every layout op 0x1d1
    /// bound is erased and its surface cleared.</summary>
    public void EndTextHistoryPresentation(GfxState gfx)
    {
        RetainedAdvTextLayoutPresentation[] retained;
        int[] surfaceSlots;
        retained = _scene.RetainedHistoryLayouts
            .Select(slot => _scene.RetainedTextLayouts.GetValueOrDefault(slot))
            .Where(presentation => presentation != null)
            .Cast<RetainedAdvTextLayoutPresentation>()
            .ToArray();
        surfaceSlots = retained
            .Select(presentation => presentation.Binding.SourceSurfaceSlot)
            .Distinct()
            .ToArray();
        foreach (int slot in _scene.RetainedHistoryLayouts)
            _scene.RetainedTextLayouts.Remove(slot);
        _scene.RetainedHistoryLayouts.Clear();
        foreach (ITextListener listener in _listeners) listener.HistoryPresentationEnded();
        if (_sourceText == null) return;

        foreach (RetainedAdvTextLayoutPresentation presentation in retained)
            presentation.ErasePublished(gfx);
        foreach (int surfaceSlot in surfaceSlots)
            ClearAllocatedSurfacePixels(surfaceSlot);
        _graphics.RequestPresentation();
        _timeline?.Event("history-presentation-end", new()
        {
            ["retained_layouts"] = retained.Length,
        });
    }

    private void ClearAllocatedSurfacePixels(int surfaceSlot)
    {
        if (!_graphics.TryGetSurfaceSize(surfaceSlot, out var dimensions)
            || dimensions.W <= 0
            || dimensions.H <= 0)
            return;
        _graphics.PublishSurfacePixels(
            surfaceSlot,
            new RgbaImage(dimensions.W, dimensions.H, new byte[checked(dimensions.W * dimensions.H * 4)]),
            updateSize: false);
    }
}
