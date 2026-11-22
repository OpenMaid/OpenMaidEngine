using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Ome.Engine.Devices;
using Ome.Engine.Frontend.Shell;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Ome.Export;
using Ome.Script.Patch;

/// <summary>
/// <c>data-export</c> (game data export spec §6.1): reads the selected game's data through the engine, as its
/// declarations (<c>build/games/&lt;game&gt;/entities.json</c>) publish it, and writes <c>&lt;revision&gt;.sqlite</c>,
/// its pictures and <c>manifest.json</c> to <c>--output</c> (default <c>build/games/&lt;game&gt;/export/</c>). Exit 0
/// on a clean export, 1 when a value was refused (§5.5; the rest is still written), 2 when it could not run.
/// </summary>
internal static class DataExportCommand
{
    private const int ListedPerSection = 12;

    public static int Run(string[] args, string gameId, string gameRoot, Sys4AssetCatalog catalog, Sys4AssetStore store,
                          Sys4ScriptProvider provider, OpcodeTable table, EngineBuild build, bool allowBmpAsAgf)
    {
        string? Option(string name)
        {
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
        if (args.Contains("--language"))
        {
            Console.Error.WriteLine("data-export: --language is exported from milestone 7 of the game data export (spec §13)");
            return 2;
        }
        string output = Path.GetFullPath(Option("--output") ?? Path.Combine(Paths.GameBuild(gameId), "export"));
        string entitiesJson = Path.Combine(Paths.GameBuild(gameId), "entities.json");
        if (!File.Exists(entitiesJson))
        {
            Console.Error.WriteLine($"data-export: no {entitiesJson}; run tools/entities_build.py --game {gameId} --build");
            return 2;
        }

        var clock = Stopwatch.StartNew();
        ExportInputs inputs;
        string revision;
        try
        {
            ExportDeclarations declarations = ExportDeclarations.Load(entitiesJson);
            if (declarations.Profile != gameId)
                throw new ExportFailure($"{entitiesJson} declares {declarations.Profile}, not {gameId}");
            RunStop bootStop = ReadAt(gameId, declarations.ReadAt, catalog, store);
            revision = RevisionOf(gameRoot, catalog, store, out string catalogFingerprint);
            inputs = new ExportInputs
            {
                GameId = gameId,
                Table = table,
                Build = build,
                Scripts = provider,
                ScriptByName = name => provider.GetByMountedName(name.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase)
                                                                     ? name
                                                                     : name + ".BIN"),
                BootStop = bootStop,
                StopOf = point => ReadAt(gameId, point, catalog, store),
                Declarations = declarations,
                Registry = RegistryView.Load(Paths.GlobalsJson(gameId)),
                Shape = BaseModShape(gameId),
                ScriptIndex = ScriptIndexCommand.Build(catalog, store, provider, table, out _),
                Names = GlobalNames.Load(Paths.GlobalsJson(gameId)),
                Pictures = new CatalogPictures(new ResourceMap(catalog, store, Console.Error.WriteLine, allowBmpAsAgf)),
                BuildInfo =
                [
                    ("title", catalog.Title),
                    ("revision", revision),
                    ("fingerprint", catalogFingerprint),
                    ("catalog_revision", catalog.Magic.Trim()),
                    ("engine_build", build.Id),
                    ("layers", string.Join(",", catalog.AppendPacks.OrderBy(pair => pair.Key).Select(pair => $"${pair.Key}$"))),
                    ("ome_version", OmeVersion()),
                    ("ome_commit", OmeCommit()),
                ],
            };
        }
        catch (Exception error) when (error is ExportFailure or InvalidDataException or IOException)
        {
            Console.Error.WriteLine($"data-export: {error.Message}");
            return 2;
        }

        ExportResult result;
        try
        {
            result = GameDataExporter.Run(inputs);
        }
        catch (Exception error) when (error is ExportFailure or InvalidDataException)
        {
            Console.Error.WriteLine($"data-export: {error.Message}");
            return 2;
        }
        TimeSpan read = clock.Elapsed;
        var manifest = new ExportManifest(DateTimeOffset.UtcNow, "Ome.Cli " + string.Join(" ", args),
                                          GameVersionRule.Of(catalog.StartupSettings), OmeVersion(), OmeCommit());
        ExportFiles files = ExportWriter.Write(result, output, revision, manifest);
        Print(result, output, files, read, clock.Elapsed);
        return result.Report.ExitCode;
    }

    /// <summary>The game's base mod's shape of its data kind, which every snippet is checked against (export plan
    /// §4.3); null for a game whose base mod ships none.</summary>
    private static Ome.Mods.Data.DataShape? BaseModShape(string gameId)
    {
        string path = Path.Combine(Paths.Repo, "mods", $"{gameId}-base", "shapes", "tables.schema.json");
        if (!File.Exists(path)) return null;
        Ome.Mods.Data.DataShapeRead read = Ome.Mods.Data.DataShape.Read(File.ReadAllText(path));
        return read.Shape ?? throw new ExportFailure($"{path}: {string.Join("; ", read.Problems)}");
    }

    /// <summary>A stop at a patch point: its site in the installed script's revision (the boot's <c>read_at</c>, a
    /// loader's).</summary>
    private static RunStop ReadAt(string gameId, string readAt, Sys4AssetCatalog catalog, Sys4AssetStore store)
    {
        OmsPatchPoints points = OmsPatchPoints.Load(Paths.PatchPointsJson(gameId));
        if (!points.TryGet(readAt, out OmsPatchPoint point))
            throw new ExportFailure($"read_at {readAt} is no patch point of {gameId}");
        AssetEntry script = catalog.ResolveMountedName(point.Script)
                            ?? throw new ExportFailure($"{readAt} lies in {point.Script}, which the install lacks");
        string fingerprint = OmsPatchPlan.FingerprintOf(store.ReadAll(script));
        int site = points.Resolve(readAt, point.Script, fingerprint, out string? reason)
                   ?? throw new ExportFailure(reason ?? $"{readAt} has no site in this install");
        return new RunStop(point.Script, site);
    }

    /// <summary>The revision id (spec §6.8): the first 12 hex digits of a SHA-256 over the sorted names and fingerprints
    /// of SYS4INI.BIN, each append catalog beside it, and each script a loose file serves.</summary>
    private static string RevisionOf(string gameRoot, Sys4AssetCatalog catalog, Sys4AssetStore store,
                                     out string catalogFingerprint)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        catalogFingerprint = OmsPatchPlan.FingerprintOf(File.ReadAllBytes(Path.Combine(gameRoot, "SYS4INI.BIN")));
        files["SYS4INI.BIN"] = catalogFingerprint;
        foreach (string append in Directory.EnumerateFiles(gameRoot, "*.AAI"))
            files[Path.GetFileName(append).ToUpperInvariant()] = OmsPatchPlan.FingerprintOf(File.ReadAllBytes(append));
        foreach (PackedAssetEntry script in catalog.EnumerateScripts())
            if (store.Locate(script.Asset) is { Loose: true } loose)
                files["loose:" + script.Asset.Name.ToUpperInvariant()] = OmsPatchPlan.FingerprintOf(File.ReadAllBytes(loose.Path));
        var text = new StringBuilder();
        foreach ((string name, string fingerprint) in files) text.Append(name).Append('\t').Append(fingerprint).Append('\n');
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())))[..12];
    }

    /// <summary>The catalog's pictures, decoded as <c>Ome.Cli picture</c> decodes them (spec §6.5), once each.</summary>
    private sealed class CatalogPictures(ResourceMap resources) : IPictureSource
    {
        private readonly Dictionary<long, ExportPicture?> _decoded = new();

        public ExportPicture? Resolve(long packedId)
        {
            if (_decoded.TryGetValue(packedId, out ExportPicture? known)) return known;
            ExportPicture? picture = null;
            if (resources.ResolveTexture(packedId) is { } entry)
                try
                {
                    RgbaImage image = resources.DecodeTexture(entry);
                    picture = new ExportPicture(Path.GetFileNameWithoutExtension(entry.Name), PngWriter.Write(image),
                                                image.Width, image.Height);
                }
                catch (Exception error) when (error is InvalidDataException or IOException)
                {
                    Console.Error.WriteLine($"data-export: picture {entry.Name}: {error.Message}");
                }
            _decoded[packedId] = picture;
            return picture;
        }
    }

    private static string OmeVersion() => Git("describe", "--tags", "--always", "--dirty") ?? "unknown";

    private static string OmeCommit() => Git("rev-parse", "HEAD") ?? "unknown";

    private static readonly Dictionary<string, string?> GitAnswers = new(StringComparer.Ordinal);

    private static string? Git(params string[] arguments)
    {
        string key = string.Join(' ', arguments);
        if (GitAnswers.TryGetValue(key, out string? known)) return known;
        string? answer = null;
        try
        {
            var start = new ProcessStartInfo("git") { WorkingDirectory = Paths.Repo, RedirectStandardOutput = true,
                                                      RedirectStandardError = true, UseShellExecute = false };
            foreach (string argument in arguments) start.ArgumentList.Add(argument);
            using Process git = Process.Start(start)!;
            string text = git.StandardOutput.ReadToEnd().Trim();
            git.WaitForExit();
            if (git.ExitCode == 0 && text.Length > 0) answer = text;
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
        GitAnswers[key] = answer;
        return answer;
    }

    private static void Print(ExportResult result, string output, ExportFiles files, TimeSpan read, TimeSpan total)
    {
        ExportReport report = result.Report;
        Console.WriteLine($"data-export: {result.Declarations.Profile}, boot {report.BootHalt}, read in {read.TotalSeconds:0.0} s");
        foreach (ExportKind kind in result.Kinds)
        {
            int texts = kind.Entities.Sum(entity => entity.Texts.Count);
            int refs = kind.Entities.Sum(entity => entity.Refs.Count);
            int pictures = kind.Entities.Sum(entity => entity.Pictures.Count);
            Console.WriteLine($"  {kind.Declaration.Id,-10} {kind.Entities.Count,5} entities, {texts,5} texts, {refs,5} refs, "
                              + $"{pictures,5} pictures");
        }
        Console.WriteLine($"  descriptions: {report.DescriptionRuns} runs, {report.EmptyDescriptions} empty, "
                          + $"{report.DroppedPieces} pieces of other scripts dropped, {report.DescriptionHalts.Count} halts, "
                          + $"{report.MissingScripts.Count} missing scripts");
        if (report.LoaderRuns > 0)
        {
            int rows = result.Kinds.SelectMany(kind => kind.Entities).Sum(entity => entity.Loaded.Count);
            Console.WriteLine($"  loaders: {report.LoaderRuns} runs, {report.LoaderHalts.Count} halts, {rows} rows");
        }
        int snippets = result.Kinds.SelectMany(kind => kind.Entities).Count(entity => entity.Snippet != null);
        Console.WriteLine($"  snippets: {snippets}, {report.SnippetProblems.Count} problems");
        Section("refused", report.Refusals.Count, report.Refusals.Select(refusal =>
            $"{refusal.Kind} {refusal.Id} {refusal.Field}[{refusal.Index}] ({BankLetter(refusal.Bank)}:0x{refusal.Address:x}): {refusal.Reason}"));
        Section("snippet problems", report.SnippetProblems.Count, report.SnippetProblems.Select(problem =>
            $"{problem.Kind} {problem.Id}: {problem.Problem}"));
        Section("unnamed cells the builders wrote", report.UnnamedCells.Values.Sum(cells => cells.Length),
                report.UnnamedCells.Select(pair => $"{pair.Key}: {pair.Value.Length}, from 0x{pair.Value[0]:x} to "
                                                   + $"0x{pair.Value[^1]:x}"));
        Section("missing references", report.MissingRefs.Values.Sum(), report.MissingRefs.Select(pair =>
            $"{pair.Key.Kind}.{pair.Key.Field} -> {pair.Key.ToKind}: {pair.Value}"));
        Section("references to unpublished tables", report.UnpublishedRefs.Values.Sum(), report.UnpublishedRefs.Select(pair =>
            $"{pair.Key.Kind}.{pair.Key.Field} -> table {pair.Key.Table}: {pair.Value}"));
        Section("unresolved pictures", report.UnresolvedPictures.Count, report.UnresolvedPictures.Select(picture =>
            $"{picture.Kind} {picture.Id} {picture.Picture}: 0x{picture.PackedId:x}"));
        int explicitFiles = result.Kinds.SelectMany(kind => kind.Entities).SelectMany(entity => entity.Pictures)
            .Where(picture => picture.Picture.Explicit).Select(picture => picture.Picture.Name).Distinct().Count();
        if (explicitFiles > 0) Console.WriteLine($"  explicit pictures: {explicitFiles} files, blurred on the site");
        Section("explicit_pictures entries that match no picture", report.UnmatchedExplicitPictures.Count,
                report.UnmatchedExplicitPictures);
        Section("loader halts", report.LoaderHalts.Count, report.LoaderHalts.Select(halt =>
            $"{halt.Scope} for {halt.Kind} {halt.Id}: {halt.Halt}"));
        Section("description halts", report.DescriptionHalts.Count, report.DescriptionHalts.Select(halt =>
            $"{halt.Kind} {halt.Id} {halt.Description}: {halt.Halt}"));
        Section("missing description scripts", report.MissingScripts.Count, report.MissingScripts.Select(missing =>
            $"{missing.Kind} {missing.Id} {missing.Description}: 0x{missing.ScriptId:x}"));
        Console.WriteLine($"  wrote {files.Database} and {files.Files.Count - 1} pictures to {output} in {total.TotalSeconds:0.0} s");
        if (report.ExitCode != 0)
            Console.WriteLine($"data-export: {report.Refusals.Count} values refused (spec §5.5), {report.SnippetProblems.Count} "
                              + "snippet problems (plan §4.3); exit 1");
    }

    private static void Section(string title, int count, IEnumerable<string> lines)
    {
        if (count == 0) return;
        Console.WriteLine($"  {title}: {count}");
        List<string> listed = lines.ToList();
        foreach (string line in listed.Take(ListedPerSection)) Console.WriteLine($"    {line}");
        if (listed.Count > ListedPerSection) Console.WriteLine($"    … and {listed.Count - ListedPerSection} more");
    }

    private static string BankLetter(Ome.Engine.Natives.GlobalBank bank) => bank switch
    {
        Ome.Engine.Natives.GlobalBank.String => "GS",
        Ome.Engine.Natives.GlobalBank.Float => "GF",
        _ => "G",
    };
}
