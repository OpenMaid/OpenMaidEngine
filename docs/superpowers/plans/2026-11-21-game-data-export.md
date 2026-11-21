# Game data export — implementation plan

**Date:** 2026-11-21
**Spec:** `docs/superpowers/specs/2026-11-21-game-data-export-design.md` (cited as "spec §n"); the site's design, in
the `ome-site` repository at `docs/specs/2026-11-21-site-design.md`, is cited as "site §n". The spec stays the design's
home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what changed
since the spec was accepted (§5).
**Status:** written when milestone 0 was built. §3 is expanded before milestone 1 starts.
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

## 3. Milestone 1: the first export (to be expanded before it starts)

In order, each with its tests on synthetic scripts before any game run:

1. **The run options** (spec §6.2) in `Ome.Engine.Vm`: recording (string origins and the writer record, the session
   keeping both across runs and in its snapshots), a stop at a site through `RequestStop()`, and the shown text out of
   the run. Where the writer record hooks the global writes, and how a snapshot carries both records, is settled first.
2. **`engine/Ome.Export`** with Microsoft.Data.Sqlite (a pinned version; its notices in `THIRD_PARTY_NOTICES.md`),
   referenced by `Ome.Cli` and by `Ome.Engine.Tests`, where the exporter's tests live beside the synthetic helpers.
3. **The boot read** through `GameModule`'s table symbols, the run-time check (spec §5.5), the descriptions
   (spec §5.7), the pictures (spec §6.5), the modder cells and writers (spec §6.6), the format (spec §7) and its
   determinism.
4. **`Ome.Cli data-export`**, and a run on the clean install compared with the Python extractors' counts; the unit
   picture columns checked by eye (spec §5.4).

## 4. Later milestones

As spec §13 orders them: more of Kamidori's tables (2), stage enemies (3, once the floor-entered global `G[0x428c]`
is named), the modder data in full (4), the reference bundle and the script index (5), script relations (6), and
Kamidori's English (7). Each gets its section here before it starts.

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
