using Ome.Mods.Linking;
using Ome.Mods.Tools;

namespace Ome.Commands;

/// <summary>
/// <c>omp build|check|export|explain|lua-stubs</c> (mod loader spec §10): <c>omp build DIR [--output FILE.omp]
/// [--mod DEP …]</c>, <c>omp check [--data-root DIR] [--mod PATH …] [--skip-failed-mods]</c>, <c>omp explain NAME</c>,
/// <c>omp export --output-dir DIR</c> and <c>omp lua-stubs --output DIR</c>. <c>--data-root</c> is a profile root (its
/// <c>mods/</c> folder, <c>load-order.txt</c> and <c>selectors.toml</c>, or its <c>profile.toml</c>'s <c>[mods]</c>
/// table, launcher spec §5.3); without it only <c>--mod</c> packages are linked. The work is <see cref="OmpCommands"/>'s.
/// </summary>
internal static class OmpCommand
{
    public static int Run(CommandInputs inputs)
    {
        string[] args = inputs.Args;
        List<string> Packages() => inputs.Options("--mod").Select(Path.GetFullPath).ToList();
        // The Lua runtime checks each lua/ file (Lua scripting spec §13); it is created only when a mod ships Lua.
        using Ome.Engine.Modding.LazyModRuntime luaRuntime = Ome.Scripting.Lua.LuaModRuntime.Lazy();
        ModLinkBase linkBase = inputs.LinkBase(luaRuntime);
        string? dataRoot = inputs.Run.DataRoot;
        Ome.Mods.Launch.ProfileMods? modsTable = inputs.Run.Mods;
        string verb = args.Length > 1 ? args[1] : "";
        return verb switch
        {
            "build" when args.Length > 2 => OmpCommands.Build(linkBase, args[2], inputs.Option("--output"), Packages(),
                Console.Out),
            "check" => OmpCommands.Check(linkBase, dataRoot, Packages(), inputs.Has("--skip-failed-mods"), Console.Out,
                modsTable),
            "export" when inputs.Option("--output-dir") is { } exportDirectory => OmpCommands.Export(linkBase, dataRoot,
                Packages(), exportDirectory, Console.Out, modsTable),
            "explain" when args.Length > 2 => OmpCommands.Explain(linkBase, inputs.Catalog, inputs.AssetStore, dataRoot,
                Packages(), args[2], Console.Out, modsTable),
            "lua-stubs" when inputs.Option("--output") is { } stubsDirectory => OmpCommands.LuaStubs(linkBase, dataRoot,
                Packages(), stubsDirectory, Console.Out, modsTable),
            _ => Usage(),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("usage: omp build DIR [--output FILE.omp] [--mod DEP] | omp check [--data-root DIR] "
                                + "[--mod PATH] [--skip-failed-mods] | omp export --output-dir DIR [--data-root DIR] [--mod PATH] | "
                                + "omp explain NAME [--data-root DIR] [--mod PATH] | "
                                + "omp lua-stubs --output DIR [--data-root DIR] [--mod PATH] | omp text …");
        return 2;
    }
}
