using Ome.Engine.Sys4;
using Ome.Mods.Linking;
using Ome.Mods.Packages;
using Ome.Script.Abi;

/// <summary>Launcher spec §5.9, the first item: the engine, <c>Ome.Cli</c>'s <c>omp</c> commands and the launcher link
/// from one base (<see cref="ModLinkBase.ForGame"/>) over the opcode metadata embedded in <c>Ome.Engine</c>. Modding from
/// a release spec §5: its toolchain is the shipped base mod's, so a checkout links as a release does, and a toolchain
/// that does not build fails only what compiles.</summary>
public sealed class SharedLinkBaseTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ome-linkbase-" + Guid.NewGuid().ToString("N"));
    private readonly List<ModPackage> _opened = [];

    public SharedLinkBaseTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        foreach (ModPackage mod in _opened) mod.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void TheEmbeddedMetadataIsTheBuiltFile()
    {
        using Stream embedded = OpcodeTableJson.OpenEmbedded();
        using var copy = new MemoryStream();
        embedded.CopyTo(copy);
        Assert.Equal(File.ReadAllBytes(Paths.OpcodesJson), copy.ToArray());
    }

    /// <summary>The scripts a profile's OMS modules run (<c>run NAME</c>), as catalog names: a synthetic install
    /// lists them so the modules' idioms build.</summary>
    internal static string[] ScriptsTheModulesRun(string profile) => SyntheticCatalog.ScriptsTheModulesRun(profile);

    [Theory]
    [InlineData("himegari")]
    [InlineData("kamidori")]
    [InlineData("arterial")]
    public void TheShippedToolchainIsTheBaseModsFiles(string profile)
    {
        // Spec §5.1: the engine embeds mods/<game>-base, so the toolchain a release links with is the checkout's.
        string folder = Path.Combine(Paths.Repo, "mods", $"{profile}-base");
        GameToolchain toolchain = GameToolchain.Shipped(profile)!;

        Assert.Equal($"{profile}-base", toolchain.Id);
        Assert.Equal(ShippedPackages.LocationOf($"{profile}-base"), toolchain.Location);
        Assert.Equal(File.ReadAllBytes(Path.Combine(folder, "oms", "globals.json")), toolchain.Globals);
        var (name, text) = Assert.Single(toolchain.Sources);
        Assert.Equal("adv.oms", name);
        Assert.Equal(File.ReadAllText(Path.Combine(folder, "oms", "adv.oms")), text);
        Assert.NotNull(toolchain.Points);
        Assert.Equal(["oms/globals.json", "oms/adv.oms", "layout/points.json"],
                     toolchain.Stamp.Select(line => line[..line.IndexOf('=')]));
    }

    [Theory]
    [InlineData("himegari", "SYS4422")]
    [InlineData("kamidori", "SYS4433")]
    [InlineData("arterial", "SYS4438")]
    public void TheSharedBaseCompilesWithTheShippedToolchain(string profile, string abiId)
    {
        // The engine linked from its embedded copy and Ome.Cli from the repository's files; both now use ForGame, which
        // stamps a link as ForInstall does over the same table and the shipped toolchain.
        string[] scripts = ScriptsTheModulesRun(profile);
        Assert.NotEmpty(scripts);
        (string sys4Ini, Sys4AssetCatalog catalog, Sys4AssetStore store) = Install(scripts);

        ModLinkBase shared = ModLinkBase.ForGame(profile, abiId, sys4Ini, catalog, store);
        ModLinkBase fromFiles = ModLinkBase.ForInstall(profile, sys4Ini, catalog, store,
            OpcodeTableJson.Load(Paths.OpcodesJson, abiId), OpcodeContracts.Load(Paths.OpcodesJson),
            GameToolchain.Shipped(profile));

        Assert.Equal(abiId, shared.Table.AbiId);
        Assert.Equal(fromFiles.ToolchainStamp, shared.ToolchainStamp);
        Assert.Null(shared.ToolchainError);
        Assert.Equal([$"{profile}.globals", $"{profile}.scripts", $"{profile}.adv"], shared.Modules.Select(module => module.Name));
        Assert.NotNull(shared.Idioms);
        Assert.NotNull(shared.PatchPoints);
        Assert.StartsWith($"{profile}-base ", shared.ToolchainSummary);
        Assert.EndsWith(": oms/globals.json, oms/adv.oms, layout/points.json", shared.ToolchainSummary);
    }

    [Fact]
    public void ABaseWithoutAToolchainCompilesWithTheScriptsAlone()
    {
        (string sys4Ini, Sys4AssetCatalog catalog, Sys4AssetStore store) = Install(["TITLE.BIN"]);
        ModLinkBase linkBase = ModLinkBase.ForInstall("himegari", sys4Ini, catalog, store,
            OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4422"), OpcodeContracts.Load(Paths.OpcodesJson));

        Assert.Null(linkBase.ToolchainError);
        Assert.Equal(["himegari.scripts"], linkBase.Modules.Select(module => module.Name));
        Assert.Equal("none: the engine ships no himegari-base, so fragments and new scripts compile with himegari.scripts "
                     + "only", linkBase.ToolchainSummary);
    }

    [Fact]
    public void AToolchainThatDoesNotBuildFailsOnlyWhatCompiles()
    {
        // Spec §12, gate 4: an install that lacks the scripts adv.oms runs. The base still loads, an asset-only mod links,
        // and a new script and a fragment each report the modules' error.
        (string sys4Ini, Sys4AssetCatalog catalog, Sys4AssetStore store) = Install(["SC0000.BIN"]);
        byte[] scene = [0x00, 0x00, 0x00, 0x00];
        File.WriteAllBytes(Path.Combine(_root, "SC0000.BIN"), scene);
        ModLinkBase linkBase = ModLinkBase.ForInstall("himegari", sys4Ini, catalog, store,
            OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4422"), OpcodeContracts.Load(Paths.OpcodesJson),
            GameToolchain.Shipped("himegari"));

        Assert.StartsWith("the OMS modules of <engine>/mods/himegari-base do not build against this install: ",
                          linkBase.ToolchainError);
        Assert.Contains("HISTORY", linkBase.ToolchainError);
        Assert.Contains("; the OMS modules of", linkBase.ToolchainSummary);

        ModLinkResult assets = ModLinker.Link(linkBase, [Mod("test.assets", "[assets]\nnames = [\"NEW.AGF\"]\n",
            new() { ["assets/NEW.AGF"] = "an image" })], []);
        Assert.True(assets.Succeeded, assets.Report.Render());
        Assert.Contains("toolchain: himegari-base ", assets.Report.Render());

        ModLinkResult script = ModLinker.Link(linkBase, [Mod("test.script", "[scripts]\nnames = [\"NEWONE\"]\n",
            new() { ["scripts/NEWONE.oms"] = "oms 1\nscript \"NEWONE\"\n\n    exit\n" })], []);
        Assert.False(script.Succeeded);
        Assert.Contains(script.Report.Lines, line => line.Class == ModReportClass.Error
            && line.Message.StartsWith("scripts/NEWONE.oms: the OMS modules of <engine>/mods/himegari-base do not build")
            && line.Message.EndsWith("; a fragment or new script cannot compile until it does"));

        ModLinkResult fragment = ModLinker.Link(linkBase, [Mod("test.fragment", "", new()
        {
            ["patches/SC0000.oms"] = $"oms 1\npatch \"SC0000.BIN\" fingerprint \"{Ome.Script.Patch.OmsPatchPlan.FingerprintOf(scene)}\"\n\n"
                                     + "before site 0x0 {\n    \"a line\"\n}\n",
        })], []);
        Assert.False(fragment.Succeeded);
        Assert.Contains(fragment.Report.Lines, line => line.Class == ModReportClass.Error && line.Script == "SC0000.BIN"
            && line.Message.StartsWith("the OMS modules of <engine>/mods/himegari-base do not build"));
    }

    private (string Sys4Ini, Sys4AssetCatalog Catalog, Sys4AssetStore Store) Install(IReadOnlyList<string> scripts)
    {
        string sys4Ini = Path.Combine(_root, "SYS4INI.BIN");
        File.WriteAllBytes(sys4Ini, Sys4StartupSettingsTests.BuildCatalog(scripts));
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        return (sys4Ini, catalog, new Sys4AssetStore(catalog, _root, _root));
    }

    private ModPackage Mod(string id, string tables, Dictionary<string, string> files)
    {
        string folder = Path.Combine(_root, "mods", id);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "manifest.toml"),
            $"omp = 1\nid = \"{id}\"\nname = \"{id}\"\nversion = \"1.0.0\"\n\n[game]\nprofile = \"himegari\"\n"
            + $"target = \"native\"\n\n{tables}");
        foreach (var (path, text) in files)
        {
            string file = Path.Combine(folder, path);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllText(file, text);
        }
        ModPackage mod = ModPackage.Open(folder);
        _opened.Add(mod);
        return mod;
    }
}
