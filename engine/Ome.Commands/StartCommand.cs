using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ome.Engine.Frontend.Control;
using Ome.Launcher.Core;
using Ome.Mods.Launch;

namespace Ome.Commands;

/// <summary>
/// <c>OME-Tool start</c> (modding from a release spec §8, plan §3.3): the launcher's start of a profile, shown in the
/// terminal. It runs the profile's checks with the <c>--mod</c> packages in the Link check, starts the engine beside
/// the tool with the authoring flags, prints each line of the engine's output as it comes (the profile's
/// <c>diagnostics/engine.log</c> gets them too), and returns the engine's exit code. Ctrl+C closes the engine over its
/// control channel, which runs with the player's input (<c>--control-input shared</c>), and ends it after
/// <see cref="Grace"/> or a second Ctrl+C.
/// </summary>
public static class StartCommand
{
    public const string Usage = "usage: OME-Tool start (--profile ID | --data-root DIR) [--home DIR] [--mod PATH ...] "
                                + "[--lua-dev] [--lua-trace] [--lua-reload] [--lua-errors MODE] [--text-log] "
                                + "[--locator-hud] [--engine-arg ARG ...] [-- ENGINE FLAGS]";
    public static readonly TimeSpan Grace = TimeSpan.FromSeconds(10);
    /// <summary>The authoring flags <c>start</c> names (spec §8), which pass to the engine as given.</summary>
    public static IReadOnlyList<string> Flags { get; } = ["--lua-dev", "--lua-trace", "--lua-reload", "--text-log", "--locator-hud"];
    private const string LuaErrorsOption = "--lua-errors", ControlOption = "--control";

    /// <summary>Exit codes: the engine's; 1 when a check refuses the start or the engine is missing; 2 for a command
    /// line it cannot read or a profile that cannot be used.</summary>
    public static int Run(string[] args, string programDirectory, LauncherEnvironment? environment = null)
    {
        try
        {
            return Start(args, programDirectory, environment ?? LauncherEnvironment.Current);
        }
        catch (Exception error) when (ToolCommands.IsUsageError(error) || error is LauncherUsageException)
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 2;
        }
        catch (IOException error)
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 1;
        }
    }

    private sealed record Parsed(string? Id, string? Root, string? Home, List<string> Mods, List<string> EngineArguments,
                                 List<string> RunArguments);

    private static Parsed Parse(string[] args)
    {
        var parsed = new Parsed(null, null, null, [], [], []);
        string? id = null, root = null, home = null;
        for (int index = 1; index < args.Length; index++)
        {
            string argument = args[index];
            if (argument == "--")
            {
                string[] after = args[(index + 1)..];
                if (after.Contains(ControlOption))
                    throw new CommandUsageException("start runs the engine with a control channel of its own, so "
                                                    + "--control is not an engine flag it passes on");
                parsed.RunArguments.AddRange(after);
                break;
            }
            string Value() => index + 1 < args.Length && args[index + 1] != "--"
                ? args[++index]
                : throw new CommandUsageException($"{argument} needs a value");
            switch (argument)
            {
                case ProfileOption.Name: id = Value(); break;
                case RunResolver.DataRootOption: root = Value(); break;
                case ProfileOption.HomeOption: home = Value(); break;
                case "--mod": parsed.Mods.Add(Path.GetFullPath(Value())); break;
                case "--engine-arg": parsed.EngineArguments.Add(Value()); break;
                case LuaErrorsOption: parsed.RunArguments.AddRange([argument, Value()]); break;
                default:
                    if (!Flags.Contains(argument))
                        throw new CommandUsageException(
                            $"start does not take {argument}: an engine flag goes after --, and a Godot flag is given as "
                            + $"--engine-arg {argument}. {Usage}");
                    parsed.RunArguments.Add(argument);
                    break;
            }
        }
        if ((id == null) == (root == null))
            throw new CommandUsageException($"start needs the profile: --profile ID or --data-root DIR. {Usage}");
        return parsed with { Id = id, Root = root, Home = home };
    }

    private static int Start(string[] args, string programDirectory, LauncherEnvironment environment)
    {
        Parsed parsed = Parse(args);
        string workingDirectory = Directory.GetCurrentDirectory();
        OmeHome home = OmeHome.Resolve(parsed.Home, programDirectory, workingDirectory, environment);
        MachineState Machine() => MachineState.Load(MachineState.DefaultFolder());
        ProfileEntry profile;
        if (parsed.Id != null)
            profile = ProfileLookup.Usable(parsed.Id, home, Machine, warning => Console.Error.WriteLine($"warning: {warning}"));
        else
        {
            LauncherSettingsFile settings = LauncherSettingsStore.Load(home);
            foreach (string warning in settings.Warnings) Console.Error.WriteLine($"warning: {warning}");
            profile = ProfileLookup.Checked(LauncherCommandLine.LocateProfile(parsed.Root!, home, settings.Settings, Machine,
                                                                              workingDirectory, environment.Platform));
        }

        EngineProgram engine = EngineProgram.BesideLauncher(programDirectory, environment.Platform)
            with { Arguments = parsed.EngineArguments };
        if (!File.Exists(engine.Program))
            throw new FileNotFoundException($"no engine beside OME-Tool: {engine.Program} does not exist. start runs a "
                                            + "release's engine; in a checkout, run the port with run-godot");

        string sessionFolder = Path.Combine(Path.GetTempPath(), "ome-start-" + Guid.NewGuid().ToString("N"));
        string session = Path.Combine(sessionFolder, "session.json");
        string token = DriveServer.NewToken();
        var run = new List<string>(parsed.RunArguments.Count + 2 * parsed.Mods.Count + 5);
        foreach (string mod in parsed.Mods) run.AddRange(["--mod", mod]);
        run.AddRange([ControlOption, "--control-input", "shared", "--control-file", session]);
        run.AddRange(parsed.RunArguments);
        EngineCommand command = EngineCommands.For(engine, profile, run, environment.Platform);
        command = command with
        {
            Environment = new Dictionary<string, string>(command.Environment, StringComparer.Ordinal)
            {
                [DriveProtocol.TokenEnvironmentVariable] = token,
            },
        };

        var stop = new EngineStop(session, token);
        void Interrupted(object? sender, ConsoleCancelEventArgs press)
        {
            // Before the engine runs there is nothing to close, and the tool ends as Ctrl+C ends any program.
            press.Cancel = stop.Interrupt();
        }
        Console.CancelKeyPress += Interrupted;
        LaunchResult result;
        try
        {
            result = EngineLaunch.Run(profile, command, new CheckOptions(Mods: parsed.Mods),
                started: stop.Started,
                line: Console.Out.WriteLine,
                stopped: () => stop.Reason,
                passed: checks =>
                {
                    foreach (ProfileCheck check in checks.Checks.Where(check => check.Status == CheckStatus.Warning))
                        Console.Error.WriteLine($"warning: {check.Kind}: {check.Message}");
                    Console.Error.WriteLine($"OME-Tool: starting {profile.Id} with {engine.Program}; Ctrl+C closes it");
                });
        }
        finally
        {
            Console.CancelKeyPress -= Interrupted;
            try
            {
                if (Directory.Exists(sessionFolder)) Directory.Delete(sessionFolder, recursive: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        if (!result.Started)
        {
            Console.Out.WriteLine($"{profile.Id} is not started: {result.Refusal}");
            if (result.LogPath == null) LauncherCommandLine.PrintChecks(result.Checks, Console.Out);
            return 1;
        }
        Console.Error.WriteLine($"OME-Tool: the engine exited with {result.ExitCode}"
                                + (stop.Reason is { } reason ? $" ({reason})" : "") + $"; its output is in {result.LogPath}");
        return result.ExitCode ?? 1;
    }

    /// <summary>
    /// Ctrl+C's stop (plan §2.3): the first sends <c>close</c> over the control channel, as the game's own quit does,
    /// and ends the engine if it has not exited after <see cref="Grace"/>; a second ends it at once. On Linux the
    /// interrupt also reaches the engine, which shares the terminal's process group.
    /// </summary>
    private sealed class EngineStop(string session, string token)
    {
        private readonly object _gate = new();
        private Process? _process;
        private int _presses;
        private string? _reason;

        public string? Reason { get { lock (_gate) return _reason; } }

        public void Started(Process process)
        {
            lock (_gate) _process = process;
        }

        /// <summary>Whether the press is taken (the engine runs); a press before it runs ends the tool.</summary>
        public bool Interrupt()
        {
            Process? process;
            int press;
            lock (_gate)
            {
                process = _process;
                if (process == null) return false;
                press = ++_presses;
                _reason = press == 1 ? "Ctrl+C: asked to close" : "a second Ctrl+C: ended";
            }
            if (press == 1)
            {
                Console.Error.WriteLine($"OME-Tool: closing the engine; Ctrl+C again ends it");
                _ = Task.Run(() => Close(process));
            }
            else End(process);
            return true;
        }

        private void Close(Process process)
        {
            string? failure = RequestClose();
            if (process.WaitForExit(Grace)) return;
            lock (_gate)
            {
                if (_presses > 1) return;
                _reason = $"Ctrl+C: it did not close within {Grace.TotalSeconds:0} s"
                          + (failure != null ? $" ({failure})" : "") + ", so it was ended";
            }
            End(process);
        }

        private static void End(Process process)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // It has exited.
            }
        }

        /// <summary>Sends <c>hello</c> with the token and <c>close</c>; null when the engine took the request, else
        /// why not.</summary>
        private string? RequestClose()
        {
            try
            {
                if (!File.Exists(session)) return "its control channel had not started";
                int port;
                using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(session)))
                    port = document.RootElement.GetProperty("port").GetInt32();
                using var client = new TcpClient();
                client.ReceiveTimeout = client.SendTimeout = 5000;
                client.Connect("127.0.0.1", port);
                using NetworkStream stream = client.GetStream();
                using var reader = new StreamReader(stream, new UTF8Encoding(false));
                using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { NewLine = "\n", AutoFlush = true };
                foreach (JsonObject request in new[]
                         {
                             new JsonObject { ["id"] = 1, ["op"] = "hello", ["token"] = token },
                             new JsonObject { ["id"] = 2, ["op"] = "close" },
                         })
                {
                    writer.WriteLine(request.ToJsonString());
                    string? reply = reader.ReadLine();
                    if (reply == null) return "the channel closed";
                    using JsonDocument answer = JsonDocument.Parse(reply);
                    if (!answer.RootElement.TryGetProperty("ok", out JsonElement ok) || !ok.GetBoolean())
                        return answer.RootElement.TryGetProperty("error", out JsonElement error)
                            ? error.GetString() : "the request was refused";
                }
                return null;
            }
            catch (Exception error) when (error is IOException or SocketException or JsonException
                                              or InvalidOperationException or KeyNotFoundException)
            {
                return error.Message;
            }
        }
    }
}
