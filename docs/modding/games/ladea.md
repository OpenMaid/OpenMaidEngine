# Modding Tenbin no La DEA

天秤のLa DEA。-specific facts for the generic guides. The identity block (game id, engine ABI, modules, tables, patch
points) is generated on [the games page](README.md#tenbin-no-la-dea).

Script mods, text packages, data files and Lua mods work for this game (modding level M3), through its base mod,
`ladea-base`: it names the game's cells, declares its events, gives a helper and accepts the data kind
`ladea.tables`. The generated page says which level the game is at.

## Install and options

The install this guide was written against mounts two append catalogs, `APPEND01.AAI` (selector 1) and `APPEND02.AAI`
(selector 2), and its title screen shows `Version 2.00.0011`, 「“野牛の蹄”亭 秘密部屋DISC 追加」 and
「ビジュアルファンブック Ver2.0 追加」; its scripts are 914 base and 140 append records. Its folder also holds 56 loose
scripts and three loose pictures beside the catalogs, from the game's updates. AGE opens a loose file named for a
record before it reads the record from the archive, and so does the port, so a loose script is the one that runs and
the one a fragment's fingerprint names. A profile made on it in the launcher needs nothing else, so the guides'
commands take `--profile <profile>` alone. Without a profile, they take the game's options:

| Install | Options |
|---|---|
| 天秤のLa DEA。 | `--game ladea --game-root DIR` |

- **From source:** the conventional install is `games/ladea/` beside the repository, so `--game-root ../games/ladea`
  from `age-reimpl/`. The port is `.\run-godot.cmd -Game ladea`, with `-Mod PATH` for a mod. The driver is
  `py -3.11 -X utf8 tools/drive.py --game ladea port start --mod PATH`. Its points for the game include `title.start`
  (開始 on the title screen), `start.begin` (ゲーム開始 on the 初期設定 page that 開始 opens), `adv.next`, and
  `menu.save` and `menu.load`, the データセーブ and データロード tabs of the screen a page's right click opens (the
  driver's [天秤のLa DEA。 points](../../../tools/drive/ladea.toml)).
- A new game passes the 初期設定 page, then the opening scene starts on black.
- Native AGE shows Japanese dialogs, so `dialogs` and `answer` use Japanese labels.
- The menu bar, the fullscreen strip, the 終了 dialog and 設定 are the port's own.
- `use ladea.globals` names the cells by [the global reference](../../global-reference-ladea.md); a cell it does not
  name is written by its address (`G[0x3a5e]`), as the decompiled scripts print it.

## First mod values

The values for [your first mod](../first-mod.md), on the opening scene:

| Step | Value |
|---|---|
| Page | The opening scene's first page (`神殺しと呼ばれる男がいる。`). |
| Site | `0xed5` |
| Fingerprint | `sha256:627b62a1c0586c3adb7e17c04fef71aac061160f2a7f45887022355895a47482` |

A page of this game starts at the instruction that turns the tick presentation off, before it resets the text
layouts and sets `G[0x3a47]` (the page has text). That instruction is the site to insert a page before:

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:627b62a1c0586c3adb7e17c04fef71aac061160f2a7f45887022355895a47482"
use ladea.adv

before site 0xed5 {
    "[MOD] Hello from my first mod."
}
```

The finished mod is [example.ladea-prologue-line](../../../oms/examples/mods/example.ladea-prologue-line/). Its line
is the first page on both targets (native AGE on the exported `SC0000.BIN`, and the port with `--mod`), at
`SC0000@0xf16`; the game's own first page follows it, at `SC0000@0x1082`. The two targets' shots of both pages are the
same pixels but the wait indicator's, which turns (the driver's
[prologue line](../../../tools/drive/scenarios/ladea-prologue-line.steps) scenario, 2026-11-26).

## Dialogue lines

The prelude's template is `line(text, speaker = none, face = none, voice = none, form = 0x0)`:

```text
"[MOD] A narration line."
セリカ "「何か面白い物でも見つけたのか？」"
シュリ "「は、はいっ。見たことがない物が沢山あったので、見入ってしまいました」" (voice: 0x51, form: 0x1)
セリカ "「そうだな、まずは宿を取るとしよう。長旅で疲れただろう？」" (face: 0x59)
```

- A **speaker** is a name of the prelude's `speaker` enum, the index into the names CHINIT writes: a row of four name
  forms for each character. The enum names the 81 speakers the scenes set; a name that is not an identifier
  (`リ・クアルー`), or that two speakers share (`レウィニア騎士`), carries its id (`speaker_0x2f`), with the name in a
  comment. A line without a speaker is narration.
- `form` picks the name form the name plate shows, 0 (the default) to 3: シュリ's form 1 is `荷物を背負った少女`, her
  name before the scene introduces her. Any other value shows `？？？？`; the scenes' few pages in form -1 stay core
  statements.
- `face` is the face picture shown by the message window, a picture's resource id; a line without one shows none.
  Copy a face from a line of the character you want, in the decompiled scene (`OME-Tool oms-decompile --level
  idiomatic --script SC0000.BIN --profile <profile>`).
- `voice` is the voice clip, played on the first of the game's three voice channels. The template mutes it as the
  game does, for a character whose voice the player turned off. Narration can carry one too: the opening's narration
  is voiced.

The scene library is 魔導巧殻's, and the subs the two games share take that game's names; the subs that load and
compose an event CG (`load_cg_parts`, `compose_cg_parts`, `mark_cg_seen`), the background voice of the replay
scenes (`start_background_voice`, on the third channel) and `set_goddess_gauge_conditions`, which gives セリカ the
conditions 女神 and 抑制 by the goddess gauge, are this game's. Pages that span more than one `show_text`
call, carry furigana or play a voice on another channel stay core statements in a decompiled scene, as in the other
games. The prelude lifts about 27,500 of the scenes' 28,900 pages.

## Text and language packages

`omp text extract` writes a catalog for every script that draws text (204 files and about 37,800 entries for this
install). The game's text uses private-use characters (`U+E000` to `U+E003` and `U+E010`), which it draws with its
own font, `AGE Extend`: the third page's source is `古の女神`, `U+E001`, `U+E003` and `アストライアを殺し、その肉体を奪った男。`,
and its page joins the two into one long dash. They do not show in most editors, so copy a `source` from the extracted
catalog instead of typing it; `omp text check` warns of an entry that matches nothing. A translation leaves them out or
keeps them as it likes. The names on the name plates are strings CHINIT stores: an entry of `_global.toml` translates
one wherever it is drawn (`source = "セリカ"`, `text = "Serika"`).

A language package with three translated lines of the opening scene, a line of its eighteenth page and the names
セリカ and ハイシェラ was checked and run in the port (2026-11-26, `--mod …` and the options panel's language `en`): the
scene's first three pages read in English, and the eighteenth too, under セリカ's translated name. Nothing larger has
been tried on this game.

## Data edits

A data file ([data files](../data-files.md)) changes the game's record tables with no Lua and no script.
`ladea-base` accepts the kind `ladea.tables`. It applies each file's boot tables at `ladea.data.layered`, after the
base and both appends have written them and before INIT2's own work reads them. A dungeon floor's part comes each time
FIELD has walked the FLINIT chain for the floor it enters (`ladea.floor.loaded`), and a town place's part each time
SETORNDATA has built the place (`ladea.town.place_loaded`). This one renames the name plate シュリ shows before the
opening scene names her, her name form 1; the comment gives the game's own value:

```toml
# data/names.toml; the manifest names it with [[data]] kind = "ladea.tables", file = "names.toml"
[character.3.name_forms]
1 = "[MOD] 旅する少女"                  # 荷物を背負った少女
```

The plate shows 18 half-width characters, the length of that form, and in the port it cuts a longer name.

A fragment can read and write the same tables (`use ladea.globals`). The tables a file edits, by what they hold (the
generated page lists them with their fields):

| Kind | Tables |
|---|---|
| Characters | `character`, `voice_setting`, `fate`, `profile` |
| Skills and battle | `special_skill`, `condition`, `element`, `routine`, `battle_animation`, `formation` |
| Items | `item`, `item_kind`, `item_class`, `stone`, `item_improvement`, `alchemy_recipe` |
| Encounters | `special_encounter` |
| The dungeon | `floor`, `map_block`, `object_kind`, `atlas_marker`, `land`, `puzzle`, `field_action`; `floor_object` through the scope `dungeon_floor` (below) |
| The town | `town_building`, `town_place`, `quest`, `inn_rite`, `unlock`; `town_spot` through the scope `place` (below) |
| Registries and screens | `scene`, `cg_gallery_entry`, `replay`, `music`, `tutorial`, `glossary` |

```text
use ladea.globals

item[201].name = "[MOD] 治癒の水・小"
```

- **Tables and fields** are those of [the global reference](../../global-reference-ladea.md), "Tables", by the names
  in its `field` column. Every table's layout, and what each of its fields holds, is in
  [天秤のLa DEA。's data tables](../../ladea-data-tables.md). A field of several cells without names takes its columns
  by number: `character.3.name_forms.1` is name form 1.
- **An item's kind and class.** An item has both. Its kind, `item.kind`, is a record of `item_kind`: 1 重要 to 10
  売却専用, the 必殺 and magic schools and the enemies' attacks. Its class, `item.class`, is a record of `item_class`:
  the weapon classes 杖 to 刀, the armour, the stones, the other items' classes and the schools'. ADDITEM sets a key
  item's (kind 1) story flag, and a character's rank is kept by kind, the schools among them.
- **Saved fields are refused:** `scene.played`, `scene.seen` and `tutorial.shown`. They lie below the saved prefix,
  so a load would bring the saved value back over the edit. Change them from Lua.
- **Tables a file does not edit:** the ones the game fills for one battle or one walk when it starts it, BTANINIT's
  effects, ESINIT's formation slots and ENINIT's encounter entries. The kind has no event at those loaders; a fragment
  or a Lua hook at the loader's script reaches them.
- **The shape.** `ladea-base` ships the kind's shape, `shapes/tables.schema.json` ([data files](../data-files.md),
  "Shapes"), so the link, `omp check` and `omp build` refuse a table, field or column the reference does not name, a
  saved field, an id or a column out of range and a value of the wrong kind, with the file's line. `omp lua-stubs
  --game ladea` writes a copy and prints the `#:schema` line for an editor.
- **A floor's objects** are named through the floor: `dungeon_floor.65.object.8.action_args.1` is the count floor
  65's chest in slot 8 gives (floors 1 to 86; a floor's slots run from 1 to its `floor_object_count` − 1). FLINIT
  writes the floor in play and clears nothing, and FIELD walks it on every map-block entry, so the part is in the
  slots while its floor is the one entered. A load on a floor runs no walk, so the part comes back at the next entry
  ([the data tables](../../ladea-data-tables.md), "FLINIT").
- **A town place's spots** are named through the place: `place.5.spot.1.x` is place 5's first spot (places 1 to 20).
  SETORNDATA clears the spots and builds them from the ORINIT chain each time TOWN shows a place, once for each opened
  place while TOWN builds its list of places, and on a load in a scene over the town. The part is applied after each.
- **Saves.** No boot table is in a save: boot rebuilds every one at each start and each load, so a changed value is
  there on an old save too, where the game reads it from the table. What the game copies out of a table into saved
  state, so that a change does not reach it, is listed in [the data tables](../../ladea-data-tables.md) under each
  part's "What is saved": among it a character's level, items and starting ranks at the first join, an atlas's hidden
  parts at a floor's first visit, an object's starting kind until it is used, an unlock's bonus when it is unlocked,
  and a quest's rewards when it is reported.
- A fragment's write to a table is not saved, and it lasts until the game builds the tables again, at its next start
  at the latest. A data file's edit is made again at each boot and each load, while its mod is enabled.
- **Run in the game:** the example's boot edit (the
  [ladea-speaker-name](../../../tools/drive/scenarios/port/ladea-speaker-name.steps) scenario) and a floor's part
  (`example.ladea-floor-chest`, in the [ladea-debug-field](../../../tools/drive/scenarios/port/ladea-debug-field.steps)
  scenario), in the port. No edit was run on native AGE (data files are port-only).

## Patch points and events

The named points ([the patch point reference](../../patch-point-reference-ladea.md) gives each one's description and
fingerprint) sit after the loaders both appends' layers run as well, so a fragment or a Lua hook there sees every
layer's work:

| Point | Script | Where |
|---|---|---|
| `ladea.boot.data_layered` | `INIT2.BIN` | after the base tables and both appends' (op `0x143` runs `$1$AUTORUN`, then `$2$AUTORUN`) |
| `ladea.boot.data_ready` | `INIT2.BIN` | INIT2's exit, after its own work on the tables |
| `ladea.floor.after_object_loaders` | `FIELD.BIN` | after the FLINIT chain, on each map-block entry |
| `ladea.town.after_spot_loaders` | `SETORNDATA.BIN` | after the ORINIT chain, for the place being built |
| `ladea.experience.before_add` | `ADDEXP.BIN` | one character's experience, before it is added and the levels are worked out |

The award's amount is the add's third operand, `arg[3]`: `G[0xe7a12]`, a work cell the caller sets just before it
runs ADDEXP, so a fragment at the point reads and changes it through the operand; a Lua subscriber of
`ladea.experience.award` reads and writes it as `e.amount`. Every award passes there: a battle's (RESULT), condition
40 (経験) as the party walks (FIELD), and the 30 scenes that give a fixed amount.

`ladea-base` declares five game events ([the Lua guide](../lua.md), "Game events"):

| Event | Fields |
|---|---|
| `ladea.data.layered`, `ladea.data.ready` | none |
| `ladea.floor.loaded` | `floor` (`current_floor_id`) |
| `ladea.town.place_loaded` | `place` (`town_current_place_id`) |
| `ladea.experience.award` | `amount` (rw), `character` (`character_arg_id`), `level` (`character_levels[character]`, before the add) |

Its helper changes the game through its own script, as the game's own gifts do, and puts back the cells it used:

- `give_item(item, count)` (item 1 to 19999; count -999 to 999, 1 by default) runs `ADDITEM`. It keeps the count
  within 0 and 999. A stone the party has had before comes as its crystal (`item_duplicate_crystals`), and of a first
  gift of several stones ADDITEM keeps one and adds the rest as crystals. It marks the item obtained and seen (in the
  save and in the shared profile), and a key item (kind 1) sets its story flag, `9001 + item`, while one is held. It
  returns the new count of `item`, and the item ADDITEM changed last: `item`, or the crystal it gave in its place.

Every event has been raised in a run of the port: the boot events, the helper and the data file's boot and town parts
by `ModLuaLinkTests`, and the boot events, sixteen awards and the floor event through the debug menu (below) by the
[ladea-debug-field](../../../tools/drive/scenarios/port/ladea-debug-field.steps) scenario, and the town event, once
for each of the 20 places, by the [ladea-debug-town](../../../tools/drive/scenarios/port/ladea-debug-town.steps)
scenario. No award from a battle has been raised in a played game yet.

## The debug menu

The title builds the developers' デバッグメニュー right after its Quit calls QUIT, which ends the game (`TITLE@0x40f`),
so no retail run reaches it. [example.ladea-debug-menu](../../../oms/examples/mods/example.ladea-debug-menu/) deletes
that call, so Quit opens the menu. Each entry that starts a mode sets it (`system_mode`) and leaves the title through
its common exit, as Game Start does, so the mode takes effect. It targets `native`: `omp export` gives a `TITLE.BIN`
that opens the same menu on native AGE.

| Entry | Point | What it runs |
|---|---|---|
| 通常ＡＤＶ, ＭＡＰ会話 | 640 195, 640 265 | a scene picker (`SC????.BIN`), AGERC's dialog on native AGE |
| ＡＤＶデバッグ２ | 640 335 | a debug scene, `DEBUGADV2` |
| 迷宮マップ１（ＰＧ用） | 640 405 | `INITGAME`, then `DEBUGFIELD`: characters joined, `unlock_points` set to 99,999, and the dungeon at map block 999 |
| 迷宮マップ２ | 640 475 | `INITGAME`, then `DEBUGFIELD2`: characters joined, items, 10,000,000 experience for each party character, and the dungeon on floor 65 (ヴィーンゴールヴ宮殿) |
| 迷宮マップ３ | 640 545 | `INITGAME`, then `DEBUGFIELD3`: the dungeon at map block 999 |
| 街 | 640 615 | `INITGAME`, then `DEBUGTOWN`: characters joined, every place opened, and the town |
| キャンセル | 640 685 | back to the title |

- Run on both targets: the menu and 迷宮マップ２, alike (the same floor, party and levels; only the developers'
  counters at the right edge differ), and 迷宮マップ１. In the port, the
  [ladea-debug-field](../../../tools/drive/scenarios/port/ladea-debug-field.steps) scenario plays 迷宮マップ２ with
  the floor part and the event log, and the [ladea-debug-town](../../../tools/drive/scenarios/port/ladea-debug-town.steps)
  scenario plays 街 with the event log.
- Map block 999 has no map script in MPINIT, so SETMAPDATA calls script 0, which is SYSTEM4, and the game boots again
  inside FIELD (`SETMAPDATA@0xd8`). 迷宮マップ１ does so on both targets; 迷宮マップ３ goes to the same block.
- The four entries that start play set `debug_play_active`: the town then skips the SCJUMP chain (`TOWN@0xc6`), and no
  scene or tutorial marks itself seen.
- The scene pickers need AGERC's dialog, which the port does not have: there op `0x140` picks nothing.

## Catalogs, scenes and saves

- **Selectors.** The install ships two append catalogs (`APPEND01.AAI` and `APPEND02.AAI`, selectors 1 and 2), so a
  first mod's catalog normally gets selector 3.
- **Untested.** No 天秤のLa DEA。 mod that adds a scene or a script has been tested, on either target. Start from one of
  the game's own append scenes, and test saving on native AGE as well as in the port.

## Examples

| Path | What it shows |
|---|---|
| [example.ladea-prologue-line](../../../oms/examples/mods/example.ladea-prologue-line/) | a line inserted before the opening scene's first page |
| [example.ladea-speaker-name](../../../oms/examples/mods/example.ladea-speaker-name/) | a data file that renames シュリ's name form 1 at `ladea.data.layered`; the [ladea-speaker-name](../../../tools/drive/scenarios/port/ladea-speaker-name.steps) scenario plays a new game to her first line under it |
| [example.ladea-event-log](../../../oms/examples/mods/example.ladea-event-log/) | a Lua mod that logs every event of `ladea-base` with its fields, and gives one 治癒の水・小 at each `ladea.data.ready` through `give_item` |
| [example.ladea-floor-chest](../../../oms/examples/mods/example.ladea-floor-chest/) | a data file with a floor's part: floor 65's chest in slot 8 gives nine 治癒の水・小 for its own four 治癒の水・特大, at `ladea.floor.loaded` |
| [example.ladea-debug-menu](../../../oms/examples/mods/example.ladea-debug-menu/) | a script mod that opens the title's debug menu from Quit; the [ladea-debug-field](../../../tools/drive/scenarios/port/ladea-debug-field.steps) scenario plays 迷宮マップ２ with the two mods above, and the [ladea-debug-town](../../../tools/drive/scenarios/port/ladea-debug-town.steps) scenario 街 with the event log |
