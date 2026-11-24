# Modding from a release — implementation plan

**Date:** 2026-11-24
**Spec:** `docs/superpowers/specs/2026-11-24-modding-from-a-release-design.md` (cited as "spec §n"). The spec stays the
design's home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what
changed since the spec was accepted (§4).
**Status:** milestone 0 measured (2026-11-24, §2); milestone 1 built (2026-11-24, §3.1), its gates passed (§2.4);
milestone 2 built (2026-11-24, §3.2), its gate passed; milestone 3 next. §3 gets each later milestone's section
before it starts.
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
