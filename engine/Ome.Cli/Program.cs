using System.Text.Json;
using System.Text.RegularExpressions;
using Ome.Engine.Diagnostics;
using Ome.Engine.Devices;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;

const string CommandList = "save-diff, save-info, run-lock, intake, omp, oms-compile, oms-append, oms-patch, oms-rebuild, oms-temps, "
                          + "oms-roundtrip, oms-decompile, oms-residue, catalog-scan, run, audio, gfx, picture, init-image, data-export, "
                          + "script-index, play, "
                          + "sweep, trace";
// UTF-8 output while the program runs, and the console's own code page back at its end (modding from a release spec
// §6.1). A command that throws an ArgumentException for a missing or wrong option exits 2 with its message (spec §6.2),
// not as a crash.
using Ome.Commands.Utf8Console utf8Console = Ome.Commands.Utf8Console.Begin();
AppDomain.CurrentDomain.UnhandledException += (_, unhandled) =>
{
    if (unhandled.ExceptionObject is not ArgumentException usage) return;
    Console.Error.WriteLine($"error: {usage.Message}");
    utf8Console.Dispose();
    Environment.Exit(2);
};
if (args.Length == 0)
{
    Console.Error.WriteLine("usage: Ome.Cli COMMAND [options] (docs/tools-reference.md); commands: " + CommandList);
    return 2;
}

// The command is the first argument; an option value that happens to equal a command name never selects it.
string command = args[0];

// save-diff and save-info read only save files, so they run before the game catalog loads.
if (command == "save-diff") return SaveDiffCommand.Run(args);
if (command == "save-info") return SaveInfoCommand.Run(args);
// run-lock takes only a data root's lock (launcher spec §5.5): the run lock tests start it as a second process.
if (command == "run-lock") return RunLockCommand.Run(args);
// intake looks at an install that has no profile yet (docs/adding-a-game.md, step 1), so it runs before a game is
// selected and with the whole opcode catalog.
if (command == "intake") return IntakeCommand.Run(args);
// start is OME-Tool's alone (modding from a release spec §8), refused before --profile is looked up.
if (command == "start")
{
    Console.Error.WriteLine("error: start is OME-Tool's: it runs a release's engine, which is beside the tool. In a "
                            + "checkout, run the port with .\\run-godot.cmd, or drive it with tools/drive.py port");
    return 2;
}

// --profile ID names a player profile in OME home (modding from a release spec §6.3): it becomes that profile's
// --data-root, for the shared commands and this program's own alike.
if (Ome.Commands.ToolCommands.ExpandProfile(args) is not { } expandedArgs) return 2;
args = expandedArgs;

// The commands OME-Tool shares (spec §6.2) run with this checkout's defaults: games/<game>/ when no install is named,
// build/opcodes.json read live, and pictures in build/games/<game>/pictures.
if (Ome.Commands.ToolCommands.Handles(command))
    return Ome.Commands.ToolCommands.Run(args, new Ome.Commands.CommandDefaults
    {
        Program = "Ome.Cli",
        DefaultInstall = Paths.GameInstall,
        OpcodeTable = abi => OpcodeTableJson.Load(Paths.OpcodesJson, abi),
        Contracts = () => Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
        PictureFolder = game => Path.Combine(Paths.GameBuild(game), "pictures"),
        PageMapFolder = Paths.GameBuild,
    });

// Every option below reads the resolver's list (launcher spec §5.2): one occurrence of each single-valued option, the
// last given, and a --data-root's profile.toml mapped to the flags it stands for. The command keeps its place.
Ome.Mods.Launch.ResolvedRun run;
try
{
    run = Ome.Mods.Launch.RunResolver.Resolve(args, Directory.GetCurrentDirectory());
}
catch (Ome.Mods.Launch.RunResolutionException error)
{
    Console.Error.WriteLine($"error: {error.Message}");
    return 2;
}
args = run.Arguments.ToArray();
if (run.Describe() is { } profileFileLine) Console.Error.WriteLine($"[profile-file] {profileFileLine}");
foreach (string warning in run.Warnings) Console.Error.WriteLine($"warning: {warning}");

string? OptionValue(string name)
{
    int index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

// The conventional game root follows the game GameProfileSelection.Resolve picks below: the last --game.
string? requestedGame = GameProfileSelection.RequestedGameId(args);
// A game the registry does not know keeps Himegari's root, so the selection below reports the unknown id.
string conventionalGameRoot =
    requestedGame?.ToLowerInvariant() is { } requestedId && GameProfileRegistry.BuiltIn.Find(requestedId) != null
        ? Paths.GameInstall(requestedId)
        : Paths.GameDir;
string gameRoot = Path.GetFullPath(OptionValue("--game-root") ?? conventionalGameRoot);
string sys4Ini = InstallFileNames.CatalogPath(gameRoot);
GameCatalogIdentity catalogIdentity = GameCatalogIdentity.ReadSys4IniHeader(sys4Ini);
SelectedGameProfile selectedProfile = GameProfileSelection.Resolve(args, catalogIdentity);
if (selectedProfile.MismatchDiagnostic is { } mismatchDiagnostic)
    Console.Error.WriteLine($"warning: {mismatchDiagnostic}");
if (selectedProfile.SupportDiagnostic is { } supportDiagnostic)
    Console.Error.WriteLine($"warning: {supportDiagnostic}");
// The engine is put together here: the profile's build gives every rule, and its ABI picks the opcode table.
EngineBuild engineBuild = EngineBuilds.ForProfile(selectedProfile.Profile);
var table = OpcodeTableJson.Load(Paths.OpcodesJson, engineBuild.AbiId);
// call-script execution: resolves ids -> scripts. Product paths pass this so subroutines run;
// `trace` stays provider-less on purpose (the base-ISA offset oracle).
var catalog = Sys4AssetCatalog.Load(sys4Ini);
AssetLaunchOptions assetOptions = AssetLaunchOptions.Resolve(args, gameRoot);
var assetStore = new Sys4AssetStore(catalog, gameRoot, assetOptions.LooseRoots.ToArray());
var provider = new Sys4ScriptProvider(
    table, catalog, assetStore);
Script ScriptByName(string name) => provider.RequireByName(name);

// script-index (game data export spec §8.2) indexes the selected install's scripts.
if (command == "script-index")
    return ScriptIndexCommand.Run(args, selectedProfile.Profile.Id, catalog, assetStore, provider, table);

// data-export (game data export spec §6.1) reads the selected install through the engine and writes the export.
if (command == "data-export")
    return DataExportCommand.Run(args, selectedProfile.Profile.Id, gameRoot, catalog, assetStore, provider, table, engineBuild,
                                 assetOptions.AllowBmpAsAgf);

// Diagnostics flags (see the TraceSetup class below): --trace (text flow), --trace-steps (every op),
// --trace-ops <csv> (only these mnemonics/hex, tagged with their script), --trace-histogram (op +
// call-site execution counts, dumped after the run), --trace-file <path> (write to a file, else console).

if (command == "oms-residue")
{
    OmsInspect.LiteralResidue(
        catalog.EnumerateScripts().Select(packed => (
            packed.Asset.Name, packed.PackedId, (Func<byte[]>)(() => assetStore.ReadAll(packed.Asset)))),
        table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
        int.TryParse(OptionValue("--limit"), out int residueLimit) ? residueLimit : 12);
    return 0;
}

// The profile's OMS modules, as the linker compiles with them (modding from a release spec §5.1): <game>.globals and the
// hand-written source modules from the shipped base mod's oms/, and <game>.scripts from the mounted catalog. The
// idiomatic level lifts the source modules' library subs and templates. --no-names decompiles without any of them.
List<Ome.Script.Modules.GameModule> OmsModules()
    => args.Contains("--no-names") ? []
        : Ome.Mods.Linking.GameToolchain.Modules(Ome.Mods.Linking.GameToolchain.Shipped(selectedProfile.Profile.Id),
                                                 selectedProfile.Profile.Id, catalog);

if (command == "oms-append")
{
    // Spec §9.3: compile OMS scripts into a new S4AC append catalog for the selected profile. Each --script is one
    // record, in order; record 0 is the script native op 0x143 runs at INIT2. A script's name is its `script "..."`
    // name, conventionally `$<selector>$NAME.BIN`; record n is packed id (selector << 24) | n.
    int selector = int.TryParse(OptionValue("--selector"), out int chosen) ? chosen
        : throw new ArgumentException("--selector N (1..127, not a mounted selector) is required");
    if (catalog.AppendPacks.ContainsKey(selector))
        throw new ArgumentException($"selector {selector} is already mounted by the install");
    string outputDirectory = OptionValue("--output-dir") ?? throw new ArgumentException("--output-dir DIR is required");
    var sources = args.Select((arg, k) => (arg, k)).Where(pair => pair.arg == "--script" && pair.k + 1 < args.Length)
        .Select(pair => args[pair.k + 1]).ToList();
    if (sources.Count == 0) throw new ArgumentException("at least one --script FILE.oms is required");
    var appendModules = OmsModules();
    var appendIntrinsics = new Ome.Script.Abi.IntrinsicCatalog(table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson));
    var records = new List<Sys4AppendCatalogWriter.File>();
    // --mod-id ID: the scripts are one mod's catalog; `run NAME` with NAME a script's file stem resolves to its record
    // (mod loader spec §6.2), here at once, since the selector is given.
    string? appendModId = OptionValue("--mod-id");
    var recordNames = sources.Select(Path.GetFileNameWithoutExtension).Select(stem => stem!).ToList();
    try
    {
        foreach (string source in sources)
        {
            var warnings = new List<Ome.Script.Compile.OmsWarning>();
            var unit = Ome.Script.Syntax.Parser.ParseScript(File.ReadAllText(source), source);
            var scope = appendModId == null ? null : new Ome.Script.Compile.OmsResourceScope(appendModId, recordNames, []);
            var compiled = Ome.Script.Compile.OmsCompiler.Compile(unit, table.AbiId, table, null, appendIntrinsics,
                name => appendModules.FirstOrDefault(module => module.Name == name), warnings, scope);
            compiled = compiled.ResolveResources(reference =>
                (uint)(selector << 24) | (uint)recordNames.IndexOf(reference[(reference.IndexOf(':') + 1)..]));
            foreach (var warning in warnings) Console.Error.WriteLine(warning);
            records.Add(new Sys4AppendCatalogWriter.File(unit.Name, Ome.Script.Ir.IrEncoder.Encode(compiled)));
        }
    }
    catch (Exception error) when (error is Ome.Script.Syntax.OmsSyntaxException or Ome.Script.Compile.OmsCompileException
                                      or InvalidDataException)
    {
        Console.Error.WriteLine($"error: {error.Message}");
        return 1;
    }
    string archiveName = $"APPEND{selector:00}.ALF";
    string appendMagic = ("S4AC" + catalog.Magic[4..]).PadRight(8);
    var (catalogBytes, archiveBytes) = Sys4AppendCatalogWriter.Write(appendMagic, catalog.Title, selector, archiveName, records);
    Directory.CreateDirectory(outputDirectory);
    File.WriteAllBytes(Path.Combine(outputDirectory, $"APPEND{selector:00}.AAI"), catalogBytes);
    File.WriteAllBytes(Path.Combine(outputDirectory, archiveName), archiveBytes);
    for (int k = 0; k < records.Count; k++)
        Console.WriteLine($"  0x{(selector << 24) | k:x8} {records[k].Name} ({records[k].Bytes.Length} bytes)");
    Console.WriteLine($"oms-append: selector {selector}, {records.Count} scripts -> {outputDirectory}");
    return 0;
}

if (command == "oms-rebuild")
{
    // Open Maid Script gate 3: canonical rebuild (no sidecar) must be VM-trace-equivalent on the scenes.
    // Scenes default to every SC/SP scene, walked page by page (--halt-at-wait stops at the first prompt);
    // --boot starts them from the profile's data-table bootstrap, itself run on both catalogs.
    var rebuildScenes = OptionValue("--scene") is { } onlyScene
        ? new List<string> { onlyScene }
        : provider.ScriptNames.Where(n => Regex.IsMatch(n, @"^S[CP]\d{4}\.BIN$")).OrderBy(n => n, StringComparer.Ordinal).ToList();
    IReadOnlyList<string> bootScripts = [];
    if (args.Contains("--boot"))
    {
        if (selectedProfile.Profile.DirectSceneDiagnostics is not { } bootPolicy)
        {
            Console.Error.WriteLine($"profile '{selectedProfile.Profile.Id}' has no proven data-table boot policy");
            return 2;
        }
        bootScripts = bootPolicy.DataTableBootstrapScripts;
    }
    return OmsRebuildCheck.Run(catalog, assetStore, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
        OmsModules(), rebuildScenes, bootScripts,
        // The profile's VM facts. Each scene runs as the root from zeroed banks, so it takes its retail entry.
        new VmOptions(HaltAtWaitForInput: args.Contains("--halt-at-wait"))
        {
            ProfileId = selectedProfile.Profile.Id,
            ProbeMode = selectedProfile.ProbeMode,
        },
        int.TryParse(OptionValue("--limit"), out int rebuildLimit) ? rebuildLimit : 30,
        engineBuild,
        idiomatic: OptionValue("--level") == "idiomatic",
        // Gate 7 (spec §12): patch every script with an inert probe plan (--patch-probe [stride], default 5).
        patchProbe: !args.Contains("--patch-probe") ? 0
            : int.TryParse(OptionValue("--patch-probe"), out int probeStride) ? probeStride : 5,
        // --dump DIR writes every rebuilt (or probed) script there, for diffing against the original.
        dumpDirectory: OptionValue("--dump"));
}

if (command == "oms-temps")
{
    OmsInspect.TemporaryResidue(
        catalog.EnumerateScripts().Select(packed => (
            packed.Asset.Name, packed.PackedId, (Func<byte[]>)(() => assetStore.ReadAll(packed.Asset)))),
        table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
        int.TryParse(OptionValue("--limit"), out int tempLimit) ? tempLimit : 25, OptionValue("--match"),
        OptionValue("--dump"));
    return 0;
}

if (command == "oms-roundtrip" && OptionValue("--level") is { } sourceLevel)
{
    // Open Maid Script gate 2 at the raw level: bytes -> OMS source + sidecar -> bytes.
    Ome.Script.OmsSourceRoundTripReport source = Ome.Script.OmsSourceRoundTrip.Run(
        catalog.EnumerateScripts().Select(packed => (
            packed.Asset.Name, packed.PackedId, (Func<byte[]>)(() => assetStore.ReadAll(packed.Asset)))),
        table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson), sourceLevel, OmsModules(),
        fixedPoint: args.Contains("--fixed-point"));
    if (OptionValue("--output") is { } sourceOutput)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(sourceOutput));
        if (directory != null) Directory.CreateDirectory(directory);
        File.WriteAllText(sourceOutput, JsonSerializer.Serialize(new
        {
            profile_id = selectedProfile.Profile.Id,
            engine_abi_id = table.AbiId,
            level = source.Level,
            scripts = source.Scripts,
            identical = source.Identical,
            passed = source.Passed,
            empty_sidecars = source.EmptySidecars,
            sidecar_field_use = source.SidecarFieldUse,
            source_bytes = source.SourceBytes,
            instructions = source.Instructions,
            raw_statements = source.RawStatements,
            folded_instructions = source.FoldedInstructions,
            fold_rejections = source.FoldRejections,
            temp_overrides = source.TempOverrides,
            library_subs = source.LibrarySubs,
            library_scripts = source.LibraryScripts,
            template_lifts = source.TemplateLifts,
            lifted_instructions = source.LiftedInstructions,
            lift_failures = source.LiftFailures,
            failures = source.Failures,
        }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));
    }
    Console.WriteLine(
        $"oms-roundtrip: profile={selectedProfile.Profile.Id} abi={table.AbiId} level={source.Level}"
        + (args.Contains("--fixed-point") ? " fixed-point" : "") + " "
        + $"scripts={source.Scripts} identical={source.Identical} empty-sidecars={source.EmptySidecars} "
        + $"sidecar-fields={string.Join(",", source.SidecarFieldUse.Select(pair => $"{pair.Key}:{pair.Value}"))} "
        + $"source-bytes={source.SourceBytes} instructions={source.Instructions} raw-statements={source.RawStatements} "
        + $"folded={source.FoldedInstructions} fold-rejections={source.FoldRejections} temp-overrides={source.TempOverrides} "
        + (source.Level == Ome.Script.OmsSourceRoundTrip.IdiomaticLevel
            ? $"library-subs={source.LibrarySubs} library-scripts={source.LibraryScripts} "
              + $"template-lifts={source.TemplateLifts} "
              + $"lift-coverage={string.Join(",", (source.LiftedInstructions ?? new Dictionary<string, long>()).Select(pair => $"{pair.Key}:{pair.Value}({100.0 * pair.Value / Math.Max(1, source.Instructions):0.0}%)"))} "
              + $"lift-failures={source.LiftFailures?.Count ?? 0} " : "")
        + $"failures={source.Failures.Count}");
    foreach (Ome.Script.Ir.IrRoundTripFailure failure in source.Failures.Take(20))
        Console.Error.WriteLine($"  0x{failure.PackedId:x8} {failure.Name}: {failure.Error}");
    foreach (Ome.Script.Ir.IrRoundTripFailure failure in (source.LiftFailures ?? []).Take(20))
        Console.Error.WriteLine($"  lift abandoned 0x{failure.PackedId:x8} {failure.Name}: {failure.Error}");
    return source.Passed ? 0 : 1;
}

if (command == "oms-roundtrip")
{
    // Open Maid Script gate 1: every catalog script -> intermediate form -> bytes, compared byte for byte.
    var contracts = Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson);
    Ome.Script.Ir.IrRoundTripReport roundTrip = Ome.Script.Ir.IrRoundTrip.Run(
        catalog.EnumerateScripts().Select(packed => (
            packed.Asset.Name, packed.PackedId, (Func<byte[]>)(() => assetStore.ReadAll(packed.Asset)))),
        table, contracts);
    var roundTripReport = new
    {
        profile_id = selectedProfile.Profile.Id,
        engine_abi_id = table.AbiId,
        scripts = roundTrip.Scripts,
        identical = roundTrip.Identical,
        passed = roundTrip.Passed,
        instructions = roundTrip.Instructions,
        label_operands = roundTrip.LabelOperands,
        literal_code_targets = roundTrip.LiteralCodeTargets,
        data_ref_operands = roundTrip.DataRefOperands,
        data_items = roundTrip.DataItems,
        referenced_raw_items = roundTrip.ReferencedRawItems,
        unreferenced_raw_dwords = roundTrip.UnreferencedRawDwords,
        explicit_table_scripts = roundTrip.ExplicitTableScripts,
        failures = roundTrip.Failures,
    };
    if (OptionValue("--output") is { } roundTripOutput)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(roundTripOutput));
        if (directory != null) Directory.CreateDirectory(directory);
        File.WriteAllText(roundTripOutput, JsonSerializer.Serialize(roundTripReport, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        }));
    }
    Console.WriteLine(
        $"oms-roundtrip: profile={selectedProfile.Profile.Id} abi={table.AbiId} scripts={roundTrip.Scripts} "
        + $"identical={roundTrip.Identical} labels={roundTrip.LabelOperands} "
        + $"literal-code-targets={roundTrip.LiteralCodeTargets} data-refs={roundTrip.DataRefOperands} "
        + $"items={string.Join(",", roundTrip.DataItems.Select(pair => $"{pair.Key}:{pair.Value}"))} "
        + $"referenced-raw={roundTrip.ReferencedRawItems} unreferenced-raw-dwords={roundTrip.UnreferencedRawDwords} "
        + $"explicit-tables={roundTrip.ExplicitTableScripts} failures={roundTrip.Failures.Count}");
    foreach (Ome.Script.Ir.IrRoundTripFailure failure in roundTrip.Failures.Take(20))
        Console.Error.WriteLine($"  0x{failure.PackedId:x8} {failure.Name}: {failure.Error}");
    return roundTrip.Passed ? 0 : 1;
}

if (command == "catalog-scan")
{
    Sys4CorpusScanResult scan = Sys4CorpusScanner.Scan(catalog, provider);
    int[] unsupportedOpcodes = scan.OpcodeOccurrences
        .Where(pair => !VirtualMachine.Implements(table, pair.Key))
        .Select(pair => pair.Key)
        .ToArray();
    var unsupportedSites = unsupportedOpcodes.ToDictionary(
        opcode => opcode,
        _ => new List<(Script Script, Instruction Instruction)>());
    foreach (PackedAssetEntry packed in catalog.EnumerateScripts())
    {
        Script? script = provider.GetById(packed.PackedId);
        if (script == null) continue;
        foreach (Instruction instruction in script.Instructions)
            if (unsupportedSites.TryGetValue(instruction.Opcode, out var sites))
                sites.Add((script, instruction));
    }
    var unsupported = unsupportedOpcodes.ToDictionary(opcode => $"0x{opcode:x}", opcode =>
    {
        List<(Script Script, Instruction Instruction)> sites = unsupportedSites[opcode];
        return new
        {
            occurrences = scan.OpcodeOccurrences[opcode],
            label = table.TryGetDefinition(opcode, out OpcodeDefinition definition)
                ? definition.CanonicalLabel : null,
            script_count = sites.Select(site => site.Script.PackedId).Distinct().Count(),
            scripts = sites
                .GroupBy(site => site.Script.PackedId)
                .OrderByDescending(group => group.LongCount())
                .ThenBy(group => group.First().Script.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    packed_id = $"0x{group.Key:x8}",
                    name = group.First().Script.Name,
                    append_selector = (group.Key >> 24) & 0xff,
                    occurrences = group.LongCount(),
                    offsets = group.Select(site => $"0x{site.Instruction.Offset:x}").ToArray(),
                })
                .ToArray(),
            operand_type_signatures = sites
                .GroupBy(site => string.Join(",", site.Instruction.Args.Select(operand => operand.Type)))
                .OrderByDescending(group => group.LongCount())
                .Select(group => new
                {
                    operand_types = group.First().Instruction.Args
                        .Select(operand => operand.Type).ToArray(),
                    occurrences = group.LongCount(),
                })
                .ToArray(),
            representative_sites = sites
                .GroupBy(site => site.Script.PackedId)
                .Select(group => group.OrderBy(site => site.Instruction.Offset).First())
                .OrderBy(site => site.Script.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(site => site.Script.PackedId)
                .Take(8)
                .Select(site => new
                {
                    packed_id = $"0x{site.Script.PackedId:x8}",
                    script = site.Script.Name,
                    offset = $"0x{site.Instruction.Offset:x}",
                    operands = site.Instruction.Args.Select(operand => new
                    {
                        type = operand.Type,
                        value = operand.Value,
                    }).ToArray(),
                })
                .ToArray(),
        };
    });
    var report = new
    {
        profile_id = selectedProfile.Profile.Id,
        engine_build = engineBuild.Id,
        engine_abi_id = engineBuild.AbiId,
        resolved_abi_layers = table.ResolvedLayers,
        catalog_identity = selectedProfile.DetectedIdentity.Display,
        selection_source = selectedProfile.SourceName,
        identity_matched = selectedProfile.IdentityMatched,
        asset_overlay_roots = assetOptions.OverlayRoots,
        allow_bmp_as_agf = assetOptions.AllowBmpAsAgf,
        script_count = scan.ScriptCount,
        base_script_count = scan.BaseScriptCount,
        append_script_count = scan.AppendScriptCount,
        instruction_count = scan.InstructionCount,
        distinct_opcode_count = scan.OpcodeOccurrences.Count,
        opcode_occurrences = scan.OpcodeOccurrences.ToDictionary(
            pair => $"0x{pair.Key:x}", pair => pair.Value),
        operand_type_occurrences = scan.OperandTypeOccurrences.ToDictionary(
            pair => $"0x{pair.Key:x}",
            pair => pair.Value.Select(position => position.ToDictionary(
                type => $"0x{type.Key:x}", type => type.Value)).ToArray()),
        unsupported_runtime_opcodes = unsupported,
        failures = scan.Failures,
    };
    string? output = OptionValue("--output");
    if (output != null)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(output));
        if (directory != null) Directory.CreateDirectory(directory);
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        }));
    }
    Console.WriteLine(
        $"catalog-scan: profile={selectedProfile.Profile.Id} abi={table.AbiId} "
        + $"scripts={scan.ScriptCount} (base={scan.BaseScriptCount}, append={scan.AppendScriptCount}) "
        + $"instructions={scan.InstructionCount} opcodes={scan.OpcodeOccurrences.Count} "
        + $"unsupported={unsupported.Count} failures={scan.Failures.Count}"
        + (output == null ? "" : $" -> {output}"));
    foreach (Sys4CorpusScanFailure failure in scan.Failures.Take(20))
        Console.Error.WriteLine($"  0x{failure.PackedId:x8} {failure.Name}: {failure.Error}");
    return scan.Failures.Count == 0 ? 0 : 1;
}

if (command == "run")
{
    var script = Sys4Loader.Load(args[1], table);
    var runHost = new HeadlessWaits();
    using var trace = TraceSetup.Build(args, table);
    // Faithful by default: halt at wait-for-input (no player => stop, don't plow past prompts). --plow opts
    // into the old walk-every-page behavior (dialogue coverage). See VmOptions.HaltAtWaitForInput.
    var vm = new VirtualMachine(script, table, runHost, new VmOptions(HaltAtWaitForInput: !args.Contains("--plow")),
                                provider, trace.Sink, build: engineBuild);
    vm.Run();
    trace.Report();
    Console.WriteLine($"{Path.GetFileName(args[1])}: {vm.Steps} steps, {vm.Emitted.Count} show-text, {vm.CallScriptDispatches} call-scripts (halt: {vm.HaltReason})");
    foreach (var (off, text, scr) in vm.Emitted.Take(30)) Console.WriteLine($"  [{scr} 0x{off:x}] {text}");
    var sources = vm.Emitted.Select(e => e.Script).Distinct().ToList();
    Console.WriteLine($"source scripts ({sources.Count}): {string.Join(", ", sources)}");
    return 0;
}

if (command == "audio")
{
    // audio <SCENE.BIN> — run the scene and dump the BGM and voice starts and BGM stops in order, each resolved via
    // ResourceMap (the resolvers the audio native uses). Diagnostic only.
    var sceneName = args[1];
    var sceneKey = Path.GetFileNameWithoutExtension(sceneName).ToUpperInvariant();
    var res = new ResourceMap(
        catalog, assetStore, Console.Error.WriteLine, assetOptions.AllowBmpAsAgf);
    var trace = new AudioTrace(res);
    var skip = SkipAndAuto.WithSystemClock();
    var audio = new Audio(trace, trace, new FrameClock(), skip, new AudioMixerSettings(), settleFadesAtOnce: true);
    var vm = new VirtualMachine(ScriptByName(sceneName), table, new HeadlessWaits(), build: engineBuild,
                                skipAndAuto: skip, audio: audio);
    // optional: seed globals, e.g. `audio SC0000.BIN 0xa57=1` to set Lily's form-A flag
    foreach (var s in args.Skip(2))
    {
        var kv = s.Split('=');
        int k = kv[0].StartsWith("0x") ? Convert.ToInt32(kv[0], 16) : int.Parse(kv[0]);
        long v = kv[1].StartsWith("0x") ? Convert.ToInt64(kv[1], 16) : long.Parse(kv[1]);
        vm.SeedGlobal(k, v);
    }
    vm.Run();
    Console.WriteLine($"{sceneName}: {trace.Events.Count} audio ops (halt: {vm.HaltReason})");
    foreach (var (kind, id, resolved) in trace.Events)
        Console.WriteLine($"  {kind,-10} 0x{id:x2} -> {resolved}");
    return 0;
}

if (command == "gfx")
{
    // gfx [--boot] <SCENE.BIN> [0xADDR=VAL ...] — run the scene and dump executed texture ops with resolved
    // file + computed geometry. --boot first runs SYSTEM4's state-setup prefix (INITCONFIG/INIT2/INIT,
    // skipping the UI scripts LOGO/OP/TITLE) through a GameSession, so scene-assumed boot state — chiefly
    // INIT2's gfx handle array 0x62455.. — is present. Diagnostic only.
    bool boot = args.Contains("--boot");
    var sceneName = args.First(a => a.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase));
    var sceneKey = Path.GetFileNameWithoutExtension(sceneName).ToUpperInvariant();
    var res = new ResourceMap(
        catalog, assetStore, Console.Error.WriteLine, assetOptions.AllowBmpAsAgf);
    var waits = new GfxTraceWaits();
    var traced = new List<string>();
    var graphics = new Graphics(res, new FrameClock(), catalog.LogicalCanvas)
    {
        OpTrace = traced.Add,
        SpriteAlphaTest = engineBuild.Rules.Gfx.SpriteAlphaTest,
        GuardsFrameSurface = engineBuild.Rules.Gfx.GuardsFrameSurface,
    };
    var session = new GameSession();
    foreach (var s in args.Where(a => a.Contains('=')))
    {
        var kv = s.Split('=');
        int k = kv[0].StartsWith("0x") ? Convert.ToInt32(kv[0], 16) : int.Parse(kv[0]);
        long v = kv[1].StartsWith("0x") ? Convert.ToInt64(kv[1], 16) : long.Parse(kv[1]);
        session.Seed(k, v);
    }
    if (boot && selectedProfile.Profile.DirectSceneDiagnostics == null)
    {
        Console.Error.WriteLine($"profile '{selectedProfile.Profile.Id}' has no proven direct-scene boot policy");
        return 2;
    }
    if (boot)
        foreach (string b in selectedProfile.Profile.DirectSceneDiagnostics!.DataBootstrapScripts)
        {
            var bs = session.RunScene(ScriptByName(b), table, new HeadlessWaits(), null, provider, build: engineBuild);
            Console.WriteLine($"[boot] {b}: {bs.Steps} steps (halt: {bs.Halt})");
        }
    var target = ScriptByName(sceneName);
    // With --boot, run the target like the real engine (call-scripts on) so subroutine-driven setup runs.
    var vm = boot ? new VirtualMachine(target, table, waits, new VmOptions(MaxSteps: 20_000_000), provider,
                                       build: engineBuild, graphics: graphics)
                  : new VirtualMachine(target, table, waits, build: engineBuild, graphics: graphics);
    foreach (var kv in session.Globals) vm.SeedGlobal(kv.Key, kv.Value);
    foreach (var kv in session.GlobalStrings) vm.GlobalStrings[kv.Key] = kv.Value;
    vm.Run();
    Console.WriteLine($"{sceneName}: {traced.Count} texture ops (halt: {vm.HaltReason})");
    foreach (var line in traced) Console.WriteLine("  " + line);
    var gfxObjs = vm.Gfx.Objects.OrderBy(o => o.Slot).ToList();
    Console.WriteLine($"  gfx objects: {gfxObjs.Count} -> " +
        string.Join(", ", gfxObjs.Select(o => $"0x{o.Handle:x}=slot{o.Slot}")));
    var vis = vm.Gfx.SnapshotVisibleObjects();
    Console.WriteLine($"  visible objects ({vis.Count}, ascending-handle = z-order):");
    foreach (var v in vis)
        Console.WriteLine($"    h=0x{v.Handle:x} surf=0x{v.SurfaceResId:x} ({res.ResolveTexture(v.SurfaceResId)?.Name ?? "?"}) src=({v.SrcX},{v.SrcY} {v.W}x{v.H}) dst=({v.DstX},{v.DstY})");
    return 0;
}

if (command == "init-image")
{
    // init-image <out.json> <SCRIPT.BIN...> — run data-table scripts in order through one session and
    // record each script's global-bank delta (new or changed cells). Names match catalog records
    // exactly, so append fragments are addressed by their literal `$n$NAME.BIN` record names.
    // tools/init_image.py drives this and projects the deltas through the profile's global registry.
    if (args.Length < 3) { Console.WriteLine("usage: init-image <out.json> <SCRIPT.BIN...>"); return 1; }
    string output = args[1];
    var byName = provider.Catalog.EnumerateScripts()
        .GroupBy(entry => entry.Asset.Name, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Last().PackedId, StringComparer.OrdinalIgnoreCase);
    var session = new GameSession();
    var steps = new List<object>();
    var initOptions = new VmOptions(HaltAtWaitForInput: false);
    foreach (string name in args.Skip(2).Where(a => a.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase)))
    {
        if (!byName.TryGetValue(name, out long packedId))
        {
            Console.Error.WriteLine($"not in catalog: {name}");
            return 2;
        }
        Script script = provider.GetById(packedId)
                        ?? throw new InvalidOperationException($"not a script: {name}");
        var ints = new Dictionary<int, long>(session.Globals);
        var strings = new Dictionary<int, string>(session.GlobalStrings);
        var result = session.RunScene(script, table, new HeadlessWaits(), initOptions, provider, build: engineBuild);
        var intDelta = session.Globals
            .Where(kv => !ints.TryGetValue(kv.Key, out long old) || old != kv.Value)
            .OrderBy(kv => kv.Key)
            .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
        var stringDelta = session.GlobalStrings
            .Where(kv => !strings.TryGetValue(kv.Key, out string? old) || old != kv.Value)
            .OrderBy(kv => kv.Key)
            .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
        steps.Add(new
        {
            script = name,
            packed_id = $"0x{packedId:x}",
            halt = result.Halt,
            steps = result.Steps,
            ints = intDelta,
            strings = stringDelta,
        });
        Console.WriteLine($"  {name,-16} 0x{packedId:x8} {result.Steps,8} steps (halt: {result.Halt}) " +
                          $"{intDelta.Count,7} int / {stringDelta.Count,5} string cells");
    }
    File.WriteAllText(output, JsonSerializer.Serialize(
        new { profile = selectedProfile.Profile.Id, game_root = gameRoot, scripts = steps },
        new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
    Console.WriteLine($"[init-image] {steps.Count} scripts -> {output}");
    return 0;
}

if (command == "play")
{
    // play [--boot] [--show N] <SCENE.BIN...> [0xADDR=VAL ...] — run a sequence of scenes carrying persistent global
    // state across them (optional up-front seeds). --boot first runs the data-table *INIT scripts so scenes
    // see the real skill/item/unit/etc. state. The state substrate for cross-scene flow; headless.
    // The *INIT boot set — all run clean (halt: exit) and populate the game's data tables into globals.
    IReadOnlyList<string> bootScripts =
        selectedProfile.Profile.DirectSceneDiagnostics?.DataTableBootstrapScripts ?? [];
    bool boot = args.Contains("--boot");
    if (boot && selectedProfile.Profile.DirectSceneDiagnostics == null)
    {
        Console.Error.WriteLine($"profile '{selectedProfile.Profile.Id}' has no proven data-table boot policy");
        return 2;
    }
    var userScenes = args.Skip(1).Where(a => a.ToUpperInvariant().EndsWith(".BIN")).ToList();
    if (userScenes.Count == 0) { Console.WriteLine("usage: play [--boot] <SCENE.BIN...> [0xADDR=VAL ...]"); return 1; }
    var scenes = (boot ? bootScripts.Concat(userScenes) : userScenes).ToList();
    // --state <file>: start from a saved snapshot (e.g. a pre-booted state) instead of booting fresh.
    string? StateArg(string flag) { int i = Array.IndexOf(args, flag); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
    var loadState = StateArg("--state");
    var saveState = StateArg("--save-state");
    var session = loadState != null ? GameSession.FromJson(File.ReadAllText(loadState)) : new GameSession();
    if (loadState != null) Console.WriteLine($"[state] loaded {session.Globals.Count} globals from {loadState}");
    foreach (var s in args.Skip(1).Where(a => a.Contains('=')))
    {
        var kv = s.Split('=');
        int k = kv[0].StartsWith("0x") ? Convert.ToInt32(kv[0], 16) : int.Parse(kv[0]);
        long v = kv[1].StartsWith("0x") ? Convert.ToInt64(kv[1], 16) : long.Parse(kv[1]);
        session.Seed(k, v);
    }
    long totalLines = 0;
    using var trace = TraceSetup.Build(args, table);   // one sink across the sequence (histogram aggregates)
    var playOpts = new VmOptions(HaltAtWaitForInput: !args.Contains("--plow"));   // faithful by default
    foreach (var name in scenes)
    {
        var script = ScriptByName(name);
        var r = session.RunScene(script, table, new HeadlessWaits(), playOpts, provider, trace.Sink, build: engineBuild);
        totalLines += r.Emitted.Count;
        Console.WriteLine($"  {name,-14} {r.Emitted.Count,4} lines, {r.Steps,7} steps (halt: {r.Halt})");
        // --show N prints each scene's first N emitted lines.
        foreach (var line in r.Emitted.Take(int.TryParse(StateArg("--show"), out int shown) ? shown : 0))
            Console.WriteLine($"      [{line.Script} 0x{line.Offset:x}] {line.Text}");
    }
    trace.Report();
    Console.WriteLine($"total: {totalLines} lines across {scenes.Count} scene(s); {session.Globals.Count} globals carried");
    if (saveState != null) { File.WriteAllText(saveState, session.ToJson()); Console.WriteLine($"[state] saved -> {saveState}"); }
    return 0;
}

if (command == "sweep")
{
    // sweep [--boot] — run every SC/SP scene through GameSession (each from a fresh or booted-from-snapshot
    // baseline) and report halt distribution + line counts. Validates the VM + state substrate at scale and
    // surfaces how booted real data affects the corpus. Headless.
    var sceneRe = new Regex(@"^S[CP]\d{4}\.BIN$");
    var names = provider.ScriptNames.Where(n => sceneRe.IsMatch(n)).OrderBy(n => n, StringComparer.Ordinal).ToList();
    bool boot = args.Contains("--boot");
    // Sweep DEFAULTS to plow (walk every page) — it's the dialogue-coverage oracle. --halt-at-wait opts into
    // the faithful "stop at the first prompt" semantics (VmOptions.HaltAtWaitForInput).
    var sweepOpts = new VmOptions(HaltAtWaitForInput: args.Contains("--halt-at-wait"));
    string? baseline = null;
    if (boot)
    {
        var bootSession = new GameSession();
        DirectSceneDiagnosticPolicy? diagnosticPolicy = selectedProfile.Profile.DirectSceneDiagnostics;
        if (diagnosticPolicy == null)
        {
            Console.Error.WriteLine($"profile '{selectedProfile.Profile.Id}' has no proven data-table boot policy");
            return 2;
        }
        foreach (string s in diagnosticPolicy.DataTableBootstrapScripts)
            bootSession.RunScene(ScriptByName(s), table, new HeadlessWaits(), null, provider, build: engineBuild);
        baseline = bootSession.ToJson();
        Console.WriteLine($"[boot] baseline = {bootSession.Globals.Count} globals; running {names.Count} scenes from it.");
    }
    // Optional seeds turn sweep into a story-state explorer: run each scene with AND without the seeds
    // (from the same baseline) and report which scenes' dialogue changes — i.e. what a story flag affects.
    var seeds = new List<(int, long)>();
    foreach (var s in args.Skip(1).Where(a => a.Contains('=')))
    {
        var kv = s.Split('=');
        int k = kv[0].StartsWith("0x") ? Convert.ToInt32(kv[0], 16) : int.Parse(kv[0]);
        long v = kv[1].StartsWith("0x") ? Convert.ToInt64(kv[1], 16) : long.Parse(kv[1]);
        seeds.Add((k, v));
    }
    GameSession Fresh() => baseline != null ? GameSession.FromJson(baseline) : new GameSession();
    int RunLines(string name, bool seeded)
    {
        var session = Fresh();
        if (seeded) foreach (var (k, v) in seeds) session.Seed(k, v);
        return session.RunScene(ScriptByName(name), table, new HeadlessWaits(), sweepOpts, provider, build: engineBuild).Emitted.Count;
    }

    if (seeds.Count > 0)
    {
        var changed = new List<string>();
        foreach (var name in names)
        {
            int baseLines = RunLines(name, false), seededLines = RunLines(name, true);
            if (baseLines != seededLines) changed.Add($"{name}: {baseLines} -> {seededLines} lines ({seededLines - baseLines:+#;-#;0})");
        }
        var seedStr = string.Join(" ", seeds.Select(s => $"0x{s.Item1:x}={s.Item2}"));
        Console.WriteLine($"seed [{seedStr}]{(boot ? " (booted)" : "")}: {changed.Count}/{names.Count} scenes change dialogue");
        foreach (var c in changed) Console.WriteLine("  " + c);
        return 0;
    }

    var haltDist = new SortedDictionary<string, int>(StringComparer.Ordinal);
    long totalLines = 0; var anomalies = new List<string>();
    using var trace = TraceSetup.Build(args, table);   // one sink across the corpus (histogram aggregates)
    foreach (var name in names)
    {
        var session = Fresh();
        var r = session.RunScene(ScriptByName(name), table, new HeadlessWaits(), sweepOpts, provider, trace.Sink, build: engineBuild);
        var halt = r.Halt ?? "null";
        haltDist[halt] = haltDist.GetValueOrDefault(halt) + 1;
        totalLines += r.Emitted.Count;
        if (halt != "exit") anomalies.Add($"{name}: {r.Emitted.Count} lines, halt={halt}");
    }
    trace.Report();
    Console.WriteLine($"swept {names.Count} scenes{(boot ? " (booted)" : "")}: {totalLines} total lines");
    Console.WriteLine("halt distribution: " + string.Join(", ", haltDist.Select(kv => $"{kv.Key}={kv.Value}")));
    if (anomalies.Count > 0) { Console.WriteLine($"non-exit halts ({anomalies.Count}):"); foreach (var a in anomalies) Console.WriteLine("  " + a); }
    return 0;
}

if (command == "trace")
{
    // --trace-json <path>: single-scene per-op offset trace for the differential oracle (docs/engine-re.md).
    // Emits EVERY instruction offset the scene executes, in order (not just show-text lines), filtered to the
    // scene's own frame so call-script subroutines into other scripts are excluded — matching the Frida engine
    // tracer's per-codebase filter. --boot first runs SYSTEM4's state-setup prefix (INITCONFIG/INIT2/INIT) so
    // the scene sees real boot state, exactly like `gfx --boot`. Offsets are bytecode WORD indices, the same
    // unit the engine emits as (pc-codebase)/4. Observe-only (a step sink) — the sweep path below is untouched.
    int tji = Array.IndexOf(args, "--trace-json");
    if (tji >= 0)
    {
        if (tji + 1 >= args.Length) { Console.WriteLine("usage: trace <SCENE.BIN> [--boot] --trace-json <out.json>"); return 1; }
        var outPath = args[tji + 1];
        var sceneName = args.First(a => a.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase));
        bool boot = args.Contains("--boot");
        var target = ScriptByName(sceneName);
        // --state <file>: start from a captured scene-entry snapshot (Frida global-write log →
        // capture_global_writes.py) — the real engine's full pre-scene state, superseding the partial
        // --boot. Otherwise fresh + optional --boot.
        int si = Array.IndexOf(args, "--state");
        var session = (si >= 0 && si + 1 < args.Length)
            ? GameSession.FromJson(File.ReadAllText(args[si + 1]))
            : new GameSession();
        // optional 0xADDR=VAL seeds — inject pre-scene state by hand (e.g. 0x6c1=1). Applied on top.
        foreach (var s in args.Where(a => a.Contains('=') && a != outPath))
        {
            var kv = s.Split('=');
            int k = kv[0].StartsWith("0x") ? Convert.ToInt32(kv[0], 16) : int.Parse(kv[0]);
            long v = kv[1].StartsWith("0x") ? Convert.ToInt64(kv[1], 16) : long.Parse(kv[1]);
            session.Seed(k, v);
        }
        if (boot && si < 0 && selectedProfile.Profile.DirectSceneDiagnostics == null)
        {
            Console.Error.WriteLine($"profile '{selectedProfile.Profile.Id}' has no proven direct-scene boot policy");
            return 2;
        }
        if (boot && si < 0)   // --state already carries boot state; don't re-run the *INIT prefix
            foreach (string b in selectedProfile.Profile.DirectSceneDiagnostics!.DataBootstrapScripts)
                session.RunScene(ScriptByName(b), table, new HeadlessWaits(), null, provider, build: engineBuild);
        var sink = new JsonOffsetTraceSink(target.Name);
        var vm = new VirtualMachine(target, table, new HeadlessWaits(),
                                    new VmOptions(HaltAtWaitForInput: true, MaxSteps: 20_000_000), provider, sink, build: engineBuild);
        foreach (var kv in session.Globals) vm.SeedGlobal(kv.Key, kv.Value);
        foreach (var kv in session.GlobalStrings) vm.GlobalStrings[kv.Key] = kv.Value;
        vm.Run();
        var dir = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(outPath, JsonSerializer.Serialize(
            new { scene = Path.GetFileNameWithoutExtension(sceneName).ToUpperInvariant(), offsets = sink.Offsets },
            new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        Console.WriteLine($"{sceneName}{(boot ? " (booted)" : "")}: {sink.Offsets.Count} offsets -> {outPath} (halt: {vm.HaltReason})");
        return 0;
    }

    var scene = new Regex(@"^S[CP]\d{4}\.BIN$");
    var trace = new SortedDictionary<string, object>(StringComparer.Ordinal);
    foreach (var name in provider.ScriptNames.Where(n => scene.IsMatch(n)).OrderBy(n => n, StringComparer.Ordinal))
    {
        var vm = new VirtualMachine(ScriptByName(name), table, new HeadlessWaits(), build: engineBuild);
        vm.Run();
        trace[name] = new { offsets = vm.Emitted.Select(e => e.Offset).ToArray(), halt = vm.HaltReason, steps = vm.Steps };
    }
    File.WriteAllText(args[1], JsonSerializer.Serialize(trace,
        new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
    Console.WriteLine($"trace: {trace.Count} scenes -> {args[1]}");
    return 0;
}
Console.Error.WriteLine($"unknown command '{command}'; commands: {CommandList}"); return 2;

// Assembles the diagnostic sink from CLI flags and owns the file writer + the post-run histogram dump.
// Inert (NullTraceSink) unless a --trace* flag is present, so normal runs are untouched (parity).
sealed class TraceSetup : IDisposable
{
    public ITraceSink Sink { get; private init; } = NullTraceSink.Instance;
    private HistogramTraceSink? _hist;
    private TextWriter? _file;              // owned file writer; null => console
    private OpcodeTable? _table;

    public static TraceSetup Build(string[] a, OpcodeTable tbl)
    {
        bool text = a.Contains("--trace");
        bool steps = a.Contains("--trace-steps");
        bool hist = a.Contains("--trace-histogram");
        string? opsCsv = ArgVal(a, "--trace-ops");
        if (!text && !hist && opsCsv == null) return new TraceSetup();   // inert

        int fi = Array.IndexOf(a, "--trace-file");
        TextWriter? file = null;
        if (fi >= 0 && fi + 1 < a.Length)
        {
            var path = a[fi + 1];
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);   // robust: create parent dir
            file = new StreamWriter(path) { AutoFlush = true };
        }
        TextWriter w = file ?? Console.Out;

        HashSet<int>? filter = null;
        if (opsCsv != null)
        {
            filter = new HashSet<int>();
            foreach (var tok in opsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int? op = tok.StartsWith("0x") ? Convert.ToInt32(tok, 16) : tbl.ByLabel(tok);
                if (op is int o) filter.Add(o);
                else Console.Error.WriteLine($"--trace-ops: unknown op '{tok}' (ignored)");
            }
        }

        var sinks = new List<ITraceSink>();
        if (text || filter != null) sinks.Add(new TextTraceSink(w, tbl, steps, filter));
        HistogramTraceSink? h = null;
        if (hist) { h = new HistogramTraceSink(); sinks.Add(h); }

        return new TraceSetup
        {
            Sink = sinks.Count == 1 ? sinks[0] : new CompositeTraceSink(sinks.ToArray()),
            _hist = h, _file = file, _table = tbl,
        };
    }

    /// <summary>After the run, dump the histogram (if enabled). Text/filter output already streamed live.</summary>
    public void Report() => _hist?.WriteReport(_file ?? Console.Out, _table);

    public void Dispose() { _file?.Flush(); _file?.Dispose(); }

    private static string? ArgVal(string[] a, string key)
    { int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
}

/// <summary>The `audio` diagnostic: the audio native's lookups and BGM commands, in order. A lookup is a start the
/// native made (it looks a sound up only to start it); the BGM channel's play gives its mode, and its unload a stop or
/// a release. Sound effects are not traced.</summary>
sealed class AudioTrace(ResourceMap res) : IAudioAssets, IAudioDevice
{
    private readonly ResourceAudioAssets _assets = new(res, Console.Error.WriteLine);
    private readonly Queue<AudioChannelEnded> _ended = new();
    public List<(string Kind, long Id, string Resolved)> Events { get; } = new();

    public AudioClip? Bgm(long trackId)                 // BGM: direct name, not the manifest
    {
        var entry = res.ResolveBgm(trackId);
        Events.Add(("play-bgm", trackId, entry != null ? $"{entry.Archive} {entry.Name}" : $"BGM{trackId:D3}.OGG <missing>"));
        return _assets.Bgm(trackId);
    }

    public AudioClip? Voice(long voiceId)               // voice: universal packed catalog id
    {
        var entry = res.ResolveVoice(voiceId);
        Events.Add(("play-voice", voiceId, entry == null ? "<unresolved>" : $"{entry.Archive} {entry.Name}"));
        return _assets.Voice(voiceId);
    }

    public AudioClip? SoundEffect(long resourceId) => null;

    public void Load(int channel, AudioBus bus, AudioClip clip) { }

    public void Play(int channel, bool loop, float volumeDb, int playId)
    {
        if (channel == Audio.BgmChannel && !loop && Events.Count > 0 && Events[^1].Kind == "play-bgm")
            Events[^1] = Events[^1] with { Kind = "play-bgm-once" };
        _ended.Enqueue(new AudioChannelEnded(channel, playId));   // nothing plays: every play ends at once
    }

    public void Stop(int channel) { }

    public void Unload(int channel)
    {
        if (channel == Audio.BgmChannel) Events.Add(("stop-bgm", 0, ""));
    }

    public void SetPaused(int channel, bool paused) { }
    public void SetVolume(int channel, float volumeDb) { }
    public void SetBusVolume(AudioBus bus, float volumeDb) { }
    public void SetBusMuted(AudioBus bus, bool muted) { }
    public bool TryTakeEnded(out AudioChannelEnded ended) => _ended.TryDequeue(out ended);
}

// The gfx command's waits, which return at once: the surface ops are the graphics native's, which traces them
// (Graphics.OpTrace).
sealed class GfxTraceWaits : IInputAndWaits
{
    public void WaitForInput() { }
    public void Sleep(long duration) { }
    public void FrameYield() { }
}
