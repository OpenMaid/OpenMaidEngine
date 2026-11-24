using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ome.Commands;
using Ome.Engine.Sys4;

namespace Ome.Engine.Tests;

/// <summary>
/// Gate 6's workspace part (modding from a release spec §12, plan §3.3): on each installed game, the listing of every
/// script is the Python loader's, and each page of the checkout's recorded page maps gives <c>locate_page.py</c>'s
/// report. Both without names, which the two take from different places (<see cref="ScriptListing"/>).
/// </summary>
public sealed class LocateWorkspaceTests
{
    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "himegari")]
    public void HimegarisListingsAndPagesAreThePythonLocators() => Compare("himegari");

    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "kamidori")]
    public void KamidorisListingsAndPagesAreThePythonLocators() => Compare("kamidori");

    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "arterial")]
    public void ArterialsListingsAndPagesAreThePythonLocators() => Compare("arterial");

    private static CommandInputs Inputs(string game, params string[] arguments)
        => CommandInputs.Open(["locate", .. arguments, "--game", game, "--game-root", Paths.GameInstall(game), "--no-names"],
                              CommandDefaults.Tool, TextWriter.Null);

    private static string Python(string game, params string[] arguments)
    {
        (int exit, string output, string error) = PythonTool.Run(
            ["tools/locate_page.py", "--game", game, "--game-root", Paths.GameInstall(game), .. arguments, "--no-names"]);
        Assert.True(exit == 0, $"locate_page.py {string.Join(' ', arguments)} exited {exit}: {error}");
        return output;
    }

    private static string Sha256(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static void Compare(string game)
    {
        Assert.True(File.Exists(Path.Combine(Paths.GameInstall(game), "SYS4INI.BIN")), $"{game} is not installed");
        CommandInputs inputs = Inputs(game, "SC0000@0x0");
        var listing = new ScriptListing(inputs.Table, inputs.Contracts);
        string Listing(Engine.Model.Script script)
            => string.Join('\n', script.Instructions.Select(instruction =>
                $"0x{instruction.Offset:x5}: {listing.Line(script, instruction)}"));

        // Every script of the Python loader's corpus (extracted and loose files) that the catalog names.
        var differences = new List<string>();
        var uncatalogued = new List<string>();
        int compared = 0;
        foreach (string line in Python(game, "--digests").Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            using JsonDocument digest = JsonDocument.Parse(line);
            string name = digest.RootElement.GetProperty("script").GetString()!;
            if (inputs.Catalog.ResolveMountedName(name) is not { IsPlaceholder: false } entry)
            {
                uncatalogued.Add(name);
                continue;
            }
            byte[] bytes = inputs.AssetStore.ReadAll(entry);
            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != digest.RootElement.GetProperty("bytes").GetString())
            {
                differences.Add($"{name}: the extracted copy is not the install's (re-extract it)");
                continue;
            }
            compared++;
            string ours = Listing(Sys4Loader.Parse(bytes, inputs.Table, name));
            if (Sha256(ours) == digest.RootElement.GetProperty("listing").GetString()) continue;
            if (differences.Count >= 5) { differences.Add($"{name}: differs"); continue; }
            string[] theirs = Python(game, "--listing", name).TrimEnd('\n').Split('\n');
            string[] mine = ours.Split('\n');
            int first = Enumerable.Range(0, Math.Min(theirs.Length, mine.Length)).FirstOrDefault(k => theirs[k] != mine[k],
                                                                                                    Math.Min(theirs.Length, mine.Length));
            differences.Add($"{name}: line {first + 1}: python \"{theirs.ElementAtOrDefault(first)}\", "
                            + $"locate \"{mine.ElementAtOrDefault(first)}\"");
        }
        Assert.True(differences.Count == 0, string.Join("\n", differences));
        Assert.True(compared > 0 && uncatalogued.Count == 0,
                    $"{compared} scripts of {game} compared; not in the catalog: {string.Join(", ", uncatalogued)}");

        // Each recorded page: the same report, or a refusal from both (a map an earlier run wrote with mods, say).
        foreach (string map in Directory.Exists(Paths.GameBuild(game))
                     ? Directory.GetFiles(Paths.GameBuild(game), "page-map-*.jsonl") : [])
            foreach (string record in File.ReadLines(map).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                using JsonDocument page = JsonDocument.Parse(record);
                string scene = page.RootElement.GetProperty("root_scene").GetString()!;
                string number = page.RootElement.GetProperty("page").GetInt32().ToString();
                (int exit, string theirs, string error) = PythonTool.Run(
                    ["tools/locate_page.py", "--game", game, "--game-root", Paths.GameInstall(game), scene, number,
                     "--map", map, "--no-names"]);
                string? mine;
                try
                {
                    string report = LocateCommand.Locate(Inputs(game, scene, number, "--map", map));
                    mine = report[(report.IndexOf('\n') + 1)..];
                }
                catch (LocateException)
                {
                    mine = null;
                }
                Assert.True(exit == 0 ? mine == theirs : mine == null,
                            $"{map} {scene} P{number}: locate_page.py exited {exit} {error}\n{theirs}\nlocate:\n{mine}");
            }
    }
}
