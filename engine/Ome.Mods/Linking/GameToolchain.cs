using System.Text;
using Ome.Engine.Sys4;
using Ome.Mods.Packages;
using Ome.Script.Modules;
using Ome.Script.Patch;

namespace Ome.Mods.Linking;

/// <summary>
/// A game's OMS toolchain as its base mod ships it (modding from a release spec §5.1):
/// <list type="bullet">
/// <item><c>oms/globals.json</c>, the module <c>&lt;game&gt;.globals</c>, which <c>tools/base_mod_build.py</c> writes from
/// the game's registry;</item>
/// <item>the hand-kept source modules <c>oms/*.oms</c>, such as <c>&lt;game&gt;.adv</c>;</item>
/// <item>the patch points' copy <c>layout/points.json</c>.</item>
/// </list>
/// The linker reads the copy the engine ships (<see cref="Shipped"/>), so a checkout and a release compile alike.
/// </summary>
public sealed record GameToolchain
{
    /// <summary>The base mod's id, <c>&lt;game&gt;-base</c>.</summary>
    public required string Id { get; init; }
    /// <summary>The base mod's version.</summary>
    public required string Version { get; init; }
    /// <summary>Where its files are, as diagnostics name it: <c>&lt;engine&gt;/mods/&lt;game&gt;-base</c> for a
    /// shipped one.</summary>
    public required string Location { get; init; }
    /// <summary><c>oms/globals.json</c>, or null when the base mod has none.</summary>
    public byte[]? Globals { get; init; }
    /// <summary>The source modules, by file name in ordinal order (<c>adv.oms</c>), with their text.</summary>
    public IReadOnlyList<(string Name, string Text)> Sources { get; init; } = [];
    /// <summary>The patch points (<c>layout/points.json</c>), or null when the base mod has none.</summary>
    public OmsPatchPoints? Points { get; init; }
    /// <summary>Each file's fingerprint, for the link cache key (mod loader spec §6.4).</summary>
    public IReadOnlyList<string> Stamp { get; init; } = [];

    /// <summary>The toolchain a base mod holds, or null for a package that is no base mod.</summary>
    public static GameToolchain? Of(ModPackage package)
    {
        if (package.Layout.BaseLayout is not { } layout) return null;
        ModToolchainFiles? files = package.Layout.Toolchain;
        byte[]? globals = files?.Globals is { } globalsFile ? package.ReadAll(globalsFile) : null;
        var sources = (files?.Modules ?? []).Select(module => (module.Name, package.ReadText(module.File))).ToList();
        var stamp = new List<string>();
        if (globals != null) stamp.Add($"oms/{ModLayout.OmsGlobalsFile}={OmsPatchPlan.FingerprintOf(globals)}");
        foreach (var (name, text) in sources)
            stamp.Add($"oms/{name}={OmsPatchPlan.FingerprintOf(Encoding.UTF8.GetBytes(text))}");
        if (layout.Points is { } pointsFile)
            stamp.Add($"layout/{ModLayout.LayoutPointsFile}={OmsPatchPlan.FingerprintOf(package.ReadAll(pointsFile))}");
        return new GameToolchain
        {
            Id = package.Id,
            Version = package.Manifest.Version.ToString(),
            Location = package.Location,
            Globals = globals,
            Sources = sources,
            Points = package.BasePoints,
            Stamp = stamp,
        };
    }

    /// <summary>The toolchain of the base mod the engine ships for <paramref name="profile"/>
    /// (<see cref="ShippedPackages"/>), or null when it ships none. An installed base mod of the same id does not
    /// replace it here: the toolchain is part of the program, as the opcode table is (spec §5.2).</summary>
    public static GameToolchain? Shipped(string profile)
    {
        using ModPackage? package = ShippedPackages.Open($"{profile}-base");
        return package == null ? null : Of(package);
    }

    /// <summary>The modules a compile for this game sees, in the order the compiler searches them:
    /// <c>&lt;game&gt;.globals</c> when the toolchain has it, <c>&lt;game&gt;.scripts</c> from the catalog, then the
    /// source modules. Throws when a file of the toolchain does not parse.</summary>
    public static List<GameModule> Modules(GameToolchain? toolchain, string profile, Sys4AssetCatalog catalog)
    {
        var modules = new List<GameModule>();
        if (toolchain?.Globals is { } globals) modules.Add(GameModule.ParseGlobals(profile, globals));
        modules.Add(GameModule.Scripts(profile, catalog.EnumerateScripts().Select(packed => (packed.Asset.Name, packed.PackedId))));
        if (toolchain != null)
            modules.AddRange(GameModule.ParseSources(
                toolchain.Sources.Select(source => ($"{toolchain.Location}/oms/{source.Name}", source.Text)), profile));
        return modules;
    }

    /// <summary>The files it holds, as the link report lists them.</summary>
    public string Describe()
    {
        var files = new List<string>();
        if (Globals != null) files.Add($"oms/{ModLayout.OmsGlobalsFile}");
        files.AddRange(Sources.Select(source => $"oms/{source.Name}"));
        if (Points != null) files.Add($"layout/{ModLayout.LayoutPointsFile}");
        return $"{Id} {Version} ({Location}): " + (files.Count == 0 ? "no OMS files" : string.Join(", ", files));
    }
}
