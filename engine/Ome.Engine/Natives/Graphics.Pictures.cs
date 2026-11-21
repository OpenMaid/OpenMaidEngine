using Ome.Engine.Model;

namespace Ome.Engine.Natives;

/// <summary>A catalog file behind what the screen shows: a picture, or a movie playing on a surface.</summary>
public readonly record struct PictureOnScreen(string Name, bool Movie)
{
    /// <summary>The list as the port shows and copies it: <c>pictures: BG012.AGF, SO001.AGF, OP.AGF (movie)</c>, a line
    /// break before every <paramref name="perLine"/>th name after the first line when it is set.</summary>
    public static string Describe(IReadOnlyList<PictureOnScreen> pictures, int perLine = 0)
    {
        if (pictures.Count == 0) return "pictures: none";
        var text = new System.Text.StringBuilder("pictures: ");
        for (int index = 0; index < pictures.Count; index++)
        {
            if (index > 0) text.Append(perLine > 0 && index % perLine == 0 ? ",\n  " : ", ");
            text.Append(pictures[index].Name);
            if (pictures[index].Movie) text.Append(" (movie)");
        }
        return text.ToString();
    }
}

// The pictures on screen, for a translator to name the picture a language asset replaces (the port's F5 and the
// driver's `pictures` step; docs/modding/translating.md, step 8). It samples the visible objects as a frame does and
// resolves names, never pixels.
public sealed partial class Graphics
{
    // How deep a render target drawn from other render targets is followed.
    private const int PictureDepthLimit = 4;

    // slot -> the catalog pictures a copy put into the surface (ops 0x207 and 0x32), so a surface the script built
    // names its pieces. A create, a load, a replacement, a clear and a release forget them.
    private readonly Dictionary<int, List<string>> _surfaceCopySources = new();

    /// <summary>The catalog files behind the objects the screen shows, back to front, each once: an object's
    /// resource; a surface's loaded resource, with text drawn on it or not; and for a surface the script built, the
    /// pictures copied into it and the objects a present drew into it. An object drawn at opacity 0 shows nothing and
    /// names nothing. The engine thread's.</summary>
    public IReadOnlyList<PictureOnScreen> PicturesOnScreen()
    {
        var found = new List<PictureOnScreen>();
        var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        GfxHandleRange range = _scene.BackbufferRange;
        foreach (RenderObject item in State.SnapshotVisibleObjects(_clock.NowMs))
            if (range.Contains(item.Handle)) AddObject(item, 0);
        return found;

        void Add(string name, bool movie)
        {
            if (seen.Add(name)) found.Add(new PictureOnScreen(name, movie));
        }

        void AddObject(in RenderObject item, int depth)
        {
            if (item.Polygon is { } polygon)
            {
                foreach (PolygonTextureState texture in (PolygonTextureState[])[polygon.Texture0, polygon.Texture1])
                    if (texture.Slot >= 0) AddSurface(texture.Slot, texture.ResourceId, depth);
                return;
            }
            if (item.Alpha == 0 && item.Blend != BlendKind.Opaque) return;
            if (State.TryGetSourceSlot(item.Handle, out int slot)) AddSurface(slot, item.SurfaceResId, depth);
            else if (item.SurfaceResId != 0) AddResource(item.SurfaceResId);
        }

        // As ResolveSurfaceTexture picks what a surface shows. A movie mask's capture names nothing.
        void AddSurface(int slot, long fallbackResourceId, int depth)
        {
            if (_movies.IsMaskedSurface(slot) && _surfaceImages.ContainsKey(slot)) return;
            if (_movies.TryResolveSurface(slot, out var movie) && movie != null)
            {
                Add(movie.Name, true);
                return;
            }
            if (_movies.IsBound(slot)) return;
            if (!_surfaceImages.ContainsKey(slot))
            {
                if (fallbackResourceId != 0) AddResource(fallbackResourceId);
                return;
            }
            // Pixels of its own: its loaded resource, what was copied into it, what a present drew there.
            if (_surfaceResources.TryGetValue(slot, out long resourceId)) AddResource(resourceId);
            if (_surfaceCopySources.TryGetValue(slot, out var copied))
                foreach (string name in copied) Add(name, false);
            if (depth < PictureDepthLimit && _renderTargetSnapshots.TryGetValue(slot, out var drawn))
                foreach (RenderObject item in drawn) AddObject(item, depth + 1);
        }

        void AddResource(long resourceId)
        {
            if (ResourcePicture(resourceId) is { } picture) Add(picture.Name, picture.Movie);
        }
    }

    private PictureOnScreen? ResourcePicture(long resourceId)
    {
        if (_movies.IsKnownMovieResource(resourceId))
            return _res?.RecordName(resourceId) is { } movie ? new PictureOnScreen(movie, true) : null;
        return _res?.ResolveTexture(resourceId) is { } asset ? new PictureOnScreen(asset.Name, false) : null;
    }

    // A copy from one surface into another: the destination now shows the source's pictures too.
    private void NoteCopySources(int source, int destination)
    {
        var names = new List<string>();
        if (_surfaceResources.TryGetValue(source, out long resourceId) && ResourcePicture(resourceId) is { } picture)
            names.Add(picture.Name);
        if (_surfaceCopySources.TryGetValue(source, out var copied)) names.AddRange(copied);
        if (names.Count == 0) return;
        if (!_surfaceCopySources.TryGetValue(destination, out var sources))
            _surfaceCopySources[destination] = sources = new List<string>();
        foreach (string name in names)
            if (!sources.Contains(name, System.StringComparer.OrdinalIgnoreCase)) sources.Add(name);
    }
}
