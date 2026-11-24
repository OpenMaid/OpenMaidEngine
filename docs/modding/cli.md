# Commands and installing mods

This page is the reference for the mod tool's commands, the engine's mod options, and where installed mods live. The
mod tool is `OME-Tool` (`OME-Tool.exe` on Windows), beside the engine in a release's folder. Run it in a terminal in
that folder: type `.\OME-Tool` in PowerShell on Windows, `./OME-Tool` on Linux. `OME-Tool --help` lists its commands,
and `OME-Tool --version` gives the release's version and commit.

Every command names the game with a player profile or with the game's options:

| Option | Meaning |
|---|---|
| `--profile <id>` | a player profile, by its id in OME home: its game, install, overlays and mods, as the launcher runs it. `--home DIR` names another OME home. A profile the launcher asks you to review is refused until you accept it there |
| `--data-root DIR` | a profile's folder, with the same effect (see "Installing mods") |
| `--game <game>` | the game, without a profile; without it, `himegari` |
| `--game-root DIR` | the install, without a profile. A release needs it then |
| `--overlay-root DIR` | a loose-file layer over the install, such as a translation, relative to the game root (repeatable) |

A profile's id is the name of its folder, which its Overview page in the launcher shows. With a profile, `--game`,
`--game-root` and `--overlay-root` are refused, since the profile names them ([troubleshooting](troubleshooting.md)).
`--profile` was `--game`'s old name until 2026-11-24, and an old command line now fails with a message: an id that
names no profile says where it looked and that the game is `--game`.

Your game guide lists what its installs need, such as a translation's overlay (the [games page](games/README.md)
links each guide).

The tool exits 0 on success, 1 when a check fails, and 2 for a command line it cannot read or a stack that does not
link.

## `omp`: packages

| Command | What it does |
|---|---|
| `omp check [--mod PATH …] [--skip-failed-mods]` | Links the mods and prints the link report, each mod catalog's records with their packed ids, and a summary. It writes nothing. It exits non-zero if the stack does not link. |
| `omp build DIR [--output FILE.omp] [--mod DEP …]` | Validates and links the mod folder `DIR`, compiles every fragment, and writes a reproducible `.omp` with the plans stored inside (default `<id>-<version>.omp` beside the folder; the output's folder is made if it is missing). The folder is not changed. Pass the mod's dependencies with `--mod`. It refuses a Lua mod whose source writes a base mod's name its `[lua] writes` list lacks ([the Lua guide](lua.md), "Game names"), and a mod whose data files do not link ([data files](data-files.md)). |
| `omp export --output-dir DIR [--mod PATH …]` | Links the mods and writes what native AGE loads: patched and replaced scripts and asset overrides as loose files, and one `APPENDnn.AAI`/`.ALF` pair per mod catalog. It refuses a stack that does not link, and a mod whose target is not `native` (a Lua mod targets `extended`). It leaves out `text/`, `fonts/`, language assets (`assets/<language>/`) and `[[language]]`, and a Lua library's `lua/`, with a note, since native AGE cannot use them. |
| `omp explain NAME [--mod PATH …]` | Says which layer and mod supplies a script or asset, such as `SC0000.BIN`, `$2$SCENE.BIN` or `my.mod:NAME`, and lists every plan applied to a script. A base entry shows its loose file or its range in the ALF archive. An asset with language versions (`assets/<language>/`) also lists each one, the language it shows in, and whether a later package's plain asset hides it. |
| `omp lua-stubs --output DIR [--mod PATH …]` | Links the mods as `omp check` does and writes lua-language-server definitions into `DIR`: `maid.lua`, `events.lua` and one `<base mod id>.lua` per base mod, with its helpers, the game's included when no mod mounts it. It prints the `.luarc.json` line that points an editor at them. Beside them it copies each accepted kind's shape as `<kind>.schema.json` (the game's `<game>.tables` included, as its base mod ships it) and prints, for each, the `#:schema` line that points a data file at it ([data files](data-files.md), "Shapes"). It writes nothing if the stack does not link ([the Lua guide](lua.md), "Editor definitions"). |

`--mod` takes a folder or an `.omp` file and repeats. With a profile, `omp check`, `export`, `explain`, `lua-stubs`
and `omp text check`/`stats` link the profile's mods too, in its load order and with its kept selectors, and the
`--mod` packages after them: they link what the profile would run with those packages added. Without a profile only
the `--mod` packages link, and selectors are allocated fresh.

**Exports and saves.** A mod catalog's selector is part of every packed id in a save. To export a stack whose
native saves must match saves made in the port, export with the profile the saves were made in, so the export uses
the same selectors.

## `omp text`: translations

| Command | What it does |
|---|---|
| `omp text extract --language TAG --output DIR` | Writes catalog templates for language `TAG` into the package folder `DIR` (`DIR/text/TAG/`), from the install's scripts: one file per script with its messages and patterns, and `_global.toml` with every literal the scripts draw or store. Run again, it keeps every entry already there, adds templates for new sources, marks `obsolete = true` the entries whose source is gone, and rewrites only the files that changed. For a language the engine has no table of its own for (it has `ja` and `en`), it also adds to `_engine.toml` an `[[engine]]` template for each engine key no file there sets ([languages](languages.md), "Engine text"). A file there that does not read stops it before it writes anything. |
| `omp text check [--mod PATH …] [--text-root DIR] [--language TAG] [--no-scripts]` | Links the packages as `omp check` does (printing its report), then validates every language's catalogs, the `--text-root` folder's last. Errors: a file that does not read, two patterns that can match one string, too many entries, a stack that does not link, an `[[engine]]` entry whose text names a value its key does not take or does not format. Warnings: entries whose source the installed scripts do not draw or store where their scope reaches, site entries that do not bind, characters no bundled font covers, a table whose first font is bundled and proportional to the game (with the width the game draws it at), `[[engine]]` keys the engine does not have. `--language` checks one language; `--no-scripts` skips reading the install. Exits non-zero on an error. |
| `omp text stats [--mod PATH …] [--text-root DIR] [--language TAG] [--scripts]` | Counts, per language, the messages, strings and composed strings of the installed scripts that its catalogs and their fallback chain translate. `--scripts` adds a line per script. |
| `omp text po-export --language TAG (--package DIR \| --text-root DIR) --output FILE.po` | Writes language `TAG`'s catalogs (`DIR/text/TAG/` of a package folder, or `DIR/TAG/` of a text root) as one gettext PO file: a message per entry with its scope as `msgctxt`, its `note` and origin as comments, templates with an empty `msgstr` and obsolete entries as `#~` messages. `[[engine]]` entries have the context `engine:KEY` and the engine's Japanese as `msgid`. |
| `omp text po-import --language TAG (--package DIR \| --text-root DIR) --input FILE.po` | Puts the PO file's translations into those catalogs, entry by entry, keeping every other key and each file's header comments, and rewrites only the files that changed. Empty, fuzzy and obsolete messages are left out, and a message no entry has is a warning. A translation that does not fit its entry stops it before it writes anything. |
| `omp text import-scripts --from GAME_ROOT [--from-overlay-root DIR] [--from-allow-bmp-as-agf] --output DIR [--language TAG] [--package] [--verify] [--compare-archives]` | Builds language `TAG`'s catalogs (default `en`) in `DIR/text/TAG/` from the install at `GAME_ROOT`, whose scripts were translated in place, against the selected install. It replaces the `.toml` files already in that folder and writes `DIR/import-report.json`. `--from-overlay-root` and `--from-allow-bmp-as-agf` apply to the translated install; the selected install is read without overlays. `--package` also writes the images the translation replaced to `DIR/assets/TAG/`, each AGF as a PNG and replacing the images already there. When `DIR` has no `manifest.toml`, it writes one with a `[[language]]` table. `--verify` checks every message against the translated scripts and exits 1 on a mismatch. `--compare-archives` compares archives by content rather than size ([languages](languages.md), "From a translation made in place"). |

The catalog format and the workflow are in [languages](languages.md).

`picture (NAME… | --all) [--output-dir DIR]` writes the game's pictures as PNG, decoded as the port draws them: a
name is the file name the port's F5 list shows (an append's with its `$n$` prefix), and `--all` writes every one.
A movie under a picture's name is refused. Without `--output-dir` they go to `pictures/` in the current folder.
The pictures a translation replaces: [the translation walkthrough](translating.md), step 8.

## `locate`: from a page to its script

`locate` turns what the port's page locator shows (**F2**, **F3** copies it) into the script and instructions behind
it:

| Command | What it does |
|---|---|
| `locate SCENE PAGE [--context N] [--map FILE] [--no-names] [--mod PATH …]` | Finds the page in the run's page map, such as `locate SC0000 3` or `locate SC0000 P003`, and prints where it starts, its last text instruction, the wait, the call stack, its text and the instructions around the wait, marked `>>>`. The first line names the map it read. |
| `locate SCRIPT@OFFSET [--context N] [--no-names] [--mod PATH …]` | Prints the instructions around an offset, such as the `wait` or `text` part of a locator: `locate SC0000@0x2a91`. It needs no map. |

The map is the one the profile's run wrote, in its `diagnostics/page-maps/`. `--map FILE` names another. A page
number counts the pages of that run, so it can change when the run took another branch; the offset does not. If the
map lacks the page, run the game to it again.

With a profile or `--mod`, `locate` lists the script the run had. A line starting `script:` says when a mod patches
or replaces it, and for a patched script, the wait's offset in the installed script, which is the offset a fragment
names ([the patching guide](patching.md)). `--no-names` leaves out the names of globals and called scripts.

## OMS tools

| Command | What it does |
|---|---|
| `oms-patch --fingerprint SCRIPT.BIN` | Prints the `patch "…" fingerprint "…"` header for a script as installed. Use the full file name. |
| `oms-decompile --script SCRIPT.BIN [--level idiomatic\|raw] [--output FILE.oms]` | Prints a script as OMS. `idiomatic` names everything the modules know, which is what you want for reading. |
| `oms-patch --source F.oms … [--output-dir DIR]` | Compiles fragments outside a mod, and optionally writes the patched scripts to `DIR`. |
| `oms-compile --source F.oms [--output F.BIN]` | Compiles one script unit to a `.BIN`. |

## `start`: running a profile with mods

`start` starts the game on a profile, as the launcher's Play does, and shows the engine's output in the terminal:

```text
OME-Tool start --profile <profile> --mod path/to/my.mod --lua-trace
```

| Option | Meaning |
|---|---|
| `--profile ID` or `--data-root DIR` | The profile to run (one is required). `--home DIR` names another OME home. |
| `--mod PATH` | Also load this mod (folder or `.omp`) for this run. Repeatable. Loaded after the profile's mods, unless its manifest orders it otherwise. |
| `--lua-dev`, `--lua-trace`, `--lua-reload`, `--lua-errors MODE`, `--text-log`, `--locator-hud` | The engine's flags of the same name, below. |
| `--engine-arg ARG` | Pass one more argument to the engine. Repeatable. |
| `-- FLAG …` | Pass everything after `--` to the engine, such as `-- --language en`. |

It runs the launcher's checks first, with the `--mod` packages in the Link check, and starts nothing when one fails
(exit code 1). It prints the engine's output as it comes, and the profile's `diagnostics/engine.log` keeps the same
lines. It exits with the engine's exit code. Ctrl+C closes the game, and a second Ctrl+C, or ten seconds, ends it at
once. Closing the game's window ends `start` too. A tool ended some other way, such as from the task manager, leaves
the game running: close it as usual.

## The engine's mod flags

`start` passes the ones it names itself, and the others after `--`:

| Option | Meaning |
|---|---|
| `--mod PATH` | Also load this mod (folder or `.omp`). Repeatable. Loaded after the installed mods, unless its manifest orders it otherwise. |
| `--no-mods` | Load no mods at all, not even installed ones. |
| `--allow-network-paths` | With a profile file, open the network paths (`\\host\share\…`) it names for this run without the launcher's approval ([troubleshooting](troubleshooting.md), "Startup refusals"). |
| `--skip-failed-mods` | If the stack does not link, drop the mods involved, say so, and start with the rest. Without it a failed link stops startup (exit code 2). |
| `--quit-after-link` | Link the stack as a start does, print its load order and layer, and quit with exit code 0 (2 if it does not link). It checks the link the engine itself makes; `omp check` prints the whole report. |
| `--forget-mod ID` | Release a removed mod's selector (see below). Repeatable. |
| `--language TAG` | Present the game's text in `TAG` for this run, whatever `OME.INI` says: the catalogs the mods (and `--text-root`) hold for it. `ja` draws the scripts' text as written; `x-pseudo` brackets and lengthens every string, with no catalog. The menus and dialogs take the mods' `[[engine]]` entries for `TAG`, then the engine's own table for it, along its fallback chain, then Japanese ([languages](languages.md), "Engine text"). |
| `--text-root DIR` | Also load a loose folder of catalogs laid out as a package's `text/`, after the mods' ([languages](languages.md), "Testing without a package"). |
| `--lua-errors continue\|halt` | What a Lua handler's error does: `continue` (the default) logs it to `diagnostics/lua.log`, shows a notice and switches the handler off after its third failure in a row; `halt` ends the run at that instruction ([the Lua guide](lua.md), "Errors and budgets"). |
| `--lua-dev` | While you write a mod: a mod loaded from a folder may write a base mod's name its `[lua] writes` list lacks, and `diagnostics/lua.log` names each such name once. A packed `.omp` keeps its list ([the Lua guide](lua.md), "Game names"). |
| `--lua-trace` | Write one JSON line per Lua handler call to `diagnostics/lua-trace.jsonl`: where it ran, how long it took, the event's fields before and after, the write a watch saw, every cell it wrote, and the scripts it called ([the Lua guide](lua.md), "The link report and the trace"). |
| `--lua-reload` | While you write a mod: when a `.lua` file under the `lua/` of a mod loaded from a folder changes, check the mod's files and rebuild the Lua session at the next instruction, keeping `maid.state`. The startup log's `[lua] --lua-reload watches` line names the folders. A packed `.omp` is not watched, and a change to the manifest or `events/` needs a restart ([the Lua guide](lua.md), "Reloading"). |
| `--text-log` | Write every string drawn without a catalog entry to `diagnostics/text-misses.jsonl`, every translated message narrowed to fit its window, with `width_percent`, the width it got, and every one cut at the bottom of its window, with `rows_cut`, the rows it lost. |
| `--locator-hud` | Show the page locator from the start, as **F2** does. |

At startup the engine prints `[mods]` lines with the load order and every file the mods supplied, and a `[lua]`
line when a mod ships Lua. The full report is `diagnostics/mod-link.txt` in the profile, and Lua's log
`diagnostics/lua.log`.

## Installing mods

The launcher installs mods. On a profile's Mods page, **Add a mod…** copies a package (an `.omp` or a folder with a
`manifest.toml`) into the shared library, and its switch turns it on for that profile. The page shows each
package's state, the load order (drag to change it), and the link report after every change. A package is stored
once however many profiles use it. Removing it from the library lists the profiles that still use it.

A profile is a folder in OME home, `<OME home>/profiles/<id>/`, or wherever the launcher was told to put it. OME home
is `%APPDATA%\OpenMaidEngine` on Windows and `$XDG_DATA_HOME/OpenMaidEngine` on Linux (`~/.local/share/OpenMaidEngine`
when `XDG_DATA_HOME` is unset). When a folder `data/` sits beside the launcher, that folder is OME home instead. The
library is `<OME home>/mods/<game>/`, and `mods/common/` holds the packages with no `[game]` table. In a profile's
folder:

| Path | Contents |
|---|---|
| `profile.toml` | What the profile is: its game, install, overlays, mode and mods. The launcher writes it; the engine only reads it. |
| `mods/selectors.toml` | Written by the engine: the selector each mod with new scripts or assets was given. Do not edit it. |
| `mods/.cache/` | The linked results, each reused while nothing changes, and only on the machine that linked it: a copied profile links once again. An entry is linked again when a base script its link read is not the install's any more (a fan patch's loose files, say) or its kept data files no longer read as the link read them, with a `[mods] link cache entry cannot be used` line in the log. A link keeps the 8 most recent and removes older ones, but none younger than an hour. Safe to delete at any time. |
| `SAVE/` | Saves. With mods loaded, each `SAVEnn.DAT` gets a `SAVEnn.mods.toml` stamp beside it. A save made while a mod ships Lua, or after loading a save that had one, gets `SAVEnn.modstate.json`, the mods' `maid.state` ([the Lua guide](lua.md), "State"). Copy a save with every `SAVEnn.*` file. |
| `diagnostics/` | `mod-link.txt` (the last link report), `mod-save-stamps.txt` (save-stamp warnings), `engine.log` (the engine's output in the last run that Play or `OME-Tool start` began), `page-maps/` (the page locator's maps), `lua.log` and, with their flags, `lua-trace.jsonl` and `text-misses.jsonl`. The launcher's Advanced page has "Open the diagnostics folder". |

`profile.toml`'s `[mods]` table names the profile's mods, as the Mods page sets them:

- **`folders`** lists the folders the packages come from. Several profiles may share one, such as a library of
  voice packs.
- **`order`** lists the mods that load: an id loads, `-ID` keeps a place without loading. A mod in a folder that
  `order` does not enable is never opened, so a broken file in a shared folder stops no profile that leaves it off.
- **The engine's base mods** are not listed. A game's base mod joins when an enabled mod requires it.
- **`selectors.toml` and `.cache/`** stay in this profile's `mods/`, so profiles that share a folder keep their own
  selectors.
- **The profile's `mode`**: `native` (Compatibility) refuses a mod that targets `extended`, ships Lua, or requires
  such a mod, the base mod included. `extended` takes every mod.

The errors it can raise are in [troubleshooting](troubleshooting.md), "Startup refusals".

**Selectors.** Removing a mod leaves its selector reserved, so reinstalling it later restores saves made inside its
scripts. When you are sure it is gone for good, release it on the Mods page (**Release**, under "Advanced"), or
start once with `--forget-mod ID`.

**Save stamps.** When you load a save, the engine compares the save's stamp with the current mods. It reports added,
removed, reordered and re-versioned mods and changed selectors, in its output and in
`diagnostics/mod-save-stamps.txt`. It warns and never blocks. A save without a stamp was made without mods, or by
native AGE.

**A folder without a profile file.** The engine also runs a data folder that has no `profile.toml`, with the game
named on its command line (`--game`, `--game-root`). Such a folder holds its installed mods itself: each `.omp` and
each folder with a `manifest.toml` in its `mods/`, in the order `mods/load-order.txt` gives (one id per line; `-ID`
disables an installed mod; mods it does not list load after the listed ones, by id). The commands read it with
`--data-root DIR`.

## Running mods in native AGE

Native AGE has no mod loader. Export the stack and put the files into a **copy** of the game:

```text
OME-Tool omp export --output-dir my-export --mod path/to/my.mod --profile <profile>
```

Copy the install's folder, put every file the export wrote into the copy's folder beside `AGE.EXE`, and run the
copy's `AGE.EXE`. Native AGE reads loose `.BIN` files before its archives, and mounts every `APPENDnn` catalog beside
it (`nn` is the mod catalog's selector). Native AGE does not check versions, conflicts or save stamps; the export
already resolved those.

## Automated testing

**From source:** `tools/drive.py` drives the port and native AGE with one step language, and takes the mod in its
`start` command:

```text
py -3.11 -X utf8 tools/drive.py --game <game> port start --mod path\to\my.mod
```

Extra port options go through `--port-arg=…`, one per argument; your game guide has the full command for its
installs. `--package DIR` drives a release's own engine instead of the checkout's. Each start uses an empty save
sandbox, and `native` makes a scratch copy of the install (`tools/native_scratch.py`). The
[tools reference](../tools-reference.md), "Game driver", has the steps and the native side, and the repository's
[driver scenarios](../../tools/drive/scenarios/) are working examples (those named `*-mod-*.steps`).

`lua [mod=ID] [timeout=S] CODE` runs a Lua chunk in the running port and returns its result: the console of
[the Lua guide](lua.md). It is a port step, like `language`. A chunk that fails fails the step, so `lua mod=ID
assert(CONDITION, MESSAGE)` checks a mod's state in a scenario.

`rand-seed SEED` makes a run's random draws repeat: the game's own, on both targets, and in the port each Lua mod's
`math.random`, from a stream of its own ([the Lua guide](lua.md), "Random numbers"). Its result counts the draws since
the last `rand-seed`, per call site and per mod; `rand-seed off` ends it.

`language TAG` switches the port's language as the settings panel's OME section does, and fails for a language no
installed package defines (other than `ja` and `en`). Native AGE has no language, so a scenario that uses it runs on
the port only; the scenarios' `port/` folder holds those.

`pictures` lists the picture files on screen as the port's F5 does ([the translation walkthrough](translating.md),
step 8). It is a port step too.

## Where the toolchain comes from

Fragments and new scripts compile against the game's OMS toolchain: `<game>.globals`, `<game>.adv` and the named
patch points. Each is a file of the game's base mod, `<game>-base` ([packages](packages.md), "Base mods"), and
the engine and every command read the copy built into the program. So a release and a source checkout compile a mod
alike, and a package compiles the same wherever it sits. The link report names the toolchain on its `toolchain:`
line.

If the toolchain does not build against an install, for instance because the install lacks a script `<game>.adv`
runs, only what compiles is refused: each fragment and new script reports why. Asset-only mods, whole-script
replacements, and fragments whose stored plan matches still link ([troubleshooting](troubleshooting.md),
"The game's toolchain does not build").

## From source

The guides start from a release. In a checkout of the repository, the same steps map as follows:

| From a release | From source |
|---|---|
| `OME-Tool X` | `dotnet run --project engine/Ome.Tool -- X` from `age-reimpl/`, or `Ome.Cli X` (`dotnet run --project engine/Ome.Cli -- X`), which has developer commands besides ([the tools reference](../tools-reference.md)) |
| `OME-Tool start …` | `.\run-godot.cmd` and its switches: `-Mod PATH` (repeatable), `-NoMods`, `-Language TAG`, `-TextRoot DIR`, `-TextLog`, `-LuaErrors continue\|halt`, `-LuaDev`, `-LuaTrace`, `-LuaReload`, and the game switches your game guide gives |
| the profile's `diagnostics/page-maps/` | `build/games/<game>/`, and `tools/locate_page.py` reads it too |
| `--game-root` required without a profile | defaults to `games/<game>/` beside the repository |
| the release's prerequisites | the .NET SDK (`global.json`), Godot 4.7 .NET, and one `dotnet build engine/OmeEngine.sln` |

Without a profile, a checkout's port keeps one data folder per game under Godot's user data
(`%APPDATA%\Godot\app_userdata\Open Maid Engine\games\<game>\` on Windows), laid out as "A folder without a profile
file" above.
