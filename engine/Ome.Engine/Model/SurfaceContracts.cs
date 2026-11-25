namespace Ome.Engine.Model;

public readonly record struct SurfaceRectFill(
    int SurfaceSlot, int X, int Y, int Width, int Height, int Alpha, long Rgb);

public readonly record struct SurfaceRectCopy(
    int SourceSurface, int DestinationSurface, int SourceX, int SourceY,
    int Width, int Height, int DestinationX, int DestinationY,
    int DestinationWidth, int DestinationHeight)
{
    public SurfaceRectCopy(
        int sourceSurface, int destinationSurface, int sourceX, int sourceY,
        int width, int height, int destinationX, int destinationY)
        : this(sourceSurface, destinationSurface, sourceX, sourceY, width, height,
               destinationX, destinationY, width, height) { }
}

/// <summary>Op 0x35's request (native MosaicTexture): both rectangles as edges, each right and bottom the origin plus
/// the extent as the handler adds them in 32 bits, and the block's edge in pixels.</summary>
public readonly record struct SurfaceMosaicCopy(
    int SourceSurface, int DestinationSurface,
    int SourceLeft, int SourceTop, int SourceRight, int SourceBottom,
    int DestinationLeft, int DestinationTop, int DestinationRight, int DestinationBottom, int Dot);

/// <summary>What op 0x35 did: it copied (an empty rectangle copies nothing, as native's does), native refused it with
/// the trace in <see cref="SurfaceMosaicResult.Detail"/>, or native reads or writes memory there that the port does not
/// model.</summary>
public enum SurfaceMosaicOutcome
{
    Copied,
    Refused,
    ReadsPastSource,
    WritesBeforeLineBuffer,
}

public readonly record struct SurfaceMosaicResult(SurfaceMosaicOutcome Outcome, string Detail = "");

public enum SurfaceBlackFadeDirection
{
    FromBlack,
    ToBlack,
}

public enum SurfacePatternTransitionMode
{
    WipeLeftToRight = 0,
    WipeRightToLeft = 1,
    WipeTopToBottom = 2,
    WipeBottomToTop = 3,
    VerticalStripsLeftToRight = 4,
    VerticalStripsRightToLeft = 5,
    HorizontalStripsTopToBottom = 6,
    HorizontalStripsBottomToTop = 7,
    StaggeredVerticalStripsLeftToRight = 8,
    StaggeredVerticalStripsRightToLeft = 9,
    StaggeredHorizontalStripsTopToBottom = 10,
    StaggeredHorizontalStripsBottomToTop = 11,
}

public readonly record struct SurfacePatternTransitionRequest(
    int Surface, long IntervalMilliseconds, int Divisions, SurfacePatternTransitionMode Mode);
