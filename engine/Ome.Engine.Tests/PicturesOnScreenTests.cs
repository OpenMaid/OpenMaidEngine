using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using static GraphicsTestSupport;

/// <summary>The port's picture list (F5 and the driver's <c>pictures</c>; docs/modding/translating.md, step 8): the
/// catalog files behind what the screen shows, back to front, each once (Natives/Graphics.Pictures.cs).</summary>
public class PicturesOnScreenTests
{
    [Fact]
    public void EachPictureBehindTheScreenIsNamedOnceBackToFront()
    {
        (Graphics graphics, Dictionary<string, long> ids) = Build();
        GfxState gfx = graphics.State;
        graphics.LoadSurface(2, ids["BG001.AGF"], -1);
        gfx.BindDraw(100, 2, 0, 0, 2, 2, 0, 0);
        graphics.LoadSurface(3, ids["SO001.AGF"], -1);
        graphics.PublishSurfacePixels(3, Opaque());              // text drawn on the picture
        gfx.BindDraw(110, 3, 0, 0, 2, 2, 0, 0);
        graphics.LoadSurface(4, ids["PARTS.AGF"], -1);           // a sheet copied from, never shown itself
        graphics.CreateSurface(5, 2, 2);
        graphics.CopySurfaceRect(new SurfaceRectCopy(4, 5, 0, 0, 1, 1, 0, 0, 1, 1));
        gfx.BindDraw(120, 5, 0, 0, 2, 2, 0, 0);
        graphics.CreateSurface(6, 2, 2);
        graphics.PublishSurfacePixels(6, Opaque());              // text alone is no picture
        gfx.BindDraw(130, 6, 0, 0, 2, 2, 0, 0);
        gfx.BindDraw(140, 2, 0, 0, 2, 2, 0, 0);                  // the background again
        graphics.LoadSurface(7, ids["FLAME.AGF"], -1);
        gfx.BindDraw(150, 7, 0, 0, 2, 2, 0, 0);
        gfx.SetStaticObjectColorResolved(150, 1, 0, 0xffffff);   // drawn at opacity 0: nothing shows

        Assert.Equal(["BG001.AGF", "SO001.AGF", "PARTS.AGF"], Names(graphics));
        Assert.DoesNotContain(graphics.PicturesOnScreen(), picture => picture.Movie);

        graphics.CreateSurface(5, 2, 2);                         // made again: the copied piece is gone
        Assert.Equal(["BG001.AGF", "SO001.AGF"], Names(graphics));
        graphics.CopySurfaceRect(new SurfaceRectCopy(4, 5, 0, 0, 1, 1, 0, 0, 1, 1));
        gfx.SelectRenderTarget(5);
        graphics.ClearRenderTarget();                            // a clear forgets it too
        gfx.SelectRenderTarget(-1);
        Assert.Equal(["BG001.AGF", "SO001.AGF"], Names(graphics));
    }

    [Fact]
    public void ARenderTargetNamesWhatAPresentDrewIntoIt()
    {
        (Graphics graphics, Dictionary<string, long> ids) = Build();
        GfxState gfx = graphics.State;
        graphics.LoadSurface(2, ids["BG001.AGF"], -1);
        gfx.BindDraw(10, 2, 0, 0, 2, 2, 0, 0);
        graphics.CreateSurface(5, 2, 2);
        gfx.SelectRenderTarget(5);
        Assert.False(graphics.PresentFrame());
        gfx.SelectRenderTarget(-1);
        gfx.Release(10);
        Assert.Empty(graphics.PicturesOnScreen());

        gfx.BindDraw(20, 5, 0, 0, 2, 2, 0, 0);                   // the target shown, drawn from the background
        Assert.Equal(["BG001.AGF"], Names(graphics));
    }

    [Fact]
    public void TheListReadsAsOneLineOrWrapped()
    {
        PictureOnScreen[] pictures =
            [new("BG001.AGF", false), new("SO001.AGF", false), new("OP.AGF", true)];
        Assert.Equal("pictures: BG001.AGF, SO001.AGF, OP.AGF (movie)", PictureOnScreen.Describe(pictures));
        Assert.Equal("pictures: BG001.AGF, SO001.AGF,\n  OP.AGF (movie)", PictureOnScreen.Describe(pictures, 2));
        Assert.Equal("pictures: none", PictureOnScreen.Describe([]));
    }

    private static string[] Names(Graphics graphics)
        => graphics.PicturesOnScreen().Select(picture => picture.Name).ToArray();

    private static RgbaImage Opaque() => Image(2, 2, (_, _) => [1, 2, 3, 255]);

    // A catalog of pictures, each a 2x2 PNG the store serves as a language asset would. The first entry packs to id
    // 0, which a surface reads as no resource.
    private static (Graphics, Dictionary<string, long>) Build()
    {
        var catalog = Sys4AssetCatalog.Parse(SyntheticCatalog.Build(["ID0.AGF", "BG001.AGF", "SO001.AGF", "PARTS.AGF", "FLAME.AGF"]));
        var resources = new ResourceMap(catalog, new PngAssets());
        var ids = catalog.EnumerateAssets().ToDictionary(asset => asset.Asset.Name, asset => (long)asset.PackedId);
        return (new Graphics(resources, new FrameClock(), new Sys4LogicalCanvas(4, 4)), ids);
    }

    private sealed class PngAssets : IAssetStore
    {
        public Stream Open(AssetEntry entry) => new MemoryStream(ReadAll(entry));
        public byte[] ReadAll(AssetEntry entry) => PngWriter.Write(Opaque());
        public (byte[] Bytes, bool Png) ReadImage(AssetEntry entry) => (ReadAll(entry), true);
    }
}
