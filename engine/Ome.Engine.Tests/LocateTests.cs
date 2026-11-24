using System.Diagnostics;
using System.Text;
using Ome.Commands;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Script.Abi;
using Ome.Script.Patch;

namespace Ome.Engine.Tests;

/// <summary>
/// <c>locate</c> (modding from a release spec §7, plan §3.3; gate 6's core part): on a synthetic install and page map,
/// its report is <c>tools/locate_page.py</c>'s, which these tests run on the same files.
/// </summary>
public sealed class LocateTests : IDisposable
{
    private const string Abi = "SYS4422";
    private static readonly OpcodeTable Table = OpcodeTableJson.LoadEmbedded(Abi);
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ome-locate-" + Guid.NewGuid().ToString("N"));
    private readonly string _install;
    private readonly string _map;
    private readonly byte[] _script;

    public LocateTests()
    {
        _install = Path.Combine(_root, "install");
        Directory.CreateDirectory(_install);
        File.WriteAllBytes(Path.Combine(_install, "SYS4INI.BIN"), Sys4StartupSettingsTests.BuildCatalog(CatalogFiles));
        _script = SyntheticScene(ScriptId("SC0001.BIN"));
        File.WriteAllBytes(Path.Combine(_install, "SC0000.BIN"), _script);
        _map = Path.Combine(_root, "page-map-SC0000.jsonl");
        File.WriteAllText(_map, string.Join("\n",
            """{"root_scene":"SC0001","page":3,"wait_script":"SC0001","wait_offset":"0x0"}""",
            """{"root_scene":"SC0000","page":1,"page_start_script":"SYSTEM4","page_start_offset":"0x0","wait_script":"SC0000","wait_offset":"0xb","text_script":"SC0000","text_offset":"0x3","text":"第一行","call_stack":["SYSTEM4","SC0000"]}""",
            """{"root_scene":"SC0000","page":2,"page_start_script":"SC0000","page_start_offset":"0xe","wait_script":"SC0000","wait_offset":"0x38","call_stack":[]}""",
            """{"root_scene":"SC0000","page":3,"page_start_script":"SC0000","page_start_offset":"0x3b","wait_script":"SC0000","wait_offset":"0x46","text_script":"SC0000","text_offset":"0x3e","text":"三行目\r\nnext","call_stack":["SYSTEM4","SC0000"]}""")
            + "\n", new UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    /// <summary>The toolchain's scripts, so a fragment compiles (SharedLinkBaseTests), then the scene and its callee.</summary>
    private static string[] CatalogFiles => [.. SyntheticCatalog.ScriptsTheModulesRun("himegari"), "SC0000.BIN", "SC0001.BIN"];

    private static int ScriptId(string name) => Array.IndexOf(CatalogFiles, name);

    private static Operand Imm(long value) => new(0, value);

    /// <summary>
    /// <paramref name="assembled"/> (with no op 0x71, so no T1 entries) in the compiler's layout, which
    /// <c>tools/sys4load.py</c> checks: F6 = 0x1C, and the strings inside the code section, so F8, F10 and F12 are
    /// the body's end. <see cref="ScriptAssembler"/> puts the strings after the tables instead, which the engine's
    /// loader reads alike (code ends at the first string either way).
    /// </summary>
    private static byte[] AsCompiled(byte[] assembled)
    {
        byte[] bytes = (byte[])assembled.Clone();
        int body = (bytes.Length - 0x3C) / 4;
        void Field(int index, int value) => BitConverter.GetBytes(value).CopyTo(bytes, 8 + index * 4);
        Assert.Equal(0, BitConverter.ToInt32(bytes, 8 + 7 * 4));   // F7: no T1 entries
        Field(6, 0x1C);
        Field(8, body);
        Field(10, body);
        Field(12, body);
        return bytes;
    }

    /// <summary>A scene with an operand of each form the listing names: strings, code targets (one of them none), a
    /// call-script id, typed globals and locals, a float, a type the registry does not name, and an unnamed opcode
    /// with a semantic name. Its waits are at 0xb, 0x38 and 0x46.</summary>
    private static byte[] SyntheticScene(int callee)
        => AsCompiled(ScriptAssembler.AssembleBytes(
        [
            (0x03, [Imm(callee)]),                                          // 0x00 call-script
            (0x6e, [Imm(0), new Operand(2, 0)]),                            // 0x03 show-text
            (0x6f, [Imm(0)]),                                               // 0x08 end-text-line
            (0x72, [Imm(0)]),                                               // 0x0b wait-for-input
            (0x55, [new Operand(3, 0x66713), Imm(1)]),                      // 0x0e mov
            (0x55, [new Operand(4, 0x10), new Operand(1, 0x3f800000)]),     // 0x13 mov
            (0x55, [new Operand(0x7777, 5), new Operand(0xb, 2)]),          // 0x18 mov
            (0x21, [Imm(1), Imm(2)]),                                       // 0x1d u00418860 fade-surface-in-from-black
            (0x5a, [new Operand(9, 0), new Operand(3, 0x62457), Imm(0)]),   // 0x22 eq
            (0xa0, [new Operand(9, 0), Imm(0xFFFFFFFF), Imm(0x35)]),        // 0x29 jcc
            (0x6e, [Imm(0), new Operand(2, 1)]),                            // 0x30 show-text
            (0x6f, [Imm(0)]),                                               // 0x35 end-text-line
            (0x72, [Imm(0)]),                                               // 0x38 wait-for-input
            (0x8f, [Imm(0x3)]),                                             // 0x3b call
            (0x6e, [Imm(0), new Operand(2, 2)]),                            // 0x3e show-text
            (0x6f, [Imm(0)]),                                               // 0x43 end-text-line
            (0x72, [Imm(0)]),                                               // 0x46 wait-for-input
            (0x55, [new Operand(5, 0x10), new Operand(2, 3)]),              // 0x49 mov
            (0x03, [Imm(callee)]),                                          // 0x4e call-script
        ], ["第一行", "二行目", "三行目", "文字"], Abi));

    private CommandInputs Inputs(params string[] arguments)
        => Inputs(CommandDefaults.Tool, arguments);

    private CommandInputs Inputs(CommandDefaults defaults, params string[] arguments)
        => CommandInputs.Open(["locate", .. arguments, "--game", "himegari", "--game-root", _install], defaults,
                              TextWriter.Null);

    private string Locate(params string[] arguments) => LocateCommand.Locate(Inputs(arguments));

    /// <summary>The report after the line that names the map, as <c>locate_page.py</c> prints it.</summary>
    private static string AfterMapLine(string report)
    {
        Assert.StartsWith("page map: ", report);
        return report[(report.IndexOf('\n') + 1)..];
    }

    private string Python(params string[] arguments)
    {
        (int exit, string output, string error) = PythonTool.Run(
            ["tools/locate_page.py", "--game", "himegari", "--game-root", _install,
             "--extracted-root", Path.Combine(_root, "no-extracted"), .. arguments]);
        Assert.True(exit == 0, $"locate_page.py exited {exit}: {error}");
        return output;
    }

    [Fact]
    public void TheListingIsThePythonLoadersOnASyntheticScene()
    {
        CommandInputs inputs = Inputs("--no-names");
        Engine.Model.Script script = Sys4Loader.Parse(_script, inputs.Table, "SC0000.BIN");
        var listing = new ScriptListing(inputs.Table, inputs.Contracts);
        string ours = string.Concat(script.Instructions.Select(instruction =>
            $"0x{instruction.Offset:x5}: {listing.Line(script, instruction)}\n"));

        Assert.Equal(Python("--listing", "SC0000", "--no-names"), ours);
        // Each form is in it, so the comparison covers them.
        Assert.Contains("0x00003: show-text 0x0 \"第一行\"\n", ours);
        Assert.Contains("0x00013: mov (global-float 0x10) (float 0x3f800000)\n", ours);
        Assert.Contains("0x00018: mov <t0x7777 0x5> (local-string 0x2)\n", ours);
        Assert.Contains("0x0001d: fade-surface-in-from-black 0x1 0x2   ; op 0x21 investigated\n", ours);
        Assert.Contains("0x00029: jcc (local-int 0x0) 0xffffffff label_35\n", ours);
        Assert.Contains("0x0003b: call label_3\n", ours);
    }

    [Theory]
    [InlineData("3", "8")]
    [InlineData("1", "2")]
    [InlineData("2", "30")]
    public void APageReportIsLocatePagesAfterTheLineThatNamesTheMap(string page, string context)
    {
        string ours = Locate("SC0000", page, "--map", _map, "--context", context, "--no-names");

        Assert.Equal(Python("SC0000", page, "--map", _map, "--context", context, "--no-names"), AfterMapLine(ours));
        Assert.StartsWith($"page map: {_map} (written ", ours);
    }

    [Fact]
    public void APageIsGivenAsItsNumberOrAsTheLocatorWritesIt()
    {
        string expected = Locate("SC0000", "3", "--map", _map);
        Assert.Equal(expected, Locate("SC0000", "P003", "--map", _map));
        Assert.Equal(expected, Locate("sc0000.bin", "p3", "--map", _map));
        Assert.Contains("SC0000 P003\n", expected);
        Assert.Contains("text: 三行目  next\n", expected);
        Assert.Contains(">>> 0x00046: wait-for-input 0x0\n", expected);

        foreach (string wrong in new[] { "P0", "x", "P-1" })
            Assert.Throws<CommandUsageException>(() => Locate("SC0000", wrong, "--map", _map));
        Assert.Throws<CommandUsageException>(() => Locate("SC0000", "3", "--map", _map, "--context", "-1"));
        Assert.Throws<CommandUsageException>(() => Locate("SC0000"));
    }

    [Fact]
    public void ASiteNeedsNoMap()
    {
        string site = Locate("SC0000@0x38", "--context", "1", "--no-names");

        Assert.Equal("disassembly around SC0000@0x38:\n"
                     + "    0x00035: end-text-line 0x0\n"
                     + ">>> 0x00038: wait-for-input 0x0\n"
                     + "    0x0003b: call label_3\n", site);
        // The locator's page report ends with the same listing.
        Assert.EndsWith(site, Python("SC0000", "2", "--map", _map, "--context", "1", "--no-names"));
        Assert.Equal(site, Locate("sc0000.bin@56", "--context", "1", "--no-names"));
        Assert.Contains("0x37 is not an instruction boundary of SC0000.BIN",
                        Assert.Throws<LocateException>(() => Locate("SC0000@0x37")).Message);
        Assert.Contains("SC0002.BIN is neither in the install's catalog",
                        Assert.Throws<LocateException>(() => Locate("SC0002@0x0")).Message);
    }

    [Fact]
    public void AMissingPageOrMapSaysWhereItLooked()
    {
        Assert.Contains($"SC0000 page 9 is not in {_map}",
                        Assert.Throws<LocateException>(() => Locate("SC0000", "9", "--map", _map)).Message);

        // A profile's maps are in its diagnostics folder, and no other place is looked at.
        string profile = Path.Combine(_root, "profile");
        Directory.CreateDirectory(profile);
        string expected = Path.Combine(profile, "diagnostics", "page-maps", "page-map-SC0000.jsonl");
        string message = Assert.Throws<LocateException>(() => Locate("SC0000", "3", "--data-root", profile)).Message;
        Assert.Contains($"looked for {expected}. ", message);

        Directory.CreateDirectory(Path.GetDirectoryName(expected)!);
        File.Copy(_map, expected);
        Assert.StartsWith($"page map: {expected} ", Locate("SC0000", "3", "--data-root", profile));
    }

    [Fact]
    public void WithoutAProfileTheCheckoutsMapsComeBeforeTheBareEngines()
    {
        string checkout = Path.Combine(_root, "checkout");
        CommandInputs inputs = Inputs(CommandDefaults.Tool with { PageMapFolder = _ => checkout }, "SC0000", "3");
        string user = Path.Combine(_root, "godot-user");

        Assert.Equal([Path.Combine(checkout, "page-map-SC0000.jsonl"),
                      Path.Combine(user, "games", inputs.Selected.Profile.Persistence.StorageNamespace, "diagnostics",
                                   "page-maps", "page-map-SC0000.jsonl")],
                     LocateCommand.MapCandidates(inputs, "SC0000", user));
        Assert.Single(LocateCommand.MapCandidates(Inputs("SC0000", "3"), "SC0000", user));
    }

    [Fact]
    public void TheNamesAreTheCatalogsAndTheBaseModsLayouts()
    {
        string listing = Locate("SC0000@0x22", "--context", "12");

        Assert.Contains("    0x00000: call-script 0x" + ScriptId("SC0001.BIN").ToString("x") + " =SC0001.BIN\n", listing);
        Assert.Contains("    0x0000e: mov (global-int 0x66713 =acting_entity_index) 0x1\n", listing);
        Assert.Contains(">>> 0x00022: eq (local-int 0x0) (global-int 0x62457 =adv_gfx_object_handles[2]) 0x0\n", listing);
        Assert.Contains("    0x00013: mov (global-float 0x10) (float 0x3f800000)\n", listing);
    }

    [Fact]
    public void TheMnemonicRulesAreTheLoaders()
    {
        // sys4load.display_label and render_listing: an unnamed label (u00…, dev_ukn…, the opcode in hex) shows the
        // semantic name; the suffix marks u00…, dev_ukn… and a label that starts with a digit.
        var table = new OpcodeTable(new Dictionary<int, (string Label, int Argc)>
        {
            [0x40] = ("u0040", 1), [0x50] = ("dev_ukn_50", 0), [0x39b] = ("39B", 0), [0xa0b] = ("a0b", 0),
            [0x51] = ("plain", 0),
        });
        OpcodeContracts contracts = OpcodeContracts.FromParts(new Dictionary<int, int[]>(),
            new Dictionary<(int, int), string[]>(),
            new Dictionary<int, string> { [0x50] = "sem", [0x39b] = "nine", [0xa0b] = "named", [0x51] = "other" });
        Engine.Model.Script script = ScriptAssembler.Assemble(table, "T.BIN",
            [(0x40, [Imm(1)]), (0x50, []), (0x39b, []), (0xa0b, []), (0x51, [])], []);
        var listing = new ScriptListing(table, contracts);

        Assert.Equal(["u0040 0x1   ; op 0x40", "sem   ; op 0x50 investigated", "nine   ; op 0x39b investigated", "named",
                      "plain"],
                     script.Instructions.Select(instruction => listing.Line(script, instruction)));
    }

    private string Mod(string id, IReadOnlyDictionary<string, byte[]> files)
    {
        string folder = Path.Combine(_root, id);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "manifest.toml"),
            $"omp = 1\nid = \"{id}\"\nname = \"{id}\"\nversion = \"1.0.0\"\n\n[game]\nprofile = \"himegari\"\ntarget = \"native\"\n");
        foreach (var (path, bytes) in files)
        {
            string file = Path.Combine(folder, path);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllBytes(file, bytes);
        }
        return folder;
    }

    [Fact]
    public void AScriptAModPatchesIsListedFromTheLinkWithTheBaseOffsets()
    {
        // The patcher decompiles its base, and the synthetic scene's operand of a type the registry does not name has
        // no source form: this scene has only operands a script can say.
        byte[] scene = AsCompiled(ScriptAssembler.AssembleBytes(
        [
            (0x6e, [Imm(0), new Operand(2, 0)]),                            // 0x00 show-text
            (0x6f, [Imm(0)]),                                               // 0x05 end-text-line
            (0x72, [Imm(0)]),                                               // 0x08 wait-for-input
            (0x55, [new Operand(3, 0x66713), Imm(1)]),                      // 0x0b mov
            (0x55, [new Operand(3, 0x10), Imm(2)]),                         // 0x10 mov
            (0x72, [Imm(0)]),                                               // 0x15 wait-for-input
        ], ["一"], Abi));
        File.WriteAllBytes(Path.Combine(_install, "SC0000.BIN"), scene);
        string fragment = $"oms 1\npatch \"SC0000.BIN\" fingerprint \"{OmsPatchPlan.FingerprintOf(scene)}\"\n\n"
                          + "before site 0xb {\n    G[9] = 1\n}\n";
        string mod = Mod("test.patcher", new Dictionary<string, byte[]>
        {
            ["patches/sc0000.oms"] = Encoding.UTF8.GetBytes(fragment),
        });

        // The base's 0x10 follows the five dwords the fragment inserts at 0xb.
        Assert.Equal("script: SC0000.BIN is patched by test.patcher (patches/sc0000)\n"
                     + "script: the offsets are the linked script's; 0x15 is 0x10 in the base script, which a fragment "
                     + "names and a run without those mods gives\n"
                     + "disassembly around SC0000@0x15:\n"
                     + ">>> 0x00015: mov (global-int 0x10) 0x2\n",
                     Locate("SC0000@0x15", "--context", "0", "--mod", mod, "--no-names"));

        string added = Locate("SC0000@0xb", "--context", "0", "--mod", mod, "--no-names");
        Assert.Contains("0xb is an instruction a patch added, which the base script does not have\n", added);
        Assert.EndsWith(">>> 0x0000b: mov (global-int 0x9) 0x1\n", added);
        // Without the mod the base's own offsets hold.
        Assert.Equal("disassembly around SC0000@0x10:\n>>> 0x00010: mov (global-int 0x10) 0x2\n",
                     Locate("SC0000@0x10", "--context", "0", "--no-names"));
    }

    [Fact]
    public void AStackThatDoesNotLinkSaysWhy()
    {
        string fragment = "oms 1\npatch \"SC0000.BIN\" fingerprint \"sha256:00\"\n\nbefore site 0x0 {\n    G[9] = 1\n}\n";
        string mod = Mod("test.stale", new Dictionary<string, byte[]> { ["patches/sc0000.oms"] = Encoding.UTF8.GetBytes(fragment) });

        string report = Locate("SC0000@0x0", "--context", "0", "--mod", mod, "--no-names");

        Assert.StartsWith("note: the stack does not link (1 mods; 1 errors, ", report);
        Assert.Contains("\n  error: [test.stale] ", report);
        Assert.EndsWith("disassembly around SC0000@0x0:\n>>> 0x00000: call-script 0x" + ScriptId("SC0001.BIN").ToString("x") + "\n",
                        report);
    }

    [Fact]
    public void AScriptAModReplacesIsTheReplacements()
    {
        byte[] replacement = AsCompiled(ScriptAssembler.AssembleBytes([(0x72, [Imm(7)])], [], Abi));
        string mod = Mod("test.replacer", new Dictionary<string, byte[]> { ["replace/SC0000.BIN"] = replacement });

        Assert.Equal("script: SC0000.BIN is replaced by test.replacer\n"
                     + "disassembly around SC0000@0x0:\n"
                     + ">>> 0x00000: wait-for-input 0x7\n",
                     Locate("SC0000@0x0", "--mod", mod, "--no-names"));
    }
}

/// <summary>Runs a checkout tool with the interpreter <c>validate.py</c> runs on (<c>OME_PYTHON</c>), else Python 3.11
/// as the checkout's conventions start it. The child gets no <c>OME_PROFILE</c>, <c>OME_GAME_ROOT</c> or
/// <c>OME_EXTRACTED_ROOT</c> from a validation run of another game: each test names its own.</summary>
internal static class PythonTool
{
    public static (int Exit, string Output, string Error) Run(IReadOnlyList<string> arguments)
    {
        string? configured = Environment.GetEnvironmentVariable("OME_PYTHON");
        var start = new ProcessStartInfo(configured ?? (OperatingSystem.IsWindows() ? "py" : "python3"))
        {
            WorkingDirectory = Paths.Repo,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
            UseShellExecute = false,
        };
        if (configured == null && OperatingSystem.IsWindows()) start.ArgumentList.Add("-3.11");
        foreach (string argument in (string[])["-X", "utf8", .. arguments]) start.ArgumentList.Add(argument);
        foreach (string name in new[] { "OME_PROFILE", "OME_GAME_ROOT", "OME_EXTRACTED_ROOT" }) start.Environment.Remove(name);
        using Process process = Process.Start(start)!;
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output.Replace("\r\n", "\n"), error.Result);
    }
}
