using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Text;
using Ome.Engine.Vm;
using Ome.Engine.Hosting;
using Ome.Engine.Devices;
using Ome.Engine.Persistence;

/// <summary>
/// The settings redraw (docs/engine-re.md, "The settings redraw"): from 4.33G, a change of message:UseAntiFont, and a
/// screen-mode switch while display:VirtualFullScreenType is 2, draw every window's live text again
/// (adv_text_redraw_live_records). 天秤のLa DEA。's page 3 (tools/drive/scenarios/ladea-antifont-redraw.steps, 2026-11-27):
/// native's text turns aliased at once and back, and the port's text window now matches it pixel for pixel.
/// </summary>
public class LiveTextRedrawTests
{
    private const int Immediate = 0, StringOperand = 2;
    private static Operand I(long value) => new(Immediate, value);
    private static Operand S(int index) => new(StringOperand, index);

    [Fact]
    public void EachBuildRedrawsAsItsNativeDoes()
    {
        Assert.Equal(LiveTextRedraw.None, EngineBuilds.Age415D.Rules.Text.LiveTextRedraw);
        Assert.Equal(LiveTextRedraw.None, EngineBuilds.Age424J.Rules.Text.LiveTextRedraw);
        Assert.Equal(LiveTextRedraw.None, EngineBuilds.Age424N.Rules.Text.LiveTextRedraw);
        static (LiveTextRedraw, int, bool) Of(EngineBuild build)
            => (build.Rules.Text.LiveTextRedraw, build.Rules.Text.LiveTextRedrawFirstLayout,
                build.Rules.Text.LiveTextRedrawClearsEmptyLayouts);
        Assert.Equal((LiveTextRedraw.WindowStyle, 1, false), Of(EngineBuilds.Age433G));   // @0x4596f0
        Assert.Equal((LiveTextRedraw.RecordStyle, 0, true), Of(EngineBuilds.Age438D));    // @0x463630
        Assert.Equal((LiveTextRedraw.RecordStyle, 0, true), Of(EngineBuilds.Age443B));    // @0x466100
        Assert.Equal((LiveTextRedraw.WindowStyle, 0, true), Of(EngineBuilds.Age445D));    // @0x466a20
    }

    [Fact]
    public void ALayoutKeepsWhatItDrewSinceItsResetWhetherHistoryRecordsOrNot()
    {
        var history = new AdvTextHistory();
        history.DefineLayout(1, 640, 160, 80, 430);
        history.AppendText(1, 0x100, "gone", AdvTextStyle.Default);
        history.ResetLayout(1);
        history.AppendText(1, 0x200, "kept", AdvTextStyle.Default);
        history.SetRecordingEnabled(false);
        history.AppendText(1, 0x300, "unrecorded", AdvTextStyle.Default);
        history.SetRecordingEnabled(true);
        history.AppendLiveRows(1, "history", AdvTextStyle.Default, [new AdvTextRow(0, 7, 0, 0, 70, 16, false)], null);

        AdvTextLiveRecord[] live = history.GetLiveRecords(1).ToArray();
        Assert.Equal(new[] { "kept", "unrecorded", "history" }, live.Select(entry => entry.Record.Text));
        int kept = history.Records.ToList().FindIndex(record => record.Text == "kept");
        Assert.Equal(new[] { kept, -1, -1 }, live.Select(entry => entry.HistoryIndex));
        // History's own list has neither the text it did not record nor what its renderer drew.
        Assert.DoesNotContain(history.Records, record => record.Text is "unrecorded" or "history");

        history.ClearLiveRecords();
        Assert.Empty(history.GetLiveRecords(1));
    }

    /// <summary>A text wait that runs <see cref="AtWait"/> as it starts.</summary>
    private sealed class WaitHost : RecordingWaits
    {
        public Action? AtWait;

        public override void WaitForInput(Func<bool> serviceInputCallback)
        {
            AtWait?.Invoke();
            AtWait = null;
            base.WaitForInput(serviceInputCallback);
        }
    }

    /// <summary>Two runs on layout 1, the first red and the second blue, shown at once; then <paramref name="then"/>, or
    /// op 0x71 on the layout when <paramref name="reset"/> is set.</summary>
    private static (VirtualMachine Vm, Graphics Graphics, TextManager Text) Shown(
        TextRules rules, bool reset = false, (int, Operand[])[]? then = null, RecordingWaits? waits = null,
        ShellServices? shell = null, IScriptProvider? provider = null, Action<Graphics>? graphicsReady = null)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
        then ??= reset ? [(0x71, [I(1)])] : [];   // reset-adv-text-layout
        var script = ScriptAssembler.Assemble(table, "SC0000",
        [
            (0x70, [I(1), I(400), I(160), I(30), I(40)]),     // define-adv-text-layout
            (0x213, [I(1), I(100), I(64)]),                   // the layout's glyph handles
            (0x75, [I(16)]),                                  // font size
            (0x1b5, [I(0)]),                                  // instant text
            (0x76, [I(0xff0000)]),
            (0x6e, [I(0), S(0)]),                             // あい
            (0x76, [I(0x0000ff)]),
            (0x6e, [I(0), S(1)]),                             // う
            .. then,
            (0x2, Array.Empty<Operand>()),
        ], ["あい", "う"]);
        var clock = new Ome.Engine.Hosting.FrameClock();
        var skip = SkipAndAuto.WithSystemClock();
        var graphics = new Graphics(null, clock, new Sys4LogicalCanvas(0, 0));
        graphicsReady?.Invoke(graphics);
        var text = new TextManager(graphics, skip, clock, TestText.Glyphs(TestText.Square()), RubyLayoutRules.FixedOffsets,
            publisherGrowsGlyphCells: rules.PublisherGrowsGlyphCells,
            publisherMovesAliasedEffectGlyphs: rules.PublisherMovesAliasedEffectGlyphs,
            layoutPublishersGrowOnce: rules.LayoutPublishersGrowOnce, dashStrip: rules.DashStrip,
            liveTextRedraw: rules.LiveTextRedraw, liveTextRedrawFirstLayout: rules.LiveTextRedrawFirstLayout,
            liveTextRedrawClearsEmptyLayouts: rules.LiveTextRedrawClearsEmptyLayouts);
        var vm = new VirtualMachine(script, table, waits ?? new RecordingWaits(), new VmOptions(), provider,
                                    build: EngineBuilds.Age445D, skipAndAuto: skip, graphics: graphics, text: text,
                                    shell: shell);
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        return (vm, graphics, text);
    }

    private static AdvTextHistoryRecord[] Texts(VirtualMachine vm)
        => vm.TextHistory.Records.Where(record => record.Kind == AdvTextHistoryRecordKind.Text).ToArray();

    private static (byte R, byte G, byte B) PixelAt(Graphics graphics, int slot, int x, int y)
    {
        RgbaImage image = Assert.IsType<RgbaImage>(graphics.SurfacePixels(slot));
        int at = (y * image.Width + x) * 4;
        return (image.Pixels[at], image.Pixels[at + 1], image.Pixels[at + 2]);
    }

    [Theory]
    [InlineData(false, 0x00, 0x00, 0xff)]   // 4.45D (@0x466a20): every record in the window's style, the last one's
    [InlineData(true, 0xff, 0x00, 0x00)]    // 4.43B (@0x466100): each record in its own
    public void AUseAntiFontChangeDrawsTheWindowsTextAgainInTheBuildsStyle(bool recordStyle, int r, int g, int b)
    {
        TextRules rules = recordStyle ? EngineBuilds.Age443B.Rules.Text : EngineBuilds.Age445D.Rules.Text;
        (VirtualMachine vm, Graphics graphics, TextManager text) = Shown(rules);
        AdvTextHistoryRecord[] records = Texts(vm);
        Assert.Equal(new[] { "あい", "う" }, records.Select(record => record.Text));
        int surface = vm.TextHistory.GetLayoutObjectBinding(1).SourceSurfaceSlot;
        AdvTextHistoryRect first = records[0].Rect;
        RgbaImage before = Assert.IsType<RgbaImage>(graphics.SurfacePixels(surface));
        Assert.Equal((0xff, 0x00, 0x00), PixelAt(graphics, surface, first.Left + 4, first.Top + 8));

        text.SetTextAntialias(false);

        Assert.NotSame(before, graphics.SurfacePixels(surface));
        Assert.Equal(((byte)r, (byte)g, (byte)b), PixelAt(graphics, surface, first.Left + 4, first.Top + 8));
        AdvTextHistoryRect second = records[1].Rect;
        Assert.Equal((0x00, 0x00, 0xff), PixelAt(graphics, surface, second.Left + 4, second.Top + 8));
        // One object per record again, over the record from its left to the pen after its last character: in mode 0
        // nothing grows.
        for (int index = 0; index < records.Length; index++)
        {
            AdvTextHistoryRect rect = records[index].Rect;
            GfxState.GfxObject bound = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(100 + index));
            Assert.Equal((rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top), bound.SrcRect);
            Assert.Equal((30.0 + rect.Left, 40.0 + rect.Top), (bound.V24.X, bound.V24.Y));
        }
        Assert.Null(vm.Gfx.TryGet(100 + records.Length));
        // Nothing is written back.
        Assert.Equal(records, Texts(vm));
    }

    [Fact]
    public void WithoutItsFirstObjectOnTheSurfaceTheRedrawBindsNothing()
    {
        // adv_text_redraw_live_records binds only while the window's first object shows its surface
        // (gfx_object_query_bound_source_slot); it draws the surface all the same.
        (VirtualMachine vm, Graphics graphics, TextManager text) = Shown(EngineBuilds.Age445D.Rules.Text);
        int surface = vm.TextHistory.GetLayoutObjectBinding(1).SourceSurfaceSlot;
        vm.Gfx.EraseRange(100, 64);
        RgbaImage before = Assert.IsType<RgbaImage>(graphics.SurfacePixels(surface));

        text.SetTextAntialias(false);

        Assert.NotSame(before, graphics.SurfacePixels(surface));
        Assert.Null(vm.Gfx.TryGet(100));
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(0, false)]
    public void AScreenModeSwitchDrawsTheTextAgainWhileVirtualFullScreenTypeIsTwo(int virtualFullScreenType, bool redraws)
    {
        // display_toggle_screen_mode_and_menu_bar (4.45D @0x413ac0) runs the redraw after a switch only while
        // display:VirtualFullScreenType is 2; the key is 0 when absent.
        string root = Path.Combine(Path.GetTempPath(), "ome-live-redraw-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string path = Path.Combine(root, Sys4RegIniStore.FileName);
            File.WriteAllText(path, $"[display]\r\nVirtualFullScreenType={virtualFullScreenType}\r\n");
            var shell = new ShellServices(new Sys4RegIniStore(path, 0, settingsRules: EngineBuilds.Age445D.Rules.Settings));
            var waits = new WaitHost();
            shell.WakeWaitsWith(((IInputAndWaits)waits).WakeInputCallbackService);
            OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
            var provider = new MapProvider(new Dictionary<long, Script>(),
                new Dictionary<string, Script>(StringComparer.OrdinalIgnoreCase)
                {
                    ["CALLBACK_WINDOW.BIN"] = ScriptAssembler.Assemble(table, "CALLBACK_WINDOW",
                        [(0x2, Array.Empty<Operand>())], []),
                });
            RgbaImage? before = null;
            Graphics? drawn = null;
            int surface = 0x15;   // layout 1's
            waits.AtWait = () =>
            {
                before = drawn!.SurfacePixels(surface);
                Assert.True(shell.SwitchScreenMode(windowed: false, "the test"));
            };
            var shown = Shown(EngineBuilds.Age445D.Rules.Text, then: [(0x72, [I(0)])], waits: waits, shell: shell,
                              provider: provider, graphicsReady: graphics => drawn = graphics);

            Assert.NotNull(before);
            if (redraws) Assert.NotSame(before, shown.Graphics.SurfacePixels(surface));
            else Assert.Same(before, shown.Graphics.SurfacePixels(surface));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ABuildBefore433GLeavesTheTextAsItWasDrawn()
    {
        // 4.24J's text_set_antialias_enabled@0x4137e0 only empties the glyph caches.
        (VirtualMachine vm, Graphics graphics, TextManager text) = Shown(EngineBuilds.Age424J.Rules.Text);
        int surface = vm.TextHistory.GetLayoutObjectBinding(1).SourceSurfaceSlot;
        RgbaImage before = Assert.IsType<RgbaImage>(graphics.SurfacePixels(surface));

        text.SetTextAntialias(false);

        Assert.Same(before, graphics.SurfacePixels(surface));
    }

    [Theory]
    [InlineData(true)]    // 4.38D's and 4.45D's loops clear each defined window first
    [InlineData(false)]   // 4.33G's skips a window without records
    public void AfterALayoutResetTheRedrawDrawsNothingThere(bool clearsEmpty)
    {
        TextRules rules = EngineBuilds.Age445D.Rules.Text with { LiveTextRedrawClearsEmptyLayouts = clearsEmpty };
        (VirtualMachine vm, Graphics graphics, TextManager text) = Shown(rules, reset: true);
        Assert.Empty(vm.TextHistory.GetLiveRecords(1));
        int surface = vm.TextHistory.GetLayoutObjectBinding(1).SourceSurfaceSlot;
        RgbaImage? before = graphics.SurfacePixels(surface);

        text.SetTextAntialias(false);

        RgbaImage? after = graphics.SurfacePixels(surface);
        if (!clearsEmpty) Assert.Same(before, after);
        else Assert.All(Assert.IsType<RgbaImage>(after).Pixels, value => Assert.Equal(0, value));
        Assert.Null(vm.Gfx.TryGet(100));
    }
}
