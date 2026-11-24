# Modding guide

This directory is the modder's documentation for Open Maid Engine. It covers writing mods in Open Maid Script (OMS),
packaging them as Open Maid Packs (OMP), and running them in the Godot port and in the original `AGE.EXE`.

The guides are the same for every supported game. Where a guide writes `<game>`, use your game's id.
Game-specific facts (fingerprints, worked-example values, quirks) live only in the game guides: the
[games page](games/README.md) lists the supported games, each with a generated identity block and a link to its
guide.

## What you need

Each guide starts from three things:

- **a release** of Open Maid Engine, `OME-windows-x64.zip` or `OME-linux-x64.tar.gz`, unpacked anywhere. It holds the
  engine (`OME`), the launcher (`OME-Launcher`) and the mod tool (`OME-Tool`);
- **the game**, installed anywhere;
- **a profile for it**, made in the launcher.

The commands are the mod tool's, run in a terminal in the release's folder, and they name the game through the
profile: `OME-Tool <command> … --profile <profile>`. [Commands and installing mods](cli.md) is their reference, and
[Your first mod](first-mod.md) walks through them. A step that needs a checkout of the repository starts with
"**From source:**"; the from-source form of every other step is in one table, in
[commands and installing mods](cli.md), "From source".

## What a mod can do

A mod is one package with a `manifest.toml`. Version 1 supports four kinds of content, in any combination:

- **Patches:** small edits to an original script that leave the rest of it untouched. Examples are inserting a
  dialogue page, changing one operand, or replacing a range of instructions. Several mods can patch one script
  as long as their edits do not overlap ([patching](patching.md)).
- **New scripts:** whole scripts written in OMS. They live in the mod's own *append catalog*, as the games' own
  DLC does. The first one runs at boot, which is how a mod edits data tables. A new script can also be a full scene
  the player saves in ([new scenes](new-scenes.md)).
- **Script replacements:** a whole compiled `.BIN` that replaces an original script. Two mods replacing the same
  script conflict, so prefer patches ([packages](packages.md)).
- **Assets:** files that override an original asset by name, or new assets that a mod's scripts use.

Format version 2 adds **language packages**: translation catalogs and fonts that present the game's text in another
language without changing a script ([the translation walkthrough](translating.md) walks through one,
[languages](languages.md) has every rule). They run in the port only.

Format version 3 adds **Lua**: handlers the engine calls at a named instruction or at an engine event, with raw
access to the game's cells and state saved beside each save ([the Lua guide](lua.md)). Lua mods run in the port
only.

Format version 4 adds **data files**: TOML files of values that the mod defining their kind applies, such as edits to
a game's record tables that its base mod writes ([data files](data-files.md)). They run in the port only.

Mods cannot yet extend the engine or patch code another mod inserted. The project's [backlog](../backlog.md) and the
mod loader design's future work list what may come later.

## Port and native

Every version 1 mod targets **native** AGE bytecode. The port and the original engine run the same result:

- **In the port,** mods load at startup: those the profile has installed, which the launcher's Mods page sets, and
  those `OME-Tool start` names with `--mod PATH`. The port links them, caches the result and stamps each save with the
  mod list ([commands and installing mods](cli.md)).
- **In native `AGE.EXE`,** `omp export` writes the linked result as plain game files: loose scripts and
  `APPENDnn.AAI`/`.ALF` catalogs. You copy those into a copy of the install. Native AGE has no mod loader of its
  own, so it does not check versions or stamp saves.

Test a mod in the port first: its link report names every conflict, while native AGE fails less helpfully.

## Where to go next

| You want to… | Read |
|---|---|
| make a first mod, end to end, on any game | [Your first mod](first-mod.md) |
| know every manifest field and folder | [Packages (Open Maid Pack)](packages.md) |
| learn the language | [Open Maid Script for modders](oms-language.md) |
| edit an original script without replacing it | [Patching original scripts](patching.md) |
| add a scene, a boot-time data edit, or new assets | [New scripts, scenes and assets](new-scenes.md) |
| translate a game, step by step | [Translating a game](translating.md) |
| look up a language package's keys and rules | [Language packages](languages.md) |
| run Lua beside the game's scripts | [Lua mods](lua.md) |
| change a game's data tables with a data file, or define a kind of data file | [Data files](data-files.md) |
| look up a command or an engine flag, or install a mod | [Commands and installing mods](cli.md) |
| understand an error or a crash | [Troubleshooting](troubleshooting.md) |
| find your game's values and quirks | [the games page](games/README.md) |

Working examples live in the repository's [example mods](../../oms/examples/mods/), whose README says what each
shows. Every one of them links, and the gates that proved the loader run them in the port and in native AGE.

## Ground rules

- **Never modify the original install.** Mods load from the profile. Native tests use a copy of the install, never
  the game's own folder.
- **A fragment is tied to exact bytes.** Each patch names the SHA-256 of the script it edits. An install whose
  scripts a translation or update replaces is a different base, and needs its own fragment.
- **Text in scripts must be encodable in CP932** (Shift-JIS), since that is the games' only text encoding. English,
  Japanese and full-width characters work. Characters outside CP932 are compile errors. A language package's
  translations are Unicode, because the port draws them itself.

## Where the deeper material lives

These guides are the home for how to make mods. The design records behind them are:

- [the language design](../superpowers/specs/2026-10-06-open-maid-script-design.md);
- [the mod loader design](../superpowers/specs/2026-11-03-mod-loader-design.md);
- [the localization design](../superpowers/specs/2026-11-10-localization-design.md);
- [the Lua design](../superpowers/specs/2026-11-10-lua-scripting-design.md);
- engine facts: [the engine's reverse engineering](../engine-re.md) and
  [the script format notes](../sys4-format-notes.md);
- the generated [opcode reference](../opcode-reference.md). Each game's global and patch-point references are
  linked from its identity block on the [games page](games/README.md).
