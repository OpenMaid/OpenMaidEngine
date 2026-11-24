using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Launcher.Core;
using Ome.Mods.Launch;

namespace Ome.Engine.Tests;

/// <summary>
/// <c>OME-Tool start</c> (modding from a release spec §8, plan §3.3): a copy of the tool's build output with the stub
/// engine beside it as <c>OME</c>, as a release has the engine, started on a profile of a synthetic Kamidori install.
/// Ctrl+C is checked by hand on Windows and in the Linux package smoke (gate 7).
/// </summary>
public sealed class StartTests : IDisposable
{
    private const string KamidoriRevision = "S4IC433 ", KamidoriTitle = "神採りアルケミーマイスター";
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ome-start-test-" + Guid.NewGuid().ToString("N"));
    private readonly OmeHome _home;
    private readonly string _install;
    private readonly ProfileEntry _profile;

    public StartTests()
    {
        _install = Path.Combine(_root, "install");
        Directory.CreateDirectory(_install);
        File.WriteAllBytes(Path.Combine(_install, "SYS4INI.BIN"), Sys4StartupSettingsTests.BuildCatalog(
            SharedLinkBaseTests.ScriptsTheModulesRun("kamidori"), KamidoriRevision, KamidoriTitle));
        _home = new OmeHome(Path.Combine(_root, "home"), OmeHomeKind.Explicit);
        string fingerprint = Installs.Recognize(_install, GameProfileRegistry.BuiltIn).Fingerprint!;
        _profile = ProfileStore.Create(_home, LauncherSettings.Defaults, "gate",
            ProfileDocument.New("Start", "kamidori", _install, fingerprint, _home)).Profile;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private string Log => Path.Combine(_profile.Root, EngineLaunch.DiagnosticsFolder, EngineLaunch.LogFile);

    /// <summary>The tool as a release has it: its build output, and the engine beside it (the stub, named OME).</summary>
    private string Release(bool withEngine = true)
    {
        string built = Path.GetDirectoryName(OmeToolProcess.Assembly())!;
        string release = Path.Combine(_root, withEngine ? "release" : "bare");
        foreach (string file in Directory.EnumerateFiles(built, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(release, Path.GetRelativePath(built, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        if (!withEngine) return release;
        string program = OmeCliProcess.BuiltAssembly("Ome.Launcher.StubEngine",
            "Ome.Launcher.StubEngine" + (OperatingSystem.IsWindows() ? ".exe" : ""));
        string stub = Path.GetDirectoryName(program)!;
        foreach (string file in Directory.EnumerateFiles(stub, "Ome.Launcher.StubEngine.*"))
            File.Copy(file, Path.Combine(release, Path.GetFileName(file)), overwrite: true);
        // The apphost finds Ome.Launcher.StubEngine.dll by the name built into it, so it runs under the engine's name.
        File.Copy(program, Path.Combine(release, OperatingSystem.IsWindows() ? "OME.exe" : "OME"));
        return release;
    }

    private (int Exit, string Output, string Error) Start(string release, params string[] arguments)
        => OmeCliProcess.RunAssembly(Path.Combine(release, "OME-Tool.dll"), _root, ["start", .. arguments]);

    [Fact]
    public void StartPrintsTheEnginesLinesAndReturnsItsExitCode()
    {
        var (exit, output, error) = Start(Release(), "--profile", "gate", "--home", _home.Root, "--lua-dev", "--text-log",
                                          "--lua-errors", "halt", "--engine-arg", "--headless",
                                          "--", "--exit-code", "3", "--note", "日本語");

        Assert.True(exit == 3, $"exit {exit}\n{output}\n{error}");
        string[] lines = output.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        // Godot's flags come before the engine's --, the profile's root after it, then start's flags and the rest.
        string[] arguments = lines.Where(line => line.StartsWith("stub out: ", StringComparison.Ordinal))
            .Select(line => line["stub out: ".Length..]).ToArray();
        Assert.Equal("--headless", arguments[0]);
        Assert.Equal(["--", "--data-root", Path.GetFullPath(_profile.Root)], arguments[1..4]);
        Assert.Equal(["--control", "--control-input", "shared", "--control-file"], arguments[4..8]);
        Assert.Equal(["--lua-dev", "--text-log", "--lua-errors", "halt", "--exit-code", "3", "--note", "日本語"], arguments[9..]);
        Assert.Contains("stub err: 日本語", lines);
        Assert.Contains("OME-Tool: starting gate with ", error);
        Assert.Contains("OME-Tool: the engine exited with 3; its output is in " + Log, error);

        // The log holds the same lines between the launcher's.
        string[] log = File.ReadAllLines(Log);
        Assert.StartsWith("[launcher] ", log[0]);
        Assert.Equal("[launcher] the engine exited with 3", log[^1]);
        Assert.Equal(lines.Order(StringComparer.Ordinal), log[1..^1].Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AProfileByItsFolderStartsAsByItsId()
    {
        var (exit, output, error) = Start(Release(), "--data-root", _profile.Root, "--home", _home.Root);

        Assert.True(exit == 0, $"exit {exit}\n{output}\n{error}");
        Assert.Contains($"stub out: {Path.GetFullPath(_profile.Root)}", output);
    }

    [Fact]
    public void AModThatFailsTheLinkCheckStopsTheStartBeforeTheEngineRuns()
    {
        string mod = Path.Combine(_root, "broken");
        Directory.CreateDirectory(mod);
        File.WriteAllText(Path.Combine(mod, "manifest.toml"),
            "omp = 1\nid = \"test.broken\"\nname = \"broken\"\nversion = \"1.0.0\"\n\n[game]\nprofile = \"kamidori\"\n"
            + "target = \"native\"\n\n[[requires]]\nid = \"test.missing\"\n");

        var (exit, output, error) = Start(Release(), "--profile", "gate", "--home", _home.Root, "--mod", mod);

        Assert.True(exit == 1, $"exit {exit}\n{output}\n{error}");
        Assert.Contains("gate is not started: a check failed: Link: ", output);
        Assert.Contains("FAILED   Link: ", output);
        Assert.Contains("test.missing", output);
        Assert.DoesNotContain("stub out:", output);
        Assert.False(File.Exists(Log), "a refused start opens no log");
    }

    [Theory]
    [InlineData("--data-root", "is refused among a run's arguments")]
    [InlineData("--control", "a control channel of its own")]
    public void StartRefusesWhatItSetsItself(string flag, string message)
    {
        var (exit, output, error) = Start(Release(), "--profile", "gate", "--home", _home.Root, "--", flag, _root);

        Assert.True(exit == 2, $"exit {exit}\n{output}\n{error}");
        Assert.Contains(message, error);
        Assert.DoesNotContain("stub out:", output);
    }

    [Theory]
    [InlineData(new[] { "--profile", "gate", "--headless" }, "start does not take --headless: an engine flag goes after --")]
    [InlineData(new[] { "--lua-dev" }, "start needs the profile: --profile ID or --data-root DIR")]
    [InlineData(new[] { "--profile", "nobody" }, "no player profile has the id 'nobody'")]
    public void AStartItCannotReadIsAUsageError(string[] arguments, string message)
    {
        var (exit, output, error) = Start(Release(), [.. arguments, "--home", _home.Root]);

        Assert.True(exit == 2, $"exit {exit}\n{output}\n{error}");
        Assert.Contains(message, error);
    }

    [Fact]
    public void StartNeedsTheEngineBesideIt()
    {
        string bare = Release(withEngine: false);

        var (exit, output, error) = Start(bare, "--profile", "gate", "--home", _home.Root);

        Assert.True(exit == 1, $"exit {exit}\n{output}\n{error}");
        Assert.Contains("no engine beside OME-Tool: " + Path.Combine(bare, OperatingSystem.IsWindows() ? "OME.exe" : "OME"), error);
    }

    [Fact]
    public void TheLaunchTellsItsCallerEachLineAndWhyItWasStopped()
    {
        string stub = OmeCliProcess.BuiltAssembly("Ome.Launcher.StubEngine",
            "Ome.Launcher.StubEngine" + (OperatingSystem.IsWindows() ? ".exe" : ""));
        var lines = new List<string>();
        ProfileCheckResult? passed = null;

        LaunchResult result = EngineLaunch.Run(_profile,
            EngineCommands.For(new EngineProgram(stub, []), _profile, ["--note", "日本語"]),
            new CheckOptions(MachineRegistry: (_, _) => null), line: lines.Add, stopped: () => "a test's stop",
            passed: checks => passed = checks);

        Assert.True(result.Started);
        Assert.True(passed!.CanPlay);
        Assert.Contains("stub out: 日本語", lines);
        Assert.Contains("stub err: 日本語", lines);
        Assert.Equal("[launcher] the engine was stopped (a test's stop); it exited with 0", File.ReadAllLines(Log)[^1]);
    }
}
