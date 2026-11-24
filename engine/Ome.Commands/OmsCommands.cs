using Ome.Engine.Sys4;
using Ome.Script.Abi;
using Ome.Script.Compile;
using Ome.Script.Layout;
using Ome.Script.Modules;
using Ome.Script.Patch;
using Ome.Script.Syntax;
using Ome.Script.Text;

namespace Ome.Commands;

/// <summary><c>oms-compile --source FILE.oms [--output FILE.BIN] [--layout FILE] [--no-layout]</c>: one OMS source
/// file to a SYS4 script. A sidecar next to the source (<c>&lt;file&gt;.omslayout.json</c>) is applied unless
/// <c>--no-layout</c> asks for the canonical layout; warnings go to stderr.</summary>
internal static class OmsCompileCommand
{
    public static int Run(CommandInputs inputs)
    {
        string sourcePath = inputs.Option("--source") ?? throw new ArgumentException("--source FILE.oms is required");
        string layoutPath = inputs.Option("--layout") ?? Path.ChangeExtension(sourcePath, ".omslayout.json");
        string binaryPath = inputs.Option("--output") ?? Path.ChangeExtension(sourcePath, ".BIN");
        List<GameModule> modules = inputs.Modules();
        var warnings = new List<OmsWarning>();
        try
        {
            string sourceText = File.ReadAllText(sourcePath);
            OmsLayout? sidecar = !inputs.Has("--no-layout") && File.Exists(layoutPath)
                ? OmsLayout.FromJson(File.ReadAllText(layoutPath)) : null;
            // A sidecar records the layout of the text it was decompiled with; applied to an edited source it would put
            // the old data back (an edited line that shared a slot keeps the old text), so it is set aside.
            if (sidecar?.SourceFingerprint is { } fingerprint && fingerprint != OmsLayout.SourceFingerprintOf(sourceText))
            {
                Console.Error.WriteLine($"warning: {sourcePath} changed since {layoutPath} was written; compiling the "
                                        + "canonical layout without it (the original byte layout is not kept)");
                sidecar = null;
            }
            ScriptUnit unit = Parser.ParseScript(sourceText, sourcePath);
            var compiled = OmsCompiler.Compile(unit, inputs.Table.AbiId, inputs.Table, sidecar,
                new IntrinsicCatalog(inputs.Table, inputs.Contracts),
                name => modules.FirstOrDefault(module => module.Name == name), warnings);
            byte[] bytes = Ome.Script.Ir.IrEncoder.Encode(compiled);
            File.WriteAllBytes(binaryPath, bytes);
            foreach (OmsWarning warning in warnings) Console.Error.WriteLine(warning);
            Console.WriteLine($"oms-compile: {sourcePath} -> {binaryPath} ({bytes.Length} bytes, {compiled.Code.Count} instructions"
                              + $"{(sidecar != null ? ", with sidecar" : "")}, {warnings.Count} warnings)");
            return 0;
        }
        catch (Exception error) when (error is OmsSyntaxException or OmsCompileException or InvalidDataException
                                          or System.Text.Json.JsonException)
        {
            foreach (OmsWarning warning in warnings) Console.Error.WriteLine(warning);
            Console.Error.WriteLine($"error: {error.Message}");
            return 1;
        }
    }
}

/// <summary><c>oms-patch</c> (OMS spec §10): <c>--fingerprint SCRIPT</c> prints a script's <c>patch</c> header;
/// <c>--source FRAGMENT.oms</c> compiles fragments into transform plans against the mounted catalog's scripts, and
/// <c>--plan PLAN.omspatch.json</c> reads plans; <c>--output-dir DIR</c> applies them, in the order given, as loose
/// overrides.</summary>
internal static class OmsPatchCommand
{
    public static int Run(CommandInputs inputs)
    {
        Sys4AssetCatalog catalog = inputs.Catalog;
        Sys4AssetStore assetStore = inputs.AssetStore;
        if (inputs.Option("--fingerprint") is { } fingerprintScript)
        {
            AssetEntry fingerprintEntry = catalog.ResolveName(fingerprintScript)
                ?? throw new ArgumentException($"{fingerprintScript} is not in the mounted catalog");
            Console.WriteLine($"patch \"{fingerprintEntry.Name}\" fingerprint "
                              + $"\"{OmsPatchPlan.FingerprintOf(assetStore.ReadAll(fingerprintEntry))}\"");
            return 0;
        }
        List<string> patchSources = inputs.Options("--source");
        List<string> planFiles = inputs.Options("--plan");
        if (patchSources.Count == 0 && planFiles.Count == 0)
            throw new ArgumentException("oms-patch needs --source FRAGMENT.oms or --plan PLAN.omspatch.json (or --fingerprint SCRIPT)");
        List<GameModule> modules = inputs.Modules();
        OpcodeContracts contracts = inputs.Contracts;
        var intrinsics = new IntrinsicCatalog(inputs.Table, contracts);
        OmsIdioms? idioms = modules.Any(module => module.Source != null)
            ? OmsIdioms.Build(modules, inputs.Table.AbiId, intrinsics) : null;
        var plans = new List<(string Origin, OmsPatchPlan Plan)>();
        // --mod-id ID --names A,B,ICON.AGF: compile the fragments as part of a mod whose catalog records are those names
        // in order; references to them stay symbolic in the plans (mod loader spec §6.2). --selector N binds them for
        // --output-dir.
        string? modId = inputs.Option("--mod-id");
        List<string> names = (inputs.Option("--names") ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        OmsResourceScope? scope = modId == null ? null
            : new OmsResourceScope(modId, names.Where(name => !Path.HasExtension(name)), names.Where(Path.HasExtension));
        int? selector = int.TryParse(inputs.Option("--selector"), out int boundSelector) ? boundSelector : null;
        // The patch points the linker resolves with: the shipped base mod's layout/points.json (modding from a release
        // spec §5.1).
        OmsPatchPoints? points = Ome.Mods.Linking.GameToolchain.Shipped(inputs.GameId)?.Points;
        try
        {
            foreach (string source in patchSources)
            {
                var warnings = new List<OmsWarning>();
                var notes = new List<string>();
                ScriptUnit unit = Parser.ParseScript(File.ReadAllText(source), source);
                if (!unit.IsPatch) throw new InvalidDataException($"{source} is not a patch unit (patch \"SCRIPT.BIN\" fingerprint ...)");
                AssetEntry baseEntry = catalog.ResolveName(unit.Name)
                    ?? throw new InvalidDataException($"{source}: {unit.Name} is not in the mounted catalog");
                OmsPatchPlan plan = OmsPatchCompiler.Compile(unit, assetStore.ReadAll(baseEntry), inputs.Table, contracts,
                    intrinsics, modules, idioms, warnings, notes, points, scope);
                foreach (OmsWarning warning in warnings) Console.Error.WriteLine(warning);
                foreach (string note in notes) Console.WriteLine($"  note: {note}");
                string planPath = inputs.Option("--plan-dir") is { } planDirectory
                    ? Path.Combine(planDirectory, Path.GetFileNameWithoutExtension(source) + ".omspatch.json")
                    : Path.ChangeExtension(source, ".omspatch.json");
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(planPath))!);
                File.WriteAllText(planPath, plan.ToJson());
                Console.WriteLine($"oms-patch: {source} -> {planPath} ({plan.Operations.Count} operations, "
                                  + $"{plan.Blocks.Sum(block => block.Count)} instructions against {unit.Name})");
                plans.Add((source, plan));
            }
            foreach (string file in planFiles)
                plans.Add((file, OmsPatchPlan.FromJson(File.ReadAllText(file))));
            if (inputs.Option("--output-dir") is { } output)
            {
                Directory.CreateDirectory(output);
                foreach (var group in plans.GroupBy(entry => entry.Plan.Script, StringComparer.OrdinalIgnoreCase))
                {
                    AssetEntry baseEntry = catalog.ResolveName(group.Key)
                        ?? throw new InvalidDataException($"{group.Key} is not in the mounted catalog");
                    var bound = group.Select(entry => entry.Plan.Resources.Count == 0 ? entry.Plan
                        : entry.Plan.ResolveResources(reference =>
                        {
                            string local = reference[(reference.IndexOf(':') + 1)..];
                            int record = names.FindIndex(name => string.Equals(name, local, StringComparison.OrdinalIgnoreCase));
                            return selector is int chosen && record >= 0 && reference.StartsWith(modId + ":")
                                ? (uint)(chosen << 24) | (uint)record
                                : throw new InvalidDataException(
                                    $"{entry.Origin}: '{reference}' needs --mod-id, --names and --selector to bind it");
                        })).ToList();
                    var result = OmsPatchApplier.Apply(assetStore.ReadAll(baseEntry), baseEntry.Name, bound, inputs.Table,
                        contracts);
                    foreach (string interaction in result.Interactions) Console.WriteLine($"  interaction: {interaction}");
                    foreach (string note in result.Notes) Console.WriteLine($"  note: {note}");
                    string path = Path.Combine(output, baseEntry.Name);
                    File.WriteAllBytes(path, result.Bytes);
                    Console.WriteLine($"oms-patch: {group.Count()} plans applied to {baseEntry.Name} -> {path} "
                                      + $"({result.Script.Code.Count} instructions, {result.Bytes.Length} bytes)");
                }
            }
            return 0;
        }
        catch (Exception error) when (error is OmsSyntaxException or OmsCompileException or OmsPatchException
                                          or InvalidDataException)
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 1;
        }
    }
}

/// <summary><c>oms-decompile (--script NAME [--output FILE] | --output-dir DIR) [--level raw|core|idiomatic]
/// [--fixed-point] [--no-names]</c>: one catalog script as OMS source (and its sidecar when non-empty), or every script
/// to a folder. <c>--level idiomatic</c> lifts the source modules' library subs and templates (OMS spec §11 steps 4
/// and 5).</summary>
internal static class OmsDecompileCommand
{
    public static int Run(CommandInputs inputs)
    {
        OpcodeContracts contracts = inputs.Contracts;
        var intrinsics = new IntrinsicCatalog(inputs.Table, contracts);
        List<GameModule> modules = inputs.Modules();
        string? level = inputs.Option("--level");
        OmsIdioms? idioms = level is "idiomatic" ? OmsIdioms.Build(modules, inputs.Table.AbiId, intrinsics) : null;
        OmsDecompilation Decompile(PackedAssetEntry packed)
        {
            byte[] scriptBytes = inputs.AssetStore.ReadAll(packed.Asset);
            // --fixed-point decompiles the canonical rebuild (gate 4's compiler-generated input) instead of the original.
            if (inputs.Has("--fixed-point"))
                scriptBytes = Ome.Script.OmsSourceRoundTrip.CanonicalRebuild(scriptBytes, packed.Asset.Name, inputs.Table,
                    contracts, intrinsics, modules, idioms);
            var decoded = Ome.Script.Ir.IrDecoder.Decode(scriptBytes, inputs.Table, contracts, packed.Asset.Name);
            OmsDecompilation result = level is "raw"
                ? OmsRawDecompiler.Decompile(decoded, inputs.Table.AbiId)
                : OmsCoreDecompiler.Decompile(decoded, inputs.Table.AbiId, intrinsics, modules: modules, idioms: idioms);
            if (result.LiftFailure != null)
                Console.Error.WriteLine($"{packed.Asset.Name}: lifting abandoned, core source written: {result.LiftFailure}");
            return result;
        }
        static void Write(OmsDecompilation decompiled, string path)
        {
            File.WriteAllText(path, decompiled.Source);
            if (!decompiled.Layout.IsEmpty)
            {
                decompiled.Layout.SourceFingerprint = OmsLayout.SourceFingerprintOf(decompiled.Source);
                File.WriteAllText(Path.ChangeExtension(path, ".omslayout.json"), decompiled.Layout.ToJson());
            }
        }
        if (inputs.Option("--output-dir") is { } directory)
        {
            Directory.CreateDirectory(directory);
            int written = 0;
            foreach (PackedAssetEntry packed in inputs.Catalog.EnumerateScripts())
            {
                Write(Decompile(packed), Path.Combine(directory, Path.ChangeExtension(packed.Asset.Name, ".oms")));
                written++;
            }
            Console.WriteLine($"oms-decompile: profile={inputs.GameId} scripts={written} output={directory}");
            return 0;
        }
        string scriptName = inputs.Option("--script") ?? throw new ArgumentException("--script NAME or --output-dir DIR is required");
        PackedAssetEntry entry = inputs.Catalog.EnumerateScripts().FirstOrDefault(packed =>
            string.Equals(packed.Asset.Name, scriptName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"no catalog script named {scriptName}");
        OmsDecompilation script = Decompile(entry);
        if (inputs.Option("--output") is { } output)
            Write(script, output);
        else
        {
            Console.Write(script.Source);
            if (!script.Layout.IsEmpty) Console.WriteLine("// sidecar:\n" + script.Layout.ToJson());
        }
        return 0;
    }
}
