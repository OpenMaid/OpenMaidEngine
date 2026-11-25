namespace Ome.Engine.Model;

/// <summary>
/// Inverse-mapped RGBA8 affine compositor used by the Godot host and pure tests. It samples as native AGE's D3D9
/// sprite does (Himegari gfx_object_blit_d3d9@0x4774c0, SYS4433 gfx_draw_surface_through_sprite@0x47dac0; docs/
/// engine-re.md, "Object sampling"): a destination pixel is covered when its centre maps inside the source
/// rectangle, and takes the bilinear blend of the four texels around that point, with 8-bit weights and
/// coordinates clamped to the source image's edge (D3DTADDRESS_CLAMP), not to the rectangle. An integer
/// translation puts every pixel centre on a texel centre, so it copies texels exactly. It blends as the hardware and
/// the GPU renderer do (docs/engine-re.md, "The blend's arithmetic"): the fragment, the texel's colour tinted and its
/// alpha times the item's opacity, is worked out in floats and written to 8 bits (FragmentUnorm), and the blend is
/// worked out in floats and rounded to the nearest value. AGE's screen and the GPU renderer's agree to the pixel;
/// integer blends that truncated left a translucent layer up to two levels away.
/// </summary>
public static partial class SoftwareAffineRasterizer
{
    /// <summary>The bilinear weights' resolution: 8 bits of subtexel position, as D3D9 hardware filters.</summary>
    private const int WeightOne = 256;

    public static void BlitRgba(byte[] dst, int dstW, int dstH, byte[] src, int srcW, int srcH,
                                int srcX, int srcY, int width, int height, Affine2D localToDest,
                                long tint, float tintStrength, float opacity, bool multiplyTint = false,
                                BlendKind blend = BlendKind.Alpha)
    {
        if (width <= 0 || height <= 0) return;
        float strength = System.Math.Clamp(tintStrength, 0f, 1f);
        float alphaScale = System.Math.Clamp(opacity, 0f, 1f);
        // A copy that writes every texel (BlendKind.CopyAll) writes even at opacity 0.
        if (alphaScale == 0 && blend != BlendKind.CopyAll) return;
        int tr=(int)(tint>>16&255), tg=(int)(tint>>8&255), tb=(int)(tint&255);
        bool unmodulatedSourceOver = alphaScale == 1 && strength == 0 && !multiplyTint
                                     && blend is BlendKind.Alpha or BlendKind.Opaque;
        if (TryIntegerTranslation(localToDest, out int tx, out int ty))
        {
            BlitTranslated(dst, dstW, dstH, src, srcW, srcX, srcY, width, height,
                           tx, ty, tr, tg, tb, strength, alphaScale, multiplyTint, blend, unmodulatedSourceOver);
            return;
        }
        if (!localToDest.TryInverse(out var inv)) return;
        Bounds(localToDest, width, height, dstW, dstH, out int x0, out int y0, out int x1, out int y1);
        if (x1 <= x0 || y1 <= y0) return;
        if (localToDest.XY == 0 && localToDest.YX == 0 && x1 - x0 <= 4096)
        {
            BlitAxisAligned(dst, dstW, src, srcW, srcH, srcX, srcY, width, height, inv,
                            x0, y0, x1, y1, tr, tg, tb, strength, alphaScale, multiplyTint, blend,
                            unmodulatedSourceOver);
            return;
        }
        for (int y=y0; y<y1; y++) for (int x=x0; x<x1; x++)
        {
            var p = inv.Apply(x + 0.5, y + 0.5);
            if (!(p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height)) continue;
            Tap(srcX + p.X, srcW, out int xa, out int xb, out int wx);
            Tap(srcY + p.Y, srcH, out int ya, out int yb, out int wy);
            Sample(src, srcW, xa, xb, wx, ya, yb, wy, out int r, out int g, out int b, out int a);
            BlendSample(dst, (y*dstW+x)*4, r, g, b, a, tr, tg, tb, strength, alphaScale, multiplyTint, blend,
                        unmodulatedSourceOver);
        }
    }

    public static void FillRgba(byte[] dst, int dstW, int dstH, int width, int height,
                                Affine2D localToDest, long color, float opacity)
    {
        if (width <= 0 || height <= 0) return;
        float a = FragmentUnorm(System.Math.Clamp(opacity, 0f, 1f) * 255); if (a == 0) return;
        int r=(int)(color>>16&255),g=(int)(color>>8&255),b=(int)(color&255);
        if (TryIntegerTranslation(localToDest, out int tx, out int ty))
        {
            FillTranslated(dst, dstW, dstH, width, height, tx, ty, r, g, b, a);
            return;
        }
        if (!localToDest.TryInverse(out var inv)) return;
        Bounds(localToDest,width,height,dstW,dstH,out int x0,out int y0,out int x1,out int y1);
        if (x1 <= x0 || y1 <= y0) return;
        if (localToDest.XY == 0 && localToDest.YX == 0 && x1 - x0 <= 4096)
        {
            FillAxisAligned(dst, dstW, width, height, inv, x0, y0, x1, y1, r, g, b, a);
            return;
        }
        for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++){
            var p=inv.Apply(x+.5,y+.5);
            if(p.X>=0&&p.X<width&&p.Y>=0&&p.Y<height) Blend(dst,(y*dstW+x)*4,r,g,b,a);
        }
    }

    /// <summary>True when the transform is a pure integer translation, which maps every pixel centre onto a texel
    /// centre: native's bilinear filter then returns each texel unchanged, and a nearest lookup is exact.</summary>
    public static bool IsIntegerTranslation(Affine2D m) => TryIntegerTranslation(m, out _, out _);

    private static bool TryIntegerTranslation(Affine2D m, out int tx, out int ty)
    {
        tx = ty = 0;
        if (m.XX != 1 || m.XY != 0 || m.YX != 0 || m.YY != 1 ||
            m.TX != System.Math.Truncate(m.TX) || m.TY != System.Math.Truncate(m.TY) ||
            m.TX < int.MinValue || m.TX > int.MaxValue || m.TY < int.MinValue || m.TY > int.MaxValue)
            return false;
        tx = (int)m.TX;
        ty = (int)m.TY;
        return true;
    }

    /// <summary>
    /// One axis of a bilinear tap. <paramref name="position"/> is the sample point in source-image texel units,
    /// whose texel centres lie at k + 0.5. Returns the two texel indices (clamped to the image) and the second
    /// one's weight out of <see cref="WeightOne"/>, rounded to the nearest 1/256 as the hardware's 8-bit subtexel
    /// position is.
    /// </summary>
    private static void Tap(double position, int size, out int first, out int second, out int weight)
    {
        double centred = position - 0.5;
        double floor = System.Math.Floor(centred);
        int index = (int)floor;
        weight = (int)System.Math.Round((centred - floor) * WeightOne);
        int last = size - 1;
        first = index < 0 ? 0 : index > last ? last : index;
        int next = index + 1;
        second = next < 0 ? 0 : next > last ? last : next;
    }

    private static void Sample(byte[] src, int srcW, int xa, int xb, int wx, int ya, int yb, int wy,
                               out int r, out int g, out int b, out int a)
    {
        int i00 = (ya * srcW + xa) * 4, i10 = (ya * srcW + xb) * 4;
        int i01 = (yb * srcW + xa) * 4, i11 = (yb * srcW + xb) * 4;
        int ux = WeightOne - wx, uy = WeightOne - wy;
        int w00 = ux * uy, w10 = wx * uy, w01 = ux * wy, w11 = wx * wy;
        const int half = WeightOne * WeightOne / 2;
        r = (src[i00] * w00 + src[i10] * w10 + src[i01] * w01 + src[i11] * w11 + half) >> 16;
        g = (src[i00 + 1] * w00 + src[i10 + 1] * w10 + src[i01 + 1] * w01 + src[i11 + 1] * w11 + half) >> 16;
        b = (src[i00 + 2] * w00 + src[i10 + 2] * w10 + src[i01 + 2] * w01 + src[i11 + 2] * w11 + half) >> 16;
        a = (src[i00 + 3] * w00 + src[i10 + 3] * w10 + src[i01 + 3] * w01 + src[i11 + 3] * w11 + half) >> 16;
    }

    private static void BlendSample(byte[] dst, int di, int r, int g, int b, int a,
                                    int tr, int tg, int tb, float strength, float alphaScale, bool multiplyTint,
                                    BlendKind blend, bool unmodulatedSourceOver)
    {
        if (unmodulatedSourceOver)
        {
            BlendUnmodulatedSourceOver(dst, di, r, g, b, a);
            return;
        }
        // The GPU renderer's fragment: the texel moved toward the tint by the strength (or multiplied by it), and its
        // alpha times the opacity, in floats, each channel then written to 8 bits as the hardware does.
        int sa = FragmentUnorm(a * alphaScale);
        if (sa == 0 && blend != BlendKind.CopyAll) return;
        float keep = 1 - strength;
        int sr = FragmentUnorm(multiplyTint ? r * tr / 255f : r * keep + tr * strength);
        int sg = FragmentUnorm(multiplyTint ? g * tg / 255f : g * keep + tg * strength);
        int sb = FragmentUnorm(multiplyTint ? b * tb / 255f : b * keep + tb * strength);
        Blend(dst, di, sr, sg, sb, sa, blend);
    }

    private static void BlitTranslated(byte[] dst, int dstW, int dstH, byte[] src, int srcW,
                                       int srcX, int srcY, int width, int height, int tx, int ty,
                                       int tr, int tg, int tb, float strength, float alphaScale, bool multiplyTint,
                                       BlendKind blend, bool unmodulatedSourceOver)
    {
        int x0 = System.Math.Max(0, tx), y0 = System.Math.Max(0, ty);
        int x1 = (int)System.Math.Min(dstW, (long)tx + width);
        int y1 = (int)System.Math.Min(dstH, (long)ty + height);
        if (unmodulatedSourceOver)
        {
            BlitTranslatedUnmodulated(dst, dstW, src, srcW, srcX, srcY, tx, ty, x0, y0, x1, y1);
            return;
        }
        for (int y = y0; y < y1; y++)
        {
            int v = y - ty;
            for (int x = x0; x < x1; x++)
            {
                int u = x - tx;
                int si = ((srcY + v) * srcW + (srcX + u)) * 4, di = (y * dstW + x) * 4;
                BlendSample(dst, di, src[si], src[si + 1], src[si + 2], src[si + 3], tr, tg, tb, strength, alphaScale,
                            multiplyTint, blend, unmodulatedSourceOver: false);
            }
        }
    }

    private static void BlitTranslatedUnmodulated(byte[] dst, int dstW, byte[] src, int srcW,
                                                   int srcX, int srcY, int tx, int ty,
                                                   int x0, int y0, int x1, int y1)
    {
        for (int y = y0; y < y1; y++)
        {
            int sourceRow = (srcY + y - ty) * srcW + srcX - tx;
            int destinationRow = y * dstW;
            for (int x = x0; x < x1; x++)
            {
                int si = (sourceRow + x) * 4;
                BlendUnmodulatedSourceOver(dst, (destinationRow + x) * 4,
                                           src[si], src[si + 1], src[si + 2], src[si + 3]);
            }
        }
    }

    private static void FillTranslated(byte[] dst, int dstW, int dstH, int width, int height,
                                       int tx, int ty, int r, int g, int b, float a)
    {
        int x0 = System.Math.Max(0, tx), y0 = System.Math.Max(0, ty);
        int x1 = (int)System.Math.Min(dstW, (long)tx + width);
        int y1 = (int)System.Math.Min(dstH, (long)ty + height);
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
                Blend(dst, (y * dstW + x) * 4, r, g, b, a);
    }

    // Axis-aligned scale is FIELD's dominant non-integer path. Its inverse source column is independent
    // of destination Y, and its source row is independent of destination X. Cache each column's two taps and
    // weight once per layer and compute the row's once per scanline, with the general path's exact arithmetic.
    // Pooled lookup storage avoids both per-layer garbage and repeated-stackalloc growth when the JIT inlines
    // this hot path; the 4096-column caller gate keeps the rented bucket small.
    private static void BlitAxisAligned(byte[] dst, int dstW, byte[] src, int srcW, int srcH,
                                        int srcX, int srcY, int width, int height, Affine2D inv,
                                        int x0, int y0, int x1, int y1,
                                        int tr, int tg, int tb, float strength, float alphaScale,
                                        bool multiplyTint, BlendKind blend, bool unmodulatedSourceOver)
    {
        int count = x1 - x0;
        int[] columns = System.Buffers.ArrayPool<int>.Shared.Rent(count * 3);
        try
        {
            for (int x = x0; x < x1; x++)
            {
                double u = inv.Apply(x + 0.5, y0 + 0.5).X;
                int slot = (x - x0) * 3;
                if (!(u >= 0 && u < width))
                {
                    columns[slot] = -1;
                    continue;
                }
                Tap(srcX + u, srcW, out columns[slot], out columns[slot + 1], out columns[slot + 2]);
            }
            for (int y = y0; y < y1; y++)
            {
                double v = inv.Apply(x0 + 0.5, y + 0.5).Y;
                if (!(v >= 0 && v < height)) continue;
                Tap(srcY + v, srcH, out int ya, out int yb, out int wy);
                int destinationRow = y * dstW;
                for (int x = x0; x < x1; x++)
                {
                    int slot = (x - x0) * 3;
                    int xa = columns[slot];
                    if (xa < 0) continue;
                    Sample(src, srcW, xa, columns[slot + 1], columns[slot + 2], ya, yb, wy,
                           out int r, out int g, out int b, out int a);
                    BlendSample(dst, (destinationRow + x) * 4, r, g, b, a, tr, tg, tb, strength, alphaScale,
                                multiplyTint, blend, unmodulatedSourceOver);
                }
            }
        }
        finally { System.Buffers.ArrayPool<int>.Shared.Return(columns); }
    }

    // Most FIELD layers have full object opacity and no tint. Preserve color-key transparency and partially
    // transparent edge texels, but make the overwhelmingly common alpha-255 case a four-byte copy instead of
    // performing tint and source-over multiply/divide work whose result is exactly the source texel.
    private static void BlendUnmodulatedSourceOver(byte[] dst, int di, int r, int g, int b, int a)
    {
        if (a == 0) return;
        if (a == 255)
        {
            dst[di] = (byte)r;
            dst[di + 1] = (byte)g;
            dst[di + 2] = (byte)b;
            dst[di + 3] = 255;
            return;
        }
        Blend(dst, di, (float)r, g, b, a);
    }

    private static void FillAxisAligned(byte[] dst, int dstW, int width, int height, Affine2D inv,
                                        int x0, int y0, int x1, int y1,
                                        int r, int g, int b, float a)
    {
        int count = x1 - x0;
        bool[] includedColumns = System.Buffers.ArrayPool<bool>.Shared.Rent(count);
        try
        {
            for (int x = x0; x < x1; x++)
            {
                double u = inv.Apply(x + 0.5, y0 + 0.5).X;
                includedColumns[x - x0] = u >= 0 && u < width;
            }
            for (int y = y0; y < y1; y++)
            {
                double v = inv.Apply(x0 + 0.5, y + 0.5).Y;
                if (v < 0 || v >= height) continue;
                int destinationRow = y * dstW;
                for (int x = x0; x < x1; x++)
                    if (includedColumns[x - x0]) Blend(dst, (destinationRow + x) * 4, r, g, b, a);
            }
        }
        finally { System.Buffers.ArrayPool<bool>.Shared.Return(includedColumns); }
    }

    private static void Bounds(Affine2D m,int w,int h,int dw,int dh,out int x0,out int y0,out int x1,out int y1)
    {
        var a=m.Apply(0,0);var b=m.Apply(w,0);var c=m.Apply(0,h);var d=m.Apply(w,h);
        x0=System.Math.Max(0,(int)System.Math.Floor(System.Math.Min(System.Math.Min(a.X,b.X),System.Math.Min(c.X,d.X))));
        y0=System.Math.Max(0,(int)System.Math.Floor(System.Math.Min(System.Math.Min(a.Y,b.Y),System.Math.Min(c.Y,d.Y))));
        x1=System.Math.Min(dw,(int)System.Math.Ceiling(System.Math.Max(System.Math.Max(a.X,b.X),System.Math.Max(c.X,d.X))));
        y1=System.Math.Min(dh,(int)System.Math.Ceiling(System.Math.Max(System.Math.Max(a.Y,b.Y),System.Math.Max(c.Y,d.Y))));
    }
    // Copy and CopyAll write the source, alpha included (ONE/ZERO); their callers have already left out what the alpha
    // test keeps out. The others blend the alpha by the colour's factors, as Direct3D 9 does with no separate alpha
    // blend, which shows in a render target with an alpha channel: SRCALPHA/INVSRCALPHA leaves a * a + d * (1 - a),
    // SRCALPHA/ONE adds a * a, and REVSUBTRACT takes a * a away.
    private static void Blend(byte[] d, int i, int r, int g, int b, int a, BlendKind blend = BlendKind.Alpha)
        => Blend(d, i, (float)r, g, b, a, blend);

    // The colour and alpha on 0..255, unrounded: the blend runs in floats, and each write rounds (Unorm).
    private static void Blend(byte[] d, int i, float r, float g, float b, float a, BlendKind blend = BlendKind.Alpha)
    {
        if (blend is BlendKind.Copy or BlendKind.CopyAll)
        {
            d[i] = Unorm(r); d[i + 1] = Unorm(g); d[i + 2] = Unorm(b); d[i + 3] = Unorm(a);
            return;
        }
        float alpha = a / 255f;
        if (blend == BlendKind.Subtractive)
        {
            d[i] = Unorm(d[i] - r * alpha); d[i + 1] = Unorm(d[i + 1] - g * alpha);
            d[i + 2] = Unorm(d[i + 2] - b * alpha); d[i + 3] = Unorm(d[i + 3] - a * alpha);
            return;
        }
        if (blend == BlendKind.Additive)
        {
            d[i] = Unorm(d[i] + r * alpha); d[i + 1] = Unorm(d[i + 1] + g * alpha);
            d[i + 2] = Unorm(d[i + 2] + b * alpha); d[i + 3] = Unorm(d[i + 3] + a * alpha);
            return;
        }
        float inverse = 1 - alpha;
        d[i] = Unorm(r * alpha + d[i] * inverse); d[i + 1] = Unorm(g * alpha + d[i + 1] * inverse);
        d[i + 2] = Unorm(b * alpha + d[i + 2] * inverse); d[i + 3] = Unorm(a * alpha + d[i + 3] * inverse);
    }

    /// <summary>A fragment's channel, a float on 0..255, as the hardware hands it to the blend: cut to 12 bits of
    /// fraction, then the nearest 8-bit value. Measured on the GPU renderer's frames and native's render targets
    /// (docs/engine-re.md, "The blend's arithmetic"): plain rounding writes 13 x 128 / 255 = 6.53 as 7, where the
    /// hardware writes 6.</summary>
    internal static byte FragmentUnorm(float value)
        => Unorm(System.MathF.Floor(value * (FragmentSteps / 255f)) * (255f / FragmentSteps));

    private const float FragmentSteps = 4096;

    /// <summary>A float on 0..255 written to an 8-bit target as the hardware writes it: the nearest value,
    /// clamped.</summary>
    internal static byte Unorm(float value) => value <= 0 ? (byte)0 : value >= 255 ? (byte)255 : (byte)(value + 0.5f);
}
