using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Sys4;

namespace Ome.Engine.Natives;

/// <summary>A texture a surface or a resource resolves to: its pixels, a name for the logs, its asset id, and whether
/// its pixels are replaced while it is shown (a script-drawn surface, a movie).</summary>
public readonly record struct SurfaceTexture(RgbaImage Image, string Name, int AssetId, bool IsDynamic);

// The surface table (native's 1,000 surface records) and its pixels. Each op that native runs as one call is one
// method here, doing the record (GfxState) and the pixels in native's order.
public sealed partial class Graphics
{
    /// <summary>The bytes of decoded catalog images the native keeps by default. A surface holds only its resource id
    /// and each frame resolves the pixels, so the images drawn lately stay and the rest are decoded again when next
    /// drawn. What the frame being lowered draws stays even past the budget. Native AGE keeps a surface's pixels until
    /// the surface is released.</summary>
    public const long DecodedImageBudgetBytes = 256L << 20;

    private readonly ByteBudgetCache<int, RgbaImage?> _images;            // packed catalog id -> decoded pixels
    // Mutable AGE surfaces are published by replacing immutable RgbaImage snapshots, so a renderer can finish reading
    // an old frame while the VM prepares a copied-rectangle update.
    private readonly Dictionary<int, RgbaImage> _surfaceImages = new();
    private readonly Dictionary<int, long> _surfaceColorKeys = new();
    private readonly Dictionary<int, long> _surfaceResources = new();    // surface slot -> packed catalog id
    // slot -> dimensions of the currently allocated surface. Slot 0 begins as the selected game's logical canvas, but
    // op 0x1fa releases it like any other slot; later queries return 0x0.
    private readonly Dictionary<int, (int W, int H)> _slotDims = new();
    // The slots op 0x1f8 made with kind 1, an A8R8G8B8 render target (the surface's +0x418): native draws a colour
    // mode 2 object into one ONE/ZERO (gfx_object_blit_d3d9; docs/engine-re.md, "Object blend modes").
    private readonly HashSet<int> _alphaRenderTargets = new();
    // The slots op 0x1f8 made with kind 2, an X8R8G8B8 render target on any device that has the format (display
    // +0x464): it has no alpha channel, so what native draws into it (a clear, a present, a fill, a copy) samples
    // opaque.
    private readonly HashSet<int> _opaqueRenderTargets = new();

    /// <summary>The engine build's <c>GfxRules.SpriteAlphaTest</c>: whether an object's draw keeps the alpha test
    /// D3DX's sprite turns on, so a copy (colour mode 2 into a kind-1 render target) leaves out its texels of alpha
    /// 0.</summary>
    public bool SpriteAlphaTest { get; init; } = true;

    /// <summary><c>--gfx-log</c>: print set-texture/create-texture slot assignments.</summary>
    public bool TraceOps { get; set; }

    /// <summary>The CLI's <c>gfx</c> oracle: one line per surface op (create, load, release, size, draw).</summary>
    public Action<string>? OpTrace { get; set; }

    // ---- the ops ----

    /// <summary>Op 0x1f8: release the slot, then allocate a blank surface of the given size there. Its
    /// <paramref name="kind"/> is native's texture kind (gfx_surface_texture_create: 1 and 2 are render targets, 1 with
    /// an alpha channel; anything else a managed texture); the port keeps whether it is 1.</summary>
    public void CreateSurface(int slot, int width, int height, long kind = 0)
    {
        ReleaseSurfacePixels(slot);
        State.CreateSurface(slot);
        CreateSurfacePixels(slot, width, height);
        if (kind == 1) _alphaRenderTargets.Add(slot);
        if (kind == 2)
        {
            _opaqueRenderTargets.Add(slot);
            KeepOpaque(slot, _surfaceImages[slot]);
        }
        OpTrace?.Invoke($"create-texture slot={slot} {width}x{height}" + (kind != 0 ? $" kind={kind}" : ""));
    }

    /// <summary>Ops 0x1f9 and 0x249: release the slot, then load a resource into it with a colour key. Op 0x249
    /// constructs native's mode-1 surface subclass (<paramref name="rawTextureMode"/>); the pixels are the same.
    /// Returns the box AGE throws for a load it refuses (<see cref="ResourceMap.CheckPictureLoad"/>), null when it
    /// loads. A record AGE cannot open leaves the slot's pixels and record, without its movie, which the ops delete
    /// before the open. A record whose header AGE refuses leaves the slot released and its record's resource id -1, as
    /// gfx_surface_load_asset leaves it. A load the port cannot decode but AGE would draw is traced and refuses
    /// nothing.</summary>
    public AgeBoxMessage? LoadSurface(int slot, long resourceId, long colorKey, bool rawTextureMode = false)
    {
        if (_res?.CheckPictureLoad(resourceId) is { } refused)
        {
            if (refused.Opened)
            {
                ReleaseSurfacePixels(slot);
                State.ClearSurface(slot);
            }
            else
            {
                foreach (ISurfaceListener listener in _listeners) listener.SurfaceMovieDeleted(slot);
            }
            OpTrace?.Invoke($"set-texture slot={slot} res=0x{resourceId:x} refused"
                            + (refused.Opened ? $" ({_res.RecordName(resourceId)}: not a picture)" : " (no record opens)"));
            return refused.Box;
        }
        ReleaseSurfacePixels(slot);
        State.SetSurface(slot, resourceId, colorKey, rawTextureMode);
        LoadSurfacePixels(resourceId, slot, colorKey);
        return null;
    }

    /// <summary>Op 0x1fb: bind object <paramref name="handle"/> to the slot's surface with a source rectangle and a
    /// position. Nothing draws now: the frame draws the retained objects.</summary>
    public void DrawTexture(long handle, int slot, int srcX, int srcY, int width, int height, int dstX, int dstY)
    {
        State.BindDraw(handle, slot, srcX, srcY, width, height, dstX, dstY);
        if (OpTrace is { } trace)
        {
            long resourceId;
            bool loaded;
            loaded = _surfaceResources.TryGetValue(slot, out resourceId);
            string file = loaded ? _res?.ResolveTexture(resourceId)?.Name ?? "<none>" : "<none>";
            trace($"draw-texture slot={slot} src=({srcX},{srcY} {width}x{height}) dst=({dstX},{dstY}) file={file}");
        }
    }

    /// <summary>A numbered load's restore loop reopens a flagged record through gfx_surface_load_asset, without a
    /// release first. A load AGE refuses raises no box there: the loop's catch writes the open's message to the host
    /// trace, and the loop writes 「画像ファイル %s の読み込みに失敗しました」 there, then goes on with the next record (here
    /// a diagnostic). A refused header leaves the slot released and its record's resource id -1, as
    /// gfx_surface_load_asset leaves it; a record that does not open leaves the slot and its restored record.</summary>
    public void ReloadSurface(int slot, long resourceId, long colorKey)
    {
        if (_res?.CheckPictureLoad(resourceId) is { } refused)
        {
            if (refused.Opened)
            {
                ReleaseSurfacePixels(slot);
                State.ClearSurface(slot);
            }
            _print($"load: surface {slot} not restored: {refused.Box.Text(AgeBoxText.JapaneseText)} "
                   + "(AGE writes it to its trace and goes on)");
            return;
        }
        State.SetSurface(slot, resourceId, colorKey);
        LoadSurfacePixels(resourceId, slot, colorKey);
    }

    /// <summary>Op 0x1fa: release one surface. The release worker leaves a guarded slot as it is, record included
    /// (<see cref="IsReleaseGuarded"/>).</summary>
    public void ReleaseSurface(int slot)
    {
        if (IsReleaseGuarded(slot))
        {
            OpTrace?.Invoke($"release-surface slot={slot} guarded");
            return;
        }
        ReleaseSurfacePixels(slot);
        State.ClearSurface(slot);
    }

    /// <summary>The release that ops 0x1f8, 0x1f9 and 0x249 run before they allocate.</summary>
    private void ReleaseSurfacePixels(int slot)
    {
        ReleaseSlotPixels(slot);
        OpTrace?.Invoke($"release-surface slot={slot}");
    }

    /// <summary>The release worker over a range (op 0x23d's fixed [42,1000), and the AutoFreeTex releases over all
    /// 1,000 slots). It passes over a guarded slot (<see cref="IsReleaseGuarded"/>).</summary>
    public void ReleaseSurfaceRange(int firstSlot, int count)
    {
        State.ReleaseSurfaceRange(firstSlot, count, IsReleaseGuarded);
        ReleaseSurfacePixelRange(firstSlot, count);
        OpTrace?.Invoke($"release-surface-range first={firstSlot} count={count}");
    }

    /// <summary>Op 0x208: the size of the slot's surface, 0 × 0 when none.</summary>
    public (int Width, int Height) SurfaceSize(int slot)
    {
        var size = _slotDims.TryGetValue(slot, out var d) ? (d.W, d.H) : (0, 0);
        OpTrace?.Invoke($"get-tex-size slot={slot} -> {size.Item1}x{size.Item2}");
        return size;
    }

    /// <summary>display:TextureBit, which engine_initialize_graphics_from_settings reads once (32 registered): the bits
    /// a texel takes in <see cref="TextureMemory"/>.</summary>
    public int TextureBits { get; init; } = 32;

    /// <summary>
    /// Op 0x24a: native's two texture memory counters, the bytes of the textures its surfaces hold, the render targets
    /// (kinds 1 and 2) first, then the rest. gfx_surface_texture_create adds a texture's width × height ×
    /// display:TextureBit / 8 to one of them and gfx_surface_texture_release subtracts it, in 32-bit arithmetic. A
    /// large image's tiles together have its size, so each surface counts its own size, and native's device gives every
    /// texture the size asked for. Native's count was its surfaces' at the logo, the title and the first field
    /// (Verita, 2026-11-25), but for one texture the port leaves out: the display background's, which takes its size
    /// from the Direct3D device (1024 × 428 there, 1,753,088 bytes; docs/engine-re.md, "Texture memory counters").
    /// </summary>
    public (int RenderTargets, int Others) TextureMemory()
    {
        int renderTargets = 0, others = 0;
        foreach (int slot in LiveSurfaceSlots())
        {
            if (!_slotDims.TryGetValue(slot, out var size)) continue;
            int bytes = unchecked(size.H * size.W * TextureBits) / 8;
            if (_alphaRenderTargets.Contains(slot) || _opaqueRenderTargets.Contains(slot))
                renderTargets = unchecked(renderTargets + bytes);
            else others = unchecked(others + bytes);
        }
        return (renderTargets, others);
    }

    /// <summary>Op 0x20b: a clipped alpha/RGB fill of a surface.</summary>
    public void FillSurfaceRect(SurfaceRectFill fill)
    {
        State.MarkSurfaceContentWritten(fill.SurfaceSlot);
        RgbaImage? destination = SurfacePixels(fill.SurfaceSlot);
        if (destination == null && _slotDims.TryGetValue(fill.SurfaceSlot, out var dimensions)
            && dimensions.W >= 0 && dimensions.H >= 0)
            destination = new RgbaImage(dimensions.W, dimensions.H,
                new byte[checked(dimensions.W * dimensions.H * 4)]);
        if (destination != null)
        {
            var updated = new RgbaImage(destination.Width, destination.Height,
                (byte[])destination.Pixels.Clone());
            if (RgbaSurfaceOps.FillRect(updated, fill.X, fill.Y, fill.Width, fill.Height,
                                        unchecked((byte)fill.Alpha), fill.Rgb))
            {
                _surfaceImages[fill.SurfaceSlot] = KeepOpaque(fill.SurfaceSlot, updated);
                RequestPresentation();
            }
        }
        _timeline?.Event("surface-fill", new()
        {
            ["surface"] = fill.SurfaceSlot, ["x"] = fill.X, ["y"] = fill.Y,
            ["w"] = fill.Width, ["h"] = fill.Height, ["alpha"] = fill.Alpha, ["rgb"] = fill.Rgb,
        });
    }

    /// <summary>
    /// Op 0x32 (gfx_scale_copy_surface_rect@0x47c6c0; 4.33G the same): a scaled copy. When one axis grows and the
    /// other shrinks, native goes through scratch surface 32: it creates a blank surface there as large as the larger
    /// extent on each axis (gfx_surface_create_blank, which destroys any surface the slot held), enlarges the source
    /// into it, shrinks it into the destination and releases it (retained_gfx_release_surface). Slot 32 is left with
    /// no surface, its record's resource id -1 and its created flag 1, whatever a script kept there. Native decides on
    /// its clipped rectangles; the port decides on the requested ones, which differ only for a rectangle that crosses
    /// a surface's edge. Retail never takes this path: Kamidori's two calls only shrink.
    /// </summary>
    public void ScaleCopySurfaceRect(SurfaceRectCopy copy)
    {
        bool mixed = copy.Width > 0 && copy.Height > 0 && copy.DestinationWidth > 0 && copy.DestinationHeight > 0
                     && (copy.DestinationWidth < copy.Width && copy.DestinationHeight > copy.Height
                         || copy.DestinationWidth > copy.Width && copy.DestinationHeight < copy.Height);
        if (!mixed || !CopyResolves(copy))
        {
            CopySurfaceRect(copy);
            return;
        }
        CreateSurface(ScratchSurface, Math.Max(copy.Width, copy.DestinationWidth),
                      Math.Max(copy.Height, copy.DestinationHeight));
        CopySurfaceRect(copy);
        ReleaseSurface(ScratchSurface);
    }

    // Whether both of a copy's surfaces are there: native traces a missing one and copies nothing.
    private bool CopyResolves(SurfaceRectCopy copy)
        => SurfacePixels(copy.SourceSurface) != null
           && (SurfacePixels(copy.DestinationSurface) != null
               || _slotDims.TryGetValue(copy.DestinationSurface, out var dimensions)
                  && dimensions.W >= 0 && dimensions.H >= 0);

    /// <summary>Ops 0x207 and 0x32: a paired-clipped (0x207) or scaled (0x32) copy between surfaces.</summary>
    public void CopySurfaceRect(SurfaceRectCopy copy)
    {
        State.MarkSurfaceContentWritten(copy.DestinationSurface);
        RgbaImage? source = SurfacePixels(copy.SourceSurface);
        RgbaImage? destination = SurfacePixels(copy.DestinationSurface);
        if (destination == null && _slotDims.TryGetValue(copy.DestinationSurface, out var dimensions)
            && dimensions.W >= 0 && dimensions.H >= 0)
            destination = new RgbaImage(dimensions.W, dimensions.H,
                new byte[checked(dimensions.W * dimensions.H * 4)]);
        if (source == null || destination == null)
        {
            System.Console.Error.WriteLine(
                $"surface copy unresolved source={copy.SourceSurface} destination={copy.DestinationSurface}");
            return;
        }

        var updated = new RgbaImage(destination.Width, destination.Height, (byte[])destination.Pixels.Clone());
        bool copied = copy.Width == copy.DestinationWidth && copy.Height == copy.DestinationHeight
            ? RgbaSurfaceOps.CopyRect(source, updated, copy.SourceX, copy.SourceY,
                                      copy.Width, copy.Height, copy.DestinationX, copy.DestinationY)
            : RgbaSurfaceOps.ScaleCopyRect(source, updated, copy.SourceX, copy.SourceY,
                                           copy.Width, copy.Height, copy.DestinationX, copy.DestinationY,
                                           copy.DestinationWidth, copy.DestinationHeight,
                                           opaqueSource: SurfaceKind(copy.SourceSurface) != 0);
        if (copied)
        {
            _surfaceImages[copy.DestinationSurface] = KeepOpaque(copy.DestinationSurface, updated);
            NoteCopySources(copy.SourceSurface, copy.DestinationSurface);
            RequestPresentation();
        }
        _timeline?.Event("surface-copy", new()
        {
            ["source"] = copy.SourceSurface, ["source_x"] = copy.SourceX, ["source_y"] = copy.SourceY,
            ["w"] = copy.Width, ["h"] = copy.Height,
            ["source_w"] = copy.Width, ["source_h"] = copy.Height,
            ["destination"] = copy.DestinationSurface,
            ["destination_x"] = copy.DestinationX, ["destination_y"] = copy.DestinationY,
            ["destination_w"] = copy.DestinationWidth,
            ["destination_h"] = copy.DestinationHeight,
        });
    }

    /// <summary>
    /// Op 0x35, native MosaicTexture (gfx_mosaic_texture: 4.24J 0x479ac0, 4.24N 0x47add0, 4.33G 0x480400, 4.38D
    /// 0x48ee10; docs/engine-re.md, "Op 0x35, the mosaic copy"). The destination's record reads created first, whatever
    /// follows. Native refuses with its trace a missing surface, the same surface twice, a dot of 0 and a rectangle
    /// outside its surface. Otherwise it walks the destination rectangle's size from the source rectangle's origin, so
    /// the source rectangle only places the reads, and each dot x dot block takes the source pixel at its top-left
    /// corner, opaque when the source is a render target. The destination keeps its pixels outside the rectangle.
    /// </summary>
    public SurfaceMosaicResult MosaicSurfaceRect(SurfaceMosaicCopy copy)
    {
        State.MarkSurfaceContentWritten(copy.DestinationSurface);
        if (MosaicRefusal(copy) is { } refusal)
            return new(SurfaceMosaicOutcome.Refused, "関数：MosaicTexture エラー：" + refusal);
        int width = unchecked(copy.DestinationRight - copy.DestinationLeft);
        int height = unchecked(copy.DestinationBottom - copy.DestinationTop);
        if (width <= 0 || height <= 0) return new(SurfaceMosaicOutcome.Copied);
        long dot = copy.Dot;
        if (dot < 0)
        {
            // Native indexes its line buffer at x / dot, which is negative from x = -dot on.
            if (width > -dot)
                return new(SurfaceMosaicOutcome.WritesBeforeLineBuffer, $"dot {copy.Dot} over {width} pixels");
            dot = -dot;   // one column, and the rows of -dot's blocks
        }

        RgbaImage? source = SurfacePixels(copy.SourceSurface);
        RgbaImage? destination = SurfacePixels(copy.DestinationSurface);
        if (destination == null && _slotDims.TryGetValue(copy.DestinationSurface, out var dimensions)
            && dimensions.W >= 0 && dimensions.H >= 0)
            destination = new RgbaImage(dimensions.W, dimensions.H, new byte[checked(dimensions.W * dimensions.H * 4)]);
        if (source == null || destination == null)
        {
            System.Console.Error.WriteLine(
                $"surface mosaic unresolved source={copy.SourceSurface} destination={copy.DestinationSurface}");
            return new(SurfaceMosaicOutcome.Copied);
        }
        // The last pixel each axis samples, which the source rectangle's test does not cover.
        long lastX = copy.SourceLeft + (width - 1) / dot * dot, lastY = copy.SourceTop + (height - 1) / dot * dot;
        if (lastX >= source.Width || lastY >= source.Height)
            return new(SurfaceMosaicOutcome.ReadsPastSource,
                       $"pixel ({lastX},{lastY}) of surface {copy.SourceSurface}, which is {source.Width}x{source.Height}");

        var updated = new RgbaImage(destination.Width, destination.Height, (byte[])destination.Pixels.Clone());
        bool opaque = SurfaceKind(copy.SourceSurface) != 0;   // native forces the alpha of a kind other than 0
        byte[] from = source.Pixels, to = updated.Pixels;
        for (int y = 0; y < height; y++)
        {
            long sampleRow = (copy.SourceTop + (y - y % dot)) * source.Width + copy.SourceLeft;
            int row = (copy.DestinationTop + y) * updated.Width + copy.DestinationLeft;
            for (int x = 0; x < width; x++)
            {
                int at = checked((int)(sampleRow + (x - x % dot)) * 4), into = (row + x) * 4;
                to[into] = from[at];
                to[into + 1] = from[at + 1];
                to[into + 2] = from[at + 2];
                to[into + 3] = opaque ? (byte)255 : from[at + 3];
            }
        }
        _surfaceImages[copy.DestinationSurface] = KeepOpaque(copy.DestinationSurface, updated);
        NoteCopySources(copy.SourceSurface, copy.DestinationSurface);
        RequestPresentation();
        _timeline?.Event("surface-mosaic", new()
        {
            ["source"] = copy.SourceSurface, ["source_x"] = copy.SourceLeft, ["source_y"] = copy.SourceTop,
            ["destination"] = copy.DestinationSurface,
            ["destination_x"] = copy.DestinationLeft, ["destination_y"] = copy.DestinationTop,
            ["w"] = width, ["h"] = height, ["dot"] = copy.Dot,
        });
        return new(SurfaceMosaicOutcome.Copied);
    }

    // MosaicTexture's refusals in its order, each the text after 「関数：MosaicTexture エラー：」.
    private string? MosaicRefusal(SurfaceMosaicCopy copy)
    {
        if (!HoldsSurface(copy.SourceSurface))
            return $"転送元テクスチャが作成されていません． TEXTURE={copy.SourceSurface}";
        if (!HoldsSurface(copy.DestinationSurface))
            return $"転送先テクスチャが作成されていません． TEXTURE={copy.DestinationSurface}";
        if (copy.SourceSurface == copy.DestinationSurface)
            return $"転送先と転送元が同じですTEXTURE = {copy.DestinationSurface}";
        if (copy.Dot == 0) return "モザイクサイズの指定が不正ですsize = 0";
        if (OutsideSurface(copy.SourceSurface, copy.SourceLeft, copy.SourceTop, copy.SourceRight, copy.SourceBottom))
            return $"転送元の座標が不正です ({copy.SourceLeft},{copy.SourceTop})-({copy.SourceRight},{copy.SourceBottom})";
        if (OutsideSurface(copy.DestinationSurface, copy.DestinationLeft, copy.DestinationTop, copy.DestinationRight,
                           copy.DestinationBottom))
            return $"転送先の座標が不正です ({copy.DestinationLeft},{copy.DestinationTop})-"
                   + $"({copy.DestinationRight},{copy.DestinationBottom})";
        return null;
    }

    // MosaicTexture's rectangle test: left | top | (width - right) | (height - bottom) is negative, in 32 bits.
    private bool OutsideSurface(int slot, int left, int top, int right, int bottom)
    {
        (int W, int H) size = _slotDims.GetValueOrDefault(slot);
        return unchecked(left | top | (size.W - right) | (size.H - bottom)) < 0;
    }

    /// <summary>Op 0x20e: clear the selected render target's pixels; the backbuffer (-1) clears at its next
    /// publication.</summary>
    public void ClearRenderTarget()
    {
        int surfaceSlot = State.CurrentRenderTargetSlot;
        if (surfaceSlot < 0)
        {
            _scene.BackbufferClearPending = true;
        }
        else
        {
            // A fresh buffer rather than an in-place clear, as every other surface write: the GPU texture cache
            // sees a changed dynamic surface by its pixel array's identity, and the front end may be drawing a frame
            // that holds the published one.
            _surfaceCopySources.Remove(surfaceSlot);
            if (_surfaceImages.TryGetValue(surfaceSlot, out var image))
                _surfaceImages[surfaceSlot] = KeepOpaque(
                    surfaceSlot, new RgbaImage(image.Width, image.Height, new byte[image.Pixels.Length]));
            else if (_slotDims.TryGetValue(surfaceSlot, out var dimensions)
                     && dimensions.W > 0 && dimensions.H > 0)
                _surfaceImages[surfaceSlot] = KeepOpaque(surfaceSlot, new RgbaImage(
                    dimensions.W, dimensions.H,
                    new byte[checked(dimensions.W * dimensions.H * 4)]));
        }
        _timeline?.Event("render-target-clear", new() { ["surface"] = surfaceSlot });
    }

    // ---- pixels ----

    // A render target without an alpha channel (kind 2) takes every write at alpha 255, as an X8R8G8B8 texture samples.
    // The image is the caller's fresh copy, set in place.
    private RgbaImage KeepOpaque(int slot, RgbaImage image)
    {
        if (!_opaqueRenderTargets.Contains(slot)) return image;
        byte[] pixels = image.Pixels;
        for (int index = 3; index < pixels.Length; index += 4) pixels[index] = 255;
        return image;
    }

    private void CreateSurfacePixels(int slot, int width, int height)
    {
        _surfaceResources.Remove(slot);
        _surfaceCopySources.Remove(slot);
        _alphaRenderTargets.Remove(slot);
        _opaqueRenderTargets.Remove(slot);
        int safeWidth = System.Math.Max(0, width);
        int safeHeight = System.Math.Max(0, height);
        _surfaceImages[slot] = new RgbaImage(safeWidth, safeHeight,
            new byte[checked(safeWidth * safeHeight * 4)]);
        _surfaceColorKeys.Remove(slot);
        _slotDims[slot] = (safeWidth, safeHeight);
        if (TraceOps) _print($"[op] create-texture slot={slot} {width}x{height}");
    }

    // The catalog image is decoded synchronously on the VM thread, so a size queried at once sees its real size.
    private void LoadSurfacePixels(long resourceId, int slot, long colorKey)
    {
        _surfaceImages.Remove(slot);
        _surfaceCopySources.Remove(slot);
        _alphaRenderTargets.Remove(slot);
        _opaqueRenderTargets.Remove(slot);
        _surfaceColorKeys[slot] = colorKey;
        _surfaceResources[slot] = resourceId;
        var asset = _res?.ResolveTexture(resourceId);
        var image = asset != null ? Decode(asset) : null;
        _slotDims[slot] = image != null ? (image.Width, image.Height) : (0, 0);
        if (TraceOps) _print($"[op] set-texture slot={slot} resId=0x{resourceId:x} -> {(asset?.Name ?? "<none>")}");
        OpTrace?.Invoke($"set-texture slot={slot} res=0x{resourceId:x} -> {(asset?.Name ?? "<unresolved>")}"
                        + (image == null ? " [NO AGF]" : ""));
    }

    private void ReleaseSlotPixels(int slot)
    {
        _renderTargetSnapshots.Remove(slot);
        // Native deletes the slot's movie object first, finished or not.
        foreach (ISurfaceListener listener in _listeners) listener.SurfaceReleasing(slot);
        _surfaceImages.Remove(slot);
        _surfaceCopySources.Remove(slot);
        _surfaceColorKeys.Remove(slot);
        _surfaceResources.Remove(slot);
        _slotDims.Remove(slot);
        _alphaRenderTargets.Remove(slot);
        _opaqueRenderTargets.Remove(slot);
        foreach (ISurfaceListener listener in _listeners) listener.SurfaceReleased(slot);
    }

    private void ReleaseSurfacePixelRange(int firstSlot, int count)
    {
        foreach (ISurfaceListener listener in _listeners) listener.SurfaceRangeReleasing(firstSlot, count);
        int end = checked(firstSlot + count);
        for (int slot = firstSlot; slot < end; slot++)
            if (!IsReleaseGuarded(slot)) _renderTargetSnapshots.Remove(slot);
        for (int slot = firstSlot; slot < end; slot++)
        {
            if (IsReleaseGuarded(slot)) continue;
            _surfaceImages.Remove(slot);
            _surfaceCopySources.Remove(slot);
            _surfaceColorKeys.Remove(slot);
            _slotDims.Remove(slot);
            _alphaRenderTargets.Remove(slot);
            _opaqueRenderTargets.Remove(slot);
        }
        for (int slot = firstSlot; slot < end; slot++)
            if (!IsReleaseGuarded(slot)) _surfaceResources.Remove(slot);
        foreach (ISurfaceListener listener in _listeners) listener.SurfaceRangeReleased(firstSlot, count);
        _timeline?.Event("surface-range-release", new() { ["first"] = firstSlot, ["count"] = count });
    }

    /// <summary>A stable copy of a surface's pixels, or null when it has none (op 0x1ae's thumbnail).</summary>
    public RgbaImage? CaptureSurfacePixels(int slot)
    {
        RgbaImage? image = SurfacePixels(slot);
        return image == null
            ? null
            : new RgbaImage(image.Width, image.Height, (byte[])image.Pixels.Clone());
    }

    /// <summary>Replace a surface's pixels from outside (op 0x1af's thumbnail): the surface forgets its resource and
    /// colour key, and the layouts retained on it go.</summary>
    public bool ReplaceSurfacePixels(int slot, RgbaImage image)
    {
        if (image.Width <= 0 || image.Height <= 0
            || image.Pixels.Length != checked(image.Width * image.Height * 4))
            return false;
        _surfaceImages[slot] =
            new RgbaImage(image.Width, image.Height, (byte[])image.Pixels.Clone());
        _surfaceColorKeys.Remove(slot);
        _surfaceResources.Remove(slot);
        _surfaceCopySources.Remove(slot);
        foreach (ISurfaceListener listener in _listeners) listener.SurfaceReplaced(slot);
        _slotDims[slot] = (image.Width, image.Height);
        return true;
    }

    /// <summary>A surface's current pixels: its own, or its resource's with the colour key applied; null when it has
    /// neither.</summary>
    public RgbaImage? SurfacePixels(int slot)
    {
        if (_surfaceImages.TryGetValue(slot, out var mutable)) return mutable;
        long resourceId;
        if (!_surfaceResources.TryGetValue(slot, out resourceId)) return null;
        var resolved = ResolveResIdTexture(resourceId);
        if (resolved == null) return null;
        long colorKey;
        colorKey = _surfaceColorKeys.GetValueOrDefault(slot, -1);
        return RgbaSurfaceOps.WithColorKey(resolved.Value.Image, colorKey);
    }

    /// <summary>Publish new pixels for a surface (text drawn into it, a movie mask's capture). The surface keeps its
    /// resource and colour key; <paramref name="updateSize"/> also records the image's size as the surface's.</summary>
    public void PublishSurfacePixels(int slot, RgbaImage image, bool updateSize = true)
    {
        _surfaceImages[slot] = image;
        if (updateSize) _slotDims[slot] = (image.Width, image.Height);
    }

    /// <summary>Take a surface's own pixels away (a movie starting on it keeps them until its first sample).</summary>
    public RgbaImage? TakeSurfacePixels(int slot)
    {
        return _surfaceImages.Remove(slot, out RgbaImage? image) ? image : null;
    }

    /// <summary>The surface draws without a colour key (a movie bound to it).</summary>
    public void ForgetSurfaceColorKey(int slot)
    {
        _surfaceColorKeys.Remove(slot);
    }

    /// <summary>Record a surface's size without pixels (a movie's before its first frame).</summary>
    public void SetSurfaceSize(int slot, int width, int height) => _slotDims[slot] = (width, height);

    /// <summary>Forget a surface's size (a movie that failed to start).</summary>
    public void ForgetSurfaceSize(int slot) => _slotDims.Remove(slot);

    /// <summary>The surface's size, if it has one.</summary>
    public bool TryGetSurfaceSize(int slot, out (int W, int H) size) => _slotDims.TryGetValue(slot, out size);

    // ---- texture resolution ----

    /// <summary>Resolve a resource through universal packed addressing and decode it from the loose-first asset
    /// store; a movie resource resolves to its playing frame, or to nothing.</summary>
    public SurfaceTexture? ResolveResIdTexture(long resId)
    {
        if (_movies.TryResolveResource(resId, out var movie) && movie != null)
            return new SurfaceTexture(movie.Image, movie.Name, movie.AssetId, true);
        // Movie payloads use the same .AGF extension as still images. Do not misclassify the MPEG program stream
        // before its first frame or during the cleanup frame after its surface binding is detached. Packed catalog
        // identity is immutable, so a resource which entered the typed movie path remains a movie even when it has
        // no live playback.
        if (_movies.IsKnownMovieResource(resId)) return null;
        var asset = _res?.ResolveTexture(resId);
        var image = asset != null ? Decode(asset) : null;
        return asset != null && image != null ? new SurfaceTexture(image, asset.Name, asset.PackedId, false) : null;
    }

    /// <summary>What a surface shows: a movie mask's capture, a movie's frame, its own pixels, or its resource.
    /// A movie bound without a frame shows nothing.</summary>
    public SurfaceTexture? ResolveSurfaceTexture(int surfaceSlot, long fallbackResourceId)
    {
        if (_movies.IsMaskedSurface(surfaceSlot))
            if (_surfaceImages.TryGetValue(surfaceSlot, out var masked))
                return new SurfaceTexture(masked, $"<movie-mask:{surfaceSlot}>", int.MinValue + surfaceSlot, true);
        if (_movies.TryResolveSurface(surfaceSlot, out var movie) && movie != null)
            return new SurfaceTexture(movie.Image, movie.Name, movie.AssetId, true);
        if (_movies.IsBound(surfaceSlot)) return null;
        if (_surfaceImages.TryGetValue(surfaceSlot, out var surface))
            return new SurfaceTexture(surface, $"<surface:{surfaceSlot}>", int.MinValue + surfaceSlot, true);
        return fallbackResourceId != 0 ? ResolveResIdTexture(fallbackResourceId) : null;
    }

    /// <summary>The packed resource id surface <paramref name="slot"/> loaded, or null when it loaded none (released,
    /// created blank, or given pixels from outside).</summary>
    public long? SurfaceResource(int slot)
    {
        return _surfaceResources.TryGetValue(slot, out long resourceId) ? resourceId : null;
    }

    /// <summary>The slots holding a surface (pixels or a loaded resource), for the timeline.</summary>
    public int[] LiveSurfaceSlots()
    {
        var slots = new SortedSet<int>();
        slots.UnionWith(_surfaceImages.Keys);
        slots.UnionWith(_surfaceResources.Keys);
        return slots.ToArray();
    }

    /// <summary>Drops the decoded images, so each is read again when next drawn: after a language switch, its
    /// language assets apply from the next load (localization spec §12).</summary>
    public void ForgetDecodedImages()
    {
        _images.Clear();
    }

    /// <summary>The bytes and count of the decoded catalog images the native keeps.</summary>
    public long DecodedImageBytes => _images.Bytes;
    public int DecodedImageCount => _images.Count;

    private RgbaImage? Decode(AssetEntry asset)
    {
        if (_images.TryGet(asset.PackedId, out var cached)) return cached;
        RgbaImage? image;
        try { image = _res!.DecodeTexture(asset); }
        catch (System.Exception e)
        {
            _print($"AGF decode failed {asset.Name}: {e.Message}");
            image = null;
        }
        _images.Set(asset.PackedId, image, image?.Pixels.LongLength ?? 0);
        _images.Trim();
        return image;
    }
}
