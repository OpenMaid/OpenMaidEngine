# 天秤のLa DEA。 data tables (`*INIT`)

Canonical home for how 天秤のLa DEA。 (ladea, SYS4445, AGE 4.45D) stores its static game data: which `*INIT` scripts
exist, how they are layered, where each table lives in the global bank, and what is known of its fields. Names for
the fields that are understood live in [the ladea globals registry](../vm-map/globals-ladea.toml) (generated →
[its global reference](global-reference-ladea.md)). Its engine, AGE 4.45D, comes after 魔導巧殻's 4.43B, and its boot has
魔導巧殻's shape (hook chains walked slot by slot), with two appends instead of one; the other games' tables
are in [魔導巧殻's](koukaku-data-tables.md), [Arterial's](arterial-data-tables.md), [Verita's](verita-data-tables.md),
[戦女神ZERO's](bg0-data-tables.md) and [Kamidori's](kamidori-data-tables.md). A reading here rests on La DEA's own data
or consumers, never on a likeness alone.

**How far this goes (2026-11-26).** Every boot table's *layout* is here: its arrays, their strides and what values
they hold, for the 34 boot tables, with the tables that run on demand read from their listings. What an array
*means* is given where the data or a consumer shows it, each reading with the script and offset it rests on. A row
that says nothing in its last column is not read yet; "What is not decoded" at the end lists the gaps.

## How the data gets into memory

Like the other games, La DEA has no data files. Every table is a script of constant writes into the flat global
bank: `mov` for integers, `set-string` for text, `copy-inline-int-array` for a few bulk arrays. Runtime code reads
the bank with `lookup-array` (`base + id`) and `lookup-array-2d` (`base + row * stride + column`).

**One array per field.** A table is a set of parallel arrays. A record's field is the cell
`base + cells_per_id * id + column`: one cell per id for a scalar, several for a short list. Each array is allocated
for the table's *capacity* whether or not an id is used, and the arrays of a table lie back to back. Ids are sparse
and mostly start at 1.

**Boot order.** `INIT2.BIN` calls 34 initializers in this order (`INIT2@0x0`..`0x63`): SCINIT, CHINIT, COINIT,
AFINIT, SSINIT, FMINIT, FAINIT, ILINIT, ITINIT, DPINIT, IMINIT, RTINIT, BLINIT, QUINIT, ALINIT, NUINIT, MPINIT,
OBINIT, OBINIT2, LAINIT, PZINIT, ATINIT, EEINIT, ELINIT, CGINIT, SPINIT, MUINIT, CVINIT, FTINIT, CIINIT, CFINIT,
VIINIT, TUINIT, BTANINIT2. Op `0x143` then runs each mounted append catalog's AUTORUN (`INIT2@0x23d`): `$1$AUTORUN`
and `$2$AUTORUN`, which call their own `$1$<TABLE>.BIN` and `$2$<TABLE>.BIN` for all 34 tables, in the same order and
at the same offsets (`@0x0`..`0x63`). The install has two appends, so a table's runtime image is the **base, then
`$1$`, then `$2$`**, later writes winning. Of the 68 fragments, 28 are empty stubs: 16 of `$1$`'s (COINIT, AFINIT,
ILINIT, BLINIT, QUINIT, NUINIT, OBINIT, LAINIT, PZINIT, EEINIT, ELINIT, MUINIT, CVINIT, CIINIT, TUINIT, BTANINIT2)
and 12 of `$2$`'s (COINIT, AFINIT, SSINIT, ILINIT, NUINIT, LAINIT, PZINIT, ATINIT, EEINIT, ELINIT, MUINIT, VIINIT).
The others write 5,928 cells (`$1$`) and 11,503 (`$2$`). All but FMINIT's only write cells the layers before them
left unwritten: they add records, or fields to records the base made. `$1$FMINIT` rewrites 2 of the base's cells and
`$2$FMINIT` 9 (the boot image, compared cell by cell; FMINIT's section). No fragment is behind a test.

**On demand.** Eight `*INIT` scripts are not run by INIT2, so they are not in the boot image: EVINIT, ORINIT, SHINIT,
FLINIT, ENINIT, ESINIT, DUINIT and BTANINIT. INIT2 puts each in slot 0 of a hook chain,
and each append its fragment in its own slot ("Hook chains" below); the screen that uses the table walks the chain.
COINIT is in both lists: INIT2 runs it, and its chain lets a screen run it again.

## Which files are authored data

`games/ladea/` holds the archives `DATA1`–`6.ALF` and the two appends `APPEND01.ALF` and `APPEND02.ALF` (each with its
`.AAI` index), and 60 loose game files in its root: `SYS4INI.BIN`, 56 scripts (two of them `$1$` records) and 3
pictures (`SO011`, `so004`, `so040`). Native AGE reads a loose file before the archive, and the data tooling reads the
install the same way (`tools/paths.py`'s `scripts()`), so every count below includes them. Fifteen tables have a loose
copy, AFINIT, CHINIT, CIINIT, DUINIT, ESINIT, FAINIT, FTINIT, IMINIT, ITINIT, MPINIT, ORINIT, QUINIT, RTINIT, SPINIT
and VIINIT, and two fragments, `$1$COINIT` (an empty stub, as the archive's is) and `$1$ITINIT`; so do the drivers
INIT2 and SCJUMP. The install's title shows `Version 2.00.0011`; where the loose files come from is not established
here.

## Tooling

All commands run from `age-reimpl/` with `py -3.11 -X utf8` and `--game ladea`
([the tools reference](tools-reference.md)):

1. Extract the archives once with the local `bin/BinExtractALF.exe` into `../extracted-ladea/`: `DATA1/` to `DATA6/`,
   and the appends' records flat in `APPEND01/` and `APPEND02/`.
2. `parse_sys4ini.py` → the asset index and call-script names.
3. `extract_phase2.py` → `build/games/ladea/disasm/*.asm` and the text corpora.
4. `init_image.py` → `build/games/ladea/init-image.json`, the **executed** boot image: the port's VM runs the boot
   order above and records what each of the 102 scripts wrote (34 tables and 68 fragments).
5. `init_layout.py` → each table's layout, inferred from the write order and the image, with La DEA's leaders
   (`LADEA` in the tool). `init_layout.py <TABLE>` prints a table's arrays column by column, `--markdown` the layout
   tables below, and `--write` saves them as `build/games/ladea/data/<TABLE>.layout.json`.
6. `global_map.py` → `build/games/ladea/global-var-map.{json,md}`, the access shape of every global, and
   `story_flags.py` → `build/games/ladea/story-flags-candidates.json`, the globals the scripts branch on.

`init_layout.py` is given the array each record writes first (the *leader*) and the table's capacity, takes a
record's id from the leader cell, and finds every other field as the `(stride, base)` on which the most records
agree. Where that cannot work, the leader declares the array (`arrays=` in the tool). Tables with no single leader
are shown by the arrays the scripts index (`lookup-array` operands), which is a weaker view: an array no script
indexes with a variable is not seen, its cells are counted in the array before it, and the base shown is the indexed
array's, not always the field's. The tool does not lay out the on-demand tables, which are not in the boot image;
their sections are read from their listings.

**The grid, and `†`.** A table's arrays lie back to back, each a whole number of capacities long, so their bases
agree modulo the capacity. The tool marks an array whose base does not: `†` in the layouts below. Such an array
either has another table's cells between it and the rest, or is not indexed by the record's id at all, and then
the base and the ids in its row are wrong. Each `†` row says which.

**The saved prefix.** A save keeps the integer globals below `0xcf2e6` (848,614) and the string globals below `0x34f`
(847), and the first float global alone, the profile's `persistence.bankDimensions` ([the format notes](sys4-format-notes.md));
the banks hold 16,015,186 integers, 30 floats and 159,379 strings. Every boot table's arrays lie above the prefix,
its integer arrays and its string arrays alike, so a save carries none of them, and INIT2 writes them on every boot
and every load ("The boot order" below): a change to a table reaches a saved game too. A table's section says where
the game copies a field into a cell below the prefix, which a save then keeps.

## Table inventory

"boot" means INIT2 runs the table and then `$1$AUTORUN` and `$2$AUTORUN` run its layers ("Boot" below; 28 of the 68
layers are empty). "On demand" tables sit in slot 0 of a hook chain and their layers in slots 1
and 2; the scripts that walk the chain are in brackets.

| table | holds | runs |
|---|---|---|
| SCINIT | scenes: the script each scene number plays, the SCJUMP context that offers it, whether it plays over the map, the mark the town shows while it is on offer | boot |
| CHINIT | characters keyed by character id, 1 to 962 (enemies from 301): base character, sort key, sex, race, temperament, sizes, boss and large-unit flags, affinity type, support, follow-up and enemy actions, routine, innate skill ranks, starting item kinds, equipment and items, equippable classes, base level, level cap, morale, base stats and growth, rewards and drops, habitats, name forms, names, description. 368 records, 16 from `$1$CHINIT`, 45 from `$2$CHINIT` | boot |
| COINIT | a character's look in one costume: status picture, list faces, battle pictures by unit state, field, jump and carry pictures, battle voices | boot (with nothing selected), then on demand: `coinit_chain_script_ids` (BTLMAIN, FIELD, CHMENU and other screens) |
| AFINIT | the battle's lists: element names and the defence-by-attack affinity matrix, races, item kinds (重要 to 売専, 必殺 and magic schools, required weapons), item classes (names, flags, use sounds), field actions, formations, rarity ratios | boot |
| SSINIT | special skills, five levels a family: kind, attack-only, HP/MP/TP costs, trigger chances, amounts, always-on, the conditions a 【常時】 level gives, effect; its hooks CHECKSS1..4, SETSSBONUS, SETSSDAMAGE | boot |
| FMINIT | the unlock board: the unlock in each cell of a 24 × 44 grid, and its right and down links | boot |
| FAINIT | unlocks (解放): names and texts, prerequisites, kind, targets and amounts, cost in unlock points, story flag, node size and picture | boot |
| ILINIT | conditions, five levels each: polarity, boss-proof, resistance and affinity column, duration, 21 modifiers, cure bits, icon | boot |
| ITINIT | one id space: key items, consumables and materials, stones, weapons, armour, shields, accessories, spells, skills, enemy actions, ★ variants: names, kinds and classes, prices, the 22-stat vector, conditions, HP/MP/TP amounts and costs, waits, targets, animations, ammunition, use limits | boot |
| DPINIT | stones (by item id 1001..1640): pictures, the sealed character, cost, and by bearer type the compatibility, skill and level, three stat bonuses | boot |
| IMINIT | improvement (by item id): points per level, three stat bonuses per level, the top level's story flag | boot |
| RTINIT | battle routines, up to 100 steps each: rule script, chance, action, arguments, story flag, use limit, tactic mask, skip bit | boot |
| BLINIT | town buildings (places 1..20, fillers, the forty houses): name, tile position and size, place, three variants of pictures and their unused conditions, place tab; by place: list order, spot layout, map marker, background | boot |
| QUINIT | quests: title, faction, client, description, board conditions (flags, item held), achieve and fail conditions, rewards (item, money, unlock points, faction points), report flag | boot |
| ALINIT | alchemy recipes: product, ingredients and counts, story flags required and forbidding, character and level, building mask | boot |
| NUINIT | the inn's rites (精力信仰): the goddess gauge they set, what they need, event picture, unread names and descriptions | boot |
| MPINIT | the floors, 74 (name, dungeon, atlas sort key, encounter class, gauge drain), and the map blocks, 234 (floor, map and event scripts, music, encounter sets, enemy levels, encounter level, battle and far backdrops, overlay, place on the floor, up to nine pieces) | boot |
| OBINIT | the object kinds, 130 (frame, idle animation, blend, fade, next kind, atlas marker, sound, picture, effect), and the six atlas markers (name, key, y offset) | boot |
| OBINIT2 | the object kinds' draw-order offset, wall polygon and event polygon, 128 kinds | boot |
| LAINIT | the lands, 15 (footstep sound, walking sheet, flags, speed change) | boot |
| PZINIT | the block puzzles, 11 (reset rule, grid, blocks' starts, colours and kinds) | boot |
| ATINIT | the floors' atlases, 71 (minimap and atlas scales, hidden parts, parts' pictures) | boot |
| EEINIT | encounter chances, five rows of ten (distance band × encounter level) | boot |
| ELINIT | special encounters (視線を感じる, 異質な気配がする), 18 ids of 1 to 27: kind, prerequisite, flags, area kinds, chances, scene or battle | boot |
| CGINIT | the CG gallery: the CG and variant pairs of each page's twenty thumbnails | boot |
| SPINIT | the scene replays: the script of each page's twelve slots | boot |
| MUINIT | the music room: each track's title, BGM number and unlocking resource | boot |
| CVINIT | voice settings keyed by setting id, 0 to 50: name colour, character, sample voice, pictures, name, epithet and voice actor; and the voice setting of each character, keyed by character id. 51 records, 1 from `$2$CVINIT`; `$1$CVINIT` is empty | boot |
| FTINIT | fates (運命) keyed by fate id, 1 to 503: order, text, chapter, required and forbidding flags. 151 records, 5 from `$1$FTINIT`, 7 from `$2$FTINIT` | boot |
| CIINIT | profiles of the 人物 book keyed by character id, 1 to 231: order, name, race, six lines, their characters and conditions. 47 records, 1 from `$2$CIINIT`; `$1$CIINIT` is empty | boot |
| CFINIT | the characters' forms in the 人物 book keyed by character id, up to nine each: name, conditions, picture, backdrop, up to 25 gallery pictures. 47 characters; `$1$CFINIT` and `$2$CFINIT` add forms to 3, 51, 66 and 2 (and 231) | boot |
| VIINIT | entries of the 用語 glossary keyed by entry id, 1 to 510: order, title, category, thirteen lines and their conditions. 101 records, 2 from `$1$VIINIT`; `$2$VIINIT` is empty | boot |
| TUINIT | tutorials: the screen, text slot, title and the conditions of each | boot |
| BTANINIT2 | battle animations: the BTANINIT effect of each of six slots, start times, length | boot |
| EVINIT | an event CG's part sets: base pictures, part pictures and positions, the save thumbnail's stand-in | on demand: `evinit_chain_script_ids` (CGMODE, the scenes, INIT2 on a load in a CG) |
| ORINIT | the spots of the place in play: grid position, pictures and animation, conditions, character, voice, exit to a field map or door to a town screen; hotspots in the click grid | on demand: `orinit_chain_script_ids` (SETORNDATA) |
| SHINIT | the shops' offers (200 slots, flags, price column, pictures) for the shop in `G[0x4d2d0]` | on demand: `shinit_chain_script_ids` (SHOP) |
| FLINIT | the object slots of one floor (starting kind, action, arguments, count), 71 floors | on demand: `flinit_chain_script_ids` (FIELD) |
| ENINIT | the encounter set in play (`encounter_set_in_play`), up to 100 entries: weight, gauge window, three formations; 52 sets (base 43, `$1$` 7, `$2$` 2) | on demand: `eninit_chain_script_ids` (FIELD and the map scripts) |
| ESINIT | the formation in play (`formation_in_play`), eight slots: unit, column and row ranges, chance, flags, item, link; and the formation's mark; 579 formations (base 516, `$1$` 23, `$2$` 40) | on demand: `esinit_chain_script_ids` (SETENP) |
| DUINIT | one fixed battle (place name, three waves' parties, flags, music and backdrops), 205 battles | on demand: `duinit_chain_script_ids` (BTLMAIN, FIELD) |
| BTANINIT | battle effects: picture, sheet size and frames, length, anchor, offsets, scale, sound and delay | on demand: `btaninit_chain_script_ids` (SETEFFECT) |
| TUMES | the tutorials' text | a chain script: `tumes_chain_script_ids` (TUTORIAL) |
| SCJUMP | the scene dispatch's rules | a chain script: `scjump_chain_script_ids` (BTL, FIELD, TOWN) |

## Boot

### The boot order (SYSTEM4)

SYSTEM4 runs these steps on each root run:

1. It stops the three voices and centres their pans (`SYSTEM4@0x4`..`0xd`, ops `0x2f6` and `0x2f8`), sets up the nine
   text layouts, the wait indicator, the text's line images and the layouts' object ranges (`SYSTEM4@0x10`..`0x1b0`),
   and puts layouts 1 and 2 at the origins of the message window's style `scene_window_layout` `G[0xed4]`, 0 or 1 (`SYSTEM4@0x1b7`,
   `@0x6d4`..`0x740`).
2. It loads `G[0x5]` from the shared profile (`SYSTEM4@0x1ba`). If the cell is set, it runs LOADCONFIG; if not, it
   runs INITCONFIG, then sets `G[0x5]` to 1 and stores it (`SYSTEM4@0x1bd`..`0x1d2`). Then it runs CHECKCONFIG
   (`SYSTEM4@0x1d5`), which falls back to ＭＳ 明朝 and ＭＳ ゴシック when a saved font is not installed
   (`find-font-family-index`, `CHECKCONFIG@0x0`..`0x33`), sets the text's effect colour for the speaker
   (`CHECKCONFIG@0x43`..`0x116`, INITCONFIG below) and the message fade (`@0x3e`).
3. It copies the inline array 0, 4, 5, 7 into `sound_channels` `G[0xcf2fe]` (`SYSTEM4@0x1d8`): the channels the
   scenes release sound on (`sfx-release (global-int 0xcf2fe)`, in 191 scripts).
4. It sets twelve input actions and maps keys to eight of them, the eight joystick buttons from `joystick_buttons`
   `G[0x6e41]` (INITCONFIG5's defaults) and a mouse button (`SYSTEM4@0x1dd`..`0x23f`).
5. It preloads script slots 0x1a, 0x1b and 0x1e with CALCABIMAX, DRAWFRAME and DRAWFIELD (`SYSTEM4@0x24d`..`0x257`),
   and sets the pictures SO002, SO045 and SO001 as textures 0xf, 0x10 and 0x11 (`@0x25c`..`0x27f`).
6. It runs INIT2 (`SYSTEM4@0x286`), then sets up the message font (`@0x289`..`0x2c5`): size 28, white, the effect
   colour, render mode and offset CHECKCONFIG chose (`G[0xcf368]`, `G[0xcf369]`, `G[0xcf36a]`), the font
   `message_font_name` `GS[0x34b]`, bold, a ruby size of 10 and a line spacing of 16, once per run
   (`G[0x3a45]` = 2).
7. It reaches op `0xae continue-save-load-stack-restore` (`SYSTEM4@0x2ca`). On a numbered load, the engine zeroes
   every bank, copies the saved prefix back, and runs each restored frame from its entry up to `0xae` ([the engine
   notes](engine-re.md), "A load zeroes each bank in full"). So INIT2 runs on every boot and on every load, and on a
   load it runs with the saved cells already in place.
8. On the first root run only (`get-initial-root-run`, `SYSTEM4@0x2cb`), it runs LOGO (`@0x2d5`). Then, on every root
   run, it runs INIT (`@0x2d8`), which sets cells below the prefix (`INIT@0x0`..`0x2a`): the two
   `town_faction_points` `G[0x4d2cd]` = 10 each, `money` `G[0x7254]` = 0, `unlock_points` `G[0x1fdab]` = 20,
   `party_leader` `G[0x7259]` = 1, `goddess_gauge` `G[0x7250]` = 300 and its maximum `G[0x7251]` = 400, and the 600
   cells from `G[0x4cc8c]` = 2 (the characters' tactics among them). Then it runs TITLE (`@0x2db`).
9. It loops on the mode `system_mode` `G[0x0]` (`SYSTEM4@0x2de`..`0x6c1`): 1 a scene, run from `current_scene_script_id` `G[0xed0]`
   (`SYSTEM4@0x4c1`; "Scenes and screens" below), 2 TOWN (`@0x673`), 3 FIELD (`@0x687`), 4 BTL (`@0x69b`) and 5 DEAL (`@0x6af`); any
   other value shows 不正な実行モード and asks to exit (`@0x6b5`..`0x6bd`).

### INITCONFIG (the options)

INITCONFIG runs its five parts (`INITCONFIG@0x0`..`0xc`) and sets five more cells, each stored in the shared profile
(`INITCONFIG@0xf`..`0x6a`). LOADCONFIG reads the same cells back, and the CONFIG screen's three pages, CONFIG1 to
CONFIG3, change them; the pages draw their labels as pictures, so a cell's meaning below comes from its other
readers. Every cell lies below the saved prefix too.

| cell | default | read by | holds |
|---|---|---|---|
| `GS[0x34b]` `message_font_name` | ＭＳ 明朝 | `SYSTEM4@0x2a5`, CONFIG | The message font: SYSTEM4 sets it with `set-font`; CHECKCONFIG puts ＭＳ 明朝 back when it is not installed (`INITCONFIG2@0x0`). |
| `GS[0x34c]` `ui_font_name` | ＭＳ ゴシック | the screens, CHECKCONFIG | The screens' font; CHECKCONFIG puts ＭＳ ゴシック back when it is not installed (`INITCONFIG2@0x8`). |
| `G[0x6e5b]` `wheel_turns_pages` | 1 | ALCHEMY, BREED, CHMENU, IMPROVE, INFOCH, … (27 scripts) | While it is set, a list turns its pages with the mouse's horizontal wheel (`consume-mouse-hwheel-delta`, `ALCHEMY@0x38d`, `BREED@0x3ec`). |
| `G[0x6e5a]` `autosave_off` | 0 | `TOWN@0x408` | TOWN autosaves (its comment オートセーブ実行, `TOWN@0x41d`) only while it is 0. |
| `G[0xcf2e3]` | 1 | BUNKI, FIELD, TOWN, INFO, CGMODE, … (33 scripts) | While it is set, a choice menu starts with its cursor on its first row, and while it is 0 on none (`BUNKI@0x18b`..`0x19a`); `$2$ARENA` tests it the same way (`@0xeb`). Not decoded further. |
| `G[0xcda9f]`, `G[0xcdaa0]` | 0 | `SETSKLIST@0xb7`, `SETITLIST@0xd3` | A mode of the skill list and of the item list each (`1 - G[...]`). Not decoded. |
| `G[0x6e5e]` `field_frame_rate` | 60 | FIELD, DRAWWALK, DRAWWALK2, MOVESKIN, CARRY | The field's frames per second: CARRY ticks every `1000 / G[0x6e5e]` ms (`CARRY@0x37c`), and DRAWWALK moves `60 / G[0x6e5e]` steps a frame (`DRAWWALK@0x182`). |
| `G[0x6e60]` | 1 | `FIELD@0x1bef`, `SETMAPDATA@0xca` | FIELD and SETMAPDATA set `G[0x4dd89]` to it × 51. Not decoded. |
| `G[0x6e5f]` | 0 | DRAWFIELD, DRAWFRAME | While it is 0, DRAWFIELD draws a further layer of the field (`DRAWFIELD@0x114e`). DEBUGFIELD2 sets it to 1. Not decoded. |
| `G[0xcdaa1]` | 12 | `DRAWFRAME@0x99` | The field frame's transparency: DRAWFRAME draws it with alpha `(16 − G[0xcdaa1]) × 16`. |
| `G[0x6e61]` | 0 | `SELACT@0xe05` | A battle command switch. Not decoded. |
| `G[0x6e5d]` | 0 | `ANIME@0x3` | While bit 0 is set, ANIME clears its effect cells before it plays. Not decoded. |
| `G[0x6e63]` | 0 | `BTLMAIN@0x5a27` | While it is 0, BTLMAIN draws three more panels. Not decoded. |
| `G[0xecc]` `confirm_save_load` | 1 | `SAVE@0xbfa` | While it is above 0, SAVE asks ロードしますか？ or セーブしますか？ before it loads or saves. |
| `G[0x6e57]` `confirm_commands` | 1 | FIELD, SAVE, CONFIG2, CONFIG3 | While it is set, a command asks first: 脱出する (`FIELD@0x1725`) and タイトル画面に戻りますか？ (`SAVE@0xa01`). |
| `G[0x6e59]` | 7 | BUNKI, SBUNKI, SELACT, SELTARGET | Switches by bit: while bit 1 (2) is set, BUNKI puts the cursor on a row (`BUNKI@0x1a7`), and bit 0 does the same for SBUNKI (`SBUNKI@0x1ca`). Not decoded further. |
| `G[0x6e67]` `speaker_text_colour_mode` | 1 | every scene, CHECKCONFIG | 1: the text's effect colour is the speaker's (`voice_name_colors`, `CHECKCONFIG@0x52`..`0xdf`); −1: white text with no effect (`@0xe7`..`0x106`); any other value keeps the default `0x3a3a3a` (`INITCONFIG2@0x10`). |
| `G[0x6e65]` `message_window_colour` | 0 | every scene, BUNKI, HISTORY, CALLBACK_LOAD | The message window's colour: a scene draws the window with it (`gfx-draw-color 0x1e460 0 alpha G[0x6e65]`, `$1$SC5000@0x1989`; `BUNKI@0x6b9`). |
| `G[0x6e66]` `voice_continues_past_page` | 1 | every scene | While it is 0, a page's end stops voices 0 and 1 (`$1$SC5000@0x2589`..`0x259a`): the voice stops when the page turns (`INITCONFIG3@0x0`). |
| `G[0x6e62]` | 0 | every scene, CALLBACK_LOAD | A background voice switch: while it is 1 a page arms the voice duck's release even when a background voice plays (`$1$SC5000@0xeaa`..`0xec6`), and a load restarts the background voice on channel 2 only while it is 0 (`CALLBACK_LOAD@0x2db`). Not decoded further. |
| `G[0xcdaa2]` | 0 | the replay scenes (47 scripts), CALLBACK_LOAD | While it is set, a page releases channel 9 instead of playing its sound `G[0xcf27d]` there (`$1$SP5016@0x5843`, `CALLBACK_LOAD@0x102`). |
| `G[0x6e5c]` | 0 | FIELD, FACT | The stick's switches: bit 0 lets FACT read the stick (op `0x2f9`, `FACT@0x302`), bit 1 lets FIELD read keys while it walks (`FIELD@0xf13`). Not decoded further. |
| `G[0x6e64]`, `G[0xcdaa3]` | 0, 0 | the screens (29 scripts), CONFIRMJOY | The button prompts: while `G[0x6e64]` is 0 a screen draws the keyboard's, otherwise the pad's of style `G[0xcdaa3]` (`ALCHEMY@0x1765`..`0x17b1`, `BREED@0x18ca`). |
| `G[0xcf27a]` | 50 | CONFIRMSTICK2, FACT, FIELD | The stick's threshold, added to 100 before the stick's value is scaled (`CONFIRMSTICK2@0x256`, `FACT@0x355`). |
| `G[0x6e4b]` … +5, `G[0x6e51]` … +5 | INITCONFIG's inline array; 0 | CONFIRMSTICK, CONFIRMSTICK2, FACT, FIELD | The stick's axes, and whether each is inverted (`CONFIRMSTICK2@0x2e3`..`0x2f6`); stored one by one (`INITCONFIG@0x2f`..`0x6a`). |
| `G[0x6e41]` … +7 `joystick_buttons` | INITCONFIG5's inline array | `SYSTEM4@0x217`..`0x23a`, CONFIG3 | The eight joystick buttons SYSTEM4 maps (`map-joystick-button`). |
| `G[0x365d]` … +999 `voice_mute_switches` | 0 | every scene, HISTORY, the screens | 1000 cells, cleared at once (`copy-to-global`, `INITCONFIG4@0x0`). A page reads it by the speaker's voice setting (characters, below). |
| `G[0x6e68]` … +999 `voice_name_colors` | CVINIT's | CHECKCONFIG, CONFIG2, every scene | 1000 colours, one per CVINIT setting: CVINIT's `G[0x157276 + i]`, or −1 where that is 0 (`INITCONFIG4@0x14`..`0x68`); INIT2 fills a −1 again when CVINIT has a colour (below). |

`G[0x6e3b]` and `G[0x6e3d]` are loaded from the shared profile by GAMESTART too (`GAMESTART@0x12`, `@0x15`); GAMESTART
clears `G[0x6e3d]` when an append the profile last saw is no longer mounted (below). Neither is decoded.

### INIT2 (the boot driver)

INIT2 calls the 34 base tables in the order above (`INIT2@0x0`..`0x63`), fills slot 0 of the hook chains
(`INIT2@0x66`..`0x11f`, below), writes its constants, and then runs op `0x143 run-mounted-append-autoruns`
(`INIT2@0x23d`). Its last steps follow. The inline arrays' values come from the install's loose `INIT2.BIN`, where
`copy-inline-int-array` reads a block with a count before it at the code offset.

| offset | write | what it is |
|---|---|---|
| `INIT2@0x124`..`0x142` | `GS[0x26e81]` … `GS[0x26e87]` = E-Rank, D-Rank, C-Rank, B-Rank, A-Rank, S-Rank, M-Rank | Seven rank names. No script reads them by a base. |
| `INIT2@0x147`..`0x15b` | `attitude_names` `GS[0x26e89]` … `GS[0x26e8d]` = 超強気, 強気, 普通, 弱気, 超弱気 | DRAWSTATUS shows one, `GS[0x26e88 + value]`, 1 to 5 (`DRAWSTATUS@0x161e`). |
| `INIT2@0x160`..`0x16a` | `sex_names` `GS[0x26e8f]` … `GS[0x26e91]` = 男, 女, 無 | DRAWSTATUS shows one, `GS[0x26e8e + sex]`, 1 to 3 (`DRAWSTATUS@0x1635`). |
| `INIT2@0x16f` | `item_kind_lists` `G[0xbe64ad + kind]`, kinds 1 to 10 = 10, 3, 4, 2, 9, 5, 6, 7, 8, 1 | The item list each kind is filed in; 0, every other kind, is in none. INFOIT and INFOIT2 gather their lists by it, SHOP sorts by it, and GAMESTART carries only an item whose kind has a list. |
| `INIT2@0x174`..`0x1ab` | `scene_graphic_element_numbers` `G[0xcf312]` … `G[0xcf31d]` = 121000, 121020, 121100, 121120, 121140, …, 121260, 122000 | The graphic element numbers of a scene's layers: every scene draws and erases through them (`gfx-blit-color (global-int 0xcf312)`, `gfx-elem-erase (global-int 0xcf312) 0xa`, in 192 scripts). |
| `INIT2@0x1b0`..`0x1f1` | `GF[0x11]` … `GF[0x1c]`; `G[0xe7a1d]` … +3 = 255, `G[0xe7a21]` … +3 = `0xffffff` | A quad's vertices: four z of 0, the u (0, 1, 0, 1) and v (0, 0, 1, 1), and four alphas and colours. 367 scripts draw through them (`create-polygon … (global-float 0x9) (global-float 0xd) (global-float 0x11) (global-int 0xe7a1d) (global-int 0xe7a21) …`), with their own x and y in `GF[0x9]` and `GF[0xd]`. A save keeps no float but the first. |
| `INIT2@0x1f6`..`0x200` | `G[0xc09f38]` = 20, `field_sprite_anchor_x` `G[0xc09f36]` = 36, `field_sprite_anchor_y` `G[0xc09f37]` = 80 | The field draws a character's sprite at its position less the anchor (`DRAWCH`, `DRAWWALK`, `CARRY@0xa36`, `@0xa44`). FIELD copies `G[0xc09f38]` into a local (`FIELD@0x3103`); not decoded. |
| `INIT2@0x205`..`0x22a` | `character_stat_caps` `G[0xf45c29]` … +21 = 99999, 999, 999, 9999, 9999, 99, 99, 9999 × 4, 99, 99, 9999 × 9 | The caps of the 22 computed stats `G[0xf45c13]`: SETCH and CALCLVUP cap each at its own (`SETCH@0x4af`..`0x4ec`, `CALCLVUP@0xe8`..`0x10b`), and SETCH caps a character's `G[0xf382bb]` cell at the first (`SETCH@0xb44`). Characters, below. |
| `INIT2@0x22f`, `0x236` | `equipment_preview_slots` `G[0xf45c97]` … +7 = −1 | Eight slots SETCH uses in place of a character's five equipment and three stone slots while one is 0 or more (`SETCH@0xa7`..`0x113`), set by CHMENU and DRAWITTIP and reset by SETCH's end (`SETCH@0xcda`): likely the equipment screen's preview (items, below). |
| `INIT2@0x23e`..`0x2a2` | for each of the 1000 colours: if `G[0x6e68 + i]` < 0 and CVINIT's `G[0x157276 + i]` > 0, copy it and store it | A colour the shared profile left unset (INITCONFIG4's −1) takes CVINIT's once a table gives it one, the appends' settings included. |
| `INIT2@0x2a5`..`0x2c2` | if `current_cg_id` `G[0x3b1e]`: clear the CG parts and walk the CG part chain (`@0x34d`..`0x39e`); then, if the scene's graphic object `G[0xcf30e]` exists, compose the parts (`@0x3a0`..`0x419`) | On a load inside an event CG, INIT2 rebuilds it from the saved selection (scenes and screens, below). |
| `INIT2@0x2c5`..`0x302` | if `scene_over_map` `G[0x3b1c]`: in the field (`G[0x4dd6f]`), place the camera (`@0x41b`), run SETMAPDATA, SETMAPTEX, SETFLOORTEX, DRAWFIELD (slot 0x1e) and DRAWCH; otherwise SETORNDATA, SETTOWNTEX and SETESTTEX | On a load inside a scene played over the field or the town, INIT2 rebuilds what lies under it from the saved state (the dungeon and the town, below). On a cold boot the cells are 0. |
| `INIT2@0x305`..`0x343` | each of `item_counts` `G[0x770a]` … +19999 below 0 is set to 0 | The items held, by item id (`ADDITEM@0x61`; items, below), saved; a negative one is cleared on every boot and load. |
| `INIT2@0x346` | `G[0xcf2e7]` = 1 | Set at the end of every boot and load, above the prefix; while it is set, FIELD starts the floor's music again (`FIELD@0x6f7`), and the MP scripts test it too. |

### The appends' drivers (`$1$AUTORUN`, `$2$AUTORUN`)

Op `0x143` runs the AUTORUN of each mounted append, in selector order. La DEA's install has two, selector 1
(`APPEND01`) and selector 2 (`APPEND02`), so it runs `$1$AUTORUN` and then `$2$AUTORUN`. Each:

1. Calls its 34 fragments in INIT2's order (`@0x0`..`0x63`), with no test before any of them.
2. Puts its own scripts and pictures in its slot of the hook chains (`$1$AUTORUN@0x66`..`0xd4`,
   `$2$AUTORUN@0x66`..`0xd9`, below).
3. Sets its bit of `installed_appends_mask` `G[0xf45cac]` (`bit-set (global-int 0xf45cac) 0x1`, `$1$AUTORUN@0xd9`;
   `0x2`, `$2$AUTORUN@0xde`).

`installed_appends_mask` records which appends are mounted. GAMESTART loads the shared profile's copy over it and
compares the two bit by bit for the 32 appends; when one the profile had is missing, it clears `G[0x6e3d]`, and then
it puts the mounted mask back (`GAMESTART@0x18`..`0x7b`). SAVE copies it into the slot's `G[0xdf8 + slot]` and stores
that in the shared profile (`SAVE@0x1065`..`0x1078`), and marks a slot whose copy has a bit the mounted mask lacks
(`SAVE@0x1a55`..`0x1a86`). The cell lies above the saved prefix, so the drivers set it again on every boot and load.

### Hook chains

INIT2 and the appends fill arrays of 32 slots, one slot per layer: slot 0 is the base game's entry and slot n append
n's. A walker goes through the slots and runs (or, for a picture chain, loads) every nonzero one, so the order is
**base, `$1$`, `$2$`**. A chain of one script is `int[32]`, and the walkers index it with `G[0xcf308]`, `G[0xcf36e]`
or a local. Three chains hold several scripts a slot, as `[32][k]` arrays read with `lookup-array-2d` (slot, stride
k, which script): the slot's scripts lie side by side, so `$1$`'s CHECKSS1 is at `G[0xf45e6d + 4]`.

| chain | slot 0 (`INIT2`) | slot 1 (`$1$AUTORUN`) | slot 2 (`$2$AUTORUN`) | walked by |
|---|---|---|---|---|
| `title_picture_chain` `G[0xf45cad]` `int[32]` | none | `$1$SO030.AGF` | `$2$SO030.AGF` | `TITLE@0xaaa`, `@0xd95`: each set slot's picture as texture 0x34 + slot, drawn on the title |
| `scjump_chain_script_ids` `G[0xf45ccd]` `int[32]` | SCJUMP | `$1$SCJUMP` | `$2$SCJUMP` | `BTL@0x86`, `FIELD@0x2d9`, `TOWN@0xf1`: the scene dispatch ("Scenes and screens" below) |
| `start_chain_script_ids` `G[0xf45ced]` `[32][3]` | CLOSE, START, READY | `$1$CLOSE`, `$1$START`, `$1$READY` | `$2$CLOSE`, `$2$START`, `$2$READY` | `FIELD@0x189`, `TOWN@0x56` |
| `evinit_chain_script_ids` `G[0xf45d4d]` `int[32]` | EVINIT | `$1$EVINIT` | `$2$EVINIT` | INIT2 (`@0x383`), CGMODE, ALCHEMY, BREED, IMPROVE and the scenes (the prelude's `load_cg_parts`, `SC0000@0x2841e`) |
| `coinit_chain_script_ids` `G[0xf45d6d]` `int[32]` | COINIT | `$1$COINIT` | `$2$COINIT` | BTLMAIN, CHMENU (`@0x1f48`), FIELD, SAVE, SELACT, SETENP and 12 more screens |
| `orinit_chain_script_ids` `G[0xf45d8d]` `int[32]` | ORINIT | `$1$ORINIT` | `$2$ORINIT` | `SETORNDATA@0xb7` |
| `shinit_chain_script_ids` `G[0xf45dad]` `int[32]` | SHINIT | `$1$SHINIT` | `$2$SHINIT` | `SHOP@0xd93` |
| `flinit_chain_script_ids` `G[0xf45dcd]` `int[32]` | FLINIT | `$1$FLINIT` | `$2$FLINIT` | `FIELD@0x399` |
| `eninit_chain_script_ids` `G[0xf45ded]` `int[32]` | ENINIT | `$1$ENINIT` | `$2$ENINIT` | `FIELD@0x684`, MPEV091 and ten MP scripts |
| `esinit_chain_script_ids` `G[0xf45e0d]` `int[32]` | ESINIT | `$1$ESINIT` | `$2$ESINIT` | `SETENP@0x51e` |
| `duinit_chain_script_ids` `G[0xf45e2d]` `int[32]` | DUINIT | `$1$DUINIT` | `$2$DUINIT` | `BTLMAIN@0xd5`, `FIELD@0x3da1` |
| `btaninit_chain_script_ids` `G[0xf45e4d]` `int[32]` | BTANINIT | `$1$BTANINIT` | `$2$BTANINIT` | `SETEFFECT@0x101` |
| `checkss_chain_script_ids` `G[0xf45e6d]` `[32][4]` | CHECKSS1 … CHECKSS4 | `$1$CHECKSS1` … `$1$CHECKSS4` | `$2$CHECKSS1` … `$2$CHECKSS4` | `CHECKSS@0x2c8` |
| `setss_chain_script_ids` `G[0xf45eed]` `[32][2]` | SETSSBONUS, SETSSDAMAGE | `$1$SETSSBONUS`, `$1$SETSSDAMAGE` | `$2$SETSSBONUS`, `$2$SETSSDAMAGE` | `CALCSS@0x68`, `COMMITSS@0x1c5` |
| `tumes_chain_script_ids` `G[0xf45f2d]` `int[32]` | TUMES | `$1$TUMES` | `$2$TUMES` | `TUTORIAL@0x75c` |
| `cg_page_sheets` `G[0xf6231]` | SO023A … SO023H (8 sheets) | `$1$SO023K` at `G[0xf623b]` | `$2$SO023L` at `G[0xf623c]` | `CGMODE@0xb49`: the gallery's thumbnail sheets, one per page; `G[0xf6239]` and `G[0xf623a]` (I and J) are not written |
| `replay_page_sheets` `G[0xf6335]` | SO024A … SO024D (4 sheets) | `$1$SO024K` at `G[0xf633f]` | `$2$SO024L` at `G[0xf6340]` | `HMODE@0x9ff`: the replay's thumbnail sheets |

The fragments' names in slots 1 and 2 are the call-script names of the ids the drivers write (`$1$EVINIT` is
`0x10000d2`, and so on); a slot an append leaves empty stays 0. INIT2 also puts the town's screens in
`G[0xbf0164]` … `G[0xbf0169]` (INN, SHOP, ALCHEMY, IMPROVE, BREED, QUEST; `INIT2@0xca`..`0xe3`), and `$2$AUTORUN` adds
`$2$ARENA` at `G[0xbf016a]` (`$2$AUTORUN@0xca`): a list by facility kind, 1 to 7, which DEAL runs as
`G[0xbf0163 + kind]` (`DEAL@0x0`; the town, below), rather than a chain.

### What is saved

- **Saved:** the options above (the shared profile keeps them too), `G[0x5]`, INIT's cells, and the item cells
  `G[0x770a]` …, which INIT2 clears of negative values on every load.
- **Rebuilt at boot:** the hook chains, the town's screens, `installed_appends_mask`, `sound_channels`,
  `scene_graphic_element_numbers`, the quad, the sprite anchor, the stat caps, `equipment_preview_slots`, the rank,
  attitude and sex names and `G[0xcf2e7]`, all above the prefix.

## Scenes and screens

The game alternates between scenes and four play screens: `SYSTEM4` loops on `system_mode` `G[0x0]` and plays a scene
for 1. The scenes are SCINIT's; the screens offer them through the `SCJUMP` chain, and the map events (the `MPEV`
scripts) hand their own over. The event CGs a scene shows are composed from EVINIT's part sets, and the title's extras
room shows the CG gallery (CGINIT), the scene replays (SPINIT) and the music room (MUINIT). The tutorials (TUINIT,
TUMES) are shown by the town and the dungeon. Every table here is built at boot (`INIT2@0x0`, `@0x48`..`0x4e`,
`@0x60`, and the same offsets in `$1$AUTORUN` and `$2$AUTORUN`). EVINIT, TUMES and SCJUMP are hook chains whose slot 0
INIT2 sets (`INIT2@0x66`, `@0x93`, `@0x98`) and whose slots 1 and 2 the appends set (`$1$AUTORUN@0x66`, `@0x93`,
`@0x98`; `$2$AUTORUN` the same): EVINIT `G[0xf45d4d]`, TUMES `G[0xf45f2d]`, SCJUMP `G[0xf45ccd]`.

### The mode loop and the scene pass (SYSTEM4)

`SYSTEM4` loops on `system_mode` (`SYSTEM4@0x2de`..`0x6be`): 1 a scene, 2 `TOWN` (`@0x673`), 3 `FIELD`, the dungeon
(`@0x687`), 4 `BTL` (`@0x69b`), 5 `DEAL` (`@0x6af`). Any other value shows 不正な実行モード and asks to exit
(`@0x6b5`..`0x6bd`). A screen hands a scene over with the same five writes everywhere (`TOWN@0x1410`..`0x1449`,
`BTL@0x4fc`..`0x531`, `GAMESTART@0x138b`..`0x13b9`, `MPEV011@0x815`..`0x83a`): `system_base_mode` `G[0x2]` = the
mode, unless it is 1; `system_mode` 1; `system_next_mode` `G[0x1]` and `system_after_battle_mode` `G[0x3]` cleared or
set; `current_scene_id` `G[0xe7a0f]` the scene; and `scjump_context_id` `G[0x3a4b]` the scene's
`scene_offer_contexts` cell (a map event sets 0 instead and copies `scene_on_map_flags` into `scene_over_map`
`G[0x3b1c]`). `system_next_mode` is the mode after the scene: 3 from a map event (`MPEV011@0x81a`), 2 from a town
visit (`TOWN@0x144e`), 4 from a battle's result scene (`BTL@0x536`). `system_after_battle_mode` is the mode a battle
returns to: `BTL@0x54a`..`0x568` sets `system_mode` to it when it is set (the map events and FIELD set 3 when they start
a battle, `MPEV014@0x16d`, `FIELD@0x3d32`), else to 3 when `G[0x4dd6f]` is set and to 2 otherwise. No script reads
`system_base_mode`.

A scene pass (`SYSTEM4@0x2e5`..`0x665`):

1. Unless `scene_run_mode` `G[0xf45f4d]` is set, `current_scene_script_id` `G[0xed0]` =
   `scene_script_ids[current_scene_id]` (`@0x2f3`..`0x308`), `SC0000` (0x34) where that is 0 (`@0x30d`..`0x31b`).
2. With `scene_over_map` set it erases only the ADV range `0x1d8a8`..`0x29810` (`@0x34d`..`0x360`); otherwise it clears
   the retained objects and draws the black backdrop.
3. `scene_window_layout` `G[0xed4]` is 1 in the replay room (`replay_room_active` `G[0xcf2f8]`) and 0 otherwise, and
   `label_6d4` sets the text origins for each (`@0x45a`..`0x471`). Then it calls the script (`@0x4c1`).
4. Back from it, it erases the same way (`@0x524`..`0x53a`), clears `current_cg_id`, `current_cg_variant` and
   `current_cg_part_set` (`@0x5c0`..`0x5ca`) and, unless `scene_run_mode`, `debug_play_active` `G[0xf45f4f]`,
   `G[0xf45f4e]` or `replay_room_active` is set, sets `scene_seen_flags[current_scene_id]` for a number 0 to 9999 and
   stores it in the shared profile (`@0x5e8`..`0x64f`).
5. It calls `SETFATE` (`@0x653`), then `system_mode` takes `system_next_mode` if that is set (`@0x656`..`0x65d`).

Unlike 魔導巧殻's, the pass does not run the dispatch again after a scene: the screen the scene returns to does.
`G[0xf45f4e]` is never written.

The run switches:

| cell | name | set by | what it changes |
|---|---|---|---|
| `G[0xf45f4d]` | `scene_run_mode` | 1 `TITLE@0x57b`, the debug scene jump (script from the shared profile, `@0x58a`..`0x596`); -1 `TITLE@0x5a6`, `DEBUGADV2`; -1 `HMODE@0xc36` for a replay's setup, 0 at `@0xdde` for its call | 1 makes a scene script loop on a label picked with `coroutine-label-yield "LABEL" "J"` (`SC0000@0x9f1`..`0xa2f`); while set the pass keeps `current_scene_script_id` and marks nothing seen |
| `G[0xcf2f8]` | `replay_room_active` | `HMODE@0x0` 1, `HMODE@0x1594` 0 | the replay layout; nothing marked seen; tested by the scene header (`SC0000@0xc3`, `@0x2c7`) |
| `G[0xf45f4f]` | `debug_play_active` | `DEBUGFIELD`, `DEBUGFIELD2`, `DEBUGFIELD3`, `DEBUGTOWN` `@0x0` | TOWN skips the chain (`TOWN@0xc6`), TUTORIAL shows nothing (`TUTORIAL@0x7`), nothing marked seen |

### SCINIT (scenes)

Leader `G[0xe7d7f]`, capacity 10000. 787 scene ids are set, 0 to 9031: 756 by the base table (86 in 0xxx, 299 in
1xxx, 12 in 2xxx, 327 in 3xxx, 14 in 4xxx, 18 in 9xxx), 10 by `$1$SCINIT` (5000, 5010 to 5018) and 21 by `$2$SCINIT`
(5100 to 5120). Scene 0 is the prologue. One scene script holds ten numbers: the base table names 133 scripts,
`$1$SCINIT` two (`$1$SC5000`, `$1$SC5010`) and `$2$SCINIT` three (`$2$SC5100` to `$2$SC5120`). No fragment writes a
base id.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xe7d7f]` | 1 |  | 787 | 52..33554511, 138 distinct | `scene_script_ids`: the script that plays the scene, a packed script id (52 is `SC0000.BIN`; an append's scenes carry its selector in the top byte). The scene pass copies it into `current_scene_script_id` (`SYSTEM4@0x301`) |
| `G[0xea48f]` | 1 |  | 457 | 1..9, 9 distinct | `scene_offer_contexts`: the SCJUMP context whose rules offer the scene (below). 411 of the chain's 436 rules offer a scene in the context its cell gives. The handovers copy it into `scjump_context_id` (`TOWN@0x1442`, `BTL@0x52a`, `GAMESTART@0x13b2`, `$2$ARENA@0x769`), so the screen after the scene runs the next context (`FIELD@0x7c2`: 8 becomes 9) |
| `G[0xecb9f]` | 1 |  | 0 |  | `scene_visit_ranks`: not written. The visit rules compare it, the scene in a slot against the new one (`SCJUMP@0x2450`..`0x245e`): with every cell 0, a slot keeps the first scene a rule puts in it |
| `G[0xef2af]` | 1 |  | 215 | 1..1, 1 distinct 1×215 | `scene_on_map_flags`: 1 on a scene played over the map (156 are 迷宮MAPイベント). The map events and FIELD copy it into `scene_over_map` (`MPEV011@0x82e`..`0x835`, `FIELD@0x2937`; 155 scripts read it) |
| `G[0xf19bf]` | 1 |  | 301 | 1..12, 5 distinct 1×264, 2×22, 7×11, 8×2, 12×2 | `scene_town_marks`: the mark TOWN shows on a place while the scene is on offer there: `TOWN@0x1b6c`..`0x1b9b` sets entry value − 1 (entry 0 for a 0 cell) of the place's 20 mark kinds. The kinds, read from the scenes' comments: 1 an event, 2 an H scene (all 22 are ユニットADVイベント H scenes), 7 a request or a hint, 8 翌日へ, 12 a 街マップ会話 |

SCINIT writes 849 cells into the 756 script cells, 507 into 436 offer cells: 66 and 49 cells are written twice, and 22
offer cells get different values (scene 13 gets 1, then 4); the last write stands, and it is the context of the rule
that offers the scene. The 330 scenes with no offer cell are the map events' own (171 of them play over the map; the
others are 迷宮ADVイベント the `MPEV` scripts hand over).

The scene scripts' comments name each scene. A scene's branch starts by setting its played cell and then has a
comment `▼Gnnnn　<kind>　<title>`: `SC0000@0xdfe` sets `G[0xee7]` and reads `▼G0000　街開始ADVイベント　プロローグ`.
1350 of the 1362 branches start so; the other 12 are repeatable map events, or set the cell later (`SC3410@0x157f`,
`SC3160@0x1ff7`, `SC3550@0xdfe`). The kinds line up with the offer cell:

| context | comment kind | scenes |
|---|---|---|
| 1 | 街開始ADVイベント, entering the town | 53 |
| 2 | 迷宮開始, entering a dungeon | 20 |
| 3 | フロア移動, a new floor | 30 |
| 4 | ユニットADVイベント, a town visit | 329 |
| 5 | 飛翔の耳飾り | 5 |
| 6 | 戦闘前MAP, a battle's setup | 4 |
| 7 | 戦闘後ADV, a battle's start (the result scene) | 6 |
| 8 | 迷宮ADV and 戦闘後, a battle's end | 4 |
| 9 | 戦闘後MAP, back on the map | 6 |

575 branches name a number SCINIT does not list, so the pass would play `SC0000` for it; 569 of them have an empty
title (`▼G0005　　`), and the other six are two 【没】 scenes (`SC3530`, 3533 and 3534) and four battle scenes of
`SC9030` (9032 to 9035).

A scene's chapter opening, the first scene of each block of ten from 0 to 100 (序章 to 終章), sets `chapter_label`
`GS[0x348]`, `chapter_title` `GS[0x349]` and `chapter_number` `G[0x3b1d]` (`SC0000@0xe03`..`0xe0d` 序章, 水の導き, 0;
`SC0010@0xe03`..`0xe0d` 第一章, 仄暗き廃坑の中へ, 1; `SC0100@0xe07`..`0xe11` 終章, 戦う者たちへの挽歌, 10). `SAVE`
copies the two strings into a slot's caption (`SAVE@0xf62`, `@0xf78`); TOWN compares the number with a place's first
and last chapter (`TOWN@0x1ebf`, `@0x1eeb`, the tables of "The town" `G[0xbf0bfa]`, `G[0xbf0f7e]`); `GAMESTART@0x1385` sets 0.

### SCJUMP (the scene dispatch)

`SCJUMP` is slot 0 of a 32-slot chain whose slots 1 and 2 are `$1$SCJUMP` (0x10000db) and `$2$SCJUMP` (0x200002c).
`TOWN`, `FIELD` and `BTL` walk it with the index `G[0xcf36e]` (`TOWN@0xf1`, `FIELD@0x2d9`, `BTL@0x86`). Before each
walk the caller sets `scjump_context_id`, opens `scjump_chain_pending` `G[0xe7a11]` (0x7fffffff), copies a mode other
than 1 into `system_base_mode` and runs `SETADVFLAG`, which clears `town_visit_scenes` to -1 when the context is 4
(`SETADVFLAG@0x0`..`0x1c`). `SCJUMP@0x0`..`0x7e` jumps on the context to its rule list:

| context | set by | rules |
|---|---|---|
| 1 | `TOWN@0xa6`, entering the town (not when `G[0x4dd62]` is 2) | 49: the chapter openings 第一章開始 (10) to 終章開始 (100), the epilogues |
| 2 | `FIELD@0x28e`, entering a dungeon (the previous map `G[0xb8464]` is 0) | 30, and one of `$2$SCJUMP` (5111) |
| 3 | `FIELD@0x2f7`, a floor change | 22 |
| 4 | `TOWN@0x192a`, gathering the town's visits | 294 visit rules, and 8 each of `$1$SCJUMP` and `$2$SCJUMP` |
| 5 | `FIELD@0x1791`, after `SBUNKI`'s 飛翔の耳飾り choice | 2 |
| 6 | `FIELD@0x3dbc`, `@0x3fea`, after the DUINIT chain sets a battle up | 5 |
| 7 | `BTL@0x3b`, a battle's first entry | 7 |
| 8 | `BTL@0x471`, after `BTLMAIN` | 5 |
| 9 | `FIELD@0x7d0`, back on the map when the context was 8 | 5 |

A rule of every context but 4 takes the turn: `if (condition) { if (scjump_chain_pending > 0) { system_mode = 1;
current_scene_id = N; scjump_chain_pending = 0 } }` (`SCJUMP@0x81`..`0xc3`), so the first rule whose condition holds
wins and every later one, in this slot or the next, finds the dispatch closed. A visit rule of context 4 fills a slot
instead: if the slot is below 0 it takes the scene, else `G[0xe7a10]` = the slot's scene and the slot takes the new
one when `scene_visit_ranks` of the old is higher (`SCJUMP@0x2435`..`0x2465`).

The slots are `town_visit_scenes` `G[0xcf36f + 100 × row + place]`, int[1000][100], which ends right before
`current_scene_id`. Most rows are character ids, read from the scenes' comments (3 シュリ, 4 サリア, 5 ロカ, 6 グロック,
7 クーン, 8 リンシャ, 11 アリシア, 18 ヨルマ, 23 カウラ, 27 ニウ＆ラクチェ, 31 レヴィア …); rows 200, 296 to 299 and 997 to
999 hold other offers (297 to 299 シュリ's 調合, 錬成 and 配合 scenes; 998 two 迷宮ADVイベント in column 1). The columns
are BLINIT's places, 1 to 20 (2 野牛の蹄亭, 3 自由市場, 4 礼拝所, 5 衛兵詰所, 6 領主の館, 7 酒場, 8 アリシアの家, 9 広場,
10 麝香商会, 12 路地裏, 13 倉庫街裏, 14 アリシアの店, 17 キャラバン; 20 only in `$2$SCJUMP`, the 闘技場 scenes). TOWN
plays the slot of the character the player picks at the place: row from `G[0xbf5e66 + n]`, column from
`G[0xbf074a + G[0x4d2d1]]` (`TOWN@0x13df`..`0x1449`), and draws the slot's `scene_town_marks` meanwhile.

What the rules test. SCJUMP's 419 rules make 2579 tests, 1874 of them on 483 cells of `scene_played_flags`. The others:

| cell | tests | what the scripts show |
|---|---|---|
| `G[0x1e8a]` | 409 | `ending_scene_played`, scene 4003's played cell (▼G4003 シュヴェルトライテ戦後, the scene that calls `ED`, `SC4000@0x19ae9`): 405 rules ask that it is not 1, so nothing more is offered after the ending; four context 1 rules ask that it is, the epilogues 103 アビルースエンド, 104 エピローグ and 105 ローレンエピローグ, each behind its own flag (7100, 7101, 7102, set by scenes 3715, 3915 and 4002: `SC3710@0x55de`, `SC3910@0x186fb`, `SC4000@0x120b5`), and 101 リンシャ＆カウラ when none of the three is set (`SCJUMP@0xec8`..`0x1071`). Every rule of `$1$SCJUMP` and `$2$SCJUMP` asks that it is not 1 |
| `G[0x7259]` | 299 | `== 1`: INIT sets 1 (`INIT@0x14`), `CHANGELEADER@0x20` copies `G[0x158dcf]` into it (the party leader, "Characters, voices, fates and the books", inferred) |
| `G[0x4d2cc]` | 294 | `== 0` on every visit rule; TOWN uses it as a bit index into place tables (`TOWN@0x1f17`) ("The town") |
| `G[0xb8463]` | 55 | the map in play ("The dungeon"); FIELD compares it with the previous map `G[0xb8464]` (`FIELD@0x272`) |
| `G[0x7250]` | 14 | INIT sets 300 with a maximum `G[0x7251]` of 400 (`INIT@0x19`); `BTL@0x26d`..`0x29f` takes 200 when `G[0xc20ad]` is 1 ("Skills, conditions and battle", "Encounters and enemies") |
| `G[0x4d2cd]`, `G[0x4d2ce]` | | INIT sets 10 each, `DEBUGTOWN@0x5` 20 each, `TOWN@0x2536` adds them ("The town") |
| `G[0xc20a6]`..`G[0xc20a8]`, `G[0xc20ad]` | | the battle in play and its result ("Skills, conditions and battle", "Encounters and enemies") |

`$1$SCJUMP` has 8 visit rules (scenes 5000 to 5018) and `$2$SCJUMP` 9 (5100 to 5115). `tools/scjump_decode.py` reads
only Himegari's chain; the counts here come from matching the rules' instruction pattern.

### The played and seen flags

- `scene_played_flags` `G[0xee7 + number]`, int[10000], below the prefix: each scene's branch sets its cell as it
  starts. The numbers SCINIT has no scene for are the game's event flags: 7001 to 7133 and 8051 to 8999, set by
  scenes, map events and the `CLOSE` hook (which sets 64 of the numbers), and by `OPENFACT` and `RECEIPT`, which take a flag from
  FAINIT's and QUINIT's tables as number + 1 (`OPENFACT@0x2ec`..`0x301`, `RECEIPT@0x889`..`0x89e`); `QUEST` and
  `TUTORIAL` test them the same way (`QUEST@0xbce`..`0xbe7`). These are the game's story flags.
- `scene_seen_flags` `G[0xc20af + number]`, int[10000], below the prefix and kept in the shared profile: set at a
  scene's end by the pass and by the scene script itself (`SC0000@0xcff`..`0xd66`). `INFO@0x706` loads all of them;
  `INFOCH` and `INFOVO` unlock their entries by scene number + 1 (`INFOCH@0x10cc`..`0x10e5` with CFINIT
  `G[0x100f8e]`, `INFOVO@0x11bd`..`0x11d6` with VIINIT `G[0x14a756]`, the tables of "Characters, voices, fates and the books").
- `cg_seen_part_sets` `G[0x3b21 + 10 × CG + variant − 1]`, int[1000][10], below the prefix and kept in the shared
  profile: a bit per part set (below).

### EVINIT (event CG parts)

A scene shows an event CG by setting `current_cg_id` `G[0x3b1e]`, `current_cg_variant` `G[0x3b1f]` (1 to 9) and
`current_cg_part_set` `G[0x3b20]`, after saving the previous pair in `previous_cg_id` `G[0xf40cf]` and
`previous_cg_variant` `G[0xf40d0]` (`SC0010@0x149c`..`0x14b0`, CG 61, variant 1, set 1). It then calls the ADV
prelude's `mark_cg_seen`, which loads the cell of `cg_seen_part_sets` from the shared profile, sets bit set − 1 and
stores it back (`SC0010@0x1155d`..`0x115be`); if the CG or the variant changed it calls `load_cg_parts`, which clears
the five arrays below and walks the EVINIT chain with the index `G[0xcf308]` (`INIT2@0x34d`..`0x39e` is the same
code); then it shows `cg_part_set_pictures[set]` (`SC0010@0x14dc`) and `compose_cg_parts` draws the set's parts. 118
scripts walk the chain: the scene and replay scripts, `CGMODE`, `ALCHEMY`, `BREED`, `IMPROVE` and `INIT2`, which
reloads and recomposes the CG on a load when `current_cg_id` is set (`INIT2@0x2a5`..`0x2c2`).

EVINIT branches on the pair (`EVINIT@0x0`..`0x15`): the base has 396 branches for 141 CGs (1 to 371), `$1$EVINIT` 6
for CGs 500 to 502 and `$2$EVINIT` 19 for 510 to 514. Each writes the CG's part sets, 1 to 16 at most:

| array | cells | written | holds |
|---|---|---|---|
| `G[0xf40d1 + set]` | int[32] | 1082 writes, sets up to 14 (16 in `$2$`) | `cg_part_set_pictures`: the set's base picture (`EVINIT@0x1c`) |
| `G[0xf40f1 + 5 × set + k]` | int[32][5] | 1155 | `cg_part_pictures`: up to five pictures drawn over it (`EVINIT@0xc5`) |
| `G[0xf4191 + 5 × set + k]` | int[32][5] | 1139 | `cg_part_x`: each part's x (`EVINIT@0xca`) |
| `G[0xf4231 + 5 × set + k]` | int[32][5] | 1124 | `cg_part_y`: each part's y (`EVINIT@0xcf`) |
| `G[0xf42d1 + set]` | int[32] | 578 | `cg_part_set_save_pictures`: the picture a save's thumbnail shows in place of the screen while the set is shown (`SAVE@0xda7`..`0xddd`) |

### CGINIT (the CG gallery)

Leader `G[0xf42f1]`, capacity 4000, group 200: an entry is `200 × page + 10 × thumbnail + k`, 20 pages of 20
thumbnails, each standing for up to ten CG and variant pairs (`CGMODE@0x9da`..`0xa11`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xf42f1]` | 1 |  | 419 | 1..514, 149 distinct | `cg_gallery_cg_ids`: the entry's CG id, which `CGMODE` copies into `current_cg_id` |
| `G[0xf5291]` | 1 |  | 419 | 1..9, 9 distinct | `cg_gallery_variants`: its variant, copied into `current_cg_variant` |

The base table fills pages 0 to 7 (45, 60, 51, 71, 60, 47, 59 and 1 entries; 141 thumbnails, at most 7 pairs to a
thumbnail), `$1$CGINIT` page 10 (6 entries, CGs 500 to 502) and `$2$CGINIT` page 11 (19 entries, 510 to 514).
`cg_page_sheets` `G[0xf6231 + page]` holds each page's thumbnail sheet: `SO023A.AGF` to `SO023H.AGF` for pages
0 to 7 (`INIT2@0xe8`..`0x10b`), `$1$SO023K.AGF` for page 10 (`$1$AUTORUN@0xca`) and `$2$SO023L.AGF` for page 11
(`$2$AUTORUN@0xcf`); `CGMODE` shows a tab for each page that has one (`CGMODE@0xb3b`..`0xb6a`).

`CGMODE` counts, for every entry whose CG and variant are set, the part sets EVINIT gives a base picture (sets 1 to
31), and those whose bit is set in `cg_seen_part_sets`, and shows seen × 100 / total (`CGMODE@0xa6b`..`0xb1f`). When
`G[0xf45f50]` is set it sets every bit first (`CGMODE@0xa32`..`0xa4b`); no script writes that cell.

### The extras room (ROOM)

`ROOM` runs one page a pass by `extras_room_page` `G[0xbdc78a]`: 0 `CGMODE`, 1 `HMODE`, 2 `MMODE`, 3 `INFO`
(`ROOM@0x1b`..`0x4d`), plays BGM 30 except on the music page (`ROOM@0x7`..`0x15`), and leaves when the page is below
0 with `exit-script`, the soft reset (`ROOM@0x4d`..`0x78`).

### SPINIT (scene replays)

Leader `G[0xf6245]`, capacity 240, group 12: `replay_scripts` `G[0xf6245 + 12 × page + slot]`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xf6245]` | 1 |  | 50 | 442..33555729, 50 distinct | `replay_scripts`: the script that replays the scene (`SP0002.BIN`, `SP1146.BIN` …) |

The base table fills pages 0 to 3 (12, 12, 12 and 10 slots), `$1$SPINIT` page 10 slot 0 (`$1$SP5016`) and `$2$SPINIT`
page 11 slots 0 to 2 (`$2$SP5105`, `$2$SP5108`, `$2$SP5119`). `HMODE` offers a slot once the catalog marks its script
seen (`HMODE@0x97a`..`0x997`) and plays it with `call-script` (`HMODE@0xde3`..`0xdee`), with `scene_run_mode` -1 for the
setup and 0 for the call (`HMODE@0xc36`, `@0xdde`). `replay_page_sheets` `G[0xf6335 + page]` holds `SO024A.AGF` to
`SO024D.AGF` for pages 0 to 3 (`INIT2@0x110`..`0x11f`), `$1$SO024K.AGF` and `$2$SO024L.AGF` for pages 10 and 11
(`$1$AUTORUN@0xcf`, `$2$AUTORUN@0xd4`; `HMODE@0x9ff`).

### MUINIT (the music room)

Leader `G[0xf6349]`, capacity 50. 36 tracks, ids 1 to 36; `$1$MUINIT` and `$2$MUINIT` write nothing.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xf6349]` | 1 |  | 36 | 2..23632, 35 distinct | `music_unlock_resources`: what the catalog must mark seen for the track (`MMODE@0xab2`..`0xac7`): the track's `BGMnnn.OGG`, or the script that plays the song (track 1 and 36 `ED.BIN`, 2 `OP.BIN`) |
| `G[0xf637b]` | 1 |  | 36 | 2..57, 36 distinct | `music_bgm_numbers`: the BGM number `play-bgm` plays (`MMODE@0x7c2`..`0x7c9`) |
| `GS[0x50a]` | 1 |  | 36 | 『La eterno』 / 『La eterno』 short. / 『女神の縁、再び……』 | `music_titles`: the title. No script reads it; `MMODE` draws its screen from `SO025.AGF` (`MMODE@0xb0d`) |

`MMODE` shows unlocked × 100 / tracks (`MMODE@0xa81`..`0xafe`), and tests three movies, `OP.MPG`, `ED.MPG` and
`EDB.MPG`, the same way (`MMODE@0xa2c`..`0xa79`).

### TUINIT (tutorials)

Leader `G[0x154396]`, capacity 1000. 73 tutorials, ids 10 to 910: 72 from the base table and one from `$2$TUINIT`
(910, 『闘技場』の説明); `$1$TUINIT` writes nothing.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x154396]` | 1 |  | 73 | 0..2, 2 distinct 0×72, 2×1 | `tutorial_text_chain_slots`: the TUMES chain slot holding the text, 0 the base `TUMES`, 2 `$2$TUMES` (`TUTORIAL@0x755`..`0x778`) |
| `G[0x15477e]` | 1 |  | 73 | 1..3, 3 distinct 1×18, 2×18, 3×37 | `tutorial_screens`: 1 the town, 2 the dungeon, 3 only the information screen's list (`TUTORIAL@0x4a1`; `INFOTU@0x94f`..`0x96b` lists the 37) |
| `G[0x154b66]` | 2 |  | 0 |  | `tutorial_required_tutorials`: not written; tutorials that must have been shown (`TUTORIAL@0x4fb`..`0x52b`) |
| `G[0x155336]` | 2 | 0..1 | 25 | 21..261, 26 distinct | `tutorial_excluding_tutorials`: tutorials whose showing rules this one out (`TUTORIAL@0x553`..`0x583`): the town's and the dungeon's copies of one explanation name each other (70 and 80) |
| `G[0x155b06]` | 3 | 0 | 35 | 1051..9000, 26 distinct | `tutorial_required_scenes`: numbers + 1 whose `scene_played_flags` must be set (`TUTORIAL@0x5ab`..`0x5dd`): 3001 is scene 3000, 8928 flag 8927 |
| `G[0x1566be]` | 3 | 0 | 4 | 8955..8960, 2 distinct 8955×2, 8960×2 | `tutorial_forbidden_scenes`: numbers + 1 that must not be set (`TUTORIAL@0x605`..`0x637`); tutorials 240 to 243, two pairs of 習得技の説明 that rule out each other's flag |
| `GS[0xc0bc]` | 1 |  | 73 | 街の説明　イベント / 迷宮の説明　開始（黒狐岩） / 迷宮の説明　宝獲得（黒狐岩） | `tutorial_titles`: the title `INFOTU` lists (`INFOTU@0xeab`) |

The block ends at CVINIT's first array, `G[0x157276]`.

`TOWN` and `FIELD` call `TUTORIAL` with `tutorial_screen` `G[0xbeb339]` 1 (`TOWN@0x3de`, `@0x12dc`) or 2
(`FIELD@0xc28`, `@0x1b37`). Unless `debug_play_active` or `G[0x6e40]` is set (`TUTORIAL@0x7`), it takes the first
tutorial in id order whose screen matches, which `tutorial_shown_flags` `G[0x6a52 + id]` does not mark, and whose
conditions hold (`TUTORIAL@0x47f`..`0x647`), puts it in `tutorial_current` `G[0xbeb33a]`, shows its text from the
TUMES chain's slot, marks it shown (`TUTORIAL@0x9c2`..`0x9c9`) and looks for the next. `INFOTU` replays one with screen 0
(`INFOTU@0x7c2`..`0x7e8`). `G[0x6e40]` is toggled on `GAMESTART`'s 初期設定 page (`GAMESTART@0x674`); that it is the
tutorials' off switch is an inference, so it has no name here.

`TUMES` branches on `tutorial_current` (72 tests, `TUMES@0x0`) and on the page `G[0xbeb33b]`, which `TUTORIAL` moves
(`TUTORIAL@0x648`..`0x679`). `$1$TUMES` is empty; `$2$TUMES` holds tutorial 910.

### What is saved

- **Saved:** the mode loop's cells `system_mode`, `system_next_mode`, `system_base_mode`, `system_after_battle_mode`;
  `current_scene_script_id`, `scene_window_layout`, `scjump_context_id`, `scene_over_map`; the chapter's
  `chapter_number`, `chapter_label` and `chapter_title` (the two strings lie below `GS[0x34f]`); `current_cg_id`,
  `current_cg_variant`, `current_cg_part_set`, which INIT2 uses to rebuild the CG on a load; `scene_played_flags`
  with the event flags, and `tutorial_shown_flags`; and `scene_seen_flags` and `cg_seen_part_sets`, which are also kept
  in the shared profile, so they outlive a playthrough.
- **Not saved:** every table array (SCINIT, CGINIT, SPINIT, MUINIT, TUINIT, EVINIT's part arrays, the sheets), above
  both prefixes and rebuilt at every boot and load, so a change to them reaches old saves; `current_scene_id` and
  `scjump_chain_pending`; `town_visit_scenes` (context 4 rebuilds it); `previous_cg_id` and `previous_cg_variant`;
  the run switches `scene_run_mode`, `replay_room_active`, `debug_play_active`; `tutorial_screen`,
  `tutorial_current` and `extras_room_page`. A load restores the scene by its frames, not by `current_scene_id`.

## Characters, voices, fates and the books

Six tables describe the people of the game and what its books show. INIT2 calls them in this order: CHINIT
`INIT2@0x3`, CVINIT `@0x51`, FTINIT `@0x54`, CIINIT `@0x57`, CFINIT `@0x5a` and VIINIT `@0x5d`. `$1$AUTORUN` and
`$2$AUTORUN` then call the appends' fragments at the same offsets. `$1$CVINIT`, `$1$CIINIT` and `$2$VIINIT` are a bare
`exit`. Checked against the executed image, the fragments of CHINIT, CVINIT, FTINIT, CIINIT and VIINIT only add
records: none writes a cell of a base record. CFINIT's fragments write base records, but only into form columns the
base leaves empty (see CFINIT).

| table | records | ids | capacity | leader | `$1$` adds | `$2$` adds |
|---|---|---|---|---|---|---|
| CHINIT (characters) | 368 (307 + 16 + 45) | 1..962 | 1000 | `G[0x15765f]` | ids 901..916 | 45 ids, 230..962 |
| CVINIT (voice settings) | 51 (50 + 1) | 0..50 | 1000 | `G[0x157276]` | none | id 50; character 231's setting |
| FTINIT (fates, 運命) | 151 (139 + 5 + 7) | 1..503 | 1000 | `G[0xf63ae]` | 5 ids, 145..342 | 7 ids, 20..26 |
| CIINIT (人物 profiles) | 47 (46 + 1) | 1..231 | 1000 | `G[0xfade6]` | none | id 231 |
| CFINIT (character forms) | 51 (46 + 3 + 2) | 1..231 | 1000 | `GS[0x5f14]`, 10 per id | forms of 3, 51 and 66 | forms of 2 and 231 |
| VIINIT (the 用語 glossary) | 101 (99 + 2) | 1..510 | 1000 | `G[0x14a36e]` | ids 240, 241 | none |

Every array of these tables lies above the saved prefix (`G[0xcf2e6]`, `GS[0x34f]`), so none of it is saved: INIT2
writes them again after a load restores the save. What a character carries is kept in separate arrays below the
prefix, keyed by character id 0..99, listed in "What is saved" below.

The six tables share one block of each bank, one capacity after another. In the integers: FTINIT `G[0xf63ae]` to
`G[0xfade5]`, CIINIT to `G[0x100f8d]`, CFINIT to `G[0x14a36d]`, VIINIT to `G[0x153fad]`, then TUINIT's ("Scenes and screens"), then
CVINIT's name colours `G[0x157276]`, one cell `G[0x15765e]`, CHINIT's leader `G[0x15765f]`, CVINIT's other four
arrays `G[0x157a47]` to `G[0x158dce]`, the three argument cells `G[0x158dcf]`..`G[0x158dd1]`, and CHINIT's other arrays
from `G[0x158dd2]` to `G[0x1effc9]`; `G[0x1effca]` after them is RTN's argument. In the strings: FTINIT
`GS[0x53c]`, `GS[0x924]` and an unwritten `GS[0xd0c:13]`; CIINIT from `GS[0x3fd4]`, CFINIT `GS[0x5f14]`, VIINIT
`GS[0x8624]` to `GS[0xc13b]`, TUINIT's, then CHINIT's name forms `GS[0xc4a4]`, CVINIT's labels `GS[0xd444]` and
CHINIT's other strings to `GS[0xf383]`. Every base agrees with its neighbours modulo the capacity.

The tool's trial leaders split four sparse arrays into pieces off the grid: CHINIT's skill ranks (stride 500), stats
(18) and growth (14), CIINIT's line conditions (18), CFINIT's form conditions (30) and gallery (250), and VIINIT's line
conditions (39) and lines (13). The tool's leaders declare them with `arrays=`; with them every table lays
out with 0 cells outside its arrays.

### CHINIT (characters)

Leader `G[0x15765f]`, capacity 1000. The record is the character id. `CHINIT@0x0` writes character 1's
`G[0x157660] = 1` first, then its name form `GS[0xc4a8]` (`@0x5`), full name `GS[0xdffd]` (`@0xa`), name `GS[0xe3e5]`
(`@0xf`), sort key (`@0x14`) and sex (`@0x19`); `@0x17c` starts character 2. Ids 1 to about 300 are the story and the
party, 301 and up enemies; an enemy form of a story character keeps its own id and names the person in its base
character. The tool reports no write ahead of a record.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x15765f]` | 1 |  | 368 | 1..962, 317 distinct | The base character: its own id for most, the story character for an enemy form (313..331 → 40..44, 391 → 51). The page reads the voice setting through it, `G[0x157a47][G[0x15765f][speaker]]` (`SC0000@0xf83`..`0xfa6`). |
| `G[0x158dd2]` | 1 |  | 366 | 10..10110, 362 distinct | The sort key: SETCHLIST (`SETCHLIST@0xa`), SETCHSEL, RESULT, INFOEN (`INFOEN@0x613`), SETSKLIST, SHOP, INFOSK2 and SORTSALLY sort by it. |
| `G[0x1591ba]` | 1 |  | 394 | 1..3: 1×110, 2×139, 3×145 | Sex: 1 男, 2 女, 3 無 (`INIT2@0x160`..`0x16a`). DRAWSTATUS draws it (`DRAWSTATUS@0x162e`); a speaker with no voice setting speaks with setting 2 when it is 2, else 1 (`SC0000@0xfae`..`0xfcb`). |
| `G[0x1595a2]` | 1 |  | 15 | 1 | 1 on the 15 characters that can join: 1, 3, 5, 23, 31, 32, 40, 46, 47, 50, 51, 53, 62, 66, 70. SETCHLIST's filter mode 1 keeps them (`SETCHLIST@0x90`); CHMENU tests it (`CHMENU@0x312c`). That they are the party is inferred from the list. |
| `G[0x15998a]` | 1 |  | 70 | 9001..9055, 54 distinct | The 【支援】 support action, an ITINIT id. DRAWSTATUS draws the support panel from it (`DRAWSTATUS@0x18a`); CHMENU (`@0x1680`), SETENP, BREED and BTLMAIN use it; SETCHLIST's mode 2 filters by it (`SETCHLIST@0xb3`). |
| `G[0x159d72]` | 1 |  | 15 | 1..7 | On the party characters: the column the character reads of the per-character item arrays ("Items, stones, improvement and alchemy") (`G[0x82ca77:0x3c]`, `G[0x9519f7:0x3c]`, `G[0x769577:0x14]`, `G[0x7caff7:0x14]`, `G[0x707af7:0x14]`): `SETCH@0x3e8`, `@0x40d`; DRAWITLIST, DRAWSTATUS, DRAWITTIP. |
| `G[0x15a15a]` | 1 |  | 340 | 1..5 | Width in formation cells. UNITECH places a joining character with it (`UNITECH@0x3e9`); DRAWSTATUS draws "w × h" (`DRAWSTATUS@0x496`); the unit's copy is `G[0xf450d3]`. |
| `G[0x15a542]` | 1 |  | 340 | 1..5 | Height in formation cells (`UNITECH@0x3be`); the unit's copy is `G[0xf450fb]`. |
| `G[0x15a92a]` | 1 |  | 129 | 1 | Boss: CALCILL skips the conditions `G[0x3286e1]` marks (即死, 喪意, 石化, 呪い1 … 恐怖1) for these (`CALCILL@0x4f4`); BTLMAIN, CHECKSS2, DRAWFRAME and INFOEN read it. |
| `G[0x15ad12]` | 1 |  | 340 | 2..15 | The affinity type, a row of AFINIT's `G[0xbd6277:0x1e][type][element]`. SETCH gives it to the unit (`G[0xf3885b]`, `SETCH@0x6c4`) when the slot-2 armour has no type of its own (`G[0x367f37]`); CALCDAMAGE uses the unit's. |
| `G[0x15b0fa]` | 1 |  | 340 | 1..16 | The race, a name `GS[0x1fc98 + k]` (人間 … 神格位 14, 魔神 15, 一般 16); DRAWSTATUS (`@0x1600`); INFOEN's filter (`INFOEN@0xb06`). |
| `G[0x15b4e2]` | 1 |  | 340 | 1..5: 1×47, 2×80, 3×183, 4×24, 5×6 | The temperament: 1 超強気, 2 強気, 3 普通, 4 弱気, 5 超弱気 (`INIT2@0x147`..`0x15b`). DRAWSTATUS (`@0x1617`); BTLMAIN reads it 13 times (`BTLMAIN@0x16fe`), CALCDAMAGE and LAUNCH too. |
| `G[0x15b8ca]` | 1 |  | 337 | 1..962, 321 distinct | The battle routine, usually the own id: when the unit's flags have 0x100, BTLMAIN sets `G[0x1effca]` to it and runs RTN (`BTLMAIN@0x1091`). |
| `G[0x15bcb2]` | 1 |  | 15 | 9501..9515 | The 追撃 follow-up action of the party characters, an ITINIT id (`BTLMAIN@0x1660`). |
| `G[0x15c09a]` | 6 | 0..5 | 322 | 201..9998, 405 distinct | An enemy's actions, ITINIT ids (`BTLMAIN@0x268e`; RTN, SETEN, INFOEN). |
| `G[0x15d80a]` | 500 | 1..217 | 312 | 1..5 | The innate rank of each skill. SETCH adds the saved learned ranks `G[0x25f10:500]` (`SETCH@0x762`, `@0x78a`), clamps to 0..5 and drops a rank whose name `GS[0xf384:5][skill][rank]` is empty; CALCREVISE, CHMENU, INFOEN. |
| `G[0x1d792a]` | 9 | 0..3 | 15 | 21..53, 23 distinct | Item kinds the party character starts trained in, the 必殺 and magic schools (names `GS[0x1fcfc + k]`). |
| `G[0x1d9c52]` | 9 | 0..3 | 15 | 1..5 | Their ranks. At the first join UNITECH adds each into the saved `G[0x3e5b0:0x50][ch][kind]` (`UNITECH@0x62`, `@0x84`, `@0x8f`). |
| `G[0x1dbf7a]` | 1 |  | 321 | 2001..9605, 18 distinct | Equipment slot 0: a weapon for a party character (セリカ 2301), a 96xx 攻撃上昇 boost for an enemy (9602). |
| `G[0x1dc362]` | 1 |  | 3 | 3201..3204 | Slot 1: a shield. |
| `G[0x1dc74a]` | 1 |  | 166 | 2801..9622, 17 distinct | Slot 2: armour, or the boost 9622. |
| `G[0x1dcb32]` | 1 |  | 306 | 9642, 9645 | Slot 3: 速度やら上昇 items, on enemies only. Not named. Slot 4, `G[0x1dcf1a]`, has no cells. At the first join with level 0 UNITECH writes the five into `G[0x404f0:5]` and runs ADDITEM with `G[0x328796] = 0` (`UNITECH@0xaf`, `@0xba`); otherwise they go to the inventory. SETEN equips an enemy from them (`SETEN@0x117`..`0x125`). |
| `G[0x1dd302]` | 1 |  | 15 | bitmasks | The item classes the character can equip, bit k for class `GS[0x1fd9c + k]`: セリカ's 0x2400040 is 剣, 中装 and 盾. DRAWSTATUS draws the weapon bits 1..20 and the armour bits 21..24 (`DRAWSTATUS@0x9fb`, `@0xa3a`); CHMENU (`@0x23a5`), SETITLIST and SHOP filter by it. |
| `G[0x1dd6ea]` | 1 |  | 337 | 1..255, 89 distinct | The base level: UNITECH sets the level to it at the first join (`UNITECH@0x39`); CALCLVUP grows the stats by the levels above it (`CALCLVUP@0x5`); INFOEN (`@0x1f1d`). |
| `G[0x1ddad2]` | 1 |  | 337 | 1..255, 24 distinct | The level cap. ADDEXP takes セリカ's (`G[0x1ddad3]`, 23) plus the saved bonus `G[0x1ffbb]` as the party's cap (`ADDEXP@0x38`), raises it to the character's own when that is higher (`@0x3f`), at most 250; enemies have 200. |
| `G[0x1ddeba]` | 1 |  | 322 | 1 | An enemy's level-scaling divisor: when `G[0x6e3a] >= 1`, SETEN adds (the leader's level `G[0x20790][G[0x7259]]` − the level) / it (`SETEN@0xda`). |
| `G[0x1de2a2]` | 1 |  | 337 | 12..200, 20 distinct | The initial morale (初期戦意). RECOVER, BTL and OPENFACT set the morale to it plus the saved bonus `G[0x1ffbc]` (`RECOVER@0x41`, `@0x48`; `OPENFACT@0x1d8`). |
| `G[0x1de68a]` | 1 |  | 337 | 24..200, 17 distinct | The maximum morale: the unit's `G[0xf3826b]`, saved through CALCREVISE in `G[0x208bc]`; CALCDAMAGE scales by it; INFOEN draws "a / b" (`INFOEN@0x1b9f`). |
| `G[0x1dea72]` | 1 |  | 336 | 1..10 | A unit counter, `G[0xf45b23]` / `G[0xf45afb]`, that SETENP, REVIVE and BTLMAIN count down. Not understood; not named. |
| `G[0x1dee5a]` | 18 | 0..17 | 337 | −20..1912500 | The base stats, columns below (`CALCLVUP@0xa1`). |
| `G[0x1e34aa]` | 14 | 0..13 | 327 | 1..85000 | Growth per level of the first 14 stat columns in hundredths: CALCLVUP adds growth × (level − base level) / 100 (`CALCLVUP@0x53`). |
| `G[0x1e7712]` | 2 | 0..1 | 315 | 48..220000, 153 distinct | An enemy's experience, two equal columns. BTLMAIN adds column `L[0x21]` to `G[0xef7d4d]` (`BTLMAIN@0x3a6b`); RESULT shares the sum among the party by the weights `G[0xef7d51]`, then runs ADDEXP. What chooses the column is not decoded. |
| `G[0x1e7ee2]` | 1 |  | 314 | 1..255, 73 distinct | Points an enemy gives: BTLMAIN adds them to `G[0xef7d4e]` (`BTLMAIN@0x3a84`), RESULT to `G[0x1fdab]`, a currency FACT spends ("The town"). |
| `G[0x1e82ca]` | 5 | 0..4 | 274 | 1..13428, 470 distinct | Five items: a party character gets them at the first join (`UNITECH@0x236`), an enemy drops them (RESULT; `INFOEN@0x1e62`). セリカ's are 1, 2, 201, 202, 221. |
| `G[0x1e9652]` | 5 | 0..4 | 274 | 1..100 | Per item: the amount a joining character gets (`UNITECH@0x291` passes it to ADDITEM as `G[0x328796]`; セリカ 1, 1, 10, 3, 1), the drop chance of an enemy's (`RESULT@0x5aa`: `random(100) < chance + G[0xef7d50]`). |
| `G[0x1ea9da]` | 10 |  | 0 |  | Two flags per drop, each n requiring `G[0xee7][n − 1] == 1` (`RESULT@0x511`). No record writes it. |
| `G[0x1ed0ea]` | 5 |  | 0 |  | A flag per drop that blocks it while set (`RESULT@0x560`). No record writes it. |
| `G[0x1ee472]` | 5 | 0..4 | 274 | −1..86, 55 distinct | Where an enemy lives: area ids, names `GS[0x26a32 + id]` ("The dungeon"), each shown once seen (`G[0xcda3b]`); −1 shows 「他」 (`INFOEN@0x1db1`). |
| `G[0x1ef7fa]` | 1 |  | 315 | 1 | Listed in the monster book; INFO and INFOEN show it once the seen flag `G[0xc5377]` is set (`INFOEN@0xaf1`). |
| `G[0x1efbe2]` | 1 |  | 17 | 1 | A large unit, 17 bosses of 5 × 2 or 5 × 3 (`DRAWUNIT@0x2bc`, `INFOEN@0x1588`). |
| `GS[0xc4a4]` | 4 | 0..3 | 99 | セリカ / ハイシェラ / シュリ | The speaker's name forms; form 1 is often an epithet (`CHINIT@0x1a4`: シュリ's 「荷物を背負った少女」). The page draws column `G[0x3a49]` (`CONFIG@0x977`); another form draws ？？？？. |
| `GS[0xdffc]` | 1 |  | 411 | セリカ・シルフィル / ハイシェラ / シュリ・レイツェン | The full name. No script reads it. |
| `GS[0xe3e4]` | 1 |  | 341 | セリカ / ハイシェラ / シュリ | The name lists and screens draw (`DRAWCHLIST@0x46b`; DRAWSTATUS, BREED, DRAWFRAME, INFOEN, RESULT, DRAWUNPARAM). |
| `GS[0xe7cc]` | 3 | 0..1 | 337 | 女神アストライアという古神の肉体を持 / … | The description, two lines used (`INFOEN@0x1c73`; DRAWUNPARAM). |

The stat columns of `G[0x1dee5a]` (and the first 14 of `G[0x1e34aa]`) are named from the 秘石 items 502..513, which
USESKILLSUB adds to the saved bonuses `G[0x20c40:18]` (`USESKILLSUB@0x95`..`0x200`), and from CALCDAMAGE:

| column | stat | column | stat |
|---|---|---|---|
| 0 | max HP | 9 | an ailment power, for condition kind 4 (CALCILL) |
| 1 | max MP | 10 | an ailment power, for condition kind 3 (CALCILL) |
| 2 | max TP | 11 | physical speed (肉体戦速) |
| 3 | physical attack | 12 | mental speed (精神戦速) |
| 4 | physical defense | 13 | a percentage CALCDAMAGE scales by |
| 5 | attack count | 14 | possession capacity (憑依容量), held against the 憑依 slots' weight `G[0x702cd7]` |
| 6 | defense count | 15 | read by CHMENU |
| 7 | magic attack | 16 | accuracy |
| 8 | magic defense | 17 | evasion |

Columns 9, 10, 13 and 15 are not named. INIT2 fills the stat caps `G[0xf45c29][22]` (`INIT2@0x205`..`0x22a`). The
attacker's power in 9 or 10 is added and the defender's subtracted.

Readings of the record:

- A party character's start is copied once. UNITECH tests bit 0 of the saved flags (`UNITECH@0x1`). When it is clear
  it copies the base level, the item kind ranks, the equipment and the items, then runs CALCREVISE and sets the bit
  (`UNITECH@0x2a7`..`0x2b6`). A patch of those fields reaches only characters not yet joined.
- Everything else is read live. SETCH builds the unit from CHINIT on every call, and CALCLVUP reads the base stats
  and growth on every call. So a patch of the stats, growth, sizes, types, skills or morale cap reaches joined
  characters too, at the next CALCREVISE.
- `G[0x158dcf]`, `G[0x158dd0]` and `G[0x158dd1]` lie inside the table's block but are work cells:
  - `G[0x158dcf]` is the character the party scripts act on: UNITECH, LEAVECH, RETIRECH, CHANGELEADER (`@0x20`),
    DRAWSTATUS, CHMENU, SELACT.
  - `G[0x158dd1]` is the character the stat scripts compute: SETCH, CALCREVISE, CALCLVUP, ADDEXP, RECOVER, SETEN,
    USESKILLSUB.
  - No script names `G[0x158dd0]`.

### CVINIT (voice settings)

Leader `G[0x157276]`, capacity 1000. The record is the voice setting: 0 ナレーション, 1 一般男性, 2 一般女性, 3 セリカ,
4 ハイシェラ, 5 シュリ, and so on to 50. CVINIT writes setting 0's name colour first (`CVINIT@0x0`), then its name
(`@0x5`), sample voice (`@0xf`) and pictures (`@0x14`, `@0x19`). It also writes each character's setting, keyed by
character id (`CVINIT@0x73`: character 1 → 3). Read by setting id, that array has 48 records with ids 3..50 and lays
out off the grid. The tool's leader declares it with `arrays=`. It belongs to the character table.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x157276]` | 1 |  | 51 | 1192..12615699, 48 distinct | The default name colour: 0x3a3a3a for narration. INITCONFIG4 copies it into `G[0x6e68]`, writing −1 where it is 0 (`INITCONFIG4@0x22`..`0x5e`); CONFIG1 reads it (`CONFIG1@0x2454`). |
| `G[0x157a47]` | 1 |  | 48 | 3..50 | Keyed by character id: the character's voice setting. 0 for a character with none (see the page below). 206 scripts read it. |
| `G[0x157e2f]` | 1 |  | 48 | 1..303 | The setting's character (`CVINIT@0x6e`). The 設定 voice page shows the row once that character is heard, `G[0xc47bf]` (`CONFIG2@0x15a8`..`0x15bd`). |
| `G[0x158217]` | 1 |  | 50 | 53..33554974 | The sample voice the 設定 voice page plays (`CONFIG2@0x12a6`). |
| `G[0x1585ff]` | 2 | 0..1 | 51 | 17555..33554631 | Two pictures of the 設定 voice page (`CONFIG2@0x1583`). |
| `GS[0xd444]` | 3 | 0..2 | 51 | ナレーション / 一般男性 / 一般女性 | The name, epithet and voice actor (`CVINIT@0x5f`..`0x69` for セリカ; `CONFIG2@0x1ed1`). |

Ids above 0xffffff carry an append's layer in the high byte: 0x1000000 is `$1$`, 0x2000000 is `$2$`. CFINIT's picture
ids are written the same way.

### The ADV page's speaker cells

Each page of the ADV prelude (`mods/ladea-base/oms/adv.oms`, the `line` template) works through these cells:

1. It sets `G[0x3a47] = 1`, the page has text (`SC0000@0x9e7`).
2. It sets the speaker `G[0x3a46]` (−1 for narration, `SC0000@0xb50`), the name form `G[0x3a49]` (`SC0000@0x3126`)
   and the face `G[0xcf2f5]` (−1 for none, `SC0000@0x1070`).
3. `set_speaker_face` keeps the face shown in `G[0x3a4a]`; −1 releases it (`SC0000@0xb57`).
4. `draw_speaker_name` copies the name form `GS[0xc4a4:4][speaker][form]` into `GS[0x34f]`, the first string above
   the saved prefix (`CONFIG@0x977`), or ？？？？. It sets `G[0x3a48]` to its half-width length, by which
   `draw_nameplate` sizes the plate.
5. `begin_page_voice` chooses the voice setting into `G[0x15765e]` (`SC0000@0xf7b`..`0xfcb`):
   - 0 for narration;
   - else `G[0x157a47][G[0x15765f][speaker]]`;
   - else 2 when the character's sex is 2;
   - else 1.

   The page copies it into `G[0xcf36c]` (`@0xfd0`) and sets the voice `G[0xcf2f6]` (`@0xfd5`). Both go into the text
   history as metadata 4 and 7 (`@0xfe1`, `@0xfed`).
6. The voice is cleared when the setting's switch `G[0x365d][setting]` is not 0, or under skip `G[0xcf2eb]` or read
   skip `G[0xcf2ec]` (`SC0000@0xff2`..`0x1026`). Otherwise it plays on voice channel 0 (`@0x1032`).
7. A page in name form 0 sets the heard flag `G[0xc47bf][speaker]` and stores it in the shared profile
   (`$1$SC5000@0x284e`).

A background voice keeps playing on channel 2 across pages: `start_background_voice` sets `G[0xcf27c]` and its speaker
`G[0xcf27b]`. While it is set, a page does not stop channel 2 (`SC0000@0xef4`). CALLBACK_LOAD and CONFIG2 choose its
setting from `G[0xcf27b]` as the page does (`CONFIG2@0x1075`).

The options:

- **Mute switches.** INITCONFIG4 zeroes `G[0x365d]` (`INITCONFIG4@0x0`), and the 設定 voice page sets a setting's
  switch to 0 or 1 (`CONFIG2@0x111a`).
- **Name colours.** `G[0x6e67] = 1` draws a speaker's name in `G[0x6e68][setting]` (`INITCONFIG2@0x10`;
  `CONFIG1@0x130a`).
- **The window and its button bar:**
  - `G[0xed3]` is 1 while the message window is shown. `adv_show_window_handler` sets it,
    `adv_hide_window_handler` clears it, and `draw_message_window` draws only while it is set.
  - `G[0xed5]` is the button bar: 1 collapsed, 2 expanded. `expand_button_bar` sets 2, and `register_adv_hotspots`
    registers the quick save, quick load, auto, read-skip and skip buttons only at 2.
  - The pin `G[0xed6]` keeps the bar open: while it is 1, the hotspot that collapses the bar is not registered.
- **Not named:**
  - `G[0xed4]`, a window style: at 1 there is no face and the nameplate has its own cursor. HMODE, DEBUGADV2 and
    SYSTEM4 write it.
  - Two voice options of the 設定 voice page, `G[0x6e62]` and `G[0x6e66]` (`CONFIG2@0xf21`, `@0xf44`; `@0xe71`, `@0xe8a`). Both are kept in the shared profile.
    INITCONFIG3 sets them to 0 and 1 (`INITCONFIG3@0x8`, `@0x0`).

### FTINIT (fates)

Leader `G[0xf63ae]`, capacity 1000. The record is the fate (運命), one of the events the fate book (INFOFT) lists by
chapter. FTINIT writes fate 1's sort key `G[0xf63af] = 10` first (`FTINIT@0x0`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xf63ae]` | 1 |  | 151 | 10..5030 | The book's order (`INFOFT@0x852`). |
| `G[0xf6796]` | 3 | 0..2 | 151 | 1..7101, 144 distinct | Up to three flags n, each requiring `G[0xee7][n − 1] == 1` (`SETFATE@0xbe`). |
| `G[0xf734e]` | 3 | 0 | 6 | 402..7101 | Up to three flags n, any set keeping the fate unreached (`SETFATE@0x118`). |
| `GS[0x53c]` | 1 |  | 151 | ミルフェの街に到着した / 攫われた民の救出に参加した / … | The fate's text. SETFATE works only on fates with one (`SETFATE@0x1d`), and INFOFT draws it (`INFOFT@0x10b9`). |
| `GS[0x924]` | 1 |  | 151 | 序章 / 序章 / 序章 | The chapter (`INFOFT@0x1073`). |

Four more arrays of three conditions follow in the block. No record writes them:

- `G[0xf7f06]` and `G[0xf8abe]` are bit 2's required and forbidding flags (`SETFATE@0x25b`, `@0x2ba`);
- `G[0xf9676]` and `G[0xfa22e]` are bit 1's (`SETFATE@0x184`, `@0x1e3`);
- `GS[0xd0c:13]` is unwritten and unread.

SETFATE keeps the saved `G[0x6282][fate]`:

- **Bit 0.** While bits 1 and 2 are clear, SETFATE recomputes bit 0 (`SETFATE@0x32`..`0x60`). It is set when every
  required flag holds and no forbidding flag does.
- **Bits 1 and 2.** While bit 0 is the only bit set, SETFATE sets bit 1, or bit 2, when that bit's conditions exist
  and hold. With no conditions written in this version, neither bit can be set.
- **The book.** INFOFT lists the fates with bit 0 (`INFOFT@0x884`) and greys those with bit 1 or 2.

SYSTEM4 runs SETFATE (`SYSTEM4@0x653`), and so does INFOFT before it draws (`INFOFT@0x1b1`). A patch of a fate's
flags therefore takes effect the next time either runs, on saves already made too.

### CIINIT (人物 profiles)

Leader `G[0xfade6]`, capacity 1000. The record is the character id: profile 1 is セリカ・シルフィル, the same character
as CHINIT's 1. The 人物 book (INFOCH) lists the profiles, and INFO decides which are open.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xfade6]` | 1 |  | 47 | 10..730 | The book's order (`INFOCH@0xc78`). |
| `G[0xfb1ce]` | 6 | 0..5 | 47 | 1..231 | A character per line, the profile's own id in all six. Only column 0 is read: the profile opens only once that character is heard, `G[0xc47bf]` (`INFO@0x8f3`..`0x915`). |
| `G[0xfc93e]` | 18 | 9..17 | 20 | 44..3916, 21 distinct | Three conditions per line at 3 × line + k. Each n is the scene-seen flag `G[0xc20af][n − 1]` of the shared profile, which the scenes set for their own number (`SC0000@0xd53`). A line shows when its first condition is 0 or any of the three is set (`INFOCH@0x1974`). Line 0's conditions decide whether the profile opens at all (`INFO@0x85a`). Only lines 3..5 have any. |
| `GS[0x3fd4]` | 1 |  | 47 | セリカ・シルフィル / ハイシェラ / シュリ・レイツェン | The name. The book lists a profile that has one (`INFO@0x845`; `INFOCH@0xcaa`). |
| `GS[0x43bc]` | 1 |  | 47 | 人間 / 魔神 / 人間 | The race shown under the name (`INFOCH@0x156e`). |
| `GS[0x47a4]` | 6 | 0..5 | 47 | 古の女神アストライアの肉体を持つ神殺 / … | The six lines (`INFOCH@0x1a20`). |

INFO rebuilds the open profiles into `G[0xc4f8f]` each time it runs (`INFO@0x823`..`0x95c`), and INFOCH lists from it
(`INFOCH@0x9a1`). `G[0xf45f4f]` set opens every profile, form and line.

### CFINIT (character forms)

Leader `GS[0x5f14]`, ten per id, capacity 1000. The record is the character id, as in CIINIT. Columns 1..9 are the
character's forms in the 人物 book; column 0 is unused. The book offers a form whose name is not empty
(`INFOCH@0x1025`).

The fragments write base records, but only into new form columns:

- `$1$CFINIT` adds an アイドル form to characters 3 (column 5), 51 (4) and 66 (6);
- `$2$CFINIT` adds 実体化 to 2 (column 2) and a form for 231.

The tool's two writes ahead of a record are `$1$CFINIT@0x0`, which writes character 3's new form condition
`G[0x100ff7]` before the form's name. It is the same record.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x5f14]` | 10 | 1..6 | 47 | 通常 / 剣 / 通常 | The form's name: 通常, 女神化, 剣, 実体化, 工房, 裸, アイドル … |
| `G[0x100f8e]` | 30 | 3..18 | 22 | 3..5105, 30 distinct | Three conditions per form at 3 × form + k, scene-seen flags `G[0xc20af][n − 1]` as for the profile lines (`INFOCH@0x1059`). |
| `G[0x1084be]` | 10 | 1..6 | 47 | 16290..33554532 | The form's picture, drawn on element 0xc4 centred on x 1120 and standing on y 720 (`INFOCH@0x1a94`). With none, the book draws a placeholder. |
| `G[0x10abce]` | 250 | 25..161 | 47 | 84..33554940, 865 distinct | Up to 25 pictures per form at 25 × form + k. The book counts them (`INFOCH@0x116e`) and steps through them on element 0xc6, centred on x 826 and standing on y 612. |
| `G[0x147c5e]` | 10 | 1..6 | 47 | 82..33554490, 31 distinct | A backdrop per form, drawn on element 0xc3 across the panel at (0, 151), 1280 × 525 (`INFOCH@0x1a3f`). It is 82 on most forms. |

### VIINIT (the 用語 glossary)

Leader `G[0x14a36e]`, capacity 1000. The record is the entry. INFOVO lists the entries and shows one; INFO decides
which are open.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x14a36e]` | 1 |  | 101 | 10..5100, 98 distinct | The order (`INFOVO@0x8e8`). |
| `G[0x14a756]` | 39 | 0..38 | 101 | 1..5016, 89 distinct | Three conditions for each of the 13 lines at 3 × line + k, scene-seen flags `G[0xc20af][n − 1]`. A line shows when its first is 0 or any is set (`INFOVO@0x114a`); line 0's open the entry (`INFO@0x758`). |
| `GS[0x8624]` | 1 |  | 101 | 二つの回廊の終わり / 三神戦争 / 古神の世界 | The title. The glossary lists an entry that has one (`INFO@0x743`; `INFOVO@0x91a`). |
| `GS[0x8a0c]` | 1 |  | 101 | 用語 / 用語 / 用語 | The category, shown right of the title (`INFOVO@0xee9`). |
| `GS[0x8df4]` | 13 | 0..12 | 101 | 二つの回廊とは、 / … | The entry's lines (`INFOVO@0x11f6`). |

INFO rebuilds the open entries into `G[0xc4ba7]` (`INFO@0x721`..), and INFOVO lists from it (`INFOVO@0xea3`).

### What is saved

None of the six tables is saved. These arrays below the prefix hold what a character carries, keyed by character id
0..99:

| array | holds | written by |
|---|---|---|
| `G[0x725a]` | Flags: bit 0 joined once, bit 1 in the party, bit 2 left for a while (LEAVECH, when `G[0x6e3f] & 2`), bit 3 retired, bit 16 active, bit 17 cleared on join; a member is `(flags & 0x10002) == 0x10002` | UNITECH (`@0x1`, `@0x2b6`), LEAVECH (`@0xf`), RETIRECH (`@0x3`), SETCHSEL (`@0x15f`) |
| `G[0x20790]`, `G[0x207f4]` | Level and experience (a level costs 100 + 3L², at most 999,999) | UNITECH, ADDEXP (`@0x88`) |
| `G[0x20858]`, `G[0x208bc]` | Morale and its maximum | RECOVER (`@0x3a`), BTL, OPENFACT; CALCREVISE (`@0x13e`) |
| `G[0x20920]`, `G[0x20984]` | HP and its maximum | ADDHP, FIELD, USESKILLSUB; CALCREVISE (`@0x8d`) |
| `G[0x209e8]`, `G[0x20a4c]` | MP and its maximum | ADDMP; CALCREVISE |
| `G[0x20ab0]`, `G[0x20b14]` | TP and its maximum | ADDTP; CALCREVISE |
| `G[0x20b78]`, `G[0x20bdc]` | The unit's attack element and affinity type | CALCREVISE (`@0x179`, `@0x185`) |
| `G[0x20c40:18]` | Permanent stat bonuses: +10 HP, +5 MP, +5 TP, +1 to each other stat | USESKILLSUB (items 502..513) |
| `G[0x218c0:60]`, `G[0x23030:60]`, `G[0x247a0:60]` | Each condition's level (names `GS[0x110d0:5]`, ILINIT), base level and turns left (−1 = 永続) | CALCREVISE (`@0x225`) |
| `G[0x25f10:500]` | Learned skill ranks, which SETCH adds to the innate | the skill scripts |
| `G[0x32260:500]` | Skill ranks in effect; each one's best is kept in `G[0xcad4f]` of the shared profile | CALCREVISE (`@0x191`) |
| `G[0x3e5b0:80]` | Rank in each item kind; CHECKACT checks an item's required rank against the rank in its kind (`CHECKACT@0x44`, `@0x938`) | UNITECH (`@0x84`) |
| `G[0x404f0:5]` | Equipment: 0 weapon, 1 shield, 2 armour, 3 and 4 | UNITECH (`@0xaf`), RETIRECH (`@0x4d`), SETEQUIP |
| `G[0x406e4:3]` | The three 憑依 slots | SETCHPSTATE (`@0x14c`), CHMENU, RETIRECH |

Between the stat bonuses and the conditions lie 1,400 cells from `G[0x21348]` that no script names. Not understood,
and not named:

- `G[0x40810:500]`: when 1, it disables a skill rank whose `G[0x3012e7:5]` is 0;
- `G[0x4cb60:3]` (SELACT, `@0xc7e`) and `G[0x4cc8c:6]` (CHMENU);
- `G[0x1ffc0]` (CHMENU, tested `& 0x101`), `G[0xcee2d]` and `G[0xcf27e]` (GAMESTART, GAMECLEAR).

These single cells are saved too:

- the party: the leader `party_leader` `G[0x7259]` (CHANGELEADER `@0x20`; SCJUMP's scenes read it 299 times), and the formation
  `G[0x1fdad:8]`, `G[0x1fdb5:8]`, `G[0x1fdbd:8]` (character, column, row) with its count `G[0x1fdc5]` (UNITECH,
  LEAVECH, SORTSALLY, CHMENU);
- the bonuses that GAMESTART zeroes (`@0x9be`, `@0x9c3`) and OPENFACT raises: `G[0x1ffbb]` on the level cap and
  `G[0x1ffbc]` on the initial morale;
- the fate states `G[0x6282][fate]`;
- the ADV cells `G[0x3a46]`..`G[0x3a4a]`, the window cells `G[0xed3]`, `G[0xed5]` and `G[0xed6]`, and the background
  voice `G[0xcf27c]`, `G[0xcf27b]`;
- the voice options `G[0x365d]`, `G[0x6e67]` and `G[0x6e68]`, which are also kept in the shared profile.

`G[0xc4f8f]` and `G[0xc4ba7]` lie in the saved range too, but INFO rebuilds them before every use. These cells lie
above the prefix and are not saved: `G[0x15765e]`, `G[0xcf2f5]`, `G[0xcf2f6]`, `G[0xcf36c]`, and the string
`GS[0x34f]`.

These flags are kept in the shared profile, which outlives a save:

- the speaker heard, `G[0xc47bf][character]`, which the 設定 voice page and the 人物 book require;
- the scene seen, `G[0xc20af][scene]` ("Scenes and screens"), which the profile lines, the forms and the glossary lines require.

## Skills, conditions and battle

Seven tables describe the battle. INIT2 calls six of them at boot: COINIT `INIT2@0x6`, AFINIT `@0x9`, SSINIT `@0xc`,
ILINIT `@0x15`, RTINIT `@0x21` and BTANINIT2 `@0x63`. `$1$AUTORUN` and `$2$AUTORUN` call the appends' fragments at
the same offsets. BTANINIT never runs at boot. SETEFFECT walks its chain `G[0xf45e4d]` to load one effect at a time
(`SETEFFECT@0x79`). COINIT also runs at boot, but its boot run selects no record. The game loads COINIT's records one
at a time through its chain `G[0xf45d6d]`.

| table | records | ids | capacity | leader | `$1$` adds | `$2$` adds |
|---|---|---|---|---|---|---|
| SSINIT (special skills) | 135 (128 + 7) | 1..219 | 500 | `G[0x2fcc97]` | families 131..133, 216..219 | empty fragment |
| AFINIT (elements, races, item kinds, item classes, field actions, formations) | lists, not records | | 16 to 100 a list | none | empty fragment | empty fragment |
| ILINIT (conditions) | 45 | 1..50 | 60 | `G[0x3286a5]` | empty fragment | empty fragment |
| RTINIT (battle routines) | 2877 steps (2360 + 60 + 457) | 100..96207 | 100000, 100 a routine | `G[0x1effcd]` | 60 steps | 457 steps |
| BTANINIT2 (battle animations) | 689 (667 + 22) | 1..1060 | 20000 | `G[0xef8696]` | empty fragment | 22 |
| BTANINIT (battle effects, loaded on demand) | 348 (343 + 5) | 1..8, 101..1001 | 46 effect slots | `G[0xef857a]` selects | empty fragment | 5 |
| COINIT (a character's look in one costume, loaded on demand) | 354 (284 + 27 + 43) | characters 1..0x3c2 | one record | `G[0xbdc65e]`, `G[0xbdc65f]` select | 27 | 43 |

Every array of these tables lies above the saved prefix (`G[0xcf2e6]`, `GS[0x34f]`), so none of them is saved. INIT2
writes them again after a load restores the save. What a character keeps from one battle to the next lies in the
character arrays below the prefix, listed in "What is saved".

The layouts below come from `init_layout.py` with this section's leaders. Three tables do not lay out
cleanly:
- AFINIT has no leader. A leader's capacity applies to all of its arrays, so a declared 30-wide affinity matrix
  reaches into the kind lists. Its lists are described by hand instead.
- The condition names lie at `GS[0x110d0]`. `ILINIT@0xa` writes condition 1's 即死 at `GS[0x110d5]`. The tool
  reports the array as `GS[0x110cb]`, one record low. Five conditions (4..6, 39, 40) write names but no leader, and
  their keys pull the array down. A declared array is moved the same way ("What is not decoded", Tooling).
- SSINIT has one write ahead of a record. `$1$SSINIT@0x0` writes family 131's `G[0x2fc932]` before its leader.

### SSINIT (special skills)

Leader `G[0x2fcc97]`, capacity 500. The record is the family id (貫通, 即死 …). Each family has up to five levels,
Ⅰ to Ⅴ. A level is the cell `5 × family + level − 1` of a 2500-cell array, the `int[500][5]` the registry gives it.
`SSINIT@0x0` writes family 1's `G[0x2fcc98] = 1` first, then its names and descriptions. The arrays lie back to
back, all ≡ 0x2fc8af modulo 500. The tool leaves the condition matrix `G[0x301cab]` (300 cells a family) as 20
loose cells until it is declared.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2fc8af]` | 1 |  | 7 | 1 | The layer that adds the family: 1 on the seven families `$1$SSINIT` adds (131..133, 216..219). No script reads it. |
| `G[0x2fcaa3]` | 1 |  | 135 | 10..1940, 135 distinct | The sort key. `sort-indices-by-key-sum` orders by it at `CHECKSS@0x2f3` and `COMMITSS@0x86`. |
| `G[0x2fcc97]` | 1 |  | 135 | 1×27, 2×9, 3×4, 4×49, 5×46 | The kind, which the description's first word names: 1 【攻撃】 (acts when the unit attacks), 2 【防御】 (when it is attacked), 3 【開始】 (at the start), 4 【常時】 (always), 5 【特殊】 (special). CHECKSS tests it (`CHECKSS@0xb1`, `@0x110`, `@0x337`, `@0x472`), and so does COMMITSS (`@0xf4`, `@0x23f`, `@0x33d`). SETCH adds a kind-4 level's conditions to the unit (`SETCH@0x8e6`). CHECKSS4 zeroes the attacker's kind-1 families (`CHECKSS4@0x65`). |
| `G[0x2fce8b]` | 1 |  | 86 | 1..25, 14 distinct | *(not decoded)* No script reads it. |
| `G[0x2fd65b]` | 1 |  | 33 | 1 | 1 when the family acts only on an attack, an action whose ITINIT `G[0x34f897]` is 1 or 2 (`CHECKSS@0x161`; "Items, stones, improvement and alchemy"). |
| `G[0x2fd84f]` | 5 |  | 0 | | The HP cost of each level. No record writes it. CHECKSS keeps a level only while HP plus the cost stays at least 1 (`CHECKSS@0x37e`..`@0x45b`), and COMMITSS applies it (`COMMITSS@0x122`..`@0x17b`). |
| `G[0x2fe213]` | 5 | 0..4 | 3 | −7..−1 | The MP cost of each level, negative. It is checked and taken like the HP cost: MP plus the cost must stay at least 0. |
| `G[0x2febd7]` | 5 | 0..4 | 9 | −7..−1 | The TP cost of each level, negative (貫通Ⅰ −3). It is checked and taken like the MP cost. |
| `G[0x2ff59b]` | 5 | 0..4 | 16 | 3..100 | The trigger chance of each level, in percent. CHECKSS drops the level when `random(100)` reaches it (`CHECKSS@0x1f9`). It skips the roll while `G[0xf45f4f]` is set (`CHECKSS@0x1c0`). |
| `G[0x2fff5f]` | 5 | 0..4 | 61 | −30..100 | The amount of each level, each family's own. For 貫通, SETSSBONUS takes it from the defender's defence and its defence count (`SETSSBONUS@0x24`, `@0x68`; the description says 50%). 抉り切り上げ adds it to damage and critical hits (`SETSSBONUS@0xac`). SETSSDAMAGE, BTLMAIN and CALCDAMAGE read it too. |
| `G[0x3012e7]` | 5 | 0..4 | 46 | 1 | 1 when the player cannot switch the level off. The skill screen refuses the switch (`CHMENU@0x1920`). SETCH zeroes a unit's level that the saved switch `G[0x40810 + 500 × char + family]` turns off, unless this is 1 (`SETCH@0x8a1`..`@0x8de`). |
| `G[0x301cab]` | 300 | 1..261 | 4 | 1..5 | The ILINIT condition levels each level gives, at cell `300 × family + 60 × (level − 1) + condition`. SETCH adds a kind-4 family's to the unit's condition floor `G[0xf3a183]` (`SETCH@0x8e6`, `@0x931`), and so does SETEN (`@0x793`). Written for family 2 (即死, condition 1) and families 81..83 (conditions 19..21, 再生 魔再 技再). |
| `G[0x32669b]` | 1 |  | 39 | 801..884, 21 distinct | A BTANINIT effect id played when the family fires. BTLMAIN (`@0x46f`) and COMMITSS (`@0x3a0`) put it in the unit's `G[0xef866e + unit]`, and SETEFFECT loads it into an effect slot (`SETEFFECT@0xb3`). |
| `G[0x32688f]` | 1 |  | 35 | 1×23, 2×12 | Whose unit shows the effect: 1 the attacker's (`BTLMAIN@0x21a3`), 2 the defender's (`COMMITSS@0x3ae`). |
| `GS[0xf384]` | 5 | 0..4 | 135 | 貫通Ⅰ / 即死Ⅰ / 破術Ⅰ | The name of each level. A level with no name does not exist: SETCH reads the name to tell (`SETCH@0x829`). The skill screens draw it (INFOSS `@0xa83`, CHMENU, DRAWUNPARAM, INFOEN, SHOP). |
| `GS[0xfd48]` | 10 | 0..9 | 135 | 【攻撃】攻撃ＨＩＴ時に確率で発動 … | Two description lines a level, at cell `10 × family + 2 × (level − 1) + line` (`INFOSS@0x175a`, `CHMENU@0x5e9c`). |

**In battle.** SETCH (party) and SETEN (enemies) build a unit's levels into `G[0xf3b493 + 500 × unit + family]`.
For a party unit, SETCH adds up four sources:
- the character's innate levels `G[0x15d80a + 500 × char]` ("Characters, voices, fates and the books") and its learned levels
  `G[0x25f10 + 500 × char]`, which are saved (`SETCH@0x757`..`@0x7a0`);
- the family and level of each equipped item (ITINIT's `G[0x769577]` and `G[0x7caff7]`, "Items, stones, improvement and alchemy";
  `SETCH@0x44a`..`@0x48e`);
- a second item pair, `G[0x6d6db7]` and `G[0x6dbbd7]` (`SETCH@0x6df`..`@0x730`).

It then drops the levels that are switched off. For each action, CHECKSS copies the levels into the working cells
`G[0xd0a83f + 500 × unit + family]`, then works through them:
- it zeroes the families that do not apply to the action (kind, attack only);
- it rolls the chances;
- it drops the levels the unit cannot pay for;
- it calls the chain's slots k 0..2 for each of the 32 entries (`CHECKSS@0x2c8`), then slot 3 (`@0x5d5`).

CALCSS walks slot 0 of the damage chain (`CALCSS@0x68`). COMMITSS walks slot 1, then applies the costs and the
effects (`COMMITSS@0x1c5`). All three are called from CALCDAMAGE. The base layer fills chain entry 0:
- **CHECKSS1** keeps the defender's family 56 (身代わり: covers an ally under 30% HP) only when the defender is
  `G[0xef7cdf]` (`CHECKSS1@0x0`). It resets the attacker's family 214, which raises an auto-battle unit's attack
  while its holder is on the field. If the attacker is a party unit whose flags `& 0x300143` are `0x103`, it gives
  the attacker the highest family 214 level among party units 1..5 with flags `& 0x300043 = 3` (`CHECKSS1@0x3c`..`@0xe9`).
  Reading the flags as the auto-battle and on-the-field states is an inference.
- **CHECKSS2** tests the conditions of about 40 families against the battle state: HP, the action's kind, weapons and
  positions. Among them are 36..39 (a size test and the 魔剣 families), 57..59 (孤高 and the ranged and close reductions), 106 and 107 (必殺無効,
  魔術無効) and 195..212 (the 好き/嫌い families and others). It reads the item kind flags `G[0xbd66eb]`
  (`CHECKSS2@0x406`). The families are listed, the tests are not decoded.
- **CHECKSS3** handles the immunities, each zeroing the other side's families:
  - the defender's 104 zeroes the attacker's 103 (オーバーキル, `CHECKSS3@0x0`);
  - the defender's 105 zeroes the attacker's 21..35 (the 〜殺し families, `@0x22`);
  - the defender's 108 zeroes the attacker's 1 (貫通, `@0x6b`);
  - the attacker's 109 zeroes the defender's 51 and 52 (反射, `@0x8d`);
  - the defender's 110 or 124 zeroes the attacker's 2 (即死, `@0xbf`).
- **CHECKSS4** acts when the defender's 51..54 (反射, 見切り, カウンター) remain. It zeroes all the attacker's kind-1
  families and sets `G[0xd0a845]` to −1 (`CHECKSS4@0x0`..`@0x8d`). That is unit 0's cell for family 6 (連続攻撃).
  Why it is written there is not decoded.
- **SETSSBONUS** sums the amounts of the attacker's 1, 8, 9, 86 and 212 and the defender's 56, 57, 211 and 212. They
  go into the bonus cells `G[0xef7cd3]`..`G[0xef7cd8]` that CALCDAMAGE applies.
- **SETSSDAMAGE** changes the damage `G[0xd0a7e6]` for the attacker's 2, 5, 7, 8, 210 and 214 and the defender's 53,
  54, 106, 107 and 125.

The appends add their own entries, for the families of `$1$SSINIT`:
- `$1$CHECKSS1` gives the defender family 132 (統率力, the defence of the holder's allies). It does so when another
  unit on the defender's side with flags `& 0x300043 = 3` holds it: party units 1..5, or enemy units 10..39
  (`$1$CHECKSS1@0x0`..`@0xa0`).
- `$1$SETSSBONUS` adds the 統率力 amount to the defence bonus.
- `$1$CHECKSS2` tests 216 and 217 (the ユニットアイドル families) on both sides.
- `$1$SETSSDAMAGE` handles the attacker's 218 and 219 (女好き, 男好き): damage by the number of women or men on the
  field, read from CHINIT's `G[0x1591ba + char]` ("Characters, voices, fates and the books"). It also handles the defender's 133 (ピンポイントガード).
- `$2$SETSSDAMAGE` only sets `G[0xf45ca2] = 1`, and a jump skips that write (`$2$SETSSDAMAGE@0x0`).
- `$1$CHECKSS3`, `$1$CHECKSS4` and the other `$2$` fragments are a bare `exit`.

### AFINIT (elements, races, item kinds, item classes, field actions, formations)

AFINIT writes lists, not records. Only the base layer has data. The matrices come from `copy-inline-int-array`
(`AFINIT@0x0`..`@0x1e`, `@0xdc`..`@0x127`), and the lists from one write a cell. The lists lie back to back, so each
one's capacity is the distance to the next.

| array | shape | ids written | holds |
|---|---|---|---|
| `GS[0x1fc5c]` | `[30]` | 1..22 | The attack element names: 無属 物理 万能 火炎 冷却 電撃 地脈 神聖 暗黒 戦意 (1..10), then 抵抗1..抵抗12 (11..22), the columns the conditions are rolled in. DRAWITLIST draws them (`DRAWITLIST@0x7bd`). |
| `GS[0x1fc7a]` | `[30]` | 1..15 | The defence element names: 無属 … 暗黒, 神格, 魔神, 反万, 霊体, 不死, 創造 (`DRAWITLIST@0x801`). |
| `GS[0x1fc98]` | `[20]` | 1..16 | The race names: 人間, 亜人間, 鬼族 … 魔神, 一般. DRAWSTATUS (`@0x1607`), DRAWUNPARAM and INFOEN (`@0x1cd3`) draw them. |
| `GS[0x1fcac]` | `[80]` | 40 ids | The item kind names (重要アイテム, 通常アイテム, 非戦闘時アイテム …), by ITINIT's `item_kinds`. No script reads them. |
| `GS[0x1fcfc]` | `[80]` | 40 ids | The item kind short names: 1 重要, 2 物品, 3 特殊, 4 素材, 5 憑石, 6 武器, 7 防具, 8 盾, 9 装飾, 10 売専, 19 近接, 20 間接; 21..30 the 必殺 schools (飛燕 一撃 弓技 射撃 盗賊 連撃 槍撃 接技 体術 魔導); 41..54 the magic schools (治癒 再生 強化 火炎 … 招聘); 39 and 40, 59 and 60 the enemies'. No script reads them. |
| `GS[0x1fd4c]` | `[80]` | 8 ids | A school's equipment note (「剣」装備必須 …). No script reads it. |
| `G[0xbd6277]` | `[30][30]` | rows 0..15, columns 0..22 | The damage percent by defence element (row) and attack element (column), −100 to 200. 0 takes nothing, and a negative value makes the damage negative. CALCDAMAGE reads the defender's row `G[0xf3885b + unit]` at the skill's element `G[0x363117 + action]` ("Items, stones, improvement and alchemy") or the attacker's `G[0xf38833 + unit]` (`CALCDAMAGE@0x413`). CALCILL scales a condition's chance by its column (`CALCILL@0x5c9`). DRAWAFF draws rows 1..15 and columns 1..10. CHECKACT (`@0x4e8`), INFOAF and BTLMAIN (`@0x2b86`) read it too. |
| `G[0xbd65fb]` | `[80]` | 40 ids | A kind's sort key, its id × 10 (980 and 990 for the enemies' 40 and 60). No script reads it. |
| `G[0xbd664b]` | `[80]` | 40 ids | The item class whose icon stands for the kind (重要 → 31 重要, 武器 → 6 剣, 飛燕 → 61). CHMENU draws its cell of the 13-pixel, 10-a-row icon sheet (`CHMENU@0x44e7`..`@0x4534`), and DRAWFRAME does too (`@0x89b`). |
| `G[0xbd669b]` | `[80]` | 9 ids | The weapon classes a 必殺 school needs, bit k for item class k: 飛燕 `0x40` (剣), 一撃 `0x2600` (両手剣 大剣 鎌), 弓技 `0x1000` (弓), 射撃 `0x10` (魔導銃), 連撃 `0x8020` (突剣 刀), 槍撃 `0x800` (槍), 接技 `0x100` (連接剣), 魔導 `0x1000000` (魔装). CHECKACT tests the unit's weapons against it (`CHECKACT@0x958`, `@0x9a2`), and so do CHECKSEAL and SETSKLIST. |
| `GS[0x1fd9c]` | `[100]` | 58 ids | The item class names: 1..15 weapons (杖 笛 短剣 魔導銃 突剣 剣 拳 連接剣 両手剣 大剣 槍 弓 鎌 箒 刀); 21..26 armour and accessories (軽装 中装 重装 魔装 盾 装飾); 27..30 stones (喚石 聘石 援石 憑石); 31..40 other items (重要 物品 魔弾 神化 素材 売却 成長 結晶 特殊); 41.. the magic schools; 61.. the 必殺 schools; 81 近接, 82 間接. An item's class is ITINIT's `item_classes` `G[0x345c57 + item]` ("Items, stones, improvement and alchemy"). DRAWSTATUS (`@0x1645`), INFOIT (`@0x10dd`), SHOP and the INFO screens draw them. |
| `G[0xbd66eb]` | `[100]` | 17 ids | Class flags: 1 on 槍 弓 箒, 2 on the other weapons and on 飛燕 一撃 連撃 体術 近接, 3 on 両手剣 and 鎌. CALCDAMAGE tests bit 1 for an attack action and then compares the two units' rows `G[0xf4514b]` (`CALCDAMAGE@0x87e`..`@0x8a1`); the inference is close range. CHECKACT (`@0xfb`, `@0x125`), CHECKSEAL, CHECKSS2, CHMENU, DRAWSTATUS, SETITLIST and SETSKLIST read it too. Bit 0 is not decoded. |
| `G[0xbd674f]` | `[100]` | 28 ids | The sound an item of the class plays when used: 0x3c0a SE036.WAV on 26 classes, 0x3c0b SE013.WAV on two (`ITEM@0xaf0`..`@0xb0c`). SKILL reads it too. |
| `GS[0x1fe00]` | `[100]` | 1..11 | The field action names: 調べる, 物を押す, ジャンプ, 壁破壊, 担ぎ上げ, 水中結界, 素材調達, 精霊結界, 天の翼, 神聖結界. No script reads them. |
| `G[0xbd67b3]` | `[100]` | 1..11 | The field action's icon, SO009A..SO009J.AGF (0x3c0c..0x3c15; `FIELD@0x46be`). |
| `G[0xbd6817]` | `[100]` | 4, 5, 10, 11 | *(not decoded)* An index into the saved `G[0x725a]`, whose bits `& 0x10006` FIELD tests against `0x10002` before the action (`FIELD@0x1d9d`..`@0x1dc7`): 壁破壊 23, 担ぎ上げ 32, 天の翼 50, 神聖結界 5. |
| `G[0xbd69db]` | `[8][8]` | rows and columns 1..7 | A ratio by two rarities, `G[0xbd69db + 8a + b]`: row 1 is 100..700, each row halving down to row 7, 1..100. IMPROVE (`@0x9f3`) and BREED (`@0xb02`) look it up with the two items' rarities `G[0x3371f7 + item]` ("Items, stones, improvement and alchemy") and use `(n − 1) / ratio + 1`. |
| `G[0xbd6a16]` | `[8][5]` | rows 1..7 | *(not decoded)* Seven rows of five, starting at `G[0xbd6a1b]` (`AFINIT@0x17c`..`@0x195`): [6 12 25 50 100], [2 1 −1 −1 −1], [3 2 1 1 1], [−5 −3 0 0 0], [10 5 3 3 3], [20 10 −3 −5 −10], [−10 −5 −3 −2 −1]. No script reads it. |
| `GS[0x1fe64]` | `[16]` | 1..8 | The formation names, 基本陣形 … 魔術陣形. No script reads them. |
| `GS[0x1fe74]` | `[16][7]` | 1..8, columns 0..1 | A formation's effect lines. Only placeholders are written (○○○○up, ○○○○down). No script reads them. |

The saved per-character kind rank `G[0x3e5b0 + 80 × char + kind]` is kept with the characters ("Characters, voices, fates and the books"). CHECKACT compares the rank in an
item's kind (`CHECKACT@0x44`) with the item's rarity (`CHECKACT@0x938`), and GAMESTART clears it (`GAMESTART@0xa0e`).

### ILINIT (conditions)

Leader `G[0x3286a5]`, capacity 60. The record is the condition id. Each condition has up to five levels: 呪い1 to
呪い5 are condition 7's. The level arrays are `int[60][5]` at `G[0x326a85 + 300 × n]`, n 0 to 23. `ILINIT@0x0`
writes condition 1's `G[0x3286a6] = 2` first. Conditions 4..6 (HP吸, MP吸, TP吸), 39 (蘇生) and 40 (経験, icon
only) have names but no leader: their effect is the action's, not the condition's. The condition in work is
`G[0x326a83]`, and `G[0x326a84]` is the levels to add, negative to take away. ADDILL and CALCILL apply them.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x326a85]` | 5 | 0..4 | 45 | 3×52, 4×154 | Which figures each level is rolled with. For 3, CALCILL adds the action's column 10 of ITINIT `G[0x6186d7]` ("Items, stones, improvement and alchemy") and the unit's `G[0xf38513]`, the sum of condition_mental_resistance_modifiers. For 4, column 9 and `G[0xf384eb]`, the physical sum (`CALCILL@0x534`..`@0x5b8`). Any other value is not rolled (`CALCILL@0x456`..`@0x481`). |
| `G[0x326bb1]` | 5 | 0..4 | 45 | 11..22 | The column of the attack-element affinity, 抵抗1..抵抗12, that scales each level's chance against the defender's defence element row (`CALCILL@0x51f`, `@0x5c9`). |
| `G[0x326cdd]` | 5 | 0..4 | 43 | −1..200 | The duration of each level, −1 until cured. ADDILL copies it into the saved duration `G[0x247a0 + 60 × char + cond]` (`ADDILL@0x3f1`..`@0x43e`), and CALCILL into the battle unit's (`CALCILL@0xa5e`). |
| `G[0x326e09]` | 5 | 0..4 | 6 | −30..30 | What each level adds to hit, `G[0xf38603]` (`SETILLREVISE@0x1c6`): 命中, 命低, 女神, 抑制, 付与, 疲労. |
| `G[0x326f35]` | 5 | 0..4 | 7 | −50..30 | What each level adds to evasion, `G[0xf3862b]` (`SETILLREVISE@0x1dd`): 回避, 回低, 麻痺 … |
| `G[0x327061]` | 5 | 0..4 | 7 | −50..50 | The percent each level adds to physical attack, `G[0xf38383]` (`SETILLREVISE@0x9b`): 物攻, 攻低, 高揚 … |
| `G[0x32718d]` | 5 | 0..4 | 7 | −100..30 | The percent each level adds to physical defence, `G[0xf383ab]` (`SETILLREVISE@0xb2`): 物防, 防低, 高揚 −20..−100 … |
| `G[0x3272b9]` | 5 | 0..4 | 5 | −2..5 | What each level adds to the attack count, `G[0xf383d3]` (`SETILLREVISE@0x125`): 攻回 … |
| `G[0x3273e5]` | 5 | 0..4 | 5 | −4..5 | What each level adds to the defence count, `G[0xf383fb]` (`SETILLREVISE@0x13c`): 防回 … |
| `G[0x327511]` | 5 | 0..4 | 7 | −50..50 | The percent each level adds to magic attack, `G[0xf38423]` (`SETILLREVISE@0xc9`): 魔攻, 攻低 … |
| `G[0x32763d]` | 5 | 0..4 | 7 | −100..30 | The percent each level adds to magic defence, `G[0xf3844b]` (`SETILLREVISE@0xe0`): 魔防, 防低 … |
| `G[0x327769]` | 5 | 0..4 | 2 | 1..5 | What each level adds to magic power, `G[0xf38473]` (`SETILLREVISE@0x20b`): 魔威, 女神. |
| `G[0x327895]` | 5 | 0..4 | 1 | 10..30 | The percent 戦攻 adds to `G[0xf3849b]` (`SETILLREVISE@0xf7`). CALCDAMAGE adds that cell to the power of an action whose ITINIT `G[0x34f897]` is 5 (`CALCDAMAGE@0x512`..`@0x53e`). |
| `G[0x3279c1]` | 5 | 0..4 | 1 | 10..30 | The percent 戦防 adds to `G[0xf384c3]` (`SETILLREVISE@0x10e`). CALCDAMAGE takes the defender's from the same power (`CALCDAMAGE@0x54a`). |
| `G[0x327aed]` | 5 | 0..4 | 1 | 10..30 | What 肉抵 adds to physical resistance, `G[0xf384eb]` (`SETILLREVISE@0x153`), which rolls the resistance-4 conditions. |
| `G[0x327c19]` | 5 | 0..4 | 1 | 10..30 | What 精抵 adds to mental resistance, `G[0xf38513]` (`SETILLREVISE@0x16a`), which rolls the resistance-3 conditions. |
| `G[0x327d45]` | 5 | 0..4 | 7 | −20..10 | What each level adds to physical speed, `G[0xf3853b]` (`SETILLREVISE@0x181`): 肉速, 速低, 束縛 … |
| `G[0x327e71]` | 5 | 0..4 | 7 | −20..10 | What each level adds to mental speed, `G[0xf38563]` (`SETILLREVISE@0x198`): 精速, 速低, 束縛 … |
| `G[0x327f9d]` | 5 | 0..4 | 1 | 2..10 | What 運 adds to luck. SETILLREVISE sums it (`@0x1af`) and applies the sum to nothing. |
| `G[0x3280c9]` | 5 | 0..4 | 2 | 2..10 | What each level adds to critical hits, `G[0xf38653]` (`SETILLREVISE@0x1f4`): 痛打, 高揚. |
| `G[0x3281f5]` | 5 | 0..4 | 5 | −16..5 | The HP each level gives or takes each turn. In battle it goes to `G[0xf3867b]` (`SETILLREVISE@0x222`), and on the map FIELD applies it (`FIELD@0x26b5`): 猛毒, 水流, 燃焼, 強酸, 再生. |
| `G[0x328321]` | 5 | 0..4 | 1 | 1..5 | The MP 魔再 gives each turn: `G[0xf386a3]` in battle (`SETILLREVISE@0x234`), and FIELD on the map. |
| `G[0x32844d]` | 5 | 0..4 | 1 | 1..5 | The TP 技再 gives each turn: `G[0xf386cb]` in battle (`SETILLREVISE@0x246`), and FIELD on the map. |
| `G[0x328579]` | 5 | 0..4 | 2 | −10..−5 | What 強酸 and 恐怖 take each tick, `G[0xf386f3]` (`SETILLREVISE@0x258`). BTLMAIN reads it too (`BTLMAIN@0x9ce`, `@0xa00`). Taking it from the action gauge is an inference; bg0 and Verita do so. |
| `G[0x3286a5]` | 1 |  | 45 | 1×24, 2×21 | 1 beneficial, 2 an ailment. An attacker with family 4 (破術) lowers the defender's beneficial conditions by its level (`CALCILL@0x3b`..`@0x67`). BTLMAIN, FIELD, CHECKSS2 and the RTN scripts read it too. |
| `G[0x3286e1]` | 1 |  | 14 | 1 | 1 when the condition fails on a unit whose character has CHINIT's `G[0x15a92a + char]` set ("Characters, voices, fates and the books"; inferred to be a boss, as the descriptions say ※ＢＯＳＳ無効) (`CALCILL@0x4ed`..`@0x51e`). |
| `G[0x32871d]` | 1 |  | 37 | 2×21, 3×14, 4×1, 7×1 | Which cures clear the condition, as bits. Bit 2 clears it when a battle ends: BTL puts the level back to its floor (`BTL@0x368`..`@0x3ca`). RECOVER clears the conditions that share a bit with its mask (`RECOVER@0xd3`..`@0x115`). The reading follows Verita, where the same field is condition_cure_bits. |
| `G[0x328759]` | 1 |  | 44 | 101..119 | The condition's icon. DRAWFRAME (`@0xdab`) and DRAWUNPARAM (`@0x5a0`) draw it. |
| `GS[0x110d0]` | 5 | 0..4 | 50 | 即死 / 喪意 / 石化 / HP吸1 … | The name of each level (`ILINIT@0xa`). DRAWSTATUS (`@0x176c`) and DRAWUNPARAM (`@0x617`) draw it. The tool reports this array as `GS[0x110cb]` (see above). |

A battle unit's levels are `G[0xf39823 + 60 × unit + cond]`, and its floor is `G[0xf3a183 + 60 × unit + cond]`.
SETCH adds the floor from the unit's 【常時】 special skills. CALCILL does not lower a level below the floor
(`CALCILL@0x4b3`..`@0x4d7`), and does not raise one past 5 (`@0x496`). SETCH and SETEN call SETILLREVISE, which sums
the 21 modifier arrays over the unit's levels into the unit's figures (`SETILLREVISE@0x5a`..`@0x26d`). It also sets
bit 6 of the unit's flags `G[0xf381cb]` while it has 石化 (condition 3), and bit 7 while it has 魅了 (condition 8)
(`SETILLREVISE@0x0`..`@0x55`).

### COINIT (a character's look in one costume)

COINIT holds one record at a time: a character's pictures and battle voices in one costume.
- **Selectors.** The caller sets the character `G[0xbdc65e]` and the costume `G[0xbdc65f]`, then walks the chain
  `G[0xf45d6d]` (indexed by `G[0xcf308]`).
- **Clearing.** COINIT clears the record's cells `G[0xbdc660]`..`G[0xbdc71f]` (`COINIT@0x0`..`@0x3c`), then
  branches on the character (`@0x41`) and the costume.
- **At boot.** INIT2 runs it with the selectors at 0, so the boot image holds only zeros. The records here were read
  from the listings: 284 in the base layer, 27 in `$1$` (costumes 24 and 25) and 43 in `$2$`.
- **Characters.** The ids run from 1 to 0x3c2, and enemies are among them. Enemy 0x12d, for one, writes only +3,
  +0xc, +0xd and two voices.
- **Costume.** It is 0 unless something changes it. For セリカ (character 1) it is 2 while the saved `goddess_gauge` `G[0x7250]` is
  at most 200, and 1 while it is at most 400. An equipped item's costume `G[0x38a217 + item]` ("Items, stones, improvement and alchemy") overrides
  it, for each item in the saved equipment `G[0x404f0 + 5 × char + slot]` (`CHANGELEADER@0x139`..`@0x1d6`,
  `BTLMAIN@0x6c24`).
- **Other callers.** Eighteen scripts walk the chain, from the battle (BTLMAIN) and the field (FIELD, OBJACT,
  CHANGELEADER) to the list screens (DRAWCHLIST, SAVE, SELCH, SHOP). ALCHEMY and BREED reuse `G[0xbdc65e]` as a
  passing character id.

| cells (offset) | shape | records | holds |
|---|---|---|---|
| `G[0xbdc660]` (+0) | 1 | 113 | The status picture: CSS001AAA, ABA or ACA for costumes 0, 1 and 2 (`DRAWSTATUS@0x82`). Enemies have an MS picture. |
| `G[0xbdc661]` (+1) | `[3]` | 36 to 354 | CB pictures. Cell +3 is written for every record: the face that the lists draw (`DRAWCHLIST@0x35c`, `SAVE@0x23ab`, DRAWDEPLOY, DRAWSKLIST, SELCH, SHOP, RESULT). BTLMAIN copies all three to the unit's `G[0xf4519b + 3 × unit]`, and CHMENU and SETENP read them too. |
| `G[0xbdc664]` (+4) | `[2]` | 29 | CI pictures, copied to `G[0xf45213 + 2 × unit]`. *(not decoded)* |
| `G[0xbdc666]` (+6) | 1 | 29 | 1..3, copied to `G[0xf45263 + unit]` for DRAWUNIT. *(not decoded)* |
| `G[0xbdc667]` (+7) | `[14]` | 36 to 351 | The battle pictures (CPB…), one a unit state. BTLMAIN copies them to `G[0xf4528b + 14 × unit]`, and DRAWUNIT picks one by the unit's flags `G[0xf381cb]` (`DRAWUNIT@0x156`). Cells +0xc and +0xd are written for nearly every record, enemies included. |
| `G[0xbdc675]` (+0x15) | `[5]` | 36 | Values 4..12. With +0x1a they are the two 5-wide argument rows ANIME passes to its source-rect animation (`ANIME@0x572`). *(not decoded)* |
| `G[0xbdc67a]` (+0x1a) | `[5]` | 36 | Values 2..8; see +0x15. |
| `G[0xbdc67f]` (+0x1f) | `[26]` | 6 | CPM field pictures (DRAWWALK, FIELD, OBJACT). |
| `G[0xbdc699]` (+0x39) | `[26]` | | CPW field pictures (DRAWWALK, DRAWCH, DRAWJUMP, DRAWHOIST, FIELD, CHANGELEADER, SETMAPTEX). |
| `G[0xbdc6b3]` (+0x53) | `[26]` | | CPR field pictures (FIELD, OBJACT). |
| `G[0xbdc6cd]` (+0x6d) | `[26]` | | CPJ pictures, which DRAWJUMP (`@0x19b`) and DRAWHOIST draw. |
| `G[0xbdc701]` (+0xa1) | 1 | | The CPC picture CARRY draws (`CARRY@0x855`). |
| `G[0xbdc702]` (+0xa2) | `[30]` | | The battle voice clips (SER0001.OGG …). BTLMAIN copies them to `G[0xf4564b + 30 × unit]`. CHMENU, RESULT and SETENP read them too. |

BTLMAIN loads a unit's look and copies it into the per-unit arrays named above, then calls DRAWUNIT
(`BTLMAIN@0x23c1`..`@0x249c`).

### RTINIT (battle routines)

Leader `G[0x1effcd]`, capacity 100000 with 100 slots a row. The id is `100 × routine + step`, so a routine has up to
100 steps. The records are 2877 steps of routines 1 to 962: 2360 in the base layer, 60 in `$1$` and 457 in `$2$`.
There are ten arrays, one each 100000 cells; `G[0x28278d]` between them is never written. BTLMAIN sets the routine
in play `G[0x1effca]` from the character's CHINIT `G[0x15b8ca + char]` ("Characters, voices, fates and the books"; `BTLMAIN@0x1098`). RTN then walks
the steps `G[0x1effcb]` and runs each step's rule script. A rule leaves 1 in `G[0x1effcc]` when its test holds
(`RTN010@0x16d`), and RTN takes the first such step (`RTN@0x2ce`, `@0x2fa`).

| array | cells per id | ids | values | holds |
|---|---|---|---|---|
| `G[0x1effcd]` | 1 | 2875 | 1..306, 29 distinct | The step's rule, RTN000..RTN307: RTN runs local `7 + rule`, the scripts 0x3bda..0x3bfe (`RTN@0x188`). 0 ends the routine. |
| `G[0x20866d]` | 1 | 2875 | 2..100 | The percent chance that the step runs (`RTN@0x2a6`). |
| `G[0x220d0d]` | 1 | 2138 | 1..5403 | The step's action, an ITINIT id the rule puts in `G[0x328795]` for CHECKACT (`RTN306@0x47`). |
| `G[0x2393ad]` | 1 | 1145 | 1..840 | The rule's first argument. For RTN010 it is a gauge: 1 HP, 2 MP, 3 TP, 4 a fourth (`RTN010@0x10`, `@0x3c`..`@0xed`). |
| `G[0x251a4d]` | 1 | 830 | 2..99 | The percent of that gauge the rule tests against (`RTN010@0x20`). |
| `G[0x26a0ed]` | 1 | 183 | 1×19, 2×164 | A third argument, read by rules such as RTN103 (`RTN103@0x30`). It is a target count in bg0 and Verita; that it is one here is an inference. |
| `G[0x29ae2d]` | 1 | 3 | 21 | A story flag the step needs, `G[0xee7 + flag − 1]` (`RTN@0x1c1`; the story flags of "Scenes and screens"). |
| `G[0x2b34cd]` | 1 | 205 | 1..5 | How many times the step may run in a battle, against the unit's count `G[0xf38883 + 100 × unit + step]` (`RTN@0x1f1`, `@0x20e`). |
| `G[0x2cbb6d]` | 1 | 89 | 4..6 | For a party unit (below 6), the tactics that skip the step: bits tested against the character's saved tactics setting `G[0x4cc8c + 6 × char + 1]` (`RTN@0x26a`). |
| `G[0x2e420d]` | 1 | 186 | 1×171, 6×15 | A bit of the saved bit array `G[0x6e3a]`; the step is skipped while the bit is set (`RTN@0x22c`..`@0x257`). |

A party unit whose tactics setting is 3 or 4 runs RTN202 (local 0xd1) or RTN201 instead of a routine (BTLMAIN).

### BTANINIT2 (battle animations)

Leader `G[0xef8696]`, capacity 20000, six slots an animation. 689 animations: 667 in the base layer and 22 in `$2$`.
An action's animation is ITINIT's `G[0x6ea637 + action]` ("Items, stones, improvement and alchemy"). BTLMAIN puts it in `G[0xef857c]` (`@0x226a`,
`@0x28a0`), and ANIME reads it (`ANIME@0x11`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xef8696]` | 6 | 0..5 | 689 | 2..1001 | The BTANINIT effect each of the six slots plays, 0 for none (961 and 2 on the first; `SETEFFECT@0x79`). |
| `G[0xf15b56]` | 6 | 1..5 | 689 | 10..1200 | When each slot's effect starts, in ms (`ANIME@0x156`, `@0x16f`). |
| `G[0xf33016]` | 1 |  | 687 | 400..1500 | The animation's length in ms (`ANIME@0x12d`, `@0x595`). |

### BTANINIT (battle effects)

BTANINIT holds one effect at a time.
- **Selectors.** SETEFFECT sets the effect `G[0xef857a]` and the slot `G[0xef857b]`, then walks the chain
  `G[0xf45e4d]` (`SETEFFECT@0x5c`..`@0xba`). The effect's cells go into slot arrays of 46 slots.
- **Slots.** Slots 0..5 are the animation's six. Slots 6..45 are the 40 units' condition effects, from
  `G[0xef866e + slot − 6]` (`SETEFFECT@0x79`, `@0xb3`).
- **Records.** There are 348 effects, read from the listings: 343 in the base layer and 5 in `$2$`.
  Their ids are 1..8, 101.. and up to 1001.

| array | holds |
|---|---|
| `G[0xf37e36]` | The picture: an AMnnn.AGF sheet, or an MVBnnn.MPG movie (`ANIME@0x1403`, `@0x1482`). |
| `G[0xf37e64]` | 0 for a movie; 1 or 2 for a still sheet. The value 1 takes another path (`ANIME@0x145f`..`@0x147b`, `@0x2290`). |
| `G[0xf37e92]` | The blend mode `gfx-draw-color` gets (`ANIME@0x2272`..`@0x2287`); 1 on every effect. |
| `G[0xf37ec0]` | The width, 160..480 (`ANIME@0x14ab`, `@0x21f5`). |
| `G[0xf37eee]` | The height, 96..600 (`ANIME@0x14b2`, `@0x2218`). |
| `G[0xf37f1c]` | A still sheet's columns, 1..7 (`ANIME@0x22ac`). |
| `G[0xf37f4a]` | A still sheet's rows, 1..8. Only SETEFFECT reads it. |
| `G[0xf37f78]` | A still sheet's frame count, 1..30 (`ANIME@0x22a5`); AM001 is 4 × 5 = 20. |
| `G[0xf37fa6]` | The effect's length in ms, 300..1000 (`ANIME@0x14d2`). |
| `G[0xf37fd4]` | Where the effect is drawn, 1 or 2. For slots 0 and 1 it is drawn at the attacker, and 2 spans the attacker's size `G[0xf450d3]` × `G[0xf450fb]` (`ANIME@0x150d`..`@0x157b`). Slots 3 and 4 are drawn at each target unit, `G[0xef857e]` (`@0x15b7`..`@0x15fe`). |
| `G[0xf38002]` | 1 places the effect from a unit's cell (`ANIME@0x2131`, `@0x2189`); every effect has 1. |
| `G[0xf38030]` | An x offset, −20..90 (`ANIME@0x222d`). |
| `G[0xf3805e]` | A y offset, −30..220 (`ANIME@0x2240`). |
| `G[0xf3808c]` | The scale in percent, 80..300, the same on both axes (`ANIME@0x22c0`..`@0x22dc`). |
| `G[0xf380ba]` | A sound effect, BTSE….WAV (`ANIME@0x179f`, `@0x17ad`). |
| `G[0xf380e8]` | The sound's delay in ms after the slot starts, 20..250 (`ANIME@0x17d9`..`@0x17e7`). |

### The battle units

The battle keeps 40 units, indexed by the unit slot. Slots below 6 are the party's. The attacker is `G[0xf381a0]`,
the defender `G[0xf381a1]`, and the unit SETCH or SETEN is building `G[0xf381a2]`. The per-unit arrays this section
reads are:
- `G[0xf381a3]`, the unit's character;
- `G[0xf381cb]`, its flags;
- HP and maximum HP `G[0xf38293]` and `G[0xf382bb]`, MP `G[0xf382e3]` and `G[0xf3830b]`, TP `G[0xf38333]` and
  `G[0xf3835b]`. The six come from `BTLMAIN@0x2d5`..`@0x3fc`;
- `G[0xf3871b + 5 × unit]`, the equipment;
- the attack and defence elements `G[0xf38833]` and `G[0xf3885b]`;
- the condition levels and floors;
- the special skill levels `G[0xf3b493]` and `G[0xd0a83f]`;
- the routine step counts `G[0xf38883]`.

All of them lie above the prefix, so a battle is not saved.

### What is saved

None of these tables' arrays, and no battle unit cell, lies below the prefix. A save keeps these character cells
("Characters, voices, fates and the books", cited by address), which the battle scripts read and write:

| cells | holds | written by | read by |
|---|---|---|---|
| `G[0x25f10 + 500 × char + family]` | A character's learned special skill levels | OPENFACT adds a fact's families (`OPENFACT@0x150`..`@0x171`); GAMESTART clears 100 × 500 (`GAMESTART@0x9f4`) | SETCH adds them into the unit's levels (`SETCH@0x78a`); CALCREVISE (`@0x52`); CHMENU (`@0x42d7`) |
| `G[0x40810 + 500 × char + family]` | The player's switch that turns a family off | CHMENU (`@0x1949`..`@0x196b`) | SETCH zeroes the level unless the family's level is always on (`SETCH@0x8a1`) |
| `G[0x218c0 + 60 × char + cond]` | A character's condition level, 0..5 | ADDILL (`@0x338`..`@0x35c`); CALCREVISE (`@0x260`..`@0x312`); BTL at a battle's end (`@0x37d`) | SETCH; RECOVER; FIELD |
| `G[0x23030 + 60 × char + cond]` | The condition floor from the character's 【常時】 skills | CALCREVISE copies the unit's floor `G[0xf3a183]` into it and moves the level by the change (`CALCREVISE@0x225`..`@0x2a5`) | ADDILL: a level at its floor lasts until cured (`@0x398`..`@0x3bc`); BTL; RECOVER |
| `G[0x247a0 + 60 × char + cond]` | A condition's remaining duration | ADDILL: 0 at level 0, −1 at the floor, else condition_durations (`@0x37a`..`@0x43e`); CALCREVISE; BTL | FIELD and the battle |
| `G[0x3e5b0 + 80 × char + kind]` | A character's rank in each item kind | GAMESTART clears it (`@0xa0e`) | CHECKACT against an item's rarity (`@0x938`); SETSKLIST; INFOSK2 |
| `G[0x404f0 + 5 × char + slot]` | The equipment | UNITECH, SETCH, CALCREVISE and RETIRECH ("Items, stones, improvement and alchemy") | COINIT's costume (`CHANGELEADER@0x139`); when 呪い is lifted to its floor, ADDILL takes off the items that carry it (ITINIT `G[0x4f3757 + 60 × item + 7]`, "Items, stones, improvement and alchemy") and puts them back in the bag (`@0x443`..`@0x4f2`) |
| `G[0x4cc8c + 6 × char + 1]` | The tactics setting | CHMENU | RTN's tactic mask (`@0x26a`); BTLMAIN picks RTN201 or RTN202 for 3 and 4 |
| `G[0x6e3a]` | A bit array a routine step tests | INFOFT, GAMESTART, TOWN | RTN (`@0x249`) |
| `G[0xee7 + flag − 1]` | The story flags | the scenes | RTN (`@0x1c1`) |
| `goddess_gauge` `G[0x7250]` | The goddess gauge ("The town"): セリカ's costume follows it, 2 while it is at most 200 and 1 while it is at most 400 | INN, BTL, ITEM and the scenes | CHANGELEADER, BTLMAIN (`@0x6c24`) |

What each copy reaches:
- A condition gained in battle reaches the save through CALCREVISE and BTL.
- A special skill level reaches the save only through OPENFACT (the 憑石 facts, inferred from the name) and the
  saved switch.
- The battle's own figures are rebuilt from the tables on every SETCH.

## Encounters and enemies

A random battle in the field draws on four tables in turn. As the party walks, FIELD rolls the chance in EEINIT. When
the roll hits, FIELD first tries the special encounters of ELINIT, the 視線を感じる and 異質な気配がする meetings, and
otherwise draws one entry of the encounter set ENINIT loaded for the map; the entry names up to three formations
(ESINIT), which SETENP places on the battle's grids. An enemy is a character of CHINIT: SETEN builds each one from the
character's fields and the map's enemy level. A fixed battle, a story battle or a special encounter's, skips the draw:
its caller sets the battle id `G[0xc20a5]`, and the DUINIT chain `G[0xf45e2d]` names its formations.

INIT2 runs EEINIT and ELINIT at boot (`INIT2@0x42`, `@0x45`), and `$1$AUTORUN` and `$2$AUTORUN` run their fragments at
the same offsets; all four fragments are empty. ENINIT and ESINIT run when the game needs them, through their hook
chains, each layer in turn:

| table | chain | callers | what it loads |
|---|---|---|---|
| ENINIT | `G[0xf45ded]` (slots 0 to 2: ENINIT, `$1$ENINIT`, `$2$ENINIT`) | FIELD on entering a map (`FIELD@0x648`..`0x69f`), and eleven map scripts after they change the map's variant (`MP091@0xdd0`..`0xe1d`, `MP092`..`MP095`, `MP421`, `MP431`, `MP461`, `MP521`, `MPEV091`, `$1$MP811`) | the encounter set `encounter_set_in_play` `G[0xd09f06]` |
| ESINIT | `G[0xf45e0d]` (slots 0 to 2: ESINIT, `$1$ESINIT`, `$2$ESINIT`) | SETENP, once for each of the battle's three formations (`SETENP@0x4f5`..`0x539`) | the formation `formation_in_play` `G[0xd0a15f]` |

Each walker runs the 32 slots with `G[0xcf308]` (FIELD) or a local (SETENP). The base table clears its arrays first
(`ENINIT@0x0`..`0xf`, `ESINIT@0x0`..`0x32`) and the fragments clear nothing; each set and each formation is defined in
exactly one layer, so the layer that has it writes it into cleared arrays. Every array of the four tables lies above
the saved prefix, so none is saved ("What is saved" below).

### EEINIT (encounter chances)

EEINIT is five `copy-inline-int-array` rows into `G[0xd0a1b1]`, ten cells a row (`EEINIT@0x0`..`0x14`). It needs no
leader. The values are close to 戦女神ZERO's (bg0-data-tables.md, EEINIT), with larger chances at the high levels.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xd0a1b1]` | 10 | 0..9 | 5 | 0..50, 19 distinct | The percent chance of a battle at a field roll: the row is the distance band, the column the encounter level (`FIELD@0x29da`..`0x2b5c`). |

| row | values |
|---|---|
| 0 | 0 0 0 0 0 1 1 2 3 4 |
| 1 | 0 0 0 0 0 3 3 6 8 10 |
| 2 | 0 1 1 2 3 6 7 10 16 15 |
| 3 | 0 1 3 4 6 9 10 16 28 30 |
| 4 | 0 2 5 6 9 12 15 24 36 50 |

FIELD adds each step's distance to `field_distance` `G[0x4dd7e]` and rolls each time it passes another 20
(`FIELD@0x25a7`..`0x25c3`). The band, its local `0x30f` = `G[0x4dd7e]` / 20, is set only when all of these hold
(`FIELD@0x25ca`..`0x2617`); otherwise it stays 0 and band 0 never rolls (`FIELD@0x29da`):

- an encounter set is loaded, `G[0xd09f06]` > 0;
- `field_encounters_limited` `G[0x4dd7f]` is 0, or the area's kind `G[0xc9eee9 + G[0xb8463]]` is 1 (MPINIT's areas 66
  to 74 are kind 1);
- `G[0x4dd85]` is 0 (eight map scripts set it, `MP021@0x7a4`) and `G[0xf45f4f]` is 0 (the debug field, set by
  `DEBUGFIELD`).

The roll (`FIELD@0x29e1`..`0x2b5c`):

- The row is the band: below 10, 20, 40, 70, or more.
- The column is `G[0xc2b679 + G[0x4dd6c]]` when it is set, else MPINIT's encounter level of the map,
  `G[0xca51be + G[0x4dd63]]` (1 to 9 in MPINIT), plus `G[0x4dd81]`, held to 0..9; while `G[0x4dd80]` is set the column
  is 0. No table writes any cell of `G[0xc2b679]`..`+100`, and no script writes `G[0x4dd80]` or `G[0x4dd81]` but FIELD
  itself, which clears both on entering (`FIELD@0xde`, `@0xe3`), so the map's level is always the column.
- The chance falls with 忍び足 (skill 162): the party's best level of it among the five members `G[0x1fdad + k]`, from
  `G[0x32260 + 500 × character + 162]`, gives SSINIT's value for that level, -10, -20 or -30
  (`G[0x2fff5f + 5 × 162 + level - 1]`), and the chance becomes chance × (100 + value) / 100 (`FIELD@0x2ac8`..`0x2b49`).
- A draw below the chance starts a battle (`FIELD@0x2b50`..`0x2b69`): FIELD calls `label_3ac2`, below.

`FIELD@0x167e`..`0x16a9` starts a battle the same way without the roll, unless `G[0x4dd85]` is 1 or no set is loaded.

### ELINIT (special encounters)

Leader `G[0xd0a1e3]`, capacity 100. 18 records, ids 1 to 27, all in the base; `$1$ELINIT` and `$2$ELINIT` are empty.
FIELD walks ids 1 to 99 (`FIELD@0x3b3f`..`0x3b55`). The tool's check passes: the nine bases agree modulo 100, and no
write comes ahead of a record's first.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xd0a1e3]` | 1 |  | 18 | 1..2, 2 distinct 1×5, 2×13 | The kind; 0 skips the id (`FIELD@0x3b5c`). 1 shows 視線を感じる……, 2 異質な気配がする…… (`FIELD@0x3bfb`..`0x3c2d`); in battle kind 2 draws its own banner and opening picture (`BTLMAIN@0x6506`..`0x652c`, `@0x67cd`..`0x67f0`). |
| `G[0xd0a247]` | 1 |  | 7 | 1..26, 7 distinct | A special encounter that must be beaten first: its state's bit 1 (`FIELD@0x4134`..`0x416a`). They make two chains, 1 → 2 and 21 → 22 → … → 27. |
| `G[0xd0a2ab]` | 1 |  | 0 |  | Not written. FIELD reads it at `G[0xc0caa0]`, not at the encounter's id, and then requires `G[0x72be + value]` & 3 = 3 (`FIELD@0x422d`..`0x426a`); with every cell 0 the test never applies. |
| `G[0xd0a30f]` | 2 | 0..1 | 18 | 27..9000, 12 distinct | Two flags that must be on, `G[0xee7 + flag - 1]` = 1; 0 for none (`FIELD@0x416b`..`0x41c9`). Ids 11 to 27 need flag 9000. |
| `G[0xd0a3d7]` | 2 | 0..1 | 11 | 1194..3951, 13 distinct | Two flags that bar it when on (`FIELD@0x41cc`..`0x422a`). The scene ids' flags: id 5 plays scene 3151 and is barred by flag 3153. |
| `G[0xd0a49f]` | 1 |  | 18 | 8..14, 2 distinct 8×11, 14×7 | The area kinds it appears in, a bit mask over the area's kind `G[0xc9eee9 + G[0xb8463]]` (`FIELD@0x3b6a`..`0x3b7f`, `check-bit`): 8 is kind 3 (MPINIT's areas 1 to 13), 14 kinds 1 to 3. An area of kind 0 has none (`FIELD@0x3ac2`..`0x3ade`). |
| `G[0xd0a503]` | 5 | 0..4 | 18 | 1..15, 5 distinct | The percent chance at a battle roll, by the band of `goddess_gauge` `G[0x7250]`: column (gauge - 1) / 200 (`FIELD@0x3b86`..`0x3b94`). 呼び寄せ (skill 161) adds SSINIT's 1, 5 or 10 for the party's best level (`FIELD@0x3ae5`..`0x3bd9`). |
| `G[0xd0a6f7]` | 1 |  | 9 | 3151..3950, 9 distinct | A scene played instead of a battle, a scene number: FIELD puts it in `G[0xe7a0f]` and its script `G[0xef2af + scene]` in `G[0x3b1c]` (`FIELD@0x3cac`..`0x3ce6`). |
| `G[0xd0a75b]` | 1 |  | 9 | 905..913, 9 distinct | The battle, a DUINIT id, into `G[0xc20a5]` (`FIELD@0x3d3e`..`0x3d45`); FIELD then runs the DUINIT chain `G[0xf45e2d]` (`FIELD@0x3d7f`..`0x3db6`). DUINIT has a branch for each (`DUINIT@0x26c5`..`0x2815`). |

How FIELD uses them (`label_3ac2`, `FIELD@0x3ac2`..`0x3e8f`): in an area of a kind other than 0, and while `G[0x4dd86]`
is 0 (38 map scripts set it, `MP091`), it walks the ids. An id is tried when its kind is set, the area's kind is in its
mask, a draw of 100 falls below its chance, and `label_4119` passes: the id is not beaten yet (bit 1 of its state), its
prerequisite is beaten, its flags allow it. The first id that passes shows its line and the choice 「待ち構える」 or
「身を隠してやり過ごす」 (`FIELD@0x3c3c`..`0x3c8c`, BUNKI). Hiding ends it (`FIELD@0x3e84`). Waiting sets bit 0 of its
state (`FIELD@0x3ca0`) and plays its scene, or starts its battle with `G[0xc20a5]` = its battle and the three
formations cleared (`FIELD@0x3d15`..`0x3d7f`). `special_encounter_in_play` `G[0xcdaa4]` holds the id meanwhile; BTL
sets bit 1 of the state when the battle's result `G[0xc20ad]` is 0, a win (`BTL@0x2aa`..`0x2cd`) and clears the id after it
(`BTL@0x57c`). When no id passes, FIELD draws from the encounter set (`label_3e92`, below).

| id | kind | needs | barred by | areas | chance by band | scene | battle |
|---|---|---|---|---|---|---|---|
| 1 | 1 | flags 8062, 3140 | flag 2222 | kind 3 | 15 each | | 912 |
| 2 | 1 | 1 beaten; flags 8062, 27 | flag 2223 | kind 3 | 15 each | | 913 |
| 5, 6, 7 | 1 | flags 1092, 1197, 1199 | 3153, 3362, 3364 | kind 3 | 15 each | 3151, 3360, 3362 | |
| 11 to 16 | 2 | flag 9000 and one more (8999, 7041, 3161, 3251, 1194; 12 none) | one or two scene flags | kind 3 | 3 2 1 1 1 (13: 4 3 2 2 2) | 3950, 3160, 3250, 3165, 3252, 3253 | |
| 21 to 27 | 2 | the previous id beaten (21 none); flag 9000 | | kinds 1 to 3 | 3 2 1 1 1 (24, 26: 1 1 1 2 3) | | 905 to 911 |

### ENINIT (encounter sets)

The selector is `encounter_set_in_play` `G[0xd09f06]`. On entering a map FIELD sets it from MPINIT,
`G[0xca421e + 2 × map + variant]`, the map being `G[0x4dd63]` and the variant `map_encounter_variant` `G[0x4dd82]`
(`FIELD@0x648`..`0x662`), then runs the chain. FIELD clears the variant on entering (`FIELD@0xe8`); a map script sets it
to 1 when the map's state changes, and loads the set again (`MP091@0xd78`..`0xe1d`: map 91's sets are 22 and 21). Set
0 loads nothing (`ENINIT@0x14`) and turns encounters off.

ENINIT clears three arrays of 100 entries and one of 300 (`ENINIT@0x0`..`0xf`) and fills them in one branch a set:

| array | cells | holds | evidence |
|---|---|---|---|
| `G[0xd09f07]` | 100 | The entry's weight; 0 skips it. 3 to 100. | `FIELD@0x3ec8`, `@0x40b5` |
| `G[0xd09f6b]` | 100 | The low end of the entry's window on `goddess_gauge` `G[0x7250]`: 1, 201 or 401; 0 when unwritten. An entry with a low above the gauge is skipped. | `FIELD@0x40cb`..`0x40e0` |
| `G[0xd09fcf]` | 100 | The high end: 200, 400 or 1000. When it is at least the low and below the gauge, the entry is skipped. | `FIELD@0x40e1`..`0x4112` |
| `G[0xd0a033]` | 300 | The entry's three formations (ESINIT ids), into `G[0xc20a6]`, `G[0xc20a7]` and `G[0xc20a8]`; 0 for none. | `FIELD@0x3f57`..`0x3f82` |

The base has 43 sets, 10 to 45 and 47 to 53; `$1$ENINIT` adds 61 to 67 and `$2$ENINIT` 68 and 69. A set has 1 to 28
entries, from entry 0 up. 771 entries name a first formation, 153 a second, one a third; every formation named is
defined in ESINIT.

How FIELD draws (`label_3e92`, `FIELD@0x3e92`..`0x3f82`): it sums the weights of entries 0 to 99 that fit the gauge
(`label_40b0`), draws below the sum and takes the entry the draw falls in. With no entry fitting it starts no battle
(`FIELD@0x3efd`..`0x3f0b`); there is no fallback entry, unlike 戦女神ZERO. It copies the entry's formations, clears
`G[0xc20a5]` (no fixed battle) and `G[0xc20a9]`..`G[0xc20ab]`, sets `G[0xc20ae]` to -1 and `system_mode` `G[0x0]` to 4
(`FIELD@0x3f87`..`0x3fb5`), and `field_distance` goes back to 0 (`FIELD@0x3e89`, `BTL@0x3d5`).

### ESINIT (formations)

The selector is `formation_in_play` `G[0xd0a15f]`. SETENP sets it to each nonzero formation `G[0xc20a6 + k]`, k = 0 to
2, and runs the chain (`SETENP@0x4c6`..`0x539`). ESINIT clears ten arrays of eight slots and one cell
(`ESINIT@0x0`..`0x32`):

| array | cells | holds | evidence |
|---|---|---|---|
| `G[0xd0a160]` | 8 | The slot's enemy, a character id (CHINIT), into `G[0x158dd1]` for SETEN; 0 for an empty slot. 315 characters, 301 to 962. | `SETENP@0x57a`, `@0x787` |
| `G[0xd0a168]` | 8 | The first column (1 to 5) the enemy may stand in; 0 for any. | `SETENP@0x793`..`0x7fe` |
| `G[0xd0a170]` | 8 | The last column; when it is not above the first, the first only. | `SETENP@0x7b5`..`0x7fe` |
| `G[0xd0a178]` | 8 | The first row (1 to 5); 0 for any. | `SETENP@0x803`..`0x86e` |
| `G[0xd0a180]` | 8 | The last row. | `SETENP@0x825`..`0x86e` |
| `G[0xd0a188]` | 8 | The percent chance the slot is used, 10 to 80; 0 for always. A draw at or above it drops the slot. | `SETENP@0x6fa`..`0x722` |
| `G[0xd0a190]` | 8 | A flag that must be on for the slot, `G[0xee7 + flag - 1]` (71, 81, 3506, 7015). | `SETENP@0x723`..`0x754` |
| `G[0xd0a198]` | 8 | A flag that drops the slot when on (71, 81, 3017, 3506, 7015). | `SETENP@0x755`..`0x786` |
| `G[0xd0a1a0]` | 8 | The item the enemy carries, an ITINIT id, into its battler's `G[0xf3b443]`; RESULT gives it when the battle is won (`RESULT@0x5ea`..`0x604`). 100 slots, 100 items. | `SETENP@0xb64`..`0xb72` |
| `G[0xd0a1a8]` | 8 | The slot (from 1) whose battler this one is bound to, into `G[0xf3b46b + battler]`; battle scripts test the bond (`CHECKACT@0x488`, `BTLMAIN@0x3db1`). 25 slots, all bound to slot 1. | `SETENP@0x5ab`..`0x5dc` |
| `G[0xd0a1b0]` | 1 | The formation's mark, 0 or 1, into `G[0xd0a7da + k]` (`SETENP@0x539`); BTLMAIN draws each formation's tag from row 0xf1 + 61 × mark of its sheet and the current one's from 0x341 + 45 × mark (`BTLMAIN@0x5b36`..`0x5b90`). The 101 marked formations all but one (8201) have a slot carrying an item. | `ESINIT@0x32` |

The base has 516 formations, 101 to 9673; `$1$ESINIT` adds 23 (8001 to 8034, 9674, 9675) and `$2$ESINIT` 40 (8101 to
8219, 9676 to 9684). A formation has one to eight slots. 232 formations are named by no encounter set: DUINIT and the
story scripts use them, and only formation 404 is named by neither ENINIT nor DUINIT.

Placement (`SETENP@0x873`..`0xa7b`): the squad's grid is five rows of five columns (`lookup-array-2d` on local `0xe`,
row major). The enemy takes a block of `G[0x15a542 + character]` rows and `G[0x15a15a + character]` columns. Every free
place gets a key, a draw of 100 plus 100 for each row or column it lies outside the slot's ranges, and the smallest
key wins (`sort-indices-by-key-sum`); the column goes to `G[0xf45123 + battler]` and the row to `G[0xf4514b + battler]`.
Formation k's battlers are 10 + 10 × k onward (`SETENP@0x54f`..`0x556`), and `G[0xd0a7e1]` is the first k with a
formation. Then SETEN builds the battler.

### SETEN (an enemy's stats)

SETEN builds battler `G[0xf381a2]` from character `G[0x158dd1]` (`SETEN@0x0`..`0x7b6`); the character's fields are
CHINIT's ("Characters"), cited here by address:

- The first time a battler is set up, the character's `enemy_book_flags` cell `G[0xc5377 + character]` becomes 1 and
  is stored in the shared profile (`SETEN@0x5b`..`0x6e`).
- The level starts at the map's enemy level, MPINIT's `G[0xca49ee + G[0x4dd63]]` (`SETEN@0xc0`). While `G[0x6e3a]` is
  at least 1 and the character's `G[0x1ddeba]` is above 0, a level below the leader's, `G[0x20790 + G[0x7259]]`, rises
  by the difference divided by `G[0x1ddeba]` (`SETEN@0xcc`..`0x120`). MPINIT's spread `G[0xca4dd6 + map]` adds a draw
  of −spread to +spread (`SETEN@0x127`..`0x15d`). The level is held to the character's `G[0x1dd6ea]` (lowest) and
  `G[0x1ddad2]` (highest), into `G[0xf381f3 + battler]` (`SETEN@0x164`..`0x1b0`). Battler 6 takes battler
  `G[0xf381a0]`'s level instead (`SETEN@0xa3`..`0xbd`).
- The 18 stats are the character's `G[0x1dee5a]` row, plus (level − lowest level) × its growth `G[0x1e34aa]` row / 100
  for the first 14 (`SETEN@0x1c4`..`0x25c`). While `G[0xcb32c]` is above 0, an enemy's (battler 10 or more) first 13
  stats gain `G[0xcb32c]` × 10 percent and its stat 16, `G[0xf45c23]`, gains `G[0xcb32c]` × 10 (`SETEN@0x25f`..`0x2d1`).
- While `G[0x6e3a]` is at least 2 the enemy wears its five equipment items `G[0x1dbf7a]`, `G[0x1dc362]`, `G[0x1dc74a]`,
  `G[0x1dcb32]`, `G[0x1dcf1a]` and adds their bonuses `G[0x6186d7]` (`SETEN@0x2d8`..`0x3cd`).
- Its skill levels are the character's `G[0x15d80a]` row of 500 plus its equipment's, held to 0..5 (`SETEN@0x602`..
  `0x6aa`). SETENP then raises `G[0xcad4f + skill]`, in the shared profile, to the battler's level of each skill
  (`SETENP@0xa83`..`0xafa`).

### What is saved

None of the four tables is saved: EEINIT and ELINIT are written on every boot and load, and the encounter set and the
formations are loaded again before each use, so a change to any of them reaches a saved game. FIELD loads the set
again each time it enters a map, so a save made on a map gets the changed set when the map is entered after loading.

What the encounters keep lies below the prefix and is saved:

| cell | name | holds |
|---|---|---|
| `G[0xc5377]` (1000) | `enemy_book_flags` | 1 for each character met as an enemy (`SETEN@0x5b`); also in the shared profile, so every save shares it. INFO counts those of the characters listed by `G[0x1ef7fa]` (`INFO@0x97c`..`0x9ca`); INFOEN shows the rest as ？？？？ (`INFOEN@0xb35`). |
| `G[0xcf216]` (100) | `special_encounter_states` | By ELINIT id: bit 0 met (the player waited), bit 1 beaten (`FIELD@0x3ca0`, `BTL@0x2c6`). |
| `G[0xcdaa4]` | `special_encounter_in_play` | The ELINIT id being met, 0 for none; BTL clears it after the battle (`BTL@0x57c`), and 43 other scripts, most of them scenes, clear it. |
| `G[0x4dd7e]` | `field_distance` | The distance walked since the last battle. |
| `G[0x4dd82]` | `map_encounter_variant` | Which of the map's two encounter sets is in play. |
| `G[0x4dd7f]` | `field_encounters_limited` | Set at a new game by bit 2 of `G[0x6e3f]` (`GAMESTART@0xe5a`..`0xe68`): random battles then happen only in areas of kind 1. |
| `G[0x7250]` | `goddess_gauge` | A gauge of 0 to `goddess_gauge_maximum` `G[0x7251]` (1000 at most; INIT starts it at 300 of 400, `INIT@0x19`). While character 1 leads the party (`G[0x7259]` = 1), FIELD lowers it by one each time the party walks the area's distance `G[0xc9ee85 + area]` × 10 (`FIELD@0x2480`..`0x24ff`); a battle whose result `G[0xc20ad]` is 1 (`BTLMAIN@0x4ce7`) takes 200 (`BTL@0x25f`..`0x289`); INN, USESKILL and some scenes fill it (`INN@0x791`); DRAWFRAME draws it as five bars of 200 (`DRAWFRAME@0x17d6`). The encounter windows, ELINIT's bands and character 1's look (`SETENP@0xc5c`..`0xc80`) follow it, and so do character 1's conditions 46 (女神) and 47 (抑制), which 18 scripts set from it by the same code: at 400 or less 女神 at level (600 - gauge) / 200, at 601 or more 抑制 at level 2 - (1000 - gauge) / 200, each for good (`BTL@0x58d`, `FIELD@0x4e4c`, `INN@0x1606`, `INITGAME@0x4d`; and five scenes as a library sub, `SC0000@0x2876d`, the ADV module's `set_goddess_gauge_conditions`). Its name in the game is not read. |
| `G[0x7251]` | `goddess_gauge_maximum` | The gauge's top: 400 at the start (`GAMESTART@0x9d7`), and OPENFACT adds 200 (`OPENFACT@0xa3`). |

The battle's formations `G[0xc20a6]`..`G[0xc20a8]` and the fixed battle `G[0xc20a5]` lie below the prefix too; they
belong to the battle ("Skills, conditions and battle").

## Items, stones, improvement and alchemy

Four boot tables and one on-demand table hold what the party carries, equips, improves and makes: ITINIT every
item, piece of equipment, spell, skill and enemy action, in one id space; DPINIT the stones; IMINIT what improving an
item gives; ALINIT the alchemy recipes; and SHINIT the shops' offers. Every array of the four boot tables lies above
the saved prefix (`G[0xcf2e6]`, `GS[0x34f]`), so none of them is saved: INIT2 writes them again on every boot and
every load. No field is copied below the prefix, with one exception: CALCREVISE copies the maxima SETCH computes from
the equipment (`item_stat_bonuses` columns 0..2, with the stones and improvements) into the character's saved
`G[0x20984]`, `G[0x20a4c]` and `G[0x20b14]` (`CALCREVISE@0x8d`..`0x116`), so a changed bonus reaches a loaded game when
CALCREVISE next runs. What the game keeps is the inventory, the improvement state and the equipment slots, below the
prefix (What is saved, at the end).

Each of the three layers only adds records: no cell `$1$` or `$2$` writes of these tables was written by the base,
and none is written ahead of its record's first field.

Several readings share a few vectors and name lists:

- The 22-stat vector (`item_stat_bonuses`, and the stat index of DPINIT's and IMINIT's bonuses, 0..21). SETCH sums
  it into `G[0xf45c13 + k]` and stores column k in the unit's arrays (`SETCH@0x502`..`0x60a`; the unit arrays are
  battle state, the battle section's). Columns 0, 1 and 2 are the maximum HP, MP and TP (CALCREVISE clamps the
  character's HP `G[0x20920]`, MP `G[0x209e8]` and TP `G[0x20ab0]` to them, `CALCREVISE@0x8d`..`0x116`); 11 and 12
  are the speeds the waits subtract (`BTLMAIN@0x6957`..`0x69e7`); 14 is the stone capacity (`SETCH@0x295`). The
  descriptions name 3 物理攻撃 (トロールリング), 4 物理防御 (ガーストリング), 7 and 8 魔力 (コールラン), 9 and 10 a
  resistance to disease and poison (巨神の手; CALCILL reads both, `CALCILL@0x54d`, `@0x593`), 11 and 12 戦術眼
  (メルティナ, グロティナ) and 16 and 18 運気 (メチュカ). For an action the vector holds the action's numbers:
  気合斬り has 250 in column 3, 火炎指弾 180 in column 7; CALCDAMAGE reads columns 16 and 20
  (`CALCDAMAGE@0x463`, `@0x527`), and 16 is 999 on the heals and on 様子を見ている (a hit rate, inferred). Columns 3..8
  likely follow ILINIT's order 物攻 物防 攻回 防回 魔攻 魔防 (沙綾の舞 raises 攻撃回数 and 防御回数), inferred.
- The condition vector (`item_inflicted_conditions`, `item_granted_conditions`): 60 columns by ILINIT condition id
  1..50, named `GS[0x110d0 + 5 × id + level − 1]` (1 即死 … 14 燃焼 … 19 再生 … 50 透明化).
- AFINIT's names: kinds `GS[0x1fcac + kind]` and `GS[0x1fcfc + kind]`, classes `GS[0x1fd9c + class]`, elements
  `GS[0x1fc5c + element]`, and likely the defence attributes `GS[0x1fc7a + value]`.
- The bearer type `G[0x159d72 + character]`, 1..7 (the characters section's), indexes DPINIT's per-type columns.

### ITINIT (items, equipment, spells, skills and enemy actions)

Leader `GS[0x111fc]` (the name), capacity 20000. 1,902 records: 1,656 in the base, 110 new ones from `$1$ITINIT`
and 136 from `$2$ITINIT`. The 46 integer and 2 string arrays agree modulo 20000 from `G[0x328797]`, and no cell falls
outside them. The argument cells `current_item` `G[0x328795]` and `item_count_change` `G[0x328796]` sit just ahead of
the first array (the helpers, below).

One id space holds every kind (`item_kinds`):

| ids | records | what |
|---|---|---|
| 1..31 | 29 | key items (kind 1 重要) |
| 201..999 | 210 | consumables (201..), the enhancement crystals 611..617, the bullets 621..643, treasures 651..686, materials (800..) |
| 1001..1640 | 116 | the stones: 喚石 1001.. (6), 聘石 1201.. (7), 援石 1401.. (64), 憑石 1601.. (39); DPINIT holds the rest of them |
| 2000..2799 | 282 | weapons (kind 6), classes 1..15 |
| 2800..3199 | 127 | armour (kind 7) |
| 3200..3399 | 16 | shields (kind 8) |
| 3400..3999 | 56 | accessories (kind 9) |
| 4000..4999 | 199 | spells, kinds 41..54 by school; 4651..4662 the 招聘 summons |
| 5000..5999 | 116 | skills (必殺), kinds 21..30 |
| 6000..6999 | 130 | the enemies' basic attacks (kinds 19, 20) |
| 7000..7999 | 151 | the enemies' actions (kind 40) |
| 8000..8999 | 89 | the enemies' spells (kind 60) |
| 9000..9999 | 110 | enemy and special entries, among them passives (9600 攻撃上昇１０％) and 9900 復活 |
| 12001..13428 | 271 | the ★ variants: each is id − 10000's record again, its sort key + 5 |

Which arrays an id carries follows its band. Every record has a name, kind, class, icon and sort key. Equipment
(2000..3999 and its ★) carries the prices, `item_stat_bonuses`, `item_granted_conditions`, `item_skills`, the
equipment limits and `item_improvable_flags`. An action carries the target, waits, animation, costs and
`item_inflicted_conditions`: the spells, skills and enemy actions, a weapon too (its record is the basic attack the
weapon gives, SETCH's slot 0 below) and the consumables used in battle. The stones carry `item_duplicate_crystals`,
the key items `item_not_carried_flags`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x328797]` | 1 |  | 1867 | 10..90600, 1762 distinct | `item_sort_keys`: 10 × id, a ★ variant + 5. SHOP sorts by it (`SHOP@0xf47`), and SETITLIST (`SETITLIST@0x8d`). |
| `G[0x32d5b7]` | 1 |  | 10 | 1..1, 1 distinct 1×10 | `item_battle_use_limits`: CHECKACT refuses the action when the character's count this battle, `G[0xd0f853 + 20000 × character + id]`, has reached it (`CHECKACT@0x5f8`..`0x626`; `CHECKSEAL@0x481`, `SETSKLIST@0x81c`). The seven 招聘 and three 全弾発射. |
| `G[0x3323d7]` | 1 |  | 25 | 621..643, 16 distinct | `item_ammunition`, the bullet an action uses up: BTLMAIN takes one (`BTLMAIN@0x1193`..`0x11bd`), CANCELACT gives it back (`CANCELACT@0x60`..`0x8a`), CHECKACT refuses the action when too few are held (`CHECKACT@0x5dc`); `DRAWSKLIST@0x832`, `SELACT@0x1df8` draw the count. |
| `G[0x3371f7]` | 1 |  | 1423 | 1..7, 7 distinct | `item_ranks`. A material gives AFINIT's `G[0xbd69db + 8 × target rank + material rank]` points times the quantity (`IMPROVE@0xfce`..`0x1006`, `BREED@0x10ca`). |
| `G[0x33c017]` | 1 |  | 435 | 1..2, 2 distinct 1×13, 2×422 | Read only by RESULT, which keeps the largest (`RESULT@0xb47`..`0xb63`). Not decoded. |
| `G[0x340e37]` | 1 |  | 1814 | 19104..33554734, 1119 distinct | `item_icons`, a packed picture id (`DRAWITLIST@0x347`). |
| `G[0x345c57]` | 1 |  | 1828 | 1..82, 60 distinct | `item_classes`, `GS[0x1fd9c + class]`. The use sound is `G[0xbd674f + class]` (`ITEM@0xae9`); class 37 成長 is used on every chosen character (`ITEM@0xa54`); AFINIT's flags `G[0xbd66eb + class]` bit 0 (`CHMENU@0x1274`) and bit 1 (`SETITLIST@0x27f`, `SETSKLIST@0x343`, when `G[0xd0a7cf]` & 1). |
| `G[0x34aa77]` | 1 |  | 1897 | 1..60, 40 distinct | `item_kinds`, `GS[0x1fcac + kind]`. 0 is no item: ADDITEM does nothing (`ADDITEM@0x1`). Kind 1 sets the key flag (`ADDITEM@0xde`..`0xec`). |
| `G[0x34f897]` | 1 |  | 1270 | 1..5, 5 distinct 1×850, 2×318, 3×30, 4×12, 5×60 | Mostly 1 on weapons and 2 on spells; BTLMAIN subtracts the speed `G[0xf38563]` where it is 2, 3 or 5 and `G[0xf3853b]` otherwise (`BTLMAIN@0x6957`..`0x69e7`). Not decoded further. |
| `G[0x3546b7]` | 1 |  | 24 | 1..1, 1 distinct 1×24 | `item_not_carried_flags`: GAMESTART does not carry these key items (3..31) into a new game (`GAMESTART@0x822`). |
| `G[0x3594d7]` | 1 |  | 116 | 611..617, 7 distinct | `item_duplicate_crystals`: the crystal a stone becomes when got again (`ADDITEM@0x2b`..`0x5c`, `@0x1f5`..`0x253`). |
| `G[0x35e2f7]` | 1 |  | 558 | 1..3, 3 distinct 1×14, 2×451, 3×93 | Read only by SETSSDAMAGE (`SETSSDAMAGE@0x12`), the battle section's chain. Not decoded. |
| `G[0x363117]` | 1 |  | 1053 | 1..17, 16 distinct | `item_attack_elements`, `GS[0x1fc5c + element]`; 10 戦意 is special-cased (`DRAWITTIP@0x7e4`, `INFOIT@0x1dc0`). SETCH sets the unit's `G[0xf38833]` from the weapon, 2 without one (`SETCH@0x663`). |
| `G[0x367f37]` | 1 |  | 199 | 1..15, 12 distinct | `item_defence_attributes`, armour's, drawn where the element is 0 (`DRAWITTIP@0x810`, `INFOIT@0x1dec`; `SETCH@0x689`, `@0x6ae`). |
| `G[0x36cd57]` | 5 | 0 | 349 | 1..70, 15 distinct | `item_allowed_characters`, CHINIT ids: CHECKACT refuses anyone else when column 0 is set (`CHECKACT@0x9be`..`0xa1f`). All 199 spells, 116 skills and 26 pieces of equipment. |
| `G[0x3853f7]` | 1 |  | 22 | 2..2, 1 distinct 2×22 | `item_required_genders`, against `G[0x1591ba + character]` (`CHECKACT@0xa26`..`0xa49`): the 【女性専用】 equipment. |
| `G[0x38a217]` | 1 |  | 13 | 1..25, 3 distinct 1×2, 24×3, 25×8 | `item_costume_overrides`: an equipped one sets `G[0xbdc65f]` (`BTLMAIN@0x6c94`..`0x6cb9`, `CHANGELEADER@0x1a9`), which DRAWCHLIST, DRAWSTATUS and SETMAPTEX read; 24 the 歌姫衣装, 25 the 透明な clothes (a costume, inferred). |
| `G[0x38f037]` | 5 | 0 | 7 | 1201..1207, 7 distinct | `item_required_items`: items that must be held (`CHECKACT@0x8f0`..`0x92e`). The summons need their 聘石. |
| `G[0x3a76d7]` | 1 |  | 43 | 166..215, 21 distinct | `item_required_skills`, an SSINIT skill (177 治癒魔術の極み for the ☆ healing spells): CHECKACT refuses the action without it (`CHECKACT@0xa50`..`0xa7e`), CALCREVISE takes off equipment that needs it (`CALCREVISE@0x1d`..`0x7d`). |
| `G[0x3ac4f7]` | 1 |  | 1351 | 1..3, 3 distinct 1×1133, 2×173, 3×45 | `item_target_sides`: 1 the enemy, 2 allies, 3 the user, by the descriptions; SETITLIST and SETSKLIST test for 1. |
| `G[0x3b1317]`, `G[0x3b6137]` | 1 |  | 1332 | 1..5, 4 distinct | Where both are 1 ITEM runs USESKILLSUB on the one chosen character, otherwise USESKILL on every chosen one (`ITEM@0xa2a`..`0xa62`): the target's extent. Not decoded further (5 is common on the 全体 spells). |
| `G[0x3baf57]` | 1 |  | 1314 | 1..54, 51 distinct | `item_waits`: less the unit's speed, into `G[0xf387e3]` for the weapon (`SETCH@0xc3d`..`0xc83`) and `G[0xf45b23]` for the action (`BTLMAIN@0x692f`..`0x6a9c`, at least 1). |
| `G[0x3bfd77]` | 1 |  | 1309 | -2..24, 21 distinct | `item_wait_floors`, the lowest the wait falls to (same places). |
| `G[0x3c4b97]` | 1 |  | 1316 | 5..60, 48 distinct | `item_second_waits`, the same into `G[0xf3880b]` (`SETCH@0xc88`..`0xcce`) and by BTLMAIN's second wait (`BTLMAIN@0x6aa3`..`0x6bb7`, called at `BTLMAIN@0x1c23`); DRAWITTIP draws both (`DRAWITTIP@0x1136`). |
| `G[0x3c99b7]` | 1 |  | 1310 | -3..30, 22 distinct | `item_second_wait_floors`. |
| `G[0x3ce7d7]` | 60 | 1..50 | 619 | -5..5, 10 distinct | `item_inflicted_conditions`, the level per ILINIT condition (item 2329: 燃焼 5, "燃焼５追加判定"). A negative value is not decoded. |
| `G[0x4f3757]` | 60 | 7..44 | 70 | 1..5, 5 distinct 1×78, 2×15, 3×21, 4×16, 5×17 | `item_granted_conditions`, the level equipment grants its bearer (item 2207: 再生 1, "再生１"). |
| `G[0x6186d7]` | 22 | 0..21 | 1525 | -300..3000, 353 distinct | `item_stat_bonuses`, the 22-stat vector (above): SETCH copies it into `G[0xf45c81]` and sums it (`SETCH@0x18b`, `@0x232`). |
| `G[0x683d97]` | 1 |  | 51 | 10..99999, 20 distinct | `item_hp_changes`: healing, into `G[0x20920 + character]` (`USESKILLSUB@0x21b`..`0x245`; `CALCDAMAGE@0xd2`); a negative one is the cost (`CHECKACT@0x1ab`..`0x1c7`). |
| `G[0x688bb7]` | 1 |  | 302 | -150..9999, 62 distinct | `item_mp_changes`, the same for MP `G[0x209e8]` (`CHECKACT@0x1cc`; `CALCDAMAGE@0x11b`); the spells' cost. |
| `G[0x68d9d7]` | 1 |  | 274 | -100..9999, 49 distinct | `item_tp_changes`, the same for TP `G[0x20ab0]` (`CHECKACT@0x1ed`; `CALCDAMAGE@0x164`); the skills' cost. |
| `G[0x6927f7]` | 1 |  | 10 | 5..50, 8 distinct | `item_morale_gains`, 戦意 added to `G[0x20858 + character]` (`USESKILLSUB@0x3c8`..`0x3f2`; `CALCDAMAGE@0x1ad`). |
| `G[0x697617]` | 1 |  | 140 | -1..-1, 1 distinct -1×140 | `item_use_count_changes`: the count held falls by one on use (`ITEM@0xad8`, `BTLMAIN@0x1185`; `CANCELACT@0x52`). |
| `G[0x69c437]` | 1 |  | 524 | 2..1200, 84 distinct | `item_random_spreads`: a random amount below it is added to the HP, MP and TP amounts (`CALCDAMAGE@0xfa`..`0x1a6`, `USESKILLSUB@0x24c`). |
| `G[0x6a1257]` | 1 |  | 10 | -500..500, 9 distinct | `item_goddess_gauge_changes`, to `goddess_gauge` `G[0x7250]`, kept within 0 and `G[0x7251]` (`USESKILL@0x0`..`0x45`, `ITEM@0xa7e`). |
| `G[0x6a6077]` | 1 |  | 68 | 1..3, 2 distinct 1×48, 3×20 | `item_use_places`: bit 0 in the field, bit 1 in town (`G[0x4dd6f]`, set by FIELD and cleared by TOWN; `SETITLIST@0x1ea`..`0x22d`). |
| `G[0x6aae97]` | 4 | 0..3 | 924 | 100..436600, 381 distinct | `item_buy_prices`, by `shop_price_column` (`SHOP@0x98f`, `@0x1256`, `@0x1e79`). |
| `G[0x6be717]` | 4 | 0..3 | 954 | 5..72767, 1190 distinct | `item_sell_prices`, the same (`SHOP@0x9ae`, `@0xf87`, `@0x1269`, `@0x1e8c`). |
| `G[0x6d1f97]` | 1 |  | 744 | 1..1, 1 distinct 1×744 | `item_improvable_flags`: IMPROVE leaves an item without it (`IMPROVE@0xedf`). |
| `G[0x6d6db7]` | 1 |  | 123 | 1..219, 50 distinct | `item_skills`, the SSINIT skill equipment adds, `GS[0xf384 + 5 × skill + level − 1]` (`DRAWITTIP@0x10ce`..`0x112a`). |
| `G[0x6dbbd7]` | 1 |  | 123 | 1..5, 4 distinct 1×99, 2×10, 3×10, 5×4 | `item_skill_levels`. |
| `G[0x6e09f7]` | 1 |  | 7 | 58..233, 7 distinct | `item_summoned_characters`, the summons' CHINIT ids (4651: 58 ミノタウロス); BTLMAIN runs its summon routine after the animation (`BTLMAIN@0x28bc`). |
| `G[0x6e5817]` | 1 |  | 10 | 1..1, 1 distinct 1×10 | ANIME takes animation mode 4 for it (`ANIME@0x3c3`; DRAWUNIT); the 舞 and 砲撃 skills. Not decoded. |
| `G[0x6ea637]` | 1 |  | 1333 | 1..1060, 522 distinct | `item_battle_animations`, copied into `G[0xef857c]` before ANIME (`BTLMAIN@0x28a0`..`0x28b6`). |
| `G[0x6ef457]` | 1 |  | 1810 | 1..1, 1 distinct 1×1810 | `item_catalogued_flags`: the ids the encyclopedia lists (`INFO@0xac6`, `INFOIT@0xa4e`, `INFOSK@0xa56`). |
| `GS[0x111fc]` | 1 |  | 1902 | 飛翔の耳飾り / 魔神を封じた剣 / 昇降機の鍵 | `item_names` (`DRAWITTIP@0x124`; SHOP, INFOIT, ALCHEMY). |
| `GS[0x1601c]` | 2 | 0..1 | 1867 | 【重要】瞬時に迷宮を脱出できる耳飾り / 【重要】魔神ハイシェラを封じた剣 / 【重要】カドラ廃坑の昇降機を動かす為 | `item_description_lines` (`DRAWITTIP@0x124`, `@0x138`). |

### The item helpers and the inventory

The inventory is `item_counts` `G[0x770a + id]`, one per id of the whole space.

ADDITEM adds items. The caller sets `current_item` and `item_count_change`.

- `ADDITEM@0x1`: an item of kind 0 changes nothing.
- `ADDITEM@0x2b`..`0x5c`: a stone already obtained becomes its crystal (`item_duplicate_crystals`) instead.
- `ADDITEM@0x61`..`0xb6` adds the change and keeps the count within 0 and 999.
- `ADDITEM@0xbb`..`0xec` and `@0x198`..`0x1a6`: a key item (kind 1) held sets the story flag 9001 + id (the cell
  `G[0xee7 + 9000 + id]`); at 0 held it clears it.
- `ADDITEM@0xf1`..`0x18e`: for a key item held, when `G[0x0]` is 1 and the change is positive, it writes the 20-entry
  log `acquisition_log_*` (kind 4, the item, the count, where), which OBJACT and RESULT share and INFOLG shows. No
  other kind is logged here.
- `ADDITEM@0x1ab`..`0x1f2`: unless `G[0xbeb33d]` is set (GAMESTART sets it, `GAMESTART@0x7a2`), it sets
  `item_obtained_flags` `G[0x1134a + id]` and `item_seen_flags` `G[0xc5f2f + id]`, the latter in the shared profile.
  On a stone's first copy with a count above 1 it keeps one stone and adds the rest as crystals
  (`ADDITEM@0x1f5`..`0x253`).

ITEM uses an item outside battle: it adds the HP, MP and TP amounts and 戦意 through USESKILLSUB (one target) or
USESKILL (every chosen character) (`ITEM@0xa2a`..`0xa62`), moves `goddess_gauge` (`ITEM@0xa7e`), takes
`item_use_count_changes` (`ITEM@0xad8`) and plays its class's sound (`ITEM@0xae9`). SETITLIST lists what may be used
where (`item_use_places`, `SETITLIST@0x182`..`0x22d`): in battle (`G[0xcf2e2]`) kind 3 非戦闘時 is left out. In
battle BTLMAIN takes the count (`BTLMAIN@0x1185`) and the ammunition, CANCELACT gives both back, and CHECKACT decides
whether an action can be chosen: the costs, the ammunition, the uses this battle, `item_required_items`,
`item_allowed_characters`, `item_required_genders` and `item_required_skills` (`CHECKACT@0x1ab`..`0xa7e`).

SHOP buys and sells; `G[0xbf016d]` is 0 to buy (`SHOP@0xe63`). To buy it runs the SHINIT chain (below,
`SHOP@0xd76`..`0xdae`) and lists `shop_offers`, sorted by `item_sort_keys` (`SHOP@0xf47`); the price is the
column `shop_price_column` of `item_buy_prices` or `item_sell_prices`. INFOIT, INFOIT2, INFOSK and INFOSK2 are the
item and skill pages: INFOIT and INFOSK list the catalogued ids, INFOIT2 toggles `item_marked_flags`
(`INFOIT2@0x8c0`) and shows the improvement points against the cost (`INFOIT2@0x1e75`). DRAWITTIP draws an item's
tooltip at `G[0xbdc786]`, `G[0xbdc787]`.

### Equipment (SETCH)

Each character has five equipment slots, `character_equipment` `G[0x404f0 + 5 × character + slot]` (slot 0 the
weapon), and three stone slots, `character_stones` `G[0x406e4 + 3 × character + k]`. SETCH builds a unit
(`G[0xf381a2]`) from a character (`G[0x158dd1]`), taking a slot of `equipment_preview_slots` `G[0xf45c97 + i]` in place
of the saved one when it is 0 or more (`SETCH@0xa7`..`0x113`; INIT2 sets the eight to −1, `INIT2@0x236`). For each
item it:

- adds `item_stat_bonuses` (`SETCH@0x18b`..`0x232`) and the first 3 × level bonuses of IMINIT
  (`SETCH@0x1b4`..`0x20b`);
- for each stone, adds DPINIT's three bonuses for the bearer type and the skill level (`SETCH@0x3fd`..`0x48e`), and
  checks the stones' costs against the capacity `G[0xf45c21]` and `G[0xf45c37]` (`SETCH@0x268`..`0x2aa`);
- from the weapon, the element (`SETCH@0x663`), the armour's attribute (`SETCH@0x689`, `@0x6ae`) and the two waits
  (`SETCH@0xc3d`..`0xcce`).

It stores the 22 sums in the unit's arrays (`SETCH@0x502`..`0x60a`), capped by `G[0xf45c29 + k]` (INIT2 sets 9999 for
each, 99999 for HP and 999 for MP, `INIT2@0x205`..`0x225`). CALCREVISE copies the maxima to the character and takes off
equipment whose required skill is missing (`CALCREVISE@0x1d`..`0x116`).

### DPINIT (stones)

Leader `G[0x6f4277]` (the pictures, 2 cells per id), capacity 20000, keyed by the stone's item id. 116 records, ids
1001..1640: 89 in the base, 2 from `$1$DPINIT` (1454, 1455) and 25 from `$2$DPINIT`. DPINIT writes no string; the
name is ITINIT's. Its 8 arrays agree modulo 20000 from `G[0x6f4277]` (+ 40000 `G[0x6fdeb7]`, + 20000 `G[0x702cd7]`,
+ 20000 `G[0x707af7]`, + 400000 each, + 1200000 `G[0x9519f7]`), and no cell falls outside them. The per-type
columns are indexed by the bearer type `G[0x159d72 + character]`, 1..7.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x6f4277]` | 2 | 0..1 | 116 | 16921..33554656, 232 distinct | `stone_pictures`: 0 the status screen's slot (`DRAWSTATUS@0x636`), 1 BREED's large picture (`BREED@0x1f2a`, 324×540). |
| `G[0x6fdeb7]` | 1 |  | 116 | 2..962, 116 distinct | `stone_characters`, the character in the stone, a CHINIT id (`BREED@0x1fcc`; `CHMENU@0x12e2`, `SETCHPSTATE@0x15c`). |
| `G[0x702cd7]` | 1 |  | 116 | 3..65, 47 distinct | `stone_costs`, against the stone capacity (`SETCH@0x268`..`0x2aa`; `DRAWSTATUS@0x6de`). |
| `G[0x707af7]` | 20 | 1..7 | 116 | 1..3, 3 distinct 1×242, 2×312, 3×258 | `stone_compatibilities` by bearer type; icons for 1 and 3 (`DRAWITLIST@0x4ac`, `@0x4f5`; `DRAWSTATUS@0x74e`, `@0x79e`). |
| `G[0x769577]` | 20 | 1..7 | 116 | 1..212, 26 distinct | `stone_skills`, the SSINIT skill by bearer type (`SETCH@0x44a`..`0x48e`; `DRAWITTIP@0x109b`). |
| `G[0x7caff7]` | 20 | 1..7 | 116 | 1..3, 3 distinct 1×543, 2×264, 3×4 | `stone_skill_levels`, added to `G[0xf3b493 + 500 × unit + skill]`. |
| `G[0x82ca77]` | 60 | 3..23 | 116 | 0..19, 18 distinct | `stone_bonus_stats`: three per bearer type, column 3 × type + k, a column of the 22-stat vector (`SETCH@0x3fd`..`0x440`; `DRAWITTIP@0x945`). |
| `G[0x9519f7]` | 60 | 3..23 | 116 | -200..100, 39 distinct | `stone_bonus_amounts`. |

### IMINIT (improvement), IMPROVE and BREED

Leader `G[0xa76977]` (the points per level, 11 cells per id), capacity 20000, keyed by item id. 844 records, ids
1001..13428: 702 in the base, 78 from `$1$IMINIT` and 64 from `$2$IMINIT` (some of them for base items, 2860 and
2953). Its 4 arrays agree modulo 20000 (+ 220000 `G[0xaac4d7]`, + 600000 `G[0xb3ec97]`, + 600000 `G[0xbd1457]`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa76977]` | 11 | 0..9 | 844 | 100..1000, 10 distinct | `item_improvement_costs`: the points to rise from level k, 100 × (k + 1); 0 at the top (`IMPROVE@0xec2`..`0xedb`, `@0x1014`). |
| `G[0xaac4d7]` | 30 | 0..28 | 844 | 0..21, 20 distinct | `item_improvement_bonus_stats`: three per level, a column of the 22-stat vector (`SETCH@0x1b4`..`0x20b`). |
| `G[0xb3ec97]` | 30 | 0..28 | 844 | 1..200, 49 distinct | `item_improvement_bonus_amounts`. |
| `G[0xbd1457]` | 1 |  | 134 | 8602..8736, 134 distinct | `item_improvement_mastery_flags`: the story flag the top level sets, `G[0xee7 + flag − 1]` (`IMPROVE@0x106c`..`0x10a4`). |

IMPROVE (the town screen, `G[0xbf0167]`'s chain) improves equipment: the target `item_improvement_target`
`G[0xbdc798]` must have `item_improvable_flags` and be held. Each material `item_improvement_material` `G[0xbdc799]`,
times the quantity `selnum_value` `G[0xbeb340]`, gives AFINIT's `G[0xbd69db + 8 × target rank + material rank]`
points (`IMPROVE@0xfb9`..`0x1006`) into `item_improvement_points` `G[0xc52a + id]`. When they reach the level's cost
the level `item_improvement_levels` `G[0x1616b + id]` rises and the points return to 0 (`IMPROVE@0x1024`..`0x1055`);
at the top level the mastery flag is set (`IMPROVE@0x106c`..`0x10a4`). BREED does the same for the stones, which
have IMINIT records but no improvable flag: it shows the stone's character and large picture
(`BREED@0x1f2a`, `@0x1fcc`) and adds to the same points (`BREED@0xb02`, `@0x10ca`..`0x10e8`).

### ALINIT (alchemy recipes) and ALCHEMY

Leader `G[0xc05099]` (the product), capacity 1000. 91 recipes, ids 1..224: 77 in the base, 13 from `$1$ALINIT`
(201..209, 221..224) and 1 from `$2$ALINIT` (7). ALINIT writes no string. Its 10 arrays agree modulo 1000 from
`G[0xc05099]`, and no cell falls outside them; the next table, NUINIT, starts at `G[0xc09eb9]`, 3000 cells after
the last array.

ALCHEMY offers a recipe when every condition below holds (`ALCHEMY@0xe18`..`0xf98`); it allows 1 of a key item and
999 of another (`ALCHEMY@0xfe0`), checks the ingredients (`ALCHEMY@0x1029`), then takes the ingredients times
`selnum_value` and adds that many of the product through ADDITEM (`ALCHEMY@0x1124`..`0x11b3`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc05099]` | 1 |  | 91 | 19..643, 91 distinct | `alchemy_products`, an ITINIT id. |
| `G[0xc05481]` | 3 | 0..2 | 91 | 801..840, 40 distinct | `alchemy_ingredients`, ITINIT ids. |
| `G[0xc06039]` | 3 | 0..2 | 91 | 1..20, 11 distinct | `alchemy_ingredient_counts`. |
| `G[0xc06bf1]` | 1 |  | 91 | 1..1, 1 distinct 1×91 | `alchemy_facility_masks`: bit 0 is needed where `G[0x4d590]` is 2 (`ALCHEMY@0xe18`). |
| `G[0xc06fd9]` | 3 | 0 | 10 | 1094..8847, 9 distinct | `alchemy_required_flags`, `G[0xee7 + flag − 1]` = 1 (`ALCHEMY@0xe57`..`0xe97`). |
| `G[0xc07b91]` | 3 | 0..2 | 7 | 51..9032, 12 distinct | `alchemy_forbidding_flags`, which hide it (`ALCHEMY@0xebf`); 9020 is key item 19 三色染料's flag. |
| `G[0xc08749]` | 1 |  | 51 | 3..70, 2 distinct 3×49, 70×2 | `alchemy_required_characters`: `G[0x725a + character]` & 0x10006 must be 0x10002 (`ALCHEMY@0xefc`). |
| `G[0xc08b31]` | 1 |  | 49 | 18..120, 16 distinct | `alchemy_required_character_levels`, against `G[0x20790 + character]` (`ALCHEMY@0xf33`). |
| `G[0xc08f19]` | 1 |  |  | (none written) | `alchemy_required_items`, an item that must be held (`ALCHEMY@0xf50`). |
| `G[0xc09301]` | 3 | 0 | 22 | 45..72, 2 distinct 45×14, 72×8 | `G[0x72be + value]` & 3 must be 3 (`ALCHEMY@0xf98`). Not decoded. |

### SHINIT (the shops' offers, on demand)

SHINIT runs on demand, through the hook chain `shinit_chain_script_ids` `G[0xf45dad]` (slot 0 SHINIT, `INIT2@0x75`;
slot 1 `$1$SHINIT`, `$1$AUTORUN@0x75`; slot 2 `$2$SHINIT`, `$2$AUTORUN@0x75`). SHOP runs the chain's 32 slots each
time it opens (`SHOP@0xd76`..`0xdae`). SHINIT writes its own cells, just below ALINIT's leader, not ALINIT's:

- `SHINIT@0x0`..`0xf` clears `shop_pictures` `G[0xc05097]`, `G[0xc05098]` and the two 200-slot lists.
- It branches on the shop `G[0x4d2d0]`, 1..4 (`SHINIT@0x14`, `@0x2b00`, `@0x41e0`, `@0x4772`), and for each sets
  the pictures (0xddd and 0x52, 0x1201 and 0x1200, 0x9f3 and 0xcf4, 0xa2c and 0x2243), `shop_price_column`
  `G[0xc05096]` (the shop less 1) and its offers: `shop_offers` `G[0xc04f06 + slot]`, the item, and
  `shop_offer_flags` `G[0xc04fce + slot]`, bit 0 not offered and bit 1 shown but not for sale.
- The offers are guarded by story flags, by `G[0x31cf]`, by `G[0x7255]` (below 50000 or 200000), by `G[0x1fe84]`
  and by `G[0x727a]` & 1.
- `$1$SHINIT` writes nothing; `$2$SHINIT` adds shop 2's slots from `G[0xc04f74]` (item 2016 on, with their flags
  `G[0xc0503c]`..).

SHOP sets bit 1 itself while 999 are held, and lists the key items (kind 1) and the stones (kind 5) only while none
is held (`SHOP@0xe8e`..`0xf3e`). `$2$ARENA` sets the pictures to 0x200003a and 0x200004b (`$2$ARENA@0x7ad`) and
shows them as textures 0x52 and 0x53 (`@0x8ff`, `@0x914`).

### What is saved

The four boot tables are not saved, and INIT2 writes them again on every load, so a changed price, bonus, wait,
recipe or improvement reaches a loaded game at once; the character maxima follow at the next CALCREVISE. SHINIT runs
each time a shop opens, so an offer does too.

These lie below the prefix, so a save keeps them:

- `item_counts` `G[0x770a]`, `item_improvement_points` `G[0xc52a]`, `item_obtained_flags` `G[0x1134a]`,
  `item_improvement_levels` `G[0x1616b]`, `item_marked_flags` `G[0x1af8b]` and `item_seen_flags` `G[0xc5f2f]`, all
  20000 wide (`G[0x1616a]` between two of them is never used);
- `character_equipment` `G[0x404f0]` and `character_stones` `G[0x406e4]`;
- the key items' story flags `G[0xee7 + 9000 + id]` and the improvement mastery flags, both in the story flag bank;
- the acquisition log `G[0x6231]`..`G[0x6281]`, and `goddess_gauge` `G[0x7250]` with its maximum `G[0x7251]`.

`current_item`, `item_count_change`, `selnum_value`, the improvement target and material, `equipment_preview_slots`
and SHINIT's lists lie above the prefix.

The shared profile carries some of it into a new game. GAMECLEAR stores the counts, obtained flags, improvement
points and levels and the marks (`GAMECLEAR@0x3a8`..`0x3d7`); `item_seen_flags` is stored as it is set. GAMESTART
loads them by the bits of `G[0x6e3e]` (`GAMESTART@0x7e2`..`0x918`, bit 1): for each id whose kind's flag
`item_kind_lists` entry is set (INIT2 fills it, `INIT2@0x16f`) and whose `item_not_carried_flags` is 0, setting a key
item's flag and a mastered item's flag again (`GAMESTART@0x8bb`, `@0x8ce`..`0x918`). Bit 2 loads `G[0x7254]` and
`G[0x7255..0x7258]`, bit 3 `G[0x1fdab]`. While it loads, `G[0xbeb33d]` keeps ADDITEM from marking the items newly
obtained (`GAMESTART@0x7a2`).

## The dungeon

The dungeons are made of floors, and each floor of map blocks. A floor has an id from 1 to 86, and a map block has
the id 10 × floor + n (11 to 863). FIELD runs the dungeon, one map block at a time. Eight tables describe it:

- MPINIT holds two tables on one grid: the floors (capacity 100) and the map blocks (capacity 1000).
- ATINIT holds each floor's atlas: its scales, the parts hidden at the start and the parts' pictures.
- OBINIT and OBINIT2 hold the object kinds, one id space in both (51 to 502). OBINIT also holds the six atlas markers.
- LAINIT holds the lands, the ground types under the player.
- PZINIT holds the block puzzles.
- FLINIT runs on demand and writes the object slots of the floor in play.
- DUINIT runs on demand and writes the fixed battle in play.

The map in play is built from these. SETMAPDATA clears it and runs the block's map script, which fills its lists
(below, "The map in play"). SETMAPTEX loads the block's pictures, SETFLOORTEX the pictures of the floor's objects, and
DRAWFIELD draws it. OBJACT runs an object's action, CARRY moves the puzzle blocks, and ATLAS and DRAWMINIMAP draw the
atlas.

Every boot table lies above the saved prefix (`G < 0xcf2e6`, `GS < 0x34f`), so none of it is saved. The floor and map
state the game saves lies below it ("What is saved", at the end).

One 100-cell grid runs from `G[0xc9ee85]` to `G[0xca2aad]`. It holds MPINIT's floor arrays (`0xc9ee85`..`0xc9f140`),
then ATINIT's (`0xc9f141`..`0xca28b8`), then FLINIT's (`0xca28b9`..`0xca2aad`). The map-block grid follows at
`G[0xca2aae]`, 1000 cells an array, and ends at `G[0xd09f05]`. The next cell, `G[0xd09f06]`, is the encounter set
FIELD hands the enemy tables ("Encounters and enemies").

### Entering a map block

FIELD enters a block in this order:

1. It copies the block's floor (`map_block_floors`) into `current_floor_id` and keeps the old one in
   `previous_floor_id` (`FIELD@0x1c4`).
2. It marks the floor visited in `floor_seen_flags` and stores the flag in the shared profile (`FIELD@0x203`).
3. On the floor's first visit it copies ATINIT's hidden parts into the saved ones (`FIELD@0x21b`..`0x249`).
4. It copies the block's name into `current_place_name` (`FIELD@0x266`).
5. It runs the FLINIT chain `G[0xf45dcd]` (`FIELD@0x399`), then applies the puzzles' reset rules
   (`FIELD@0x422`..`0x492`).
6. It sets up the floor's object slots (`FIELD@0x545`..`0x5de`): see FLINIT.
7. It copies the block's encounter set into `G[0xd09f06]` (`FIELD@0x657`), runs the ENINIT chain (`@0x684`), and
   plays the block's music (`FIELD@0x703`..`0x720`).

After a load on a map, INIT2 rebuilds the map in play from the saved cells (`INIT2@0x2c5`..`0x2f6`). It sets the
camera, then runs SETMAPDATA, SETMAPTEX, SETFLOORTEX and DRAWCH. It does not run the FLINIT chain, and CALLBACK_LOAD
does not run it either; FIELD's own opening does. A load zeroes every bank past the saved prefix and then runs each
saved frame's script again from its entry to `continue-save-load-stack-restore` (op `0xae`; `ScriptMemory.cs`,
`ReadSaveRecords`), and FIELD's runs from its entry to `FIELD@0x9ee`, past the FLINIT walk (`FIELD@0x37c`..`0x3b1`).
So the floor's slots are written again after each load (FLINIT, below).

### MPINIT, the floors (one floor's name, dungeon and atlas place)

The floor arrays are 100 cells each. There are 74 floors in the base (ids 1 to 74), plus floors 81 and 82 from
`$1$MPINIT` and floor 86 from `$2$MPINIT`. The appends write no base cell. In the tool, with the map blocks as the
leader, these arrays show as off-grid rows (†): one table has one id space.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x26a32]` | 1 |  | 74 | カドラ廃坑跡Ｂ１Ｆ / カドラ廃坑跡Ｂ２Ｆ / カドラ廃坑跡Ｂ３Ｆ | The floor's name. ATLAS shows it (`ATLAS@0x63d`), and INFOEN does too (`INFOEN@0x1e17`). Not saved. |
| `G[0xc9ee85]` † | 1 |  | 20 | 4..30, 6 distinct 4×5, 5×1, 8×4, 20×4, 25×5, 30×1 | The gauge drain. When `party_leader` `G[0x7259]` is 1 and `goddess_gauge` `G[0x7250]` is above 0, each step adds to `G[0x4dd6e]`. Each time that reaches the value × 10, `G[0x7250]` drops by 1 (`FIELD@0x2479`..`0x24e9`). Set on 20 floors. |
| `G[0xc9eee9]` † | 1 |  | 47 | 1..3, 3 distinct 1×9, 2×25, 3×13 | The encounter class: see below. |
| `G[0xc9ef4d]` † | 1 |  | 74 | 1..86, 29 distinct | The dungeon's first floor. Every value is the lowest floor id that has it (checked for all 29). ATLAS lists the floors that share it (`ATLAS@0x353`..`0x3ba`), and READY reads it (`READY@0x79`). |
| `G[0xc9efb1]` † | 1 |  | 74 | 10..860, 74 distinct | The atlas's sort key, 10 × floor. `ATLAS@0x373` sorts the floors by it (`sort-indices-by-key-sum`). `$1$MPINIT@0xa` writes `G[0xc9f002]` and `$2$MPINIT` writes `G[0xc9f007]`. |

The encounter class has two uses:

- **Random encounters.** A random encounter can start when the block has an encounter set and either `G[0x4dd7f]` is
  0 or the class is 1 (`FIELD@0x25ca`..`0x2610`). So floors of class 1 keep their encounters while `G[0x4dd7f]` is
  set.
- **The special encounters.** When the class is not 0 and `G[0x4dd86]` is 0, FIELD walks ELINIT's 100 entries
  (`FIELD@0x3ac2`..`0x3b78`). It takes an entry only when `check-bit` finds the class's bit in the entry's mask
  `G[0xd0a49f + k]`. The entry's chance is then read from `G[0xd0a503 + 5k + (G[0x7250] − 1) / 200]`, and the
  message 視線を感じる…… or 異質な気配がする…… follows (`FIELD@0x3b86`..`0x3c39`). The cells are described in "Encounters and enemies".

Floors 1 to 13 have class 3, floors 66 to 74 class 1, and most of the rest class 2. Reading the class as an encounter
class is an inference from these two uses.

Three more floor arrays are never written:

- `G[0xc9f015]` has no reader.
- `G[0xc9f079]` and `G[0xc9f0dd]` are subtracted as the atlas's origin x and y (`ATLAS@0x98a`, `@0x9ed`, `@0xa57`,
  `@0xa7e`; `DRAWMINIMAP@0x2b8`, `@0x31b`). They are always 0.

### MPINIT, the map blocks (one block's scripts, music, encounters and pictures)

Leader `GS[0x26a96]` (the name), capacity 1000. There are 245 blocks: 234 in the base, 8 from `$1$MPINIT` (811..813
and 821..825) and 3 from `$2$MPINIT` (861..863). The arrays agree modulo 1000, from `0xca2aae` to `0xd09f05`. Eleven
writes come before a layer's first record: the floor arrays above.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x26a96]` | 1 |  | 245 | カドラ廃坑跡Ｂ１Ｆ / カドラ廃坑跡Ｂ１Ｆ / カドラ廃坑跡Ｂ１Ｆ | The block's name. `FIELD@0x266` copies it into `current_place_name`, which is saved. BTLMAIN copies that into the battle's place name (`BTLMAIN@0x6a`). |
| `G[0xca2aae]` | 1 |  | 245 | 1..86, 74 distinct | The block's floor. `FIELD@0x1c4` copies it into `current_floor_id`. 208 scripts read it. |
| `G[0xca2e96]` | 1 |  | 245 | 20060..33554773, 245 distinct | The block's map script (MP011.BIN …). `SETMAPDATA@0xd8` calls it to fill the map in play. |
| `G[0xca327e]` | 1 |  | 245 | 20061..33554774, 245 distinct | The block's event script (MPEV011.BIN …), called at `FIELD@0x1e36`. |
| `G[0xca3666]` | 3 | 0..2 | 244 | 14..55, 9 distinct | The music for each of three variants. `map_music_variant` picks the column (`FIELD@0x703`..`0x720`), and map scripts set it (`MP091@0xe27`). |
| `G[0xca421e]` | 2 | 0..1 | 226 | 10..69, 52 distinct | The encounter set for each of two variants. `map_encounter_variant` picks the column, and `FIELD@0x657` copies the set into `G[0xd09f06]` for the ENINIT chain. |
| `G[0xca49ee]` | 1 |  | 228 | 1..190, 31 distinct | The enemies' base level. SETEN adds the difficulty's adjustment `G[0x6e3a]` and a random spread, clamps the level by CHINIT's `G[0x1dd6ea]` and `G[0x1ddad2]`, and stores it in `G[0xf381f3 + slot]` (`SETEN@0xc0`..`0x1b0`). |
| `G[0xca4dd6]` | 1 |  | 228 | 1..2, 2 distinct 1×3, 2×225 | The level spread (`SETEN@0xc0`..`0x1b0`). |
| `G[0xca51be]` | 1 |  | 226 | 1..9, 9 distinct | The encounter level. A nonzero `land_encounter_levels` replaces it, `G[0x4dd81]` is added, and the result, clamped to 0..9, is the column of EEINIT's `G[0xd0a1b1 + 10k + level]` (`FIELD@0x2a4d`..`0x2a6a`). |
| `G[0xca55a6]` | 2 | 0..1 | 245 | 17777..33554660, 85 distinct | The battle backdrops, MPB001AA.AGF and MPB001AB.AGF, set on textures 0x52 and 0x53. BTLMAIN uses them when the battle's wave sets none (`BTLMAIN@0x5979`..`0x59c7`). |
| `G[0xca5d76]` | 1 |  | 47 | 20462..23564, 17 distinct | The far backdrop (MP07101C.AGF …). `SETMAPTEX@0x459` loads it on texture 0x70, and `DRAWFIELD@0x95b` draws it as element 0x64. |
| `G[0xca692e]` | 1 |  | 47 | 2..20, 6 distinct 2×5, 5×14, 6×10, 8×2, 10×10, 20×6 | The backdrop's parallax divisor d. `SETMAPDATA@0xb2` copies it into `G[0xc0caaa]`. `DRAWFIELD@0x96c`..`0x9c9` scales the backdrop to 101 + 199 / d percent. It moves the backdrop by 1/d of the scroll across and 1/(2d) down. |
| `G[0xca6d16]` | 1 |  | 141 | 20062..20062, 1 distinct 20062×141 | The overlay picture, SO031A.AGF. `SETMAPTEX@0x48a` sets it on texture 0x74. |
| `G[0xca70fe]` | 1 |  | 141 | 100..250, 8 distinct | The overlay's scale in percent. `SETMAPDATA@0xbe` copies it into `G[0x4dd8a]` (saved), and `DRAWFIELD@0x10b4`..`0x111b` draws the overlay centred and scaled by it. It may be a light circle around the player, but that is an inference. |
| `G[0xca74e6]` | 1 |  | 163 | 60..14130, 143 distinct | The block's x on its floor. DRAWFIELD adds it to each piece's x (`DRAWFIELD@0x9fa`), and DRAWMINIMAP and ATLAS add it to the objects' places (`DRAWMINIMAP@0x291`). |
| `G[0xca78ce]` | 1 |  | 168 | 20..7850, 129 distinct | The block's y on its floor, used in the same way (`DRAWFIELD@0xa1a`). |
| `G[0xca7cb6]` | 1 |  | 245 | 1000..9000, 112 distinct | No script reads it. The values look like a width. |
| `G[0xca809e]` | 1 |  | 245 | 1000..5000, 109 distinct | No script reads it. The values look like a height. |
| `G[0xca8486]` | 100 | 0..8 | 245 | 20063..33554790, 1781 distinct | The pieces' lower pictures. `SETMAPTEX@0x15d`..`0x18f` sets piece i's picture on texture 0x12c + i. `DRAWFIELD@0xa2f`..`0xa4f` draws the 1000 × 1000 area at (12, 12) of it as element 0xc8 + i. |
| `G[0xcc0b26]` | 100 | 0..8 | 200 | 20064..33554791, 1365 distinct | The pieces' upper pictures, 0 for none: on texture 0x190 + i, drawn as element 0x11170 + i (`SETMAPTEX@0x19c`, `DRAWFIELD@0xa60`..`0xa80`). The high element number puts them above the sprites, but that is an inference. |
| `G[0xcd91c6]` | 100 | 0..8 | 245 | -90..8003, 585 distinct | The piece's x within the block. Its place on screen is the block's x + the piece's x − the camera `G[0xc0caa5]` (`DRAWFIELD@0x9ef`..`0xa08`). |
| `G[0xcf1866]` | 100 | 0..8 | 240 | -1..4000, 603 distinct | The piece's y, used in the same way with the camera `G[0xc0caa6]` (`DRAWFIELD@0xa0f`..`0xa28`). |

The tool lists 27 arrays for MPINIT, the five floor arrays among them. Two map-block arrays are never written:

- **`G[0xca615e]`** has no reader.
- **`G[0xca6546]`.** `SETMAPDATA@0xa6` copies it into `G[0xc0caa9]`. When that is nonzero, DRAWFIELD draws the
  backdrop as 20 tiles and animates each over that many frames of a 20-column sheet at 100 ms
  (`DRAWFIELD@0x6d8`..`0x703`, `@0x89e`..`0x94c`). No block uses this.

### ATINIT (the floors' atlas)

Leader `G[0xc9f141]`, stride 2, capacity 100: the floors' ids. There are 71 floors in the base, and floors 81 and 82
from `$1$ATINIT`. Four writes come before a layer's first record.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc9f141]` | 2 | 0..1 | 73 | 5..30, 21 distinct | The atlas's scale in percent. Column 0 is the minimap's (`DRAWMINIMAP@0x2c6`, `@0x329`; DRAWFIELD and DRAWFRAME read it too), and column 1 is the ATLAS screen's (`ATLAS@0x998`, `@0xa65`). |
| `G[0xc9f209]` | 20 | 1..14 | 27 | 1..1, 1 distinct 1×77 | The parts hidden at the start: 1 hides the part. On the floor's first visit FIELD copies the row into `floor_atlas_parts_hidden` (`FIELD@0x21b`..`0x249`), which is saved. |
| `G[0xca1919]` | 40 | 0..29 | 73 | 15382..16777391, 316 distinct | The parts' pictures, two per part: column 2p is the minimap's (MPS001A.AGF, MPS001A01.AGF …; `DRAWMINIMAP@0x39`..`0x99`), and column 2p + 1 is the ATLAS screen's (MPS001B.AGF …; `ATLAS@0x6b1`, `@0x75f`). |

The span from `G[0xc9f9d9]` to `G[0xca1918]` (8000 cells) is never written.

### OBINIT and OBINIT2 (the object kinds and the atlas markers)

Leader `G[0xc2e5bd]` (the picture), capacity 1000. There are 132 kinds, ids 51 to 502: 130 in the base, and 501 and 502
from `$2$OBINIT`. `$1$OBINIT` is empty. Field k of the grid starts at `0xc2b6dd + 1000k`, so the bases agree modulo
1000. OBINIT2 goes on with the same grid from `G[0xc2f945]`, with 130 kinds: 128 in the base, and 501 and 502 from
`$2$OBINIT2`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc2b6dd]` | 1 |  | 132 | 8..80, 9 distinct | The frame's anchor x, its centre: DRAWFIELD subtracts it from the object's x (`DRAWFIELD@0xcda`..`0xd98`). |
| `G[0xc2bac5]` | 1 |  | 132 | 8..150, 11 distinct | The frame's anchor y, its bottom, used in the same way. |
| `G[0xc2bead]` | 1 |  | 132 | 16..160, 9 distinct | The frame's width (the source rectangle, `DRAWFIELD@0xcda`..`0xd98`). |
| `G[0xc2c295]` | 1 |  | 132 | 16..150, 10 distinct | The frame's height. |
| `G[0xc2c67d]` | 1 |  | 10 | 4..30, 4 distinct 4×1, 12×2, 16×6, 30×1 | The idle animation's frame count (`animate-gfx-srcrect-loop`, `DRAWFIELD@0x816`..`0x832`). |
| `G[0xc2ca65]` | 1 |  | 10 | 4..12, 4 distinct 4×1, 5×1, 8×6, 12×2 | The idle animation's sheet columns. |
| `G[0xc2ce4d]` | 1 |  | 10 | 100..100, 1 distinct 100×10 | The idle animation's period in ms. |
| `G[0xc2d235]` | 1 |  | 3 | 1..1, 1 distinct 1×3 | The blend mode, set on the sprite (`DRAWFIELD@0xdb0`). |
| `G[0xc2d61d]` | 1 |  | 13 | 200..800, 3 distinct 200×3, 600×6, 800×4 | The fade time in ms when the object turns (`OBJACT@0x144`..`0x1a4`). |
| `G[0xc2da05]` | 1 |  | 68 | -1..502, 52 distinct | The kind it turns into when its action runs; −1 removes it (`OBJACT@0x40`..`0x111`, `@0x11ca`..`0x120b`). A treasure's closed kind gives its open kind: 211 → 401, …, 258 → 448. |
| `G[0xc2dded]` | 1 |  | 63 | 1..6, 6 distinct 1×1, 2×1, 3×1, 4×6, 5×53, 6×1 | The atlas marker, 1 to 6, 0 for none (`DRAWMINIMAP@0x1e1`..`0x280`, `ATLAS@0x9d8`). |
| `G[0xc2e1d5]` | 1 |  | 62 | 23646..23714, 5 distinct 23646×3, 23649×4, 23654×48, 23710×1, 23714×6 | The sound played when the action runs: MAPSE002.WAV for the treasures (`OBJACT@0x127`). |
| `G[0xc2e5bd]` | 1 |  | 132 | 23639..33554793, 125 distinct | The kind's picture. SETFLOORTEX sets it on texture 0x258 + slot for every slot of the floor (`SETFLOORTEX@0x1d`..`0x56`), and OBJACT does so for the new kind (`OBJACT@0x73`). |
| `G[0xc2e9a5]` | 1 |  | 61 | 23647..23718, 4 distinct 23647×6, 23655×49, 23715×2, 23718×4 | The effect played when the action runs: SO027D.AGF for the treasures (`OBJACT@0x1aa`..`0x278`). |
| `G[0xc2ed8d]` | 1 |  | 61 | 15..30, 3 distinct 15×6, 24×49, 30×6 | The effect's frame count. |
| `G[0xc2f175]` | 1 |  | 61 | 5..6, 2 distinct 5×6, 6×55 | The effect's sheet columns. |
| `G[0xc2f55d]` | 1 |  | 61 | 20..50, 2 distinct 20×6, 50×55 | The effect's ms per frame. |
| `G[0xc2f945]` | 1 |  | 130 | -9999..-10, 7 distinct | OBINIT2: added to the object's y for the draw order; −9999 puts it behind everything (`DRAWFIELD@0x1ed`, `@0x2b7`, `@0x421`). |
| `G[0xc2fd2d]` | 1 |  | 115 | 4..6, 3 distinct 4×6, 5×103, 6×6 | OBINIT2: the wall polygon's point count. FIELD hands the polygon to AGERC's `_SetMapWall*` (`FIELD@0x1fac`..`0x2075`). A slot whose flag has bit 1 set is skipped. |
| `G[0xc30115]` | 100 | 0..5 | 115 | -60..60, 11 distinct | OBINIT2: the wall points' x, from the object's place. |
| `G[0xc487b5]` | 100 | 0..5 | 115 | -60..0, 10 distinct | OBINIT2: the wall points' y. |
| `G[0xc60e55]` | 1 |  | 67 | 4..7, 3 distinct 4×19, 6×42, 7×6 | OBINIT2: the event polygon's point count, handed to `_SetMapEv*` (`FIELD@0x209a`..`0x218b`). A slot whose flag has bit 2 set is skipped. Touching it runs the object's action. |
| `G[0xc6123d]` | 100 | 0..6 | 67 | -130..130, 17 distinct | OBINIT2: the event points' x. |
| `G[0xc798dd]` | 100 | 0..6 | 67 | -85..45, 21 distinct | OBINIT2: the event points' y. |

`$1$OBINIT2` writes only a wall polygon of five points for kind 999, which has no other field. Its use is not traced.

The kinds fall into families:

| Kinds | What they are |
|---|---|
| 51..55 | The puzzle blocks (`puzzle_block_kinds`). |
| 100..105 | Markers. 101, 102 and 103 carry the atlas markers 1, 2 and 3: up stairs, down stairs and a passage. |
| 211..258 (odd) with 401..447 (odd) | The treasures: a closed kind and its open kind, with MAPSE002.WAV and the effect SO027D (24 frames in 6 columns at 50 ms). They carry marker 5. |
| 301..306 | The healing feathers, marker 4. |
| 321 | The gathering point, marker 6. |
| 201, 202, 206..209, 261..266, 281..283, 341, 501, 502 | Other objects: doors, locks and props. They are not split further. |

**The atlas markers** (`OBINIT@0x0`..`0x3e`, capacity 100, ids 1 to 6) are 13 cells outside the kind grid:

| Cells | Holds |
|---|---|
| `GS[0x269ce + k]` | The marker's name: 上り階段, 下り階段, 移動, 回復の羽, 財宝, 採取. No script outside OBINIT reads it. |
| `G[0xc91f7d + k]` | The key that shows the marker: 652, 652, 652, 104, 653 and 53. A marker is drawn when the debug switch `G[0xf45f4f]` is 1, when the key is 0, or when `G[0x72be + key] & 3` is 3 (`DRAWMINIMAP@0x1ef`..`0x233`; `ATLAS@0x4c5`..`0x4fd`). `G[0x72be]` is another group's. |
| `G[0xc91fe1 + k]` | The marker's y offset on the ATLAS screen: −35 for the feather (`ATLAS@0x9df`). |

DRAWMINIMAP cuts marker k's 9 × 9 icon from texture 0xac at (400 + 10 (k mod 9), 82 + 10 (k div 9))
(`DRAWMINIMAP@0x23a`..`0x280`).

### LAINIT (the lands)

Leader `G[0xc2b421]`, capacity 100. There are 15 lands, ids 1 to 91, all in the base. `$1$LAINIT` and `$2$LAINIT` are
empty. FIELD reads the land under the player from AGERC's `_GetLand` into `G[0x4dd6d]`, and DRAWCH moves it into
`field_land_id` (`DRAWCH@0x29`..`0x74`). Every LAINIT read is by `field_land_id`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc2b3bd]` | 1 |  | 15 | 20051..20059, 6 distinct 20051×1, 20052×1, 20054×4, 20057×4, 20058×3, 20059×2 | The footstep sound (WALKB.WAV, WALKI.WAV, …) on channel 3. 0 plays WALKA.WAV (`FIELD@0x2c62`..`0x2c7f`, `DRAWWALK@0x1ee`..`0x20b`). |
| `G[0xc2b421]` | 1 |  | 15 | 1..14, 10 distinct | The walking sheet: an index into COINIT's `G[0xbdc699]`. DRAWCH sets the sheet on texture 0x67 when the land changes (`DRAWCH@0x37`..`0x6d`; `SETMAPTEX@0x94`). OBJACT reads it with `G[0xbdc6b3]` and `G[0xbdc67f]` for textures 0x66 and 0x67 (`OBJACT@0x117a`..`0x11ab`). |
| `G[0xc2b485]` | 1 |  | 12 | 1..1, 1 distinct 1×12 | Flags. With bit 0 set, DRAWCH draws the 56 × 42 picture under the player with alpha 0 (`DRAWCH@0x1af`..`0x1d2`). CARRY, DRAWHOIST, DRAWJUMP and DRAWWALK do the same. That the picture is the player's shadow is an inference. |
| `G[0xc2b4e9]` | 1 |  | 11 | -40..-20, 2 distinct -40×5, -20×6 | The speed change in percent. The step is max(1, 50 (100 + v) / 1000) (`FIELD@0x2bc0`..`0x2bdc`, `DRAWWALK@0x153`). Land 31 has only this field. |

FIELD reads four more arrays that LAINIT never writes:

- **`G[0xc2b54d]` and `G[0xc2b5b1]`** are a current's x and y: each frame adds v × step / 100 to the move
  (`FIELD@0x3032`..`0x3055`, `@0x3912`..`0x392e`).
- **`G[0xc2b615]`.** When it is 0, FIELD drops the part of the move that opposes the current (`FIELD@0x2fdb`..`0x302d`,
  `@0x38bb`).
- **`G[0xc2b679]`** is an encounter level that replaces the block's when it is nonzero (`FIELD@0x2a4d`).

These are 戦女神ZERO's drift and encounter-level fields, read by the same code. This game sets none of them.

### PZINIT (the block puzzles)

Leader `G[0xc92045]` (the reset rule), capacity 100. There are 11 puzzles, ids 3 to 13, all in the base. A map script
places a puzzle in the map in play (`G[0xc2b38f]`, counted by `G[0xc2b3bc]`). CARRY moves its blocks.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc92045]` | 1 |  | 11 | 3..3, 1 distinct 3×11 | The reset rule: 1 resets the puzzle on entering the dungeon, 2 on a floor change and 3 on a map-block change (`FIELD@0x422`..`0x492`). Every puzzle has 3. |
| `G[0xc920a9]` | 1 |  | 11 | 3..11, 4 distinct 3×3, 6×6, 8×1, 11×1 | The grid's width in cells. |
| `G[0xc9210d]` | 1 |  | 11 | 1..9, 6 distinct 1×1, 3×2, 4×4, 5×1, 7×1, 9×2 | The grid's height in cells. |
| `G[0xc92171]` | 484 | 23..209 | 11 | 0..11, 4 distinct 0×151, 1×187, 10×16, 11×2 | The grid, 22 × 22, row-major, with a border (`copy-inline-int-array`). 1 is floor. A value of 10 or more is a goal, and 10 + colour is a goal for blocks of that colour (`CARRY@0x16f`..`0x274`). |
| `G[0xc9de81]` | 1 |  | 11 | 1..8, 4 distinct 1×7, 2×1, 4×1, 8×2 | The block count. |
| `G[0xc9dee5]` | 8 | 0..7 | 11 | 1..10, 10 distinct | Each block's starting column. `FIELD@0x4d3f`..`0x4d70` copies it into the saved `puzzle_block_columns`. |
| `G[0xc9e205]` | 8 | 0..7 | 11 | 1..8, 8 distinct | Each block's starting row, copied into `puzzle_block_rows`. |
| `G[0xc9e525]` | 8 |  | 1 | 2 cells (`G[0xc9e546]`, `G[0xc9e547]`) = 1 | Each block's colour (`CARRY@0x230`, `@0x6da`). Only puzzle 4's blocks 1 and 2 have one. The tool counts these as the 2 cells outside its arrays. |
| `G[0xc9e845]` | 8 | 0..7 | 11 | 1..1, 1 distinct 1×19 | A block flag. CARRY refuses a move when it is 1 and its local `0x15` is above 0 (`CARRY@0x55c`..`0x66a`), and FIELD tests it at `FIELD@0x44ae`. Not decoded further. |
| `G[0xc9eb65]` | 8 | 0..7 | 11 | 51..55, 5 distinct 51×10, 52×1, 53×4, 54×5, 55×9 | Each block's object kind, 51 to 55. |

A puzzle is solved when every goal holds a block of its colour. CARRY then sets bit 0 of `puzzle_states`
(`CARRY@0x713`).

### FLINIT (the objects of the floor in play)

FLINIT runs on demand: it is slot 0 of the chain `G[0xf45dcd]` (`INIT2@0x7a`). FIELD walks the chain on every map-block
entry (`FIELD@0x399`). The script branches on `current_floor_id`: 71 base floors, floors 81 and 82 in `$1$FLINIT`, and
floor 86 in `$2$FLINIT`. A branch writes the floor's slots:

| Cells | Holds |
|---|---|
| `G[0xca28b9 + s]` | The slot's starting kind, an OBINIT id. Slots run from 1 to the count − 1; slot 0 is never written. |
| `G[0xca291d + s]` | The slot's action, 1 to 6, which `OBJACT@0x0` dispatches with a value switch. 0 is none. |
| `G[0xca2981 + 3s + c]` | The action's three arguments. |
| `G[0xca2aad]` | The slot count (`FIELD@0x545`). |

The six actions:

| Action | Slots | Kinds | What OBJACT does |
|---|---|---|---|
| 1 | 372 | the treasures, 201 and 202 | Gives item argument 0, argument 1 of them, through ADDITEM (`OBJACT@0x2ae`..`0x2e0`), and shows 「…を手に入れた」. |
| 2 | 25 | 201, 206 and 211 | Gives argument 0 シリン (`OBJACT@0x645`..`0x73f`). |
| 3 | 45 | the feathers 301..304 | Runs RECOVERALL (`OBJACT@0x745`). |
| 4 | 18 | 261..266 | A lock: shows 何か仕掛けがあるようだ…… or 「…で施錠されているようだ……」, with item argument 0 as the key (`OBJACT@0x9bb`..`0xa36`). |
| 5 | 160 | the gathering point 321 | Gathers: gives an item through ADDITEM by arguments 0 and 1 (`OBJACT@0xf60`..`0x100d`). |
| 6 | 6 | the feather 306 | Adds 1000 to `goddess_gauge` `G[0x7250]`, clamped to `goddess_gauge_maximum` `G[0x7251]`. When `party_leader` `G[0x7259]` is 1 it runs the COINIT chain and sets the walking sheets again (`OBJACT@0x10ef`..`0x11ab`). |

After the action, OBJACT turns the slot's kind into its next kind (`object_kind_next_kinds`).

On entry, FIELD sets up each slot below the count (`FIELD@0x545`..`0x5de`):

- If the slot has a starting kind, FIELD copies it into the saved `floor_object_kinds`, unless the slot's flags have
  both bit 0 (used) and bit 16 (present) set. So a used object keeps its turned kind.
- FIELD then sets bit 16, unless the action is 5 and `G[0x76ad]` is not 1. Gathering points only show once
  `G[0x76ad]` is 1.
- A slot with no starting kind loses bit 16.

FLINIT clears nothing, and every base floor writes every slot below its count (checked). A smaller count hides a
previous floor's higher slots, and no map script places them.

**The scope.** The registry's scope `dungeon_floor`, 1 to 86, holds the table `floor_object` (100 records, slot s).
FLINIT's branch for the floor in play fills it on each entry, like 戦女神ZERO's RMINIT for its `dungeon` scope. The
base mod's `ladea.floor.loaded` comes after the walk (`FIELD@0x3b4`), so a data file's floor part is in the slots
when FIELD sets them up. A load on a map runs FIELD's opening again, the walk with it (the map in play, above), so the
slots, the event and the part come back after each load too. Run on both targets on 2026-11-27: saved on floor 65 at
the debug menu's 迷宮マップ２, the save loaded, slot 9's chest gives its two 血廉の息吹 (`223`, 2) on native AGE as in
the port; in the port a load from the title raises `ladea.floor.loaded floor=65` again, with the data file's part in
slot 8.

### DUINIT (the fixed battles)

DUINIT runs on demand: it is slot 0 of the chain `G[0xf45e2d]` (`INIT2@0x89`). The game walks the chain in two places:

- FIELD, after clearing the parties (`FIELD@0x3d7f`).
- BTLMAIN, when a battle id is set (`BTLMAIN@0xac`..`0xea`).

The script branches on the battle id `G[0xc20a5]`:

- The base has 205 battles, ids 1 to 913.
- `$1$DUINIT` has 6, ids 701 to 706.
- `$2$DUINIT` has 17: 541, 542 and 751 to 765.

No id is in two layers. MPEV and SC scripts set the id (135 of them). So does FIELD, for a special encounter, from
`G[0xd0a75b + k]` (`FIELD@0x3d3e`..`0x3d45`).

Before the chain, BTLMAIN sets the defaults (`BTLMAIN@0x65`..`0xb3`):

- The place name becomes `current_place_name`.
- The flags and `G[0xd0a7bf..0xd0a7c4]` are cleared.
- The backdrops are cleared.
- The music is `G[0xc20ac]`, or 0x14.
- The parties are cleared.

A branch then writes:

| Cells | Holds |
|---|---|
| `GS[0x26e7e]` | The battle's place name (アルマの途, 黄金樹の谷, 東館Ｂ６Ｆ, …). |
| `G[0xc20a6 + w]` | The enemy party of wave w (0 to 2). It lies in the saved prefix. SETENP reads it (`SETENP@0x4d4`). |
| `G[0xd0a7cf]` | The battle's flags, such as 0x40. CHECKACT, CHECKSEAL and others read them; the bits are not decoded. |
| `G[0xd0a7d1 + w]` | Wave w's music (`BTLMAIN@0x4aa`, by the wave `G[0xd0a7e1]`). |
| `G[0xd0a7d4 + 2w + k]` | Wave w's two backdrops. When they are 0, the map block's are used (`BTLMAIN@0x5979`..`0x59c7`). |

The battle cells above the prefix are BTLMAIN's, described in "Skills, conditions and battle". The parties are ESINIT's or
ENINIT's ids ("Encounters and enemies").

### The map in play

SETMAPDATA clears the map in play (`SETMAPDATA@0x1c`..`0x6c`), and the block's map script fills it:

- It copies the backdrop's frame count and parallax into `G[0xc0caa9]` and `G[0xc0caaa]`, and the overlay's scale
  into `G[0x4dd8a]` (`SETMAPDATA@0xa6`..`0xbe`).
- It calls the block's map script (`SETMAPDATA@0xd8`).

None of the map in play is saved, except `G[0x4dd8a]`. Its lists each have a count:

| Count | List |
|---|---|
| `G[0xc2b3b7]` | Props (the arrays from `G[0xc0ce3f]`; their flags are saved in `map_block_prop_flags`). |
| `G[0xc2b3b8]` | Walls: polygons at `G[0xc0d47f]` and `G[0xc0d597]`, handed to `_SetMapWall*` (`FIELD@0x2370`..`0x23f2`). |
| `G[0xc2b3b9]` | A third list, not decoded. |
| `G[0xc2b3ba]` | The floor objects placed in the block: the slot `G[0xc2016f + i]`, x `G[0xc20557 + i]` and y `G[0xc2093f + i]` (`MP011@0xebd`..`0xf26`). |
| `G[0xc2b3bb]` | Event areas (`G[0xc21687]`, `G[0xc2174f]`), handed to `_SetMapEv*`. |
| `G[0xc2b3bc]` | The block's puzzles, `G[0xc2b38f + i]`. |

These cells lie just below LAINIT's, which is why a range survey counts their 273 readers (the MP and MPEV scripts)
as LAINIT's. LAINIT's own readers are DRAWCH, DRAWWALK, DRAWJUMP, DRAWHOIST, CARRY, OBJACT, SETMAPTEX and FIELD.

The AGERC module that FIELD loads (`FIELD@0x0`..`0x53`) tests the polygons: `_FindClash`, `_GetLand`, `_GetEv`,
`_SetMapWall*`, `_SetMapLand*`, `_SetMapEv*` and `_SetChParams`.

### What is saved

The game copies into these cells below the prefix:

**The place:**

| Cells | Holds |
|---|---|
| `G[0x4dd63]` | `current_map_block_id`: the block in play. |
| `G[0x4dd64]` | `map_block_destination_id`: the block to go to. |
| `G[0x4dd65]` | `previous_map_block_id`: the previous block. |
| `G[0xb8463]` | `current_floor_id`: the floor in play, from `map_block_floors` (`FIELD@0x1c4`). |
| `G[0xb8464]` | `previous_floor_id`: the previous floor. |
| `GS[0x34d]` | `current_place_name`: the block's name (`FIELD@0x266`). |
| `G[0x4dd62]` | A map-entry state, 0, 1 or 2 (`FIELD@0x1b1`, `@0xb9e`, `@0x1b95`, `@0x4cc3`). Not decoded. |

**The player:**

| Cells | Holds |
|---|---|
| `G[0x4dd66]`, `G[0x4dd68]`, `G[0x4dd6a]` | `map_player_x`, `map_player_y`, `map_player_facing`. The camera is x − 640 and y − 360 (`INIT2@0x436`; `DRAWCH@0x1b`). |
| `G[0x4dd67]`, `G[0x4dd69]`, `G[0x4dd6b]` | `map_player_target_x`, `map_player_target_y`, `map_player_target_facing`: the values the first three are set from. |
| `G[0x4dd6c]` | `field_land_id`: the land in play (`DRAWCH@0x74`). |
| `G[0x4dd6d]` | The land just found, before DRAWCH moves it in. |
| `G[0x4dd83]`, `G[0x4dd82]` | `map_music_variant` and `map_encounter_variant`. |
| `G[0x4dd8a]` | The overlay's scale, from `map_block_overlay_scales`. |

**The floor objects,** four arrays of 100 floors × 100 slots:

| Cells | Holds |
|---|---|
| `G[0xb8465]` | `floor_object_flags`: bit 0 used, bit 16 present (`FIELD@0x56c`, `@0x5c6`). Bits 1 and 2 skip the wall and the event polygon (`FIELD@0x1fac`, `@0x209a`). DRAWFIELD and DRAWMINIMAP draw a slot with bit 16 set (`DRAWMINIMAP@0x1b0`..`0x1c2`). |
| `G[0xbab75]` | `floor_object_kinds`: the slot's kind now (`FIELD@0x581`; OBJACT turns it). |
| `G[0xbd285]` | `floor_object_x_offsets`: added to the placed x (`DRAWMINIMAP@0x2a6`). MPEV322 resets slots 4 and 5 to 0 (`MPEV322@0x25d4`, `@0x2b69`). |
| `G[0xbf995]` | `floor_object_y_offsets`: added to the placed y. |

**The visits:**

| Cells | Holds |
|---|---|
| `G[0xcda3b + f]` | `floor_seen_flags`: the floor was visited. It is also stored in the shared profile (`FIELD@0x203`). INFOEN sets it, or loads it from the shared profile (`INFOEN@0x1df0`..`0x1e06`). |
| `G[0x4ef22 + f]` | `floor_atlas_initialised`: the hidden parts have been copied (`FIELD@0x242`). |
| `G[0x4e752 + 20f + p]` | `floor_atlas_parts_hidden`: bit 0 hides the part. DRAWMINIMAP and ATLAS draw a part with a picture and bit 0 clear (`DRAWMINIMAP@0x40`..`0x72`, `ATLAS@0x75f`..`0x791`). The map and event scripts clear the bit to reveal a part, 146 times, and set it 99 times (`MP011@0x12ba`). |
| `G[0x4ef86 + f]` | `floor_visited_flags`: bit 0, the floor was visited (ATLAS lists only these). |
| `G[0x4efea + b]` | `map_block_visited_flags`: bit 0, the block was visited (`FIELD`). |

**The blocks and puzzles:**

| Cells | Holds |
|---|---|
| `G[0x80112 + 100b + i]` | `map_block_prop_flags`: each block's prop flags (`DRAWFIELD@0xac9`). |
| `G[0x987b2 + 100b + i]` | `map_block_wall_flags`: each block's wall flags. |
| `G[0xb0e52]`, `G[0xb21da]`, `G[0xb3562]`, `G[0xb48ea]`, `G[0xb5c72]`, `G[0xb6ffa]` | Six [1000][5] arrays by block. They look like the block's actors, but they are not decoded. |
| `G[0x4dd8e + p]` | `puzzle_states`: bit 0 solved (`CARRY@0x713`), bit 4 the blocks placed (`FIELD@0x4d7f`). |
| `G[0x4ddf2 + 8p + b]` | `puzzle_block_states`: each block's flags. |
| `G[0x4e112 + 8p + b]`, `G[0x4e432 + 8p + b]` | `puzzle_block_columns` and `puzzle_block_rows`: each block's place, from PZINIT's start (`FIELD@0x4d3f`..`0x4d70`). |

**The battle:**

| Cells | Holds |
|---|---|
| `G[0xc20a5]` | `battle_id`: the fixed battle, DUINIT's key. |
| `G[0xc20a6..0xc20a8]` | The waves' parties (`battle_wave_parties`). |

Nothing of MPINIT, ATINIT, OBINIT, OBINIT2, LAINIT, PZINIT or FLINIT is saved. A save keeps the floor objects' kinds
and flags, the visits, the hidden parts, the puzzles' places and the place. After a load INIT2 rebuilds the map in play
from the block's map script, and FIELD's opening, which the load runs again, walks the FLINIT chain.

## The town: buildings, places, quests, the inn and the unlock board

The town is ミルフェ, drawn as an isometric map of buildings. Game mode 2 (`G[0x0]`, SYSTEM4@0x665..0x6bd) runs TOWN.
A place is one of the buildings you can enter, ids 1 to 20. Inside a place, TOWN draws its spots: characters you can
talk to, exits to the dungeon, and doors to the town's screens. The screens are the inn, the shop, alchemy,
improvement, breeding, the quest board and, with the second append, the arena. Game mode 5 runs DEAL, which calls the
screen. The quest board and the unlock board (解放, FACT) are town screens too, and each keeps its progress in the
save.

Five boot tables feed the town: BLINIT (buildings and places), QUINIT (quests), NUINIT (the inn's rites), FMINIT (the
unlock board's grid) and FAINIT (unlocks). One table is built on demand: ORINIT, the spots of the place in play. Only
the town reads them, with QUEST, RECEIPT, INFOQU, INFOQU2, INN, FACT, OPENFACT and INFOFA. CLOSE and the scenes
change the town's saved state.

### The town's screens (DEAL)

INIT2 writes the screen scripts into `G[0xbf0164]`..`G[0xbf0169]` (`INIT2@0xca`..`0xe3`). They are 1 INN, 2 SHOP,
3 ALCHEMY, 4 IMPROVE, 5 BREED and 6 QUEST. `$2$AUTORUN@0xca` adds 7, `$2$ARENA` (`0x2000039`), in
`G[0xbf016a]`. The array starts at `G[0xbf0163]`, indexed by screen kind 1..7, and element 0 is never written.

A spot of kind "screen" sets the screen kind `G[0x4d2cf]` and its argument `G[0x4d2d0]`, then switches to mode 5
(`TOWN@0x13c6`, `@0x13d2`). DEAL runs `G[0xbf0163][G[0x4d2cf]]` if it is set (`DEAL@0x0`..`0x15`). It then clears
the kind (`@0x18`). If the mode is still 5, DEAL sets the previous mode `G[0x2]` to 5, returns to mode 2, and sets
the field cell `G[0xc09f31]` to 1 (`@0x1d`..`0x4d`). The argument picks the shop: SHINIT branches on it
(`SHINIT`: `G[0x4d2d0] == 1`, `2`, `3`).

This array is not a hook chain. It has no layer slots, and the second append writes the seventh element directly.

### BLINIT (buildings and places)

Leader `GS[0x229dc]` (the building's name), capacity 300. There are 81 records. Ids 1 to 19 are the places
(郊外へ出る, 野牛の蹄亭, 自由市場, イーリュン教会, ミルフェ衛兵団, 領主の館, 酒場, アリシアの家, 広場, 麝香商会,
地虫の巣, 裏路地, 倉庫街裏, アリシアの店, 邪神官の隠れ家, 麝香商会倉庫, 広場, カドラ廃坑, シュリの工房). Ids 101
to 121 are fillers named 建物１０１.., and ids 201 to 240 are the forty houses. `$2$BLINIT` adds building 20,
闘技場, with place 20. The `$1$` layer is empty. No layer rewrites a base cell.

The building arrays sit on the 300-cell grid from `G[0xbf016e]` to `G[0xbf4ec6]`. Five per-place arrays of 100 cells
follow, from `G[0xbf4ff2]` to `G[0xbf5182]`, and the run ends at `G[0xbf51e6]`, where ORINIT's arrays start. Every
base agrees with its grid. The tool merges the five 100-cell arrays into two: `G[0xbf4ff2]` with 60 records and
`G[0xbf511e]` with 40. The table below splits them by their readers.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x229dc]` | 1 |  | 81 | 郊外へ出る / 野牛の蹄亭 / 自由市場 | The name. TOWN lists the places' names (`TOWN@0x23a8`, `@0x244d`). |
| `G[0xbf016e]` | 1 |  | 80 | 1..23 | The tile x on the town map (`TOWN@0x1a5d`). |
| `G[0xbf029a]` | 1 |  | 80 | 1..32 | The tile y (`TOWN@0x1a64`, `@0x2234`). |
| `G[0xbf03c6]` | 1 |  | 81 | 1..3 | The width in tiles. TOWN uses it to find the building under the cursor (`TOWN@0x221f`, `@0x2265`). |
| `G[0xbf04f2]` | 1 |  | 81 | 1..3 | The height in tiles (`TOWN@0x223b`, `@0x226c`). |
| `G[0xbf061e]` | 1 |  | 3 | -10 | A depth bias in the draw order. The sort key is `((x + y) × 16 + bias) << 8` (`TOWN@0x1a79`). |
| `G[0xbf074a]` | 1 |  | 20 | 1..20 | The place you enter. TOWN copies it into the place in play, `G[0x4d590]` (`TOWN@0x32c`, `@0x10e7`). Only buildings 1..20 have one, and each is its own id. |
| `G[0xbf0876]` | 3 | | | never written | The leader each variant requires, `G[0x7259]`. 0 means none (`TOWN@0x1e76`). |
| `G[0xbf0bfa]` | 3 | | | never written | The lowest story progress `G[0x3b1d]` for each variant (`TOWN@0x1ea2`). |
| `G[0xbf0f7e]` | 3 | | | never written | The highest story progress (`TOWN@0x1ece`). |
| `G[0xbf1302]` | 3 | | | never written | A mask on `G[0x4d2cc]`: `check_bit(mask, G[0x4d2cc])` must pass (`TOWN@0x1efa`, `@0x1f17`). |
| `G[0xbf1686]` | 15 | | | never written | Five required story flags for each variant, `G[0xee7 + n − 1] == 1` (`TOWN@0x1f58`). |
| `G[0xbf281a]` | 15 | | | never written | Five forbidding story flags for each variant (`TOWN@0x1fce`). |
| `G[0xbf39ae]` | 18 | 0, 2, 4 | 81 | packed picture ids, TB001AAA (`0x3d52`) on | Six pictures for each of three variants, column `variant × 6 + kind + G[0x4d2cc]`. The kind is 0 for a building, 2 for a house of faction 1 and 4 for a house of faction 2 (SETTOWNTEX@0xd7). A variant is drawn when its column `variant × 6` has a picture (`TOWN@0x1a1d`). The data fills only variant 0's columns 0, 2 and 4. |
| `G[0xbf4ec6]` | 1 |  | 20 | packed picture ids, TN001 (`0x3d50`) on | The place's tab picture. A shown building that has one gets a tab on the place list, up to 30 (`TOWN@0x1ab6`, `@0x21af`). |
| `G[0xbf4ff2]` | 1 (100 cells) |  | 20 | 10..170 | The list order. TOWN copies the first 100 cells as the sort keys of buildings 1..99 (`TOWN@0x1990`). |
| `G[0xbf5056]` | 1 (by place) |  | 20 | 1 or 3 | The place's layout: where the spot grid's origin goes. Layout 1 is (640, 23), 2 is (430, 53), 3 is (850, 53), 4 is (430, 353) and 5 is (850, 353) (`TOWN@0x1dc9`..). |
| `G[0xbf50ba]` | 1 (by place) |  | 20 | 159..977 | The x in pixels of the place's marker on the town map (`TOWN@0x1cbd`, `@0x237b`). |
| `G[0xbf511e]` | 1 (by place) |  | 20 | 92..635 | The marker's y (`TOWN@0x1cd9`, `@0x2389`). |
| `G[0xbf5182]` | 1 (by place) |  | 20 | packed picture ids, TM001 (`0x3d51`) on | The place's background (SETESTTEX@0x0; `TOWN@0x28ce`). |

**The variants.** TOWN rebuilds the town map each time it opens (TOWN label_192a, `@0x1990`..). It sorts the 300
buildings by the list order. For each building it tries variants 0, 1 and 2 in turn, and the last one whose picture
is set and whose conditions pass wins (TOWN label_1e71). The winner goes in the saved `G[0x4d3ff][building]`, and
−1 hides the building (`TOWN@0x19b3`). BLINIT writes no condition, so every building shows its variant 0. All six
condition arrays exist and TOWN reads them, so a mod can add a variant with its own conditions.

**The houses.** SETTOWNTEX picks a house's picture by faction (SETTOWNTEX@0xd7). Houses 201 to `200 + G[0x4d2cd]`
show faction 1's picture (商工会). Houses from `241 − G[0x4d2ce]` up show faction 2's (麝香商会). The faction points
are 0 to 40 each, and their sum is at most 40 (RECEIPT, below), so the forty houses split between the two.

**`G[0x4d2cc]`.** TOWN tests it as a bit mask (`TOWN@0x1f17`, `@0x20a5`), and SETTOWNTEX adds it to the picture
column. It also picks the town's texture, SO007B (`0x64ff`) when set and SO007A (`0x6500`) when not. No script writes
it, so it is always 0, the save included. What it stands for is not established.

**Places.** A building whose tab picture is set and whose place is open appears on the place list. Bit 0 of the
saved `G[0x4d52b][place]` marks the place open. When TOWN lists the place it also sets bit 1 (`TOWN@0x1ae5`,
`@0x1b08`), and nothing reads bit 1. The scenes open and close places with `bit-set` and `bit-reset` on the single
cells. For example, SC0000@0x187ca opens places 2 and 6, and SC0020@0x9495 closes place 17 and opens 9. CLOSE opens
place 19 once story flag 8961 is set (`CLOSE@0x156`). DEBUGTOWN opens every place.

### ORINIT (the spots of a place, on demand)

ORINIT is not in the boot image. INIT2 puts it in slot 0 of the chain `G[0xf45d8d]`, and the appends put their
`$1$`/`$2$` fragments in slots 1 and 2 ("Hook chains"). TOWN builds the place in play from the chain each time it
enters or redraws a place (TOWN label_1d1b, `@0x1d1b`, runs SETORNDATA). SETORNDATA zeroes every ORINIT array,
including the grid's 400 cells (`SETORNDATA@0x7`), then walks the chain (`@0xcc`). Each script branches on the place
in play, `if G[0x4d590] == place`. The base covers places 1 to 19 with 3,402 cells, `$1$` adds spots to places 1, 2,
9 and 17 (76 cells), and `$2$` adds to 3 and to 20, the arena (133 cells). No fragment rewrites a base cell.

The record is a spot, ids 1 to 199 on a 200-cell grid. A spot below 100 is a positioned spot: it has a grid position,
a picture and an action. A spot from 100 is a hotspot: ORINIT writes it straight into the click grid, and it has only
a picture. A record writes x, y and the pictures before any other field, so the table has no single leader write.
`init_layout.py` cannot lay it out: ORINIT is not in the boot image, and its branches are by place.

| array | cells per id | columns written | spots (place, id) | values | holds |
|---|---|---|---|---|---|
| `G[0xbf51e6]` | 20 × 20 | 2..19 | 162 hotspots in 14 places | 101..138 | The click grid `[y][x]`, which holds a spot id. ORINIT writes the hotspots, ids 100 and up. TOWN places each shown positioned spot at its (x, y), and a spot whose cell is already taken is not shown (`TOWN@0x1d95`, `@0x1db6`; drawn `@0x293a`). |
| `G[0xbf5376]` | 1 |  | 219 | 1 (212), 31 (7) | The party leader the spot requires, `G[0x7259]`. 0 means anyone (TOWN label_201f, `@0x2024`). |
| `G[0xbf543e]` | 1 |  |  | never written | The lowest story progress `G[0x3b1d]` (`TOWN@0x2048`). |
| `G[0xbf5506]` | 1 |  |  | never written | The highest story progress (`TOWN@0x206c`). |
| `G[0xbf55ce]` | 1 |  | 2 | 1, 2 | A mask on `G[0x4d2cc]` (`TOWN@0x2090`). It is always 0, so these two spots are never shown. |
| `G[0xbf5696]` | 5 | 0..4 | 233 | flags 11..10000 | Five required story flags (`TOWN@0x20d8`). Flag 10000 is required 42 times. |
| `G[0xbf5a7e]` | 5 | 0..4 | 179 | flags 22..8975 | Five forbidding story flags (`TOWN@0x2132`). |
| `G[0xbf5e66]` | 1 |  | 209 | 2..999 | The spot's character. It is the column of the scene matrix `G[0xcf36f][character][place]` that the spot plays (`TOWN@0x13df`, `@0x13f7`). It also keys the voice settings that gate the spot's voice (`TOWN@0x14ab`). Values 997 to 999 appear 16 times. |
| `G[0xbf5f2e]` | 1 |  | 263 | 3..15 | The x on the place's grid. The screen x is `origin + (x − y) × 40` (`TOWN@0x988`). |
| `G[0xbf5ff6]` | 1 |  | 263 | 3..17 | The y. The screen y is `origin + (x + y) × 20` (`TOWN@0x994`). |
| `G[0xbf60be]` | 2 | 0..1 | 425 | packed picture ids | [0] is the spot's picture, which SETESTTEX loads as texture `0x2bc + id`. A spot without one is not shown (`TOWN@0x1d40`). [1] is a sprite played when the spot is chosen (`TOWN@0x1543`, `@0x156c`). |
| `G[0xbf624e]` | 2 | 0..1 | 263 | 1..12 | The frame count of each picture's animation, for `animate_gfx_srcrect_target` (`TOWN@0x15b5`, `@0x2994`). |
| `G[0xbf63de]` | 2 | 0..1 | 263 | 1..12 | The column count of each picture's frame sheet (`TOWN@0x158c`). |
| `G[0xbf656e]` | 2 |  |  | never written, never read | SETORNDATA zeroes it; nothing else names it. |
| `G[0xbf66fe]` | 1 |  | 82 | voice resources | The voice the spot plays when chosen (`TOWN@0x149d`, `@0x1535`). |
| `G[0xbf67c6]` | 1 |  | 34 in 6 places | field maps 11..861 | An exit: the field map to go to. TOWN sets `G[0x4dd64]` and switches to mode 3 (`TOWN@0x130b`, `@0x1342`). |
| `G[0xbf688e]` | 1 |  | 34 | 310..3560 | The exit's x, added to the map's origin `G[0xca74e6][map]`, into `G[0x4dd67]` (`TOWN@0x134e`). |
| `G[0xbf6956]` | 1 |  | 34 | 270..2635 | The exit's y, added to `G[0xca78ce][map]`, into `G[0x4dd69]` (`TOWN@0x1363`). |
| `G[0xbf6a1e]` | 1 |  | 34 | 0..7 | The facing on arrival, into `G[0x4dd6b]` (`TOWN@0x1371`). |
| `G[0xbf6ae6]` | 1 |  | 20 in 9 places | 1..7 | A door: the screen kind, into `G[0x4d2cf]`, then mode 5 ("The town's screens") (`TOWN@0x138f`, `@0x13c6`). |
| `G[0xbf6bae]` | 1 |  | 4 | 1..4 | The screen's argument, into `G[0x4d2d0]` (`TOWN@0x13cb`, `@0x13d2`). |

**A spot's action.** Clicking a spot does the first of these that applies (`TOWN@0x130b`..`0x153c`):
1. An exit goes to its field map.
2. A door opens its screen.
3. A character whose scene matrix cell `G[0xcf36f][character][place]` is 0 or more plays that scene. TOWN sets mode
   1, `G[0xe7a0f]` to the scene and `G[0x3a4b] = G[0xea48f][scene]`.
4. Otherwise the spot plays its voice, unless the voice settings mute it, and its sprite.

The scene matrix is not ORINIT's. SETADVFLAG fills it with −1 when `G[0x3a4b]` is 4 (`SETADVFLAG@0x1c`), and
"Scenes and screens" covers what fills its cells. When a place opens, TOWN marks the place tab of each kind of spot it holds: a scene's tab,
picked by `G[0xf19bf][scene]`, an exit's and a door's (`TOWN@0x1b42`..`0x1bbe`).

### QUINIT (quests)

Leader `G[0xbf6c76]` (the sort key), capacity 1000. Each record writes the key before the title
(`QUINIT@0x0` writes `G[0xbf6c77]`, then `GS[0x22b09]`). There are 217 records, ids 1 to 336. `$2$QUINIT` adds 11:
35, 166 to 174, and 336. The `$1$` layer is empty, and no layer rewrites a base cell. The integer arrays sit on the
1000-cell grid from `G[0xbf6c76]` to `G[0xc04b1e]`, and the strings from `GS[0x22b08]` to `GS[0x236c0]`. NUINIT's
strings follow at `GS[0x26988]`.

A quest exists if its title is set (`QUEST@0xad8`). QUEST is the town's quest board (screen 6), and RECEIPT its
report counter (`QUEST@0x294`). INFOQU2 is the quest page of the in-game record, INFO2 (`INFO2@0x60`), and lists this
save's quests by their states. INFOQU is the quest page of INFO, the record book (`INFO@0x0` lists it), which shows
the quests ever accepted from the shared profile. INFOFA is INFO's unlock page.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xbf6c76]` | 1 |  | 217 | 10..4100 | The list order. INFOQU, INFOQU2 and RECEIPT sort by it (`INFOQU@0x8e8`, `INFOQU2@0x997`, `RECEIPT@0x172`), and so does INFOTU (`INFOTU@0x929`). |
| `G[0xbf705e]` | 3 | 0..2 | 216 | flags 21..9001 | Story flags that must be set for the board to offer the quest (QUEST label_e1a, `QUEST@0xe3c`). |
| `G[0xbf7c16]` | 3 | 0..2 | 13 | flags 51..1207 | Story flags that keep it off the board (`QUEST@0xe96`). |
| `G[0xbf87ce]` | 1 |  | 135 | items 2002..13108 | An item that must be held to offer it, `G[0x770a][item] + G[0xbeb343][item] > 0` (`QUEST@0xed3`). The 武器錬成 and 防具錬成 quests name the item to refine. |
| `G[0xbf8bb6]` | 3 | 0 | 177 | flags 14..8979 | Story flags that achieve the quest (QUEST label_c60, `QUEST@0xc60`). |
| `G[0xbf976e]` | 3 | 0 | 8 | flags 1026..1207 | Story flags that stop it from being achieved (QUEST label_c60). |
| `G[0xbfa326]` | 1 |  | 40 | items 31..2330 | An item that achieves it when held, counted the same way (`QUEST@0xc8b`, `@0xd76`). The 憑石研究 quests ask to see a stone. |
| `G[0xbfa70e]` | 3 | 0..1 | 13 | flags 51, 8824 | Story flags that fail the quest (QUEST label_b6c, `QUEST@0xb6c`). |
| `G[0xbfb2c6]` | 3 | 0 | 1 | flag 3422 | Story flags that stop it from failing (quest 25). |
| `G[0xbfbe7e]` | 12 | 0..3 | 217 | 1..4 | Each reward's kind: 1 an item (183), 2 シリン, money (75), 3 解放ポイント, unlock points (81), 4 勢力ポイント, faction points (12) (`RECEIPT@0x22f`; `INFOQU@0x117b`). |
| `G[0xbfed5e]` | 12 | 0..3 | 192 | 1..13425 | The reward's item, or for kind 4 the faction, 1 or 2. |
| `G[0xc01c3e]` | 12 | 0..3 | 217 | 1..200000 | The reward's amount (`INFOQU@0x118d`). |
| `G[0xc04b1e]` | 1 |  | 3 | flags 8832, 8850, 8961 | A story flag that reporting the quest sets (`RECEIPT@0x87b`). |
| `GS[0x22b08]` | 1 |  | 217 | 調査　『光の正体』 / 討伐　『黒狐岩』 | The title (`QUEST@0xad8`; `INFOQU@0x91a`). |
| `GS[0x22ef0]` | 1 |  | 217 | －－ (205), 商工会 (7), 麝香商会 (5) | The faction that posts it (`INFOQU@0x10ff`). |
| `GS[0x232d8]` | 1 |  | 217 | 盗賊団に捕まっていた人 / ミルフェの情報通 | The client (`INFOQU@0x110f`). |
| `GS[0x236c0]` | 13 | 0..3 | 217 | 黒狐団に捕まった時に… | The description lines. INFOQU draws columns 0 to 2 (`INFOQU@0x111f`..`0x1147`). Column 3 is written for one quest and nothing reads it. |

**Progress.** The saved `G[0x4d592][quest]` holds each quest's state:
- Bit 0: accepted. QUEST sets it and also sets the shared profile's `G[0xc575f][quest]` to 1
  (`QUEST@0x9c8`, `@0x9e0`).
- Bit 1: failed (`QUEST@0xc5a`).
- Bit 2: achieved (`QUEST@0xda1`).
- Bit 3: reported (`RECEIPT@0x8aa`).

QUEST checks each accepted quest whose state is exactly 1 (QUEST label_abb). The quest fails if every fail flag is
set and no fail-forbidding flag is. It is achieved if every success flag is set, no success-forbidding flag is, and the
success item is held. The fail check is skipped when both its flag lists are empty, and the success check when its
flag lists and its item are (`QUEST@0xb6c`, `@0xc60`). INFO counts the quests ever accepted from
the shared profile (`INFO@0xa06`).

**Rewards.** RECEIPT pays an achieved quest on its report:
- money `G[0x7254]`, kept within 0 to 9,999,999 (`RECEIPT@0x671`..`0x699`);
- unlock points `G[0x1fdab]`, kept within 0 to 999 (`@0x6a3`);
- items, through ADDITEM;
- faction points `G[0x4d2cd][faction − 1]`, each kept within 0 to 40. When the sum would pass 40, the other faction
  loses the excess (`@0x7e1`).

**Days.** CLOSE runs when TOWN opens: it is slot 0 of the first column of the chain `G[0xf45ced]` (`TOWN@0x56`).
For quests 13 to 30 and 34, CLOSE tests whether the state is exactly 1 (accepted, not yet resolved). If so, it sets
one or two story flags: 8823 to 8847 for quests 13 to 30, and 8138 for quest 34 (`CLOSE@0x5d`..). The quests'
conditions name these flags. Quest 25 fails once its flag 8824 and flag 51 are set. CLOSE also turns other counters
into flags. Once flag 8961 is set (the report of quest 33, 調査　『威圧の妙薬』), it opens place 19 and makes unlocks
46 and 48 available (`CLOSE@0x156`..`0x1aa`).

### NUINIT (the inn's rites)

Leader `GS[0x26988]` (the name), capacity 10, ids 1 to 5: 精力信仰・限, 静, 流, 動 and 極. The `$1$` and `$2$`
layers are empty. Only INN reads the table.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x26988]` | 1 |  | 5 | 精力信仰・限 / 精力信仰・静 / 精力信仰・流 | The rite's name. |
| `G[0xc09eb9]` | 1 |  | 5 | 200..1000 | The goddess gauge `G[0x7250]` after the rite, capped at the gauge's maximum `G[0x7251]` (`INN@0x77e`..`0x791`). |
| `G[0xc09ec3]` | 1 |  | 3 | 600..1000 | The gauge maximum the rite needs (INN's menu, `INN@0x7cb`..). |
| `G[0xc09ecd]` | 3 |  |  | never written | Unlocks the rite needs, each `(G[0x72be][unlock] & 3) == 3` (INN label_901). |
| `G[0xc09eeb]` | 3 | 0 | 2 | flags 8995, 8996 | Required story flags (INN label_901). |
| `G[0xc09f09]` | 3 |  |  | never written | Forbidding story flags (INN label_901). |
| `G[0xc09f27]` | 1 |  | 5 | packed picture ids, EV020A on | The event picture (INN). |
| `GS[0x26992]` | 6 | 1..5 | 5 | 女神化ゲージを200まで消耗する / シュリの疲労度が『０』の時 | The rite's description, five lines. Nothing reads them. |

INN builds its menu from the five rites (INN label_7cb, `INN@0x7cb`). A rite is offered when its conditions pass.
Its step depends on how far the gauge has to move, `|G[0x7250] − target|`: up to 100 is step 1, 200 step 2, 300
step 3, 400 step 4, and further step 5. The step sets シュリ's fatigue `G[0x7253]` to 3, 6, 10, 15 or 20. INN refuses
every rite while the fatigue is not 0. A rite sets the gauge, then the fatigue (`INN@0x77e`, `@0x7a0`). It then runs
ADDILL (`INN@0x7c2`) with ILINIT's cells: `G[0x158dd1] = 3`, `G[0x326a83] = 0x31` when the gauge rises or `0x30`
when it falls, and `G[0x326a84]` = the step. RESULT lowers the fatigue by 1 (`RESULT@0xab`), and LEAVECH clears it
(`LEAVECH@0xb8`).

### FMINIT (the unlock board)

FMINIT has no records. It writes three boards of 26 rows × 46 columns, one row at a time with
`copy-inline-int-array`, 44 cells a row, rows 1 to 24 and columns 1 to 44 (`FMINIT@0x0`..`0x163`). FACT reads them as
`[row][col]` with stride 46 (`FACT@0xbf2`..`0xddd`).

| array | type | cells | holds |
|---|---|---|---|
| `G[0xbd6a39]` | int[26][46] | 430 non-zero | The unlock (FAINIT id) in each cell of the board (`FACT@0xd8a`, `@0xdcb`). |
| `G[0xbd6ee5]` | int[26][46] |  | A link from the cell to the cell at column + 1 (`FACT@0xc13`, `@0x1211`). |
| `G[0xbd7391]` | int[26][46] |  | A link from the cell to the cell at row + 1 (`FACT@0xbf2`, `@0xc34`). |

`$1$FMINIT` places unlock 665 at (20, 21). `$2$FMINIT` places 852, 851, 853, 666 and 485 and their links. Each fills a
cell the base left 0. The boards end at `G[0xbd783d]`, FAINIT's current unlock.

### FAINIT (unlocks)

Leader `GS[0x1fee4]` (the name), capacity 1000. There are 436 records, ids 1 to 992. `$1$FAINIT` adds 665
(街機能『新商品入荷ＡＰ』), and `$2$FAINIT` adds five records, 485 among them. No layer rewrites a base cell. The
integer arrays sit on the 1000-cell grid from `G[0xbd783e]` to `G[0xbdc276]`, and the run ends at `G[0xbdc65e]`.
`G[0xbd783d]`, just below it, is the unlock in play: FACT, CLOSE and the scenes set it before running OPENFACT.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x1fee4]` | 1 |  | 436 | 戦闘機能『女神化』解放 / 戦闘機能『連鎖』解放 | The name. |
| `G[0xbd783e]` | 1 |  | 436 | 10..4440 | The list order (`INFOFA@0x8e8`). |
| `G[0xbd7c26]` | 2 | 0..1 | 357 | unlocks 1..992 | Two prerequisites. When an unlock has at least one and every one is unlocked (`& 3 == 3`), FACT makes it available, bit 0, and opens it at once if it costs nothing (FACT label_f4a, `FACT@0xfb8`). An unlock without prerequisites is made available by a scene or by CLOSE. |
| `G[0xbd83f6]` | 1 |  | 397 | 1..303 | The subject: the key of the shared-profile record `G[0xc47bf]` that reveals the node (FACT label_eea, `FACT@0xf3d`), and for kinds 7, 8, 11 and 12 the character who gets the bonus (`OPENFACT@0xb0`). |
| `G[0xbd87de]` | 1 |  | 436 | 1..14 | The kind, which OPENFACT switches on (`OPENFACT@0x5d`, `@0x64`); see below. |
| `G[0xbd8bc6]` | 5 | 0..2 | 289 | 1..215 | The targets of kinds 7 and 8: a special attack or a skill (`OPENFACT@0xd9`). |
| `G[0xbd9f4e]` | 5 | 0..2 | 399 | 1..32 | The amounts, one for each target. Column 0 is the only amount, or an index, for the other kinds (`OPENFACT@0x69`). |
| `G[0xbdb2d6]` | 1 |  | 149 | flags 8902..8978 | A story flag that unlocking sets (OPENFACT label_2d7). |
| `G[0xbdb6be]` | 1 |  | 370 | 10..300 | The cost in unlock points `G[0x1fdab]` (`FACT@0xb40`..`0xcb2`). An unlock with no cost opens as soon as it is available: CLOSE and the scenes run OPENFACT for it (`CLOSE@0x181`; SC0020@0x3767). |
| `G[0xbdbaa6]` | 1 |  | 61 | 1, 2 | The node's size: 2 makes a 2 × 2 node (`FACT@0xe2a`, `@0x14a9`). |
| `G[0xbdbe8e]` | 1 |  | 61 | packed picture ids | The node's picture, drawn once `G[0xc47bf][subject]` is set (`FACT@0xb5e`, `@0xe1c`). A node without a picture is drawn only next to an unlocked node it is linked to. |
| `G[0xbdc276]` | 1 |  | 436 | 1 | Listed in INFOFA and INFO (`INFOFA@0x91a`, `INFO@0xa5f`). |
| `GS[0x202cc]` | 10 | 0..9 | 436 | 【ヒント】初期自動解放 / 【条件】… / descriptions | The texts: columns 0 and 1 are the hint, 2 and 3 the condition, and 4 to 9 the description. |

**The kinds** (OPENFACT@0x64..0x2bd). The cells the kinds change belong to other domains:

| kind | records | example | what OPENFACT does |
|---|---|---|---|
| 1 | 4 | 迷宮機能『採取』解放 | `G[0x76a6][amount] = 1` |
| 2 | 27 | 街機能『夜伽』解放 | nothing: the screens test `G[0x72be]` themselves |
| 3 | 7 | 戦闘機能『女神化』解放 | nothing |
| 4 | 54 | 全体能力『ＬＶ上限＋３』解放 | `G[0x1ffbb] += amount` |
| 5 | 3 | 女神化『女神ゲージ＋１』解放 | the gauge maximum `G[0x7251] += 200` |
| 6 | 3 | ミニマップ機能『羽察知』解放 | nothing |
| 7 | 112 | 必殺技『飛燕剣』解放 | `G[0x3e5b0][subject][target] += amount` for each target |
| 8 | 177 | スキル『見切り』解放 | `G[0x25f10][subject][target] += amount`, then CALCREVISE |
| 9 | 5 | 戦闘機能『初期戦意＋３』解放 | `G[0x1ffbc] += amount`; then a loop over the party's `G[0x20858]`, capped at `G[0x208bc]` |
| 10 | 0 |  | `G[0x1ffbd] += amount` |
| 11 | 35 | 能力『憑依コスト＋１８』解放 | `G[0x20c40][subject][14] += amount` |
| 12 | 0 |  | `G[0x20c40][subject][15] += amount` |
| 13 | 5 | 戦闘機能『招聘延長＋１』解放 | `G[0x1ffbe] += amount` |
| 14 | 4 | 女神化『抑制＋１』解放 | `G[0x1ffbf] += amount` |

After the switch, OPENFACT sets the unlock's story flag, stores `G[0xc5b47][unlock] = 1` in the shared profile, and
sets bit 1 of the saved `G[0x72be][unlock]` (`OPENFACT@0x319`, `@0x323`). An unlock that already has bit 1 is skipped
(`OPENFACT@0x0`). FACT spends the cost and runs OPENFACT (`FACT@0xca1`, `@0x102c`). The kind's effect lands in saved
cells once, so a save keeps the bonus of every unlock it has, even if a patch later changes the unlock's amount.

### What is saved

None of the six tables is saved. BLINIT, QUINIT, NUINIT, FMINIT and FAINIT lie above the prefix (`G < 0xcf2e6`,
`GS < 0x34f`), so INIT2 builds them again on every boot and load. TOWN rebuilds ORINIT's arrays from the chain each
time it opens a place. A data patch on any of them takes effect for every save, except for what the game copied into
saved cells: an unlock's bonus is added to its targets once, when it is unlocked, and a quest's rewards are paid once,
when it is reported.

The town's saved state, all below the prefix:

| cell | holds |
|---|---|
| `G[0x4d592]` [1000] | The quest states: bit 0 accepted, 1 failed, 2 achieved, 3 reported (QUINIT, above). |
| `G[0x72be]` [1000] | The unlock states: bit 0 available, bit 1 unlocked (FACT label_f4a; `OPENFACT@0x323`). INN tests `& 3 == 3` (INN label_901). |
| `G[0x4d52b]` [100] | The place states: bit 0 open (set by the scenes and CLOSE), bit 1 listed (`TOWN@0x1b08`, no reader). |
| `G[0x4d3ff]` [300] | Each building's shown variant, or −1. TOWN refills it each time it opens (`TOWN@0x19b3`). |
| `G[0x4d2cd]` [2] | The faction points of 商工会 and 麝香商会, 0 to 40 each, at most 40 together. INIT sets 10 and 10 (RECEIPT@0x7e1; SETTOWNTEX). |
| `G[0x4d2cc]` | The mask the variants and pictures test. Never written. |
| `G[0x4d2cf]`, `G[0x4d2d0]` | The screen kind and its argument (DEAL; SHINIT). |
| `G[0x4d2d1]` | The building selected on the town map. TOWN sets the place in play from it (`TOWN@0x11a`, `@0x2ee`). |
| `G[0x4d590]` | The place in play (`TOWN@0x2f5`). SETORNDATA builds its spots, and SETESTTEX its background. |
| `G[0x4d2d2]`, `G[0x4d2d3]` [300] | The count and the list of the buildings TOWN shows (`TOWN@0x19c6`, `@0x1aa3`). |
| `G[0x7254]` | Money, 0 to 9,999,999. INIT sets 0 (RECEIPT; SHOP). |
| `G[0x1fdab]` | Unlock points (解放ポイント), 0 to 999. INIT sets 20 (RECEIPT; FACT). |
| `G[0x7250]`, `G[0x7251]` | The goddess gauge (女神化ゲージ) and its maximum. INIT sets 300 and 400. The rites set the gauge (INN), BTL changes it, and unlock kind 5 raises the maximum. |
| `G[0x7253]` | シュリ's fatigue (INN; RESULT; LEAVECH). |

These records are kept in the shared profile, which outlives a save:
- the quest accepted, `G[0xc575f][quest]` (`QUEST@0x9e0`; read by INFO and INFOQU);
- the unlock opened, `G[0xc5b47][unlock]` (`OPENFACT@0x319`; read by INFO and INFOFA);
- the record `G[0xc47bf][subject]` that reveals a node, which the scenes write and FACT loads (`FACT@0xf3d`).

`G[0xbd783d]` (the unlock in play), `G[0xbdc65e]` (the character of the spot chosen) and the screen array
`G[0xbf0163]` lie above the prefix and are not saved.

## What is not decoded

**Boot.**

- `G[0x5]`, the cell SYSTEM4 keeps in the shared profile to choose between INITCONFIG and LOADCONFIG; the options
  `G[0xcf2e3]`, `G[0xcda9f]`, `G[0xcdaa0]`, `G[0x6e60]`, `G[0x6e5f]`, `G[0x6e61]`, `G[0x6e5d]`, `G[0x6e63]`,
  `G[0x6e59]`, `G[0x6e62]` and `G[0x6e5c]` beyond what INITCONFIG's table cites, and `G[0x6e3b]` and `G[0x6e3d]`.
  The CONFIG pages draw their labels as pictures, so a look at the screen would name most of them.
- The 600 cells from `G[0x4cc8c]` that INIT sets to 2, beyond the tactics; INIT2's ranks `GS[0x26e81]` …, which no
  script reads by a base; and `G[0xc09f38]`.

**Scenes and screens.**

- `G[0xed1]`, which SC0000 sets to its own script id on the way out of its debug loop (`SC0000@0xd6d`), and the two
  debug switches `G[0xf45f4e]` and `G[0xf45f50]`, which nothing writes.
- The rows 200, 296 to 299 and 997 to 999 of `town_visit_scenes`, and what TOWN keys them by.
- What the marks of `scene_town_marks` look like (their kinds come from the scenes' comments), and whether `SO025.AGF`
  carries the music room's titles (`music_titles` has no reader).
- Whether a replay marks a CG's part set seen: `mark_cg_seen` does not test `replay_room_active` (not run).
- `scjump_decode.py` is written for Himegari's SCJUMP and was not run on this game; SCJUMP's rules are counted by
  their pattern.
- Of the 635 story-flag candidates only the scene and ending flags are named; the epilogue's flags 7100 to 7102 wait
  for their characters' names.

**Characters, voices, fates and the books.**

- Stat columns 9, 10, 13 and 15 of `character_base_stats`, `G[0x1dea72]` (a countdown of 1 to 10 on the unit), and
  equipment slots 3 and 4 (`G[0x1dcb32]`, `G[0x1dcf1a]`).
- Which column of `character_experience_rewards` BTLMAIN takes (`L[0x21]`; both columns are equal in the data).
- What fate bits 1 and 2 mean: no record fills their conditions, so neither can be set.
- Saved and not understood: `G[0x40810]` `[500]` (it turns a skill rank off), `G[0x4cb60]` `[3]`, `G[0x4cc8c]` `[6]`
  (bit 1 is the tactics), `G[0x1ffc0]`, `G[0xcee2d]`, `G[0xcf27e]`, and the 1,400 cells from `G[0x21348]` that no
  script names.
- `character_playable_flags` as "can join the party" is read from its 15 ids and SETCHLIST's filter: an inference.

**Skills, conditions and battle.**

- SSINIT `G[0x2fce8b]`, the unwritten 2,500 cells `G[0x300923]` … `G[0x3012e6]`, the second per-unit copy of the
  levels `G[0xf402b3]`, CHECKSS2's tests one by one, and SETSSBONUS's bonus cells `G[0xef7cd3]` … `G[0xef7cd8]`.
- AFINIT's `item_class_flags` bit 0, `G[0xbd6817]` and `G[0xbd6a16]`; the kind names, formations and field action
  names have no reader.
- ILINIT: the cure bits in full, which gauge `condition_gauge_drains` drains (read after 戦女神ZERO and Verita), and
  conditions 47 to 50 beyond their values (47, 抑制, and 46, 女神, follow `goddess_gauge`, "The town").
- COINIT's CI pictures, its value of 1 to 3, the animation rows and the field sheets, documented and not named.
- RTINIT: each rule script RTN000 to RTN307 beyond RTN010, RTN103 and RTN306; `routine_step_target_counts` is read
  after 戦女神ZERO and Verita.
- BTANINIT's still kinds 1 and 2, and the battle units' flags beyond bits 6 and 7, positions and sizes.

**Encounters and enemies.**

- `G[0xd0a2ab]` (ELINIT +200) is never written, and FIELD reads it at a cell that looks like a slip for the
  encounter's id (`FIELD@0x422d`); with every cell 0 the test never applies. `G[0x4dd80]` and `G[0x4dd81]` are only
  cleared.
- What `formation_mark`'s two tags look like, what a bound enemy's bond does in battle, and a battle result of 2
  (`BTLMAIN@0x148b`), which no script tests.

**Items, stones, improvement and alchemy.**

- ITINIT `G[0x33c017]`, `G[0x34f897]`, `G[0x35e2f7]`, `G[0x3b1317]`, `G[0x3b6137]` and `G[0x6e5817]`; a negative
  level in `item_inflicted_conditions`; the stat vector's columns beyond 0 to 4, 11, 12 and 14.
- ALINIT `G[0xc09301]` and `alchemy_facility_masks`' value 2; what `item_costume_overrides` targets; CHECKSEAL past
  its first reads; whether equipping takes an item out of `item_counts`.

**The dungeon.**

- `G[0x4dd62]` (a map-entry state of 0 to 2), the six saved `[1000][5]` arrays from `G[0xb0e52]`, the third list of
  the map in play `G[0xc2b3b9]`, a puzzle block's flag `G[0xc9e845]`, and the facing values.
- MPINIT's unwritten `G[0xc9f015]`, `G[0xca615e]` and `G[0xc9f9d9]` … `G[0xca1918]`, the unread `G[0xca7cb6]` and
  `G[0xca809e]`, and the backdrop's frame count `G[0xca6546]`.
- OBINIT's kinds 201 to 341 beyond their actions, and `$1$OBINIT2`'s kind 999.

**The town.**

- `G[0x4d2cc]`, tested as a mask by the buildings' variants and two spots and written by nothing; the variants' six
  condition arrays and the spots' progress bounds, read and never written, so every building shows variant 0.
- The hotspots (spots 100 to 199), which nothing gives an action; spot characters 997 to 999; flag 10000
  (`G[0x35f6]`), which 42 spots need and no script sets.
- `G[0xbf656e]`, bit 1 of `G[0x4d52b]`, `G[0x4d591]`, `G[0xbf016b]` and `G[0xbf016c]`; the NUINIT strings and
  QUINIT's description column 3, which nothing reads; INFOTU sorting by `quest_sort_keys` (`INFOTU@0x929`).

**Tooling.** `init_layout.py` gives one capacity to all of a leader's arrays, so AFINIT (a 30-wide matrix among lists
of 16 to 100) has no leader, and ILINIT's level names come out one record low; it has no mode for a table loaded on
demand by a selector (COINIT, FLINIT, DUINIT, ORINIT, ENINIT, ESINIT, BTANINIT) or written as boards of rows (FMINIT,
EEINIT), whose sections are read from their listings; and MPINIT's floors are a second id space on the blocks' grid,
declared with `arrays=`.
