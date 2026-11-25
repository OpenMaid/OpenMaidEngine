using System.Collections.Generic;
using System.Linq;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// The surfaces native's engine makes in the slots for itself (docs/engine-re.md, "Engine-owned surfaces"), which op
/// 0x208 sizes and op 0x24a counts: start-up's composite surfaces and their release guards, op 0x70's layout surface,
/// the op 0x223/0x24f targets and the legacy transitions' scratch surface 32.
/// </summary>
public sealed class EngineSurfacesTests
{
    private const int Immediate = 0;

    private static Operand I(long value) => new(Immediate, value);

    private static Graphics NewGraphics(int pixelShaderLevel = 2, bool guardsFrameSurface = false)
        => new(null, new FrameClock(), new Sys4LogicalCanvas(1024, 576))
        {
            PixelShaderLevel = pixelShaderLevel,
            GuardsFrameSurface = guardsFrameSurface,
        };

    [Fact]
    public void StartUpCreatesTheTwoCompositeRenderTargetsAtTheScreensSize()
    {
        Graphics graphics = NewGraphics();

        Assert.Equal([0x24, 0x25], graphics.LiveSurfaceSlots());
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x24));
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x25));
        Assert.Equal((2 * 1024 * 576 * 4, 0), graphics.TextureMemory());
        // Their records read created, as gfx_surface_create_blank writes them.
        Assert.Contains(graphics.State.CapturePersistenceSnapshot().Surfaces, s => s.Slot == 0x24 && s.Created);
        // A native with no screen makes none.
        Assert.Empty(Graphics.Headless().LiveSurfaceSlots());
    }

    [Fact]
    public void BelowPixelShaderLevelTwoTheCompositeSurfacesAreOpaqueRenderTargets()
    {
        // Kind 1 has an alpha channel and starts clear; kind 2 has none, so it samples opaque.
        Assert.Equal(0, NewGraphics(pixelShaderLevel: 2).SurfacePixels(0x24)!.Pixels[3]);
        Graphics opaque = NewGraphics(pixelShaderLevel: 1);
        Assert.Equal(255, opaque.SurfacePixels(0x24)!.Pixels[3]);
        Assert.Equal(255, opaque.SurfacePixels(0x25)!.Pixels[3]);
        // Both kinds count as render targets.
        Assert.Equal((2 * 1024 * 576 * 4, 0), opaque.TextureMemory());
    }

    [Fact]
    public void TheReleaseWorkerLeavesTheGuardedSlotsAndACreateStillReplacesOne()
    {
        Graphics graphics = NewGraphics();
        graphics.CreateSurface(0x26, 64, 64);
        graphics.CreateSurface(0x30, 64, 64);

        graphics.ReleaseSurface(0x24);
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x24));
        graphics.ReleaseSurfaceRange(0, 1000);
        Assert.Equal([0x24, 0x25], graphics.LiveSurfaceSlots());

        graphics.CreateSurface(0x24, 10, 10);
        Assert.Equal((10, 10), graphics.SurfaceSize(0x24));
    }

    [Fact]
    public void FromAge424NTheFrameSurfaceSlotIsGuardedToo()
    {
        Graphics graphics = NewGraphics(guardsFrameSurface: true);
        graphics.CreateSurface(0x26, 64, 64);

        graphics.ReleaseSurface(0x26);
        graphics.ReleaseSurfaceRange(0, 1000);

        Assert.Equal([0x24, 0x25, 0x26], graphics.LiveSurfaceSlots());
        Assert.False(EngineBuilds.Age424J.Rules.Gfx.GuardsFrameSurface);
        Assert.All(new[] { EngineBuilds.Age424N, EngineBuilds.Age433G, EngineBuilds.Age438D },
                   build => Assert.True(build.Rules.Gfx.GuardsFrameSurface));
    }

    [Fact]
    public void ATransitionTargetTakesSurface36sKind()
    {
        Graphics graphics = NewGraphics();
        // An empty slot is created at surface 36's size.
        graphics.PrepareTransitionTarget(0x60);
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x60));
        Assert.Equal((3 * 1024 * 576 * 4, 0), graphics.TextureMemory());

        // One of another kind is created again at its own size; one of 36's kind stays as it is.
        graphics.CreateSurface(0x61, 300, 200);
        graphics.PrepareTransitionTarget(0x61);
        Assert.Equal((300, 200), graphics.SurfaceSize(0x61));
        Assert.Equal((3 * 1024 * 576 * 4 + 300 * 200 * 4, 0), graphics.TextureMemory());
        RgbaImage kept = graphics.SurfacePixels(0x61)!;
        graphics.PrepareTransitionTarget(0x61);
        Assert.Same(kept, graphics.SurfacePixels(0x61));
    }

    [Fact]
    public void AFadeLeavesScratch32EmptyAndAFinishOnBlackOrWhiteKeepsIt()
    {
        Graphics graphics = NewGraphics();
        graphics.CreateSurface(0x20, 8, 8);

        // Begin creates it and the fade's end releases it: no surface, and a record of resource id -1, created 1.
        graphics.UseFadeScratch(0x24);
        Assert.DoesNotContain(0x20, graphics.LiveSurfaceSlots());
        Assert.Equal((0, 0), graphics.SurfaceSize(0x20));
        graphics.State.SnapshotSurfaceRecords();   // op 0x71's copy, which a save writes
        byte[] records = graphics.State.CaptureNativeSurfaceRecords();
        Assert.Equal(-1, System.BitConverter.ToInt32(records, 0x20 * 20));
        Assert.Equal(1, System.BitConverter.ToInt32(records, 0x20 * 20 + 0x10));

        graphics.FinishTransitionOnColour(0xffffff);
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x20));
        Assert.Equal(new byte[] { 255, 255, 255, 255 }, graphics.SurfacePixels(0x20)!.Pixels[..4]);
        Assert.Equal((2 * 1024 * 576 * 4, 1024 * 576 * 4), graphics.TextureMemory());
    }

    [Fact]
    public void TheMosaicKeepsScratch32AtItsSourcesSizeCutToTheScreen()
    {
        Graphics graphics = NewGraphics();
        graphics.CreateSurface(0x40, 1336, 640);

        graphics.BeginMosaicScratch(0x40);

        Assert.Equal((1024, 576), graphics.SurfaceSize(0x20));
        Assert.Equal((1024, 576), graphics.SurfaceSize(0x24));
    }

    [Fact]
    public void Op0x70CreatesItsLayoutsBlankSurfaceAtTheLayoutsSize()
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        Graphics graphics = NewGraphics();
        var code = new List<(int, Operand[])>
        {
            (table.ByLabel("define-adv-text-layout")!.Value, [I(1), I(1024), I(85), I(0), I(491)]),
            (0x2, []),
        };
        Script script = ScriptAssembler.Assemble(table, "LAYOUT", code, []);
        var vm = new VirtualMachine(script, table, new RecordingWaits(), graphics: graphics);

        vm.Run();

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal((1024, 85), graphics.SurfaceSize(0x15));
        Assert.Equal((2 * 1024 * 576 * 4, 1024 * 85 * 4), graphics.TextureMemory());
    }
}
