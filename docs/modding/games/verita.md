# Modding Ikusa Megami VERITA

Verita-specific facts for the generic guides. The identity block (game id, engine ABI, modules, tables,
patch points) is generated on [the games page](README.md#ikusa-megami-verita).

Script mods, text packages, Lua mods and data files work for this game (modding level M3). Its base mod,
`verita-base`, names its cells for Lua, declares its events and applies its data files; the generated page says
which level it is at.

## Install and options

The install this guide was written against mounts three append catalogs (`APPEND01`–`03`) and carries 19 loose
append scripts beside them (`$1$…` and `$2$…`). A profile made on it in the launcher needs nothing else, so the
guides' commands take `--profile <profile>` alone. Without a profile, they take the game's options:

| Install | Options |
|---|---|
| 戦女神VERITA | `--game verita --game-root DIR` |

- **From source:** the conventional install is `games/verita/` beside the repository, so
  `--game-root ../games/verita` from `age-reimpl/`. The port is `.\run-godot.cmd -Game verita`, with `-Mod PATH`
  for a mod. The driver is `py -3.11 -X utf8 tools/drive.py --game verita port start --mod PATH`. Its points for the
  game include `title.start` (Game Start on the title screen, which goes straight to the opening scene), `adv.next`,
  and the character menu's `menu.save`, `menu.load` and `menu.close` (the driver's
  [Verita points](../../../tools/drive/verita.toml)). The title's entries are a column on the left, so the middle of
  the canvas, `adv.next`, is none of them.
- Native AGE shows Japanese dialogs, so `dialogs` and `answer` use Japanese labels.
- The menu bar, the fullscreen strip, the 終了 dialog and 設定 are the port's own.
- `use verita.globals` names the page's cells, the scene dispatch, the data tables' arrays and the saved cells
  the game reads them through (7,573 names; [reference](../../global-reference-verita.md)). A cell without a
  name is written by its address (`G[0x3633]`), as the decompiled scripts print it.

## First mod values

The values for [your first mod](../first-mod.md), on the opening scene:

| Step | Value |
|---|---|
| Page | The opening scene's first page (`フレスラント王国の国土の大半を占める広大なマータ砂漠。`). |
| Site | `0xa14` |
| Fingerprint | `sha256:18acc4028e92fb49b1f3dfe63d35378fa5ef583caa405fcea840cd90e9798633` |

A page of this game starts at the instruction that resets the text layout, before the one that sets `G[0x348a]`
(the page has text). That instruction is the site to insert a page before:

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:18acc4028e92fb49b1f3dfe63d35378fa5ef583caa405fcea840cd90e9798633"
use verita.adv

before site 0xa14 {
    "[MOD] Hello from my first mod."
}
```

The finished mod is [example.verita-prologue-line](../../../oms/examples/mods/example.verita-prologue-line/). Its
line is the first page on both targets (native AGE on the exported `SC0000.BIN`, and the port with `--mod`), at
`SC0000@0xa2d`, pixel for pixel alike; the game's own first page follows it, at `SC0000@0xa49`.

## Dialogue lines

The prelude's template is `line(text, speaker = none, voice = none)`:

```text
"[MOD] A narration line."
セリカ "「……」" (voice: 0x3e)
エクリア "「やっと……叶えて、くれる……？」" (voice: 0x64)
```

- A **speaker** is a name of the prelude's `speaker` enum, the index into the name table (`speaker_entry`, CNINIT's
  names), whose name the name plate shows. The enum names the 157 speakers the scenes set; a name that is not an
  identifier (`？？？？`) or that two speakers share carries its id. A line without a speaker is narration.
- `voice` is the voice clip.

Pages that carry furigana, interpolate a name or span several `show_text` calls stay core statements in a decompiled
scene, as in the other games.

## Text and language packages

`omp text extract` writes a catalog for every script that draws text (294 files and about 42,000 entries for this
install). The game's text has no private-use characters, so a `source` can be typed as the page shows it. The
names on the name plates are table strings, drawn from `speaker_entry`: an entry of `_global.toml` translates one
wherever it is drawn (`source = "セリカ"`, `text = "Serika"`).

A language package with three translated lines of the opening scene and the name セリカ was checked and run in the
port (2026-11-25, `--mod … --language en`): the first page and セリカ's first line read in English, her name plate
too. Nothing larger has been tried on this game.

## Data edits

A data file ([data files](../data-files.md)) changes the game's record tables with no Lua and no script.
`verita-base` accepts the kind `verita.tables`. It applies each file's boot tables at `verita.data.layered`, after
every layer has written them and before INIT2's own work reads them, and a dungeon map's part each time the map's
loaders have run, at `verita.map.loaded`. This one renames the name plate セリカ (speaker 1) shows; the comment
gives the game's own value:

```toml
# data/speakers.toml; the manifest names it with [[data]] kind = "verita.tables", file = "speakers.toml"
[speaker_entry.1]
name = "[MOD] セリカ"   # セリカ
```

A fragment can read and write the same tables (`use verita.globals`). The tables a file edits, by what they hold
(the generated page lists them with their fields):

| Kind | Tables |
|---|---|
| Characters and units | `character`, `unit`, `speaker_entry`, `lead_growth`, `profile`, `voice_setting` |
| Items and crafting | `item`, `item_class`, `item_growth`, `recipe`, `treasure` |
| Battle | `skill`, `condition`, `element`, `action_tree`, `school_tree`, `tactic`, `formation`, `troop`, `commander`, `battle_animation`, `arena_match`, `rank`, `title` |
| The world | `area`, `land`, `map`, `map_routine`, `routine`, `terrain`, `object_kind`, `objective` |
| Fishing | `fish`, `fishing_bait`, `fishing_rod`, `fishing_spot` |
| Registries | `scene`, `cg`, `music_track`, `glossary_entry`, `replay` |
| A dungeon map's blocks | `map_block`, through the scope `dungeon` (below) |

```text
use verita.globals

item[101].name = "[MOD] 治癒の水"
```

- **Tables and fields** are those of [the global reference](../../global-reference-verita.md), "Tables", by the names
  in its `field` column.
- **Saved fields are refused:** `item.count`, `item.obtained`, `item.growth_exp`, `item.growth_level`, `title.earned`
  and `treasure.opened`. They lie below the saved prefix, so a load would bring the saved value back over the edit.
  Change them from Lua, or with the helper below.
- **Tables a file does not edit:** the ones the game fills for one screen or one battle when it opens it
  (`shop_slot`, `battle_effect`, `formation_slot`, `scripted_party_slot`, `encounter_entry`, `town_site`, `war_unit`,
  `puzzle_block`, `quest`). The kind has no event at those loaders yet; a fragment or a Lua hook at the loader's
  script reaches them.
- **The shape.** `verita-base` ships the kind's shape, `shapes/tables.schema.json`
  ([data files](../data-files.md), "Shapes"), so the link, `omp check` and `omp build` refuse a table, field
  or column the reference does not name, a saved field, an id or a column out of range and a value of the wrong
  kind, with the file's line. `omp lua-stubs --game verita` writes a copy and prints the `#:schema` line for an
  editor.
- **A dungeon map's blocks** are `map_block`, which a file names through the map: `dungeon.3.block.31.music` is map
  3's block 31's music (map ids 1 to 49, the game's are 1 to 47; a block is `30 * row + column`). RMINIT, the
  base's and each append's, clears and fills them each time the game sets the map up: when FIELD starts on it (which
  it does again at each change of screen on the map) and when a save made on a map is loaded. The part is applied
  each of those times, after the loaders.
- Every table's layout, and what each of its fields holds, is in [Verita's data tables](../../verita-data-tables.md);
  a few numbers are read only as far as the script that uses them, and that document's last section lists them.
- **Saves.** No boot table is in a save: boot rebuilds every one at each start and each load, so a changed value is
  there on an old save too, where the game reads it from the table. What the game copies out of a table into a
  save, so that a change does not reach an old one, is listed in [Verita's data tables](../../verita-data-tables.md),
  "Saved state": among it a joining character's equipment, money, drop items, base level and starting title, a
  character's stats at each level up, and a map block's music when the map starts.
- A fragment's write to a table is not saved, and it lasts until the game builds the tables again, at its next
  start at the latest. A data file's edit is made again at each boot and each load, while its mod is enabled.
- **Run in the game:** the example's boot edit (the
  [verita-speaker-name](../../../tools/drive/scenarios/port/verita-speaker-name.steps) scenario) and a dungeon map's
  part (`example.verita-map-block`, in the
  [verita-debug-events](../../../tools/drive/scenarios/port/verita-debug-events.steps) scenario), in the port. No
  edit was run on native AGE (data files are port-only).

## Patch points and events

The named points ([the patch point reference](../../patch-point-reference-verita.md) gives each one's description and
fingerprint) sit after the chains the appends hook, so a fragment or a Lua hook there sees every layer's work:

| Point | Script | Where |
|---|---|---|
| `verita.boot.data_layered` | `INIT2.BIN` | after every layer's data tables (op `0x143` runs the appends') |
| `verita.boot.data_ready` | `INIT2.BIN` | INIT2's exit, after its own work on the tables |
| `verita.map.after_block_loaders` | `FIELD.BIN` | after every layer's `RMINIT`, the dungeon map's blocks |
| `verita.experience.before_add` | `ADDEXP.BIN` | one character's award, before it is added and the levels are worked out |

The award's amount is `shared_number` (`G[0x269b2c]`), which the caller sets before it calls `ADDEXP`, so a
fragment at the point reads and changes it; a Lua subscriber of `verita.experience.award` reads and writes it as
`e.amount`.

`verita-base` declares four game events ([the Lua guide](../lua.md), "Game events"):

| Event | Fields |
|---|---|
| `verita.data.layered`, `verita.data.ready` | none |
| `verita.map.loaded` | `map` (`current_place_id`); raised at each start of FIELD, so again at each change of screen on a map |
| `verita.experience.award` | `amount` (rw), `character` (`character_arg_id`), `level` (`character_levels[character_arg_id]`) |

Its helper changes the game through its own script: `give_item(item, count)` (count -999 to 999, 1 by default) runs
`ADDITEM`, as the game's own gifts do. `ADDITEM` keeps the count from 0 to 999 (what passes 999 is lost), marks the item
obtained and records it in the shared collection, and for an item of class 1 sets its story flag
(`story_flags[0x2328 + item]`) to whether one is held. The helper puts back the cells it used and returns the new
count.

Every event has been raised in a run of the port: the boot events, the helper and the data file's boot edit by
`ModLuaLinkTests` and the scenarios above, the map event on a new game's first map (map 1), and the map event (map 11)
and the experience award through the debug menu (below) by the
[verita-debug-events](../../../tools/drive/scenarios/port/verita-debug-events.steps) scenario. A battle's own award
was not reached yet.

Other chains the appends hook have no event: the scene dispatch (`SCJUMP`'s chain), the town's, war stage's and box
puzzle's loaders, the shop and the battle's tables ([Verita's data tables](../../verita-data-tables.md), "Hook
chains").

## The debug menu

The title builds the developers' デバッグメニュー right after its Quit, whose `throw-exit-request` (`TITLE@0x34d`) ends the
game, so no retail run reaches it. [example.verita-debug-menu](../../../oms/examples/mods/example.verita-debug-menu/)
deletes that instruction, so Quit opens the menu. Each entry sets the game mode (`G[0x0]`) and leaves the title
through its common exit, as Game Start does, so the mode takes effect. It targets `native`: `omp export` gives a
`TITLE.BIN` that opens the same menu on native AGE.

| Entry | Point | What it runs |
|---|---|---|
| ＡＤＶパート, Ｈシーン | | a scene picker (`SC????.BIN`, `SP????.BIN`) |
| ＡＤＶデバッグ１, ２ | | the scenes `DEBUGADV1`, `DEBUGADV2` |
| ＡＤＶデバッグ（ＰＧ用） | 505 180 | the scene `DEBUGADV`, which gives character 6 999,999 experience (`DEBUGADV@0xc43`, through `ADDEXP`) and then opens a shop |
| 迷宮マップ１ | 445 210 | `DEBUGMAP1`, then a dungeon map: FIELD on map 11, at row 1, column 1 |
| 迷宮マップ２, ３, （ＰＧ用） | | `DEBUGMAP2`, `DEBUGMAP3`, `DEBUGMAP`, then a dungeon map |
| 全体マップ | | `DEBUGAM`, then the whole map |
| 戦闘 | 420 359 | `DEBUGBTL`, then a battle |
| 戦闘アニメ（味方）, （敵） | | `DEBUGANIME` |
| ＳＬＧパート | | `DEBUGSLG`, then the SLG part |
| ミニゲーム | | `DEBUGEXTRA` |

- Run on both targets: the menu (pixel for pixel alike) and the start of ＡＤＶデバッグ（ＰＧ用）. In the port, the
  [verita-debug-events](../../../tools/drive/scenarios/port/verita-debug-events.steps) scenario plays 迷宮マップ１ and
  ＡＤＶデバッグ（ＰＧ用） with the map part and the event log.
- Between two entries the scenario goes back to the title through the 終了 dialog's タイトルに戻る. That starts a new
  session, so a Lua mod's state starts over with it.

## Catalogs, scenes and saves

- **Selectors.** The install ships three append catalogs (`APPEND01`–`03`), so a first mod's catalog normally gets
  selector 4.
- **Record 0 runs at boot.** Op `0x143` runs it in `INIT2`, after the install's own appends have run their
  `$n$AUTORUN` and before INIT2's own work on the tables, so it sees every layer's tables and can edit any of them.
  INIT2 runs again on each load, before the save is restored, so an edit of a table above the saved prefix holds
  after a load.

## Examples

| Path | What it shows |
|---|---|
| [example.verita-prologue-line](../../../oms/examples/mods/example.verita-prologue-line/) | a line inserted before the opening scene's first page |
| [example.verita-speaker-name](../../../oms/examples/mods/example.verita-speaker-name/) | a data file that renames セリカ's name plate at `verita.data.layered`; the [verita-speaker-name](../../../tools/drive/scenarios/port/verita-speaker-name.steps) scenario plays a new game to her first line with it |
| [example.verita-event-log](../../../oms/examples/mods/example.verita-event-log/) | a Lua mod that logs every event of `verita-base` with its fields, and gives one 治癒の水・小 at each `verita.data.ready` through `give_item` |
| [example.verita-map-block](../../../oms/examples/mods/example.verita-map-block/) | a data file with a dungeon map's part: map 11's block 31 takes its second music as its first, at `verita.map.loaded` |
| [example.verita-debug-menu](../../../oms/examples/mods/example.verita-debug-menu/) | a script mod that opens the title's debug menu from Quit; the [verita-debug-events](../../../tools/drive/scenarios/port/verita-debug-events.steps) scenario plays two of its entries with the two mods above |
