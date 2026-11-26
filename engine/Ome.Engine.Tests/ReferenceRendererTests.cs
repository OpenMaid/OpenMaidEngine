using Ome.Engine.Devices;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Xunit;

/// <summary>The reference renderer's own rules (engine layering spec §6.3) that the lowering tests do not reach.</summary>
public class ReferenceRendererTests
{
    private static FrameDescription OneStaticDraw(RgbaImage pixels)
    {
        var frame = new FrameDescription { Width = 1, Height = 1 };
        frame.Items.Add(new FrameItem(
            FrameItemKind.Texture, pixels, AssetId: 7, Dynamic: false, DynamicKey: 0, ColorKey: -1, SourceX: 0,
            SourceY: 0, Width: 1, Height: 1, Matrix: new Affine2D(1, 0, 0, 1, 0, 0), Opacity: 1, Tint: 0,
            TintStrength: 0, MultiplyTint: false, Blend: BlendKind.Opaque));
        return frame;
    }

    [Fact]
    public void AStaticAssetDecodedAgainIsPreparedAgain()
    {
        // A language switch: the graphics native forgets its decoded images, so the asset id's next decode is another
        // language's file. A frame lowered before the switch can be drawn after the renderer forgot its pixels; the
        // next frame's newer image must replace what that frame left.
        var renderer = new ReferenceRenderer(new Sys4LogicalCanvas(1, 1));
        renderer.Draw(OneStaticDraw(new RgbaImage(1, 1, [10, 20, 30, 255])));
        Assert.Equal([10, 20, 30, 255], renderer.Pixels[..4]);

        renderer.Draw(OneStaticDraw(new RgbaImage(1, 1, [40, 50, 60, 255])));

        Assert.Equal([40, 50, 60, 255], renderer.Pixels[..4]);
    }

    [Fact]
    public void ASourceRectangleIsClippedToItsImageAsNativeClipsIt()
    {
        // gfx_object_blit_d3d9 (4.15D 0x469fa0): a left or top edge outside the surface moves onto it and the position
        // moves by as much before the object's transform; right and bottom are cut.
        int x = -1, y = -3, width = 5, height = 6;
        var matrix = new Affine2D(2, 0, 0, 2, 10, 20);
        Assert.True(FrameItem.ClipSourceToImage(3, 2, ref x, ref y, ref width, ref height, ref matrix));
        Assert.Equal((0, 0, 3, 2), (x, y, width, height));
        Assert.Equal(new Affine2D(2, 0, 0, 2, 12, 26), matrix);

        // Inside the image nothing changes; a rectangle with nothing inside it draws nothing and is left as it was.
        x = 1; y = 0; width = 2; height = 2; matrix = new Affine2D(1, 0, 0, 1, 4, 5);
        Assert.True(FrameItem.ClipSourceToImage(3, 2, ref x, ref y, ref width, ref height, ref matrix));
        Assert.Equal((1, 0, 2, 2, new Affine2D(1, 0, 0, 1, 4, 5)), (x, y, width, height, matrix));
        x = -4; y = 0; width = 4; height = 2;
        Assert.False(FrameItem.ClipSourceToImage(3, 2, ref x, ref y, ref width, ref height, ref matrix));
        Assert.Equal((-4, 0, 4, 2), (x, y, width, height));
    }

    [Fact]
    public void ARectangleStartingOutsideItsImageDrawsTheImageInPlaceAndNothingElse()
    {
        // 戦女神ZERO's FIELD scrolls a map picture's source rectangle past its left edge (DRAWMAP@0x2c2, source x =
        // -scroll on map 1's balcony). Native draws the picture from 1 px in and nothing left of it; reading on from the
        // row before (the reference renderer's old indexing) or dropping the draw (the GPU renderer's) are both wrong.
        byte[] texels =
        [
            10, 0, 0, 255, 20, 0, 0, 255, 30, 0, 0, 255,
            40, 0, 0, 255, 50, 0, 0, 255, 60, 0, 0, 255,
        ];
        var image = new RgbaImage(3, 2, texels);
        var frame = new FrameDescription { Width = 4, Height = 3, BackgroundRgb = 0x0000ff };
        frame.Items.Add(new FrameItem(
            FrameItemKind.Texture, image, AssetId: 7, Dynamic: false, DynamicKey: 0, ColorKey: -1, SourceX: -1,
            SourceY: -1, Width: 4, Height: 3, Matrix: new Affine2D(1, 0, 0, 1, 0, 0), Opacity: 1, Tint: 0,
            TintStrength: 0, MultiplyTint: false, Blend: BlendKind.Opaque));
        var renderer = new ReferenceRenderer(new Sys4LogicalCanvas(4, 3));

        renderer.Draw(frame);

        byte[] Pixel(int px, int py) => renderer.Pixels[((py * 4 + px) * 4)..((py * 4 + px) * 4 + 4)];
        byte[] background = Pixel(0, 0);
        Assert.Equal(background, Pixel(3, 0));
        Assert.Equal(background, Pixel(0, 1));
        Assert.Equal(background, Pixel(0, 2));
        Assert.Equal([10, 0, 0, 255], Pixel(1, 1));
        Assert.Equal([30, 0, 0, 255], Pixel(3, 1));
        Assert.Equal([40, 0, 0, 255], Pixel(1, 2));
        Assert.Equal([60, 0, 0, 255], Pixel(3, 2));
    }
}
