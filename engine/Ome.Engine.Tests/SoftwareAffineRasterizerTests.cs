using Ome.Engine.Model;
using Xunit;

public class SoftwareAffineRasterizerTests
{
    [Theory]
    [InlineData(-1, 2, false)]
    [InlineData(4, -1, false)]
    [InlineData(1, 1, true)]
    public void BlitRgba_IntegerTranslationFastPath_MatchesGeneralAffine(int tx, int ty, bool multiplyTint)
    {
        byte[] src = new byte[5 * 4 * 4];
        for (int i = 0; i < src.Length; i += 4)
        {
            src[i] = (byte)(20 + i % 211); src[i + 1] = (byte)(70 + i % 173);
            src[i + 2] = (byte)(130 + i % 107); src[i + 3] = (byte)(40 + i % 216);
        }
        byte[] expected = new byte[6 * 5 * 4];
        for (int i = 0; i < expected.Length; i++) expected[i] = (byte)(i * 17 + 3);
        byte[] actual = (byte[])expected.Clone();
        var translation = new Affine2D(1, 0, 0, 1, tx, ty);

        ReferenceBlit(expected, 6, 5, src, 5, 4, 1, 1, 3, 2, translation,
                      0x4080c0, 0.35f, 0.6f, multiplyTint);
        SoftwareAffineRasterizer.BlitRgba(actual, 6, 5, src, 5, 4, 1, 1, 3, 2, translation,
                                          0x4080c0, 0.35f, 0.6f, multiplyTint);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(-2, 1)]
    [InlineData(4, -1)]
    [InlineData(1, 2)]
    public void FillRgba_IntegerTranslationFastPath_MatchesGeneralAffine(int tx, int ty)
    {
        byte[] expected = new byte[6 * 5 * 4];
        for (int i = 0; i < expected.Length; i++) expected[i] = (byte)(i * 11 + 5);
        byte[] actual = (byte[])expected.Clone();
        var translation = new Affine2D(1, 0, 0, 1, tx, ty);

        ReferenceFill(expected, 6, 5, 4, 3, translation, 0x9a4f21, 0.42f);
        SoftwareAffineRasterizer.FillRgba(actual, 6, 5, 4, 3, translation, 0x9a4f21, 0.42f);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BlitRgba_QuarterTurnLandsOnTexelCentresAndCopiesTexels()
    {
        byte[] src = { 255,0,0,255, 0,255,0,255 };
        byte[] dst = new byte[4*4*4];
        var world = Transform2DMath.Build(
            new TransformState(1,1,1,0,0,0,1,1,0, 0,0,1,90));
        SoftwareAffineRasterizer.BlitRgba(dst,4,4,src,2,1,0,0,2,1,
            world.FromLocalOrigin(1,1),0xffffff,0,1);
        Assert.Equal(new byte[] {255,0,0,255}, dst[16..20]);
        Assert.Equal(new byte[] {0,255,0,255}, dst[32..36]);
    }

    [Fact]
    public void BlitRgba_ScaleBlendsNeighbouringTexelsWithEightBitWeights()
    {
        // Native draws objects through a D3D9 sprite with MIN/MAG LINEAR (gfx_object_blit_d3d9). Doubling a
        // black/white pair samples at 0.25, 0.75, 1.25 and 1.75 texels: the outer two clamp to the image edge,
        // the inner two take 64/256 and 192/256 of white.
        byte[] src = { 0,0,0,255, 255,255,255,255 };
        byte[] dst = new byte[4 * 4];
        SoftwareAffineRasterizer.BlitRgba(dst, 4, 1, src, 2, 1, 0, 0, 2, 1,
            new Affine2D(2, 0, 0, 1, 0, 0), 0xffffff, 0, 1);
        Assert.Equal(new byte[] { 0,0,0,255, 64,64,64,255, 191,191,191,255, 255,255,255,255 }, dst);
    }

    [Fact]
    public void BlitRgba_ScaledRegionBlendsTheTexelsBeyondItsEdge()
    {
        // D3DTADDRESS_CLAMP clamps to the texture, not the source rectangle, so a scaled one-texel region of a
        // red/green/blue strip takes some of its red and blue neighbours.
        byte[] src = { 255,0,0,255, 0,255,0,255, 0,0,255,255 };
        byte[] dst = new byte[2 * 4];
        SoftwareAffineRasterizer.BlitRgba(dst, 2, 1, src, 3, 1, 1, 0, 1, 1,
            new Affine2D(2, 0, 0, 1, 0, 0), 0xffffff, 0, 1);
        Assert.Equal(new byte[] { 64,191,0,255, 0,191,64,255 }, dst);
    }

    [Fact]
    public void BlitRgba_GeneralAffineSamplesAsTheAxisAlignedPath()
    {
        // A rotation too small to move any centre past a texel boundary takes the general path; its samples must
        // agree with the axis-aligned path to within the subtexel weight's rounding.
        var random = new Random(0x47d);
        byte[] src = new byte[7 * 5 * 4];
        random.NextBytes(src);
        for (int i = 3; i < src.Length; i += 4) src[i] = 255;
        byte[] axis = new byte[12 * 9 * 4];
        byte[] general = new byte[12 * 9 * 4];
        SoftwareAffineRasterizer.BlitRgba(axis, 12, 9, src, 7, 5, 0, 0, 7, 5,
            new Affine2D(1.05, 0, 0, 1.05, 0.3, 0.2), 0xffffff, 0, 1);
        SoftwareAffineRasterizer.BlitRgba(general, 12, 9, src, 7, 5, 0, 0, 7, 5,
            new Affine2D(1.05, 1e-12, -1e-12, 1.05, 0.3, 0.2), 0xffffff, 0, 1);
        for (int i = 0; i < axis.Length; i++) Assert.InRange(general[i] - axis[i], -1, 1);
    }

    [Fact]
    public void FillRgba_UsesAffineShapeRatherThanBoundingBox()
    {
        byte[] dst = new byte[5*5*4];
        var m = new Affine2D(1,0.5,-0.5,1,2,1);
        SoftwareAffineRasterizer.FillRgba(dst,5,5,2,2,m,0xff0000,1);
        int colored = 0;
        for(int i=3;i<dst.Length;i+=4) if(dst[i]!=0) colored++;
        Assert.InRange(colored, 3, 5);
        Assert.Equal(0, dst[(1*5+1)*4+3]);
    }

    [Fact]
    public void BlitRgba_MultiplyTintUsesD3dStyleRgbModulationAndOpacity()
    {
        byte[] src = { 200, 100, 50, 255 };
        byte[] dst = new byte[4];
        SoftwareAffineRasterizer.BlitRgba(dst, 1, 1, src, 1, 1, 0, 0, 1, 1,
            new Affine2D(1, 0, 0, 1, 0, 0), 0x80ff40, 0, 0.5f, multiplyTint: true);
        Assert.InRange(dst[0], 49, 50);
        Assert.InRange(dst[1], 49, 50);
        Assert.InRange(dst[2], 5, 7);
        Assert.InRange(dst[3], 63, 64);   // alpha 127 x 127 / 255 over 0
    }

    [Fact]
    public void BlitRgba_WhiteModulationPreservesYellowAndZeroOpacityHidesIt()
    {
        byte[] src = { 240, 192, 16, 255 };
        byte[] visible = new byte[4];
        var identity = new Affine2D(1, 0, 0, 1, 0, 0);
        SoftwareAffineRasterizer.BlitRgba(visible, 1, 1, src, 1, 1, 0, 0, 1, 1,
            identity, 0xffffff, 0, 1, multiplyTint: true);
        Assert.Equal(new byte[] { 240, 192, 16, 255 }, visible);

        byte[] hidden = new byte[4];
        SoftwareAffineRasterizer.BlitRgba(hidden, 1, 1, src, 1, 1, 0, 0, 1, 1,
            identity, 0xffffff, 0, 0, multiplyTint: true);
        Assert.Equal(new byte[4], hidden);
    }

    [Fact]
    public void BlitRgba_AdditiveMakesBlackTransparentAndAddsScaledColor()
    {
        var identity = new Affine2D(1, 0, 0, 1, 0, 0);
        byte[] background = { 40, 50, 60, 255 };
        byte[] black = { 0, 0, 0, 255 };
        SoftwareAffineRasterizer.BlitRgba(background, 1, 1, black, 1, 1, 0, 0, 1, 1,
            identity, 0xffffff, 0, 1, multiplyTint: true, blend: BlendKind.Additive);
        Assert.Equal(new byte[] { 40, 50, 60, 255 }, background);

        byte[] blueGlow = { 16, 32, 200, 128 };
        SoftwareAffineRasterizer.BlitRgba(background, 1, 1, blueGlow, 1, 1, 0, 0, 1, 1,
            identity, 0xffffff, 0, 0.5f, multiplyTint: true, blend: BlendKind.Additive);
        // Alpha 128 at half opacity is 64: red 40 + 16 x 64 / 255 is 44.0, blue 60 + 200 x 64 / 255 is 110.2.
        Assert.Equal(new byte[] { 44, 58, 110, 255 }, background);
    }

    [Theory]
    // The products a translucent layer meets: texel alpha times 128 / 255. Plain rounding writes 6.53 as 7 and 7.53 as
    // 8; the hardware, cutting to 12 bits of fraction first, writes 6 and 7, and 11.55 still as 12.
    [InlineData(13 * 128 / 255f, 6)]
    [InlineData(15 * 128 / 255f, 7)]
    [InlineData(23 * 128 / 255f, 12)]
    [InlineData(29 * 128 / 255f, 15)]
    [InlineData(127.5f, 128)]
    [InlineData(0f, 0)]
    [InlineData(-3f, 0)]
    [InlineData(255f, 255)]
    [InlineData(300f, 255)]
    public void AFragmentIsCutToTwelveBitsBeforeItsNearestEightBitValue(float value, int written)
        => Assert.Equal(written, SoftwareAffineRasterizer.FragmentUnorm(value));

    [Fact]
    public void AFragmentKeepsEveryWholeValue()
    {
        for (int value = 0; value <= 255; value++)
            Assert.Equal(value, SoftwareAffineRasterizer.FragmentUnorm(value));
    }

    [Fact]
    public void ATranslucentWindowsEdgeBlendsAsTheHardwareDid()
    {
        // Verita's message window on SC0000's page 4: a white texel of alpha 13, tinted 0x1c1e25 and drawn at opacity
        // 128 / 255 over the CG's (104, 184, 255). The GPU renderer's frame, and native's thumbnail of the page, have
        // (102, 180, 250): alpha 6, so red (28 x 6 + 104 x 249) / 255 = 102.2.
        byte[] target = { 104, 184, 255, 255 };
        SoftwareAffineRasterizer.BlitRgba(target, 1, 1, new byte[] { 255, 255, 255, 13 }, 1, 1, 0, 0, 1, 1,
            new Affine2D(1, 0, 0, 1, 0, 0), 0x1c1e25, 0, 128 / 255f, multiplyTint: true);
        Assert.Equal(new byte[] { 102, 180, 250 }, target[..3]);
    }

    [Fact]
    public void BlitRgba_AxisAlignedScaleFastPath_MatchesGeneralAffineAcrossRandomizedCases()
    {
        var random = new Random(0x231);
        double[] scales = { -2.0, -1.25, -0.5, 0.25, 0.5, 0.75, 1.25, 2.0, 3.0 };
        double[] translations = { -5.5, -2.25, -0.5, 0.0, 0.5, 1.25, 4.5, 8.0 };
        BlendKind[] blends = { BlendKind.Opaque, BlendKind.Alpha, BlendKind.Additive };
        for (int iteration = 0; iteration < 250; iteration++)
        {
            int srcW = random.Next(2, 12), srcH = random.Next(2, 10);
            byte[] src = new byte[srcW * srcH * 4];
            random.NextBytes(src);
            int srcX = random.Next(0, srcW), srcY = random.Next(0, srcH);
            int width = random.Next(1, srcW - srcX + 1), height = random.Next(1, srcH - srcY + 1);
            int dstW = random.Next(3, 15), dstH = random.Next(3, 13);
            byte[] expected = new byte[dstW * dstH * 4];
            random.NextBytes(expected);
            byte[] actual = (byte[])expected.Clone();
            var transform = new Affine2D(
                scales[random.Next(scales.Length)], 0, 0, scales[random.Next(scales.Length)],
                translations[random.Next(translations.Length)], translations[random.Next(translations.Length)]);
            long tint = random.Next(0x1000000);
            float tintStrength = random.NextSingle();
            float opacity = random.NextSingle();
            bool multiplyTint = random.Next(2) != 0;
            BlendKind blend = blends[random.Next(blends.Length)];

            ReferenceBlit(expected, dstW, dstH, src, srcW, srcH, srcX, srcY, width, height,
                          transform, tint, tintStrength, opacity, multiplyTint, blend);
            SoftwareAffineRasterizer.BlitRgba(actual, dstW, dstH, src, srcW, srcH,
                                               srcX, srcY, width, height, transform,
                                               tint, tintStrength, opacity, multiplyTint, blend);

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void FillRgba_AxisAlignedScaleFastPath_MatchesGeneralAffineAcrossRandomizedCases()
    {
        var random = new Random(0x22d);
        double[] scales = { -2.0, -1.0, -0.5, 0.25, 0.5, 0.75, 1.5, 2.0, 3.0 };
        double[] translations = { -5.5, -1.25, -0.5, 0.0, 0.5, 2.25, 7.0 };
        for (int iteration = 0; iteration < 250; iteration++)
        {
            int width = random.Next(1, 10), height = random.Next(1, 9);
            int dstW = random.Next(3, 15), dstH = random.Next(3, 13);
            byte[] expected = new byte[dstW * dstH * 4];
            random.NextBytes(expected);
            byte[] actual = (byte[])expected.Clone();
            var transform = new Affine2D(
                scales[random.Next(scales.Length)], 0, 0, scales[random.Next(scales.Length)],
                translations[random.Next(translations.Length)], translations[random.Next(translations.Length)]);
            long color = random.Next(0x1000000);
            float opacity = random.NextSingle();

            ReferenceFill(expected, dstW, dstH, width, height, transform, color, opacity);
            SoftwareAffineRasterizer.FillRgba(actual, dstW, dstH, width, height,
                                               transform, color, opacity);

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void BlitRgba_UnmodulatedSourceOverFastPaths_MatchGeneralAffineAcrossAlphaEdges()
    {
        var random = new Random(0x2e0);
        Affine2D[] transforms =
        {
            new(1, 0, 0, 1, -2, 1),
            new(1, 0, 0, 1, 3, -1),
            new(0.5, 0, 0, 1.5, -1.25, 0.5),
            new(-1.25, 0, 0, 0.75, 8.5, -0.5),
        };
        foreach (var transform in transforms)
        foreach (BlendKind blend in new[] { BlendKind.Opaque, BlendKind.Alpha })
        for (int iteration = 0; iteration < 50; iteration++)
        {
            int srcW = random.Next(3, 12), srcH = random.Next(3, 10);
            byte[] src = new byte[srcW * srcH * 4];
            random.NextBytes(src);
            for (int pixel = 0; pixel < srcW * srcH; pixel++)
                src[pixel * 4 + 3] = (byte)(pixel % 4 switch { 0 => 0, 1 => 1, 2 => 254, _ => 255 });
            int dstW = random.Next(5, 16), dstH = random.Next(5, 14);
            byte[] expected = new byte[dstW * dstH * 4];
            random.NextBytes(expected);
            byte[] actual = (byte[])expected.Clone();

            ReferenceBlit(expected, dstW, dstH, src, srcW, srcH, 1, 1, srcW - 1, srcH - 1,
                          transform, 0x6a4c2e, 0, 1, false, blend);
            SoftwareAffineRasterizer.BlitRgba(actual, dstW, dstH, src, srcW, srcH,
                                               1, 1, srcW - 1, srcH - 1, transform,
                                               0x6a4c2e, 0, 1, false, blend);

            Assert.Equal(expected, actual);
        }
    }

    // Per-pixel affine algorithm, kept free of the fast paths as an independent differential oracle: each
    // covered pixel centre is mapped back and sampled bilinearly as native's D3D9 sprite does (8-bit weights
    // rounded to the nearest 1/256, coordinates clamped to the source image's edge). The fragment is worked out in
    // floats and written to 8 bits as the hardware does (Fragment); the blend rounds to the nearest value.
    private static void ReferenceBlit(byte[] dst, int dstW, int dstH, byte[] src, int srcW, int srcH,
                                      int srcX, int srcY, int width, int height, Affine2D transform,
                                      long tint, float tintStrength, float opacity, bool multiplyTint,
                                      BlendKind blend = BlendKind.Alpha)
    {
        if (!transform.TryInverse(out var inv)) return;
        ReferenceBounds(transform, width, height, dstW, dstH, out int x0, out int y0, out int x1, out int y1);
        float strength = System.Math.Clamp(tintStrength, 0f, 1f), keep = 1 - strength;
        float alphaScale = System.Math.Clamp(opacity, 0f, 1f);
        int tr = (int)(tint >> 16 & 255), tg = (int)(tint >> 8 & 255), tb = (int)(tint & 255);
        for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
        {
            var p = inv.Apply(x + 0.5, y + 0.5);
            if (!(p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height)) continue;
            double gx = srcX + p.X - 0.5, gy = srcY + p.Y - 0.5;
            int ix = (int)System.Math.Floor(gx), iy = (int)System.Math.Floor(gy);
            int fx = (int)System.Math.Round((gx - ix) * 256), fy = (int)System.Math.Round((gy - iy) * 256);
            int[] sample = new int[4];
            for (int channel = 0; channel < 4; channel++)
            {
                int sum = 0;
                for (int corner = 0; corner < 4; corner++)
                {
                    int cx = System.Math.Clamp(ix + (corner & 1), 0, srcW - 1);
                    int cy = System.Math.Clamp(iy + (corner >> 1), 0, srcH - 1);
                    int weight = ((corner & 1) != 0 ? fx : 256 - fx) * ((corner >> 1) != 0 ? fy : 256 - fy);
                    sum += src[(cy * srcW + cx) * 4 + channel] * weight;
                }
                sample[channel] = (sum + 32768) >> 16;
            }
            int di = (y * dstW + x) * 4;
            int sa = Fragment(sample[3] * alphaScale);
            if (sa == 0) continue;
            int sr = Fragment(multiplyTint ? sample[0] * tr / 255f : sample[0] * keep + tr * strength);
            int sg = Fragment(multiplyTint ? sample[1] * tg / 255f : sample[1] * keep + tg * strength);
            int sb = Fragment(multiplyTint ? sample[2] * tb / 255f : sample[2] * keep + tb * strength);
            ReferenceBlend(dst, di, sr, sg, sb, sa, blend);
        }
    }

    private static void ReferenceFill(byte[] dst, int dstW, int dstH, int width, int height,
                                      Affine2D transform, long color, float opacity)
    {
        if (!transform.TryInverse(out var inv)) return;
        ReferenceBounds(transform, width, height, dstW, dstH, out int x0, out int y0, out int x1, out int y1);
        int a = Fragment(System.Math.Clamp(opacity, 0f, 1f) * 255);
        if (a == 0) return;
        int r = (int)(color >> 16 & 255), g = (int)(color >> 8 & 255), b = (int)(color & 255);
        for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
        {
            var p = inv.Apply(x + 0.5, y + 0.5);
            if (p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height)
                ReferenceBlend(dst, (y * dstW + x) * 4, r, g, b, a);
        }
    }

    private static void ReferenceBounds(Affine2D m, int w, int h, int dw, int dh,
                                        out int x0, out int y0, out int x1, out int y1)
    {
        var a = m.Apply(0, 0); var b = m.Apply(w, 0); var c = m.Apply(0, h); var d = m.Apply(w, h);
        x0 = System.Math.Max(0, (int)System.Math.Floor(System.Math.Min(System.Math.Min(a.X, b.X), System.Math.Min(c.X, d.X))));
        y0 = System.Math.Max(0, (int)System.Math.Floor(System.Math.Min(System.Math.Min(a.Y, b.Y), System.Math.Min(c.Y, d.Y))));
        x1 = System.Math.Min(dw, (int)System.Math.Ceiling(System.Math.Max(System.Math.Max(a.X, b.X), System.Math.Max(c.X, d.X))));
        y1 = System.Math.Min(dh, (int)System.Math.Ceiling(System.Math.Max(System.Math.Max(a.Y, b.Y), System.Math.Max(c.Y, d.Y))));
    }

    private static void ReferenceBlend(byte[] dst, int i, int r, int g, int b, int a,
                                       BlendKind blend = BlendKind.Alpha)
    {
        if (blend == BlendKind.Additive)
        {
            dst[i] = Nearest(dst[i] * 255 + r * a);
            dst[i + 1] = Nearest(dst[i + 1] * 255 + g * a);
            dst[i + 2] = Nearest(dst[i + 2] * 255 + b * a);
            dst[i + 3] = Nearest(dst[i + 3] * 255 + a * a);
            return;
        }
        dst[i] = Nearest(r * a + dst[i] * (255 - a));
        dst[i + 1] = Nearest(g * a + dst[i + 1] * (255 - a));
        dst[i + 2] = Nearest(b * a + dst[i + 2] * (255 - a));
        dst[i + 3] = Nearest(a * a + dst[i + 3] * (255 - a));   // Direct3D 9: the alpha by the colour's factors
    }

    // A blend's value times 255, an integer, written as the nearest 8-bit value. With 8-bit inputs it never lies
    // half-way, so the rasterizer's float blend must give the same.
    private static byte Nearest(int timesTwoFiftyFive)
        => (byte)System.Math.Min(255, (2 * timesTwoFiftyFive + 255) / 510);

    // A fragment's channel on 0..255 as the hardware hands it to the blend: 12 bits of fraction, then the nearest
    // 8-bit value (docs/engine-re.md, "The blend's arithmetic").
    private static int Fragment(float value)
    {
        float steps = System.MathF.Floor(value * (4096f / 255f));
        double exact = steps * 255.0 / 4096;
        return (int)System.Math.Clamp(System.Math.Floor(exact + 0.5), 0, 255);
    }
}
