# 魔導巧殻 data tables (`*INIT`)

Canonical home for how 魔導巧殻 (koukaku, SYS4443, AGE 4.43B) stores its static game data: which `*INIT` scripts
exist, how they are layered, where each table lives in the global bank, and what is known of its fields. Names for
the fields that are understood live in [the koukaku globals registry](../vm-map/globals-koukaku.toml) (generated →
[its global reference](global-reference-koukaku.md)). The game's engine is 創刻のアテリアル's 4.38D, one build
later, and its boot has Verita's and Arterial's shape (hook chains, one append); the other games' tables are in
[Arterial's](arterial-data-tables.md), [Verita's](verita-data-tables.md), [戦女神ZERO's](bg0-data-tables.md) and
[Kamidori's](kamidori-data-tables.md). A reading here rests on 魔導巧殻's own data or consumers, never on a likeness
alone.

**How far this goes (2026-11-26).** Every table's *layout* is here: its arrays, their strides and what values they
hold, for the 37 boot tables and the 3 that run on demand. What an array *means* is given where the data or a
consumer shows it, each reading with the script and offset it rests on. A row that says nothing in its last column
is not read yet; "What is not decoded" at the end lists the gaps.

## How the data gets into memory

Like the other games, 魔導巧殻 has no data files. Every table is a script of constant writes into the flat global
bank: `mov` for integers, `set-string` for text, `copy-inline-int-array` for a few bulk arrays. Runtime code reads
the bank with `lookup-array` (`base + id`) and `lookup-array-2d` (`base + row * stride + column`).

**One array per field.** A table is a set of parallel arrays. A record's field is the cell
`base + cells_per_id * id + column`: one cell per id for a scalar, several for a short list. Each array is allocated
for the table's *capacity* whether or not an id is used, and the arrays of a table lie back to back. Ids are sparse
and mostly start at 1.

**Boot order.** `INIT2.BIN` calls 37 initializers in this order (`INIT2@0x0`..`0x6c`): SCINIT, CHINIT, CLINIT,
TRINIT, ITINIT, RFINIT, FOINIT, SKINIT, ARINIT, COINIT, PTINIT, RTINIT, CNINIT, RGINIT, ROINIT, CRINIT, USINIT,
LAINIT, OBINIT, DPINIT, RUINIT, QUINIT, HAINIT, BRINIT, ALINIT, CVINIT, CGINIT, SPINIT, MUINIT, NAINIT, TUINIT,
TIINIT, CIINIT, VIINIT, ANINIT, ANINIT2, SAINIT. Op `0x143` then runs the mounted append catalog's
`$1$AUTORUN.BIN` (`INIT2@0x33a`), which calls its own `$1$<TABLE>.BIN` for 36 of them, every table but SAINIT, in
the same order (`$1$AUTORUN@0x0`..`0x69`). The install has one append, so a table's runtime image is the **base,
then `$1$`**, later writes winning. Of the 36 fragments, 13 are empty stubs (`exit` alone: RFINIT, FOINIT, ARINIT,
COINIT, PTINIT, LAINIT, RUINIT, HAINIT, SPINIT, MUINIT, NAINIT, TUINIT, TIINIT). The other 23 write only cells the
base table left unwritten, 3,089 in all: they add records, or fields to records the base made, and none rewrites a
value the base wrote (the boot image, compared cell by cell). No fragment is behind a test: unlike 戦女神ZERO's,
`$1$AUTORUN` has no game mode.

**On demand.** Three tables are not run by INIT2, so they are not in the boot image: DUINIT, SHINIT and MPINIT. INIT2
puts each in slot 0 of a hook chain and `$1$AUTORUN` its `$1$` fragment in slot 1 ("Hook chains" below), and the
screen that uses the table walks the chain.

## Which files are authored data

`games/koukaku/` holds the archives `DATA1`–`6.ALF` and `APPEND01.ALF` (with its `.AAI` index), and 79 loose game files in
its root: `SYS4INI.BIN`, `SYS4RK.BIN`, 71 scripts (three of them `$1$` records) and 6 pictures (`SO011`, `SO012`,
`SO014`, `SO028`, `SO040`, `SO041`). Every loose script and picture has a record of the same name in an archive
(`DATA1`, or `APPEND01` for the `$1$` ones). 76 differ from it; `SORTCORP.BIN` is identical. Native AGE reads a loose
file before the archive, and the data tooling reads the install the same way (`tools/paths.py`'s `scripts()`), so
every count below includes them. Eleven tables have a loose copy, CLINIT, DUINIT, FOINIT, ITINIT, NAINIT, OBINIT,
QUINIT, RTINIT, SKINIT, TRINIT and VIINIT, and two fragments, `$1$CLINIT` and `$1$SKINIT`; so do the drivers INIT2,
`$1$AUTORUN` and SCJUMP. The install's title shows `Version 1.03.0010`; where the loose files come from is not
established here.

## Tooling

All commands run from `age-reimpl/` with `py -3.11 -X utf8` and `--game koukaku`
([the tools reference](tools-reference.md)):

1. Extract the archives once with the local `bin/BinExtractALF.exe` into `../extracted-koukaku/`: `DATA1/` to
   `DATA6/`, and the append's records flat in `APPEND01/`.
2. `parse_sys4ini.py` → the asset index and call-script names.
3. `extract_phase2.py` → `build/games/koukaku/disasm/*.asm` and the text corpora.
4. `init_image.py` → `build/games/koukaku/init-image.json`, the **executed** boot image: the port's VM runs the boot
   order above and records what each of the 73 scripts wrote (37 tables and 36 fragments).
5. `init_layout.py` → each table's layout, inferred from the write order and the image, with 魔導巧殻's leaders
   (`KOUKAKU` in the tool). `init_layout.py <TABLE>` prints a table's arrays column by column, `--markdown` the layout
   tables below, and `--write` saves them as `build/games/koukaku/data/<TABLE>.layout.json`.
6. `global_map.py` → `build/games/koukaku/global-var-map.{json,md}`, the access shape of every global, and
   `story_flags.py` → `build/games/koukaku/story-flags-candidates.json`, the globals the scripts branch on.

`init_layout.py` is given the array each record writes first (the *leader*) and the table's capacity, takes a
record's id from the leader cell, and finds every other field as the `(stride, base)` on which the most records
agree. Where that cannot work, the leader declares the array (`arrays=` in the tool): OBINIT writes two arrays
through local pointers, so their cells follow no record, and has one whose eight records fit a wrong stride as well
as the true one. Tables with no single leader are shown by the arrays the scripts index (`lookup-array` operands), which is a
weaker view: an array no script indexes with a variable is not seen, its cells are counted in the array before it,
and the base shown is the indexed array's, not always the field's. The tool does not lay out the on-demand tables,
which are not in the boot image; their sections are read from their listings.

**The grid, and `†`.** A table's arrays lie back to back, each a whole number of capacities long, so their bases
agree modulo the capacity. The tool marks an array whose base does not: `†` in the layouts below. Such an array
either has another table's cells between it and the rest, or is not indexed by the record's id at all, and then
the base and the ids in its row are wrong. Each `†` row says which.

**The saved prefix.** A save keeps the integer globals below `0x211c08` (2,169,864), the string globals below
`0x1b1` (433) and the float globals below `0x158b1` (88,241), the profile's `persistence.bankDimensions`
([the format notes](sys4-format-notes.md)). Every boot table's arrays lie above the prefix, its integer arrays and its
string arrays alike (the lowest, CHINIT's `character_name_forms`, starts at `GS[0x39e]`), so a save carries none of
them, and INIT2 writes them on every boot and every load ("The boot order" below): a change to a table reaches a saved
game too. A table's section says where the game copies a
field into a cell below the prefix, which a save then keeps.

## Table inventory

"boot" means INIT2 runs the table and then `$1$AUTORUN` runs its `$1$` layer ("Boot" below; 13 of the layers are
empty). "On demand" tables sit in slot 0 of a hook chain, their `$1$` layers in slot 1.

| table | holds | runs |
|---|---|---|
| SCINIT | scenes: the script each scene number plays, the SCJUMP context that offers it, whether it plays over the map | boot |
| CHINIT | characters (monsters from 471): base character, sex, kind, size, element, starting title and costume, master, levels, base stats and growth, loyalty, costs, pictures, battle voices, names and descriptions | boot |
| CLINIT | titles (称号): line, unit class, conditions, movement kind, size, range, stat modifiers, attacks, skills, formations and their unlocks, picture and voice overrides | boot |
| TRINIT | troop types: kind, attribute, movement and size, parameters per soldier, attacks, passive skill, costs, upgrade and hiring conditions | boot |
| ITINIT | items: key items, materials and equipment, their prices, bonuses and skills | boot |
| RFINIT | the battle's lists: troop kind, element, attribute and passive effect names, which kinds a commander's kind leads, element damage by attribute, skill type waits | boot |
| FOINIT | formations: soldier slots, parameter percents, squads needed, whether the unit moves while attacking | boot |
| SKINIT | active skills (cost, element, effect, power, duration, animation, bonuses) and passive skills (effects and levels), two id spaces | boot |
| ARINIT | attacks: element, timing, parameter bonus, animation | boot |
| COINIT | the 魔導巧殻's costumes: the attacks and formations each gives its wearer's unit, and the parts, level and stats it starts with | boot |
| PTINIT | costumes and parts: kind, who may use them, their bonuses and skills | boot |
| RTINIT | routines by routine and step, in four blocks | boot |
| CNINIT | nations: name, colour, starting funds, provisions and items, capital, leader, pictures, the flags that make a nation playable | boot |
| RGINIT | regions: name, kind, field map, starting owner and population, production sets, world-map position, pictures, the flags and conditions that open one, the battle fought there | boot |
| ROINIT | routes between two regions: kind, path, pictures, the flags that open and close one | boot |
| CRINIT | production sets: the items each set yields and their rates | boot |
| USINIT | starting units: the corps a nation starts with, and the corps scenes and quest rewards add by id | boot |
| LAINIT | land types: name, passability by movement type, speed percent | boot |
| OBINIT | map objects: kind, footprint, passability, speed and stats by level, costs, build weeks, materials, pictures, region effects, build conditions | boot |
| DPINIT | object kinds (name, command, icon, book tab, class); the nation matrices, CALCFORT's thresholds and tables by nation | boot |
| RUINIT | ruses (策略): category, costs, target and relation mask, friendship change, animation, texts | boot |
| QUINIT | quests: kind, battle, conditions, delivery, rewards, texts | boot |
| HAINIT | the brothel's girls: where and when offered, price, visit thresholds, scenes, picture | boot |
| BRINIT | breeding, sacrifice and siege-weapon development: parents, result, conditions, experience, messages | boot |
| ALINIT | alchemy recipes: the product, ingredients, price, facility, level, technology and flag conditions | boot |
| CVINIT | voice settings: name colour, character, sample voice, pictures, name, epithet and voice actor; each character's voice setting | boot |
| CGINIT | the CG gallery: each CG's picture and stand-in, its page, group and place | boot |
| SPINIT | the scene replays: the script of each page's twelve slots | boot |
| MUINIT | the music room: each track's title, BGM number and unlocking resource | boot |
| NAINIT | the strategy screens' help lines | boot |
| TUINIT | tutorials: the screen, order, text slot and conditions of each | boot |
| TIINIT | the tips (解説): name, page, category, picture, order and unlocking scenes | boot |
| CIINIT | profiles of the 人物 book: name, heading, pictures, 16 lines and their conditions | boot |
| VIINIT | the glossary: title, page, order and each line's unlocking scenes | boot |
| ANINIT | battle animations: file, model or texture, size, scale and offsets | boot |
| ANINIT2 | animation sequences, the rows a skill plays per facing | boot |
| SAINIT | skill areas: the field picture and an ellipse's radius or a polygon | boot (no `$1$` layer) |
| DUINIT | battle setups by battle id: the place's name, the end conditions and the battle's starting cells | on demand: `FIELD`, through `duinit_chain_script_ids` |
| SHINIT | the shop's items and the employment office's characters on offer, by facility level, region type and flags | on demand: `SHOP`, `EMPLOY`, through `shinit_chain_script_ids` |
| MPINIT | one battlefield map: land, preset objects and levels, size, bounds, picture, deploy entries | on demand: `FIELD`, `CHECKREG`, `DELCN`, `OCCUPY`, `PALACE`, `REIGN`, through `mpinit_chain_script_ids`, by `battlefield_map_id` |

## Boot

### The boot order (SYSTEM4)

SYSTEM4 runs these steps on each root run:

1. It sets up the nine text layouts, the wait indicator and the text's paddings (`SYSTEM4@0x0`..`0x1a4`), and puts
   layouts 1 and 2 at the origins of the message window's style `scene_window_layout` `G[0xc5e]`, 0 to 2 (`SYSTEM4@0x1ab`, `@0x74b`..`0x7ac`).
2. It loads `G[0x5]` from the shared profile (`SYSTEM4@0x1ae`). If the cell is set, it runs LOADCONFIG; if not, it
   runs INITCONFIG, then sets `G[0x5]` to 1 and stores it (`SYSTEM4@0x1b1`..`0x1c6`). Then it runs CHECKCONFIG
   (`SYSTEM4@0x1c9`), which falls back to ＭＳ 明朝 and ＭＳ ゴシック when a saved font is not installed
   (`find-font-family-index`, `CHECKCONFIG@0x0`..`0x33`) and sets the text's effect colour for the speaker
   (`CHECKCONFIG@0x43`..`0x10e`, INITCONFIG below).
3. It copies the inline array 0, 4, 5, 9 into `sound_channels` `G[0x211c20]` (`SYSTEM4@0x1cc`): the channels the
   scenes play and release sound on (`sfx-release (global-int 0x211c20)`).
4. It maps the keys and the mouse, and the six joystick buttons from the shared profile, with defaults 0, 3, 2, 1, 6,
   7 stored on the first run (`SYSTEM4@0x1d1`..`0x276`).
5. It preloads script slots 0x1d to 0x1f with CALCABI, MVSEEK and SETROUTE (`SYSTEM4@0x27f`..`0x289`), and sets the
   pictures SO002 and SO001 as textures 0xf and 0x11.
6. It runs INIT2 (`SYSTEM4@0x2aa`), then sets up the message font (`@0x2ad`..`0x2e9`).
7. It reaches op `0xae continue-save-load-stack-restore` (`SYSTEM4@0x2ee`). On a numbered load, the engine zeroes
   every bank, copies the saved prefix back, and runs each restored frame from its entry up to `0xae` ([the engine
   notes](engine-re.md), "A load zeroes each bank in full"). So INIT2 runs on every boot and on every load, and on a
   load it runs with the saved cells already in place.
8. On the first root run only (`get-initial-root-run`, `SYSTEM4@0x2ef`), it runs LOGO (`@0x2f9`). Then, on every root
   run, it runs INIT (`@0x2fc`), which sets four cells (`nation_player` `G[0x639f]` = 1, `G[0x19a329]` = 2, `turn_week` `G[0x170a4]` = 1,
   `turn_nation` `G[0x170a5]` = 0, `INIT@0x0`..`0xf`), and TITLE (`@0x2ff`).
9. It loops on the mode (`SYSTEM4@0x302` onwards; "Scenes" below).

### INITCONFIG (the options)

INITCONFIG sets each option and stores it in the shared profile (`INITCONFIG@0x0`..`0xc5`). LOADCONFIG reads the
same cells back (`LOADCONFIG@0x0`..`0x55`), and the option pages CONFIG1 and CONFIG2 change them. All these cells lie
below the saved prefix too. INITCONFIG2 sets the engine's own settings (the window's alpha, the glyph delay, the auto
mode's times and pitches, the volumes; `INITCONFIG2@0x0`..`0x40`).

| cell | default | read by | holds |
|---|---|---|---|
| `message_font_name` `GS[0x1a7]` | メイリオ | `SYSTEM4@0x2c9`, CONFIG1 | The message font: SYSTEM4 sets it with `set-font`. CHECKCONFIG puts ＭＳ 明朝 in its place when it is not installed. |
| `ui_font_name` `GS[0x1a8]` | ＭＳ ゴシック | ALCHEMY, ALLMAP, ARMAMENT, BREED, CONQUEST, … | The screens' font; CHECKCONFIG puts ＭＳ ゴシック back when it is not installed. |
| `confirm_save_load` `G[0xc56]` | 1 | `SAVE@0x6fc`, `@0x7c1` | While it is above 0, SAVE asks ロードしますか？ or セーブしますか？ before it loads or saves. |
| `confirm_commands` `G[0x63b0]` | 1 | `MENU@0x592`, CONQUEST, ORGANIZE | While it is set, a command asks first: タイトル画面に戻りますか？ (MENU), 討伐戦を開始しますか？ and 戦闘を放棄して撤退しますか？ (CONQUEST). |
| `G[0x63b2]` | 3 | BUNKI, SBUNKI | Two switches. While bit 1 (2) is set, a choice menu puts the cursor on a row (`set-cursor-virtual`, `BUNKI@0x19c`..`0x1c0`). Bit 0 is not read here. |
| `autosave_off` `G[0x63b3]` | 0 | `ALLMAP@0x2ac8` | ALLMAP autosaves (its comment オートセーブ実行, `@0x2add`) only while it is 0. |
| `G[0x63b4]` | 1 | ALCHEMY, BROTHEL, CONQUEST, EMPLOY, QUEST, RECEIPT, SELCAPITAL, SELLORD, SELOBJ, SHOP | While it is set, a list turns its pages with the mouse's horizontal wheel (`consume-mouse-hwheel-delta`, `SHOP@0x48e`). Not decoded further. |
| `G[0x63b5]` | 0 | ALLMAP, EXECRUSE, EXECSKILL, FIELD, REIGN | Switches by bit; while bit 0 is clear, FIELD draws more of the field (`FIELD@0xbb83`, `@0xc098`). Not decoded. |
| `G[0xc4b5]` | 0 | DRAWENTRY, DRAWUNIT, EXECSKILL, FIELD, SETROUTE | Switches by bit; bit 4 (0x10) sets DRAWUNIT's units to 75 by 75 (`DRAWUNIT@0x18e`). Not decoded. |
| `message_window_colour` `G[0x63b7]` | 0 | every scene, BUNKI, HISTORY, … | The message window's colour: a scene draws the window with it (`gfx-draw-color 0xd2f0 0 alpha G[0x63b7]`, `SC0000@0xf6e9`). |
| `voice_continues_past_page` `G[0x63b8]` | 1 | every scene, HMODE | While it is 0, a page's end resets the voice (`reset-message-voice-state`, `SC0000@0x1030b`, `@0x104c3`): the voice stops when the page turns. |
| `speaker_text_colour_mode` `G[0x63b9]` | 1 | every scene, CHECKCONFIG, HISTORY | 1: the text's effect colour is the speaker's (CVINIT's colours, `CHECKCONFIG@0x52`..`0xd7`); −1: white text with no effect (`@0xdf`..`0xfe`); any other value keeps the default `0x3a3a3a`. |
| `voice_mute_switches` `G[0x33e7]` … +999 | 0 | every scene, HISTORY, the screens | 1000 cells, cleared and stored at once (`copy-to-global`, `INITCONFIG@0x60`..`0x89`). A page reads `G[0x33e7 + G[0x211c90]]` (`SC0000@0x1309`): characters, below. |
| `voice_name_colors` `G[0x63ba]` … +999 | CVINIT's | CHECKCONFIG, every scene | 1000 colours, one per CVINIT setting: CVINIT's `G[0x22a757 + i]`, or −1 where that is 0 (`INITCONFIG@0x8c`..`0xc5`); INIT2 fills a −1 again when CVINIT has a colour (below). Characters, below. |

### INIT2 (the boot driver)

INIT2 calls the 37 base tables in the order above (`INIT2@0x0`..`0x6c`), fills slot 0 of the hook chains
(`INIT2@0x6f`..`0x150`, below), writes its constants, and then runs op `0x143 run-mounted-append-autoruns`
(`INIT2@0x33a`). Its last steps follow. The inline arrays' values come from `games/koukaku/INIT2.BIN`, where
`copy-inline-int-array` reads a block with a count before it at the code offset.

| offset | write | what it is |
|---|---|---|
| `INIT2@0x155`..`0x18c` | `scene_graphic_element_numbers` `G[0x211c34]` … `G[0x211c3f]` = 51000, 51010, 51110, 51120, …, 51200, 52000 | `scene_graphic_element_numbers`, the graphic element numbers of a scene's layers: every scene draws and erases through them (`gfx-blit-color (global-int 0x211c34)`, `SC0000@0x108c`; `gfx-elem-erase`, `@0x155b`). |
| `INIT2@0x191`..`0x1d2` | `GF[0x158c1]` … `GF[0x158cc]`, `G[0x213f01]` … +3 = 255, `G[0x213f05]` … +3 = `0xffffff` | A quad's vertices: four z of 0, the u (0, 1, 0, 1) and v (0, 0, 1, 1), and four alphas and colours. ALLMAP draws its map through them (`create-polygon 0x15f90 … (global-float 0x158c1) (global-int 0x213f01) (global-int 0x213f05) (global-float 0x158c5) (global-float 0x158c9) 4`, `ALLMAP@0x177`), with its own x and y in `GF[0x158b9]`, `GF[0x158bd]`. |
| `INIT2@0x1d7`, `0x1dc` | `neighbour_dx` `G[0x22c8c7]` … +7 = −1, 0, 1, 0, −1, 1, 1, −1 and `neighbour_dy` `G[0x22c8cf]` … +7 = 0, −1, 0, 1, −1, −1, 1, 1 | `neighbour_dx` and `neighbour_dy`, the eight steps to a cell's neighbours: left, up, right, down, then the diagonals. SETROUTE adds them to a cell (`SETROUTE@0x67a`, `@0x68d`); CRAFTACT, RTN_M011 and SETENTRY read them too. |
| `INIT2@0x1e1`..`0x1eb` | `G[0x22cacc]` … +32, three rows of 11: 1000, 1000, 100 × 9 / 9, 9990, 100 × 8, 0 / 0, 999, 100 × 8, 0 | Caps, a `[3][11]` block. CALCABI caps the values it computes, `unit_calc_parameters` `G[0x22caf0][row][i]`, at it (`CALCABI@0x858`..`0x892` for row 0, `@0x992`..`0x9cc` for row 2), and caps `troop_arg_soldiers` `G[0x24e812]` at row 1's first cell, 9 (`CALCABI@0xa9d`, `SELTR@0x13df`). Not decoded further. |
| `INIT2@0x1f0` | `G[0x8da33b]` … +4 = −1, 4, 6, 5, 7 | CALCABI compares a character's `character_elements` `G[0x22ec40]` (CHINIT) with `G[0x8da33b + G[0x170a7]]` (`CALCABI@0x375`, `@0x3d7`). Not decoded. |
| `INIT2@0x1f5`..`0x1ff` | `G[0xa10ed4]` … +3 = 1, 2, 3, 5; `G[0xa10ed8]` … +4 = −30, −10, 0, 10, 30; `G[0xa10edd]` … +6 = 10, 8, 6, 4, 3, 2, 1 | The ruses' numbers. RUSEJUDGE adds `G[0xa10ed8 + 2 + d]` to a chance before its roll of 100 (`RUSEJUDGE@0x556`..`0x569`), and RUSEACT subtracts `G[0xa10edd + n]` (`RUSEACT@0x118`); no script reads `G[0xa10ed4]` by its base. The map and its ruses, below. |
| `INIT2@0x204`..`0x2f4` | `GS[0xa1c9]` … `GS[0xa1fe]` | The strategy screens' words: the relations 自国, 同盟, 中立, 敵対, 隷属, 本国 (`nation_relation_names`, `GS[0xa1c8 + relation]`, 1 to 6); the movement kinds 地上, 飛行, 潜水 (`movement_type_names`, `GS[0xa1cf + type]`, 1 to 3); the sizes ＳＳ … ＬＬ (`GS[0xa1d4]`..`0xa1d8`); the ranks Ｅ, Ｄ, Ｃ, Ｂ, Ａ, Ｓ, Ｍ (`GS[0xa1d9]`..`0xa1df`); the object classes 城門, 城壁, 結界, 橋, 拠点, 施設, 防柵, 自然, 他, 罠 (`map_object_class_names`, `GS[0xa1e0]`..`0xa1e9`); and the four orders 【手動】指示を出す, 【自動】進軍, 【自動】制圧, 【自動】防衛 (`GS[0xa1eb]`..`0xa1ee`) with their texts of two to four lines (`GS[0xa1ef]`..`0xa1fe`). Their readers are in the sections below. |
| `INIT2@0x2f9`..`0x335` | `G[0x20e32f]` … `G[0x20e33b]` = file numbers 0xd9 … 0xe5 (RIS0061.OGG, RIS0062.OGG, CS002AAB.AGF, CA002AAB.AGF, RIS0063.OGG … RIS0067.OGG, C0904.WAV, CS013AAA.AGF, ELM0001.OGG, ELM0002.OGG); `$1$AUTORUN@0xc1` adds `G[0x20e33c]` = 0xe6 (CS013AAE.AGF) | Thirteen file numbers, the append's one more. No script reads the array by its base. Not decoded; it lies below the saved prefix. |
| `INIT2@0x33b`..`0x39f` | for each of the 1000 colours: if `G[0x63ba + i]` < 0 and CVINIT's `G[0x22a757 + i]` > 0, copy it and store it | A colour the shared profile left unset (INITCONFIG's −1) takes CVINIT's once a table gives it one, the append's settings included. |
| `INIT2@0x3a2`..`0x3eb` | if `system_mode` `G[0x0]` == 1 and `scene_over_map` `G[0x38a5]` == 1: READMAP when `system_base_mode` `G[0x2]` == 2; or, when `G[0x2]` == 4, `region_arg_id` `G[0x8e0267]` = `G[0x19a329]` and READFIELD, READREIGN, READCH | On a load inside a scene played over the map ("Scenes", the scene pass), INIT2 rebuilds what lies under it from the saved state: the world map, or a region's battlefield (the strategic map and battlefields, below). On a cold boot the cells are 0. |

### The append's driver (`$1$AUTORUN`)

Op `0x143` runs the AUTORUN of each mounted append. 魔導巧殻 has one append, selector 1, so it runs `$1$AUTORUN`
(from the install's loose copy). That script:

1. Calls the 36 fragments, every boot table's but SAINIT's, in INIT2's order (`$1$AUTORUN@0x0`..`0x69`), with no
   test before any of them.
2. Puts its own scripts and pictures in slot 1 of 17 hook-chain cells (`$1$AUTORUN@0x6c`..`0xbc`, below).
3. Adds the file number 0xe6 to INIT2's list (`$1$AUTORUN@0xc1`), and sets bit 1 of `installed_appends_mask`
   `G[0xa6b7f8]` (`bit-set (global-int 0xa6b7f8) 0x1`, `$1$AUTORUN@0xc6`).

`installed_appends_mask` records which appends are mounted. GAMESTART loads the shared profile's copy over it and
compares the two bit by bit for the 32 appends (`GAMESTART@0x21`..`0x84`). An autosave and SAVE copy it into the
slot's `G[0xb82 + slot]` and store that in the shared profile (`ALLMAP@0x2e1a`..`0x2e2d`, `SAVE@0xc06`..`0xc19`),
and SAVE tests a slot's copy against it (`SAVE@0x1853`). The cell lies above the saved prefix, so `$1$AUTORUN` sets
it again on every boot and load.

### Hook chains

INIT2 and the append fill arrays of 32 slots, one slot per layer: slot 0 is the base game's entry and slot n
append n's. A walker goes through the slots and runs (or, for a picture chain, loads) every nonzero one, so the
order is **base, `$1$`**. A chain of one script is `int[32]`, and the walkers index it with `G[0x211c92]` or a
local. Four chains hold several scripts a slot, as `[32][k]` arrays the walkers read with `lookup-array-2d` (slot,
stride k, which script): the slot's scripts lie side by side, so `$1$`'s RUSEJUDGE is at `G[0xa6b959 + 3]`.

| chain | slot 0 (`INIT2`) | slot 1 (`$1$AUTORUN`) | walked by |
|---|---|---|---|
| `title_picture_chain` `G[0xa6b7f9]` `int[32]` | none | `$1$SO026.AGF` | `TITLE@0xb2a`, `@0xd89`: each set slot's picture as texture 0x3e + slot, drawn on the title |
| `board_picture_chain` `G[0xa6b819]` `[32][3]` | SO013, SO019, SO022 (`.AGF`) | `$1$SO013`, `$1$SO019` | `READBOARD@0x99`..`0x285`: picture row `G[0x211c2a]` of each slot |
| `scjump_chain_script_ids` `G[0xa6b879]` `int[32]` | SCJUMP | `$1$SCJUMP` | ALLMAP (`@0x74`), FIELD, PALACE, REIGN, SYSTEM4, with `G[0x211c92]`: the scene dispatch ("Scenes" below) |
| `mpinit_chain_script_ids` `G[0xa6b899]` `int[32]` | MPINIT | `$1$MPINIT` | CHECKREG (`@0xc6`), DELCN, FIELD (`@0x828`), OCCUPY, PALACE, REIGN |
| `skillact_chain_script_ids` `G[0xa6b8b9]` `int[32]` | SKILLACT | none | `EXECSKILL@0xd54` |
| `craft_chain_script_ids` `G[0xa6b8d9]` `[32][4]` | CRAFTACT, CRAFTATK, CRAFTBRK, CRAFTSK | none | `EXECSKILL@0x618`, `FIELD@0x5e5c`, `@0x7e43` |
| `ruse_chain_script_ids` `G[0xa6b959]` `[32][3]` | RUSEJUDGE, RUSEACT, RUSECHECK | `$1$RUSEJUDGE`, `$1$RUSEACT`, `$1$RUSECHECK` | `EXECRUSE@0x732`, `RUSE@0xe54` |
| `duinit_chain_script_ids` `G[0xa6b9b9]` `int[32]` | DUINIT | `$1$DUINIT` | `FIELD@0x35f` |
| `shinit_chain_script_ids` `G[0xa6b9d9]` `int[32]` | SHINIT | `$1$SHINIT` | `EMPLOY@0xa60`, `SHOP@0xf66` |
| `start_chain_script_ids` `G[0xa6b9f9]` `[32][5]` | START, CLOSE, READY, READYREG, READYQUEST | `$1$START` … `$1$READYQUEST` | `INITGAME@0x257`, ALLMAP (`@0x70ef`), FIELD (`@0xf82d`), QUEST, REIGN, SETMAP, with `G[0x211c92]` |
| `tumes_chain_script_ids` `G[0xa6ba99]` `int[32]` | TUMES | `$1$TUMES` | `TUTORIAL@0x598` |
| `cg_page_sheets` `G[0x22a5db]` | SO043A … SO043I (9 sheets) | `$1$SO043K` at `G[0x22a5e5]` | `CGMODE@0x668`, `@0x12db`: the gallery's thumbnail sheets, one per page; `G[0x22a5e4]` (a J) is not written |
| `replay_page_sheets` `G[0x22a6df]` | SO044A … SO044F (6 sheets) | none | `HMODE@0x5a7`, `@0xfd3`: the replay's thumbnail sheets |

INIT2 also puts the facilities' scripts in `G[0x22c3e3]` … `G[0x22c3ec]` (PALACE, ALCHEMY, ALCHEMY, QUEST, BROTHEL,
SHOP, EMPLOY, DOLL, BREED, BREED; `INIT2@0xc9`..`0xf6`), a list by facility rather than a chain (items and facilities,
below).

### What is saved

- **Saved:** the options above (the shared profile keeps them too), `G[0x5]`, the joystick buttons, and INIT2's
  thirteen file numbers `G[0x20e32f]` …, which INIT2 writes again on every load.
- **Rebuilt at boot:** the hook chains, `installed_appends_mask`, `sound_channels`, `scene_graphic_element_numbers`,
  the quad, the neighbour steps, the caps, the ruses' numbers and the strategy screens' words `GS[0xa1c9]` …, all
  above the prefix.

## Scenes

### SCINIT (scenes)

Leader `G[0x214263]`, capacity 10000. 791 scene ids are set, 0 to 6986: 778 by the base table and 13 by `$1$SCINIT`
(4000 to 4015). Scene 0 is the prologue. One scene script holds ten scene numbers and branches on the number:
`SC0000` holds scenes 0 to 9, `SC0200` scenes 200 to 209. The base table names 112 scripts and `$1$SCINIT` two,
`$1$SC4000` and `$1$SC4010`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x214263]` | 1 |  | 791 | 86..16777453, 114 distinct | the script that plays scene `id`, a packed script id (86 is `SC0000.BIN`; an append's scenes carry its selector in the top byte). `SYSTEM4@0x325` reads it for `current_scene_id` into `current_scene_script_id` `G[0xc5a]`, and plays `SC0000` where the cell is 0 (`@0x331`..`0x33f`) |
| `G[0x216973]` | 1 |  | 791 | -1..13, 13 distinct | the context the scene is offered in, 1 to 13, the number of the `SCJUMP` branch whose rules name it (below), or -1 for a scene the chain never offers (29: the 娼婦の館 scenes, which `BROTHEL` plays itself, and others). Of 563 rules, 543 offer a scene in the context its cell gives; the rest offer it in another (a 訪問 scene at the turn's end). No script reads the array |
| `G[0x219083]` | 1 |  | 0 |  | not written. `SCJUMP`'s visit rules compare it, the scene already in a visit slot against the new one (`SCJUMP@0x470a`..`0x4716`): with every cell 0, a slot keeps the first scene a rule puts in it |
| `G[0x21b793]` | 1 |  | 239 | 1..1, 1 distinct 1×239 | 1 on a scene played on the map rather than in the ADV screen. `ALLMAP@0x6dd4`, `FIELD`, `PALACE` and `REIGN` copy it into `scene_over_map` `G[0x38a5]` when they hand a scene to `SYSTEM4`. The 239 are every scene whose script comment calls it a MAPイベント, マップ会話 or a battle's 出撃, 撤退 or 勝利 event (`G[0x21b793 + scene]` = 1 for all of them, and for none of the ADVイベント) |

The next 30000 cells, `G[0x21dea3]` to `G[0x2253d2]` where CGINIT's first array starts, are not written or read.

The scene scripts' comments name each scene. Every scene's branch starts by setting its played cell and then has a
comment `▼Gnnnn　<kind>　<title>`: `SC0000@0xcf2`..`0xcf7` sets `G[0xc71]` and reads `▼G0000　週切替開始ADVイベント
　プロローグ`. The kinds the comments give line up with the context cell:

| context | comment kind | scenes |
|---|---|---|
| 1 | 週切替開始ADVイベント, the turn's start | 60 |
| 2 | 訪問ADVイベント and 訪問MAPイベント, a visit | 340 |
| 3 | 全体マップADVイベント, on the world map | 109 |
| 4 | 全体マップMAPイベント | 43 |
| 5 | 内政MAPイベント, internal affairs | 2 |
| 6 | 内政ADVイベント | 25 |
| 7 | 戦闘開始ADVイベント and 戦闘開始MAPイベント, a battle begins | 24 |
| 8 | 出撃イベント, a sortie on the battlefield | 29 |
| 9 | 撤退イベント, a retreat | 86 |
| 10 | 勝利イベント, a victory | 25 |
| 11 | 週切替開始MAPイベント | 7 |
| 13 | 週切替終了ADVイベント, the turn's end | 12 |
| -1 | 娼婦の館, ※その他イベント, the 汎用Ｈ scenes | 29 |

The scene scripts have 1150 such branches. 359 of them name a number SCINIT does not list, so `SYSTEM4` never plays
them (it would play `SC0000` for the number); 354 of those have an empty title (`▼G0024　`, `SC0020`). No script
sets one of those numbers.

### The scene pass (SYSTEM4)

After the boot (the boot section above) `SYSTEM4` loops on `system_mode` `G[0x0]` (`SYSTEM4@0x302`..`0x746`): 1 a
scene, 2 `ALLMAP` (the world map, `@0x6fb`), 3 `FIELD` (a battle, `@0x70f`), 4 `REIGN` (`@0x723`), 5 `DEAL`
(`@0x737`). Any other value shows 不正な実行モード and asks to exit (`@0x73d`..`0x745`).

A scene pass (`SYSTEM4@0x309`..`0x6ea`):

1. Unless `scene_run_mode` `G[0xa6bab9]` is set, it looks the scene up: `current_scene_script_id` `G[0xc5a]` =
   `scene_script_ids[current_scene_id]` (`@0x317`..`0x32c`), `SC0000` where that is 0.
2. It picks the message window's layout, `scene_window_layout` `G[0xc5e]`: 2 for a scene over the map
   (`scene_over_map`), 1 in the replay room, 0 otherwise (`@0x47a`..`0x4a3`); `label_74b` sets the text origins for
   each. Then it calls the script (`@0x4f0`).
3. Back from it, unless a debug or replay switch is set (`scene_run_mode`, `debug_map_active` `G[0xa6baba]`,
   `debug_am_active` `G[0xa6babb]`, `replay_room_active` `G[0x211c1a]`), it sets `scene_seen_flags[current_scene_id]`
   and stores it in the shared profile (`@0x5f1`..`0x658`). Every scene script does the same at its own end
   (`SC0000@0xbf3`..`0xc5a`).
4. If `system_next_mode` `G[0x1]` is set, `system_mode` takes it (`@0x663`). Unless the context is 2 or 7, it then
   runs the dispatch once more in the same context: it reopens `scjump_chain_pending` `G[0x213ef5]` (0x7fffffff),
   copies a mode other than 1 into `system_base_mode` `G[0x2]`, runs `SETADVFLAG` and, unless `debug_am_active` is
   set, every nonzero slot of `scjump_chain_script_ids` `G[0xa6b879]` (`@0x668`..`0x6e4`). So a scene can lead
   straight into the next.

The callers set `system_next_mode` before they hand a scene over: `ALLMAP` 2 (`ALLMAP@0x704`), `REIGN` 4
(`REIGN@0xb05`), `PALACE` 5 (`PALACE@0x7f7`), `BROTHEL` 4 (`BROTHEL@0xfe0`). `system_base_mode` keeps the mode the
scene interrupts: `INIT2`'s end reads it on a load inside a scene played on the map (`system_mode` 1 and
`scene_over_map` set), to rebuild what lies under it: `READMAP` for 2, `READFIELD`, `READREIGN` and `READCH` for 4
(`INIT2@0x3a2`..`0x3eb`).

### Scene scripts

A scene script's header (`SC0000@0x8ff`..`0xc5e`) branches on `scene_run_mode` `G[0xa6bab9]`:

- **0, a normal run:** it jumps on `current_scene_id` to the scene's branch (`SC0000@0xc66`..`0xceb`, ten tests), which
  sets the scene's played cell first (`SC0000@0xcf2`).
- **1, the debug scene jump:** TITLE's debug menu picks a script with `coroutine-label-yield … "BIN" "SC????.BIN"`,
  keeps it in the shared profile (`TITLE@0x49e`..`0x4b9`), and the script then loops on a label picked the same way
  (`SC0000@0x912`..`0xc5e`, `"LABEL" "J"`), with `system_mode` 1.
- **-1:** TITLE's `DEBUGADV` and `DEBUGADV2` entries (`TITLE@0x4c4`..`0x4f3`) and the replay room (`HMODE@0x7db`) set
  it. The seen and played cells are left alone while it is set.

`current_scene_id` `G[0x213ef3]` is set by the dispatch (`SCJUMP`), by a visit (`ALLMAP@0x6fa`, `REIGN@0xafb` and
`@0x1d90`, `PALACE@0x7ed`, from the visit slots below), by `BROTHEL@0xfdb` (from `G[0xa2448a + 4 * row + column]`, a
HAINIT array) and to 0 by `GAMESTART@0x16d8` (the prologue) and TITLE's debug entries.

Three arrays record a scene, each indexed by its number:

- `scene_played_flags` `G[0xc71 + scene]`, in a save: set to 1 by the scene's branch as it starts (1150 branches, every
  one `G[0xc71 + its number]`). The dispatch's rules test them most: they are this game's story flags.
- `scene_turns_since_played` `G[0x38a6 + scene]`, in a save: at each new turn `ALLMAP` adds 1 to the cell of every
  scene already played (`ALLMAP@0x244c`..`0x248c`), before it raises the turn counter `G[0x170a4]` (`@0x248f`, "The strategic map"; `ALLMAP@0x4f2b` draws it). The rules
  test it 192 times: the rule for scene 206 asks `G[0x3973] >= 2`, scene 205 played two turns ago or more
  (`SCJUMP@0x1be`).
- `scene_seen_flags` `G[0x1a1ba2 + scene]`, in a save and kept in the shared profile: set once the scene has been seen
  by this player, at the scene's end (above). The galleries' and information screens' unlocks read it.

The played range also holds cells no scene branch sets at its start: `G[0x2801]` (number 7056) is set by
`SC0200@0x10949`, `G[0x27ff]` and `G[0x2800]` by `SC0220@0x3ffb` and `@0x4451`, and `READY@0x422`..`0x438` sets
`G[0x2802]` from `G[0x19a331]`. They are story flags in the same array, under numbers SCINIT does not use; 630 rules test
`G[0x2801] != 1` and 668 `G[0xff5] != 1` (scene 900's played cell, `SC0900@0xcf2`).

### SCJUMP (the scene dispatch)

`SCJUMP` picks the next scene. A caller sets the context `scjump_context_id` `G[0x37d4]` and `scjump_chain_pending`
`G[0x213ef5]` = 0x7fffffff, runs `SETADVFLAG`, and runs every nonzero slot of a 32-slot chain,
`scjump_chain_script_ids` `G[0xa6b879 + n]`, with `G[0x211c92]` as the index (`ALLMAP@0x57`..`0x82`,
`SYSTEM4@0x6b2`..`0x6e4`). `INIT2@0x6f` puts `SCJUMP` in slot 0 and `$1$AUTORUN@0x6c` `$1$SCJUMP` in slot 1, so the
chain runs **base, then `$1$`**. `SCJUMP` branches on the context, 1 to 13 (`SCJUMP@0x0`..`0xb6`), to a list of
rules of this form:

```
if <condition> {
    if G[0x213ef5] > 0x0 { G[0x0] = 0x1; G[0x213ef3] = <scene>; G[0x213ef5] = 0x0 }
}
```

The first rule whose condition holds takes the turn: it clears `scjump_chain_pending`, so every later rule, in this
script and in the rest of the chain, finds it clear. The caller then sees `system_mode` 1 and hands the turn back to
`SYSTEM4`.

| script | rules | scenes they name | rules by context |
|---|---|---|---|
| `SCJUMP` | 549, and 360 visit rules | 416, from 1 to 6986 | 1: 87, 3: 173, 4: 50, 5: 2, 6: 26, 7: 22, 8: 29, 9: 90, 10: 26, 11: 9, 13: 35 |
| `$1$SCJUMP` | 14 | 11, from 4000 to 4015 | 1: 1, 3: 4, 7: 2, 9: 6, 13: 1 |

**The contexts**, from the scripts that set `scjump_context_id`. `ALLMAP` walks the turn through the phase cell
`G[0x170a6]` ("The strategic map"), and runs the chain at each step:

| context | set by | when |
|---|---|---|
| 1 | `ALLMAP@0x27ee` | the turn's start, after the new turn is counted (`G[0x170a6]` 1 to 2, `ALLMAP@0x24a9`..`0x277c`) |
| 11 | `ALLMAP@0x2885` | `G[0x170a6]` 2 to 3 |
| 3 | `ALLMAP@0x292a`; `@0x29` and `@0xa33` | `G[0x170a6]` 3 to 4, while `G[0x170a5]` is the player's `G[0x639f]` (`@0x2909`); on entering the map other than back from a scene (`G[0x8858]` 0 and `G[0x170a6]` not 1); after a 策略 (`RUSE`, `@0xa2d`) |
| 4 | `ALLMAP@0x29c1`; `@0x1fc` and `@0xab7` | `G[0x170a6]` 4 to 5; the same entry; after a command |
| 2 | `ALLMAP@0x2f7e`, `PALACE@0x9b7`, `REIGN@0x2845` | the visits on offer are gathered (below) |
| 7 | `ALLMAP@0xb81`, `@0x4b22` | a battle is about to start (after `CONQUEST`, `@0xb6d`, while `system_mode` is not 2) |
| 5, 6 | `REIGN@0x2c18`, `@0x2c97` | internal affairs |
| 8 | `FIELD@0xab6`, `@0x1f8d`, `@0x218a`, `@0x96a1` | on the battlefield, a sortie |
| 9 | `FIELD@0x9ad0` | a retreat |
| 10 | `FIELD@0xefe2` | a victory |
| 12, 13 | `ALLMAP@0xe6a`, `@0xf01` | the turn's end (`G[0x170a6]` to 10, then 11). No rule answers 12 |
| 0 | `GAMESTART@0x16d3`, `BROTHEL@0xfcb` | no rule tests it |

**The visit rules (context 2)** gather instead of taking the turn. `SETADVFLAG` first clears three sets of visit
slots (`SETADVFLAG@0x0`..`0x57`): `G[0x211c93]`, 200 slots, with three cells each at `G[0x21252b]`; `G[0x211d5b]`,
1000 slots, with `G[0x212783]`; and `G[0x212143]`, 1000 slots, with `G[0x21333b]`. A slot is -1 when empty. A rule
whose condition holds writes its scene into one slot and three numbers into the slot's cells, if the slot is empty
or holds a scene of a higher `G[0x219083]` value (`SCJUMP@0x46e5`..`0x4733`). The screens then offer them: `ALLMAP`
by region (`ALLMAP@0x6de`..`0x6fa`, `G[0x211c93 + G[0x19a329]]`), `REIGN` by a domain object (`REIGN@0xad7`..`0xafb`,
the slot is an OBINIT object id, `G[0x8f1152 + id]` at `REIGN@0xa81`), and `PALACE` and `REIGN` by character
(`PALACE@0x7e6`, `REIGN@0x1d89`; inferred from `PALACE`'s grid of characters). What the three numbers mean is not
decoded (the first is often the slot's id, the second 0x65 to 0x69).

**What a rule tests.** Of the 1158 cells the base script's rules test, 864 are played cells (4401 tests), so a scene
is offered until it has been played (`G[0xc71 + scene] != 1`) and once others have been. 811 rules ask that the
prologue has been played (`G[0xc71] == 1`). The rest are mostly the turn (`G[0x170a4]`, 17 tests;
`== 0x3e7` in 12, the 他国999ターン経過 scenes), the turns since a scene (`scene_turns_since_played`, 192 tests), the
nation acting `G[0x170a5]` (156 tests), and cells the callers set for the place in play: `G[0xc077]`, `G[0xc078]`,
`G[0xc07c]`, `G[0xc07d]`, `G[0xc07e]` (424 tests between them), and research values `G[0x1717c]` and `G[0x1717d]`
(`>= 0xc8`, the 魔術研究値 scenes). Of the 830 story-flag candidates `tools/story_flags.py` lists, 294 lie in the played
range.

`tools/scjump_decode.py` reads only Himegari's `SCJUMP`: it hard-codes Himegari's cells (the chapter `0x3234`, the
decision `0x62ccf`, the end `0x2f78d`) and walks every path through the script. Run with `--game koukaku` it ran
through most of this session without output and was stopped, so the counts above come from a scratch reading of each rule's block.
`docs/scjump-progression.md` is Himegari's too.

### What is saved

- **Saved:** `system_mode`, `system_next_mode`, `system_base_mode`, `current_scene_script_id` `G[0xc5a]`,
  `scjump_context_id`, `scene_over_map`, the played cells and the turns since, `scene_seen_flags` (also in the shared
  profile), and the turn and phase cells of "The strategic map", `G[0x170a4]` and `G[0x170a6]`.
- **Not saved:** SCINIT's arrays, `scjump_chain_script_ids`, `current_scene_id` and `scjump_chain_pending`, the visit
  slots (rebuilt each time context 2 runs) and the run switches (`scene_run_mode`, `replay_room_active`, the debug
  switches), all above the prefix. A load restores the scene by its frames, not by `current_scene_id`.

## Characters, titles and voices

Six tables describe the people of the game. INIT2 calls them in this order: CHINIT `INIT2@0x3`, CLINIT `@0x6`, HAINIT
`@0x42`, CVINIT `@0x4b`, CIINIT `@0x60` and SAINIT `@0x6c`. `$1$AUTORUN` then calls the append's fragments at the same
offsets (`$1$AUTORUN@0x3`, `@0x6`, `@0x42`, `@0x4b`, `@0x60`). SAINIT has no fragment, and `$1$HAINIT` is a bare
`exit`. Checked against the executed image, every fragment only adds records: none writes a cell of a base record.

| table | records | ids | capacity | leader | `$1$` adds |
|---|---|---|---|---|---|
| CHINIT (characters) | 406 (388 + 18) | 1..907 | 1000 | `G[0x22ab40]` | ids 40, 41, 271..274, 308, 327, 336, 744, 745, 751..753, 761..764 |
| CLINIT (titles, 称号) | 394 (376 + 18) | 1..912 | 1000 | `GS[0x3666]` | ids 361, 362, 365..368, 400, 415, 424, 726, 727, 731..733, 736..739 |
| CVINIT (voice settings) | 39 (37 + 2) | 1..41 | 1000 | `G[0x22a757]` | ids 40 (ハイシェラ), 41 |
| CIINIT (profiles) | 37 (35 + 2) | 1..37 | 1000 | `G[0xa4d780]` | ids 36 (ハイシェラ), 37 (シュタイフェ・ギタル) |
| HAINIT (the brothel) | 18 | 1..26 | 30 | `GS[0xe09d]` | none |
| SAINIT (skill areas) | 9 | 1..14 | 20 | `G[0x2b26f9]` | no fragment |

Every array of these tables lies above the saved prefix (`G[0x211c08]`, `GS[0x1b1]`), so none of it is saved: INIT2
writes them again after a load restores the save.
What a character carries from one week to the next is kept in separate arrays, listed in "What is saved" below.

The tool's stride list lacks 11, and the characters' and titles' stat arrays are 11 wide. These layouts were read with
11 added (`leaders.txt`). Without it, `init_layout.py` reads the stat arrays as stride 10 and reports a false
`G[0x232f3c] †`.

### CHINIT (characters)

Leader `G[0x22ab40]`, capacity 1000. The record is the character id. `CHINIT@0x0` writes character 1's
`G[0x22ab41] = 1` first, then its names `GS[0x3a4]`, `GS[0x3a5]` and `GS[0x26c8]`. The tool reports one write ahead
of a record: `$1$CHINIT@0x0` writes character 40's icon layer `G[0x22cd28]` before its `G[0x22ab68]`. That array
exists only in the fragment, so it cannot lead. One character id serves every
form of a person and every enemy. Monsters are ids 471 and up: they are the ones with a breeding picture.

The arrays are not on one grid. `G[0x22ab40]` and CVINIT's `G[0x22af28]` (one capacity above it) are ≡ 64 modulo
1000. Every array from `G[0x22cd00]` on is ≡ 704. Between them lie CVINIT's arrays and the work cells the
character scripts share, `G[0x22cacc]`..`G[0x22ccff]`. The current character is `G[0x22ccff]`: the
character scripts read it, and callers set it.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x22ab40]` | 1 |  | 406 | 1..907, 340 distinct | The base character, which is the id itself for most records. Other forms name the person they belong to: 152 アル→3, 166 and 167 ガルムス→8, 168 ベル→9, 176 エイフェリア→4. Every voiced page looks up the speaker's voice setting through it, as `G[0x22af28][G[0x22ab40][speaker]]` (`SC0000@0x12a8`, `@0x12af`; `SETPARTYVOICE@0xb6`). |
| `G[0x22af28]` | 1 |  | 37 | 3..41 | The character's voice setting, a CVINIT id. CVINIT writes it (`CVINIT@0x55`: character 1 → 3; `@0xaf`: character 3 → 5). The CVINIT layout shows it as `G[0x22af26] †`: the tool reads it by setting id, two cells early. A character with no setting speaks with setting 2 (エキストラ女) when its sex is 2, and setting 1 (エキストラ男) otherwise (`SC0000@0x12d3`..`@0x12f0`). |
| `G[0x22cd00]` | 1 |  | 18 | 1 | The layer whose icon sheet holds the character's icon. Only `$1$CHINIT` writes it (`$1$CHINIT@0x0`: character 40), with 1 on its 18 records. READBOARD copies the 1000 cells (`READBOARD@0x44`) and builds each layer's sheets from the hook chain `G[0xa6b819 + 3 × layer + kind]` (`READBOARD@0x99`). Kind 0 is the skill icons, `G[0x2a4dbe]`, 130 × 24 each. Kind 1 is the character icons, 50 × 50 each, 10 a row. Kind 2 is `G[0xa11c4a]`, 102 × 56 each. |
| `G[0x22d0e8]` | 1 |  | 386 | 10..8850, 385 distinct | The sort key. `sort_indices_by_key_sum(…, 1000)` orders by it in INFO26 (`INFO26@0x711`), RESTRUCT, BREED and DOLL (over 100). SORTCORP sorts a nation's members by it (`SORTCORP@0x32`); EMPLOY (`@0x7b1`), PALACE and READCORP read it too. |
| `G[0x22d4d0]` | 1 |  | 406 | 1..3: 1×126, 2×163, 3×117 | Sex: 1 male, 2 female, 3 neither. The page template falls back on it for the voice setting (`SC0000@0x12d3`). |
| `G[0x22d8b8]` | 1 |  | 386 | 1..3: 1×193, 2×174, 3×19 | The kind: 1 people, 2 monsters, 3 machines and things. RESTRUCT tests each value (`RESTRUCT@0x1e7f`, `@0x1ea0`), and the book shows it (`INFO26@0x743`). BREED (`@0xadb`), ADDCORP, SELLORD and INFO2 read it too. |
| `G[0x22dca0]` | 1 |  | 382 | 1..912, 271 distinct | The starting title, a CLINIT id. ADDCORP makes it the title in play, `G[0x23af90]` (`ADDCORP@0x5d7`, `@0x5de`), and runs SETCLASSLIST. RESTRUCT reads it (`@0xa5a`) and marks the title seen, `G[0x1a5a22][title]` (`@0xa6f`). |
| `G[0x22e088]` | 1 |  | 4 | 2, 22, 42, 62 | The starting costume of a 魔導巧殻, a PTINIT id: アル 2, リューン 22, ベル 42, ナフカ 62. At the first join UNITECH sets `G[0x269882]` to it and `G[0x269883] = 1`, runs ADDPART, and puts it in the saved costume `G[0xfc3b]` (`UNITECH@0x2f`..`@0x58`). GAMESTART reads it (`@0xacd`), and so does RESULT. |
| `G[0x22e470]` | 1 |  | 82 | 1 | *(not decoded)* A flag on 82 named characters. When such a character joins a nation with no free slot, ADDCORP runs SORTCORP and frees the slot of a member that has 0 (`ADDCORP@0x2aa`, `@0x315`). Also read by ALLMAP (`@0x2313`), READCORP (`@0xd0`), RESTRUCT (`@0x1124`) and FIELD. |
| `G[0x22e858]` | 1 |  | 386 | 1..5: 1×10, 2×38, 3×211, 4×97, 5×30 | The size: CALCABI reads it (`CALCABI@0x2bb`), and the status screen shows its name, `GS[0xa1d3 + size]` (ＳＳ to ＬＬ, `DRAWSTATUS@0x231a`). A title's size `G[0x23ee11]` replaces it when set (`CALCABI@0x29e`). |
| `G[0x22ec40]` | 1 |  | 386 | 2..15, 14 distinct | The element (属性). Its name is `GS[0xa1b4 + element]`, written by RFINIT: 2 物理, 3 万能 … 15 創造 (`DRAWSTATUS@0x2296`). When it equals the war's wind, `G[0x8da33b][G[0x170a7]]`, CALCABI adds 20% to stats 2 to 6 (`CALCABI@0x326`, `@0x36e`, `@0x375`). |
| `G[0x22f028]` | 1 |  | 221 | 1 | 1 on 221 characters, inferred to be the generic ones (troop types and monsters, not named people). When one joins, ADDCORP lowers the count `G[0x68d8][G[0x8eabf1]]` by one; for any other it clears the nation's stock of the character instead (`ADDCORP@0x56`, `@0x91`). It allows a nation a stock of 999 of it instead of 1 (`ADDCORP@0x222`, `@0x32f`). UNITECH sets the saved flag bit 2 only for a character with 0 (`UNITECH@0xb1`). BREED (`@0xabf`) and EMPLOY read it too. |
| `G[0x22f410]` | 1 |  | 10 | 1..296, 8 distinct | A 魔導巧殻's master: アル→1 (ヴァイスハイト), リューン→4 (エイフェリア), ナフカ→6 (オルファン), ベル→8 (ガルムス), and 168→166, 298→296. ADDCORP pairs the two (`ADDCORP@0x164`, `@0x172`): a joining character takes its partner's slot cells `G[0x3d370]` and `G[0xb2a0]` ("Troops and battle", what is saved). SORTCORP sorts a doll right after its master, on the key `(master's key << 16) + own key` (`SORTCORP@0x1d`, `@0x2b`). ARMAMENT reads it (`@0xdca`). |
| `G[0x22f7f8]` | 1 |  | 386 | 1..45, 37 distinct | The base level, the level of the base stats. UNITECH makes it the saved level at the first join (`UNITECH@0x23` → `G[0xf46b]`). CALCABI uses it for a character who has not joined and for every other nation's character (`CALCABI@0xb0`, `@0xd6`). |
| `G[0x22fbe0]` | 1 |  | 386 | 5..50, 12 distinct | The highest level another nation's character reaches (`CALCABI@0x162`, `@0x177`). Option bit `G[0x63af] & 1` raises it to 99 (`CALCABI@0x13e`). |
| `G[0x22ffc8]` | 1 |  | 330 | 4..20, 12 distinct | The weeks per level for another nation's character. CALCABI adds `(G[0x170a4] − 1) / interval` levels (`CALCABI@0xe2`..`@0x129`). `G[0x170a4]` is inferred to be the week, since INIT sets it to 1. When `G[0x63a0] > 0`, the interval becomes `(interval − 1) / 2 + 1` (`CALCABI@0xf0`). |
| `G[0x2303b0]` | 11 | 0..10 | 386 | 1..900, 415 distinct | The base stats. UNITECH copies them into the saved stats at the first join (`UNITECH@0x64`, `@0x74`). For a character who has not joined, CALCABI uses them as they are (`CALCABI@0xbc`). For another nation's character it computes `stat = base + growth × (level − base level) / 50`, then multiplies by `(5 + G[0x63a0]) / 5` (`CALCABI@0x1f5`..`@0x22b`). The book draws them (`INFO26@0x1c01`). The columns are named in the note under this table. |
| `G[0x232ea8]` | 11 | 0..8 | 364 | 1..2036, 493 distinct | The growth per stat, in hundredths of a point per level (inferred). BREED adds it to the saved remainders `G[0x12b1b]` and carries every 100 into a stat (`BREED@0x18f6`, `@0x18eb`). CALCABI uses it as above. |
| `G[0x2359a0]` | 1 |  | 384 | 40..100, 12 distinct | The starting loyalty. ADDCORP writes it into the nation slot's loyalty, `G[0x3c68c + 110 × nation + slot]` (`ADDCORP@0x61e`, `@0x629`). The book (`INFO26`) and RESTRUCT show it. |
| `G[0x235d88]` | 1 |  | 273 | 550..70000, 90 distinct | The hiring cost. EMPLOY subtracts it from the nation's money (`EMPLOY@0x607`, `@0x69e`, `@0x6e7`, `@0x1c10`), and the book shows it (`INFO26@0x1d45`). |
| `G[0x236170]` | 1 |  | 261 | 55..3000, 84 distinct | The weekly 契約 fee. ALLMAP takes it from the nation's money `G[0x17120]` every week (`ALLMAP@0x21c9`) and adds it to the week's expense record `G[0x136196]` (`@0x223e`, `@0x2277`). The status screen (`DRAWSTATUS@0x2446`, row 0x1b4) and the book (`INFO26@0x1d59`) show it. |
| `G[0x236558]` | 2 | 0..1 | 386 | packed picture ids | The unit pictures, CB…AA and CB…AB (character 1: `CB001AA.AGF`). The shared picture loader takes them for slots `G[0x22cc81]` and `G[0x22cc82]` (`DRAWSTATUS@0x3d3f`, `@0x3d74`). The status screen sets `G[0x22cc81]` on surface 0xb9 (`DRAWSTATUS@0x1574`). SETUN loads `G[0x22cc82]` as the unit's sprite (`SETUN@0x1055`). A title's `G[0x24a5a9]` replaces them when set (`DRAWSTATUS@0x3d1a`). |
| `G[0x236d28]` | 1 |  | 386 | packed picture ids | The portrait sheet (CPC…), for slot `G[0x22cc7c]` (`DRAWSTATUS@0x3d02`), which the status screen sets on surface 0xb8 (`DRAWSTATUS@0x848`). The book draws one quadrant of it (`INFO26@0x1ab7`). Replaced by the title's `G[0x24ad79]` (`DRAWSTATUS@0x3cd9`). |
| `G[0x237110]` | 1 |  | 75 | 50..90: 50×6, 60×3, 70×22, 80×26, 90×18 | The portrait's scale in percent, for slot `G[0x22cc7e]` (`DRAWSTATUS@0x3d0e`). ARMAMENT and BREED pass it to `gfx_set_scale_current`. Replaced by the title's `G[0x24b161]`. |
| `G[0x2374f8]` | 1 |  | 75 | packed picture ids | A CI picture, for slot `G[0x22cc83]` (`DRAWSTATUS@0x3da1`). SETUN keeps it per battle slot (`SETUN@0x1063` → `G[0xf079 + slot]`), and EXECSKILL reads it (`EXECSKILL@0x3b4`). Inferred to be the skill cut-in. Replaced by the title's `G[0x24b549]`. |
| `G[0x2378e0]` | 1 |  | 382 | 1..3: 1×33, 2×329, 3×20 | *(not decoded)* A value for slot `G[0x22cc84]` (`DRAWSTATUS@0x3dca`). SETUN keeps it per battle slot in `G[0xf06f + slot]` (`SETUN@0x106f`). EXECSKILL reads it (`EXECSKILL@0x14e`) to choose `G[0xa3e165] = 0x398 + 3 × G[0x8da4d1] + value`, with 2 when it is 0. Replaced by the title's `G[0x24b931]`. |
| `G[0x237cc8]` | 2 | 0..1 | 180 | packed picture ids | The breeding pictures (BR…) of monsters 471 to 907 (`BREED@0x164c`, `@0x20d0`). INFO27 reads them too. |
| `G[0x238498]` | 11 | 0..10 | 282 | packed voice ids | The battle voices, one column a situation. SETPARTYVOICE takes the column from its input `G[0x213ef6]` (`SETPARTYVOICE@0x0`). It plays the clip of a random party member (`@0x146`), and the title's `G[0x24bd19]` comes first when set (`@0x121`). RESULT plays column 10 (`RESULT@0x41b`, title `@0x3f6`). ORGANIZE, FIELD and EXECSKILL read it too. The tool, run without a leader, shows the cells of ids 1000 to 1083 as this array's: they are CLINIT's `G[0x23af91]`. |
| `GS[0x39e]` | 6 | 0..3 | 406 | ヴァイスハイト / リセル / アル | The name forms. The page's `draw_speaker_name` draws column `G[0x37d2]` of the speaker's row for forms 0 to 5, and ？？？？ for any other (`SC0000@0x10cfa`). Columns 4 and 5 are never written. |
| `GS[0x26c6]` | 2 | 0..1 | 386 | ヴァイスハイト / リセル / アル | The name in lists and the book. Column 1 holds an alias where there is one: character 11, ギュランドロス, is 仮面の紳士. The saved `G[0x15613][character]` picks the column (`RESTRUCT@0x1e3c`). The book reads column 0 (`INFO26@0xdb7`, `@0x1b8e`). |
| `GS[0x2e96]` | 2 | 0..1 | 386 | メルキア帝国のために戦う若き将 / … | Two description lines, shown by the book (`INFO26@0x1d9b`) and EMPLOY. |

The 11 stat columns are named by the TIINIT tips `GS[0xfd0c]`..`GS[0xfd1e]` and the rows that DRAWSTATUS draws them on
(the work copy is `G[0x22caf0 + column]`). The columns are:

- 0 支配 (`DRAWSTATUS@0x2406`, x 0x13d, row 0x19d);
- 1 ＨＰ (`@0x2082`, row 0xfc, with the current HP `G[0x22cc18]`);
- 2 物理攻撃 (`@0x20d0`, row 0x12a);
- 3 物理防御 (row 0x141);
- 4 魔法攻撃 (`@0x21ba`, row 0x186);
- 5 魔法防御 (row 0x19d);
- 6 移動 (`@0x23b8`, x 0x13d, row 0x186);
- 7 物理速度 (`@0x216c`, row 0x158);
- 8 魔法速度 (`@0x2256`, row 0x1b4);
- 9 not drawn: its work cell `G[0x22caf9]` has no reader;
- 10 政治 (`@0x2468`, row 0x1cb).

CALCABI clamps each stat to the cap `G[0x22cacc + 11 × G[0x211c2a] + column]` (`CALCABI@0x863`), and raises it
to 0 when it falls below, except column 6. The cap's writer is in INIT2, the parent's part.

How CALCABI chooses the values (`CALCABI@0x65`..`@0x22b`):

- For a character of the player's nation `G[0x639f]` who has joined (flag bit 0), it takes the saved level `G[0xf46b]`
  and the saved stats `G[0x10023]`. When a costume is worn (`G[0xfc3b]`), it takes the costume's values from
  CALCCOS instead.
- For one of the player's nation who has not joined, it takes the base level and the base stats.
- For every other nation's character, it computes level and stats from the week, as in the table.

It then adds the title's modifiers `G[0x240581]` (`CALCABI@0x491`), and the title's class and movement kind.

### CLINIT (titles)

Leader `GS[0x3666]` (the name), capacity 1000. The record is the title id (称号), and a character's title in
play is `G[0x23af90]`. The tool, run without a leader, puts CLINIT's sort key `G[0x23af91]` in CHINIT's voice array as
`G[0x238498]` ids 1000..1083: that array ends at `G[0x23af90]`. A title carries the unit's class (兵種), its
movement kind, its attacks, its special, skills and formations, its picture and voice overrides, and stat modifiers.

SETCLASSLIST offers a character up to five titles to change to. They come from the same line `G[0x23b379]`, taken in
sort order (`SETCLASSLIST@0x46`..`@0x68`), and each must pass the conditions below (`@0xea`..`@0x24c`). SETBRAND
gives the title, fixing it in `G[0x211801]` and marking it earned (`G[0x159fb]`, `SETBRAND@0x9bd`) and opened (`G[0x15de3]`, `@0x9c9`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x23af91]` | 1 |  | 394 | 10..9115, 394 distinct | The sort key (`SETCLASSLIST@0x68`; INFO26). |
| `G[0x23b379]` | 1 |  | 394 | 1..912, 267 distinct | The title's line: the titles a character can move between share it. SETCLASSLIST lists only titles whose line equals the current title's (`SETCLASSLIST@0x46`, `@0xa8`, `@0xaf`). With 0, it offers the current title alone. |
| `G[0x23b761]` | 1 |  | 394 | 1..36, 34 distinct | The unit class (兵種), drawn as an icon cell. CALCABI puts it in `G[0x22cb21]` (`CALCABI@0x2d5`), and EMPLOY draws it (`EMPLOY@0x1785`). |
| `G[0x23bb49]` | 1 |  | 71 | 1..237, 35 distinct | The character the title is exclusive to; 0 means anyone (`SETCLASSLIST@0xea`, `@0xf8`). |
| `G[0x23bf31]` | 1 |  | 91 | 16..50, 16 distinct | The level a character needs to be offered the title (`SETCLASSLIST@0x115`). |
| `G[0x23ced1]` | 3 | 0..2 | 60 | 24..9076, 35 distinct | Up to three required story flags: each nonzero n needs `G[0xc71 + n − 1] == 1` (`SETCLASSLIST@0x1e0`, `@0x1f2`). |
| `G[0x23da89]` | 3 | 0..2 | 5 | 1065, 2631, 8306 | Up to three forbidding story flags: the title is not offered when one is set (`SETCLASSLIST@0x23a`, `@0x24c`). |
| `G[0x23ea29]` | 1 |  | 394 | 1..3: 1×273, 2×102, 3×19 | The movement kind (形態): 1 地上, 2 飛行, 3 潜水, named by `GS[0xa1cf + kind]` (`CALCABI@0x284`; `DRAWSTATUS@0x22d8`, row 0x141; `EMPLOY@0x18f6`). |
| `G[0x23ee11]` | 1 |  | 5 | 3, 5 | A size that replaces the character's (`CALCABI@0x29e`, `@0x2ac`). |
| `G[0x23f1f9]` | 1 |  | 229 | 1..10, 8 distinct | The range (射程) the title adds. CALCABI adds it to `G[0x22cb1a]` and clamps that to 0..10 (`CALCABI@0x7bb`). The status screen draws it (`DRAWSTATUS@0x236a`, row 0x16f). |
| `G[0x240581]` | 11 | 0..10 | 130 | -40..80, 26 distinct | The stat modifiers, by the 11 stat columns (`CALCABI@0x491`). |
| `G[0x243079]` | 3 | 0..2 | 394 | 1..572, 130 distinct | The three attacks (ARINIT ids), which ADDCORP copies into `G[0x22cb2a]`..`G[0x22cb2c]` (`ADDCORP@0x976`..`@0x996`). |
| `G[0x243c31]` | 1 |  | 355 | 1..265, 140 distinct | The special (必殺技), into `G[0x22cb31]` (`ADDCORP@0x8cc`, `@0xcd7`). |
| `G[0x244019]` | 4 | 0..3 | 391 | 3..422, 203 distinct | Four passive skills (SKINIT's passive ids: DRAWSTATUS names them from `passive_skill_names`, `DRAWSTATUS@0x25ff`), into `G[0x22cb38]` (`ADDCORP@0x901`). |
| `G[0x244fb9]` | 2 | 0..1 | 394 | 2..53, 20 distinct | Two formations (FOINIT ids), into `G[0x22cb42]`..`G[0x22cb43]` (`ADDCORP@0x943`, `@0x953`). |
| `G[0x245789]` | 7 | 0..4 | 159 | 9012..9051, 16 distinct | The story flag that unlocks each of the seven slots: 0 the special, 1 to 4 the skills, 5 and 6 the formations (`ADDCORP@0x83c`, `@0x85c`). It applies only to the player's nation: for another, a slot with a flag is open while `G[0x63a0] >= 0`. |
| `G[0x2472e1]` | 7 | 2..3 | 155 | 9..40, 30 distinct | The level that unlocks each of the seven slots, against the saved level `G[0xf46b]`, for a slot with no flag (`ADDCORP@0x897`, `@0x8a9`). ADDCORP works the slots out into the work cells `G[0x22cb30]`..`G[0x22cb43]` each time it runs. |
| `G[0x248e39]` | 1 |  | 394 | 5..30, 7 distinct | *(not decoded)* A percent of the other unit's title, `G[0xce37][G[0xcddc]]`. EXECSKILL takes `100 + 10 × (G[0xcdf1] of that unit − of the acting one)`, clamped to 30..150, scales it by the percent and adds it to `G[0xc0cd]` of the acting unit's character (`EXECSKILL@0x9d8`); only when that character is the player's and has joined. Inferred to be the experience a unit is worth; `FIELD@0x81e1` reads it too. |
| `G[0x249221]` | 5 | 0..4 | 394 | 1..49, 37 distinct | *(not decoded)* Five values that SETUN copies for a battle unit (`SETUN@0xd7b`), with a worn costume's three (PTINIT `G[0x268fea]`) in their place. The unit's choice `G[0xbff2][slot]`, 1 to 5, picks one into `G[0xe179][unit]`; DRAWSTATUS names the choice with `GS[0xa1eb + choice]`. |
| `G[0x24a5a9]` | 2 | 0..1 | 6 | packed picture ids | Overrides the character's unit pictures `G[0x236558]` (`DRAWSTATUS@0x3d1a`). |
| `G[0x24ad79]` | 1 |  | 196 | packed picture ids | Overrides the portrait sheet `G[0x236d28]` (`DRAWSTATUS@0x3cd9`). |
| `G[0x24b161]` | 1 |  | 48 | 60..90 | Overrides the portrait scale `G[0x237110]`. |
| `G[0x24b549]` | 1 |  | 6 | packed picture ids | Overrides the CI picture `G[0x2374f8]`. |
| `G[0x24b931]` | 1 |  | 6 | 2..3 | Overrides `G[0x2378e0]`. |
| `G[0x24bd19]` | 11 | 0..10 | 7 | packed voice ids | Overrides the battle voices `G[0x238498]` (`SETPARTYVOICE@0x121`, `@0x133`; `RESULT@0x3f6`). The tool, run without a leader, shows TRINIT's cells as this array's ids 1004..1066: the array ends at the current troop, `G[0x24e811]`. |
| `GS[0x3666]` | 1 |  | 394 | 帝国軍千騎長 / 東領元帥 / 魔術を求めし戦王 | The title's name. |
| `GS[0x3a4e]` | 5 | 0..4 | 394 | 千の騎士を指揮する将軍に与えられる階 / … | Five description lines; the last ones list the 【指揮可能兵種】. |

SETCLASSLIST reads two conditions that no table writes:

- the nation pair `G[0x23c701]`, against the nation's `G[0x1717a]` (`SETCLASSLIST@0x179`, `@0x19e`);
- `G[0x23e641]` (`@0x14e`).

So neither condition ever fires. CALCABI likewise adds `G[0x23f5e1 + 3 × title + k]` to the three-cell group
`G[0x22cb11]` (`CALCABI@0x6fb`), and no table writes that array either.

While a costume is worn, PTINIT's arrays take the title's place for the picture, voice and choice values ("Items, parts and facilities", PTINIT:
`G[0x268d92]`, `G[0x268ebe]`, `G[0x268c66]`, `G[0x16293]`, `G[0x268cca]`, `G[0x268b3a]` and `G[0x268fea]`). The
costume's level is `G[0x161cb]`.

### CVINIT (voice settings)

Leader `G[0x22a757]`, capacity 1000. The record is the voice setting id. Setting 1 is エキストラ男, 2 エキストラ女, and
3 and up are the voiced characters. Setting 40, ハイシェラ, comes from `$1$CVINIT@0x0`. CVINIT also writes a cell keyed
by the character id, CHINIT's `G[0x22af28]` (see CHINIT).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x22a757]` | 1 |  | 39 | 130..11815546, 37 distinct | The default colour of the name. INIT2 copies it into the option `G[0x63ba + setting]` where that is below 0 and the default above 0, and stores the option in the shared profile (`INIT2@0x358`..`@0x395`). |
| `G[0x22af26]` † | 1 |  | 37 | 3..41 | *(the tool's view of CHINIT's `G[0x22af28]`, by character id)* |
| `G[0x22b310]` | 1 |  | 37 | 1..41, 37 distinct | The setting's character. The 設定 voice page shows a row once that character has been heard, `G[0x1a42b2][character]` (`CONFIG2@0xfad`, `@0xfc2`). |
| `G[0x22b6f8]` | 1 |  | 39 | packed voice ids | The sample voice the 設定 page plays (`CONFIG2@0xedd`). |
| `G[0x22bae0]` | 2 | 0..1 | 39 | packed picture ids | Two pictures of the 設定 page, CV…A and CV…B (setting 1: `CV000A.AGF`; `CONFIG2@0xf88`, `@0x1a36`). |
| `GS[0x1b0e]` | 3 | 0..2 | 39 | エキストラ男 / エキストラ女 / ヴァイスハイト | The name, the epithet and the voice actor (`CONFIG2@0x1b66`). |

Two options are keyed by the setting, and both are kept in the shared profile:

- the mute switch `G[0x33e7 + setting]`: CONFIG2 loads all 1000 (`CONFIG2@0x82f`) and toggles one (`@0x851`);
- the name colour `G[0x63ba + setting]`: the page uses it while the colour mode `G[0x63b9]` is 1 (`SC0000@0x10c1a`,
  `@0x10c83`).

### The ADV page's speaker cells

The page template in `mods/koukaku-base/oms/adv.oms` writes them, and `SC0000` shows each step:

- **The text and the speaker.** `G[0x37d1] = 1` once the page has text (`SC0000@0x8f6`). Then the speaker
  `G[0x37d0]` (`@0xfe3`), a character id, or −1 for narration. Then the name form `G[0x37d2]` (`@0x1227`).
- **The face.** The face picture `G[0x211c17]` (`@0xe05`), or −1 when the page passes none. `set_speaker_face`
  caches the face shown in `G[0x37d3]`; −1 releases element 0xd (`@0xa62`).
- **The voice setting.** A voiced page chooses the setting `G[0x22ab3f]` (`@0x12cb`..`@0x12f0`) and copies it into
  `G[0x211c90]` (`@0x12f5`). It is 0 for narration, which also ducks the music.
- **The voice.** The voice `G[0x211c18]` (`@0x12fa`). The page clears it when the setting's mute switch
  `G[0x33e7][G[0x211c90]]` is nonzero (`@0x1309`). `G[0x211c0d]` and `G[0x211c0e]` hold the message-skip and
  read-skip states for that test.
- **The history.** The page adds metadata 4, the setting (`@0x12ff`), and 5, the voice (`@0x1304`). The prelude adds
  2, the speaker, and 3, the form.
- **The heard flag.** `G[0x1a42b2][speaker]` is set and stored in the shared profile (`@0x1057a`..`@0x105a9`). This
  happens only on a page in form 0 while `G[0xa6bab9]`, `G[0xa6baba]` and `G[0xa6babb]` are all 0.

The scratch cells of the name's text effect (`G[0x211c8b]`..`G[0x211c8f]`, `G[0x211c14]`) and the name string
`GS[0x1b1]` are the prelude's working state. `G[0x63b8]` chooses whether a voice carries over into the next page
(`SC0000@0x1030b`, `begin_page_voice`). It is an option, and INITCONFIG's part.

### CIINIT (profiles)

Leader `G[0xa4d780]` (the sort key), capacity 1000: each record starts with it (`CIINIT@0x0` writes profile 1's
`G[0xa4d781] = 10`, `$1$CIINIT@0x0` profile 36's `G[0xa4d7a4]`), and no write comes before it. The record is the
profile id of the 人物 book, INFO23. The book lists every profile whose name is not empty (`INFO23@0x7c8`;
`INFO2@0x2be`) in the order of its sort key.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa4d780]` | 1 |  | 37 | 10..370 | The sort key (`INFO23@0x796`). |
| `G[0xa4db68]` | 16 | 0..15 | 37 | 1..41 | One cell per line, every one the profile's character. A line shows only once that character has been heard, `G[0x1a42b2][character]` (`INFO23@0x15d5`, `@0x15e7`, `@0x15f7`). INFO2 tests column 0 for the whole entry (`INFO2@0x36c`). |
| `G[0xa519e8]` | 48 | 33..47 | 10 | 231..2631, 12 distinct | Three conditions per line, at `48 × profile + 3 × line + k`; only lines 11 to 15 of profiles 2, 3, 6, 10, 18, 19, 23, 27, 31 and 34 have any. Each nonzero value n is a shared-profile flag `G[0x1a1ba2 + n − 1]`; the line shows when any of its three is set, or when it has none (`INFO23@0x1530`, `@0x157c`, `@0x15b5`). `CIINIT@0x136` writes profile 2's line 13: `0xa519e8 + 96 + 39 = 0xa51a6f`. The tool shows this array as `G[0xa51a3f] †`, stride 24. |
| `G[0xa5e508]` | 2 | 0..1 | 37 | packed picture ids | Two pictures. Column 0 (profile 1: `CB001AA.AGF`) goes on element 0xc3 (`INFO23@0x13fb`, `@0x140d`). Column 1 (`CR001.AGF`) goes on 0xc5 (`INFO23@0x1c61`); the list tests it too (`@0xa8d`, `@0xf83`). |
| `GS[0x11c1b]` | 1 |  | 37 | ヴァイスハイト・ツェリンダー / リセル・ルルソン / アル | The full name (`INFO23@0xf17`). |
| `GS[0x12003]` | 1 |  | 37 | メルキア帝国 / … / その他 | The group the book files the profile under (`INFO23@0x7dd`, `@0x828`). |
| `GS[0x123eb]` | 16 | 0..15 | 37 | 【所属】… / 【性別】… / 【種族】… | The lines: 【所属】, 【性別】, 【種族】, 【好きなもの】 and 【嫌いなもの】, then the text (`INFO23@0x161b`). |

The 4000 cells from `G[0xa5d568]` to `G[0xa5e508]` are never written and have no reader. The cells just below the
table belong to TIINIT ("Information screens"), not here: `GS[0x100c3]`..`GS[0x11c1a]` (TIINIT's 7 lines a tip) and `G[0xa4bc28]`
(7 cells a tip, read by `INFO22`, never written).

### HAINIT (the brothel)

Leader `GS[0xe09d]` (the girl's name), capacity 30. Only BROTHEL reads it. Every base sits on one 30-cell grid,
from `G[0xa2426e]` to `G[0xa24502]`. The name has no reader: BROTHEL draws a picture instead.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa2426e]` | 1 |  |  | never written | The nation the player must be, `G[0x639f]` (`BROTHEL@0x788`, `@0x796`). |
| `G[0xa2428c]` | 1 |  |  | never written | The town the brothel must be in, `G[0x19a329]` (`BROTHEL@0x7ac`, `@0x7ba`). |
| `G[0xa242aa]` | 1 |  | 18 | 4 or 60 | A bit mask: the girl is offered where the town's `G[0x8e0268][G[0x19a329]]` has a bit of it (`BROTHEL@0x74e`). |
| `G[0xa242c8]` | 1 |  | 18 | 1..5 | The lowest `G[0x70a9]` at which she is offered (`BROTHEL@0x772`). |
| `G[0xa242e6]` | 1 |  |  | never written | A town that the player's nation must hold: `G[0x3ed38][town] & 1` and `G[0x3ee00][town] == G[0x639f]` (`BROTHEL@0x7d0`). |
| `G[0xa24304]` | 3 | 0 | 17 | 201..8558 | Required story flags, `G[0xc71 + n − 1] == 1` (`BROTHEL@0x83b`). |
| `G[0xa2435e]` | 3 | 0 | 5 | 8552..8556 | Forbidding story flags (`BROTHEL@0x895`). |
| `G[0xa243b8]` | 1 |  | 18 | 1500..20000 | The price, against the nation's money `G[0x17120]` (`BROTHEL@0x4da`, `@0x52e`). |
| `G[0xa243d6]` | 1 |  | 18 | 132..833 | An item id, which BROTHEL makes the item in play, `G[0x2597db]` (`BROTHEL@0x58e`). |
| `G[0xa243f4]` | 4 | 0..3 | 18 | 1..12 | Thresholds on the saved visit count `G[0x7c81][girl]` (`BROTHEL@0x4c1`, `@0x4cc`, `@0xb13`). |
| `G[0xa2446c]` | 1 |  | 18 | 8541..8559 | The story flag a visit sets (`BROTHEL@0xf20`). |
| `G[0xa2448a]` | 4 | 0..3 | 18 | 250..6968 | Four scene ids, one per threshold, which BROTHEL passes in `G[0x213ef3]` (`BROTHEL@0xcfd`, `@0xf97`). |
| `G[0xa24502]` | 1 |  | 18 | packed picture ids | The girl's picture (`BROTHEL@0x5d2`, `@0xb58`). |
| `GS[0xe09d]` | 1 |  | 18 | ルイーネ / エルミナ / パティルナ | The name; nothing reads it. |

### SAINIT (skill areas)

Leader `G[0x2b26f9]`, capacity 20. The record is a skill's area, SKINIT's `G[0x2b23ce][skill]` (`SEARCHSKILL@0x39`;
`FIELD@0xd2d6`). It lies in the gap SKINIT leaves between its active and passive skills ("Troops and battle", SKINIT). Every base sits on the 20-cell grid.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2b26f9]` | 1 |  | 9 | packed picture ids | The area's picture on the field, MS001.AGF on (`FIELD@0xd2dd`, `@0xd2f7`). |
| `G[0x2b270d]` | 1 |  |  | never written | The area's x offset from the unit, as for a unit of facing 1 (`G[0xce73][unit]`). SEARCHSKILL adds it to the unit's position `GF[0xc8]` for that facing, and turns the two offsets for the other three: facing 0 adds y to x and takes x from y (`SEARCHSKILL@0xc2`, `@0xef`). |
| `G[0x2b2721]` | 1 |  | 2 | -300, -50 | The area's y offset, the same way (`SEARCHSKILL@0xad`, `@0x104`). |
| `G[0x2b2735]` | 1 |  | 4 | 100, 150 | The radius of an elliptic area; SEARCHSKILL tests it with `ellipses_intersect` (`SEARCHSKILL@0x45`, `@0x53`). |
| `G[0x2b2749]` | 10 | 0..3 | 4 | -150..150 | The x of each point of a polygon area, up to 10, as for facing 1 (`SEARCHSKILL@0x2af`, `@0x2e5`). SEARCHSKILL swaps and negates the two coordinates for the other facings and tests with `ellipse_polygon_intersect`. |
| `G[0x2b2811]` | 10 | 0..3 | 4 | -350..100 | The y of each point, the same way (`SEARCHSKILL@0x298`, `@0x2fc`). |
| `G[0x2b28d9]` | 1 |  | 4 | 4 | The polygon's point count (`SEARCHSKILL@0x251`, `@0x26e`). |

### What is saved

None of the six tables is saved. CHINIT, CLINIT, CVINIT, CIINIT, HAINIT and SAINIT lie above the prefix, so INIT2
builds them on every boot and every load. Their strings are in a save, but INIT2 writes them again after the load.
A data patch on these tables therefore takes effect for every save. The exception is a value that the game copies into
the saved arrays below at a character's first join: such a character keeps the old value.

**What UNITECH copies at the first join** (bit 0 of `G[0xf083]`): the base level, into `G[0xf46b]`; the base stats,
into `G[0x10023]`; and a 魔導巧殻's starting costume, into `G[0xfc3b]`. A save made after a character joined
keeps that character's level and stats. A patch to `character_base_levels` or `character_base_stats` then reaches
only characters who have not joined, and every other nation's.

The saved state keyed by character id, all below the prefix:

| array | holds |
|---|---|
| `G[0xf083]` [1000] | The character's flags. Bit 0 is set at the first join, when UNITECH copies the level and stats (`UNITECH@0x0`, `@0x8b`). UNITECH and EMPLOY set bit 1. UNITECH sets bit 2 while option `G[0x63a4] & 4` is on and the character is not generic (`UNITECH@0xa3`, `@0xb1`, `@0xcd`). ADDCORP tests `& 3` (`ADDCORP@0x0`..), and EMPLOY tests `& 7 == 5` (`EMPLOY@0x65e`). |
| `G[0xf46b]` [1000] | The level (`UNITECH@0x1c`, `CALCABI@0x65`). |
| `G[0xf853]` [1000] | The experience, 0 to 99. BREED adds to it and takes off 100 a level (`BREED@0x182c`, `@0x1833`); the status screen draws its bar (`DRAWSTATUS@0x16b8`). |
| `G[0xfc3b]` [1000] | The costume worn, a PTINIT id (`UNITECH@0x51`; `CALCABI@0x71`; ADDCORP). |
| `G[0x10023]` [1000][11] | The stats (`UNITECH@0x74`; `CALCABI@0x96`). |
| `G[0x12b1b]` [1000][11] | The growth remainders, in hundredths: BREED adds the growth and keeps the remainder of 100 (`BREED@0x18eb`, `@0x1901`). |
| `G[0x15613]` [1000] | Which name column the lists show, 0 or 1 (`RESTRUCT@0x1e3c`, `@0x2237`). |
| `G[0x211801]` [1000] | The title given to the character. SETBRAND sets it to the title in play (`SETBRAND@0x1`) and copies it into every nation slot that holds the character (`@0x494`). Once it is set, SETCLASSLIST offers that title alone (`SETCLASSLIST@0x24`, `@0x32`). |
| `G[0x7c81]` [30] | HAINIT's visit count by girl (`BROTHEL@0x4cc`, `@0x519`). |

The saved state keyed by title id, below the prefix:

| array | holds |
|---|---|
| `G[0x159fb]` [1000] | Earned (`SETBRAND@0x9bd`; tested by `SETCLASSLIST@0x15c`). |
| `G[0x15de3]` [1000] | Opened. SETCLASSLIST sets it (`@0x132`, `@0x285`) and so does SETBRAND (`@0x9c9`). GAMESTART loads it from the shared profile (`@0xb79`), and GAMECLEAR reads it (`@0x61b`). |

These single cells are saved too:

- the ADV cells `G[0x37d0]`..`G[0x37d3]`;
- the voice options `G[0x33e7]` and `G[0x63ba]`, which are also kept in the shared profile.

`G[0x22ab3f]`, `G[0x211c17]`, `G[0x211c18]` and `G[0x211c90]` lie above the prefix and are not saved.

These flags are kept in the shared profile, which outlives a save:

- the speaker heard, `G[0x1a42b2][character]`;
- the character met, `G[0x1a7192][character]`, which ADDCORP sets (`ADDCORP@0xc6d`) and the book requires;
- the book entry viewed, `G[0x1a983e][character]` (`INFO26@0xdcb`; INFO2);
- the title seen, `G[0x1a5a22][title]` (`RESTRUCT@0xa6f`);
- the special, skills and formations seen: `G[0x1a757a]`, `G[0x1a770a]` and `G[0x1a7bba]` (`ADDCORP@0xcf7`,
  `@0xd6f`, `@0xdee`).

Nation slots hold the members of a nation: `[nation][110]` arrays, the saved units ("Troops and battle", what is saved). They are the character
`G[0x35f6c]`, the title `G[0x3b9a8]`, the loyalty `G[0x3c68c]`, the flags `G[0x35288]`, the item `G[0x39fe0]` and the
troops `G[0x36c50]` and `G[0x37934]`. A nation's stock of a character is `G[0x2680a + 1000 × nation + character]`
(`ADDCORP@0x91`, `@0x32f`; BREED, RESTRUCT, EMPLOY).

## Troops and battle

The army's data: troop types (TRINIT), active and passive skills (SKINIT), attacks (ARINIT), formations (FOINIT), the
魔導巧殻 (COINIT), the battle's animations (ANINIT, ANINIT2), routines (RTINIT) and the reference lists the battle
reads (RFINIT). INIT2 calls them in its order (TRINIT, RFINIT, FOINIT, SKINIT, ARINIT, COINIT, RTINIT, ANINIT,
ANINIT2), and `$1$AUTORUN` the append's layers after the base tables. Every integer array lies above the saved prefix
(`G < 0x211c08`) and every string array above `GS[0x1b1]`, so nothing in these tables is saved: INIT2 writes them
again on every boot and load, and a change reaches a saved game at once. The exception is a unit's two attacks: the
unit saves the attack ids it was set up with, so a changed `troop_attacks` or `costume_attacks` reaches it only when a
script sets it up again. What a game keeps of its army is the per-unit state in the saved prefix, under "What is
saved" at the end.

A unit is a commander (a character with a class, or a 魔導巧殻) and its troop: a troop type and a number of soldiers.
The scripts that build one copy the unit's saved cells into work cells, look the type up in these tables and combine
the result in CALCABI's `unit_calc_parameters` (`G[0x22caf0]`, three rows of 11: row 0 the commander, row 1 the
result, row 2 the troop).

The layouts below come from `init_layout.py --game koukaku <TABLE> --markdown` with the leaders of `leaders.txt`, run
from a copy whose stride list adds 11 and 22: the shipped tool lacks them and lays `troop_parameters` and
`skill_parameter_bonuses` out wrong (notes, tool gaps).

### TRINIT (troop types)

The leader is the name `GS[0x4dd6 + id]`, 1000 a field. 183 troop types, ids 46..729: 176 in the base table, 7 in
`$1$TRINIT` (ヘタレ騎士 49, ヘタレ重騎士 89, ヘタレ馬騎士 104, ベルデーモン 638, ラウマカール 641, プロア 646, プローヴァ
647; the three ヘタレ ones replace base records). Ids come in families of upgrades (騎士 46 → 近衛騎士 47 → 守護騎士 48). The
work cells `troop_arg_id` (`G[0x24e811]`) and `troop_arg_soldiers` (`G[0x24e812]`) lie just before the arrays.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x24e813]` | 1 |  | 183 | 450..7290, 183 distinct | `troop_sort_keys`, id × 10: SELTR@0xbe1 and INFO14@0x66c sort the list by it (`sort-indices-by-key-sum`). |
| `G[0x24ebfb]` | 1 |  | 183 | 2..36, 26 distinct | `troop_kinds`, named by `troop_kind_names` (`GS[0xa042 + kind]`, INFO14@0xa14): 騎士 2, 弓騎士 3 … 創造体 36. CALCABI@0x34a puts it in `G[0x22cb22]`, beside the commander's kind in `G[0x22cb21]`; SELTR@0xc2d offers a type only where `commander_troop_kind_matrix[commander kind][kind]` is set. |
| `G[0x24efe3]` | 1 |  | 74 | 47..682, 74 distinct | `troop_upgrade_targets`: the type a unit becomes. SUPPLE@0x270..0x298 converts the unit when the target is set and the commander's command (`G[0x22caf0]`, row 0 column 0) reaches the target's `troop_command_minimums`. |
| `G[0x24f3cb]` | 1 |  | 129 | 100..500, 5 distinct 100×54, 250×5, 300×49, 400×5, 500×16 | `troop_command_minimums`, the command that upgrade needs (SUPPLE@0x270..0x298). |
| `G[0x24f7b3]` | 1 | | 0 | (no record writes it) | `troop_available_turns`: SELTR@0xd34 offers the type once the turn `G[0x170a4]` reaches it. Read as 0. |
| `G[0x24fb9b]` | 1 | | 0 | (no record writes it) | `troop_required_regions`: SELTR@0xd94..0xdda offers the type only while region `G[0x3ed38 + r]` bit 0 is set and `G[0x3ee00 + r]` is the player's nation `G[0x639f]` ("The strategic map"). Read as 0, no region. |
| `G[0x24ff83]` | 1 |  | 95 | 501..851, 95 distinct | `troop_required_characters`: SELTR@0xde2..0xe13 offers the type only while `G[0xf083 + character] & 3` == 3 ("Characters, titles and voices"). |
| `G[0x25036b]` | 1 |  | 95 | 1..1, 1 distinct 1×95 | Set on the same 95 records; no script reads it. |
| `G[0x250753]` | 2 | 0 | 52 | 80..500, 19 distinct | `troop_required_nation_levels`: the two values the player's nation must reach, `G[0x1717a + 2 × nation + 0/1]` (SELTR@0xd4a..0xd8c; "The strategic map"). `$1$TRINIT`'s text reads 【雇用条件】魔術技術５００以上 for プローヴァ, so these are likely the nation's technology levels (inferred). |
| `G[0x250f23]` | 3 | 0..2 | 79 | 8301..10000, 28 distinct | `troop_required_flags`: SELTR@0xe31..0xe63 offers the type only while `G[0xc71 + flag − 1]` == 1 for each one set (the story flags, "Scenes"). |
| `G[0x251adb]` | 3 | | 0 | (no record writes it) | `troop_forbidden_flags`: SELTR@0xe8b refuses the type while one is set. |
| `G[0x252693]` | 1 |  | 174 | 10..140, 58 distinct | `troop_command_costs`: the most soldiers a unit takes, `G[0x22cafb]` = command / cost, capped at `G[0x22cad7]` (CALCABI@0xa7c..0xaab; SUPPLE@0x2a9). |
| `G[0x252a7b]` | 1 |  | 183 | 1..3, 3 distinct 1×117, 2×57, 3×9 | `troop_movement_types`, `GS[0xa1cf + t]` 地上 / 飛行 / 潜水 (INIT2's list; DRAWSTATUS@0x3824). CALCABI@0x332 puts it in `G[0x22cb1e]`. |
| `G[0x252e63]` | 1 |  | 183 | 1..5, 5 distinct 1×1, 2×15, 3×119, 4×45, 5×3 | `troop_sizes`, `GS[0xa1d3 + s]` ＳＳ … ＬＬ (INIT2's list; DRAWSTATUS@0x3866). |
| `G[0x25324b]` | 1 |  | 183 | 2..15, 12 distinct | `troop_attributes`, named by `attribute_names` (`GS[0xa1b4 + a]`, DRAWSTATUS@0x1039). When it equals `G[0x8da33b + G[0x170a7]]` (INIT2's cell), CALCABI@0x3d0..0x462 adds a fifth to the troop's parameters 2..6. |
| `G[0x253633]` | 1 |  | 97 | 1..10, 5 distinct 1×22, 2×26, 3×41, 4×7, 10×1 | Added to `G[0x22cb1c]` and capped at 0..10 (CALCABI@0x7e5, @0xa50). Not decoded. |
| `G[0x253a1b]` | 3 | 0..2 | 83 | 5..25, 5 distinct 5×2, 10×25, 15×58, 20×6, 25×4 | Added to row 2 of `G[0x22cb11]` (three rows of 3; CALCABI@0x773). Not decoded. |
| `G[0x2545d3]` | 1 |  | 42 | 1..2, 2 distinct 1×35, 2×7 | Added to a march count (MARCHSEEK@0x12a, ARMAMENT@0x1dac). Not decoded. |
| `G[0x2549bb]` | 11 | 1..9 | 183 | -8..150, 73 distinct | `troop_parameters`, per soldier. CALCABI@0x509 adds them to row 2; CALCABI@0xaca..0xafc multiplies columns 1..5 by the soldiers and copies 6..9. The columns, read from the formation texts that change them (FOINIT): 2 physical attack, 3 physical defence, 4 magic attack, 5 magic defence, 6 movement speed, 7 physical attack speed, 8 magic attack speed, 9 痛打 (inferred); 1 is likely the soldier's HP (騎士 20, 守護騎士 40; inferred). Column 0 is the commander's command in row 0 and is not written here. |
| `G[0x2574b3]` | 3 | 0..1 | 183 | 1..571, 70 distinct | `troop_attacks`, ARINIT ids: column 0 the attack (1..), column 1 the troop attack (501..). ADDCORP@0x9f8..0xa23 copies the three into `G[0x22cb2d..0x22cb2f]`, and @0xa3b saves column 0 in the unit's `unit_troop_attacks`. |
| `G[0x25806b]` | 1 |  | 180 | 5..319, 88 distinct | `troop_passive_skills`, a passive skill id (騎士 22 盾Ⅰ, 弓騎士 77 対空迎撃Ⅰ). ADDCORP@0x9e2..0x9f3 puts it in the unit's passive slot `G[0x22cb37]`, and @0xe66..0xeab sets and stores the passive's shared-profile flag `G[0x1a770a + passive]`. |
| `G[0x25883b]` | 1 |  | 70 | 2..18, 10 distinct | `troop_upkeep_costs`, per soldier: ALLMAP@0x21f7..0x2213 takes them from the nation's funds `G[0x1713e + n]` each turn and enters them in the ledger `G[0x136196 + 8 × turn + 1/3]` ("The strategic map"). |
| `G[0x258c23]` | 1 |  | 174 | 2..81, 45 distinct | `troop_prices`, per soldier, against the funds (ARMAMENT@0x13ca..0x1438; SUPPLE@0x31c). |
| `G[0x25900b]` | 1 |  | 173 | 1..10, 10 distinct | In the battle's damage: halved when `G[0xcdf1 + target]` is below the attacker's (FIELD@0x82c1, EXECSKILL@0xab8). Not decoded. |
| `G[0x2593f3]` | 1 |  | 183 | 18914..16777641, 136 distinct | `troop_pictures`, file ids (`$1$`'s with the append's high byte): EMPLOY@0x3119 and ORGANIZE@0x4b53 put it in `G[0x22cc7d]`. |
| `GS[0x4dd6]` | 1 |  | 183 | 騎士 / 近衛騎士 / 守護騎士 | `troop_names` (the leader). |
| `GS[0x51be]` | 3 | 0..2 | 183 | 標準的な力を持った騎士 / 功績を積み重ね出世した優秀な騎士 / 信仰する神の祝福を受けた熟練の騎士 | `troop_description_lines`: the description, then 【雇用条件】 and the condition in words (INFO26). |

### SKINIT (skills)

SKINIT writes two tables with ids of their own, so each has its own leader. A run with one leader lays the other's
arrays out wrong (the † rows of either run).

**Active skills.** The leader is the name `GS[0x8c56 + id]`, 400 a field: 146 skills, ids 1..265; `$1$SKINIT` adds
four (玄武の滅鎌撃 16, 爆裂ミョウギ 65, ティルワンの死磔 101, 魔人参謀の采配 248). EXECSKILL runs one in battle.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2a4dbe]` | 1 |  | 4 | 1..1, 1 distinct 1×4 | `skill_script_slots`: EXECSKILL@0xd4d..0xd70 calls the script in `G[0xa6b8b9 + slot]` when it is set. INIT2@0x88 sets `G[0xa6b8b9]` = 0x4c00 (SKILLACT), so a base skill (slot 0) calls it; the four `$1$` skills set 1 and call `G[0xa6b8ba]` (the hook chains, the parent's). |
| `G[0x2a4f4e]` | 1 |  | 145 | 20..95, 16 distinct | `skill_costs`: EXECSKILL@0x32..0x39 takes it from the side's pool `G[0xc539 + side]`. |
| `G[0x2a50de]` | 1 |  | 97 | 1..9, 9 distinct | `skill_elements`, named by `element_names` (二段攻撃 2 物理, 焔炎剣 5 火炎). EXECSKILL@0x6b0 uses it as the column of `element_damage_percents`; DRAWSTATUS@0xbdd draws icon 0x32 + element. |
| `G[0x2a526e]` | 1 |  | 146 | 1..6, 6 distinct 1×72, 2×25, 3×12, 4×27, 5×7, 6×3 | `skill_effect_kinds`: 1 physical and 2 magic damage, from the unit's physical attack (column 2) or magic attack (column 4) plus `skill_powers` (EXECSKILL@0x63b..0x6ab); 3.. branch at @0xb7c. Not all decoded. |
| `G[0x2a53fe]` | 1 |  | 97 | 3..300, 65 distinct | `skill_powers`, added to that attack (EXECSKILL@0x69d): 二段攻撃 36 for its 物理攻撃＋３６. |
| `G[0x2a558e]` | 1 |  | 49 | 10..30, 5 distinct 10×5, 15×16, 20×11, 25×7, 30×10 | Set on the skills whose text says 吹飛ばし効果 (59..62: 15, 15, 15, 20), so likely the knockback (inferred). EXECSKILL@0x221a, @0x2350. |
| `G[0x2a571e]` | 1 |  | 17 | 1..3, 3 distinct 1×11, 2×5, 3×1 | ORGANIZE@0xdd8 plays a sound effect when it is 2; EXECSKILL@0x203e. Not decoded. |
| `G[0x2a58ae]` | 1 | | 0 | (no record writes it) | When set, CALCABI@0x562..0x59a counts the skill only if `G[0xc4c4 + value − 1]` == 100 (a saved cell). Read as 0. |
| `G[0x2a5a3e]` | 100 | 0..2 | 4 | 12..35, 8 distinct | `skill_bonus_kinds`, troop kinds ended by 0: CALCABI@0x5a1..0x648 counts whether the commander's kind and the troop's are among them. |
| `G[0x2af67e]` | 2 | 0..1 | 7 | 1..100, 9 distinct | EXECSKILL@0xc4b, @0xcaf. Not decoded. |
| `G[0x2af99e]` | 22 | 2..19 | 61 | -20..30, 27 distinct | `skill_parameter_bonuses`: CALCABI@0x66b..0x6c9 adds columns 0..10, times the commander's count, to row 0 and columns 11..21, times the troop's, to row 2. |
| `G[0x2b1bfe]` | 1 |  | 69 | 3..300, 11 distinct | `skill_durations`, the text's 効果時間 (ねこぱんち 5 for 効果時間５): EXECSKILL@0xb3b..0xb63 sets the unit's `G[0xe575]` to the skill and `G[0xe57f]` to it × 40. |
| `G[0x2b1d8e]` | 4 | 0..3 | 146 | 21..138, 117 distinct | `skill_animation_rows`, an ANINIT2 row for each of the unit's four facings `G[0xce73 + unit]` (EXECSKILL@0xd88..0xd9a). |
| `G[0x2b23ce]` | 1 |  | 133 | 1..14, 9 distinct | Not decoded. |
| `G[0x2b255e]` | 1 |  | 146 | 1..11, 11 distinct | `skill_types`: EXECSKILL@0x4e..0x6a sets the type's wait `G[0xc4d0 + type]` = `skill_type_waits[type]` × 40 for side 1, and FIELD@0x55f0 reads RFINIT's per-type arrays by it. |
| `GS[0x8c56]` | 1 |  | 146 | 二段攻撃 / 連槍撃 / 三段攻撃 | `skill_names` (the leader). |
| `GS[0x8de6]` | 1 |  | 146 | 範囲内の敵部隊に物理属性の物理ダメー… | `skill_descriptions` (EXECSKILL@0x540). |
| `GS[0x8f76]` | 2 | 0..1 | 146 | 【範囲内の敵部隊／物理属性】 / 物理攻撃＋３６の物理属性ダメージを与える | `skill_description_lines`: the target and element line, then the effect. |

**Passive skills.** The leader is the name `GS[0x9296 + id]`, 1000 a field: 209 skills, ids 3..422 (`$1$` adds 忍の心得
317). A unit has six passive slots `G[0x22cb32..0x22cb37]`: ADDCORP fills them from the costume's four parts (PTINIT's
`G[0x29effc]`, "Items, parts and facilities"), the item (ITINIT's `G[0x26729d]`) and the troop (`troop_passive_skills`). SETUN@0x10ec..0x119a
walks them with `passive_skill_arg_id` (`G[0x2b28ed]`), sums each effect's level into `G[0x22cb44 + effect]` and caps
it at 5. SAINIT ("Characters, titles and voices") is laid in the gap at `0x2b26f9..0x2b28d9` between the two kinds.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2b28ee]` | 2 | 0..1 | 209 | 1..82, 58 distinct | `passive_skill_effects`, named by `passive_effect_names` (`GS[0xa0d8 + effect]`: 騎馬殺し 1 … ). Column 1 is a second effect, on 23 records from 311 (戦闘複合スキル). |
| `G[0x2b30be]` | 2 | 0..1 | 209 | 1..5, 5 distinct 1×80, 2×38, 3×38, 4×38, 5×38 | `passive_skill_levels`, one per effect. |
| `GS[0x9296]` | 1 |  | 209 | 騎馬殺し / 重騎殺し / 天使殺し | `passive_skill_names` (the leader). |
| `GS[0x967e]` | 2 | 0..1 | 209 | 【戦闘スキル】 / 攻撃時に敵兵種が『騎乗』の場合にダメージが２５％上昇 | `passive_skill_description_lines`: the category, then the effect. |

### ARINIT (attacks)

The leader is the name `GS[0x809e + id]`, 1000 a field: 131 attacks, ids 1..572; ids 501.. are the troops' attacks.
`$1$ARINIT` is empty. A unit has the commander's attack (`G[0x22cb2a..0x22cb2c]`, from the class or the costume) and the
troop's (`G[0x22cb2d..0x22cb2f]`); CALCABI reads them.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x29f3e5]` | 1 |  | 131 | 10..5720, 131 distinct | Id × 10, likely a sort key; no script reads it. |
| `G[0x29f7cd]` | 1 |  | 110 | 1..6, 6 distinct 1×22, 2×30, 3×18, 4×15, 5×5, 6×20 | No script reads it. |
| `G[0x29fbb5]` | 1 |  | 131 | 1..1, 1 distinct 1×131 | `attack_uses_item_element`: CALCABI@0x2ef takes the element from the item's ITINIT `G[0x25ce8d]` instead ("Items, parts and facilities"). |
| `G[0x29ff9d]` | 1 |  | 35 | 1..1, 1 distinct 1×35 | Set on ids 501..572. CALCABI@0xc8b subtracts the commander's physical attack speed (`G[0x22caf7]`, row 0 column 7) from the interval `G[0x22cb27]` only when it is 0. |
| `G[0x2a0385]` | 1 |  | 131 | 1..9, 9 distinct | `attack_elements`, named by `element_names` (CALCABI@0x31a; DRAWSTATUS@0x1059). |
| `G[0x2a076d]` | 2 | 0..1 | 131 | 11..58, 36 distinct | `attack_timings`, into `G[0x22cb27]` (CALCABI@0xc7b). From the texts: 剣攻撃 [48, 22], 通常よりも僅かに早い 鍛剣攻撃 [47, 21], 初動を早めた 連剣攻撃 [46, 20], so column 0 is likely the interval and 1 the first delay (inferred). |
| `G[0x2a0f3d]` | 1 |  | 5 | 1..1, 1 distinct 1×5 | Adds 1 to `G[0x22cb1a]` and `G[0x22cb1c]` (CALCABI@0x7d7, @0x7f3). Not decoded. |
| `G[0x2a1325]` | 3 | | 0 | (no record writes it) | Read by CALCABI@0x74b. |
| `G[0x2a1edd]` | 11 | 9 | 10 | 3..6, 3 distinct 3×4, 5×4, 6×2 | `attack_parameter_bonuses`, in `troop_parameters`' columns (CALCABI@0x4e1); only 痛打 is written. |
| `G[0x2a49d5]` | 1 |  | 131 | 11..20, 10 distinct | `attack_animations`, an ANINIT id: FIELD@0xc14d puts it in `animation_arg_id`, and @0xc188.. draws it. |
| `GS[0x809e]` | 1 |  | 131 | 剣攻撃 / 鍛剣攻撃 / 連剣攻撃 | `attack_names` (the leader). |
| `GS[0x8486]` | 2 | 0..1 | 131 | 【攻撃／物理属性】 / 剣による物理攻撃 | `attack_description_lines`: the element line, then the description. |

### FOINIT (formations)

The leader is the name `GS[0x9e4e + id]`, 100 a field: 22 formations, ids 0..53 (無陣 0, the 魔導巧殻's 巧接陣形 51,
巧全陣形 52 and 巧砲陣形 53). `$1$FOINIT` is empty. The formation in play is `formation_arg_id` (`G[0x2b388e]`). A unit
offers three formations, `G[0x22cb3e..0x22cb40]`: `G[0x22cb41]` (1, 移動陣形, for a 魔導巧殻) and its class's or
costume's two, each multiplied by a cell (`G[0x22cc0d]` and `G[0x22cc0e]` for a costume, ADDCORP@0x79d..0x7ce;
`G[0x22cc11]` and `G[0x22cc12]` for a class, @0x953..0x96f), so likely a learned switch (inferred); SETUN@0x97d chooses
among them.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2b3890]` | 1 | | 0 | (no record writes it) | `formation_centre_y`, read as 0. |
| `G[0x2b38f4]` | 1 |  | 17 | 5..110, 12 distinct | `formation_centre_x`. ADDUN@0x13a..0x173 places soldier slot s at the unit's position plus (`formation_slot_x_offsets[s]` − centre x, `formation_slot_y_offsets[s]` − centre y), turned by the facing `G[0xce73 + unit]`. |
| `G[0x2b3958]` | 10 | 0..9 | 22 | -70..70, 23 distinct | `formation_slot_y_offsets`, ten slots (ADDUN@0x15a). |
| `G[0x2b3d40]` | 10 | 0..9 | 22 | -50..140, 33 distinct | `formation_slot_x_offsets` (ADDUN@0x13a). |
| `G[0x2b4128]` | 2 | 0..1 | 10 | 27..90, 9 distinct | SETFORM@0x9e. Not decoded. |
| `G[0x2b42b8]` | 2 | 1 | 2 | -75..-35, 2 distinct -75×1, -35×1 | Not decoded. |
| `G[0x2b4380]` | 200 | 0..107 | 17 | -110..110, 25 distinct | SETFORM. Not decoded. |
| `G[0x2b91a0]` | 200 | 0..107 | 17 | -150..150, 35 distinct | SETFORM. Not decoded. |
| `G[0x2bdfc0]` | 2 | 0..1 | 17 | 3..8, 5 distinct 3×8, 4×18, 5×1, 7×1, 8×1 | SETFORM. Not decoded. |
| `G[0x2be0ec]` | 3 | 0..2 | 9 | 10..30, 4 distinct 10×8, 15×1, 20×2, 30×2 | No script reads it. |
| `G[0x2be218]` | 11 | 2..9 | 21 | -50..200, 13 distinct | `formation_parameter_percents`, in `troop_parameters`' columns: SETUNABI@0x3a multiplies each by 100 + it (移動陣形 +200 movement, 防御陣形 +50 both defences). |
| `G[0x2be664]` | 1 |  | 18 | 1..5, 4 distinct 1×1, 3×9, 4×5, 5×3 | `formation_required_squads`: CHANGEFORM@0x7..0x1c drops to 無陣 when the unit's `G[0xd0fd + unit]` is below it (無陣's text: 小隊が不在、又は不足している状態); DRAWSTATUS@0x1226. |
| `G[0x2be6c8]` | 1 |  | 14 | 1..1, 1 distinct 1×14 | `formation_moves_while_attacking`, 1 on the formations whose text says 移動攻撃可能: FIELD@0x5ef0 stops an attacking unit when it is 0 (攻撃時移動停止). |
| `G[0x2be72c]` | 1 |  | 12 | 1..1, 1 distinct 1×12 | SETUN@0x989 ends its formation choice when it is set. Not decoded. |
| `G[0x2be790]` | 1 |  | 22 | 17914..17936, 22 distinct | `formation_pictures` (EMPLOY@0x312a). |
| `GS[0x9e4e]` | 1 |  | 22 | 無陣 / 移動陣形 / 防御陣形 | `formation_names` (the leader). |
| `GS[0x9eb2]` | 4 | 0..3 | 22 | 【移動特化型陣形】移動攻撃可能 / 移動速度が大幅上昇し、それ以外の / 戦闘力が低下 | `formation_description_lines`. |

`G[0x2be088]` (FIELD) has no writer.

### COINIT (costumes)

COINIT holds the costumes (衣装) of the four 魔導巧殻, keyed by costume id: a PTINIT id of part kind 51 ("Items, parts
and facilities", PTINIT). A character in a costume leads a unit as a class does: CALCCOS@0x8e sets the commander's
kind `G[0x22cb21]` to 13 (魔導巧殻), and ADDCORP@0x72f..0x800 fills from the costume's record what it fills from CLINIT
for a character (ADDCORP@0x953..0x9a1). The record in play is `costume_arg_id` (`G[0x267a6d]`) for ADDCORP and
ORGANIZE, and `current_part` (`G[0x269882]`) for ADDPART and CALCCOS, which a caller sets from the character's
`character_costumes` (`G[0xfc3b + character]`; CALCABI@0x8b, GAMESTART@0xafd), itself seeded by GAMESTART@0xb0a from
`character_initial_costumes` (`G[0x22e088 + character]`). ADDPART stocks a costume only while its stock is 0
(@0x24..0x6f), where it adds `part_count_change` of any other part (@0x2a9..0x2df), and the first time it copies the
costume's starting parts, level and stats into the saved `costume_parts`, `costume_levels` and `costume_stats`
(@0x224..0x28f). CALCABI@0x71..0xa6 runs CALCCOS for a commander in a costume and otherwise copies the character's
`G[0x10023]` row into row 0. The table has no string of its own; its first write is the pair
`G[0x267a6e + 2 × id]`, so that is the leader (stride 2, 100 a field). 14 records, ids 1..8 (アル's), 21 and 22
(リューン's), 41 and 42 (ナフカ's), 61 and 62 (ベル's). `$1$COINIT` is empty.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x267a6e]` | 2 | 0..1 | 14 | 1..1, 1 distinct 1×28 | The leader; no script reads it. |
| `G[0x267b36]` | 1 |  | 14 | 1..1, 1 distinct 1×14 | CALCCOS@0x59 puts it in `G[0x22cb1d]`. |
| `G[0x267b9a]` | 1 | | 0 | (no record writes it) | CALCCOS@0x65..0x7a. |
| `G[0x267bfe]` | 1 |  | 8 | 1..5, 5 distinct 1×2, 2×2, 3×1, 4×1, 5×2 | CALCCOS@0x4d puts it in `G[0x22cb1a]`; ALCHEMY. |
| `G[0x267c62]` | 4 | 0..3 | 4 | 101..412, 14 distinct | `costume_initial_parts`, PTINIT ids in four slots. ADDPART@0xe5..0x144 walks them when it stocks the costume: it caps each one's saved stock `G[0x16cbb + part]` at 999 (the add is of 0, so the fitted parts are not stocked) and sets its flag `G[0x1a17ba + part]`; ADDPART@0x258..0x268 copies them into `costume_parts`. |
| `G[0x267df2]` | 1 |  | 14 | 1..1, 1 distinct 1×14 | `costume_initial_levels`: ADDPART@0x224..0x240 puts it in `costume_levels` while that is 0. |
| `G[0x267e56]` | 1 |  | 14 | 1..30, 2 distinct 1×4, 30×10 | `costume_max_levels`, 1 on 1, 21, 41, 61 and 30 on the rest: RESULT raises `costume_levels` up to it (RESULT@0x155a, @0x1629, @0x178d); DOLL reads it. |
| `G[0x267eba]` | 1 |  | 14 | 35..35, 1 distinct 35×14 | INFO26, SELARM. |
| `G[0x2682a2]` | 11 | 0..10 | 10 | 1..68, 29 distinct | `costume_initial_stats`: ADDPART@0x27f..0x28f copies them into `costume_stats`; ALCHEMY (18 reads) and INFO26 read them. |
| `G[0x2686ee]` | 11 | 0..8 | 10 | 1..672, 57 distinct | `costume_stat_growth`, hundredths of a point a level: on a level-up RESULT adds it to the remainders `G[0x1686f]` and moves each whole hundred into `costume_stats` (RESULT@0x1670..0x16c0). |
| `G[0x268b3a]` | 3 | 0..1 | 14 | 1..565, 16 distinct | `costume_attacks`, ARINIT ids: ADDCORP@0x7d5..0x800 puts them in the commander's attack cells `G[0x22cb2a..0x22cb2c]`. |
| `G[0x268c66]` | 1 |  | 10 | 41..229, 10 distinct | ADDCORP@0x72f: `G[0x22cb31]`, and × `G[0x22cc0c]` into `G[0x22cb30]`. Not decoded. |
| `G[0x268cca]` | 2 | 0..1 | 14 | 2..53, 6 distinct | `costume_formations`, FOINIT ids: ADDCORP@0x7a2..0x7ce puts them in the unit's second and third formation. |
| `G[0x268fea]` | 3 | 0..2 | 14 | 2..37, 6 distinct | SETUN. Not decoded. |
| `G[0x269116]` | 1 |  | 14 | 17730..17781, 14 distinct | Picture ids (DOLL). |
| `G[0x26917a]` | 2 | 0..1 | 14 | 15671..17779, 28 distinct | Picture ids (15 scripts: ORGANIZE, EMPLOY, DRAWSTATUS …). |
| `G[0x269242]` | 1 |  | 10 | 16212..17782, 10 distinct | Picture ids. |
| `G[0x2692a6]` | 1 |  | 10 | 1..3, 2 distinct 1×2, 3×8 | Not decoded. |
| `G[0x26930a]` | 1 |  | 14 | 15673..17780, 14 distinct | Picture ids. |
| `G[0x2693d2]` | 6 | 0..5 | 14 | 90..260, 17 distinct | DOLL. Likely screen positions (inferred). |
| `G[0x26962a]` | 6 | 0..5 | 14 | 50..472, 14 distinct | DOLL. Likely screen positions (inferred). |

### ANINIT and ANINIT2 (the battle's animations)

ANINIT's leader is the file `G[0xa43756 + id]`, 1000 a field: 156 animations, ids 1..941; `$1$ANINIT` adds 278. The
animation in play is `animation_arg_id` (`G[0xa3e165]`): ALLMAP@0x407e..0x4196 and FIELD@0xc183.. draw it as a
model when its kind is 3 and as a texture when it is 0.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa43756]` | 1 |  | 156 | 15374..16777457, 115 distinct | `animation_files`: `load-mesh` at ALLMAP@0x409f and FIELD@0xc1a4 for kind 3, a picture otherwise. |
| `G[0xa43b3e]` | 1 |  | 22 | 1..3, 3 distinct 1×8, 2×4, 3×10 | `animation_render_kinds` (ALLMAP@0x4083, @0x412b): 3 a model, 0 a texture, 1 and 2 not decoded. |
| `G[0xa43f26]` | 1 |  | 146 | 1..3, 2 distinct 1×128, 3×18 | Operand 2 of `set-model-color` (op 0x332, ALLMAP@0x40b0). Not decoded. |
| `G[0xa4430e]` | 1 |  | 146 | 120..640, 6 distinct | `animation_widths` (`create-texture`, ALLMAP@0x414e). |
| `G[0xa446f6]` | 1 |  | 146 | 64..368, 8 distinct | `animation_heights` (ALLMAP@0x414e). |
| `G[0xa44ade]`, `G[0xa44ec6]`, `G[0xa452ae]`, `G[0xa45696]` | 1 |  | 12 each | 2..8, 2..7, 8..40, 300..1000 | Not decoded. |
| `G[0xa45a7e]` | 1 |  | 19 | 100..200, 2 distinct 100×11, 200×8 | `animation_scales`, both axes of `set-model-scale` (op 0x335, ALLMAP@0x40de); likely percent (inferred). |
| `G[0xa45e66]` | 1 |  | 19 | 1..2, 2 distinct 1×18, 2×1 | EXECSKILL@0xdfa branches on it. Not decoded. |
| `G[0xa4624e]` | 1 |  | 156 | -150..150, 13 distinct | `animation_x_offsets` (ALLMAP@0x40f5, @0x4173). |
| `G[0xa46636]` | 1 |  | 156 | -120..50, 14 distinct | `animation_y_offsets` (ALLMAP@0x4111, @0x418f). |
| `G[0xa46a1e]` | 1 |  | 10 | 1..1, 1 distinct 1×10 | Not decoded. |

ANINIT2 writes one array, `animation_sequences` (`G[0xa3e166]`, ten cells a row, 1000 rows: EXECSKILL's own arrays
begin at `G[0xa3e166 + 10000]`): 120 rows, 1..3 and 21..138, each with only column 0 written (an ANINIT id, 1..327);
`$1$ANINIT2` adds row 101 = 278. EXECSKILL@0xdc3..0xe0f plays a skill's row (`skill_animation_rows`) cell by cell up
to the first 0.

### RTINIT (routines)

64 arrays at `G[0x2beb7b + k × 100000]`; an id is routine × 100 + step. They fall in four blocks of 16:

| block | arrays | ids | read by |
|---|---|---|---|
| A | k 0..15 | 101..1436, 431 records | no script names them |
| B | k 16..31 | 101..1442 | no script names them |
| C | k 32..47 | | ALLMAP@0x360e..0x3d78 |
| D | k 48..63 | | MVRTN@0x7f..0x25c; RTN_M0xx@0x0 (k 51 `G[0x79bd5b]`, 52, 53) |

Arrays k 9, 41, 44, 57 and 60 take three cells an id. The cursor is `G[0x2beb79]` (the row) and `G[0x2beb7a]` (the
step; FIELD also uses it as a temporary); MVRTN and SETROUTE use `G[0x2beb78]`. `$1$RTINIT` writes block C ids
7101..7203. A layout with the leader `G[0x2beb7b]` and group 100 finds block A only (11 arrays), and leaves 8401 cells
outside. Not decoded further; no names.

### RFINIT (reference lists)

RFINIT writes the lists other tables index by kind. `$1$RFINIT` is empty.

| array | cells | values | holds |
|---|---|---|---|
| `GS[0xa042 + kind]` | 1..36 of 50 | 国家元首 / 騎士 / 弓騎士 … 創造体 | `troop_kind_names` (INFO14@0xa14). |
| `GS[0xa074 + k]` | 43 | 重要 / 素材 / 鉱石 | Item kinds ("Items, parts and facilities"). |
| `GS[0xa0d8 + effect]` | 66 of 200 | 騎馬殺し / 重騎殺し / 天使殺し | `passive_effect_names`. |
| `GS[0xa1a0 + element]` | 1..16 of 20 | 無属 / 物理 / 万能 / 地脈 / 火炎 / 冷却 / 電撃 / 神聖 / 暗黒 / 抵抗1..7 | `element_names`. |
| `GS[0xa1b4 + attribute]` | 1..15 of 20 | 無属 / 物理 / 万能 … 霊体 / 不死 / 創造 | `attribute_names`. |
| `G[0x8d982c]` | 50 × 50, rows 1..38 | 0..570 | `commander_troop_kind_matrix`, [commander kind][troop kind]: SELTR@0xc2d offers a troop type only where it is set. |
| `G[0x8da340]` | 20 × 20, rows 1..15, columns 1..19 | 0..200 | `element_damage_percents`, [the target's attribute][the element]: EXECSKILL@0x73b..0x760 and FIELD@0x5be4; 0 reads as 100. |
| `G[0x8d9763 + type]` | 11 | 3..999 | `skill_type_waits`, × 40 into `G[0xc4d0 + type]` (EXECSKILL@0x63). |
| `G[0x8d937b]`, `0x8d93df`, `0x8d9443`, `0x8d94a7` (2 a type), `0x8d956f` (2), `0x8d9637`, `0x8d969b`, `0x8d96ff`, `0x8d97c7` | by skill type, 1..11 | | Read by FIELD@0x560a..0x5c67 for the type in `G[0x2beb7a]`. Not decoded. |
| `G[0x8da254]`, `G[0x8da2b9]` (60 a row), `G[0x8da331]`, `G[0x8da336]` | | | ALCHEMY and ALLMAP ("Items, parts and facilities", "The strategic map"). |

### What is saved

None of these tables. The army's saved state is per unit, in arrays of 30 nations × 110 slots
(`G[base + 110 × nation + slot]`, nation `G[0x8df3bf]`, slot `G[0x8da4d0]`; the arrays lie 3300 apart, which gives
the 30). ADDCORP@0x9a6..0x9d1 reads a new unit's item, troop and soldiers and @0xa2b..0xaab writes its attacks and
formation; SUPPLE@0x1d8..0x248 copies all of them back into the work cells:

| array | work cell | holds |
|---|---|---|
| `G[0x35f6c]` | `G[0x22ccff]` | `unit_commanders`, the character |
| `G[0x36c50]` | `troop_arg_id` | `unit_troops`, the troop type |
| `G[0x37934]` | `troop_arg_soldiers` | `unit_soldiers` |
| `G[0x38618]` | `G[0x22cc14]` | `unit_commander_attacks`, from the class or costume (ADDCORP@0xa2b) |
| `G[0x392fc]` | `G[0x22cc15]` | `unit_troop_attacks`, from `troop_attacks` (ADDCORP@0xa3b) |
| `G[0x39fe0]` | `G[0x2597db]` | `unit_items`, an ITINIT id |
| `G[0x3acc4]` | `formation_arg_id` | `unit_formations` (ADDCORP@0xa4b..0xaab) |
| `G[0x3b9a8]` | `G[0x23af90]` | `unit_classes`, a CLINIT id |

`G[0x35288]`, `G[0x3c68c]`, `G[0x3d370]` and `G[0x3e054]` have the same shape and are not decoded. A nation's troops in
reserve are `nation_troop_reserves`, `G[0x2dd3a + 1000 × nation + troop]`, capped at 999. A costume's parts are saved in
`costume_parts` (`G[0x16293 + 4 × costume + slot]`) and its stats in `costume_stats` (`G[0x16423 + 11 × costume + column]`) (CALCCOS@0x28, @0xb0;
ADDCORP@0x764 reads the parts); under a condition on `G[0x63a4]` GAMESTART@0xcf2..0xd2b loads both from the shared
profile. The parts stock is `G[0x16cbb + part]` ("Items, parts and facilities"). The battle's per-side and per-unit arrays (`G[0xc539]` … `G[0xccff]`, by the battle unit
`G[0xcddb]`) are saved cells too, not decoded here.

## Items, parts and facilities

Four boot tables and one on-demand table hold what a nation owns and makes, and what its towns' facilities offer:
ITINIT the items, PTINIT the 魔導巧殻's costumes and parts, ALINIT the alchemy recipes, BRINIT the breeding and
development combinations, and SHINIT the shop's and the employment office's offers. Every integer array of the four
boot tables lies above the saved prefix (`G[0x211c08]`), and so does every string array (`GS[0x1b1]`), so none of
them is saved: INIT2 writes them again on every load. What the game keeps is the inventory and the facility
state, below the prefix (What is saved, at the end).

Several readings use a value vector the item, part, character, class and troop tables share. The 11-stat vector's
columns are 0 支配, 1 ＨＰ, 2 物理攻撃, 3 物理防御, 4 魔法攻撃, 5 魔法防御, 6 移動速度, 7 物理速度, 8 魔法速度,
9 クリティカル率, 10 政治 (from the descriptions of ITINIT records 131 to 205, which name the stat they raise, and
INFO24's item page, which draws the columns in that order from `INFO24@0x12a9`). The 3-vector is 0 対地, 1 対空,
2 対潜. Kinds are named by RFINIT's `GS[0xa074 + kind]`, elements by `GS[0xa1a0 + element]`, movement types by
INIT2's `GS[0xa1cf + type]` (1 地上, 2 飛行, 3 潜水) and ranks by INIT2's `GS[0xa1d9 + rank]` (0 Ｅ to 6 Ｍ,
`INIT2@0x24a`..).

### ITINIT (items and equipment)

Leader `GS[0x5d76]` (the name), capacity 2000. 457 records: 449 in the base and 8 new ones from `$1$ITINIT` (34
配合大全　特集; 73 and 74, the title mementos パルトナツァイリー and サタナスツァイリー; the exclusive weapons 323
アヴェンジェンス and 570 斬鉄爪; 781 はいちぇらそーど, 819 エウシュリティア, 839 ヘタレスの指輪). The layer
overwrites no base record. The 29 integer slices from `G[0x2597dd]` to `G[0x267a6d]` (exclusive) agree modulo 2000,
and no cell falls outside them; the argument cells `G[0x2597db]` and `G[0x2597dc]` sit just ahead of the first field
(the helpers, below). The record order in the source is the name, the integers, then the description
(`ITINIT@0x14`..).

The ids fall in ranges by kind (`G[0x25a77d]`):

- 1..93 key items (kind 1 重要, 88 records);
- 101..127 and 1001..1366 materials: kinds 4 素材, 5 鉱石, 6 作物, 7 動物, 8 木材 and 9 土 (192 records);
- 131..856 equipment, kind 35 装飾 (177 records): 131..205 stat items, 208..216 the 紋章 (an attack element),
  229..235 the 領域の宝珠 (射程), 236..249 the 指南書 (the 3-vector), the rest weapons, armour and accessories, 30 of
  them exclusive to one character.

The stat array `G[0x260d0d]` has 11 cells per id (`0x260d0d = 0x2597dd + 15 × 2000`). CALCABI reads it with an
11-cell row (`CALCABI@0x4b9`), and the descriptions agree with the 11 columns.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2597dd]` | 1 |  | 457 | 5..13660, 457 distinct | The list order, mostly 10 × id (`sort-indices-by-key-sum` at `SELARM@0x61d`, `INFO24@0x702`, `INFO15@0x661`, `REPORT@0x56c`, `RUSE@0x11a6`). |
| `G[0x259fad]` | 1 |  | 399 | 1..6, 6 distinct 1×60, 2×72, 3×66, 4×107, 5×40, 6×54 | The rank, drawn as `GS[0xa1d9 + rank]` (`INFO24@0x123d`). |
| `G[0x25a77d]` | 1 |  | 457 | 1..35, 8 distinct | The kind, `GS[0xa074 + kind]` (INFO24 tests it from `INFO24@0x734`). 0 is no item: ADDITEM does nothing (`ADDITEM@0x1`). Kind 1 sets the key flag (`ADDITEM@0x166`). SELARM takes an item whose kind is the slot's or 35 (`SELARM@0x76f`..`0x792`). |
| `G[0x25af4d]` | 1 |  | 62 | 1..1, 1 distinct 1×62 | 1 on 62 key items (ids 11..75): carried into a new game with the inventory (`GAMESTART@0xd8f`). |
| `G[0x25b71d]` | 1 |  | 30 | 1..41, 29 distinct | The one character who may equip it, a CHINIT id; SELARM refuses anyone else (`SELARM@0x79a`..`0x7bd`, against `G[0x22ccff]`). 321 ザイムリッター has 1, ヴァイスハイト. |
| `G[0x25beed]` | 1 |  |  | (none written) | SELARM refuses the item unless it equals `G[0x22cb21]`, the value CALCABI derives from the class and CALCCOS sets to 13 for a 魔導巧殻 (`SELARM@0x7be`..`0x7e1`). No record writes it. |
| `G[0x25c6bd]` | 1 |  | 1 | 2 | The gender required, against CHINIT's `G[0x22d4d0 + character]` (`SELARM@0x7e2`..`0x80c`). Only 819 エウシュリティア writes it, 2, 【女性専用】. |
| `G[0x25ce8d]` | 1 |  | 39 | 1..9, 9 distinct | The attack element, `GS[0xa1a0 + element]` (`INFO24@0x12d3`): CALCABI uses it when the attack's own element is 1 (`CALCABI@0x2e1`). The 紋章 208..216 have 1..9. |
| `G[0x25d65d]`, `G[0x25de2d]` | 1 |  |  | (none written) | No reader seen. |
| `G[0x25e5fd]` | 1 |  |  | (none written) | The movement type it imposes: CALCABI copies a nonzero value into the result `G[0x22cb1d]` (`CALCABI@0x259`); INFO24 draws it (`INFO24@0x12ea`). No record writes it. |
| `G[0x25edcd]` | 1 |  | 15 | 1..10, 8 distinct | The 射程 bonus, added into `G[0x22cb1a]` (`CALCABI@0x7c9`; `INFO24@0x1301`). The 領域の宝珠 have it. |
| `G[0x25f59d]` | 3 | 0..2 | 27 | 4..20, 8 distinct | The 対地, 対空 and 対潜 bonuses, added into `G[0x22cb11]` (`CALCABI@0x723`; `INFO24@0x141f`). The 指南書 have them. |
| `G[0x260d0d]` | 11 | 0..10 | 153 | -10..300, 43 distinct | The 11-stat bonuses, added into `G[0x22caf0]` (`CALCABI@0x4b9`; `INFO24@0x12a9`..). |
| `G[0x2662fd]` | 2 | 0..1 | 410 | 10..100000, 215 distinct | Column 0 the buying price, 1 the selling price: SHOP reads the column of its mode (`SHOP@0x92d`, `@0x9a4`); ADDITEM sells what goes above 999 at column 1 (`ADDITEM@0x71`); INFO24 draws column 0 (`INFO24@0x1254`). |
| `G[0x26729d]` | 1 |  | 57 | 5..311, 40 distinct | The skill the equipment carries, `GS[0x9296 + skill]` (`INFO24@0x151b`); ADDITEM marks it known in `G[0x1a770a + skill]` (`ADDITEM@0x134`..`0x163`). |
| `GS[0x5d76]` | 1 |  | 457 | 魔導肩叩き機 / 魔導まかない機 / 魔導お叱り機 | The name (`INFO24@0x11b5`; SHOP, SELARM, ALCHEMY). |
| `GS[0x6546]` | 2 | 0..1 | 457 | 【重要】エイフェリアのためにリューン / … | The description's two lines (`INFO24@0x155f`). The first line's bracket gives the kind, the element and the exclusive user (【装備／万能属性／ハイシェラ専用】). |

### The item helpers and the inventory

The inventory is a nation's: `item_counts` `G[0x17daa + 2000 × nation + item]`, the nation being an id of the
strategic map's (the player's is `G[0x639f]`). ADDITEM, ALCHEMY, SHOP, REIGN, SELOBJ, RUSE, INFO15, QUEST, SELARM,
CHECKREG, GAMECLEAR and GAMESTART read it.

ADDITEM adds items. 40 scripts call it, among them SHOP, ALCHEMY, SELARM, BROTHEL and RECEIPT. The caller sets the
item in `current_item` `G[0x2597db]`, the change of its count in `item_count_change` `G[0x2597dc]` and the nation
in `G[0x8df3bf]` (the callers copy `G[0x639f]` into it, e.g. `SELARM@0x59c`..`0x5ab`). It waits for nothing: the
script is one statement (`ADDITEM@0x0`, `@0x18e`).

- `ADDITEM@0x1`: an item of kind 0 changes nothing.
- `ADDITEM@0xf`..`0x55` adds the change and keeps the count from going below 0.
- `ADDITEM@0x58`..`0xff`: above 999, it sells the excess at the selling price, if the item has one, into the
  nation's money `G[0x17120 + nation]` (kept within 0 and 99,999,999), and sets the count to 999.
- `ADDITEM@0x104`..`0x163`, for the player's nation only: it sets `item_obtained_flags` `G[0x1a0fea + item]`, sets
  `item_seen_flags` `G[0x1a4a82 + item]` and stores it in the shared profile, and marks the item's skill known in
  `G[0x1a770a + skill]`, also in the shared profile.
- `ADDITEM@0x166`..`0x189`: for a key item (kind 1) it sets `G[0xc71 + 9000 + item]` to 1, the story flag
  `9001 + item` (the cells `G[0x2f99 + item]`). It never clears it. ALCHEMY's recipes test these flags to make a key
  item once (ALINIT, below): recipe 1, 魔導肩叩き機, refuses flag 9002.

SHOP buys and sells. Its mode `local 0x493c` (0 buy, 1 sell, toggled at `SHOP@0x7d0`) picks the price column. To
buy it lists SHINIT's offers (below, `SHOP@0xf31`..`0xfc6`); to sell it lists every item held (`SHOP@0xfcc`). It
calls ADDITEM with the count (`SHOP@0x10fb`) or its negative (`SHOP@0x119b`) and moves the money in
`G[0x17120 + G[0x639f]]`.

Three more scripts give items or parts from other tables' data: BROTHEL gives HAINIT's item for a visit
(`BROTHEL@0x58e`..`0x59f`, item `G[0xa243d6 + visit]`, its price `G[0xa243b8 + visit]`), RECEIPT gives a quest's
rewards from QUINIT (`RECEIPT@0xb13`..`0xc0b`: the reward kind `G[0xa21f46 + 2 × quest + k]` is 1 a troop through
ADDCORP, 2 an item, 3 a costume, 4 a part; the id `G[0xa22716 + …]`, the count `G[0xa22ee6 + …]`; the money
`G[0xa236b6 + …]`), and SELARM swaps equipment (below).

### Equipment

SELARM equips an item on a 部隊 slot. It refuses an item that has not been obtained this game
(`item_obtained_flags`, `SELARM@0x759`), one of another kind than the slot's unless its kind is 35, one exclusive to
another character, one with a `G[0x25beed]` that differs, and one of the other gender (`SELARM@0x754`..`0x812`). On
a change it gives the old item back and takes the new one, both through ADDITEM, and stores the new one in the
troop's slot `G[0x39fe0 + 110 × nation + slot]`, the slot being `G[0x8da4d0]` (`SELARM@0x552`..`0x5ce`).

CALCABI adds the equipped item's numbers into its result block: the stats into `G[0x22caf0]` row 0, the 3-vector
into `G[0x22cb11]`, 射程 into `G[0x22cb1a]`, the movement type into `G[0x22cb1d]` and the element (CALCABI, cited in
the table). For a character who wears a costume, CALCABI calls CALCCOS (`CALCABI@0x71`, the costume in
`current_part`), which adds the costume and its four parts (below).

### PTINIT (costumes and parts)

Leader `GS[0x74e6]` (the name), capacity 1000. 111 records, all in the base: the costumes 1..8, 21, 22, 41, 42, 61
and 62 (kind 51 衣装), and the parts 101..525, kinds 52 頭, 53 体, 54 腕, 55 足 and 56 部品. The costumes belong to
the four 魔導巧殻: 1..8 アル (character 3), 21 and 22 リューン (5), 41 and 42 ナフカ (7), 61 and 62 ベル (9). The
arrays agree modulo 1000 from `G[0x269884]`, and no cell falls outside them; the argument cells `G[0x269882]` and
`G[0x269883]` sit just ahead. As in ITINIT, the stat array `G[0x29c504]` has 11 cells per id, as CALCCOS reads it
(`CALCCOS@0xe8`), `0x29c504 = 0x269884 + 208 × 1000`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x269884]` | 1 |  | 111 | 10..5250, 111 distinct | The list order (`sort-indices-by-key-sum` at `DOLL@0xbd2`, `INFO24@0x7a8`, `INFO26@0x846`). |
| `G[0x269c6c]` | 1 |  | 93 | 1..6, 6 distinct | The rank, `GS[0xa1d9 + rank]` (`INFO24@0x168c`). |
| `G[0x26a054]` | 1 |  | 111 | 51..56, 6 distinct 51×14, 52×19, 53×20, 54×20, 55×21, 56×17 | The kind, `GS[0xa074 + kind]` (`INFO24@0x16a3`). ADDPART, ALCHEMY and GAMESTART test 51 (衣装). |
| `G[0x26a43c]` | 100 | 0 | 41 | 3..9, 4 distinct 3×16, 5×9, 7×8, 9×8 | The characters who may use the part, CHINIT ids, a zero ending the list; DOLL refuses it to anyone else (`DOLL@0xd83`..`0xdf0`). Only column 0 is written: one each. |
| `G[0x282adc]` | 100 | 0..3 | 4 | 1..61, 4 distinct | The costumes the part fits, a zero ending the list; DOLL refuses it to another (`DOLL@0xdf1`..`0xe5e`). Only the 無衣装専用 parts 524 and 525 write it (1, 21, 41, 61). |
| `G[0x29b17c]` | 1 |  | 10 | 2..3, 2 distinct 2×8, 3×2 | The movement type it imposes (2 飛行, 3 潜水): CALCCOS copies it into `G[0x22cb1d]` (`CALCCOS@0x15e`). |
| `G[0x29b564]` | 1 |  | 5 | 1..3 | The 射程 bonus (`CALCCOS@0x150`). |
| `G[0x29b94c]` | 3 | 0..2 | 16 | 5..20, 5 distinct | The 対地, 対空 and 対潜 bonuses (`CALCCOS@0x130`). |
| `G[0x29c504]` | 11 | 0..10 | 88 | -35..80, 29 distinct | The 11-stat bonuses (`CALCCOS@0xe8`; `INFO24@0x16ba`). |
| `G[0x29effc]` | 1 |  | 49 | 12..406, 41 distinct | The part's skill; ADDPART marks it known (`ADDPART@0x96`). |
| `GS[0x74e6]` | 1 |  | 111 | 闇衣装解除 / シュバルクライ / エルタットウォルフ | The name. |
| `GS[0x78ce]` | 2 | 0..1 | 111 | 【衣装／アル専用】 / … | The description's two lines. |

### ADDPART, the costumes and DOLL

ADDPART adds parts. 19 scripts call it, among them ALCHEMY, DOLL and RECEIPT. The caller sets the part in
`current_part` `G[0x269882]` and the change in `part_count_change` `G[0x269883]`. It adds the change to
`part_counts` `G[0x16cbb + part]`, at most 999 and with no lower bound (`ADDPART@0x2a9`..`0x2df`), sets `part_obtained_flags` `G[0x1a17ba + part]`, sets
`part_seen_flags` `G[0x1a5252 + part]` in the shared profile and marks the part's skill known. Parts are not a
nation's: there is one count for the game.

A costume is owned once: ADDPART adds 1 only while its count is 0 (`ADDPART@0x16`..`0x6f`). Then it marks its four default parts obtained,
from COINIT `G[0x267c62 + 4 × costume + slot]`, sets two flags from COINIT (`G[0x1a757a + G[0x268c66 + costume]]`,
`G[0x1a7bba + …]` from `G[0x268cca]`), and, if `costume_levels` is 0, initializes the costume's state: the level
`costume_levels` `G[0x161cb + costume]` from COINIT `G[0x267df2]` (1 for all 14), the experience
`costume_experience` `G[0x1622f + costume]` 0, the parts `costume_parts` `G[0x16293 + 4 × costume + slot]` from
COINIT `G[0x267c62]` and the stats `costume_stats` `G[0x16423 + 11 × costume + k]` from COINIT `G[0x2682a2]`
(`ADDPART@0x224`..). COINIT is the costumes' own table, keyed by these ids (troops and battle).

- CALCCOS adds a costume to a character (`CALCCOS@0x0`..`0x2b5`): the character's stats `G[0x10023 + 11 × c + k]`
  plus `costume_stats`, then each of the four `costume_parts`' stats, 3-vector, 射程 and movement type. COINIT gives
  the costume's own 射程 `G[0x267bfe]` and movement type `G[0x267b36]`. It clamps the 3-vector to 0..100 and 射程 to
  0..10, and stats below 0 to 0 except column 6.
- DOLL is the costume screen. It swaps a part into a slot: the new part leaves `part_counts`, the old one returns, and
  `costume_parts` takes the new id (`DOLL@0x8a9`..`0x920`). It draws the level and the experience out of 100
  (`DOLL@0x1a93`..`0x1b20`). The character's costume is `G[0xfc3b + character]` (characters).
- RESULT, after a battle, adds experience to `costume_experience` (`RESULT@0x160d`). At 100, while the level is below
  COINIT's maximum `G[0x267e56 + costume]`, the costume levels up: each stat gains COINIT's growth `G[0x2686ee + 11 ×
  costume + k]` in hundredths, kept in `costume_stat_growth_remainders` `G[0x1686f + 11 × costume + k]`, the whole
  part going into `costume_stats` (`RESULT@0x1622`..`0x16dd`).

### ALINIT (alchemy recipes) and ALCHEMY

Leader `G[0xa2f101]` (the product's type), capacity 2000. 199 recipes, ids 1..706: 196 in the base and 3 new ones
from `$1$ALINIT` (103, 119, 146). ALINIT writes no string; the product's name is ITINIT's or PTINIT's. Its 30
slices agree modulo 2000 from `G[0xa2f101]`, and no cell falls outside them. ALCHEMY reads everything; INFO2 and
INFO27 read the facility mask.

ALCHEMY runs for facility kinds 12 and 13 (the facilities, below); kind 13 is its variant 1, kind 12 its variant 2
(`ALCHEMY@0x131`). It offers a recipe when all of these hold (`ALCHEMY@0xf2e`..`0x1269`), and marks it seen in
`alchemy_recipe_seen_flags` `G[0x1a65da + recipe]`, in the shared profile (`ALCHEMY@0xf59`..`0xf6c`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa2f101]` | 1 |  | 199 | 1..2, 2 distinct 1×103, 2×96 | What the recipe makes: 1 an item (ADDITEM), 2 a part or costume (ADDPART) (`ALCHEMY@0x1388`..`0x13d8`). |
| `G[0xa2f8d1]` | 1 |  | 199 | 1..1366, 182 distinct | The product, an ITINIT or PTINIT id. A key item, or a costume, already held is not offered (`ALCHEMY@0x11df`..`0x1268`); the most to make is 1 for those and 999 less the count otherwise (`ALCHEMY@0x84a`..`0x8e3`). |
| `G[0xa300a1]` | 2 | 0..1 | 199 | 1..10, 3 distinct 1×170, 5×16, 10×13 | The technology gained the first time it is made, added to the nation's `G[0x171b6 + 2 × nation + k]` and kept within 0 and 1000 (`ALCHEMY@0x13db`..`0x1496`). |
| `G[0xa31041]` | 1 |  | 199 | 1..2, 2 distinct 1×69, 2×130 | The facilities that make it, a mask: bit 0 variant 1 (kind 13), bit 1 variant 2 (kind 12) (`ALCHEMY@0xf2e`; `INFO2@0xd71`, `INFO27@0x7cc`). |
| `G[0xa31811]` | 1 |  | 171 | 1..5, 5 distinct | The facility level needed: `facility_level` must reach it (`ALCHEMY@0xf78`). |
| `G[0xa31fe1]` | 1 |  | 59 | 1..41, 29 distinct | A character who must be with the nation (`G[0xf083 + character] & 3` = 3) (`ALCHEMY@0xfd8`..`0x100e`). |
| `G[0xa327b1]` | 1 |  | 45 | 35 | That character's level must reach it (`G[0xf46b + character]`, `ALCHEMY@0x100f`). |
| `G[0xa32f81]` | 1 |  |  | (none written) | An item the nation must hold (`ALCHEMY@0x102c`). |
| `G[0xa33751]` | 2 |  |  | (none written) | Compared with `G[0x8da2b9 + 60 × k + G[0x170a4] mod 60]` (`ALCHEMY@0x1078`). |
| `G[0xa346f1]` | 1 |  |  | (none written) | Must equal `G[0x170a7]` (`ALCHEMY@0x10b9`). |
| `G[0xa34ec1]` | 1 |  |  | (none written) | A region the nation must own (`G[0x3ed38 + r] & 1` and `G[0x3ee00 + r]` = the nation, `ALCHEMY@0x10dd`). |
| `G[0xa35691]` | 2 | 0..1 | 67 | 30..270, 18 distinct | The technology needed: the nation's `G[0x1717a + 2 × nation + k]` must reach both columns (`ALCHEMY@0xf8e`..`0xfd7`). |
| `G[0xa36631]` | 3 | 0..1 | 75 | 7055..8899, 43 distinct | Story flags that must be set, `G[0xc71 + flag − 1]` = 1 (`ALCHEMY@0x1148`..`0x1181`). |
| `G[0xa37da1]` | 3 | 0 | 7 | 9002..9008, 7 distinct | Story flags that must not be set (`ALCHEMY@0x11a2`..`0x11db`): the key items 1..7's flags, so each is made once. |
| `G[0xa39511]` | 1 |  | 199 | 100..50000, 88 distinct | The price of one (`ALCHEMY@0x789`, `@0x1274`). |
| `G[0xa39ce1]` | 4 | 0..3 | 199 | 807..1267, 143 distinct | The four ingredients, ITINIT ids (`ALCHEMY@0x7cb`). |
| `G[0xa3bc21]` | 4 | 0..3 | 199 | 1..50, 18 distinct | How many of each ingredient one takes (`ALCHEMY@0x7b9`, `@0x12c5`). |

Making a recipe the first time sets `alchemy_recipe_made_flags` `G[0x8087 + recipe]` and grants the technology
(`ALCHEMY@0x13db`..`0x13f7`).

### BRINIT (breeding and development) and BREED

Leader `G[0xa24cf0]` (the facility type), capacity 1000. 309 records: 291 in the base and 18 new ones from
`$1$BRINIT` (96..104 and 240..248). The arrays agree modulo 1000 from `G[0xa24520]`, and no cell falls outside them.
BREED reads everything; INFO2 and INFO27 read the result, the type and the parents.

BREED runs for facility kinds 19 and 20: kind 19 is type 1, the 魔物 breeding, kind 20 type 2, the development of
siege weapons (`BREED@0xcd`). For the pair the player chose it takes the first record of its type whose conditions
hold (`BREED@0xe8b`..`0x1113`). Type 1 records with a result breed a new species from two parents (1: アースドラゴン
and 恨みの地震剣 make カミルドラゴン); type 1 records with no result give the second parent up for experience
(145: オーク, 生贄, ※経験値＋３); type 2 records develop a weapon (501: two バリスタ make カタパルト).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa24520]` | 2 | 0..1 | 308 | 15376..15528, 4 distinct | The picture drawn beside the message, column 0 before the combination is done and 1 after (`BREED@0x4347`..`0x4387`, by `breed_done_flags`). |
| `G[0xa24cf0]` | 1 |  | 309 | 1..2, 2 distinct 1×296, 2×13 | The type, which facility it belongs to: 1 kind 19, 2 kind 20 (`BREED@0xea8`). |
| `G[0xa250d8]` | 2 | 0..1 | 306 | 471..907, 173 distinct | The two parents, CHINIT ids; 0 takes any (`BREED@0xef3`..`0xf2c`). |
| `G[0xa258a8]`, `G[0xa26078]`, `G[0xa26848]` | 2, 2, 22 |  |  | (none written) | More tests on the pair (`BREED@0xf2d`..`0xfd7`). |
| `G[0xa2be38]` | 1 |  | 145 | 473..907, 143 distinct | The result, a CHINIT id; 0 for the sacrifices (`BREED@0xfde`, `@0x1124`). |
| `G[0xa2c220]` | 2 |  |  | (none written) | The technology needed, against `G[0x1717a]` (`BREED@0xfec`). |
| `G[0xa2c9f0]` | 3 | 0..1 | 118 | 6931..8936, 10 distinct | Story flags that must be set (`BREED@0x1053`..`0x108c`). |
| `G[0xa2d5a8]` | 3 |  |  | (none written) | Story flags that must not be set (`BREED@0x10ad`). |
| `G[0xa2e548]` | 1 |  |  | (none written) | A cell of `G[0x159fb]` that must be 0 (`BREED@0x10ea`). |
| `G[0xa2e160]` | 1 |  | 50 | 8446..8500, 48 distinct | A story flag set when it is done (`BREED@0x15ef`..`0x1619`); GAMESTART sets it again for combinations the shared profile carries (`GAMESTART@0x894`). |
| `G[0xa2e930]` | 1 |  | 296 | 3..34, 20 distinct | The experience it gives, added to `G[0xf853 + character]` with level-ups at 100 (`BREED@0x1804`..`0x1887`). The sacrifices' messages name it (※経験値＋３ on 3). |
| `GS[0xe0bb]` | 6 | 0..5 | 308 | ふむ……この組み合わせなら、 / … | The message, three lines before it is done and three after (`BREED@0x4329`, line `3 × done + k`). |

A done combination sets `breed_done_flags` `G[0x7c9f + combination]` (`BREED@0x15e3`) and `breed_seen_flags`
`G[0x1a61f2 + combination]` in the shared profile (`BREED@0x1636`..`0x1649`). Breeding takes the first parent from
the nation's stock `G[0x2680a + 1000 × nation + unit]` (`BREED@0x113e`..`0x11cd`).

### The facilities

A town's facility is an object on its region's grid. When the player opens one, REIGN sets `system_mode` `G[0x0]`
to 5, the object in `facility_object` `G[0x70a8]` and its level in `facility_level` `G[0x70a9]`, from
`G[0xb4420 + 400 × region + cell]` (`REIGN@0xab3`..`0xad2`). DEAL then runs the facility kind's script:
`facility_scripts` `G[0x22c3d8 + G[0x8f1152 + object]]`, the kind being OBINIT's (`DEAL@0x9b`..`0xbe`). INIT2
fills kinds 11 to 20 (`INIT2@0xc9`..`0xf6`): 11 PALACE, 12 and 13 ALCHEMY, 14 QUEST, 15 BROTHEL, 16 SHOP, 17 EMPLOY,
18 DOLL, 19 and 20 BREED.

### SHINIT (the shop's and the employment office's offers)

SHINIT runs on demand, through the hook chain `shinit_chain_script_ids` `G[0xa6b9d9]` (slot 0 SHINIT, slot 1
`$1$SHINIT`). SHOP and EMPLOY set `shop_mode` `G[0xa2ed18]` (1 SHOP, items; 2 EMPLOY, characters), clear
`shop_offers` `G[0xa2ed19 + slot]` (1000 cells) and run the chain's 32 slots (`SHOP@0xf3f`..`0xfc6`,
`EMPLOY@0xa39`..`0xa78`). Mode 1 runs `SHINIT@0x0`..`0x5999`, mode 2 `SHINIT@0x59a0`..`0xb715`. Each offer is
guarded: by `facility_level` (`G[0x70a9]` ≥ n), by the region type `G[0x8e0268 + G[0x19a329]]` in 2..5, and by
flags. For example `SHINIT@0x0`..`0x8c` offers item 21 while `G[0x2f12]` is set and item 21's key flag `G[0x2fae]`
is not. The base writes 385 slots (`G[0xa2ed1a`..`0xa2eeca]`, `G[0xa2ef0e]`, `G[0xa2ef0f]`); `$1$SHINIT` writes
slot `G[0xa2ed27]` (item 34, mode 1) and `G[0xa2ee0e`..`0xa2ee10]` (mode 2).

### What is saved

The four boot tables are not saved, and INIT2 writes their strings again on every load. So a changed price, stat,
recipe or combination reaches a loaded game at once. SHINIT runs each time the shop opens, so an offer does too.

These lie below the prefix, so a save keeps them:

- `item_counts` (per nation), the money `G[0x17120 + nation]`, `item_obtained_flags`, `item_seen_flags`, the key
  items' story flags `G[0x2f99 + item]` and the skill flags `G[0x1a770a]`;
- `part_counts`, `part_obtained_flags`, `part_seen_flags`, and the costumes' `costume_levels`, `costume_experience`,
  `costume_parts`, `costume_stats` and `costume_stat_growth_remainders`;
- `alchemy_recipe_made_flags`, `alchemy_recipe_seen_flags`, `breed_done_flags`, `breed_seen_flags`;
- the troops' equipped items `G[0x39fe0]` and `facility_object` and `facility_level`.

`current_item`, `item_count_change`, `current_part`, `part_count_change` and `shop_mode` are arguments, and
`shop_offers` a list SHOP and EMPLOY rebuild. They lie above the prefix.

The shared profile carries some of it into a new game. GAMECLEAR stores the inventory and the obtained flags
(`GAMECLEAR@0x72c`..`0x752`), the costumes and parts (`GAMECLEAR@0x688`..`0x710`) and `breed_done_flags`
(`GAMECLEAR@0x38a`). GAMESTART loads them by the bits of `G[0x63a4]`:

- 0x10 the items (`GAMESTART@0xd56`..`0xdfb`): those of a kind above 1, and the key items with `item_carried_flags`
  set, setting their key flags;
- 0x8 the costumes and parts (`GAMESTART@0xc81`..`0xd53`): a costume's level, experience, parts, stats and
  remainders, and every part's count and obtained flag;
- 0x2, among other things (`GAMESTART@0x731`..`0x8c5`), `breed_done_flags`, setting their story flags again
  (`GAMESTART@0x86e`..`0x8ca`).

Since a costume's state is initialized only when its level is 0, a changed COINIT default reaches only costumes
obtained after it. A loaded or carried costume keeps its stats.

## The strategic map: nations, regions, routes, production, starting units, ruses and quests

INIT2 runs CNINIT, RGINIT, ROINIT, CRINIT, USINIT, RUINIT and QUINIT at every boot and every load, and `$1$AUTORUN`
runs their `$1$` layers after them (`$1$RUINIT` is empty: `$1$RUINIT@0x0` is `exit`). DUINIT is not a boot table:
FIELD walks the hook chain `G[0xa6b9b9]` (slot 1 is `$1$DUINIT`) before each battle (`FIELD@0x342`..`0x377`).

Every integer array of these seven tables lies above the saved prefix (`G ≥ 0x211c08`), so a change to them reaches a
loaded game at once, and only what the game copies out of them is saved ("What is saved" below). Their string
arrays lie above the string prefix (`GS[0x1b1]`) too, so a changed name reaches a loaded game as well.

The scripts pass a record to the shared scripts in one argument cell per table: the nation `nation_arg_id`
`G[0x8df3bf]` (ADDCN, ADDCORP, CHANGELEADER, CALCCN, DELCN, OCCUPY), the region `region_arg_id` `G[0x8e0267]`
(CHECKREG, CALCFORT, SETFORT, UNITEFORT, OCCUPY), the starting unit `starting_unit_arg_id` `G[0x8eabf1]` (ADDCORP),
and the ruse `ruse_arg_id` `G[0xa10eca]` (RUSE, RUSEJUDGE, RUSEACT, EXECRUSE, REPORT), with its target nation
`G[0xa10ecb]`, region `G[0xa10ecc]` and corps `G[0xa10ecd]`. All four lie above the prefix and are not saved.

Story flags are tested as `G[0xc71 + flag − 1] == 1` throughout. A record's "flags required" must all be set, and its
"flags closing" close it when any is set.

### The game start (INITGAME) and the turn

INITGAME builds a new game's map (`INITGAME@0x0`..`0x2a0`):

1. ADDCN for nations 1 to 12, then for each nation 13 to 29 whose `nation_playable_flags` is set, when bit 2 of
   `G[0x63af]` is set or the nation is the player's (`INITGAME@0x54`..`0x8c`). So of the base data, メルキア帝国 (14)
   is never added here, and ハイシェラ魔族国 (16, from `$1$CNINIT`) only under that bit or as the player's nation.
2. CHECKREG, which opens the regions.
3. For each USINIT record 0 to 1999 with a nation and a region above 0, ADDCORP with `nation_arg_id`,
   `region_arg_id` and `G[0x8da4d0]` = −1 (`INITGAME@0x95`..`0xfb`).
4. CHANGELEADER for each active nation, with `nation_start_leaders` in `G[0x22ccff]` and `nation_start_pictures` in
   `G[0x213f00]` (`INITGAME@0x133`, `@0x13f`).
5. Column 0 of the hook chain `G[0xa6b9f9]` (START, `INITGAME@0x257`).
6. When the player is not nation 1, CHANGERELATE sets every other active nation's relation to 4, 敵対
   (`INITGAME@0x1a9`..`0x1b8`).
7. CALCFORT for each region whose `(region_flags & 3) == 3`, then CALCCN for each active nation.
8. Column 2 of `G[0xa6b9f9]` (READY, `INITGAME@0x299`).

`G[0xa6b9f9]` is a `[32][5]` chain: START, CLOSE, READY, READYREG, READYQUEST. `G[0xa6b959]` is a `[32][3]` chain,
RUSEJUDGE, RUSEACT and RUSECHECK, whose `$1$` row is `G[0xa6b95c]`..`G[0xa6b95e]`.

The turn: `turn_week` `G[0x170a4]` is the week, set to 1 by `INIT@0xa` and raised by `ALLMAP@0x248f`.
`turn_nation` `G[0x170a5]` is the nation in turn: 0 between turns, then the player, then each active nation that is
not the player's, in id order (`ALLMAP@0xf85`..`0x1009`; `INIT@0xf` sets 0). `nation_player` `G[0x639f]` is the
player's nation (`INIT` sets 1; read as the player's by SHOP, QUEST, RUSE, PALACE and SHOWREL). At each turn change
ALLMAP clears `quest_battle_counts` (`ALLMAP@0x1025`) and reopens the repeatable quests (`ALLMAP@0x1047`..`0x107f`).

### CNINIT (nations)

Leader `GS[0xa1ff]`, capacity 30. 15 records: 14 in the base (ids 0 to 12 and 14) and 1 from `$1$CNINIT` (16,
ハイシェラ魔族国). Nation 0 is 無所属 (no nation; its colour is 0x979797). The nation id is the record key; ADDCN builds
a nation from it (`nation_arg_id`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8df3c1]` | 1 |  | 15 | 41747..16777215, 15 distinct | The nation's colour, 0xRRGGBB (無所属 0x979797). ALLMAP@0x525c, ARMAMENT@0x3181, DRAWMAP@0x176, DRAWSTATUS@0x1123. |
| `G[0x8df3df]` | 1 |  | 14 | 5000..1000000, 12 distinct | Starting funds: ADDCN@0x47 adds it to the saved `nation_funds`. |
| `G[0x8df3fd]` | 1 |  | 14 | 5000..1000000, 11 distinct | Starting provisions: ADDCN@0x63 adds it to the saved `nation_provisions`. |
| `G[0x8df41b]` | 2 | 0..1 | 12 | 20..400, 14 distinct | Added into the saved `G[0x171b6 + 2n + c]` (ADDCN@0x7c, @0xa4). Not decoded. |
| `G[0x8df457]` | 10 | 0..6 | 14 | 121..1112, 27 distinct | Starting items: ADDCN@0xde..0x115 runs ADDITEM with the item in `G[0x2597db]` and the count in `G[0x2597dc]`. |
| `G[0x8df583]` | 10 | 0..6 | 14 | 10..30, 4 distinct 10×24, 15×2, 20×37, 30×6 | The starting items' counts. |
| `G[0x8e013b]` | 1 |  | 14 | 2..96, 14 distinct | The starting capital, a region id: ADDCN@0x2d copies it to the saved `nation_capitals`. GAMESTART@0x925..0x937 moves the player's corps there. |
| `G[0x8e0159]` | 1 |  | 14 | 1..271, 14 distinct | The starting leader, a character id: ADDCN@0x7 copies it to the saved `nation_leaders`; INITGAME@0x133 passes it to CHANGELEADER. |
| `G[0x8e0177]` | 1 |  | 14 | 17688..16777600, 14 distinct | A picture: ADDCN@0x1a copies it to the saved `nation_pictures`; INITGAME@0x13f passes it to CHANGELEADER. |
| `G[0x8e0195]` | 1 |  | 15 | 17686..16777601, 15 distinct | A picture: ALLMAP@0x644d by `turn_nation`, OUTBREAK@0x314, GAMESTART@0xf09 (set-texture), READFRAME@0x61. Which picture is not decoded. |
| `G[0x8e01b3]` | 1 |  | 15 | 17687..16777602, 15 distinct | A picture: GAMESTART@0xf52 (set-texture). Not decoded. |
| `G[0x8e01d1]` | 2 | 0..1 | 13 | 15658..16777527, 20 distinct | The palace pictures: PALACE@0x13e1 and READPALACE@0x70 by `nation_player`. |
| `G[0x8e020d]` | 1 |  | 14 | 8122..8136, 14 distinct | A story flag RUSE tests for a diplomacy ruse (category 1) against the nation (RUSE@0x126b..0x12a3). Which way it gates is not read. |
| `G[0x8e022b]` | 1 |  | 13 | 1..1, 1 distinct 1×13 | 1 on nations 1 to 12 and 16: INITGAME adds a nation above 12 only when set; GAMESTART@0x1ad..0x1c7 lists the nations that have it (inferred: the nations a player may choose). GAMECLEAR@0x7f3. |
| `G[0x8e0249]` | 1 |  | 12 | 4004..8009, 7 distinct | A story flag GAMECLEAR@0x806..0x830 tests for each nation with `G[0x8e022b]`. Not decoded. |
| `GS[0xa1ff]` | 1 |  | 15 | 無所属 / センタクス領 / キサラ領 | The nation's name (SHOWREL@0x953, RUSE, REPORT and others). |

ADDCN ends by setting bit 0 of the saved `nation_flags` (the nation is active, `ADDCN@0x2a1`). Before that it copies
DPINIT's three `[30][30]` nation matrices ("Battlefields", DPINIT) into the saved `nation_friendship` `G[0x1731e]`,
`nation_relations` `G[0x176a2]` and `G[0x17a26]`, mirroring a relation of 5 or more as 11 − x (5 隷属 ↔ 6 本国,
`ADDCN@0x235`..`0x260`).

Relations are 1 自国, 2 同盟, 3 中立, 4 敵対, 5 隷属 and 6 本国; INIT2 writes their names, `nation_relation_names`
`GS[0xa1c8 + relation]` (`GS[0xa1c9]`..`GS[0xa1ce]`), which SHOWREL@0x96e, @0xa77 and RUSE@0x3b9a draw.

### RGINIT (regions)

Leader `GS[0xa21d]`, capacity 200. 100 records: 94 in the base (ids 1 to 93 and 98) and 6 from `$1$RGINIT` (94 to
97, 99 and 100; 94 is 強欲者の狩場). The region id is the record key; loops run it to below 200. Region 1 is センタクス城,
region 2 東の都センタクス.

Correction to the layout: the tool's `G[0x8e1b6b]` † row is the array `G[0x8e1b68]`, two cells per region
(`FIELD@0x2f68`); `0x8e19d8 + 2 × 200 = 0x8e1b68`. Four arrays have no base data but are read:
`G[0x8e1078]`, `G[0x8e1140]`, `G[0x8e1208]` and `G[0x8e12d0]` `[2]` (rows below the tool's).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8e0268]` | 1 |  | 100 | 1..5, 5 distinct 1×1, 2×14, 3×16, 4×22, 5×47 | The region's kind: 1 only センタクス城; 2 the capitals (東の都センタクス, 北の都キサラ); 3 towns; 4 forts and passes; 5 open land, roads and forests (the meanings are inferred from the names). CALCFORT@0x699..0x6ec adds a percent bonus: +100 for kind 2, +50 for 3, +20 for 4. CHECKREG@0x1d skips a region with 0. RUSEACT@0xc7d. |
| `G[0x8e0330]` | 1 |  | 100 | 1..100, 100 distinct | The region's field map: CHECKREG@0x9d copies it to MPINIT's argument `G[0x8f01e2]` before walking the MPINIT chain `G[0xa6b899]`. Equal to the region id in the data. DELCN@0x93, OCCUPY@0x18c, FIELD@0x3c0, REIGN@0x13e. |
| `G[0x8e03f8]` | 1 |  | 71 | 1..16, 13 distinct | The starting owner nation. CHECKREG@0x5f..0x109 opens an owned region only while its nation is active; SETFORT@0x192..0x1d6 copies it to the saved `region_owners` and the owner's capital to `region_owner_capitals`. RESTRUCT@0x3400, RUSEACT@0x37c, REIGN. |
| `G[0x8e04c0]` | 2 | 0..1 | 99 | 2..5187, 133 distinct | A pair of base values: CALCFORT@0x7bd..0x8c8 computes the saved `G[0xef890 + 2r + c]` from it with the kind's percent bonus (column 0) and `G[0x3f058 + r]` / 100 (column 1), clamped 0..10000. INFO23@0x1750. Not decoded. |
| `G[0x8e0650]` | 2 | 0..1 | 99 | 4..5130, 132 distinct | The same for the saved `G[0xefa20 + 2r + c]`. INFO23@0x1798. Not decoded. |
| `G[0x8e07e0]` | 1 |  | 99 | 139..179562, 97 distinct | The starting population (inferred): SETFORT@0x7 copies it to the saved `region_populations`. INFO23@0x17b0. |
| `G[0x8e08a8]` | 1 |  | 99 | 2500..230000, 45 distinct | The population limit (inferred): RUSEACT@0xcf8..0xd22 caps `region_populations` at it after 流民保護. INFO23@0x17c4, ALLMAP@0x1be4, REIGN@0x3c57. |
| `G[0x8e0970]` | 1 |  | 99 | 10..70, 11 distinct | SETFORT@0x1a copies it to the saved `G[0xeee68 + r]`, which RUSEACT raises to at most 100 (@0x537..0x568) and lowers by 5 (@0xd27..0xd58). INFO23@0x17d8. Not decoded. |
| `G[0x8e0b00]` | 5 | 0..4 | 99 | 2..100, 99 distinct | The production set of each of five kinds: ALLMAP@0x1f92..0x1fab reads CRINIT record 200 × kind + value. Equal to the region id in the data. SHOWCROP@0x5d9. |
| `G[0x8e0ee8]` | 1 |  | 99 | 136..2366, 96 distinct | The x position on the world map (60 uses in ALLMAP; EXECRUSE@0x16b). |
| `G[0x8e0fb0]` | 1 |  | 99 | 46..1366, 91 distinct | The y position on the world map. |
| `G[0x8e1078]` | 1 |  |  | *(no data)* | A character the region needs: CHECKREG@0x12d.. requires it to have joined. |
| `G[0x8e1140]` | 1 |  |  | *(no data)* | That character's minimum of `G[0xf46b + character]` (CHECKREG). |
| `G[0x8e1208]` | 1 |  |  | *(no data)* | An item the player's nation must hold, `G[0x17daa + 2000 × nation_player + item]` (CHECKREG). |
| `G[0x8e12d0]` | 2 |  |  | *(no data)* | Minimums of the player's `G[0x1717a]` pair (CHECKREG@0x1cd). |
| `G[0x8e1460]` | 3 | 0 | 25 | 8302..8927, 23 distinct | Flags required to open the region (CHECKREG@0x212). |
| `G[0x8e16b8]` | 3 | 0 | 2 | 8043..8043, 1 distinct 8043×2 | Flags closing the region (CHECKREG@0x26c). |
| `G[0x8e1910]` | 1 |  | 3 | 18..202, 3 distinct 18×1, 201×1, 202×1 | The DUINIT battle fought at the region: when no battle is set, FIELD@0x1ce..0x1ed makes it `battle_id` if the next two arrays allow. Regions 3, 4 and 18. |
| `G[0x8e19d8]` | 2 | 0..1 | 3 | 12..8302, 4 distinct 12×1, 15×1, 442×1, 8302×3 | Flags required for that battle (FIELD@0x2f0e). |
| `G[0x8e1b68]` | 2 | 1 | 2 | 14..19, 2 distinct 14×1, 19×1 | Flags closing that battle (FIELD@0x2f68). The tool's `G[0x8e1b6b]` †. |
| `G[0x8e1cf8]` | 1 |  | 99 | 18412..16777628, 99 distinct | A picture READMAP@0x7c loads. |
| `G[0x8e1dc0]` | 1 |  | 99 | 19..2258, 97 distinct | An x position: DRAWMAP@0x11b, SETMAP@0x2a, ARMAMENT@0x313f. Which map is not decoded. |
| `G[0x8e1e88]` | 1 |  | 96 | 64..1225, 92 distinct | The matching y (SETMAP@0x3f). |
| `G[0x8e1f50]` | 1 |  | 100 | 18411..16777629, 100 distinct | A picture: ALLMAP@0x55f7, ARMAMENT@0x3015, MARCH@0xb3b, REIGN@0x37ae. |
| `GS[0xa21d]` | 1 |  | 100 | センタクス城 / 東の都センタクス / レイムレス要塞 | The region's name (15 scripts; INFO23@0x1710). |
| `GS[0xa2e5]` | 5 | 0..4 | 99 | メルキア帝国東方、センタクス領の中心 / メルキア帝国とユン・ガソル連合国を隔 / 中原東部の中央に広がる広大な森林地帯 | Five description lines; the fifth is often 【付近生息】. |

CHECKREG opens each region that passes (kind above 0, owner active or none, the four conditions, the flags): it sets
bit 0 of the saved `region_flags` (`CHECKREG@0xeb`) and runs the MPINIT chain for its map. When the owner is the
player and `G[0x1ac52b + r]` is 1, it runs UNITEFORT.

### ROINIT (routes)

Leader `G[0x8e2019]`, capacity 200. 133 records: 125 in the base (ids 1 to 130, with gaps) and 8 from `$1$ROINIT`
(114, 116, 131 to 136). The tool finds 2 writes before a layer's first record. The route id is the record key; no
script reads a route's name.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8e2019]` | 1 |  | 133 | 1..3, 3 distinct 1×112, 2×6, 3×15 | The route's kind: MARCHSEEK@0x31b..0x3a1 lets every corps take kind 1; kinds 2 and 3 need properties of the marching corps (`G[0x22cb1d]`..`G[0x22cb20]`, `G[0x24e812]`); the code tests a kind 4 too. |
| `G[0x8e20e1]` | 2 | 0..1 | 133 | 2..100, 99 distinct | The two end regions (ALLMAP@0x66cb, DRAWMAP@0x566, MARCHSEEK@0x1b6, RUSE@0xd9b). |
| `G[0x8e2271]` | 5 | 0..4 | 125 | 168..2426, 412 distinct | The path's points, x (ALLMAP@0x6778..0x688b). |
| `G[0x8e2659]` | 5 | 0..4 | 125 | 47..1351, 372 distinct | The path's points, y. |
| `G[0x8e2a41]` | 1 |  | 133 | 192..2409, 129 distinct | An x position (DRAWMAP@0x5be). Not decoded. |
| `G[0x8e2b09]` | 1 |  | 133 | 88..1312, 125 distinct | Its y. |
| `G[0x8e2bd1]` | 1 |  | 133 | 18598..16777637, 133 distinct | The route's picture (READMAP@0xc4, SETMAP@0x11b). |
| `G[0x8e2c99]` | 1 |  | 133 | 132..2360, 125 distinct | An x position (DRAWMAP@0x504). Not decoded. |
| `G[0x8e2d61]` | 1 |  | 133 | 38..1300, 124 distinct | Its y. |
| `G[0x8e2e29]` | 3 | 0 | 15 | 24..10000, 7 distinct | Flags required to use the route (MARCHSEEK@0x3f0, SETMAP@0x3a3). |
| `G[0x8e3081]` | 3 | 0..1 | 16 | 7003..8055, 9 distinct | Flags closing the route (MARCHSEEK@0x44a, SETMAP@0x3fd). |
| `GS[0xa6cd]` | 1 |  | 133 | 東の都センタクス～レイムレス要塞 / 東の都センタクス～折玄の森 / 東の都センタクス～城塞都市ヘンダルム | The route's name. No script reads it. |

### CRINIT (production sets)

Leader `G[0x8e32d9]`, capacity 1000. 500 records: 470 in the base and 30 from `$1$CRINIT`. The record id is 200 ×
kind + set, for five production kinds 0 to 4; a region names its set of each kind in `region_production_sets`, and
in the data the set is the region id (records 1..100, 201..300, …, 801..900). The leader holds kind + 1 and no
script reads it. RUINIT's 豊作強化 and 発掘強化 suggest two of the kinds (harvest and excavation). Seven slots per set.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8e32d9]` | 1 |  | 500 | 1..5, 5 distinct 1×100, 2×100, 3×100, 4×100, 5×100 | The kind + 1. No script reads it. |
| `G[0x8e36c1]` | 10 | 0..6 | 453 | -40..100, 26 distinct | The slot's rate: SHOWCROP@0x65f..0x6bc adds the region's saved `G[0xef4a8]` and `G[0xf0830]` and colours the number by its sign. ALLMAP@0x200e..0x20d7. |
| `G[0x8e5dd1]` | 10 | 0..6 | 490 | 100..100, 1 distinct 100×1219 | Always 100 (ALLMAP@0x2062). Not decoded. |
| `G[0x8e84e1]` | 10 | 0..6 | 490 | 1001..1248, 114 distinct | The item the slot yields, an ITINIT id drawn by its name `GS[0x5d76 + item]` (SHOWCROP@0x620, @0x6d0). ALLMAP@0x5e2c..0x5e98. |

### USINIT (starting units)

Leader `G[0x8eb3c2]`, capacity 2000. 862 records: 829 in the base and 33 from `$1$USINIT` (844, 845, 867 to 869,
901 to 928). INITGAME turns each record with a nation and a region into a corps (above); the nations' corps state it
fills (`G[0x35288]`..`G[0x3e054]`, `[30][110]` arrays) are the saved units ("Troops and battle", what is saved).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8eabf2]` | 1 |  | 33 | 1..1, 1 distinct 1×33 | GAMESTART@0xc04. Not decoded. |
| `G[0x8eb3c2]` | 1 |  | 862 | 0..16, 15 distinct | The nation. INITGAME skips a record with 0. |
| `G[0x8ec362]` | 1 |  | 861 | 1..907, 275 distinct | The corps' leader, a character id: ADDCORP@0x2b, and @0x446 copies it into the corps' `G[0x35f6c + 110n + slot]`. |
| `G[0x8ecb32]` | 1 |  | 3 | 138..152, 3 distinct 138×1, 145×1, 152×1 | An item: ADDCORP → `G[0x39fe0]`; SETUN@0x4b8 → `G[0x2597db]`. |
| `G[0x8ed302]` | 1 |  | 821 | 46..726, 124 distinct | The troop kind, a TRINIT id: ADDCORP@0x45d → `G[0x36c50]`; SETUN@0x4a0 → `G[0x24e811]`. |
| `G[0x8edad2]` | 1 |  | 821 | 9..9, 1 distinct 9×821 | The troop count, always 9: → `G[0x37934]`; SETUN@0x4ac → `G[0x24e812]`. |
| `G[0x8ee2a2]` | 1 |  | 514 | 2..100, 95 distinct | The starting region. INITGAME skips a record with 0; ALLMAP@0x444a. |
| `G[0x8eea72]` | 2 | 0..1 | 74 | 8..61, 24 distinct | SETUN@0xdaf puts them in a unit's slots 3 and 4 (inferred: skills). |
| `G[0x8efa12]` | 1 |  | 62 | 1..72, 36 distinct | ALLMAP@0x3345, @0x35ef. Not decoded. |

### RUINIT (ruses, 策略)

Leader `GS[0xb3b1]`, capacity 100. 17 records, all in the base: 1 to 5 (動静偵察, 転移の城門, 守護結界, 豊作強化,
発掘強化), 12 to 14 (戦力補充, 建築物修復, 環境改善), 21 (工兵派遣), 31 (流民保護), 35 (報奨) and 51 to 54, 58, 59
(宣戦の布告, 停戦の申し出, 同盟の破棄, 同盟の申し出, 援助要請, 援助提供). The ruse id is the record key (`ruse_arg_id`).
The tool's two cells outside the arrays, `G[0xa1206c]` and `G[0xa120d0]`, are row 58 of `G[0xa12032]` and
`G[0xa12096]`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa11b82]` | 2 | 0..1 | 17 | 15377..15378, 2 distinct 15377×17, 15378×5 | The result pictures (EXECRUSE@0xc5e, REPORT@0xdca). |
| `G[0xa11cae]` | 1 |  | 17 | 1..5, 5 distinct 1×6, 2×5, 3×3, 4×1, 5×2 | The category: 1 diplomacy (51 to 59, RUSE@0xdf6, @0x126b), 2 for 1 to 5, 3 for 12 to 14, 4 for 21, 5 for 31 and 35. |
| `G[0xa11d12]` | 1 |  | 2 | 1..1, 1 distinct 1×2 | A switch (EXECRUSE@0x6a1). Not decoded. |
| `G[0xa11d76]` | 1 |  | 5 | 1..1, 1 distinct 1×5 | A switch (EXECRUSE@0x71d). Not decoded. |
| `G[0xa11dda]` | 2 | 0..1 | 10 | 200..10000, 9 distinct | The funds cost, lowest and highest: RUSE@0x9f7..0xa31 offers at most the smaller of column 1 and `nation_funds` (流民保護's text: 資金と兵糧は1,000～5,000で設定可能). |
| `G[0xa11ea2]` | 2 | 0..1 | 3 | 1000..5000, 4 distinct 1000×1, 1500×2, 2000×2, 5000×1 | The provisions cost, lowest and highest, against `nation_provisions` (RUSE@0xa39..0xa73). |
| `G[0xa11f6a]` | 2 | 0..1 | 8 | 101..130, 13 distinct | RUSE@0x113c. Not decoded. |
| `G[0xa1215e]` | 2 | 0 | 7 | -100..30, 6 distinct | The friendship change: EXECRUSE@0x871..0x91b adds it to `nation_friendship[target][turn_nation]`, clamps 0..100 and mirrors it. −100 宣戦の布告, +10 停戦の申し出, −30 同盟の破棄, +30 同盟の申し出, ±10 援助. |
| `G[0xa12226]` | 1 |  | 17 | 1..4, 4 distinct 1×7, 2×8, 3×1, 4×1 | What the ruse targets (EXECRUSE@0x10c..0x166): 1 the target nation's capital `nation_capitals`, 2 the region `G[0xa10ecc]`, otherwise the region of the corps `G[0xa10ecd]` (`G[0x3d370]`). |
| `G[0xa1228a]` | 1 |  | 17 | 2..124, 7 distinct | The relations the target may have, bit (1 << relation): 124 any other nation, 2 自国, 16 敵対, 40 中立 or 隷属, 4 同盟, 8 中立, 12 同盟 or 中立. |
| `G[0xa122ee]` | 1 |  | 2 | -1..-1 | Not decoded. |
| `G[0xa123b6]` | 1 |  | 3 | -1..-1 | Not decoded. |
| `G[0xa1241a]` | 1 |  | 2 | 44..49 | RUSE@0x1326. Not decoded. |
| `G[0xa124e2]` | 1 |  | 17 | 931..941, 10 distinct | The animation the ruse plays, an ANINIT id: EXECRUSE@0x1f4 → `G[0xa3e165]`. |
| `GS[0xb3b1]` | 1 |  | 17 | 動静偵察 / 転移の城門 / 守護結界 | The ruse's name (REPORT@0x97b). |
| `GS[0xb415]` | 4 | 0..3 | 17 | 「隠密」持ち部隊を派遣し、他国の情勢 / … | Four description lines (RUSE@0x23c0). |
| `GS[0xb5a5]` | 10 | 0..6 | 17 | 「対象国の情報入手に成功 / … | The result messages (EXECRUSE@0xc2f). |

RUSEJUDGE compares a draw of 0 to 99 with `nation_friendship` (or half of it) plus
`G[0xa10ed8 + 2 + G[0x17a26][turn_nation][target]]` (`RUSEJUDGE@0x539`..`0x570`, `@0x61c`..`0x653`); when the draw
is not below it, it sets bit 1 of `G[0x19a672 + 100 × turn_nation + ruse]` (`RUSEJUDGE@0x577`, `@0x661`). Which
outcome that bit records is not read.

### QUINIT (quests)

Leader `GS[0xb98d]`, capacity 1000. 333 records: 326 in the base (1 to 770, with gaps) and 7 from `$1$QUINIT` (18,
196 to 198, 319 to 321; 18 is 「ターペ＝エトフに侵攻する」). The quest id is the record key; QUEST, CONQUEST, INFO27 and
RECEIPT keep it in a local. QUEST lists and accepts quests, CONQUEST starts a quest's battle, and RECEIPT reports a
done quest and pays it out.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa12546]` | 2 | 0..1 | 333 | 15377..18410, 4 distinct | Pictures (RECEIPT@0x95a). |
| `G[0xa12d16]` | 1 |  | 333 | 1..760, 333 distinct | The list order: CONQUEST@0x50a sorts by it. |
| `G[0xa130fe]` | 1 |  | 262 | 1..6, 6 distinct | Drawn as an icon row (QUEST@0x1c28) and a sort key (QUEST@0x7e4). Not decoded. |
| `G[0xa134e6]` | 1 |  | 84 | 5..40, 25 distinct | Drawn as a number (QUEST@0x1c90) and a sort key (QUEST@0x801). Not decoded. |
| `G[0xa138ce]` | 1 |  | 333 | 1..4, 4 distinct 1×95, 2×75, 3×52, 4×111 | The quest's kind (17 uses). Kind 1 is a battle quest: the 95 quests with `quest_battles` are its 95. Kinds 2 to 4 are not decoded. |
| `G[0xa13cb6]` | 1 |  | 95 | 3..198, 95 distinct | The quest's battle: CONQUEST@0x4cf..0x4d6 makes it `battle_id`, then sets `quest_in_battle`. |
| `G[0xa1409e]` | 1 |  | 150 | 1..1 | Compared with whether the current place's MPINIT kind `G[0x8f1152 + G[0x70a8]]` is not 14 (QUEST@0xb37, RECEIPT@0x3e6). Not decoded. |
| `G[0xa14486]` | 1 |  | 52 | 1..1 | Not decoded. |
| `G[0xa1486e]` | 1 |  | 51 | 1..1 | Repeatable (inferred): each turn ALLMAP@0x1047..0x107f clears bit 0 of a quest's `quest_states` when this is 1 and bits 0 and 3 are set, so it may be accepted again. |
| `G[0xa14c56]` | 1 |  | 19 | 1..1 | Not decoded. |
| `G[0xa1503e]` | 1 |  | 115 | 1..769, 109 distinct | The quest that must come first: QUEST@0xdc5..0xdf6 refuses unless that quest's `quest_states & 0xf` is 0xd. |
| `G[0xa15426]` | 1 |  | 4 | 10..25 | The first week: QUEST@0xdf7 refuses while `turn_week` is below it. |
| `G[0xa1580e]` | 10 | 0 | 7 | 3..9 | Characters the quest needs: each must have `G[0xf083 + character] & 3 == 3` (QUEST@0xe74..0xeb2). |
| `G[0xa17f1e]` | 10 | 0 | 7 | 10..35 | Each one's minimum of `G[0xf46b + character]` (QUEST@0xeb3..0xed3). |
| `G[0xa1a62e]` | 1 |  |  | *(no data)* | An item the player's nation must hold (QUEST@0xed7..0xf05). |
| `G[0xa1aa16]` | 2 |  |  | *(no data)* | Minimums of `G[0x8da2b9 + 60c + turn_week mod 60]` (QUEST@0xf23..0xf60). Not decoded. |
| `G[0xa1b1e6]` | 1 |  |  | *(no data)* | Must equal `G[0x170a7]` (QUEST@0xf64..0xf87). Not decoded. |
| `G[0xa1b5ce]` | 1 |  | 71 | 2..98, 45 distinct | A region that must be open and owned by the player (QUEST@0xf88..0xfd5). |
| `G[0xa1b9b6]` | 2 | 1 | 6 | 60..200 | Minimums of the player's `G[0x1717a]` pair (QUEST@0xe0d..0xe56). |
| `G[0xa1c186]` | 3 | 0..2 | 252 | 20..8931, 109 distinct | Flags required to offer the quest (QUEST@0xfd6..0x102d). |
| `G[0xa1cd3e]` | 3 | 0..2 | 72 | 901..8925, 6 distinct | Flags closing the quest (QUEST@0x1030..0x1087). |
| `G[0xa1d8f6]` | 1 |  | 92 | 1..4, 3 distinct 1×5, 2×67, 4×20 | What the quest asks to deliver (QUEST@0x13ee..0x1471): 2 items (ADDITEM), 4 parts (ADDPART). Kind 1 is not decoded. |
| `G[0xa1dcde]` | 3 | 0 | 92 | 1..1366, 81 distinct | The delivery's ids. |
| `G[0xa1e896]` | 3 | 0 | 92 | 1..1 | The delivery's counts. |
| `G[0xa1f44e]` | 1 |  | 22 | 1..1 | When set, completing the quest takes the delivery from the player's nation (QUEST@0x13b1..0x1471). |
| `G[0xa1fc1e]` | 3 | 0 | 146 | 8446..8930, 146 distinct | Flags required by the check at QUEST@0x10f7. Not decoded what it decides. |
| `G[0xa207d6]` | 3 |  |  | *(no data)* | Flags closing the same check (QUEST@0x116e). |
| `G[0xa2138e]` | 3 | 0..2 | 49 | 461..901, 3 distinct | Flags completing the quest: QUEST@0x1090..0x10f2 sets bit 1 of `quest_states` when one is set. |
| `G[0xa21f46]` | 2 | 0..1 | 227 | 1..4, 3 distinct 1×60, 2×177, 4×7 | The reward's kind (RECEIPT@0xb13..0xc0b): 1 ADDCORP, 2 ADDITEM, 3 and 4 ADDPART. |
| `G[0xa22716]` | 2 | 0..1 | 227 | 35..1267, 94 distinct | The reward's id. |
| `G[0xa22ee6]` | 2 | 0..1 | 227 | 1..20, 6 distinct | The reward's count. |
| `G[0xa236b6]` | 2 | 0 | 245 | 2000..50000, 38 distinct | Money paid: RECEIPT@0xc0e..0xc6c adds it to `nation_funds`. |
| `G[0xa23e86]` | 1 |  | 149 | 7902..8803, 138 distinct | A story flag RECEIPT@0xc89..0xc9e sets on payment. |
| `GS[0xb98d]` | 1 |  | 333 | 「戦鬼と魔物討伐」 / 「折玄の森調査」 / 「魔銀結晶入手」 | The quest's name. |
| `GS[0xbd75]` | 1 |  | 333 | ガルムス / ガルムス / 国任務 | Who asks (国任務 for the nation's own). |
| `GS[0xc15d]` | 2 | 0..1 | 323 | 2,000　ルドラ / 6,000　ルドラ / 30,000　ルドラ | The reward as text. |
| `GS[0xc92d]` | 2 | 0..1 | 333 | 貴様が奪還した地で、強力な魔物が出没 / … | The request's text. |
| `GS[0xd0fd]` | 4 | 0..3 | 333 | この特別任務を達成したので以下の / … | The result messages (RECEIPT@0x8d1). |

### DUINIT (battle setups, on demand)

DUINIT is a chain of 115 branches on `battle_id` `G[0xc077]` (0 to 0x26, 0x33 to 0x4b, 0x52 to 0x66, 0x79 to 0x85,
0x97 to 0xa1, 0xbf to 0xc3, 0xc9, 0xca), and `$1$DUINIT` adds 0x27 and 0xc4 to 0xc6 (`$1$DUINIT@0x0`: 0x27 is
ターペ＝エトフ). FIELD clears the battle's texts (`FIELD@0x2c1`..`0x2ff`), zeroes ranges of the battle cells (`FIELD@0x302`..`0x33d`, op zero-int-range),
then walks `G[0xa6b9b9]`, 32 slots (`FIELD@0x342`..`0x377`). A branch writes scalars of the battle setup: the place's
name `GS[0x1a9]` (東の都センタクス), the two sides' three end conditions `GS[0x1aa + 3 × side + k]` (防衛部隊の全滅,
防衛側帰還拠点・本城の破壊, 侵攻部隊の全滅, 侵攻側帰還拠点の破壊, 制限時間１００Ｃの経過; FIELD@0x2ec and OUTBREAK@0x529 read
them as `[2][3]`), and `G[0xc07c]`..`G[0xc0c1]`, `G[0xc4b9]`..`G[0xc538]` and `G[0x211800]`, the battle's own cells
("Battlefields"). Story scenes set `battle_id` directly (17 scripts); otherwise FIELD takes the region's
`region_battles`, and CONQUEST a quest's `quest_battles`. No DUINIT write is a record array.

### What is saved

Every array of the seven tables lies above the prefix and is not saved. The game copies these out of them into saved
state, below the prefix:

- **Nations**, `[30]` arrays: ADDCN copies `nation_start_capitals` into `nation_capitals` `G[0x170c6]`,
  `nation_start_leaders` into `nation_leaders` `G[0x170e4]` and `nation_start_pictures` into `nation_pictures`
  `G[0x17102]`, and adds `nation_start_funds` to `nation_funds` `G[0x17120]`, `nation_start_provisions` to
  `nation_provisions` `G[0x1713e]` and `G[0x8df41b]` to `G[0x171b6]`. It runs once per nation, at INITGAME or when a
  scene adds a nation, so a changed starting value reaches only nations added after it. The items go through ADDITEM
  into the saved item counts `G[0x17daa + 2000 × nation + item]` ("Items, parts and facilities"). ADDCN also sets bit 0 of `nation_flags` `G[0x170a8]`
  and copies DPINIT's matrices into `nation_friendship` `G[0x1731e]`, `nation_relations` `G[0x176a2]` and
  `G[0x17a26]` (`[30][30]`). Other saved nation cells, not decoded: `G[0x1715c]`, `G[0x1717a]` `[2]` (260 uses),
  `G[0x171f2]`, `G[0x17210]`, `G[0x1722e]` `[6]`, `G[0x172e2]`, `G[0x17300]`.
  The nations' corps (`G[0x35288]`..`G[0x3e054]`), filled from USINIT by ADDCORP, are saved too: the saved units of
  "Troops and battle".
- **Regions**, `[200]` arrays: CHECKREG sets bit 0 of `region_flags` `G[0x3ed38]` (open); SETFORT and UNITEFORT set
  bit 1 (`SETFORT@0x1fc`, `UNITEFORT@0x15f`), RUSEACT bit 8 (`RUSEACT@0xf6`). When the starting owner is active,
  SETFORT copies `region_start_owners` into `region_owners` `G[0x3ee00]` and the owner's capital into
  `region_owner_capitals` `G[0x3ef90]` (`SETFORT@0x192`..`0x1d6`); it always sets the corps slot
  `region_corps` `G[0x3eec8]` to −1 and `G[0xf0380]` to 0, and copies `region_start_populations` into
  `region_populations` `G[0xeeda0]` and `G[0x8e0970]` into `G[0xeee68]`. OCCUPY writes the new owner the same way
  (`OCCUPY@0x1d3`..`0x207`). CALCFORT recomputes `G[0xef890]` and `G[0xefa20]` `[2]` from the live base values on every
  run, so a changed base value reaches a loaded game at the next CALCFORT. Not decoded: `G[0x3f058]`, `G[0xef0c0]` and
  `G[0xef4a8]` `[5]`, `G[0xefbb0]` `[6]`, `G[0xf0060]`..`G[0xf1000]`. The `[200][400]` grids `G[0xa0ba0]`,
  `G[0xb4420]`, `G[0x122380]`, `G[0xc7ca0]` and `G[0xdb520]`, which SETFORT fills from MPINIT, are the region map layers of "Battlefields".
- **Quests**, `[1000]` arrays: `quest_states` `G[0x70ab + quest]`, bits: 0 accepted (QUEST@0x777 writes 1), 1 done by
  a flag (QUEST@0x10ed), 2 done by its battle or delivery (FIELD@0xf2a8, QUEST@0x147e), 3 paid out (RECEIPT@0xcaa).
  RECEIPT pays a quest with bit 1 or 2 and without bit 3 (`RECEIPT@0x402`..`0x433`). `quest_in_battle` `G[0x70aa]` is
  the quest whose battle runs (CONQUEST@0x4ea; FIELD@0xf28c..0xf2a8 sets its bit 2 when `G[0xc078]` equals
  `G[0xc535]`). `quest_battle_counts` `G[0x787b + quest]` counts the battles started this turn (CONQUEST@0x4ef..0x4fd;
  ALLMAP@0x1025 clears it).
- **The turn**: `turn_week`, `turn_nation`, `nation_player` and `G[0x170a6]`.
- **Ruses**: nothing of RUINIT. RUSEJUDGE's outcomes `G[0x19a672]` `[30][100]` are saved.
- **Routes and production**: nothing. MARCHSEEK, SETMAP, ALLMAP and SHOWCROP read ROINIT and CRINIT live.
- **DUINIT**: its cells lie below the prefix, so a save holds the last battle's setup; FIELD writes them again before
  each battle.

## Battlefields

Every region has a map of up to 20 × 20 cells. A battle (FIELD, `system_mode` 3) is fought on it, and in the reign
mode (REIGN, mode 4) the player builds on it. Four tables describe it:

- LAINIT holds the land types.
- OBINIT holds the map objects: buildings, nature, bridges, barriers, walls, traps and the scenery.
- DPINIT holds the object kinds, plus several other constants.
- MPINIT holds the maps. It runs on demand and writes one map at a time.

The region's map state is saved in five layers of 200 regions × 400 cells. SETFORT fills the layers from MPINIT the
first time a region is set up. SETFIELD builds the battlefield from them each time a field is entered.

The runtime grids are 24 × 24, with a 2-cell border. A cell is `[row + 2][column + 2]`, where row and column count
from 0. A map array is `20 × row + column`.

A unit moves on either of two movement types, `G[0xd021 + 2 × unit + k]` for k = 0, 1. The values are 1..3, named by
INIT2's `GS[0xa1cf + type]`: 地上, 飛行, 潜水 (`INIT2@0x222`; INFO25@0x1a1d reads `GS[0xa1cf + 1 + k]`).

A passability value is one of three:

| Value | Meaning |
|---|---|
| 0 | the type cannot enter |
| 1 | the type enters at full speed |
| 2 | the type enters at the cell's speed percent |

### LAINIT (land types)

Leader `GS[0xa795]` (the name), capacity 100. There are 14 land types, ids 1..14, all in the base; `$1$LAINIT` writes
nothing. Every array lies 100 cells from the next, so the bases agree modulo the capacity:

- `0x8f0728 + 100 = 0x8f078c`
- `0x8f078c + 300 = 0x8f08b8`
- `0x8f08b8 + 100 = 0x8f091c`

The cell after the last array, `G[0x8f0980]`, is FIELD's probe object. `GS[0xa795] + 100 = GS[0xa7f9]` is OBINIT's
names.

The land types are:

| Id | Name | Id | Name |
|---|---|---|---|
| 1 | 地面 | 8 | 氷 |
| 2 | 地面 | 9 | 溶岩 |
| 3 | 不可 | 10 | 堀 |
| 4 | 道 | 11 | 湿地 |
| 5 | 沼地 | 12 | 谷（崖） |
| 6 | 川・湖 | 13 | 空 |
| 7 | 深水 | 14 | 砂地 |

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8f0728]` | 1 |  | 2 | 2..3, 2 distinct 2×1, 3×1 | Land 1 → 3, land 2 → 2, the others 0. REIGN copies it into its placement grid (local `0x7ab`) for each cell of an object's footprint (`REIGN@0x1cfc`, `@0x2832`). Not decoded further. |
| `G[0x8f078c]` | 3 | 0..2 | 14 | 0..2, 3 distinct 0×12, 1×22, 2×8 | Passability by movement type: `3 × land + type − 1` (`FIELD@0x3b53`; also ADDUN, CHANGEFORM, CRAFTACT, DISPOSE, EXECSKILL, MVSEEK, ORGANIZE, REIGN, SETENTRY, SETROUTE). 不可 is [0,0,0]; 溶岩 is [0,2,0]; 空, 堀 and 谷 are [0,1,0]. REIGN's placement test reads the 地上 value (`REIGN@0x2a46`). |
| `G[0x8f08b8]` | 1 |  | 14 | 50..100, 5 distinct 50×3, 60×1, 70×1, 80×1, 100×8 | Speed percent on a value-2 cell (`FIELD@0x3bf1`; 8 scripts). 沼地, 川・湖 and 溶岩 50; 氷 60; 湿地 70; 砂地 80. |
| `G[0x8f091c]` | 1 |  | 14 | 2..7, 5 distinct 2×5, 4×3, 5×1, 6×4, 7×1 | No script reads it. |
| `GS[0xa795]` | 1 |  | 14 | 地面 / 地面 / 不可 | The name. No script outside LAINIT reads it. |

### OBINIT (map objects)

Leader `GS[0xa7f9]` (the name), capacity 1000. There are 280 objects, ids 1..951: 279 in the base and one in
`$1$OBINIT`. The bases agree modulo 1000, and the per-level arrays have 10 cells an object, one a level (0..9):

- `0x8f0982 + 1000 = 0x8f0d6a`
- `0x8f4bea + 500 000 = 0x96ed0a`
- `0x96ed0a + 30 000 = 0x97623a`
- `GS[0xa7f9] + 1000 = GS[0xabe1]`

The readers loop 1000 objects (`SELOBJ@0x7cc`).

Most objects use level 1. A building of several levels uses 1..5. Level 0 is the ruin or unbuilt state that nature,
bridges and barriers have, and SETFORT places it.

The kinds, with their counts:

| Kind | Name | Objects |
|---|---|---|
| 1 | 帰還拠点 | 2 |
| 2 | 自然物 | 75 |
| 3 | 橋梁 | 8 |
| 4 | 結界 | 8 |
| 5 | 防壁 | 14 |
| 6 | 宝物 | none |
| 7 | 配置 | 111: churches, houses, ruins |
| 8 | 防柵 | 14 |
| 9 | 罠 | 12 |
| 10 | 演出 | 12: scenery such as 本城, 正門, 天井 |
| 11 | 本城 | 13 |
| 12 | 兵器研究所 | 2 |
| 13 | 魔術研究所 | 2 |
| 14..20 | the other facilities | one each |

Each object has two description lines. The first is 【class】 and a description, and the second is 【性能】 or 【効果】
with grades.

Examples:

- **1 帰還拠点** has kind 1, size 1 × 1 and sort key 2001. At level 1, its stats +1, +3 and +5 are 500, 20 and 20,
  and its picture is 0x4694.
- **11 木Ｂ** has kind 2 and sort key 111. Its level-0 durability is 5. Level 1 costs 100, takes 1 week, and needs
  materials 1111 and 1051.
- **109 槍陥穽** is a level-1 trap. Its stat +2 is 20, its element is 2, its speed is 60% and its passability is
  [2, 1, 2].

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x8f0982]` | 1 |  | 280 | 110..9510, 272 distinct | The sort key of the book and the build list (`INFO25@0x94a`, `SELOBJ@0x609`). |
| `G[0x8f0d6a]` | 1 |  | 232 | 1..1, 1 distinct 1×232 | 1 when the facility book lists it (`INFO25@0x97c`, `INFO2@0x878`). SETFIELD records the levels seen only of these (`SETFIELD@0x1f8`). |
| `G[0x8f1152]` | 1 |  | 280 | 1..20, 19 distinct | The kind: an id of DPINIT's kinds (33 scripts; `SETFIELD@0x2f1`, `FIELD@0x3a73`). |
| `G[0x8f153a]` | 1 |  | 280 | 1..3, 3 distinct 1×146, 2×111, 3×23 | The width in columns (`REIGN@0x1c64`, `DRAWOBJ@0x136`, `INFO25@0x16ce`). |
| `G[0x8f1922]` | 1 |  | 280 | 1..3, 3 distinct 1×146, 2×111, 3×23 | The depth in rows (`SETFIELD@0x258`, `@0x27c`; `REIGN@0x1c40`, `@0x1eda`; `DRAWOBJ@0xfe`). |
| `G[0x8f1d0a]` | 1 |  | 133 | 1..9, 3 distinct 1×18, 4×94, 9×21 | Values 1, 4 or 9 that CALCFORT adds into the region's `G[0xf0060 + region]` per built object (`CALCFORT@0x222`). Not decoded. |
| `G[0x8f20f2]` | 1 |  | 13 | 1..1, 1 distinct 1×13 | 1 on objects 301..313. FIELD@0xa0c4, RTN_M037 and RTN_M054 read it. Not decoded. |
| `G[0x8f24da]` | 10 | 0..2 | 211 | 1..1, 1 distinct 1×224 | 1 for a level drawn on the ground layer: graphic element `0x5dc + cell`, under the units. Other objects are depth-sorted at `0x7d0 + 500 × (row + depth − 1) + column` (`DRAWOBJ@0xd6`..`0x12f`). |
| `G[0x8f4bea]` | 500 | 62..99 | 3 | 1..1, 1 distinct 1×49 | The races a barrier bars, `500 × object + 50 × level + race`, with 50 race codes. Objects 116 光結界, 117 暗黒結界 and 119 魔焔結界 set them at level 1. For example, 光結界 sets 21 and 33..49, which its description calls 死王・悪魔・不死・霊体. FIELD tests the unit's `G[0xd0c1 + 2 × unit + k]`: a set cell bars the cell (`FIELD@0x3ad6`..`0x3b06`). 118 飛行結界 bars by passability instead ([1, 0, 1]). |
| `G[0x96ed0a]` | 30 | 0..8 | 228 | 0..2, 3 distinct 0×73, 1×662, 2×24 | Passability by level and movement type, `30 × object + 3 × level + type − 1` (`FIELD@0x3b2e`; `INFO25@0x1a39` draws ○ or × for each type; 12 scripts). |
| `G[0x97623a]` | 10 | 0..1 | 15 | 40..85, 6 distinct 40×1, 50×4, 60×3, 70×1, 80×7, 85×2 | Speed percent by level on a value-2 cell (8 scripts, those that read the land's). |
| `G[0x97894a]` | 30 | 0..11 | 34 | 1..1, 1 distinct 1×153 | 30 flags an object. The traps set 3 and 5. Only FIELD reads it. Not decoded. |
| `G[0x97fe7a]` | 1 |  | 280 | 4..60, 6 distinct 4×16, 12×14, 16×1, 28×3, 48×2, 60×244 | A bit mask over the region's type `G[0x8e0268 + region]`. When the bit is clear, the build list marks the object not buildable here (bit 1, `SELOBJ@0x9a0`..`0x9c8`). |
| `G[0x980262]` | 1 |  | 16 | 86..221, 5 distinct 86×4, 218×4, 219×3, 220×4, 221×1 | The one region the object can be built in. Elsewhere the list marks bit 2 (`SELOBJ@0x9c9`..`0x9f1`). |
| `G[0x980a32]` | 1 |  | 53 | 25..67, 11 distinct | A region the player must own. Unless bit 0 of `G[0x3ed38 + it]` is set and its owner `G[0x3ee00 + it]` is the player's nation `G[0x639f]`, the list hides the object (`SELOBJ@0x867`..`0x8b4`). |
| `G[0x980e1a]` | 1 |  | 7 | 12..25, 4 distinct 12×1, 13×1, 15×1, 25×4 | An index into `G[0xf083]`. The list hides the object unless that cell's bits 0 and 1 are both set (`SELOBJ@0x8b5`..`0x8e6`). Not decoded. |
| `G[0x981202]` | 2 | 0..1 | 19 | 30..160, 7 distinct | Two minimums. The list hides the object while the nation's `G[0x1717a + 2 × nation + k]` is below them (`SELOBJ@0x81d`..`0x866`). Not decoded. |
| `G[0x9819d2]` | 3 | 0..2 | 253 | 19..10000, 29 distinct | Up to three required flags. Each nonzero v needs `G[0xc71 + v − 1]` = 1, or the list hides the object (`SELOBJ@0x904`..`0x93d`). The objects marked 建築不可 hold 10000. |
| `G[0x983142]` | 10 | 0..5 | 137 | 1..6, 6 distinct 1×82, 2×53, 3×49, 4×62, 5×31, 6×17 | A grade by level. The book shows it as INIT2's letter `GS[0xa1d9 + v]` (Ｅ Ｄ Ｃ Ｂ Ａ Ｓ Ｍ, `INFO25@0x16ff`..`0x1711`). What it grades is not decoded: it does not follow the 耐久 of the description. |
| `G[0x985852]` | 10 | 1..2 | 89 | -1..2, 3 distinct -1×66, 1×37, 2×3 | By level. 16 scripts read it (FIELD, ADDUN, ALLMAP, the RTN_M036..M040 routines). Not decoded. |
| `G[0x987f62]` | 110 | 1..60 | 271 | 2..1000, 92 distinct | Eleven stats by level, `110 × object + 11 × level + s`. See the table below. |
| `G[0x9a2d12]` | 10 | 0..3 | 8 | 1..4, 4 distinct 1×12, 2×6, 3×3, 4×2 | CRAFTACT and FIELD read it. Not decoded. |
| `G[0x9a5422]` | 10 | 0..3 | 24 | 10..400, 17 distinct | The traps and a few others: 槍陥穽 200, 炎陥穽 and 毒陥穽 20. FIELD, REIGN and SELOBJ read it. Not decoded. |
| `G[0x9a7b32]` | 10 | 0..3 | 24 | 1..9, 8 distinct | A trap's element by level: 槍陥穽 2 (物理), 炎陥穽 5 (火炎), 毒陥穽 9 (暗黒). FIELD scales the trap's power by `G[0x8da340 + 20 × class + element]` of the unit's class `G[0xd099 + 2 × unit + k]`, then divides by 100 (`FIELD@0x91a2`..`0x9208`). |
| `G[0x9aa242]` | 20 | 2..11 | 11 | 1..80, 19 distinct | By level, two cells; CALCFORT sums it into `G[0x135c00]` (2 cells a region). The book shows "NＰ（追加NＰ）" with `G[0x9af062]` (`INFO25@0x1745`..`0x17a1`). |
| `G[0x9af062]` | 20 | 2..11 | 9 | 1..8, 7 distinct | The "追加" part of the line above. CALCFORT sums it into `G[0x135c00]`. |
| `G[0x9b3e82]` | 20 | 2..11 | 114 | 4..650, 62 distinct | By level, two cells; CALCFORT sums it into `G[0xef890]` (`INFO25@0x17c9`). |
| `G[0x9b8ca2]` | 20 | 2..11 | 82 | 1..300, 29 distinct | By level, two cells; CALCFORT sums it into `G[0xefa20]` (`INFO25@0x1847`). |
| `G[0x9bdac2]` | 50 | 5..29 | 28 | 1..3, 3 distinct 1×28, 2×10, 3×5 | By level, five cells; CALCFORT sums it into `G[0xef0c0]`. The book shows "+N回" (`INFO25@0x1995`). |
| `G[0x9c9e12]` | 50 | 5..29 | 8 | 1..50, 14 distinct | By level, five cells; CALCFORT sums it into `G[0xef4a8]`. The book shows "+N％" (`INFO25@0x1942`). |
| `G[0x9e4bc2]` | 10 | 1..5 | 68 | 4..61, 37 distinct | By level; CALCFORT sums it into `G[0x3f058]` (`INFO25@0x18be`). |
| `G[0x9e72d2]` | 10 | 0..5 | 155 | -20..30, 19 distinct | By level; CALCFORT sums it into `G[0xf0128]`, clamped to 0..100 (`INFO25@0x18e8`). |
| `G[0x9e99e2]` | 10 | 1 | 156 | 1..5, 5 distinct 1×118, 2×27, 3×8, 4×2, 5×1 | Weeks to build level 1. REIGN copies it into the region's weeks-left layer (`REIGN@0x20b2`..`0x20c8`). |
| `G[0x9ec0f2]` | 10 | 1..5 | 217 | 100..50000, 56 distinct | The money cost by level. REIGN checks it against the nation's funds `G[0x17120 + nation]` ("資金が不足しています", `REIGN@0x1dc3`) and deducts it (`REIGN@0x1fbd`). SELOBJ marks bit 3 when level 1 costs more (`SELOBJ@0x9f2`). |
| `G[0x9ee802]` | 40 | 0..24 | 236 | 50..1221, 66 distinct | Up to four material item ids by level, `40 × object + 4 × level + k`. Compared with the nation's stock `G[0x17daa + 2000 × nation + item]` ("資源が不足しています", `REIGN@0x1e42`; `SELOBJ@0xa55`, bit 4). ADDITEM with a negative count consumes them (`REIGN@0x2052`..`0x208c`). Level 2's column 0 holds 100 on the traps (not decoded). |
| `G[0x9f8442]` | 40 | 4..22 | 217 | 1..60, 20 distinct | The amounts of those materials (`SELOBJ@0xa3c`, `REIGN@0x1e42`). |
| `G[0xa02082]` | 40 | 0..23 | 201 | 1..12, 9 distinct | Beside each material (`REIGN@0x1321`). Not decoded. |
| `G[0xa0bcc2]` | 10 | 0..5 | 202 | 10..1000, 16 distinct | By level (`REIGN@0x124d`). The traps hold 100. Not decoded. |
| `G[0xa0e3d2]` | 1 |  | 4 | 8502..8505, 4 distinct 8502×1, 8503×1, 8504×1, 8505×1 | Objects 327..330. Building one sets the flag `G[0xc71 + v − 1]` (`REIGN@0x1f8e`). |
| `G[0xa0e7ba]` | 10 | 0..5 | 280 | 18068..16777616, 338 distinct | The picture by level. READFIELD loads it for each anchor cell (`READFIELD@0xf9`). |
| `GS[0xa7f9]` | 1 |  | 280 | 帰還拠点 / 帰還拠点 / 木Ｂ | The name (`FIELD@0xe1b3`; the book, the build list). |
| `GS[0xabe1]` | 2 | 0..1 | 245 | 【重要拠点／建築不可】部隊を帰還させ / 【重要拠点／建築不可】部隊を帰還させ / 【自然／建築不可】一般的な木 | The two description lines. |

OBINIT writes `G[0x8f4bea]` and `G[0x987f62]` through local pointers, so their cells follow no record's leader:
the tool's leader declares them, and `G[0x9c9e12]` too, whose eight records fit stride 48 as well as 50. A
third array of that shape, `G[0x9d6162]` (60 cells an object, six a level), is read by CALCFORT, which sums it
into `G[0xefbb0]` (`CALCFORT@0x3f4`), but no record writes it.

The stats of `G[0x987f62]`:

| Stat | Holds |
|---|---|
| +1 | The durability. SETFORT copies it into the region's durability layer (`SETFORT@0x13e`). 18 scripts read it. |
| +2 | A trap's power (`FIELD@0x91a2`). CRAFTACT, REIGN and SELOBJ also read it. |
| +3, +5 | The object's two defences. FIELD subtracts them from the attacking unit's `G[0xed63 + 33 × unit + 11 × X + 2]` and `+ 4` (`FIELD@0x846c`, `@0x84bd`). REIGN@0x473b, RUSEACT@0xa57 and SELOBJ@0x1d37 also read them. |

The facility book (INFO25) shows several fields of an object:

- the kind's tab (`INFO25@0x996`);
- width × depth (`@0x16ce`);
- the grade (`@0x170a`);
- the summed effects above;
- ○ or × per movement type (`@0x1a1d`..`0x1a55`).

### DPINIT (object kinds and other constants)

Leader `GS[0xf82b]` (the kind's name), capacity 100. There are 20 kinds, ids 1..20, and the kind arrays lie 100 apart.
DPINIT also writes three other groups of constants, listed after the table.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa3db61]` | 1 |  | 20 | 10..100, 20 distinct | An order value, 10 × kind. No script reads it. |
| `G[0xa3dbc5]` | 1 |  | 20 | 6..45, 6 distinct 6×1, 41×4, 42×1, 43×2, 44×10, 45×2 | The kind's icon in a sheet 10 icons wide, each 19 pixels: column `(v − 1) mod 10`, row `(v − 1) div 10` (`SELOBJ@0x15eb`..`0x1631`). |
| `G[0xa3dc29]` | 1 |  | 14 | 1..4, 4 distinct 1×1, 2×2, 3×10, 4×1 | The facility book's tab: 1 自然, 2 仕掛, 3 施設, 4 その他. The book's 公共 tab is 0 (`INFO25@0x996` against `G[0x22c31e]`). SELOBJ lists only kinds with a tab (`SELOBJ@0x7e1`). Tab 2 uses bit 2 of the placement mask (`REIGN@0x1eb0`). |
| `G[0xa3dc8d]` | 1 |  | 20 | 1..10, 10 distinct | The class. Its name is INIT2's `GS[0xa1e0 + class]`; see the class list below. FIELD tests whether a unit may act on an object with `check-bit G[0xe15b + unit]` by class (`FIELD@0x4daa`..`0x4df7`). 37 reads: ADDUN, FIELD, MVSEEK, the RTN_M036..M040 routines. |
| `G[0xa3dcf1]` | 1 |  | 10 | 8044..8053, 10 distinct | Kinds 11..20, the facilities: a flag. When `G[0xc71 + v − 1]` = 1, REIGN refuses the kind (`REIGN@0x2d47`..`0x2d86`). |
| `GS[0xf82b]` | 1 |  | 20 | 帰還拠点 / 自然物 / 橋梁 | The kind's name: 帰還拠点, 自然物, 橋梁, 結界, 防壁, 宝物, 配置, 防柵, 罠, 演出, 本城, 兵器研究所, 魔術研究所, 斡旋所, 娼館, 商会, 商会, 魔導巧殻研究所, 魔物配合研究所, 兵器開発所 (`INFO25@0x9ab`). |
| `GS[0xf88f]` | 1 |  | 20 | なし / なし / なし | The facility's command: なし for kinds 1..10, then 城内視察, 兵器研究, 魔術研究, 討伐, 娼館, 売買, 売買, 魔導巧殻, 魔物配合, 兵器開発. |

The classes, named by INIT2's `GS[0xa1e0 + class]`:

| Class | Name | Kinds |
|---|---|---|
| 0 | 城門 | a 防壁 on 道; FIELD sets class 0 there (`FIELD@0x4dc4`..`0x4deb`) |
| 1 | 城壁 | 防壁 |
| 2 | 結界 | 結界 |
| 3 | 橋 | 橋梁 |
| 4 | 拠点 | 帰還拠点, 本城 |
| 5 | 施設 | 12..20 |
| 6 | 防柵 | 防柵 |
| 7 | 自然 | 自然物 |
| 8 | 他 | 宝物, 配置 |
| 9 | 罠 | 罠 |
| 10 | 演出 | INIT2 sets no name for it |

DPINIT's other constants, all read on the strategic map ("The strategic map"):

- **The nation matrices.** Three 16 × 16 matrices of stride 30: `G[0x8df6af]`, `G[0x8dfa33]` and `G[0x8dfdb7]`.
  ADDCN reads them by `G[0x8df3bf]` (`ADDCN@0x12e`, `@0x197`), and `$1$DPINIT` adds rows.
- **CALCFORT's thresholds.** `G[0xa3dd55 + i]` (5000..200000) and `G[0xa3dd5f + i]` (20..200), read against
  `G[0xeeda0 + region]` (`CALCFORT@0x635`, `@0x64a`).
- **Tables by nation.**
  - `G[0xa3dd69]`, stride 2 (`FIELD@0xf553`).
  - `G[0xa3dfdf]` and `G[0xa3dffd]` (`ALLMAP@0x2ff8`, `@0x3015`).
  - `G[0xa3e0ed]`, stride 4 (`SUPPLE@0x65`).
- **Tables by `G[0x170a5]`, read by ALLMAP.**
  - `G[0xa3dda5]`, `G[0xa3ddc3]` and `G[0xa3dde1]`.
  - `G[0xa3ddff]`, stride 4.
  - `G[0xa3de77]`, stride 3.
  - `G[0xa3ded1]`, stride 4.

### MPINIT (battlefield maps)

MPINIT runs on demand, through the hook chain `G[0xa6b899]`:

- INIT2 puts MPINIT (0x4bfe) in slot 0 (`INIT2@0x7e`).
- `$1$AUTORUN` puts `$1$MPINIT` (0x1000119) in slot 1 (`$1$AUTORUN@0x7b`).

A walker sets the selector `G[0x8f01e2]` to a map id and calls every slot (`FIELD@0x806`..`0x840`). Each script
tests `eq G[0x8f01e2] N` for each of its maps and writes only constants. CHECKREG and DELCN also set it and walk the chain
(`CHECKREG@0xa4`, `@0xc6`; `DELCN@0x9a`, `@0xbc`). OCCUPY, PALACE and REIGN also set the selector.

The maps:

- The base has 114: ids 1..0x5d, 0x62 and 0xc9..0xdc.
- `$1$MPINIT` adds 7: 0x5e..0x61, 0x63, 0x64 and 0xdd.

A map row is one inline array of 20 cells (`MPINIT@0x22`..). MPINIT clears nothing, so a cell outside the map's size
keeps the previous map's value. SETFIELD and SETFORT read only rows × columns.

Map 1 is a test map: all land 1, with 9 entries. Map 2 is 20 × 20, with bounds 1..17 × 1..17, picture 17956 and 6
entries. Maps 0xc9..0xdd are the story battles: DUINIT's branches set them (`DUINIT@0x22f`, `$1$DUINIT@0xe`). Map 201
has bounds 0, 0, 0, 0.

| cell | holds |
|---|---|
| `G[0x8f01e2]` | The selector, the map to write. |
| `G[0x8f01e3 + 20 × row + column]` | The land, a LAINIT id; 400 cells. |
| `G[0x8f0373 + 20 × row + column]` | The preset object, an OBINIT id. |
| `G[0x8f0503 + 20 × row + column]` | The preset object's level. |
| `G[0x8f0693]` | Columns, 20 or fewer. |
| `G[0x8f0694]` | Rows. |
| `G[0x8f0695..0x8f0698]` | The bounds: first column, last column, first row, last row. Map 1 has 1, 20, 1, 20. |
| `G[0x8f0699 + i]` | Entry i's side, 1..3. Entries count from 1. |
| `G[0x8f06ad + i]` | Entry i's region: 0, or a region whose owner `G[0x3ee00 + it]` FIELD reads as the entering nation (`FIELD@0x843`..`0x924`). |
| `G[0x8f06c1 + 4 × i + 0..3]` | Entry i's deploy rectangle: x0, x1, y0, y1. |
| `G[0x8f0711 + i]` | 0..3; FIELD and PALACE read it. Not decoded, perhaps a facing. |
| `G[0x8f0725]` | The entry count + 1, the loops' end. |
| `G[0x8f0726]` | The ground picture. |

`G[0x8f0727]` follows MPINIT's cells. It is not MPINIT's: it is FIELD's probe land, below.

### Setting up a battlefield (FIELD, SETFIELD, SETFORT)

FIELD's map:

1. FIELD walks the DUINIT chain `G[0xa6b9b9]` (`FIELD@0x35f`..`0x374`). A story battle's branch sets the battle's map
   `G[0xc07c]`.
2. With a region `G[0xc07b]`, `G[0xc07c]` becomes the region's map `G[0x8e0330 + region]` (`FIELD@0x3b9`..`0x3c7`).
3. Unless `G[0x8858]` is 1, FIELD copies `G[0xc07c]` into the selector and walks MPINIT (`FIELD@0x7f8`..`0x840`).
4. It marks the entries in `G[0xc0b9 + i]` (`FIELD@0x843`..`0x924`) and runs SETFIELD (`FIELD@0x946`).
5. It sets `G[0x885b]` = rows × 50 and `G[0x885c]` = columns × 25 (`FIELD@0x94e`).

The region in SETFIELD and SETFORT is `G[0x8e0267]`. INIT2 sets it from `G[0x19a329]` in mode 4 (`INIT2@0x3a2`..).
SETFIELD does the following:

1. It copies the picture, columns, rows and bounds into `G[0xb155]`, `G[0x885d]`, `G[0x885e]` and `G[0x885f..0x8862]`
   (`SETFIELD@0x0`..`0x3a`).
2. It builds the land grid `G[0x8863]` from MPINIT's land, a row at a time (`SETFIELD@0x52`..`0x9b`).
3. While bit 1 of `G[0x3ed38 + region]` is clear, it runs SETFORT (`SETFIELD@0xa5`).
4. It clears the object grid `G[0x8aa3]` and `G[0xb154]` (`@0xcb`, `@0xd0`).
5. It walks the region's object layer. It removes a kind-1 object the map does not preset (`@0x10e`..`0x15a`). It
   hides an object with weeks left to build outside REIGN (`@0x19c`). Then it places each object's footprint into the
   object grid, the level grid `G[0x8ce3]` and the anchor grid `G[0x8f23]`, which holds the anchor's map index
   (`@0x29f`..`0x2db`).
6. For each object the book lists, it raises the shared profile's `G[0x1a563a + object]` to the level seen
   (`SETFIELD@0x1ea`..`0x246`).
7. A kind-4 object with a level-1 durability sets bit 0 of `G[0xb154]`, and bit 1 if it stands at level 0
   (`@0x2f1`..`0x332`).
8. In mode 3, it clears kind-1 objects inside the deploy rectangles of unused entries (`@0x33a`..`0x482`).

SETFORT is a region's first setup:

1. It copies RGINIT's `G[0x8e07e0]` and `G[0x8e0970]` into the region's `G[0xeeda0]` and `G[0xeee68]` (`SETFORT@0x0`..
   `0x1a`).
2. It clears the region's five layers (`@0x26`..`0x7a`).
3. It places the map's objects and levels (`@0x9e`..`0x119`), and sets each durability from `G[0x987f62]` +1
   (`@0x13e`, `@0x149`).
4. A level-0 object with a level-1 durability sets `G[0x122380]` = 1 (`@0x163`, `@0x175`).
5. It sets bit 1 of `G[0x3ed38 + region]` (`@0x1f5`).

FIELD's probe reads one cell into scratch globals: `G[0x8f0727]` the land, `G[0x8f0980]` the object and `G[0x8f0981]`
its level (`FIELD@0x38a4`..`0x38cf`). If the unit's class bits `G[0xc5c]` exclude the object's class, it clears the
object (`FIELD@0x3a73`..`0x3a9b`).

Then FIELD tests each of the unit's two movement types (`FIELD@0x3aa0`..`0x3c2b`):

1. With an object, a set barrier cell bars the cell.
2. Otherwise, with an object, the passability is `G[0x96ed0a + 30 × object + 3 × level + type − 1]`.
3. Without an object, it is the land's `G[0x8f078c + 3 × land + type − 1]`.
4. A value of 2 takes the speed percent: the object's `G[0x97623a]`, or else the land's `G[0x8f08b8]`.

### Drawing and building (READFIELD, DRAWOBJ, SELOBJ, REIGN)

**READFIELD** loads the ground picture `G[0xb155]` into texture 0x52 and 0x4b79 into 0x53. For each anchor cell, it
loads the level's picture `G[0xa0e7ba + 10 × object + level]` (`READFIELD@0xf9`):

- kinds 3..5 into texture 0x122 + kind − 3, once per kind;
- the others into texture 0x12c + cell.

**DRAWOBJ** erases the 400 elements `G[0x22c93c + cell]`, then draws each anchor cell. A bridge (kind 3) picks its
frame from its neighbours in the object grid and the land (`DRAWOBJ@0x194`..`0x30b`).

**SELOBJ** is REIGN's build list. It lists the objects whose kind has a book tab (`SELOBJ@0x7bd`..`0x801`). For each,
its filter (`SELOBJ@0x802`) returns at the first failing test. A hidden object returns with no bits set; the
`G[0x98064a]` and `G[0x98258a]` tests also hide, but no cell of theirs is written:

1. It hides the object for `G[0x98064a]` > `G[0x170a4]`.
2. It hides the object for the two minimums `G[0x981202]`.
3. It hides the object unless the player owns the required region `G[0x980a32]`.
4. It hides the object unless both bits of `G[0xf083 + G[0x980e1a]]` are set.
5. It hides the object unless every required flag `G[0x9819d2]` is set.
6. It hides the object if a flag of `G[0x98258a]` is set.
7. It sets bit 0, listed.
8. It sets bit 1, not for this region's type, and returns, if the mask `G[0x97fe7a]` lacks that type.
9. It sets bit 2, wrong region, and returns, if `G[0x980262]` names another region.
10. It sets bit 3, funds short, and returns, if level 1's cost is above `G[0x17120 + nation]`.
11. It sets bit 4, materials short, if any material is short (`SELOBJ@0xa18`..`0xa95`).

**REIGN** builds on the region `G[0x19a329]`:

1. It checks the funds and materials (`REIGN@0x1dc3`, `@0x1e42`) and the footprint (`@0x1c40`..`0x1d08`, `@0x1eda`).
2. It sets `G[0x1abd5a + object]` to 1 (`@0x1f82`) and the object's flag `G[0xa0e3d2]` (`@0x1f8e`).
3. It deducts the cost (`@0x1fbd`) and consumes the materials (`@0x2052`..`0x208c`).
4. At the anchor's map index, it writes the object and level 1 into the region's layers (`REIGN@0x2092`..`0x20ad`).
5. It writes the weeks left from `G[0x9e99e2 + 10 × object + 1]` (`@0x20b2`..`0x20c8`).

### CALCFORT (a region's totals)

CALCFORT walks a region's object and level layers. For each built object, it adds the object's per-level effects into
the region's totals; those total cells are the regions' ("The strategic map"):

| Object array | Region total |
|---|---|
| `G[0x9e4bc2]` | `G[0x3f058]` |
| `G[0x9e72d2]` | `G[0xf0128]`, clamped to 0..100 |
| `G[0x9aa242]` and `G[0x9af062]` | `G[0x135c00]`, 2 cells |
| `G[0x9b3e82]` | `G[0xef890]`, 2 cells |
| `G[0x9b8ca2]` | `G[0xefa20]`, 2 cells |
| `G[0x9bdac2]` | `G[0xef0c0]`, 5 cells |
| `G[0x9c9e12]` | `G[0xef4a8]`, 5 cells |
| `G[0x9d6162]` | `G[0xefbb0]`, 6 cells |
| `G[0x8f1d0a]` | `G[0xf0060]` |

It also keeps two counts per object:

- `G[0xf1640 + 1000 × region + object]` counts the region's built objects of each id.
- `G[0x6fe60]` keeps the highest level of each.

For kind 5 (walls), it sums the durability into `G[0xf01f0]` and `G[0xeef30]`, and sets `G[0xf14b0]` to their percent.
`G[0x122380]` marks a level-0 object as being built (`CALCFORT@0x484`; `CRAFTACT@0x35f4`).

### What is saved

None of LAINIT, OBINIT, DPINIT or MPINIT is saved: all four lie above the prefix `G < 0x211c08`, `GS < 0x1b1`. Their
effects reach a loaded game as follows:

- **MPINIT.** A changed map reaches only regions not yet set up, since SETFORT copies the preset objects into the
  saved layers once. It also reaches every battle's land grid, because SETFIELD rebuilds that from MPINIT on each
  field.
- **OBINIT and LAINIT.** These are read live: passability, speed, costs, stats, pictures, the book, the build list and
  CALCFORT's totals. The exception is the durability and weeks left that SETFORT and REIGN copied into the layers.
- **DPINIT.** It is read live.

| cell | holds | saved |
|---|---|---|
| `G[0xa0ba0 + 400 × region + cell]` | The region's object layer | yes |
| `G[0xb4420 + …]` | The region's level layer | yes |
| `G[0xc7ca0 + …]` | The weeks left to build | yes |
| `G[0xdb520 + …]` | The current durability | yes |
| `G[0x122380 + …]` | Level-0 objects being built | yes |
| `G[0x8863]`, `G[0x8aa3]`, `G[0x8ce3]`, `G[0x8f23]` | The battlefield's land, object, level and anchor grids (24 × 24) | yes |
| `G[0xb155]`, `G[0x885d..0x8862]`, `G[0x885b]`, `G[0x885c]`, `G[0xb154]` | The battlefield's picture, size, bounds, extents and barrier bits | yes |
| `G[0x1abd5a + object]` | Set when the object is first built (`REIGN@0x1f82`) | yes |
| `G[0x1a563a + object]` | The highest level seen of each book object | yes; the shared profile keeps it too (`SETFIELD@0x21b`, `@0x246`) |
| `G[0x8f01e2]`, MPINIT's cells, `G[0x8f0727]`, `G[0x8f0980]`, `G[0x8f0981]` | The selector, the map's cells and the probe | no |

A field resumed from a save keeps its grids. FIELD skips MPINIT when `G[0x8858]` is 1 (`FIELD@0x7f8`).

## Information screens

The game's menu (`MENU`) opens `INFO`, which runs `INFO1` or `INFO2` by `G[0x22c31c]` (`INFO@0x0`..`0x25`); the
title's extras room enters `INFO2` directly (`ROOM@0x66`, `G[0x22c31c]` 1). Each runs one page a pass by
`G[0x22c31d]`: `INFO2` runs `INFO21` to `INFO27` for 0 to 6 (`INFO2@0x13`..`0x60`), `INFO1` the pages `INFO11` to
`INFO17`. Two pages read this section's tables: `INFO22`, the tips (TIINIT), and `INFO23`, whose tabs `G[0x22c31e]` are the
character profiles (0, CIINIT, "Characters, titles and voices"), the regions (1, RGINIT, "The strategic map") and the glossary (2, VIINIT; `INFO23@0x783`,
`@0x860`, `@0x8cc`). The others read the other sections' tables: `INFO11` to
`INFO13` and `INFO17` the nations and regions, `INFO14`, `INFO15`, `INFO24` and `INFO26` the troops, items and skills,
`INFO25` and `INFO27` the domain objects and quests; `INFO16` and `INFO21` read no boot table. `INFO2`'s first pass
(`INFO2@0xe0`..`0x57a`) rebuilds the unlocked tips and glossary entries.

### NAINIT (help lines)

174 strings, `GS[0xf8f3 + id]`, ids 1 to 326: the line a strategy screen shows at its top for the button under the
cursor (左クリックで画面移動します, 自国行動を終了します …). 19 scripts draw it (`ALLMAP`, `REIGN`, `PALACE`, `MARCH`,
`ORGANIZE`, `SHOP` and others): each sets `help_line_id` `G[0xa46e06]` and draws `GS[0xf8f3 + G[0xa46e06]]` centred
(`ALLMAP@0x7162`..`0x718a`). NAINIT has no `$1$` cells. The array lies above the string prefix, so a save does not
hold it: INIT2 writes it again on every load.

### TUINIT (tutorials) and TUMES

Leader `G[0xa46e08]`, capacity 1000; 17 tutorials, ids 1 to 76. `TUTORIAL` picks one for the screen asking
(`tutorial_screen` `G[0x22c3d0]`: 1 and 2 `FIELD@0xa51`, `@0xb58`; 3 `ALLMAP@0x2cc`; 4 `REIGN@0x2b9`), in order of
the sort key, the first that is not shown yet and whose conditions hold (`TUTORIAL@0x30e`..`0x50b`). It shows only for
the player's own nation (`G[0x639f]` 1, `TUTORIAL@0xf`) and never under the debug switches.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa46e08]` | 1 |  | 17 | 0..0, 1 distinct 0×17 | the layer whose text script plays it: the slot of the `TUMES` chain `G[0xa6ba99]` (`TUTORIAL@0x591`..`0x5b4`); 0 is the base `TUMES` |
| `G[0xa471f0]` | 1 |  | 17 | 1..4, 4 distinct 1×2, 2×4, 3×10, 4×1 | the screen it belongs to, compared with `tutorial_screen` (`TUTORIAL@0x360`) |
| `G[0xa475d8]` | 1 |  | 17 | 10..760, 17 distinct | the sort key (`sort-indices-by-key-sum`, `TUTORIAL@0x32e`) |
| `G[0xa479c0]` | 1 |  | 6 | 51..76, 5 distinct | a tutorial that must have been shown first (`tutorial_shown_flags`, `TUTORIAL@0x39d`..`0x3c7`) |
| `G[0xa47da8]` | 1 |  | 0 |  | not written: a tutorial whose showing rules this one out (`TUTORIAL@0x3cf`..`0x3f9`) |
| `G[0xa48190]` | 2 | 0..1 | 15 | 6..8302, 8 distinct | up to two scenes, as number + 1, that must have been played (`scene_played_flags`, `TUTORIAL@0x474`..`0x4a6`) |
| `G[0xa48960]` | 2 | 0 | 2 | 8019..8019 | up to two scenes, as number + 1, that must not have been played (`TUTORIAL@0x4ce`..`0x500`) |
| `G[0xa49130]` | 1 |  | 6 | 1..35, 4 distinct | a value `G[0xc077]` must have (`TUTORIAL@0x401`..`0x424`) |
| `G[0xa49518]` | 1 |  | 1 | 2 | the region the player must be in: `G[0x19a329]`, or `G[0xc07b]` in a battle (`TUTORIAL@0x313`..`0x329`, `@0x42c`..`0x44f`). Only tutorial 76 has one; the tool counts it outside the table |

Once shown, `TUTORIAL@0x75c`..`0x77b` sets `tutorial_shown_flags` `G[0x1ab972 + id]` (in a save, so for this playthrough) and
`tutorial_seen_flags` `G[0x1a469a + id]`, which it stores in the shared profile. `TUMES` is the text: it branches on
the chosen tutorial `G[0x22c3d1]` (`TUMES@0x0`) and is the chain's slot 0 (`INIT2@0x7e`); `$1$AUTORUN@0x80` puts
`$1$TUMES` in slot 1. TUINIT has no `$1$` fragment that writes cells.

### TIINIT (the explanations)

Leader `GS[0xfcdb]`, capacity 1000; 221 topics, ids 1 to 224: the 解説 pages of the information screens (戦闘, 戦場,
制限時間 …). `INFO2` counts them by category and unlocks them (`INFO2@0xe0`..`0x299`), `INFO22` shows one.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa49900]` | 1 |  | 221 | 10..1910, 208 distinct | the sort key (`INFO22@0x6ef`) |
| `G[0xa49ce8]` | 1 |  | 221 | 1..3, 3 distinct 1×67, 2×98, 3×56 | the category, 1 to 3: `INFO2` counts topics, unlocked topics and read topics per category (`INFO2@0x103`, `@0x20d`..`0x292`) |
| `G[0xa4a0d0]` | 3 | 0 | 136 | 1..8032, 22 distinct | up to three scenes: the topic is unlocked once one is seen (`scene_seen_flags`, `INFO2@0x147`..`0x1a2`). Only the first cell is written |
| `G[0xa4ac88]` | 1 |  | 201 | 1..560, 10 distinct | one more unlock: `INFO2` loads `tutorial_seen_flags[value]` from the shared profile and then tests `scene_seen_flags[value]` (`INFO2@0x1ac`..`0x1e5`). A topic with neither is unlocked from the start (`INFO2@0x10a`..`0x13f`) |
| `G[0xa4b070]` | 1 |  | 221 | 18732..20328, 186 distinct | the picture `INFO22` draws (`INFO22@0xd07`) |
| `G[0xa4b458]` | 1 |  | 221 | 712..712 | the picture's x (`INFO22@0xd38`) |
| `G[0xa4b840]` | 1 |  | 221 | 220..220 | the picture's y (`INFO22@0xd3f`) |
| `G[0xa4bc28]` | 7 |  | 0 |  | not written: a scene for each text line, which must be seen for `INFO22` to draw the line; 0 draws it (`INFO22@0xe3f`..`0xe7d`) |
| `GS[0xfcdb]` | 1 |  | 221 | 戦闘 / 戦場 / 制限時間 | the topic's name in the list |
| `GS[0x100c3]` | 7 | 0..6 | 221 | 【戦闘】 / 【戦場】 / 【制限時間】 | the page: a heading and up to six lines (`INFO22@0xe72`) |

`INFO22@0xcac`..`0xcd4` marks a topic read in `tip_read_flags` `G[0x1a7c1e + id]`, kept in the shared profile;
`INFO22@0xb6f` draws a mark beside a topic not yet read. The unlocked state is rebuilt on each entry, in
`G[0xa6ac40 + id]`, above the prefix. TIINIT has no `$1$` cells. The string arrays lie above the string prefix (not saved,
written again on every load).

### VIINIT (the glossary)

Leader `G[0xa5ecd8]`, capacity 1000; 99 entries, ids 1 to 231, 2 of them from `$1$VIINIT`. `INFO23` shows them, and
`BREED` shows the entry of a breed (`BREED@0x142`, through `G[0x20e267 + 0xc8 + …]`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa5ecd8]` | 1 |  | 99 | 3..2160, 99 distinct | the sort key (`INFO23@0x8da`) |
| `G[0xa5f0c0]` | 48 | 0..46 | 74 | 4..9035, 52 distinct | the unlocks, three cells for each of the sixteen lines: a scene as number + 1, of which one must be seen for the line to show (`INFO23@0x1974`..`0x1a0c`). The first line's three unlock the entry (`INFO2@0x503`..`0x57a`); an entry whose first cell is 0 is open from the start (`INFO2@0x50e`..`0x51c`). The tool does not try stride 48 and lists these cells outside the table |
| `GS[0x1626b]` | 1 |  | 99 | 【用語】二つの回廊の終わり / 【用語】三神戦争 / 【用語】先史文明期 | the entry's title in the list (`INFO2@0x4ee`, `INFO23@0x90c`) |
| `GS[0x16653]` | 16 | 0..15 | 99 | 『二つの回廊の終わり（ディル＝リフィ / … | the page: a heading and up to fifteen lines |

`INFO23@0x185e`..`0x1886` marks an entry read in `glossary_read_flags` `G[0x1a83ee + id]` (shared profile). The
unlocked state is rebuilt in `G[0xa6b410 + id]`. `INFO23` also lays out CIINIT's character profiles (`G[0x22c31e]` 2
is the glossary, `INFO23@0x8cc`).

### What is saved

- **Saved:** `tutorial_shown_flags`; and `tutorial_seen_flags`, `tip_read_flags` and `glossary_read_flags`, which
  are also kept in the shared profile (the screens load them from it before they test them). A change to the tables
  reaches old saves: every string array lies above the string prefix, and INIT2 writes them again on every load.
- **Not saved:** the integer arrays, the string arrays (above the prefix `GS[0x1b1]`),
  `help_line_id`, the rebuilt unlock states.

## Galleries

The extras room, `ROOM`, picks a page in `G[0x22c31a]`: 0 `CGMODE`, 1 `HMODE`, 2 `MMODE`, 3 `INFO2`
(`ROOM@0x1b`..`0x6b`), and ends with `exit-script`, the soft reset (`ROOM@0x8e`).

### CGINIT (the CG gallery)

Leader `G[0x2253d3]` (a CG writes its picture first, two cells per id), capacity 3000; 1336 CGs, ids 1 to 1336, 9 of them from `$1$CGINIT`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x2253d3]` | 2 | 0..1 | 1336 | 91..16777436, 1448 distinct | column 0 the CG's picture: `CGMODE` shows it once the catalog marks it seen (`is-catalog-resource-unlocked`, `CGMODE@0x5a9`..`0x5b4`) and draws it (`@0x8f1`). Column 1, on 764 CGs, the picture a save's thumbnail shows in its place when that CG is on screen (`SAVE@0x8b0`..`0x8ed`, `ALLMAP@0x2b50`..`0x2b8d`) |
| `G[0x226b43]` | 1 |  | 1336 | 1..11, 10 distinct | the page: 1 to 9 for the base CGs, 11 for the append's (`CGMODE@0x558`) |
| `G[0x2276fb]` | 1 |  | 1336 | 1..20, 20 distinct | the group on the page, 1 to 20 (`CGMODE@0x566`) |
| `G[0x2282b3]` | 1 |  | 1336 | 1..22, 22 distinct | the CG's place in its group; 0 leaves it out of the gallery (`CGMODE@0x54a`, `@0x57b`: entry `100 × (group - 1) + place - 1` of the page's list) |
| `G[0x228e6b]` | 1 |  | 0 |  | not written: an x offset `CGMODE@0x943` applies to a picture of another size |
| `G[0x229a23]` | 1 |  | 1 | 100 | the y offset, the same way (`CGMODE@0x956`); only CG 47 has one |

`INIT2@0x10a`..`0x132` puts the thumbnail sheets of pages 1 to 9 in `G[0x22a5db + page - 1]` (`SO043A.AGF` to
`SO043I.AGF`) and `$1$AUTORUN@0xbc` the sheet of page 11, the append's nine CGs, in `G[0x22a5e5]`; page 10 has none.
`CGMODE` walks the 20 cells and shows a page tab where one is set (`CGMODE@0x65a`..`0x66f`).

### SPINIT (scene replay)

61 cells, `G[0x22a5ef + 12 × page + slot]`, pages 0 to 19 and slots 0 to 11: the replay's scenes, each a script
(`SP0482.BIN`, `SP0484.BIN` …, `SP3006A.BIN` to `SP3006E.BIN`). `HMODE` offers a slot once the catalog marks its script
seen (`HMODE@0x522`..`0x53f`), and plays it with `call-script` (`HMODE@0x993`..`0x99e`), with `scene_run_mode` set to
-1 for the setup and to 0 for the call (`HMODE@0x7db`, `@0x98e`), and `replay_room_active` set for the whole room
(`HMODE@0x0`, `@0x113d`). `INIT2@0x137`..`0x150` puts the pages' sheets in `G[0x22a6df + page]` (`SO044A.AGF` to
`SO044F.AGF`). SPINIT has no `$1$` cells.

### MUINIT (the music room)

Leader `G[0x22a6f3]`, capacity 50; 42 tracks, ids 1 to 42.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x22a6f3]` | 1 |  | 42 | 1..6887, 42 distinct | what unlocks the track: a resource the catalog must mark seen (`MMODE@0x611`..`0x618`), the track's own `BGMnnn.OGG`, or for the songs the script that plays them (`OP.BIN`, `ED.BIN`, `ED2.BIN`) |
| `G[0x22a725]` | 1 |  | 42 | 2..57, 42 distinct | the BGM number `play-bgm` plays (`MMODE@0x449`, `@0x4f6`) |
| `GS[0x36c]` | 1 |  | 42 | 月女神の詠唱～アリア～ / 月女神の詠唱～アリア～ Short Ver. / 月の序曲～プレリュード～ | the title (`MMODE@0xae1`) |

### What is saved

Nothing of the galleries is in a save: what they show comes from the catalog's seen marks, which SAVE.DAT keeps for
the player, and from `scene_seen_flags`. MUINIT's titles lie above the string prefix and are written again on every
load.

## What is not decoded

**Boot.**

- `G[0x5]`, the cell SYSTEM4 keeps in the shared profile to choose between INITCONFIG and LOADCONFIG, and the options
  `G[0x63b2]`, `G[0x63b4]`, `G[0x63b5]` and `G[0xc4b5]` beyond the bits cited in INITCONFIG's table.
- INIT2's caps block `G[0x22cacc]` beyond CALCABI's use, `G[0x8da33b]`, and the thirteen file numbers `G[0x20e32f]` …,
  which no script reads by their base.

**Scenes.**

- The three cells of each visit slot (`G[0x21252b]`, `G[0x212783]`, `G[0x21333b]`): their readers in ALLMAP, REIGN and
  PALACE were not read. `visit_scenes_by_character`'s key is inferred from PALACE's grid and REIGN's list `G[0x67a5]`.
- The 359 scene branches whose number SCINIT does not list (354 with an empty title). No script sets one of them as
  `current_scene_id`.
- Most played-range cells SCINIT does not use (7000 and up), among them `G[0x2801]`, the story flag tested most after
  the prologue (630 rules, `!= 1`), which `SC0200@0x10949` sets.
- `scjump_decode.py` is written for Himegari's `SCJUMP` and was not run on this game; SCJUMP's rules are read by
  their protocol and counts only.
- Whether INFO2 means `tutorial_seen_flags` where it loads it by a tip's `tip_second_unlock_scenes` (1 to 560): a
  native run that unlocks such a tip would settle it.

**Characters, titles and voices.**

- CHINIT `G[0x22e470]` (a flag on 82 named characters, `ADDCORP@0x2aa`), `G[0x2378e0]` (EXECSKILL's value 1..3) and
  stat column 9, whose work cell `G[0x22caf9]` has no reader.
- CLINIT's conditions no record writes (`G[0x23c701]`, `G[0x23e641]`, the bonus `G[0x23f5e1]`), `G[0x248e39]`,
  `G[0x249221]` `[5]` and `G[0x24b931]`.
- HAINIT `G[0xa242c8]` (a minimum of `G[0x70a9]`) and `G[0xa243d6]` (an item BROTHEL puts in play).
- The stat columns' names pair DRAWSTATUS's rows with the TIINIT tips' labels; the rows' labels are pictures.

**Troops and battle.**

- The troop parameters' columns 1 and 10, and `commander_troop_kind_matrix`'s values (SELTR tests only for non-zero).
- SKINIT's active fields `G[0x2a571e]`, `G[0x2a558e]` (likely the knockback), `G[0x2af67e]`, `G[0x2b23ce]` and
  `skill_effect_kinds` 3 to 6; the `$1$` skills' script cell `G[0xa6b8ba]`.
- FOINIT's stride-200 arrays `G[0x2b4380]`, `G[0x2b91a0]`, `G[0x2b4128]` and `G[0x2bdfc0]`.
- ANINIT `G[0xa43f26]` (operand 2 of op 0x332), `G[0xa44ade]` … `G[0xa45696]`, `G[0xa45e66]` and `G[0xa46a1e]`.
- RTINIT: every block. Blocks A and B have no reader by base address; FIELD's and MVRTN's index arithmetic would show
  whether they are read through a computed base. The tool lays out block A only.

**Items, parts and facilities.**

- ITINIT `G[0x25beed]`, `G[0x25d65d]` and `G[0x25de2d]`, which no record writes, and `G[0x25e5fd]`, read and not
  written. ALINIT's and BRINIT's conditions no record writes.
- What BREED does with a result beyond taking the parent, EMPLOY's hiring, and `G[0x1ac5f3]` `[100]`, which GAMESTART
  carries and GAMECLEAR stores.

**The strategic map.**

- The nations' saved cells `G[0x1715c]`, `G[0x1717a]` `[2]` (260 uses), `G[0x171f2]`, `G[0x17210]`, `G[0x1722e]`
  `[6]`, `G[0x172e2]`, `G[0x17300]` and `G[0x17a26]` `[30][30]`, and CNINIT `G[0x8df41b]`, `G[0x8e0195]`,
  `G[0x8e01b3]` and `G[0x8e0249]`.
- The regions' saved cells `G[0x3f058]`, `G[0xef0c0]` … `G[0xf1000]`, which CALCFORT and the ruses write, and RGINIT
  `G[0x8e04c0]`, `G[0x8e0650]`, `G[0x8e0970]`, `G[0x8e12d0]` and the second x/y pair.
- Routes: the two x/y pairs and route kind 4. Production: `G[0x8e5dd1]` and what the five kinds are. USINIT
  `G[0x8eabf2]`, `G[0x8eea72]`, `G[0x8efa12]`, and the saved `G[0x68d8 + record]`.
- RUSEACT's effects ruse by ruse, and RUINIT's undecoded arrays (`G[0xa11d12]` … `G[0xa1241a]`).
- QUINIT's kinds 2 to 4, delivery kind 1, reward kinds 3 and 4, and the arrays the section lists without a name.
- DUINIT: which side each end condition is.

**Battlefields.**

- LAINIT `G[0x8f0728]` and `G[0x8f091c]`; OBINIT `G[0x8f1d0a]`, `G[0x8f20f2]`, `G[0x97894a]`, `G[0x985852]`,
  `G[0x9a2d12]`, `G[0x9a5422]`, `G[0xa02082]`, `G[0xa0bcc2]`, `G[0x980e1a]`, `G[0x981202]`, the grade `G[0x983142]`,
  and what each CALCFORT effect does. `G[0x98064a]` and `G[0x98258a]` are tested by SELOBJ, and nothing writes them.
- CRAFTACT, MVSEEK, SETROUTE, DISPOSE and the RTN_M routines, the battle's object actions, AI and routing, were not
  read.
- MPINIT has no layout from the tool, which lays out a table of records, not one map's cells chosen by a selector;
  its section is read from its listing.
