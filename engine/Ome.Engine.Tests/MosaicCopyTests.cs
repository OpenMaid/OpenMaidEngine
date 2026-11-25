using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// Op 0x35, native MosaicTexture (docs/engine-re.md, "Op 0x35, the mosaic copy"): each dot x dot block of the
/// destination rectangle takes the source pixel at its top-left corner, over the destination rectangle's size.
/// </summary>
public sealed class MosaicCopyTests
{
    private const int Source = 0x40, Destination = 0x41;
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);

    private static Operand I(long value) => new(0, value);

    private static Graphics NewGraphics()
        => new(null, new FrameClock(), new Sys4LogicalCanvas(1024, 576));

    // A plain source whose pixel (x, y) reads red x, green y, blue 7 and alpha 100.
    private static Graphics WithSource(int width, int height, long kind = 0)
    {
        Graphics graphics = NewGraphics();
        graphics.CreateSurface(Source, width, height, kind);
        byte[] pixels = graphics.SurfacePixels(Source)!.Pixels;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int at = (y * width + x) * 4;
            (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = ((byte)x, (byte)y, 7, 100);
        }
        return graphics;
    }

    private static (int R, int G, int B, int A) Pixel(Graphics graphics, int slot, int x, int y)
    {
        RgbaImage image = graphics.SurfacePixels(slot)!;
        int at = (y * image.Width + x) * 4;
        return (image.Pixels[at], image.Pixels[at + 1], image.Pixels[at + 2], image.Pixels[at + 3]);
    }

    private static SurfaceMosaicCopy Copy(int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, int dot,
                                          int source = Source, int destination = Destination)
        => new(source, destination, sx, sy, sx + sw, sy + sh, dx, dy, dx + dw, dy + dh, dot);

    [Fact]
    public void EachBlockTakesTheSourcePixelAtItsTopLeftCornerAndTheRestOfTheDestinationStays()
    {
        Graphics graphics = WithSource(6, 6);
        graphics.CreateSurface(Destination, 6, 6);

        var result = graphics.MosaicSurfaceRect(Copy(1, 1, 3, 3, 2, 2, 3, 3, dot: 2));

        Assert.Equal(SurfaceMosaicOutcome.Copied, result.Outcome);
        Assert.Equal((1, 1, 7, 100), Pixel(graphics, Destination, 2, 2));
        Assert.Equal((1, 1, 7, 100), Pixel(graphics, Destination, 3, 3));
        Assert.Equal((3, 1, 7, 100), Pixel(graphics, Destination, 4, 2));
        Assert.Equal((1, 3, 7, 100), Pixel(graphics, Destination, 2, 4));
        Assert.Equal((3, 3, 7, 100), Pixel(graphics, Destination, 4, 4));
        Assert.Equal((0, 0, 0, 0), Pixel(graphics, Destination, 1, 2));
        Assert.Equal((0, 0, 0, 0), Pixel(graphics, Destination, 5, 5));
    }

    [Fact]
    public void TheReadsWalkTheDestinationRectanglesSizeFromTheSourceRectanglesOrigin()
    {
        Graphics graphics = WithSource(4, 4);
        graphics.CreateSurface(Destination, 4, 4);

        // A 1 x 1 source rectangle still gives the destination's 4 x 4 pixels of the source.
        Assert.Equal(SurfaceMosaicOutcome.Copied, graphics.MosaicSurfaceRect(Copy(0, 0, 1, 1, 0, 0, 4, 4, 1)).Outcome);

        Assert.Equal((3, 2, 7, 100), Pixel(graphics, Destination, 3, 2));
        // One that would read past the source's pixels is refused to the port.
        Assert.Equal(SurfaceMosaicOutcome.ReadsPastSource,
                     graphics.MosaicSurfaceRect(Copy(1, 0, 1, 1, 0, 0, 4, 4, 1)).Outcome);
    }

    [Fact]
    public void ARenderTargetSourceGivesOpaquePixels()
    {
        Graphics graphics = WithSource(4, 4, kind: 1);
        graphics.CreateSurface(Destination, 4, 4);

        graphics.MosaicSurfaceRect(Copy(0, 0, 4, 4, 0, 0, 4, 4, 2));

        Assert.Equal((2, 2, 7, 255), Pixel(graphics, Destination, 3, 3));
    }

    [Fact]
    public void ANegativeDotNarrowerThanTheRectangleWritesBeforeTheLineBuffer()
    {
        Graphics graphics = WithSource(4, 4);
        graphics.CreateSurface(Destination, 4, 4);

        // Within the dot's width every column takes column 0, and the rows take blocks of the dot's size.
        Assert.Equal(SurfaceMosaicOutcome.Copied, graphics.MosaicSurfaceRect(Copy(0, 0, 4, 4, 0, 0, 2, 4, -2)).Outcome);
        Assert.Equal((0, 2, 7, 100), Pixel(graphics, Destination, 1, 3));
        Assert.Equal(SurfaceMosaicOutcome.WritesBeforeLineBuffer,
                     graphics.MosaicSurfaceRect(Copy(0, 0, 4, 4, 0, 0, 3, 4, -2)).Outcome);
    }

    [Fact]
    public void NativesRefusalsTraceAndCopyNothing()
    {
        Graphics graphics = WithSource(4, 4);
        graphics.CreateSurface(Destination, 4, 4);
        const string Prefix = "関数：MosaicTexture エラー：";

        (SurfaceMosaicCopy Copy, string Trace)[] refusals =
        [
            (Copy(0, 0, 4, 4, 0, 0, 4, 4, 1, source: 0x50), "転送元テクスチャが作成されていません． TEXTURE=80"),
            (Copy(0, 0, 4, 4, 0, 0, 4, 4, 1, destination: 0x50), "転送先テクスチャが作成されていません． TEXTURE=80"),
            (Copy(0, 0, 4, 4, 0, 0, 4, 4, 1, destination: Source), "転送先と転送元が同じですTEXTURE = 64"),
            (Copy(0, 0, 4, 4, 0, 0, 4, 4, 0), "モザイクサイズの指定が不正ですsize = 0"),
            (Copy(1, 0, 4, 4, 0, 0, 4, 4, 1), "転送元の座標が不正です (1,0)-(5,4)"),
            (Copy(0, -1, 4, 4, 0, 0, 4, 4, 1), "転送元の座標が不正です (0,-1)-(4,3)"),
            (Copy(0, 0, 4, 4, 0, 0, 5, 4, 1), "転送先の座標が不正です (0,0)-(5,4)"),
        ];
        foreach ((SurfaceMosaicCopy copy, string trace) in refusals)
            Assert.Equal(new SurfaceMosaicResult(SurfaceMosaicOutcome.Refused, Prefix + trace),
                         graphics.MosaicSurfaceRect(copy));

        Assert.All(graphics.SurfacePixels(Destination)!.Pixels, value => Assert.Equal(0, value));
    }

    [Fact]
    public void TheOpWritesTheDestinationsRecordFirstAndReportsARefusalAsAWarning()
    {
        Graphics graphics = WithSource(4, 4);
        var host = new RecordingWaits();
        VirtualMachine vm = Run(graphics, host, Mosaic(Source, 0x50, 0, 0, 4, 4, 0, 0, 4, 4, 1));

        Assert.Equal("exit", vm.HaltReason);
        Assert.Contains(host.Warnings, w => w.Contains("「関数：MosaicTexture エラー：転送先テクスチャが作成されていません． TEXTURE=80」"));
        graphics.State.SnapshotSurfaceRecords();
        byte[] records = graphics.State.CaptureNativeSurfaceRecords();
        Assert.Equal(1, System.BitConverter.ToInt32(records, 0x50 * 20 + 0x10));
    }

    [Fact]
    public void TheOpAddsEachExtentToItsOriginAndHaltsWhereNativeReadsPastTheSource()
    {
        Graphics graphics = WithSource(4, 4);
        graphics.CreateSurface(Destination, 8, 8);
        VirtualMachine vm = Run(graphics, new RecordingWaits(), Mosaic(Source, Destination, 1, 1, 2, 2, 4, 4, 2, 2, 1));
        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal((2, 2, 7, 100), Pixel(graphics, Destination, 5, 5));

        vm = Run(graphics, new RecordingWaits(), Mosaic(Source, Destination, 0, 0, 4, 4, 0, 0, 8, 8, 1));
        Assert.StartsWith("mosaic-reads-past-source: MOSAIC@0x0 ", vm.HaltReason);
        Assert.EndsWith(": AGE reads pixel (7,7) of surface 64, which is 4x4, past its rows", vm.HaltReason);
    }

    private static (int, Operand[]) Mosaic(params long[] operands)
        => (Table.ByLabel("mosaic-copy-surface-rect")!.Value, operands.Select(I).ToArray());

    private static VirtualMachine Run(Graphics graphics, RecordingWaits host, (int, Operand[]) instruction)
    {
        Script script = ScriptAssembler.Assemble(Table, "MOSAIC", new List<(int, Operand[])> { instruction, (0x2, []) }, []);
        var vm = new VirtualMachine(script, Table, host, graphics: graphics);
        vm.Run();
        return vm;
    }
}
