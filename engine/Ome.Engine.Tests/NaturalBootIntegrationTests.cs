using Ome.Engine.Diagnostics;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

[Trait("Category", "Workspace")]
public class NaturalBootIntegrationTests
{
    private sealed class ReachedSc0000Exception : Exception { }
    private sealed class ReachedKamidoriAdvPageException(
        string scriptName, string text, int visibleObjectCount, int audioRequestCount) : Exception
    {
        public string ScriptName { get; } = scriptName;
        public string Text { get; } = text;
        public int VisibleObjectCount { get; } = visibleObjectCount;
        public int AudioRequestCount { get; } = audioRequestCount;
    }
    private sealed class ReachedChmenuRosterException : Exception { }
    private sealed class ReachedTitleWaitException : Exception { }

    private sealed record NaturalBootResult(
        OpcodeTable Table, Sys4ScriptProvider Scripts, VirtualMachine Vm,
        NewGameInputHost Host, StopAtSc0000Sink Sink);

    private sealed class StopAtSc0000Sink : ITraceSink
    {
        public readonly List<string> Entered = new();
        public bool SawStringEqualsStub;
        public bool SawUnitDataCopyStub;
        public Action<string>? OnEnter;
        public bool TracingSteps => true;

        public void Emit(in TraceEvent e)
        {
            if (e.Kind == TraceEventKind.Stub && e.Opcode == 0x194)
                SawStringEqualsStub = true;
            if (e.Kind == TraceEventKind.Stub && e.Opcode is 0x63 or 0x1b0)
                SawUnitDataCopyStub = true;
            if (e.Kind == TraceEventKind.FrameEnter && e.Name != null)
            {
                Entered.Add(e.Name);
                OnEnter?.Invoke(e.Name);
                if (e.Name.Equals("SC0000.BIN", StringComparison.OrdinalIgnoreCase))
                    throw new ReachedSc0000Exception();
            }
        }
    }

    private sealed class StopAfterChmenuRosterSink : ITraceSink
    {
        public VirtualMachine Vm = null!;
        public bool SawIndexSort;
        public bool SawIndexSortFallback;
        public bool TracingSteps => true;

        public void Emit(in TraceEvent e)
        {
            if (e.Kind == TraceEventKind.Stub && e.Opcode == 0x12f)
                SawIndexSortFallback = true;
            if (e.Kind != TraceEventKind.Step || e.Ins == null) return;
            if (e.Opcode == 0x12f) SawIndexSort = true;

            // The step event is emitted before execution. At CHMENU@0x1dbd, the first sort has
            // completed and 0x1db8 has copied the chosen roster slot back to G[0x6718].
            if (e.Ins.Offset == 0x1dbd
                && string.Equals(Vm.DebugFrame?.CurrentScript, "CHMENU.BIN",
                                 StringComparison.OrdinalIgnoreCase))
                throw new ReachedChmenuRosterException();
        }
    }

    private sealed class KamidoriNaturalBootSink : ITraceSink
    {
        public readonly List<string> Entered = new();
        public Action<string>? OnEnter;
        public bool TracingSteps => true;

        public void Emit(in TraceEvent e)
        {
            if (e.Kind != TraceEventKind.FrameEnter || e.Name == null) return;
            Entered.Add(e.Name);
            OnEnter?.Invoke(e.Name);
        }
    }

    private sealed class NewGameInputHost : RecordingWaits
    {
        public VirtualMachine Vm = null!;
        private readonly int _titlePointerX;
        private readonly int _titlePointerY;
        private long _now;
        public int TitlePollSleeps;
        private bool _inGameStart;
        private bool _stopAtKamidoriAdvPage;
        private int _kamidoriAdvLineBaseline;
        private int _kamidoriAdvAudioBaseline;
        private int _gameStartPollSleeps;
        /// <summary>Stop the run at the next sleep of a frame or less, as TITLE's menu loop polls.</summary>
        public bool StopAtNextPollSleep;
        public override long InputClockMilliseconds => _now;

        private readonly (int X, int Y)? _gameStartPointer;

        /// <param name="gameStartPointer">Where GAMESTART's own page is clicked, for a game whose Game Start opens a
        /// page with a start button (魔導巧殻's 初期設定); without it GAMESTART gets the input callbacks alone.</param>
        public NewGameInputHost(int titlePointerX = 400, int titlePointerY = 300, (int X, int Y)? gameStartPointer = null)
        {
            _titlePointerX = titlePointerX;
            _titlePointerY = titlePointerY;
            _gameStartPointer = gameStartPointer;
        }

        public void BeginGameStart()
        {
            _inGameStart = true;
            _gameStartPollSleeps = 0;
            Vm.UpdateMouseButtonState(0x1, false);
            Vm.UpdateInputCallbackState(4, false);
            Vm.QueueInputCallback(10);
        }

        public void StopAtKamidoriAdvPage()
        {
            _stopAtKamidoriAdvPage = true;
            _kamidoriAdvLineBaseline = Lines.Count;
            _kamidoriAdvAudioBaseline = AudioRequestCount;
        }

        public RecordingAudioDevice? AudioDevice;

        private int AudioRequestCount => AudioDevice?.Commands.Count ?? 0;

        public override void WaitForInput(Func<bool> serviceInputCallback)
        {
            if (_stopAtKamidoriAdvPage && Lines.Count > _kamidoriAdvLineBaseline)
                throw new ReachedKamidoriAdvPageException(
                    "SC0000.BIN", Lines[^1].Text,
                    Vm.Gfx.SnapshotVisibleObjects().Count,
                    AudioRequestCount - _kamidoriAdvAudioBaseline);
            base.WaitForInput(serviceInputCallback);
        }

        public override void Sleep(long duration)
        {
            if (StopAtNextPollSleep && duration <= 1) throw new ReachedTitleWaitException();
            base.Sleep(duration);
            _now += Math.Max(1, duration);
            if (duration > 1) return;

            // TITLE's first menu entry is Game Start. Hold the pointer over its native 800x600
            // rectangle, then provide one complete primary-button edge to its raw input callback.
            int polls = _inGameStart ? ++_gameStartPollSleeps : ++TitlePollSleeps;
            int cycle = polls % 200;
            if (_inGameStart && polls <= 200) return;
            if (cycle == 1)
            {
                int input = _inGameStart && polls <= 400 ? 0 : 4;
                if (!_inGameStart)
                {
                    Vm.UpdatePointer(_titlePointerX, _titlePointerY);
                    Vm.UpdateMouseButtonState(0x1, true);
                }
                else if (_gameStartPointer is { } point)
                {
                    Vm.UpdatePointer(point.X, point.Y);
                    Vm.UpdateMouseButtonState(0x1, true);
                }
                Vm.UpdateInputCallbackState(input, true);
            }
            // TITLE polls its registered mouse callback every 50 ms and activates on the release edge.
            // Keep the button down through one callback, then release it before the next.
            else if (cycle == 120)
            {
                int input = _inGameStart && polls <= 400 ? 0 : 4;
                if (!_inGameStart || _gameStartPointer is not null) Vm.UpdateMouseButtonState(0x1, false);
                Vm.UpdateInputCallbackState(input, false);
                Vm.QueueInputCallback(10); // native/main-thread release callback unlocks the menu input gate
            }
        }
    }

    private static NaturalBootResult RunNaturalNewGameToSc0000()
    {
        var table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4422");
        var scripts = Sys4ScriptProvider.Load(table);
        var host = new NewGameInputHost();
        var sink = new StopAtSc0000Sink();
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("himegari")!;
        var vm = new VirtualMachine(scripts.RequireByName("SYSTEM4.BIN"), table, host,
            new VmOptions(MaxSteps: 5_000_000), scripts, sink,
            build: EngineBuilds.ForProfile(profile));
        host.Vm = vm;
        sink.OnEnter = name =>
        {
            if (name.Equals("GAMESTART.BIN", StringComparison.OrdinalIgnoreCase)) host.BeginGameStart();
        };

        var exception = Record.Exception(() => vm.Run());
        Assert.True(exception is ReachedSc0000Exception,
            $"halt={vm.HaltReason}; title_sleeps={host.TitlePollSleeps}; entered={string.Join(",", sink.Entered)}");
        return new NaturalBootResult(table, scripts, vm, host, sink);
    }

    [Fact]
    public void System4Root_NewGameSelectionNaturallyCallsSc0000()
    {
        var boot = RunNaturalNewGameToSc0000();
        var vm = boot.Vm;
        var host = boot.Host;
        var sink = boot.Sink;

        Assert.Equal(new[]
        {
            "SYSTEM4.BIN", "INITCONFIG.BIN", "INIT2.BIN",
        }, sink.Entered.Take(3));
        int logo = sink.Entered.IndexOf("LOGO.BIN");
        int opening = sink.Entered.IndexOf("OP.BIN");
        int init = sink.Entered.IndexOf("INIT.BIN");
        int title = sink.Entered.IndexOf("TITLE.BIN");
        Assert.True(logo >= 0 && logo < opening && opening < init && init < title,
            $"entered={string.Join(",", sink.Entered)}");
        Assert.Equal(new[] { (0x335fL, 42, 4L), (0x3364L, 42, 4L) }, host.ModalMovies);
        Assert.Contains("TITLE.BIN", sink.Entered);
        Assert.Contains("GAMESTART.BIN", sink.Entered);
        Assert.Contains("UNITECH.BIN", sink.Entered);
        Assert.Contains("CALCARR.BIN", sink.Entered);
        Assert.Equal("SC0000.BIN", sink.Entered[^1]);
        int baseBtanInit2 = sink.Entered.IndexOf("BTANINIT2.BIN");
        int appendAutorun = sink.Entered.IndexOf("$1$AUTORUN.BIN");
        int appendEbInit = sink.Entered.IndexOf("$1$EBINIT.BIN");
        int tune = sink.Entered.IndexOf("TUNE.BIN");
        Assert.Equal(new[] { 1 }, boot.Scripts.MountedAppendSelectors);
        Assert.True(baseBtanInit2 >= 0 && baseBtanInit2 < appendAutorun
            && appendAutorun < appendEbInit && appendEbInit < tune,
            $"entered={string.Join(",", sink.Entered)}");
        Assert.Equal(40, vm.Globals.GetValueOrDefault(0x7a37e + 81));       // append unit starting level
        Assert.Equal(0x0100002d, vm.Globals.GetValueOrDefault(0x6fb86 + 81)); // packed battle sprite
        Assert.Equal(1, vm.Globals.GetValueOrDefault(0));
        Assert.Equal(0x22, vm.Globals.GetValueOrDefault(0x699));
        Assert.Equal(1, vm.Globals.GetValueOrDefault(0x6c1));
        // The boot scripts own every value SC0000 reads: SYSTEM4@0x1ca copies the SFX channel table, and nothing
        // writes adv_hide_window_enabled, so it stays at the zero-filled bank's 0.
        Assert.Equal(new long[] { 0, 4, 5, 6, 9 },
            Enumerable.Range(0x6242c, 5).Select(address => vm.Globals.GetValueOrDefault(address)).ToArray());
        Assert.Equal(0, vm.Globals.GetValueOrDefault(0x62425));
        Assert.False(sink.SawStringEqualsStub);
        Assert.False(sink.SawUnitDataCopyStub);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Profile", "kamidori")]
    public void KamidoriSystem4Root_NewGameSelectionNaturallyCallsFirstAdvScene(
        bool translationOverlay)
    {
        string gameRoot = translationOverlay ? Paths.KamidoriTranslatedDir : Paths.KamidoriDir;
        string[] looseRoots = translationOverlay
            ? [Path.Combine(gameRoot, "patch"), gameRoot]
            : [gameRoot];
        var (reachedAdv, entered) = RunKamidoriNewGameToFirstAdvPage(gameRoot, looseRoots);
        Assert.Contains("TITLE.BIN", entered);
        Assert.Contains("GAMESTART.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        Assert.NotEmpty(reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
        Assert.True(reachedAdv.AudioRequestCount > 0);
        Assert.Equal(reachedAdv.ScriptName, entered[^1]);
    }

    /// <summary>Mod loader gate 6 (plan M7): a fragment patched into the translated Kamidori SC0000
    /// (<c>patch/SC0000.bin</c>) links against the overlay base, and its line is the first ADV page of a natural
    /// new game.</summary>
    [Fact]
    [Trait("Profile", "kamidori")]
    public void KamidoriTranslatedPrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Paths.KamidoriTranslatedDir;
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"translated Kamidori install not found at {gameRoot}");
        string[] baseRoots = [Path.Combine(gameRoot, "patch"), gameRoot];
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("kamidori")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, baseRoots);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("kamidori", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("kamidori"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.kamidori-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-gate6-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            var (reachedAdv, _) = RunKamidoriNewGameToFirstAdvPage(gameRoot, [layer, ..baseRoots]);
            Assert.Equal("[OMS] A mod inserted this line before the prologue's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>創刻のアテリアル's P2 gate (docs/adding-a-game.md, step 6): its own boot goes through the logo, whose
    /// fades are AGE 4.38D's polygon objects, and the title, which builds the 3D scene, to SC0000's first page, with
    /// no opcode skipped. Game Start goes straight to SC0000: GAMESTART.BIN shows no page of its own.</summary>
    [Fact]
    [Trait("Profile", "arterial")]
    public void ArterialSystem4Root_NewGameSelectionNaturallyReachesTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("arterial");
        // The title's Game Start entry (tools/drive/arterial.toml, title.start).
        var (reachedAdv, entered) = RunNewGameToFirstAdvPage("arterial", gameRoot, [gameRoot], 510, 284);
        Assert.Contains("LOGO.BIN", entered);
        Assert.Contains("TITLE.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        Assert.EndsWith("重苦しい風が吹く。", reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
    }

    /// <summary>戦女神VERITA's P2 gate (docs/adding-a-game.md, step 6): its own boot goes through the title to SC0000's
    /// first page, with no opcode skipped. Game Start goes straight to SC0000, as on native AGE
    /// (tools/drive/scenarios/verita-opening.steps).</summary>
    [Fact]
    [Trait("Profile", "verita")]
    public void VeritaSystem4Root_NewGameSelectionNaturallyReachesTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("verita");
        // The title's Game Start entry (tools/drive/verita.toml, title.start).
        var (reachedAdv, entered) = RunNewGameToFirstAdvPage("verita", gameRoot, [gameRoot], 183, 338);
        Assert.Contains("TITLE.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        Assert.EndsWith("フレスラント王国の国土の大半を占める広大なマータ砂漠。", reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
    }

    /// <summary>戦女神ZERO's P2 gate (docs/adding-a-game.md, step 6): its own boot goes through the logo and opening
    /// movies and the title, then GAMESTART's game-mode box (BUNKI), whose 通常 goes to SC0000's first page, with no
    /// opcode skipped (tools/drive/scenarios/bg0-opening.steps).</summary>
    [Fact]
    [Trait("Profile", "bg0")]
    public void Bg0System4Root_NewGameSelectionNaturallyReachesTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("bg0");
        // The title's Game Start entry (tools/drive/bg0.toml, title.start).
        var (reachedAdv, entered) = RunNewGameToFirstAdvPage("bg0", gameRoot, [gameRoot], 700, 95);
        Assert.Contains("TITLE.BIN", entered);
        Assert.Contains("GAMESTART.BIN", entered);
        Assert.Contains("BUNKI.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        Assert.EndsWith("「よし、これでこっちの動きは封じた！」", reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
    }

    /// <summary>P2 (docs/adding-a-game.md): 魔導巧殻's own boot reaches SC0000's first page with no opcode skipped,
    /// through the title, GAMESTART's 初期設定 page and the PROLOGUE movie.</summary>
    [Fact]
    [Trait("Profile", "koukaku")]
    public void KoukakuSystem4Root_NewGameSelectionNaturallyReachesTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("koukaku");
        // The title's Game Start entry and GAMESTART's ゲーム開始 (tools/drive/koukaku.toml, title.start and
        // start.begin).
        var (reachedAdv, entered) = RunNewGameToFirstAdvPage("koukaku", gameRoot, [gameRoot], 160, 560, (995, 475),
                                                          maxSteps: 20_000_000);   // SC0000 comes after 8.9M steps
        Assert.Contains("TITLE.BIN", entered);
        Assert.Contains("GAMESTART.BIN", entered);
        Assert.Contains("PROLOGUE.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        // The two U+E000 are cp932 0xF040, a glyph of the game's own (SC0000.BIN's string at 0x10d91).
        Assert.EndsWith("一面を覆い尽くす、焼け焦げた木々や兵\uE000\uE000。", reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
    }

    /// <summary>P2 (docs/adding-a-game.md): 天秤のLa DEA。's own boot reaches SC0000's first page with no opcode
    /// skipped, through the title and GAMESTART's 初期設定 page. SYSTEM4 stops and levels the three voices as it boots
    /// (ops 0x2f6 and 0x2f8), and the page plays its voice after stopping them.</summary>
    [Fact]
    [Trait("Profile", "ladea")]
    public void LaDeaSystem4Root_NewGameSelectionNaturallyReachesTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("ladea");
        // The title's Game Start entry and GAMESTART's ゲーム開始 (tools/drive/ladea.toml, title.start and start.begin).
        var (reachedAdv, entered) = RunNewGameToFirstAdvPage("ladea", gameRoot, [gameRoot], 160, 620, (1005, 638),
                                                          maxSteps: 20_000_000);
        Assert.Contains("TITLE.BIN", entered);
        Assert.Contains("GAMESTART.BIN", entered);
        Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
        Assert.Equal("神殺しと呼ばれる男がいる。", reachedAdv.Text);
        Assert.True(reachedAdv.VisibleObjectCount > 0);
    }

    /// <summary>M1's sample package (docs/adding-a-game.md, section 1): the example fragment links against
    /// 天秤のLa DEA。's opening scene through the game's ADV module, and its line is the first ADV page of a natural
    /// new game.</summary>
    [Fact]
    [Trait("Profile", "ladea")]
    public void LaDeaPrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("ladea");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"ladea install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("ladea")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("ladea", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("ladea"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.ladea-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-ladea-line-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            // The title's Game Start entry and GAMESTART's ゲーム開始, as the boot test above.
            var (reachedAdv, _) = RunNewGameToFirstAdvPage("ladea", gameRoot, [layer, gameRoot], 160, 620, (1005, 638),
                                                           maxSteps: 20_000_000);
            Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
            Assert.Equal("[OMS] A mod inserted this line before the opening scene's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>M1's sample package (docs/adding-a-game.md, section 1): the example fragment links against 魔導巧殻's
    /// opening scene through the game's ADV module, and its line is the first ADV page of a natural new game.</summary>
    [Fact]
    [Trait("Profile", "koukaku")]
    public void KoukakuPrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("koukaku");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"koukaku install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("koukaku")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("koukaku", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("koukaku"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.koukaku-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-koukaku-line-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            // The title's Game Start entry and GAMESTART's ゲーム開始, as the boot test above.
            var (reachedAdv, _) = RunNewGameToFirstAdvPage("koukaku", gameRoot, [layer, gameRoot], 160, 560, (995, 475),
                                                           maxSteps: 20_000_000);
            Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
            Assert.Equal("[OMS] A mod inserted this line before the opening scene's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>M1's sample package (docs/adding-a-game.md, section 1): the example fragment links against
    /// 戦女神ZERO's opening scene through the game's ADV module, and its line is the first ADV page of a natural new
    /// game.</summary>
    [Fact]
    [Trait("Profile", "bg0")]
    public void Bg0PrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("bg0");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"bg0 install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("bg0")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("bg0", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("bg0"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.bg0-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-bg0-line-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            // The title's Game Start entry (tools/drive/bg0.toml, title.start), as the boot test above.
            var (reachedAdv, _) = RunNewGameToFirstAdvPage("bg0", gameRoot, [layer, gameRoot], 700, 95);
            Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
            Assert.Equal("[OMS] A mod inserted this line before the opening scene's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>M1's sample package (docs/adding-a-game.md, section 1): the example fragment links against
    /// 戦女神VERITA's opening scene through the game's ADV module, and its line is the first ADV page of a natural new
    /// game.</summary>
    [Fact]
    [Trait("Profile", "verita")]
    public void VeritaPrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("verita");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"verita install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("verita")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("verita", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("verita"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.verita-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-verita-line-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            var (reachedAdv, _) = RunNewGameToFirstAdvPage("verita", gameRoot, [layer, gameRoot], 183, 338);
            Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
            Assert.Equal("[OMS] A mod inserted this line before the opening scene's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>Op 0x24a's counters (Graphics.TextureMemory) at 戦女神VERITA's title, where its menu loop first polls
    /// (TITLE@0xd7), are native's surfaces' there. Native AGE's surface slots held two 1024 x 576 render targets and
    /// 18,398,624 bytes of other textures at TITLE@0xd7 (read with Frida, 2026-11-25); its counters held 1,753,088 bytes
    /// more, the display background's texture, which the port leaves out (docs/engine-re.md, "Texture memory
    /// counters").</summary>
    [Fact]
    [Trait("Profile", "verita")]
    public void VeritasTitleHoldsTheTexturesNativeCountsForOp0x24a()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("verita");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"verita install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("verita")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        var scripts = new Sys4ScriptProvider(table, catalog, store);
        var graphics = new Graphics(new ResourceMap(catalog, store), new FrameClock(), catalog.LogicalCanvas);
        var rig = new AudioRig();
        var host = new NewGameInputHost(183, 338) { AudioDevice = rig.Device, Clock = rig.Clock };
        var sink = new KamidoriNaturalBootSink
        {
            OnEnter = name => host.StopAtNextPollSleep |= name.Equals("TITLE.BIN", StringComparison.OrdinalIgnoreCase),
        };
        var vm = new VirtualMachine(
            scripts.RequireByName(profile.NaturalBootScript), table, host,
            new VmOptions(MaxSteps: 5_000_000), scripts, sink,
            build: EngineBuilds.ForProfile(profile), skipAndAuto: rig.Skip, audio: rig.Audio, tick: rig.Tick,
            graphics: graphics);
        host.Vm = vm;

        Exception? exception = Record.Exception(() => vm.Run());

        Assert.True(exception is ReachedTitleWaitException,
            $"exception={exception?.GetType().Name ?? "none"}; halt={vm.HaltReason}; entered={string.Join(",", sink.Entered)}");
        Assert.Null(vm.CompatibilityFailure);
        Assert.Equal((4_718_592, 18_398_624), graphics.TextureMemory());
    }

    /// <summary>M1's sample package (docs/adding-a-game.md, section 1): the example fragment links against
    /// 創刻のアテリアル's opening scene through the game's ADV module, and its line is the first ADV page of a natural
    /// new game.</summary>
    [Fact]
    [Trait("Profile", "arterial")]
    public void ArterialPrologueFragmentIsTheFirstAdvPage()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("arterial");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"arterial install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("arterial")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("arterial", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("arterial"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.arterial-prologue-line");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());

        string layer = Path.Combine(Path.GetTempPath(), "ome-arterial-line-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            File.WriteAllBytes(Path.Combine(layer, "SC0000.BIN"), link.Scripts["SC0000.BIN"].Bytes);
            var (reachedAdv, _) = RunNewGameToFirstAdvPage("arterial", gameRoot, [layer, gameRoot], 510, 284);
            Assert.Equal("SC0000.BIN", reachedAdv.ScriptName);
            Assert.Equal("[OMS] A mod inserted this line before the opening scene's first page.", reachedAdv.Text);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    /// <summary>A script mod of 創刻のアテリアル's (docs/modding/games/arterial.md, "Catalogs, scenes and saves"): the
    /// example's catalog takes selector 5, after the install's four appends, and op 0x143 runs its record 0 at the end
    /// of INIT2. That writes the scene's record, the SCJUMP chain's slot 5 and 龍司's third name.</summary>
    [Fact]
    [Trait("Profile", "arterial")]
    public void ArterialNewSceneModRegistersItsSceneAndRenamesAtBoot()
    {
        string gameRoot = Environment.GetEnvironmentVariable("OME_GAME_ROOT") ?? Paths.GameInstall("arterial");
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"arterial install not found at {gameRoot}");
        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find("arterial")!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var store = new Sys4AssetStore(catalog, gameRoot, [gameRoot]);
        Ome.Mods.Linking.ModLinkBase linkBase = Ome.Mods.Linking.ModLinkBase.ForInstall("arterial", sys4Ini, catalog,
            store, table, Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson),
            Ome.Mods.Linking.GameToolchain.Shipped("arterial"));
        string source = Path.Combine(Paths.Repo, "oms", "examples", "mods", "example.arterial-new-scene");
        using var mod = Ome.Mods.Packages.ModPackage.Open(source);
        var link = Ome.Mods.Linking.ModLinker.Link(linkBase, [mod], []);
        Assert.True(link.Succeeded, link.Report.Render());
        Ome.Mods.Linking.LinkedCatalog linked = Assert.Single(link.Catalogs);
        Assert.Equal(5, linked.Selector);

        string layer = Path.Combine(Path.GetTempPath(), "ome-arterial-scene-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(layer);
        try
        {
            var (aai, alf) = linked.Write(linkBase.AppendMagic, linkBase.CatalogTitle);
            File.WriteAllBytes(Path.Combine(layer, linked.CatalogFile), aai);
            File.WriteAllBytes(Path.Combine(layer, linked.ArchiveFile), alf);
            Sys4AssetCatalog mounted = Sys4AssetCatalog.Load(sys4Ini, [layer]);
            var scripts = new Sys4ScriptProvider(table, mounted, new Sys4AssetStore(mounted, gameRoot, [gameRoot]));
            var session = new GameSession();
            SceneResult result = session.RunScene(scripts.RequireByName("INIT2.BIN"), table, new HeadlessWaits(),
                                                  provider: scripts, build: EngineBuilds.ForProfile(profile));
            Assert.True(result.Halt is null or "exit" or "end", result.Halt);

            // Records 0 to 2 are AUTO, OFFER and SCENE: scene 9990's script, the chain's slot of selector 5, and
            // character_names[12][2] (GS[0x4154 + 6 * 12 + 2]).
            Assert.Equal(linked.PackedId(2), (uint)session.Globals[0x214b71 + 0x2706]);
            Assert.Equal(linked.PackedId(1), (uint)session.Globals[0x4525dd + 5]);
            Assert.Equal(0x10001df, session.Globals[0x4525dd + 1]);   // the first append's $1$SCJUMP stays
            Assert.Equal("[MOD] 傷だらけの男", session.GlobalStrings[0x4154 + 6 * 12 + 2]);
        }
        finally
        {
            Directory.Delete(layer, recursive: true);
        }
    }

    private static (ReachedKamidoriAdvPageException Reached, List<string> Entered) RunKamidoriNewGameToFirstAdvPage(
        string gameRoot, string[] looseRoots)
        // Kamidori's 1024x576 title menu begins at x=606; its first row spans y=286..340.
        => RunNewGameToFirstAdvPage("kamidori", gameRoot, looseRoots, 700, 300);

    private static (ReachedKamidoriAdvPageException Reached, List<string> Entered) RunNewGameToFirstAdvPage(
        string profileId, string gameRoot, string[] looseRoots, int titlePointerX, int titlePointerY,
        (int X, int Y)? gameStartPointer = null, long maxSteps = 5_000_000)
    {
        string sys4Ini = Path.Combine(gameRoot, "SYS4INI.BIN");
        Assert.True(File.Exists(sys4Ini), $"{profileId} install not found at {gameRoot}");

        GameProfileManifest profile = GameProfileRegistry.BuiltIn.Find(profileId)!;
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, profile.EngineAbiId);
        Sys4AssetCatalog catalog = Sys4AssetCatalog.Load(sys4Ini);
        var scripts = new Sys4ScriptProvider(
            table, catalog, new Sys4AssetStore(catalog, gameRoot, looseRoots));
        EngineBuild build = EngineBuilds.ForProfile(profile);
        var rig = new AudioRig(rules: build.Rules.Audio);
        var host = new NewGameInputHost(titlePointerX, titlePointerY, gameStartPointer)
        {
            AudioDevice = rig.Device, Clock = rig.Clock,
        };
        var sink = new KamidoriNaturalBootSink();
        var vm = new VirtualMachine(
            scripts.RequireByName(profile.NaturalBootScript), table, host,
            new VmOptions(MaxSteps: maxSteps), scripts, sink,
            build: build, skipAndAuto: rig.Skip, audio: rig.Audio, tick: rig.Tick);
        host.Vm = vm;
        sink.OnEnter = name =>
        {
            if (name.Equals("GAMESTART.BIN", StringComparison.OrdinalIgnoreCase)) host.BeginGameStart();
            if (name.Equals("SC0000.BIN", StringComparison.OrdinalIgnoreCase)) host.StopAtKamidoriAdvPage();
        };

        Exception? exception = Record.Exception(() => vm.Run());
        Assert.True(exception is ReachedKamidoriAdvPageException,
            $"exception={exception?.GetType().Name ?? "none"}; halt={vm.HaltReason}; "
            + $"title_sleeps={host.TitlePollSleeps}; entered={string.Join(",", sink.Entered)}");
        var reachedAdv = Assert.IsType<ReachedKamidoriAdvPageException>(exception);
        Assert.Null(vm.CompatibilityFailure);
        return (reachedAdv, sink.Entered);
    }

    [Fact]
    public void NaturalNewGamePartyStateSurvivesRealChmenuRosterSort()
    {
        var boot = RunNaturalNewGameToSc0000();
        Assert.Equal(2, boot.Vm.Globals.GetValueOrDefault(0x6718));
        Assert.Equal(0x13, boot.Vm.Globals.GetValueOrDefault(0x673c + 2));
        Assert.Equal(2, boot.Vm.Globals.GetValueOrDefault(0x67a0 + 2));

        var sink = new StopAfterChmenuRosterSink();
        var menuVm = new VirtualMachine(boot.Scripts.RequireByName("CHMENU.BIN"), boot.Table,
            new RecordingWaits(), new VmOptions(MaxSteps: 5_000_000), boot.Scripts, sink);
        sink.Vm = menuVm;
        foreach (var (address, value) in boot.Vm.Globals) menuVm.SeedGlobal(address, value);
        foreach (var (address, value) in boot.Vm.GlobalStrings) menuVm.GlobalStrings[address] = value;

        var exception = Record.Exception(() => menuVm.Run());

        Assert.IsType<ReachedChmenuRosterException>(exception);
        Assert.True(sink.SawIndexSort);
        Assert.False(sink.SawIndexSortFallback);
        Assert.Equal(2, menuVm.Globals.GetValueOrDefault(0x6718));
    }
}
