using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;

public class TextRedrawOpsTests
{
    private const int Immediate = 0, GlobalInt = 3;
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);
    private static Operand I(long value) => new(Immediate, value);
    private static Operand G(int address) => new(GlobalInt, address);

    [Fact]
    public void QueryThenRedrawRestoresTheNewestRetainedAdvGroup()
    {
        var history = new AdvTextHistory();
        history.DefineLayout(1, 640, 160, 80, 430);
        history.AppendText(1, 0x100, "older", AdvTextStyle.Default);
        history.ResetLayout(1);
        history.AppendText(1, 0x200, "restored", AdvTextStyle.Default with
        {
            TextColor = 0x112233,
            EffectColor = 0x445566,
        });

        var script = ScriptAssembler.Assemble(Table, "CONFIG_REDRAW",
        [
            (0x83, [G(0x10), G(0x11), G(0x12)]),
            (0x82, [G(0x11), G(0x12), I(2), I(0xffffff), I(0x123456)]),
            (0x2, Array.Empty<Operand>()),
        ], []);
        var host = new RecordingWaits();
        var vm = new VirtualMachine(script, Table, host, textHistory: history);

        vm.Run();

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(0, vm.Globals[0x10]);
        Assert.Equal(1, vm.Globals[0x11]);
        Assert.Equal(1, vm.Globals[0x12]);
        // A redraw is not a History presentation, which History's exit would erase.
        Assert.Empty(host.HistoryRenders);
        var (binding, redraw) = Assert.Single(host.Redraws);
        Assert.Equal((1, 1, 1, 2), (binding.LayoutSlot, redraw.LayoutSlot, redraw.FirstRecordIndex, redraw.Flags));
        AdvTextRedrawRecord record = Assert.Single(redraw.Records);
        Assert.Equal(("restored", 0xffffffL, 0x123456L),
            (record.Text, record.Style.TextColor, record.Style.EffectColor));
    }

    [Fact]
    public void ARedrawKeepsEachRecordsColorsAndHeightInTheCurrentStyle()
    {
        // text_redraw_retained_group@0x4553c0 restores each record's colors and pixel height; face, weight, effect and
        // render mode are the text manager's current ones, and a ruby record draws in mode 0.
        var history = new AdvTextHistory();
        history.DefineLayout(1, 640, 160, 80, 430);
        AdvTextStyle recorded = AdvTextStyle.Default with
        {
            PrimaryFontSize = 24, TextColor = 0x112233, EffectColor = 0x445566, RenderMode = 3, FontFace = "old",
        };
        history.AppendText(1, 0, "first", recorded,
            rows: [new AdvTextRow(0, 5, 0, 0, 120, 24, WrapsAfter: true)],
            ruby: [new AdvRubyGlyph(0, "る", new AdvRetainedGlyphRecord(1, 3, -8, 11, 0))]);
        history.AppendText(1, 0, "filtered", recorded, AdvTextHistoryRecordFlags.NavigationFiltered);
        AdvTextStyle current = AdvTextStyle.Default with
        {
            PrimaryFontSize = 20, TextColor = 0xffffff, EffectColor = 0, RenderMode = 1, FontFace = "new", Bold = true,
        };

        Assert.True(history.TryBuildRedrawBatch(1, 0, 0, 0, 0, current, out AdvTextRedrawBatch batch));

        // The line break draws nothing but is in the batch, since native binds an object for it too; the
        // navigation-filtered record ends the walk without flag 1.
        Assert.Equal(
            new[]
            {
                ("first", false, 24, 0x112233L, 0x445566L, 1, "new", true, new AdvTextHistoryRect(0, 0, 120, 24)),
                ("る", true, 8, 0x112233L, 0x445566L, 0, "new", true, new AdvTextHistoryRect(3, -8, 11, 0)),
                ("", false, 24, 0x112233L, 0x445566L, 1, "new", true, new AdvTextHistoryRect(0, 0, 0, 0)),
            },
            batch.Records.Select(record => (record.Text, record.IsRuby, record.Style.PrimaryFontSize,
                record.Style.TextColor, record.Style.EffectColor, record.Style.RenderMode, record.Style.FontFace,
                record.Style.Bold, record.Rect)));

        Assert.True(history.TryBuildRedrawBatch(1, 0, 1, 0, 0, current, out AdvTextRedrawBatch all));
        Assert.Equal(new[] { "first", "る", "", "filtered" }, all.Records.Select(record => record.Text));
    }

    [Fact]
    public void ARedrawBindsOneObjectPerRecordAndWritesItsRightAndBottomBack()
    {
        // text_redraw_retained_group (4.45D @0x463b40, as 4.33G's, 4.38D's and 4.43B's): each text record's bottom is its
        // top plus its pixel height, plus the effect's y offset while the render mode at that character is not 0 and
        // message:UseAntiFont is set; a ruby character sets the mode to 0 and one that is not ruby puts the entry mode
        // back. Then one object per text record, a line break included, handles from the layout's first, each over the
        // record grown by the paddings. 天秤のLa DEA。's page 4 (2026-11-26): native's save holds seven objects for its
        // seven records, the first ruby record one pixel taller than the next.
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4445");
        Operand S(int index) => new(2, index);
        var script = ScriptAssembler.Assemble(table, "SC0000",
        [
            (0x70, [I(1), I(400), I(160), I(30), I(40)]),     // define-adv-text-layout
            (0x213, [I(1), I(100), I(64)]),                   // the layout's glyph handles
            (0x75, [I(16)]),                                  // font size
            (0x197, [I(8)]),                                  // ruby font size
            (0x78, [I(3)]),                                   // render mode 3
            (0x1a4, [I(1), I(1)]),                            // effect offset
            (0x1b5, [I(0)]),                                  // instant text
            (0x260, [I(2), I(0), I(2), I(2)]),                // paddings: left, right, top, bottom
            (0x261, [I(1)]),
            (0x6e, [I(0), S(0)]),                             // あい
            (0x196, [I(0), S(1), S(2)]),                      // うえ, ruby かな
            (0x6e, [I(0), S(3)]),                             // お
            (0x6f, [I(0)]),                                   // line break
            (0x6e, [I(0), S(4)]),                             // き
            (0x83, [G(0x10), G(0x11), G(0x12)]),
            (0x82, [G(0x11), G(0x12), I(2), I(0xffffff), I(0)]),
            (0x2, Array.Empty<Operand>()),
        ], ["あい", "うえ", "かな", "お", "き"]);
        // The text native as the port builds it for 4.45D (godot/Main.cs): its publishers grow cells.
        var clock = new Ome.Engine.Hosting.FrameClock();
        var skip = Ome.Engine.Natives.SkipAndAuto.WithSystemClock();
        var graphics = new Ome.Engine.Natives.Graphics(null, clock, new Sys4LogicalCanvas(0, 0));
        Ome.Engine.Text.TextRules rules = EngineBuilds.Age445D.Rules.Text;
        var text = new Ome.Engine.Natives.TextManager(graphics, skip, clock, TestText.Glyphs(TestText.Square()),
            Ome.Engine.Text.RubyLayoutRules.FixedOffsets, publisherGrowsGlyphCells: rules.PublisherGrowsGlyphCells,
            publisherMovesAliasedEffectGlyphs: rules.PublisherMovesAliasedEffectGlyphs,
            layoutPublishersGrowOnce: rules.LayoutPublishersGrowOnce);
        var vm = new VirtualMachine(script, table, new RecordingWaits(), build: EngineBuilds.Age445D, skipAndAuto: skip,
                                    graphics: graphics, text: text);
        AdvTextHistoryRecord[] Texts() => vm.TextHistory.Records
            .Where(record => record.Kind == AdvTextHistoryRecordKind.Text).ToArray();

        vm.Run();

        Assert.Equal("exit", vm.HaltReason);
        AdvTextHistoryRecord[] records = Texts();
        Assert.Equal(new[] { "あい", "うえ", "か", "な", "お", "", "き" }, records.Select(record => record.Text));
        int top = records[0].Rect.Top, rubyTop = records[2].Rect.Top, second = records[6].Rect.Top;
        Assert.Equal(
            new[]
            {
                (records[0].Rect.Left + 32, top + 17),      // two characters: the entry mode at the last
                (records[1].Rect.Left + 32, top + 17),
                (records[2].Rect.Left + 8, rubyTop + 9),    // ruby right after text: still mode 3
                (records[3].Rect.Left + 8, rubyTop + 8),    // after a ruby character: mode 0
                (records[4].Rect.Left + 16, top + 16),      // one character after the ruby: mode 0
                (records[5].Rect.Left, records[5].Rect.Bottom),
                (records[6].Rect.Left + 16, second + 17),
            },
            records.Select(record => (record.Rect.Right, record.Rect.Bottom)));

        for (int index = 0; index < records.Length; index++)
        {
            AdvTextHistoryRect rect = records[index].Rect;
            bool ruby = records[index].Flags.HasFlag(AdvTextHistoryRecordFlags.Ruby);
            (int grow, int down) = ruby ? (0, 0) : (2, 2);
            GfxState.GfxObject bound = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(100 + index));
            Assert.Equal((rect.Left - grow, rect.Top - grow, rect.Right - rect.Left + grow, rect.Bottom - rect.Top + grow + down),
                         bound.SrcRect);
            Assert.Equal((30.0 + rect.Left - grow, 40.0 + rect.Top - grow), (bound.V24.X, bound.V24.Y));
        }
        Assert.Null(vm.Gfx.TryGet(100 + records.Length));
    }

    [Fact]
    public void QueryWithoutRetainedEntriesReturnsNativeSentinels()
    {
        var script = ScriptAssembler.Assemble(Table, "EMPTY_REDRAW",
        [
            (0x83, [G(0x10), G(0x11), G(0x12)]),
            (0x2, Array.Empty<Operand>()),
        ], []);
        var vm = new VirtualMachine(script, Table, new RecordingWaits());

        vm.Run();

        Assert.Equal(0, vm.Globals[0x10]);
        Assert.Equal(-1, vm.Globals[0x11]);
        Assert.Equal(-1, vm.Globals[0x12]);
    }
}
