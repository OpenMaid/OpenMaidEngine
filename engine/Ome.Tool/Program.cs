// OME-Tool (modding from a release spec §6): the commands a mod author runs from a release, against a player profile
// (--profile ID, --data-root DIR) or an install named on the command line (--game ID --game-root DIR). Exit codes: 0
// success, 1 a failed check, 2 a usage or link error.
using Ome.Commands;
using Ome.Engine.Frontend.Shell;

using Utf8Console console = Utf8Console.Begin();
// start is the tool's alone: it runs the engine beside it, which a checkout's Ome.Cli has not (spec §8).
string commands = string.Join(", ", [.. ToolCommands.Names, "start"]);

if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
{
    TextWriter output = args.Length == 0 ? Console.Error : Console.Out;
    output.WriteLine("usage: OME-Tool COMMAND [options], or OME-Tool --version");
    output.WriteLine($"commands: {commands}");
    output.WriteLine("the game: --profile ID (a player profile in OME home, --home DIR for another), --data-root DIR (a "
                     + "profile's folder), or --game ID --game-root DIR (an install)");
    output.WriteLine("the modding guides: docs/modding/ in the source, and on the project site");
    return args.Length == 0 ? 2 : 0;
}
if (args[0] == "--version")
{
    // The stamps are this program's own (Directory.Build.targets, OmeProgram=true).
    System.Reflection.Assembly program = typeof(Program).Assembly;
    string version = EngineIdentity.DisplayVersion(EngineIdentity.Stamp(program, EngineIdentity.VersionMetadataKey));
    string commit = EngineIdentity.Stamp(program, EngineIdentity.CommitMetadataKey) ?? "unknown";
    Console.WriteLine($"OME-Tool {version} (commit {commit})");
    return 0;
}
if (args[0] == "start") return StartCommand.Run(args, AppContext.BaseDirectory);
if (!ToolCommands.Handles(args[0]))
{
    Console.Error.WriteLine($"error: OME-Tool has no command '{args[0]}'; its commands: {commands} "
                            + "(the others are Ome.Cli's, in a checkout)");
    return 2;
}
return ToolCommands.Run(args, CommandDefaults.Tool);
