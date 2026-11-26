using System;
using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// AGE 4.43B's text additions (docs/engine-re.md, "Arterial against 魔導巧殻, triaged"): line images under a layout's
/// rows (ops 0x25c to 0x25f), glyph paddings (ops 0x260 and 0x261), the glyph delay without its setting (op 0x74) and
/// MessageFade (op 0x2ee), each run through the VM as 魔導巧殻's SYSTEM4 and scenes run it.
/// </summary>
public class Age443BTextOpsTests
{
    private const int Exit = 0x2, ShowText = 0x6e, EndLine = 0x6f, DefineLayout = 0x70, ResetLayout = 0x71;
    private const int GlyphDelayOnly = 0x74, FontSize = 0x75, LineSpacing = 0x8b, MessageSpeed = 0x1b5;
    private const int PublishLayout = 0x20a, LayoutHandles = 0x213;
    private const int LineImages = 0x25c, LineImageHandles = 0x25d, LineImagesFade = 0x25e, LineImagesColor = 0x25f;
    private const int Paddings = 0x260, PaddingFlags = 0x261, MessageFade = 0x2ee;

    private static Operand I(long value) => new(0, value);

    /// <summary>message:MessageFade is a setting of 4.43B alone: its text native reads it at startup and writes it back,
    /// after MessageSpeed, and an older build's ignores the key whatever the file holds.</summary>
    [Fact]
    public void MessageFadeIsReadAndWrittenWhereTheBuildBindsIt()
    {
        string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ome-message-fade-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);
        try
        {
            static Ome.Engine.Natives.TextManager Text(Ome.Engine.Persistence.Sys4RegIniStore store)
            {
                var clock = new FrameClock();
                return new Ome.Engine.Natives.TextManager(new Ome.Engine.Natives.Graphics(null, clock, new Sys4LogicalCanvas(0, 0)),
                    Ome.Engine.Natives.SkipAndAuto.WithSystemClock(), clock, TestText.Fonts([]),
                    Ome.Engine.Text.RubyLayoutRules.FixedOffsets, settings: store);
            }
            foreach ((EngineBuild build, int read) in new[] { (EngineBuilds.Age443B, 250), (EngineBuilds.Age438D, 0) })
            {
                string path = System.IO.Path.Combine(directory, build.Id, Ome.Engine.Persistence.Sys4RegIniStore.FileName);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                System.IO.File.WriteAllText(path, "[message]\r\nMessageFade=250\r\n");
                var store = new Ome.Engine.Persistence.Sys4RegIniStore(path, 0, settingsRules: build.Rules.Settings);
                Ome.Engine.Natives.TextManager text = Text(store);
                Assert.Equal(read, text.MessageFade);

                text.SetMessageFade(120);
                text.WriteSettings(store);
                string[] lines = System.IO.File.ReadAllLines(path);
                if (build == EngineBuilds.Age443B)
                    Assert.Equal("MessageFade=120", lines[Array.FindIndex(lines, line => line.StartsWith("MessageSpeed=")) + 1]);
                else
                    Assert.DoesNotContain("MessageFade=120", lines);
            }
        }
        finally
        {
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }
    private static Operand S(int index) => new(2, index);

    private static VirtualMachine Run(IEnumerable<(int, Operand[])> instructions, params string[] strings)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4443");
        var ops = new List<(int, Operand[])>(instructions) { (Exit, []) };
        var vm = TestText.Vm(ScriptAssembler.Assemble(table, "SC0000", ops, strings), table, new RecordingWaits(),
                             TestText.Glyphs(TestText.Fixed(20, 10)), EngineBuilds.Age443B);
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        return vm;
    }

    // Layout 1 at (30, 40), its glyphs at handles 100 on, 16-pixel glyphs 4 pixels apart, revealed at once; then
    // SYSTEM4.BIN@0xb7's line images: three under layout 1's rows, 5 right of its origin and 18 below each row's top.
    private static List<(int, Operand[])> WithLineImages() =>
    [
        (DefineLayout, [I(1), I(400), I(160), I(30), I(40)]),
        (LayoutHandles, [I(1), I(100), I(64)]),
        (FontSize, [I(16)]),
        (LineSpacing, [I(4)]),
        (MessageSpeed, [I(0)]),
        (LineImages, [I(1), I(5), I(18), I(17), I(0), I(249), I(911), I(1)]),
        (LineImageHandles, [I(1), I(500), I(3)]),
        (LineImagesColor, [I(1), I(0), I(0), I(0xffffff)]),
        (LineImagesFade, [I(0), I(0), I(300), I(0x80), I(0xffffff)]),   // layout 0: the current one, 1
        (ShowText, [I(0), S(0)]),
        (EndLine, [I(0)]),
        (ShowText, [I(0), S(1)]),
    ];

    [Fact]
    public void TheTimedPublisherBindsALineImageUnderEachRowItsGlyphsReachAndFadesItIn()
    {
        // adv_text_publish_next_glyph@0x459440: image i at the origin plus the offsets, i row pitches (16 + 4) lower,
        // in op 0x25f's colour, then op 0x25e's animation. Two rows of text reach two of the three images.
        VirtualMachine vm = Run(WithLineImages(), "あい", "う");

        GfxState.GfxObject first = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(500));
        GfxState.GfxObject second = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(501));
        Assert.Null(vm.Gfx.TryGet(502));
        Assert.Equal((35.0, 58.0), (first.V24.X, first.V24.Y));
        Assert.Equal((35.0, 78.0), (second.V24.X, second.V24.Y));
        Assert.Equal((17, (0, 249, 911, 1)), (first.SourceSlot, first.SrcRect));
        Assert.All(new[] { first, second }, image =>
        {
            Assert.Equal(0x00ffffffL, image.Color);
            Assert.Equal((true, 0x80ffffffL, 0L, 300L),
                         (image.OneShotColorEnabled, image.OneShotColorTarget, image.ColorDelayMs, image.ColorDurationMs));
        });
    }

    [Fact]
    public void TheLayoutPublisherBindsTheLineImagesAgainInTheAnimationsEndColour()
    {
        // adv_text_publish_layout@0x4583e0 erases them, binds them again in +0x110 and starts no animation.
        VirtualMachine vm = Run([.. WithLineImages(), (PublishLayout, [I(1)])], "あい", "う");

        GfxState.GfxObject first = Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(500));
        Assert.Equal((0x80ffffffL, false), (first.Color, first.OneShotColorEnabled));
        Assert.NotNull(vm.Gfx.TryGet(501));
        Assert.Null(vm.Gfx.TryGet(502));
    }

    [Fact]
    public void ALayoutResetErasesItsLineImagesAndCountsRowsFromOneAgain()
    {
        // adv_text_layout_reset@0x45c1d0 erases [+0x114, +0x118) and sets the row count to 1: one row after it binds
        // the first image alone.
        VirtualMachine vm = Run([.. WithLineImages(), (ResetLayout, [I(1)])], "あい", "う");
        Assert.All(new long[] { 500, 501, 502 }, handle => Assert.Null(vm.Gfx.TryGet(handle)));

        vm = Run([.. WithLineImages(), (ResetLayout, [I(1)]), (ShowText, [I(0), S(0)])], "あい", "う");
        Assert.NotNull(vm.Gfx.TryGet(500));
        Assert.Null(vm.Gfx.TryGet(501));
    }

    [Fact]
    public void WithoutOp25cNoLineImageIsBound()
    {
        VirtualMachine vm = Run(WithLineImages().Where(op => op.Item1 != LineImages), "あい", "う");
        Assert.All(new long[] { 500, 501, 502 }, handle => Assert.Null(vm.Gfx.TryGet(handle)));
    }

    // One glyph of layout 1, with the paddings op 0x260 sets turned on or off by op 0x261.
    private static GfxState.GfxObject PaddedGlyph(int flags)
    {
        VirtualMachine vm = Run(
            [
                (DefineLayout, [I(1), I(400), I(160), I(30), I(40)]),
                (LayoutHandles, [I(1), I(100), I(64)]),
                (FontSize, [I(16)]),
                (MessageSpeed, [I(0)]),
                (Paddings, [I(2), I(3), I(4), I(5)]),
                (PaddingFlags, [I(flags)]),
                (ShowText, [I(0), S(0)]),
            ],
            "あ");
        return Assert.IsType<GfxState.GfxObject>(vm.Gfx.TryGet(100));
    }

    [Fact]
    public void WithItsFlagThePaddingsGrowAGlyphsCellOnEverySideAndMoveItBack()
    {
        // adv_text_publish_next_glyph@0x459440: left - L, top - T, right + R, bottom + B; the object moves by -(L, T).
        GfxState.GfxObject plain = PaddedGlyph(0), padded = PaddedGlyph(1);

        (int x, int y, int w, int h) = plain.SrcRect;
        Assert.Equal((x - 2, y - 4, w + 2 + 3, h + 4 + 5), padded.SrcRect);
        Assert.Equal((plain.V24.X - 2, plain.V24.Y - 4), (padded.V24.X, padded.V24.Y));
    }

    [Fact]
    public void Op74SetsTheGlyphDelayAndLeavesTheSettingOp1b5Writes()
    {
        // op_0x74_set_message_glyph_delay@0x41e5c0 writes +0x560 alone; 魔導巧殻 makes it 0 for a screen of instant text.
        VirtualMachine vm = Run([(MessageSpeed, [I(40)]), (GlyphDelayOnly, [I(0)]), (MessageFade, [I(250)])]);

        Assert.Equal((0, 40), (vm.Text.MessageGlyphDelayMilliseconds, vm.Text.MessageSpeedSetting));
        Assert.Equal(40, vm.MessageSettings.MessageSpeed);
        Assert.Equal(250, vm.Text.MessageFade);
    }
}
