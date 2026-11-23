# Game data export — implementation plan

**Date:** 2026-11-21
**Spec:** `docs/superpowers/specs/2026-11-21-game-data-export-design.md` (cited as "spec §n"); the site's design, in
the `ome-site` repository at `docs/specs/2026-11-21-site-design.md`, is cited as "site §n". The spec stays the design's
home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what changed
since the spec was accepted (§5).
**Status:** milestones 0 to 7 built (2026-11-21 and 2026-11-22; milestone 2 in §4.1, milestone 3 in §4.2,
milestone 4 in §4.3, milestone 5 in §4.4, milestone 6 in §4.5, milestone 7 in §4.6).
§4 gets each later milestone's section before it starts.
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
  last column (`CB…AB`) is a close-up face, so its picture is renamed `battle_face`. The user kept the `AAB` variants,
  to be shown blurred until a click (§5, "Explicit pictures").
- **Explicit pictures** (2026-11-21, the user's request after the first run): `[export] explicit_pictures` in the
  declarations names them by catalog name (a `*` pattern allowed, an append's `$n$` prefix ignored); the export marks
  each `picture` row `explicit`, the mark joins the content hash, and an entry that matches no picture is reported.
  Every picture Kamidori publishes was looked at on contact sheets: 14 files are explicit (the eleven `CB…AAB` and
  `CB052AAA`, `CB504AAA`, `CB505AAA`), on 29 units; suggestive but covered art (`CB551AAA`, `CB311AAA`,
  `CB016AAA`) is not marked, and no icon, face, sprite or furniture picture is. The site blurs them (site §8.11).

## 4. Later milestones

As spec §13 orders them: more of Kamidori's tables (2), stage enemies (3, once the floor-entered global `G[0x428c]`
is named), the modder data in full (4), the reference bundle and the script index (5), script relations (6), and
Kamidori's English (7). Each gets its section here before it starts.

### 4.1 Milestone 2: more of Kamidori's tables

Started 2026-11-21; the user left the choice between this and milestone 5 to me ("Your call, whichever path you
want"). It comes first because the spec orders it, and milestone 5's script index belongs to milestone 4 first.

**What the code says** (ran: the boot image, `tools/init_image.py`; read: the registry and the disassembly):

- Every array of the eight tables is already a named global of `vm-map/globals-kamidori.toml`, and every one lies above
  the saved prefix (integers from `0x140d4c`, strings from `0x24b3`; the prefix ends at `0xfd40e` and `0x321`). So
  no field of them is saved state, and each becomes editable by data files.
- **Records**, as the boot leaves them:
  - 187 quests (20 of them `仮` placeholders, whose first appear flag is 10000);
  - 492 recipes (485 with their own item's id);
  - 28 conditions;
  - 209 stages;
  - 93 dig tables (each with its first slot set);
  - 43 character profiles;
  - 126 chapter objectives;
  - 163 glossary entries.

  These are the counts of `docs/kamidori-data-tables.md`.
- **The three message scripts are called as ITMES is:**
  - INFO8 calls `vocabulary_message_script_ids[current_vocabulary_id]` (`INFO8@0xd95`);
  - INFO7 calls `character_profile_biography_script_ids[character_profile_selected_id]` (`INFO7@0xe9a`);
  - INFO1 calls `objective_message_script_ids[current_objective_id]` (`INFO1@0xffa`).
- **Two registry shapes are wider than the data.**
  - `vocabulary_unlock_scene_ids` was typed `[2000x3]` from the gap to the next array, but the glossary's other
    arrays have 1,000 entries and its scripts write rows 1 to 193 only.
  - `condition_stat_deltas` names nine columns of a 45-cell row: the row holds nine stats for each of five levels.

**The plan:**

- **The registry's eight `[[table]]` entries**, `built_by = "boot"`, with `[table.refs]`:
  - `quest`: its reward items, prerequisite quest and requester icon (`picture`);
  - `recipe`: its output, ingredients, mutation and grown item, and prerequisite recipe;
  - `condition`: its immunity skill;
  - `stage_catalog`: its required party units, reward items and backdrop (`picture`);
  - `dig_table`: its items;
  - `character_profile`: its unit and portrait (`picture`);
  - `objective`;
  - `glossary_entry`.

  The stage table is not `stage`: the registry's scope has that name, and the lint refuses a table named like a
  scope. `vocabulary_unlock_scene_ids` becomes `[1000x3]`, and the condition rows get a column name for each level
  (`level_1_accuracy` … `level_5_movement`, as Himegari's condition table names them). `kamidori-base` goes to 1.2.0; its layout, record map and shape are regenerated.
- **A kind's title** may also be a reference field, whose target's title the entity takes (a recipe is named by its
  output item), or be left out (a dig table has no name; the site shows "Dig table 12"). `entities_build.py`, the
  exporter and the site learn both.
- **The declarations:** eight kinds, with descriptions for the glossary (VIMES), the profiles (CIMES) and the
  objectives (FAMES).
- **Pictures:** the quests' requester icons, after each is looked at. Condition icons are not pictures: their 1 to 16
  index a sheet. The profiles' portraits are standing art (`CS…AAA`), which the user's choice for units left out, so
  they wait for the user. The stage backdrops are marked `picture` in the registry and not published.
- **Saved copies** (the data patches plan's §11.3 method): which of the new fields the game copies into saved state,
  for `docs/modding/games/kamidori.md`.
- **Checks:** an export of the clean install with no refusal, the counts above, the Python extractors' values for
  each kind, the site built from it.

**Built** (2026-11-21), as planned, with these results and changes:

- **The export of the clean install** refuses nothing and finds no missing reference. Its counts are the ones above,
  less the 20 placeholder quests: 167 quests. Its descriptions are 2,102 runs, six of them empty, with nothing of
  another script dropped. It writes 2,307 pictures.
- **Checked against the Python extractors** (`build/games/kamidori/data/*INIT.json`, the base and the three layers),
  every kind matches. The ids match, and so do:
  - the names, chapter labels and condition level names;
  - the quests' reward items, the recipes' outputs and ingredients, and the dig tables' slots;
  - the stages' labyrinths and floors;
  - the profiles' biographies and the glossary's explanations, as the extractors decode CIMES and VIMES.
- **A data file edits each table.** `kamidori-base`'s applier is generic over the record map, so a scratch package
  setting one cell of each new table (ten values, two of them strings) passes `omp check`. Booted in the port with
  it, the game holds all ten at the title (`drive.py port`, a `lua` step reading the cells).
- **Saved copies** (a scan of the installed scripts, then a reading of every script that reads the tables, 30 of
  them): the game copies four fields into saved state. These are a condition's turns, a stage's labyrinth and
  floor, and a stage's name, which reaches a save's label. The quests, recipes, conditions, stages and dig tables
  also decide results that play keeps; a recipe's learned flag reaches the shared profile. The profiles, objectives
  and glossary are only shown. `docs/modding/games/kamidori.md`, "Data edits", has the sites.
- **The requester icons**, 101 files for 153 quests, were looked at on one sheet. They are item and place icons and
  small portraits, none explicit, so the list of explicit pictures stays as it was.
- **The profiles' portraits are published** on the user's word ("We can publish them", 2026-11-21): 37 files for
  43 profiles, each looked at. None is explicit. `CS011AAA` (土精アト) is revealing but covered, as unit art left
  unmarked is.
- **A reference value below 0 is none**, as 0 is. The stages' `required_party_unit_ids` writes -1 for none, which the
  first run counted as four missing units.
- **The registry's array types:** the eight tables' arrays were written `int[RxC]`, which `globals_build.py` reads as
  a flat array, so they are now `int[R][C]` like every other table. The condition resource deltas name their columns
  too (`level_1_hp` … `level_5_fs`). These renames change `kamidori-base`'s layout. Its version goes to 1.2.0, and
  nothing released named the old columns.
- **The glossary's description is named `explanation`**, not `text`, which gave its page a "Text" heading under
  "Text".
- **The site** builds every new entity (3,566 pages, 3,536 entities). It names a recipe by its item and shows the
  item's icon; it shows a quest's requester icon in lists.

Found by milestone 1 for a later one: `manifest.json` keeps the command line as given, local paths included
(`--output`, `--game-root`). Milestone 5, which publishes a bundle, decides what of the manifest goes in it.

### 4.2 Milestone 3: the stage enemies

Started 2026-11-21. The user left the choice between this and milestone 5 to me ("move onto milestone 3 or
milestone 5, your choice"). It comes first because the spec orders it and it completes the stage pages milestone 2
added; milestone 5's script index serves milestone 4 too.

**What the code says** (read: the disassembly, the registry, the data patches' stage tests):

- **`G[0x428c]` is 1 while the party is out on the field.** FIELD sets it on arriving when it is 0, and only then runs
  the arrival block (`FIELD@0x1ef` to `0x304`): the stage's visit flags, the world area's travel days, and the
  arrival scenes through SCJUMP, which can leave FIELD before its loaders. Every way off the field clears it: FIELD's
  exits to the town, the workshop and the map (`FIELD@0xcb05`, `0xcb36`, `0xcb54`), and TOWN, STUDIO and ALLMAP on
  entry (`TOWN@0x30`, `STUDIO@0x39f`, `ALLMAP@0x13a`). Scenes test it to send the party home (`SC0110@0x4ad7`), and
  INIT2 rebuilds the field at a load when it is set (`INIT2@0x274`). The registry names it `party_on_field`; the
  spec's `field_floor_entered` stood for it.
- **FIELD runs the stage loaders just before the point.** It calls each `stage_loader_script_ids` script (STINIT and
  its layers' `$n$STINIT`, `FIELD@0x50c` to `0x521`); `kamidori.stage.after_loader` is the next instruction
  (`FIELD@0x527`). STINIT zeroes all 30 slots of each `stage_enemy` array and writes the stage's, so every cell's
  writer is a builder the export already has (`*INIT`).
- **Keys:** the stage kind's 209 stages, less the 38 event-entry ones (`event_entry_flag` 1), which have no map.

**The plan:**

- **The registry** names `party_on_field` (`0x428c`). The data patches' stage test and the patch points' notes use
  the name.
- **A loader publishes its tables** under its key kind, which the spec does not say. The `[[loader]]` entry gains
  `[loader.tables.<short name>]` for each table of its scope it publishes, with `exists` and `fields` as a kind has.
  `entities_build.py` lints them against the scope's table (no strings: a loaded table's text waits for a need) and
  refuses a short name that is also a field of the key kind's table.
- **The exporter** runs each loader for each key entity: a copy of the booted session, the seeds, the script with a
  stop at the `read_at` site (the CLI resolves it as it does the boot's), the writer check on every cell of the
  tables' records that exist, with the export's builders and the loader's own. A run that ends before the point is
  reported with its key, and the export goes on (spec §6.3).
- **The export format** (spec §7.2 has no place for it):
  - `loaded(kind, name, table_name, scope, records)`: one row per published loaded table;
  - `loaded_field(kind, loaded, name, type, published, ref_table, ref_kind, bank, address, width, column_index,
    columns)`: as `field`, for each field of a loaded table;
  - `"<kind>.<name>"(id, record, …)`: the rows, by the key entity's id and the table's record, a column per
    published field as a kind's table has them (`stage.enemy`);
  - references in `ref`, from the key entity, with the field written `<name>.<field>` and `idx` the record times the
    field's width plus the element, so a unit's page lists the stages it appears on;
  - the rows in the key entity's content hash.

  The cells and writers of a loaded table, and its data file snippet, wait for milestone 4 (the modder data in
  full).
- **The declarations:** one loader, the stage scope's `enemy` table, every field published, `exists = "unit_id"`;
  seeds `current_stage_id`, `field_map_continue_mode` 0 and `party_on_field` 1; `read_at` the after-loader point.
- **The site** shows a stage's enemies as a table on its page, with each unit linked.
- **Checks:** the exporter's tests on a synthetic loader; the export of the clean install with no refusal and no
  halted run; the enemies against `build/games/kamidori/data/STINIT.json` and its layers'; the site built from it.

**Built** (2026-11-21), as planned, with these details:

- **The declarations:** `tables` in `LOADER_KEYS`, and each `[loader.tables.<short name>]` linted as planned; a
  loader's `skip` is now checked against the key kind's table too. `entities.json` writes each loaded table with its
  registry table beside the short name (`{"enemy": {"table": "stage_enemy", "exists": …, "fields": […]}}`), since the
  exporter's registry view reads no scopes. `loaded` and `loaded_field` join the export's reserved table names.
- **The exporter:** `ExportDeclarations` parses loaders (script relations are still refused); `ExportInputs.StopOf`
  resolves a loader's point, and the CLI gives it the boot's resolver. A loader's script must be the one its point
  lies in. The runs record writes and stop at the site, with `LoaderMaxSteps` (50,000,000) as their bound. A record
  exists when its `exists` cell is not default. A refused element is null in its row, as a kind's is.
- **The format:** as planned, and `loaded` has a `rows` column too. The format version stays 1: none has been
  released. An entity without loaded rows keeps the content hash it had before.
- **The site** (`ome-site`, `feature/kamidori-more-kinds`): a stage page has an Enemies table, one row per record,
  the reference columns first; a column at 0 in every row is named under it. A unit's page lists the stages under
  "Stages: enemy unit".

**Results:**

- **The export of the clean install:** 171 loader runs, none halted, no refusal, no missing reference; 2,459 enemy
  rows, linked to 341 distinct units. The loaders add 1.5 s (17.8 s in all).
- **Against the extractor:** every field of every row of the 171 stages equals `STINIT.json`'s enemies with the
  layers' records laid over them slot by slot. A layer's record for a stage the base has edits it, not replaces it:
  one enemy slot (seven stages, in APPEND02 and APPEND03) or the objects only (stage 91). Its other records are
  stages of their own.
- **The site** builds 3,566 pages; every enemy's unit links to its page.

### 4.3 Milestone 4: the modder data in full

Started 2026-11-21. The user left the choice between this and milestone 5 to me ("Your call, either is fine"). It
comes first because it builds the script index, which milestone 5 then only publishes, and it finishes the modder
panel the site shows half of; milestone 3's loaded tables have no cells yet (§4.2).

**What the code says:**

- **The snippet and `script_use` tables exist and stay empty**; nothing writes a data file yet. `DataValue` (a data
  file's document) is in `Ome.Engine`, and `Ome.Mods/Data` has the reader and the shape check, which `Ome.Export`
  does not reference.
- **The operand contracts are already loaded in C#:** `Ome.Script.Abi.OpcodeContracts` gives each operand's access
  (`read`, `write`, `address` for an array's base, and their string and float forms), and the opcode table each
  opcode's `waits`. `Ome.Cli` reads an install's scripts as the engine mounts them (the catalog, its appends, loose
  files); the Python tools' `paths.scripts()` is the legacy map of extracted files.
- **The auto map knows few lengths:** of its 7,671 entries for Kamidori, 7,348 are a bare `int` or `string`. An
  unnamed array's length cannot come from it.

**The plan:**

- **`DataFileWriter`** (`Ome.Mods/Data`, spec §6.6) writes a `DataTable` as TOML: a table that holds values gets a
  header (`[item.12]`), a table of values only under it is one inline table (`price = { 0 = 10, sell = 5 }`), and a
  table of tables only gets no header of its own. Keys and values are written as `DataValue` prints them. The tests
  read its output back with `DataDocumentReader` and compare.
- **A snippet for each entity:** `[<table>.<id>]` with every published field and text field at its current value, a
  row's columns by name where the registry names them and by number otherwise, and each loaded table's rows as
  `[<scope>.<id>.<name>.<record>]`. A refused element is left out. The exporter reads each snippet back and checks it
  against the base mod's shape (`mods/<game>-base/shapes/tables.schema.json`, given by the CLI); a problem is
  reported and the export exits 1, as a refusal does. `Ome.Export` references `Ome.Mods`.
- **The loaded tables' cells** go in `cell` as a kind's do, the field written `<name>.<field>` and `idx` the record
  times the width plus the element, as their references are (§4.2).
- **The script index** is built in C#, not Python (spec §8.2 names `tools/script_index.py`): `Ome.Cli script-index
  --game <game>` writes `docs/script-index-<game>.json`, and `--check` compares. For each script the catalog mounts:
  its fingerprint, the scripts it calls by a constant id and the count of computed calls, the scripts that call it,
  the named globals it reads, writes or indexes (`address`) with their counts, the patch points whose site is in it,
  and the waiting opcodes it uses. An operand is counted when its type is a global bank's (3, 4, 5) and its
  contract says how; a cell counts for the narrowest named global that holds it. A workspace gate runs `--check`.
  Renaming or naming a global changes the index, so the registry's update trigger gains it.
- **`script_use`** comes from the same index, built from the install the export reads: for each kind, each script
  that reads, writes or indexes a global of its table (a loaded table's count for its key kind). The table gains
  an `addresses` column, since most table access in the scripts is by index, which says neither read nor write.
- **Unnamed arrays** (spec §6.6) wait for a measurement: an unnamed integer array whose cell `base + id` the
  table's builders wrote for the kind's entities, found from the boot's writer record. If it finds what a reverse
  engineer would want and little noise, it goes in; else it goes to the backlog with the numbers.
- **The site** shows the snippet, the scripts and the loaded tables' cells in the modder panel.
- **Checks:** the writer's round trip; the exporter's tests (a snippet with columns by name and by number, a loaded
  table's rows and cells, a shape problem reported, `script_use`); the script index's tests on synthetic scripts;
  every Kamidori snippet passing the shape; the index's counts spot-checked against the disassembly.

**Built** (2026-11-21), as planned, with these details:

- **`DataFileWriter`:** a table that holds only tables gets no header of its own, and an empty one keeps its header
  alone, so the reader gives it back. A table of values only is written inline when its table also holds values
  (`price = {0 = 10, sell = 5}` under `[item.12]`) and as its own section otherwise (`[stage.51.enemy.3]`). The one
  thing a read-back table may lose is order: its lines come before its sections.
- **The snippets:** a kind's record has its published fields, its text fields and its picture fields, in the
  table's order and each once, whole or by the columns the declaration names. A loaded row has the published fields.
  The snippet is in `ExportEntity.Snippet` and the `snippet` table. A problem is in `ExportReport.SnippetProblems`,
  named `<kind>/<id>.toml` with its line, and makes the exit code 1, as a refusal does.
- **The loaded tables' cells:** every field's, with the integers checked as a kind's are; a string field, which a
  loaded table cannot publish, gives its address and writer and no value.
- **The script index:** `engine/Ome.Export/ScriptIndex.cs` (`ScriptIndex`, `GlobalNames`) and
  `Ome.Cli script-index` (`ScriptIndexCommand`), committed as `docs/script-index-kamidori.json` and
  `docs/script-index-himegari.json`, with the workspace gate `script-index-check` for each game that has one. Beside
  the plan it holds the preloads (`preload-script-slot`, op `0x06`, which loads a script into a frame slot that later
  ops run), the counts of the cells no global names, and each script's instruction count. A use is written
  `[reads, writes, indexes]` on one line. A global whose registry type gives no length (`int[]`, `int[unit][30]`)
  holds its cells up to the next named global of its bank; `int[1000x50]` and `int[1000][50]` are both 50,000.
- **`script_use`:** the CLI builds the index from the install it exports and gives it to the exporter
  (`ExportInputs.ScriptIndex`); a script counts for a kind when it uses a cell of any array of the kind's table or
  of its loaded tables, whatever name the cell has.
- **Unnamed arrays:** measured, and not published (§5). The export reports instead, for each kind, the integer cells
  its builders wrote that no global names (`ExportReport.UnnamedCells`, `ExportInputs.Names`).
- **The site** (`ome-site`, `feature/kamidori-more-kinds`): the modder panel shows a loaded table's cells by record,
  with the record's data file header, and the snippet with a line on how to use it. A kind's page lists the scripts
  that use its tables in modder mode, and has the modder switch; each entity's panel gives their count and links
  there.

**Results:**

- **The export of the clean install:** 3,536 snippets, every one read back and passed by `kamidori-base`'s shape;
  604 `script_use` rows over the eleven kinds (400 scripts for units, 84 for items, 20 for stages); 133,617 cells,
  71,311 of them the stages' enemies. No refusal, exit 0, in about 22 s.
- **The script index:** Kamidori has 732 scripts and 2,123,406 instructions. 145 of them make 3,945 computed calls,
  and 398 have no constant caller. The nine patch points are found in five scripts. Of the operands on a global
  bank, the named ones are 69,018 reads, 188,617 writes and 110,820 indexes; the unnamed ones are 327,579 reads,
  372,218 writes and 23,873 indexes. Himegari has 520 scripts and 1,522,942 instructions, with eight patch points in
  three scripts. Each index takes about 2 s.
- **Spot check:** `ADDEXP.BIN`'s counts equal its disassembly's operands: `current_character_id` 49 reads and 2
  writes of its 51, `current_battle_slot` 10 and 1 of 11, `current_unit_id` 3 and 1 of 4, `character_levels` 7
  indexes.
- **Unnamed arrays:** the auto map (`global-var-map.json`) has 686 array bases, 138 of them integer bases that no
  global names. Not one cell at such a base plus an entity's id (times its stride) was written by its kind's
  builders, once cells of named globals are left out. Every integer cell any kind's builders wrote lies in a named
  global, most in the table's own fields (items: 16,751 cells; units: 21,463). The four globals of unknown length
  are the units' runtime state, which no builder writes.
- **The site** builds 3,566 pages with 133,708 links.

### 4.4 Milestone 5: the reference bundle

Started 2026-11-21 at the user's word ("let's do milestone 5"). Milestone 4 built the script index (§4.3); this one
adds the notes it lacks, packs what the site's reference section reads, and publishes it with each release. Its plan
was written with the build, not before it.

**What the code says:**

- **Each input has a builder already:** `opcodes_build.emit_json` gives `opcodes.json` from `vm-map/opcodes.toml`;
  `globals_build` gives a game's `globals.json`, but `--build-json` merges the auto map when the workspace has one, and
  its meta names the registry of the process's `--game`; each base mod commits `layout/globals.json`,
  `layout/points.json`, `events/*.toml` and `shapes/tables.schema.json`; the profile manifests hold each game's titles,
  engine build, support levels and store site.
- **The release publishes three fixed names** (`publish_github_release.py`'s expected assets), from the artifacts of
  the `linux-release` and `windows-release` jobs, which the tag-gated `publish-release` job downloads, checks and
  promotes. OME has no version file: a version is a `v*` tag, and `git describe` names the commits between.
- **Arterial had no committed script index** (milestone 4 committed Kamidori's and Himegari's), so its bundle would
  have had no script pages.

**The plan:**

- **The notes,** `vm-map/scripts-<game>.toml` (spec §8.2): a `[[script]]` for each script with a `name` (without
  `.BIN`), a `summary`, the `source` the fact comes from (a doc of this repository or a patch point), and optional
  `[script.arguments]`, each a named global the script reads as its caller's input and what it means. They are drawn
  only from what `docs/kamidori-data-tables.md`, `docs/arterial-data-tables.md`, `docs/scjump-progression.md`,
  `docs/script-inventory.md` and the patch points establish; `script-inventory.md`'s lists by subsystem are inferred
  from the names, so they give no note.
- **`tools/reference_bundle.py`** builds `OME-reference-<version>.zip` from the repository: `bundle.json`,
  `games.json`, `opcodes.json`, `docs/modding/**` as git tracks them, and under `games/<game>/` the curated
  `globals.json`, the base mod's files and `scripts.json`, the committed index with each note merged in and each
  layer's script (`$n$NAME`) linked to the base script it writes over (`layer_of`). `--version` names it; by default
  `git describe --tags --always --dirty`. The zip's entries carry one fixed time, so one commit's bundle has the same
  bytes wherever it is built.
- **`globals_build.curated_json(registry)`** gives the bundle's `globals.json`: the registry linted and merged with no
  auto map, its meta naming that registry.
- **The core gate `reference-bundle-check`** (`reference_bundle.py --check`) refuses a note whose script the committed
  index lacks, and an argument that is not a named global the script reads; then it builds the bundle in memory, so a
  registry that does not lint or a missing base mod file fails it too.
- **The release:** a `reference-bundle` job on every run of the release workflow, with no `if:`, so a commit that
  breaks the bundle fails before it is tagged. On a tag it passes the tag as the version. It uploads the zip and its
  `sha256sum`. `publish-release` needs it and downloads it; the publisher expects four assets, named for the tag,
  checks the zip against its checksum and its `bundle.json` (the tag, the tagged commit, a clean checkout, every
  file's hash, no file unlisted), and writes its line into `RELEASE-SHA256SUMS`.
- **Arterial's index** is built and committed as the others are, which adds its workspace `script-index-check`.
- **Checks:** the notes' form and their check against an index; the merge; the zip's round trip, its same bytes on a
  second build, and each refusal of `verify_archive`; the repository's own bundle; the publisher's refusal of a bundle
  of another tag or commit, from a dirty checkout, or changed after its hashes; the workflow's new job.

**Built** (2026-11-21), as planned, with these details:

- **The notes:** 47 for Kamidori (INIT2 and the three AUTORUNs, the 28 boot tables, the tables run on demand, FIELD,
  SELSTAGE, INFO3 and ADDEXP), 39 for Arterial (INIT2 and the four AUTORUNs, the 29 boot tables, the four tables run
  on demand and SETMAPDATA) and 10 for Himegari (SYSTEM4, SCJUMP, SCINIT and the message scripts). Each summary was
  checked against its source, and ADDEXP's slot argument against its disassembly (ADDEXP@0x0 reads
  `battle_unit_unit_ids` by `current_battle_slot`). A note's name matches an index key without case and without
  `.BIN`.
- **`games.json`** gives each game its `id`, `title` and catalog `titles`, `engine_build`, `script_revisions`,
  `support`, the store `site` from its launcher entry, its base mod's `id` and `version`, whether it has a
  `script_index`, and its `references`: the repository paths of its generated global and patch point references
  (`globals`, `patch_points`), which the guides link and the site replaces with its own pages. Added 2026-11-21, when
  the site's reference pages (site milestone 3) were the bundle's first reader: without it the site kept OME's rule
  for those files' names (Himegari's have no suffix). `tools/paths.py` now names both files
  (`patch_point_reference_doc` beside `global_reference_doc`), and `patch_points_build.py` and
  `modding_games_build.py` take the name from it.
- **`scripts.json`** keeps the index's fields as they are and adds `layer_of` and `note` (`summary`, `source`,
  `arguments` when it has some); its `index_format` names the index's format.
- **The publisher** imports `reference_bundle` for `verify_archive` and `archive_name`, so the builder and the release
  check one format. The release's description names the bundle. `_require_exact_assets` checks the verified set
  rather than a constant, since the reference asset's name changes with each tag.
- **The release job** builds with `fetch-depth: 0`, for `git describe` on a branch; values reach its script through
  `env:`, as the workflow tests require.

**Results:**

- **The bundle** of this branch has 44 files, 7.5 MB unpacked and 0.99 MB zipped, and builds in about half a second.
  Two builds of one version give the same bytes.
- **The script pages it gives:** Kamidori 732 scripts, 47 with a note, 149 layers linked to their base; Himegari 520,
  10, 32; Arterial 693, 39, 178.
- **Arterial's index:** 693 scripts, its seven patch points found in five scripts (BTL, INIT2, READYEND, RESULT,
  SETMAPDATA), about 3 s.
- **The release path** runs only on a tag, so the workflow's new job and the fourth asset are first exercised by CI on
  the push after this one.

### 4.5 Milestone 6: script relations

Started 2026-11-21 at the user's word ("Go ahead with the next milestone"). Spec §6.7 gives the declaration and the
rule; this milestone exports ADDITEM's relations, names the globals UNITECH takes, and exports its relations too.

**What the code says:**

- **The declarations side exists:** `tools/entities_build.py` lints `[[script_relation]]` (unique relation ids,
  `call` a script, `kind` a kind, `key` and `count` integer globals the registry names, `exclude` patterns) and
  renders it into `entities.json`; `ExportDeclarations.Parse` refuses a non-empty list. The `relation` table
  (spec §7.2) is created and left empty, and each entity's content hash has an empty `relations` array.
- **The scan has its inputs:** the export builds the install's script index (§4.3), which holds every script as the
  engine reads it; `OpcodeContracts` gives each operand's access and code-target positions (one opcode, `0x140`, has
  a code address as a value); the opcode table gives each implemented opcode's `leaves`, `calls` among them
  (call-script, the preloaded slot's call, wait-for-input, the load's restore, `0xfa` and `0x143`). A local `call`
  (`0x8f`) is a code target of kind `call`, not a `calls` fact. A call-script id resolves through the script
  provider as the engine resolves it.
- **ADDITEM** (the registry): `current_item_id` and the signed `current_item_count_change`. A scan of the disassembly
  (2026-11-21) gives spec §6.7's numbers: 143 calls in 65 scripts, 122 with both from constants (39 give, 83 take),
  21 computed in 14 scripts; no call has another call between its writes and itself.
- **UNITECH** joins units to the party. It loops over a count at `0x2d2842` and reads three parallel lists of eight
  at `0x2d2822` (the unit, an EBINIT id), `0x2d282a` (a mode) and `0x2d2832` (a level), none of them named. Its 55
  calls, in 29 scripts, all set the count to 1 and the unit and mode of the first entry, and no caller writes the
  level list: 50 from constants, 3 in DEBUGAM from a local, and 2 in GAMESTART computed.

**The plan:**

- **The registry** names UNITECH's four globals (`joining_unit_ids`, `joining_unit_modes`, `joining_unit_levels`,
  `joining_unit_count`), with what each mode does as UNITECH shows it; `docs/kamidori-data-tables.md` says how a unit
  joins and how ADDITEM gives and takes, and `vm-map/scripts-kamidori.toml` gets notes on both scripts.
- **The declaration gains `where`** (a table of integer globals and values): a call resolves only when the last write
  of each in its block is that constant. UNITECH's relation needs it: its key is the first entry of the joining list,
  and only a call whose count is 1 joins that unit alone. The key may be an integer global or a one-dimensional array,
  which names its first cell; `count` and `where` name single integer cells. `negative` needs a `count`.
- **The scan** (`ScriptRelations`, in `Ome.Export`): for each script of the index whose base name no `exclude`
  pattern matches, it walks the instructions and keeps, for each global the relation reads, the last write in the
  current block. A block starts at each code target (an operand at a code-target position, or a code address, that
  is an immediate). A write is an integer-global operand whose contract has a `write` kind: `mov G N` gives N, `sub G
  0 N` gives -N, anything else is computed. An operand that takes the global's address, an opcode whose `leaves`
  hold `calls`, and a local `call` make each value unknown, since other code may write it. At a call-script whose
  target's base name is `call`: an unknown key, a `where` not met, a zero count, or a negative count with no
  `negative` leaves the call unresolved, with the reason; a resolved call whose key no entity of `kind` has is
  reported apart. Each other call is a row on its entity: the relation (`id` for a positive count or none,
  `negative` for a negative one), the script's indexed name, the call's offset and the count as the script sets it,
  signed.
- **The export:** the `relation` table gets the rows, with a primary key of the entity, the relation, the script and
  the offset; each entity's content hash lists them. The report gives, for each relation, the calls, the rows, the
  excluded calls, and the unresolved and keyless calls with their reasons. Declared relations with no script index
  fail the export.
- **Kamidori declares two relations:** ADDITEM's `given_in` and `taken_in` on items, and UNITECH's `joins_in` on
  units (`where = { joining_unit_count = 1 }`), both excluding `DEBUG*`.
- **The site** labels each relation, gives a taken count as a number taken, lists a script once with its counts,
  and links it to the script's reference page.
- **Checks:** a synthetic game with a scene that gives an item and one that takes one (spec §12), and calls the scan
  leaves unresolved (a computed key, a label between, a call between, a zero count), an excluded script, a key no
  entity has and a `where` not met; the lint's new rules; the Kamidori export's numbers against the scan above.

**Built** (2026-11-21), as planned, with these details:

- **The index keeps how it read its scripts** (`ScriptReading`: each operand's access, whether it holds a code
  offset, whether an opcode calls a subroutine), which composition binds to the install's ABI
  (`ScriptReading.Of(contracts, abiId)`); `ScriptIndex.Build` takes it in place of the access function, and the scan
  reads it from the index. `RegistryView.IntegerCell` gives a key's cell, a list's first.
- **The bundle's note check** takes an argument a script reads through an index as read: UNITECH reads its three
  lists only through `lookup-array`, which the index counts as indexes.
- **The registry's `character_join_flags`** said bit 1 is set for modes 0 and 2; mode 2 does not write the flags
  (UNITECH@0x5fe), so it now says mode 0 sets bit 1 and mode 2 leaves them.
- **The report** gives each relation's calls, scripts, rows and excluded calls in one line, then the unresolved calls
  and those whose key no entity has, each `script@offset` with its reason.

**Results:**

- **Kamidori:** ADDITEM's 143 calls in 65 scripts give 120 rows (38 given, 82 taken: spec §6.7's 39 and 83, less
  DEBUGADV's one of each), on 101 items; 6 calls in debug scripts are excluded and 17, in 11 scripts, are unresolved:
  each sets the item from a local. UNITECH's 55 calls in 29 scripts give 48 rows on 40 units; 5 are excluded and
  GAMESTART's 2, whose unit is computed, are unresolved. No key names a missing entity. The export still takes about
  22 s.
- **Tests:** five on the synthetic game (the rows of a giving and a taking scene, each unresolved case with its
  reason, a `where` and a list key, the database's rows and the content hash, and the export's failure without an
  index), and the lint's new errors.
- **The site** (its plan, §7) lists each entity's relations with links to the scripts' reference pages, and the
  calls with their offsets in the modder panel.

### 4.6 Milestone 7: languages

Started 2026-11-21 at the user's word ("Go ahead with the next one"). Spec §6.4 gives the rule: each published table
string is looked up at its origin and each piece of a description at its `show-text` site, as the engine translates
them when drawn; a miss keeps the source text and is counted; the export records each package's id, version and
authors.

**What the code says:**

- **The port translates as it draws** (localization spec §5.2, §6.2): the text native's `Presenter`, which the VM asks
  at each run. A message of several runs (`ScriptMessageIndex`) resolves as one sentence at its first run's site; its
  later runs draw nothing (the listener hears them as hidden runs) and the sentence draws its own line breaks, while
  the listener still hears each `end-text-line` inside it. A single run resolves with its operand's origin. A miss
  draws the source text and goes to the miss log, once per message. `GameSession.RunScene` takes no presenter, so a
  headless run draws source text.
- **A package's language** is its manifest's `[[language]]` with its `text/` folder, laid out as a text root
  (`text/<tag>/**.toml`). `TextRootLoader.Load(sources, tag, binder, report)` loads it, and the binder drops a site
  entry whose script's fingerprint is not the install's (`LanguageLaunch.BaseFingerprints`). The port mounts a
  package through the link cache; with no other mod a language package changes no script, so its `text/` loaded as a
  source, as `--text-root` loads a folder, gives the same catalogs.
- **Kamidori's converted package** is `build/games/kamidori/import-en/` (local test data: never committed or shipped,
  `docs/localization-import-test.md`): 217 catalog files, 48,586 entries. Its manifest gives an id, a name and a
  version, and no authors; neither it nor the translated install names the translators.
- **A table string's drawing site is not known:** the game draws it from a UI script the export does not run. Its
  origin is (`GameSession.StringStores`), and the importer scoped each stored literal by its origin (the import test,
  R2), so a lookup at the origin finds what the port finds there.

**The plan:**

- **`RunScene` takes a presenter.** The run's text native presents with it, and the run records string stores,
  seeded from the session's, so a drawn string resolves at its origin as it does in the port.
- **`--language PACKAGE`,** repeatable. The CLI reads each package's manifest (id, name, version, authors and its
  `[[language]]` tags), refuses one for another game, and loads each declared language from every package given, in
  that order, a later one winning at equal scope as in the port's stack; the install's fingerprints bind the site
  entries, and the load report's lines are printed. The exporter gets each language with the packages that declare
  it, and the message index.
- **One presenter per language,** with its own miss log:
  - **a published table string** is looked up at its origin, which stands for the drawing site too. An entry gives
    the string's row in the language. A miss gives none, and is logged; the site shows the source text in its place,
    as the engine draws it.
  - **a description** runs again from the boot with the presenter. Its text keeps each run's presented text, and the
    source text of a run with no entry; a presented message's hidden runs and the line ends inside it give nothing.
    It has a row in the language when any of its runs was presented, and it is counted as translated, or as partly
    translated when a run missed.
- **The writer:** the `text` rows in each language; `build_info`'s `languages` (the source language, then each
  exported one, comma-separated as `layers` is) and `language_packages` (a JSON array of each language's packages:
  language, id, name, version and authors); the content hash's text entries carry their language.
- **The report:** per language, its packages, the strings and descriptions translated, partly translated and missed,
  and the misses, `script@offset` with the source text.
- **The site** shows the game text in each exported language with a switch, and credits the packages' authors (site
  §8.6; the site plan's next part).
- **Checks:** a synthetic game with two languages: a site entry at a table string's origin, a global entry, a
  description presented as one sentence with its own breaks, the same description resolved run by run where the
  sentence misses (partly translated), a miss that gives no row, and the database's rows, `build_info` and hashes;
  the Kamidori export with the converted package.

**Built** (2026-11-22), as planned, with these details:

- **A description's misses are its own.** Its run from the boot also draws what the scripts it calls draw, which
  the export drops (milestone 1, §5), so each language's run gets its own miss log and the report keeps only the
  misses in the description's script. A table string that is only a number is not a miss, as in the port's log.
- **`TextMissLog.Misses()`** lists the log's misses, each source with its script, offset and count, for the report.
- **The CLI's refusals** (exit 2): a package with no manifest, no `[[language]]`, another game's profile, or the
  source language as its tag, and a catalog the loader refuses, with its problems listed.
- **The report** gives, per language, its packages with their versions and authors ("no authors given" when the
  manifest names none), the strings and descriptions translated and partly translated, the misses and the halts;
  then the misses, `script@offset` with the source text, and any halt of a description's run.

**Results:**

- **Kamidori with the converted package** (`build/games/kamidori/import-en/`, 48,586 entries from 217 files): 7,563
  of 8,276 table strings translated, and 2,076 of 2,096 descriptions, 15 more partly; 733 misses (713 table strings,
  20 description runs), no halts. Every table string missed is absent from the catalog: the converted install keeps
  it in Japanese (its ITINIT.BIN holds 装備衣装 22 times). A partly translated description is a message with no
  entry, resolved run by run, so only its item's name translates (`【基本攻撃：Anti-Air Bow】`). 54 more English
  descriptions hold Japanese because the converted install's own text does, often garbled (`BAS: 】手`), which the
  importer carries as it is. The export reads in about 37 s, from 22 s.
- **The package's manifest named no authors,** and neither the converted install nor the repository names the
  translators. The user named the translator, Aroduc (2026-11-22), who is now in the local package's `authors`, so
  the export's `language_packages` and the site's credit name them.
- **Tests:** two on the synthetic game with English and French catalogs (each lookup rule with its rows and report,
  a message presented as one sentence, the same message resolved run by run, a miss with no row; the database's text
  in each language, `build_info` and the content hashes), beside the earlier ones; 57 pass with the session and script
  index tests. Core validation passes.
- **The site** (its plan, §8) shows each language with a switch and credits the package.

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
- **A kind's title may be a reference or be left out** (milestone 2, §4.1). Spec §5.2 has a string field only. A
  reference title is a published field, not text; the entity takes its target's title, and the lint refuses one
  whose target's table no kind shows. With no title, the site names an entity by its kind and id. The export's
  `kind.title_field` holds the field, or null.
- **A negative reference value is none** (milestone 2), beside spec §5.3's 0: no record has a negative id, and
  Kamidori writes -1 for none.
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
  - **A description is its pieces joined line by line** with `\n`, the empty lines at its end dropped. Ruby is not
    kept yet (the base text only); the pieces of another script are dropped and counted.
  - **The export's own table names are reserved** as kind ids, beside `tooltips`, in the lint and the exporter.
  - **The fourth battle column's picture is `battle_face`** (§3): it is a close-up face, not battle art.
  - **Explicit pictures** (§3), which the spec does not have: `[export] explicit_pictures` in the declarations, an
    `explicit` column in the `picture` table (spec §7.2), and the mark in the content hash (spec §7.3). The user's
    words: "I think it's fine to keep them (Again, adult game witha site warning anyway) but something like that may
    be nice to separate out truly explicit stuff".
  - **The `field` table has a `columns` column** (spec §7.2 does not): for an array field, a JSON array of its
    elements' names from the registry's `columns`, with null for an unnamed element (`unit.stats` is `["accuracy", …,
    "capacity"]`). The site's milestone 2 needs it to label a record's arrays and to write a modder's name for one
    element (`unit[1].stats.max_hp`). Values with a meaning per number (a race, a category) had no names in the
    registry then; the value facets below give them names.
- **A loader publishes its tables** (milestone 3, §4.2). Spec §6.3 runs loaders but does not say where their output
  goes. The `[[loader]]` entry names the tables of its scope it publishes, with `exists` and `fields` as a kind has
  them, and the export gives them three things spec §7.2 lacks: the `loaded` and `loaded_field` tables, a
  `"<kind>.<name>"` table of rows (`stage.enemy`), and references from the key entity with the field written
  `<name>.<field>`. A loaded table has no text yet; its cells and its rows in the data file snippet came with
  milestone 4 (§4.3).
- **The spec's `field_floor_entered` is `party_on_field`** (`0x428c`, milestone 3): the registry's name for it.
- **The script index is built in C#** (milestone 4, §4.3), by `Ome.Cli script-index`, not `tools/script_index.py`
  (spec §8.2). The engine already reads an install's scripts as it mounts them, appends and loose files included,
  and loads the operand contracts and each opcode's `waits`. The exporter builds the same index for `script_use`.
  Its file holds the preloads and the unnamed cells' counts beside what spec §8.2 lists, and it gives how many reads
  and writes where the spec names the globals read and written.
- **`script_use` has an `addresses` column** (spec §7.2 lists `reads` and `writes`): most of a table's use in the
  scripts is through an index, an operand that is the array's base, which is neither. It counts a kind's loaded
  tables for the kind, and it is per kind, not per record (spec §6.6 says "the scripts that read or write the
  record"). An operand names a record's cell only when it writes that record directly; the site lists the scripts on
  the kind's page.
- **The snippet sets text and picture fields too** (spec §6.6 says the published fields), since those are what the
  page shows, and a loaded table's rows under their scope. A snippet the base mod's shape refuses makes the exit code
  1.
- **The arrays no field names are not published** (spec §6.6 and §13's milestone 4). Measured on Kamidori (§4.3), they
  would show nothing: every integer cell the builders write lies in a named global. The export reports such cells
  for each kind instead, so a game that has them shows them, and the site's list can be built when one does.
- **The bundle's globals never merge the auto map** (milestone 5, §4.4). Spec §8.1 names `globals_build.py
  --build-json`, which merges it when the workspace has one; the bundle uses `curated_json`, so a local build and
  CI's give the same file, as spec §8.1's "the globals the auto map infers … are not in it" intends.
- **A note says where its fact comes from** (`source`), and the core gate checks its arguments too: each must be a
  named global the script reads. Spec §8.2 checks only that a note names an indexed script.
- **`scripts.json` links a layer's script to its base** (`layer_of`), so a note on `ITINIT` serves `$1$ITINIT` to
  `$3$ITINIT` on the site; spec §8.2 does not have it.
- **`games.json` holds more than the support levels** (spec §8.1): the titles, the engine build and script
  revisions, the store site and the base mod's version, which site §6 shows, and the paths of the game's generated
  references, which the site maps the guides' links to them by.
- **The reference asset's name carries the tag** (`OME-reference-v0.10.0.zip`), as spec §8.1's
  `OME-reference-<version>.zip` says; the publisher's expected assets are therefore computed from the tag. The
  publisher checks the bundle's `bundle.json` against the tag and the tagged commit, which spec §8.3 does not ask.
- **Arterial has a committed script index too** (milestone 5): spec §8.2 builds one for each game, and milestone 4
  had committed two.
- **A script relation may have a `where`** (milestone 6, §4.5): globals whose last write in the block must be the
  given constant, or the call is unresolved. Spec §6.7 has `key` and `count` only; UNITECH's relation needs it, since
  its key is the first entry of a list and only a count of 1 joins that unit alone. A key may therefore be a
  one-dimensional list, which names its first cell.
- **What else leaves a value unknown** (spec §6.7 says only "the last write"): an operand that takes the global's
  address, an opcode that calls other code and a subroutine call, any of which may write it. A block starts at every
  code offset an operand names, a code address kept as a value too, not only at the disassembly's labels; on
  Kamidori this changes no call.
- **A zero count, and a negative one with no `negative`, leave a call unresolved**, and the lint refuses a `negative`
  with no `count`. The count is kept signed, as the script sets it, since the relation's name carries the sign.
- **A call whose key no entity has is reported**, not written, and the report counts the excluded calls. The
  `relation` table has a primary key (the entity, the relation, the script and the offset), which spec §7.2 does not
  give.
- **A miss gives no row** (milestone 7, §4.6). Spec §6.4 says a miss keeps the source text: the row would repeat the
  source, so the export leaves it out and the site shows the source text in its place, as the engine draws it. The
  report counts the miss.
- **A description is presented as the engine draws it** (spec §6.4 looks up each piece at its `show-text` site): a
  message of several runs resolves as one sentence at its first run's site, and only a message with no entry resolves
  run by run. It has a row in the language when any run was presented, and the report counts it as partly translated
  when a run missed.
- **The report counts the misses of exported text only:** a description's run draws the scripts it calls too, and
  their misses are not the description's.
- **`language_packages` gives each package's name** beside the id, version and authors spec §6.4 lists, so the site's
  credit can name it; it is JSON in `build_info`, and `languages` is the source language first, comma-separated.
- **The content hash's text entries carry their language** (spec §7.3 says the entity's text), so a page changes
  when its translation does.
- **A kind may group its fields** (the site's design pass, 2026-11-22; spec §5.2 has none). `[[kind.group]]` names two
  or more published rows of cells, which the site shows as one table: a recipe's ingredient items beside their
  quantities, a unit's stats beside their growth and caps. The narrowest field's width is the number of rows; each
  field's width is a multiple of it, and a field some times as wide gives that many columns, its elements row by row
  (a condition's turns, nine stat changes and three resource changes at each of its five levels). The export gives
  them in a `field_group` table (kind, name, position, field), which spec §7.2 lacks; the content hash leaves them out,
  since they arrange the page and change no value, and the declarations' hash in `build_info` covers them. Kamidori
  declares six: a recipe's ingredients, a quest's reward items, a unit's stats and its drops, a condition's levels and
  a stage's objective rewards.
- **Kamidori publishes no story flags or scene numbers** (the user, 2026-11-22, on the built site: "the numbers shown
  dont really mean anything, so those would be better as modder mode things"). The flags that gate a quest, a recipe,
  a stage, a dig table, a character profile, an objective and a stage's enemies, and a glossary entry's unlock scenes,
  are left out of `fields`, so the site's modder panel alone lists them, with the cells a script wrote.
- **The design pass's second batch** (the user's notes on the built site, 2026-11-22), none of which spec §5 or §7.2
  has. The lint checks each, `tools/test_entities_build.py` and the exporter's tests cover it, and the content hash
  leaves out what only arranges or explains the page:
  - **Notes.** `[kind.notes]` gives a line (at most 300 characters) that explains a detail, keyed by what the infobox
    shows as one row: a field outside every group, a text field, or a group. A loaded table's `notes` key its fields.
    The `note` table holds them (kind, name, text), a loaded table's field written `table.field`. The user: "Some entity
    details may want their own simpler explanation tooltip where we can explain a bit", and a note says only what the
    registry grounds.
  - **A group's labels.** `labels = "table.field"` names a text field of a table the kind's loaders publish, whose
    record r names row r: a stage's objective rewards are labelled by each objective's condition ("Stage objective
    rewards should have the objective text"). The `group_label` table holds them; the site then shows that loaded
    table only in the group.
  - **A picture cut from a sheet.** A `[kind.pictures]` value may be a table: `field` numbers a cell of the catalog
    picture `sheet`, `first` being the first, `columns` to a row from `origin`, `step` apart, each `size`. A number
    below `first` is no picture; a cell off the sheet is reported as unresolved. Kamidori's condition icons are SO009's
    15-pixel cells, as DRAWCHSTATE draws them.
  - **Text blocks.** `[kind.text_blocks.<field>]` divides a text field's row of lines into the blocks the game draws
    them in, `name = [first, last]`, in order and sharing no line. The `text_block` table holds them. A quest's lines are
    an overview (0 to 4), a clear condition (5, 6) and a failure condition (7, 8), as INFO2 draws them, so the site
    shows its text as a table ("Quest text should be a table"); the registry's column names, `source_header` for line
    0, would have split the overview, and the English package fills lines 0 to 4 as one paragraph.
  - **Loaded text and literals.** A loaded table's `text` lists string fields, written to the `text` table as
    `table.field`, its index the row times the width plus the element, and translated as a kind's text is; the
    `loaded_field` table gains a `text` column. A `literals` entry gives one record's text field the string a script
    stores at a dword offset instead: STINIT leaves a stage's first objective text empty, and AIM draws
    「占有率１００％」 in its place.
  - **A loader with no scope.** It names each table's registry table (`table`), runs every key or those in
    `key_range = [first, last]`, and may run each key `steps` times, its rows numbered step times the table's records
    plus the record. Its `seed` may be a list of cells, `{ global, index, value }`, an index's terms being numbers,
    `"key"`, `"step"` or `"*"` (every element). `loaded.scope` is null for such a table. Kamidori's title changes are
    one: CALCCC for each unit and each of its twenty title columns, with that column's rule state 2, the others 1 and
    the unit's level 99, read at the new patch point `kamidori.class_change.rules_run` (CALCCC@0x60, after the rule
    scripts). CALCCC zeroes the block before the rules add to it, so it is the loader's builder. 127 rows, as many as
    the rule scripts have rules.
  - **A one-record table.** A registry `[[table]]` with `records = 1` may name scalars (its one cell) and 1-D arrays
    (its one row), so the class change block is a table without its globals being retyped, which would change the
    base mod's released layout. `globals_build`, `base_mod_build`, `entities_build` and the exporter's `RegistryView`
    read it alike. Its `built_by` is `class_change`, which `ome-records` does not edit, so no data file reaches it.
  - **World areas and the dig tables.** A `world_area` kind (AMINIT2's areas) gives a stage's labyrinth and a unit's
    habitat areas a name and a link ("maybe we add a labyrinth entity or at least get the location name instead of an
    id"). The `dig_table` kind is dropped, since its page could not say where a dig site is; the backlog's stage maps
    item keeps it.
- **The player view** (the usability review's findings 5, 6 and 9, the user's decisions of 2026-11-22: "Sure, give
  allies their own table", "I'm fine with your recommendations"). None of it is in spec §5 or §7.2. The lint checks
  each part, `tools/test_entities_build.py`, `tools/test_globals.py` and the exporter's tests cover it, and the content
  hash leaves out labels, formats and value names, which are the kind's and the registry's, as it leaves out the name
  of an entity a reference links to:
  - **Engine fields are not published.** Sort keys, catalog switches, variant and group ids, a skill's area shape and
    target mode, an enemy's faction, routines, spawn place, weight and level divisor, a unit's boss class, and two
    recipe fields no record writes (the required synthesis rank and the fourth room's level) left `fields`, so only the
    modder panel lists them.
  - **Labels.** `[kind.labels]` and a loaded table's `labels` give a field or a group the name a reader knows it by,
    at most 40 characters, where its own name reads as the game stores it (`min_range_encoded` is "Min range",
    `boss_flag` "Boss stage"). The `label` table holds them (kind, name, text), a loaded table's field written
    `table.field`.
  - **Value facets** in the registry, on an integer `[[global]]` (`tools/globals_build.py` lints them, the global
    reference shows them in a line): `values`, OME's English words for some values (`sex` 1 "Male", `boss_flag` 1
    "Yes", `duration` -1 "Until cleared"); `values_from = { global, first }`, a string list of the game's that names
    them, element k naming value first + k (a unit's race from `species_names`, an element resistance from
    `affinity_level_names` with first -5); `bits_of`, a mask whose bit k stands for value k of that global (an enemy's
    difficulty, a skill's weapon categories); and `scale`, `offset` and `unit`, a number shown as stored × scale +
    offset in its unit (growth in hundredths, a skill's range less one, `%`, days, turns, tiles). A mask takes no
    words, scale or offset. Kamidori has 42 globals with facets, from game readers each (`usage` cites them), and five
    new ones: the element, craftsman rank, synthesis rank and item category name lists INIT2 writes, and
    `labyrinth_difficulty` (`0x320b`, 1 normal, 2 hard, as GAMESTART sets it).
  - **Their tables.** `value_format` (kind, field, scale, offset, unit, bits) for each published field whose global
    has a facet; `value_name` (kind, field, value, lang, text), with lang `''` for OME's word, `ja` for the game's name
    and each language's translation of it, which comes from the game's strings as a published text does
    (`language.String` at its origin). A game's name is refused as published text is, by the field's writer, and a
    refusal is reported, field `<name>:<global>`. A mask's names are its target's. A loaded table's field is written
    `table.field`.
  - **A loaded table's parts.** `[[loader.tables.<table>.parts]]` names a part (`name`, `note`) and its rule: a row is
    in the part when the list `global` at the row's integer `field` is not 0 and equals the list's element `same_as`;
    the first part whose rule holds takes it. Kamidori's stage enemies have one, `ally`: a row whose faction
    `stage_faction_alliance` puts with faction 1, the player's, which is the registry's hostility rule and FIELD@0xc45's
    test. 42 rows on 18 stages are allies; STINIT gives faction 3 the player's group on 14 stages. The `loaded_part` table holds a table's
    parts, and the table's rows gain a `part` column; a data file and the modder panel still treat the table as one.
    A refusal of a cell of the rule's global is reported, field `<table>.<part>:<global>`, and decides nothing.
- **Names and links that read apart** (the usability review's findings 7 and 8, which the user approved on 2026-11-22).
  Neither is in spec §5 or §7.2. Both are declarations, which the lint checks and
  `tools/test_entities_build.py` and the exporter's tests cover, and neither enters the content hash:
  - **Qualifiers.** A kind's `qualifiers` lists at most three of its shown fields or texts, one cell a record, that
    tell apart its entities with one name, in order. The `qualifier` table holds them (kind, position, field). The site
    adds those whose text differs among the entities that share a name in a language, until each reads apart, and the
    id where none does; a name is told apart only in the languages it is shared in. Kamidori qualifies seven kinds: a
    unit by class title, starting level and level cap, a stage by labyrinth and floor, an objective by chapter, an item
    by category, a skill by its short description, a quest by its requester and a character profile by its epithet.
    198 units, 50 stages, 48 items and 43 objectives read apart that way; 50 units and 25 objectives differ in nothing
    published and take their id (22 pairs of townsfolk units among them).
  - **How a reference reads on its target's page.** A kind's `referenced_as` keys a published reference field, or a
    loaded table's (`table.field`), to the heading of the list of entities whose field names the target ("Dropped by",
    "Enemy on stages"), or to `{ heading, details, parts }`: `details`, at most four, are shown beside each referring
    entity (a field of the kind or a text, one cell a record; the element of the reference's group beside it, as wide
    as the reference, a drop's chance; or a field or text of the reference's loaded table, one cell a row, an enemy's
    levels), and `parts` heads the rows of each of the loaded table's parts ("Ally on stages"). The `referenced_as`
    table holds them (kind, field, part, heading, details as a JSON array), part `''` for the table's own rows. Kamidori
    heads each of its 24 references, from the registry's usages: a garden's product (CHECKREWARD), a craft's mutation
    (ALCHEMY), a recipe's grown item and prerequisite (CHECKRECIPE), a skill's bundle (CALCREVISE), a condition's
    immunity (ADDILLSUB), a stage's prerequisite labyrinth (CHECKSTAGE). Three quantity and floor fields gained labels
    for their columns.
- **Columns the game names, and what a kind is** (the usability review's findings 14 and 18, which the user approved
  on 2026-11-22). Neither is in spec §5 or §7.2:
  - **Column facets** in the registry, on an integer array (`tools/globals_build.py` lints them, the global reference
    shows them): `columns_from = { global, first }`, a string list of the game's whose element k names column
    first + k, and `columns_ref`, a registry table whose record k column k is; one of them at most. They name columns
    where `columns` gives OME's words, which stay, since they are the base mods' layout names. Kamidori's element rows
    take `element_names` (a unit's resistances, an item's resistance changes, a title change's, CALCARM's totals), and
    its condition rows the `condition` table (an item's condition changes and equipped levels, a skill's inflictions,
    a unit's condition state), as ILINIT's ids index them. An item's resistance change also takes
    `affinity_level_names` with first -5, the grade INFO3@0x017a4 draws for it, as a unit's resistance does.
  - **Their tables.** `column_name` (kind, field, column_index, lang, text), the game's names as `value_name` holds a
    value's, read, refused and translated alike (a refusal's field is `<name>:<global>`); and a `column_kind` column in
    `field` and `loaded_field`, the kind whose table `columns_ref` names, null when no kind shows it. A published column
    of a field (`price.sell`) is one cell, so it has neither.
  - **About.** A kind's `about` is one line, at most 160 characters, that says what its entities are; the site shows it
    on the kind's card and its list. The `kind` table has an `about` column, null for none. Kamidori says each of its
    eleven kinds from its fields and its table's usage.
- **The second design review** (2026-11-22; the user: "start by taking on the reviewer feedback. I'll leave it to your
  discretion which items you think have merit"; site plan §13 keeps every finding and what was decided). Three of its
  findings took OME's declarations or registry, none of them in spec §5 or §7.2:
  - **A heading for a row of cells by another kind's records.** `referenced_as` may key a published row of cells whose
    global's `columns_ref` makes column k record k of a table (C2): the site lists the referring entity on the page of
    each record whose cell is not 0, the cell beside it. Kamidori heads three: a skill's condition levels ("Inflicted
    by skills"), an item's condition level deltas ("Changed by items") and its equipped condition levels ("Given while
    equipped"), so a condition's page lists what gives it.
  - **The value that means none** (C3). The registry's value facet `none` is the stored value that means none; the
    export's `value_format` publishes it, and a list shows such a cell empty, as a page leaves it out. Kamidori's quest
    deadline (INFO2 draws なし for 0) and quest reward money take `none = 0`; the item prices do not, since nothing
    shows that a buy price of 0 means the shops do not sell the item.
  - **Labels for what read as storage names** (P3): a unit's short name, a quest's category, a profile's race and sex,
    an item's garden points per product (now in `points`, the unit CHECKREWARD divides by), and "Stages unlocked by a
    floor here".

- **A second game: Arterial** (2026-11-22; the user: "I'd like to make sure what we've done is generalizable to the
  other games without a bunch of hardcoding"). `vm-map/entities-arterial.toml` declares 18 kinds from the registry's
  tables: cards, talents, skills, items, conditions, 奥義, characters, character profiles, glossary entries, tips,
  recipes, shop entries, decks, battles, battle rules, arena opponents, goals and areas. A kind that is Kamidori's too
  takes Kamidori's id (`character_profile`, `glossary_entry`). The site needed its settings (`site.config.mjs`) and
  three fixes of wording that named Kamidori's kinds or its English (site plan §12). The first export ran unchanged
  and exited 1 on 265 refusals. Four things the declarations and the registry could not say were added, none of them
  in spec §5 or §7.2. The lint checks each, and `tools/test_entities_build.py`, `tools/test_globals.py` and the
  exporter's tests cover it:
  - **Derived scripts.** `[export] derived` names scripts that work out play state from the tables during the boot.
    INIT2 runs COUNTSKILL, which counts the party's skills (none at the boot) into the skill table's `party_count`, the
    265 refused cells. A derived script's writes are withheld without a refusal: no value, and the writer in the modder
    panel. The unnamed-cells count leaves out what it wrote. The lint refuses a derived script that is a builder or a
    text script.
  - **Two-field references.** A `[table.refs]` value may be `{ table, times, plus }`: the record is the field's value
    times `times`, plus the value of field `plus` at the same element. An Arterial card is a row and a slot (row × 30 +
    slot), and every table that names a card names it that way: a deck's cards and drops, a recipe's product and
    ingredients, a reward. A value of 0 or below stays none. The JSON's `tables` give `ref_keys`, and the export writes
    the record's id as it writes a one-field reference's, so the site needed no change. The lint refuses a two-field
    reference to `picture`, a `times` below 1, and a `plus` that is not another integer field of the same width.
  - **A mask named by a list of strings.** `bits_of` may name a string list, its element k naming bit k. A card's races
    take INIT2's `race_names` (`0x12a7c`), since no integer global's values carry them.
  - **A title of several references.** A kind's `title` may be a list of two or more reference fields: the entity is
    named by the first that names a record. An Arterial recipe or shop entry makes an item or a card (432 of the 617
    recipes make a card), so both are titled `["item", "card_row"]`. The export's `kind` table keeps the first in
    `title_field` and the rest, a JSON array, in `title_fallbacks`. The lint refuses a list of one, a name given twice,
    and an entry that is no reference to a table. `test_entities_build.py`'s title tests, which its `main` had never
    run, now run.

  The registry gained six globals: the card rarity and item kind name lists INIT2 writes, the arena names ARINIT
  writes, and the ids INFOVI, INFOCH and INFOTI set before they run VIMES, CIMES and TIMES, which seed the glossary,
  profile and tip descriptions. A card is named for its character, so cards are qualified by rarity, cost and attack;
  shop entries, which sell some things more than once, by their least chapter and rank. The export reads in 14 s: 18
  kinds, 303 description runs with none empty, 4967 snippets with no problem, and 1638 pictures, 72 of them explicit,
  all card art, found by looking at each. Kamidori's export is the same as before but for the `kind` table's new
  column.
  What the declarations still cannot say is in the backlog ("Game data export and the site"): a table keyed by another
  table's records, a card named by two elements of one field, and the 235 cells the builders wrote that no global
  names.
- **Joined tables and paired references** (2026-11-22; the user: "Go ahead with those 2, then we'll backlog 3 for now
  since that's RE work that will lead to increased export fidelity, not explicitly export fidelity alone"). The first
  two of the backlog's three are built. The lint checks each, and `tools/test_globals.py`,
  `tools/test_entities_build.py` and the exporter's tests (`AJoinedTablesRecordIsTheEntitysAndAPairOfElementsNamesARecord`,
  `AKindJoinsOnlyATableWhoseRecordsAreItsOwn`) cover them:
  - **A paired reference.** A `[table.refs]` value may be `{ table, times, pairs = true }`: elements 2k and 2k + 1 of
    one field are a record's two parts, 2k's value times `times` plus 2k + 1's. A kind publishes it by the first
    column of each pair, whose reference the export writes as a two-field reference's, and the lint refuses the field
    whole or a pair's second column. Arterial's battle publishes each side's leader card, side 0's being none where the
    player fights with their own deck (208 of 257 battles). `area_unit_cards` is declared too, though no kind shows
    an area's units.
  - **A joined table.** A registry table's `records_of` names the table whose record r its record r is: Arterial's
    `party_member` is the character's, by its character id. A kind's `joins` lists such tables, and its fields,
    groups, notes, labels and `referenced_as` name their fields as `table.field`. The exporter reads a joined cell as
    the kind's own, refusals, modder cells and script use included. An entity past the joined table's records has none
    of its cells: its values are withheld, with no refusal. The snippet writes the joined record under its own table
    (`[party_member.82]`). The exporter refuses a join whose table's `records_of` is not the kind's table. An Arterial
    character publishes `party_member`'s level cards, skills and 奥義 levels as the group `levels`.
  - **A group's row word.** A group's `row_label` names row r with its number ("Level 12"). The export's
    `group_label` table gained a `row_label` column, and its `labels` may be null.
  - **A snippet names the whole record.** Each published two-field reference now brings its second cell into the
    snippet: its `plus` field, or a pair's second column. A modder could not change the card a row names without its
    slot.

  Arterial's export reads 108 characters, 801 level cards and 278 level skills. It also reads 49 player and 257
  opponent leader cards, every one a card the export has. Kamidori's export is the same as before but for the
  `group_label` column. The third of the backlog's points, the unnamed cells, went to Arterial's reverse-engineering
  item there, by the user's word: naming them is RE work that the export gains from. The backlog keeps an item's card
  restriction too. A row alone names every card of the row, so it is no paired or two-field reference.
- **What the branch surfaced, settled before merge, and Himegari's section** (2026-11-22; the user: "Go ahead with
  the 4 items we can do now. Then I want to take the data we do understand from Himegari right now and also set that
  section up"). The lint checks each new form, and `tools/test_globals.py`, `tools/test_entities_build.py`
  (`test_loaded_where_and_joins`, `test_keyed_picture`) and the exporter's tests
  (`ARowReferenceNamesEveryRecordOfItsRow`, `AReferenceWithBasesNamesAListedValuesBasePlusItsPlus`,
  `ALoadedRowKeepsTheRecordsItsWhereNamesAndTheRecordItsJoinReaches`, `AKeyedPictureIsAskedForWithItsColourKey`, and
  the marks in `AShownFieldsValuesHaveTheRegistrysWordsAndTheGamesNamesAndItsNumbersTheirFormat`) cover them:
  - **Kamidori's overlapping arrays** were one registry error: its three battle routine arrays sat one RTINIT bank
    too late (`kamidori-data-tables.md`, "Enemy AI routines"). The registry has no Kamidori overlap left.
  - **A mark** (site plan §13, P13). A kind's `marks = { field = "word" }` gives a word the site shows after every
    link to an entity whose field is not none; the export's `mark` table holds them. Kamidori's `unit_boss_class` has
    its five values and `none = 0`, and a unit is marked "Boss", in a stage's enemies too.
  - **A row reference.** A `[table.refs]` value `{ table, times, plus, any = 0 }` names, where the plus field holds
    `any`, every record of the value's row (value × times to value × times + times − 1). The export writes one
    reference for each record the row has, under the same element, so the `ref` table's key gained `to_kind` and
    `to_id`, and the site shows an element's links side by side. 191 Arterial items name the cards that may carry
    them, 2,478 links ("Carried only by").
  - **A reference by bases.** `{ table, bases = { value = base }, plus }` names, for a value `bases` lists, its base
    plus the plus field's value, and none for another value. Kamidori's map object of type 32 (採取) or 33 (発掘)
    gathers from dig table 0 or 100 plus its primary parameter (SELSTAGE@0x17f8, FIELD@0x897e).
  - **A loaded table's `where` and `joins`.** `where = { field = [values] }` keeps the records whose one integer cell
    holds a listed value. `joins = { table = field }` publishes the fields of the record the row's field refers to as
    `table.field`, read from the run's state at that record and withheld past the table's records. A data file edits
    the scope's table, so the snippet leaves joined fields out. A field a join goes through to a table no kind shows
    links nothing and is not reported unpublished; to a kind's table it links as any reference. A kind's `labels` may
    name one of its loaded tables, and the site heads the table with it. Kamidori's stage loader publishes
    `stage_object` (a new registry table and scope table) as `object`, its map objects of type 32 and 33 joined to their
    dig tables: 267 dig sites on 75 stages, labelled "Dig site", and 1,034 item links ("Dug on stages"). The dig
    tables stay without a kind (the user).
  - **A keyed picture.** A kind's picture may be `{ field, key }`: the colour (0xRRGGBB) the game loads it with as its
    colour key, which the export makes transparent and names `-key-RRGGBB` after the catalog name
    (`IPictureSource.ResolveKeyed`). Himegari's map sprites need it: DRAWCH loads them with the key `0x00ff00`
    (DRAWCH@0x9c), and their sheets are drawn on green.
  - **Himegari's section.** The registry gains the item and skill names and summaries (GS `0x1bd2`, `0x1fba`,
    `0x23a2` and `0x24ce`, which ITINIT and SKINIT write before each record's numbers), value facets drawn from what
    its usages establish (categories, sexes, species, boss classes, the AFINIT element names, the skill ranges stored
    plus one, the sex mask), `[table.refs]` on its four tables, and two tables: `stage_catalog`, STINIT2's per-stage
    arrays, and `stage_goal`, the stage's four goal lines, which STINIT writes for the stage it loads (scope table
    `goal`). `vm-map/entities-himegari.toml` declares units, items, skills and stages, a stage's enemies and goals
    read by a loader at `himegari.stage.after_loader`. The export reads 284 units, 291 items, 131 skills and 78
    stages (69 with enemies and goals, 1,408 enemy rows), 422 description runs (ITMES and SKMES), nothing refused,
    no halt and no missing reference, and 424 pictures. Four battle figures are explicit, found by looking at every
    published picture.
- **Himegari's decoded tables** (2026-11-22; the user: "spin off a subagent to do more of the RE of the himegari record
  tables, and then take the resulting findings and also apply them to the site"). The decode playbook had closed
  most of Himegari's `*INIT` tables (`docs/vm-mapping-plan.md`, 2.2 to 2.3s, with their writeups in
  `build/games/himegari/data/`), and the registry named their globals, but declared only six tables. A delegated pass
  confirmed each field against the scripts that read it and declared fifteen more: `condition`, `element`,
  `character_profile`, `glossary_entry`, `magic_action`, `recipe`, `card`, `card_list_entry`, `training_action`,
  `terrain`, `object_type`, `map_texture` and `class_change` from the boot or CALCCC, and `stage_object` and
  `stage_map_texture` under `stage` (scope tables `object` and `texture`). It raised `unit_boss_class` and
  `unit_species_category` to high, named `stage_enemy_faction_id`'s armies (all three hostile to the player and to
  each other, so no part splits allies off as Kamidori's does), resized the element arrays to their sixteen rows
  (which cleared the registry's two overlap warnings), and found the icon sheets. `himegari-base` goes to 1.2.0.
  What the export needed:
  - **A sheet's colour key and last number.** A sheet picture may name `key`, the colour the game loads it with as
    its key, which the cut cell makes transparent as a keyed picture does (named `-key-RRGGBB`), and `last`, past which
    a number is no picture of it and is not reported unresolved. Himegari's icons are drawn on green (READICON@0x54),
    and an icon numbered from 501 is a cell of the append's sheet, which READICON puts beside the base one
    (READICON@0x5e to 0x11f; INFOIT@0x12c0 to 0x1306): a kind declares both sheets, `icon` to 500 and `icon_append`
    from 501. An append's own picture keeps its pack's prefix in the sheet's name (`$1$SO016A.AGF`), as the catalog
    names it.
  - **A patch point for the class changes.** `himegari.class_change.rules_run` (CALCCC@0x68) is where the rules have
    run and nothing is applied, as Kamidori's is. A stepped loader runs CALCCC for each unit's three rule columns with
    the others marked applied, the unit at level 99 and the familiar another unit: 73 title changes, the 71 base
    rules and the append's two.
  - **The rest is declarations.** Fourteen kinds: units (with icons and class changes), items, skills, stages (with
    enemies, their army, goals, and treasure: the chests among a stage's objects, `where` type 7 or 8, whose item
    `stage_object.type_id`'s `bases` reference names), recipes, conditions (a group of their five levels, as
    Kamidori's), elements, terrain, map objects, cards, training actions, magic (MAMES), and the information menu's
    character profiles (CIMES) and glossary (VIMES). The export reads 1,193 entities, 522 description runs (two of
    them empty: MAMES has no text for the growth rituals, 10 and 11), 333 loader runs giving 2,191 rows (641 chests),
    811 pictures, nothing refused and no picture unresolved. The 19 profile portraits are clothed and none is
    explicit. Card lists, where a map object stands, the unnamed cells INIT2 writes and two fields read at medium
    confidence are in the backlog's Himegari item; what a stage map needs is in its stage maps item.
- **Capture, recruiting and the summoning stones** (2026-11-23; the user: "can you see if we can also show: whether
  the unit can be recruited after being captured, if it can, what 'recruited' unit it links to, anything about the
  unit's summoning stone"). The registry had named the tables; how they connect was still open, and it reads off
  five scripts. A capture (FIELD@0x749d to 0x7518) passes the enemy to UNITECH, which joins it as its
  `unit_canonical_id` when it has one, a captive in a slot from 50 to 99 (UNITECH@0x50 to 0x16d): every capturable
  enemy with one counts as a summonable unit, 52 to 80, whose contract needs nothing, and the 39 with none stay
  themselves, whose contract requirement of 0 removes the action (SALLY@0x142c). SALLY counts each action it performs
  on a unit (`unit_sally_action_counts`, named now, as is `unit_joined_flags`); the contracts, brainwashings, sex magic
  and sacrifices added up reach `unit_summon_knowledge_threshold`, and SALLY gives item (`unit_summon_unlock_flag_index`
  − 1901), the unit's 喚石, items 51 to 80 (SALLY@0x17c7 to 0x18fa). ADDITEM sets a key item's flag 1900 + id, which is
  the one SUMMON tests (ADDITEM@0x92 to 0xd2, SUMMON@0x100f). GAMECLEAR keeps both new tables in the shared profile.
  - **A reference by offset.** `{ table, offset }` names, for a value above 0, the value plus the offset; it has no
    second cell. `unit.summon_unlock_flag_index` is `{ table = "item", offset = -1901 }`.
  - **The unit pages** show `canonical_id` ("Counts as", and on the unit it names "Its other forms" with whether each
    is capturable) and the stone ("Summoning stone", and "Summons" on the stone's page), with the capture, the
    contract and the stone's count in their notes. A unit without a threshold (ヒューリー, the bosses' forms) gets its
    stone another way, which the export does not say yet.
- **Kamidori's field tables** (2026-11-23; the user: Kamidori has the cards, terrain, map objects and elements that
  Himegari's section shows, and for elements: "keep the references to things that use it as an attack or modify that
  particular resistance", not the units, each of which has a grade for every element). The registry had named the
  globals; four tables declare them (`element`, `terrain`, `object_type`, `card`), and four kinds publish 7 elements,
  31 terrains, 50 map objects and 85 cards. What reading them settled: each card fills only its type's effects, so
  the type has its words; a card's `visual_asset_id` is a movie, the animation FIELD plays (`play-movie-to-surface`,
  FIELD@0xbc91), so cards have no picture; a map object's seven battle columns are a skill's first seven, as
  CALCBTPARAM reads both (@0x6c9 and @0x512), where columns 2 to 5 had been taken for display-only.
  - **The elements' links.** An item's `attack_element` and a terrain's `element_id` refer to the element, a skill's
    `attack_element_encoded` by `{ table = "element", offset = -1 }`, and an item's `affinity_level_deltas` names its
    columns by element (`columns_ref`), so an element's page lists the items and skills that attack with it, the
    equipment that changes its resistance and its terrain. A unit's resistances keep `columns_from`.
  - **A kind's `key_range`.** A kind may publish a range of its records (`key_range = [first, last]`, as a loader
    already could). It was made to leave out Kamidori's element 0, 無属, while no reference reached it; the next bullet
    links that 0, and no kind uses a range now.
  - **無属, element 0, is Omni.** The user asked for a deeper look and a way to show it, and named it Omni in English,
    the English patch's word in its descriptions (the patch names the element " --"). The battle scripts settle what
    it does: no unit resists it or is weak to it, since nothing sets its column (`docs/kamidori-data-tables.md`,
    "Combat pipeline"). What the export gained:
    - A reference's `zero` (`[table.refs]`) makes a 0 name record 0: `true` everywhere, or `{ field, values }` where
      another one-cell field holds one of the values, a 0 elsewhere staying none. Its nones are -1 among the keyed
      ids, so `Link` takes its 0. A skill's attack element is `{ table = "element", offset = -1, zero = true }`, and an
      item's is `zero = { field = "category", values = [...] }`, the innate attacks (0) and the weapons (11 to 25).
      ティアン頭① to ⑤, Tian's costume heads, are of category 0 and link it too.
    - The element kind publishes all eight.
    - `[export] text_overrides` gives a game string the declarations' text in a language, in place of the package's,
      wherever the export publishes the string: Kamidori's `{ en = { "無属" = "Omni" } }` names the element's page,
      its value names and its columns. The report counts the strings overridden and names an override no string used.
    - The site shows a reference's 0 when the export linked it (`format.mjs` `noneIn`); the registry's word for
      column 0 of an item's resistances is `omni`, no longer `unused`.
  - Open, in the backlog's "Kamidori's field tables": the floors a card is dealt on, where a map object stands, the
    doors' keys, INIT2's 98 unnamed cells (the element kind makes INIT2 a
    builder) and the fields read at medium or low confidence.
