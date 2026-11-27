using System.Collections.Generic;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Text;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// History's renderer binds one object per record, not per glyph (text_history_render_records: 4.24J @0x4526c0, 4.45D
/// @0x464fa0, its bind at 0x4661fb to 0x4664d5; docs/engine-re.md, "History binds one object per record"). With op
/// 0x260's paddings on, per-glyph objects would overlap their neighbours by the left and right paddings and draw those
/// columns twice.
/// </summary>
public class HistoryRecordObjectsTests
{
    private const int Exit = 0x2, ShowText = 0x6e, EndLine = 0x6f, DefineLayout = 0x70, FontSize = 0x75;
    private const int SetCursor = 0x7a, MessageSpeed = 0x1b5, RenderHistory = 0x1d1, LayoutHandles = 0x213;
    private const int Paddings = 0x260, PaddingFlags = 0x261;

    private static Operand I(long value) => new(0, value);
    private static Operand S(int index) => new(2, index);

    // "あいう", a line break and "え" shown in layout 1, then that group drawn by op 0x1d1 into layout 4 at (10, 6),
    // its objects from handle 300; paddings 2, 3, 4, 5 on when the build has them and paddingFlags is 1.
    private static VirtualMachine RenderGroup(string revision, EngineBuild build, int? paddingFlags)
    {
        var ops = new List<(int, Operand[])>
        {
            (DefineLayout, [I(1), I(400), I(160), I(30), I(40)]),
            (LayoutHandles, [I(1), I(100), I(64)]),
            (FontSize, [I(16)]),
            (MessageSpeed, [I(0)]),
            (ShowText, [I(0), S(0)]),
            (EndLine, [I(0)]),
            (ShowText, [I(0), S(1)]),
            (DefineLayout, [I(4), I(600), I(150), I(50), I(300)]),
            (LayoutHandles, [I(4), I(300), I(64)]),
            (SetCursor, [I(4), I(10), I(6)]),
        };
        if (paddingFlags is { } flags)
        {
            ops.Add((Paddings, [I(2), I(3), I(4), I(5)]));
            ops.Add((PaddingFlags, [I(flags)]));
        }
        ops.Add((RenderHistory, [I(4), I(0), I(0), I(0), I(0)]));
        ops.Add((Exit, []));
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, revision);
        VirtualMachine vm = TestText.Vm(ScriptAssembler.Assemble(table, "SC0000", ops, ["あいう", "え"]), table,
                                        new RecordingWaits(), TestText.Glyphs(TestText.Fixed(20, 10)), build);
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        return vm;
    }

    public static TheoryData<string, string> Builds => new()
    {
        { "SYS4422", nameof(EngineBuilds.Age424J) },
        { "SYS4443", nameof(EngineBuilds.Age443B) },
        { "SYS4445", nameof(EngineBuilds.Age445D) },
    };

    private static EngineBuild Build(string id)
        => id switch
        {
            nameof(EngineBuilds.Age424J) => EngineBuilds.Age424J,
            nameof(EngineBuilds.Age443B) => EngineBuilds.Age443B,
            _ => EngineBuilds.Age445D,
        };

    [Theory]
    [MemberData(nameof(Builds))]
    public void EachRecordIsOneObjectAndALineBreaksDrawsNothing(string revision, string build)
    {
        VirtualMachine vm = RenderGroup(revision, Build(build), paddingFlags: null);

        // "あいう" spans its three glyphs; the line break's object is empty (its row starts below the bottom the record
        // before it reached), so it draws nothing and is left unbound; "え" is the third object.
        GfxState.GfxObject first = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(300));
        GfxState.GfxObject third = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(302));
        Assert.Null(vm.Gfx.TryGet(301));
        Assert.Null(vm.Gfx.TryGet(303));
        Assert.Equal((0x18, 0x18), (first.SourceSlot, third.SourceSlot));
        Assert.Equal((10, 6, 60), (first.SrcRect.X, first.SrcRect.Y, first.SrcRect.W));
        Assert.Equal(20, third.SrcRect.W);
        Assert.True(third.SrcRect.Y > first.SrcRect.Y + first.SrcRect.H - 1);
        Assert.Equal((60.0, 306.0), (first.V24.X, first.V24.Y));
    }

    [Theory]
    [InlineData("SYS4443", nameof(EngineBuilds.Age443B))]
    [InlineData("SYS4445", nameof(EngineBuilds.Age445D))]
    public void ThePaddingsGrowEachRecordsObjectOnceNotEachGlyph(string revision, string build)
    {
        // The bind's padded case (4.45D 0x46633a): left - L, top - T, right + R, bottom + B, and the object moves back by
        // (L, T). So the record's three glyphs are one rectangle 65 wide, with no columns shown twice.
        GfxState.GfxObject plain = Assert.IsType<GfxState.GfxObject>(RenderGroup(revision, Build(build), 0).Gfx.TryGet(300));
        VirtualMachine vm = RenderGroup(revision, Build(build), 1);
        GfxState.GfxObject padded = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(300));

        (int x, int y, int w, int h) = plain.SrcRect;
        Assert.Equal((x - 2, y - 4, w + 2 + 3, h + 4 + 5), padded.SrcRect);
        Assert.Equal((plain.V24.X - 2, plain.V24.Y - 4), (padded.V24.X, padded.V24.Y));
        // The line break's empty rectangle runs from the next row's top (28: the first row's 6, plus 16 and a line
        // spacing of 6) to the bottom "あいう" reached (22). The paddings grow it to 5 by 3 at the start of the next row,
        // where "え"'s padded object starts too. Native binds it the same way; with a line spacing of the top and bottom
        // paddings or more (天秤のLa DEA。's History: 19 against 4) it stays empty.
        GfxState.GfxObject lineBreak = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(301));
        GfxState.GfxObject third = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(302));
        Assert.Equal((third.SrcRect.X, third.SrcRect.Y, 5, y + h + 5 - third.SrcRect.Y), lineBreak.SrcRect);
        Assert.Equal(3, lineBreak.SrcRect.H);
        Assert.Equal((third.V24.X, third.V24.Y), (lineBreak.V24.X, lineBreak.V24.Y));
        Assert.Null(vm.Gfx.TryGet(303));
    }

    [Fact]
    public void ARebindBindsTheRecordObjectsAgainNotTheGlyphs()
    {
        // A suspended page coming back binds its layout again (Republish): a History run's glyphs keep their objects.
        var gfx = new GfxState();
        var presentation = new RetainedAdvTextLayoutPresentation(
            new AdvTextLayoutPresentationBinding(4, 0x18, 300, 64, -1, 0, 0));
        int first = presentation.AppendRun(
            [
                new AdvRetainedGlyphRecord(0, 10, 6, 30, 22),
                new AdvRetainedGlyphRecord(0, 30, 6, 50, 22),
                new AdvRetainedGlyphRecord(1, 12, 0, 20, 8),
            ],
            [false, false, true], 50, 300, chainEveryRubyRecord: true);
        var padded = new GlyphPublication(5, 2, 4) { GrowLeft = 2, GrowTop = 4, GrowRight = 3 };
        presentation.PublishRecordObjects(
            gfx, first, [new AdvTextRecordObject(0, 2), new AdvTextRecordObject(2, 1)],
            new GlyphPublications(padded), 50, 300);

        void AssertBound()
        {
            GfxState.GfxObject record = Assert.IsType<GfxState.GfxObject>(gfx.TryGet(300));
            Assert.Equal((8, 2, 45, 25), record.SrcRect);
            Assert.Equal((58.0, 302.0), (record.V24.X, record.V24.Y));
            Assert.Equal((12, 0, 8, 8), Assert.IsType<GfxState.GfxObject>(gfx.TryGet(301)).SrcRect);
            Assert.Null(gfx.TryGet(302));
        }

        AssertBound();
        Assert.Equal(3, presentation.PublishedGlyphCount);
        presentation.ErasePublished(gfx);
        Assert.Null(gfx.TryGet(300));
        presentation.Republish(gfx);
        AssertBound();
    }
}
