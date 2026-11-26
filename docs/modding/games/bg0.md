# Modding Ikusa Megami ZERO

戦女神ZERO-specific facts for the generic guides. The identity block (game id, engine ABI, modules, tables, patch
points) is generated on [the games page](README.md#ikusa-megami-zero).

Script mods, text packages, Lua mods and data files work for this game (modding level M3). Its base mod, `bg0-base`,
names its cells for Lua, declares its events, gives a helper and applies its data files; the generated page says which
level it is at.

## Install and options

The install this guide was written against mounts one append catalog, `APPEND.AAI` (selector 1), and its scripts
are 1,251 base and 328 append records. A profile made on it in the launcher needs nothing else, so the guides'
commands take `--profile <profile>` alone. Without a profile, they take the game's options:

| Install | Options |
|---|---|
| 戦女神ZERO | `--game bg0 --game-root DIR` |

- **From source:** the conventional install is `games/bg0/` beside the repository, so `--game-root ../games/bg0`
  from `age-reimpl/`. The port is `.\run-godot.cmd -Game bg0`, with `-Mod PATH` for a mod. The driver is
  `py -3.11 -X utf8 tools/drive.py --game bg0 port start --mod PATH`. Its points for the game include `title.start`
  (Game Start on the title screen), `mode.normal` (通常 in the game-mode box that Game Start opens, which goes to the
  opening scene), `adv.next`, and the character menu's `menu.save` and `menu.load` (the driver's
  [戦女神ZERO points](../../../tools/drive/bg0.toml)). The title's entries are a column on the right half of the
  canvas, from x 400, and the middle of the canvas, `adv.next`, is on the edge of Option's row: do not click it on
  the title.
- Game Start asks for a game mode, 通常 or 特別編, before a new game starts.
- Native AGE shows Japanese dialogs, so `dialogs` and `answer` use Japanese labels. On a machine with the LAV Filters
  codec pack, native AGE waits for ever in the game's movies; the driver keeps LAV's DirectShow classes from it (the
  `refuse_class` entries of the driver's points file).
- The menu bar, the fullscreen strip, the 終了 dialog and 設定 are the port's own.
- `use bg0.globals` names the page's cells, the scene dispatch, the data tables' arrays and the saved cells the
  game reads them through (422 names; [reference](../../global-reference-bg0.md)). A cell without a name is written
  by its address (`G[0x2b80]`), as the decompiled scripts print it.

## First mod values

The values for [your first mod](../first-mod.md), on the opening scene:

| Step | Value |
|---|---|
| Page | The opening scene's first page (セリカ: `「よし、これでこっちの動きは封じた！」`). |
| Site | `0xdf3` |
| Fingerprint | `sha256:50d75547dd7ffcee34c3671133e67d39cd6ca031f03dd0cfc1258bddcac0929f` |

A page of this game starts at the instruction that resets the text layout, before the one that sets
`adv_page_has_text`. That instruction is the site to insert a page before:

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:50d75547dd7ffcee34c3671133e67d39cd6ca031f03dd0cfc1258bddcac0929f"
use bg0.adv

before site 0xdf3 {
    "[MOD] Hello from my first mod."
}
```

The finished mod is [example.bg0-prologue-line](../../../oms/examples/mods/example.bg0-prologue-line/). Its line is the
first page on both targets (native AGE on the exported `SC0000.BIN`, and the port with `--mod`), at `SC0000@0xe25`;
the game's own first page follows it, at `SC0000@0xefc`. The two targets' second pages are alike to the pixel, and
their first differ only in the turning wait icon's frame (the driver's
[prologue line](../../../tools/drive/scenarios/bg0-prologue-line.steps) scenario, 2026-11-25).

## Dialogue lines

The prelude's template is `line(text, speaker = none, voice = none)`:

```text
"[MOD] A narration line."
セリカ "「よし、これでこっちの動きは封じた！」" (voice: 0x2f)
カヤ "「やるわねセリカ！　さすがあたしの弟！」" (voice: 0x31)
```

- A **speaker** is a name of the prelude's `speaker` enum, the index into the name table that CNINIT writes, whose
  name the name plate shows. The enum names the 135 speakers the scenes set; a name that is not an identifier
  (`ルナ＝クリア`, `？？？？？`) or that two speakers share carries its id (`speaker_0xa0`), with the name in a comment.
  A line without a speaker is narration.
- `voice` is the voice clip, a resource id. The template mutes it as the game does, for a character whose voice the
  player turned off on the game's Option screen (CONFIG, which keeps the switches in `voice_mute_switches`). セリカ's lines all name `0x2f`, `SERIKA_A999.OGG`, a short clip of 4,558 bytes; the
  other characters' name their own numbered clips (`0x31` is `KAYA0001.OGG`). Copy a voice from a line of the
  decompiled scene (`OME-Tool oms-decompile --level idiomatic --script SC0000.BIN --profile <profile>`).

The scene library is an earlier form of Verita's: a page starts its voice and draws its name plate itself, which
Verita's scenes do through library functions. Pages that carry furigana, interpolate a name or span several
`show_text` calls stay core statements in a decompiled scene, as in the other games.

## Text and language packages

`omp text extract` writes a catalog for every script that draws text (152 files and about 33,000 entries for this
install). The game's text has no private-use characters, so a `source` can be typed as the page shows it. The names on
the name plates are strings CNINIT stores: an entry of `_global.toml` translates one wherever it is drawn
(`source = "セリカ"`, `text = "Serika"`).

A language package with three translated lines of the opening scene and the name セリカ was checked and run in the port
(2026-11-25, `--mod …` and the options panel's language `en`): the scene's first three pages read in English, and
セリカ's name plate too. Nothing larger has been tried on this game.

## Data edits

A data file ([data files](../data-files.md)) changes the game's record tables with no Lua and no script.
`bg0-base` accepts the kind `bg0.tables`. It applies each file's boot tables at `bg0.data.layered`, after the base
and the append have written them and before INIT2's own work reads them, and a place's part each time the place's
loader has run: the world map's areas at `bg0.world.loaded`, a town's sites at `bg0.town.loaded`, and a field map's
blocks at `bg0.map.loaded`. This one renames the name plate セリカ (speaker 1) shows; the comment gives the game's own
value:

```toml
# data/speakers.toml; the manifest names it with [[data]] kind = "bg0.tables", file = "speakers.toml"
[speaker_entry.1]
name = "[MOD] セリカ"   # セリカ
```

A fragment can read and write the same tables (`use bg0.globals`). The tables a file edits, by what they hold (the
generated page lists them with their fields):

| Kind | Tables |
|---|---|
| Characters and units | `unit`, `speaker_entry`, `lead_form`, `profile`, `voice_setting` |
| Items | `item`, `item_class`, `treasure` |
| Battle | `condition`, `skill_support`, `element`, `formation`, `routine`, `battle_animation`, `arena_round`, `arena_rank` |
| The world | `map`, `land`, `object_kind`, `field_sprite` |
| Registries | `scene`, `cg`, `glossary_entry`, `replay` |
| A place's records | `area` through the scope `world`, `town_site` through `town`, `map_block` through `dungeon` (below) |

```text
use bg0.globals

item[101].name = "[MOD] 治癒の水・小"
```

- **Tables and fields** are those of [the global reference](../../global-reference-bg0.md), "Tables", by the names in
  its `field` column. Every table's layout, and what each of its fields holds, is in
  [戦女神ZERO's data tables](../../bg0-data-tables.md).
- **Saved fields are refused:** every field of `character`, and `item.count`, `item.obtained`, `treasure.opened`,
  `area.state` and `town_site.state`. They lie below the saved prefix, so a load would bring the saved value back
  over the edit. Change them from Lua, or with the helper below.
- **Tables a file does not edit:** the ones the game fills for one screen or one battle when it opens it
  (`shop_slot`, `battle_effect`, `formation_slot`, `encounter_entry`). The kind has no event at those loaders; a
  fragment or a Lua hook at the loader's script reaches them.
- **The shape.** `bg0-base` ships the kind's shape, `shapes/tables.schema.json` ([data files](../data-files.md),
  "Shapes"), so the link, `omp check` and `omp build` refuse a table, field or column the reference does not name, a
  saved field, an id or a column out of range and a value of the wrong kind, with the file's line. `omp lua-stubs
  --game bg0` writes a copy and prints the `#:schema` line for an editor.
- **A place's records** are named through the place. `world.1.area.10.name` is area 10 of the first story part's world
  map (worlds 1 to 4, `story_part`; an area's id is its place id, so the same id may be in several worlds with other
  data). `town.50.site.2.name` is site 2 of town 50 (towns 50 to 99; site 1 of every town is its exit).
  `dungeon.1.block.50.music.0` is map 1's block 50's first music (maps 1 to 49; a block is `30 * row + column`, and a
  column of a row field is its index). AMINIT, SIINIT and RMINIT clear and fill them each time the game sets the place
  up, and the part is applied each of those times, after the loader: the world map at each opening of ALLMAP, and
  when a scene shows a world map (SHOWALLMAP); a town at each visit; a field map each time FIELD starts on it, and on
  a load of a save made on it or in a scene after it.
- **特別編.** In the special edition (`special_edition_mode`) the append's `$1$AMINIT`, `$1$SIINIT` and `$1$RMINIT`
  fill the places in place of the base's, and `$1$SCINIT` adds its scenes; the events come after either, so a part
  applies in both game modes. A fragment that changes those tables in both modes patches both scripts.
- **Saves.** No boot table is in a save: boot rebuilds every one at each start and each load, so a changed value is
  there on an old save too, where the game reads it from the table. What the game copies out of a table into saved
  state, so that a change does not reach an old one, is listed in [戦女神ZERO's data tables](../../bg0-data-tables.md)
  under each part's "What is saved": among it a joining character's base level, stats and starting equipment, セリカ's
  parameters at a chapter change, and a field map's start, copied on entry from the world map.
- A fragment's write to a table is not saved, and it lasts until the game builds the tables again, at its next start
  at the latest. A data file's edit is made again at each boot and each load, while its mod is enabled.
- **Run in the game:** the example's boot edit (the
  [bg0-speaker-name](../../../tools/drive/scenarios/port/bg0-speaker-name.steps) scenario) and a field map's part
  (`example.bg0-map-block`, in the [bg0-debug-events](../../../tools/drive/scenarios/port/bg0-debug-events.steps)
  scenario), in the port. The world map's and a town's parts are run by `ModLuaLinkTests` on the installed scripts,
  not yet in a played game. No edit was run on native AGE (data files are port-only).

## Patch points and events

The named points ([the patch point reference](../../patch-point-reference-bg0.md) gives each one's description and
fingerprint) sit after the loaders the append's layers run as well, so a fragment or a Lua hook there sees every
layer's work:

| Point | Script | Where |
|---|---|---|
| `bg0.boot.data_layered` | `INIT2.BIN` | after the base tables and the append's (op `0x143` runs `$1$AUTORUN`) |
| `bg0.boot.data_ready` | `INIT2.BIN` | INIT2's exit, after its own work on the tables |
| `bg0.world.after_area_loaders` | `ALLMAP.BIN` | after `AMINIT` (`$1$AMINIT` in 特別編), the world map's areas |
| `bg0.world.after_shown_area_loaders` | `SHOWALLMAP.BIN` | after `AMINIT`, for the world map a scene shows |
| `bg0.town.after_site_loaders` | `TOWN.BIN` | after `SIINIT` (`$1$SIINIT`), the town's sites |
| `bg0.map.after_block_loaders` | `FIELD.BIN` | after `RMINIT` (`$1$RMINIT`), the field map's blocks |
| `bg0.experience.before_add` | `ADDEXP.BIN` | one character's award, before it is added and the levels are worked out |

The award's amount is `script_arg_1` (`G[0x91865]`), which the caller sets before it calls `ADDEXP`, so a fragment at
the point reads and changes it; a Lua subscriber of `bg0.experience.award` reads and writes it as `e.amount`.

`bg0-base` declares six game events ([the Lua guide](../lua.md), "Game events"):

| Event | Fields |
|---|---|
| `bg0.data.layered`, `bg0.data.ready` | none |
| `bg0.world.loaded` | `world` (`story_part`); raised at each opening of ALLMAP, and in a scene that shows a world map |
| `bg0.town.loaded` | `town` (`current_place_id`); raised at each start of TOWN |
| `bg0.map.loaded` | `map` (`current_place_id`); raised at each start of FIELD |
| `bg0.experience.award` | `amount` (rw), `character` (`character_arg_id`), `level` (`character_levels[character_arg_id]`) |

`bg0.map.loaded` is not raised on a load of a save made in a scene after a field map: the game's load callback runs
the map's block loader again before `bg0.data.ready` (`CALLBACK_LOAD@0xbc`..`0xde`). A handler that should hold then
subscribes to `bg0.data.ready` as well, and checks `system_mode` (1, a scene) and `map_entered`, as the data files'
handler does.

Its helper changes the game through its own script: `give_item(item, count)` (count -999 to 999, 1 by default) runs
`ADDITEM`, as the game's own gifts do. `ADDITEM` keeps the count from 0 to 999 (what passes 999 is lost) and marks the
item obtained, in the save and in the shared profile. For an item of class 1 (a key item) it sets the item's story
flag (`item_key_held_flags`, `story_flags[1900 + item]`) to whether one is held, and a held key item from 13 to 20
teaches formation `item - 12`. The helper puts back the cells it used and returns the new count.

Every event has been raised in a run of the port: the boot events, the helper and the data file's boot edit by
`ModLuaLinkTests` and the scenarios above; the map event on a new game's first map (map 1), and the map event and the
experience award through the debug menu (below) by the
[bg0-debug-events](../../../tools/drive/scenarios/port/bg0-debug-events.steps) scenario, and a won battle's awards
from RESULT by the [bg0-battle-award](../../../tools/drive/scenarios/port/bg0-battle-award.steps) scenario. The world and town events were raised by
`ModLuaLinkTests` on the installed ALLMAP, SHOWALLMAP and TOWN, not yet in a played game.

## The debug menu

The title builds the developers' デバッグメニュー right after its Quit, whose `throw-exit-request` (`TITLE@0x3de`) ends
the game, so no retail run reaches it. [example.bg0-debug-menu](../../../oms/examples/mods/example.bg0-debug-menu/)
deletes that instruction, so Quit opens the menu. Each entry that starts a mode sets it (`system_mode`) and leaves the
title through its common exit, as Game Start does, so the mode takes effect. It targets `native`: `omp export` gives a
`TITLE.BIN` that opens the same menu on native AGE.

| Entry | Point | What it runs |
|---|---|---|
| ＡＤＶパート, ＡＤＶパート（回想登録） | | a scene picker (`SC????.BIN`, `SP????.BIN`), AGERC's dialog on native AGE |
| ＡＤＶデバッグ, ＡＤＶデバッグ（ＰＧ１用） | | the scenes `DEBUGADV`, `DEBUGADV2` |
| 迷宮マップ | | `DEBUGMAP`: story part 4, then FIELD on map 23 |
| 迷宮マップ（ＰＧ１用） | 370 383 | `DEBUGMAP2`: it joins the party, gives character 53 999,999 experience (`DEBUGMAP2@0x8f`, through `ADDEXP`) and starts FIELD on map 1, at row 1, column 20 |
| 迷宮マップ（ＰＧ２用） | | `DEBUGMAP3`, then a field map |
| 戦闘 | 290 443 | `DEBUGBTL`: a party of sixteen, 999,999 experience each for characters 53 to 57 (through `ADDEXP`), then battle formation 989, six ボズア; the battle ring's AUTO (175 312) wins it |
| 戦闘アニメ | | `DEBUGANIME`, in the battle mode |
| ショップ | | `DEBUGMAP2`'s party and 1,000 money, then scene 2000 |
| キャンセル | | back to the title |

- Run on both targets: the menu (alike) and 迷宮マップ（ＰＧ１用）, to the same field map. In the port, the
  [bg0-debug-events](../../../tools/drive/scenarios/port/bg0-debug-events.steps) scenario plays 迷宮マップ（ＰＧ１用）
  with the map part and the event log, and the [bg0-battle-award](../../../tools/drive/scenarios/port/bg0-battle-award.steps) scenario plays 戦闘 with the event log to
  RESULT's awards.
- The scene pickers need AGERC's dialog, which the port does not have: there op `0x140` picks nothing, and the title
  goes on with the scene id the shared profile kept ([the backlog](../../backlog.md), "Sweep for ops that are still
  stubbed").

## Catalogs, scenes and saves

- **Selectors.** The install ships one append catalog (`APPEND.AAI`, selector 1), so a first mod's catalog normally
  gets selector 2.
- **Untested.** No 戦女神ZERO mod that adds a scene or a script has been tested, on either target. Start from one of
  the game's own append scenes, and test saving on native AGE as well as in the port.

## Examples

| Path | What it shows |
|---|---|
| [example.bg0-prologue-line](../../../oms/examples/mods/example.bg0-prologue-line/) | a line inserted before the opening scene's first page |
| [example.bg0-speaker-name](../../../oms/examples/mods/example.bg0-speaker-name/) | a data file that renames セリカ's name plate at `bg0.data.layered`; the [bg0-speaker-name](../../../tools/drive/scenarios/port/bg0-speaker-name.steps) scenario plays a new game to his first line with it |
| [example.bg0-event-log](../../../oms/examples/mods/example.bg0-event-log/) | a Lua mod that logs every event of `bg0-base` with its fields, and gives one 治癒の水・小 at each `bg0.data.ready` through `give_item` |
| [example.bg0-map-block](../../../oms/examples/mods/example.bg0-map-block/) | a data file with a field map's part: map 1's block 50 takes music 14 for its own 5, at `bg0.map.loaded` |
| [example.bg0-debug-menu](../../../oms/examples/mods/example.bg0-debug-menu/) | a script mod that opens the title's debug menu from Quit; the [bg0-debug-events](../../../tools/drive/scenarios/port/bg0-debug-events.steps) scenario plays 迷宮マップ（ＰＧ１用） with the two mods above, and the [bg0-battle-award](../../../tools/drive/scenarios/port/bg0-battle-award.steps) scenario 戦闘 with the event log |
