# Game data export — implementation plan

**Date:** 2026-11-21
**Spec:** `docs/superpowers/specs/2026-11-21-game-data-export-design.md` (cited as "spec §n"); the site's design, in
the `ome-site` repository at `docs/specs/2026-11-21-site-design.md`, is cited as "site §n". The spec stays the design's
home. This plan keeps what each milestone checks and builds, what that settles for the later ones, and what changed
since the spec was accepted (§5).
**Status:** milestones 0, 1, 2 and 3 built (2026-11-21; milestone 2 in §4.1, milestone 3 in §4.2). §4 gets each
later milestone's section before it starts.
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
    element (`unit[1].stats.max_hp`). Values with a meaning per number (a race, a category) have no names in the
    registry, so the export still gives them as numbers.
- **A loader publishes its tables** (milestone 3, §4.2). Spec §6.3 runs loaders but does not say where their output
  goes. The `[[loader]]` entry names the tables of its scope it publishes, with `exists` and `fields` as a kind has
  them, and the export gives them three things spec §7.2 lacks: the `loaded` and `loaded_field` tables, a
  `"<kind>.<name>"` table of rows (`stage.enemy`), and references from the key entity with the field written
  `<name>.<field>`. A loaded table has no text yet, and its cells, writers and data file snippet wait for milestone 4.
- **The spec's `field_floor_entered` is `party_on_field`** (`0x428c`, milestone 3): the registry's name for it.
