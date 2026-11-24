using System.Globalization;
using System.Text.Json;
using Ome.Engine.Diagnostics;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Mods.Launch;
using Ome.Mods.Linking;
using Ome.Mods.Packages;

namespace Ome.Commands;

/// <summary>A place <c>locate</c> cannot find: a missing map, page, script or offset. Printed as <c>error: …</c>,
/// exit 1.</summary>
public sealed class LocateException(string message) : Exception(message);

/// <summary>
/// <c>locate SCENE PAGE</c> and <c>locate SCRIPT@OFFSET</c> (modding from a release spec §7, plan §3.3): a page the
/// locator HUD names, resolved through the page map the engine wrote, or a site, with the listing around it. It prints
/// what <c>tools/locate_page.py</c> prints, after a line that names the map it read.
/// </summary>
public static class LocateCommand
{
    public const int DefaultContext = 8;
    public const string Usage = "usage: locate SCENE PAGE | locate SCRIPT@OFFSET [--context N] [--map FILE] "
                                + "[--no-names] [--mod PATH ...] (a page as 3 or P003; an offset as 0x8a6 or 2214)";

    public static int Run(CommandInputs inputs)
    {
        try
        {
            Console.Out.Write(Locate(inputs));
            return 0;
        }
        catch (LocateException error)
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 1;
        }
    }

    /// <summary>The report, every line ending with a line feed. Throws <see cref="CommandUsageException"/> for a command
    /// line it cannot read, and <see cref="LocateException"/> for a place it cannot find.</summary>
    public static string Locate(CommandInputs inputs)
    {
        string[] args = inputs.Args;
        if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal)) throw new CommandUsageException(Usage);
        string? second = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        int context = DefaultContext;
        if (inputs.Option("--context") is { } contextText
            && (!int.TryParse(contextText, NumberStyles.None, CultureInfo.InvariantCulture, out context)))
            throw new CommandUsageException($"--context takes a number of instructions, not {contextText}");

        var output = new StringWriter { NewLine = "\n" };
        if (second == null)
        {
            if (!args[1].Contains('@')) throw new CommandUsageException(Usage);
            (string site, int offset) = ParseSite(args[1]);
            using var listed = ListedScript.Find(inputs, site);
            foreach (string note in listed.Notes(offset)) output.WriteLine(note);
            WriteListing(output, listed, site, offset, context);
            return output.ToString();
        }

        string scene = PageLocatorState.NormalizeScript(args[1]);
        int page = ParsePage(second);
        string map = FindMap(inputs, scene);
        output.WriteLine($"page map: {map} (written {File.GetLastWriteTime(map):yyyy-MM-dd HH:mm:ss})");
        JsonElement record = FindPage(map, scene, page);
        string waitScript = Text(record, "wait_script") ?? throw new LocateException($"{map}: page {page} has no wait_script");
        int waitOffset = Text(record, "wait_offset") is { } waitText && Offset(waitText) is { } parsed
            ? parsed
            : throw new LocateException($"{map}: page {page} has no wait_offset that is an offset");
        using var script = ListedScript.Find(inputs, waitScript);
        foreach (string note in script.Notes(waitOffset)) output.WriteLine(note);

        output.WriteLine($"{Text(record, "root_scene")} P{page:000}");
        output.WriteLine($"page start: {Text(record, "page_start_script") ?? "?"}@{Text(record, "page_start_offset") ?? "?"}");
        output.WriteLine($"last text:  {Text(record, "text_script") ?? "?"}@{Text(record, "text_offset") ?? "?"}");
        output.WriteLine($"wait:       {waitScript}@0x{waitOffset:x}");
        if (record.TryGetProperty("call_stack", out JsonElement stack) && stack.ValueKind == JsonValueKind.Array
            && stack.GetArrayLength() > 0)
            output.WriteLine("call stack: " + string.Join(" > ", stack.EnumerateArray().Select(frame => frame.ToString())));
        if (Text(record, "text") is { } text)
            output.WriteLine("text: " + text.Replace('\r', ' ').Replace('\n', ' '));
        output.WriteLine();
        WriteListing(output, script, waitScript, waitOffset, context);
        return output.ToString();
    }

    /// <summary>The maps <c>locate</c> looks for, in order (spec §7.2): <c>--map</c>; else the profile's
    /// <c>diagnostics/page-maps/</c>; else, with no profile, the checkout's folder (<c>Ome.Cli</c>) and the bare
    /// engine's, <c>games/&lt;namespace&gt;/diagnostics/page-maps/</c> in Godot's user folder for the project.</summary>
    public static IReadOnlyList<string> MapCandidates(CommandInputs inputs, string scene, string? godotUserFolder = null)
    {
        string file = PageLocatorState.MapFileName(scene);
        if (inputs.Option("--map") is { } map) return [Path.GetFullPath(map)];
        if (inputs.Run.DataRoot is { } root)
            return [Path.Combine(Path.GetFullPath(root), Ome.Launcher.Core.EngineLaunch.DiagnosticsFolder,
                                 PageLocatorState.MapFolder, file)];
        var candidates = new List<string>();
        if (inputs.Defaults.PageMapFolder?.Invoke(inputs.GameId) is { } checkout)
            candidates.Add(Path.Combine(checkout, file));
        candidates.Add(Path.Combine(godotUserFolder ?? MachineState.DefaultFolder(), "games",
                                    inputs.Selected.Profile.Persistence.StorageNamespace,
                                    Ome.Launcher.Core.EngineLaunch.DiagnosticsFolder, PageLocatorState.MapFolder, file));
        return candidates;
    }

    private static string FindMap(CommandInputs inputs, string scene)
    {
        IReadOnlyList<string> candidates = MapCandidates(inputs, scene);
        return candidates.FirstOrDefault(File.Exists) ?? throw new LocateException(
            $"no page map for {scene}: looked for {string.Join(", then ", candidates)}. Run the engine to the page first "
            + "(the map is written at each page's wait), or give the place as SCRIPT@OFFSET from the locator's wait part");
    }

    private static JsonElement FindPage(string map, string scene, int page)
    {
        int number = 0;
        foreach (string line in File.ReadLines(map))
        {
            number++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            JsonElement record;
            try
            {
                using JsonDocument document = JsonDocument.Parse(line);
                record = document.RootElement.Clone();
            }
            catch (JsonException error)
            {
                throw new LocateException($"{map}:{number}: invalid JSON: {error.Message}");
            }
            if (record.ValueKind == JsonValueKind.Object
                && PageLocatorState.NormalizeScript(Text(record, "root_scene") ?? "") == scene
                && record.TryGetProperty("page", out JsonElement value) && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out int recorded) && recorded == page)
                return record;
        }
        throw new LocateException($"{scene} page {page} is not in {map}; a page number holds for the run that wrote "
                                  + "the map only, so run the engine to the page again");
    }

    private static void WriteListing(TextWriter output, ListedScript listed, string script, int offset, int context)
    {
        IReadOnlyList<Instruction> instructions = listed.Script.Instructions;
        if (!listed.Script.IndexByOffset.TryGetValue(offset, out int index))
            throw new LocateException($"offset 0x{offset:x} is not an instruction boundary of {listed.Name}");
        output.WriteLine($"disassembly around {script}@0x{offset:x}:");
        int begin = Math.Max(0, index - context), end = Math.Min(instructions.Count, index + context + 1);
        for (int k = begin; k < end; k++)
        {
            Instruction instruction = instructions[k];
            output.WriteLine($"{(k == index ? ">>>" : "   ")} 0x{instruction.Offset:x5}: "
                             + listed.Listing.Line(listed.Script, instruction));
        }
    }

    /// <summary>A page as <c>3</c>, <c>P003</c> or <c>p3</c>, as the locator HUD and its copy give it.</summary>
    public static int ParsePage(string text)
    {
        string digits = text.Length > 1 && text[0] is 'P' or 'p' ? text[1..] : text;
        return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int page) && page > 0
            ? page
            : throw new CommandUsageException($"{text} is not a page: give its number, as 3 or P003");
    }

    /// <summary>A site as <c>SC0000@0x8a6</c> (the locator's wait or text part) or with a decimal offset.</summary>
    public static (string Script, int Offset) ParseSite(string text)
    {
        int at = text.LastIndexOf('@');
        string script = PageLocatorState.NormalizeScript(text[..at]);
        if (script.Length == 0) throw new CommandUsageException($"{text} names no script: give SCRIPT@OFFSET");
        return (script, Offset(text[(at + 1)..])
                        ?? throw new CommandUsageException($"{text}: {text[(at + 1)..]} is not an offset (0x8a6, or 2214)"));
    }

    /// <summary>A dword offset in hex with <c>0x</c> or in decimal, as <c>int(text, 0)</c> reads the map's.</summary>
    private static int? Offset(string text)
    {
        bool hex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        return int.TryParse(hex ? text[2..] : text, hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
                            CultureInfo.InvariantCulture, out int offset)
            ? offset
            : null;
    }

    /// <summary>A string field, or null when it is absent, not a string, or empty (as <c>locate_page.py</c>'s
    /// <c>or '?'</c> reads it).</summary>
    private static string? Text(JsonElement record, string name)
        => record.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
           && value.GetString() is { Length: > 0 } text
            ? text
            : null;

    /// <summary>The script the engine ran (spec §7.2): the stack's, when a mod in the profile or a <c>--mod</c>
    /// changes it, else the install's, from the catalog with the profile's overlays.</summary>
    private sealed class ListedScript : IDisposable
    {
        private readonly ModInspection? _inspection;
        private readonly Ome.Engine.Modding.LazyModRuntime? _lua;
        private readonly LinkedScript? _linked;
        private readonly string? _newIn;
        private readonly string? _linkNote;

        private ListedScript(string name, Ome.Engine.Model.Script script, ScriptListing listing,
                             (ModInspection? Inspection, Ome.Engine.Modding.LazyModRuntime? Lua) link,
                             LinkedScript? linked, string? newIn, string? linkNote)
        {
            Name = name;
            Script = script;
            Listing = listing;
            (_inspection, _lua) = link;
            _linked = linked;
            _newIn = newIn;
            _linkNote = linkNote;
        }

        public string Name { get; }
        public Ome.Engine.Model.Script Script { get; }
        public ScriptListing Listing { get; }

        public static ListedScript Find(CommandInputs inputs, string script)
        {
            string name = PageLocatorState.NormalizeScript(script) + ".BIN";
            ScriptListing listing = ListingFor(inputs);
            List<string> packages = inputs.Options("--mod").Select(Path.GetFullPath).ToList();
            ModInspection? inspection = null;
            Ome.Engine.Modding.LazyModRuntime? lua = null;
            string? note = null;
            if (inputs.Run.DataRoot != null || packages.Count > 0)
            {
                // The Lua runtime checks each lua/ file as the link reads it (Lua scripting spec §13), as omp check's does.
                lua = Ome.Scripting.Lua.LuaModRuntime.Lazy();
                inspection = ModInspection.Inspect(inputs.LinkBase(lua), inputs.Run.DataRoot, packages,
                                                   inputs.Has(ModLaunchOptions.SkipFailedOption), inputs.Run.Mods);
                // The engine would refuse to start, so the listing may not be what ran: the lines that stop the link say
                // why (omp check prints the whole report).
                IEnumerable<ModReportLine> stops = (inspection.Link?.Report.Lines ?? inspection.Discovery.Lines)
                    .Where(line => line.Stops);
                if (inspection.Link == null)
                    note = "note: a package did not open, so the stack does not link; listing the install's script";
                else if (!inspection.Link.Succeeded)
                    note = $"note: the stack does not link ({inspection.Link.Report.Summary()}); listing what linked";
                if (note != null) note = string.Join('\n', stops.Select(line => $"  {line}").Prepend(note));
            }
            try
            {
                ModLinkResult? link = inspection?.Link;
                if (link != null && link.Scripts.TryGetValue(name, out LinkedScript? linked))
                    return new ListedScript(name, Parse(inputs, linked.Bytes, name), listing, (inspection, lua), linked,
                                            null, note);
                foreach (LinkedCatalog catalog in link?.Catalogs ?? [])
                    foreach (LinkedRecord record in catalog.Records)
                        if (record.IsScript && !record.Stub
                            && record.CatalogName.Equals(name, StringComparison.OrdinalIgnoreCase))
                            return new ListedScript(name, Parse(inputs, record.Bytes, name), listing, (inspection, lua),
                                                    null, catalog.ModId, note);
                if (inputs.Catalog.ResolveMountedName(name) is not { IsPlaceholder: false } entry)
                    throw new LocateException($"{name} is neither in the install's catalog nor added by a mod");
                return new ListedScript(name, Parse(inputs, inputs.AssetStore.ReadAll(entry), name), listing,
                                        (inspection, lua), null, null, note);
            }
            catch
            {
                inspection?.Dispose();
                lua?.Dispose();
                throw;
            }
        }

        /// <summary>The lines before the listing: whether the link failed, and which mods changed the script.</summary>
        public IEnumerable<string> Notes(int offset)
        {
            if (_linkNote != null) yield return _linkNote;
            if (_newIn != null) yield return $"script: {Name} is {_newIn}'s, from its append catalog";
            if (_linked == null) yield break;
            if (_linked.ReplacedBy != null)
            {
                yield return $"script: {Name} is replaced by {_linked.ReplacedBy}"
                             + (_linked.PatchedBy.Count > 0 ? $" and patched by {string.Join(", ", _linked.PatchedBy)}" : "");
                yield break;
            }
            // Each plan names its mod: "mod.id (patches/stem)".
            yield return $"script: {Name} is patched by "
                         + string.Join(", ", _linked.Plans.Count > 0 ? _linked.Plans : _linked.PatchedBy);
            // A fragment names the base script's offsets, which a run without those mods gives (spec §7.2).
            yield return _linked.Offsets?.Unmap(offset) is { } baseOffset
                ? $"script: the offsets are the linked script's; 0x{offset:x} is 0x{baseOffset:x} in the base script, "
                  + "which a fragment names and a run without those mods gives"
                : $"script: the offsets are the linked script's; 0x{offset:x} is an instruction a patch added, which the "
                  + "base script does not have";
        }

        public void Dispose()
        {
            _inspection?.Dispose();
            _lua?.Dispose();
        }

        private static Ome.Engine.Model.Script Parse(CommandInputs inputs, byte[] bytes, string name)
        {
            try
            {
                return Sys4Loader.Parse(bytes, inputs.Table, name);
            }
            catch (Exception error) when (error is InvalidDataException or Sys4OpcodeDecodeException)
            {
                throw new LocateException($"{name} does not decode: {error.Message}");
            }
        }

        private static ScriptListing ListingFor(CommandInputs inputs)
        {
            if (inputs.Has("--no-names")) return new ScriptListing(inputs.Table, inputs.Contracts);
            Ome.Engine.Modding.GameLayout? layout = ShippedPackages.BaseLayoutOf(inputs.GameId)?.Layout;
            Sys4AssetCatalog catalog = inputs.Catalog;
            return new ScriptListing(inputs.Table, inputs.Contracts,
                id => catalog.ResolvePacked(id) is { IsPlaceholder: false } entry ? entry.Name : null,
                layout == null ? null : (kind, address) => layout.NameAt(kind, address));
        }
    }
}
