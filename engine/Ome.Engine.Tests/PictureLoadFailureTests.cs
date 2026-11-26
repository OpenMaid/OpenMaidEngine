using System.Text;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using static GraphicsTestSupport;

/// <summary>
/// The picture loads AGE refuses (ops 0x1f9 and 0x249; docs/engine-re.md, "A failed picture load"): the resource
/// open's boxes, then gfx_surface_decode_and_create's header test on any record, whatever its name, and what each
/// leaves in the slot.
/// </summary>
public sealed class PictureLoadFailureTests
{
    private const int Immediate = 0, GlobalInt = 3;
    private static Operand I(long value) => new(Immediate, value);
    private static Operand G(int address) => new(GlobalInt, address);

    // Record 0 a script, as SYSTEM4.BIN is in each catalog; 2 a placeholder (no file).
    private static readonly (string Name, byte[] Bytes)[] Records =
    [
        ("SYSTEM4.BIN", [0x53, 0x59, 0x53, 0x34, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00]),
        ("BG.AGF", Png()),
        ("@", []),
        ("MV.AGF", [0x00, 0x00, 0x01, 0xba, 0x21, 0x00, 0x01, 0x00, 0x01, 0x80, 0xa2, 0x61, 0x00, 0x00]),
        ("BM16.AGF", Bmp(16)),
        ("BM24.AGF", Bmp(24)),
        ("UNTAGGED.AGF", Agf("\0\0\0\0", type: 2, eight: 0)),
        ("EIGHT.AGF", Agf("ACGF", type: 1, eight: 5)),
        ("TINY.AGF", [0x41, 0x43, 0x47, 0x46, 0x01, 0x00]),
    ];

    [Fact]
    public void TheCheckSaysWhatAgeSaysForEachRecord()
    {
        ResourceMap resources = Resources();
        string Says(long id) => resources.CheckPictureLoad(id) is { } failure
            ? $"{failure.Box.Text(AgeBoxText.JapaneseText)} ({(failure.Opened ? "opened" : "not opened")})"
            : "loads";

        // The resource open's boxes, before the slot is touched.
        Assert.Equal("ファイル番号 9 は不正な値です． (not opened)", Says(9));
        Assert.Equal("@ を開くことが出来ません．\nERROR CODE = 2 (not opened)", Says(2));
        Assert.Equal("拡張ファイル情報ファイル 1 は読み込まれていません． (not opened)", Says(0x01000000));
        Assert.Equal("loads", Says(0x80000000));   // below native's table of catalogs: the port cannot say
        // The header test, on any record: a script, a movie under an AGF name, a 16-bit BMP, an AGF whose +8 is not
        // 0, and a file shorter than the header AGE reads.
        Assert.Equal("画像ファイル SYSTEM4.BIN の読み込みに失敗しました (opened)", Says(0));
        Assert.Equal("画像ファイル MV.AGF の読み込みに失敗しました (opened)", Says(3));
        Assert.Equal("画像ファイル BM16.AGF の読み込みに失敗しました (opened)", Says(4));
        Assert.Equal("画像ファイル EIGHT.AGF の読み込みに失敗しました (opened)", Says(7));
        Assert.Equal("画像ファイル TINY.AGF の読み込みに失敗しました (opened)", Says(8));
        // What AGE reads: a language package's PNG (which stands for the record), a 24-bit BMP and an AGF of type 2
        // without its tag, which AGE does not read.
        Assert.Equal("loads", Says(1));
        Assert.Equal("loads", Says(5));
        Assert.Equal("loads", Says(6));
    }

    [Fact]
    public void ARefusedHeaderReleasesTheSlotAndIgnoreGoesOn()
    {
        (VirtualMachine vm, RecordingWaits host) = Run(
        [
            (0x1f9, [I(1), I(5), I(-1)]),   // BG.AGF into slot 5
            (0x1f9, [I(0), I(5), I(-1)]),   // SYSTEM4.BIN into it: refused
            (0x55, [G(0x200), I(1)]),
        ], ScriptErrorChoice.Ignore);

        Assert.Equal("exit", vm.HaltReason);
        DiagnosticMessage box = Assert.Single(host.Diagnostics);
        Assert.Equal(AgeBoxButtons.AbortRetryIgnore, box.Parts!.Buttons);
        Assert.Contains("画像ファイル SYSTEM4.BIN の読み込みに失敗しました", box.Text);
        Assert.Equal(1, vm.Globals[0x200]);
        Assert.False(vm.Graphics.TryGetSurfaceSize(5, out _));
        Assert.Equal(-1, RecordResourceId(vm, 5));
    }

    [Fact]
    public void AnIdNoRecordOpensForLeavesTheSlotAsItWas()
    {
        (VirtualMachine vm, RecordingWaits host) = Run(
        [
            (0x1f9, [I(1), I(5), I(-1)]),
            (0x249, [I(9), I(5), I(-1)]),   // past the file table
        ], ScriptErrorChoice.Ignore);

        Assert.Equal("exit", vm.HaltReason);
        Assert.Contains("ファイル番号 9 は不正な値です．", Assert.Single(host.Diagnostics).Text);
        Assert.True(vm.Graphics.TryGetSurfaceSize(5, out (int W, int H) size));
        Assert.Equal((2, 2), size);
        Assert.Equal(1, RecordResourceId(vm, 5));
    }

    [Theory]
    [InlineData(9, true)]    // no record opens: the pixels stay
    [InlineData(0, false)]   // a refused header: the slot is released
    public void ARefusedLoadDeletesTheSlotsMovie(long resourceId, bool pixelsStay)
    {
        var graphics = new Graphics(Resources(), new FrameClock(), new Sys4LogicalCanvas(4, 4));
        var movies = new Movies(graphics, new FakeMovieAssets(),
                                new FakeMovieDecoderFactory(new FakeMovieDecoder { StopTimeMs = 1000 }), sound: null,
                                dependMovieSound: 0);
        Assert.Null(graphics.LoadSurface(5, 1, -1));
        movies.PlayToSurface("LOADS", 0x2be3, 5, 0, 0);
        Assert.True(movies.IsSurfaceActive(5));

        Assert.NotNull(graphics.LoadSurface(5, resourceId, -1));

        Assert.False(movies.IsSurfaceActive(5));
        Assert.Empty(movies.SurfaceDiagnostics());
        Assert.Equal(pixelsStay, graphics.TryGetSurfaceSize(5, out _));
    }

    [Theory]
    [InlineData(0, "画像ファイル SYSTEM4.BIN の読み込みに失敗しました", false)]   // a refused header: the slot released
    [InlineData(9, "ファイル番号 9 は不正な値です．", true)]                    // no record opens: the slot stays
    public void ALoadsRestoreTracesARefusedPictureAndGoesOn(long resourceId, string message, bool slotStays)
    {
        var printed = new List<string>();
        var graphics = new Graphics(Resources(), new FrameClock(), new Sys4LogicalCanvas(4, 4), print: printed.Add);
        Assert.Null(graphics.LoadSurface(5, 1, -1));

        graphics.ReloadSurface(5, resourceId, -1);
        graphics.ReloadSurface(6, 1, -1);   // the next record still loads

        Assert.Contains(printed, line => line.Contains("surface 5") && line.Contains(message));
        Assert.Equal(slotStays, graphics.TryGetSurfaceSize(5, out _));
        graphics.State.SnapshotSurfaceRecords();
        Assert.Equal(slotStays ? 1 : -1,
                     BitConverter.ToInt32(graphics.State.CaptureNativeSurfaceRecords(), 5 * 20));
        Assert.True(graphics.TryGetSurfaceSize(6, out (int W, int H) size));
        Assert.Equal((2, 2), size);
    }

    [Fact]
    public void AbortEndsTheRunAtTheLoad()
    {
        (VirtualMachine vm, _) = Run([(0x1f9, [I(3), I(5), I(-1)])], ScriptErrorChoice.Abort);

        Assert.Equal("script-error: 画像ファイル MV.AGF の読み込みに失敗しました", vm.HaltReason);
    }

    private static (VirtualMachine, RecordingWaits) Run(List<(int, Operand[])> code, ScriptErrorChoice answer)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        code.Add((0x2, []));
        Script script = ScriptAssembler.Assemble(table, "LOADS", code, []);
        var host = new RecordingWaits();
        host.ScriptErrorAnswers.Enqueue(answer);
        var graphics = new Graphics(Resources(), new FrameClock(), new Sys4LogicalCanvas(4, 4));
        var vm = new VirtualMachine(script, table, host, graphics: graphics);
        vm.Run();
        return (vm, host);
    }

    private static int RecordResourceId(VirtualMachine vm, int slot)
    {
        vm.Graphics.State.SnapshotSurfaceRecords();
        return BitConverter.ToInt32(vm.Graphics.State.CaptureNativeSurfaceRecords(), slot * 20);
    }

    private static ResourceMap Resources()
    {
        var catalog = Sys4AssetCatalog.Parse(SyntheticCatalog.Build(Records.Select(record => record.Name).ToArray()));
        return new ResourceMap(catalog, new Bytes(Records.ToDictionary(record => record.Name, record => record.Bytes)));
    }

    private static byte[] Png() => PngWriter.Write(Image(2, 2, (_, _) => [1, 2, 3, 255]));

    // A BMP's file and information headers, the bit count at 28.
    private static byte[] Bmp(int bits)
    {
        var bytes = new byte[54];
        bytes[0] = (byte)'B';
        bytes[1] = (byte)'M';
        BitConverter.GetBytes((ushort)bits).CopyTo(bytes, 28);
        return bytes;
    }

    private static byte[] Agf(string tag, int type, int eight)
    {
        var bytes = new byte[0x18];
        Encoding.ASCII.GetBytes(tag).CopyTo(bytes, 0);
        BitConverter.GetBytes(type).CopyTo(bytes, 4);
        BitConverter.GetBytes(eight).CopyTo(bytes, 8);
        return bytes;
    }

    // The records' bytes; a PNG one is served as a language package's picture is.
    private sealed class Bytes(Dictionary<string, byte[]> files) : IAssetStore
    {
        public Stream Open(AssetEntry entry) => new MemoryStream(ReadAll(entry));
        public byte[] ReadAll(AssetEntry entry) => files[entry.Name];
        public (byte[] Bytes, bool Png) ReadImage(AssetEntry entry)
            => (ReadAll(entry), entry.Name == "BG.AGF");
    }
}
