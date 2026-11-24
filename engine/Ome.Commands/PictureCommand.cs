using Ome.Engine.Model;
using Ome.Engine.Sys4;

namespace Ome.Commands;

/// <summary><c>picture NAME... | --all [--output-dir DIR]</c>: decode the game's pictures as the port draws them
/// (<see cref="ResourceMap.DecodeTexture"/>: a loose file first, then the archives, and a language package's PNG) and
/// write each as <c>NAME.png</c>. A name is the catalog record's, with an append's <c>$n$</c> prefix; <c>--all</c>
/// writes every AGF record of the catalog and its appends. Without <c>--output-dir</c>, the program's folder
/// (<see cref="CommandDefaults.PictureFolder"/>).</summary>
internal static class PictureCommand
{
    /// <summary>The options that take a value, whose values are no picture's name.</summary>
    private static readonly string[] Valued =
        ["--output-dir", "--game", "--game-root", "--overlay-root", "--data-root", "--mod", "--home"];

    public static int Run(CommandInputs inputs)
    {
        string[] args = inputs.Args;
        Sys4AssetCatalog catalog = inputs.Catalog;
        string outputDir = Path.GetFullPath(inputs.Option("--output-dir") ?? inputs.Defaults.PictureFolder(inputs.GameId));
        var pictures = new ResourceMap(catalog, inputs.AssetStore, Console.Error.WriteLine, inputs.AssetOptions.AllowBmpAsAgf);
        var names = new List<string>();
        for (int k = 1; k < args.Length; k++)
        {
            if (Valued.Contains(args[k])) { k++; continue; }
            if (!args[k].StartsWith("--", StringComparison.Ordinal)) names.Add(args[k]);
        }
        bool all = inputs.Has("--all");
        List<AssetEntry> entries;
        if (all)
            entries = catalog.Files.Concat(catalog.AppendPacks.OrderBy(pair => pair.Key).SelectMany(pair => pair.Value.Files))
                .Where(entry => entry.Name.EndsWith(".AGF", StringComparison.OrdinalIgnoreCase)).ToList();
        else
        {
            if (names.Count == 0)
            {
                Console.Error.WriteLine("usage: picture <NAME...> | --all [--output-dir DIR]");
                return 2;
            }
            entries = [];
            foreach (string name in names)
            {
                if (catalog.ResolveMountedName(name) is { IsPlaceholder: false } entry) entries.Add(entry);
                else { Console.Error.WriteLine($"picture: no record {name} in the catalog or its appends"); return 1; }
            }
        }
        Directory.CreateDirectory(outputDir);
        int written = 0, failed = 0, movies = 0;
        foreach (AssetEntry entry in entries)
        {
            try
            {
                RgbaImage image = pictures.DecodeTexture(entry);
                string file = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(entry.Name) + ".png");
                File.WriteAllBytes(file, PngWriter.Write(image));
                written++;
                if (!all) Console.WriteLine($"{entry.Name}: {image.Width}x{image.Height} -> {file}");
            }
            // A movie is an MPEG stream under an AGF name (ResourceMap.ReadMovie): --all passes over it.
            catch (InvalidDataException) when (IsMoviePayload(entry))
            {
                movies++;
                if (all) continue;
                failed++;
                Console.Error.WriteLine($"picture: {entry.Name} is a movie (an MPEG stream), not a picture");
            }
            catch (Exception error) when (error is InvalidDataException or IOException)
            {
                failed++;
                Console.Error.WriteLine($"picture: {entry.Name}: {error.Message}");
            }
        }
        Console.WriteLine($"picture: {written} written to {outputDir}" + (all && movies > 0 ? $", {movies} movies passed over" : "")
                          + (failed > 0 ? $", {failed} failed" : ""));
        return failed > 0 ? 1 : 0;

        bool IsMoviePayload(AssetEntry entry)
        {
            try { pictures.ReadMovie(entry); return true; }
            catch (InvalidDataException) { return false; }
        }
    }
}
