using System.Text;
using Ome.Launcher.Core;
using Ome.Mods.Launch;

namespace Ome.Commands;

/// <summary>
/// The commands OME-Tool and Ome.Cli share (modding from a release spec §6.2), each from one command line: the command
/// is its first argument. A usage error exits 2 with its message, and so does a command line the resolver refuses.
/// </summary>
public static class ToolCommands
{
    /// <summary>The shared commands, as a usage line lists them.</summary>
    public static IReadOnlyList<string> Names { get; } = ["omp", "oms-patch", "oms-compile", "oms-decompile", "picture"];

    public static bool Handles(string command) => Names.Contains(command);

    /// <summary>Runs the command <paramref name="args"/> names, which <see cref="Handles"/> accepts: <c>--profile</c>
    /// first (<see cref="ExpandProfile"/>), then the inputs, which <c>omp text po-export</c> and <c>po-import</c> do
    /// without.</summary>
    public static int Run(string[] args, CommandDefaults defaults)
    {
        if (ExpandProfile(args) is not { } expanded) return 2;
        try
        {
            if (OmpTextCommand.IsPo(expanded)) return OmpTextCommand.RunPo(expanded);
            CommandInputs inputs = CommandInputs.Open(expanded, defaults, Console.Error);
            return expanded[0] switch
            {
                "omp" when inputs.Args.Length > 1 && inputs.Args[1] == "text" => OmpTextCommand.Run(inputs),
                "omp" => OmpCommand.Run(inputs),
                "oms-patch" => OmsPatchCommand.Run(inputs),
                "oms-compile" => OmsCompileCommand.Run(inputs),
                "oms-decompile" => OmsDecompileCommand.Run(inputs),
                "picture" => PictureCommand.Run(inputs),
                _ => throw new ArgumentException($"{expanded[0]} is not a shared command"),
            };
        }
        catch (Exception error) when (IsUsageError(error))
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 2;
        }
    }

    /// <summary>The command line with <c>--profile ID</c> as the profile's <c>--data-root</c> (<see cref="ProfileOption"/>),
    /// from OME home as the launcher beside the program finds it; or null, with the reason printed, when the profile
    /// cannot be used.</summary>
    public static string[]? ExpandProfile(string[] args)
    {
        try
        {
            return ProfileOption.Expand(args, AppContext.BaseDirectory,
                                        warning => Console.Error.WriteLine($"warning: {warning}"));
        }
        catch (Exception error) when (IsUsageError(error))
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return null;
        }
    }

    /// <summary>What a program reports as a usage error (exit 2, spec §6.2): a command line the resolver or a command
    /// refuses, a profile that cannot be used, and an <see cref="ArgumentException"/> for a missing or wrong
    /// option.</summary>
    public static bool IsUsageError(Exception error)
        => error is RunResolutionException or CommandUsageException or ProfileLookupException or ArgumentException;
}

/// <summary>
/// <c>--profile ID</c> (modding from a release spec §6.3): a player profile, by its id in OME home (<c>--home DIR</c>,
/// else the home the launcher beside the program resolves). The programs turn it into <c>--data-root ROOT</c> before the
/// resolver reads the command line, so it gives the game, the install, the overlays and the profile's mods. The engine,
/// which runs a profile by its folder alone, refuses it (<see cref="RunResolver"/>).
/// </summary>
public static class ProfileOption
{
    public const string Name = RunResolver.ProfileOption, HomeOption = "--home";

    /// <summary>The command line with <c>--profile ID</c> as <c>--data-root ROOT</c> and <c>--home DIR</c> taken out.
    /// Throws <see cref="ProfileLookupException"/> for a profile that cannot be used, and
    /// <see cref="CommandUsageException"/> for a command line that names it wrongly.</summary>
    public static string[] Expand(string[] args, string programDirectory, Action<string> warnings,
                                  LauncherEnvironment? environment = null)
    {
        string? id = null, home = null;
        var kept = new List<string>(args.Length);
        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];
            if (argument is not (Name or HomeOption))
            {
                kept.Add(argument);
                continue;
            }
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new CommandUsageException($"{argument} needs a value: {(argument == Name ? "a profile's id" : "OME home's folder")}");
            if (argument == Name) id = args[++index];
            else home = args[++index];
        }
        if (id == null) return [.. kept];
        if (RunResolver.LastValue(kept, RunResolver.DataRootOption) != null)
            throw new CommandUsageException("--profile and --data-root both name a profile: give one");

        environment ??= LauncherEnvironment.Current;
        OmeHome omeHome = OmeHome.Resolve(home, programDirectory, Directory.GetCurrentDirectory(), environment);
        ProfileEntry profile = ProfileLookup.Usable(id, omeHome, () => MachineState.Load(MachineState.DefaultFolder()),
                                                    warnings);
        return [.. kept, RunResolver.DataRootOption, profile.Root];
    }
}

/// <summary>The programs' console (spec §6.1): UTF-8 while the program runs, so Japanese text and the engine's lines
/// print whole on any console, and the console's own code page back when it ends.</summary>
public sealed class Utf8Console : IDisposable
{
    private readonly Encoding? _previous;

    private Utf8Console(Encoding? previous) => _previous = previous;

    public static Utf8Console Begin()
    {
        Encoding? previous = null;
        try
        {
            previous = Console.OutputEncoding;
            Console.OutputEncoding = new UTF8Encoding(false);
        }
        catch (IOException)
        {
            // No console to set (a redirected handle some hosts refuse it on): the writers keep their encoding.
        }
        return new Utf8Console(previous);
    }

    public void Dispose()
    {
        if (_previous == null || _previous.CodePage == Console.OutputEncoding.CodePage) return;
        try
        {
            Console.OutputEncoding = _previous;
        }
        catch (IOException)
        {
        }
    }
}
