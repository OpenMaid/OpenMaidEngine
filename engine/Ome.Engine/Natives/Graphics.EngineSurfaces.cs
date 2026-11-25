using Ome.Engine.Model;

namespace Ome.Engine.Natives;

/// <summary>
/// The surfaces native's engine makes in the slots for itself, which op 0x208 sizes and op 0x24a counts as it does a
/// script's (docs/engine-re.md, "Engine-owned surfaces"): start-up's composite surfaces 36 and 37, which the release
/// worker never releases; the target ops 0x223 and 0x24f draw into; scratch surface 32, which the legacy screen
/// transitions draw their black or white through; and op 0x70's layout surface, which the VM creates as
/// <see cref="CreateSurface"/> does.
/// </summary>
public sealed partial class Graphics
{
    /// <summary>The composite surfaces start-up creates at the screen's size: 36 and 37 (4.24J
    /// gfx_create_composite_scratch_surfaces@0x47b530, 4.24N 0x47c880, 4.33G 0x481d70, 4.38D
    /// gfx_create_device_objects@0x4907f0). The mosaic transition copies its two surfaces into them, and the transition
    /// targets take their kind.</summary>
    public const int CompositeSurface = 0x24, SecondCompositeSurface = 0x25;

    /// <summary>The frame's surface, which native creates only for display:VirtualFullScreen's type 1, at the back
    /// buffer's size. The port draws its frame in canvas units and never creates it.</summary>
    public const int FrameSurface = 0x26;

    /// <summary>The scratch surface of op 0x32's mixed-axis copy and of the legacy screen transitions.</summary>
    public const int ScratchSurface = 0x20;

    /// <summary>The engine build's <c>GfxRules.GuardsFrameSurface</c>: whether start-up guards slot 38 from the release
    /// worker as well as 36 and 37 (4.24N and later).</summary>
    public bool GuardsFrameSurface { get; init; }

    /// <summary>
    /// Whether the release worker (retained_gfx_release_surface: op 0x1fa, the AutoFreeTex releases) leaves the slot as
    /// it is, record included. Start-up sets the guard of 36 and 37 (gfx manager +0xb61c[slot] in 4.24J and 4.24N,
    /// +0xb638 in 4.33G, +0xb650 in 4.38D), and of 38 from 4.24N; nothing clears one. A create or a load replaces a
    /// guarded slot's surface, as it does any other's.
    /// </summary>
    public bool IsReleaseGuarded(int slot)
        => slot is CompositeSurface or SecondCompositeSurface || slot == FrameSurface && GuardsFrameSurface;

    // Start-up's composite surfaces: render targets of kind 1 at pixel shader level 2 and of kind 2 below it. A native
    // with no screen (Graphics.Headless) makes none.
    private void CreateCompositeSurfaces()
    {
        if (_screenWidth <= 0 || _screenHeight <= 0) return;
        long kind = PixelShaderLevel >= 2 ? 1 : 2;
        CreateSurface(CompositeSurface, _screenWidth, _screenHeight, kind);
        CreateSurface(SecondCompositeSurface, _screenWidth, _screenHeight, kind);
    }

    /// <summary>
    /// Ops 0x223 and 0x24f (gfx_queue_surface_alpha_transition@0x47f440, gfx_queue_patterned_surface_transition;
    /// the later builds the same): the target becomes a surface of surface 36's kind. An empty slot is created at 36's
    /// size, a surface of another kind is created again at its own size, and one of 36's kind stays. Without surface
    /// 36 (a headless native), the slot's record is written as the create would write it and nothing is made.
    /// </summary>
    public void PrepareTransitionTarget(int slot)
    {
        if (!HoldsSurface(CompositeSurface) || !_slotDims.TryGetValue(CompositeSurface, out var composite))
        {
            State.RecordTransitionTargetCreated(slot);
            return;
        }
        long kind = SurfaceKind(CompositeSurface);
        if (!HoldsSurface(slot)) CreateSurface(slot, composite.W, composite.H, kind);
        else if (SurfaceKind(slot) != kind)
        {
            (int W, int H) own = _slotDims.GetValueOrDefault(slot);
            CreateSurface(slot, own.W, own.H, kind);
        }
    }

    /// <summary>
    /// A black or white fade of <paramref name="surfaceSlot"/> begun (screen_transition_begin@0x439da0 modes 0 to 3, ops
    /// 0x21, 0x22 and 0x24): begin creates scratch surface 32 at the transition's size and fills it, and the tick
    /// releases it when the fade ends (screen_transition_tick@0x43a7a0), so the slot is left with no surface and its
    /// record created.
    /// </summary>
    public void UseFadeScratch(int surfaceSlot)
    {
        var (width, height) = TransitionSize(surfaceSlot);
        CreateSurface(ScratchSurface, width, height);
        ReleaseSurface(ScratchSurface);
    }

    /// <summary>
    /// A transition finished on black or white (screen_transition_finalize@0x4399c0 with a negative surface): scratch
    /// surface 32 is created at the screen's size, filled with <paramref name="rgb"/> and kept.
    /// </summary>
    public void FinishTransitionOnColour(int rgb)
    {
        CreateSurface(ScratchSurface, _screenWidth, _screenHeight);
        RgbaImage image = _surfaceImages[ScratchSurface];
        byte r = (byte)(rgb >> 16), g = (byte)(rgb >> 8), b = (byte)rgb;
        byte[] pixels = image.Pixels;
        for (int at = 0; at < pixels.Length; at += 4)
        {
            pixels[at] = r;
            pixels[at + 1] = g;
            pixels[at + 2] = b;
            pixels[at + 3] = 255;
        }
    }

    /// <summary>
    /// Op 0x36's mosaic begun (screen_transition_begin mode 9): surfaces 36 and 37 hold copies of the two surfaces for
    /// the run and are created again as start-up made them when it ends (0x47b410), and scratch surface 32 is created
    /// blank at the source's size, cut to the screen, and kept.
    /// </summary>
    public void BeginMosaicScratch(int sourceSlot)
    {
        var (width, height) = TransitionSize(sourceSlot);
        CreateSurface(ScratchSurface, width, height);
    }

    // Begin's size (+0x2018, +0x201c): the surface's, cut to the screen's rectangle.
    private (int Width, int Height) TransitionSize(int surfaceSlot)
    {
        (int W, int H) surface = _slotDims.GetValueOrDefault(surfaceSlot);
        return (Math.Min(surface.W, _screenWidth), Math.Min(surface.H, _screenHeight));
    }

    // Whether the slot holds a surface (native's wrapper at gfx+0xa590[slot]): pixels or a loaded resource.
    private bool HoldsSurface(int slot) => _surfaceImages.ContainsKey(slot) || _surfaceResources.ContainsKey(slot);

    // Native's texture kind (+0x418): 1 and 2 are render targets; the port keeps no other kind apart.
    private long SurfaceKind(int slot)
        => _alphaRenderTargets.Contains(slot) ? 1 : _opaqueRenderTargets.Contains(slot) ? 2 : 0;
}
