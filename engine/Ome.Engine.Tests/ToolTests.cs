using System.Text.RegularExpressions;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Launcher.Core;
using Ome.Mods.Launch;

/// <summary>
/// Modding from a release plan §3.2 (gate 5, less its Windows part): <c>OME-Tool</c>, run as a process. Its version,
/// its usage errors (exit 2), a player profile named by <c>--profile ID</c>, and the tool outside any repository, where
/// <c>Ome.Engine.Sys4.Paths</c> cannot find one. On a synthetic Kamidori install, as gate 6 of the launcher builds it.
/// </summary>
public sealed partial class ToolTests : IDisposable
{
    private const string KamidoriRevision = "S4IC433 ", KamidoriTitle = "神採りアルケミーマイスター";
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ome-tool-" + Guid.NewGuid().ToString("N"));
    private readonly OmeHome _home;
    private readonly string _install;

    public ToolTests()
    {
        _install = Path.Combine(_root, "install");
        Directory.CreateDirectory(_install);
        File.WriteAllBytes(Path.Combine(_install, "SYS4INI.BIN"), Sys4StartupSettingsTests.BuildCatalog(
            [.. SharedLinkBaseTests.ScriptsTheModulesRun("kamidori"), "GATESIX.BIN"], KamidoriRevision, KamidoriTitle));
        _home = new OmeHome(Path.Combine(_root, "home"), OmeHomeKind.Explicit);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private (int Exit, string Output, string Error) Tool(params string[] arguments) => OmeToolProcess.Run(_root, arguments);

    /// <summary>A profile the launcher made in the test home, on the synthetic install.</summary>
    private ProfileEntry Profile(string id)
    {
        string fingerprint = Installs.Recognize(_install, GameProfileRegistry.BuiltIn).Fingerprint!;
        return ProfileStore.Create(_home, LauncherSettings.Defaults, id,
            ProfileDocument.New("Tool", "kamidori", _install, fingerprint, _home)).Profile;
    }

    [GeneratedRegex(@"^OME-Tool \S+ \(commit [0-9a-f]{12}\)$")]
    private static partial Regex VersionLine();

    [Fact]
    public void VersionNamesTheVersionAndTheCommit()
    {
        var (exit, output, error) = Tool("--version");

        Assert.True(exit == 0, error);
        Assert.Matches(VersionLine(), output.TrimEnd());
    }

    [Fact]
    public void ACommandTheToolLacksIsRefusedWithItsOwnList()
    {
        var (exit, _, error) = Tool("trace", "--game", "kamidori");

        Assert.Equal(2, exit);
        Assert.Contains("OME-Tool has no command 'trace'; its commands: omp, oms-patch, oms-compile, oms-decompile, picture",
                        error);
    }

    [Fact]
    public void WithoutAProfileOrAnInstallTheToolAsksForOne()
    {
        var (exit, _, error) = Tool("omp", "check", "--game", "kamidori");

        Assert.Equal(2, exit);
        Assert.Contains("error: OME-Tool needs the game's install: --profile ID", error);
    }

    [Fact]
    public void AMissingOptionIsAUsageError()
    {
        var (exit, _, error) = Tool("oms-compile", "--game", "kamidori", "--game-root", _install);

        Assert.Equal(2, exit);
        Assert.Contains("error: --source FILE.oms is required", error);
    }

    [Fact]
    public void AProfileByItsIdChecksWhatItsFolderDoes()
    {
        ProfileEntry profile = Profile("gate");

        var byId = Tool("omp", "check", "--profile", "gate", "--home", _home.Root);
        var byFolder = Tool("omp", "check", RunResolver.DataRootOption, profile.Root);
        var cli = OmeCliProcess.Run(_root, "omp", "check", "--profile", "gate", "--home", _home.Root);

        Assert.True(byId.Exit == 0, $"exit {byId.Exit}\n{byId.Output}\n{byId.Error}");
        Assert.Contains("toolchain: kamidori-base ", byId.Output);
        Assert.Equal(byFolder.Output, byId.Output);
        Assert.True(cli.Exit == 0, $"exit {cli.Exit}\n{cli.Output}\n{cli.Error}");
        Assert.Equal(byFolder.Output, cli.Output);
    }

    [Fact]
    public void AnIdThatNamesNoProfileSaysWhereTheToolLookedAndThatTheGameIsGame()
    {
        Profile("gate");

        var (exit, _, error) = Tool("omp", "check", "--profile", "kamidori", "--home", _home.Root);

        Assert.Equal(2, exit);
        Assert.Contains($"no player profile has the id 'kamidori' in OME home {_home.Root}; a game is named with --game ID",
                        error);
    }

    [Fact]
    public void AProfileThatNeedsReviewIsRefusedWithTheLaunchersReason()
    {
        string root = Path.Combine(_root, "received");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, ProfileFile.FileName),
            $"ome_profile = 1\nname = \"Theirs\"\ngame = \"kamidori\"\nmode = \"native\"\n\n[install]\nroot = '{_install}'\n");
        LauncherSettings settings = ProfileTrust.AddExisting(_home, LauncherSettings.Defaults, root, "theirs",
            () => MachineState.Load(Path.Combine(_root, "machine")), picked: true);
        LauncherSettingsStore.Save(_home, LauncherSettingsStore.Load(_home), settings);

        var (exit, _, error) = Tool("omp", "check", "--profile", "theirs", "--home", _home.Root);

        Assert.Equal(2, exit);
        Assert.Contains($"error: {root} is a profile root the launcher did not create", error);
    }

    [Fact]
    public void AProfileAndADataRootTogetherAreRefused()
    {
        ProfileEntry profile = Profile("gate");

        var (exit, _, error) = Tool("omp", "check", "--profile", "gate", "--home", _home.Root,
                                    RunResolver.DataRootOption, profile.Root);

        Assert.Equal(2, exit);
        Assert.Contains("error: --profile and --data-root both name a profile: give one", error);
    }

    [Fact]
    public void TheToolAndItsCommandsNeverReadTheCheckoutsPaths()
    {
        // Spec §6: Paths finds the repository above the program, which a release does not have.
        var offenders = new List<string>();
        foreach (string project in new[] { "Ome.Tool", "Ome.Commands" })
            foreach (string file in Directory.EnumerateFiles(Path.Combine(Paths.Repo, "engine", project), "*.cs",
                                                             SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(Paths.Repo, file).Replace('\\', '/');
                if (relative.Contains("/bin/") || relative.Contains("/obj/")) continue;
                string[] lines = File.ReadAllLines(file);
                for (int index = 0; index < lines.Length; index++)
                    if (PathsUse().IsMatch(lines[index])) offenders.Add($"{relative}:{index + 1}: {lines[index].Trim()}");
            }
        Assert.Empty(offenders);
    }

    [GeneratedRegex(@"\bPaths\.")]
    private static partial Regex PathsUse();

    [Fact]
    public void OutsideARepositoryTheToolChecksAnInstallWithItsShippedToolchain()
    {
        // A copy of the tool's build output where no repository is above it: Paths would throw on any use (spec §1.1).
        string built = Path.GetDirectoryName(OmeToolProcess.Assembly())!;
        string copy = Path.Combine(_root, "release");
        foreach (string file in Directory.EnumerateFiles(built, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(copy, Path.GetRelativePath(built, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        Assert.Throws<IOException>(() => FindRepositoryAbove(copy));

        var (exit, output, error) = OmeCliProcess.RunAssembly(Path.Combine(copy, "OME-Tool.dll"), _root,
            ["omp", "check", "--game", "kamidori", "--game-root", _install]);

        Assert.True(exit == 0, $"exit {exit}\n{output}\n{error}");
        Assert.Contains("toolchain: kamidori-base ", output);
        Assert.DoesNotContain("repository root not found", error);
    }

    /// <summary>The folder above <paramref name="start"/> that holds <c>vm-map/opcodes.toml</c>, one of the files Paths
    /// requires of the repository it finds.</summary>
    private static string FindRepositoryAbove(string start)
    {
        for (DirectoryInfo? folder = new(start); folder != null; folder = folder.Parent)
            if (File.Exists(Path.Combine(folder.FullName, "vm-map", "opcodes.toml"))) return folder.FullName;
        throw new IOException($"no repository above {start}");
    }
}
