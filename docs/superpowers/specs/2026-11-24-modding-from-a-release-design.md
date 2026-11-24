# Modding from a release design

**Date:** 2026-11-24
**Status:** proposed. The user decided its four questions on 2026-11-24 (§15), and an independent agent review of
the same day checked it against the code; §16 keeps what the review changed. Not yet accepted.
**How it was decided:** the backlog items "Modding from a release" and "Toolchain packaging for exported ports"
(`docs/backlog.md`, "Mod loader and modding"). The site's usability review found the first one (site plan §10.3,
finding 10, in the `ome-site` repository: "a release ships the mod commands, and the guide then starts from a
release"). The user asked for it to be fixed on its own, once the site's design and workflow had settled
(2026-11-22: "it's been an ongoing 'issue' that you need to repo to perform modding"). They asked for this design on
2026-11-24 ("Go ahead and start speccing out modding from a release").
**Checked against:** `e6b92761` on `develop`, and the release workflow and packagers at that commit. Code references
name files and members, not lines. Paths are relative to `age-reimpl/`; a short path is under `engine/`.
**Direction:** `docs/remake-architecture-and-roadmap.md`: an open engine with first-class modding, whose runtime is
C# and whose Python tools are the developers' offline chain. It also follows the rule the user set on 2026-11-16:
the engine knows nothing about a game, and a game's facts live in its base mod.
**Builds on:**
- the mod loader design (`docs/superpowers/specs/2026-11-03-mod-loader-design.md`);
- the Lua scripting design's base mods (`docs/superpowers/specs/2026-11-10-lua-scripting-design.md`, §9);
- the launcher design (`docs/superpowers/specs/2026-11-15-launcher-design.md`): OME home, profiles, the shared
  library, the engine's command line and the captured `engine.log`;
- the NativeAOT engine library (`docs/platform-portability.md`, "Fewer files in a package").

**Not verified.** Each is work for M0 (§13):

- that the mod tool publishes under NativeAOT for `win-x64` and `linux-x64`, and how large it is. The engine library
  already compiles `Ome.Engine` (with `Ome.Engine.Devices`), `Ome.Engine.Vm`, `Ome.Engine.Frontend`, `Ome.Script`,
  `Ome.Mods`, `Ome.Scripting.Lua` and Tomlyn under NativeAOT. `Ome.Launcher.Core` is not marked
  `IsAotCompatible`, and the commands moved out of `Ome.Cli` have not been compiled that way;
- that the package smoke's synthetic install can carry a loose script for a fragment to patch (§12, gate 2). The
  smoke's install (`tools/package_linux_x64.py`, `write_smoke_home`) holds one archive and no script;
- that the C# listing in `locate` can print each operand as `tools/locate_page.py` does (§7.3);
- whether the engine can be asked to quit cleanly when `start` is stopped with Ctrl+C (§8);
- that `base_mod_build.py` can write each of Kamidori's symbolic dimensions as a number (§5.1).

## 1. Purpose and scope

### 1.1 The problem

A release holds two programs, the engine (`OME`, `OME.exe`) and the launcher (`OME-Launcher`). A player needs
nothing else. A modder needs a source checkout, because:

1. **The mod commands are not in a release.** `omp check`, `omp build`, `omp text`, `oms-patch`, `oms-decompile`,
   `oms-compile` and `picture` are commands of `Ome.Cli`, and no workflow publishes `Ome.Cli`. Outside a checkout
   it cannot start at all: it computes the conventional game root from `Paths.GameInstall` or `Paths.GameDir`
   before it looks at `--game-root`, then loads `build/opcodes.json`. `Paths.Repo` throws when no checkout lies above
   the program (`Ome.Engine/Sys4/Paths.cs`, `FindRepo`).
2. **An exported engine cannot compile most mods.** A fragment or a new script compiles against the game's OMS
   modules (`oms/<game>/adv.oms`), its generated globals (`build/games/<game>/globals.json`) and its patch points
   (`build/games/<game>/patch-points.json`). `ModLinkBase.ForGame` finds them through `Paths` and uses none when
   `Paths` finds no checkout. The port then warns "no repository toolchain" (`godot/Main.Mods.cs`). A fragment that
   `use`s `<game>.globals` fails with "unknown module", and a named patch point fails with "no patch-point registry
   is loaded" (`OmsCompiler`). The guides say so (`docs/modding/cli.md`, "Outside the repository"). This is the
   backlog's "Toolchain packaging for exported ports".
3. **The page locator is a Python tool.** `tools/locate_page.py` reads the page map and disassembles around the
   wait with the Python loader (`sys4load.py`), from the scripts that `extracted/` holds.
4. **The guides start from a checkout.** "Your first mod" and "Translating" list as prerequisites:
   - the .NET SDK, Python 3.11 and Godot;
   - `dotnet build engine/OmeEngine.sln`;
   - `globals_build.py` and `patch_points_build.py` runs.

   They run every command from `age-reimpl/`, as `dotnet run --project engine/Ome.Cli -- …`, `.\run-godot.cmd` and
   `py -3.11 -X utf8 tools/…`. They expect the game at `games/<game>/` beside the repository. Nine guides use the
   link text `games/README.md`, a file's path.

### 1.2 What this design delivers

- **The game's toolchain in its base mod** (§5). The OMS modules and the globals the compiler reads move into
  `mods/<game>-base/`. The engine and every tool read them from the shipped base mod, inside a checkout or not.
  This closes "Toolchain packaging for exported ports".
- **A mod tool in every release** (§6). It is a console program, `OME-Tool`, beside the engine and the launcher.
  Its commands are `Ome.Cli`'s mod commands, under the same names and with the same options. It runs with no
  checkout and no SDK.
- **The page locator in the tool** (§7): `OME-Tool locate`, reading the page map where the engine wrote it.
- **A start for authors** (§8): `OME-Tool start` runs the engine with the authoring flags and prints its output in
  the terminal.
- **Guides that start from a release** (§9). Each step is written for a release. A from-source step is marked as
  such. `cli.md` keeps the one table that maps the two.

When this is built, both backlog items are deleted.

### 1.3 Not in this design

- **A GUI for authors.** That is the backlog's "A devkit for mod authors". This design gives the devkit the commands
  it would drive.
- **The developers' Python chain.** The registries' builds, the driver (`tools/drive.py`), `native_scratch.py` and
  the reverse-engineering tools stay as they are. A modder needs none of them (§9.4).
- **Data beside the library.** Everything this design adds to the engine is embedded, as the base mods are today
  (§5.5). Where data may sit beside the library is the backlog's "Data beside the program", which is the user's to
  decide.
- **macOS.** No release is built for it.
- **The site.** It copies the guides from the reference bundle. Nothing on the site changes beyond what the bundle
  carries, except perhaps the downloads page naming `OME-Tool` (a note for the site's plan).

## 2. What the code gives to build on

- **Embedded data.**
  - `Ome.Engine` embeds the game manifests (`Profiles/*.json`) and the opcode table (`build/opcodes.json`), read by
    `OpcodeTableJson.LoadEmbedded` and `OpcodeContracts.LoadEmbedded`.
  - `Ome.Mods` embeds all of `mods/`, which is the three base mods and `ome-records`, read by `ShippedPackages`.
  - `ShippedPackages.BaseLayoutOf(profile)` finds a game's base mod by the name `<profile>-base`.
- **The base mod already carries the patch points.** `layout/points.json` has the form of `patch-points.json` and
  differs from the build copy only in its `note` line, in all three games. `base_mod_build.py` copies it "so an
  exported build carries them". `ModPackage.ReadBaseLayout` reads it as `OmsPatchPoints`, the type the compiler
  takes, and `ModLuaLinker` falls back to it. The OMS compiler does not.
- **The base mod's layout is not the compiler's globals.** `layout/globals.json` gives each exported name its bank,
  address and dimensions, and no type, columns or tables.
- **The package format grows by version.**
  - An unknown top folder is a note today ("outside the package layout and is ignored", `ModLayout.Classify`).
  - Each folder added so far came with a format version that `Classify` checks: `text/` and `fonts/` with
    `ModManifest.LanguageOmpVersion`, `lua/`, `events/` and `layout/` with `LuaOmpVersion`.
  - `layout/` may hold `globals.json`, `points.json` and `exports.txt`, and nothing else. The base mods are
    `omp = 4`.
- **What the compiler's globals loader reads.**
  - `GameModule.LoadGlobals` opens a file path. It reads `cells`, an object keyed by cell, and takes each named
    cell's `name`, `type`, `address` and `columns`. From `tables` it takes `name`, `records` and `fields`.
  - It reads no old name: not a cell's `aliases`, nor a table's or a field's. So the rename rule (`exports.txt`,
    `base_mod_build.py --check`) protects the Lua layout only, and a renamed global breaks a fragment that names it.
  - Its type pattern, `^(int|string|float)((?:\[\d*\])*)$`, skips a type written any other way.
- **The auto map adds nothing the compiler reads.**
  - A checkout's `build/games/<game>/globals.json` merges the game's inferred map (`global-var-map.json`, a
    workspace input derived from the game's scripts). Its cells have no names, and `LoadGlobals` skips a cell
    without a name.
  - On 2026-11-24, for all three games, the named cells and the tables of the build copy equalled those of
    `globals_build.curated_json`, the registry alone: 469, 642 and 369 named cells. So the registry alone gives the
    compiler everything it reads, with no game file and no CI-only input.
  - The loader takes all of Himegari's and Arterial's named cells, but only 598 of Kamidori's. The other 44 are typed
    as `int[1000x100]` or with a symbolic dimension, so no fragment can name them today.
- **The idioms need the scripts the modules run.**
  - `OmsIdioms.Build` compiles each source module with every library sub linked (`linkAll: true`). So each script a
    module `run`s must be in `<game>.scripts`, which comes from the install's catalog. Himegari's `adv.oms` runs
    `HISTORY`, `MENU` and `HIDEWIN`.
  - A missing one is a compile error, which `ForInstall` turns into an `InvalidDataException`, and the link stops.
  - The tests' synthetic installs list these scripts (`SyntheticCatalog.ScriptsTheModulesRun`).
- **Sizes.**
  - The OMS modules are one file per game, `oms/<game>/adv.oms`: 34.8 KB, 41.2 KB and 85.4 KB.
  - The curated globals, with every registry key, are 745 KB, 783 KB and 338 KB. The build copies, with the auto
    map, are 2.9, 5.9 and 7.0 MB.
- **The link base is shared.** The engine, `omp` and the launcher's Link check link through
  `ModLinkBase.ForGame`, so they agree (launcher spec §5.9). Its `ToolchainStamp` hashes the toolchain files it read.
- **Profiles resolve without a checkout.**
  - `RunResolver.Resolve` reads `--data-root`'s `profile.toml` for game, install and overlays. `Ome.Cli` already
    calls it.
  - Its `Canonical` maps `--profile` to `--game`. Once a profile file names the game, it refuses `--game`,
    `--profile`, `--game-root`, `--overlay-root`, `--allow-bmp-as-agf` and `--probe` (`RefusedWithProfileFile`).
  - `Ome.Launcher.Core` resolves OME home (`OmeHome.Resolve`, including the portable `data/` beside the launcher)
    and the profiles in it (`ProfileStore`). It runs the checks (`ProfileChecks.Run`) and starts the engine
    (`EngineCommands.For`, `EngineLaunch.Run`).
  - `Ome.Launcher.Core` has no Avalonia type. It references `Ome.Engine`, `Ome.Engine.Frontend`, `Ome.Mods` and
    `Ome.Scripting.Lua`.
- **A release engine's output can be read.**
  - The release engine has no console on Windows. Godot gives a console wrapper only to debug exports.
  - The launcher captures the engine's standard output and error into `diagnostics/engine.log` (`EngineLaunch.Run`).
    Its M0 measured that a release engine's output reaches a .NET parent, once `flush_stdout_on_print` is on
    (launcher plan, M0 measure 6).
  - `EngineCommands.For` puts the engine program's own arguments (`EngineProgram.Arguments`) before `--`, and the
    run's after it.
- **Page maps in an export.**
  - `Main.DefaultPageMapPath` writes `build/games/<game>/page-map-<SCENE>.jsonl` in an editor build (`TOOLS`).
  - An export writes `<profile root>/diagnostics/page-maps/page-map-<SCENE>.jsonl`, on every run but `--selftest`.
  - With no `--data-root`, the profile root is `user://games/<namespace>/` (`GamePersistencePolicy`).
- **No C# listing.** `oms-decompile --level raw` prints `@op(...)` lines with labels only at jump targets. No C#
  command prints each instruction's offset, as `locate_page.py` does (`0x00871: wait-for-input 0x0`).
- **Programs and their version.** `Directory.Build.targets` stamps the `OmeVersion` assembly metadata (`git
  describe`) into a project that sets `OmeProgram=true`, and `EngineIdentity` reads it. `BUILD-INFO.json` names the
  commit and the toolchain, not the version.
- **The release's shape.**
  - The top folder holds the engine program, `OME.pck`, the launcher's files, the notices, the repository's
    `README.md`, `BUILD-INFO.json` and `SHA256SUMS`.
  - `data_OME_<platform>_x86_64/` holds exactly 11 files: the engine library, the .NET notices and FFmpeg. The
    packagers refuse anything else there (`package_linux_x64.py`, and `package_windows_x64.py`'s `verify_export`).
  - The Windows archive is cross-packaged on Linux and checked structurally (`verify_windows_native.py`). The Linux
    package runs a smoke from a copy outside the repository (`released_copy`).

## 3. Decisions

### 3.1 The user's

- A modder should not need the repository (2026-11-22, quoted above).
- The guides start from a release, with the from-source steps marked as such (the backlog item's "done when").
- Tag only a commit CI has passed, and push only on the user's word (`AGENTS.md`).
- The engine stays game-agnostic, and a game's facts go in its base mod (2026-11-16).
- Their answers to this design's questions, 2026-11-24 (§15):
  - the tool is `OME-Tool`;
  - `adv.oms` moves into the base mod;
  - the examples are linked, not shipped;
  - `--profile` names a player profile, and stops being `--game`'s old name: "I think it's fine to deprecated the old
    usage for the new one, unless you disagree. We're still pre 1.0 and I don't think anyone's using this yet except
    me".

### 3.2 This design's

1. **A console program of its own.** Both programs in a release are windowed on Windows, and neither has a console
   there. Commands that print reports, and whose exit codes scripts test, belong in a console program. An
   `AttachConsole` call in a windowed program prints into the console, but the shell does not wait for it (launcher
   plan, M0 measure 5).
2. **One command set, two ways to run it.** The tool's commands keep `Ome.Cli`'s names and options. Their code moves
   out of `Ome.Cli` into code both programs call. So `OME-Tool X` from a release and
   `dotnet run --project engine/Ome.Tool -- X` from a checkout print the same, and `Ome.Cli X` still works.
3. **One link base for a checkout and a release.** The toolchain moves into the base mod (§5), and the repository
   walk leaves the linker. A program inside a checkout then links as a release does. This removes two problems:
   - a fresh clone has no compiler globals until `globals_build.py` has run;
   - a package copied into a checkout picks up that checkout's files, which is why the smoke copies the package out
     first.
4. **The compiler's globals come from the registry alone.** The base mod gets the curated form, cut down to what the
   loader reads, with the old names (§5.1). The auto map stays a checkout's input for the Python tools and the global reference docs, and
   never enters a release.
5. **The locator reads where the engine writes.** Where the map goes depends on the engine's build, not on where
   the tool runs (`Main.DefaultPageMapPath`). `OME-Tool locate` reads the profile's `diagnostics/page-maps/`, which
   an exported engine writes. `Ome.Cli locate` also reads the editor build's `build/games/<game>/` (§7.2).
6. **Python stays the developers' chain.** No step a modder takes needs it. `tools/locate_page.py` keeps working for
   agents until `locate` is shown to print the same (§11).
7. **NativeAOT where it builds.** The tool is published as the engine is, as one native program, if M0 finds no
   blocker. If it finds one, the fallback is a self-contained single file, as the launcher is.

## 4. Terms

- **Toolchain:** what a fragment or new script compiles against:
  - the game's OMS modules: `<game>.globals`, `<game>.adv`, and `<game>.scripts`, which is built from the install's
    catalog;
  - the idioms built from them;
  - the game's patch points.
- **Mod tool:** `OME-Tool`, the program this design adds.
- **Profile option:** how a command names a player profile: `--data-root <profile root>` or `--profile <id>` (§6.3).
- **Game options:** `--game`, `--game-root` and `--overlay-root`, as today.
- **From source:** a step that needs a checkout of this repository, and is marked as such in a guide.

## 5. The game's toolchain in its base mod

### 5.1 What moves

| Today | Becomes | Kept by |
|---|---|---|
| `oms/<game>/adv.oms` | `mods/<game>-base/oms/adv.oms` | hand, as the base mod's `events/` and `lua/` are |
| `build/games/<game>/globals.json` (as the compiler's input) | `mods/<game>-base/oms/globals.json` | `base_mod_build.py --build`, from the registry alone (§3.2, item 4) |
| `build/games/<game>/patch-points.json` (as the compiler's input) | `mods/<game>-base/layout/points.json`, already there | `base_mod_build.py --build` |

- **`oms/` names the modules.**
  - `oms/globals.json` is `<game>.globals`.
  - Each `oms/*.oms` declares its own name (`module himegari.adv`), which must start with `<game>.`, as
    `GameModule.LoadSources` requires today.
- **What `oms/globals.json` holds.** It holds what the loader reads, plus the old names, under a generated note:
  - `cells`: `name`, `aliases`, `type`, `address` and `columns`;
  - `tables`: `name`, `aliases`, `records`, `fields` and the fields' old names.

  `base_mod_build.py` writes it from the registry alone, as `globals_build.curated_json` reads it.
- **The loader changes** in three ways (§2):
  - It reads the package's bytes, not a path. `LoadSources`, which reads a folder on disk, gains the same form, and
    so do `ModLinkBase.ForInstall`'s inputs and its stamp.
  - It takes the old names, so a renamed global or table still compiles under its old name until the base mod's
    major version rises (§5.2).
  - Each type is written in the form it reads. `base_mod_build.py` writes Kamidori's `int[1000x100]` as
    `int[1000][100]`, and a symbolic dimension as its number. It reports any named cell it cannot write. So
    Kamidori's 44 cells become nameable.
- **`oms/examples/` stays** where it is. It holds example mods, not a game's toolchain.
- **`build/games/<game>/globals.json` stays.** The Python tools, `data-export`, `script-index` and `save-diff` read
  it. It stops being the compiler's input.
- **The `oms/<game>/` folders go, and `Paths.OmsModules` with them.**
  - `GameModule.LoadSources` returns no module for a missing folder. So a reader this design missed would quietly
    lose `<game>.adv`; with `Paths.OmsModules` deleted, it fails to build instead.
  - `modding_games_build.py` lists a game's modules from the base mod.
  - `validate.py` runs its `oms-idiomatic-rebuild` gate when the base mod has `oms/*.oms`, and `test_validate.py`
    follows.
- **A new game** (`docs/adding-a-game.md`) writes its ADV module into `mods/<id>-base/oms/`. Today that step comes
  before the base mod's. The plan either starts the base mod at that step, or moves the module to the base mod's
  step.

### 5.2 The package format

- **A base mod may hold `oms/`, from a new format version.**
  - `oms/` needs `omp = 5`, as each folder before it needed its own version, and the base mods move to it.
  - `ModLayout.Classify` reads `oms/globals.json` and `oms/*.oms` in a package whose layout is a base mod's
    (`layout/globals.json`).
  - In any other package, `oms/` keeps today's note that it is ignored.
  - A mod that offers OMS modules to other mods is not part of this design (§11).
- **The linker reads the shipped base mod.**
  - It reads the copy the engine ships (`ShippedPackages`), not an installed package of the same id. The toolchain
    is part of the program, as the opcode table is.
  - An installed base mod of that id still replaces the shipped one in the stack, as today, but not as the
    compiler's input. When its version differs from the shipped one's, the link report names both.
- **Old names.** A module's names are part of what mods depend on, as the layout's names are.
  - The rename rule (`exports.txt`, `base_mod_build.py --check`) extends to `oms/globals.json`, whose old names the
    loader now takes (§5.1).
  - For `adv.oms`'s own names (its subs, templates and enums), the plan extends the check.

### 5.3 The link base

- **`ModLinkBase.ForGame` reads the toolchain from the shipped base mod.** It no longer reads it from `Paths`, and
  loses its `repositoryToolchain` result. The port's "no repository toolchain" warning goes.
- **A toolchain that does not build fails only what needs it.**
  - Today a module set that does not build stops every link (§2). After the move it always loads, so an install
    missing a script a module runs would refuse every mod.
  - Instead, the link base keeps the failure. A fragment or new script that compiles reports it, while asset-only
    mods, whole-script replacements and stored plans that match still link.
  - The launcher's Link check shows it as a warning when no mod needs it.
- **A game with no base mod** links as an export does today, with only `<game>.scripts`. The link report says so
  once.
- **The stamp is kept.** `ToolchainStamp` hashes the base mod's `oms/` files and `layout/points.json`, as it hashes
  the repository's files today.
- **`Ome.Cli`'s own compiles share the link base.** That is every caller of its `OmsModules()`: `oms-compile`,
  `oms-decompile`, `oms-patch`, `oms-append`, `oms-rebuild` and `oms-roundtrip`. So every compile in a checkout sees
  what the engine sees.
- **Tests** that read `Paths.OmsModules`, `Paths.GlobalsJson` or `Paths.PatchPointsJson` for a link base move to the
  shipped base mod. Among them are `OmsCoreTests`, `SyntheticCatalog.ScriptsTheModulesRun` and the `ForInstall`
  tests. A synthetic link base keeps `ModLinkBase.ForInstall`'s inputs, in their form from §5.1.

### 5.4 What changes in a checkout

- **Editing `adv.oms`** takes effect at the next build. `dotnet run` and `run-godot.cmd` build first, and they already
  do so for a base mod's Lua.
- **Renaming a global or a table** already needs `globals_build.py --build` and `base_mod_build.py --build`
  (`AGENTS.md`). The second now also writes `oms/globals.json`, and the core gate checks it is current.
- **A fresh clone** links fragments with no Python run. The checked-in base mods carry everything.

### 5.5 Data beside the program

This adds about 360 to 440 KB to the base mods, which are already embedded: 161 KB of modules, and 200 to 280 KB of
globals, written compact or indented. The rule stays that all data is inside the library. If the user later moves the
base mods beside it, the toolchain moves with them in one step. That is a reason to keep the toolchain in the base
mod rather than embed it on its own.

## 6. The mod tool

### 6.1 The program

- **Project.** `engine/Ome.Tool/`, a console program in `OmeEngine.sln`. Its assembly name gives the program
  `OME-Tool` (`OME-Tool.exe` on Windows).
- **No repository, and no checkout defaults.**
  - It never reads `Paths`, in a checkout or not. So `dotnet run --project engine/Ome.Tool -- X` behaves as the
    released `OME-Tool X`.
  - The checkout's defaults, the install at `games/<game>/` and the page maps under `build/games/<game>/`, stay
    `Ome.Cli`'s.
  - Its opcode table and contracts are the embedded ones.
  - `ResourceMap` and `Sys4ScriptProvider` fall back to `Paths.GameDir` when given no asset store, so the tool
    always passes one.
  - A test fails on any use of `Paths` in the tool's code, and `EngineBuildTests.AllowedReaders` names the moved
    command code.
- **Libraries.**
  - It references the projects the engine library holds: `Ome.Engine` (and through it `Ome.Engine.Devices`),
    `Ome.Engine.Vm`, `Ome.Engine.Frontend`, `Ome.Script`, `Ome.Mods` and `Ome.Scripting.Lua`. `omp text` needs
    `Ome.Engine.Frontend` for the shell's strings.
  - It also references `Ome.Launcher.Core`, for profiles and the start. That project is not marked
    `IsAotCompatible` today (M0).
  - It does not reference `Ome.Export`, whose SQLite needs a native library.
- **Shared code.** The commands' code moves out of `Ome.Cli`'s `Program.cs`, `OmpCommands` and `OmpTextCommand`
  into one place both programs call. Where that code lives, a library or `Ome.Tool` itself, is the plan's choice.
- **`--version`** prints the version and commit stamped into the program. The project sets `OmeProgram=true`, so
  `Directory.Build.targets` stamps it as it does the engine and the launcher (§2).
- **UTF-8 output.** The tool writes UTF-8, so Japanese text and the engine's lines print whole on any Windows
  console. Neither `Ome.Cli` nor `Ome.Launcher.Core` sets the encoding today.

### 6.2 Commands

| Command | As in `Ome.Cli` | Notes |
|---|---|---|
| `omp check`, `build`, `export`, `explain`, `lua-stubs` | yes | link through `ModLinkBase.ForGame` (§5.3) |
| `omp text extract`, `check`, `stats`, `po-export`, `po-import`, `import-scripts` | yes | contracts from the embedded registry; `import-scripts`' options for the `--from` install are renamed (§6.3) |
| `oms-patch --fingerprint`, `--source`, `--plan` | yes | |
| `oms-decompile`, `oms-compile` | yes | modules from the link base |
| `picture` | yes | `ResourceMap`, in `Ome.Engine`; writes to `--output-dir`, else `pictures/` in the current folder (`Ome.Cli` keeps `build/games/<game>/pictures`) |
| `locate` | new (§7) | `Ome.Cli` gets it too |
| `start` | new (§8) | release only: it starts the engine program beside the tool |

- **Usage.** A command the tool lacks (`sweep`, `trace`, `data-export`, …) is refused with the list of the tool's
  own.
- **Exit codes.** 0 is success, 1 a failed check, and 2 a usage or link error. Some `oms-*` commands throw an
  `ArgumentException` for a missing option, and today `Ome.Cli` crashes on it. Both programs catch it at the top and
  exit 2 with its message.

### 6.3 Naming the game and the profile

- **A profile** is the usual way, since the launcher makes one for every game a player adds. A command takes either:
  - `--data-root <profile root>`, which exists and is read through `RunResolver`; or
  - `--profile <id>`, a profile's id resolved through OME home as the launcher resolves it, with `--home DIR` as in
    the launcher.

  Either one gives the game, the install, the overlays and the profile's `[mods]`, so `omp check` checks what the
  profile would run (launcher spec §5.7).
- **A profile's trust.**
  - The lookup by id honours the profile's trust as the launcher does (`ProfileTrust`). A profile that needs review
    is refused with the launcher's reason.
  - Inside a checkout, `Ome.Cli`'s OME home is the user's real one (`OmeHome.Implied`). So `--profile <id>` there
    runs that profile's install and mods, which is what it names.
- **`--profile` stops being `--game`'s old name** (the user, 2026-11-24, §3.1). Every place that takes it for the
  game drops that meaning in the same change:
  - C#: `GameProfileSelection` (the engine and `Ome.Cli`), `RunResolver` (its `Canonical` mapping and
    `RefusedWithProfileFile`), `SaveDiffCommand` and `SaveInfoCommand` (which check game options before
    `RunResolver`), and `picture`'s option skipping in `Ome.Cli`;
  - `run-godot.ps1`'s `-Profile` alias of `-Game`;
  - Python: `tools/paths.py`, `validate.py`, `text_geometry_compare.py` and `tools/frida/capture_gdi_fonts.py`;
  - their tests (`GameProfileSelectionTests`, `RunResolverTests`, `tools/test_paths.py`) and their docs.
- **An old command line fails plainly.**
  - In `OME-Tool` and `Ome.Cli`, an id that names no player profile is refused. The message says where the tool
    looked and that the game is `--game`.
  - The engine runs a profile from its folder alone (launcher spec §2), so it keeps `--data-root`. Given
    `--profile`, it refuses it and names `--data-root` and `--game`.
  - `tools/paths.py` parses with `parse_known_args`, so a dropped alias would pass through unread, and a tool with
    no parser of its own would run Himegari. `paths.py` refuses `--profile` with a message instead.
- **`omp text import-scripts` under a profile.** Its `--overlay-root` and `--allow-bmp-as-agf` describe the `--from`
  install, and `RunResolver` refuses both once a profile names the game. They become `--from-overlay-root` and
  `--from-allow-bmp-as-agf`, so the import runs under a profile.
- **Game options without a profile** still work: `--game <game> --game-root <install>`, with `--overlay-root` where
  needed. `OME-Tool` requires `--game-root` without a profile. `Ome.Cli` keeps its default, `games/<game>/` beside
  the repository.
- **Mods.** `--mod PATH` adds a folder or `.omp` to what the profile lists, as the engine's `--mod` does.

### 6.4 In the release

- **Published per platform** by the engine's jobs, since NativeAOT builds for the platform it runs on.
  `windows-engine` builds `win-x64`, and `linux-engine` builds `linux-x64` in its manylinux container.
- **Placed at the top of the archive**, beside `OME` and `OME-Launcher`. The data folder's rule of 11 files is
  unchanged.
- **What changes for it:**
  - `release-build.yml`: the engine jobs publish and upload it, and the release jobs take it in.
  - `publish_engine.py`, or a script beside it: publishes the tool and checks its file list, as it checks the
    library's.
  - `tools/build-linux-x64.sh` and `tools/build-windows-x64.sh`: stage it beside the engine.
  - `package_linux_x64.py`: its file list and glibc 2.28 check, and the smoke's new steps and markers
    (`SMOKE_MARKERS`).
  - `package_windows_x64.py` (`verify_export`) and `verify_windows_native.py`: its file list, and its imports, which
    may be only what the engine library's may.
  - `publish_github_release.py`: the artifact lists, the markers, the `WINDOWS-VERIFICATION.json` schema it
    validates, the `SHA256SUMS` check and the release notes.
  - **Notices.** The `PublishPackageNotices` target puts the .NET runtime notices beside a program unless
    `OmePackageNoticeFolder` is set, so the tool sets it and adds no top-level file. The notices check covers the
    tool's packages, LuaCSharp and Tomlyn among them.
  - **The README.** The archive's README is the repository's `README.md` (`PROJECT_FILES`). It names the tool and the
    guides' address.

## 7. The page locator in a release

### 7.1 Where the map is

The locator HUD (F2), the copy (F3) and the page map already work in an export (§2). What a release lacks is the
command that resolves a locator.

### 7.2 `locate`

```text
OME-Tool locate SC0000 3 [--context N] <profile option>
OME-Tool locate SC0000@0x8a6 [--context N] <profile option>
```

- **By page** (`SC0000 3`, or `SC0000 P003` as F3 copies it; `locate_page.py` takes the number only), it reads the
  map from:
  - `--map FILE`, when given;
  - else the profile's `diagnostics/page-maps/`;
  - else, with game options and no profile, the bare engine's `user://games/<namespace>/diagnostics/page-maps/`.

  `Ome.Cli locate` also reads the checkout's `build/games/<game>/`, where an editor build writes, when no profile is
  named. Where the map goes depends on the engine's build (`#if TOOLS`), not on where the tool runs. An exported
  engine run from a checkout (`drive.py port start --package`) writes the profile's maps. Each run prints the map
  file it read, so a stale map shows.
- **By site** (`SC0000@0x8a6`, the `wait` or `text` part of a locator), it needs no map.
- **Missing pages.** A page the map does not hold, or a missing map, is refused with where it looked. The tool never
  guesses a page from the script (`AGENTS.md`, "Runtime page-location handoff").
- **Output.** It prints what `locate_page.py` prints:
  - the page's header;
  - the page start, last text and wait;
  - the call stack and the page's text;
  - a listing of `--context` instructions (default 8) before and after the wait, with the wait marked `>>>`.
- **The script it lists is the one the engine ran.**
  - That is the install's script, from the catalog with the profile's overlays, unless a mod in the profile's
    stack, or a `--mod`, patches it.
  - Then `locate` lists the linked script and says which mods changed it. A fragment names the base script's
    offsets, so it adds that a run without those mods gives them.

### 7.3 The listing

The listing is the new code. Each line holds an instruction's dword offset, its mnemonic from the embedded opcode
table, and its operands, in the form the guides show:

```text
    0x00871: wait-for-input 0x0
    0x00874: end-text-line 0x0
>>> 0x00877: reset-adv-text-layout 0x1
```

The mnemonics come from the same registry as the Python loader's (`vm-map/opcodes.toml`). Operands follow
`locate_page.py`'s text where the two can agree. Gate 6 compares them.

## 8. Running a mod: `start` and the launcher

There are two ways, and the guides give both.

- **Through the launcher**, for trying a mod as a player would:
  - add the mod's folder or `.omp` to the library, enable it in the profile, and press Play;
  - the launcher's checks link it first, and the engine's output goes to `diagnostics/engine.log`.

  This works today, with no change.
- **`OME-Tool start`**, for the authoring flags:

  ```text
  OME-Tool start <profile option> [--mod PATH ...] [--lua-dev] [--lua-trace] [--lua-reload]
                 [--lua-errors halt] [--text-log] [--locator-hud] [--engine-arg ARG ...]
                 [-- other engine flags]
  ```

  - It runs the profile's checks (`ProfileChecks.Run`). The `--mod` packages join the Link check, which today checks
    the profile's own mods only.
  - It starts the engine program beside the tool with the profile's command (`EngineCommands.For`), plus these
    flags.
  - It prints each line of the engine's output in the terminal as it comes, in UTF-8. It also writes each line to
    `diagnostics/engine.log`, between the launcher's `[launcher]` lines.
  - It returns the engine's exit code.
  - A failed check is printed and stops the start, as in the launcher.
- **Engine flags.**
  - A flag `start` does not name goes after `--` to the engine unchanged. `--data-root` is refused there, as the
    launcher refuses it.
  - A Godot flag, such as `--headless` or `--rendering-driver`, must come before `--`. It is given with
    `--engine-arg`, which feeds `EngineProgram.Arguments`, as the launcher's own `--engine-arg` does.
- **Ctrl+C** stops the engine.
  - On Linux, the tool passes the interrupt on.
  - On Windows, the engine runs with no window of its own and with redirected streams, so the tool ends it.
  - The log's last `[launcher]` line says the engine was stopped. Whether it can be asked to quit cleanly instead is
    for M0 to find out.
- **What the launcher's code gains.** `EngineLaunch.Run` gains a line callback, the extra mods and a way to stop the
  engine. `start` is the launcher's start, shown in the terminal.
- **From source,** `.\run-godot.cmd` stays the way to run the port. `start` needs the engine program beside the
  tool, which a checkout does not have.

## 9. The guides

### 9.1 The shape

- **What you need.** Each how-to guide starts from:
  - a release: `OME-windows-x64.zip` or `OME-linux-x64.tar.gz`, unpacked anywhere;
  - the game, installed anywhere;
  - a profile for it, made in the launcher.

  The guide then gives the steps.
- **Commands** are written as `OME-Tool <command> … <profile option>`, run in a terminal in the release's folder.
- **The from-source mapping lives in `cli.md` alone**, in one table:

  | From a release | From source |
  |---|---|
  | `OME-Tool X` | `dotnet run --project engine/Ome.Tool -- X` from `age-reimpl/`, or `Ome.Cli X` |
  | `OME-Tool start …` | `.\run-godot.cmd` and its switches |
  | the profile's `diagnostics/page-maps/` | `build/games/<game>/` |
  | `--game-root` required without a profile | defaults to `games/<game>/` beside the repository |
  | the release's prerequisites | the .NET SDK (`global.json`), Godot 4.7 .NET, and one `dotnet build engine/OmeEngine.sln` |

- **From-source steps.** Where a step has no release form, the guide marks it with a line that starts
  "**From source:**". The guides' other steps carry no from-source text.

### 9.2 What each guide loses

- **Gone from every guide:**
  - the prerequisites of a checkout: .NET SDK, Python, Godot, and the solution build;
  - the registry builds (`globals_build.py`, `patch_points_build.py`): the base mods carry what they wrote;
  - "Commands run from `age-reimpl/`";
  - the game at `games/<game>/` as an expectation.
- **`cli.md`:**
  - its "Outside the repository" section, which §5 makes untrue;
  - its port flags and `run-godot` switches, which become the `start` table plus the from-source row.
- **`troubleshooting.md`:** its "Outside the repository" entry, and the fresh-clone entry about missing generated
  globals.
- **Paths into the repository:**
  - `oms/<game>/adv.oms` becomes the module's name, `<game>.adv`, with a link to the file in the base mod;
  - `oms/examples/mods/` and the driver's scenarios stay as links. On the site they are links into the repository
    on GitHub, as the site already makes them.
- **The game guides** (`docs/modding/games/<game>.md`):
  - they keep each game's facts: fingerprints, install options and quirks;
  - their run lines become `OME-Tool start` with a profile;
  - the conventional install folder becomes a from-source note.

  The generated `games/README.md` changes with `modding_games_build.py`: its modules come from the base mod, and
  its install line names a profile.

### 9.3 Links

- A link's text is words, never a file's path: `[the games page](games/README.md)`, not `[games/README.md](…)`. This
  covers the nine guides that link the games page that way, and `../backlog.md` where a guide names it.
- Inline file names written as code (`` `patching.md` ``) become links with words as their text.

### 9.4 What stays from source

| Step | Why it stays | The release-side text |
|---|---|---|
| Testing an `omp export` on native AGE (`tools/native_scratch.py`) | it makes a scratch copy of the install with overlays | copy the install, put the exported files into the copy's folder, and run its `AGE.EXE` |
| Driving the port or AGE (`tools/drive.py`) | the driver is a developer's tool | none: the guides mark it from source |
| `--game-root ../games/kamidori-translated`-style paths | they assume the workspace's layout | a profile made on that install |
| The Lua console (`lua.md`, "The console") | it is the driver's `lua` step over the port's control channel | none: marked from source; a console for `start` is future work (§11) |

### 9.5 Gaps the survey found, fixed in the same pass

- `games/kamidori.md` lists `globals_build.py` but not `patch_points_build.py`, though Kamidori has patch points.
  Both lines go (§9.2).
- `games/arterial.md` names the Arterial data tables five times as a path written as code, which a release's reader
  cannot open: four times as `docs/arterial-data-tables.md` and once as `../../arterial-data-tables.md`. It links it
  nowhere. All five become links.
- `lua.md` and `data-files.md` write `omp check` with no program and no options, and say nowhere where it runs. The
  pass writes them as the other guides do.

## 10. Alternatives considered

- **Publish `Ome.Cli` as it is.** It holds about twenty developer commands beside the mod commands. `data-export`
  brings SQLite's native library, and `sweep`, `trace` and `play` are of no use to a modder. Its start also needs a
  checkout (§1.1). Rejected: a modder would get a program whose usage line is mostly not for them, and the archive a
  native library for one developer's command.
- **The commands in the launcher** (`OME-Launcher omp check …`). It is windowed on Windows, so its output and exit
  code are unreliable there (§3.2, item 1). Rejected.
- **The commands in the engine** (`OME -- omp check`). It is windowed as well, and starts Godot for a text command.
  Rejected.
- **Copy the toolchain into the base mod and keep `oms/<game>/` as the source.** This is less churn, but leaves a
  hand-kept file in two places, with a gate to keep them equal. Moving it gives one home, which the user chose
  (§15).
- **Embed the toolchain on its own** (`Ome.Engine` embedding `oms/` and the curated globals), with no base mod
  change. It is simpler in the package format, but puts a game's facts in the engine's own resources, beside the
  manifests that "Data beside the program" may move out. The base mod is where the user placed a game's facts.
- **Keep the repository walk for a checkout** and read the base mod only in a release. That gives two link bases,
  and the checkout keeps its fresh-clone and copied-package problems. Rejected (§3.2, item 3).
- **Ship the auto map's globals.** They are derived from the game's scripts, CI has no game files, and the compiler
  reads none of them (§2). Rejected.
- **A Python locator in the release** (an embedded interpreter, or a frozen `locate_page.py`). It needs the
  extracted scripts and the Python loader. Rejected for a C# command over the catalog.

## 11. Future work

- **Retire `tools/locate_page.py`** once gate 6 shows `locate` prints the same on each installed game. Then point
  `AGENTS.md`'s handoff at `Ome.Cli locate` (or `OME-Tool`), and delete the Python one.
- **Modules from mods:** a library package that offers OMS modules to the mods that require it, as `ome-records`
  offers Lua. Until then, §5.2 ignores `oms/` outside a base mod.
- **An installed base mod's toolchain:** whether a newer base mod installed by a player should also replace the
  compiler's input. It touches trust and the stamp.
- **A Lua console in a release:** `OME-Tool start --console`, or a command of its own, over the port's control
  channel, as the driver's `lua` step uses it.
- **The modules in the reference bundle.** The bundle carries the base mods' `layout/`, `events/` and shapes.
  Carrying `oms/*.oms` too would let the site show `<game>.adv`. Meanwhile the guides link the file on GitHub.
- **The devkit** (backlog) drives these commands.
- **Editor support:** OMS language support that reads the base mod's modules. This is the devkit item's
  "editor extension".

## 12. Validation gates

1. **The toolchain is current and complete.**
   - `base_mod_build.py --check` (a core gate) fails on a stale `oms/globals.json`.
   - The file names every named cell and table of the registry, with its old names, in the form the loader reads.
     The build reports any named cell it cannot write, and Kamidori's 44 load.
   - A test compiles a fragment that names a renamed global by its old name.
2. **A release links fragments outside a repository** (package smoke, Linux, on `released_copy`).
   - The smoke's synthetic install lists the scripts the base mods' modules run, as the tests' installs do. Without
     that, the launcher's check would stop passing after M1.
   - `OME-Tool omp check` checks a mod with a new script that `use`s `<game>.globals` and `<game>.adv` and names a
     global. If the smoke's install can carry a loose script (Not verified), it also checks a fragment at a named
     patch point.
   - Both link, and the report names the base mod's toolchain. The engine links the same mod in `--package-smoke`.
3. **The move changes nothing it should not** (workspace, M1). For each installed game, two things give the same
   output before and after M1:
   - `oms-decompile --level idiomatic` of every script, and the idiomatic rebuild;
   - `omp build` of the example mods, whose plans must match.
4. **A broken toolchain fails only what needs it** (core). On an install missing a script a module runs, an
   asset-only mod links, and a fragment reports the module's error.
5. **The tool stands alone:**
   - `OME-Tool --version` gives the archive's version and commit;
   - a command `OME-Tool` lacks is refused with its own list;
   - `--profile` with an id that names no player profile is refused as §6.3 says, in the tool, `Ome.Cli`, the engine
     and `tools/paths.py`;
   - with no profile and no `--game-root`, it says to give one;
   - a usage error exits 2;
   - the test that the tool's code uses no `Paths` passes;
   - on Windows, `verify_windows_native.py` passes on it.
6. **`locate` matches `locate_page.py`.**
   - Core: synthetic scripts and page maps, including the `P003` form, which only `locate` takes.
   - Workspace: each installed game's page maps, from a port run.
7. **`start` shows the engine's output** (Linux smoke).
   - `OME-Tool start --engine-arg --headless` relays the engine's lines and exit code, and `diagnostics/engine.log`
     holds the same lines.
   - A `--mod` that fails the Link check stops the start before the engine runs.
8. **The guides from a release** (workspace, on this machine).
   - An agent follows "Your first mod" on Himegari with the CI archive unpacked outside the repository.
   - The launcher has no command that makes a profile, so the agent writes its `profile.toml` as the package smoke
     does (`write_smoke_home`).
   - The mod's line must show in the port (`drive.py port start --package DIR`, which drives a package's own
     engine).
   - One run uses `OME-Tool start` with `--lua-trace`, and its output shows in the terminal and in `engine.log`.
9. **The site** builds against the bundle with the new guides, and its link check passes.
10. **Himegari's full validation** passes before the merge.

## 13. Milestones

- **M0, measure.**
  - Publish a stub `Ome.Tool` with the moved code and `Ome.Launcher.Core` under NativeAOT for both platforms: the
    warnings and the size.
  - Check that a synthetic loose script can serve gate 2.
  - Find out how `start` can stop the engine on Ctrl+C (§8).
  - Decide the fallback (§3.2, item 7) if needed.
- **M1, the toolchain in the base mods** (§5):
  - the loader's changes, and `oms/globals.json` with the old names and Kamidori's types in the loader's form;
  - `omp = 5` with `oms/`, `adv.oms` moved, and `Paths.OmsModules` deleted;
  - `ModLinkBase` reading the shipped base mod, with a toolchain failure that fails only what needs it;
  - `Ome.Cli`'s compiles on the link base;
  - the tests, `validate.py`'s gate, `modding_games_build.py` and `adding-a-game.md`;
  - the smoke's synthetic install listing the modules' scripts;
  - gates 1, 3 and 4.

  The backlog's "Toolchain packaging for exported ports" is deleted here.
- **M2, the tool** (§6):
  - the project and the moved commands, with `Ome.Cli` calling them;
  - UTF-8 output, usage errors that exit 2, the test against `Paths`, and `AllowedReaders`;
  - `--profile <id>` with the profile's trust, the old meaning retired everywhere (§6.3), and `import-scripts`'
    options renamed;
  - `--version`;
  - gate 5, less its Windows part.
- **M3, `locate` and `start`** (§7, §8), with gates 6 and 7.
- **M4, the release** (§6.4):
  - the files §6.4 names;
  - the sizes recorded in `docs/platform-portability.md`;
  - gates 2, 5 (Windows) and 7 on CI.
- **M5, the guides** (§9), with gates 8 and 9. The backlog's "Modding from a release" is deleted here.

## 14. Docs this will touch when built

- `docs/modding/`: every guide (§9), and `games/README.md` through `modding_games_build.py`.
- `docs/tools-reference.md`: `OME-Tool` and its commands, `locate`, `start`, `Ome.Cli`'s forwarded commands, the
  retired `--profile` alias, and `import-scripts`' renamed options.
- `docs/PROJECT-STRUCTURE.md`: `engine/Ome.Tool/`, the base mods' `oms/`, and the `oms/<game>/` folders gone.
- `docs/platform-portability.md`: the tool in the package, its checks and its sizes.
- `docs/adding-a-game.md`: where a new game's ADV module goes, and the step that starts its base mod.
- The other docs that name `oms/<game>/`: `docs/sys4-format-notes.md`, `docs/arterial-data-tables.md`, and the
  comment in `vm-map/globals-arterial.toml`.
- `docs/backlog.md`:
  - both items deleted;
  - "Data beside the program" names the toolchain as part of the base mods;
  - its other mentions of `oms/<game>/`.
- `mods/<game>-base/README.md`: the `oms/` folder.
- `run-godot.ps1`: the `-Profile` alias and its comment.
- `AGENTS.md` (the workspace's):
  - the generated-files table: `base_mod_build.py` also writes `oms/globals.json`, and the `modding_games_build.py`
    row names `oms/<game>/`;
  - the trigger lines for editing `adv.oms` and for `modding_games_build.py`;
  - the canonical map's base mod row;
  - the conventions' line that names `--profile` as `--game`'s old name;
  - the page-location handoff, if `locate` replaces the Python tool (§11).
- The status memory and the changelog, at each milestone.

## 15. Decisions by the user

Asked and answered on 2026-11-24. The recommended answer was taken for questions 1, 2 and 4; question 3 took the
user's own.

1. **The program's name:** `OME-Tool`. The others offered were `OME-Mod`, and `ome`, which on Windows is the same
   name as `OME`.
2. **The toolchain's home:** `adv.oms` moves into the base mod (§5.1). The others offered were a copy there with a gate
   that keeps it equal, and embedding the toolchain in `Ome.Engine` on its own (§10).
3. **How a command names a profile:** `--profile <id>`, which stops being `--game`'s old name (§6.3). The options
   offered were a new `--player-profile`, `--data-root` only, and the current folder. The user: "If --profile is the
   old name of --game, why are we blocked from using it? I think it's fine to deprecated the old usage for the new
   one, unless you disagree. We're still pre 1.0 and I don't think anyone's using this yet except me". Nothing argues
   against it: an old command line fails with a message rather than meaning something else, unless a player profile
   happens to be named after the game. Even then it runs that profile, with its install and its mods, and only if the profile's trust allows (§6.3).
4. **The example mods:** not shipped. The guides link them on GitHub. The other option offered was an `examples/`
   folder at the archive's top.

## 16. Changed after the review of 2026-11-24

An independent agent checked this design against the code at `13d0fd53`. Each finding below was checked against the
code before the text changed.

- **Facts corrected:**
  - the projects the tool references (§6.1): `Ome.Engine` references `Ome.Engine.Devices`, and
    `Ome.Launcher.Core` references `Ome.Engine.Frontend`;
  - the version's source (§2, §6.1);
  - the exit codes (§6.2);
  - the sizes (§2, §5.5);
  - which packager refuses extra data files (§2);
  - Arterial's links (§9.5);
  - the order of `Ome.Cli`'s start (§1.1).
- **The globals loader** reads no old names and skips 44 of Kamidori's named cells. §5.1 changes it, and gate 1
  checks it.
- **A toolchain that does not build** no longer stops every link (§5.3, gate 4). The moved modules always load, and
  their idioms need the scripts they run. The smoke's install lists those scripts (gate 2).
- **The move fails loudly:**
  - `Paths.OmsModules` goes, and every `OmsModules()` caller and test is named;
  - `validate.py`'s gate follows the modules;
  - gate 3 now compares output before and after the move, where it compared two link bases that read the same
    files.
- **The tool has no checkout defaults** (§6.1).
- **`locate`** reads the profile's map, says which map it read, and lists the script the engine ran (§7.2).
- **Retiring `--profile`** now names every reader. `tools/paths.py` refuses it instead of letting it pass through, and
  a profile's trust is honoured (§6.3). `import-scripts`' options for the `--from` install are renamed (§6.3).
- **`start`** gains `--engine-arg`, checks the `--mod` packages, handles Ctrl+C and prints UTF-8 (§8).
- **The package format:** `oms/` comes with `omp = 5` and stays a note in other packages, and the link report names a
  shipped and an installed base mod that differ (§5.2).
- **Also:**
  - the release's files are named (§6.4);
  - `adding-a-game.md` and the other docs join §14;
  - the Lua console is marked from source (§9.4).
