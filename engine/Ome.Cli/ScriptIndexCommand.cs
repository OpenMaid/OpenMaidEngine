using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Export;
using Ome.Script.Abi;
using Ome.Script.Patch;

/// <summary>
/// <c>script-index</c> (game data export spec §8.2, plan §4.3): indexes every script the selected install's catalog
/// mounts and writes <c>docs/script-index-&lt;game&gt;.json</c> (or <c>--output</c>); <c>--check</c> compares it with the
/// committed file instead. Exit 0 when written or equal, 1 when the check differs, 2 when it could not run.
/// </summary>
internal static class ScriptIndexCommand
{
    public static int Run(string[] args, string gameId, Sys4AssetCatalog catalog, Sys4AssetStore store,
                          Sys4ScriptProvider provider, OpcodeTable table)
    {
        int at = Array.IndexOf(args, "--output");
        string path = Path.GetFullPath(at >= 0 && at + 1 < args.Length
                                           ? args[at + 1]
                                           : Path.Combine(Paths.Repo, "docs", $"script-index-{gameId}.json"));
        bool check = args.Contains("--check");
        string globals = Paths.GlobalsJson(gameId);
        if (!File.Exists(globals))
        {
            Console.Error.WriteLine($"script-index: no {globals}; run tools/globals_build.py --game {gameId} --build");
            return 2;
        }

        ScriptIndex index = Build(catalog, store, provider, table, out IReadOnlyList<string> unreadable);
        foreach (string problem in unreadable) Console.Error.WriteLine($"script-index: {problem}");
        string pointsPath = Paths.PatchPointsJson(gameId);
        string text = index.ToJson(gameId, GlobalNames.Load(globals),
                                   File.Exists(pointsPath) ? OmsPatchPoints.Load(pointsPath) : null,
                                   id => NameOfId(catalog, id));
        int scripts = index.Entries.Count;
        if (check)
        {
            string? committed = File.Exists(path) ? File.ReadAllText(path).ReplaceLineEndings("\n") : null;
            if (committed == text)
            {
                Console.WriteLine($"script-index: {path} matches the install ({scripts} scripts)");
                return unreadable.Count > 0 ? 1 : 0;
            }
            Console.WriteLine(committed is null
                ? $"script-index: {path} is missing; run Ome.Cli script-index --game {gameId}"
                : $"script-index: {path} differs from the install ({FirstDifference(committed, text)}); run Ome.Cli "
                  + $"script-index --game {gameId} and commit it");
            return 1;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
        Console.WriteLine($"script-index: wrote {path} ({scripts} scripts, {text.Length / 1024} KiB)");
        return unreadable.Count > 0 ? 1 : 0;
    }

    /// <summary>The index of every script the catalog and its appends mount, each read as the engine reads it (loose
    /// files included); a script that does not parse is left out and named in <paramref name="unreadable"/>.</summary>
    public static ScriptIndex Build(Sys4AssetCatalog catalog, Sys4AssetStore store, Sys4ScriptProvider provider,
                                    OpcodeTable table, out IReadOnlyList<string> unreadable)
    {
        var scripts = new List<IndexedScript>();
        var problems = new List<string>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (PackedAssetEntry packed in catalog.EnumerateScripts())
        {
            if (packed.Asset.IsPlaceholder) continue;
            if (!names.Add(packed.Asset.Name))
            {
                problems.Add($"{packed.Asset.Name} (id {packed.PackedId}) repeats an earlier script's name; the first is indexed");
                continue;
            }
            try
            {
                Script script = provider.GetById(packed.PackedId)
                                ?? throw new InvalidDataException("the provider reads no script");
                scripts.Add(new IndexedScript(packed.Asset.Name, packed.PackedId,
                                              OmsPatchPlan.FingerprintOf(store.ReadAll(packed.Asset)), script));
            }
            catch (Exception failure) when (failure is InvalidDataException or IOException)
            {
                problems.Add($"{packed.Asset.Name} (id {packed.PackedId}): {failure.Message}");
            }
        }
        unreadable = problems;
        OpcodeContracts contracts = OpcodeContracts.Load(Paths.OpcodesJson);
        return ScriptIndex.Build(scripts, table, ScriptReading.Of(contracts, table.AbiId));
    }

    /// <summary>The script a call-script id names, as the engine resolves it; null for none.</summary>
    public static string? NameOfId(Sys4AssetCatalog catalog, long id)
        => catalog.ResolvePacked(id) is { IsPlaceholder: false } entry
           && entry.Name.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase)
            ? entry.Name
            : null;

    private static string FirstDifference(string committed, string built)
    {
        string[] a = committed.Split('\n'), b = built.Split('\n');
        for (int k = 0; k < Math.Max(a.Length, b.Length); k++)
            if (k >= a.Length || k >= b.Length || a[k] != b[k])
                return $"first at line {k + 1}: committed {(k < a.Length ? a[k].Trim() : "(end)")}, "
                       + $"install {(k < b.Length ? b[k].Trim() : "(end)")}";
        return "line endings";
    }
}
