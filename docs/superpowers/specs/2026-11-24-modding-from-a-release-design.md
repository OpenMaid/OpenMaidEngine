# Modding from a release design

**Date:** 2026-11-24
**Status:** proposed. The user decided its four questions on 2026-11-24 (§15), and the text follows their answers.
Not yet accepted.
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
  already compiles `Ome.Engine`, `Ome.Script`, `Ome.Mods`, `Ome.Scripting.Lua` and Tomlyn under NativeAOT. The
  commands moved out of `Ome.Cli` and `Ome.Launcher.Core` have not been compiled that way;
- that the package smoke's synthetic install can carry a loose script for a fragment to patch (§12, gate 2). The
  smoke's install (`tools/package_linux_x64.py`, `write_smoke_home`) holds one archive and no script;
- that the C# listing in `locate` can print each operand as `tools/locate_page.py` does (§7.3).

## 1. Purpose and scope

### 1.1 The problem

A release holds two programs, the engine (`OME`, `OME.exe`) and the launcher (`OME-Launcher`). A player needs
nothing else. A modder needs a source checkout, because:

1. **The mod commands are not in a release.** `omp check`, `omp build`, `omp text`, `oms-patch`, `oms-decompile`,
   `oms-compile` and `picture` are commands of `Ome.Cli`, and no workflow publishes `Ome.Cli`. Inside a checkout it
   still cannot run on its own: before it reads `--game-root`, its start reads `Paths.GameInstall` or
   `Paths.GameDir` and loads `build/opcodes.json`. `Paths.Repo` throws when no checkout lies above the program
   (`Ome.Engine/Sys4/Paths.cs`, `FindRepo`).
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
  differs from Himegari's build copy only in its `note` line. `base_mod_build.py` copies it "so an exported build
  carries them". `ModPackage.ReadBaseLayout` reads it as `OmsPatchPoints`, the type the compiler takes, and
  `ModLuaLinker` falls back to it. The OMS compiler does not.
- **The base mod's layout is not the compiler's globals.**
  - `layout/globals.json` gives each exported name its bank, address and dimensions.
  - `GameModule.LoadGlobals` also reads each cell's `type` and `columns`, and the record tables' `records` and
    `fields`.
  - `ModLayout.Classify` allows `layout/` to hold `globals.json`, `points.json` and `exports.txt`, and nothing else.
- **The auto map adds nothing the compiler reads.** A checkout's `build/games/<game>/globals.json` merges the game's
  inferred map (`global-var-map.json`, a workspace input derived from the game's scripts). Its cells have no names,
  and `LoadGlobals` skips a cell without a name. On 2026-11-24, for all three games, the named cells and the tables
  of the build copy equalled those of `globals_build.curated_json`, the registry alone: 469, 642 and 369 named
  cells. So the registry alone gives the compiler everything it reads, and needs no game file and no CI-only input.
- **Sizes.**
  - The OMS modules are one file per game: `oms/<game>/adv.oms`, 36 KB, 44 KB and 84 KB on disk.
  - The curated globals, with every registry key, are 745 KB, 783 KB and 338 KB. The build copies, with the auto
    map, are 2.9, 5.9 and 7.0 MB.
- **The link base is shared.** The engine, `omp` and the launcher's Link check link through
  `ModLinkBase.ForGame`, so they agree (launcher spec §5.9). Its `ToolchainStamp` hashes the toolchain files it read.
- **Profiles resolve without a checkout.**
  - `RunResolver.Resolve` reads `--data-root`'s `profile.toml` for game, install and overlays. `Ome.Cli` already
    calls it.
  - `Ome.Launcher.Core` resolves OME home (`OmeHome.Resolve`, including the portable `data/` beside the launcher)
    and the profiles in it (`ProfileStore`). It runs the checks (`ProfileChecks.Run`) and starts the engine
    (`EngineCommands.For`, `EngineLaunch.Run`). It references no Avalonia type.
- **A release engine's output can be read.**
  - The release engine has no console on Windows. Godot gives a console wrapper only to debug exports.
  - The launcher captures the engine's standard output and error into `diagnostics/engine.log` (`EngineLaunch.Run`).
    Its M0 measured that a release engine's output reaches a .NET parent, once `flush_stdout_on_print` is on
    (launcher plan, M0 measure 6).
- **Page maps in an export.** `Main.DefaultPageMapPath` writes `build/games/<game>/page-map-<SCENE>.jsonl` in an
  editor build (`TOOLS`). An export writes `<profile root>/diagnostics/page-maps/page-map-<SCENE>.jsonl`, on every run
  but `--selftest`. With no `--data-root`, the profile root is `user://games/<namespace>/`
  (`GamePersistencePolicy`).
- **No C# listing.** `oms-decompile --level raw` prints `@op(...)` lines with labels only at jump targets. No C#
  command prints each instruction's offset, as `locate_page.py` does (`0x00871: wait-for-input 0x0`).
- **The release's shape.**
  - The top folder holds the engine program, `OME.pck`, the launcher's files, the notices, `BUILD-INFO.json` and
    `SHA256SUMS`.
  - `data_OME_<platform>_x86_64/` holds exactly 11 files: the engine library, the .NET notices and FFmpeg. The
    packagers refuse anything else there (`tools/package_linux_x64.py`, `tools/verify_windows_native.py`).
  - The Windows archive is cross-packaged on Linux and checked structurally. The Linux package runs a smoke from a
    copy outside the repository (`released_copy`).

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
4. **The compiler's globals come from the registry alone.** The base mod gets the curated form, cut down to the keys
   `LoadGlobals` reads. The auto map stays a checkout's input for the Python tools and the global reference docs, and
   never enters a release.
5. **The locator reads where the engine writes.** `locate` follows `Main.DefaultPageMapPath`'s split. Inside a
   checkout that is `build/games/<game>/`; otherwise it is the profile's `diagnostics/page-maps/`.
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
  - `oms/globals.json` is `<game>.globals`. Each `oms/*.oms` declares its own name (`module himegari.adv`), and it
    must start with `<game>.`, as `GameModule.LoadSources` requires today. That method reads a folder on disk, so it
    gains a form that reads the package's files.
  - `oms/globals.json` holds `cells` (`name`, `type`, `address`, `columns`) and `tables` (`name`, `records`,
    `fields`), with a generated note. It is the form `LoadGlobals` reads, so the loader does not change.
- **`oms/examples/` stays** where it is. It holds example mods, not a game's toolchain.
- **`build/games/<game>/globals.json` stays.** The Python tools, `data-export`, `script-index` and `save-diff` read
  it. It stops being the compiler's input.
- **The `oms/<game>/` folders go.** `modding_games_build.py` lists a game's modules from the base mod instead.

### 5.2 The package format

- **A base mod may hold `oms/`.** `ModLayout.Classify` accepts `oms/globals.json` and `oms/*.oms` in a package whose
  layout is a base mod's (`layout/globals.json`), and refuses `oms/` in any other package. A mod that offers OMS
  modules to other mods is not part of this design (§11).
- **The linker reads the shipped base mod.** It reads the copy the engine ships (`ShippedPackages`), not an installed
  package of the same id. The toolchain is part of the program, as the opcode table is. An installed base mod of
  that id still replaces the shipped one in the stack, as today, but not as the compiler's input.
- **Versioning.** A module's names are part of what mods depend on, as the layout's names are. A rename in `adv.oms`
  or in the registry follows the base mod's existing rule: the old name stays as an alias until the major version
  rises. `exports.txt` and `base_mod_build.py --check` already enforce it for globals. Extending the check to
  `adv.oms`'s exported names is in the plan.

### 5.3 The link base

- **`ModLinkBase.ForGame` reads the toolchain from the shipped base mod.** It no longer reads it from `Paths`, and
  loses its `repositoryToolchain` result. The port's "no repository toolchain" warning goes.
- **A game with no base mod** links as an export does today, with only `<game>.scripts`. The link report says so
  once.
- **The stamp is kept.** `ToolchainStamp` hashes the base mod's `oms/` files and `layout/points.json`, as it hashes
  the repository's files today.
- **`Ome.Cli`'s own compiles share the link base.** `oms-compile`, `oms-decompile` and `oms-patch --source` build
  their modules through the link base, so every compile in a checkout sees what the engine sees.
- **Tests** that build a link base from `Paths.OmsModules`, `Paths.GlobalsJson` or `Paths.PatchPointsJson` move to
  the shipped base mod. A synthetic link base keeps `ModLinkBase.ForInstall`'s file arguments.

### 5.4 What changes in a checkout

- **Editing `adv.oms`** takes effect at the next build. `dotnet run` and `run-godot.cmd` build first, and they already
  do so for a base mod's Lua.
- **Renaming a global or a table** already needs `globals_build.py --build` and `base_mod_build.py --build`
  (`AGENTS.md`). The second now also writes `oms/globals.json`, and the core gate checks it is current.
- **A fresh clone** links fragments with no Python run. The checked-in base mods carry everything.

### 5.5 Data beside the program

This adds about 200 KB of modules and globals to the base mods, which are already embedded. The rule stays that all
data is inside the library. If the user later moves the base mods beside it, the toolchain moves with them in one
step. That is a reason to keep the toolchain in the base mod rather than embed it on its own.

## 6. The mod tool

### 6.1 The program

- **Project.** `engine/Ome.Tool/`, a console program in `OmeEngine.sln`. Its assembly name gives the program
  `OME-Tool` (`OME-Tool.exe` on Windows).
- **No repository.** It never reads `Paths`. Its opcode table and contracts are the embedded ones, and its game root
  comes from a profile or `--game-root` (§6.3).
- **Libraries.** It references the libraries the engine library already holds (`Ome.Engine`, `Ome.Script`,
  `Ome.Mods`, `Ome.Scripting.Lua`) and `Ome.Launcher.Core` for profiles and the start. It does not reference
  `Ome.Export`, whose SQLite needs a native library, or `Ome.Engine.Devices`.
- **Shared code.** The commands' code moves out of `Ome.Cli`'s `Program.cs`, `OmpCommands` and `OmpTextCommand`
  into one place both programs call. Where that code lives, a library or `Ome.Tool` itself, is the plan's choice.
- **`--version`** prints the release's version and commit, as `BUILD-INFO.json` gives them.

### 6.2 Commands

| Command | As in `Ome.Cli` | Notes |
|---|---|---|
| `omp check`, `build`, `export`, `explain`, `lua-stubs` | yes | link through `ModLinkBase.ForGame` (§5.3) |
| `omp text extract`, `check`, `stats`, `po-export`, `po-import`, `import-scripts` | yes | contracts from the embedded registry |
| `oms-patch --fingerprint`, `--source`, `--plan` | yes | |
| `oms-decompile`, `oms-compile` | yes | modules from the link base |
| `picture` | yes | `ResourceMap`, in `Ome.Engine` |
| `locate` | new (§7) | `Ome.Cli` gets it too |
| `start` | new (§8) | release only: it starts the engine program beside the tool |

- **Usage.** A command the tool lacks (`sweep`, `trace`, `data-export`, …) is refused with the list of the tool's
  own.
- **Exit codes and messages** stay those of `Ome.Cli` today: 0 for success, 1 for a failed check, 2 for a usage or
  link error.

### 6.3 Naming the game and the profile

- **A profile** is the usual way, since the launcher makes one for every game a player adds. A command takes either:
  - `--data-root <profile root>`, which exists and is read through `RunResolver`; or
  - `--profile <id>`, a profile's id resolved through OME home as the launcher resolves it, with `--home DIR` as in
    the launcher.

  Either one gives the game, the install, the overlays and the profile's `[mods]`, so `omp check` checks what the
  profile would run (launcher spec §5.7).
- **`--profile` stops being `--game`'s old name** (the user, 2026-11-24, §3.1).
  - Every program that took it for the game drops that meaning in the same change: `GameProfileSelection` (so the
    engine and `Ome.Cli`), and the Python tools' shared option (`tools/paths.py`, `validate.py`,
    `text_geometry_compare.py`).
  - In `OME-Tool` and `Ome.Cli`, an id that names no player profile is refused with where the tool looked and a
    reminder that the game is `--game`, so an old command line fails plainly.
  - The engine runs a profile from its folder alone (launcher spec §2), so it keeps `--data-root`. Given
    `--profile`, it refuses it and names `--data-root` and `--game`.
- **Game options without a profile** still work: `--game <game> --game-root <install>`, with `--overlay-root` where
  needed. In a release `--game-root` is required, since there is no conventional install beside a repository. Inside
  a checkout it defaults to `games/<game>/`, as today.
- **Mods.** `--mod PATH` adds a folder or `.omp` to what the profile lists, as the engine's `--mod` does.

### 6.4 In the release

- **Published per platform** by the engine's jobs, since NativeAOT builds for the platform it runs on.
  `windows-engine` builds `win-x64`, and `linux-engine` builds `linux-x64` in its manylinux container. The script
  that publishes it (`publish_engine.py`, or one beside it) checks its file list, as `publish_engine.py` checks the
  library's.
- **Placed at the top of the archive**, beside `OME` and `OME-Launcher`. The data folder's rule of 11 files is
  unchanged. `BUILD-INFO.json` and `SHA256SUMS` list it.
- **Checked on Windows** by `verify_windows_native.py`, structurally as the engine is. It may import only what the
  engine library may. The Linux package checks it as it checks the library, against glibc 2.28.
- **Its notices.** The .NET runtime notices it needs are already in the archive. The plan checks that it brings no
  other third-party code.
- **The archive's README** names the tool and the guides' address.

## 7. The page locator in a release

### 7.1 Where the map is

The locator HUD (F2), the copy (F3) and the page map already work in an export (§2). What a release lacks is the
command that resolves a locator.

### 7.2 `locate`

```text
OME-Tool locate SC0000 3 [--context N] <profile option>
OME-Tool locate SC0000@0x8a6 [--context N] <profile option>
```

- **By page** (`SC0000 3`, or `SC0000 P003` as F3 copies it), it reads the map from:
  - `--map FILE`, when given;
  - else `build/games/<game>/`, when the tool runs inside a checkout (an editor build writes there);
  - else the profile's `diagnostics/page-maps/`;
  - else, with game options and no profile, the bare engine's `user://games/<namespace>/diagnostics/page-maps/`.
- **By site** (`SC0000@0x8a6`, the `wait` or `text` part of a locator), it needs no map.
- **Missing pages.** A page the map does not hold, or a missing map, is refused with where it looked. The tool never
  guesses a page from the script (`AGENTS.md`, "Runtime page-location handoff").
- **Output.** It prints what `locate_page.py` prints:
  - the page's header;
  - the page start, last text and wait;
  - the call stack and the page's text;
  - a listing of `--context` instructions (default 8) before and after the wait, with the wait marked `>>>`.

  The script comes from the install's catalog (`ModLinkBase.ReadScript`), with the profile's overlays. It does not
  come from `extracted/`.

### 7.3 The listing

The listing is the new code. Each line holds an instruction's dword offset, its mnemonic from the embedded opcode
table, and its operands, in the form the guides show:

```text
    0x00871: wait-for-input 0x0
    0x00874: end-text-line 0x0
>>> 0x00877: reset-adv-text-layout 0x1
```

The mnemonics come from the same registry as the Python loader's (`vm-map/opcodes.toml`). Operands follow
`locate_page.py`'s text where the two can agree. Gate 5 compares them.

## 8. Running a mod: `start` and the launcher

There are two ways, and the guides give both.

- **Through the launcher**, for trying a mod as a player would:
  - add the mod's folder or `.omp` to the library, enable it in the profile, and press Play;
  - the launcher's checks link it first, and the engine's output goes to `diagnostics/engine.log`.

  This works today, with no change.
- **`OME-Tool start`**, for the authoring flags:

  ```text
  OME-Tool start <profile option> [--mod PATH ...] [--lua-dev] [--lua-trace] [--lua-reload]
                 [--lua-errors halt] [--text-log] [--locator-hud] [-- other engine flags]
  ```

  - It runs the profile's checks (`ProfileChecks.Run`) and starts the engine program beside the tool with the
    profile's command (`EngineCommands.For`) plus these flags.
  - It prints each line of the engine's output in the terminal as it comes, and also writes it to
    `diagnostics/engine.log`, between the launcher's `[launcher]` lines.
  - It returns the engine's exit code.
  - A failed check is printed and stops the start, as in the launcher.

  `EngineLaunch.Run` gains a line callback for this. It is the launcher's start, shown in the terminal.
- **Flags `start` does not name** go after `--` to the engine unchanged. `--data-root` is refused there, as the
  launcher refuses it.
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

### 9.5 Gaps the survey found, fixed in the same pass

- `games/kamidori.md` lists `globals_build.py` but not `patch_points_build.py`, though Kamidori has patch points.
  Both lines go (§9.2).
- `games/arterial.md` names `docs/arterial-data-tables.md` four times as a path written as code, which a release's
  reader cannot open, and links it once (`../../arterial-data-tables.md`). All five become links.
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

- **Retire `tools/locate_page.py`** once gate 5 shows `locate` prints the same on each installed game. Then point
  `AGENTS.md`'s handoff at `Ome.Cli locate` (or `OME-Tool`), and delete the Python one.
- **Modules from mods:** a library package that offers OMS modules to the mods that require it, as `ome-records`
  offers Lua. §5.2 refuses `oms/` outside a base mod until then.
- **An installed base mod's toolchain:** whether a newer base mod installed by a player should also replace the
  compiler's input. It touches trust and the stamp.
- **The devkit** (backlog) drives these commands.
- **Editor support:** OMS language support that reads the base mod's modules. This is the devkit item's
  "editor extension".

## 12. Validation gates

1. **The toolchain is current.** `base_mod_build.py --check` (a core gate) fails on a stale `oms/globals.json`, and
   `oms/globals.json` names each cell and table that `globals_build.curated_json` does.
2. **A release links fragments outside a repository** (package smoke, Linux, on `released_copy`):
   - `OME-Tool omp check` of a mod with a new script that `use`s `<game>.globals` and `<game>.adv` and names a
     global;
   - and a fragment at a named patch point, if the smoke's install can carry a loose script (Not verified).

   Both link, and the report names the base mod's toolchain. The engine links the same mod in `--package-smoke`.
3. **A checkout links as a release does** (core tests): a link base built in a checkout and one built with no
   repository give the same modules, idioms, patch points and stamp.
4. **The tool stands alone:**
   - `OME-Tool --version` gives the archive's version and commit;
   - a command `OME-Tool` lacks is refused with its own list;
   - `--profile` with an id that names no player profile is refused as §6.3 says, in the tool, `Ome.Cli` and the
     engine;
   - with no profile and no `--game-root`, it says to give one;
   - on Windows, `verify_windows_native.py` passes on it.
5. **`locate` matches `locate_page.py`.**
   - Core: synthetic scripts and page maps.
   - Workspace: each installed game's page maps, from a port run.
6. **`start` shows the engine's output.** In the Linux smoke, `OME-Tool start` with `--headless` relays the
   engine's lines and exit code, and `diagnostics/engine.log` holds the same lines.
7. **The guides from a release** (workspace, on this machine): an agent follows "Your first mod" on Himegari with
   the CI archive unpacked outside the repository. The launcher has no command that makes a profile, so the agent
   writes its `profile.toml` as the package smoke does (`write_smoke_home`). The mod's line must show in the port
   (`drive.py port start --package DIR`, which drives a package's own engine).
8. **The site** builds against the bundle with the new guides, and its link check passes.
9. **Himegari's full validation** passes before the merge.

## 13. Milestones

- **M0, measure.**
  - Publish a stub `Ome.Tool` with the moved code under NativeAOT for both platforms: the warnings, the size, and
    whether `Ome.Launcher.Core` compiles that way.
  - Check that a synthetic loose script can serve gate 2.
  - Decide the fallback (§3.2, item 7) if needed.
- **M1, the toolchain in the base mods** (§5):
  - the package format (`oms/`) and `base_mod_build.py` writing `oms/globals.json`;
  - `adv.oms` moved, and `ModLinkBase` reading the shipped base mod;
  - `Ome.Cli`'s compiles on the link base, and the tests moved;
  - `modding_games_build.py` updated;
  - gates 1 and 3.

  The backlog's "Toolchain packaging for exported ports" is deleted here.
- **M2, the tool** (§6):
  - the project and the moved commands, with `Ome.Cli` calling them;
  - profile resolution and `--profile <id>`, with the old meaning retired everywhere (§6.3) and its docs:
    `docs/modding/cli.md`, `troubleshooting.md`, `docs/tools-reference.md`, and `AGENTS.md`'s convention that
    names `--profile` as `--game`'s old name;
  - `--version`;
  - gate 4, less its Windows part.
- **M3, `locate` and `start`** (§7, §8), with gates 5 and 6.
- **M4, the release** (§6.4):
  - the workflows and packagers, the Windows verification, the smoke steps and the archive's README;
  - the sizes recorded in `docs/platform-portability.md`;
  - gates 2, 4 (Windows) and 6 on CI.
- **M5, the guides** (§9), with gates 7 and 8. The backlog's "Modding from a release" is deleted here.

## 14. Docs this will touch when built

- `docs/modding/`: every guide (§9), and `games/README.md` through `modding_games_build.py`.
- `docs/tools-reference.md`: `OME-Tool` and its commands, `locate`, `start`, and `Ome.Cli`'s forwarded commands.
- `docs/PROJECT-STRUCTURE.md`: `engine/Ome.Tool/`, the base mods' `oms/`, and the `oms/<game>/` folders gone.
- `docs/platform-portability.md`: the tool in the package, its checks and sizes.
- `docs/backlog.md`: both items deleted. "Data beside the program" names the toolchain as part of the base mods.
- `mods/<game>-base/README.md`: the `oms/` folder.
- `AGENTS.md` (the workspace's):
  - the generated-files table, where `base_mod_build.py` also writes `oms/globals.json`;
  - the trigger for editing `adv.oms`;
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
   happens to be named after the game. Even then it runs that game.
4. **The example mods:** not shipped. The guides link them on GitHub. The other option offered was an `examples/`
   folder at the archive's top.
