using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ome.Engine.Model;
using Ome.Engine.Persistence;
using Ome.Engine.Sys4;
using Ome.Engine.Text;
using Ome.Engine.Text.Windows;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// AGE 4.38D's user-defined characters (docs/engine-re.md, "Kamidori against Arterial, triaged"): the font the
/// catalog ships, the face that draws Shift-JIS codes 0xF040 to 0xF9FC, and the join of the two dashes.
/// </summary>
public class ExtendedGlyphTests
{
    private const string Extended = "AGE Extend";

    /// <summary>Records every request, and answers with a one-pixel glyph whose ascent says which face drew it.</summary>
    private sealed class RecordingRasterizer(Func<GlyphRasterRequest, GlyphMask>? factory = null) : IGlyphMaskRasterizer
    {
        public List<GlyphRasterRequest> Requests { get; } = [];

        public GlyphMask Rasterize(GlyphRasterRequest request)
        {
            Requests.Add(request);
            return factory?.Invoke(request)
                   ?? new GlyphMask(1, 1, 1, 0, 1, 4, 0, 4, 5, [16], ascent: request.FontFace == Extended ? 30 : 19);
        }
    }

    private static GlyphRasterRequest Request(ushort cp932, int scalar, string face = "メイリオ")
        => new(face, 22, -18, 700, scalar, cp932, GlyphRasterPolicy.NativeCp932Gray4);

    // ---- the face ----

    [Theory]
    [InlineData(0xf040)]
    [InlineData(0xf042)]
    [InlineData(0xf9fc)]
    public void ACodeOfTheUserDefinedAreaIsDrawnWithTheExtendedFace(int cp932)
    {
        var inner = new RecordingRasterizer();
        var rasterizer = new ExtendedFaceGlyphRasterizer(inner, Extended);

        GlyphMask mask = rasterizer.Rasterize(Request((ushort)cp932, 0xe002) with { WidthPercent = 80 });

        // The primary font's height and weight, the uncalibrated width, and the text font's own ascent.
        GlyphRasterRequest drawn = inner.Requests[0];
        Assert.Equal((Extended, 22, -11, 700, 0xe002, (ushort?)cp932, 80),
            (drawn.FontFace, drawn.PixelHeight, drawn.RequestedWidth, drawn.Weight, drawn.UnicodeScalar,
             drawn.Cp932Code, drawn.WidthPercent));
        Assert.Equal("メイリオ", inner.Requests[1].FontFace);
        Assert.Equal(19, mask.Ascent);
    }

    [Fact]
    public void ARubyCharacterOfTheAreaIsDrawnInThePrimaryFontAndTakesTheRubyFontsCell()
    {
        // The extended font is made once, from the primary LOGFONT; the ruby layout measures with the ruby font.
        var inner = new RecordingRasterizer(request => request.FontFace == Extended
            ? new GlyphMask(1, 1, 1, 0, 1, 24, 0, 24, 22, [16], ascent: 30)
            : new GlyphMask(1, 1, 1, 0, 1, 9, 0, 9, 8, [16], ascent: 7));
        GlyphRasterRequest ruby = new GlyphRasterRequest(
            RubyTextLayout.FontFace, 8, -4, 0, 0xe002, 0xf042, GlyphRasterPolicy.NativeCp932Gray4)
        {
            ExtendedFont = new ExtendedGlyphFont(22, 700),
        };

        GlyphMask mask = new ExtendedFaceGlyphRasterizer(inner, Extended).Rasterize(ruby);

        GlyphRasterRequest drawn = inner.Requests[0];
        Assert.Equal((Extended, 22, -11, 700), (drawn.FontFace, drawn.PixelHeight, drawn.RequestedWidth, drawn.Weight));
        Assert.Equal((24, 9, 8, (int?)7), (mask.CellAdvanceX, mask.CellWidth, mask.CellHeight, mask.Ascent));
    }

    [Fact]
    public void ARubysRequestsNameThePrimaryFontOnlyForTheAreasCharacters()
    {
        AdvTextStyle style = AdvTextStyle.Default with { PrimaryFontSize = 22, Bold = true, RubyFontSize = 8 };

        IReadOnlyList<GlyphRasterRequest> requests = RubyTextLayout.CreateRequests(
            "あ", style, GlyphRasterPolicy.NativeCp932Gray4, antialias: true);

        Assert.Null(requests[0].ExtendedFont);
        Assert.Equal((ushort?)0xf042, requests[1].Cp932Code);
        Assert.Equal((8, 0), (requests[1].PixelHeight, requests[1].Weight));
        Assert.Equal(new ExtendedGlyphFont(22, 700), requests[1].ExtendedFont);
    }

    [Theory]
    [InlineData(0xf03f)]
    [InlineData(0xf9fd)]
    [InlineData(0x8c83)]
    [InlineData(0x41)]
    public void AnyOtherCodeKeepsTheTextsFace(int cp932)
    {
        var inner = new RecordingRasterizer();
        GlyphRasterRequest request = Request((ushort)cp932, 0x6fc0);

        new ExtendedFaceGlyphRasterizer(inner, Extended).Rasterize(request);

        Assert.Same(request, Assert.Single(inner.Requests));
    }

    [Fact]
    public void OnlyFourThirtyEightDNamesAnExtendedFace()
    {
        Assert.Null(EngineBuilds.Age424J.Rules.Text.ExtendedGlyphFace);
        Assert.Null(EngineBuilds.Age433G.Rules.Text.ExtendedGlyphFace);
        Assert.Equal(Extended, EngineBuilds.Age438D.Rules.Text.ExtendedGlyphFace);
    }

    [Fact]
    public void FourFortyFiveDTakesTheDashStripFromTheGlyphsBlackBox()
    {
        Assert.Equal(DashStripRows.Cell, EngineBuilds.Age438D.Rules.Text.DashStrip);
        Assert.Equal(DashStripRows.Cell, EngineBuilds.Age443B.Rules.Text.DashStrip);
        Assert.Equal(DashStripRows.BlackBox, EngineBuilds.Age445D.Rules.Text.DashStrip);
    }

    // ---- the join ----

    private static RgbaImage Surface(int width, int height) => new(width, height, new byte[width * height * 4]);

    private static void Fill(RgbaImage image, int left, int top, int right, int bottom, byte value)
    {
        for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                image.Pixels.AsSpan((y * image.Width + x) * 4, 4).Fill(value);
    }

    // The columns of one row that hold ink, as a string of '#' and '.'.
    private static string Row(RgbaImage image, int y)
        => new(Enumerable.Range(0, image.Width).Select(x => image.Pixels[(y * image.Width + x) * 4 + 3] != 0 ? '#' : '.')
            .ToArray());

    [Fact]
    public void TheLeftJoiningDashIsDrawnAgainOverTheGapBeforeIt()
    {
        // A dash 10 wide drawn at x = 20 with a (1, 1) effect: its strip is columns 21 to 27, and the copy ends at 21.
        RgbaImage surface = Surface(40, 16);
        Fill(surface, 20, 6, 30, 8, 200);

        ExtendedDashJoin.Apply(surface, ExtendedDashJoin.LeftOnly, drawX: 20, top: 2, nextPenX: 32, blackBoxWidth: 10,
                               fontSize: 8, effectOffsetX: 1, effectOffsetY: 1);

        Assert.Equal("..............################..........", Row(surface, 6));
        Assert.Equal(Row(surface, 6), Row(surface, 7));
        Assert.Equal("........................................", Row(surface, 5));
    }

    [Fact]
    public void TheBothSidesDashAlsoReachesTheNextGlyph()
    {
        RgbaImage surface = Surface(40, 16);
        Fill(surface, 20, 6, 30, 8, 200);

        ExtendedDashJoin.Apply(surface, ExtendedDashJoin.BothSides, drawX: 20, top: 2, nextPenX: 32,
                               blackBoxWidth: 10, fontSize: 8, effectOffsetX: 1, effectOffsetY: 1);

        // The second copy ends at next + the effect offset, 33.
        Assert.Equal("..............###################.......", Row(surface, 6));
    }

    [Fact]
    public void TheStripIsTheFontSizePlusTwiceTheEffectOffsetTall()
    {
        RgbaImage surface = Surface(40, 20);
        Fill(surface, 20, 0, 30, 20, 200);

        ExtendedDashJoin.Apply(surface, ExtendedDashJoin.LeftOnly, drawX: 20, top: 3, nextPenX: 32, blackBoxWidth: 10,
                               fontSize: 8, effectOffsetX: 0, effectOffsetY: 2);

        // Rows 3 to 14 (8 + 2 * 2) are copied: column 13, the copy's first, has ink in exactly those rows.
        Assert.Equal(Enumerable.Range(3, 12),
            Enumerable.Range(0, 20).Where(y => surface.Pixels[(y * surface.Width + 13) * 4 + 3] != 0));
    }

    [Fact]
    public void ADashAtTheSurfacesEdgeCopiesOnlyWhatFits()
    {
        // Drawn at x = 0, the first copy would end at column 1: it is clipped to that one column, and its source moves
        // by as much. The second copy then starts from the clipped source, one column wide.
        RgbaImage surface = Surface(24, 12);
        Fill(surface, 1, 4, 10, 6, 200);

        ExtendedDashJoin.Apply(surface, ExtendedDashJoin.BothSides, drawX: 0, top: 0, nextPenX: 14, blackBoxWidth: 10,
                               fontSize: 8, effectOffsetX: 0, effectOffsetY: 0);

        Assert.Equal("##########...#..........", Row(surface, 4));
    }

    [Fact]
    public void TheBlackBoxStripIsTheGlyphsInkRowsWidenedByTheEffectOffsetInAnEffectMode()
    {
        // A dash whose black box is 3 rows, its top 5 above the baseline at 12: rows 7 to 9. The cell rule would take
        // rows 2 to 11 for a size of 8 and a (1, 1) effect.
        foreach ((bool effect, int[] rows) in new[] { (false, new[] { 7, 8, 9 }), (true, new[] { 6, 7, 8, 9, 10 }) })
        {
            RgbaImage surface = Surface(40, 20);
            Fill(surface, 20, 0, 30, 20, 200);

            ExtendedDashJoin.ApplyBlackBox(surface, ExtendedDashJoin.LeftOnly, drawX: 20, baselineY: 12, originY: 5,
                                           blackBoxHeight: 3, effectMode: effect, nextPenX: 32, blackBoxWidth: 10,
                                           effectOffsetX: 1, effectOffsetY: 1);

            // Column 14, the copy's first, has ink in exactly the strip's rows.
            Assert.Equal(rows, Enumerable.Range(0, 20).Where(y => surface.Pixels[(y * surface.Width + 14) * 4 + 3] != 0));
        }
    }

    [Fact]
    public void ADashAfterAKanjiKeepsTheKanjisInkAboveAndBelowItOnlyUnderTheBlackBoxRule()
    {
        // A kanji's cell is 8 wide with ink in all 10 rows; the dash after it is 8 wide with ink in rows 4 and 5 only. The
        // strip, the dash's first 6 columns, is copied to end at the dash's x + 1: over the kanji's last 5 columns. 4.43B
        // copies the cell's rows, so the strip's blank rows clear the kanji's ink there; 4.45D copies the dash's own rows.
        static GlyphMask Glyph(GlyphRasterRequest request) => request.Cp932Code == ExtendedDashJoin.LeftOnly
            ? new(8, 2, 8, 0, 5, 8, 0, 8, 10, Enumerable.Repeat((byte)16, 16).ToArray(), ascent: 9)
            : new(8, 10, 8, 0, 9, 8, 0, 8, 10, Enumerable.Repeat((byte)16, 80).ToArray(), ascent: 9);
        AdvTextStyle style = AdvTextStyle.Default with { PrimaryFontSize = 10, RenderMode = 0 };
        var options = new GlyphTextLayoutOptions(0, 0, 0, 40, 16, false, style);
        GlyphRasterRequest[] glyphs =
        [
            new("face", 10, -5, 0, 0x6f22, 0x8abf, GlyphRasterPolicy.NativeCp932Gray4),
            new("face", 10, -5, 0, 0xe003, ExtendedDashJoin.LeftOnly, GlyphRasterPolicy.NativeCp932Gray4),
        ];

        RgbaImage cell = Surface(40, 16), blackBox = Surface(40, 16);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Glyph), joinsExtendedDashes: true)
            .Render(cell, options, glyphs);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Glyph), joinsExtendedDashes: true,
                                      dashStrip: DashStripRows.BlackBox).Render(blackBox, options, glyphs);

        // The dash's rows join under both rules.
        Assert.Equal("################", Row(cell, 4)[..16]);
        Assert.Equal(Row(cell, 4), Row(blackBox, 4));
        // Above it, the cell rule clears the kanji's columns 3 to 7; the black box rule leaves them.
        Assert.Equal("###.............", Row(cell, 0)[..16]);
        Assert.Equal("########........", Row(blackBox, 0)[..16]);
        Assert.Equal("###.............", Row(cell, 9)[..16]);
        Assert.Equal("########........", Row(blackBox, 9)[..16]);
    }

    [Fact]
    public void AnyOtherCodeIsNotJoined()
    {
        RgbaImage surface = Surface(40, 16);
        Fill(surface, 20, 6, 30, 8, 200);
        byte[] before = (byte[])surface.Pixels.Clone();

        ExtendedDashJoin.Apply(surface, 0xf041, 20, 2, 32, 10, 8, 1, 1);

        Assert.False(ExtendedDashJoin.Applies(0xf041));
        Assert.Equal(before, surface.Pixels);
    }

    [Fact]
    public void ALayoutThatJoinsDrawsTheDashOverTheGapAndOneThatDoesNotLeavesIt()
    {
        // Each dash is 4 wide in a cell 6 wide: two of them leave a gap of 2.
        static GlyphMask Dash(GlyphRasterRequest _) => new(4, 1, 4, 0, 1, 6, 0, 6, 5, [16, 16, 16, 16], ascent: 4);
        AdvTextStyle style = AdvTextStyle.Default with { PrimaryFontSize = 5, RenderMode = 0 };
        var options = new GlyphTextLayoutOptions(0, 0, 0, 40, 16, false, style);
        GlyphRasterRequest[] glyphs =
        [
            new("face", 5, -2, 0, 0xe001, 0xf041, GlyphRasterPolicy.NativeCp932Gray4),
            new("face", 5, -2, 0, 0xe003, 0xf043, GlyphRasterPolicy.NativeCp932Gray4),
        ];

        RgbaImage plain = Surface(40, 16), joined = Surface(40, 16);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Dash)).Render(plain, options, glyphs);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Dash), joinsExtendedDashes: true)
            .Render(joined, options, glyphs);

        Assert.Equal("####..####..", Row(plain, 3)[..12]);
        // The second dash's strip is its first two columns (4 - 2), copied to end at its own x + 1.
        Assert.Equal("####.#####..", Row(joined, 3)[..12]);
    }

    [Fact]
    public void ARubysDashesJoinAsTheBaseTextsDoWhereTheBuildJoins()
    {
        // The ruby goes through the same glyph drawers: op 0x82's redraw of a ruby record, with no gap.
        static GlyphMask Dash(GlyphRasterRequest _) => new(4, 1, 4, 0, 1, 6, 0, 6, 5, [16, 16, 16, 16], ascent: 4);
        AdvTextStyle style = AdvTextStyle.Default with { PrimaryFontSize = 5, RenderMode = 0 };

        RgbaImage plain = Surface(40, 16), joined = Surface(40, 16);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Dash)).RenderRubyAt(
            plain, 0, 0, "", style, GlyphRasterPolicy.NativeCp932Gray4, false, RubyLayoutRules.FixedOffsets);
        new RetainedGlyphLayoutEngine(new RecordingRasterizer(Dash), joinsExtendedDashes: true).RenderRubyAt(
            joined, 0, 0, "", style, GlyphRasterPolicy.NativeCp932Gray4, false, RubyLayoutRules.FixedOffsets);

        Assert.Equal("####..####..", Row(plain, 3)[..12]);
        Assert.Equal("####.#####..", Row(joined, 3)[..12]);
    }

    // ---- the font ----

    private sealed class Files(Dictionary<string, byte[]> files) : IAssetStore
    {
        public Stream Open(AssetEntry entry) => new MemoryStream(ReadAll(entry));

        public byte[] ReadAll(AssetEntry entry)
            => files.TryGetValue(entry.Name, out byte[]? bytes) ? bytes : throw new FileNotFoundException(entry.Name);
    }

    private static Sys4AssetCatalog Catalog(byte[] afterSettings, params string[] files)
        => Sys4AssetCatalog.Parse(SyntheticCatalog.Wrap(
            [.. SyntheticCatalog.Expanded([("SCREENX", "800")], includeTrailer: true, files), .. afterSettings],
            "S4IC438 ", "Synthetic"));

    private static byte[] FontList(params (int File, string Face)[] fonts)
    {
        byte[] records = fonts.SelectMany(font => (byte[])
            [.. BitConverter.GetBytes(font.File), .. System.Text.Encoding.ASCII.GetBytes(font.Face), 0]).ToArray();
        return [.. BitConverter.GetBytes(records.Length), .. records];
    }

    [Fact]
    public void TheGameFontIsTheFirstOfTheCatalogsList()
    {
        // Native registers the first record's file and no other.
        Sys4AssetCatalog catalog = Catalog(FontList((1, Extended), (2, "Second")), "A.BIN", "FONT.TTF", "OTHER.TTF");
        var files = new Files(new() { ["FONT.TTF"] = [1, 2, 3], ["OTHER.TTF"] = [4] });

        GameFont font = Assert.IsType<GameFont>(GameFont.Load(catalog, files));

        Assert.Equal((Extended, "FONT.TTF"), (font.FaceName, font.FileName));
        Assert.Equal(new byte[] { 1, 2, 3 }, font.Data);
    }

    [Fact]
    public void TheGameFontReadThroughTheTrackingStoreIsMarkedSeen()
    {
        // Native opens the file through asset_open_indexed_entry, which marks it seen in SAVE.DAT: 魔導巧殻's
        // AGE-EXTEND.TTF (file 46) is unlocked in every SAVE.DAT native writes, a run that only reaches the title too.
        Sys4AssetCatalog catalog = Catalog(FontList((1, Extended)), "A.BIN", "FONT.TTF");
        var profile = new SharedProfile();
        var store = new CatalogTrackingAssetStore(new Files(new() { ["FONT.TTF"] = [1, 2, 3] }),
                                                  entry => profile.MarkCatalogResourceOpened(entry.PackedId));

        Assert.NotNull(GameFont.Load(catalog, store));

        Assert.True(profile.IsCatalogResourceUnlocked(1));
        Assert.False(profile.IsCatalogResourceUnlocked(0));
    }

    [Fact]
    public void ACatalogWithNoFontListOrAFirstFileOfZeroHasNoGameFont()
    {
        var files = new Files(new() { ["A.BIN"] = [9] });

        Assert.Null(GameFont.Load(Catalog([], "A.BIN"), files));
        // Native tests the first id for being above 0 before it opens anything.
        Assert.Null(GameFont.Load(Catalog(FontList((0, Extended)), "A.BIN"), files));
    }

    [Fact]
    public void AFontTheCatalogCannotGiveIsReportedAndLeftOut()
    {
        var warnings = new List<string>();

        Assert.Null(GameFont.Load(Catalog(FontList((7, Extended)), "A.BIN"), new Files([]), warnings.Add));
        Assert.Null(GameFont.Load(Catalog(FontList((1, Extended)), "A.BIN", "FONT.TTF"), new Files([]), warnings.Add));

        Assert.Equal(2, warnings.Count);
        Assert.Contains("file 7", warnings[0]);
        Assert.Contains("FONT.TTF", warnings[1]);
    }

    [Fact]
    public void AFontIsGivenOnlyToARasterizerThatTakesFontFiles()
        => Assert.False(new GameFont(Extended, "FONT.TTF", [1]).AddTo(new RecordingRasterizer()));

    [Trait("Category", "Workspace")]
    [Trait("Profile", "arterial")]
    [Fact]
    public void TheInstalledGamesFontDrawsItsDashesUnderGdi()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("arterial");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"arterial install not found at {gameRoot}");
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);

        GameFont font = Assert.IsType<GameFont>(GameFont.Load(catalog, new Sys4AssetStore(catalog, gameRoot, gameRoot)));

        Assert.Equal((Extended, "AGE-EXTEND.TTF"), (font.FaceName, font.FileName.ToUpperInvariant()));
        Assert.Equal(new byte[] { 0, 1, 0, 0 }, font.Data[..4]);    // a TrueType file
        if (!WindowsGdiGlyphMaskRasterizer.TryGetAvailability(out _)) return;

        using var gdi = new WindowsGdiGlyphMaskRasterizer();
        Assert.True(font.AddTo(gdi));
        var rasterizer = new ExtendedFaceGlyphRasterizer(gdi, Extended);
        // 0xF042 and 0xF043 are dashes nearly as wide as their cell; the text's face has no such glyph.
        foreach (ushort code in new ushort[] { ExtendedDashJoin.BothSides, ExtendedDashJoin.LeftOnly })
        {
            GlyphMask dash = rasterizer.Rasterize(new GlyphRasterRequest(
                "ＭＳ ゴシック", 22, -11, 700, 0xe000 + (code - 0xf040), code, GlyphRasterPolicy.NativeCp932Gray4));
            Assert.InRange(dash.Width, 18, 24);
            Assert.InRange(dash.Height, 1, 6);
        }
    }
}
