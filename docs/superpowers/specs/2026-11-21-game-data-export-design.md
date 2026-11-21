# Game data export design

**Date:** 2026-11-21
**Status:** accepted by the user on 2026-11-21 ("Go ahead and approve it and get started"), after they decided its
questions (§14) and an independent agent review of the same day checked it against the code. Its plan,
`docs/superpowers/plans/2026-11-21-game-data-export.md`, keeps what changed since, the departures from this text
included.
**How it was decided:** the backlog item "A project site with each game's data as a knowledge base" (2026-11-19),
the user's outline (`Site Ideas.md` at the workspace root), and the user's answers of 2026-11-21 (§3.1). The site
that reads what this design produces has its own design, in its own repository: `ome-site`, beside this one, at
`docs/specs/2026-11-21-site-design.md` (cited below as "site §n").
**Checked against:** `fcfc12a2` on `feature/backlog-batch`, and Kamidori's disassembly in
`build/games/kamidori/disasm/`. Code references name files and members, not lines. Paths are relative to
`age-reimpl/`; a short path is under `engine/`.
**Direction:** `docs/remake-architecture-and-roadmap.md` (an open engine with first-class modding), and the rule the
user set on 2026-11-16: the engine knows nothing about a game, also where two games happen to work alike.
**Builds on:** the game registries (`vm-map/globals-<game>.toml`, their `[[table]]` and `[[scope]]` entries), the
base mods and their record maps (the data patches design, `docs/superpowers/specs/2026-11-17-data-patches-design.md`),
the patch points (`vm-map/patch-points-<game>.toml`), and the localization design
(`docs/superpowers/specs/2026-11-10-localization-design.md`, its §5.2 string stores and §6.4 lookup order).

**Not verified.** Each is work for the plan:

- that Kamidori's boot writes every table string through an instruction that records an origin (`set-string`,
  `mov`, `concat`, `toString`, the `lookup-array` copy). The converted translation package translates INIT2's own
  table literals at their origins (`docs/localization-import-test.md`), which suggests so. A string with no origin
  is refused (§5.5), so a miss shows in the first export's report;
- that the writer record (§6.2) costs little on a boot of Kamidori's size;
- that seeding the stage, the continue mode and the floor-entered global takes FIELD to
  `kamidori.stage.after_loader` for every stage with a map (§6.3);
- that a description script entered headless with only its seed shows its text, and how its `show-text` pieces
  and line breaks join into lines (§5.7);
- Kamidori's record counts in §13, which come from `docs/kamidori-data-tables.md` (the Python extractors), not from
  a run of the exporter.

## 1. Purpose and scope

### 1.1 The two products

OME produces two things the website reads:

- **The game export:** one game revision's data, written to SQLite with its pictures. A page for every item, unit,
  skill, quest and recipe is built from it (site §8). It holds game data, so it is made on a machine with the game
  installed and is never committed (§9).
- **The reference bundle:** OME's modding guides and the registries' structured data, for each OME release. The
  site's modding reference is built from it (site §7). Its files build from this repository alone, so CI builds it.

### 1.2 What this design delivers

- **Entity declarations:** which record tables are kinds the site shows, how a record of each is named, described
  and pictured, which fields refer to other records, and what may be published (§5). They are data beside the
  game's registry, so the exporter knows no game.
- **What the engine adds for it:** recording where values came from, and stopping a run at a site (§6.2).
- **The exporter:** an `Ome.Cli` command that boots a game headless, reads the declared tables as the game builds
  them, and writes the export (§6).
- **The export format** (§7).
- **The reference bundle** and the new script index it needs (§8).

### 1.3 Not in this design

- **The site.** Its design is in `ome-site` (site §1). This design fixes only what the site reads.
- **Maps, shops and class changes.** They need more than a table read (§11).
- **An export of a modded game.** The exporter reads the unmodded game. Running it with a mod stack, so a mod's
  author sees their mod's data on the site, is future work (§11).

## 2. What the code gives to build on

- **A headless run.** `GameSession.RunScene` (`Ome.Engine.Vm/GameSession.cs`) seeds a new `VirtualMachine` from
  the session's banks, runs a script with `HeadlessWaits`, which answers every wait at once, and merges the banks
  back after `vm.Run()` returns. `Ome.Cli init-image` runs a list of table scripts this way. The data patch
  workspace tests boot Kamidori's install through INIT2 the same way (`DataPatchWorkspaceTests.Files.cs`,
  `KamidoriBoot`) and reach a stage's tables by seeding FIELD (`DataPatchWorkspaceTests.Stages.cs`).
- **What a run cannot do yet.** `RunScene` has no way to turn on string-origin recording, and the origin records
  are lost when the run's VM goes. It cannot stop at a site: the tests stop a run by throwing from an `ITraceSink`,
  which skips the merge, and a sink is observe-only by contract (`Ome.Engine/Diagnostics/ITraceSink.cs`). The VM
  itself has both parts: `VirtualMachine.RecordsStringStores` and `RequestStop()`. §6.2 adds them to the session.
- **The points where tables are complete.** Kamidori's base mod raises `kamidori.data.ready` at
  `kamidori.boot.data_ready`, which is INIT2's exit (`INIT2@0x2cf`), and stage tables are complete at
  `kamidori.stage.after_loader` (`FIELD@0x527`). Arterial's are `arterial.boot.data_layered` and
  `arterial.area.after_loaders`. A base mod's `layout/points.json` gives each point's script and offset, and
  `ModLayout` (`Ome.Mods/Packages`) reads it.
- **The record map.** A registry's `[[table]]` entries name each table's fields and the global behind each.
  `tools/globals_build.py` writes them into `build/games/<game>/globals.json`, which `GameModule.LoadGlobals`
  (`Ome.Script/Modules/GameModule.cs`) reads as `TableSymbol`s; `tools/base_mod_build.py` writes them into the base
  mod for Lua. No C# code reads a table field's values by name yet. The exporter does, through `GameModule`.
- **Values.** `GameSession.Globals` and `GlobalStrings` give the banks. Strings are already decoded; a byte CP932
  cannot round-trip is `U+F0000 + byte` (`Sys4TextEncoding`).
- **Where a string came from.** With recording on, `set-string`, `mov`, `concat`, `toString` and the `lookup-array`
  copy record the instruction that stored a global string cell (`TextManager.OriginOfCell`). A copy keeps its
  source's origin. Localization scopes a table string's translation by it.
- **Shown text.** `VirtualMachine.Emitted` lists each text a run shows, with its script and offset.
- **Pictures.** Tables hold packed resource ids (Kamidori's item icons `IC0001.AGF`…, skill icons `IC2001.AGF`…,
  quest requester icons `IC3xxx.AGF`, a unit's 17 graphics columns). `ResourceMap.ResolveTexture` names one and
  `ResourceMap.DecodeTexture` decodes it; `Ome.Cli picture` writes one to PNG.
- **Translations.** `TextPresenter.Present(source, site, origin)` translates a string without drawing it, from a
  language loaded by `LanguageLaunch.Load` or `TextRootLoader.Load`. The source language is
  `LanguageTag.Source` (`ja`).
- **An install's identity.** `OmsPatchPlan.FingerprintOf` gives a file's `sha256:` fingerprint, the form mods'
  `[game] revisions` match against `SYS4INI.BIN`. `Sys4AssetCatalog.AppendPacks` lists the mounted append catalogs.
  The profile gives the engine build. (`Installs.Recognize` does more, but it is the launcher's, in
  `Ome.Launcher.Core`, which `Ome.Cli` does not reference.)
- **Data files.** `DataDocumentReader` reads a `<game>.tables` file. Nothing writes one yet.
- **Synthetic test data.** `SyntheticCatalog` (internal to `Ome.Engine.Tests`) builds a catalog, `ScriptAssembler`
  builds scripts.

## 3. Decisions

### 3.1 The user's (2026-11-21)

- **One domain** holds the modding reference and the game database.
- **The database starts with Kamidori.**
- **Comments "would be nice eventually"**: players answer what data mining cannot ("how do you complete this
  quest?"). For this design that means stable keys (§5.6).
- **No scene text.** These are visual novels, and their story text "may cross the line". Quest text and the other
  data fill the niche of Wowhead and the existing wikis (§5.5). The rule is about the story scenes only: "The only
  reason why I'm particular about scene text is due to the nature of this being a VN".
- **The site is a separate repository** (`ome-site`). OME keeps the exporter, the entity declarations and the docs.
- **The stack is open**, and the site need not be static. The site design proposes one (site §5).
- **Kamidori's English text is published, tentatively.** The user's words: "a tentative yes for now, assuming that
  the data text is in the same ballpark as the original. If we get asked to take it down, we can address it then"
  (§6.4).
- The answers to this design's own questions are in §14.

### 3.2 This design's

- **The exporter knows no game.** What a game publishes, and from where, is in its declarations (§5).
- **Values come from the game's own scripts.** The exporter runs the game's boot and loaders, as the base mods'
  events see them, rather than parsing the table scripts on its own. The Python extractors (`tools/extract_init.py`,
  `tools/init_kamidori_*.py`) stay as a cross-check.
- **Text an information screen shows for a record is data**, as quest text is: an item's or a skill's description,
  a glossary entry, a character profile, a chapter objective. The scripts that show it are named in the
  declarations (§5.7). A story scene's text is not data. The user agreed (§14, answer 6).
- **Only declared fields and descriptions reach the export**, and the exporter checks at run time where each value
  came from (§5.5).
- **The export is deterministic:** the same install, OME commit, declarations and options write the same bytes
  (§7.3).
- **Game data stays off public storage** (§9).

## 4. Terms

- **Kind:** a sort of entity the site shows, such as `item` or `quest`, built from one record table.
- **Entity:** one record of a kind, such as item 123.
- **Entity key:** (game, kind, id). Stable across exports and game revisions (§5.6).
- **Script name:** a script's name with the `$n$` prefix of a loose update or append dropped, so `$1$CGINIT` is
  `CGINIT`. Every script list in the declarations matches script names.
- **Builder:** a script whose stored values may be published. Its script name matches a `builders` pattern and no
  `excluded_builders` name.
- **Origin:** the instruction that stored a string, carried through copies (§2).
- **Writer:** the script that last wrote a global cell, as the writer record keeps it (§6.2).
- **Text script:** a script whose shown text may be published as a description (§5.7).
- **Loader:** a game script the exporter runs once for each key of a scope, to build an on-demand table (§6.3).
- **Script relation:** a fact found in the game's scripts, such as "scene SC1234 gives item 56" (§6.7).

## 5. Entity declarations

### 5.1 Where they live

- **References** are facts about fields, useful to modders as much as to the site, so they go on the field's
  table in the game's registry, `vm-map/globals-<game>.toml`, as a `[table.refs]` table (§5.3).
- **Everything else** is what the site publishes, a new category, so it gets its own file:
  `vm-map/entities-<game>.toml`. A builder, `tools/entities_build.py`, checks it against the registry and writes
  `build/games/<game>/entities.json`, which the exporter reads.

### 5.2 A kind

```toml
[export]
boot = { script = "INIT2", read_at = "kamidori.boot.data_ready" }
builders = ["INIT2", "*INIT", "*INIT2", "AUTORUN"]   # §5.5
excluded_builders = ["CGINIT", "SPINIT", "EVINIT"]   # never builders, whatever pattern matches them
text_scripts = ["ITMES", "SKMES", "VIMES", "CIMES", "FAMES"]   # §5.7

[[kind]]
id = "item"                     # stable (§5.6); never `docs` or `api` (site §4)
table = "item"                  # a [[table]] of the registry
exists = "name"                 # a record exists when this field is not empty or 0
title = "name"
text = ["short_description"]    # string fields published as text (§5.5)
fields = ["category", "rank", "price"]      # published values; the rest only in the modder data (§6.6)
filters = ["category", "rank"]  # the site's list filters (site §8.3)
pictures = { icon = "icon_asset_id" }       # name on the site -> field holding a packed resource id (§5.4)

[[kind.description]]            # §5.7
name = "description"
script = { field = "handler_script_id" }
seed = { current_item_id = "key" }
```

- `exists` decides which of a table's records are entities: Kamidori's quest table has 2,000 slots and 187 quests.
  A kind may add `skip`, conditions on fields that drop a record the game never shows (Kamidori's 20 `仮`
  placeholder quests, gated on flag 10000).
- A field is named as the record map names it, a column by its name after a dot: `graphic.face_CA_AA`. The record
  map refuses a column number where the column has a name.
- `fields`, `text` and `filters` name what the site shows. A field left out is still in the modder data, so a
  modder sees every named field; a string field left out of `text` shows there without its value (§6.6).
- The builder lints every name against the registry, every `filters` entry against `fields`, every picture field's
  type (an integer), every seed's globals, and the script lists' patterns (an `excluded_builders` name is a plain
  script name).

### 5.3 References

```toml
[[table]]
name = "unit"
# … fields as today …
[table.refs]
drop_item = "item"              # every element is an item id; 0 is none
initial_equipment = "item"
skill = "skill"
graphic = "picture"             # packed resource ids
```

- A reference names a kind by its table, or `picture` for a packed resource id. The four tables the registry has
  already give Kamidori's first references: a unit's drops, equipment and skills, an item's granted skill and
  garden product, a stage enemy's unit.
- `tools/globals_build.py` learns them: its table lint checks each names a field and a table, and it writes them
  into `globals.json` and the global reference. Without that they would be dropped on the way to the exporter and
  the bundle.
- The exporter writes each reference as a row (§7.2), so the site lists both directions with one query: a quest's
  reward items, and on an item's page the quests that reward it.
- A value that names no existing entity is written with its target missing, and the export's report counts them.
  A game's data can hold such values; they are worth a look, not an error.
- **The same annotation helps modders.** The base mod's shape could check that a data file names an existing
  record. That is the data patches design's business, offered, not part of this design.

### 5.4 Pictures

A picture field holds a packed resource id. The kind's `pictures` table names each picture for the site (`icon`,
`face`, `sprite`). Only pictures a declared picture field names are exported, so a picture reaches the site only
through a declaration and the run-time check (§5.5). A field whose ids name cells of a sheet rather than whole
pictures needs a cell rule; none of Kamidori's first kinds does.

For Kamidori's units the user chose the face, the bestiary sprite and the battle sprites (2026-11-21). In the record
map's names, from the registry's `unit_graphic_asset_ids`, they are column 4 `face_CA_AA`, column 7
`map_sprite_compact_CP_AA` (the bestiary and town sprite), and columns 11 to 14, the `battle_sprite_…` columns. The
other columns are not published: status art, room art, the other faces, the directional map sprite and the
cut-ins. The registry names the columns from file prefixes, with medium confidence; milestone 1 checks each
column's pictures by eye before it is declared.

### 5.5 What may be published

The user's rule: no scene text; quest text and the other data are published. It holds by declaration, and the
exporter checks every value at run time.

- **By declaration.** Only declared fields are exported, and only strings from fields listed under `text`, or
  descriptions shown by text scripts (§5.7). `builders` names the scripts whose stored values may be published.
  `excluded_builders` names the scripts that are never builders, whatever pattern matches them: Kamidori's CG
  gallery (CGINIT), H-scene replays (SPINIT) and event-CG substitution (EVINIT).
- **At run time.** The exporter refuses a published value whose writer (§6.2) is not a builder of the run that read
  it, and a published string whose origin is not in a builder or that has no origin. So a scene's string copied
  into a table cell is refused, and so is a table an excluded builder wrote, its pictures included. A cell no
  script wrote holds its default and is published as such.
- **A refused value** is left out of the export and listed in the report with its cell, its writer or origin, and
  its kind. The command then ends with exit code 1, so a refusal is never missed; the files are written without
  the refused values, for a look.
- **Shown text** is read only from description runs (§5.7). A string drawn by any other run is never read: the
  exporter reads the global banks, and discards the shown text of the boot and the loaders.
- **Script relations** (§6.7) are facts about scenes ("this scene gives item 56"), not their text.
- **Text from a language package** is the same published text in another language (§6.4).

The registry's `[[table]]` entries say only when a table is built (`built_by = "boot"`), not by which scripts, so
no core gate can tell which scripts write a table. The run-time check is where the rule is enforced; the core gate
checks the declarations' form (§12).

### 5.6 Stable keys

The site's comments attach to an entity key (site §9), so keys outlive every export:

- **A kind's id never changes once published.** A renamed kind keeps its old id in `aliases`, as a renamed table
  does, and the export lists both.
- **The record id is the game's own:** the item id, the quest id. Game revisions keep them.
- The export carries each entity's key and its content hash, so the site can tell a new entity from a changed one.

### 5.7 Descriptions

Some of a record's text is not in a table cell. Kamidori's item and skill descriptions are shown by a handler
script the record names (`item_handler_script_id`, `skill_handler_script_id`: ITMES or SKMES, or one of their `$n$`
updates), run with the record's id in `current_item_id`. The glossary (VIMES), the character profiles (CIMES) and
the chapter objectives (FAMES) work the same way. A kind declares such text as a description:

- `script` is the script to run: a field holding the script id the game calls for the record
  (`{ field = "handler_script_id" }`), or a script name (`{ name = "FAMES" }`).
- `seed` sets globals before the run, as a loader's does (§6.3).
- The exporter restores the boot snapshot, seeds, runs the script, and keeps the text that script itself shows,
  in order, with each piece's script and offset. Text shown by any other script in the run is dropped and counted.
- The script's name must be in `text_scripts`, or the description is refused (§5.5). Each entry of `text_scripts` is
  a decision that a script's text is data (§3.2); a scene script is never one.

## 6. The exporter

### 6.1 The command

```
Ome.Cli data-export --game kamidori [--game-root DIR] [--language PACKAGE …] [--output DIR]
```

- Default output: `build/games/<game>/export/`, which is ignored by git.
- It writes `<revision>.sqlite`, `pictures/<revision>/…` and `manifest.json` (§7.1), and prints a report: kinds and
  counts, missing references, unresolved script relations, translation misses, refused values (§5.5).
- The code lives in a new project, `engine/Ome.Export`, so the SQLite dependency stays out of the engine.
  `Ome.Cli` calls it. The dependency (Microsoft.Data.Sqlite) goes in `THIRD_PARTY_NOTICES.md`. Its tests live in
  `Ome.Engine.Tests`, beside the synthetic helpers.

### 6.2 Runs, and what the engine adds

The export needs three things of a headless run that `GameSession.RunScene` does not give (§2). Each is a general
engine feature, tested on synthetic scripts, with no game in it:

- **Recording, as a run option.** String-origin recording, and a new **writer record**: for each global cell, the
  script that last wrote it. The session keeps both across runs, as it keeps the banks, and its snapshots carry
  them, so a restore brings them back.
- **A stop at a site.** A run option naming a script and offset. When the run reaches it, the VM stops through
  `RequestStop()`, and the session merges the banks as at a normal exit. A trace sink stays observe-only.
- **Shown text out of the run.** The run's result keeps `Emitted` with each piece's script and offset (it does
  today) and the session passes it to the exporter.

The boot:

1. Load the install as the workspace tests do: the catalog, the store, the script provider and the profile's
   script table.
2. Run the boot script the declarations name (`INIT2`) with recording on. Kamidori's `read_at` is INIT2's exit, so
   the boot runs to its end; the exporter checks that it ended at the point's site (`layout/points.json`), and
   fails if not. A `read_at` before the exit is a stop at its site.
3. Read each kind's table through `GameModule`'s table symbols: for each record that `exists`, every named field.
4. Keep the session's snapshot, with its records, as the starting point for loaders and descriptions.

### 6.3 On-demand tables

Some tables are built only when the game needs them, such as a stage's enemies when the stage loads. Each is
declared as a loader:

```toml
[[loader]]
scope = "stage"                                 # a [[scope]] of the registry
keys = "stage"                                  # a kind whose ids are the keys
skip = { event_entry = 1 }                      # keys it cannot build; a stage catalog field (milestone 2)
script = "FIELD"
seed = { current_stage_id = "key", field_map_continue_mode = 0, field_floor_entered = 1 }
read_at = "kamidori.stage.after_loader"
```

- For each key the exporter restores the boot snapshot, writes the seeded globals, runs the script with a stop at
  `read_at`, and reads the scope's tables. A loader may add `builders` patterns of its own for the scripts that
  build its tables; the run-time check (§5.5) holds as for the boot.
- **Kamidori's stages need two things the registry lacks today.** The workspace tests also seed `G[0x428c]`, an
  unnamed global, so that FIELD skips the arrival scenes a booted session's story state reaches at some floors
  (`FIELD@0x1ef` to `0x304`, before the read point). It gets a name in the registry before the loader is declared;
  `field_floor_entered` above stands for it. And the 38 event-entry stages have no map: the stage catalog's
  event-entry flag (`0x14c8cc`) skips them, once the stage catalog is a table (§13, milestone 2).
- A run that ends before `read_at` is reported with its key, and the export goes on.

### 6.4 Text and languages

- The source text is the game's, tagged `LanguageTag.Source` (`ja`).
- `--language` names a language package. Each published table string is looked up with `TextPresenter.Present` at
  its origin, and each piece of a description at its `show-text` site, as the engine translates them when drawn. A
  miss keeps the source text and is counted in the report.
- The export records each package's id, version and authors, so the site can credit them.
- **Publishing a language is the site's decision.** Kamidori's English comes from a fan translation, which OME's
  docs say to share only with the translators' permission (`docs/modding/languages.md`). The user decided on
  2026-11-21 to publish its text tentatively and to take it down if asked (§3.1). The site credits the
  translators; the converted package itself is still never committed or shipped (§9). The exporter makes the
  text; it does not publish it.

### 6.5 Pictures

- For each declared picture field, the packed id is resolved to its catalog record (`ResourceMap.ResolveTexture`)
  and decoded (`DecodeTexture`), as `Ome.Cli picture` does.
- Each distinct picture is written once, as PNG, under `pictures/<revision>/<catalog name>.png`. A picture shared
  by many entities is one file.
- The export records each entity's picture by its catalog name, its size and its content hash.

### 6.6 Modder data

What the site's modder mode shows (site §8.7), for each entity:

- the table, the record index, and each named field's name as OMS and data files write it (`unit[761].stats.max_hp`,
  `[unit.761] stats.max_hp`);
- each field's global, bank and address, and the cell's address for this record;
- each field's raw values and their writer: which script, and so which layer (`ITINIT`, `$1$ITINIT`), set the
  value. A string field the kind does not publish under `text` shows its address and writer, not its value (§5.5);
- a data file that sets this entity's published fields to their current values, ready to edit. It is written by a
  new `DataFileWriter` in `Ome.Mods/Data`, which the tests check by reading its output back with
  `DataDocumentReader`;
- the scripts that read or write the table's globals, from the script index (§8.2);
- each picture's catalog name.

Later (§13, milestone 4): arrays the builders write that no field names yet. The global map knows arrays by shape
(`build/games/<game>/global-var-map.json`); one with as many elements as the table has records, written by the
table's builders, is listed beside the record as "unnamed, by shape". That helps reverse engineering, and is
labelled as inferred. Only its integer values are shown.

### 6.7 Script relations

Wikis list where an item is given by playing. The scripts say it directly. Kamidori gives or takes an item by
setting two globals and calling ADDITEM, as the base mod's `give_item` does.

```toml
[[script_relation]]
call = "ADDITEM"                                # a call-script target
kind = "item"
key = "current_item_id"                         # the global holding the entity id
count = "current_item_count_change"             # optional
id = "given_in"                                 # a positive count
negative = "taken_in"                           # a negative count
exclude = ["DEBUG*"]                            # scripts not scanned
```

- The exporter scans every script for calls to `call`. For each argument global it takes the last write before the
  call within the same block (from the previous label), when that write is a constant: `mov G N`, or `sub G 0 N`,
  Kamidori's idiom for `-N`. A block with no such write, or whose last write is computed, leaves the call
  unresolved.
- Each resolved call is a row (§7.2): the entity, the relation, the calling script and offset, the count.
- **Kamidori's numbers** (a scan of the disassembly, 2026-11-21): 143 calls in 65 scripts. 122 set both values from
  constants: 39 give and 83 take, the taking ones mostly in the `$3$SC29xx` scenes. DEBUGADV holds 2 of them, which
  the exclusion drops. The 21 computed calls are in 14 scripts: FIELD, GAMESTART, SHOP, UNITECH, ALCHEMY,
  CHECKIMPROVE, INVITECH, RECEIPT, REWARD, SETSTUDIO, STAGECLEAR and three debug scripts. They are counted as
  unresolved in the report, not guessed.
- Unit gifts (UNITECH) follow once the globals they take are named in the registry.

### 6.8 Identity and revisions

An export is one game revision, as the game's data stands:

- **The revision id** is the first 12 hex digits of a SHA-256 over the sorted names and fingerprints of
  `SYS4INI.BIN`, each mounted append catalog (`Sys4AssetCatalog.AppendPacks`) and each loose script the script
  provider mounts. Two installs that share a catalog but differ in their appends or updates get different ids. It
  names the files.
- The export records the game id, title, catalog revision (`S4IC433`), engine build (`4.33G`), the append layers
  present, and `SYS4INI.BIN`'s full fingerprint (`OmsPatchPlan.FingerprintOf`), the value mods' `[game] revisions`
  match.
- The game version is the machine's report (the launcher reads it from the registry), so it goes only in
  `manifest.json`, which keeps the database the same on every machine (§7.3).
- Kamidori's clean install (`games/kamidori`) is the reference. The translated install is a different engine build
  (4.33F) and its own revision; it is not exported.

## 7. The export format

### 7.1 Files

```
<output>/
  <revision>.sqlite
  pictures/<revision>/<catalog name>.png
  manifest.json        export time, command line, game version, OME version and commit, file hashes
```

The export time and the game version are only in `manifest.json`, so the database itself is reproducible.

### 7.2 Tables

| Table | Rows |
|---|---|
| `build_info(key, value)` | format version, game, title, revision, fingerprint, catalog revision, engine build, layers, languages and their packages, OME version and commit, the declarations' hash |
| `kind(id, table_name, title_field, aliases)` | one per kind |
| `field(kind, name, type, published, text, filter, ref_kind, picture)` | one per named field of a kind's table and one per description, so the site's generic templates know what each is |
| `<kind>(id, …, content_hash)` | one table per kind, named by the kind: a column per published scalar field, a JSON array per array field |
| `text(kind, id, field, idx, lang, value)` | each published string and description in each language |
| `ref(from_kind, from_id, field, idx, to_kind, to_id)` | each reference; `to_id` set and the target missing when no such entity exists |
| `relation(kind, id, relation, script, offset, count)` | each script relation |
| `picture(kind, id, name, file, width, height, hash)` | each entity's pictures |
| `cell(kind, id, field, idx, bank, address, raw, writer)` | every named field's cells; `raw` empty for a string the kind does not publish (§6.6) |
| `snippet(kind, id, text)` | the ready data file (§6.6) |
| `script_use(kind, script, reads, writes)` | which scripts touch the kind's table (§8.2) |

- One table per kind keeps the file easy to browse in Datasette and to query by field.
- Text is apart from values so a language adds rows, not columns.

### 7.3 Determinism and hashes

- Rows are written in key order, with a fixed page size, and the file is vacuumed. Nothing in it depends on the
  machine or the time. The same install, OME commit, declarations and options give the same bytes. A test exports
  twice and compares.
- **Content hash:** SHA-256 over a canonical form of an entity's values, text, references, relations and
  pictures' hashes. The site skips unchanged pages and compares revisions with it (site §8.8).

## 8. The reference bundle

### 8.1 Contents

A zip for each OME release, `OME-reference-<version>.zip`:

- `docs/modding/**`, as committed;
- `opcodes.json` (`tools/opcodes_build.py --build`);
- for each game: the curated globals with their record tables and references (`tools/globals_build.py
  --build-json`), the base mod's `layout/globals.json`, `layout/points.json` (the patch points), its events
  (`events/*.toml`) and the data shape (`shapes/tables.schema.json`), and the script index (§8.2);
- the games and their support levels, from the profile manifests;
- `bundle.json`: the format version, OME version and commit, the games, and every file's hash.

Everything in it builds from the repository alone. The globals the auto map infers from the game files are not
in it: they have no names, and CI has no game files.

### 8.2 The script index

No tool indexes scripts today. The site's script pages (site §7.2) and the modder data's script list (§6.6) need
one:

- `tools/script_index.py --game <game> --build` reads the installed scripts and writes, for each script: its
  callers and callees (`call-script`), the named globals it reads and writes (by the operand access contracts in
  `vm-map/opcodes.toml`), the patch points in it, and whether it waits for input. It holds no text.
- It needs the game files, so it is built where the game is installed and committed, as
  `docs/script-index-<game>.json`, beside the generated references that already name scripts and offsets. A
  workspace gate rebuilds it and fails on a difference. The committed file holds only what comes from the game.
- What a script does, in words, is hand-written in a new source, `vm-map/scripts-<game>.toml` (a summary, what its
  argument globals mean). `reference_bundle.py` merges it with the index when it builds the bundle, so editing a
  note needs no game. A core gate checks that every note names a script the committed index has.

### 8.3 Publication

- `tools/reference_bundle.py --output DIR` builds the zip from the repository.
- The release workflow builds it and publishes it as a fourth release asset: `tools/publish_github_release.py`'s
  expected assets grow by one, and `prepare_release_assets` writes its checksum into `RELEASE-SHA256SUMS` with the
  others. Each OME version's reference then comes from the release's own commit, with no second workflow to keep
  in step (§14).
- The site's build can also read a local checkout, to preview a guide before a release (site §10.2).

## 9. Where game data may go

- The exporter runs only where the game is installed. Its output stays under `build/`, which git ignores, until
  the user copies it to the site's private storage (site §10.1).
- No export, picture or extracted string is committed to this repository or to `ome-site`, and none is attached to
  a release. The script index (§8.2) holds names, offsets and global addresses, as the committed generated
  references do, and no text.
- CI never runs the exporter on game data. Its tests run on synthetic catalogs and scripts (§12).

## 10. Alternatives considered

- **The Python extractors as the exporter.** `tools/init_kamidori_*.py` already parse Kamidori's tables. Set
  aside: they are a second reading of the game, per game, while the engine runs the game's own scripts, sees the
  append layers as the boot applies them and reads the record map a mod reads. They stay as a cross-check.
- **A Lua mod as the exporter**, run in the port at the base mod's events. It would use `maid` as a mod does. Set
  aside: Lua is slow at building large tables in this runtime, and an export needs pictures, SQLite and a
  deterministic file, which belong in C#.
- **Entity declarations in the base mod.** The base mod is what Lua mods name the game through. What the site
  publishes is not a modding interface, and the references are registry facts, so both live with the registry.
- **Each table's builder scripts in the registry**, so a core gate could refuse a kind whose table an excluded
  builder writes. Set aside: which scripts write a table is a fact of the installed game, which only a run with the
  game can check, and the writer record does that on every export (§5.5).
- **One generic value table instead of one table per kind.** Simpler to write, harder to browse and query. Per-kind
  tables cost the exporter little, since the declarations name every column.

## 11. Future work

- **Shops and class changes.** Kamidori's shop catalogue (SHINIT) is gated in code by the craftsman's rank and
  story flags, and its class-change rules (CCINIT) test the character, event bits and level. A run from one state
  shows only what that state unlocks, so neither is a loader. Reading them needs the rules read from their
  scripts, or a run for each gate: a design of its own.
- Maps: Kamidori's dungeon floors (MPINIT, LAINIT, OBINIT) rendered to pictures with their objects and enemies.
- An export with a mod stack, for a mod's author.
- Pages of what changed between two game revisions, from the content hashes.
- The base mod's shape checking references in data files (§5.3).
- Arterial and Himegari declarations.

## 12. Validation gates

- **Core:** `entities_build.py --check` for every game with declarations: names, references, seeds and script
  lists (§5.2). `globals_build.py`'s table lint checks `[table.refs]` (§5.3).
- **Core:** the engine's tests of the run options (§6.2) on synthetic scripts: the writer record and origins kept
  across runs and restored from a snapshot, a stop at a site that keeps the banks.
- **Core:** the exporter's tests on a synthetic game: a boot script that builds a table, a loader, a description
  run whose text script and a scene script both show text (only the first is kept), a scene that gives an item and
  one that takes one, a string a scene copies into a table cell (refused), a table an excluded builder writes
  (refused), a string with no origin (refused), the exit code on a refusal, two exports compared byte for byte, and
  `DataFileWriter`'s output read back.
- **Core:** `reference_bundle.py --check`: the bundle builds, its manifest lists every file, and every script note
  names an indexed script.
- **Workspace (Kamidori):** an export of the clean install, with no refused value, compared with the Python
  extractors' counts for each kind; `script_index.py --check`.

## 13. Milestones

Kamidori throughout. Each milestone ends with the user able to look at the result in Datasette or on the site.

0. **Declarations.** `vm-map/entities-kamidori.toml`, `tools/entities_build.py` and its gate; `[table.refs]` on the
   four tables the registry has (item, unit, skill, stage_enemy), with `globals_build.py` writing and checking them.
   The new sources and builders get their rows in AGENTS.md's generated-files table and update triggers, and in
   `docs/tools-reference.md`; `docs/PROJECT-STRUCTURE.md` gets `engine/Ome.Export`.
1. **The first export.** The run options (§6.2), the run-time check (§5.5), `Ome.Export` and `Ome.Cli data-export`:
   items (about 1,300 with the append layers), units (about 450) and skills (about 450), with their references,
   icons and unit pictures, names, short descriptions and descriptions (§5.7), and the modder data's fields, cells
   and writers; determinism. The site's milestone 2 builds on it.
2. **More of Kamidori's tables.** Quests (187), recipes (about 490), gathering drops, conditions, the glossary, the
   character profiles, the chapter objectives and the stage catalog go into the registry as `[[table]]` entries,
   then into the declarations, with their descriptions. Each also becomes editable by data files (a minor version
   of `kamidori-base`).
3. **Loaders.** Stage enemies, once the floor-entered global is named (§6.3).
4. **Modder data in full.** `DataFileWriter` and the snippets, the script index and `script_use`, unnamed arrays by
   shape.
5. **The reference bundle** and the script index in the release workflow. The site's milestone 3 builds on it.
6. **Script relations:** ADDITEM, then UNITECH once its globals are named.
7. **Languages:** Kamidori's English from the converted package (§6.4).

## 14. Decisions by the user

The user answered this design's questions on 2026-11-21:

1. **The split of declarations** (§5.1): references in the registry, kinds in `vm-map/entities-<game>.toml`.
   Accepted ("That's fine").
2. **The read point** for Kamidori's boot tables: `kamidori.boot.data_ready`, INIT2's exit, the tables as the game
   plays with them (§6.2). The alternative was `kamidori.boot.data_layered`, where data files apply.
3. **Kamidori's English text:** published, tentatively, and taken down if asked (§3.1, §6.4).
4. **Unit pictures:** the face, the bestiary sprite and the battle sprites (§5.4).
5. **The release asset** (§8.3): left to this design, which takes a fourth asset on each release.
6. **Information-screen text as data** (§3.2, §5.7). The review found that item and skill descriptions, glossary
   entries, character profiles and chapter objectives are shown by message scripts, not stored in table cells. All
   of them are published ("Those all sound fine to include"), through the scripts `text_scripts` names.
