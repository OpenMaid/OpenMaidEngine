using System.Diagnostics;
using Ome.Engine.Sys4;

/// <summary><c>Ome.Cli</c> as a child process, for the tests that need the real command line (the project reference
/// builds it without referencing its assembly). <see cref="OmeToolProcess"/> runs <c>OME-Tool</c> the same way.</summary>
internal static class OmeCliProcess
{
    public static readonly TimeSpan Patience = TimeSpan.FromSeconds(60);

    /// <summary>Ome.Cli's build output beside this assembly's: the same configuration and framework.</summary>
    public static string Assembly() => BuiltAssembly("Ome.Cli", "Ome.Cli.dll");

    /// <summary>The assembly <paramref name="file"/> of the project <c>engine/<paramref name="project"/></c>, built
    /// beside this one.</summary>
    public static string BuiltAssembly(string project, string file)
    {
        string relative = Path.GetRelativePath(Path.Combine(Paths.Repo, "engine", "Ome.Engine.Tests"), AppContext.BaseDirectory);
        string path = Path.GetFullPath(Path.Combine(Paths.Repo, "engine", project, relative, file));
        Assert.True(File.Exists(path), $"{project} is not built at {path}");
        return path;
    }

    public static ProcessStartInfo StartInfo(string? workingDirectory, IEnumerable<string> arguments)
        => StartInfo(Assembly(), workingDirectory, arguments);

    public static ProcessStartInfo StartInfo(string assembly, string? workingDirectory, IEnumerable<string> arguments)
    {
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
        };
        if (workingDirectory != null) start.WorkingDirectory = workingDirectory;
        start.ArgumentList.Add(assembly);
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        return start;
    }

    /// <summary>Runs a command to its end, with no standard input.</summary>
    public static (int Exit, string Output, string Error) Run(string workingDirectory, params string[] arguments)
        => RunAssembly(Assembly(), workingDirectory, arguments);

    /// <summary>Runs <paramref name="assembly"/> with <paramref name="arguments"/> to its end, with no standard
    /// input.</summary>
    public static (int Exit, string Output, string Error) RunAssembly(string assembly, string workingDirectory,
                                                                      IEnumerable<string> arguments)
    {
        string[] list = [.. arguments];
        using Process process = Process.Start(StartInfo(assembly, workingDirectory, list))!;
        process.StandardInput.Close();
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(Patience))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"{Path.GetFileName(assembly)} {string.Join(' ', list)} did not finish");
        }
        return (process.ExitCode, output.Result, error.Result);
    }
}

/// <summary><c>OME-Tool</c> as a child process (modding from a release plan §3.2, gate 5).</summary>
internal static class OmeToolProcess
{
    public static string Assembly() => OmeCliProcess.BuiltAssembly("Ome.Tool", "OME-Tool.dll");

    public static (int Exit, string Output, string Error) Run(string workingDirectory, params string[] arguments)
        => OmeCliProcess.RunAssembly(Assembly(), workingDirectory, arguments);
}
