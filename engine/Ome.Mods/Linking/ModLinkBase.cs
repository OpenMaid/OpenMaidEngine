using Ome.Engine.Model;
using Ome.Engine.Modding;
using Ome.Engine.Sys4;
using Ome.Script.Abi;
using Ome.Script.Compile;
using Ome.Script.Modules;
using Ome.Script.Patch;
using Ome.Script.Syntax;
using Ome.Script.Text;

namespace Ome.Mods.Linking;

/// <summary>
/// The base a stack links against (spec §2): the selected profile, the catalog's fingerprint, the scripts the
/// catalog serves (overlays included), and the toolchain that compiles fragments against them.
/// </summary>
public sealed record ModLinkBase
{
    public required string Profile { get; init; }
    /// <summary><c>sha256:</c> of <c>SYS4INI.BIN</c>, matched against <c>[game].revisions</c>.</summary>
    public required string CatalogFingerprint { get; init; }
    /// <summary>A script's bytes as the base serves them, or null when the catalog has no such script.</summary>
    public required Func<string, byte[]?> ReadScript { get; init; }
    /// <summary>Every installed script, the append packs' included (<c>$1$CIINIT.BIN</c>), with a reader of its bytes:
    /// the link reports catalog entries that match none of their text (localization spec §9.4) and binds site entries
    /// by their fingerprints. Empty skips that report.</summary>
    public IReadOnlyList<(string Name, Func<byte[]> Read)> InstalledScripts { get; init; } = [];
    public required OpcodeTable Table { get; init; }
    public required OpcodeContracts Contracts { get; init; }
    public required IntrinsicCatalog Intrinsics { get; init; }
    /// <summary>The profile's OMS modules: <c>&lt;game&gt;.globals</c>, <c>&lt;game&gt;.scripts</c> and the source modules
    /// (<see cref="GameToolchain.Modules"/>).</summary>
    public IReadOnlyList<GameModule> Modules { get; init; } = [];
    /// <summary>The game's toolchain, from its base mod (modding from a release spec §5.1); null for a game the engine
    /// ships no base mod for, whose fragments compile with <c>&lt;game&gt;.scripts</c> only.</summary>
    public GameToolchain? Toolchain { get; init; }
    /// <summary>Why the toolchain does not load or build against this install, or null (spec §5.3). A fragment or new
    /// script that compiles reports it; a mod that compiles nothing still links.</summary>
    public string? ToolchainError { get; init; }
    public OmsIdioms? Idioms { get; init; }
    public OmsPatchPoints? PatchPoints { get; init; }
    /// <summary><c>--lua-dev</c> (Lua scripting spec §9.3, §13): an unpacked mod's write to a name its <c>[lua] writes</c>
    /// lacks goes through and is reported. A packed mod's list is enforced.</summary>
    public bool LuaDev { get; init; }
    /// <summary>The base catalog's title, which every append catalog repeats (AGE mounts only its own game's).</summary>
    public string CatalogTitle { get; init; } = "";
    /// <summary>The append magic of the base catalog family, such as <c>"S4AC422 "</c>.</summary>
    public string AppendMagic { get; init; } = "S4AC422 ";
    /// <summary>The selectors the install's own <c>*.AAI</c> catalogs use; mods never receive them.</summary>
    public IReadOnlySet<int> InstallSelectors { get; init; } = new HashSet<int>();
    /// <summary>Whether the base catalog, or an append catalog the install mounts, has a real (non-placeholder)
    /// record of that basename; an <c>assets/</c> file of such a name overrides it, any other is a new asset (spec
    /// §4.4). An append record's name keeps its <c>$n$</c> prefix, as AGE opens it from the game root.</summary>
    public Func<string, bool> HasAsset { get; init; } = _ => false;
    /// <summary>The toolchain's identity for the link cache key (spec §6.4): the compiler and linker builds and the
    /// profile's opcode table, preludes and patch points.</summary>
    public string ToolchainStamp { get; init; } = "";
    /// <summary>The Lua runtime the link checks each <c>lua/</c> file with (Lua scripting spec §14.2), created only
    /// when a mod ships Lua; null in a build without one, where a mod that ships Lua fails the link. Its identity
    /// joins the cache key of a stack with Lua.</summary>
    public LazyModRuntime? Lua { get; init; }
    /// <summary>This machine's link cache value (launcher spec §5.3; <c>MachineState.CacheValueFor</c>), which joins
    /// every cache key, so an entry another machine wrote is never mounted. Null in a link that writes no cache.</summary>
    public string? MachineValue { get; init; }
    /// <summary>A profile file's mode (<c>native</c> or <c>extended</c>, launcher spec §5.4), which joins the key of a
    /// link that dropped its failed mods; null without a profile file.</summary>
    public string? ProfileMode { get; init; }

    /// <summary>The opcode table's content (each opcode's label, operand count and semantic name), for the stamp.</summary>
    private static string TableFingerprint(OpcodeTable table)
    {
        var text = new System.Text.StringBuilder();
        foreach (OpcodeDefinition entry in table.Entries.OrderBy(entry => entry.Opcode))
            text.Append(entry.Opcode).Append(':').Append(entry.Label).Append(':').Append(entry.Argc).Append(':')
                .Append(entry.SemanticName).Append('\n');
        return OmsPatchPlan.FingerprintOf(System.Text.Encoding.UTF8.GetBytes(text.ToString()));
    }

    /// <summary>
    /// The link base every linker of an install uses: the engine, the mod commands and the launcher (launcher spec
    /// §5.9), so they link alike and share a toolchain stamp. The opcode table and contracts are the copy embedded in
    /// <c>Ome.Engine</c>, and the toolchain is the profile's shipped base mod (<see cref="GameToolchain.Shipped"/>), so a
    /// build inside a checkout links as a release does (modding from a release spec §3.2).
    /// </summary>
    public static ModLinkBase ForGame(string profile, string abiId, string sys4IniPath, Sys4AssetCatalog catalog,
                                      IAssetStore store)
        => ForInstall(profile, sys4IniPath, catalog, store, OpcodeTableJson.LoadEmbedded(abiId),
                      OpcodeContracts.LoadEmbedded(), GameToolchain.Shipped(profile));

    /// <summary>
    /// The base of an installed game: the mounted catalog and its asset store (overlay roots included), with the
    /// game's toolchain when given: its globals, its source modules (which also give the idioms that name base labels)
    /// and its patch points. A toolchain that does not load or build is kept as <see cref="ToolchainError"/>, not
    /// thrown (modding from a release spec §5.3).
    /// </summary>
    public static ModLinkBase ForInstall(string profile, string sys4IniPath, Sys4AssetCatalog catalog, IAssetStore store,
                                         OpcodeTable table, OpcodeContracts contracts, GameToolchain? toolchain = null)
    {
        var intrinsics = new IntrinsicCatalog(table, contracts);
        var stamp = new List<string>
        {
            typeof(Ome.Script.Compile.OmsCompiler).Assembly.ManifestModule.ModuleVersionId.ToString(),
            typeof(ModLinkBase).Assembly.ManifestModule.ModuleVersionId.ToString(),
            table.AbiId,
            // The table's content, not only its ABI id: opcodes_build.py can change framing or a contract under the
            // same id, and a link compiled against the old registry must not be mounted.
            TableFingerprint(table),
            contracts.Fingerprint(),
        };
        if (toolchain != null) stamp.AddRange(toolchain.Stamp);
        string? toolchainError = null;
        List<GameModule> modules;
        try
        {
            modules = GameToolchain.Modules(toolchain, profile, catalog);
        }
        catch (Exception error) when (error is OmsSyntaxException or InvalidDataException or System.Text.Json.JsonException
                                          or KeyNotFoundException or InvalidOperationException or FormatException
                                          or OverflowException)
        {
            modules = GameToolchain.Modules(null, profile, catalog);
            toolchainError = $"the OMS toolchain of {toolchain!.Location} does not load: {error.Message}";
        }
        OmsIdioms? idioms = null;
        if (toolchainError == null && modules.Any(module => module.Source != null))
        {
            try
            {
                idioms = OmsIdioms.Build(modules, table.AbiId, intrinsics);
            }
            catch (Exception error) when (error is OmsCompileException or OmsSyntaxException)
            {
                // Spec §5.3: such as an install that lacks a script a module runs. Kept, so a mod that compiles nothing
                // still links, and each compile reports it.
                toolchainError = $"the OMS modules of {toolchain!.Location} do not build against this install: {error.Message}";
            }
        }
        return new ModLinkBase
        {
            Profile = profile,
            CatalogFingerprint = OmsPatchPlan.FingerprintOf(File.ReadAllBytes(sys4IniPath)),
            ReadScript = name => catalog.ResolveName(name) is { IsPlaceholder: false } entry
                                 && entry.Name.EndsWith(".BIN", StringComparison.OrdinalIgnoreCase)
                ? store.ReadAll(entry) : null,
            InstalledScripts = catalog.EnumerateScripts().Where(packed => !packed.Asset.IsPlaceholder)
                .Select(packed => (packed.Asset.Name, (Func<byte[]>)(() => store.ReadAll(packed.Asset)))).ToList(),
            Table = table,
            Contracts = contracts,
            Intrinsics = intrinsics,
            Modules = modules,
            Idioms = idioms,
            PatchPoints = toolchain?.Points,
            Toolchain = toolchain,
            ToolchainError = toolchainError,
            CatalogTitle = catalog.Title,
            AppendMagic = ("S4AC" + catalog.Magic[4..]).PadRight(8)[..8],
            InstallSelectors = catalog.AppendPacks.Keys.ToHashSet(),
            HasAsset = name => catalog.ResolveMountedName(name) is { IsPlaceholder: false },
            ToolchainStamp = string.Join("\n", stamp),
        };
    }

    /// <summary>What the link report says of the toolchain (spec §12, gate 2).</summary>
    public string ToolchainSummary => Toolchain == null
        ? $"none: the engine ships no {Profile}-base, so fragments and new scripts compile with {Profile}.scripts only"
        : ToolchainError == null ? Toolchain.Describe() : $"{Toolchain.Describe()}; {ToolchainError}";

    /// <summary>Refuses a compile when the toolchain is broken (spec §5.3). The linker reports the error against the
    /// fragment or script that needed it.</summary>
    internal void RequireToolchain()
    {
        if (ToolchainError != null)
            throw new InvalidDataException($"{ToolchainError}; a fragment or new script cannot compile until it does");
    }
}
