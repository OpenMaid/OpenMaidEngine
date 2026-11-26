# 戦女神ZERO data tables (`*INIT`)

Canonical home for how 戦女神ZERO (bg0, SYS4415, AGE 4.15D) stores its static game data: which `*INIT` scripts
exist, how they are layered, where each table lives in the global bank, and what is known of its fields. Names for
the fields that are understood live in [the bg0 globals registry](../vm-map/globals-bg0.toml) (generated →
[its global reference](global-reference-bg0.md)). The game is 戦女神VERITA's predecessor, and many of its
tables have Verita's shapes at other addresses; Verita's are in [Verita's data tables](verita-data-tables.md),
the other games' in [Arterial's](arterial-data-tables.md) and [Kamidori's](kamidori-data-tables.md). A reading here
rests on bg0's own data or consumers, never on the likeness alone.

**How far this goes (2026-11-25).** Every table's *layout* is here: its arrays, their strides and what values they
hold, for the 25 boot tables and the 6 that run on demand. What an array *means* is given where the data or a
consumer shows it, each reading with the script and offset it rests on. A row that says nothing in its last column
is not read yet; "What is not decoded" at the end lists the gaps.

## How the data gets into memory

Like the other games, 戦女神ZERO has no data files. Every table is a script of constant writes into the flat global
bank: `mov` for integers, `set-string` for text, `copy-inline-int-array` for a few bulk arrays. Runtime code reads
the bank with `lookup-array` (`base + id`) and `lookup-array-2d` (`base + row * stride + column`).

**One array per field.** A table is a set of parallel arrays. A record's field is the cell
`base + cells_per_id * id + column`: one cell per id for a scalar, several for a short list. Each array is allocated
for the table's *capacity* (1000 units, 10000 items) whether or not an id is used, and the arrays of a table lie
back to back. Ids are sparse and mostly start at 1.

**Boot order.** `INIT2.BIN` calls 25 initializers in this order: CBINIT, EBINIT, CNINIT, ITINIT, ILINIT, SSINIT,
RTINIT, AFINIT, AMINIT, MPINIT, SIINIT, ARINIT, EEINIT, TRINIT, LAINIT, OBINIT, CPINIT, CGINIT, SPINIT, CIINIT,
VIINIT, CVINIT, SCINIT, BTANINIT2, OBINIT2. Op `0x143` then runs the mounted append catalog's `$1$AUTORUN.BIN`,
which calls its own `$1$<TABLE>.BIN` for 19 of them. The install has one append, so a table's runtime image is the
**base, then `$1$`**, later writes winning. A fragment adds records and also rewrites cells of records the base
made; a fragment of a few lines is an empty stub. Of the 19 fragments, 9 are empty stubs and 10 write cells:

- `$1$CBINIT` writes CBINIT's 60 cells again with the same values, so it changes nothing.
- `$1$AMINIT`, `$1$SIINIT` and `$1$SCINIT` run only while `special_edition_mode` `G[0x90c1a]`, the game mode, is
  set to 特別編 (`$1$AUTORUN@0xf`, `@0x3a`).
- Six change the boot image: `$1$EBINIT` (4066 integer and 333 string cells), `$1$ITINIT` (1289 and 84),
  `$1$TRINIT` (181), `$1$CNINIT` (70 and 70), `$1$MPINIT` (16 and 9) and `$1$CGINIT` (16).

The game mode is 0 at a cold boot, so the boot image (`init_image.py`) records the three guarded fragments as
skipped (`halt: skipped`): it is the image of a 通常 game. It records only the cells a script changes. On a load of
a 特別編 save, INIT2 runs after the saved prefix is back ("The boot order" below), so `$1$AUTORUN` runs the three
then. A new 特別編 game runs `$1$CHINIT` and `$1$SCINIT` itself (`GAMESTART@0x3ab`..`0x3b5`). Six boot tables
have no fragment: ILINIT, SSINIT, AFINIT, ARINIT, EEINIT and OBINIT2.

**On demand.** Six tables are not run by INIT2, so they are not in the boot image: CHINIT, which INIT runs on every
root run before the title (`SYSTEM4@0x2dd`, `INIT@0x0`), and BTANINIT, ENINIT, ESINIT, RMINIT and SHINIT,
which the screens that use them run. Each of those but SHINIT has a `$1$` fragment, and its caller runs the fragment
itself while the game mode is 特別編: GAMESTART runs `$1$CHINIT` for a new 特別編 game, ANIME runs `$1$BTANINIT`
and FIELD runs `$1$RMINIT` in place of the base table (`ANIME@0xc`..`0x19`, `FIELD@0x7b`), and FIELD and
SETENP run `$1$ENINIT` and `$1$ESINIT` first, and the base table only where the fragment loaded nothing
(`FIELD@0x5e5`, `SETENP@0x347`; the battle part's table, below). There are no hook chains of 32 slots as in
Verita: the append reaches the tables through `$1$AUTORUN` and those direct calls.

## Which files are authored data

`games/bg0/` holds the archives `DATA1`–`6.ALF` and `APPEND.ALF` (with its `.AAI` index), and 52 loose `.BIN` files
in its root: `SYS4INI.BIN`, `SYS4RK.BIN`, `SYS4AB.BIN` and 49 scripts. All 49 have a record of the same name in an
archive (1251 in `DATA1`, 328 in `APPEND01`); 45 differ from it and 4 are identical (`MP27F0104`, `MP41B0103`,
`MPEV02B0304` and `MPEV41B0102`). Native AGE reads a loose file before the archive, and the data tooling reads the
install the same way (`tools/paths.py`'s `scripts()`), so every count below includes them. Five tables and the boot
driver have a loose copy: AFINIT, CGINIT, EBINIT, ITINIT, SIINIT and INIT2. (Where the loose files come from is not
established here.)

## Tooling

All commands run from `age-reimpl/` with `py -3.11 -X utf8` and `--game bg0`
([the tools reference](tools-reference.md)):

1. Extract the archives once with the local `bin/BinExtractALF.exe` into `../extracted-bg0/`: `DATA1/` to `DATA6/`,
   and the append's records flat in `APPEND01/`.
2. `parse_sys4ini.py` → the asset index and call-script names.
3. `extract_phase2.py` → `build/games/bg0/disasm/*.asm` and the text corpora.
4. `init_image.py` → `build/games/bg0/init-image.json`, the **executed** boot image: the port's VM runs the boot
   order above and records what each of the 44 scripts wrote. A table call behind a `jcc` on an integer global runs
   only while that global is set, as in the game; while it is 0 the image records the call as `halt: skipped`, with
   no cells.
5. `init_layout.py` → each table's layout, inferred from the write order and the image, with bg0's leaders (`BG0`
   in the tool). `init_layout.py <TABLE>` prints a table's arrays column by column, `--markdown` the layout tables
   below, and `--write` saves them as `build/games/bg0/data/<TABLE>.layout.json`.
6. `global_map.py` → `build/games/bg0/global-var-map.{json,md}`, the access shape of every global, and
   `story_flags.py` → `build/games/bg0/story-flags-candidates.json`, the globals the scripts branch on.

`init_layout.py` is given the array each record writes first (the *leader*) and the table's capacity, takes a
record's id from the leader cell, and finds every other field as the `(stride, base)` on which the most records
agree. A table whose records start at one of several arrays (LAINIT, OBINIT, TRINIT) gets them all as leaders. Tables
with no single leader are shown by the arrays the scripts index (`lookup-array` operands), which is a weaker view: an
array no script indexes with a variable is not seen, its cells are counted in the array before it, and the base shown
is the indexed array's, not always the field's. The tool does not lay out the on-demand tables, which are not in the
boot image; their sections are read from their listings. So are those of the tables run for one place at a time,
AMINIT, SIINIT and RMINIT. The tool's area mode (`AREAS`) takes one rule a game, with one selector and one slot
count, while SIINIT has 30 slots and RMINIT 450. AMINIT picks its world by four jumps at its start
(`AMINIT@0x32`..`0x6a`), so area mode credits every write to the last test, world 4. And AMINIT and SIINIT set
their states with `bit-set`, which area mode does not count. At boot no place is in play, so the image holds only
AMINIT's and SIINIT's clears.

**The grid, and `†`.** A table's arrays lie back to back, each a whole number of capacities long, so their bases
agree modulo the capacity. The tool marks an array whose base does not: `†` in the layouts below. Such an array
either has another table's cells between it and the rest, or is not indexed by the record's id at all, and then
the base and the ids in its row are wrong. Each `†` row says which.

**The saved prefix.** A save keeps the integer globals below `0x91003` (593,923) and the string globals below
`0xe0` (224), the profile's `persistence.bankDimensions`. Every boot table but AMINIT and SIINIT lies above both,
so it is rebuilt at boot and a save does not carry it. AMINIT and SIINIT write cells below the prefix; their
sections say what that means for a save. A table's section says where the game copies a field into a cell below
the prefix, which a save then keeps.

## Table inventory

| table | holds | runs |
|---|---|---|
| CBINIT | セリカ's four forms: the parameters' minimums and cost percents, and the HP, MP and TP percent | boot |
| EBINIT | units: party characters and enemies, their pictures, stats, equipment, routines, actions, rewards and drops | boot |
| CNINIT | speakers: the name plate, and the unit whose voice setting each speaker uses | boot |
| ITINIT | items, equipment, magic, arts and the enemies' actions | boot |
| ILINIT | conditions, five levels each | boot |
| SSINIT | the names of the support effects | boot |
| RTINIT | battle routines, step by step | boot |
| AFINIT | elements, affinities, item classes, formations and the battle AI types' texts | boot |
| AMINIT | the world map's areas, for the story part in play | boot (it only clears); on demand: `ALLMAP`, `SHOWALLMAP` |
| MPINIT | field maps: the start, the object sheet and the floors | boot |
| SIINIT | town sites, for the town in play | boot (it only clears); on demand: `TOWN` |
| ARINIT | the arena's rounds | boot; `ARENA` again |
| EEINIT | encounter chances | boot |
| TRINIT | treasure contents | boot |
| LAINIT | land types | boot |
| OBINIT | map object kinds: their sprites and what they turn into | boot |
| CPINIT | the field character's sheets | boot |
| CGINIT | the CG gallery | boot |
| SPINIT | scene replay | boot |
| CIINIT | character profiles (the 人物 book) | boot |
| VIINIT | the glossary (用語) | boot |
| CVINIT | voice settings | boot |
| SCINIT | scenes: the script each scene id plays, and its context | boot |
| BTANINIT2 | battle animations | boot |
| OBINIT2 | map object kinds: their depth and walls | boot |
| CHINIT | each character's unit | on demand: `INIT`, on every root run before the title |
| BTANINIT | the battle animations' effects | on demand: `ANIME` |
| ENINIT | encounter sets | on demand: `FIELD` and 25 map scripts |
| ESINIT | formations | on demand: `SETENP` (`BTL`) |
| RMINIT | map blocks: each block's scripts, pictures, music and encounters | on demand: `FIELD`, `CALLBACK_LOAD` |
| SHINIT | the shop in play | on demand: `SHOP` |

The layouts below give, for each array: its base, the cells it has per id, which columns are written, how many ids
have a value, and the values (for a string array, the first three).

## Boot

### The boot order (SYSTEM4)

SYSTEM4 runs these steps on each root run:

1. It sets up the text layouts (`SYSTEM4@0x0`..`0x1bc`).
2. It loads `config_initialized` `G[0x3]` from the shared profile (`SYSTEM4@0x1c3`). If the cell is set, it runs
   LOADCONFIG. If not, it runs INITCONFIG, then sets `G[0x3]` to 1 and stores it (`SYSTEM4@0x1c6`..`0x1db`).
3. It sets the message window's picture `G[0x456]` = 0x4593 and its colour `G[0x458]`. Then it copies the inline
   array 0, 4, 5, 6, 9 into `G[0x91014]` (`SYSTEM4@0x1de`..`0x1e8`). That array is not decoded: `DEBUGADV2@0xa22` and
   `SC0360@0x12740` use its first cell as a sound channel.
4. It maps the keys and buttons (`SYSTEM4@0x1ed`..`0x236`).
5. It preloads script slots 0x1b..0x1f: `$1$ITMES`, DRAWENLIST, ITMES, DRAWMAP, DRAWCHP (`SYSTEM4@0x23e`..`0x252`).
6. It runs INIT2 (`SYSTEM4@0x257`).
7. It reaches op `0xae continue-save-load-stack-restore` (`SYSTEM4@0x25a`). On a numbered load, the engine zeroes
   every bank, copies the saved prefix back, and runs each restored frame from its entry up to `0xae` ([the engine
   notes](engine-re.md), "A load zeroes each bank in full"). So INIT2 runs on every boot and on every load, and on a
   load it runs with the saved cells already in place.
8. It sets up the font and textures. On the first root run only (`get-initial-root-run`, `SYSTEM4@0x2cd`), it runs
   LOGO and OP (`SYSTEM4@0x2d7`, `0x2da`). Then, on every root run, it runs INIT (`SYSTEM4@0x2dd`), which only calls
   CHINIT, and TITLE (`SYSTEM4@0x2e0`).
9. It loops on the mode, `system_mode` `G[0x0]` (`SYSTEM4@0x2e3`..`0x5e5`): 1 a scene pass, 2 ALLMAP (`@0x59a`),
   3 FIELD (`@0x5ae`), 4 BTL (`@0x5c2`), 5 TOWN (`@0x5d6`). Any other value shows "不正な実行モード" and asks to exit
   (`@0x5dc`..`0x5e4`).

TITLE offers a new game (GAMESTART, `TITLE@0x327`), a load (SAVE, `@0x350`), the extras room (ROOM, `@0x3b4`), CONFIG
(`@0x3ca`) and a debug menu (`@0x3d6`). ROOM sets `in_room` `G[0x9100e]` to 1 (`ROOM@0x0`). It offers CGMODE, HMODE,
MMODE, INFO and HOSOKU (`ROOM@0x3fe`..`0x44e`). It ends with `exit-script` (`ROOM@0x627`), the soft reset that zeroes
the banks and reloads SYSTEM4, so the boot runs again.

### INITCONFIG (the options)

INITCONFIG sets each option and stores it in the shared profile (`INITCONFIG@0x0`..`0x67`). LOADCONFIG reads the
options back, and CONFIG changes them. All these cells also lie below the saved prefix.

| cell | default | read by | holds |
|---|---|---|---|
| `G[0x451]` | 1 | `SAVE@0x7e7` | SAVE asks "セーブしますか？" or "ロードしますか？" only while it is above 0. The test just before the question (`SAVE@0x7f5`..`0x81d`) is not decoded, so the cell stays unnamed. |
| `confirm_escape` `G[0x9050e]` | 1 | `MENU@0x50d` | While it is set, the escape command asks 脱出する / 脱出しない before it runs SCJUMP's context 6. |
| `menu_cursor_auto_move_enabled` `G[0x2fec]` | 1 | `BUNKI@0x2a1`, SBUNKI, BUNKIMOVE | While it is set, a choice menu puts the cursor on its first row. |
| `G[0x9050f]`, `G[0x90510]` | 0 (cleared together) | ANIME; BTL | Battle options. While `G[0x9050f]` is set, `ANIME@0x0` plays no animation; `CONFIG@0x886`..`0x8a4` sets it and stores it. While `G[0x90510]` is set, the battle picture does not split as it slides away (`BTL@0x6328`). |
| `G[0x90511]` | 0 | BTL | Not decoded. |
| `map_frame_rate` `G[0x30af]` | 60 | `FIELD@0xaab`, `FIELD@0x1063`, DRAWWALK | The map's frame rate. FIELD waits 1000 / value ms a frame and steps 60 / value. `CONFIG@0x91c` and `@0x935` set 60 or 30. |
| `skip_read_messages_only` `G[0x30ae]` | 1 | each scene (`SC0000@0x201`), HOSOKU, the SP scripts | With 1, a scene's skip button toggles read-message skip. Otherwise it sets message skip on, which skips every message. |
| `voice_mute_switches` `G[0x2be7]` … `G[0x2bf6]` | 0 (16 cells) | each scene (`SC0000@0xeaa`), BTL, HISTORY, ANIME | A mute per voice setting (CVINIT's 0 to 15), set from the Option screen (`CONFIG@0xeb0`..`0xeea`). A scene runs `mute-next-voice` when `G[0x2be7 + unit_voice_settings[speaker_voice_units[adv_speaker_id]]]` is set, through CNINIT's `G[0x91059]` and CVINIT's `G[0x91441]`. |

### INIT2 (the boot driver)

INIT2 calls the 25 base tables in the order above (`INIT2@0x0`..`0x48`). Then it runs op `0x143
run-mounted-append-autoruns` (`INIT2@0x4b`). Its own writes follow. The inline arrays' values come from
`games/bg0/INIT2.BIN`, where `copy-inline-int-array` reads a block with a count before it at the code offset.

| offset | write | what it is |
|---|---|---|
| `INIT2@0x4c`..`0x6f` | `G[0x91027]` = 10000, `G[0x91028]` = 10010, `G[0x91032]` … `G[0x91037]` = 10110 … 10160 | `scene_graphic_element_numbers`, graphic element numbers by layer: `G[0x91027 + n]` = 10000 + 10n for n = 0, 1 and 11..16. A scene sets the layer in `G[0x91022]` (0, 1 and 11..16 in SC0000) and indexes this array (`SC0000@0xfee1`..`0xff3c`). Cells 2..10 have no writer and no reader. |
| `INIT2@0x74`..`0x83` | `G[0x386a5a]` … `G[0x386a5d]` = 0x4585 … 0x4588 (AL001A.AGF … AL004A.AGF) | `world_map_pictures`, the world map's picture for each story part. `ALLMAP@0x967`, `SHOWALLMAP@0x8f` and `TOWN@0x5cb` read `G[0x386a59 + story_part]` and set it as texture 0x3b. `[0]` is not written. |
| `INIT2@0x88` | `G[0xc57bb]` … +4 = 10, 10, 10, 230, 230 | `party_name_x`, the x of the five party names. `$1$MPEV49B0102@0x253` draws `GS[0x9fe + G[0x8b236 + G[0x8b14c + i]]]` at (x + 16, y + 7) for each filled party slot i below 5. Only 特別編 map events read it (`$1$MPEV49B0102`, 0103, 0109). |
| `INIT2@0x8d` | `G[0xc57c0]` … +4 = 390, 455, 520, 455, 520 | `party_name_y`, the y of the same names. `$1$MPEV49B0102@0x1e8` also fills the panel from y = `G[0xc57c0]`. |
| `INIT2@0x92` | `G[0x389180]` … +7 = 51, 52, 53, 54, 55, 57, 56, 53 | Unit ids (EBINIT's names: イソラ王国弓兵, マーズテリア騎士, ヴィルト, ロコパウル, ゾノ, 反乱兵, 水竜騎士, ヴィルト). `BTL@0x154b` and `UNSUMMON@0x16f` read them. Not decoded. |
| `INIT2@0x97`, `0x9c` | `G[0x389319]` … +8 = 1 … 9 and `G[0x38934b]` … +6 = 10, 11, 12, 13, 14, 16, 15 | `character_sort_keys`, one array `G[0x389318 + character]` for characters 1..9 and 51..57 (0x38934b = 0x389318 + 51). `CHMODE1@0x21a` and `CHMODE2@0x1c0` sort the characters by it (`sort-indices-by-key-sum`). |
| `INIT2@0xa1`..`0xb4` | if `G[0x8b225]` == 0: `bit-set G[0x8b226] 1`, `G[0x8b225]` = 1 | `formation_current` and `formation_learned_mask` (items, below), both saved. A game with no formation gets the first one. On a load the saved value is already back when INIT2 runs, so the step does nothing then; it is not a first-run flag. |
| `INIT2@0xb9`..`0xeb` | for `character_arg_id` `G[0x8b158]` = 1..99: run CALCREVISE if `G[0x8b1c1 + character]` & 1 | CALCREVISE recomputes each character who has joined (units, below). |

### The append's driver (`$1$AUTORUN`)

Op `0x143` runs the AUTORUN of each mounted append. bg0 has one append, selector 1, so it runs `$1$AUTORUN`. That
script:

1. Calls `$1$CBINIT`, `$1$EBINIT`, `$1$CNINIT`, `$1$ITINIT` and `$1$RTINIT` (`$1$AUTORUN@0x0`..`0xc`).
2. Calls `$1$AMINIT` and `$1$SIINIT` only while `special_edition_mode` `G[0x90c1a]` is set
   (`jcc (global-int 0x90c1a) 0xffffffff label_1c`, `$1$AUTORUN@0xf`).
3. Calls `$1$MPINIT`, TRINIT, LAINIT, OBINIT, CPINIT, CGINIT, SPINIT, CIINIT, VIINIT and CVINIT (`@0x1c`..`0x37`).
   `$1$SPINIT` and `$1$VIINIT` hold only `exit`.
4. Calls `$1$SCINIT` only while `G[0x90c1a]` is set (`@0x3a`).
5. Calls `$1$BTANINIT2` (`@0x44`).

`jcc c T F` goes to T when c is nonzero and to F when it is zero; `0xffffffff` is the next instruction. So the gated
calls run when the cell is set and are jumped over when it is 0. INIT2@0xa1 (it sets the formation when the cell is
0), `SYSTEM4@0x1c6` (it runs LOADCONFIG when `G[0x3]` is set) and `SYSTEM4@0x479` (it uses the default script when
`G[0x454]` is 0) all need the same reading.

### The game mode (`special_edition_mode`)

GAMESTART@0x1f..0x78 offers 通常 and 特別編 through BUNKI. `GAMESTART@0x7b` stores the choice in
`special_edition_mode` `G[0x90c1a]`: 0 for 通常, 1 for 特別編. The cell is saved. In 特別編 a `$1$` script replaces
the base one; it is a mode of its own, not a layer over the base tables:

- A new 特別編 game runs `$1$CHINIT` and `$1$SCINIT` itself (`GAMESTART@0x3ab`..`0x3b5`). It sets
  `scene_run_mode` `G[0x386a6d]` to −2 (`@0x3b8`) and `story_part` `G[0x30b0]` to 4 (`@0x423`). A new 通常 game
  jumps to `GAMESTART@0x665`, which sets the story part to 1.
- At the title the cell is 0, so the boot's `$1$AUTORUN` ran without the gated tables. That is why GAMESTART runs
  `$1$SCINIT` itself.
- On a load the saved cell is back before INIT2 runs, so `$1$AUTORUN` loads 特別編's scenes, AM and SI tables again.
  `CALLBACK_LOAD@0x5`..`0xc` sets `scene_run_mode` to −2 again, since that cell is above the prefix.
- While it is set, ALLMAP@0x5a, TOWN@0x5d, FIELD@0x7b and CALLBACK_LOAD@0xd1 run `$1$AMINIT`, `$1$SIINIT` and
  `$1$RMINIT` in place of the base scripts, and every caller of SCJUMP runs `$1$SCJUMP`: `ALLMAP@0x31`,
  `FIELD@0x13b`, `TOWN@0x39`, `TOWN@0x522`, `BTL@0x706a`, `MENU@0x58a`.

The same base is also an array in the shared profile, at other indexes:

- `G[0x90c1a + cg]` is an append CG's unlock flag. `$1$SC0060@0x4ae8`..`0x4b14` finds the CG whose picture is shown,
  sets its cell to 1 and stores it. `CGMODE@0x9b7` reads it. CG ids start at 1, so the CG cells never touch cell 0.
- `G[0x90c1a + 800 + slot]` is a flag per save slot (`SAVE@0xd13`, `@0xf68`, `@0x103a`). It is not decoded.

### What is saved

- **Saved:** the mode `system_mode` `G[0x0]`, the game mode `special_edition_mode` `G[0x90c1a]`, and the options
  above, which the shared profile keeps too. INIT2's formation step writes saved cells (`formation_current`,
  `formation_learned_mask`).
- **Rebuilt at boot:** INIT2's constants (`scene_graphic_element_numbers`, `world_map_pictures`, the party name
  positions, `G[0x389180]`, `character_sort_keys`), all above the prefix.

## Scenes

### SCINIT (scenes)

INIT2 runs SCINIT, its 23rd call. Leader `G[0xbc8f6]`, capacity 10000; the table is keyed by scene id. 849 records,
701 scenes, ids 0 to 2009, all in the base layer: the boot image is 通常's, since `$1$SCINIT` runs only in 特別編.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xbc8f6]` | 1 |  | 701 | 40..11894, 86 distinct | The script that plays the scene, a packed script id: 0x28 is SC0000.BIN. `SYSTEM4@0x46d` copies it into `current_scene_script_id` `G[0x454]`. |
| `G[0xbf006]` | 1 |  | 701 | -1..3, 3 distinct -1×669, 2×3, 3×29 | The scene's offer context (SCJUMP, below). Its only reader is each scene's header (`SC0000@0x15f`): it registers the right-click menu unless ROOM is open or the value is 4. No scene has 4. |

SCINIT writes in two passes:

- **Pass 1** (`SCINIT@0x0`..`0x771`) writes a script and a context for each of SCJUMP's 184 rules, so a scene that
  several rules name is written as often (39 twelve times, 309 six times, 1511 four times). Seven more pairs match
  no rule: scene 1510 five times (context 3), a second 35 (context 3), and 395 (context 5). That is 191 pairs for
  164 scenes, with contexts 1 (9 scenes), 2 (61), 3 (87), 5 (5) and 6 (2), the contexts of the rules that name them.
- **Pass 2** (`SCINIT@0x776`..) writes the script of 669 scenes again, with the same values, and sets their context
  to −1. Scene 0's −1 is its first context write (`SCINIT@0x77b`).

So the context survives only on the 32 scenes that pass 2 leaves out: 249, 1009 and 1020 (context 2), and 1000,
1002, 1004, 1022, 1023, 1028, 1040..1042, 1045, 1046, 1060, 1063, 1064, 1150, 1151 and 1500..1512 (context 3).

`$1$SCINIT`, which a 特別編 game runs, uses the same two passes over 30 scenes: 0, 1, 10..13, 20..23, 30..34, 40..44,
50..54, 60..63 and 100. Pass 1 (`$1$SCINIT@0x0`..`0xf`) gives scene 0 to `$1$SC0000.BIN` (0x1000183) with context 1
and scene 100 to `$1$SC0100.BIN` (0x10001ab) with context 5, `$1$SCJUMP`'s two rules. Pass 2 (`@0x14`..) writes the
other 29, scene 0 among them, with context −1, so only scene 100 keeps a context. Its scenes play eight append
scripts, `$1$SC0000` to `$1$SC0060` and `$1$SC0100`.

### The scene pass

When `system_mode` `G[0x0]` is 1, SYSTEM4 plays a scene (`SYSTEM4@0x2ea`..`0x589`):

1. If `scene_run_mode` `G[0x386a6d]` is 0 and `current_scene_id` `G[0x91869]` is 0..1999, it sets
   `scene_seen_flags` `G[0x8f56d + scene]` to 1 and stores it in the shared profile (`SYSTEM4@0x2f9`..`0x336`).
2. If `scene_over_map` `G[0x91052]` is set, it erases only graphic elements 10000..49999; otherwise it clears every
   retained object (`SYSTEM4@0x34a`..`0x360`).
3. If `scene_run_mode` is 0 or −2, it sets `G[0x454]` = `G[0xbc8f6 + G[0x91869]]` (`SYSTEM4@0x44a`..`0x474`). A 0
   there becomes 0x28, SC0000.BIN (`@0x479`..`0x487`). Then it runs the script in `G[0x454]` (`@0x48c`).

A map event plays a scene without leaving the map. `MPEV01B0102@0x1d0` sets `G[0x91052]` to 1 and `G[0x91869]` to 9.
It then repeats SYSTEM4's pass inline (`@0x20c`..`0x47f`) while `G[0x91869]` differs from `played_scene_id`
`G[0x9186a]`. Each round first copies `G[0x91869]` into `G[0x9186a]` (`@0x21a`), so a scene that sets `G[0x91869]`
chains to the next. `MPEV01B0102@0x49a` clears `G[0x91052]`. 283 map events hold this loop. `MENU@0x5dc`..`0x73a`
does the same after context 6.

These scripts set `G[0x91869]` to a constant:

- 618 map events, for 368 scenes;
- SCJUMP and `$1$SCJUMP`, for 162 scenes;
- 56 scene scripts, for scene 0 (at a scene's end) and scenes 320, 321, 670 and 2001;
- TITLE, for scenes 0 and 2000 (`TITLE@0xa10`).

509 of SCINIT's 701 scenes are named by a map event or a SCJUMP rule. No script names the other 188 with a
constant.

### Scene scripts

A scene script holds up to ten scenes. SC0000 holds 0..9 and dispatches on `G[0x91869]` (`SC0000@0x729`..`0x7ae`).
Its other steps:

- **Header.** `SC0000@0x158`..`0x1a6` registers the right-click menu when `in_room` `G[0x9100e]` is 0 and the scene's
  context is not 4.
- **Start.** `SC0000@0x470` clears `G[0x2b80]` … `G[0x2be3]` (100 cells). 99 scene scripts do this, and no script
  reads those cells.
- **Debug path.** If `scene_run_mode` is 1, `SC0000@0x475` takes the debug path (`label_49c`).
- **Dispatch.** Otherwise `SC0000@0x483`..`0x494` calls the dispatch, and repeats while `G[0x2be4]` is −1. No script
  writes −1 there; 3,441 instructions write 0.
- **Story flag.** Each scene first sets its own story flag, `story_flags[scene]`. `SC0000@0x7b5` sets `G[0x470]` to 1
  for scene 0 (comment "▼G0000 嵐の勇者セリカ"). 649 of the 701 scenes do this. These 52 do not: 98, 224, 517..519,
  538, 539, 565..567, 687, 688, 747..749, 755..759, 762..769, 775..779, 791..799 and 2000..2009.
- **End.** A scene ends by setting the mode and clearing the scene (`SC0000@0x4810`..`0x4824`): `G[0x0]` = 3 (FIELD),
  `G[0x2be4]` = 0, `G[0x91869]` = 0, `current_place_id` `G[0x30b2]` = 1, `map_entered` `G[0x317c]` = 0. Other
  endings set the mode to 2 or 5 (`SC0000@0xf986`..`0xf9c3`).

### The run mode (`scene_run_mode`)

| value | set by | what it does |
|---|---|---|
| 0 | 100 scripts reset it (86 scene scripts, 11 append scripts, ALLMAP, DEBUGADV and others) | The story. Scenes seen are recorded, and the scene table is read. |
| 1 | `TITLE@0x678`, the debug menu's "SC????.BIN" pick | TITLE sets `G[0x454]` to the chosen script and keeps it in the shared profile (`TITLE@0x682`..`0x68e`). `SC0000@0x475` takes the debug path. |
| −1 | `TITLE@0x69e` (DEBUGADV, `G[0x454]` = 0x3ec1) and `TITLE@0x6bc` (DEBUGADV2, 0x3ec7) | SYSTEM4 neither records the scene nor reads the table. |
| −2 | `GAMESTART@0x3b8`, `CALLBACK_LOAD@0xc` | 特別編. The table is read; scenes seen are not recorded. |

The cell `G[0x386a6d]` is above the saved prefix.

### SCJUMP (the scene dispatch)

`scjump_context_id` `G[0x2fd2]` picks a rule list (`SCJUMP@0x0`..`0x54`). A rule that fires sets `G[0x0]` to 1,
`G[0x2be4]` to 0 and `G[0x91869]` to its scene (`SCJUMP@0x57`..`0x74`). SCJUMP holds 184 rules, and 144 of them
require their own scene's story flag `G[0x470 + scene]` not to be 1. ALLMAP and TOWN run SCJUMP before their
loaders and leave at once when a rule fired: they test `system_mode` for 1 and exit (`ALLMAP@0x41`..`0x4f`,
`TOWN@0x49`..`0x57`), so SYSTEM4 plays the scene before the place is set up.

| context | set by | rules | scenes | what the rules test |
|---|---|---|---|---|
| 1 | `ALLMAP@0x2c`, on each world map pass | 15 | 0, 300..304, 308, 309, 400 | story flags; 14 test the story part |
| 2 | `FIELD@0x136` on entry, unless `debug_map_mode` `G[0x386a6f]` is set (`FIELD@0x128`); `TOWN@0x34` on entry. `FIELD@0x166` sets 0 after. | 61 | 61 | all test the story part and the place `current_place_id` |
| 3 | `TOWN@0x51d`, once for each site in a list TOWN walks (`TOWN@0x4f5`..`0x545`), after `TOWN@0x518` sets `town_site_id` `G[0x3149]` to it; the walk stops when a rule plays a scene | 102 | 86 | all test `town_site_id` and the place; 101 the story part; 79 a story flag |
| 4 | no writer | 0 | — | the list at `SCJUMP@0x3a02` is empty |
| 5 | `BTL@0x7065`, after a battle when `G[0x952ab]` is 1 and `G[0x9529d]` ≤ 0; restored at `BTL@0x707a` | 4 | 169, 362, 649, 689 | 3 test the place, 1 the story part |
| 6 | `MENU@0x585`, the escape command, after the question of `confirm_escape`; restored at `MENU@0x59a` | 2 | 568, 760 | the place and a story flag |

`$1$SCJUMP` has two rules. In context 1 it plays scene 0 while `G[0x470]` is not 1 (`$1$SCJUMP@0x57`). In context 5
it plays scene 100 every time (`@0x86`).

The story part, `story_part` `G[0x30b0]`, is 1..4. `TITLE@0x633` and `GAMESTART@0x665` set 1. `SC0180@0x11ae0` sets
2, `SC0500@0x129a8` sets 3, and `SC0390@0x1b880` sets 4, as `GAMESTART@0x423` does for 特別編. The world map follows
it (the world, below). It is not the chapter: the chapter names 序章 … come from `GS[0xdc]`, which each scene sets
(`SC0000@0x7be`).

**Story flags from 1900.** `story_flags` `G[0x470]` holds 10000 flags, one a scene id. No scene has an id from 1900 to
1999, and the game keeps the key items there: `[1900 + item]` is 1 while key item `item` is held,
`item_key_held_flags` (`ADDITEM@0x78`..`0xe7`, items below). No SCJUMP rule tests one. Map scripts read the keys
1..3 (`MP02B0207`, `MPEV02B0101` and others), and ARENA reads `[1928 + rank]` for its passes (the arena, below).
Flag 9999 is tested by AMINIT, `$1$AMINIT` and `SCJUMP@0x307e`, and no script writes it by its address.

### What is saved

- **Saved:** `current_scene_script_id` `G[0x454]`, the story flags `G[0x470]` … `G[0x2b7f]` (with the key items
  held), `scjump_context_id` `G[0x2fd2]`, `story_part` `G[0x30b0]` and `town_site_id` `G[0x3149]`.
- **Shared profile:** the scenes seen, `scene_seen_flags` `G[0x8f56d + scene]` for scenes 0..1999. They also lie
  below the prefix, so a save carries a copy.
- **Rebuilt at boot or not kept:** SCINIT's arrays, `current_scene_id` `G[0x91869]`, `played_scene_id`
  `G[0x9186a]`, `scene_over_map` `G[0x91052]`, `scene_run_mode` `G[0x386a6d]`, `debug_map_mode` `G[0x386a6f]` and
  `in_room` `G[0x9100e]`.

## Units, characters and speakers

INIT2 runs CBINIT, EBINIT and CNINIT first, CIINIT and CVINIT later; `$1$AUTORUN` runs `$1$CBINIT` (the same writes
as CBINIT), `$1$EBINIT`, `$1$CNINIT` and the empty `$1$CIINIT` and `$1$CVINIT`. Then `INIT2@0xb9`..`0xeb` runs
CALCREVISE for each character 1 to 99 who has joined. INIT runs CHINIT on every root run (`SYSTEM4@0x2dd`,
`INIT@0x0`), and `GAMESTART@0x3ab`..`0x3b2` runs `$1$CHINIT` for a new 特別編 game.

No array of these six tables lies below the saved prefix except CHINIT's. So a change to EBINIT, CBINIT, CNINIT,
CVINIT or CIINIT reaches a loaded game at once, and only what the scripts copy out of them is saved ("What is saved"
below).

### EBINIT (units)

Leader `GS[0x616]` (the full name), capacity 1000. 375 records, 302 in the base and 73 from `$1$EBINIT`, for 372
unit ids, 1 to 950. The unit id is the record key. A party character fights as a unit too: CHINIT names the unit
(below). SETEN builds an enemy battle slot from a unit; its input is `G[0x9540c]` (`SETEN@0x1c`), and the slot is
`G[0x38937c]`. SETCH builds a party slot from the saved character state and reads a few unit fields live.

Correction to the layout: the first integer array starts at `G[0x9540d]`, not at the tool's `G[0x95402]`.
`G[0x95402]` is CBINIT's HP, MP and TP percent (its `G[0x95402 + form]`), and `G[0x9540c]` is SETEN's input.
`0x9540d + 30 × 1000 = 0x9c93d`, the next array. The tool's `G[0x95402] †` row is this voice array seen 11 cells
early: its column c is the real column c − 11, and its columns below 11 belong to the id before. The second row
below is recomputed from the image. Every other base is ≡ `0x9540d` modulo 1000. The integer arrays end at
`G[0xbc8f4]`; SCINIT starts at `G[0xbc8f6]`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x95402]` † | 30 | 0..29 | 59 | 9543..18871, 336 distinct | *(the voice array `G[0x9540d]`, next row, seen 11 cells early)* |
| `G[0x9540d]` | 30 | 0..26 | 49 | 9543..18871, 336 distinct | Voice clips. `BTL@0x19d2` plays column 17, `BTL@0x3bf0` column 21, `CHANGECH@0xb80` column 20; `BTL@0x3863` sets column 18 and `label_522a` plays it (`BTL@0x5264`) when a slot leaves the battle (`BTL@0x36ef` clears its bit 0), the fall. `BTL@0x1067`, `ANIME@0xf70` and `@0x1017`, `RESULT@0x115c` play computed columns. `BTL@0x19aa` mutes the clip when `voice_mute_switches[unit_voice_settings[speaker_voice_units[unit]]]` is set. |
| `G[0x9c93d]` | 3 | 0..2 | 270 | 12172..16777273, 218 distinct | Pictures. Column 0 is the bestiary's portrait (`INFO3@0xbfb`, surface 0x7f) and a battle picture (`BTL@0x6069`, `@0x6141`, surface 0x30); column 1 `BTL@0x5f6b` (surface 0x2f); `CHANGECH@0x1038`. |
| `G[0x9d4f5]` | 10 | 0..9 | 270 | 12175..16777270, 387 distinct | Battle sprite sheets. `BTL@0x1cda` loads column 0 into surface 0x34 + slot; `BTL@0x1610`, `@0x16e4`, `@0x1723` read columns 3, 5, 7. `DRAWCHP@0x193` draws column 0, or column 9 when the character's condition 3 (石化) `G[0x8c42e + 40 × character + 3]` is set (`DRAWCHP@0x177`). Also `INFO3@0xb99`, `CALCILL@0x669` (column 9). Only 26 units fill columns 1..9. |
| `G[0x9fc05]` | 1 |  | 26 | 12185..13043, 19 distinct | The status screen's portrait of a party unit (`DRAWCHP@0x150`, surface 0x85). |
| `G[0x9ffed]` | 1 |  | 206 | 10..2981, 206 distinct | The bestiary's order: `INFO3@0x706` sorts by it, and `INFO3@0x738` lists only units with a value above 0. |
| `G[0xa03d5]` | 1 |  | 306 | 3..3, 1 distinct 3×306 | The FS temperament. `CALCDAMAGE@0x9a6` turns it into the FS change `G[0x952aa]`: 1 gives −2, 2 gives −1, other values +1. `BTL@0x250b`..`0x266a` adds +3/+2/+1, subtracts 5/3, or adds +10/+5 by it. Every unit has 3. |
| `G[0xa07bd]` | 1 |  | 270 | 1..3, 3 distinct 1×244, 2×15, 3×11 | The unit's kind: 2 for party humans (セリカ, サティア, カヤ, ダルノス…), 3 for summoned partners (パズモ, ペルル, リタ, ナベリウス…), 1 for an enemy; the 102 units EBINIT writes no kind for are names with no battle body (speakers: 村人, 男の声). A party unit's battle sprite (kinds 2 and 3) is a strip of six 120 × 120 frames, an enemy's one picture. `CHANGECHAPTER@0x466` and `LEAVECH@0x142` run UNSUMMON for kind 3; `UNITECH@0x2b7` sets the character's flag bit 2 for kind 3; CHMODE1/2/3 and `CHANGECH@0xc9c` pick a picture at (kind − 2) × 45; `DRAWENLIST@0x268` a label row at kind × 12. |
| `G[0xa0ba5]` | 1 |  | 80 | 1..5, 5 distinct 1×24, 2×4, 3×15, 4×17, 5×20 | The boss class. `BTL@0x100`..`0x17b` plays battle music 0x17 + the highest class among the enemy slots (ordered by an inline array). `CALCILL@0x30b` returns early for a target with a class, so actions of target kind 1 miss it. `BTL@0x38a8`, `@0x3a2b`, `@0x3f13` test it too. |
| `G[0xa0f8d]` | 1 |  | 270 | 1..5, 4 distinct 1×135, 2×59, 3×65, 5×11 | The unit's size. `SETENP@0x4ca`..`0x679` places it in a 5-wide grid (5 − size positions); `SELTARGET@0xb33` and `TACTICS@0xc9a` scale its sprite by it when it is above 1; `INFO3@0xbce` moves the bestiary sprite down 24 per size; `DRAWENLIST@0x164` by 48. Size 5 is giants and 魔神. |
| `G[0xa175d]` | 1 |  | 272 | 1..14, 14 distinct | The defense element. `INFO3@0x1156` shows `GS[0x8028 + value]` (防御属性); `SETEN@0x82b` copies it into the slot's `G[0x389701]` when the armor gives none (`G[0x1366fd]`). |
| `G[0xa1b45]` | 1 |  | 23 | 1001..1702, 15 distinct | The starting weapon (an item id). `SETEN@0x49b` puts it in the slot's `G[0x38962f]`; `UNITECH@0x26f` copies it into the character's saved weapon. |
| `G[0xa1f2d]` | 1 |  | 9 | 2801..2806, 3 distinct 2801×7, 2805×1, 2806×1 | The starting shield (2801 is 小さな円形の盾). `SETEN@0x4b3` → `G[0x38964d]`; `UNITECH@0x282` → saved. |
| `G[0xa2315]` | 1 |  | 56 | 2001..2799, 18 distinct | The starting armor. `SETEN@0x4a7` → `G[0x38966b]`; `UNITECH@0x295` → saved. |
| `G[0xa26fd]` | 1 |  | 2 | 2923..2933, 2 distinct 2923×1, 2933×1 | The starting accessory. `SETEN@0x4bf` → `G[0x389689]`; `UNITECH@0x2a8` → saved. |
| `G[0xa2ae5]` | 1 |  | 62 | 8192..123328, 15 distinct | The item categories the unit may equip: bit `G[0x12cabd + item]` (`CHMODE1@0x18ce` for equipment, `SHOP@0x3a1c`). |
| `G[0xa2ecd]` | 9 | 0..7 | 26 | 4..46, 24 distinct | The unit's skill classes, up to 9: item class ids named by `GS[0x803c + id]` (小型武器, 魔術・電撃, 必殺・飛燕…). `ADDSKILL@0x31`, `CHECKACT@0x69`, `CHMODE2@0x14d1`, `CHMODE1@0x1a6a`, `BTL@0x2b77` match a skill's class `G[0x12f1cd]` against them. |
| `G[0xa51f5]` | 9 | 0..7 | 26 | 1..1, 1 distinct 1×103 | 1 if the class is open when the character joins: `UNITECH@0x1cc` copies the starting rank, and `UNITECH@0x22c` clears the rank of a class with 0. |
| `G[0xa751d]` | 9 | 0..7 | 26 | 1..7, 7 distinct | The class's starting rank. `UNITECH@0x1de` copies it into the saved rank `G[0x8bace + 9 × character + k]` and zeroes the saved class experience `G[0x8be52]`. |
| `G[0xa9845]` | 9 | 0..7 | 26 | 1..7, 3 distinct 1×3, 6×1, 7×107 | The class's highest rank. `BTL@0x6e25` raises a saved rank only below it; `BTL@0x6e87` uses a 12/10 threshold when it is below 7. `CHMODE2@0x1549`, `CHMODE3@0xf7f` and `RESULT@0x6df` read it too. |
| `G[0xabb6d]` | 1 |  | 26 | 1..12, 12 distinct | The support effect the unit gives (支援効果), an SSINIT id (`CHMODE2@0x1205`, `@0x2651`, `@0x2661`). `BTL@0x4bed` counts each party member's effect in `G[0x95311 + id]`; `CHMODE4@0x1d57`. |
| `G[0xad2dd]` | 1 |  | 269 | 10..500, 103 distinct | The base level: the level of the base stats. `SETEN@0x178`, `@0x1c0`; `UNITECH@0x8d` makes it a joining character's level; `INFO3@0xd48`. |
| `G[0xad6c5]` | 1 |  | 268 | 30..500, 64 distinct | The highest level an enemy reaches (`SETEN@0x157`). |
| `G[0xadaad]` | 1 |  | 247 | 1..3, 3 distinct 1×243, 2×3, 3×1 | The divisor of the enemy's level catch-up toward the party (`SETEN@0x68`..`0x107`). |
| `G[0xade95]` | 1 |  | 272 | 8..200, 27 distinct | The starting FS (戦意) of an enemy slot (`SETEN@0xa49` → `G[0x3893f5]`); `INFO3@0xd06`, `@0xef4`. |
| `G[0xae27d]` | 1 |  | 272 | 16..200, 25 distinct | The maximum FS. `SETEN@0x1da` → `G[0x389413]`, at least 2 (`SETEN@0x837`); `SETCH@0x47`; `CALCREVISE@0x337` for a character; `INFO3@0xfcf`. |
| `G[0xae665]` | 1 |  | 269 | 5200..248000000, 215 distinct | The base max HP ×100, at the base level (Stats, below). |
| `G[0xaea4d]` | 1 |  | 163 | 2500..160000, 97 distinct | The base max MP ×100, at the base level (Stats, below). |
| `G[0xaee35]` | 1 |  | 227 | 500..140000, 128 distinct | The base max TP ×100, at the base level (Stats, below). |
| `G[0xaf21d]` | 1 |  | 267 | 2600..176000, 189 distinct | The base 攻撃力 ×100, at the base level (Stats, below). |
| `G[0xaf605]` | 1 |  | 267 | 900..92000, 154 distinct | The base 防御力 ×100, at the base level (Stats, below). |
| `G[0xaf9ed]` | 1 |  | 159 | 400..152000, 117 distinct | The base 魔法攻撃 ×100, at the base level (Stats, below). |
| `G[0xafdd5]` | 1 |  | 267 | 600..350000, 173 distinct | The base 魔法防御 ×100, at the base level (Stats, below). |
| `G[0xb01bd]` | 1 |  | 265 | 200..9800, 57 distinct | The base 攻撃回数 ×100, at the base level (Stats, below). |
| `G[0xb05a5]` | 1 |  | 267 | 100..8000, 52 distinct | The base 防御回数 ×100, at the base level (Stats, below). |
| `G[0xb098d]` | 1 |  | 267 | 200..99900, 78 distinct | The base 肉体抵抗 ×100, at the base level (Stats, below). |
| `G[0xb0d75]` | 1 |  | 267 | 100..99900, 75 distinct | The base 精神抵抗 ×100, at the base level (Stats, below). |
| `G[0xb115d]` | 1 |  | 267 | 100..9900, 45 distinct | The base 肉体戦速 ×100, at the base level (Stats, below). |
| `G[0xb1545]` | 1 |  | 268 | 100..9900, 48 distinct | The base 精神戦速 ×100, at the base level (Stats, below). |
| `G[0xb192d]` | 1 |  | 259 | 100..7200, 53 distinct | The base 運 ×100, at the base level (Stats, below). |
| `G[0xb1d15]` | 1 |  | 262 | 80..200000, 95 distinct | The max HP growth ×100 a level. |
| `G[0xb20fd]` | 1 |  | 136 | 50..400, 44 distinct | The max MP growth ×100 a level. |
| `G[0xb24e5]` | 1 |  | 196 | 25..350, 44 distinct | The max TP growth ×100 a level. |
| `G[0xb28cd]` | 1 |  | 262 | 100..500, 51 distinct | The 攻撃力 growth ×100 a level. |
| `G[0xb2cb5]` | 1 |  | 262 | 23..250, 54 distinct | The 防御力 growth ×100 a level. |
| `G[0xb309d]` | 1 |  | 155 | 50..320, 37 distinct | The 魔法攻撃 growth ×100 a level. |
| `G[0xb3485]` | 1 |  | 262 | 25..450, 66 distinct | The 魔法防御 growth ×100 a level. |
| `G[0xb386d]` | 1 |  | 260 | 8..32, 19 distinct | The 攻撃回数 growth ×100 a level. |
| `G[0xb3c55]` | 1 |  | 262 | 5..25, 20 distinct | The 防御回数 growth ×100 a level. |
| `G[0xb403d]` | 1 |  | 244 | 10..50, 23 distinct | The 肉体抵抗 growth ×100 a level. |
| `G[0xb4425]` | 1 |  | 244 | 5..90, 28 distinct | The 精神抵抗 growth ×100 a level. |
| `G[0xb480d]` | 1 |  | 263 | 4..32, 21 distinct | The 肉体戦速 growth ×100 a level. |
| `G[0xb4bf5]` | 1 |  | 263 | 4..25, 20 distinct | The 精神戦速 growth ×100 a level. |
| `G[0xb4fdd]` | 1 |  | 263 | 1..28, 25 distinct | The 運 growth ×100 a level. |
| `G[0xb53c5]` | 9 | 0..8 | 372 | 0..999, 207 distinct | The RTINIT routine of each tactic (作戦). `CHMODE2@0x11d8` lists tactic k (0..7) when column k is set and names it `GS[0x812f + k + 1]` (通常攻撃型, おまかせ型, 攻撃重視型, 支援重視型, 生存重視型, 魔法重視型, 必殺重視型, ランダム型; `CHMODE2@0x227e`). `BTL@0x1401` puts the column of the character's saved tactic `G[0x8e36e]` (`BTL@0x13e6`) into `routine_in_play` `G[0x35a784]` and runs RTN. Enemies fill column 0 only. |
| `G[0xb76ed]` | 6 | 0..5 | 244 | 157..7207, 236 distinct | The unit's action list, ITINIT ids. `RTN@0x8b` copies the six into `G[0x3866a7]`; when the first is 0, `RTN@0xb0` uses the slot's weapon, or 9999. |
| `G[0xb8e5d]` | 1 |  | 22 | 1..1, 1 distinct 1×22 | The experience curve. `CALCNEXTEXP@0x2b`..`0xb4`: curve 1..5 gives 200, 215, 225, 235, 250 per level; any other value gives no next level. |
| `G[0xb9245]` | 1 |  | 22 | 80..300, 8 distinct | The experience rate in percent, 0 for 100 (`CALCNEXTEXP@0xd9`..`0xf5`). The next level costs curve × level × 124 / 1000 × rate / 100, at least 1 (level capped at 499, `CALCNEXTEXP@0x18`). |
| `G[0xb962d]` | 2 | 0..1 | 224 | 20..70000, 167 distinct | The experience the battle's pool `G[0x952ac]` gets from the unit. `BTL@0x374c` uses column 0 when the slot's HP `G[0x389431]` is 0, else column 1 (`BTL@0x379e`, `@0x3835`). `RESULT@0x68a` hands each character's pool to ADDEXP. |
| `G[0xb9dfd]` | 1 |  | 136 | 15..100000, 74 distinct | The money the unit gives. `RESULT@0x433` sums it over the defeated slots; `RESULT@0x11cf` adds it to `money` `G[0x8b159]`, at most 9999999. |
| `G[0xba1e5]` | 5 | 0..4 | 218 | 1..100, 14 distinct | Each drop's chance in percent. `RESULT@0x49b`..`0x4b2` draws 0..99 and takes the first drop whose chance beats the draw. |
| `G[0xbb56d]` | 5 | 0..4 | 218 | 3..2942, 202 distinct | The drop items (item ids). `RESULT@0x489`, `@0x509`; ADDITEM at `RESULT@0x11c9`. |
| `GS[0x616]` | 1 |  | 372 | セリカ・シルフィル / セリカ・シルフィル / 神殺しハイシェラ | The full name. No script reads it in bg0. |
| `GS[0x9fe]` | 1 |  | 372 | セリカ / セリカ / ハイシェラ | The name the game shows (331 scripts; `INFO3@0x1011`). |
| `GS[0xde6]` | 1 |  | 365 | バリハルト魔法剣士 / 放浪の女神体 / 大いなる力を得た暴君 | The epithet under the name (CHANGECH, CHMODE1..4, DRAWCHP). |
| `GS[0x11ce]` | 2 | 0..1 | 270 | 魔法と必殺技が両立 / HPなど大幅上昇 / 女神体のハイシェラ | Two lines on how the unit fights (`INFO3@0x1185`, `@0x1199`; DRAWENP). |

Two arrays sit in the block but EBINIT writes neither: `G[0xa1375]` (1000 cells, no script reads it) and
`G[0xabf55]` (5 cells per unit, which `CHMODE2@0x124a` lists with the support effect, so they look like room for
more support effects). They are left unnamed.

**Stats.** SETEN sets an enemy slot's stat to (base + growth × (level − base level)) / 100. CALCLVUP adds growth ×
levels gained to the character's saved stat, capped at 999900 for HP, MP and TP and 99900 for the rest. The
meanings rest on the status screen's labels (INFO3) and on items whose descriptions name the stat.

| stat | base | growth | slot (SETEN) | saved (CALCLVUP) | evidence |
|---|---|---|---|---|---|
| max HP | `G[0xae665]` | `G[0xb1d15]` | `G[0x38944f]` (`SETEN@0x1e6`) | `G[0x8b48e]` (`CALCLVUP@0x1f`) | `CALCREVISE@0x0` |
| max MP | `G[0xaea4d]` | `G[0xb20fd]` | `G[0x38948b]` (`SETEN@0x210`) | `G[0x8b556]` (`CALCLVUP@0x42`) | CALCREVISE |
| max TP | `G[0xaee35]` | `G[0xb24e5]` | `G[0x3894c7]` (`SETEN@0x23a`) | `G[0x8b61e]` (`CALCLVUP@0x65`) | CALCREVISE |
| 攻撃力 | `G[0xaf21d]` | `G[0xb28cd]` | `G[0x3894e5]` (`SETEN@0x264`) | `G[0x8b682]` (`CALCLVUP@0x88`) | `INFO3@0x108a`, `DRAWCHP@0xb7a` |
| 防御力 | `G[0xaf605]` | `G[0xb2cb5]` | `G[0x389503]` (`SETEN@0x28e`) | `G[0x8b6e6]` (`CALCLVUP@0xab`) | `INFO3@0x10c0` |
| 魔法攻撃 | `G[0xaf9ed]` | `G[0xb309d]` | `G[0x389521]` (`SETEN@0x30c`) | `G[0x8b74a]` (`CALCLVUP@0xce`) | `INFO3@0x10f6` |
| 魔法防御 | `G[0xafdd5]` | `G[0xb3485]` | `G[0x38953f]` (`SETEN@0x336`) | `G[0x8b7ae]` (`CALCLVUP@0xf1`) | `INFO3@0x1111` |
| 攻撃回数 | `G[0xb01bd]` | `G[0xb386d]` | `G[0x38955d]` (`SETEN@0x2b8`) | `G[0x8b812]` (`CALCLVUP@0x114`) | `INFO3@0x10a5` |
| 防御回数 | `G[0xb05a5]` | `G[0xb3c55]` | `G[0x38957b]` (`SETEN@0x2e2`) | `G[0x8b876]` (`CALCLVUP@0x137`) | `INFO3@0x10db` |
| 肉体抵抗 | `G[0xb098d]` | `G[0xb403d]` | `G[0x389599]` (`SETEN@0x360`) | `G[0x8b8da]` (`CALCLVUP@0x15a`) | 地脈の衣 "肉体抵抗が高い" (item bonus `G[0x3260ad]`); `CALCILL@0x38d` |
| 精神抵抗 | `G[0xb0d75]` | `G[0xb4425]` | `G[0x3895b7]` (`SETEN@0x38a`) | `G[0x8b93e]` (`CALCLVUP@0x17d`) | 魔術師のローブ "精神抵抗+2" (`G[0x3287bd]`); `CALCILL@0x34b`, `CALCDAMAGE@0x373` |
| 肉体戦速 | `G[0xb115d]` | `G[0xb480d]` | `G[0x3895d5]` (`SETEN@0x3b4`) | `G[0x8b9a2]` (`CALCLVUP@0x1a0`) | 剣の耳飾り "肉体戦速+1" (`G[0x32aecd]`); `BTL@0x17dd` |
| 精神戦速 | `G[0xb1545]` | `G[0xb4bf5]` | `G[0x3895f3]` (`SETEN@0x3de`) | `G[0x8ba06]` (`CALCLVUP@0x1c3`) | 杖の耳飾り "精神戦速+1" (`G[0x32d5dd]`); `BTL@0x17c7` for damage kinds 2, 3 and 5 |
| 運 (low confidence) | `G[0xb192d]` | `G[0xb4fdd]` | `G[0x389611]` (`SETEN@0x408`) | `G[0x8ba6a]` (`CALCLVUP@0x1e6`) | a hit-chance term in `CALCDAMAGE@0x2b6`; party and enemy averages at `BTL@0x4d86`, `@0x4df8`; 神刀フォルトゥナ gives 20 |

### CHINIT (characters)

CHINIT writes no records into a table area. It sets the saved `character_unit_ids` `G[0x8b236 + character]`, the
character's EBINIT unit, for characters 1..9 (1 セリカ, 100 サティア, 10 ダルノス, 103 カヤ, 45 アビルース,
155 シャマーラ, 140 レクシュミ, 200 白銀公, 509 ハイシェラ) and 51..57 (333 パズモ, 224 リ・クティナ, 240 ペルル,
127 リタ, 506 アムドシアス, 704 空の勇士, 508 ナベリウス). `$1$CHINIT` changes character 51 to unit 638 and 53 to
637. The array holds 100 cells; the next array starts at `G[0x8b29a]`. 342 scripts read it, among them
`UNITECH@0x1`, `CALCLVUP@0x0`, `CALCREVISE@0x330`, `CALCNEXTEXP@0x0`, `DRAWCHP@0x12f` and
`CHMODE2@0x293`.

The story changes it too. `CHANGECHAPTER@0x40` sets セリカ's unit `G[0x8b237]` to `lead_unit_id` `G[0x30b1]`
(1..4, セリカ's four forms), which `CHANGECHAPTER@0x6`..`0x3d` derives from `story_part`.
`SC0180@0x19194` sets character 51's unit to 334.

The array lies below the saved prefix, so the save keeps it. INIT runs CHINIT on every root run, and a load then
overwrites it, so a change to CHINIT reaches only new games. The tool has no leader for CHINIT, and it needs none:
it writes 16 cells, not records. The registry's `character` table keys this saved array by character id.

The saved character state, all keyed by character id (100 cells per array unless said):

- `character_flags` `G[0x8b1c1]`. Bit 0 (1): the character has joined (`UNITECH@0x57`); INIT2 runs CALCREVISE
  for these at boot. Bit 1 (2): in the party (`UNITECH@0x31e` sets it, `LEAVECH@0xa8` clears it;
  `ADDEXP@0x8` gives experience only to these). Bit 2 (4): `UNITECH@0x2d3` sets it for a kind-3 unit,
  `UNSUMMON@0x7` too; not decoded.
- `character_levels` `G[0x8b2fe]`; `character_experience` `G[0x8b362]` (`ADDEXP@0x27`; 現在経験値 on
  `CHMODE2@0x1c5d`).
- `G[0x8b42a]` current HP in points (`CALCREVISE@0x13d` compares it with the derived max) and `G[0x8b48e]` max HP
  ×100; `G[0x8b4f2]` / `G[0x8b556]` MP; `G[0x8b5ba]` / `G[0x8b61e]` TP, the same way; `G[0x8b682]` … `G[0x8ba6a]`
  the 11 other stats (the table above).
- `G[0x8b3c6]` an FS offset that `CALCREVISE@0x3c4`..`0x403` caps at max − max / 2. Not decoded further.
- `character_skill_ranks` `G[0x8bace]`, 9 per character, and `character_skill_experience` `G[0x8be52]`, 9 per
  character (`UNITECH@0x1e9`, `@0x1f9`; `BTL@0x6e08`..`0x6ee4` raises a rank when the experience reaches the
  threshold).
- Six equipment slots: `character_weapons` `G[0x8c1d6]`, the shield `G[0x8c302]`, the armor `G[0x8c366]` and the
  accessory `G[0x8c3ca]`, which `SETCH@0x1f1`..`0x2af` puts in the slot's `G[0x38962f]`, `G[0x38964d]`,
  `G[0x38966b]` and `G[0x389689]`; and `G[0x8c23a]` and `G[0x8c29e]`, two more items SETCH and CALCREVISE read, not
  decoded. LOSTITEM clears an item from all six (items, below).
- `character_tactics` `G[0x8e36e]`, the chosen tactic (`BTL@0x13e6`; `CHMODE2@0x225f` marks it).
- The conditions, 40 cells per character by ILINIT id: the level `G[0x8c42e]`, the time left `G[0x8d3ce]` and the
  floor `G[0x8e5c6]` (ILINIT, below).

### CALCREVISE (a character's derived values)

CALCREVISE recomputes the character `character_arg_id` `G[0x8b158]` from saved state, its unit and its six equipment
items. Max HP `G[0x389188 + character]` is `G[0x8b48e]` / 100 plus the items' `G[0x34356d]`, clamped to 1..9999;
the current HP `G[0x8b42a]` is capped by it, or filled when `map_entered` `G[0x317c]` is 0. MP works the same into
`G[0x3891ec]` (`G[0x8b556]`, items' `G[0x345c7d]`, current `G[0x8b4f2]`), and TP into `G[0x389250]` (`G[0x8b61e]`,
`G[0x34838d]`, `G[0x8b5ba]`). Max FS `G[0x3892b4]` is the unit's `G[0xae27d]` (`CALCREVISE@0x337`) plus the
items' `G[0x34aa9d]`, at least 2. `CALCREVISE@0x40a`..`0x620` then sets the character's condition floors
`G[0x8e5c6]` and levels `G[0x8c42e]` from the lasting conditions `G[0x2ae69d + 40 × item + k]` of the six equipped
items, clamped to 0..5.

### CBINIT (セリカ's forms)

Leader `G[0x95376]`, capacity 10. 8 records, 4 in the base and the same 4 again from `$1$CBINIT`. Rows 1..4 are
セリカ's forms, `lead_unit_id` (`CHANGECHAPTER@0x6`..`0x3d`). The 15 arrays sit 10 cells apart. All four rows
hold the same values except the last array.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x95376]` | 1 |  | 4 | 48..48, 1 distinct 48×4 | 筋力 minimum |
| `G[0x95380]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 筋力 cost percent |
| `G[0x9538a]` | 1 |  | 4 | 41..41, 1 distinct 41×4 | 体力 minimum |
| `G[0x95394]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 体力 cost percent |
| `G[0x9539e]` | 1 |  | 4 | 36..36, 1 distinct 36×4 | 敏捷性 minimum |
| `G[0x953a8]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 敏捷性 cost percent |
| `G[0x953b2]` | 1 |  | 4 | 42..42, 1 distinct 42×4 | 器用度 minimum |
| `G[0x953bc]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 器用度 cost percent |
| `G[0x953c6]` | 1 |  | 4 | 26..26, 1 distinct 26×4 | 知性 minimum |
| `G[0x953d0]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 知性 cost percent |
| `G[0x953da]` | 1 |  | 4 | 24..24, 1 distinct 24×4 | 精神力 minimum |
| `G[0x953e4]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 精神力 cost percent |
| `G[0x953ee]` | 1 |  | 4 | 26..26, 1 distinct 26×4 | 魅力 minimum |
| `G[0x953f8]` | 1 |  | 4 | 100..100, 1 distinct 100×4 | 魅力 cost percent |
| `G[0x95402]` | 1 |  | 4 | 100..200, 4 distinct 100×1, 150×1, 180×1, 200×1 | HP, MP and TP percent |

- The minimum `G[0x95376 + 20k + form]`: `CHANGECHAPTER@0x45`..`0x127` raises the saved parameter
  `lead_parameters` `G[0x8b228 + k]` to it. The parameters are 筋力, 体力, 敏捷性, 器用度, 知性, 精神力, 魅力, by the
  labels at `CHMODE2@0x1adf`..`0x1b39`.
- The cost percent `G[0x95380 + 20k + form]`: `CHMODE2@0x298`..`0x2e0` reads it, and `CHMODE2@0x1758` adds
  percent × value / 100 to the cost of raising the parameter.
- The HP, MP and TP percent `G[0x95402 + form]`: `CALCCHP@0x244` scales HP's 体力 term by it, `CALCCHP@0x298`
  MP's and `@0x2d7` TP's. `CHANGECHAPTER@0x12c` copies it into the saved `G[0x8b227]`.

CALCCHP computes セリカ's level as `G[0x8b2ff]` = (sum of the parameters − 1) / 14 + 1 and his HP, MP and TP from
the seven parameters.

### CNINIT (speakers) and CVINIT (voice settings)

CNINIT: leader `GS[0xe0]`, capacity 1000. CNINIT and `$1$CNINIT` write 375 records, 372 names and 370 voice units.
The speaker id is the unit id.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x91059]` | 1 |  | 370 | 1..950, 220 distinct | The unit whose voice setting the speaker uses. `SC0470@0xbca`..`0xbdf` mutes a line when `voice_mute_switches[unit_voice_settings[this]]` is set. Battle reads it by unit: `BTL@0x19aa`, `ANIME@0xcf0`, `CHANGECH@0xb58`, `RESULT@0x1134`. |
| `GS[0xe0]` | 1 |  | 372 | セリカ / セリカ / ハイシェラ | The name plate (`SC0470@0xb81`, `HISTORY@0x105f`). |

The page's speaker is `adv_speaker_id` `G[0x2fd0]`, −1 for narration. Each spoken page sets the shared-profile flag
`speaker_heard_flags` `G[0x8fd3d + speaker]` to 1 and stores it (`SC0470@0xb62`..`0xb75`); CONFIG, INFO2 and
CIINIT read it below.

CVINIT: leader `G[0x91847]`, capacity 30; 16 settings, ids 0..15. `0x91847 + 30 = 0x91865`, the shared argument
`script_arg_1`, which ADDEXP reads as its input (`ADDEXP@0x16`), so 30 is the capacity. `$1$CVINIT` is empty.
CVINIT also writes the voice unit map `G[0x91441]`, 1000 cells keyed by unit, which ends at `G[0x91829]`. The tool
reads it as `G[0x9149b]` at stride 5 and leaves 6 of its cells outside (`G[0x91531]`, `G[0x9158e]`, `G[0x91639]`,
`G[0x9163b]`, `G[0x9163d]`, `G[0x91700]`: units 240, 333, 504, 506, 508 and 703). The last row below is
recomputed from the image.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x9149b]` | 5 | 0..3 | 8 | 1..15, 9 distinct | *(the unit map `G[0x91441]`, last row)* |
| `G[0x91829]` | 1 |  | 15 | 84..703, 15 distinct | The unit that reveals the setting's row: `CONFIG@0x10e7` reads its heard flag `G[0x8fd3d + unit]`. The row labels are pictures (sheet 0x89, `CONFIG@0x1137`, `@0x1175`). Setting 0 has none. |
| `G[0x91847]` | 1 |  | 16 | 266..18652, 16 distinct | The sample voice clip the 設定 voice page plays (`CONFIG@0xe90`). |
| `G[0x91441]` | 1, by unit |  | 15 | 1..15, 15 distinct | `unit_voice_settings`: the voice setting of a voice unit (the value of `speaker_voice_units`), units 100..703. |

The settings: 0 the default; 1 サティア (unit 100); 2 ハイシェラ (row unit 509, voice unit 504); 3 カヤ (103);
4 パズモ (333); 5 リタ (row unit 84, voice unit 126); 6 リ・クティナ (220); 7 ペルル (240); 8 空の勇士 (703);
9 ナベリウス (508); 10 白銀公 (200); 11 アムドシアス (506); 12 シャマーラ (155); 13 レクシュミ (140);
14 ルナ＝クリア (160); 15 古神アイドス (165).

### CIINIT (profiles)

Leader `G[0x3acd41]`, capacity 100; 51 profiles. INFO2 is the 人物 book.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x3acd41]` | 1 |  | 51 | 1..703, 51 distinct | The profile's unit. `INFO2@0x844` lists profiles 1..99 with a unit; `INFO2@0x876` loads its heard flag `G[0x8fd3d + unit]`; `INFO2@0x860` marks it heard when `debug_map_mode` is set. |
| `G[0x3acda5]` | 1 |  | 51 | 76..18620, 51 distinct | The picture (`INFO2@0xe0f`). |
| `G[0x3ace09]` | 1 |  | 48 | -170..240, 8 distinct | The picture's x offset (`INFO2@0xe40`). |
| `G[0x3ace6d]` | 1 |  | 5 | -203..-203, 1 distinct -203×5 | The picture's y offset (`INFO2@0xe53`). |
| `GS[0x8177]` | 1 |  | 51 | セリカ / サティア / アストライア | The name, shown only once the unit is heard (`INFO2@0xc45`, `@0xc8b`). |

`G[0x3acd40]` is the profile on screen (`INFO2@0xde7`); `CIMES@0x6` writes its text. Not saved.

### What is saved

UNITECH runs when a character joins (`character_arg_id` = character). The first time (flag bit 0 clear,
`UNITECH@0x34`..`0x49`), when the level is 0 or bit 0 of `G[0x2fe8]` (its log calls it LoadClear,
`UNITECH@0x19`) is clear (`UNITECH@0x5c`..`0x7f`), it copies from the character's unit: the base level into
`character_levels` (`UNITECH@0x8d`), experience 0, the 14 base stats into the saved stats
(`UNITECH@0xa5`..`0x1aa`), the open classes' starting ranks, and the four starting equipment items
(`UNITECH@0x268`..`0x2af`). CALCLVUP reads the growth live at every level-up. So a changed base stat or starting
equipment reaches only characters who join after it; a changed growth reaches later level-ups of every character.
SETEN reads everything live, so enemy changes need no new game. CALCREVISE and SETCH read the maximum FS live.

CHINIT's `character_unit_ids` is saved, and so is the saved character state listed under CHINIT. CBINIT itself is not
saved: the save keeps セリカ's parameters `lead_parameters` (raised at a chapter change) and the HP, MP and TP percent
copy `G[0x8b227]`, so a changed minimum reaches a loaded game at the next chapter change. CNINIT, CVINIT and CIINIT
are not saved; the mute switches `voice_mute_switches` are. The shared profile keeps them and the heard flags
`speaker_heard_flags`, which lie below the prefix too, so a save carries a copy.

## Items, conditions and constants

ITINIT is the hub of this part. Its ids name the items, the equipment, the spells, the arts and the enemies' actions.
ILINIT's conditions are the columns of its condition rows. AFINIT names its elements and classes and holds the
affinity matrix. TRINIT hides items in the dungeon, and SHINIT sells them. SSINIT holds only the names of twelve
support effects. INIT2 runs all of them but SHINIT, which SHOP runs each time a shop opens.

### ITINIT (items, equipment, magic and arts)

Leader `GS[0x2e70]` (the name), capacity 10000. 742 records: 658 in the base and 84 from `$1$ITINIT` (ids 1011 to
7207: new weapons, armour and enemy actions). Its 51 arrays agree modulo 10000; none is off the grid and no cell falls
outside them.

The ids fall in ranges by what they are:

- 1..35 key items: 13..20 the eight 陣形の書, 6..12 and 21..27 the 喚石 and 招聘 stones, 28..32 the arena passes;
- 101..242 consumables and 絵札, 251..258 growth stones, 301..303 the 支配者の杖 staffs;
- 1001..1711 weapons, 2001..2345 armour, 2790..2799 enemy-only armour, 2801..2817 shields, 2901..2942 accessories;
- 3001..3137 magic, 4001..4057 arts, 5001..7207 the enemies' actions;
- 9992..9999 special actions: 9998 逃亡, 9999 素手.

ITMES holds every description. It branches on the item's category `G[0x12cabd]` (`ITMES@0x6`) and then on the id.
The descriptions confirm most readings below ("Cランク", "範囲W×H", "命中+30", "/MP30】", "HP+500").

Equipment works through the battle slot. `SETCH@0x2e4`..`0x522` adds each equipped item's numbers into the slot's
arrays, 30 cells apart, the slot being `G[0x38937c]`. SETEN does the same for an enemy. SETILLREVISE writes the same
slot arrays from ILINIT, and the conditions named 肉抵, 精抵, 肉速, 精速, 運, 痛打 and 魔威 write only into the arrays
it shares with these, which confirms the stat names. CHMODE1 decides whether a character may use or equip an item: it
checks the category, the rank, the exclusive users and the menu-use mask.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x12558d]` | 1 |  | 358 | 10..29404, 357 distinct | The list order (`sort-indices-by-key-sum` at `CHMODE1@0x17bb`, `INFO4@0x8b7`, `SHOP@0x1e26`). |
| `G[0x127c9d]` | 1 |  | 22 | 1..1, 1 distinct 1×22 | 1 on growth stones, staffs and some arts. No reader found. |
| `G[0x12a3ad]` | 1 |  | 516 | 1..7, 7 distinct | The rank, 1..7. `CHMODE1@0x1a5c`..`0x1ab1` refuses the item (bit 0x11) when the character's rank in the item's class, `character_skill_ranks` `G[0x8bace + 9 × character + k]`, is below it. Also `CHECKACT@0x8d`, `CHMODE3@0x18f`. The "Cランク" descriptions have 3. |
| `G[0x12cabd]` | 1 |  | 679 | 1..46, 36 distinct | The category: 1 key, 2 consumable, 3 non-battle; weapons 4 杖, 5 投石, 6 短剣, 7 剣, 8 拳, 9 双剣, 10 連接, 11 槍, 12 弓; 13 衣服, 14 鎧, 15 盾, 16 装飾; 21..34 the magic schools; 41..46 the art schools. `ITMES@0x6`; `CHMODE1@0x18c0` tests it against the unit's mask `G[0xa2ae5]`; `SHOP@0x27b0` draws its icon. |
| `G[0x12f1cd]` | 1 |  | 633 | 1..50, 33 distinct | The class, an AFINIT class (`GS[0x803c + class]`): 1 重要, 2 通常, 3 非戦闘時; 4..6 the weapon sizes; 7 軽装, 8 中装, 9 盾, 10 装飾; 21..35 magic; 40 enemy magic; 41..46 arts; 50 enemy arts. `ADDITEM@0x78` (class 1 is a key item); `CALCDAMAGE@0x672` (above 20 the charge level counts). |
| `G[0x1318dd]` | 1 |  | 498 | 1..5, 5 distinct 1×297, 2×164, 3×10, 4×9, 5×18 | The damage kind: 1 physical, 2 magic, 3 a mind check, 4 special, 5 an FS (戦意) attack. `CALCDAMAGE@0x15d`, `@0x352`, `@0x49d`, `@0x731`; `BTL@0x1788` (kinds 2, 3 and 5 use 精神戦速, the others 肉体戦速); `SETCH@0x3e0`. |
| `G[0x133fed]` | 1 |  | 393 | 1..13, 11 distinct | The attack element (`GS[0x8014 + e]`). `CALCDAMAGE@0x1b7` takes the action's, else the weapon's `G[0x3896e3]`; `SETCH@0x548` takes the weapon's, else 1 物理 (`SETCH@0x557`). |
| `G[0x1366fd]` | 1 |  | 84 | 1..14, 13 distinct | The defense element (`GS[0x8028 + e]`). `SETCH@0x56a` takes the armour's, else the unit's `G[0xa175d]` (`SETCH@0x595`), into the slot's `G[0x389701]`; `SETEN@0x7f9` alike. |
| `G[0x138e0d]` | 100 | 0..12 | 145 | 1..57, 16 distinct | The characters who may use it. `CHMODE1@0x19ea`..`0x1a45` refuses it (bit 0x10) unless `character_arg_id` `G[0x8b158]` is listed; `SHOP@0x3a2a`. 253 天王石【セリカ専用】 lists only 1. |
| `G[0x22d04d]` | 5 | 0 | 14 | 6..27, 14 distinct | The item that must be held to use the action: the summons' 喚石 (6..12) and the 招聘 stones (21..27), for actions 3121..3137. `CHECKACT@0xda`, `CHMODE3@0x1595`. |
| `G[0x23939d]` | 1 |  | 8 | 1..1, 1 distinct 1×8 | Cursed: it cannot be taken off (`CHMODE1@0x1c23`, "は外すことが出来ない"). `ADDILL@0x1bf`..`0x2c0` and `CALCILL@0x69`..`0x139` take it off when 呪い is cured. SHOP, SETCHSEL and DRAWCHP read it. |
| `G[0x23baad]` | 1 |  | 556 | 1..4, 4 distinct 1×430, 2×101, 3×17, 4×8 | The target kind: 1 enemies, 2 allies, 3 self, 4 a summon. `CALCDAMAGE@0x19` (any but 1 takes the recovery path); `CALCILL@0x2ef`; `USESKILL@0x10d` (4). |
| `G[0x23e1bd]` | 1 |  | 537 | 1..5, 4 distinct 1×291, 2×50, 3×67, 5×129 | The area's width; the 24 "範囲W×H" descriptions agree. RTN009 reads it. |
| `G[0x2408cd]` | 1 |  | 537 | 1..5, 5 distinct 1×306, 2×50, 3×94, 4×1, 5×86 | The area's height, the same way. |
| `G[0x242fdd]` | 1 |  | 543 | 1..90, 44 distinct | The charge time, less the unit's speed (`BTL@0x177c`); `CHMODE3@0x1160` shows it. |
| `G[0x2456ed]` | 1 |  | 539 | 1..90, 17 distinct | The least charge time (`BTL@0x17f0`). |
| `G[0x247dfd]` | 1 |  | 533 | 1..90, 40 distinct | The recovery time after acting (`BTL@0x2da9`). |
| `G[0x24a50d]` | 1 |  | 531 | 1..90, 11 distinct | The least recovery time (`BTL@0x2e1d`). |
| `G[0x24cc1d]` | 40 | 1..35 | 234 | -5..5, 10 distinct | The condition row, by ILINIT id: the levels the action inflicts (+) or cures (−). `CALCILL@0x51` loops the 40 conditions; the level is the absolute value (`CALCILL@0x256`). 129 女神の滴・大 holds −5 on 8..12, 14, 17 and 19. |
| `G[0x2ae69d]` | 40 | 7..36 | 46 | 1..5, 5 distinct 1×16, 2×16, 3×17, 4×3, 5×32 | The lasting conditions an equipped item gives, by ILINIT id. `SETCH@0x5fc`..`0x61d` adds them into the slot's `G[0x389cbf + 40 × slot + condition]`; SETEN; CALCREVISE sets the character's floors from them. 2927 メチュカの微笑み, "命中+30 痛打+10", holds 5 on conditions 20 and 34. |
| `G[0x31011d]` | 1 |  | 273 | -30..999, 16 distinct | 命中 → the slot's `G[0x38971f]` (`CALCDAMAGE@0x290`); 77 of 80 descriptions agree. |
| `G[0x31282d]` | 1 |  | 91 | -30..100, 29 distinct | 回避 → `G[0x38973d]`; 34 of 37 agree. |
| `G[0x314f3d]` | 1 |  | 201 | -700..9000, 99 distinct | 攻撃力 ("攻撃+") → `G[0x3894e5]` (`CALCDAMAGE@0x5e9`); 71 of 71 agree. |
| `G[0x31764d]` | 1 |  | 84 | 3..286, 57 distinct | 防御力 → `G[0x389503]`; 54 of 54 agree. |
| `G[0x319d5d]` | 1 |  | 154 | -99..100, 21 distinct | 攻撃回数 → `G[0x38955d]` (`CALCDAMAGE@0x560`); 69 of 72 agree. |
| `G[0x31c46d]` | 1 |  | 68 | -3..15, 12 distinct | 防御回数 → `G[0x38957b]`; 44 of 50 agree. |
| `G[0x31eb7d]` | 1 |  | 131 | -130..9999, 66 distinct | 魔法攻撃 → `G[0x389521]` (`CALCDAMAGE@0x520`); for damage kind 5 the FS attack (`SETCH@0x3e0` skips it, `CALCDAMAGE@0x367`). |
| `G[0x32128d]` | 1 |  | 91 | 1..320, 45 distinct | 魔法防御 → `G[0x38953f]`; 67 of 67 agree. |
| `G[0x32399d]` | 1 |  | 113 | -3..200, 31 distinct | 魔威 → `G[0x389779]` (`CALCDAMAGE@0x4f6`); 50 of 51 agree. |
| `G[0x3260ad]` | 1 |  | 36 | 2..20, 9 distinct | 肉体抵抗 → `G[0x389599]`; an action's value adds to the roll (`CALCILL@0x37a`). |
| `G[0x3287bd]` | 1 |  | 48 | 2..20, 11 distinct | 精神抵抗 → `G[0x3895b7]` (`CALCILL@0x338`, `CALCDAMAGE@0x373`). |
| `G[0x32aecd]` | 1 |  | 92 | -5..1, 5 distinct -5×1, -3×17, -2×41, -1×32, 1×1 | 肉体戦速 → `G[0x3895d5]`. |
| `G[0x32d5dd]` | 1 |  | 92 | -5..1, 5 distinct -5×1, -3×17, -2×41, -1×32, 1×1 | 精神戦速 → `G[0x3895f3]`. |
| `G[0x32fced]` | 1 |  | 24 | 2..20, 5 distinct 2×2, 3×2, 5×18, 10×1, 20×1 | 運 → `G[0x389611]`. |
| `G[0x3323fd]` | 1 |  | 143 | 1..50, 17 distinct | 痛打 → `G[0x38975b]` (`CALCDAMAGE@0x3f1`..`0x412`). |
| `G[0x334b0d]` | 1 |  | 32 | -400..99999, 20 distinct | HP restored (+) or spent (−). `CALCDAMAGE@0x33`..`0x8a` caps it at 99999 and adds the random part. 101 holds 497 for "HP+500". |
| `G[0x33721d]` | 1 |  | 163 | -300..9999, 51 distinct | MP restored or spent; a cost is negative, and the 90 "/MPn】" descriptions agree. `CALCDAMAGE@0x8f`. |
| `G[0x33992d]` | 1 |  | 116 | -150..9999, 36 distinct | TP, the same way; 43 of 43 "TPn】" agree. `CALCDAMAGE@0xeb`. |
| `G[0x33c03d]` | 1 |  | 9 | 1..20, 6 distinct 1×1, 3×2, 5×2, 10×1, 15×1, 20×2 | FS (戦意) restored; 6 of 6 agree. `CALCDAMAGE@0x147`; `USESKILLSUB@0x1d0`. |
| `G[0x33e74d]` | 1 |  | 3 | 5..15, 3 distinct 5×1, 10×1, 15×1 | 迷宮支配力 gained: `USESKILL@0xd9`..`0x108` adds it to the saved field gauge `G[0x3178]`, at most 15; SETCHSEL. |
| `G[0x340e5d]` | 1 |  | 82 | -1..-1, 1 distinct -1×82 | −1 on 82 of the ids 101..303. No reader found. |
| `G[0x34356d]` | 1 |  | 31 | -400..3000, 14 distinct | Max HP → `G[0x38944f]` (`SETCH@0x30e`); 7 of 7 agree. |
| `G[0x345c7d]` | 1 |  | 23 | -300..300, 9 distinct | Max MP → `G[0x38948b]` (`SETCH@0x338`). |
| `G[0x34838d]` | 1 |  | 18 | -100..300, 7 distinct | Max TP → `G[0x3894c7]` (`SETCH@0x362`). |
| `G[0x34d1ad]` | 1 |  | 280 | 5..100, 8 distinct | The random part added to an amount (`CALCDAMAGE@0x5b`, `@0xb7`, `@0x113`, `@0x5fc`; USESKILLSUB). |
| `G[0x34f8bd]` | 1 |  | 545 | 10..7063, 106 distinct | The battle animation, a BTANINIT2 id: `BTL@0x1ca9` and `@0x2127` put it in `battle_animation_in_play` `G[0x38a9f8]` and run ANIME. |
| `G[0x351fcd]` | 1 |  | 267 | 100..676000, 180 distinct | The buying price (`SHOP@0x10db`, `@0x11ff`, `@0x28e7`). |
| `G[0x3546dd]` | 1 |  | 267 | 25..169000, 180 distinct | The selling price, a quarter of the buying price on 259 of 267 (`SHOP@0x1143`, `@0x1252`, `@0x1642`). |
| `G[0x356ded]` | 1 |  | 50 | 1..3, 2 distinct 1×33, 3×17 | Where it can be used from the menu: `CHMODE1@0x19c9` ANDs it with 1 while `map_entered` `G[0x317c]` is set (in the dungeon) and with 2 otherwise (`CHMODE1@0x206`..`0x215`); CHMODE3. |
| `GS[0x2e70]` | 1 |  | 742 | 嵐神の鍵 / 真紅の鍵 / 黄牛の鍵 | The name (ARENA, BTL, CHMODE1, CHMODE3, DRAWCHP, GETITEMMES, INFO4, RESULT, SHOP and others). |
| `GS[0x5580]` | 1 |  | 13 | 最初から / ダルノスと二人で鉄屑の谷に行くところ / ニアクールに行く前にスフィーダから | An author's note on where 13 key items come from. No reader found. |

SETCH reads one more item array, `G[0x34aa9d]`, the maximum FS (`SETCH@0x2f2` adds it into the slot's `G[0x389413]`,
and CALCREVISE reads it). ITINIT writes none of it, so it is not in the table.

The summons lower their caster's maximum MP and TP: their max-stat cells are negative (3121 パズモ召喚 holds −30 and
−30, "MP&TP30低下"). `CALCREVISE@0x0`..`0x91` adds the max HP, MP and TP of action 3121 + b for each bit b set in the
character's `G[0x8e3d2]`.

No ITINIT field is copied into a saved cell: a scan of every script but the `*INIT` ones found no such copy. The slot
arrays SETCH fills lie above the prefix, and each battle rebuilds them. So a changed item reaches a loaded game at
once.

### The item helpers and the inventory

ADDITEM adds items. 52 scripts call it, among them GETITEM, SHOP, GAMESTART and scenes such as `SC0060@0x1dfc`. The
caller sets the item in `current_item` `G[0x12558b]` and the change of its count in `item_count_change`
`G[0x12558c]`.

- `ADDITEM@0x8`..`0x5d` adds the change to `item_counts` `G[0x8632a + item]` and keeps it within 0 and 999.
- `ADDITEM@0x62`..`0x75` sets `item_obtained_flags` `G[0x88a3a + item]` to 1 and stores it in the shared profile.
- `ADDITEM@0x78`..`0xe7`: for a key item (class 1), it sets `item_key_held_flags` `G[0xbdc + item]` to 1 while the
  item is held and to 0 when it is not. The cell is computed as `0x470 + 0x76c + item`: it is story flag
  `1900 + item` (SCJUMP, above). SCJUMP tests none of these. ARENA reads the passes' flags by a computed index (the
  arena, below). 17 map scripts read the keys 1, 2 and 3 by address (`MP02B0207`, `MPEV02B0101`, `MPEV41F0102`
  and others), and the debug script DEBUGMAP2 reads 28..31.
- `ADDITEM@0xae`..`0xd1`: items 13..20, the 陣形の書, also set bit (item − 12) of `formation_learned_mask`
  `G[0x8b226]`. `CHMODE4@0x1541` tests that mask, and `CHMODE4@0x1639` sets `formation_current` `G[0x8b225]`, whose
  name it draws from `GS[0x80f0 + f]` (`CHMODE4@0x1c42`). Both new-game paths start with bit 1 and formation 1
  (`GAMESTART@0x47d` for 特別編, `@0x687` for 通常), and INIT2 sets the same when the formation is 0.

LOSTITEM (SC0180, SC0320, SC0360 and SC0390 call it) takes the item in `current_item` off every character who has it
equipped. It clears each of the six equipment slots that holds it (`LOSTITEM@0x2a`..`0x100`): `character_weapons`
`G[0x8c1d6]`, `G[0x8c23a]`, `G[0x8c29e]`, the shield `G[0x8c302]`, the armour `G[0x8c366]` and the accessory
`G[0x8c3ca]` (units, above).

The party's `money` `G[0x8b159]` is kept within 0 and 9,999,999 (`GETITEM@0x12f`..`0x15a`,
`GAMESTART@0x62a`..`0x655`). SHOP subtracts the bill and adds what it pays for an item sold (`SHOP@0x1813`,
`@0x186d`).

All these cells lie below the prefix, so a save keeps them: the counts, the obtained marks, the key items held, the
formation, the equipment, the money and the treasures opened. `current_item` and `item_count_change` are arguments
only.

### ILINIT (conditions)

Leader `GS[0x7c90]` (the name), 5 cells a condition, capacity 40. 36 conditions of up to five levels, ids 1..36, all
in the base. The readers loop 40 conditions (`CALCILL@0x51`, `DRAWENLIST@0x63`), and the slot rows are 40 cells
(`G[0x38980f + 40 × slot + condition]`). The names are `GS[0x7c90 + 5 × condition + level − 1]` and the descriptions
`GS[0x7d58 + 10 × condition + line]`; `0x7c90 + 200 = 0x7d58`, and `0x7d58 + 400 = 0x7ee8`, SSINIT's names.

The conditions: 1 即死, 2 喪意, 3 石化 (one level each); 4 HP吸, 5 MP吸, 6 TP吸 (names only); 7 呪い, 8 魅了, 9 混乱,
10 沈黙, 11 麻痺, 12 猛毒, 13 酔い, 14 束縛, 15 水流, 16 強酸, 17 高揚, 18 再生, 19 恐怖; the stat changes 20 命中,
21 回避, 22 物攻, 23 物防, 24 攻回, 25 防回, 26 魔攻, 27 魔防, 28 魔威, 29 肉抵, 30 精抵, 31 肉速, 32 精速, 33 運,
34 痛打; 35 蘇生, 36 経験 (names only).

A caller names a condition in `condition_argument_id` `G[0x3594fd]` and its levels in `condition_argument_levels`
`G[0x3594fe]`. `CALCILL@0x34` and `ADDILL@0x55` read them, and `MPEV01F0120@0x7b5` sets them before it runs ADDILL.

The integer arrays are 24, k = 0..23. Arrays 0..22 have 5 cells a condition, one a level, at `G[0x3594ff + 200k]`;
array 23 has one cell a condition, at `G[0x35a6f7]`. No integer cell lies outside `G[0x3594ff]`..`G[0x35a71e]`. The
tool lays out the arrays that several conditions write. An array only one condition writes (k 8, 9, 12, 13, 14 and
17) has no grid of its own, and its 5 cells are among the tool's 30 outside. The three `†` rows are k 19, 22 and 23,
seen off the grid; the second table below gives them.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x3594ff]` | 5 | 0..4 | 31 | 3..4, 2 distinct 3×57, 4×86 | k 0, conditions 1..3 and 7..34: the resistance kind, 3 mental, 4 physical (`CALCILL@0x262`..`0x28d`, `@0x31f`..`0x39b`). |
| `G[0x3595c7]` | 5 | 0..4 | 31 | 11..17, 7 distinct | k 1, the same conditions: the resistance column 11..17 (抵抗1..7) of AFINIT's matrix (`CALCILL@0x3b3`, the matrix at `CALCILL@0x3c3`). |
| `G[0x35968f]` | 5 | 0..4 | 27 | 2..3, 2 distinct 2×85, 3×50 | k 2, conditions 8..34: cure bits. `RECOVER@0x8f` clears a condition to its floor when its bits meet 1 (in the dungeon, `map_entered` set) or 2 (outside) (`RECOVER@0x3a`..`0x49`). `BTL@0x367a` resets a slot's level to its lasting level when bit 2 is set. |
| `G[0x359757]` | 5 | 0..4 | 28 | -1..300, 10 distinct | k 3, conditions 7..34: the duration of each level, −1 for 呪い (it lasts). `ADDILL@0x16d`..`0x1ba` copies it into the character's time left `G[0x8d3ce]`; CALCILL, DRAWENLIST. |
| `G[0x35981f]` | 5 | 0..4 | 3 | -25..30, 14 distinct | k 4, conditions 7, 13, 20: 命中 (`SETILLREVISE@0x13e` → `G[0x38971f]`). |
| `G[0x3598e7]` | 5 | 0..4 | 4 | -50..30, 17 distinct | k 5, conditions 7, 11, 13, 21: 回避 (`SETILLREVISE@0x155` → `G[0x38973d]`). |
| `G[0x3599af]` | 5 | 0..4 | 2 | 10..30, 5 distinct 10×2, 15×2, 20×2, 25×2, 30×2 | k 6, conditions 17, 22: 攻撃力, in percent (`SETILLREVISE@0x41` → `G[0x3894e5]`). |
| `G[0x359a77]` | 5 | 0..4 | 2 | -100..30, 10 distinct | k 7, conditions 17, 23: 防御力, in percent (`SETILLREVISE@0x58`). |
| `G[0x359ccf]` | 5 | 0..4 | 2 | 10..30, 5 distinct 10×2, 15×2, 20×2, 25×2, 30×2 | k 10, conditions 17, 26: 魔法攻撃, in percent (`SETILLREVISE@0x6f`). |
| `G[0x359d97]` | 5 | 0..4 | 2 | -100..30, 10 distinct | k 11, conditions 17, 27: 魔法防御, in percent (`SETILLREVISE@0x86`). |
| `G[0x35a0b7]` | 5 | 0..4 | 3 | -20..10, 13 distinct | k 15, conditions 7, 14, 31: 肉体戦速 (`SETILLREVISE@0xf9`). |
| `G[0x35a17f]` | 5 | 0..4 | 3 | -20..10, 13 distinct | k 16, conditions 7, 14, 32: 精神戦速 (`SETILLREVISE@0x110`). |
| `G[0x35a30f]` | 5 | 0..4 | 3 | -10..10, 10 distinct | k 18, conditions 7, 17, 34: 痛打 (`SETILLREVISE@0x16c`). |
| `G[0x35a3e5]` † | 4 | 0..3 | 7 | -16..5, 15 distinct | *(k 19, `G[0x35a3d7]`, below)* |
| `G[0x35a652]` † | 3 | 0..2 | 4 | -10..-5, 5 distinct -10×2, -8×2, -7×2, -6×2, -5×2 | *(k 22, `G[0x35a62f]`, below)* |
| `G[0x35a6f8]` † | 1 |  | 14 | 1..1, 1 distinct 1×14 | *(k 23, `G[0x35a6f7]`, below)* |
| `GS[0x7c90]` | 5 | 0..4 | 36 | 即死 / 喪意 / 石化 | The name of each level (BTL, CHANGECH, DRAWCHP, DRAWENP). |
| `GS[0x7d58]` | 10 | 0..8 | 36 | 【一瞬】判定に成功したら即死。　回復 / 【一瞬】判定に成功したら戦意0。　回 / 【一瞬】判定に成功したら石化。　回復 | The description's lines. No reader indexes this base. |

The arrays the tool leaves outside or off the grid, recomputed from the image:

| k | array | conditions | values | holds |
|---|---|---|---|---|
| 8 | `G[0x359b3f]` | 24 | 1..5 | 攻撃回数 (`SETILLREVISE@0x9d`) |
| 9 | `G[0x359c07]` | 25 | 1..5 | 防御回数 (`SETILLREVISE@0xb4`) |
| 12 | `G[0x359e5f]` | 28 | 1..5 | 魔威 (`SETILLREVISE@0x183`) |
| 13 | `G[0x359f27]` | 29 | 10..30 | 肉体抵抗 (`SETILLREVISE@0xcb`) |
| 14 | `G[0x359fef]` | 30 | 10..30 | 精神抵抗 (`SETILLREVISE@0xe2`) |
| 17 | `G[0x35a247]` | 33 | 2..10 | 運 (`SETILLREVISE@0x127`) |
| 19 | `G[0x35a3d7]` | 12, 15, 16, 18 | -16..5, 15 distinct | The HP change each frame in battle and each step in the dungeon (`SETILLREVISE@0x19a` → `G[0x389797]`; `FIELD@0x11f1` into the character's HP `G[0x8b42a]`). 猛毒, "1フレーム毎に…", holds −1..−5. |
| 22 | `G[0x35a62f]` | 16, 19 | -10..-5 | The FS change each 10 frames: `BTL@0x5b7`..`0x625` adds it to the slot's FS `G[0x3893f5]`, keeping at least 1. 恐怖 reads "戦闘中は10フレーム毎に入力値がそのまま引かれる". |
| 23 | `G[0x35a6f7]`, one cell a condition | 3, 7..19 | 1×14 | 1: the condition is its own entry in the enemy list. `DRAWENLIST@0x71` lists each such condition a unit has; any other above its lasting level adds one shared entry, 0x1d (`DRAWENLIST@0xb5`..`0xf4`). |

Arrays 20 (`G[0x35a49f]`, the MP change) and 21 (`G[0x35a567]`, the TP change) are read (`SETILLREVISE@0x1b1`,
`@0x1c8`; `FIELD@0x1211`, `@0x1231`) and never written. The percent arrays scale the stat
(`SETILLREVISE@0x1e2`..`0x267`); the others add to it (`SETILLREVISE@0x26e`..`0x378`). `SETILLREVISE@0x37f` sets
slot flag bit 6 for 石化 (3), and `SETILLREVISE@0x3ac` bit 7 for 魅了 (8).

The character side is saved: the level `G[0x8c42e + 40 × character + condition]`, the floor `G[0x8e5c6]` it cannot
fall below, and the time left `G[0x8d3ce]` (ADDILL). CALCREVISE sets the floors and levels from the six equipped
items' lasting conditions (units, above). A cure of 呪い to its floor takes the cursed items off all six slots
(`ADDILL@0x1bf`..`0x2c0`). ILINIT reaches these cells only through ADDILL's duration copy, so a changed ILINIT reaches
a loaded game at once, except for the time left of a condition already given.

### SSINIT (support effects)

Leader `GS[0x7ee8]`, capacity 100 (`0x7ee8 + 100 = 0x7f4c`, the descriptions). It writes no integers and 25 strings:
12 support effects, ids 1..12, all in the base.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x7ee8]` | 1 |  | 12 | 支援効果・祈り / 支援効果・応援 / 支援効果・結界 | The name. |
| `GS[0x7f4c]` | 2 | 0..1 | 12 | 10フレーム毎にHPが回復 / 10フレーム毎に戦意up / 味方全体の魔法防御up | The description, two lines; only 6 警戒 writes line 1. |

`CHMODE2@0x2651`..`0x267e` draws the character's support effect, its name and its two lines. The id is the unit's
`G[0xabb6d]` (EBINIT, above). What each effect does in battle is in BTL, which counts them (`BTL@0x4bed`) and is not
read further here.

### AFINIT (elements, classes, formations)

AFINIT has no leader: 440 integers and 218 strings in small arrays. The tool shows them by the arrays the scripts
index:

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x3866ad]` | 20 | 0..17 | 15 | -100..200, 22 distinct | The affinity in percent, `G[0x3866ad + 20 × defense + attack]`, rows 0..14 the defense elements and columns 0..17 the attack elements. Row 0 and column 0 are 100; column 10 is 0. The 火炎 defense takes −100 from 火炎 and 150 from 冷却. `CALCDAMAGE@0x909` (physical, magic) and `@0x3ac` (FS), the row being the slot's `G[0x389701]`; a negative value gives negative damage (`CALCDAMAGE@0x930`..`0x953`). `CALCILL@0x3c3` reads columns 11..17 for conditions; RTN002 and RTN008 for targets. |
| `G[0x38683d]` | 1 |  | 33 | 1..46, 30 distinct | The class's icon, a cell of the 10-wide sheet of 12×14 icons (`CHMODE2@0x23c2`, `RESULT@0xf4c`). |
| `G[0x386879]` | 1 |  | 3 | 704..4096, 3 distinct 704×1, 3968×1, 4096×1 | The weapon categories an art school needs, a bit mask by category: 41 = 704 (短剣, 剣, 双剣), 42 = 3968, 43 = 4096 (弓). `CHECKACT@0x114`, `CHMODE3@0x15db`. The notes `GS[0x80b4]` agree. |
| `G[0x3868b5]` | 6 | 0..5 | 21 | 1000..150000, 30 distinct | The most class experience a rank holds, `G[0x3868b5 + 6 × class + rank − 1]` for classes 4..6 and 21..46: `BTL@0x2c32`..`0x2c8d` caps `character_skill_experience` at it. The values rise by rank (10000 … 150000 for weapons). `CHMODE2@0x1569`, CHMODE3. |
| `G[0x386a1d]` | 1 |  | 8 | 1..3, 3 distinct 1×3, 2×4, 3×1 | Weapon flags, by category 4..12. Bit 1 (杖, 短剣, 剣, 拳, 双剣): `CALCDAMAGE@0x776` halves the damage when a unit stands between. Bit 0 (双剣, 連接, 槍, 弓): `CHMODE1@0x12b9`, `@0x1409` and `@0x15ee` clear or block the shield slot `G[0x8c302]`. |
| `GS[0x8014]` | 1 |  | 16 | 物理 / 万能 / 火炎 | The attack elements, 1..17: 1 物理, 2 万能, 3 火炎, 4 冷却, 5 電撃, 6 地脈, 7 神聖, 8 暗黒, 9 戦意, 10 blank, 11..17 抵抗1..7. There is no 無属. |
| `GS[0x8028]` | 1 |  | 14 | 物理 / 万能 / 火炎 | The defense elements, 1..14: 1..8 as above, 9 神格位, 10 魔神, 11 反万能, 12 霊体, 13 不死, 14 創造物. |
| `GS[0x803c]` | 1 |  | 66 | 重要アイテム / 通常アイテム / 非戦闘時アイテム | Two arrays of 60: `item_class_names` `GS[0x803c + class]` (33 classes, 1 重要アイテム … 10 装飾品, 21 魔術・治癒 … 35 魔術・招聘, 40 敵魔術, 41 必殺・飛燕 … 46 必殺・盗賊, 50 敵必殺) and `item_class_short_names` `GS[0x8078 + class]` (重要, 物品, …, 召喚Ａ, 召喚Ｂ), which no script indexes, so the tool counts its 33 cells here. |
| `GS[0x80b4]` | 1 |  | 4 | 「盾」装備不可 / 「短剣」「剣」「双剣」装備必須 / 「中型」「両手」武器装備必須 | `item_class_equipment_notes`, classes 6 and 41..43: what the class needs or forbids. |
| `GS[0x80f0]` | 1 |  | 8 | 基本陣形 / 攻撃陣形 / 防御陣形 | `formation_names`, formations 1..8 (`CHMODE4@0x1c42`). |
| `GS[0x80f9]` | 6 | 0..5 | 8 | 効果なし / 物理攻撃up / 物理防御up | `formation_effect_lines`: lines 0..1 the front row's up and down, 2..3 the rear row's, 4..5 the description (`CHMODE4@0x1ca1`..). |
| `GS[0x812f]` | 1 |  | 8 | 通常攻撃型 / おまかせ型 / 攻撃重視型 | The tactics' names, 1..8, which `CHMODE2@0x227e` reads as `GS[0x812f + k + 1]` for EBINIT's routine column k. |
| `GS[0x8138]` | 7 | 0..6 | 8 | ◎ / ○ / ○ | Each tactic's ◎/○/× ratings and its description, read by CHMODE2. |

The elements pair by number up to 8. Attack element 9 戦意 has no defense element of that name; 10 is blank; 11..17
are the condition resistance columns. ITINIT uses attack elements 1..13.

### TRINIT (treasure)

Leaders `G[0x10cc93]` and `G[0x118fe3]`, 5 cells a treasure, capacity 10000. 532 treasures: 448 in the base (ids
1..1207 and 9999) and 84 from `$1$TRINIT` (ids 2000..2082 and 9998). Each has five slots. An item slot holds an item
and a count; a money slot holds no item and the money as its count. 94 treasures hold only money and write no item
cell, which is why the table has two leaders.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x10cc93]` | 5 | 0..4 | 438 | 4..2942, 242 distinct | The item in each slot; 0 or unwritten for money. |
| `G[0x118fe3]` | 5 | 0..4 | 532 | 1..36000, 80 distinct | The count, or the money (10..36000) for a money slot. |

Slots used: 1 on 510 treasures, 2 on 17, 3 on 3, 5 on 2. There are 467 item slots and 96 money slots.

How a treasure is opened:

1. A map script puts a treasure id in its object's cell, `treasure_object_ids` `G[0xd4d3d + object]` (100 cells;
   `MP03F0101@0x1f4` writes 121). FIELD clears them. About 370 map scripts write them.
2. `GETTREASURE@0x0` takes the id from `G[0xd4d3d + G[0xc57ac]]` into `treasure_current_id` `G[0x10cc92]`.
3. An id of 0, or a treasure already opened (`treasure_opened` `G[0x83c1a + id]` = 1), shows 何も見つからなかった.
4. Otherwise `GETTREASURE@0x48` marks it opened and runs EVOLVEOBJ, then GETITEM.
5. `GETITEM@0x1d`..`0x7c` collects the slots with a count above 0. `GETITEM@0x109`..`0x129` runs ADDITEM for each
   item slot, `GETITEM@0x12f`..`0x15a` adds the money, and `GETITEM@0x15f` clears `G[0x10cc92]`.

Treasures 9998 and 9999 are the starting kits, not chests: GAMESTART gives 9998 to a new 特別編 game
(`GAMESTART@0x5a5`) and 9999 to a new 通常 game (`@0x691`), through the same slot loop (`GAMESTART@0x5c7`..`0x655`).
Both hold 3 治癒の水・小, 1 治癒の水・中, 100 money, 飛翔の耳飾り and 基本陣形の書.

`treasure_opened` is saved. No treasure field is copied into a saved cell; GETITEM and GAMESTART only add to the counts
and the money. So a changed treasure reaches a loaded game for every treasure not yet opened.

### SHINIT (the shop in play)

SHINIT is not in the boot image. `SHOP@0x82`..`0x8c` clears the two picture cells and runs it each time a shop opens.
`SHINIT@0x0`..`0x7` zeroes 20 slot cells from `G[0xc1e5d]` (op `0x6c`, zero-int-range), then branches on the shop.
There are 12 shops and 572 lines, and the only other cells they read are story flags.

| cell | holds |
|---|---|
| `shop_id` `G[0xc1e5b]` | The shop, 1..12. SC1500 sets 1..10 (`SC1500@0x9be`), `SC0190@0x16e6c` sets 11, `SC1510@0xf69` sets 12, SC2000 sets 1, 2, 5 and 11. `SHOP@0x3b1d` clears it. |
| `G[0xc1e5c]` | The shop's sign, 0..3: `SHOP@0x23b2`..`0x2434` picks one of four 140×40 cells of texture 0x7c. Item shops write 0, equipment shops 1. |
| `G[0xc1e5d + 10 × page + slot]` | The item in the slot, pages 0 and 1, slots 0..6 (shop 12 also writes slot 7); 0 for none. A slot's item is negated while its story flag is not set (`SHINIT@0x51`..`0x66`, shop 1 slot 10 and flag 20); SHOP takes the absolute value (`SHOP@0x27ab`). |
| `G[0xc1e71]` | The shopkeeper's picture (CS136AA and others), loaded at `SHOP@0x2316` into slot 0x7f. |
| `G[0xc1e72]` | The shop's background (BG018A and others), loaded at `SHOP@0x2308` into slot 0x7e. Shop 11 has none. |

The flags the slots wait for are the story flags of scenes 20, 31, 37, 167, 200, 216, 246, 276, 448, 454, 457, 463,
475, 497 and 953..955 (`G[0x484]` … `G[0x82b]`).

SHOP buys through ADDITEM: `SHOP@0x17c3`..`0x180d` adds each chosen count, and `SHOP@0x1813` takes the bill from the
money. Selling runs ADDITEM again (`SHOP@0x1867`) and adds the money (`SHOP@0x186d`).

None of SHINIT's cells is saved, and unlike Verita, bg0 has no stock limits and no saved bought counts. So a changed
shop reaches a loaded game the next time the shop opens.

## Battle, enemies and routines

A field battle draws on these tables in turn. As the party walks, FIELD rolls the chance in EEINIT. When a battle
starts, SETENP draws one entry of the encounter set ENINIT loaded and places its formations (ESINIT). SETEN sets up
each enemy from EBINIT. In battle, BTL runs RTN for each battler under routine control, and RTN follows the routine in
RTINIT. An action's animation is a BTANINIT2 record; ANIME runs BTANINIT to fill that record's effects, then plays
them. ARENA runs the arena's battles from ARINIT.

INIT2 runs RTINIT, ARINIT, EEINIT and BTANINIT2 (`INIT2@0x12`, `@0x21`, `@0x24`, `@0x45`), and ARENA runs ARINIT
again each time it opens (`ARENA@0x44`). ENINIT, ESINIT and BTANINIT run when the game needs them. Their callers run
the `$1$` fragment while the game mode is 特別編:

| table | caller | the `$1$` fragment | the base runs when |
|---|---|---|---|
| RTINIT | `$1$AUTORUN@0xc` | writes nothing | at boot (`INIT2@0x12`) |
| BTANINIT2 | `$1$AUTORUN@0x44` | writes nothing | at boot (`INIT2@0x45`) |
| ENINIT | `FIELD@0x5e5`..`0x603` | 12 sets, 201 to 302 | entry 0's weight `G[0x125334]` is still 0 after it (`FIELD@0x5ef`), or in 通常 |
| ESINIT | `SETENP@0x347`..`0x365` | 66 formations, 1001 to 1923 | slot 0's unit `G[0x125529]` is still 0 after it (`SETENP@0x351`), or in 通常 |
| BTANINIT | `ANIME@0xc`..`0x19` | a full copy of BTANINIT | in 通常: ANIME runs one or the other |

EEINIT and ARINIT have no fragment. No array of these seven tables lies below the saved prefix ("What is saved"
below).

### RTINIT (battle routines)

A routine is a list of up to 20 steps. Each step names a rule (the `RTNnnn` script that runs it), the chance it runs,
an action and up to three arguments. Leader `G[0x35a787]`, capacity 20000, 20 slots a row: a row is a routine and a
slot a step, so a record's id is 20 × routine + step. Nine banks follow, 20000 cells apart. There are 237 routines,
ids 1 to 999, with 2190 steps, all in the base; no routine has a gap. The tool counts 2205 records because routine
200's 15 steps are written twice. The second write wins: it changes step 0's chance from 15 to 30 and step 1's from
10 to 20 (`RTINIT@0x890d` against `@0x90e7`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x35a787]` | 1 |  | 2190 | 1..306, 19 distinct | Bank 0, the rule: the script RTN runs for the step (`RTN@0x0`..`0x6e`). 0 ends the routine (`RTN@0xef`). |
| `G[0x35f5a7]` | 1 |  | 2189 | 4..102, 31 distinct | Bank 1, the percent chance (`RTN@0x1a1`..`0x1b8`). |
| `G[0x3643c7]` | 1 |  | 1773 | 1..4057, 181 distinct | Bank 2, the action: below 6 a slot of the unit's six actions `G[0x3866a7]`, else an ITINIT id (`RTN003@0x0`..`0x2d`); for rules 301 to 306, an item. |
| `G[0x3691e7]` | 1 |  | 1249 | 1..400, 13 distinct | Bank 3, the first argument: the gauge (1 to 4) for rules 4 to 6, 10, 11, 101 to 103 and 304 to 306; the unit for rule 7; how many targets for 9 and 302; the battle time for 12 and 303. |
| `G[0x36e007]` | 1 |  | 603 | 1..100, 13 distinct | Bank 4, the percent of the gauge's maximum, for rules 5, 6, 10, 11, 101 to 103 and 304 to 306 (`RTN304@0x20`, `@0x14f`). |
| `G[0x372e27]` | 1 |  | 211 | 1..2, 2 distinct 1×54, 2×157 | Bank 5, how many battlers must pass the gauge test, for rules 103 and 306 (`RTN103@0x30`, `@0x215`). |
| `G[0x381887]` | 1 |  | 89 | 1..5, 5 distinct 1×57, 2×28, 3×1, 4×2, 5×1 | Bank 8, how many times the step may run in a battle (`RTN@0x15f`..`0x195`). |

Banks 6 (`G[0x377c47]`) and 7 (`G[0x37ca67]`) are written by no layer, so the tool does not list them. No script reads
bank 6. Bank 7, when set, makes the step wait for story flag `value − 1000`, `G[0x470 + value − 1000]`
(`RTN@0x12f`..`0x15a`). The cells at bank 9's place (`G[0x3866a7]` on) are not RTINIT's: they are RTN's copy of the
unit's actions, AFINIT's matrix and BTANINIT2.

How the game reads it (`BTL@0x13b0`..`0x1411`): when bit `0x100` of the battler's state `G[0x38939b + battler]` is
set, BTL takes the routine from EBINIT, `G[0xb53c5 + 9 × unit + column]`, the unit being `G[0x38937d + battler]`. The
column is the character's `character_tactics` `G[0x8e36e]` for a party battler (below 10), and 0 for an enemy. The
routine goes into `routine_in_play` `G[0x35a784]`, and RTN runs. `SETENP@0x6e2` sets the bit for every enemy; a party
battler gets it from battle command 7 (`BTL@0x124f`..`0x12a1`), and `BTL@0x1bc2` clears it.

RTN first maps rule r to the script in its local `5 + r` (`RTN@0x0`..`0x6e`). It copies the unit's six actions
(EBINIT's `G[0xb76ed]`) into `G[0x3866a7]` (`RTN@0x84`..`0x9b`); if action 0 is empty, it uses the battler's weapon
`G[0x38962f]`, or 9999 (素手) (`RTN@0xa2`..`0xcd`). Then it walks the steps 0 to 19 (`RTN@0xd2`..`0x1f7`):

1. A step whose rule is 0, or a rule with no script, ends the walk (`RTN@0xef`..`0x122`).
2. Bank 7, when set, must name a story flag that is set (above).
3. Bank 8, when set, limits the runs. `G[0x38a787 + 20 × battler + step]` counts them (`RTN@0x15f`..`0x195`); RTN adds
   one when the step acts (`RTN@0x1d5`..`0x1eb`), and `SETENP@0x38` clears the counts.
4. RTN draws 0 to 99 and goes on when the draw is below bank 1 (`RTN@0x1a1`).
5. The rule's script runs with the step in `G[0x35a785]` (`RTN@0x1bf`..`0x1cb`). A script that acts sets
   `G[0x35a786]`, and the walk ends (`RTN@0x1ce`, `@0x1f2`).
6. If no step acted, RTN000 uses action 0 on a target the battle picks (`RTN@0x1fa`..`0x208`).

Unlike Verita, bg0 has no tactic-mask bank: the party's tactic picks the routine instead, through EBINIT's column.

The gauges a rule tests are numbered by bank 3. The item rules confirm the numbering: they pair 治癒の水 with 1,
魔力石 with 2, 闘技石 with 3 and 勇壮の水 with 4.

| gauge | the battler's value | its maximum | where it comes from |
|---|---|---|---|
| 1, HP | `G[0x389431]` | `G[0x38944f]` | the character's `G[0x8b42a]` (ADDHP) |
| 2, MP | `G[0x38946d]` | `G[0x38948b]` | `G[0x8b4f2]` (ADDMP) |
| 3, TP | `G[0x3894a9]` | `G[0x3894c7]` | `G[0x8b5ba]` (ADDTP) |
| 4, FS (戦意) | `G[0x3893f5]` | `G[0x389413]` | `SETENP@0x149` sets it to half its maximum; for an enemy, `SETEN@0xa49` sets EBINIT's starting FS `G[0xade95]` |

`SETENP@0x7b`..`0xaf` copies the character values into the battler arrays. ADDHP, ADDMP and ADDTP take their
arguments in `script_arg_1` `G[0x91865]` and `script_arg_2` `G[0x91866]`, the shared argument cells that ADDEXP,
CALCLVUP, GETMAPEVF, GETMONEYMES and SHOWALLMAP read too.

The rules:

| rule | script | what it does | steps |
|---|---|---|---|
| 0 | RTN000 | action 0 on target −1 (the battle picks) | — |
| 1 | RTN001 | one of the six actions, drawn among those CHECKACT allows (`RTN001@0x0`..`0x7d`) | 113 |
| 2 | RTN002 | the nearest target: key `abs(dy) << 16 + abs(dx) << 8 + draw`, smallest first, with an AFINIT rate above 0 (`RTN002@0x14e`..`0x1e0`) | 313 |
| 3 | RTN003 | the action on target −1 | 395 |
| 4 | RTN004 | the target whose bank-3 gauge is lowest (`RTN004@0x17e`..`0x213`) | 150 |
| 5 | RTN005 | a random target whose gauge is at least bank 4 percent of its maximum (`RTN005@0x27c`) | 82 |
| 6 | RTN006 | the same, at most bank 4 percent | 1 |
| 7 | RTN007 | a battler whose unit is bank 3 (`RTN007@0x17e`) | 0 |
| 8 | RTN008 | the target with the largest AFINIT rate (`RTN008@0x16c`..`0x18c`) | 9 |
| 9 | RTN009 | an area action (ITINIT's `G[0x2408cd]`, `G[0x23e1bd]`) when at least bank 3 targets in reach are affected; a negative rate counts −100 (`RTN009@0x4c`..`0x2c0`) | 431 |
| 10 | RTN010 | its own bank-3 gauge is at least bank 4 percent (`RTN010@0x125`) | 0 |
| 11 | RTN011 | the same, at most | 159 |
| 12 | RTN012 | the battle clock is at least bank 3 (`RTN012@0x20`..`0x2e`) | 27 |
| 101 | RTN101 | its own gauge is at most bank 4 percent: it acts on itself, then runs CHECKRECV | 0 |
| 102 | RTN102 | some battler's gauge is at most bank 4 percent, then CHECKRECV; its side test reads `G[0x952a3]` before CHECKACT sets it (`RTN102@0x3e`) | 23 |
| 103 | RTN103 | at least bank 5 battlers pass the gauge test (`RTN103@0x30`, `@0x215`) | 53 |
| 201 | RTN201 | 9998 (逃亡) on itself | 4 |
| 301 | RTN301 | rule 3 with an item | 79 |
| 302 | RTN302 | rule 9 with an item | 12 |
| 303 | RTN303 | rule 12 with an item | 5 |
| 304 | RTN304 | rule 101 with an item, on itself | 71 |
| 305 | RTN305 | rule 102 with an item | 104 |
| 306 | RTN306 | rule 103 with an item | 159 |

There are no rules 13, 51 to 53, 202 or 203, which Verita has. An item rule uses bank 2 as the item, and a party
battler needs one held, `item_counts` `G[0x8632a + item]` above 0 (`RTN301@0x25`, `RTN304@0x45`). CHECKRECV sets
`G[0x35a786]` and may clear it again (`CHECKRECV@0x573`).

The battle clock is `100 × G[0x9529f] + G[0x9529e]`. `BTL@0x25` zeroes both, `BTL@0x328` adds one, and
`BTL@0x962`..`0x993` carries at 100.

The data has slips:

- Seven item steps (rules 304 and 305, in routines 34, 74 and 98) have 30 or 50 in bank 3 and nothing in bank 4: the
  percent sits where the gauge goes. RTN304 then keeps 100 as the gauge's percent and tests it against 0, so these
  steps never act (`RTN304@0x61`, `@0x14f`).
- Routine 35 step 5 (rule 6) has 30 in bank 3, and routine 50 step 5 (rule 103) has 80 in bank 3 and 3 in bank 4: the
  same slip, or the two swapped.
- Three rule-4 steps (routine 35 step 4, routine 146 step 16, routine 147 step 14) name no gauge.
- Six rule-12 steps name no time, so their test always passes.
- 41 rule-306 steps have no bank 4. This looks deliberate: they pair item 113 with gauge 1, so they test for a gauge at
  0 percent.

### BTANINIT2 (battle animations)

An animation has six effect slots, each an effect with a start time, and two values of its own. Leader `G[0x38a9fa]`,
6 cells an animation, capacity 10000. There are 114 animations, ids 1 to 9999, all in the base (615 cells); the tool
counts 118 records. Every effect a slot names is defined in BTANINIT, and no slot has a start without an effect.

ITINIT names an action's animation, `G[0x34f8bd + action]`: 545 actions name 106 animations, all defined. Eight
animations are named by no action: 1, 2, 3, 11, 12, 27, 4053 and 9999. `DEBUGANIME@0x0` plays 7049.

The tool leaves 97 cells outside its arrays. They are the length array `G[0x3a7eba]`, one cell an animation: animation
1's length is `G[0x3a7ebb]` (`BTANINIT2@0x32`), written right after its effects, and `ANIME@0x2b2` reads
`G[0x3a7eba + animation]`. Its row below is recomputed from the image.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x38a9fa]` | 6 | 0..5 | 114 | 1..9999, 132 distinct | Each slot's effect, a BTANINIT id. Slot 2 is set in 112 animations (110 distinct effects), slot 5 in 93 (7 distinct: 14×48, 13×17, 15×17, 12×8 …), slot 0 in 12, slots 1 and 3 in 2, slot 4 in 1. |
| `G[0x39945a]` | 6 | 2..5 | 104 | 50..3000, 16 distinct | When each slot's effect starts, in ms (`ANIME@0xbca`, `@0xc2d`); slot 2 mostly 200, slot 5 mostly 680. |
| `G[0x3a7eba]` | 1 |  | 97 | 600..4000, 6 distinct 600×3, 680×1, 700×89, 1000×1, 1800×1, 4000×2 | *(outside in the tool)* The animation's length in ms (`ANIME@0x2b2`). A voice is scheduled at it (`ANIME@0x1022`..`0x1030`), and the end fade runs at it plus 300 to 400 (`ANIME@0x12ab`..`0x1330`). |
| `G[0x3aa5ca]` | 1 |  | 96 | 1..6, 6 distinct 1×9, 2×4, 3×16, 4×21, 5×7, 6×39 | How the battle picture splits as it slides away after the animation (the switch at `BTL@0x6352`..`0x6701`, in `label_623d` after ANIME). It moves objects `0x4650`..`0x4652` over 250 ms, with the portrait from EBINIT's `G[0x9c93d]`: 1 and 2 two-pixel strips, 3 left and right halves, 4 top and bottom halves, 5 three bands, 6 eight bands. Skipped while `G[0x8f566]` or the option `G[0x90510]` is set (`BTL@0x6328`). |

BTL sets the animation in play, `battle_animation_in_play` `G[0x38a9f8]`, from the action's (`BTL@0x1cb0`, `@0x212e`,
`@0x4a32`) and runs ANIME. `ANIME@0x0` sets it to 0 while the option `G[0x9050f]` is set (boot, above). BTL also sets
bits in `G[0x38a9f9]` (`BTL@0x1ca4`, `@0x2086`, `@0x2122`): while bit 0 is clear, ANIME skips slots 0 and 1 and moves
the timing back by the earliest start of slots 2 to 5 (`ANIME@0x243`..`0x2b9`). Slot 5 plays only when the hit result
`G[0x952a4]` is not 0 (`ANIME@0x4c1`..`0x4fb`).

### BTANINIT (the animation's effects)

ANIME runs BTANINIT, or `$1$BTANINIT` in 特別編 (`ANIME@0xc`..`0x19`); the two are the same script but for the header.
BTANINIT clears 15 work arrays of six cells, one cell a slot. It then loops the six slots of the animation in play
(`BTANINIT@0x4b`..`0x71`) and branches on each slot's effect id. There are 133 effects, ids 1 to 9999.

| array | cells | holds | evidence |
|---|---|---|---|
| `G[0x3accda]` | 6 | The picture: an `MVB…A.AGF` movie in 106 effects, an `AM…` sheet in 27; 0 for an empty slot. | `ANIME@0xa6c`; the movie at `@0xc3f`, the texture at `@0xb9d`, `@0xbbc` |
| `G[0x3acce0]` | 6 | The still kind: 0 a movie (106); 1 a texture with colour key `0xff00` (9); 2 a texture with key −1 (18). Textures are set only while bit 0 of `G[0x38a9f9]` is set. | `ANIME@0xb5e`..`0xbbc` |
| `G[0x3acce6]` | 6 | The blend: when set, `gfx-draw-color obj 1 0xff 0xffffff`. 115 effects have 1; the kind-2 sheets have none. | `ANIME@0xc5b`..`0xc70` |
| `G[0x3accec]` | 6 | The width (288 in 102 effects). | `ANIME@0xae1`, `@0xc16` |
| `G[0x3accf2]` | 6 | The height (368 in 117 effects). | `ANIME@0xaf6` |
| `G[0x3accf8]` | 6 | A sheet's columns, 27 effects. | `animate-gfx-srcrect-target`, `ANIME@0xbf1` |
| `G[0x3accfe]` | 6 | A sheet's rows, inferred: ANIME does not read it, but columns × rows is the frame count in all 27 sheets. | — |
| `G[0x3acd04]` | 6 | The frame count. | `ANIME@0xbe3` |
| `G[0x3acd0a]` | 6 | The length in ms. For a movie ANIME writes the movie's own length into it, and keeps the latest end. | `ANIME@0xc56`, `@0xc85`..`0xccc` |
| `G[0x3acd10]` | 6 | Placed: 1 at the target's side (x `0x76` or `0x2aa`, y `0xdc`), 0 at the centre (400, 300). 125 effects have 1; effects 8 and 3131 to 3137 have 0. | `ANIME@0xa7a`..`0xadc` |
| `G[0x3acd16]` | 6 | The x offset; 0 in every effect. | `ANIME@0xb0b` |
| `G[0x3acd1c]` | 6 | The y offset; 0 in every effect. | `ANIME@0xb1e` |
| `G[0x3acd22]` | 6 | The sound, 127 effects (A0120.WAV …), played on channel 3 + slot. | `ANIME@0x1055`..`0x1071` |
| `G[0x3acd28]` | 6 | The sound's delay in ms, 87 effects (100×49, 10×18, 200×14). | `ANIME@0x1088`..`0x109d` |
| `G[0x3acd2e]` | 18 | Up to three cue times a slot, 8 effects. At each cue the picture object `0x4651` shakes by 4 pixels for 100 ms, only while `G[0x952a4]` is set. | `ANIME@0x11b7`..`0x1287`; the shake at `@0x1129`..`0x114c` |

The effect is drawn centred: its base point less half its width or height, plus the offset
(`ANIME@0xae1`..`0xb2c`). Unlike Verita, bg0's work arrays hold the six animation slots only; there are no condition
slots. The work arrays are refilled for each animation.

### EEINIT (encounter chances)

EEINIT is five `copy-inline-int-array` rows into `G[0x125559]`, ten cells a row (`EEINIT@0x0`..`0x14`). It has no
leader and needs none. Its values match Verita's.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x125559]` | 10 | 0..9 | 5 | 0..40, 16 distinct | The percent chance of a battle at a field roll: the row is the distance band, the column the encounter level (`FIELD@0x14b1`..`0x1589`). |

| row | values |
|---|---|
| 0 | 0 0 0 0 0 0 0 0 1 2 |
| 1 | 0 0 0 0 0 1 1 2 3 4 |
| 2 | 0 1 1 2 3 3 4 6 8 10 |
| 3 | 0 1 3 4 6 6 8 12 16 20 |
| 4 | 0 2 5 6 9 9 12 18 32 40 |

FIELD rolls each time the distance `G[0x317a]` passes another 20 (`FIELD@0x10f1`..`0x110d`). `FIELD@0x1153` sets the
band, its local `0x5125` = `G[0x317a]` / 20, only while `G[0x9050d]` is 0, bit 0 of `G[0x2fe5]` is clear,
`debug_map_mode` `G[0x386a6f]` is 0 and `G[0xbdb]` (story flag 1899) is 0. Band 0 never rolls.

- The row is the band: below 10, 20, 40, 70, or more (`FIELD@0x14b8`..`0x1510`).
- The column is LAINIT's `G[0xd9fae + field_land_id]` when it is set, else RMINIT's `G[0xc5285 + 30 × row + block]`,
  plus `G[0x3181]`, held to 0..9 (`FIELD@0x1515`..`0x1572`).
- A draw below the cell starts a battle (`FIELD@0x1572`..`0x1589`).

There is no condition bonus and no in-play test, unlike Verita. After the roll, `label_1fb3` handles a special
encounter, and `label_20da` sets `system_mode` to 4 and clears `G[0x9529b]` and `G[0x9529c]`.

### ENINIT (encounter sets)

The selector is `encounter_set_in_play` `G[0x125333]`. `FIELD@0x5c7`..`0x5e0` sets it from RMINIT,
`G[0xc4b7d + 60 × row + 2 × block + variant]`, the variant being `map_encounter_variant` `G[0x3180]`. FIELD then
runs the fragment and the base (above). 25 map scripts set the selector and run ENINIT themselves (`MP02B0207@0x276`,
for one).

ENINIT clears four arrays of 100 entries and fills them in one branch a set:

| array | cells | holds | evidence |
|---|---|---|---|
| `G[0x125334]` | 100 | The entry's weight; 0 skips it. | `SETENP@0x209`, `@0x264` |
| `G[0x125398]` | 100 | The low end of the entry's window, plus 16. | `SETENP@0x217`..`0x233` |
| `G[0x1253fc]` | 100 | The high end of the window, plus 16. | `SETENP@0x23a`..`0x248` |
| `G[0x125460]` | 200 | The entry's two formations (ESINIT ids), into `G[0x9529b]` and `G[0x9529c]`. | `SETENP@0x2eb`..`0x306` |

The base has 81 ids, 1 to 104. 21 of them, the even ids 2 to 50, write only windows, so they are empty. The other 60
sets hold 2 to 98 entries, with formations 511 to 933. `$1$ENINIT` adds 12 sets, 201 to 302, with formations 1511 to
1923. All 337 formations named are defined in ESINIT.

How SETENP draws (`SETENP@0x1c6`..`0x306`):

- It draws only when `G[0x386a6e]` is 0 and `G[0x9529b]` is still 0. A scripted battle sets `G[0x9529b]` itself: 177
  scripts write a formation into it (`MPEV01B0104@0x129`, for one; `FIELD@0x2090` adds `G[0x8f56c]` to 941 for
  異質な気配).
- It sums the weights of entries 1 to 99 (`SETENP@0x1ec`). Entry 0 is never drawn, but when no entry fits the draw
  keeps entry 0, so entry 0 is the fallback. Every set gives entry 0 a weight, and that weight is what FIELD tests to
  see whether the fragment loaded the set.
- An entry fits when its low is 0, or when the field gauge `G[0x3178]` is at least low − 16 and at most high − 16. The
  data uses five windows: −15 to −11, −10 to −5, −4 to 4, 5 to 10, 11 to 15.

The field gauge `G[0x3178]` is saved. FIELD lowers it by 1 each time the party walks MPINIT's decay distance
(`FIELD@0x10ad`..`0x10ec`; the world, below), and an item's 迷宮支配力 raises it, up to 15 (`USESKILL@0xee`).

### ESINIT (formations)

The selector is `formation_in_play` `G[0x125528]`. `SETENP@0x30b`..`0x365` runs the fragment and the base once for
each nonzero formation in `G[0x9529b + k]`, k = 0 and 1. ESINIT clears six arrays of eight slots:

| array | cells | holds | evidence |
|---|---|---|---|
| `G[0x125529]` | 8 | The slot's enemy, an EBINIT unit, into SETEN's input `G[0x9540c]`; 0 for none. | `SETENP@0x38a` |
| `G[0x125531]` | 8 | The first column the enemy may stand in, from 1; 0 for any. | `SETENP@0x3d6`..`0x441` |
| `G[0x125539]` | 8 | The last column; when it is not above the first, the first only. | `SETENP@0x3d6`..`0x441` |
| `G[0x125541]` | 8 | The first row, from 1; 0 for any of the five. | `SETENP@0x446`..`0x4b1` |
| `G[0x125549]` | 8 | The last row. | `SETENP@0x446`..`0x4b1` |
| `G[0x125551]` | 8 | The percent chance the slot is used; 0 for always. A draw at or above it drops the slot. | `SETENP@0x396`..`0x3be` |

Unlike Verita, there are no flag or link arrays. Placement (`SETENP@0x4b6`..`0x61a`) gives every free cell a key, a
draw plus 100 when the cell lies outside the slot's range, and the smallest key wins. The unit takes a square of its
size, EBINIT's `G[0xa0f8d]`. The second formation's enemies stand five rows further (`SETENP@0x6b7`..`0x6c5`). SETEN
then sets up the enemy, and `SETENP@0x6e2` sets bit `0x100` of its state. Enemy battlers count from 10
(`SETENP@0x30b`).

The base has 507 formations, 11 to 991, with units 11 to 950. `$1$ESINIT` adds 66, 1001 to 1923, with units 521 to
633. `ESINIT@0x844e` holds the comment 敵セットの設定が不足しています. 208 formations are named by no table: the
scripts that set `G[0x9529b]` themselves use them.

### ARINIT (the arena)

The arena has four ranks of seven rounds. A record is a round, id = 10 × rank + round, so the capacity of 40 is four
rows of ten. Leader `GS[0x1a20]` (the title). 28 records, ids 0 to 6, 10 to 16, 20 to 26 and 30 to 36, all in the
base. Each rank also has two cells of its own.

The `†` row `G[0xc1e64]` is not a round array: its cells are two arrays of four, one cell a rank, `G[0xc1e75]` and
`G[0xc1e79]` (`ARENA@0x970`, `@0x98c`). Each rank writes them before its first round, which is why the tool counts 3
writes before a layer's first record. The 2 cells the tool leaves outside, `G[0xc204f]` and `G[0xc20df]`, are the only
written cells of the party limit and the required characters, both for round 26 (`ARINIT@0x505`, `@0x50a`). Rows for
both are added below, from the image.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc1e64]` † | 3 | 0..2 | 4 | 17743..18186, 2 distinct 17743×4, 18186×4 | *(the two rank arrays, next rows)* |
| `G[0xc1e75]` | 1, by rank |  | 4 | 18186..18186, 1 distinct 18186×4 | The picture drawn at the bottom, CS480AA.AGF, centred at x 670 (`ARENA@0x98c`..`0x9a1`, `@0xc81`..`0xcb2`). |
| `G[0xc1e79]` | 1, by rank |  | 4 | 17743..17743, 1 distinct 17743×4 | The full-screen backdrop, BG048A.AGF (`ARENA@0x970`..`0x985`, `@0xc5b`..`0xc70`); `CHMODE@0x15e` reads it too. |
| `G[0xc1e7d]` | 1 |  | 28 | 1..7, 7 distinct | The round's mark: nonzero lists the round (`ARENA@0xe21`, `@0x1243`), and the value picks a 12 × 14 glyph of SO016.AGF (`ARENA@0xe59`..`0xe72`). It is the round number plus one in every record. |
| `G[0xc1ea5]` | 1 |  | 28 | 30..460, 28 distinct | The enemies' level (`SETEN@0x45`). |
| `G[0xc1ecd]` | 2 | 0 | 28 | 951..991, 28 distinct | The battle: two formations, into `G[0x9529b]` (column 0) and `G[0x9529c]` (column 1, never written) (`ARENA@0x708`..`0x756`). |
| `G[0xc1f1d]` | 1 |  | 28 | 100..12600, 28 distinct | The fee (掛金), drawn at `ARENA@0xf99` and paid at `@0x6d3`..`0x6e5`. |
| `G[0xc1f45]` | 1 |  | 28 | 150..50400, 28 distinct | The prize money (賞金), paid on the first win (`RESULT@0x307`; drawn at `ARENA@0x1316`). |
| `G[0xc1f6d]` | 2 | 0..1 | 28 | 18..2909, 31 distinct | Two reward items (賞品), ITINIT ids, given on the first win (`RESULT@0x342`, `@0x390`; drawn at `ARENA@0x1373`). |
| `G[0xc1fbd]` | 2 | 0..1 | 28 | 1..3, 2 distinct 1×28, 3×4 | How many of each (`RESULT@0x362`, `@0x3b5`; `ARENA@0x13a5`). |
| `G[0xc200d]` | 1 |  | 17 | 35..200, 16 distinct | The required average party level; 0 for none (`ARENA@0xa6c`..`0xa97`). |
| `G[0xc2035]` | 1 |  | 1 | 1..1, 1 distinct 1×1 | *(outside in the tool)* The largest party allowed; 0 for any (`ARENA@0xa9f`..`0xaca`). Round 26 has 1. |
| `G[0xc205d]` | 5 | 0 | 1 | 1..1, 1 distinct 1×1 | *(outside in the tool)* Five characters who must be in the party (`ARENA@0xaef`..`0xb29`). Round 26 needs character 1, セリカ. |
| `GS[0x1a20]` | 1 |  | 28 | プテテットタイプ / 混合タイプ / 砂鉄の悪漢者 | `arena_round_names`, the round's title (タイトル, `ARENA@0xef0`). |
| `GS[0x1a48]` | 4 | 0..2 | 28 | 必要LV無し / 必要LV無し / 必要LV無し | `arena_round_texts`: column 0 the condition (条件), drawn in the list (`ARENA@0xf2d`); columns 1 and 2 the description lines of the round under the cursor (`ARENA@0x1288`..`0x12cc`), which also reads a column 3 that is never written. |

`GS[0x1a48]` ends at `GS[0x1ae8]`, MPINIT's floor names.

ARENA shows one rank's seven rounds. Rank 0 is always listed; rank n (1 to 3) is listed while story flag `1928 + n`,
`G[0x470 + 0x788 + n]`, is set (`ARENA@0x863`..`0x8ad`). That is `item_key_held_flags` for item 28 + n, which
ADDITEM keeps at 1 while the item is held, so the ranks open with Ａクラス挑戦権 (29), Ｓクラス挑戦権 (30) and
Ｍクラス挑戦権 (31). The last round of rank n − 1 gives item 28 + n, and the last round of rank 3 gives 闘技場制覇の証
(32).

ARENA keeps the rank in `arena_rank_in_play` `G[0xc1e73]` (`ARENA@0x96b`) and the round in `arena_round_in_play`
`G[0xc1e74]`. The saved `arena_rounds_won` `G[0x8f568 + rank]` counts the rounds won, and the round in play starts
there (`ARENA@0xbb`). A round can be chosen up to the first one not yet won (`ARENA@0x5ce`..`0x600`). It can be entered
(`ARENA@0x66c`..`0x6e5`) when:

- its required average level is 0, or the party's average level is at least it. The average is of `character_levels`
  `G[0x8b2fe]` over the party list `G[0x8b14c]` (`ARENA@0x9a9`..`0xa1c`, `@0xa6c`..`0xa97`), both saved character
  state. The texts confirm it: the round with 35 reads 必要平均LV35;
- its party limit is 0, or the party is no larger (`ARENA@0xa9f`..`0xaca`);
- each of its five required characters is in the party (`ARENA@0xad2`..`0xb29`);
- and the money covers the fee (`ARENA@0x691`..`0x6c3`).

Otherwise a buzzer plays, and a closed round's condition text is drawn red (`ARENA@0xf04`..`0xf1d`). The fee comes off
the money (`ARENA@0x6d3`..`0x6e5`). `ARENA@0x708`..`0x79a` then sets the round's two formations into `G[0x9529b]` and
`G[0x9529c]` and `system_mode` to 4, and runs BTL. It also sets `G[0x95375]` = MPB999A.AGF and `GS[0x615]` = 闘技場,
which BTL uses in place of the block's battle backdrop (RMINIT's `G[0xc40f1]`) and the place's name
(`BTL@0x5556`..`0x55a5`).

ARENA sets `G[0x91054]` to 1 while it runs (`ARENA@0x19`, cleared at `@0x1531`). While it is set,
`SETEN@0x3e`..`0x50` takes each enemy's level from the round, not from RMINIT's `G[0xc4f01]`.

After the battle, when `G[0x952ab]` is 0 and the round was the first not yet won, `ARENA@0x7a6`..`0x810` adds one to
`arena_rounds_won` and moves to the next round. BTL sets `G[0x952ab]` to 1 at `BTL@0x475a` and `@0x4773`; 0 is taken
here as a win. RESULT pays only that first win (`RESULT@0x2e8`..`0x3d6`): the prize money, added to `money` up to
9999999 (`RESULT@0x11cf`..`0x11e4`), and the two reward items through ADDITEM (`RESULT@0x11a3`..`0x11c9`). For a round
already won, `RESULT@0x260`..`0x28a` zeroes each character's experience pool `G[0x952ac]` (units, above), so a round
won again gives no experience. A changed prize or reward reaches an old save only for a round not yet won.

### What is saved

No cell of these seven tables is saved, so a changed table reaches every loaded game at once. The saved cells this part
reads:

| cell | what it is | read at |
|---|---|---|
| `arena_rounds_won` `G[0x8f568 + rank]` | the rounds won in a rank | `ARENA@0xbb`, `@0x7b4`..`0x7d7`; `RESULT@0x267`, `@0x2ef` |
| story flags 1929..1931 | a rank's pass is held (`item_key_held_flags`, items 29..31) | `ARENA@0x88c` |
| `item_counts` `G[0x8632a + item]` | how many of an item are held | `RTN301@0x25`, `RTN304@0x45` |
| `character_tactics` `G[0x8e36e]` | the character's tactic, EBINIT's routine column | `BTL@0x13b0`..`0x1411` |
| `character_levels` `G[0x8b2fe]`, the party list `G[0x8b14c]` | the arena's average level | `ARENA@0x9a9`..`0xa1c` |
| `G[0x3178]` | the field gauge, against ENINIT's windows | `SETENP@0x233`, `@0x248` |
| `G[0x317a]`, `field_land_id` `G[0x3176]`, `G[0x3181]` | what the encounter roll reads: the distance walked, the land, and an offset added to the encounter level | `FIELD@0x10f1`, `@0x1515`..`0x1572` |
| `map_encounter_variant` `G[0x3180]` | which of the block's encounter sets | `FIELD@0x5ce` |
| `money` `G[0x8b159]` | the arena's fee and prize | `ARENA@0x691`, `RESULT@0x11cf` |

## The world

The world is the world map's areas, the towns and their sites, the field maps and their blocks, the land types, the
map objects, and the field sheets of the player's character. Resource ids resolve through
`build/games/bg0/asset-index.json`; an append id (`0x1000000 | n`) names a `$1$` file.

### Who runs what

- **At boot,** INIT2 runs AMINIT, MPINIT, SIINIT, LAINIT, OBINIT, CPINIT and OBINIT2 (`INIT2@0x18`, `0x1b`, `0x1e`,
  `0x2a`, `0x2d`, `0x30`, `0x48`). `$1$AUTORUN` then runs `$1$MPINIT`, which adds map 49, and the empty `$1$LAINIT`,
  `$1$OBINIT` and `$1$CPINIT` (`$1$AUTORUN@0x1c`..`0x28`). There is no `$1$OBINIT2`.
- **AMINIT** runs again each time the world map opens (`ALLMAP@0x5a`, `@0x67`) and when a scene shows the map
  (`SHOWALLMAP@0x19`).
- **SIINIT** runs again on each town visit (`TOWN@0x6a`).
- **RMINIT** runs each time FIELD starts (`FIELD@0x88`), and in CALLBACK_LOAD when `system_mode` is 1 and
  `map_entered` is set (`CALLBACK_LOAD@0xbc`..`0xde`).
- In 特別編, `ALLMAP@0x5a`, `TOWN@0x5d`, `FIELD@0x7b` and `CALLBACK_LOAD@0xd1` run `$1$AMINIT`, `$1$SIINIT` and
  `$1$RMINIT` in place of the base scripts (boot, above).

### Selectors and field state

| cell | what it is | evidence |
|---|---|---|
| `story_part` `G[0x30b0]` | The world map in play, 1 to 4 (scenes, above). AMINIT dispatches on it. | `AMINIT@0x32`..`0x6a` |
| `lead_unit_id` `G[0x30b1]` | セリカ's form, 1 to 4, which CHANGECHAPTER sets from `story_part` and copies into his unit `G[0x8b237]`. CBINIT and CPINIT are read by it. | `CHANGECHAPTER@0x1b`..`0x40`; `FIELD@0x1806` |
| `current_place_id` `G[0x30b2]` | The place in play: an area id, which is a map id (1 to 49) or a town id (50 up). FIELD, TOWN, SIINIT and RMINIT read their tables by it. | `ALLMAP@0x78d` sets it when an area is entered |
| `town_site_id` `G[0x3149]` | The town site entered. SCJUMP's context 3 tests it. | `TOWN@0x518` |
| `map_block_row` `G[0x3169]`, `map_block_column` `G[0x316a]` | The block's row (the floor) and column (the section). RMINIT's arrays are read at 30 × row + column, MPINIT's floor names by the row. | `FIELD@0x5c7`, `@0x606`; `FIELD@0x170` |
| `map_section_slot` `G[0x316c]` | The section slot in play, an index of `map_section_codes` `G[0x3182 + slot]`, whose value is map × 10000 + floor × 100 + section. DRAWMAP reads the object arrays by it. | `FIELD@0x1a7`..`0x230`; `DRAWMAP@0x3c6` |
| `map_continue` `G[0x316d]` | "MapContinue", per the debug text at `FIELD@0x235`..`0x25c`. While it is 0, FIELD copies the target position into the position (`FIELD@0x18e`..`0x198`); EVOLVEOBJ plays an object's change sound only while it is set. | the two readers |
| `map_player_x` `G[0x3170]`, `G[0x3172]`, `G[0x3174]` | The player's x, y and facing on the map. | DRAWMAP, DRAWWALK |
| `map_player_target_x` `G[0x3171]`, `G[0x3173]`, `G[0x3175]` | The target x, y and facing; MPINIT's start is written here. DRAWWALK walks toward them and sets the facing from the direction. | `FIELD@0xba`..`0xf1`; `DRAWWALK@0x0`..`0x130` |
| `field_land_id` `G[0x3176]` | The land under the player. LAINIT's arrays are read by it. | `_GetLand` (AGERC slot 2) at `FIELD@0x1734` |
| `map_entered` `G[0x317c]` | Set once FIELD has started on the current entry; FIELD reads MPINIT's start only while it is 0. | set at `FIELD@0x99`; cleared at `ALLMAP@0xe`, `TOWN@0x11` |
| `map_music_variant` `G[0x317f]`, `map_encounter_variant` `G[0x3180]` | Which of the block's three musics and two encounter sets are in play. Map scripts set them. | `FIELD@0x606`..`0x64c`, `@0x5c7`..`0x5e0` |
| `current_place_name` `GS[0xde]` | The place's name: "全体マップ" on the world map (`ALLMAP@0x50`), the area's name `GS[0x199e + place]` (`TOWN@0x28`, `FIELD@0x159`), or the floor's name (`FIELD@0x170`). | the writers |
| `script_arg_1` `G[0x91865]`, `script_arg_2` `G[0x91866]` | The shared arguments (battle, above). SHOWALLMAP saves `story_part` and `current_place_id`, shows the world and the area its caller put in these, and restores the two. | `SHOWALLMAP@0x5`..`0x14`, `@0x375` |

The facing (`DRAWWALK@0x0`..`0x130`, screen y downward): 0 down-left, 1 left, 2 up-left, 3 up, 4 up-right, 5 right,
6 down-right, 7 down.

### AMINIT (the world map's areas)

AMINIT clears its arrays, 100 areas each: `area_states` `G[0x30b3]`, `G[0xc1716]` (3 cells an area), `G[0xc18a6]`
(4), and `G[0xc1a36]`, `G[0xc1a9a]`, `G[0xc1afe]`, `G[0xc1b62]`, `G[0xc1bc6]`, `G[0xc1c2a]` and `G[0xc1c8e]`. It does
not clear `G[0xc1842]` or `area_names` `GS[0x199e]`, which it only overwrites. Then four tests at its start jump to
the branch of the world `story_part` names (`AMINIT@0x32`..`0x6a`): world 1 at `label_6d` (16 areas), 2 at `label_58f`
(20), 3 at `label_b40` (8), 4 at `label_ddf` (25), 69 records in all. INIT2 runs it with `story_part` 0, so at boot it
only clears: the tool's layout for AMINIT shows ten arrays of zeros, and its `G[0xc1842]` row of 400 cells is
`G[0xc18a6]`'s clears, which no script indexes by a variable.

An area's id is its place id, and the same id can appear in several worlds with other data: area 50 マクルの街 is in
worlds 1 and 2. The states are set only by `bit-set G[0x30b3 + area] 0x0` (bit 0) under story-flag tests.

| world | areas | towns |
|---|---|---|
| 1 | 1 to 12 | 50, 51, 52; and 90, a junction |
| 2 | 1, 2, 4, 6, 8, 12, 15 to 19, 21 to 24 | 51, 52, 60, 61, 62 |
| 3 | 25 to 31 | 70 |
| 4 | 1, 2, 4, 6, 7, 12, 15 to 19, 27, 30, 35 to 39, 41 | 51, 52, 61, 80, 81, 82 |

`$1$AMINIT` clears the same arrays. Its worlds 1 to 3 are empty, and its world 4 has one area, 49 夢幻回廊: pictures
ALT027A and ALS027A, kind 1, its point at (580, 140), and state bit 0 while story flag 9999 `G[0x2b7f]` is 1
(`$1$AMINIT@0x94`).

| array | cells per area | holds | evidence |
|---|---|---|---|
| `area_states` `G[0x30b3]` | 1 | Bit 0 shows the area. Bit 1 would play SE016.WAV (0x2ea4) and refuse entry, but no script sets it. Saved. | `ALLMAP@0x90`, `@0x734`..`0x751`; `SHOWALLMAP@0x3e` |
| `area_names` `GS[0x199e]` | 1 | The area's name (廃都ノヒア, マクルの街 …), copied into `current_place_name`. | `TOWN@0x28`, `FIELD@0x159` |
| `G[0xc1716]` | 3 | Pictures. Column 0 is the icon ALT0nnA.AGF, on texture 0x3e + n, drawn 120×120 centred. Column 1 is ALS0nnA.AGF, the 200×200 panel of the area under the cursor (texture 0x3d, at 600,400 or 600,−15); SHOWALLMAP marks the shown area with it. Column 2, towns only, is the town's backdrop MT0nnA.AGF (texture 0x3d, full screen). | `ALLMAP@0x9b3`, `@0xa30`, `@0xb1e`..`0xb8e`; `SHOWALLMAP@0xb2`, `@0xf4`; `TOWN@0x5ee`, `@0x691` |
| `G[0xc1842]` | 1 | The town's music, a BGM number (2 to 18). | `TOWN@0x188` |
| `G[0xc18a6]` | 4 | 1 in every cell of every town but area 62, which has a 2. Nothing reads it. | — |
| `G[0xc1a36]` | 1 | The kind: 1 enters FIELD (`system_mode` 3), 2 enters TOWN (5), 0 enters nothing (the junction). | `ALLMAP@0x752`..`0x78c` |
| `G[0xc1a9a]`, `G[0xc1afe]` | 1 | The x and y of the area's point (70 to 720, 60 to 550). The hot rectangle is ±40 around it. | `ALLMAP@0x118`, `@0x12b`; `SHOWALLMAP@0x5a`, `@0x6d` |
| `G[0xc1b62]` | 1 | The area the up direction reaches (joy 0). | `ALLMAP@0x84f`, `@0x5db`..`0x6f7` |
| `G[0xc1c8e]` | 1 | The area the right direction reaches (joy 1). | `ALLMAP@0x921` |
| `G[0xc1bc6]` | 1 | The area the down direction reaches (joy 2). | `ALLMAP@0x895` |
| `G[0xc1c2a]` | 1 | The area the left direction reaches (joy 3). | `ALLMAP@0x8db` |

The cursor skips an area whose state is 0 and follows its neighbour onward. The geometry agrees: area 1 is at
(150, 190), its up neighbour 51 at (200, 110) and its down neighbour 50 at (180, 250).

The world map's own pictures are not per area. Its background is `world_map_pictures` `G[0x386a59 + story_part]`
(INIT2, above), drawn on texture 0x3b (`ALLMAP@0x967`, `TOWN@0x5cb`, `SHOWALLMAP@0x8f`). ALLMAP plays BGM 0x1f
(`ALLMAP@0x6b`). Its label is a 250×60 cut of SO005.AGF (0x4539) at
(250 × ((world − 1) / 2), 600 + 60 × ((world − 1) mod 2)) (`ALLMAP@0xa44`..`0xa86`); the four cuts read
ブレニア内海付近, ケレース地方付近, ブレニア内海付近 and アヴァタール地方付近.

### SIINIT (town sites)

SIINIT clears its arrays, 30 sites each: `town_site_states` `G[0x314a]`, `G[0xc1cf2]` (2 cells a site), `G[0xc1d2e]`,
`G[0xc1d4c]`, `G[0xc1d6a]`, `G[0xc1d88]`, `G[0xc1da6]`, `G[0xc1dc4]`, and `G[0xc1de2]` (4). Then a chain of tests on
`current_place_id` fills the town in play: towns 50, 51, 52, 60, 61, 70, 80, 81 and 82 have branches, 51 sites in all;
the branch for 90 is empty (`SIINIT@0x5a`), and town 62 has none. At boot no town is in play, so the image holds the
clears alone, and the tool's layout for SIINIT shows only zeros. `$1$SIINIT`, which TOWN runs in 特別編, only clears.

Site ids run from 1, and TOWN loops over sites 0 to 29 (`TOWN@0x6d`..`0x182`). Site 1 of every town is the exit
(街を出る, 村を出る, 集落を出る). The states are set only by `bit-set … 0x0`.

| array | cells per site | holds | evidence |
|---|---|---|---|
| `town_site_states` `G[0x314a]` | 1 | Bit 0: the site is open. Only open sites are drawn and reached. Saved. | `TOWN@0x8f`, `@0x5b5` |
| `town_site_names` `GS[0x1a02]` | 1 | The site's name (街を出る, 大聖堂, ラギールの店 …). No script indexes it. | the data |
| `G[0xc1cf2]` | 2 | Column 1 is the site's picture MTS0nnX.AGF, on texture 0x3e + n: its top half drawn normally, its bottom half when the site is selected. Column 0 is never written. | `TOWN@0x630`, `@0x6f0`, `@0x7ae` |
| `G[0xc1d2e]`, `G[0xc1d4c]` | 1 | The x and y where the picture is drawn. | `TOWN@0x141`, `@0x154`, `@0x6e2` |
| `G[0xc1d6a]`, `G[0xc1d88]` | 1 | The hot area's left and top, relative to the site's x and y. | `TOWN@0xb9`..`0x133`; `find-hit-rectangle` (op `0x12e`) at `TOWN@0x268` |
| `G[0xc1da6]`, `G[0xc1dc4]` | 1 | The hot area's width and height. | `TOWN@0xb9`..`0x133` |
| `G[0xc1de2]` | 4 | The neighbours: 0 up (joy 0), 1 down (joy 2), 2 left (joy 3), 3 right (joy 1). A closed site is passed through to its own neighbour. | `TOWN@0x395`, `@0x3e8`, `@0x43b`, `@0x48e`, `@0x56c`..`0x5c3` |

Entering a site sets `town_site_id` and runs SCJUMP's context 3 (`TOWN@0x518`..`0x52f`). The town's music is
AMINIT's `G[0xc1842 + place]` (`TOWN@0x188`) and its backdrop AMINIT's `G[0xc1716 + 3 × place + 2]`.

### MPINIT (field maps)

Leader `G[0xc214d]`, capacity 50. 34 maps, ids 1 to 49: the base's 33 (1–8, 10–12, 15–19, 21–31, 35–39 and 41) and
map 49 from `$1$MPINIT`. FIELD reads the start arrays on entry (`FIELD@0xba`..`0xf1`) while `map_entered` is 0,
`debug_map_mode` is 0 and `G[0xc57b1]` is not 2.

The tool's `G[0xc22de] †` is the floor array seen one cell late: its base is `G[0xc22dd]` = `0xc22ab + 50`, on the
grid, and its columns are the floors 1 to 9, read at 100 × map + floor as `GS[0x1ae8]` is. No script indexes it, which
is why the tool guesses the base. It ends at `0xc22dd + 5000 = 0xc3665`, where RMINIT's arrays begin. Its row below is
recomputed from the image.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc214d]` | 1 |  | 34 | 13189..13190, 2 distinct 13189×33, 13190×1 | The object sheet, MO01.AGF (MO18.AGF for map 18), set on texture 0x66 (`FIELD@0x21f2`); DRAWMAP cuts every object from it (`DRAWMAP@0x44c`). |
| `G[0xc217f]` | 1 |  | 34 | 1..2, 2 distinct 1×33, 2×1 | The start floor, into `map_block_row`. |
| `G[0xc21b1]` | 1 |  | 34 | 1..14, 6 distinct 1×27, 2×3, 3×1, 9×1, 12×1, 14×1 | The start section, into `map_block_column`. |
| `G[0xc21e3]` | 1 |  | 34 | 190..1690, 31 distinct | The start x, into `G[0x3171]`. |
| `G[0xc2215]` | 1 |  | 34 | 100..1865, 31 distinct | The start y, into `G[0x3173]`. |
| `G[0xc2247]` | 1 |  | 30 | 2..7, 5 distinct 2×5, 3×6, 4×11, 6×7, 7×1 | The start facing, into `G[0x3175]`; four maps leave it 0. |
| `G[0xc2279]` | 1 |  | 34 | 50..300, 9 distinct | The field gauge's decay distance. While the gauge `G[0x3178]` is above −15, FIELD adds each step to `G[0x3179]`; when that reaches 10 × this value, the gauge falls by 1 and `G[0x3179]` restarts at 0 (`FIELD@0x10ad`..`0x10ec`). |
| `G[0xc22ab]` | 1 |  | 13 | 1..1, 1 distinct 1×13 | 1 on maps 2, 4, 6, 7, 8, 12, 16, 17, 18, 19, 21, 27 and 30. `CHANGECHAPTER@0x4a4`..`0x576` clears all 500 saved section slots unless this cell is 1. It reads the cell of the map of the slot in play (`map_section_slot`'s code / 10000), not each slot's own map, so the choice is all or nothing. |
| `G[0xc22de]` † | 100 | 0..8 | 34 | -7..6, 13 distinct | *(the floor numbers `G[0xc22dd]`, next row)* |
| `G[0xc22dd]` | 100 | 1..9 | 34 | -7..6, 13 distinct | Each floor's number as its name shows it; below 0 is under ground. No script reads it. |
| `GS[0x1ae8]` | 100 | 1..9 | 34 | 廃都ノヒア / 鉄屑の谷 / クライナの集落 | `map_floor_names`, each floor's name. `FIELD@0x170` copies it into `current_place_name`, and `BTL@0x559a` reads it. |

After the start arrays are read, `FIELD@0x18e`..`0x198` copies the target x, y and facing into the position while
`map_continue` is 0, and `FIELD@0x1a7`..`0x230` finds or allocates the section slot. There is no name-plate array and
no stray-demon array, as Verita has.

### RMINIT (map blocks)

RMINIT clears all its arrays, then a chain of tests on `current_place_id` fills the map in play. The base covers the
same 33 maps as MPINIT; `$1$RMINIT` covers only map 49. A block is 30 × `map_block_row` + `map_block_column`; the
arrays are 450 cells, 15 rows of 30. The base's blocks use rows 1 to 7, and map 49's rows 1 to 9, up to slot 282.

| array | cells per block | holds | evidence |
|---|---|---|---|
| `G[0xc3665]` | 1 | The section's map script, MPmmFrrss.BIN. | run at `FIELD@0x2c0` |
| `G[0xc3827]` | 1 | The section's event script, MPEVmmFrrss.BIN. | run at `FIELD@0xd2a` |
| `G[0xc39e9]` | 1 | Picture A, …A.AGF (2040×2040 for MP01F0101A), loaded on raw surface 0x3b. Its size becomes the map's size `G[0xc57b5]`, `G[0xc57b6]`. | `FIELD@0x2132` |
| `G[0xc3bab]` | 1 | Picture B, …B.AGF, on surface 0x3c, or 0 for none. DRAWMAP draws an 800×600 window of it at the scroll as element 0x9c4. | `FIELD@0x2152`; `DRAWMAP@0x46f`..`0x484` |
| `G[0xc3d6d]` | 1 | An overlay: SO026A.AGF (a 200×200 picture) in 235 blocks, SO026B.AGF in 1, set on texture 0x3d. DRAWMAP draws it as element 0x1387 at the negative scroll, scaled to cover the map. | `FIELD@0x217d`; `DRAWMAP@0x495`..`0x4fc` |
| `G[0xc3f2f]` | 1 | The minimap MPS…A.AGF, on texture 0x8e in the 200×200 surface 0x70. | `FIELD@0x24a8`..`0x24d3` |
| `G[0xc40f1]` | 1 | The battle backdrop MPBnnnX.AGF, on texture 0x2e; 0 uses MPB001A.AGF (0x2e9c). | `BTL@0x556c`..`0x5593` |
| `G[0xc42b3]` | 1 | Never written and never read. | — |
| `G[0xc4475]` | 3 | The music: a BGM number (3 to 35), read at 90 × floor + 3 × section + `map_music_variant`. 0 fades the music out. | `FIELD@0x606`..`0x64c` |
| `G[0xc49bb]` | 1 | Always 1, never read. | — |
| `G[0xc4b7d]` | 2 | The encounter set, an ENINIT id (1 to 105 in the base, 201 to 302 on map 49), read at 60 × floor + 2 × section + `map_encounter_variant` into `encounter_set_in_play`. Map scripts read it too (`MP02B0207@0x26b`). | `FIELD@0x5c7`..`0x5e0` |
| `G[0xc4f01]` | 1 | The enemies' base level (1 to 210 in the base, 250 to 350 on map 49). SETEN adds part of the party's average-level lead, through EBINIT's `G[0xadaad]`, and holds the result between EBINIT's base level and highest level. | `SETEN@0x58` |
| `G[0xc50c3]` | 1 | The level spread v, 1 to 10: SETEN draws 0 to 2v and subtracts v. | `SETEN@0x10e`..`0x150` |
| `G[0xc5285]` | 1 | The encounter level, 2 to 9, EEINIT's column (battle, above). LAINIT's `G[0xd9fae]` replaces it when set. | `FIELD@0x1515`..`0x1577` |

### LAINIT (land types)

Leaders `G[0xd9cf2]` and the six other arrays, capacity 100: a land's record may start at any of them (the currents
write only the drift and the walk flag, lands 19 and 21 only the encounter level, land 2 only the speed). 17 lands,
ids 1 to 21, all in the base. FIELD reads them by `field_land_id`; land 0 is none.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xd9c8e]` | 1 |  | 7 | 13185..13188, 4 distinct 13185×3, 13186×1, 13187×2, 13188×1 | The footstep loop on this land, WALKB.WAV, WALKC.WAV, B0208.WAV or B0016.WAV; 0 plays WALKA.WAV (0x453a). It loops on channel 3 (`FIELD@0x181f`..`0x1841`, `@0x1f84`; `DRAWWALK@0x1a6`). |
| `G[0xd9cf2]` | 1 |  | 7 | 1..5, 5 distinct 1×3, 2×1, 3×1, 4×1, 5×1 | The column of CPINIT's sheets, loaded on texture 0x67 when the land changes; 0 gives column 0 (`FIELD@0x17d2`..`0x1811`; `DRAWCH@0xe`; `DRAWJUMP@0xdc`). |
| `G[0xd9d56]` | 1 |  | 6 | -50..-30, 2 distinct -50×3, -30×3 | The speed change in percent: the step is max(1, 60 × (100 + v) / 1000), doubled while FIELD's local `0x5137` is set (`FIELD@0x1795`..`0x17cb`; `DRAWWALK@0x135`). |
| `G[0xd9dba]` | 1 |  | 7 | -50..50, 4 distinct -50×1, -35×3, 35×2, 50×1 | The drift in x: each frame adds v × FIELD's local `0x28b` / 100 to the step (`FIELD@0x1a4d`..`0x1a62`, `@0x1ec2`). |
| `G[0xd9e1e]` | 1 |  | 7 | -50..50, 4 distinct -50×1, -35×2, 35×3, 50×1 | The drift in y, the same way (`FIELD@0x1a69`..`0x1a7e`, `@0x1ede`). |
| `G[0xd9e82]` | 1 |  | 9 | 1..1, 1 distinct 1×9 | 1 lets the player walk against the drift; with 0, FIELD drops the part of the step that opposes it (`FIELD@0x19f6`..`0x1a48`, `@0x1e6b`). Every land with a drift has 1. |
| `G[0xd9fae]` | 1 |  | 2 | 9..9, 1 distinct 9×2 | An encounter level that replaces the block's (`FIELD@0x1515`): 9 on lands 19 and 21. |

The lands: 1 sheet column 1, WALKB, speed −50; 2 speed −30 only; 7 to 14 currents in eight directions, 7 (0, 50) down,
8 (−35, 35), 9 (−50, 0), 10 (−35, −35), 11 (0, −50), 12 (35, −35), 13 (50, 0), 14 (35, 35); 15 column 2, WALKC, −30;
16 column 3, B0208, −50; 17 column 4, B0208, −50; 18 column 5, B0016; 19 encounter level 9; 20 column 1, WALKB, −30
and the current (−35, 35); 21 column 1, WALKB, encounter level 9. The two arrays between the drift flag and the
encounter level, `G[0xd9ee6]` and `G[0xd9f4a]`, are neither written nor read. LAINIT's arrays end at `0xda012`, where
OBINIT begins.

### OBINIT and OBINIT2 (map object kinds)

One kind id runs through both tables, capacity 1000: 129 kinds, ids 1 to 658, all in the base.

OBINIT's leaders are `G[0xda7e2]`, the sprite x `G[0xda012]` and the sprite y `G[0xda3fa]`, since each record writes
its x and y before its width.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xda012]` | 1 |  | 117 | 40..960, 31 distinct | The sprite's x on the map's object sheet (texture 0x66; MO01.AGF is 1024×1024). |
| `G[0xda3fa]` | 1 |  | 112 | 60..940, 25 distinct | The sprite's y on the sheet. |
| `G[0xda7e2]` | 1 |  | 129 | 20..140, 8 distinct | The sprite's width. |
| `G[0xdabca]` | 1 |  | 129 | 40..120, 6 distinct 40×5, 50×2, 60×78, 80×12, 100×9, 120×23 | The sprite's height. |
| `G[0x10c4c2]` | 1 |  | 41 | -1..658, 35 distinct | The kind the object turns into. `EVOLVEOBJ@0x1d`..`0xe3` turns it when the object's switch `G[0xd4829 + k]` is above 0 and `G[0x83c1a + switch]` is set: a value above 0 replaces the kind, −1 removes the object (kind 0). |
| `G[0x10c8aa]` | 1 |  | 29 | 13191..13193, 3 distinct 13191×24, 13192×3, 13193×2 | The sound played when the object turns, SE001.WAV, B0401.WAV or B0302.WAV; EVOLVEOBJ plays it only while `map_continue` is set. |

`DRAWMAP@0x40c`..`0x44c` cuts the sprite (x, y, width, height) from surface 0x66 and draws it at the object's map x
`G[0x45226 + 100 × slot + k]` and y `G[0x51576 + 100 × slot + k]`, less the scroll `G[0xc57b3]`, `G[0xc57b4]`. The
objects belong to the section slot: the kind `G[0x38ed6 + 100 × slot + k]`, the flags `G[0x2cb86 + 100 × slot + k]`
(drawn while bit 16 is set and bit 0 clear, `DRAWMAP@0x3c6`) and the graphics element `G[0xc56cc + k]`. 67 scripts read
the next kind and the sound: EVOLVEOBJ, OPENDOOR, 14 MP scripts, 42 MPEV scripts and 9 `$1$` scripts. There is no
blend-mode array, as Verita has.

OBINIT2: leader `G[0xdafb2]`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xdafb2]` | 1 |  | 129 | -9999..9999, 16 distinct | The depth offset: y + this value is the object's sort key (`DRAWMAP@0x7f`..`0xc0`); ±9999 puts it at an end of the order. |
| `G[0xdb39a]` | 1 |  | 111 | 5..7, 2 distinct 5×85, 7×26 | The number of wall points, into `G[0xc597d + polygon]` (`FIELD@0xefa`..`0xfe6`). |
| `G[0xdb782]` | 100 | 0..6 | 111 | 0..130, 14 distinct | The wall points' x. FIELD adds the object's position and writes the sum to `G[0xc5a45]`, 100 a polygon. |
| `G[0xf3e22]` | 100 | 0..6 | 111 | 20..115, 19 distinct | The wall points' y, written the same way to `G[0xca865]`. |

`FIELD@0xefa`..`0xfe6` makes one polygon per object, polygon = `G[0xd9c8a]` + k, sets `G[0xcf685 + polygon]` = 1 and
calls AGERC's `_SetMapWall*`. The two tables' arrays run on without a gap: x `0xda012`, y `0xda3fa`, width `0xda7e2`,
height `0xdabca`, depth `0xdafb2`, count `0xdb39a`, wall x `0xdb782`, wall y `0xf3e22`, then OBINIT's next kind
`0x10c4c2` and sound `0x10c8aa`.

### CPINIT (the field character's sheets)

Leader `G[0xc5447]`, 30 cells a row, capacity 10. Four rows, keyed by `lead_unit_id`, セリカ's form (`FIELD@0x1806`);
columns 0 to 5 (A to F) are LAINIT's sheet columns.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc5447]` | 30 | 0..5 | 4 | 12124..12170, 24 distinct | The walking sheet CPM00nX.AGF (492×800). FIELD loads the column of the land under the player on texture 0x67 (`FIELD@0x1806`, `@0x2207`; `DRAWCH@0x36`; `DRAWJUMP@0x221`). |
| `G[0xc5573]` | 30 | 0..5 | 4 | 12125..12171, 24 distinct | The jumping sheet CPJ00nX.AGF (492×360) (`DRAWJUMP@0xe8`). |

Each array is 300 cells, 10 rows of 30; `G[0xc569f]`, the next array in use, follows the second at once. Sheets B to F
are variants of the walking sheet, and F shows the character in a box-like vehicle. Which land shows which look is
LAINIT's data (columns 1 to 5 on lands 1, 15 to 18, 20 and 21).

### What is saved

The world's tables lie above the prefix and are rebuilt, not saved. These cells are saved: the area states
`G[0x30b3]` … `G[0x3116]`, the site states `G[0x314a]` … `G[0x3167]`, the selectors above, `current_place_name`,
`map_section_codes` `G[0x3182]`, and the objects of each of the 500 section slots (`G[0x2cb86]`, `G[0x38ed6]`,
`G[0x45226]`, `G[0x51576]`, 100 a slot). What that means for a loaded game:

- **Area and site states:** AMINIT and SIINIT rebuild them on every opening, so a changed condition reaches a loaded
  game at the next world map or town visit. SHOWALLMAP leaves the shown world's states in place until the next ALLMAP.
- **MPINIT's start:** it is copied into the saved floor, section, x, y and facing only on entry from the world map
  (`map_entered` 0), so a changed start reaches a loaded game at the next entry.
- **Object kinds:** a section's kinds are saved, so a changed OBINIT next kind applies only to objects that have not
  turned yet.

## Information screens

INFO (`ROOM@0x43a`) leads to INFO1, the glossary. INFO2 is the 人物 book (CIINIT, above), INFO3 the bestiary
(EBINIT), and INFO4 lists items (ITINIT, `INFO4@0x8b7`).

### VIINIT (the glossary)

INIT2 runs VIINIT, its 21st call; `$1$VIINIT` is empty. Leader `GS[0x81db]` (the title), capacity 200: 90 entries,
ids 1 to 90 (`INFO1@0x88a` walks 0..199). The tool counts one write before the first record: `VIINIT@0x0` writes entry
1's first unlock before its title.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x3aced2]` | 3 | 0..2 | 90 | 1..1029, 45 distinct | Up to three unlock scenes, each plus 1. Column 0 is set on all 90 entries, column 1 on 7, column 2 on 2. `INFO1@0x8ba`..`0x921` lists an entry when any of these scenes has its `scene_seen_flags` cell set, loaded from the shared profile (`@0x902`); while `debug_map_mode` is set, `INFO1@0x8ec` sets the flag instead. |
| `G[0x3ad12a]` | 1 |  | 90 | 18619..18866, 40 distinct | The entry's picture (CI502A.AGF …). `INFO1@0xeaa` sets it as a texture and draws it at (590 − width / 2, 600 − height), plus the offsets. |
| `G[0x3ad1f2]` | 1 |  | 90 | -23..-23, 1 distinct -23×90 | The picture's x offset (`INFO1@0xedb`). |
| `G[0x3ad2ba]` | 1 |  | 90 | -203..-203, 1 distinct -203×90 | The picture's y offset (`INFO1@0xeee`). |
| `GS[0x81db]` | 1 |  | 90 | 【用語】二つの回廊の終わり / 【用語】三神戦争 / 【用語】先史文明期 | `glossary_titles`, the entry's title; `INFO1@0xd2d` lists it. |

`INFO1@0xe89` sets `glossary_current_entry` `G[0x3aced1]` to the chosen entry. `VIMES@0x6` branches on that cell and
shows the entry's text (`VIMES@0x14` onward). None of VIINIT's arrays is saved; what an entry needs is the shared
profile's scenes seen.

## Galleries

ROOM offers CGMODE, the CG gallery, and HMODE, scene replay (`ROOM@0x3fe`..`0x44e`).

### CGINIT (the CG gallery)

INIT2 runs CGINIT, its 18th call, and `$1$AUTORUN` runs `$1$CGINIT`, which adds CGs 712..715 on tab 5, slots 24 to
27. Leader `G[0x9186b]`, capacity 2000: 715 CGs, ids 1 to 715, 711 in the base and 4 from the fragment.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x9186b]` | 2 | 0..1 | 715 | 45..16777246, 829 distinct | Column 0 is the CG's picture (EV002A.AGF …; `$1$EV200A.AGF` … for 712..715). Column 1, set on 338 CGs, is a 160 × 120 thumbnail (EVM008G.AGF …, 114 distinct). After a save from the world map, `ALLMAP@0x2aa`..`0x2fc` finds the picture on screen in column 0 and draws column 1 as the slot's thumbnail, if it is 160 × 120; `SAVE@0xb04`..`0xb2f` does the same. `CGMODE@0x9a7` and `@0xb7f` show column 0. |
| `G[0x9280b]` | 1 |  | 709 | 1..5, 5 distinct 1×157, 2×133, 3×154, 4×150, 5×115 | The CGMODE tab (`CGMODE@0x959` takes 1 off it). |
| `G[0x92fdb]` | 1 |  | 709 | 1..30, 30 distinct | The slot on the tab (`CGMODE@0x967`). |
| `G[0x937ab]` | 1 |  | 709 | 1..30, 17 distinct | The variant within the slot. `CGMODE@0x94b` skips a CG whose variant is 0, and `@0x983`..`0x9a0` counts each slot's variants. |
| `G[0x93f7b]` | 1 |  | 31 | 26..148, 7 distinct | The x scroll of a picture larger than the screen: `CGMODE@0xbcc` places it at x = −value. |
| `G[0x9474b]` | 1 |  | 77 | 40..235, 10 distinct | The y scroll, the same way (`CGMODE@0xbdf`). |

CGMODE counts a CG as unlocked when `is-catalog-resource-unlocked` holds for its picture (`CGMODE@0x9b2`), or when the
shared profile's `G[0x90c1a + cg]` is set (`@0x9b7`..`0x9c8`; boot, above). CGs 154, 155, 156, 254, 293 and 379 have
a picture but no tab, slot or variant, so CGMODE never shows them.

### SPINIT (scene replay)

INIT2 runs SPINIT, its 19th call; `$1$SPINIT` is empty. Leader `G[0x94f1b]`, 15 cells a page, capacity 75: a record's
id is 15 × page + cell. 41 replays, ids 0 to 40.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x94f1b]` | 1 |  | 41 | 16021..16061, 41 distinct | The replay's script: SP0003.BIN, SP0124.BIN … SP0582.BIN, one for each of the 41 SP scripts. `HMODE@0x8b9` reads it by page × 15 + cell over 5 pages, and `HMODE@0xb44` runs the chosen one. A cell counts as unlocked when its script is catalog-unlocked (`HMODE@0x8dd`); `HMODE@0x922` turns the count into a percentage. |

HMODE turns pages modulo 3 (`HMODE@0x84c`), so it shows pages 0..2: the 41 replays fill pages 0 and 1 and 11 cells of
page 2.

### What is saved

No gallery array is saved: they all lie above the prefix. The unlocks are kept by the engine's catalog and by the
shared profile's `G[0x90c1a + cg]`, which lies below the prefix too, so a save carries a copy.

## What is not decoded

**Boot.**

- `G[0x451]`: the test just before SAVE's question (`SAVE@0x7f5`..`0x81d`).
- The option `G[0x90511]`, which BTL reads.
- `G[0x91014]` … `G[0x91018]` = 0, 4, 5, 6, 9 from SYSTEM4; its first cell is a sound channel in DEBUGADV2 and SC0360.
- INIT2's unit list `G[0x389180]` … `G[0x389187]` (`BTL@0x154b`, `UNSUMMON@0x16f`).
- SAVE's flag per save slot, `G[0x90c1a + 800 + slot]`.

**Scenes.**

- `G[0x2be4]`: the dispatch repeats while it is −1, but no script writes −1.
- `G[0x2b80]` … `G[0x2be3]`: each scene clears them, and nothing reads them.
- Story flag 9999, `G[0x2b7f]`: AMINIT (18 tests, the first at `AMINIT@0x477`), `$1$AMINIT@0x94` and `SCJUMP@0x307e`
  test it for 1, and no script writes it by its address. `$1$AMINIT` opens 夢幻回廊 by it.
- The 52 scenes that do not set their own story flag, and the 188 scenes that no script names with a constant.
- SCINIT's seven pass-1 pairs that match no SCJUMP rule: scene 1510 five times, a second 35, and 395.
- `TITLE@0xa06`..`0xa10`, which sets the mode to 1 and plays scene 2000 (SC2000.BIN, scenes 2000..2009).
- `G[0x952ab]` and `G[0x9529d]`, which gate SCJUMP's context 5; `G[0x952ab]`'s values besides 0 and 1.

**Units, characters and speakers.**

- EBINIT: what the voice columns mean beyond those cited, picture columns 1 and 2, sprite columns 1..9, the tactic
  column 8, the FS temperament values other than 3 (no unit has them), and the boss-class tests at `BTL@0x38a8`,
  `@0x3a2b` and `@0x3f13`.
- EBINIT's block also holds two arrays EBINIT does not write: `G[0xa1375]` (no reader) and `G[0xabf55]` (5 cells a
  unit, listed by `CHMODE2@0x124a` with the support effect).
- Character flag bit 2; the FS offset `G[0x8b3c6]`; the equipment slots `G[0x8c23a]` and `G[0x8c29e]`; CALCCHP's caps
  `G[0x8b22f]` …; bit 0 of `G[0x2fe8]` (LoadClear in UNITECH's log).

**Items, conditions and constants.**

- ITINIT's `G[0x127c9d]` and `G[0x340e5d]` (no reader), `GS[0x5580]` (the author's notes), and what the 7 ranks spell
  beyond "Cランク" = 3.
- ILINIT: the use of cure bit 1 in battle; arrays 20 and 21, read and never written; conditions 4..6 and 35..36, which
  have names only.
- AFINIT's `G[0x386a1d]` bit 0: it blocks the shield slot, which suggests a two-handed weapon; not confirmed.
- SHOP's pages 2 and 3 (its local `0x31b3`), and whether shop 12's slot 7 is shown: the display loops stop at 7 slots.
- The object switches `G[0xd4829 + k]`: EVOLVEOBJ tests `G[0x83c1a + switch]`, which is the `treasure_opened` bank,
  so a switch is a treasure id or shares its numbering; which is not established.

**Battle, enemies and routines.**

- RTINIT's bank 6, never written and never read, and bank 7, never written.
- What CHECKRECV decides after RTN101, RTN102 and RTN304 to RTN306.
- Bit 1 of `G[0x38a9f9]`, `G[0x8f566]`, and how BTANINIT2's split styles 1 and 2 differ: both cut two-pixel strips.
- The encounter roll's gates `G[0x9050d]`, bit 0 of `G[0x2fe5]` and `G[0xbdb]` (story flag 1899), and who sets
  `G[0x3181]`, the encounter level's offset.

**The world.**

- AMINIT's `G[0xc18a6]`, all 1 but area 62's 2: nothing reads it.
- RMINIT's `G[0xc42b3]` (never written or read) and `G[0xc49bb]` (always 1, never read); what RMINIT's overlay
  `G[0xc3d6d]` shows in play.
- LAINIT's `G[0xd9ee6]` and `G[0xd9f4a]`, neither written nor read.
- MPINIT's `G[0xc22ab]`: what the game means by keeping section states through a chapter change, when the cell read
  is only the slot in play's map.
- `map_continue` `G[0x316d]` beyond its two readers, and what reads `current_place_name` `GS[0xde]`.
- The other per-slot arrays: `G[0x3376]` (200 a slot), `G[0x1ba16]` (40 a slot), `G[0x20836]` and `G[0x5d8c6]`.
- What each of CPINIT's six looks means on its land; no one has checked them in play.

**Information screens and galleries.**

- VIINIT's unlock value 995, which names scene 994; SCINIT has no scene 994.
- The six CGs with no place in CGMODE.
