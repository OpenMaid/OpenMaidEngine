# Modding Madou Koukaku

魔導巧殻-specific facts for the generic guides. The identity block (game id, engine ABI, modules, tables, patch
points) is generated on [the games page](README.md#madou-koukaku).

Script mods, text packages, data files and Lua mods work for this game (modding level M3), through its base mod,
`koukaku-base`: it names the game's cells, declares its events, gives helpers and accepts the data kind
`koukaku.tables`. The generated page says which level the game is at.

## Install and options

The install this guide was written against mounts one append catalog, `APPEND01.AAI` (selector 1), and its title
screen shows `Version 1.03.0010` and `AP01 北方の超剛力軍DISC 追加`; its scripts are 401 base and 52 append records. A
profile made on it in the launcher needs nothing else, so the guides' commands take `--profile <profile>` alone.
Without a profile, they take the game's options:

| Install | Options |
|---|---|
| 魔導巧殻 | `--game koukaku --game-root DIR` |

- **From source:** the conventional install is `games/koukaku/` beside the repository, so `--game-root ../games/koukaku`
  from `age-reimpl/`. The port is `.\run-godot.cmd -Game koukaku`, with `-Mod PATH` for a mod. The driver is
  `py -3.11 -X utf8 tools/drive.py --game koukaku port start --mod PATH`. Its points for the game include
  `title.start` (開始 on the title screen), `start.begin` (ゲーム開始 on the 初期設定 page that 開始 opens), `adv.next`,
  and the right-click menu's `menu.save` and `menu.load` (the driver's [魔導巧殻 points](../../../tools/drive/koukaku.toml)).
- A new game passes the 初期設定 page (difficulty and the game's modes), then plays the prologue movie, about a minute
  and a half, before the opening scene.
- Native AGE shows Japanese dialogs, so `dialogs` and `answer` use Japanese labels. コメント編集 on a saved slot opens
  the game's own comment dialog on native AGE and the port's on the port; both keep a comment to 564 pixels at 15 for a
  full-width character and 8 for a half-width one.
- The menu bar, the fullscreen strip, the 終了 dialog and 設定 are the port's own.
- `use koukaku.globals` names the cells by [the global reference](../../global-reference-koukaku.md); a cell it does
  not name is written by its address (`G[0x37d0]`), as the decompiled scripts print it.

## First mod values

The values for [your first mod](../first-mod.md), on the opening scene:

| Step | Value |
|---|---|
| Page | The opening scene's first page (`一面を覆い尽くす、焼け焦げた木々や兵……。`). |
| Site | `0xecd` |
| Fingerprint | `sha256:ea1a8a1337210691675abdc219f4d83c2a5eaac4920c376278a52b64ac6f1bff` |

A page of this game starts at the instruction that turns the tick presentation off, before it resets the text
layouts and sets `G[0x37d1]` (the page has text). That instruction is the site to insert a page before:

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:ea1a8a1337210691675abdc219f4d83c2a5eaac4920c376278a52b64ac6f1bff"
use koukaku.adv

before site 0xecd {
    "[MOD] Hello from my first mod."
}
```

The finished mod is [example.koukaku-prologue-line](../../../oms/examples/mods/example.koukaku-prologue-line/). Its
line is the first page on both targets (native AGE on the exported `SC0000.BIN`, and the port with `--mod`), at
`SC0000@0xef2`; the game's own first page follows it, at `SC0000@0xf1a`. The two targets' shots of both pages are the
same pixels (the driver's [prologue line](../../../tools/drive/scenarios/koukaku-prologue-line.steps) scenario,
2026-11-26).

## Dialogue lines

The prelude's template is `line(text, speaker = none, face = none, voice = none, form = 0x0)`:

```text
"[MOD] A narration line."
ヴァイスハイト "「リセル。お前の力あてにさせてもらうぞ」"
リセル "「微力ながら、副官として支えさせていただきます」" (voice: 0x60)
エルミナ "「奇襲っ！？」" (face: 0x85)
```

- A **speaker** is a name of the prelude's `speaker` enum, the index into the names CHINIT writes: a row of six name
  forms for each character. The enum names the 59 speakers the scenes set; a name that is not an identifier
  (`リ・アネス`) carries its id (`speaker_0x10`), with the name in a comment. A line without a speaker is narration.
- `form` picks the name form the name plate shows, 0 (the default) to 5: ヴァイスハイト's form 1 is
  `戦場を眺める若き将軍`. Any other value shows `？？？？`; the scenes' few pages in form -1 stay core statements.
- `face` is the face picture shown by the message window, a picture's resource id; a line without one shows none.
  Copy a face from a line of the character you want, in the decompiled scene (`OME-Tool oms-decompile --level
  idiomatic --script SC0000.BIN --profile <profile>`).
- `voice` is the voice clip. The template mutes it as the game does, for a character whose voice the player turned
  off. Narration can carry one too.

The scene library is 創刻のアテリアル's, and its subs take that game's names. Most of this game's pages span two
`show_text` calls; those, and pages that carry furigana or interpolate a name, stay core statements in a decompiled
scene, as in the other games. The prelude lifts the pages of one call, about 11,600 of the scenes' 30,300.

## Text and language packages

`omp text extract` writes a catalog for every script that draws text (194 files and about 39,500 entries for this
install). The game's text uses private-use characters (`U+E000` to `U+E003`), which it draws with its own font,
`AGE Extend`: the first page's source is `一面を覆い尽くす、焼け焦げた木々や兵`, two `U+E000` and `。`, and its page
shows them as `……`. They do not show in most editors, so copy a `source` from the extracted catalog instead of typing
it; `omp text check` warns of an entry that matches nothing. A translation leaves them out or keeps them as it likes.
The names on the name plates are strings CHINIT stores: an entry of `_global.toml` translates one wherever it is drawn
(`source = "ヴァイスハイト"`, `text = "Weisheit"`).

A language package with three translated lines of the opening scene, a line of its twelfth page and the names
ヴァイスハイト and リセル was checked and run in the port (2026-11-26, `--mod …` and the options panel's language
`en`): the scene's first three pages read in English, and the twelfth too, under ヴァイスハイト's translated name.
Nothing larger has been tried on this game.

## Data edits

A data file ([data files](../data-files.md)) changes the game's record tables with no Lua and no script.
`koukaku-base` accepts the kind `koukaku.tables`. It applies each file's boot tables at `koukaku.data.layered`, after the
base and the append have written them and before INIT2's own work reads them, and a battlefield map's part each time a
script has loaded the map (`koukaku.battlefield.loaded`). This one renames the name plate ヴァイスハイト shows before
the opening scene names him, his name form 1; the comment gives the game's own value:

```toml
# data/names.toml; the manifest names it with [[data]] kind = "koukaku.tables", file = "names.toml"
[character.1.name_forms]
1 = "[MOD] 戦場を眺める若き将軍"   # 戦場を眺める若き将軍
```

A fragment can read and write the same tables (`use koukaku.globals`). The tables a file edits, by what they hold (the
generated page lists them with their fields):

| Kind | Tables |
|---|---|
| Characters | `character`, `title`, `voice_setting`, `profile`, `costume` |
| Troops and battle | `troop`, `skill`, `passive_skill`, `attack`, `formation`, `animation`, `animation_sequence`, `skill_area` |
| Items and facilities | `item`, `part`, `alchemy_recipe`, `breed_combination`, `brothel_girl` |
| The strategic map | `nation`, `region`, `route`, `production_set`, `starting_unit`, `ruse`, `quest` |
| Battlefields | `land`, `map_object`, `map_object_kind`; `battlefield_cell` and `battlefield_entry` through the scope `battlefield` (below) |
| Registries and screens | `scene`, `cg`, `replay`, `music`, `help_line`, `tutorial`, `tip`, `glossary` |

```text
use koukaku.globals

item[101].name = "[MOD] 転移の石"
```

- **Tables and fields** are those of [the global reference](../../global-reference-koukaku.md), "Tables", by the names
  in its `field` column. Every table's layout, and what each of its fields holds, is in
  [魔導巧殻's data tables](../../koukaku-data-tables.md). A field of several cells without names takes its columns by
  number: `character.1.name_forms.1` is name form 1.
- **Saved fields are refused:** `scene.played`, `scene.seen`, `scene.turns_since_played`, `tip.read`, `glossary.read`,
  `tutorial.seen` and `tutorial.shown`. They lie below the saved prefix, so a load would bring the saved value back
  over the edit. Change them from Lua.
- **Tables a file does not edit:** the ones the game fills for one screen or one battle when it opens it, DUINIT's
  battle setups and SHINIT's offers. The kind has no event at those loaders; a fragment or a Lua hook at the loader's
  script reaches them.
- **The shape.** `koukaku-base` ships the kind's shape, `shapes/tables.schema.json` ([data files](../data-files.md),
  "Shapes"), so the link, `omp check` and `omp build` refuse a table, field or column the reference does not name, a
  saved field, an id or a column out of range and a value of the wrong kind, with the file's line. `omp lua-stubs
  --game koukaku` writes a copy and prints the `#:schema` line for an editor.
- **A battlefield map's cells** are named through the map. `battlefield.98.cell.21.land` is map 98's land at row 1,
  column 1 (a cell is `20 * row + column`; maps 1 to 221, MPINIT's ids being 1 to 100 and 201 to 221), and
  `battlefield.98.entry.1` one of its deploy entries. MPINIT writes the map in play and clears nothing, and six scripts
  load a map: FIELD for a battle, REIGN for the region map in play, PALACE for the palace's map (1), and CHECKREG,
  OCCUPY and DELCN for a region's first setup. The part is applied after each of them, so it is in the cells while
  its map is the one loaded.
- **Saves.** No boot table is in a save: boot rebuilds every one at each start and each load, so a changed value is
  there on an old save too, where the game reads it from the table. What the game copies out of a table into saved
  state, so that a change does not reach it, is listed in [魔導巧殻's data tables](../../koukaku-data-tables.md) under
  each part's "What is saved": among it a character's base level, stats and costume at the first join, a costume's
  starting parts and stats the first time ADDPART adds it, a nation's starting capital, leader, funds and items when
  it is added, and a region's preset objects, which SETFORT copies into the region's saved layers at its first setup.
  A battle's land grid is rebuilt from the map on each field (SETFIELD), and a field resumed from a save keeps its
  grids.
- A fragment's write to a table is not saved, and it lasts until the game builds the tables again, at its next start
  at the latest. A data file's edit is made again at each boot and each load, while its mod is enabled.
- **Run in the game:** the example's boot edit (the
  [koukaku-speaker-name](../../../tools/drive/scenarios/port/koukaku-speaker-name.steps) scenario) and a battlefield
  map's part (`example.koukaku-battlefield-land`, in the
  [koukaku-debug-events](../../../tools/drive/scenarios/port/koukaku-debug-events.steps) scenario), in the port. No
  edit was run on native AGE (data files are port-only).

## Patch points and events

The named points ([the patch point reference](../../patch-point-reference-koukaku.md) gives each one's description and
fingerprint) sit after the loaders the append's layers run as well, so a fragment or a Lua hook there sees every
layer's work:

| Point | Script | Where |
|---|---|---|
| `koukaku.boot.data_layered` | `INIT2.BIN` | after the base tables and the append's (op `0x143` runs `$1$AUTORUN`) |
| `koukaku.boot.data_ready` | `INIT2.BIN` | INIT2's exit, after its own work on the tables |
| `koukaku.battlefield.after_map_loaders_field` | `FIELD.BIN` | after the MPINIT chain, for a battle's map |
| `koukaku.battlefield.after_map_loaders_reign` | `REIGN.BIN` | after the MPINIT chain, for the region map in play |
| `koukaku.battlefield.after_map_loaders_palace` | `PALACE.BIN` | after the MPINIT chain, for the palace's map |
| `koukaku.battlefield.after_map_loaders_checkreg` | `CHECKREG.BIN` | after the MPINIT chain, at a region's first setup |
| `koukaku.battlefield.after_map_loaders_occupy` | `OCCUPY.BIN` | after the MPINIT chain, at a captured region's first setup |
| `koukaku.battlefield.after_map_loaders_delcn` | `DELCN.BIN` | after the MPINIT chain, at a region's first setup as a nation goes |
| `koukaku.turn.before_week_advance` | `ALLMAP.BIN` | the end of a week, before `turn_week` is raised |
| `koukaku.experience.before_add` | `RESULT.BIN` | one commander's experience from a battle, before it is added and the levels are worked out |

The award's amount is the add's third operand, `arg[3]`: a pointer to the commander's experience from the battle,
`G[0xc0cd + character]`, so a fragment at the point reads and changes it through the operand; a Lua subscriber of
`koukaku.experience.award` reads and writes it as `e.amount`.

`koukaku-base` declares five game events ([the Lua guide](../lua.md), "Game events"):

| Event | Fields |
|---|---|
| `koukaku.data.layered`, `koukaku.data.ready` | none |
| `koukaku.battlefield.loaded` | `map` (`battlefield_map_id`); raised by each of the six scripts above |
| `koukaku.turn.week_ends` | `week` (`turn_week`, the week ending), `player` (`nation_player`) |
| `koukaku.experience.award` | `amount` (rw), `character` (RESULT's `L[0x275]`), `level` (`character_levels[character]`) |

Its helpers change the game through its own scripts, as the game's own gifts do, and put back the cells they used:

- `give_item(item, count, nation)` (count -999 to 999, 1 by default; the player's nation, `nation_player`, by
  default) runs `ADDITEM`. It keeps the count from going below 0 and sells what goes above 999 at the item's selling
  price into the nation's money. For the player's nation it marks the item obtained and seen (in the save and in the
  shared profile), and a key item (kind 1) sets its story flag, `9001 + item`. It returns the new count.
- `give_part(part, count)` runs `ADDPART`: the count goes up to 999, and the part is marked obtained and seen. A costume
  is added once, whatever the count: the first time, it takes its starting parts, level and stats from COINIT. It
  returns the new count.

Every event has been raised in a run of the port: the boot events, the helpers and the data file's parts by
`ModLuaLinkTests`; the boot events, the battlefield event (100 regions' maps at the game's start, through CHECKREG) and
the week's end through the debug menu (below) by the
[koukaku-debug-events](../../../tools/drive/scenarios/port/koukaku-debug-events.steps) scenario, and FIELD's
battlefield event and a battle's awards from RESULT by the
[koukaku-battle-award](../../../tools/drive/scenarios/port/koukaku-battle-award.steps) scenario. REIGN's, PALACE's,
OCCUPY's and DELCN's loads bind in the tests, and PALACE's runs there; they were not yet raised in a played game.

## The debug menu

The title builds the developers' デバッグメニュー right after its Quit calls QUIT, which ends the game (`TITLE@0x360`),
so no retail run reaches it. [example.koukaku-debug-menu](../../../oms/examples/mods/example.koukaku-debug-menu/) deletes
that call, so Quit opens the menu. Each entry that starts a mode sets it (`system_mode`) and leaves the title through
its common exit, as Game Start does, so the mode takes effect. It targets `native`: `omp export` gives a `TITLE.BIN`
that opens the same menu on native AGE.

| Entry | Point | What it runs |
|---|---|---|
| ＡＤＶパート | 640 265 | a scene picker (`SC????.BIN`), AGERC's dialog on native AGE |
| ＡＤＶデバッグ（ＰＧ用）, ＡＤＶデバッグ２ | 640 335, 640 405 | two debug scenes |
| 戦闘 | 640 475 | `DEBUGMAP`: 100 of every item and one of every part, corps for nations 1 and 5, then a battle on map 999 |
| 全体マップ | 640 545 | `DEBUGAM`: `INITGAME` (each region's first setup, which loads its map), every open region another nation holds taken by the player's (`OCCUPY`), items, parts and corps, then the strategic map |
| システム | 640 615 | `DEBUG` |
| キャンセル | 640 685 | back to the title |

- Run on both targets: the menu (alike, the title's falling leaves aside) and 戦闘. In the port, the
  [koukaku-debug-events](../../../tools/drive/scenarios/port/koukaku-debug-events.steps) scenario plays 全体マップ to the
  end of the first week with the battlefield part and the event log, and the
  [koukaku-battle-award](../../../tools/drive/scenarios/port/koukaku-battle-award.steps) scenario plays 戦闘 with the
  event log to RESULT's awards.
- 戦闘's map 999 is not one of MPINIT's, so the battle has no field: it ends at once in 戦闘敗北, and each commander
  gets 2 experience. Both targets first stop at READFIELD with 「画像ファイル SYSTEM4.BIN の読み込みに失敗しました」: the
  map's picture is 0, a record that is no picture. After 無視 both take the same route to the same RESULT screen.
- The scene picker needs AGERC's dialog, which the port does not have: there op `0x140` picks nothing.

## Catalogs, scenes and saves

- **Selectors.** The install ships one append catalog (`APPEND01.AAI`, selector 1), so a first mod's catalog normally
  gets selector 2.
- **Untested.** No 魔導巧殻 mod that adds a scene or a script has been tested, on either target. Start from one of the
  game's own append scenes, and test saving on native AGE as well as in the port.

## Examples

| Path | What it shows |
|---|---|
| [example.koukaku-prologue-line](../../../oms/examples/mods/example.koukaku-prologue-line/) | a line inserted before the opening scene's first page |
| [example.koukaku-speaker-name](../../../oms/examples/mods/example.koukaku-speaker-name/) | a data file that renames ヴァイスハイト's name form 1 at `koukaku.data.layered`; the [koukaku-speaker-name](../../../tools/drive/scenarios/port/koukaku-speaker-name.steps) scenario plays a new game to his first line under it |
| [example.koukaku-event-log](../../../oms/examples/mods/example.koukaku-event-log/) | a Lua mod that logs every event of `koukaku-base` with its fields, and gives one 転移の石 at each `koukaku.data.ready` through `give_item` |
| [example.koukaku-battlefield-land](../../../oms/examples/mods/example.koukaku-battlefield-land/) | a data file with a battlefield map's part: map 98's cell at row 1, column 1 becomes 沼地 for its own 地面, at `koukaku.battlefield.loaded` |
| [example.koukaku-debug-menu](../../../oms/examples/mods/example.koukaku-debug-menu/) | a script mod that opens the title's debug menu from Quit; the [koukaku-debug-events](../../../tools/drive/scenarios/port/koukaku-debug-events.steps) scenario plays 全体マップ with the two mods above, and the [koukaku-battle-award](../../../tools/drive/scenarios/port/koukaku-battle-award.steps) scenario 戦闘 with the event log |
