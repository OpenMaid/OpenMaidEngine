using Ome.Engine.Model;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Ome.Mods.Launch;
using Ome.Mods.Linking;
using Ome.Script.Abi;
using Ome.Script.Modules;

namespace Ome.Commands;

/// <summary>
/// What differs between the two programs that run the shared commands (modding from a release spec §6.1, plan §3.2):
/// <c>Ome.Cli</c> passes its checkout's defaults, and <c>OME-Tool</c> passes <see cref="Tool"/>, which has none.
/// </summary>
public sealed record CommandDefaults
{
    /// <summary>The program's name, as its messages give it.</summary>
    public required string Program { get; init; }
    /// <summary>A game's install when the command line names none, or null when the program has no default, and asks
    /// for one.</summary>
    public Func<string, string>? DefaultInstall { get; init; }
    /// <summary>The opcode table of an ABI.</summary>
    public required Func<string, OpcodeTable> OpcodeTable { get; init; }
    /// <summary>The opcodes' operand contracts.</summary>
    public required Func<OpcodeContracts> Contracts { get; init; }
    /// <summary>Where <c>picture</c> writes for a game when <c>--output-dir</c> is not given.</summary>
    public required Func<string, string> PictureFolder { get; init; }
    /// <summary>The checkout's folder where an editor build writes a game's page maps, which <c>locate</c> reads when no
    /// profile is named (spec §7.2); null for a program with no checkout.</summary>
    public Func<string, string>? PageMapFolder { get; init; }

    /// <summary>OME-Tool's: no install unless one is named, the opcode metadata embedded in Ome.Engine, and pictures in
    /// <c>pictures/</c> of the current folder.</summary>
    public static CommandDefaults Tool { get; } = new()
    {
        Program = "OME-Tool",
        OpcodeTable = abi => OpcodeTableJson.LoadEmbedded(abi),
        Contracts = OpcodeContracts.LoadEmbedded,
        PictureFolder = _ => Path.Combine(Directory.GetCurrentDirectory(), "pictures"),
    };
}

/// <summary>A command's refusal of its command line: printed as <c>error: …</c>, exit 2 (spec §6.2).</summary>
public sealed class CommandUsageException(string message) : Exception(message);

/// <summary>
/// What a shared command reads, opened once from its command line as <c>Ome.Cli</c>'s start has always opened it: the
/// resolver's run (launcher spec §5.2: a profile file's game, install and mods), the game and its engine build, the
/// install's catalog and asset store, and the opcode metadata.
/// </summary>
public sealed class CommandInputs
{
    private OpcodeContracts? _contracts;

    private CommandInputs(string[] args, ResolvedRun run, CommandDefaults defaults, string gameRoot,
                          SelectedGameProfile selected, EngineBuild build, OpcodeTable table, Sys4AssetCatalog catalog,
                          AssetLaunchOptions assetOptions, Sys4AssetStore assetStore)
    {
        Args = args;
        Run = run;
        Defaults = defaults;
        GameRoot = gameRoot;
        Selected = selected;
        Build = build;
        Table = table;
        Catalog = catalog;
        AssetOptions = assetOptions;
        AssetStore = assetStore;
    }

    /// <summary>The command line as the resolver gave it back: one occurrence of each single-valued option, and a
    /// profile file's flags.</summary>
    public string[] Args { get; }
    public ResolvedRun Run { get; }
    public CommandDefaults Defaults { get; }
    public string GameRoot { get; }
    public string Sys4Ini => InstallFileNames.CatalogPath(GameRoot);
    public SelectedGameProfile Selected { get; }
    public string GameId => Selected.Profile.Id;
    public EngineBuild Build { get; }
    public OpcodeTable Table { get; }
    public OpcodeContracts Contracts => _contracts ??= Defaults.Contracts();
    public Sys4AssetCatalog Catalog { get; }
    public AssetLaunchOptions AssetOptions { get; }
    public Sys4AssetStore AssetStore { get; }

    /// <summary>The value after the first <paramref name="name"/>, or null.</summary>
    public string? Option(string name)
    {
        int index = Array.IndexOf(Args, name);
        return index >= 0 && index + 1 < Args.Length ? Args[index + 1] : null;
    }

    /// <summary>The value after each <paramref name="name"/>, in order.</summary>
    public List<string> Options(string name)
        => Args.Select((arg, k) => (arg, k)).Where(pair => pair.arg == name && pair.k + 1 < Args.Length)
            .Select(pair => Args[pair.k + 1]).ToList();

    public bool Has(string flag) => Args.Contains(flag);

    /// <summary>The game's OMS modules, as the linker compiles with them (spec §5.1): <c>&lt;game&gt;.globals</c> and
    /// the source modules from the shipped base mod's <c>oms/</c>, and <c>&lt;game&gt;.scripts</c> from the mounted
    /// catalog. <c>--no-names</c> gives none.</summary>
    public List<GameModule> Modules()
        => Has("--no-names") ? [] : GameToolchain.Modules(GameToolchain.Shipped(GameId), GameId, Catalog);

    /// <summary>The engine's link base for the run (launcher spec §5.9): the embedded opcode metadata, the install's
    /// catalog and the store with the profile's overlays. A toolchain that does not build is its
    /// <see cref="ModLinkBase.ToolchainError"/>, which each compile reports (modding from a release spec §5.3).</summary>
    public ModLinkBase LinkBase(Ome.Engine.Modding.LazyModRuntime lua)
        => ModLinkBase.ForGame(GameId, Build.AbiId, Sys4Ini, Catalog, AssetStore)
            with { Lua = lua, ProfileMode = Run.ProfileFile?.ModeName };

    /// <summary>Opens the inputs, printing the resolver's and the selection's warnings to <paramref name="error"/>.
    /// Throws <see cref="RunResolutionException"/> and <see cref="CommandUsageException"/> for what the command line
    /// gets wrong.</summary>
    public static CommandInputs Open(string[] args, CommandDefaults defaults, TextWriter error)
    {
        ResolvedRun run = RunResolver.Resolve(args, Directory.GetCurrentDirectory());
        args = [.. run.Arguments];
        if (run.Describe() is { } profileFileLine) error.WriteLine($"[profile-file] {profileFileLine}");
        foreach (string warning in run.Warnings) error.WriteLine($"warning: {warning}");

        string? requested = GameProfileSelection.RequestedGameId(args);
        string? named = RunResolver.LastValue(args, RunResolver.GameRootOption);
        string gameRoot;
        if (named != null) gameRoot = Path.GetFullPath(named);
        else if (defaults.DefaultInstall != null)
            // A game the registry does not know keeps Himegari's root, so the selection below reports the unknown id.
            gameRoot = Path.GetFullPath(defaults.DefaultInstall(
                requested?.ToLowerInvariant() is { } id && GameProfileRegistry.BuiltIn.Find(id) != null ? id : "himegari"));
        else
            throw new CommandUsageException(
                $"{defaults.Program} needs the game's install: --profile ID (a player profile), --data-root DIR (a "
                + "profile's folder), or --game-root DIR (an install, with --game ID)");
        string sys4Ini = InstallFileNames.CatalogPath(gameRoot);
        if (!File.Exists(sys4Ini))
            throw new CommandUsageException($"{gameRoot} holds no {InstallFileNames.CatalogName}: it is not a game's install");

        GameCatalogIdentity identity = GameCatalogIdentity.ReadSys4IniHeader(sys4Ini);
        SelectedGameProfile selected = GameProfileSelection.Resolve(args, identity);
        if (selected.MismatchDiagnostic is { } mismatch) error.WriteLine($"warning: {mismatch}");
        if (selected.SupportDiagnostic is { } support) error.WriteLine($"warning: {support}");
        // The engine is put together here: the profile's build gives every rule, and its ABI picks the opcode table.
        EngineBuild build = EngineBuilds.ForProfile(selected.Profile);
        OpcodeTable table = defaults.OpcodeTable(build.AbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        AssetLaunchOptions assetOptions = AssetLaunchOptions.Resolve(args, gameRoot);
        var assetStore = new Sys4AssetStore(catalog, gameRoot, [.. assetOptions.LooseRoots]);
        return new CommandInputs(args, run, defaults, gameRoot, selected, build, table, catalog, assetOptions, assetStore);
    }
}
