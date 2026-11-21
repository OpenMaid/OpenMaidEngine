# Game data export — implementation plan

**Date:** 2026-11-21
**Spec:** `docs/superpowers/specs/2026-11-21-game-data-export-design.md` (cited as "spec §n"); the site's design, in
the `ome-site` repository at `docs/specs/2026-11-21-site-design.md`, is cited as "site §n". The spec stays the design's
home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what changed
since the spec was accepted (§5).
**Status:** milestones 0 and 1 built (2026-11-21). §4 gets each later milestone's section before it starts.
**Branch:** `feature/data-export`, off `develop` at `fcfc12a2` (rebased onto it on 2026-11-21, when the backlog
batch was merged); its first commit (`0848b00a`) is the spec.

Paths are relative to `age-reimpl/`.

## 1. What the code says today

The claims milestones 0 and 1 rest on, checked while planning. "Ran" marks a check made by running code, "read" one
made by reading the code or the disassembly.

- **A headless run** (read: `Ome.Engine.Vm/GameSession.cs`, `RunScene`): a new `VirtualMachine` for each run, seeded
  from the session's banks; the banks merge back after `vm.Run()` returns. Nothing turns on string-origin recording,
  and the origin records die with the run's `TextManager`. A trace sink that throws to stop the run skips the merge,
  and `ITraceSink` is observe-only by contract. The VM has `RecordsStringStores` (`VirtualMachine.TextStores.cs`)
  and `RequestStop()`. `VmOptions` has no stop at a site.
- **Origins** (read: `TextManager.Presentation.cs`): `set-string`, `mov`, `concat`, `toString` and the `lookup-array`
  copy record the storing instruction; a copy keeps its source's origin.
- **Descriptions** (read: the disassembly): ITMES dispatches on `item_category[current_item_id]`, and SHOP calls
  `item_handler_script_id[current_item_id]` at `0x3a49`. SKMES reads `current_skill_id`, and INFO6 calls
  `skill_handler_script_id[current_skill_id]` at `0xd3a`. The registry's usage notes say the same (FELLOW and INFO6 for
  skill descriptions).
- **ADDITEM** (ran: a scan of `build/games/kamidori/disasm/`): as spec §6.7 says, 143 calls, 122 with constant
  arguments (39 give, 83 take), 21 computed in 14 scripts; DEBUGADV holds 2 of the constant ones.
- **State fields** (ran: the record map, `mods/kamidori-base/lua/generated/tables.lua`): five of the item table's
  fields are saved state (`inventory_count`, `growth_exp`, `growth_level`, `costume_enhancement_grid`,
  `costume_gem_sockets`); no unit or skill field is.
- **The registry's tables in C#** (read: `GameModule.LoadGlobals`): it reads each table's `name`, `records` and
  `fields` and ignores other keys, so `refs` in `globals.json` changes nothing there.
- **`--game`** (read: `tools/paths.py`): `paths.py` takes `--game` off the command line before a tool parses it, so a
  tool cannot tell whether it was given. `entities_build.py` works on the selected game and takes `--all` for every
  game with declarations.
- **`Ome.Cli` is not shipped** (read: `tools/package_*.py`, `.github/workflows/`): no release archive or workflow
  publishes it, so a SQLite dependency of `Ome.Export`, which `Ome.Cli` alone references, stays out of the releases.
  NuGet versions are pinned in each `.csproj`.

## 2. Milestone 0: declarations (built 2026-11-21)

- **`[table.refs]`** (spec §5.3) in `tools/globals_build.py`: `lint_table_refs` (a current field of the table, a
  target that is a table or `picture`, an integer array), `refs` in the JSON's `tables`, and a "refers to" column in
  the reference doc of a table that has any. Tests in `tools/test_globals.py` (`test_table_refs`, workspace level, as
  the table lint's are).
- **Kamidori's references** on its four tables (`vm-map/globals-kamidori.toml`), each from the registry's value
  domain: a unit's natural weapon, costume, drops, equipment, skills, voice family, graphics and backlog face; an item's
  granted skill, garden product, mastery reward, icon, costume graphics and furniture sprite; a skill's bundled skills,
  family root, icon and area effect; a stage enemy's unit. `unit.canonical_character_id` is left out: its 0 means
  "itself", not "none". `docs/global-reference-kamidori.md` is regenerated.
- **`tools/entities_build.py`** and **`vm-map/entities-kamidori.toml`**: the lint of spec §5, then
  `build/games/<game>/entities.json`. Kamidori declares `item`, `unit` and `skill`, with descriptions for items
  (`handler_script_id`, seeded `current_item_id`) and skills (`current_skill_id`), the unit pictures the user chose,
  and the item icon and furniture sprite. No loader and no script relation yet: their milestones (3, 6) declare them.
- **Gates:** `entities-<game>` in `validate.py`'s registry gates (core), and the core test
  `tools/test_entities_build.py` (57 checks, synthetic sources, and every game's declarations against the repository).
- **References:** `docs/tools-reference.md` (the two tools, the test, `[table.refs]` under `globals_build.py`),
  `docs/PROJECT-STRUCTURE.md`, and the workspace `AGENTS.md` (the generated-files table and an update trigger).

## 3. Milestone 1: the first export (built 2026-11-21)

- **The run options** (spec §6.2), general engine features with no game in them:
  - `VmOptions.RecordsWrites` turns on the string origins (`RecordsStringStores`) and a new **writer record**:
    `ScriptMemory.RecordWriters` names, at each of the five banks' setters, the running frame's script
    (`ScriptMemory.Writers.cs`, keyed by `GlobalCell(bank, address)`). A seed is no write; a load or a reset drops
    the record, which is scene state. A Lua write is named by the frame's script, as any write in it is.
  - `GameSession` keeps `Writers` and `StringStores` across runs, a string store only while its cell still holds its
    text. Its snapshots carry them (`Writers`, `Origins`, written only when not empty, so an old snapshot reads as
    before), and `CopyState()` copies the banks and both records.
  - `VmOptions.StopAt` (`RunStop`): before each instruction, a frame at the site stops the whole run through
    `RequestStop()`, with the halt `stop:SCRIPT@0xOFF`; the banks merge as at an exit.
  - **Shown text:** `RunScene`'s `textListener` hears each run as it is shown, with its script stack and ruby, and a
    new `ITextListener.LineEnded` hears `end-text-line`.
  - Tests: `RunRecordingTests` (13, synthetic scripts); `SceneReloadTests` keeps the record's hook.
- **`engine/Ome.Export`** (Microsoft.Data.Sqlite 10.0.15; `THIRD_PARTY_NOTICES.md`): `ExportDeclarations` (refuses
  loaders and script relations until their milestones), `RegistryView` (`globals.json` read directly; see §5),
  `ScriptNames` and `BuilderRule`, `GameDataExporter` (the reader) and `ExportWriter` (the database, pictures,
  manifest). `ProjectBoundaryTests.OnlyTheExportProjectNamesTheSqliteLibrary` holds that only it names SQLite and only
  `Ome.Cli` and the tests reference it.
- **The reader:** the boot with recording on, stopped at `read_at`'s site; each kind's records (`exists`, `skip`);
  every named field's cells checked once (spec §5.5): an integer is shown, so a value no builder wrote is refused; a
  string is checked when the kind publishes it as text, for its writer and its origin. A refused value is written as
  null, its raw left out of the modder cells, and the command exits 1. Then the descriptions, from `CopyState()` of
  the booted session with the seeds; the references; the pictures, each packed id decoded once.
- **`Ome.Cli data-export`** (`engine/Ome.Cli/DataExportCommand.cs`): the boot stop from `patch-points.json` for the
  fingerprint of the installed script, the revision id (spec §6.8: `SYS4INI.BIN`, each `*.AAI` beside it, and each
  script a loose file serves, by `Sys4AssetStore.Locate`), `build_info` (title, revision, fingerprint, catalog
  revision, engine build, layers, OME's `git describe` and commit), the game version in the manifest only, and the
  report.
- **Tests:** `GameDataExportTests` (16, a synthetic game: a boot calling builders and an append layer, a description
  script calling another script, and one variant per refusal: a scene's write and a scene's string copied by a
  builder, an excluded builder, a string with no origin, a description script that is no text script; a boot that
  misses its point, a reserved kind id, two exports the same bytes, the tables the site reads). `entities_build.py`
  refuses the export's table names as kind ids (`test_entities_build.py`).
- **Kamidori's clean install** (2026-11-21): the boot stops at `INIT2@0x2cf`; 1318 items, 445 units and 452 skills,
  nothing refused, every reference resolves (3,766), 1770 description runs, of which 6 show nothing (items 171 to 175,
  handler 15700, and skill 11, 進行不可), no piece of another script, 2205 pictures, in 22 s. Compared with the Python
  extractors (`build/games/kamidori/data/`, base and the three layers): the same ids and names for items, skills and
  units (`MERGED-EBINIT.json`), and all 1313 item descriptions equal ITMES's title and text. Two exports into two
  folders are the same bytes: database, pictures and all.
- **The unit pictures, looked at** (spec §5.4): `face` is the party list's face plate with the name (`CA…AA`, 47
  units); `sprite` the map's four-frame walking sheet (`CP…AA`); `battle_1` the battle art (`CB…AAA`); `battle_2` and
  `battle_3` the same art, except for 28 units whose `CB…AAB` is the clothing-damaged variant, which is explicit; the
  last column (`CB…AB`) is a close-up face, so its picture is renamed `battle_face`. Whether the site shows the `AAB`
  variants is the user's decision (§5).

## 4. Later milestones

As spec §13 orders them: more of Kamidori's tables (2), stage enemies (3, once the floor-entered global `G[0x428c]`
is named), the modder data in full (4), the reference bundle and the script index (5), script relations (6), and
Kamidori's English (7). Each gets its section here before it starts.

Found by milestone 1 for a later one: `manifest.json` keeps the command line as given, local paths included
(`--output`, `--game-root`). Milestone 5, which publishes a bundle, decides what of the manifest goes in it.

## 5. What changed since the spec

- **The kind ids a site path takes** (spec §5.2's comment): `docs` and `api` are reserved for game ids, the first path
  segment (site §4). A kind id is the second, where only `tooltips` is taken, so the lint refuses that one.
- **Script names** in the declarations drop the extension as well as the `$n$` prefix (`INIT2`, where a patch point
  writes `INIT2.BIN`), in upper case; the lint refuses any other spelling.
- **Three lint rules the spec implies:** a string field is published only under `text`, so `fields` refuses one; a
  published field may not be saved state, which is the player's progress (Kamidori's item table has five such
  fields); and a picture's field must be marked `picture` in `[table.refs]`, so the registry says what the site shows.
- **A description's name** may not be a field of its table.
- **Pictures beyond the spec's examples:** the item's furniture sprite (`furniture`, SF0301 to SF0540). The costume
  graphics (`item.costume_graphic_asset_ids`) are not published: they include standing and room art, which the
  user's choice for units left out.
- **References are written only for published fields** (an exporter rule for milestone 1; spec §5.3 does not say), so
  a reference never shows a value the kind does not publish.
- **What milestone 1 built differently** (2026-11-21):
  - **Shown text comes from a listener**, not the run's `Emitted` (spec §6.2): `RunScene`'s `textListener` gives each
    run with its script stack and ruby, and the line ends (`LineEnded`), which `Emitted` does not carry.
  - **The boot always stops at the point's site** and checks the halt. Kamidori's site, `0x2cf`, is INIT2's `exit`,
    so the boot stops just before it, which reads the same as running to its end (spec §6.2, step 2).
  - **The tables are read through `globals.json`** (`RegistryView`), not `GameModule`'s table symbols (spec §6.2,
    step 3): `GameModule` keeps OMS identifiers and drops a column name such as `face_CA_AA`, which the declarations
    use as the record map does.
  - **The `cell` table is sparse:** it holds the cells a script wrote or that are not default. The `field` table
    adds `ref_table`, `bank`, `address`, `width` and `column_index`, so the site can find any cell.
  - **A description is its pieces joined line by line** with `
`, the empty lines at its end dropped. Ruby is not
    kept yet (the base text only); the pieces of another script are dropped and counted.
  - **The export's own table names are reserved** as kind ids, beside `tooltips`, in the lint and the exporter.
  - **The fourth battle column's picture is `battle_face`** (§3): it is a close-up face, not battle art.
  - **Open for the user:** `battle_2` and `battle_3` show, for 28 units, the explicit clothing-damaged art (`AAB`).
    The declarations keep them as the user chose the battle sprites; whether the site shows those 28 is theirs to
    decide.
