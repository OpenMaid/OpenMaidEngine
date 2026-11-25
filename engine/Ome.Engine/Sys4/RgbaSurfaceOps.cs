using Ome.Engine.Model;

namespace Ome.Engine.Sys4;

/// <summary>Platform-neutral mutation helpers for AGE's software-modeled RGBA surfaces.</summary>
public static class RgbaSurfaceOps
{
    public static bool FillRect(RgbaImage destination, int x, int y, int width, int height,
                                byte alpha, long rgb)
    {
        long left = x, top = y, right = left + width, bottom = top + height;
        if (width <= 0 || height <= 0) return false;
        left = System.Math.Max(0, left);
        top = System.Math.Max(0, top);
        right = System.Math.Min(destination.Width, right);
        bottom = System.Math.Min(destination.Height, bottom);
        if (right <= left || bottom <= top) return false;

        byte red = (byte)((rgb >> 16) & 0xff);
        byte green = (byte)((rgb >> 8) & 0xff);
        byte blue = (byte)(rgb & 0xff);
        for (int row = (int)top; row < (int)bottom; row++)
        {
            int offset = checked((row * destination.Width + (int)left) * 4);
            for (int column = (int)left; column < (int)right; column++, offset += 4)
            {
                destination.Pixels[offset] = red;
                destination.Pixels[offset + 1] = green;
                destination.Pixels[offset + 2] = blue;
                destination.Pixels[offset + 3] = alpha;
            }
        }
        return true;
    }

    public static RgbaImage WithColorKey(RgbaImage source, long colorKey)
    {
        if (!Ome.Engine.Model.BlendMath.HasColorKey(colorKey)) return source;
        byte[] pixels = (byte[])source.Pixels.Clone();
        for (int index = 0; index < pixels.Length; index += 4)
            if (Ome.Engine.Model.BlendMath.ColorKeyMatches(
                    pixels[index], pixels[index + 1], pixels[index + 2], colorKey))
                pixels[index + 3] = 0;
        return new RgbaImage(source.Width, source.Height, pixels);
    }

    /// <summary>Copy a rectangle while clipping source and destination together. A temporary buffer
    /// preserves native blit behavior when the two rectangles overlap in the same surface.</summary>
    public static bool CopyRect(RgbaImage source, RgbaImage destination,
                                int sourceX, int sourceY, int width, int height,
                                int destinationX, int destinationY)
    {
        long sx = sourceX, sy = sourceY, dx = destinationX, dy = destinationY;
        long w = width, h = height;
        if (w <= 0 || h <= 0) return false;

        if (sx < 0) { long n = -sx; sx = 0; dx += n; w -= n; }
        if (sy < 0) { long n = -sy; sy = 0; dy += n; h -= n; }
        if (dx < 0) { long n = -dx; dx = 0; sx += n; w -= n; }
        if (dy < 0) { long n = -dy; dy = 0; sy += n; h -= n; }
        w = System.Math.Min(w, source.Width - sx);
        h = System.Math.Min(h, source.Height - sy);
        w = System.Math.Min(w, destination.Width - dx);
        h = System.Math.Min(h, destination.Height - dy);
        if (w <= 0 || h <= 0) return false;

        int clippedWidth = checked((int)w);
        int clippedHeight = checked((int)h);
        int rowBytes = checked(clippedWidth * 4);
        byte[] pixels = new byte[checked(rowBytes * clippedHeight)];
        for (int row = 0; row < clippedHeight; row++)
        {
            int sourceOffset = checked(((int)sy + row) * source.Width * 4 + (int)sx * 4);
            source.Pixels.AsSpan(sourceOffset, rowBytes).CopyTo(pixels.AsSpan(row * rowBytes, rowBytes));
        }
        for (int row = 0; row < clippedHeight; row++)
        {
            int destinationOffset = checked(((int)dy + row) * destination.Width * 4 + (int)dx * 4);
            pixels.AsSpan(row * rowBytes, rowBytes)
                .CopyTo(destination.Pixels.AsSpan(destinationOffset, rowBytes));
        }
        return true;
    }

    /// <summary>
    /// Scale one RGBA rectangle into another with area-weighted sampling, as AGE's shrink does
    /// (gfx_downsample_surface_rect_area, 4.24J <c>0x4786b0</c>; docs/engine-re.md, "Scaled surface rectangle copy --
    /// opcode 0x32"). Each destination pixel's edges are its index times the source extent over the destination extent;
    /// every source pixel it covers adds its covered area, and its channels times that area, to five sums, and each
    /// channel is <c>trunc(sum / total + 0.5)</c>. AGE's x87 runs at single precision, so each step rounds to a float,
    /// in native's order: an average that is an exact half in doubles can land just under it. A source whose kind is
    /// not 0 (<paramref name="opaqueSource"/>) gives alpha 255. Source pixels are read from a stable image, and writes
    /// are clipped to the destination surface. Native's enlargement sampler is not modelled: growing takes this path
    /// too.
    /// </summary>
    public static bool ScaleCopyRect(RgbaImage source, RgbaImage destination,
                                     int sourceX, int sourceY, int sourceWidth, int sourceHeight,
                                     int destinationX, int destinationY,
                                     int destinationWidth, int destinationHeight, bool opaqueSource = false)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0
            || destinationWidth <= 0 || destinationHeight <= 0) return false;

        long left = System.Math.Max(0L, destinationX);
        long top = System.Math.Max(0L, destinationY);
        long right = System.Math.Min((long)destination.Width, (long)destinationX + destinationWidth);
        long bottom = System.Math.Min((long)destination.Height, (long)destinationY + destinationHeight);
        if (right <= left || bottom <= top) return false;

        bool wrote = false;
        for (int y = (int)top; y < (int)bottom; y++)
        {
            long row = y - (long)destinationY;
            float sy0 = (float)(row * sourceHeight) / destinationHeight;
            float sy1 = (float)((row + 1) * sourceHeight) / destinationHeight;
            int firstRow = (int)sy0, lastRow = CeilingOf(sy1);
            for (int x = (int)left; x < (int)right; x++)
            {
                long column = x - (long)destinationX;
                float sx0 = (float)(column * sourceWidth) / destinationWidth;
                float sx1 = (float)((column + 1) * sourceWidth) / destinationWidth;
                int firstColumn = (int)sx0, lastColumn = CeilingOf(sx1);

                float total = 0, alpha = 0, red = 0, green = 0, blue = 0;
                for (int sourceRow = firstRow; sourceRow < lastRow; sourceRow++)
                {
                    long imageRow = (long)sourceY + sourceRow;
                    if (imageRow < 0 || imageRow >= source.Height) continue;
                    float yWeight = (sy1 < sourceRow + 1 ? sy1 : sourceRow + 1) - (sy0 > sourceRow ? sy0 : sourceRow);
                    for (int sourceColumn = firstColumn; sourceColumn < lastColumn; sourceColumn++)
                    {
                        long imageColumn = (long)sourceX + sourceColumn;
                        if (imageColumn < 0 || imageColumn >= source.Width) continue;
                        float xWeight = (sx1 < sourceColumn + 1 ? sx1 : sourceColumn + 1)
                                        - (sx0 > sourceColumn ? sx0 : sourceColumn);
                        float weight = xWeight * yWeight;
                        int sourceOffset = checked((int)(imageRow * source.Width + imageColumn) * 4);
                        total += weight;
                        alpha += source.Pixels[sourceOffset + 3] * weight;
                        red += source.Pixels[sourceOffset] * weight;
                        green += source.Pixels[sourceOffset + 1] * weight;
                        blue += source.Pixels[sourceOffset + 2] * weight;
                    }
                }
                if (total <= 0) continue;

                int destinationOffset = checked((y * destination.Width + x) * 4);
                destination.Pixels[destinationOffset] = AverageByte(red, total);
                destination.Pixels[destinationOffset + 1] = AverageByte(green, total);
                destination.Pixels[destinationOffset + 2] = AverageByte(blue, total);
                destination.Pixels[destinationOffset + 3] = opaqueSource ? (byte)255 : AverageByte(alpha, total);
                wrote = true;
            }
        }
        return wrote;
    }

    // An edge's last pixel: native truncates the edge and adds one unless it was whole.
    private static int CeilingOf(float edge)
    {
        int truncated = (int)edge;
        return truncated == edge ? truncated : truncated + 1;
    }

    // Native's channel: the float quotient, plus 0.5 rounded to a float, truncated and clamped.
    private static byte AverageByte(float sum, float total)
    {
        float quotient = sum / total;
        float half = quotient + 0.5f;
        return (byte)System.Math.Clamp((int)half, 0, 255);
    }
}
