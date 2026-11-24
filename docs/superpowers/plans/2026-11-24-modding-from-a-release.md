# Modding from a release — implementation plan

**Date:** 2026-11-24
**Spec:** `docs/superpowers/specs/2026-11-24-modding-from-a-release-design.md` (cited as "spec §n"). The spec stays the
design's home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what
changed since the spec was accepted (§4).
**Status:** milestone 0 measured (2026-11-24, §2); milestone 1 built (2026-11-24, §3.1), its gates passed (§2.4);
milestone 2 built (2026-11-24, §3.2), its gate passed; milestone 3 built (2026-11-24, §3.3), its gates
passed; milestone 4 built (2026-11-24, §3.4), its gates waiting on CI. §3 gets each later
milestone's section before it starts.
**Branch:** `feature/modding-from-release`, off `develop` at `e6b92761`. Its first commits are the spec (`ba5dbd18`,
`13d0fd53`, `b45e6c0b`).

Paths are relative to `age-reimpl/`.

## 1. What the code says today

These are the claims milestones 0 and 1 rest on, beyond spec §2, checked while planning. "Ran" marks a check made by
running code; "read" marks one made by reading it.

- **A named patch point is pinned to one revision of its script** (read: `Ome.Script/Patch/OmsPatchPoints.cs`).
  Each site carries that revision's `fingerprint`, and `Resolve(name, script, fingerprint)` returns no offset for
  another revision. So a synthetic script resolves no real patch point.
- **A synthetic install builds the idioms** when its catalog lists the scripts the modules run (read:
  `SharedLinkBaseTests`, with `Sys4StartupSettingsTests.BuildCatalog(SyntheticCatalog.ScriptsTheModulesRun(...))`).
  The asset store reads a loose file of the entry's exact name first (read: `Sys4AssetStore`). So a synthetic
  script can be a loose file that the catalog names.
- **The control channel works in every build** (read).
  - `godot/Main.cs` starts it on `--control`, with no `#if`.
  - The driver's `stop` sends `close` and waits for the exit (`tools/drive_port.py`). `close` quits as the game's
    own quit does (`Ome.Engine.Frontend/Control/DriveDispatcher.cs`).
  - The server and its protocol (`DriveServer`, `DriveProtocol`) are in `Ome.Engine.Frontend`, which the tool
    references.
- **The run lock is an exclusive file handle** (read: `Ome.Mods/Launch/RunLock.cs`, `FileShare.None`). The operating
  system releases it when the process dies, so an engine that is ended leaves no stale lock.
- **Kamidori's 44 named cells the loader skips** (ran, on `build/games/kamidori/globals.json`):
  - 38 are typed `int[AxB]` and 2 `string[AxB]`;
  - 4 are typed `int[unit][N]`, where `unit` names the record table whose record count is the dimension.
- **Old names in the registry's JSON** (ran):
  - A cell keeps its old names as `aliases`, a list. Himegari has one (`current_stage_id`, once
    `scjump_progress_a`), and Kamidori has one.
  - A table's renamed fields are `field_aliases`, a map from old name to new. Kamidori's `quest` has one. No table
    has its own `aliases` today.
- **This machine** (ran):
  - Windows has the Visual Studio 2019 C++ tools that NativeAOT links with. The ILCompiler's targets call
    `vswhere.exe` by name, so its folder (`C:\Program Files (x86)\Microsoft Visual Studio\Installer`) must be on
    `PATH`, or the link step fails with exit 123.
  - WSL's Ubuntu has neither the .NET SDK nor clang, so a Linux NativeAOT build is CI's.

## 2. Milestone 0: measures (2026-11-24)

### 2.1 NativeAOT

- **All of `Ome.Cli`** (`dotnet publish engine/Ome.Cli -c Release -r win-x64 -p:PublishAot=true`):
  - It links: `Ome.Cli.exe` is 23.9 MB, plus SQLite's `e_sqlite3.dll` (2.0 MB), which the tool will not carry.
  - It has 30 warning pairs (IL2026 and IL3050, reflection-based `System.Text.Json`). Every one is in a developer
    command: `oms-roundtrip`, `catalog-scan`, `init-image`, `trace`, `intake`, `OmsRebuildCheck` and `Ome.Export`'s
    writer.
  - The mod commands (`omp`, `omp text`, `oms-patch`, `oms-compile`, `oms-decompile`, `picture`) and `Ome.Mods`'
    tools (`OmpCommands`, `LuaStubs`) have none.
  - Run outside the repository, its `omp check` dies with `TypeInitializationException`, "repository root not found
    above …" (spec §1.1, now seen running).
- **A stub of the tool** (scratch, not committed). It references `Ome.Launcher.Core` and `Ome.Script`, and reaches
  `OmeHome.Resolve`, `LauncherSettingsStore.Load`, `ProfileStore.List`, `ProfileChecks.Run` (and through it the link
  composition), `EngineCommands.For` and `EngineLaunch.Run`.
  - It publishes for `win-x64` with no warning, at 14.8 MB, and runs.
  - So `Ome.Launcher.Core` compiles under NativeAOT, though it is not marked `IsAotCompatible`. M2 marks it.
- **Imports** (`dumpbin /dependents`): both programs import only system DLLs (`KERNEL32`, `USER32`, `ADVAPI32`,
  `ole32`, `bcrypt`, and `VERSION` for `Ome.Cli`) and the Universal CRT's `api-ms-win-crt-*`. None matches the
  engine's forbidden imports (`verify_windows_native.py`, `FORBIDDEN_ENGINE_IMPORTS`).
- **Settled:** the tool is published with NativeAOT, and spec §3.2's fallback is not needed. Its size will lie
  between the stub's and `Ome.Cli`'s, about 15 to 24 MB on Windows. Linux is measured the first time the branch runs
  on CI (the release workflow's `linux-engine` job, M4), with the same analysis behind it.

### 2.2 Gate 2 on a synthetic install

- **A fragment on a synthetic script can link in the smoke.**
  - The smoke writes a catalog that names the base script and the scripts the base mods' modules run.
  - It compiles a small OMS script into the install's root with `OME-Tool oms-compile`.
  - A fragment that `use`s `<game>.globals` then patches that script.
- **A named patch point cannot.** It resolves only on the real revision's fingerprint (§1). So gate 2 checks that
  the report lists the base mod's patch points, and the fragment at a named point moves to the workspace (§4).

### 2.3 Stopping `start` on Ctrl+C

`start` passes `--control` with a session file in a temporary folder of its own. On Ctrl+C it sends `close` over the
channel and waits, as the driver's `stop` does. If the engine has not exited after a grace period, it ends the
process, which the run lock survives (§1). A second Ctrl+C ends it at once. This settles spec §8's open point; gate 7
runs it.

### 2.4 The before-snapshot for gate 3

Before any M1 change, on this machine's three installs (Himegari, Kamidori and Arterial at `../games/<game>/`), the
snapshot records two things:
- `Ome.Cli oms-decompile --level idiomatic --output-dir` of every script;
- `Ome.Cli omp build` of each example mod of that game.

The outputs and their log are in the session's scratch folder, not in the repository, since the decompiled scripts
are game data. After M1 the same script runs again, and the two must be equal.

- **Decompiled:** 969, 1,286 and 1,176 scripts, each command exiting 0.
- **Built:** 25 of the 26 example mods. `example.kamidori-prologue-line` is refused, as it should be: it targets the
  translated install's `SC0000.BIN` (`sha256:54d6bc4d…`), and the snapshot gives the clean one (`sha256:e8db06e1…`).
  After M1 it must be refused the same way.

**After M1** (2026-11-24), the same script again:
- **Himegari and Arterial** decompile identically: 969 and 1,176 files, none changed.
- **Kamidori** changes in 1,007 of its 1,286 files, as M1 means it to (§4). Every changed line is one of three kinds:
  - 168,635 name one of the 44 cells the loader once skipped, where the decompiler printed a raw cell before;
  - 21 are a `use kamidori.globals` line, in scripts that name a global now and named none before;
  - 486 are sidecar fingerprints of the changed source.
- **The example builds** are byte-identical, all 25. The 26th is refused with the same message.
- **The idiomatic rebuild** (`oms-rebuild --level idiomatic`) passes for each game: 297, 279 and 271 scenes
  equivalent, none divergent.

## 3. Later milestones

### 3.1 Milestone 1: the toolchain in the base mods

**The loader** (`Ome.Script/Modules/GameModule.cs`):
- `LoadGlobals` gains a form over bytes. It reads a cell's `aliases` as more names for the same cell, and a table's
  `aliases` and `field_aliases` likewise. The file path form goes.
- `LoadSources` gains a form over named source texts.

**The generator** (`tools/base_mod_build.py`):
- It writes `mods/<game>-base/oms/globals.json` from the registry alone. The data is `globals_build.curated_json`'s,
  cut down to `cells` (`name`, `aliases`, `type`, `address`, `columns`) and `tables` (`name`, `aliases`, `records`,
  `fields`, `field_aliases`), under a generated note.
- Each type is written in the loader's form: `T[AxB]` becomes `T[A][B]`, and a table's name as a dimension becomes
  that table's record count. A named cell it cannot write is reported, and fails `--check`.
- `--check` covers the file, and the rename rule covers its old names.

**The move:**
- `git mv oms/<game>/adv.oms mods/<game>-base/oms/adv.oms` for the three games. The base mods' manifests go to
  `omp = 5`.

**The package format** (`Ome.Mods/Packages`):
- `ModManifest` gains the version for `oms/`.
- `ModLayout.Classify` reads `oms/globals.json` and `oms/*.oms` in a base mod of that version. Any other package
  keeps the note.
- `ModPackage` exposes them.

**The link base** (`Ome.Mods/Linking/ModLinkBase.cs`):
- `ForGame` reads the shipped base mod's `oms/` and `layout/points.json`, and loses `repositoryToolchain`.
- `ForInstall` takes those inputs in memory, and its stamp hashes them.
- A module set that does not build no longer throws. The base keeps the error, and each compile reports it.
- The port's warning (`godot/Main.Mods.cs`) and its startup stop (`godot/Main.cs`) go. The launcher's Link check
  shows the error as a warning when no mod needs a compile.

**Every reader of the old files:**
- `Paths.OmsModules` is deleted.
- `Ome.Cli`'s `OmsModules()` builds on the link base (`oms-compile`, `oms-decompile`, `oms-patch`, `oms-append`,
  `oms-rebuild`, `oms-roundtrip`).
- The tests that build a link base from the repository's files move to the shipped base mod, and
  `SyntheticCatalog.ScriptsTheModulesRun` reads the shipped modules.
- `validate.py`'s `oms-idiomatic-rebuild` gate looks for the base mod's `oms/*.oms`, and `test_validate.py`
  follows. `modding_games_build.py` lists the base mod's modules.
- The package smoke's `write_smoke_home` lists the scripts the base mods' modules run.

**The module's names** (spec §5.2 leaves them to this plan):
- `base_mod_build.py` adds them to `layout/exports.txt`, last, as `module MODULE.NAME`. They are what
  `OmsCompiler.Use` imports from a module: its constants, enums and their members, templates, cells, tables, and
  subs with their labels. Himegari's module has 312, Kamidori's 178 and Arterial's 126.
- It reads them by the module's layout: a declaration starts at column 0, and a `}` at column 0 closes its block.
  `BaseModTests.TheExportsListNamesWhatTheShippedModulesDeclare` checks the list against the parser for each game.
- A module's names have no old-name form. Renaming or removing one needs the base mod's major version raised, and
  `--check` refuses it otherwise. A sub can stay as one that calls its new name.

**Docs:**
- `docs/modding/cli.md`'s and `troubleshooting.md`'s "Outside the repository" entries;
- `docs/adding-a-game.md`;
- `mods/<game>-base/README.md`;
- `PROJECT-STRUCTURE.md` and `tools-reference.md`;
- `sys4-format-notes.md`, `arterial-data-tables.md`, and the comment in `vm-map/globals-arterial.toml`;
- the backlog: "Toolchain packaging for exported ports" is deleted;
- `AGENTS.md`'s generated-files rows and trigger lines.

**Gates:** 1, 3 (§2.4's snapshot again) and 4, then core validation. All passed on 2026-11-24.

**Surfaced on the way:** the second core run failed one launcher test, `AdvancedTests.TheEnginesOptionsAreWrittenToTheProfileFile`,
in its teardown: `TestHome.Dispose` could not delete the test home's `SYS4INI.BIN`, "being used by another process".
The data patches plan saw the same in another window test on 2026-11-18 (its §12). The test's UI job ended while
the page's last write was still running its check, which reads the install. Settling in `TestHome.Dispose` was too
late: the job's end leaves what it queued on the UI thread unrun, so a write that waited for its turn never ended.
`MainViewModel.SettleAsync` now waits for the model's background work (its first checks, the page's check run, its
writes, the gate), and each `Headless.Run` job settles the models built in it (`Headless.Track`) before it ends.

### 3.2 Milestone 2: the tool

**The projects:**
- `engine/Ome.Commands/`, a library: the commands both programs run (spec §6.1's shared code). It references
  `Ome.Launcher.Core`, and through it the engine library's projects, with `Ome.Script`, `Ome.Engine.Vm` and
  `Ome.Engine.Devices`. It is marked `IsAotCompatible`, and so is `Ome.Launcher.Core` (M0, §2.1).
- `engine/Ome.Tool/`, the program, in `OmeEngine.sln`: assembly `OME-Tool`, `OmeProgram=true`, `PublishAot`. It
  handles `--version` and hands every other command to `Ome.Commands` with the tool's defaults.
- `OmpCommands` and `LuaStubs` stay in `Ome.Mods/Tools`. The launcher's Link check prints `OmpCommands.CheckReport`,
  so they are already shared. What moves is the argument handling around them in `Ome.Cli`'s `Program.cs` (§4).

**What moves into `Ome.Commands`:** `omp` with its verbs, `omp text` (`OmpTextCommand.cs`), `oms-patch`,
`oms-compile`, `oms-decompile` and `picture`.
- One `CommandInputs.Open(args, defaults)` gives every command the game, the install, its catalog and asset store,
  the script provider, the opcode table and contracts, the engine build and the resolved run, as `Program.cs`'s top
  does today.
- `CommandDefaults` holds what differs between the programs:
  - the install when the command line names none: `Ome.Cli` takes `games/<game>/` beside the repository, and the
    tool says to give `--game-root` or `--profile`;
  - the opcode metadata: `Ome.Cli` reads `build/opcodes.json` live, and the tool the copy embedded in `Ome.Engine`;
  - `picture`'s folder: `Ome.Cli` writes `build/games/<game>/pictures`, and the tool `pictures/` in the current
    folder.
- `Ome.Cli` hands those commands over before its own start, and keeps the rest.

**Both programs:**
- **UTF-8 output.** `Console.OutputEncoding` is UTF-8 for the run, and the console's own code page comes back at
  exit.
- **Usage errors.** An `ArgumentException` that reaches the top exits 2 with its message.
- **`--profile <id>`** is resolved in `Ome.Commands`:
  - through OME home, `--home DIR` or else `OmeHome.Resolve`, as the launcher's command line resolves it, with
    `LauncherCommandLine`'s lookup made public;
  - a profile that needs review is refused with the launcher's reason;
  - an id that names no profile is refused, naming the home it looked in and that the game is `--game`;
  - the id becomes `--data-root <root>` before `RunResolver` reads the arguments, and `--profile` beside
    `--data-root` is refused.

**The old meaning goes** (spec §6.3's list):
- `GameProfileSelection.ProfileOptionName` and the alias in `IsGameOption`. `RunResolver` refuses `--profile`; the
  engine's message names `--data-root` and `--game`.
- `SaveDiffCommand`, `SaveInfoCommand` and `picture`'s option skipping; `run-godot.ps1`'s `-Profile`.
- `tools/paths.py` refuses `--profile`; `validate.py`, `text_geometry_compare.py` and `capture_gdi_fonts.py` drop it.
  Their tests and docs follow.
- `omp text import-scripts` takes `--from-overlay-root` and `--from-allow-bmp-as-agf` for the `--from` install.

**`--version`** prints `OME-Tool <version> (commit <hash>)`. The `OmeVersion` target also stamps `OmeCommit`
(`git rev-parse --short=12 HEAD`), which `EngineIdentity` reads, since a tagged build's `git describe` names no
commit. The engine and the launcher show the version alone, as before.

**Usage:** a command the tool lacks is refused with its own list, and exits 2.

**Tests** (gate 5, less its Windows part):
- `ToolTests`, running the tool as a process: `--version`; a command it lacks; no install named; a usage error;
  `--profile` that names no profile, one that needs review, and one that runs, in a test home; `--profile` beside
  `--data-root`.
- **No `Paths`:** a scan of `engine/Ome.Tool/` and `engine/Ome.Commands/`. `EngineBuildTests.AllowedReaders` names
  `engine/Ome.Commands/`, which reads the ABI id to load the table.
- **Outside a repository:** a test copies the tool's build output to a temporary folder and runs `omp check` there,
  on a synthetic Kamidori install. `Paths` finds the repository from the program's folder, so in that copy any use
  throws. The report names the toolchain.
- The engine's refusal of `--profile` (`RunResolverTests`) and `paths.py`'s (`test_paths.py`).

**Docs:** `tools-reference.md` (a row for `OME-Tool`, and `--profile` in `Ome.Cli`); `PROJECT-STRUCTURE.md` (the two
projects); `docs/modding/cli.md` (`--profile`, the renamed `import-scripts` options); `AGENTS.md`'s line on the
guides, which calls `--profile` `--game`'s old name. The guides move to `OME-Tool` in M5.

**Built** (2026-11-24):
- `engine/Ome.Commands/` holds the six commands, `CommandInputs` and `CommandDefaults`, `ProfileOption` and
  `Utf8Console`. `omp text` moved there from `Ome.Cli` (`OmpTextCommand.cs`); the others were cut from `Program.cs`
  unchanged, apart from what the inputs now give them.
- `engine/Ome.Tool/` is `OME-Tool`: `--version`, `--help`, the refusal of a command it lacks, and the shared
  commands with `CommandDefaults.Tool`.
- `Ome.Launcher.Core` gained `ProfileLookup`, the id lookup its own command line now uses too, and is
  `IsAotCompatible`. Both new projects build with the trim and AOT analyzers on and no warning.
- `omp text po-export` and `po-import` read no install, so the tool runs them without one.
- `picture`'s option skipping also passes over `--data-root`, `--mod` and `--home`, whose values were read as
  pictures' names before.
- `Ome.Cli` exits 2 on an `ArgumentException` that reaches the top through an unhandled-exception handler, since its
  top-level statements are one long body; the shared commands catch it themselves.

**Gates:** 5, less its Windows part (`ToolTests`, 10 tests), with the engine's 3,535 tests, the launcher's 86, the
Godot build and `tools/test_paths.py`. All passed on 2026-11-24.

**Checks beyond the gate:**
- The tool's `oms-decompile --level idiomatic --output-dir` of every Kamidori script (732, the clean install) is
  byte-identical to `Ome.Cli`'s, so the embedded opcode metadata gives what the live file does.
- **NativeAOT on this machine** (`dotnet publish engine/Ome.Tool -c Release -r win-x64 -p:PublishAot=true`, with
  `vswhere.exe`'s folder on PATH): no warning, and `OME-Tool.exe` is 18.4 MB, within M0's estimate. The publish also
  writes `.pdb` files beside it, which M4 leaves out of the release. Run from the scratch folder, outside the
  repository:
  - `--version` prints `OME-Tool 0.10.1-9-ge8d17655+ (commit e8d17655a17a)`;
  - `omp check` on the clean Kamidori install links and names `kamidori-base 1.3.0`'s toolchain;
  - `omp build` of the 26 example mods gives 25 packages byte-identical to `Ome.Cli`'s, and refuses
    `example.kamidori-prologue-line` with the same message, as §2.4's snapshot does;
  - `oms-decompile` of Himegari's `SC0000.BIN` to stdout, with its Japanese text, is byte-identical to `Ome.Cli`'s.

### 3.3 Milestone 3: `locate` and `start`

**`locate`** (spec §7), in `Ome.Commands`, so `Ome.Cli` has it too:
- **The forms.** `locate SCENE PAGE` takes `3`, `P003` or `p3`; `locate SCRIPT@OFFSET` takes the `wait` or `text` part
  of a locator, with a hex or decimal offset. `--context N` (default 8) and `--map FILE` as `locate_page.py` has
  them.
- **The map, by page** (`PageMapSearch`), the first that exists:
  - `--map FILE`;
  - with a profile (`--profile`, `--data-root`), its `diagnostics/page-maps/page-map-<SCENE>.jsonl`;
  - without one, the bare engine's: `games/<namespace>/diagnostics/page-maps/` in Godot's user folder for the project
    (`MachineState.DefaultFolder`, the game's `Persistence.StorageNamespace`);
  - `Ome.Cli` only, and without a profile: the checkout's `build/games/<game>/`, where an editor build writes
    (`CommandDefaults.PageMapFolder`), looked at before the bare engine's.

  A missing map or page is refused (exit 1) with every place looked at. The first line printed names the map read.
- **The script.** The run's link, as `omp explain` makes it (`ModInspection.Inspect` with the profile's mods and the
  `--mod` packages). A script the stack patches or replaces is listed from the linked bytes, with a line naming the
  mods and the plans; a patched one adds that the base offsets hold only without them. A script no mod touches comes
  from the catalog with the profile's overlays. A mod's own new script is listed from its catalog record.
- **The listing** (`ScriptListing`): the script decoded with the ABI's table (`Sys4Loader.Parse`), each line as
  `sys4load.render_listing` gives it, which `locate_page.py` prints:
  - the mnemonic is the table's label, or its semantic name where the label is unnamed (`u00…`, `dev_ukn…`, or the
    opcode in hex), and an unnamed one is followed by `   ; op 0x…`;
  - a string operand is `"text"`; a code target `label_<hex>`; an immediate `0x…`; a typed operand
    `(<type> 0x…)`, with the type names of the metadata's `meta.arg_types`, and `<t0x… 0x…>` for a type it lacks;
  - the annotations come from what a release has: `call-script`'s target from the catalog (`=SCENE.BIN`), and a
    global's name from the base mod's layout (`=name`). `locate_page.py` reads its own from `build/` (the
    registry's JSON with categories, `callscript-names.json`), so gate 6 compares the two without the ` =…`
    annotations.
- **The report** keeps `locate_page.py`'s lines: `SCENE P003`, the page start, the last text, the wait, the call
  stack, the text, and the listing around the wait with `>>>` on it. By site it prints the listing alone.

**`start`** (spec §8), in `Ome.Commands` and the tool only (`Ome.Cli` has no engine beside it):
- **The command line.** `--profile` or `--data-root` names the profile (`ProfileLookup` for the id; a folder goes
  through `ProfileStore.Open` and its trust, as the launcher's `--launch DIR` does). `--mod PATH` repeats; the
  authoring flags (`--lua-dev`, `--lua-trace`, `--lua-reload`, `--lua-errors MODE`, `--text-log`, `--locator-hud`)
  pass to the engine; `--engine-arg ARG` repeats and goes before Godot's `--`; whatever follows a `--` goes to the
  engine unchanged, and `EngineCommands.For` refuses `--data-root` there.
- **The checks.** `CheckOptions` gains `Mods`, which the Link check links with the profile's own
  (`LinkComposition.Link`). A failed check is printed as the launcher's `--check` prints it, and stops the start
  (exit 1).
- **The engine** is `OME.exe` (`OME` elsewhere) beside the tool (`EngineProgram.BesideLauncher`, given the tool's own
  folder), with `--engine-arg`s. `EngineLaunch.Run` gains a line callback, which `start` prints to stdout as each line
  comes, and the extra environment the control channel needs. The log keeps the launcher's `[launcher]` lines.
- **Ctrl+C** (§2.3, corrected below). `start` passes `--control --control-input shared --control-file
  <temporary>/session.json`, with a token of its own in `OME_DRIVE_TOKEN`. On the first Ctrl+C it sends `hello` and
  `close` over the channel and waits 10 seconds; then, or on a second Ctrl+C, it ends the process. The log's last
  `[launcher]` line says which.
- **`--control-input shared`** is new in the engine (`godot/Main.Drive.cs`). §2.3 missed that `--control` takes the
  player's input: the port drops every real event, redirects the cursor warp and logs AGE's boxes instead of
  showing them, since a driven run has nobody at it. With `shared` the channel serves its requests and the run is
  otherwise a player's.

**Tests:**
- Gate 6, core: `ScriptListing` and `locate`'s report against `locate_page.py`'s on a synthetic script and map (the
  Python run is in the test, as other tests run Python), with `P003`, the site form, a missing page, the map search
  and a patched script.
- Gate 6, workspace: each installed game's page maps that the checkout's `build/games/<game>/` holds, every page
  against `locate_page.py`; and, since a few maps reach few instructions, every script's whole listing against
  `locate_page.py --digests`.
- `start`, core: with the stub engine (`Ome.Launcher.StubEngine`) beside a copy of the tool's build output: the
  lines relayed and in the log, the exit code, a `--mod` that fails the Link check stopping it before the engine
  runs, and `--data-root` refused after `--`. Ctrl+C is checked by hand on Windows, and on Linux in the smoke
  (gate 7, M4).

**Docs:** `tools-reference.md` (the tool's table, the engine's `--control-input`); `docs/modding/cli.md`;
"Runtime page-location handoff" in `AGENTS.md` names `locate`; the driving design's control channel section.

**Built** (2026-11-24):
- `engine/Ome.Commands/` gained `ScriptListing`, `LocateCommand` and `StartCommand`. `locate` is a shared command;
  `start` is dispatched by `OME-Tool` alone, and `Ome.Cli start` points to `run-godot` (exit 2).
- `OpcodeContracts` keeps the metadata's operand type names (`OperandTypeNames`), which the listing prints.
- `CommandInputs` gained `PageMapFolder` (`Ome.Cli`: `build/games/<game>/`) and `LinkBase`, which `omp` and `locate`
  link with. `PageLocatorState` names the export's map folder (`MapFolder`) and makes `NormalizeScript` public.
- `Ome.Launcher.Core`: `CheckOptions.Mods`, which `LinkComposition.Link` links after the profile's own;
  `EngineLaunch.Run`'s `line`, `stopped` and `passed` callbacks and the `[launcher] the engine was stopped (…)` line;
  `ProfileLookup.Checked`; `LauncherCommandLine.LocateProfile` and `PrintChecks`, public for `start`.
- The engine's `--control-input exclusive|shared` (`godot/Main.Drive.cs`, `Main.Input.cs`).
- `tools/locate_page.py` gained `--no-names`, `--listing SCRIPT` and `--digests`. Its page report is unchanged.
  `validate.py` passes `OME_PYTHON` to the engine tests, which run it.
- The tests' synthetic scripts go through `AsCompiled`, which gives them the header the Python loader requires
  (F6 = 0x1C, and F8, F10 and F12 at the body's end). `ScriptAssembler` lays them out otherwise, and 114 test files
  use it, so it stays as it is.
- The driving design (`2026-11-03-game-driving-design.md`) stays as built; the flag lives in `tools-reference.md`,
  "Port control channel".

**Gates:** 6, core (`LocateTests`, 13 tests) and workspace (`LocateWorkspaceTests`, one per game), and `start`'s
(`StartTests`, 10 tests), with the engine's 3,561 tests, the launcher's 86, the Godot build,
`tools/test_validate.py` and core validation. All passed on 2026-11-24.

**Checks beyond the gate:**
- Every script lists identically in C# and Python: Himegari's 481, Kamidori's 732 and Arterial's 693.
- The workspace gate's page maps came from a port run of each game's opening (`drive.py port`, 30 `advance`s
  from SC0000's first page), whose pages the maps keep under the root scene, `SYSTEM4` (Himegari 30 pages, Kamidori 31, Arterial 31; with an older map of Himegari's SC0000, 95). With names,
  `Ome.Cli locate SYSTEM4 12` on Himegari's map names the map and its time, then gives `locate_page.py`'s report,
  but for the names' categories, which the base mod's layout lacks (`=adv_gfx_layer_index` where the Python
  tool prints `=adv_gfx_layer_index(index-pointer)`).
- **Ctrl+C on Windows**, by hand: a scratch harness ran the tool from a folder with a stand-in `OME.exe`, which ran
  the checkout's port, and sent `CTRL_C_EVENT` to the tool's console. The first press closed the port over the
  channel in about half a second (exit 0, `the engine was stopped (Ctrl+C: asked to close)`). With the stub engine,
  which has no channel, one press ended it after the 10 s grace (`it did not close within 10 s (its control channel
  had not started), so it was ended`), and a second press ended it at once (`a second Ctrl+C: ended`). The engine
  runs on a console of its own (`CreateNoWindow`), so the press reaches the tool alone. On Linux it reaches the
  engine too, which shares the terminal's process group; gate 7 checks that in M4.

### 3.4 Milestone 4: the release

**The files** (spec §6.4):
- **`tools/publish_engine.py`** publishes the tool after the library, in the same run and under the same stamp, into
  `build/engine-publish/tool-<rid>/`. It refuses any file but the program and the runtime's two notices in
  `OME-Tool-notices/` (`tool_files`). The tool's project keeps its dependency list (`KeepDependencyList`,
  `tool-<rid>.deps.json`). `--place-tool DIR` checks the stamp and the files, and copies them beside the engine in an
  export.
- **The workflow** names the engine jobs' step "Publish the engine library and the tool". Their upload of
  `build/engine-publish/` and the package jobs' download already carry the new folder.
- **`tools/build-*-x64.sh`** run `--place-tool` after the launcher's publish, before the payload is verified.
- **The packagers** require the tool's files and refuse the other platform's tool.
  - Linux: `OME-Tool` executable, an x86-64 ELF program that needs glibc alone, at most 2.28 (`verify_tool`, which
    shares the library's check, `_verify_aot_binary`).
  - Windows: `OME-Tool.exe` an AMD64 PE with the console subsystem, importing what the library may
    (`verify_windows_native.verify_tool`). It goes into `WINDOWS-VERIFICATION.json` as `tool`.
  - Both: the notices check reads the tool's list (`package_notices.verify_tool_notices`, `--tool-deps`).
- **`publish_github_release.py`** validates the report's `tool` section and requires the tool in both ledgers. The
  release notes name the three programs and the tool's checks. It requires the smoke's fourth line.
- **Linux's smoke** gains the tool's step (`run_tool`), on the released copy, after the launcher's check and before
  the hand-over, which replaces the copy's launcher. It writes `TOOL OK: gates=2,5,7` (`SMOKE_MARKERS`; the step is
  in `tools-reference.md`):
  - gate 5: `--version` names `BUILD-INFO.json`'s commit; a command the tool lacks exits 2 with its list;
  - gate 2: on the smoke's install, which also lists `SMOKE.BIN`, `oms-compile` writes that script, `oms-patch
    --fingerprint` gives its header, and `omp check` links a fragment on it that `use`s `himegari.globals` and
    `himegari.adv`, with the toolchain line naming the base mod's three files;
  - gates 2 and 7: `start --mod FRAGMENT --engine-arg --headless -- --quit-after-link` has the engine link the
    fragment itself; the tool relays its lines and its exit 0, and `engine.log` keeps them. With `-- --game-root`
    the engine refuses, and the tool relays its exit 2. A `--mod` that requires a missing mod stops the start
    before the engine runs;
  - Ctrl+C (§3.3), on `--package-smoke --hold`, below.
- **The port's `--quit-after-link`** (`godot/Main.cs`): startup ends once the stack has linked. `--package-smoke`
  returns before the engine resolves a game, so it cannot link a mod (§4).
- **The README** names the tool and the guides' address.

**Ctrl+C on Linux.** The smoke runs `start` in a process group of its own, as a terminal does, and signals it once the
engine has printed its smoke line. The smoke runs before the control channel starts (`Main.cs`), so the close
request fails and the tool ends the engine after its grace, which is the same on every run. Two runs:
- **SIGINT to the tool alone** checks the tool's path: it says it closes the engine, ends within 30 s, and
  `engine.log` ends with `the engine was stopped (Ctrl+C…`.
- **SIGINT to the process group** is what a terminal sends. It reaches the engine too, and which process ends first
  decides what the tool prints: a dead engine can end the run before the tool's handler names the reason. So only the
  end is checked: the tool ends within 30 s.

Neither run may leave a process of the group running (`/proc`, zombies aside, 5 s after the tool ends).

**Gates:** 2, 5 (Windows) and 7, on CI: the Windows package job's `verify` runs gate 5's Windows part, and the Linux
job's smoke runs gates 2 and 7 with gate 5's version and refusal. They need the branch pushed and the release
workflow dispatched.

**Built** (2026-11-24): the files above, with tests:
- `test_publish_engine.py` covers the tool's publish command, its file check, the stamp, `--place-tool`, and the
  executable bit on POSIX;
- the packager tests cover the tool's files, its ELF and PE checks, its notices, a forbidden import, the smoke's four
  steps in order and the tool step's checks;
- `test_publish_github_release.py` covers the report's `tool` section, the ledgers and the fourth line;
- `test_release_workflow.py` covers the build script's marker loop.

**Checks beyond the gate:**
- A win-x64 publish on this machine: `OME-Tool.exe` 19.4 MB, no `.pdb` (§4), AMD64 with the console subsystem. Its
  list names `LuaCSharp`, `LuaCSharp.Annotations`, `Microsoft.NETCore.App` and `Tomlyn`.
- The tool's step (`run_tool`) on Windows, against a folder with M3's tool and a stand-in `OME.exe` that ran the
  checkout's port, passed with Ctrl+C left out: the version, the refusal, the compile, the fragment's link, the
  engine's own link (`[mods] layer SMOKE.BIN: script from smoke.fragment (base, patched by smoke.fragment)`), the
  relayed refusal and the stopped start. Ctrl+C on Linux runs in CI only.

## 4. What changed since the spec

- **Gate 2** (spec §12) cannot link a fragment at a named patch point on the smoke's synthetic install, since a point
  resolves only on the real script's revision (§2.2). The smoke checks that the base mod's points load, and links a
  fragment that uses `<game>.globals` on a script it compiles itself. A fragment at a named point is linked by the
  workspace step of gate 8 instead, on the installed game, from the release archive outside the repository.
- **Ctrl+C in `start`** (spec §8) closes the engine over its control channel first, and ends it only after a grace
  period or a second Ctrl+C (§2.3). The spec left the clean quit open.
- **`Ome.Launcher.Core` is marked `IsAotCompatible`** in M2, after M0 found it compiles that way with no warning.
- **Gate 3** (spec §12) cannot hold Kamidori's decompiled scripts equal. M1 loads the 44 cells the old loader
  skipped (§1), so the decompiler names them. The gate now asks that Himegari and Arterial decompile identically,
  that Kamidori's changes be only those names and what follows from them, and that the example builds be
  byte-identical (§2.4).
- **The link report names the toolchain in a header line**, `toolchain: …`, after the load order. A note would change
  the reports' note counts, which the tests and the launcher read.
- **The port's startup catch stays** (`godot/Main.cs`). M1 removes its one cause in the toolchain, but the catch
  also covers the mod layer's I/O.
- **A new game starts its base mod at `adding-a-game.md`'s script step**, since its OMS module lives there: a
  manifest of `omp = 5` with `[game]` and `[lua] capabilities = ["vm"]`, and `oms/adv.oms`. Its Lua parts come at the
  modding track's third level, as before.
- **The launcher's Link check** shows a toolchain that does not build as a warning, naming what a mod that compiles
  would meet, when the stack links. A stack with a mod that compiles fails as it did, with the error in its report.
- **The module's names join the rename rule** (§3.1), which spec §5.2 left to this plan.
- **`OmpCommands` stays in `Ome.Mods`** (§3.2). Spec §6.1 moves the commands' code out of it with the rest, but the
  launcher's Link check already calls it, so it is shared as it is. What moves is `Ome.Cli`'s handling around it.
- **`--version` names the commit** from a second stamp, `OmeCommit` (§3.2), since `git describe` of a tagged commit
  is the tag alone.
- **`--control` takes the player's input** (§3.3): M0's §2.3 chose the control channel for Ctrl+C without seeing that
  the port, under `--control`, drops every real input event. `start` passes `--control-input shared`, new in the
  engine, which keeps the channel and leaves the run a player's.
- **The engine links gate 2's mod under `--quit-after-link`**, a port flag new in M4 (§3.4), not in
  `--package-smoke` as spec §12 says: the package smoke returns before the engine resolves a game.
- **Ctrl+C on Linux is checked in two runs** (§3.4): to the tool alone, its exact path, and to the process group,
  only its end. The order in which a terminal's SIGINT ends the two processes is not fixed, so the lines are not
  checked there.
- **A NativeAOT publish copies a stale native `.pdb`** from `obj/` even with `DebugType=None`, so the tool's publish
  passes `-p:CopyOutputSymbolsToPublishDirectory=false` (§3.4).
