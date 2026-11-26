using System.Text;
using Ome.Engine.Persistence;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;

public sealed class GameProfileSelectionTests
{
    private static readonly GameCatalogIdentity HimegariIdentity =
        new("S4IC422", "姫狩りダンジョンマイスター");
    private static readonly GameCatalogIdentity KamidoriIdentity =
        new("S4IC433", "神採りアルケミーマイスター");

    [Fact]
    public void BuiltInManifestsExposeGameIdentityAndEngineAbiWithoutContentPaths()
    {
        GameProfileRegistry registry = GameProfileRegistry.BuiltIn;

        GameProfileManifest himegari = Assert.IsType<GameProfileManifest>(registry.Find("himegari"));
        Assert.Equal("SYS4", himegari.SysFrontendId);
        Assert.Equal("4.24J", himegari.EngineBuildId);
        Assert.Equal("SYS4422", himegari.EngineAbiId);
        Assert.Equal(["SYS4422"], himegari.ScriptRevisions);
        Assert.Equal("SYSTEM4.BIN", himegari.NaturalBootScript);
        Assert.Equal("himegari", himegari.PersistenceNamespace);
        Assert.True(himegari.PersistenceWritesEnabled);
        Assert.Equal(310, himegari.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal("姫狩りダンジョンマイスター", himegari.Persistence.GameId);
        Assert.Equal(["蟋ｫ迢ｩ繧翫ム繝ｳ繧ｸ繝ｧ繝ｳ繝槭う繧ｹ繧ｿ繝ｼ"], himegari.Persistence.LegacyGameIds);
        Assert.Equal([402459, 1, 789, 1, 1, 1], himegari.Persistence.BankDimensions.ToArray());
        Assert.Equal("res://config/japanese-text-rendering.json",
            himegari.MetadataReferences["portableTextRendering"]);
        DirectSceneDiagnosticPolicy himegariDiagnostics =
            Assert.IsType<DirectSceneDiagnosticPolicy>(himegari.DirectSceneDiagnostics);
        Assert.Equal(["INITCONFIG.BIN", "INIT2.BIN", "INIT.BIN"],
            himegariDiagnostics.DataBootstrapScripts);
        Assert.Equal([
            "SKINIT.BIN", "ITINIT.BIN", "EBINIT.BIN", "CGINIT.BIN", "MPINIT.BIN",
            "AFINIT.BIN", "CCINIT.BIN", "STINIT.BIN", "STINIT2.BIN",
        ], himegariDiagnostics.DataTableBootstrapScripts);
        DebugSceneLaunchPolicy debugLaunch =
            Assert.IsType<DebugSceneLaunchPolicy>(himegari.DebugSceneLaunch);
        Assert.Equal("SYSTEM4.BIN", debugLaunch.RootScript);
        Assert.Equal("TITLE.BIN", debugLaunch.CoordinatorScript);
        Assert.Equal(0x699, debugLaunch.PackedScriptIdAddress);
        Assert.Equal([new ProfileCellSeed(0x0, 1), new ProfileCellSeed(0xaba5c, -1), new ProfileCellSeed(0x62ccf, 0)],
            debugLaunch.CoordinatorWrites);   // TITLE's ＡＤＶデバッグ writes (TITLE@0x61b)
        Assert.True(himegari.Matches(HimegariIdentity));

        GameProfileManifest kamidori = Assert.IsType<GameProfileManifest>(registry.Find("kamidori"));
        Assert.Equal("SYS4", kamidori.SysFrontendId);
        Assert.Equal("4.33G", kamidori.EngineBuildId);
        Assert.Equal("SYS4433", kamidori.EngineAbiId);
        Assert.Equal(["SYS4433"], kamidori.ScriptRevisions);
        Assert.Equal("SYSTEM4.BIN", kamidori.NaturalBootScript);
        Assert.Equal("kamidori", kamidori.PersistenceNamespace);
        Assert.True(kamidori.PersistenceWritesEnabled);
        Assert.Equal(320, kamidori.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal("神採りアルケミーマイスター", kamidori.Persistence.GameId);
        Assert.Empty(kamidori.Persistence.LegacyGameIds);
        Assert.Equal([1037327, 1, 802, 1, 1, 1], kamidori.Persistence.BankDimensions.ToArray());
        Assert.Null(kamidori.Persistence.ReadOnlyReason);
        Assert.Null(kamidori.DirectSceneDiagnostics);
        Assert.Null(kamidori.DebugSceneLaunch);
        Assert.True(kamidori.Matches(KamidoriIdentity));
    }

    /// <summary>創刻のアテリアル is at P3 and M2 (docs/adding-a-game.md): the engine names its install, reads all
    /// of it, has a handler for every opcode its scripts use and saves; its build is declared from 4.33G; script
    /// mods and text packages work for it, and its data tables are decoded and named. Its base mod is not built
    /// yet.</summary>
    [Fact]
    public void ArterialIsCompleteForPlayAndHasItsBaseMod()
    {
        GameProfileManifest arterial = Assert.IsType<GameProfileManifest>(GameProfileRegistry.BuiltIn.Find("arterial"));

        Assert.Equal(("SYS4", "4.38D", "SYS4438", "SYSTEM4.BIN"),
                     (arterial.SysFrontendId, arterial.EngineBuildId, arterial.EngineAbiId, arterial.NaturalBootScript));
        Assert.Equal(["SYS4438"], arterial.ScriptRevisions);
        Assert.True(arterial.Matches(new GameCatalogIdentity("S4IC438", "創刻のアテリアル")));
        Assert.Equal(new GameSupport(PlayLevel.Complete, ModdingLevel.BaseMod), arterial.Support);
        Assert.True(arterial.Support.IsPlayable);
        Assert.Equal(("P3", "M3"), (arterial.Support.PlayId, arterial.Support.ModdingId));
        Assert.Equal("none", new GameSupport(PlayLevel.Complete, ModdingLevel.None).ModdingId);
        // The launcher's art for it comes from the publisher's site, as the other two games' does.
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu13/banner/banner_eu13_a.jpg", arterial.Launcher!.Banner!.Url.ToString());
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu13/image/eu13_package.jpg", arterial.Launcher.Cover!.Url.ToString());
        // A game below P3 is still being added: the run says so, and the launcher does not offer it.
        Assert.False(new GameSupport(PlayLevel.Boots, ModdingLevel.None).IsPlayable);
        Assert.Equal(GameSupport.Supported, GameProfileRegistry.BuiltIn.Find("himegari")!.Support);
        Assert.Equal(GameSupport.Supported, GameProfileRegistry.BuiltIn.Find("kamidori")!.Support);
        Assert.Throws<InvalidOperationException>(() => GameSupport.Parse("P0", "none"));
        Assert.Throws<InvalidOperationException>(() => GameSupport.Parse("P3", null));
        // Its saves are on since a native save was read and loaded both ways (2026-11-19).
        Assert.True(arterial.PersistenceWritesEnabled);
        Assert.Null(arterial.Persistence.ReadOnlyReason);
        Assert.Equal(320, arterial.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal([260993, 1, 231, 1, 1, 1], arterial.Persistence.BankDimensions.ToArray());
        Assert.Null(arterial.DirectSceneDiagnostics);
        Assert.Null(arterial.DebugSceneLaunch);
        Assert.Equal("SYS4438", Ome.Engine.Vm.EngineBuilds.ForProfile(arterial).AbiId);
    }

    /// <summary>戦女神VERITA is at P3 and M3 (docs/adding-a-game.md): every opcode has a handler, its build is declared
    /// from 4.24J, its saves load on native AGE and the port alike (tools/drive/scenarios/verita-save-load.steps), and
    /// its base mod, verita-base, names its cells, raises its events and applies its data files.</summary>
    [Fact]
    public void VeritaIsAddedFromHimegarisBuild()
    {
        GameProfileManifest verita = Assert.IsType<GameProfileManifest>(GameProfileRegistry.BuiltIn.Find("verita"));

        Assert.Equal(("SYS4", "4.24N", "SYS4424", "SYSTEM4.BIN"),
                     (verita.SysFrontendId, verita.EngineBuildId, verita.EngineAbiId, verita.NaturalBootScript));
        Assert.Equal(["SYS4424"], verita.ScriptRevisions);
        Assert.True(verita.Matches(new GameCatalogIdentity("S4IC424", "戦女神VERITA")));
        Assert.Equal(("P3", "M3"), (verita.Support.PlayId, verita.Support.ModdingId));
        Assert.True(verita.Support.IsPlayable);
        Assert.True(verita.PersistenceWritesEnabled);
        Assert.Null(verita.Persistence.ReadOnlyReason);
        Assert.Equal(310, verita.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal([2525977, 1, 438, 1, 1, 1], verita.Persistence.BankDimensions.ToArray());
        Assert.Null(verita.DirectSceneDiagnostics);
        Assert.Null(verita.DebugSceneLaunch);
        // The publisher's banner and box art for the game (eu11), as for the other games.
        GameLauncherArt art = Assert.IsType<GameLauncherArt>(verita.Launcher);
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu11/banner_eu11a.jpg", art.Banner!.Url.AbsoluteUri);
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu11/eu11_package.jpg", art.Cover!.Url.AbsoluteUri);
        Ome.Engine.Vm.EngineBuild build = Ome.Engine.Vm.EngineBuilds.ForProfile(verita);
        Assert.Equal(("4.24N", "SYS4424"), (build.Id, build.AbiId));
        Assert.Same(Ome.Engine.Vm.EngineBuilds.Age424N, build);
    }

    /// <summary>戦女神ZERO is at P3 and M3 (docs/adding-a-game.md): the engine names its install, reads all of it, has a
    /// handler for every opcode its scripts use, boots it by its own route to SC0000's first page, and saves as native
    /// does; its scripts rebuild from Open Maid Script, its data tables are decoded and named (docs/bg0-data-tables.md),
    /// and its base mod carries their toolchain, its events and its data files' handlers. Its build is declared from
    /// 4.24J.</summary>
    [Fact]
    public void ZeroIsCompleteForPlayAndHasItsBaseMod()
    {
        GameProfileManifest zero = Assert.IsType<GameProfileManifest>(GameProfileRegistry.BuiltIn.Find("bg0"));

        Assert.Equal(("SYS4", "4.15D", "SYS4415", "SYSTEM4.BIN"),
                     (zero.SysFrontendId, zero.EngineBuildId, zero.EngineAbiId, zero.NaturalBootScript));
        Assert.Equal(["SYS4415"], zero.ScriptRevisions);
        Assert.True(zero.Matches(new GameCatalogIdentity("S4IC415", "戦女神ZERO")));
        Assert.Equal(("P3", "M3"), (zero.Support.PlayId, zero.Support.ModdingId));
        Assert.True(zero.Support.IsPlayable);
        // Each target loads the other's saves, and save-diff shows only the time (tools/drive/scenarios/bg0-save-load.steps).
        Assert.True(zero.PersistenceWritesEnabled);
        Assert.Null(zero.Persistence.ReadOnlyReason);
        Assert.Equal(300, zero.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal([593923, 1, 224, 1, 1, 1], zero.Persistence.BankDimensions.ToArray());
        Assert.Null(zero.DirectSceneDiagnostics);
        Assert.Null(zero.DebugSceneLaunch);
        // The launcher's art for it comes from the publisher's site, as the other games' does.
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu09/banner_eu09a.jpg", zero.Launcher!.Banner!.Url.ToString());
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu09/package.jpg", zero.Launcher.Cover!.Url.ToString());
        Ome.Engine.Vm.EngineBuild build = Ome.Engine.Vm.EngineBuilds.ForProfile(zero);
        Assert.Equal(("4.15D", "SYS4415"), (build.Id, build.AbiId));
        Assert.Same(Ome.Engine.Vm.EngineBuilds.Age415D, build);
        Assert.Equal(Ome.Engine.Vm.EngineBuilds.Age424J.Rules.Text, build.Rules.Text);
    }

    /// <summary>魔導巧殻 is at P3 and M1 (docs/adding-a-game.md): the engine names its install, reads all of it, has a
    /// handler for every opcode its scripts use, boots it by its own route to SC0000's first page, and saves as native
    /// does; its scripts rebuild from Open Maid Script, and its base mod carries their toolchain. Its build is declared
    /// from 4.38D.</summary>
    [Fact]
    public void KoukakuIsAddedFromArterialsBuild()
    {
        GameProfileManifest koukaku = Assert.IsType<GameProfileManifest>(GameProfileRegistry.BuiltIn.Find("koukaku"));

        Assert.Equal(("SYS4", "4.43B", "SYS4443", "SYSTEM4.BIN"),
                     (koukaku.SysFrontendId, koukaku.EngineBuildId, koukaku.EngineAbiId, koukaku.NaturalBootScript));
        Assert.Equal(["SYS4443"], koukaku.ScriptRevisions);
        Assert.True(koukaku.Matches(new GameCatalogIdentity("S4IC443", "魔導巧殻 ～闇の月女神は導国で詠う～")));
        Assert.Equal(("P3", "M3"), (koukaku.Support.PlayId, koukaku.Support.ModdingId));
        Assert.True(koukaku.Support.IsPlayable);
        // Its saves load on native AGE and the port alike (koukaku-save-load.steps, 2026-11-26).
        Assert.True(koukaku.PersistenceWritesEnabled);
        Assert.Null(koukaku.Persistence.ReadOnlyReason);
        Assert.Equal(320, koukaku.Persistence.ExpectedPackedSaveVersion);
        Assert.Equal([2169864, 88241, 433, 1, 1, 1], koukaku.Persistence.BankDimensions.ToArray());
        Assert.Null(koukaku.DirectSceneDiagnostics);
        Assert.Null(koukaku.DebugSceneLaunch);
        // The launcher's art for it comes from the publisher's site, as the other games' does.
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu14/banner/banner_eu14_a.jpg",
            koukaku.Launcher!.Banner!.Url.ToString());
        Assert.Equal("https://www.eukleia.co.jp/eushully/eu14/image/eu14_package.jpg", koukaku.Launcher.Cover!.Url.ToString());
        Ome.Engine.Vm.EngineBuild build = Ome.Engine.Vm.EngineBuilds.ForProfile(koukaku);
        Assert.Equal(("4.43B", "SYS4443"), (build.Id, build.AbiId));
        Assert.Same(Ome.Engine.Vm.EngineBuilds.Age443B, build);
        // 4.38D's text rules with 4.43B's two publisher changes (EngineBuilds.Age443B).
        Assert.Equal(Ome.Engine.Vm.EngineBuilds.Age438D.Rules.Text with
                     {
                         Ruby = Ome.Engine.Text.RubyRule.ShiftModesRaisedByTopPadding, LayoutPublishersGrowOnce = true,
                     },
                     build.Rules.Text);
    }

    [Fact]
    public void NormalKamidoriProfileInjectsNoHimegariState()
    {
        // A natural boot takes no profile state at all; only direct-scene and debug launches have any.
        GameProfileManifest kamidori = Assert.IsType<GameProfileManifest>(
            GameProfileRegistry.BuiltIn.Find("kamidori"));

        Assert.Null(kamidori.DirectSceneDiagnostics);
        Assert.Null(kamidori.DebugSceneLaunch);
        Assert.True(kamidori.PersistenceWritesEnabled);
    }

    [Theory]
    [InlineData("S4IC422", "姫狩りダンジョンマイスター", "himegari", true)]
    [InlineData("S4IC433", "神採りアルケミーマイスター", "kamidori", true)]
    public void AutomaticSelectionRequiresOneExactIdentityMatch(
        string revision, string title, string expectedProfile, bool writesEnabled)
    {
        SelectedGameProfile selected = GameProfileSelection.Resolve(
            [], new GameCatalogIdentity(revision, title));

        Assert.Equal(expectedProfile, selected.Profile.Id);
        Assert.Equal(GameProfileSelectionSource.Automatic, selected.Source);
        Assert.True(selected.IdentityMatched);
        Assert.False(selected.ProbeMode);
        Assert.Equal(writesEnabled, selected.PersistenceWritesEnabled);
    }

    [Fact]
    public void ExplicitCrossPairContinuesWithRequestedProfileAndRecordsMismatch()
    {
        SelectedGameProfile selected = GameProfileSelection.Resolve(
            ["--game", "kamidori"], HimegariIdentity);

        Assert.Equal("kamidori", selected.Profile.Id);
        Assert.Equal(GameProfileSelectionSource.Explicit, selected.Source);
        Assert.False(selected.IdentityMatched);
        Assert.False(selected.ProbeMode);
        Assert.True(selected.IsReadOnly);
        Assert.Equal("catalog/game identity mismatch", selected.ReadOnlyReason);
        Assert.StartsWith("explicit game mismatch: requested=kamidori", selected.MismatchDiagnostic);
        Assert.Contains("detected=S4IC422 / 姫狩りダンジョンマイスター", selected.MismatchDiagnostic);
        Assert.EndsWith("continuing with the requested game read-only", selected.MismatchDiagnostic);
    }

    [Fact]
    public void ExplicitGameUsesLastValueAndMatchesIdsCaseInsensitively()
    {
        SelectedGameProfile selected = GameProfileSelection.Resolve(
            ["--game", "himegari", "--game", "KAMIDORI"], KamidoriIdentity);

        Assert.Equal("kamidori", selected.Profile.Id);
        Assert.True(selected.IdentityMatched);
        Assert.Equal(GameProfileSelectionSource.Explicit, selected.Source);
    }

    /// <summary>The last <c>--game</c> wins. <c>--profile</c> names a player profile since 2026-11-24 (modding from a
    /// release spec §6.3), so it no longer names the game.</summary>
    [Theory]
    [InlineData(new[] { "--game", "himegari", "--game", "kamidori" }, "kamidori")]
    [InlineData(new[] { "--game", "kamidori", "--mod", "x", "--game", "himegari" }, "himegari")]
    [InlineData(new[] { "--profile", "kamidori", "--game", "himegari" }, "himegari")]
    public void TheLastGameOptionWinsAndProfileIsNotOne(string[] arguments, string expected)
    {
        Assert.Equal(expected, GameProfileSelection.RequestedGameId(arguments));
        Assert.Equal(expected, GameProfileSelection.Resolve(arguments, KamidoriIdentity).Profile.Id);
    }

    [Fact]
    public void ProfileAloneRequestsNoGame()
        => Assert.Null(GameProfileSelection.RequestedGameId(["--profile", "kamidori"]));

    [Fact]
    public void NoGameOptionRequestsNothing()
        => Assert.Null(GameProfileSelection.RequestedGameId(["--data-root", "x", "--probe"]));

    [Fact]
    public void ProbeModeRequiresAnExplicitAbiAndSuppressesWrites()
    {
        var missingProfile = Assert.Throws<ArgumentException>(
            () => GameProfileSelection.Resolve(["--probe"], HimegariIdentity));
        Assert.Contains("--probe requires --game <id>", missingProfile.Message);

        SelectedGameProfile selected = GameProfileSelection.Resolve(
            ["--game", "himegari", "--probe"], HimegariIdentity);
        Assert.True(selected.IdentityMatched);
        Assert.True(selected.ProbeMode);
        Assert.True(selected.IsReadOnly);
        Assert.Equal("probe mode", selected.ReadOnlyReason);
    }

    [Fact]
    public void UnknownAutomaticIdentityFailsBeforeRuntimeConstruction()
    {
        var error = Assert.Throws<ArgumentException>(() => GameProfileSelection.Resolve(
            [], new GameCatalogIdentity("S4IC999", "Unknown Game")));

        Assert.Contains("no game matches S4IC999 / Unknown Game", error.Message);
        Assert.Contains("pass --game <id> to choose a game explicitly", error.Message);
    }

    [Fact]
    public void AmbiguousAutomaticIdentityRequiresAnExplicitChoice()
    {
        var registry = new GameProfileRegistry([
            SyntheticProfile("first", HimegariIdentity),
            SyntheticProfile("second", HimegariIdentity),
        ]);

        var error = Assert.Throws<ArgumentException>(
            () => GameProfileSelection.Resolve([], HimegariIdentity, registry));

        Assert.Contains("multiple games match", error.Message);
        Assert.Contains("first, second", error.Message);
        Assert.EndsWith("pass --game <id>", error.Message);
    }

    [Theory]
    [InlineData("--game", "--game")]
    [InlineData("--game", "--game", "--probe")]
    [InlineData("--game", "--game", "himegari", "--game")]
    public void MissingExplicitGameIdIsRejected(string option, params string[] arguments)
    {
        var error = Assert.Throws<ArgumentException>(
            () => GameProfileSelection.Resolve(arguments, HimegariIdentity));

        Assert.Equal($"{option} requires a game id", error.Message);
    }

    [Fact]
    public void UnknownExplicitProfileIdIsRejectedWithoutChangingTheRequest()
    {
        var error = Assert.Throws<ArgumentException>(() => GameProfileSelection.Resolve(
            ["--game", "not-a-game"], HimegariIdentity));

        Assert.Contains("unknown game 'not-a-game'; available games: ", error.Message);
    }

    [Fact]
    public void IdentityHeaderReaderNormalizesPaddingAndDecodesCp932Title()
    {
        string path = Path.Combine(
            Path.GetTempPath(), "ome-profile-tests", Guid.NewGuid().ToString("N"), "SYS4INI.BIN");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var header = new byte[264];
            Encoding.ASCII.GetBytes("S4IC422 ").CopyTo(header, 0);
            Encoding.GetEncoding(932).GetBytes("姫狩りダンジョンマイスター").CopyTo(header, 8);
            File.WriteAllBytes(path, header);

            Assert.Equal(HimegariIdentity, GameCatalogIdentity.ReadSys4IniHeader(path));
        }
        finally
        {
            string directory = Path.GetDirectoryName(path)!;
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [Trait("Category", "Workspace")]
    [InlineData("himegari")]
    [InlineData("kamidori")]
    [InlineData("arterial")]
    [InlineData("verita")]
    [InlineData("bg0")]
    [InlineData("koukaku")]
    public void InstalledCatalogAutoSelectsExpectedProfile(string expectedProfile)
    {
        string path = Path.Combine(Paths.GameInstall(expectedProfile), "SYS4INI.BIN");
        Assert.True(File.Exists(path), $"installed-data profile gate requires {path}");

        GameCatalogIdentity identity = GameCatalogIdentity.ReadSys4IniHeader(path);
        SelectedGameProfile selected = GameProfileSelection.Resolve([], identity);

        Assert.Equal(expectedProfile, selected.Profile.Id);
        Assert.True(selected.IdentityMatched);
    }

    /// <summary>The numbered-save bank prefix is SYS4INI header data (+0xf0..+0x104), which
    /// engine_load_sys4ini_and_mount_append_catalogs reads into ctx+0xa0bb8; the profile copy must match.</summary>
    [Theory]
    [Trait("Category", "Workspace")]
    [InlineData("himegari")]
    [InlineData("kamidori")]
    [InlineData("arterial")]
    [InlineData("verita")]
    [InlineData("bg0")]
    [InlineData("koukaku")]
    public void InstalledSys4IniHeaderDeclaresProfileSaveBankDimensions(string game)
    {
        string path = Path.Combine(Paths.GameInstall(game), "SYS4INI.BIN");
        Assert.True(File.Exists(path), $"installed-data profile gate requires {path}");
        byte[] header = File.ReadAllBytes(path).AsSpan(0, 0x12c).ToArray();
        int[] savedPrefix = Enumerable.Range(0, 6)
            .Select(k => BitConverter.ToInt32(header, 0xf0 + 4 * k)).ToArray();
        int[] fullBanks = Enumerable.Range(0, 6)
            .Select(k => BitConverter.ToInt32(header, 0x114 + 4 * k)).ToArray();

        SelectedGameProfile selected = GameProfileSelection.Resolve(
            [], GameCatalogIdentity.ReadSys4IniHeader(path));

        Assert.Equal(selected.Profile.Persistence.BankDimensions.ToArray(), savedPrefix);
        Assert.All(Enumerable.Range(0, 6), k => Assert.True(fullBanks[k] >= savedPrefix[k]));
    }

    /// <summary>Verita's workspace gate runs only the tests its trait names, so it runs the two theories' cases
    /// itself.</summary>
    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "verita")]
    public void InstalledVeritaSelectsItsProfileWithItsBankDimensions()
    {
        InstalledCatalogAutoSelectsExpectedProfile("verita");
        InstalledSys4IniHeaderDeclaresProfileSaveBankDimensions("verita");
    }

    /// <summary>戦女神ZERO's workspace gate, likewise.</summary>
    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "bg0")]
    public void InstalledZeroSelectsItsProfileWithItsBankDimensions()
    {
        InstalledCatalogAutoSelectsExpectedProfile("bg0");
        InstalledSys4IniHeaderDeclaresProfileSaveBankDimensions("bg0");
    }

    /// <summary>魔導巧殻's workspace gate, likewise.</summary>
    [Fact]
    [Trait("Category", "Workspace")]
    [Trait("Profile", "koukaku")]
    public void InstalledKoukakuSelectsItsProfileWithItsBankDimensions()
    {
        InstalledCatalogAutoSelectsExpectedProfile("koukaku");
        InstalledSys4IniHeaderDeclaresProfileSaveBankDimensions("koukaku");
    }

    private static GameProfileManifest SyntheticProfile(string id, GameCatalogIdentity identity)
        => new(
            id,
            id,
            [identity],
            ["SYS4000"],
            "SYS4",
            "0.00A",
            "SYS4000",
            "SYSTEM4.BIN",
            metadataReferences: null,
            new GamePersistencePolicy(
                id,
                writesEnabled: false,
                readOnlyReason: "synthetic test profile",
                NativeSaveMagic.S4SD,
                gameId: id,
                expectedPackedSaveVersion: 300,
                new NativeSaveBankDimensions(1, 1, 1, 1, 1, 1)));
}
