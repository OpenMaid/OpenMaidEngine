using System.Collections.Generic;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// Op 0x24a, native's texture memory counters (docs/engine-re.md, "Texture memory counters"): the graphics native counts
/// its surfaces as gfx_surface_texture_create and gfx_surface_texture_release count their textures, and the op writes
/// the two counters with a 0 between them. Verita's title is held to native's in NaturalBootIntegrationTests.
/// </summary>
public sealed class TextureMemoryTests
{
    private const int Immediate = 0, GlobalInt = 3;
    // Start-up's two composite render targets at the 1024 x 576 screen (EngineSurfacesTests).
    private const int Composites = 2 * 1024 * 576 * 4;

    private static Operand I(long value) => new(Immediate, value);
    private static Operand G(int address) => new(GlobalInt, address);

    private static Graphics NewGraphics(int textureBits = 32)
        => new(null, new FrameClock(), new Sys4LogicalCanvas(1024, 576)) { TextureBits = textureBits };

    [Fact]
    public void KindsOneAndTwoAreRenderTargetsAndEverySurfaceElseIsTheOtherCounter()
    {
        Graphics graphics = NewGraphics();
        graphics.CreateSurface(50, 1024, 576, kind: 1);
        graphics.CreateSurface(51, 200, 100, kind: 2);
        graphics.CreateSurface(40, 300, 230);
        // Kind 3 is native's tiled surface, whose tiles together have its size.
        graphics.CreateSurface(41, 1336, 640, kind: 3);

        Assert.Equal((Composites + 1024 * 576 * 4 + 200 * 100 * 4, 300 * 230 * 4 + 1336 * 640 * 4),
                     graphics.TextureMemory());
    }

    [Fact]
    public void AReleasedOrRecreatedSurfaceCountsNoMoreAndTheScreenSlotIsNotATexture()
    {
        Graphics graphics = NewGraphics();
        // Slot 0 reads the screen's size (op 0x208), but native's slot 0 holds no texture.
        Assert.Equal((Composites, 0), graphics.TextureMemory());
        graphics.CreateSurface(40, 300, 230);
        graphics.ReleaseSurface(40);
        // A create releases what the slot held first, kind included.
        graphics.CreateSurface(36, 10, 10);

        Assert.Equal((Composites / 2, 400), graphics.TextureMemory());
    }

    [Fact]
    public void ATexelTakesDisplayTextureBitAndTheArithmeticIsNativesThirtyTwoBits()
    {
        Assert.Equal((0, 3 * 5 * 16 / 8), Count(NewGraphics(16), (3, 5)));
        // 16384 x 8192 x 32 bits is 2^32, which wraps to 0 in native's imul, so the texture adds nothing.
        Assert.Equal((0, 0), Count(NewGraphics(), (16384, 8192)));
        // A product past 2^31 is negative, and native's shift divides it toward zero.
        Assert.Equal((0, unchecked(9000 * 9000 * 32) / 8), Count(NewGraphics(), (9000, 9000)));

        static (int, int) Count(Graphics graphics, (int W, int H) size)
        {
            graphics.CreateSurface(40, size.W, size.H);
            return (0, graphics.TextureMemory().Others);
        }
    }

    [Fact]
    public void TheOpWritesTheRenderTargetsZeroAndTheRest()
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson);
        Graphics graphics = NewGraphics();
        var code = new List<(int, Operand[])>
        {
            (table.ByLabel("create-texture")!.Value, [I(50), I(1024), I(576), I(1)]),
            (table.ByLabel("create-texture")!.Value, [I(40), I(300), I(230), I(0)]),
            (table.ByLabel("mov")!.Value, [G(0x1001), I(77)]),
            (0x24a, [G(0x1000), G(0x1001), G(0x1002)]),
            (0x2, []),
        };
        Script script = ScriptAssembler.Assemble(table, "TEXMEM", code, []);
        var vm = new VirtualMachine(script, table, new RecordingWaits(), graphics: graphics);

        vm.Run();

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal(Composites + 1024 * 576 * 4, vm.Globals[0x1000]);
        Assert.Equal(0, vm.Globals[0x1001]);
        Assert.Equal(300 * 230 * 4, vm.Globals[0x1002]);
    }
}
