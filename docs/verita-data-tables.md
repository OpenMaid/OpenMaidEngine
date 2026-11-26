# Verita data tables (`*INIT`)

Canonical home for how 戦女神VERITA (Verita, SYS4424) stores its static game data: which `*INIT` scripts exist,
how they are layered, where each table lives in the global bank, and what is known of its fields. Names for the
fields that are understood live in [`vm-map/globals-verita.toml`](../vm-map/globals-verita.toml) (generated →
[`global-reference-verita.md`](global-reference-verita.md)). The other games' equivalents are
[`arterial-data-tables.md`](arterial-data-tables.md) and [`kamidori-data-tables.md`](kamidori-data-tables.md).

**How far this goes (2026-11-25).** Every table's *layout* is here: its arrays, their strides and what values they
hold, for the 37 boot tables and the 11 that run on demand. What an array *means* is given
where the data or a consumer shows it, each reading with the script and offset it rests on. A row that says nothing
in its last column is not read yet; "What is not decoded" at the end lists the gaps.

## How the data gets into memory

Like the other games, Verita has no data files. Every table is a script of constant writes into the flat global
bank: `mov` for integers, `set-string` for text, `copy-inline-int-array` for a few bulk arrays. Runtime code reads
the bank with `lookup-array` (`base + id`) and `lookup-array-2d` (`base + row * stride + column`).

**One array per field.** A table is a set of parallel arrays. A record's field is the cell
`base + cells_per_id * id + column`: one cell per id for a scalar, several for a short list. Each array is allocated
for the table's *capacity* (1000 units, 10000 items, 600 skills) whether or not an id is used, and the arrays of a
table lie back to back. Ids are sparse and mostly start at 1.

**Boot order.** `INIT2.BIN` calls 37 initializers in this order: OBINIT2, MUINIT, CBINIT, EBINIT, CNINIT, CHINIT,
ITINIT, ILINIT, SSINIT, RTINIT, AFINIT, TIINIT, SKINIT, AMINIT2, ALINIT, ARINIT, IMINIT, MPINIT, EEINIT, TRINIT,
LAINIT, OBINIT, SOINIT, TPINIT, CAINIT, RUINIT, SRINIT, GRINIT, CGINIT, SPINIT, CIINIT, VIINIT, CVINIT, FAINIT,
BTANINIT2, EX1INIT, SCINIT. Op `0x143` then runs each mounted append catalog's `$n$AUTORUN.BIN` in ascending
selector order, and each of those calls its own `$n$<TABLE>.BIN` for 33 of the tables. The install has three
appends, so a table's runtime image is the **base, then `$1$`, `$2$`, `$3$`**, later writes winning. A fragment
adds records and also rewrites cells of records an earlier layer made; a fragment of a few lines is an empty stub.
Of the 99 fragments, 18 of `$1$`, 14 of `$2$` and 22 of `$3$` write anything; the largest are `$3$EBINIT` (3158 cells) and
`$3$ITINIT` (2681). Four tables have no fragment: EEINIT, EX1INIT, ILINIT and MUINIT.

**Hook chains.** `INIT2` and each `$n$AUTORUN` also fill 36 arrays of 32 slots, `G[0xc78130]` to `G[0xc78590]`:
slot 0 the base script and slot n append n's. The game's walkers run every slot in turn, so an append's part of a
table or a handler runs after the base's. Eleven tables run only through such a chain, not at boot: AMINIT,
SIINIT, SHINIT, QUINIT, RMINIT, ENINIT, ESINIT, CSINIT, PZINIT, STINIT and BTANINIT, so they are not in the boot
image. "Boot" below has every chain and its walker; each on-demand table's section says who runs it and when.

## Which files are authored data

`games/verita/` holds the archives `DATA1`–`7.ALF` and `APPEND01`–`03.ALF` (with their `.AAI` indexes), and 182
loose `.BIN` files in its root: `SYS4INI.BIN`, `SYS4RK.BIN`, 161 with the name of a base record and 19 `$n$` ones
(12 of `$1$`, 7 of `$2$`). Of the 180 that have a record in an archive, 176 differ from it and 4 are identical.
Native AGE reads a loose file before the archive, and the data tooling reads the install the same way
(`tools/paths.py`'s `scripts()`), so every count below includes them. (Where the loose files come from is not
established here.)

## Tooling

All commands run from `age-reimpl/` with `py -3.11 -X utf8` and `--game verita`
([`tools-reference.md`](tools-reference.md)):

1. Extract the archives once: `bin/BinExtractALF.exe ../games/verita/SYS4INI.BIN ../extracted-verita`, and each
   `APPENDnn.AAI` so that its records land flat in `../extracted-verita/APPENDnn/`.
2. `parse_sys4ini.py` → the asset index and call-script names.
3. `extract_phase2.py` → `build/games/verita/disasm/*.asm` and the text corpora.
4. `init_image.py` → `build/games/verita/init-image.json`, the **executed** boot image: the port's VM runs the boot
   order above and records what each of the 136 scripts wrote.
5. `init_layout.py` → each table's layout, inferred from the write order and the image, with Verita's leaders
   (`VERITA` in the tool). `init_layout.py <TABLE>` prints a table's arrays column by column, `--markdown` the
   layout tables below, and `--write` saves them as `build/games/verita/data/<TABLE>.layout.json`.

`init_layout.py` is given the array each record writes first (the *leader*) and the table's capacity, takes a
record's id from the leader cell, and finds every other field as the `(stride, base)` on which the most records
agree. LAINIT has six leaders, since fifteen of its records write no cell of the first. 31 boot tables have a
leader. The others (AFINIT, SKINIT, IMINIT, EEINIT, TRINIT, EX1INIT) are shown by the arrays the scripts index
(`lookup-array` operands), which is a weaker view: an array no script indexes with a variable is not seen, its
cells are counted in the array before it, and the base shown is the indexed array's, not always the field's.
AFINIT's arrays have three capacities (20, 60 and 16), so no one leader fits it. The tool does not lay out the
on-demand tables, which are not in the boot image; their sections are read from their listings.

**The grid, and `†`.** A table's arrays lie back to back, each a whole number of capacities long, so their bases
agree modulo the capacity. The tool marks an array whose base does not: `†` in the layouts below. Such an array
either has another table's cells between it and the rest, or is not indexed by the record's id at all, and then
the base and the ids in its row are wrong. Each `†` row says which.

## Table inventory

| table | holds | runs |
|---|---|---|
| OBINIT2 | map object kinds: four more arrays of OBINIT's grid | boot |
| MUINIT | the music room's tracks | boot |
| CBINIT | the two leads' growth parameters | boot |
| EBINIT | units: party characters and enemies, their stats, equipment, actions and drops | boot |
| CNINIT | speakers: the names the name plate shows, and each speaker's voice group | boot |
| CHINIT | characters | boot |
| ITINIT | items, magic and arts | boot |
| ILINIT | conditions | boot |
| SSINIT | skills | boot |
| RTINIT | battle routines, step by step | boot |
| AFINIT | elements, affinities, classes and formations | boot |
| TIINIT | titles | boot |
| SKINIT | the schools' skill trees | boot |
| AMINIT2 | world-map areas | boot |
| ALINIT | alchemy recipes | boot |
| ARINIT | arena matches | boot |
| IMINIT | equipment growth | boot |
| MPINIT | field maps | boot |
| EEINIT | encounter chances | boot |
| TRINIT | treasure contents | boot |
| LAINIT | land types | boot |
| OBINIT | map object kinds | boot |
| SOINIT | the war game's terrain | boot |
| TPINIT | the war game's troops | boot |
| CAINIT | the war game's commanders | boot |
| RUINIT | the war game's tactics | boot |
| SRINIT | the war game's map-unit routines | boot |
| GRINIT | the war game's ranks | boot |
| CGINIT | the CG gallery | boot |
| SPINIT | scene replay | boot |
| CIINIT | character profiles | boot |
| VIINIT | the glossary | boot |
| CVINIT | voice settings | boot |
| FAINIT | objectives | boot |
| BTANINIT2 | battle animations | boot |
| EX1INIT | the fishing game | boot |
| SCINIT | scenes: the script each scene id plays | boot |
| AMINIT | the world map's open areas | on demand: `ALLMAP` |
| SIINIT | town sites | on demand: `TOWN` |
| SHINIT | the shop in play | on demand: `SHOP` |
| QUINIT | quests | on demand: `INN` |
| RMINIT | map blocks: each block's scripts, pictures and music | on demand: `FIELD` |
| ENINIT | encounter sets | on demand: `FIELD` and map scripts |
| ESINIT | formations | on demand: `SETENP` (`BTL`) |
| CSINIT | scripted battle parties | on demand: `BTL` |
| PZINIT | box puzzles | on demand: `SETMAPDATA` |
| STINIT | war stages | on demand: `SLG` |
| BTANINIT | the battle animations' effects | on demand: `SETEFFECT` (`ANIME`) |

The layouts below give, for each array: its base, the cells it has per id, which columns are written, how many ids
have a value, and the values (for a string array, the first three).

## Boot

### The boot order (SYSTEM4)

`SYSTEM4` boots the game. If the shared-profile cell `G[0x3]` is unset it runs `INITCONFIG` and sets `G[0x3]`
(`SYSTEM4@0x1ac`); otherwise `LOADCONFIG` reads the options back. Then it runs `INIT2` (`SYSTEM4@0x2d1`), which calls the 37 boot
tables, fills slot 0 of the hook chains and runs each mounted append's `$n$AUTORUN` (op `0x143`, `INIT2@0x17c`).
`SYSTEM4@0x2d4` restores a save's stack when one is being loaded; otherwise `LOGO` (on the first run of the root),
then `INIT` (`SYSTEM4@0x312`), then `TITLE`. A load runs `SYSTEM4` again up to the restore op, so `INIT2` runs on
every boot and on every load; its conditional parts (below) test saved cells that are 0 at a cold boot, so they act
only on a load (inferred, from the cells they test).

After the title, `SYSTEM4` runs one mode a pass, from `G[0x0]`: 1 a scene, 2 `ALLMAP` (the world map), 3 `FIELD`
(a dungeon map), 4 `BTL` (a battle), 5 `TOWN`, 6 `ARENA`, 7 `INN`, 8 `SLG`, and 9 the script
`G[0xc77b2e + G[0xc77b2d]]` (`SYSTEM4@0x719`). A scene pass is described under "Scene dispatch".

### INITCONFIG (the first run's options)

`INITCONFIG` runs once per shared profile and stores every cell it writes in it: `G[0x90c] = 1`,
`G[0x3636] = 1`, `G[0x34a5] = 1`, `G[0x3637]` and `G[0x3638]` cleared, `G[0x3639] = 0`, `G[0x3634] = 60`,
`G[0x3633] = 1`, `G[0x30a0]` to `G[0x30a0 + 999]` cleared, and `G[0x266408] = 0`. `CONFIG` toggles each between two
values (`CONFIG@0xf1a` to `0x106a`); its labels are pictures, so the options' names are not read. What the readers
show:

| cell | default | read by | reading |
|---|---|---|---|
| `G[0x30a0 + group]` | 0 | the scene scripts | a voice group's mute: a scene reads `G[0x30a0 + G[0x268f74 + G[0x268b8c + G[0x3489]]]]` (the speaker's voice group) and runs `mute-next-voice` when it is set (`SC0000@0xbdf`) |
| `G[0x3634]` | 60 | `FIELD`, `DRAWWALK`, `DRAWWALK2`, `CARRY`, `LOOK`, `MOVESKIN` | 60 or 30: the map's frame rate. `FIELD@0x29b` waits `1000 / G[0x3634]` ms a frame, and the walkers step `60 / G[0x3634]` (`DRAWWALK@0x182`) |
| `G[0x3637]` | 0 | `ANIME@0x0`, `@0x54c`, `@0x98d` | `ANIME` branches on it first (not decoded) |
| `G[0x90c]` | 1 | `SAVE@0xaad`, `@0xff6` | not decoded |
| `G[0x3636]` | 1 | `MENU@0x5d0` | not decoded |
| `G[0x34a5]` | 1 | `BUNKI`, `BUNKIMOVE`, `SBUNKI`, `SBUNKIMOVE` | not decoded |
| `G[0x3638]` | 0 | no reader found | |
| `G[0x3639]` | 0 | `BTL@0x2f6d` | not decoded |
| `G[0x3633]` | 1 | the append scenes (`$1$SC0630@0x27b` and others) | not decoded |
| `G[0x266408]` | 0 | `FIELD@0xcee` | not decoded |

### INIT2 (the boot driver)

Besides calling the tables, `INIT2` writes these:

- **The hook chains**, slot 0 of each (`INIT2@0x6f` to `0x10f`): see "Hook chains".
- `G[0xc77b2f] = EXTRA1.BIN`: slot 1 of the scripts `SYSTEM4`'s mode 9 runs, `G[0xc77b2e + G[0xc77b2d]]`. The
  `MPEV` scripts set `G[0xc77b2d] = 1` before they hand mode 9 to `SYSTEM4`.
- `G[0x268b40]` to `G[0x268b49]` = 10000, 10010, 10110, 10120, 10130, 10140, 10150, 10160, 10210, 10220: graphic
  element numbers that scenes move (`$1$SC0630@0x936`).
- `G[0x5dd8f9] = 35` and `G[0x5dd8fa] = 80`: what `DRAWCH@0x82`, `DRAWWALK` and `CARRY` take from 0x200 and 0x120,
  a map character's anchor. `G[0x5dd8fb] = 20` has no reader found.
- Two inline arrays (`copy-inline-int-array`): `G[0xc0ba09]` to `G[0xc0ba14]` = 55, 60, 57, 59, 62, 20, 14, 65, 67,
  68, 69, 70, which `BTL@0x3914`, `CHECKACT@0x8fd` and `CHMODE3@0x180e` index with a skill id less `0xca9` and
  compare `G[0x1218f6 + value]` with `G[0xc1ca18]` (not decoded further); and `G[0xc4b0e3]` to `G[0xc4b0ea]` =
  −1, 0, 1, 0 and 0, −1, 0, 1, the four directions' x and y steps, read by `ATSEEK`, `MVSEEK` and `RTN_M031`.
- `G[0x266384] = 1` where it is 0 (`INIT2@0x169`; `INIT` sets it too): a saved index into `G[0x11967e]`, which
  `ALCHEMY`, `SHOP`, `CHMODE1` and `CHMODE4` read and `PUTHAI` sets.
- For each character 1 to 99 with `G[0x118fd8 + i] & 0x101 == 1`, `CALCREVISE` (`INIT2@0x19a`).
- On a load, three re-entries. Where `G[0x36e5]` is set (the party is on a dungeon map: `FIELD@0xc5` sets it,
  `ALLMAP` and `TOWN` clear it), it clears `G[0x5db5d1]` (750 cells) and walks the `RMINIT` chain
  (`INIT2@0x1e5`). Where `G[0x36e6]` is set (a scene is playing over the map, see "Scene dispatch"), it rebuilds
  the map's picture: the camera `G[0x5dda64]`, `G[0x5dda65]` (label_270), `G[0x266407] = 1`, `SETMAPDATA`,
  `SETMAPTEX`, `DRAWCH`. Where `G[0x118ece]` is set (the party is in an inn: `INN@0xe` sets it, `INN@0x1630`
  clears it), it walks the `QUINIT` chain (`INIT2@0x24e`).
- `G[0x268b1a] = 1` last (`INIT2@0x269`), above the saved prefix: `FIELD@0x655` and the `MP` scripts test it.

### INIT (a new game's parties)

`INIT` runs on every boot before the title (`SYSTEM4@0x312`). For party 0 with area 1 and party 1 with area 2 it
sets `G[0x118ef7 + party] = 1` and `G[0x118efa + party] = area` (where `G[0x363d]` is 0; otherwise for each party
`G[0x118ef4 + i]` marks active), then `G[0x266384] = 1` and `G[0x36a7] = 4`. Every one of these is a saved cell,
so they are a new game's values and a load overwrites them. `G[0x118efa + party]` is the party's area (an
`AMINIT2` id: `FIELD@0x1f7` sets `G[0x118ef9] = G[0x42a588 + G[0x363f]]` and `G[0x118efc] = G[0x363f]`);
`G[0x118ef7 + party]` is read back by `ALLMAP@0x22b` into `G[0x36a4]`; `G[0x363c]` is the party in play (0 to 2);
`G[0x36a7]` is an index 0 to 6 of `ALLMAP` (`ALLMAP@0xcbc`, not decoded).

### Hook chains

`INIT2` and the appends fill 36 arrays of 32 slots, `G[0xc78130]` to `G[0xc78590]`, `0x20` apart. Slot 0 is
the base game's entry (`INIT2@0x6f` to `0x10f`) and slot n append n's (`$n$AUTORUN@0x63` onwards). A walker
goes through slots 0 to 31 and runs (or, for a picture chain, loads) every nonzero one, so the order is
**base, `$1$`, `$2$`, `$3$`**. Most walkers use `G[0x268b36]` as the slot index. The `*MES`, `INFO2`, `INFO3` and
`SETMAPDATA` walkers clear `G[0xc7812f]` first (`DRAWSSMES@0x36`, `INFO2@0x10b6`, `INFO3@0x1080`,
`SETMAPDATA@0xe3`) and stop once a script sets it to 1, which a script does when it handled the id
(`VIMES@0x1726`); `SCJUMP`'s walkers stop on `G[0x309d]` instead. An append's stub that adds nothing reads
`jmp; mov G[0xc7812f] 1; exit` with the `mov` skipped (`$1$SSMES`, `$1$VIMES`).

| chain | slot 0 | appends | walked by, and when |
|---|---|---|---|
| `G[0xc78130]` | none | `$n$SO021G.AGF`, 1 to 3 | `CGMODE@0x1198`: append n's CG thumbnail sheet into texture `0x5e + n` |
| `G[0xc78150]` | none | `$n$SO022F.AGF`, 1 to 3 | `HMODE@0xd29`: append n's replay thumbnail sheet into texture `0x5e + n` |
| `G[0xc78170]`, `G[0xc78190]`, `G[0xc781b0]` | `SO034`, `SO015`, `SO038` | `$n$` of each, 1 to 3 | `READICON`, as rows 0 to 2 of one `[3][32]` array; the caller sets the row in `G[0x268b36]` (row 0 `ALCHEMY`, `CHMODE`, `SHOP`, `TOWN`; row 1 `DIVIDECH`, `SELCH`; row 2 `RESULT`). It composes each layer's icon sheet into texture `G[0x268b3c]`, copying the icon of each character 1 to 99 whose `G[0xc1a865 + id]` is above 0 |
| `G[0xc781d0]` | `SO039` | 1 to 3 (`$2$` is `0x20000d6`) | `CHMODE3@0x1f62` |
| `G[0xc781f0]` | `SO036` | `$1$` only | `BTL@0x9148`: one sheet per layer into texture `0xac + layer`; `BTL@0x91e4` picks a cell by `G[0x9f31b9 + i]` |
| `G[0xc78210]` | `SSMES` | 1 to 3 | `DRAWSSMES` |
| `G[0xc78230]` | `ITMES` | 1 to 3 | `DRAWITMES` |
| `G[0xc78250]` | `TIMES` | 1 to 3 | `DRAWTIMES` |
| `G[0xc78270]` | `VIMES` | 1 to 3 | `INFO2@0x10e6` (the glossary's text) |
| `G[0xc78290]` | `CIMES` | 1 to 3 | `INFO3@0x10b0` |
| `G[0xc782b0]` | `AMINIT` | 1 to 3 | `ALLMAP@0x254` on each map pass, and `ALLMAP@0x30` when entered in mode 1 (a scene's `SHOWALLMAP`) |
| `G[0xc782d0]` | `SIINIT` | 1 to 3 | `TOWN@0x1af`, after the `START` chain and context 2 |
| `G[0xc782f0]` | `SHINIT` | 1 to 3 | `SHOP@0x1f51` |
| `G[0xc78310]` | `QUINIT` | 1 to 3 | `INN@0x8a` on entry; `INIT2@0x24e` on a load in an inn |
| `G[0xc78330]` | `RMINIT` | 1 to 3 | `FIELD@0x92` on entry; `INIT2@0x1e5` on a load on a map |
| `G[0xc78350]` | `ENINIT` | 1 to 3 | `FIELD@0x63a`; `MP01B0101@0x50b`, `MP13B0101@0x2ab`, `MP13B0404@0x355`, `MPEV01B0203@0x159f` |
| `G[0xc78370]` | `ESINIT` | 1 to 3 | `SETENP@0x414`, which `BTL` calls |
| `G[0xc78390]` | `CSINIT` | 1 to 3 | `BTL@0xcf` |
| `G[0xc783b0]` | `PZINIT` | 1 to 3 | `SETMAPDATA@0x113` (a stop-flag walk), which `FIELD` and `INIT2` call |
| `G[0xc783d0]` | `STINIT` | 1 to 3 | `SLG@0x2df` |
| `G[0xc783f0]` | none | none | no walker |
| `G[0xc78410]` | `BTANINIT` | 1 to 3 | `SETEFFECT@0xf0`, which `ANIME` calls |
| `G[0xc78430]` | `SCJUMP` | 1 to 3 | see "Scene dispatch" |
| `G[0xc78450]`, `G[0xc78470]`, `G[0xc78490]` | `ROUND`, `BEFOREFIGHT`, `AFTERFIGHT` | none | `SLG@0x1667`, rows 0 to 2 by its local `0x4dd` |
| `G[0xc784b0]`, `G[0xc784d0]` | `CLOSE`, `START` | 1 to 3 | `CLOSE` (row 0): `ALLMAP@0x112`. `START` (row 1): `FIELD@0x225`, `TOWN@0x105` |
| `G[0xc784f0]` to `G[0xc78550]` | `CHECKSS1` to `CHECKSS4` | `$3$CHECKSS2` (`0x300016c`) | `CHECKSS@0x249` (rows 0 to 2), `CHECKSS@0x548` (row 3); `DEBUGADV` and scenes call `CHECKSS` |
| `G[0xc78570]`, `G[0xc78590]` | `SETSSBONUS`, `SETSSDAMAGE` | `$3$SETSSBONUS` (`0x300016d`) | `CALCSS@0x4c` (row 0), `COMMITSS@0x177` (row 1); `CALCDAMAGE` calls both |

How much each append adds, in instructions (base, `$1$`, `$2$`, `$3$`): `SSMES` 3447, 3, 3, 34; `ITMES` 8940, 81,
130, 755; `TIMES` 1316, 132, 102, 224; `VIMES` 1284, 3, 3, 3; `CIMES` 879, 58, 3, 36; `AMINIT` 1356, 61, 28, 28;
`SIINIT` 1641, 33, 8, 1; `SHINIT` 2475, 1, 1, 1; `QUINIT` 1060, 16, 1, 1; `RMINIT` 8947, 101, 1, 808; `ENINIT`
7228, 207, 2, 644; `ESINIT` 9575, 148, 2, 751; `CSINIT` 198, 2, 2, 7; `PZINIT` 266, 3, 3, 59; `STINIT` 484, 1, 1, 1;
`BTANINIT` 8358, 1, 134, 191; `SCJUMP` 12499, 245, 93, 28; `CLOSE` 33, 1, 4, 1; `START` 185, 1, 1, 1; `CHECKSS2`
290 and `$3$` 22; `SETSSBONUS` 66 and `$3$` 13. A count of 1 is a bare `exit`.

Each `$n$AUTORUN` also sets bit n of `G[0xc7812e]`, the mounted appends: `ALLMAP@0x5c8`, `CGMODE@0x1e`,
`HMODE@0x19`, `MP07B0101@0x2eb`, `SAVE@0xfc4` and `@0x175f`, `SC0040` and `SC4300` read it, and `GAMECLEAR@0x502`
stores it. `$n$AUTORUN` also sets story flag 8114 + n to 1 (`G[0x28dc]`, `G[0x28dd]`, `G[0x28de]`; saved, and two `SCJUMP`
rules test them).
`$2$AUTORUN` sets `G[0x28e5] = 1` when catalog resource `0x53` is unlocked, which `$2$CLOSE@0x0` reads, and
`$3$AUTORUN` loads `G[0x2619]` and `G[0x261a]` from the shared profile.

## Scenes

### SCINIT

Leader `G[0x4256a0]`, capacity 10000; 1334 scene ids are set, 0 to 6665: 1251 by the base script, 20 by `$1$`,
3 by `$2$`, 60 by `$3$`. 177 scripts hold them; scene 0 is the opening, `SC0000`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x4256a0]` | 1 |  | 1334 | 94..50332242, 177 distinct | the script that plays scene `id`: a packed script id (an append's scenes carry its selector in the top byte). `SYSTEM4@0x366` copies it into `G[0x90f]` and runs it; `MENU@0x88a`, `ROUND@0x290` and `AFTERFIGHT@0x206` run it directly |
| `G[0x427db0]` | 1 |  | 723 | 1..9, 8 distinct | the `SCJUMP` context that offers the scene (579 of the 606 tagged scenes it names agree; 25 differ, mostly a 3 named in context 1 or 2). Its one reader is each scene script's header (`SC0000@0x1fe`, and the `SP`, `HOSOKU` and `DEBUGADV` scripts), which registers the right-click menu (`label_48f`) unless the value is 4 or `G[0x262e7c]` is set. No scene has 4 |

### Scene dispatch (SCINIT and SCJUMP)

A scene is a number, and `G[0x269b30]` holds the one to play. In a scene pass `SYSTEM4` copies its script into
the saved cell `G[0x90f]` (`SYSTEM4@0x366`, unless `G[0xc0ba04]` is set; the default is `0x5e`), sets
`G[0x25febb + scene] = 1` and stores it in the shared profile (`SYSTEM4@0x3b0`, for ids under 10000), and runs the
script (`SYSTEM4@0x516`). One script holds several scenes and branches on the number.

Two cells record that a scene ran. **`G[0x929 + scene]`** is set by the scene's own code at its start: each of the
1334 scenes' scripts sets it (`SC0000@0x8f9` for scene 0, `SC0000@0x1074` for scene 1, `SC0010@0x8f9` for scene
10). It lies in a 10000-cell bank of flags, `G[0x929]` to `G[0x3038]`, that `SCJUMP`, `FAINIT`'s screen and
`QUINIT`'s board all test, and it is in a save: the scene has been played in this game. Flags above the scenes'
ids are set by scene code too (`SC1170` sets flag 8114) and by the appends (8115 to 8117). **`G[0x25febb + scene]`**
is set by `SYSTEM4` and kept in the shared profile: the scene has been seen by this player, which `INFO2` reads to
unlock glossary entries.

`SCJUMP` picks the next scene. A caller sets the context `G[0x348b]`, sets `G[0x309d] = 1` and walks the chain
`G[0xc78430]` while `G[0x309d]` is 1, with `G[0x268b36]` as the slot: **base, `$1$`, `$2$`, `$3$`** (`$3$SCJUMP`
is an empty stub). `SCJUMP` branches on the context (`SCJUMP@0x0`) to a list of rules. Most rules take the turn:

```
if <condition> { G[0x0] = 1; G[0x309d] = 0; G[0x269b30] = <scene>; G[0x3626] = 0; jmp end }
```

The first rule that holds ends the script and, by clearing `G[0x309d]`, the walk. The caller sees `G[0x0] == 1`
and hands the pass back to `SYSTEM4`.

| script | rules | scenes they name |
|---|---|---|
| `SCJUMP` | 686 | 590, from 0 to 6663 |
| `$1$SCJUMP` | 14 | 13, from 630 to 5905 |
| `$2$SCJUMP` | 3 | 3, from 5930 to 5932 |

Three contexts use other forms. **Context 3** (a town's location) gives the location a scene instead of taking
the turn: `G[0x309d] = 0; G[0x42aa58 + loc] = scene; G[0x42aa76 + 5 × loc + 0..4] = five values; bit 1 of
G[0x36b1 + loc]`. `TOWN` clears both arrays first (`TOWN@0x1ca`, 30 and 150 cells), plays a location's scene when
the player enters it (`TOWN@0x6d4`) and draws up to five icons beside it from the five values (`TOWN@0x8ab`; they
look like characters, not confirmed). **Context 8** (an inn) gathers quest scenes:
`G[0x5d852d] = quest; CHECKCONTRACT; if G[0x5d852e] { G[0x0] = 1; G[0x355e + 2 × G[0x3626]] = scene;
G[0x355f + 2 × G[0x3626]] = quest; G[0x3626] += 1 }`, so every quest the party holds whose condition holds queues
its scene. `SYSTEM4@0x31f` plays the queue in turn (count `G[0x3626]`, index `G[0x3627]`) and, after each scene with
a quest, sets `G[0x5d852d]` and runs `QUESTCLEAR` (`SYSTEM4@0x631`). **Context 9** (back on the map) plays the
scene over the dungeon map: `G[0x36e6] = 1; G[0x5dda5f] = 1; G[0x0] = 1; G[0x309d] = 0; scene`, and on the first
such scene saves the map position (`G[0x36da] = G[0x36d9]`, `G[0x36dc] = G[0x36db]`, `G[0x36de] = G[0x36dd]`,
`G[0x5dda60] = 1`). After the scene `SYSTEM4` clears `G[0x36e6]`, sets `G[0x5dda5f] = 1` and runs `FADEMAPFRAME`.
33 of context 9's rules draw `rand(100) < N`.

| context | set by | when | rules (`SCJUMP`, `$1$`, `$2$`) | scenes |
|---|---|---|---|---|
| 1 | `ALLMAP@0x181` | each pass of the world map | 93, 4, 0 | 85, 0 to 5507 |
| 2 | `FIELD@0x256`, `TOWN@0x128` | a dungeon map or a town is entered, after the `START` chain (`FIELD` only where `G[0xc0ba06]` is 0) | 83, 3, 2 | 81, 106 to 6663 |
| 3 | `TOWN@0x20b` | for each location 1 to 29 (`G[0x36b0]`) whose `G[0x36b1 + loc]` has bit 0 | 270, 4, 1 | 192, 181 to 6641 |
| 4 | none | never; the context has no rules | | |
| 5 | `BTL@0xa2b1` | a battle is over, named no scene of its own (`G[0x276e4a + outcome]`, `G[0x5d820c + 3 × G[0x36cf] + 1 or 2]`) and `G[0x276e49] <= 0` | 2, 0, 0 | 1800, 4301 |
| 6 | `MENU@0x648` | the menu's command (the menu saves and restores the context, and plays the scene itself, `MENU@0x88a`) | 6, 0, 0 | 3, 6311 to 6396 |
| 7 | `INN@0x76b` | the player takes a quest: `INN@0x71d` marks it, then `G[0x118ecf]` holds the quest's row for the walk | 91, 1, 0 | 90, 814 to 6415 |
| 8 | `INN@0xa5` | an inn is entered, after the `QUINIT` chain; the quest form above | 91, 1, 0 | 89, 816 to 6515 |
| 9 | `FIELD@0x55f` | back on the map after a random battle (`G[0x276e49] < 0` and `G[0x276e99] == 0`; `FIELD@0x2bbd` sets −1 for a random encounter) | 50, 1, 0 | 50, 1463 to 2928 |

The other walks of the chain: `FIELD@0x28b`, `TOWN@0x15d` and `@0x240`, `INN@0xda` and `@0x7a0`, `MENU@0x67d`,
`BTL@0xa2e6`, `ALLMAP@0x1b6`.

What a rule tests. The flags `G[0x929 + n]`, 0 to 9999, first of all: a rule that names a scene mostly asks that
the scene has not been played (`G[0x929 + scene] != 1` in 92 of context 1's 93 rules, 67 of 83 in context 2, 181 of
270 in context 3, both of context 5's, 17 of 50 in context 9, none of contexts 6 to 8) and that another has. Then
the story's place: the chapter `G[0x363a]` (0 to 10: `SC0000@0x902` sets 0, 序章; `SC0020` 2; `SC0090` 8, 第八章;
`SC0100` 9, 終章; `SC0140`, `SC0160` and `SC0180` 10; `GAMESTART@0x1a5a` 0), the area `G[0x363f]` (an `AMINIT2` id:
`ALLMAP@0x57` reads `G[0x42a650 + G[0x363f]]`; 334 tests), the town's location `G[0x36b0]`, the party in play
`G[0x363c]` and `G[0x363d]`. Context 2 also tests a character's bits, `(G[0x118fd8 + ch] & 0x10007) == 0x10003`,
and the character's level `G[0x1196e2 + ch] >= 350` or `375` ("Units, characters and titles"). Contexts 7 and 8 test the inn
`G[0x118ecf]`, the quest `G[0x5d852c]` and the quest states `G[0x1167be + 100 × row + quest]`.

Not decoded: what `G[0x363d]` is (it switches the inn and `INIT` between one party and every active party), the 25
tags of `G[0x427db0]` that differ from the context naming the scene, and the five values of a location's scene.
`tools/scjump_decode.py` was written for Himegari and was not run.

## Units, characters and titles

A **unit** (`EBINIT`) is anything that fights: every enemy, and the body each party character fights with. A
**character** (`CHINIT`) is a member of the party; it names its unit, and the save keeps its level, experience,
stats, equipment, skills and title by character id (`G[0x118f0c]` in the menus, `G[0x5dda74]` in the shared
routines). A **title** (`TIINIT`) is a character's current class, `G[0x1218f6 + character]` in a save: id
20 × character + k, k from 0 to 19, title 20 × character the one a character joins with. `CBINIT` holds the growth rules of
the two leads, whose stats come from seven parameters instead of a unit's growth arrays. `CNINIT` and `CVINIT` name
the speakers of the scenes and their voice settings, and `CIINIT` holds the profiles of the 人物 book.

A battle builds a **slot** (`G[0xc1a8c9]`: 0 to 5 the party, 10 to 29 the enemies) from these tables. `SETCH` builds a
character's slot from the saved state and the title, `SETEN` an enemy's from the unit alone. The slot's cells are
`G[0xc1a8ca + slot]` (its unit), `G[0xc1a924]` (level), `G[0xc1ca12]` (title), `G[0xc1a9ba]`, `G[0xc1a9f6]` and
`G[0xc1aa32]` (max HP, MP, TP), and the stat cells named in the `EBINIT` layout below.

### EBINIT (units)

Leader `GS[0x12a4]` (the full name), capacity 1000. 578 units: 494 in the base data, 11, 3 and 70 from appends 1, 2
and 3. The integer arrays run from `G[0x370817]` to `G[0x4252b7]`, every base ≡ `0x377d47` modulo 1000. `SETEN`
builds an enemy slot from the unit in `G[0x370816]`, the cell before the table, which `BTL@0x039a6` and
`SETENP@0x0055a` set. It marks the unit seen in the bestiary (`G[0x2629b3 + unit]`, stored to the shared profile,
`SETEN@0x00000`). Then it fills the slot. The level lies between the base level `G[0x4131a7]` and the max level
`G[0x41358f]` (500 when `G[0x266409]` is set, `SETEN@0x0007a`), and it rises toward the party's average level
`G[0x2756d3]` by the divisor `G[0x413977]` (`SETEN@0x000ce`). Each of the 14 stats is (base + growth × (level − base
level)) / 100 (`SETEN@0x001f2` to `SETEN@0x00422`). It also copies the equipment, the defense element and its level, 命中, 回避, FS and the
skill levels. The bestiary (`INFO4`) draws a unit's picture, its two lines and its numbers.

A party character uses its unit for what does not change with play. `SETCH` reads 命中, 回避, max FS and the defense
element straight from this table, so a changed value reaches an old save. Other fields reach the save once, when the
character joins (`UNITECH`): the equipment, the money, the drop items (given as items) and the base level. The stats
reach it each time `CALCLVUP` runs, on joining and at every level up (`ADDEXP`). It writes base + growth × (level − base level)
into the character's saved stats `G[0x119872]` … `G[0x119e4e]`, so a changed base or growth reaches an old save at the
character's next level. The two leads (`G[0x412dbf]` set) take their stats from `CBINIT` instead.

Corrections to the layout: the three off-grid arrays the tool shows at stride 300 (`G[0x388604]`, `G[0x389629]`,
`G[0x3b7618]`) and its four cells outside the arrays (`G[0x38454d]`, `G[0x384b68]`, `G[0x385cbd]`, `G[0x385e2c]`) are one
array, `G[0x3805ff]`, 600 cells a unit: `SETEN@0x00cf9` and `INFO4@0x01136` index it
`lookup-array-2d … (global-int 0x3805ff) unit 0x258 skill`. The tool's `G[0x412db5]` stride 11 † is the one-cell array
`G[0x412dbf]` (`UNITECH@0x001b0`), which only units 1 and 31 write. `G[0x37b00f]` (`SETEN@0x00808`,
`UNITECH@0x00085`) is the initial shield, which no record writes. The 19,000 cells from `G[0x37bfaf]` to `G[0x3805fe]`
are neither written nor read.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x370817]` | 30 | 18..27 | 29 | 9364..50333348, 44 distinct | 30 voice clips. `BTL@0x08b2b` plays column 27 when the battle starts and column 18 when the unit falls; a slot with a title plays the title's clip instead (`TIINIT` `G[0xbfcb0d]`, `BTL@0x08b18`, `ANIME@0x01fe3`). Only columns 18 and 27 are written |
| `G[0x377d47]` | 1 |  | 404 | 13029..50332114, 346 distinct | the battle sprite (a `CPB…` picture): `BTL@0x09102` loads it into surface `0x34 + slot`; the bestiary draws it (`INFO4@0x00cef`) |
| `G[0x37812f]` | 1 |  | 18 | 13994..14225, 11 distinct | the bestiary's picture where it is not the sprite (an `EI…` picture, `INFO4@0x00ca6`) |
| `G[0x378517]` | 1 |  | 381 | 10..5390, 380 distinct | the bestiary's order: `INFO4@0x00784` lists only units with a value above 0, sorted by it |
| `G[0x3788ff]` | 1 |  | 300 | 1..2, 2 distinct 1×136, 2×164 | sex, 1 male or 2 female: `ALCHEMY@0x03d63`, `CHMODE1@0x01ae0` and `SHOP@0x04154` refuse an item for women (`ITINIT` `G[0x84ada9]` = 1) unless this is 2; `BTL@0x02925` tests it for the 性魔術 capture |
| `G[0x378ce7]` | 1 |  | 442 | 3..3, 1 distinct 3×442 | FS temperament, 1 to 5 (every record writes 3): `BTL@0x0438f` moves the slot's FS `G[0xc1a960]` by it, and `CALCDAMAGE@0x00a29` sets `G[0x276e95]` to −2, −1 or +1 from it |
| `G[0x3790cf]` | 1 |  | 454 | 1..2, 2 distinct 1×400, 2×54 | not read: no script indexes it. 2 for the party's units |
| `G[0x3794b7]` | 1 |  | 31 | 2..4, 3 distinct 2×8, 3×13, 4×10 | `BTL@0x09719` draws an extra overlay (`0x2717`) for slot 6 when it is 3. Meaning not read |
| `G[0x37989f]` | 1 |  | 206 | 1..6, 6 distinct 1×81, 2×93, 3×17, 4×11, 5×1, 6×3 | boss class: `BTL@0x001e1` picks the battle music from the highest class among the enemies (6: BGM `0xf`, 5: `0xb`, else `0x18` + class); defeating one sets `G[0xc25965]` bit 2 (`BTL@0x05e89`). `CALCILL@0x005c1` makes it immune to the conditions with `G[0x9f313f]` = 1; `BTLMENU@0x001ea`, `CHECKSS2@0x00007` and `CHMODE3@0x0186f` test it |
| `G[0x379c87]` | 1 |  | 454 | 1..5, 5 distinct 1×204, 2×71, 3×126, 4×1, 5×52 | width on the battle grid, in cells (`BTL@0x027bc`, `SELTARGET@0x00ae1`, `ANIME@0x0046f`) |
| `G[0x37a06f]` | 1 |  | 454 | 1..5, 5 distinct 1×193, 2×85, 3×126, 4×1, 5×49 | height on the battle grid, in cells (`BTL@0x058c8`, `SELTARGET@0x00af4`, `ANIME@0x004a7`) |
| `G[0x37a457]` | 1 |  | 444 | 2..15, 14 distinct | defense element, named by `GS[0xf660 + value]` (`AFINIT`: 2 物理 … 9 暗黒, 10 神格, 11 魔神, 12 反万, 13 霊体, 14 不死, 15 創造). `SETEN@0x00c02` and `SETCH@0x00a89` copy it into the slot's `G[0xc1ac8a]` when the armor gives none; `INFO4@0x01084` draws it |
| `G[0x37a83f]` | 1 |  | 305 | 1..5, 5 distinct 1×108, 2×77, 3×69, 4×30, 5×21 | the defense element's level, into `G[0xc1aca8]` (`SETEN@0x00c15`, `SETCH@0x00a9c`, `INFO4@0x01092`) |
| `G[0x37ac27]` | 1 |  | 33 | 1001..1751, 30 distinct | initial weapon, an `ITINIT` id: `SETEN@0x007f0` into the slot's `G[0xc1ab9a]`; `UNITECH@0x00072` into the saved 主武器 `G[0x11cd92 + character]` |
| `G[0x37b3f7]` | 1 |  | 31 | 2001..2403, 22 distinct | initial armor: `SETEN@0x007fc` into `G[0xc1abd6]`; `UNITECH@0x00098` into the saved 防具 `G[0x11cf22]` |
| `G[0x37b7df]` | 1 |  | 6 | 2917..2957, 3 distinct 2917×1, 2945×1, 2957×4 | initial accessory: `SETEN@0x00814` into `G[0xc1abf4]`; `UNITECH@0x000ab` into the saved 装飾 `G[0x11cf86]` |
| `G[0x37bbc7]` | 1 |  | 33 | 256..1508672, 28 distinct | the item kinds the unit may equip, bit k for `ITINIT` kind k (セリカ `0x120048`: kinds 3, 6, 17, 20). `DRAWCHP@0x00eae` and `CHANGETITLE@0x00133` OR in the title's kinds (`G[0xaa473d]`) and kind 22 (accessories); `SHOP@0x040fa`, `ALCHEMY@0x03d09`, `CHMODE1@0x01809` test it |
| `G[0x3805ff]` | 600 | 1..373 | 341 | 1..5, 5 distinct 1×504, 2×129, 3×154, 4×42, 5×101 | skill levels, column = an `SSINIT` skill id: `SETEN@0x00cf9` adds the title's (`G[0xaadbad]`) and clamps the sum into the slot's `G[0xc1ca4e]` (0 to 5); `INFO4@0x01136` lists up to 16 by `GS[0x811c + 5 × skill + level − 1]` (skill 401 is 捕縛); `SETSSLIST@0x000a0` |
| `G[0x412dbf]` | 1 |  | 2 | 1..31, 2 distinct 1×1, 31×1 | the unit's `CBINIT` row (unit 1 セリカ, unit 31 リウイ). Where set, `UNITECH@0x001b0` copies the row into the saved parameters and runs `CALCCHP`; otherwise it runs `CALCLVUP`. `ADDEXP@0x00065`, `AUTOGROW`, `BUILDUP`, `CALCABIMAX`, `CHMODE1`, `CHMODE2`, `SETCHSEL`, `USESKILLSUB` read it |
| `G[0x4131a7]` | 1 |  | 444 | 8..500, 86 distinct | base level, the level the stats are given at: `SETEN@0x00184` floors the slot's level at it; `UNITECH@0x0020d` sets the saved level `G[0x1196e2]` to it on joining; `CALCLVUP@0x00013`; the bestiary draws it (`INFO4@0x00f69`) |
| `G[0x41358f]` | 1 |  | 444 | 8..500, 54 distinct | max level of an enemy slot (`SETEN@0x0007a`) |
| `G[0x413977]` | 1 |  | 413 | 1..1, 1 distinct 1×413 | level catch-up divisor: `SETEN@0x000ce` adds (party average level − level) / this |
| `G[0x413d5f]` | 1 |  | 166 | -10..400, 19 distinct | 命中, into the slot's `G[0xc1acc6]` (`SETEN@0x00445`, `SETCH@0x00196`) |
| `G[0x414147]` | 1 |  | 159 | -20..50, 13 distinct | 回避, into `G[0xc1ace4]` (`SETEN@0x00458`, `SETCH@0x001a9`) |
| `G[0x41452f]` | 1 |  | 443 | 200..266000, 321 distinct | 攻撃力 at the base level, ×100: `SETEN@0x0027e` into `G[0xc1aa50]`; `CALCLVUP@0x000ce` into the saved `G[0x119a66]` |
| `G[0x414917]` | 1 |  | 443 | 600..142000, 289 distinct | 防御力, ×100: `SETEN@0x002a8` into `G[0xc1aa6e]`; `CALCLVUP@0x000ff` into `G[0x119aca]` |
| `G[0x414cff]` | 1 |  | 257 | 500..270000, 209 distinct | 魔法攻撃, ×100: `SETEN@0x00326` into `G[0xc1aa8c]`; `CALCLVUP@0x00130` into `G[0x119b2e]` |
| `G[0x4150e7]` | 1 |  | 444 | 1100..356000, 304 distinct | 魔法防御, ×100: `SETEN@0x00350` into `G[0xc1aaaa]`; `CALCLVUP@0x00161` into `G[0x119b92]` |
| `G[0x4154cf]` | 1 |  | 443 | 100..13200, 87 distinct | 攻撃回数, ×100: `SETEN@0x002d2` into `G[0xc1aac8]`; `CALCLVUP@0x00192` into `G[0x119bf6]` |
| `G[0x4158b7]` | 1 |  | 444 | 100..99900, 73 distinct | 防御回数, ×100: `SETEN@0x002fc` into `G[0xc1aae6]`; `CALCLVUP@0x001c3` into `G[0x119c5a]` |
| `G[0x415c9f]` | 1 |  | 444 | 100..99900, 139 distinct | 肉体抵抗, ×100: `SETEN@0x0037a` into `G[0xc1ab04]`; `CALCLVUP@0x001f4` into `G[0x119cbe]` |
| `G[0x416087]` | 1 |  | 444 | 100..99900, 139 distinct | 精神抵抗, ×100: `SETEN@0x003a4` into `G[0xc1ab22]`; `CALCLVUP@0x00225` into `G[0x119d22]` |
| `G[0x41646f]` | 1 |  | 444 | 100..8000, 69 distinct | 肉体戦速, ×100: `SETEN@0x003ce` into `G[0xc1ab40]`; `CALCLVUP@0x00256` into `G[0x119d86]` |
| `G[0x416857]` | 1 |  | 443 | 100..9200, 68 distinct | 精神戦速, ×100: `SETEN@0x003f8` into `G[0xc1ab5e]`; `CALCLVUP@0x00287` into `G[0x119dea]` |
| `G[0x416c3f]` | 1 |  | 444 | 100..10500, 76 distinct | 運, ×100: `SETEN@0x00422` into `G[0xc1ab7c]`; `CALCLVUP@0x002b8` into `G[0x119e4e]` |
| `G[0x417027]` | 1 |  | 418 | 150..258000, 235 distinct | HP growth a level, ×100 (`SETEN@0x001f2`, `CALCLVUP@0x00049`) |
| `G[0x41740f]` | 1 |  | 239 | 24..500, 89 distinct | MP growth a level (`SETEN@0x0021c`, `CALCLVUP@0x0007a`) |
| `G[0x4177f7]` | 1 |  | 366 | 2..500, 122 distinct | TP growth a level (`SETEN@0x00246`, `CALCLVUP@0x000ab`) |
| `G[0x417bdf]` | 1 |  | 419 | 1..413, 113 distinct | 攻撃力 growth a level (`SETEN@0x00270`, `CALCLVUP@0x000dc`) |
| `G[0x417fc7]` | 1 |  | 419 | 60..550, 97 distinct | 防御力 growth (`SETEN@0x0029a`, `CALCLVUP@0x0010d`) |
| `G[0x4183af]` | 1 |  | 234 | 15..303, 76 distinct | 魔法攻撃 growth (`SETEN@0x00318`, `CALCLVUP@0x0013e`) |
| `G[0x418797]` | 1 |  | 420 | 31..550, 125 distinct | 魔法防御 growth (`SETEN@0x00342`, `CALCLVUP@0x0016f`) |
| `G[0x418b7f]` | 1 |  | 418 | 1..30, 18 distinct | 攻撃回数 growth (`SETEN@0x002c4`, `CALCLVUP@0x001a0`) |
| `G[0x418f67]` | 1 |  | 419 | 1..200, 24 distinct | 防御回数 growth (`SETEN@0x002ee`, `CALCLVUP@0x001d1`) |
| `G[0x41934f]` | 1 |  | 401 | 10..54, 37 distinct | 肉体抵抗 growth (`SETEN@0x0036c`, `CALCLVUP@0x00202`) |
| `G[0x419737]` | 1 |  | 401 | 5..61, 34 distinct | 精神抵抗 growth (`SETEN@0x00396`, `CALCLVUP@0x00233`) |
| `G[0x419b1f]` | 1 |  | 420 | 3..24, 21 distinct | 肉体戦速 growth (`SETEN@0x003c0`, `CALCLVUP@0x00264`) |
| `G[0x419f07]` | 1 |  | 419 | 1..40, 26 distinct | 精神戦速 growth (`SETEN@0x003ea`, `CALCLVUP@0x00295`) |
| `G[0x41a2ef]` | 1 |  | 420 | 1..30, 29 distinct | 運 growth (`SETEN@0x00414`, `CALCLVUP@0x002c6`) |
| `G[0x41a6d7]` | 1 |  | 444 | 100..415000000, 390 distinct | max HP at the base level, ×100: `SETEN@0x00200` into `G[0xc1a9ba]`; `CALCLVUP@0x0003b` into the saved `G[0x119872]`; `INFO4@0x00d9f` |
| `G[0x41aabf]` | 1 |  | 261 | 1100..365000, 197 distinct | max MP, ×100: `SETEN@0x0022a` into `G[0xc1a9f6]`; `CALCLVUP@0x0006c` into `G[0x11993a]` |
| `G[0x41aea7]` | 1 |  | 387 | 300..365000, 249 distinct | max TP, ×100: `SETEN@0x00254` into `G[0xc1aa32]`; `CALCLVUP@0x0009d` into `G[0x119a02]` |
| `G[0x41b28f]` | 1 |  | 446 | 12..999, 20 distinct | starting FS of an enemy slot (`SETEN@0x01069`); the bestiary draws it (`INFO4@0x00e57`) |
| `G[0x41b677]` | 1 |  | 446 | 24..999, 19 distinct | max FS, into `G[0xc1a97e]` (`SETEN@0x001e6`, `SETCH@0x0005d`, `CALCREVISE@0x0081b`) |
| `G[0x41ba5f]` | 1 |  | 434 | 2..15, 14 distinct | the wait before the first action, into the slot's `G[0xc1c346]` and `G[0xc1c328]` (`BTL@0x03a33`, `SETENP@0x00254`, `REVIVE@0x00062`) |
| `G[0x41be47]` | 1 |  | 446 | 1..747, 361 distinct | the AI routine, an `RTINIT` id: `BTL@0x031e2` sets `G[0xa26f69]` to it and runs `RTN` |
| `G[0x41c22f]` | 6 | 0..5 | 412 | 1402..7514, 477 distinct | the actions the AI may use, `ITINIT` ids (unit 12 水の巫女: 速氷弾ヴァルレン, 覚醒 …): `RTN@0x0009d` copies them to `G[0x36fa00]`; 0 means the weapon |
| `G[0x41d99f]` | 1 |  | 29 | 1..1, 1 distinct 1×29 | the experience curve, 1 to 5 (200, 215, 225, 235 or 250 × level × 124 / 1000): `CALCNEXTEXP@0x0002b`. Party units only |
| `G[0x41dd87]` | 1 |  | 29 | 85..125, 6 distinct 85×1, 90×19, 95×2, 100×5, 110×1, 125×1 | a percentage on the experience a level needs (`CALCNEXTEXP@0x000d9`) |
| `G[0x41e16f]` | 2 | 0..1 | 394 | 10..240000, 190 distinct | experience the battle's pool `G[0x276e9a]` gets from the unit (`BTL@0x05fb4`, `BTL@0x0610f`): by our reading column 0 when a hit defeats it, column 1 when it survives (med) |
| `G[0x41e93f]` | 1 |  | 282 | 5..162000, 103 distinct | money: `RESULT@0x00542` adds it for each enemy defeated; `UNITECH@0x000b7` adds it to the party's saved money `G[0x118f0d + party]` when the unit's character joins |
| `G[0x41ed27]` | 5 | 0..4 | 360 | 1..2974, 256 distinct | five drop items, `ITINIT` ids (`RESULT@0x00577`, `BTL@0x0160d`); `UNITECH@0x00137` gives a joining character's to the party (`ADDITEM`, saved `G[0x10cb17]`) |
| `G[0x4200af]` | 5 | 0..4 | 360 | 2..100, 17 distinct | each drop's chance in percent (`RESULT@0x006bc`) |
| `G[0x421437]` | 10 | 0..8 | 35 | 2907..8088, 2 distinct 2907×39, 8088×1 | two story flags each drop needs, column 2 × drop + k: flag value − 1 of `G[0x929]` must be set (`RESULT@0x005b9`) |
| `G[0x423b47]` | 5 | 0..2 | 4 | 2027..7229, 4 distinct 2027×3, 2210×1, 2929×1, 7229×2 | a flag that stops each drop (`RESULT@0x00608`) |
| `G[0x424ecf]` | 1 |  | 16 | 54..97, 16 distinct | the summon stone (an `ITINIT` item, 植物魔獣援石 …) gained when the unit is captured with 性魔術 (item 3201) (`BTL@0x028f4`, `RESULT@0x007c4`) |
| `G[0x4252b7]` | 1 |  | 16 | 1..5, 5 distinct 1×4, 2×5, 3×3, 4×1, 5×3 | the 捕縛 (skill 401) level the capture needs (`BTL@0x05ef7`) |
| `GS[0x12a4]` | 1 |  | 578 | セリカ・シルフィル / 魔神ハイシェラ / エクリア・フェミリンス テシュオス  | the full name; no script reads it |
| `GS[0x168c]` | 1 |  | 575 | セリカ / ハイシェラ / エクリア | the name the game shows: `DRAWCHP@0x00500`, `DRAWENP@0x0019e`, `INFO4@0x00acb`, `ADDTITLE@0x0004e` and ten more |
| `GS[0x1a74]` | 2 | 0..1 | 412 | 素早い攻撃と飛燕剣が得意 / 純粋系と冷却系魔法を扱う / 戦闘は苦手だが回避力の高い | two lines on how the unit fights: the bestiary (`INFO4@0x00f17`) and the enemy panel (`DRAWENP@0x00276`) draw them; a slot with a title draws the title's (`GS[0xe6ac]`) |

### CBINIT (the leads' growth parameters)

Leader `G[0x36fa06]`, 100 rows of 7 columns; the tool's `--leader G:0x36fa06:100:0:7` shows the leader array as
`G[0x36fa00] †`, a display artefact. Two rows, both in the base data: row 1 for セリカ (unit 1) and row 31 for
リウイ (unit 31), named by `EBINIT` `G[0x412dbf]`. A lead grows by seven parameters, kept in the save at
`G[0x119106 + 7 × character + k]`. `UNITECH@0x001e2` copies the row's first array there when the lead joins. `CALCCHP`
then derives the 14 stats from the level and the seven, ×1000 (`CALCCHP@0x0000a`). `CALCABIMAX` sets the level
to (sum of the seven − 1) / 14 + 1 and caps each parameter. `AUTOGROW` raises the parameters by spending saved
experience `G[0x119746]`. The parameters' names are not read.

The starting parameters reach the save once, at joining; the other arrays are read live, so a change reaches an
old save.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x36fa06]` | 7 | 0..6 | 2 | 174..224, 13 distinct | the seven starting parameters: `UNITECH@0x001e2` copies them into the saved `G[0x119106]` |
| `G[0x36fcc2]` | 7 | 0..6 | 2 | 14..17 | the cap's level divisor: a parameter's cap is (level × 1000 / this + sum × 1000 / the next) / 100 + the third, at most 999 (`CALCABIMAX@0x000bc`), written to the saved `G[0x1193c2]` |
| `G[0x36ff7e]` | 7 | 0..6 | 2 | 72..90 | the cap's sum divisor (`CALCABIMAX@0x000ce`) |
| `G[0x37023a]` | 7 | 1..6 | 2 | 6..30 | the cap's addend (`CALCABIMAX@0x00120`) |
| `G[0x3704f6]` | 7 | 0..6 | 2 | 80..120 | the cost of a point, in percent of the parameter: `AUTOGROW@0x00041` spends this × parameter / 100 experience to raise it by one; `CHMODE2@0x01d5b` shows it |
| `G[0x3707b2]` | 1 |  | 2 | 100 | a percentage on max HP, MP and TP (`CALCCHP@0x000be`) |

### CHINIT (characters)

Leader `G[0xc1a671]`, capacity 100. 54 characters: 44 in the base data, 3, 2 and 5 from appends 1, 2 and 3. The id
is the character id the save keys everything by. `UNITECH` runs when a character joins. It reads the character's unit
(`UNITECH@0x00001`) and fills the saved state from `EBINIT` (see above). It sets the title to 20 × character
(`UNITECH@0x00231`, saved `G[0x1218f6]`) and runs `ADDTITLE`. Each append's record writes its stamp
`G[0xc1a865]` before the leader.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc1a671]` | 1 |  | 54 | 0..1, 2 distinct 0×28, 1×26 | the party the character belongs to, 0 (セリカ's) or 1 (リウイ's): `GAMESTART@0x011e3` sets the current party `G[0x363c]` to it before `UNITECH` for a carried-over character; `BTL@0x01878` plays BGM `0x18` for party 1, else `0x17` |
| `G[0xc1a6d5]` | 1 |  | 54 | -1..1, 3 distinct -1×29, 0×15, 1×10 | the party the character joins: `UNITECH@0x00302` sets the saved `G[0x121892 + character]` to it, or to the current party when −1 |
| `G[0xc1a739]` | 1 |  | 54 | 1..720, 54 distinct | the character's `EBINIT` unit: `UNITECH@0x00001`, `SETCH@0x00000`, `CHANGETITLE`, `DRAWCHP@0x00ea7` and 28 more |
| `G[0xc1a79d]` | 1 |  | 43 | 10..850, 41 distinct | the order of party lists (`SORTSALLY@0x000a1`, `SETPTORDER@0x00000`, `DIVIDECH@0x009e3`, `RESULT@0x00114`, `TEMPT@0x00068`) |
| `G[0xc1a801]` | 1 |  | 11 | 7138..9087, 11 distinct | a story flag (value − 1 of `G[0x929]`): for a character the shared profile's carry-over `G[0x2661db]` marks, `GAMESTART@0x011b1` sets this flag at a new game instead of joining it; `GAMECLEAR@0x0056e` reads it |
| `G[0xc1a865]` | 1 |  | 10 | 1..3, 3 distinct 1×3, 2×2, 3×5 | the append the character comes from: `READICON@0x000d1` copies its icon from that append's sheet (surface `0xac` + append) when that append's slot of the hook chain `G[0xc78170 + 0x20 × G[0x268b36]]` is set, that is when the append is installed (med) |

### TIINIT (titles)

Leader `GS[0xdedc]` (the title's name), capacity 2000: `init_layout.py --leader GS:0xdedc:2000` fits it, every array
on its grid. 220 titles: 175 in the base data, 14, 9 and 22 from appends 1, 2 and 3; ids 20 to 1400. A title's id is
20 × character + k. Title 20 × character is the one the character joins with, and the others are earned.
`CHECKTITLE` goes through the character's 20 titles in the order of `G[0xaa108d]` (`CHECKTITLE@0x00023`). It runs
`ADDTITLE` for each one not yet earned whose conditions hold: the level, the title before it, and the story flags.
`ADDTITLE` sets the saved `G[0x16affb + title]`. For a character who has joined, and a class other than 3, it also announces
"…が称号『…』を獲得した" and switches the character to the title (`CHANGETITLE`, saved `G[0x1218f6]`). Below class 3
it grants the title's 特技 (`ADDSKILL`, saved `G[0x119eb2 + 60 × character + …]`).
`GAMECLEAR@0x003b4` stores `G[0x16affb]` to the shared profile, and `GAMESTART@0x00e45` loads it back.

A record's last write sets the next title's required level to −1 (`sub G[0xaa1872] 0x0 0x1`, `G[0xaa185d + 21]`
after title 20): it belongs to the next record, ahead of its name.

A slot with a title takes the title's stat bonuses, skill levels, pictures and voices (`SETCH`, `SETEN`, `ANIME`,
`BTL`). The tool's six off-grid arrays `G[0xaaf383]`, `G[0xab0fc7]` (stride 300), `G[0xb43b3f]`, `G[0xb4beef]`,
`G[0xb4ed0b]`, `G[0xb68346]` and its 60 cells outside the arrays all lie in `G[0xaadbad]`, 600 cells a title,
which ends at `G[0xbd2b2c]`. `G[0xaa2fcd]` (`CHECKTITLE@0x00190`) is read but written by no record. Only `G[0x16affb]`
(earned) and `G[0x1218f6]` (current) are saved, so a changed title reaches an old save. The exception is the 特技 an earned
title granted, which stay in `G[0x119eb2]`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xaa108d]` | 1 |  | 23 | 10..90, 9 distinct | the order `CHECKTITLE@0x00023` tries the character's titles in (`sort_indices_by_key_sum`); `CHMODE2@0x01a33` and `BTL@0x03963` read it |
| `G[0xaa185d]` | 1 |  | 168 | -1..500, 28 distinct | the level the title needs (`CHECKTITLE@0x000b1`): −1 never by level, 0 none; for a class 3 or 4 title it is the master's level (`G[0xaa3f6d]`) that counts |
| `G[0xaa202d]` | 1 |  | 48 | 21..1082, 15 distinct | a title that must be earned first (`CHECKTITLE@0x00125`, `G[0x16affb]`) |
| `G[0xaa27fd]` | 1 |  | 17 | 1..7413, 2 distinct 1×16, 7413×1 | a story flag the title needs, value − 1 of `G[0x929]` (`CHECKTITLE@0x00157`) |
| `G[0xaa379d]` | 1 |  | 115 | 1..4, 4 distinct 1×18, 2×41, 3×26, 4×30 | the title's class. The menus draw the tag from `SO012` (`CHMODE1@0x029bf`): 1 使徒, 2 召喚, 4 支援, any other Master. `ADDTITLE@0x00001` announces only a class other than 3 and grants 特技 only below 3; for a joined character `SETCHPSTATE@0x00045` sets `bit_set(0)` of its `G[0x11967e]` below 3 and `bit_set(0x10)` for class 4; `CHANGETITLE@0x0000d`, `DIVIDECH@0x00052`, `CALCILL@0x00603` test it. Class 3 is not read further |
| `G[0xaa3f6d]` | 1 |  | 107 | 1..45, 4 distinct 1×68, 11×20, 12×17, 45×2 | the master (契約者), a character: 1 セリカ, 11 リウイ, 12 カーリアン, 45 リリィ. `CHMODE1@0x02908` tests the master's `G[0x11967e]`, `SETCH@0x00d74` tests it for a class 2 title, `CHECKTITLE@0x000f4` takes the master's level |
| `G[0xaa473d]` | 1 |  | 25 | 512..917504, 9 distinct | item kinds the title adds to the unit's (`EBINIT` `G[0x37bbc7]`, same bits): `CHANGETITLE@0x0013f` unequips the saved slots the new mask no longer allows; `DRAWCHP@0x00ec1`, `SHOP@0x0410d`, `ALCHEMY@0x03d1c` |
| `G[0xaa4f0d]` | 9 | 0..7 | 164 | 4..55, 31 distinct | the 特技 the title grants, ids below 60 (`ADDTITLE@0x000e8` runs `ADDSKILL`); `DRAWCHP@0x00f7b` and `CHMODE3@0x00fc7` draw them; `GAMESTART@0x00ced` and `GAMECLEAR@0x0025c` collect them |
| `G[0xaa955d]` | 9 | 0..7 | 164 | 1..7, 7 distinct | each 特技's level (`ADDTITLE@0x00136`): `ADDSKILL` raises the saved `G[0x119eb2]` to it |
| `G[0xaadbad]` | 600 | 1..524 | 220 | 1..5, 5 distinct 1×496, 2×195, 3×233, 4×85, 5×90 | skill levels, column = an `SSINIT` skill id, added to the unit's: `SETEN@0x00d21`, `SETCH@0x00b8c`, `CALCREVISE@0x000d5`, `DRAWCHP@0x01844` and seven more |
| `G[0xbd2b2d]` | 1 |  | 134 | 50..1800, 24 distinct | bonus to max HP: `SETCH@0x001ff` and `SETEN@0x004c1` add it to the slot's `G[0xc1a9ba]`; `CALCREVISE@0x005bc` |
| `G[0xbd32fd]` | 1 |  | 107 | 20..400, 18 distinct | bonus to max MP (`G[0xc1a9f6]`) |
| `G[0xbd3acd]` | 1 |  | 89 | 20..275, 18 distinct | bonus to max TP (`G[0xc1aa32]`) |
| `G[0xbd429d]` | 1 |  | 56 | 2..100, 12 distinct | bonus to 命中 (`G[0xc1acc6]`, `SETCH@0x00387`, `SETEN@0x00649`) |
| `G[0xbd4a6d]` | 1 |  | 50 | 2..20, 8 distinct | bonus to 回避 (`G[0xc1ace4]`) |
| `G[0xbd523d]` | 1 |  | 121 | 20..300, 20 distinct | bonus to 攻撃力 (`G[0xc1aa50]`) |
| `G[0xbd5a0d]` | 1 |  | 102 | 5..90, 13 distinct | bonus to 防御力 (`G[0xc1aa6e]`) |
| `G[0xbd61dd]` | 1 |  | 110 | 10..200, 21 distinct | bonus to 魔法攻撃 (`G[0xc1aa8c]`) |
| `G[0xbd69ad]` | 1 |  | 73 | 10..120, 14 distinct | bonus to 魔法防御 (`G[0xc1aaaa]`) |
| `G[0xbd717d]` | 1 |  | 94 | 1..8, 8 distinct | bonus to the slot's magic power multiplier `G[0xc1ad20]` (`SETCH@0x003db`, `SETEN@0x0069d`) |
| `G[0xbd794d]` | 1 |  | 115 | 1..5, 5 distinct 1×12, 2×41, 3×28, 4×20, 5×14 | bonus to 攻撃回数 (`G[0xc1aac8]`) |
| `G[0xbd811d]` | 1 |  | 84 | 1..5, 5 distinct 1×22, 2×31, 3×16, 4×13, 5×2 | bonus to 防御回数 (`G[0xc1aae6]`) |
| `G[0xbd88ed]` | 1 |  | 72 | 2..15, 8 distinct | bonus to 肉体抵抗 (`G[0xc1ab04]`) |
| `G[0xbd90bd]` | 1 |  | 82 | 2..15, 8 distinct | bonus to 精神抵抗 (`G[0xc1ab22]`) |
| `G[0xbd988d]` | 1 |  | 124 | 1..12, 9 distinct | bonus to 肉体戦速 (`G[0xc1ab40]`) |
| `G[0xbda05d]` | 1 |  | 110 | 1..14, 11 distinct | bonus to 精神戦速 (`G[0xc1ab5e]`) |
| `G[0xbda82d]` | 1 |  | 103 | 2..15, 8 distinct | bonus to 運 (`G[0xc1ab7c]`) |
| `G[0xbdb7cd]` | 1 |  | 220 | 1..5, 4 distinct 1×88, 2×11, 4×80, 5×41 | the cut-in style: for a party slot, when an entry of `G[0xc25966]` is 4, `ANIME@0x005a6` plays the cut-in movie with the title's picture (`G[0xbdc76d]`); 1 animates it one way, other values another (med) |
| `G[0xbdbf9d]` | 1 |  | 220 | 13859..50332250, 60 distinct | the battle portrait, drawn at the screen's lower right (`ANIME@0x00952`, `DRAWCARD@0x00032`) |
| `G[0xbdc76d]` | 1 |  | 218 | 2165..50331971, 58 distinct | the cut-in picture (`ANIME@0x00620`) |
| `G[0xbdcf3d]` | 10 | 0..9 | 220 | 17600..50332252, 389 distinct | ten battle sprite sheets: `DRAWCHP@0x001a9` loads column 0, or column 9 when the character's saved `G[0x11cfea + 60 × character + 3]` is set (`DRAWCHP@0x0018d`), and draws its first 120 × 120 (`DRAWCHP@0x002c3`); `BTL` loads columns 0, 2, 3, 5, 7 and 9 (`BTL@0x032fe`). Column 0 of every title is a strip of six frames side by side, the character seen from behind: 720 × 120, or 1920 × 300 (frames of 320 × 300) for the large forms of 3 × 3 cells, units 501 to 513 and ディアーネ's `CPB035BA` |
| `G[0xbe1d5d]` | 1 |  | 194 | 164..50332249, 50 distinct | the status screen's portrait (`DRAWCHP@0x00166`, surface `0x85`) |
| `G[0xbe252d]` | 1 |  | 164 | 17610..50331981, 37 distinct | the face in the map's party panel (`DRAWMAPF@0x00081`, `CHANGETITLE@0x00362`, surface `0x98` + place) |
| `G[0xbe2cfd]` | 25 | 0..9 | 41 | 17614..18514, 71 distinct | the party leader's sprites on the field map, column picked by `G[0x5f2073 + G[0x36df]]` (`CHANGETITLE@0x00051`, `FIELD@0x01bde`, `SETMAPTEX@0x0014e`, `ALLMAP@0x0148f`); `G[0x363b]` is the leader's title |
| `G[0xbef04d]` | 25 | 0..9 | 41 | 17615..18515, 71 distinct | the leader's jump sprites, by the same column (`DRAWJUMP@0x0019b`) |
| `G[0xbfb39d]` | 1 |  | 41 | 17613..18513, 8 distinct | the leader's sprite in `CARRY@0x00750` |
| `G[0xbfbb6d]` | 2 | 0..1 | 22 | 17611..17793, 10 distinct | two pictures of the leader in `EXTRA1@0x002b0` |
| `G[0xbfcb0d]` | 30 | 0..27 | 169 | 2429..50333438, 867 distinct | 30 voice clips, the columns as the unit's (`EBINIT` `G[0x370817]`): `BTL@0x08b18`, `ANIME@0x01fd0`, `RESULT@0x01612`, `CHANGECH@0x009b8` |
| `GS[0xdedc]` | 1 |  | 220 | 放浪の旅人 / 魔剣を持つ者 / 放浪の神殺し | the title's name (`ADDTITLE@0x0005c`, `DRAWCHP@0x00541`, `CHMODE1@0x02be5` …); `CHECKTITLE@0x0007d` skips a title without one |
| `GS[0xe6ac]` | 2 | 0..1 | 164 | 魔術と必殺技が両立 / 前衛攻撃型 … | two lines on how the title fights, which the enemy panel draws for a slot with a title (`DRAWENP@0x00221`) |

### CNINIT (speakers) and CVINIT (voice settings)

The two tables share one grid of capacity 1000 from `G[0x268b8c]`: `CNINIT` writes `GS[0x1b6]` and `G[0x268b8c]`,
`CVINIT` the next three integer arrays, each 1000 cells on, and `GS[0x59e]` (`GS[0x1b6]` + 1000). `GS[0x1b6]` is the first string cell above the saved prefix, so none of
it is saved.

`CNINIT`: leader `GS[0x1b6]`, 576 speakers, ids 1 to 975: 492 in the base data, 11, 3 and 70 from appends 1, 2 and
3. The speaker id is the scene's `G[0x3489]`, the same number as the speaker's `EBINIT` unit. A scene draws the name
plate from `GS[0x1b6]` (`SC0000@0x03c9a`), sets the speaker seen in the shared profile (`G[0x2625cb + speaker]`),
and mutes the voice when the saved switch `G[0x30a0 + setting]` is set for the speaker's setting,
`G[0x268f74 + G[0x268b8c + speaker]]` (`SC0000@0x00bd1`).

`CVINIT`: leader `G[0x269744]`, 48 settings, ids 0 to 47: 45 in the base data, 2 from append 1 and 1 from append 3.
The 設定 voice page (`CONFIG`) lists them. With this leader the tool cuts `G[0x268f74]` into fragments
(`G[0x268f67]`, `G[0x268f89]`, `G[0x268f91]`), because that array is keyed by unit, not by setting.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `GS[0x1b6]` | 1 |  | 576 | セリカ / ハイシェラ / エクリア | `CNINIT`: the speaker's name on the name plate (`SC0000@0x03c9a`, `HISTORY@0x0107e`) |
| `G[0x268b8c]` | 1 |  | 570 | 2..975, 425 distinct | `CNINIT`: the unit whose voice setting the speaker uses (`SC0000@0x00bd1`, `HISTORY@0x00978`, `CHANGECH@0x009d9`) |
| `G[0x268f74]` | 1 |  | 47 | 1..47, 47 distinct | `CVINIT`, by unit: the unit's voice setting; the scenes' mute test (255 scripts, `SC0000@0x00bd8`) |
| `G[0x26935c]` | 1 |  | 47 | 2..720, 47 distinct | `CVINIT`: the setting's unit; `CONFIG@0x017bc` shows a setting's name once `G[0x2625cb]` has that unit |
| `G[0x269744]` | 1 |  | 48 | 169..50332465, 48 distinct | `CVINIT`: the sample voice clip `CONFIG@0x01618` plays; `CONFIG@0x017a2` lists the settings that have one |
| `GS[0x59e]` | 3 | 0..2 | 48 | エキストラ / エクリア / ハイシェラ | `CVINIT`: the name, a descriptor shown until the unit has spoken, and the voice actress after "CV:" (`CONFIG@0x023e7`) |

### CIINIT (profiles)

Leader `G[0xc48eff]`, capacity 200. 92 profiles: 84 in the base data, 5 from append 1 and 3 from append 3. The 人物
book (`INFO3`) lists a profile that has a name (`INFO3@0x00948`). It hides one until its unit has spoken,
`G[0x2625cb + unit]` (`INFO3@0x00a0c`). It also needs one of its scenes seen, unless it names none (`INFO3@0x00973`).
`G[0xc48efe]` is the profile shown, and `CIMES@0x00000` writes that profile's text.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc48eff]` | 1 |  | 92 | 1..752, 73 distinct | the speaker (unit) whose having spoken shows the profile (`INFO3@0x00a0c`) |
| `G[0xc48fc7]` | 3 | 0 | 18 | 31..8111, 15 distinct | up to three scenes, each scene + 1: the profile is listed once one is seen (`G[0x25febb]` in the shared profile, `INFO3@0x00973`); with none it is always listed |
| `G[0xc4921f]` | 1 |  | 92 | 164..50332097, 91 distinct | the profile's picture (`INFO3@0x00fcd`) |
| `G[0xc492e7]` | 1 |  | 87 | -90..330, 14 distinct | the picture's x offset (`INFO3@0x0100c`) |
| `G[0xc493af]` | 1 |  | 8 | -230..130, 3 distinct -230×6, 80×1, 130×1 | the picture's y offset (`INFO3@0x0101f`) |
| `GS[0x10800]` | 1 |  | 92 | セリカ・シルフィル / エクリア・フェミリンス / エクリア・フェミリンス | the name in the list (`INFO3@0x00948`, `INFO3@0x00e2b`) |

A picture value of `0x1000000` or more carries an append's selector in its top byte (`50332097` = `0x30001c1`, append 3), as in the other tables.

## Items, skills and constants

ITINIT is the hub of this part. Its ids name the items, the equipment, the spells, the arts and the enemies'
actions, and every other table here points into it: IMINIT grows its equipment, SKINIT arranges its actions into
trees by class, SSINIT's skills are what its items require or give, ILINIT's conditions are the columns of its
condition rows, AFINIT names its elements and classes, and ALINIT, TRINIT, EX1INIT, SHINIT and CSINIT make, hide,
catch, sell and field what it names. SHINIT and CSINIT are not in the boot image; their sections say who runs them.

### ITINIT (items, magic and arts)

ITINIT is one table for everything a character can carry or do: items, equipment, spells, arts and the enemies'
actions share its ids. Leader `GS[0x2f78]` (the name), capacity 10000, 1501 records: 1334 in the base data, and 8,
13 and 146 from the three appends. Its 60 arrays agree modulo 10000; the two condition arrays (60 cells a record)
and the two character lists (100 a record) fit the grid, and no cell falls outside an array.

The ids fall in ranges by what they are. 1..97 key items; 101..242 consumables and 絵札; 251..281 growth stones;
301..306 the 支配率 staffs; 401..491 錬液 (synthesis materials); 501..508 sell-only goods; 601..608 baits; 611..625
fish. 1001..1756 weapons; 2001..2997 armour, shields and accessories. 3001..3273 magic; 4001..4265 arts (技).
5007..5974, 6001..6419 and 7002..7514 the enemies' actions. 9979 盗む, 9980..9998 special actions, and 9999 素手,
the weapon of a unit that holds none (SETCH@0x17cc).

The current item is `G[0x63df37]` and a change of its count `G[0x63df38]`. `ADDITEM` keeps the count held,
`G[0x10cb17 + item]`, within 0 to 999 (ADDITEM@0x8..0x5d). It marks the item as once obtained in
`G[0x10f227 + item]` and stores that mark in the shared profile (ADDITEM@0x62..0x75). A key item (class 1) also
sets or clears story flag 9001 + item, `G[0x929 + 0x2328 + item]` (ADDITEM@0x78..0xbf). An action in battle is `G[0x276e55]`.
`ITMES` holds every description, chosen by the item's category (ITMES@0x0), and the descriptions confirm most
readings below (the rank letter, "範囲W×H", "N属性", "魔法威力N", "MP+300").

Equipment works through the battle unit. SETCH@0x4f9..0x729 adds each equipped item's numbers into the unit's
arrays (30 cells apart, the unit in `G[0xc1a8c9]`), and SETEN does the same for an enemy. The tooltip
(DRAWTIP@0x0..0x84) draws twelve of them over its background, `SO012.AGF` at (0, 0x591). Its top row is 命中, 物攻,
攻回, 魔攻 (戦意 for a damage kind 5 action), 肉速 and 運; its bottom row is 回避, 物防, 防回, 魔防, 精速 and CT率. The
picture confirms the order. `CHMODE1` decides whether a character may use or equip an item, through the checks the
table names: rank, owners, exclusive users, ハイシェラ, women only and a required skill. `SETCH` reads one more item
array, `G[0x9d17a9]` (SETCH@0x4f9 adds it into the unit's `G[0xc1a97e]`, and CALCREVISE@0x84b sums it), but ITINIT
writes none of it, so it is not in the table.

Saved state: no item field is copied into a saved cell. The unit arrays SETCH fills lie above `0x268b19`, and battle
rebuilds them. The saved cells hold item ids and per-item state: the count, the obtained mark, the growth exp and
level (IMINIT below), and the characters' equipment slots (`G[0x11ce5a + char]`, `G[0x11cebe + char]` and their
neighbours, which hold item ids). So a changed number reaches an old save, but a renumbered item does not. Two
fields steer a new game: GAMECLEAR@0x3e1 stores the count of every item of class over 1 in the shared profile, and
GAMESTART@0xe85 loads it.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x63df39]` | 1 |  | 702 | 10..29970, 700 distinct | a sort number in steps of ten, on the 702 items and on no action. The item lists are in its order (`sort-indices-by-key-sum`: ALCHEMY@0x149a, CHMODE1@0x1734, INFO5@0x8d7, SHOP@0x1dcd) |
| `G[0x640649]` | 1 |  | 995 | 1..7, 7 distinct | the rank, 1 to 7: E, D, C, B, A, S, M (each description names it). CHMODE1@0x1caf refuses an equipment or action while the character's rank in its class (`G[0x119eb2 + 60*char + class]`) is lower. ADDSKILL@0xbd reads it for actions |
| `G[0x642d59]` | 1 |  | 1060 | 1..59, 48 distinct | the category, 1 to 59. Weapons: 1 杖, 2 投石, 3 短剣, 5 突剣, 6 剣, 7 拳, 8 連接, 9 双剣, 10 大剣, 11 槍, 12 弓. Armour: 16 衣服, 17 中装鎧, 18 重装鎧, 19 魔導鎧, 20 盾, 22 装飾. Others: 26 重要, 27 消費, 29 支配率操作, 30 合成, 31 売却専用, 33 成長, 34 餌, 35 魚. Then 36..49 the magic schools and 51..59 the art schools. ITMES@0x0 dispatches on it. DRAWTIP@0x273 draws its icon from surface 0x7b at ((c−1)%10·12, (c−1)/10·14). AFINIT's `G[0xc0b991 + c]` holds its flags |
| `G[0x645469]` | 1 |  | 1253 | 1..55, 42 distinct | the class, 1 to 55, an index into AFINIT's class arrays (long names `GS[0xf674 + class]`). 1 重要, 2 物品, 3 非戦; 4..6 小型, 中型, 大型 weapons; 7..10 軽装, 中装, 重装, 魔装; 11 小盾, 12 大盾, 13 装飾; 21..39 the magic schools, 40 敵魔; 41..55 the art schools, 50 敵必. ADDITEM@0x78 treats class 1 as a key item. BTL@0x33b6 adds the weapon's 攻撃回数 to an action of class over 40. GAMECLEAR@0x3e1 carries over the count of every item of class over 1 |
| `G[0x647b79]` | 1 |  | 1010 | 1..5, 5 distinct 1×704, 2×244, 3×12, 4×12, 5×38 | the damage kind: 1 physical, 2 magic, 3 a mind check (精神判定), 4 a buff or special, 5 a 戦意 attack. CALCDAMAGE@0x213 sends 3 and 4 down their own path. BTL@0x332d takes 精神戦速 for 2, 3 and 5, and 肉体戦速 otherwise. With 5 the tooltip calls stat 6 戦意 (DRAWTIP@0x20f), and SETCH@0x5e7 does not add it to 魔攻 |
| `G[0x64a289]` | 1 |  | 5 | 1..1, 1 distinct 1×5 | 1 on five 軍配 arts, usable once a battle (※戦闘中一度のみ: CHECKACT@0x869, CHMODE3@0x17a3) |
| `G[0x64c999]` | 1 |  | 197 | 1..1, 1 distinct 1×197 | 1 on 197 attack spells that charge. The caster's charge level `G[0xc1c418 + unit]` stretches the charge time by (8 + level)/8 (BTL@0x33f1) and raises the damage, ×1.2 at level 1 (CALCDAMAGE@0x7de) |
| `G[0x64f0a9]` | 1 |  | 791 | 1..13, 11 distinct | the attack element, 1 to 17, an index into AFINIT's `GS[0xf64c]` (無属, 物理, 万能, 火炎, 冷却, 電撃, 地脈, 神聖, 暗黒, 戦意, 抵抗1..7). It is the affinity matrix's column (CALCDAMAGE@0x179..0x20c). SETCH@0x94f gives a unit with no weapon element 2 at level 0 |
| `G[0x6517b9]` | 1 |  | 505 | 1..5, 5 distinct 1×167, 2×161, 3×125, 4×30, 5×22 | the attack element's level, 1 to 5 (the digit of "N属性" in the descriptions). Each level above the defender's adds 20 to the affinity (CALCDAMAGE@0x179..0x20c) |
| `G[0x653ec9]` | 1 |  | 127 | 2..15, 13 distinct | the armour's defense element, 2 to 15, an index into `GS[0xf660]` (1..9 as above, then 神格, 魔神, 反万, 霊体, 不死, 創造). It is the affinity matrix's row. Without armour SETCH@0xa33 takes the character's own (`G[0x37a457]`, `G[0x37a83f]`) |
| `G[0x6565d9]` | 1 |  | 86 | 1..4, 4 distinct 1×32, 2×28, 3×25, 4×1 | the defense element's level, 1 to 4 |
| `G[0x660219]` | 100 | 0..1 | 91 | 1..51, 4 distinct 1×55, 2×37, 11×25, 51×1 | the characters who own the item (CHINIT ids; the "/リウイ所持" key items hold 11). CHMODE1@0x19f4 refuses it unless one of them is in the party (bit 0 of `G[0x11967e + char]`). ALCHEMY, CHMODE4 and SHOP read it too |
| `G[0x754459]` | 1 |  | 10 | 1..1, 1 distinct 1×10 | 1 on ten ハイシェラ items. CHMODE1@0x1a82 refuses them unless the character in `G[0x266384]` is present. PUTHAI reads it |
| `G[0x756b69]` | 100 | 0..4 | 222 | 1..63, 30 distinct | the characters the item is exclusive to (専用; CHINIT ids). CHMODE1@0x1b80 refuses it unless the current character is one of them. ADDSKILL@0x135, CHECKSKILL, CHMODE3, ALCHEMY and SHOP read it too |
| `G[0x84ada9]` | 1 |  | 17 | 1..1, 1 distinct 1×17 | 1 on 17 items for women. CHMODE1@0x1acb refuses them when `G[0x3788ff + G[0xc1a739 + char]]` is not 2 (inferred: that cell is the sex) |
| `G[0x859809]` | 1 |  | 40 | 438..458, 9 distinct | a skill the user must have, an SSINIT id: 438 主従の絆, 451 魔剣解放, 452 魔導鎧, 454, 456, 457. CHMODE1@0x1b08 checks the character's level of it (`G[0x12195a]`), the class's (`G[0xaadbad]`) and the player's switch for it (`G[0x1303ba]`). CALCREVISE@0xa9 reads it |
| `G[0x85bf19]` | 1 |  | 5 | 1..1, 1 distinct 1×5 | 1 on five cursed items ("一度装備すると……" in their descriptions). Read by ADDILL@0x486, CALCILL@0x113, SETCHSEL@0x56b and SHOP@0x168a |
| `G[0x85e629]` | 1 |  | 449 | 1..3, 3 distinct 1×22, 2×407, 3×20 | the reach: 1 ground, 2 melee, 3 ranged. Against a defender with skill 373 飛行能力 or 379 飛竜使い, 1 does nothing, 2 does ×0.8 and 3 does ×1.5 (SETSSDAMAGE@0x24..0xb3) |
| `G[0x860d39]` | 1 |  | 1100 | 1..5, 4 distinct 1×901, 2×133, 3×54, 5×12 | the target: 1 enemies, 2 allies, 3 a growth item, 5 a summon (招聘). CALCDAMAGE@0x32 takes the recovery path for anything but 1. SELTARGET and the RTN scripts read it |
| `G[0x863449]` | 1 |  | 1062 | 1..5, 4 distinct 1×669, 2×73, 3×80, 5×240 | the area's width (the W of "範囲W×H": all 491 descriptions that give one agree) |
| `G[0x865b59]` | 1 |  | 1062 | 1..5, 5 distinct 1×622, 2×88, 3×157, 4×2, 5×193 | the area's height (the H) |
| `G[0x868269]` | 1 |  | 1056 | 3..82, 55 distinct | the charge time before the action (the name is inferred). BTL@0x3321 subtracts the unit's 肉体戦速 or 精神戦速 (by damage kind), down to `G[0x86a979]`. SETCH@0x17cc puts the weapon's, or 9999's, in the unit's `G[0xc1ac12]`. CHMODE3@0x1206 shows it |
| `G[0x86a979]` | 1 |  | 1055 | -3..20, 19 distinct | the least charge time (BTL@0x3321) |
| `G[0x86d089]` | 1 |  | 1058 | 2..82, 58 distinct | the recovery time after the action (the name is inferred), down to `G[0x86f799]` (BTL@0x4be7; the unit's `G[0xc1ac30]`) |
| `G[0x86f799]` | 1 |  | 1054 | 1..15, 13 distinct | the least recovery time |
| `G[0x871ea9]` | 60 | 1..44 | 449 | -5..5, 10 distinct | condition levels, the column an ILINIT condition id: positive inflicts, negative cures (127 女神の滴 is −1 on 8, 9, 10, 11, 12, 14, 17 and 19). Read by BTL, CALCILL@0xa7, CHECKRECV, SETTARGET, SETCHSEL, USESKILLSUB and the RTN scripts |
| `G[0x904669]` | 60 | 7..40 | 58 | 1..5, 5 distinct 1×20, 2×18, 3×29, 4×3, 5×28 | the conditions an equipment keeps on its wearer, by ILINIT id and level (義賊王の魂刀 17 at 3; the cursed items 7). Read by CALCREVISE@0x41d, SETCH@0xe89 and SETEN |
| `G[0x996e29]` | 1 |  | 622 | -99..999, 21 distinct | added to 命中 (the unit's `G[0xc1acc6]`) |
| `G[0x999539]` | 1 |  | 151 | -20..40, 15 distinct | added to 回避 (`G[0xc1ace4]`) |
| `G[0x99bc49]` | 1 |  | 467 | -50..18000, 211 distinct | added to 物攻, physical attack (`G[0xc1aa50]`) |
| `G[0x99e359]` | 1 |  | 142 | -3..420, 71 distinct | added to 物防, physical defense (`G[0xc1aa6e]`) |
| `G[0x9a0a69]` | 1 |  | 412 | -99..999, 26 distinct | added to 攻回, attacks per action (`G[0xc1aac8]`) |
| `G[0x9a3179]` | 1 |  | 117 | -7..5, 12 distinct | added to 防回, defenses (`G[0xc1aae6]`) |
| `G[0x9a5889]` | 1 |  | 241 | 5..30000, 127 distinct | added to 魔攻, magic attack (`G[0xc1aa8c]`). A damage kind 5 action shows it as 戦意, and SETCH skips it |
| `G[0x9a7f99]` | 1 |  | 132 | 3..354, 66 distinct | added to 魔防, magic defense (`G[0xc1aaaa]`) |
| `G[0x9aa6a9]` | 1 |  | 206 | 1..120, 45 distinct | added to 魔法威力 (`G[0xc1ad20]`, which CALCDAMAGE@0x707 applies). 火弾's "魔法威力5" is 5 |
| `G[0x9acdb9]` | 1 |  | 56 | 2..99, 14 distinct | added to 肉体抵抗 (`G[0xc1ab04]`, CALCILL@0x68b). The ハイシェラ armours confirm it |
| `G[0x9af4c9]` | 1 |  | 68 | -5..99, 22 distinct | added to 精神抵抗 (`G[0xc1ab22]`, CALCILL@0x649) |
| `G[0x9b1bd9]` | 1 |  | 150 | -20..5, 14 distinct | added to 肉速, 肉体戦速 (`G[0xc1ab40]`) |
| `G[0x9b42e9]` | 1 |  | 144 | -20..5, 15 distinct | added to 精速, 精神戦速 (`G[0xc1ab5e]`) |
| `G[0x9b69f9]` | 1 |  | 16 | 2..30, 7 distinct | added to 運 (`G[0xc1ab7c]`) |
| `G[0x9b9109]` | 1 |  | 447 | 1..100, 22 distinct | added to CT率 (`G[0xc1ad02]`) |
| `G[0x9bb819]` | 1 |  | 47 | -80..999999, 30 distinct | HP restored (positive) or spent (negative). CALCDAMAGE@0x4c..0x15b caps a restore at 99999. CHMODE3@0x1187 shows a cost |
| `G[0x9bdf29]` | 1 |  | 279 | -350..9999, 83 distinct | MP restored or spent ("MP10" is −10) |
| `G[0x9c0639]` | 1 |  | 299 | -210..9999, 80 distinct | TP restored or spent (身妖舞's "TP14" is −14) |
| `G[0x9c2d49]` | 1 |  | 16 | 2..80, 11 distinct | 戦意 (FS) restored: CALCDAMAGE@0x160 sets `G[0x276e95]` to minus it (勇壮の水, 祝福) |
| `G[0x9c5459]` | 1 |  | 6 | -15..15, 6 distinct -15×1, -10×1, -5×1, 5×1, 10×1, 15×1 | the change to the dungeon's 支配率, ±5 to 15. USESKILL@0x0 adds it to the saved `G[0x36e1]` and keeps that at 15 or less. SETCHSEL@0x5 reads it |
| `G[0x9c7b69]` | 1 |  | 205 | -1..-1, 1 distinct -1×205 | −1 on 205 consumables and materials (ids 101..625); no script reads it |
| `G[0x9ca279]` | 1 |  | 19 | 100..2000, 10 distinct | added to 最大HP (`G[0xc1a9ba]`; 燐武 HP+500) |
| `G[0x9cc989]` | 1 |  | 6 | 100..300, 3 distinct 100×4, 200×1, 300×1 | added to 最大MP (`G[0xc1a9f6]`) where not 0 (杖の耳飾り MP+300) |
| `G[0x9cf099]` | 1 |  | 6 | 100..300, 3 distinct 100×4, 200×1, 300×1 | added to 最大TP (`G[0xc1aa32]`) where not 0 (剣の耳飾り TP+300) |
| `G[0x9d3eb9]` | 1 |  | 569 | 3..300, 23 distinct | a random part: `random-modulo` of it is added to the amount (CALCDAMAGE@0x74, USESKILLSUB@0x32a) |
| `G[0x9d65c9]` | 1 |  | 1074 | 101..9998, 893 distinct | the battle animation: BTL@0x1b89 puts it in `G[0xc25964]` and calls ANIME. Most records hold their own id; the fish hold a consumable's |
| `G[0x9d8cd9]` | 1 |  | 595 | 50..777777, 230 distinct | the buy price. SHOP@0x2b6b draws it, and SHOP@0x2bd0 keeps a purchase total at 9999999 or less |
| `G[0x9db3e9]` | 1 |  | 592 | 12..194444, 230 distinct | the sell price, a quarter of the buy price; 0 cannot be sold (SHOP@0x1213, @0x2db8, @0x3307). INFO5 shows both prices |
| `G[0x9ddaf9]` | 1 |  | 73 | 1..3, 2 distinct 1×51, 3×22 | where the menu offers the item: bit 0 while `G[0x36e5]` is set, bit 1 while it is not (CHMODE1@0x3f4, CHMODE3@0x2e3). FIELD@0xc5 sets that cell and the map and scene scripts clear it, so (inferred) bit 0 is the field and bit 1 elsewhere. 1 on the potions, 3 on the growth stones and 解石/破術. Battle use does not check it (CHMODE3@0x158e) |
| `G[0x9e0209]` | 1 |  | 109 | 1..458, 42 distinct | a skill (an SSINIT id) the equipment gives its wearer (大胆な水着: 375 挑発). SETCH@0xafb adds it to the unit's `G[0xc1ca4e]` row. CALCREVISE@0x175 reads it |
| `G[0x9e2919]` | 1 |  | 109 | 1..5, 5 distinct 1×68, 2×19, 3×11, 4×2, 5×9 | that skill's level, 1 to 5 |
| `GS[0x2f78]` | 1 |  | 1501 | 飛翔の耳飾り / 霞の地図 / 英傑の挑戦証 | the name. Every list and message prints it (RESULT@0xb0c, ALCHEMY@0x1e13) |
| `GS[0x5688]` | 1 |  | 106 | MP016のミニゲームで獲得 / MP016のミニゲームで獲得 / MP016のミニゲームで獲得 | an author's note on 106 records ("MP016のミニゲームで獲得", "★セリカ", "合成専用 盾", "特殊処理"); no script reads it |

### IMINIT (equipment growth)

IMINIT gives 128 equipment items a growth path, keyed by the ITINIT item id: 99 weapons and 29 armours. The base
data has 94 of them, and the three appends add 2, 2 and 30. It has no leader. The arrays are listed as the scripts
index them, and they line up at capacity 10000: one cell an item (`G[0x42edc9]`, `G[0x5d2c89]`, `G[0x5d5399]`),
11 cells an item (`G[0x4314d9]`), and 10 cells an item for the per-level bonuses, which are 100000 cells apart.
`Leader("G", 0x42edc9, 10000)` fits worse: it cuts `G[0x4314d9]` into a † fragment at `0x4318ef` and loses
`G[0x5d2c89]`. So the table keeps no leader.

An item's level `G[0x114049 + item]` and its exp `G[0x111937 + item]` are saved. GAMECLEAR@0x400 stores both in the
shared profile for every item with a growth kind, and GAMESTART@0xea4 loads them. A kind 2 item grows in battle:
BTL@0x6017 adds 1 to the exp of each such item that the unit has equipped (the slots `G[0xc1ab9a]`, `G[0xc1abb8]`,
`G[0xc1abd6]`, `G[0xc1abf4]`). RESULT@0xa91 then walks the items 1001..2999. Where the exp reaches the cost of the
next level, it raises the level, empties the exp and prints "『…』の性能が上がった". A kind 1 item is refined in
`ALCHEMY` instead.

For every level below the item's own, SETCH@0x754..0x942 adds that level's column of each bonus array to the unit's
numbers. DRAWTIP@0x10a sums the same columns for the tooltip. The bonus arrays come in the order of ITINIT's stat
arrays: 命中, 回避, 物攻, 物防, 攻回, 防回, 魔攻, 魔防, 魔法威力, 肉体抵抗, 精神抵抗, 肉体戦速, 精神戦速, 運, CT率, then
the element level. Three of them get no writes, so they are not in the table: `G[0x527e29]` 肉体抵抗 (SETCH@0x829),
`G[0x5404c9]` 精神抵抗 (SETCH@0x849) and `G[0x5898a9]` 運 (SETCH@0x8a9, DRAWTIP@0x1be).

At the last level, where the next cost is 0, the item may yield another item and set a flag (RESULT@0xb54,
ALCHEMY@0x1df1). The flag is a saved cell. A table change reaches an old save, except that an item already past its
old last level keeps the flag it set.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x42edc9]` | 1 |  | 128 | 1..2, 2 distinct 1×57, 2×71 | how the item grows: 1 錬成可, by refining in ALCHEMY (57 items, all with "錬成可" in their description); 2 成長可, by battles (71; 70 say "成長可"). BTL@0x6017 adds 1 to the saved `G[0x111937 + item]` of each kind 2 item a unit has equipped, and RESULT@0xa91 levels it up. DRAWTIP@0x2d1 labels the two kinds apart |
| `G[0x4314d9]` | 11 | 0..9 | 128 | 1..512000, 54 distinct | what the next level costs, by the current level (column = `G[0x114049 + item]`). For kind 2 it is the battles needed: RESULT@0xab2 raises the level once `G[0x111937 + item]` reaches it, and empties the exp. For kind 1 it is the price of refining, summed in ALCHEMY@0xaab (inferred from its seven-digit total, ALCHEMY@0x2a58). 0 marks the last level |
| `G[0x44c289]` | 10 | 0..9 | 66 | -5..15, 12 distinct | 命中 added per level below the item's level (SETCH@0x8c9, DRAWTIP@0x10a) |
| `G[0x464929]` | 10 | 0..9 | 30 | -5..11, 10 distinct | 回避 per level (SETCH@0x8e9) |
| `G[0x47cfc9]` | 10 | 0..9 | 81 | -142..150, 21 distinct | 物攻 per level (SETCH@0x754; ALCHEMY@0x380d and SHOP@0x3ac1 show it) |
| `G[0x495669]` | 10 | 0..9 | 24 | 1..60, 13 distinct | 物防 per level (SETCH@0x774) |
| `G[0x4add09]` | 10 | 1..9 | 64 | -1..5, 6 distinct -1×2, 1×70, 2×13, 3×7, 4×1, 5×1 | 攻回 per level (SETCH@0x794) |
| `G[0x4c63a9]` | 10 | 1..7 | 15 | -1..2, 3 distinct -1×6, 1×10, 2×2 | 防回 per level (SETCH@0x7b4) |
| `G[0x4dea49]` | 10 | 0..9 | 28 | 1..120, 16 distinct | 魔攻 per level, not for damage kind 5 (SETCH@0x7e9; CALCDAMAGE@0x4bb) |
| `G[0x4f70e9]` | 10 | 0..9 | 25 | 1..60, 14 distinct | 魔防 per level (SETCH@0x809) |
| `G[0x50f789]` | 10 | 3..9 | 13 | 1..8, 7 distinct | 魔法威力 per level (SETCH@0x929, into `G[0xc1ad20]`); the tooltip does not show it |
| `G[0x558b69]` | 10 | 1..7 | 27 | 1..2, 2 distinct 1×27, 2×5 | 肉体戦速 per level (SETCH@0x869) |
| `G[0x571209]` | 10 | 2..7 | 27 | 1..5, 3 distinct 1×27, 2×5, 5×1 | 精神戦速 per level (SETCH@0x889) |
| `G[0x5a1f49]` | 10 | 2..4 | 17 | 5..15, 3 distinct 5×6, 10×5, 15×8 | CT率 per level (SETCH@0x909) |
| `G[0x5ba5e9]` | 10 | 2..9 | 48 | 1..1, 1 distinct 1×86 | element level per level: a weapon's adds to the unit's attack element level `G[0xc1ac6c]`, an armour's to its defense level `G[0xc1aca8]` (SETCH@0x9b3, SETEN@0xb2c) |
| `G[0x5d2c89]` | 1 |  | 19 | 1052..2811, 18 distinct | the item a fully grown one yields: RESULT@0xb54 and ALCHEMY@0x1df1 give it (ADDITEM) when the next level costs 0, and say "『…』を手に入れた" |
| `G[0x5d5399]` | 1 |  | 12 | 7113..7413, 11 distinct | a flag set when the item is fully grown: `G[0x929 + value − 1]` = 1 (RESULT@0xba5, ALCHEMY@0x1e75), a saved cell. GAMESTART@0xedf sets it again for a carried-over item at its last level |

### ALINIT (alchemy recipes)

`ALINIT` holds the recipes of the alchemy screen, `ALCHEMY`. Its leader is `G[0x42ab61]`, the item a recipe makes,
with a capacity of 1000. The seven arrays follow one another at the capacity's step (`+1000`, `+1000`, `+4000`,
`+2000`, `+1000`, `+4000`), and `IMINIT`'s first array, `G[0x42edc9]`, follows the last at `+4000`, so the layout is
the tool's. There are 231 recipes, ids 1 to 810: 171 from `ALINIT.BIN`, 2 from `$1$`, 2 from `$2$` and 56 from `$3$`.

`ALCHEMY` lists every recipe whose output is not 0 (`ALCHEMY@0x01590`). The output's `ITINIT` class
(`G[0x645469]`) picks the tab, through a map the script loads at `ALCHEMY@0x003ec` and reads at `ALCHEMY@0x015b4`:
classes 2 and 3 go to the first tab, 4 to 6 to the second, 7 to 10 to the third, 11 and 12 to the fourth, 13 to the
fifth and 1 (key items) to the sixth. The recipes fall 50, 78, 50, 8, 42 and 3 to those tabs. A seventh tab lists the
held growth items instead (`IMINIT`, `ALCHEMY@0x01659`), and takes nothing from this table. A recipe is listed only
when three tests pass:

- its party mask, ANDed with a mask of the party in play, is not 0 (`ALCHEMY@0x01a7b`). The party mask has bit
  `G[0x363c]` set (the party in play, 0 to 2) and bit 3 when `G[0x363d]` is set (`ALCHEMY@0x01562`..`0x0156e`);
- every story flag of its four required flags is set (`G[0x929 + flag - 1] == 1`, `ALCHEMY@0x01ab5`);
- neither of its two blocking flags is set (`ALCHEMY@0x01b0f`).

Crafting queues counts. Each craft reserves the ingredients from a copy of the inventory (`ALCHEMY@0x00b94`..`0x00bdd`)
and adds the price to the bill (`ALCHEMY@0x00b72`). A craft is refused when an ingredient's count exceeds what is held
minus the copies the party has equipped (`ALCHEMY@0x01b52`..`0x01bf0`), or when the output would reach 999
(`ALCHEMY@0x018ea`). At the commit the money in play loses the bill (`ALCHEMY@0x01bfe`..`0x01c13`), the inventory takes
the reserved counts (`ALCHEMY@0x01c7f`), and `ADDITEM` adds the outputs (`ALCHEMY@0x01d4d`..`0x01d69`).

The blocking flags appear only in recipes 601 to 603, which make the rods 47 to 49 (ミスリルの釣竿, 神木の釣竿,
魔神の釣竿). Flag `9001 + item` is set while a key item is held (`ADDITEM@0x0009b`), so each rod's recipe hides once
its rod is held. Recipe 602 also requires flag 9048 (rod 47 held), and 603 requires flag 9049. Among the required
flags, 9004 is item 3 (英傑の挑戦証) held, on 52 recipes. The flags 7101 to 7105, 7409, 7410, 100, 51, 81, 2907 and
8100 are not read.

No field is copied into saved state. The craft changes the inventory, the money and `G[0x26637a + party]`, a saved
total of the money spent on alchemy for parties 0 and 1 (`ALCHEMY@0x01c5c`..`0x01c78`). `ALCHEMY@0x03f24` tests that
total against 10000.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x42ab61]` | 1 |  | 231 | 47..2997, 228 distinct | the item the recipe makes (an `ITINIT` id). `ALCHEMY` lists the recipe on the output's tab, by its name and held count (`ALCHEMY@0x024ac`, `0x025b8`, `0x0260d`) |
| `G[0x42af49]` | 1 |  | 231 | 9..15, 4 distinct 9×2, 10×17, 13×28, 15×184 | the parties that may craft it: bit k for party k (0 to 2), bit 3 for the second party switch `G[0x363d]` (`ALCHEMY@0x01a7b`) |
| `G[0x42b331]` | 4 | 0..3 | 226 | 32..9075, 37 distinct | four story flags that must all be set (`ALCHEMY@0x01ab5`) |
| `G[0x42c2d1]` | 2 | 0 | 3 | 9048..9050, 3 distinct 9048×1, 9049×1, 9050×1 | two story flags, either of which hides the recipe (`ALCHEMY@0x01b0f`); written only for the rod recipes 601 to 603 |
| `G[0x42caa1]` | 1 |  | 231 | 10..500000, 75 distinct | the price of one craft. `ALCHEMY` shows it and its multiple by the count (`ALCHEMY@0x02621`, `0x02658`) and charges the sum at the commit |
| `G[0x42ce89]` | 4 | 0..3 | 231 | 101..2957, 241 distinct | four ingredients (`ITINIT` ids), 0 for none |
| `G[0x42de29]` | 4 | 0..3 | 231 | 1..10, 8 distinct | how many of each ingredient one craft takes (`ALCHEMY@0x00b94`, `0x01b52`) |

### SKINIT (the schools' skill trees)

`SKINIT` has no leader: it writes four arrays, which the scripts index by school and by action. A **school** is an
ITINIT class, `G[0x645469]`, of 21 or more (21 魔術・治癒 … 55 奥義・飛燕; the names are AFINIT's `GS[0xf674 + school]`).
Every school has a tree of 15 slots, 5 tiers by 3 lines: slot = 15 × school + 3 × tier + line. A slot holds an
action, an ITINIT id (3001 to 4265). The base data writes 825 cells, `$1$` none, `$2$` 31 (schools 29, 51 and 52) and
`$3$` 64 (schools 29, 31, 42, 48, 53, 54 and 55). 28 schools have a tree: 21 to 32, 35, 39, 41 to 49 and 51 to 55.
The arrays lie back to back from the end of ITINIT's last array (`G[0x9e2919]` + 10000) to `G[0x9f16fd]`, where
ILINIT's two argument cells begin. For every action in a tree, its ITINIT school equals its slot / 15.

The game learns actions through the tree. `CHECKSKILL@0x109`–`0x262` walks the schools 21 to 59 in which the
character has a level (`G[0x119eb2 + 60 × char + school]`, saved) and sets bit 0 (learned) of the slot's state
`G[0x13ee1a + 900 × char + slot]` (saved) when the slot has an action, the action's ITINIT rank `G[0x640649]` is at
most the school level, the character is on the action's ITINIT whitelist `G[0x756b69]` (100 cells; an empty list
allows anyone), the flag it needs is set, and its prerequisites are mastered. `CHECKSKILL@0x263` sets bit 1
(mastered) when the slot's proficiency `G[0x154daa + 900 × char + slot]` (saved) reaches 100, and shows
「<name>が『<action>』をマスターした」. `ADDSKILL@0x1c`–`0x12d` raises a school level (school in `G[0x269b2c]`, level
in `G[0x269b2d]`) and gives bit 0 to each slot whose action passes the checks, and bit 1 too when its rank is below
the new level. After an action is used in battle, `BTL@0x4936`–`0x49ba` raises its proficiency by max(1, level − rank
+ 1) unless it is mastered, then runs `CHECKSKILL`. `CHECKACT@0x7eb` lets a character use an action only when its slot
has bit 0. `CHMODE3@0x2471`–`0x2517` draws the tree: 5 tiers across (x = 0x2e + 190 × tier) by 3 lines down (y = 0x1ab +
27 × line); the same action in consecutive slots of a tier is one box 1 to 3 lines wide. `CHMODE3@0x2563` draws the
links to the prerequisites, and `@0x2b2c` tests the flag.

No script copies an SKINIT value into saved state, so boot rebuilds the trees. The learned and mastered bits and the
proficiency are kept **by slot**, not by action: a mod that moves an action to another slot leaves an old save's bits
and proficiency on the old slot, which now names another action.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x9e5029]` | 15 | 0..14 | 28 | 0..4265, 288 distinct | a school's tree: the action (ITINIT id) in each slot, 3 × tier + line; 0 for an empty slot (71 of the 370 cells). `CHMODE3@0x2471`, `RTN051@0x73` |
| `G[0x9e53ad]` | 1 |  | 287 | 315..837, 287 distinct | keyed by action: its slot, the inverse of the tree (the first slot of a wide box). `ADDSKILL@0x213`, `CHECKSKILL@0x222`, `CHMODE3@0x1b91`, `BTL@0x4936`, `CHECKACT@0x7eb` |
| `G[0x9e7abd]` | 3 | 0..2 | 217 | 3001..4243, 196 distinct | keyed by action: up to three actions that must be mastered (bits 0 and 1) before it can be learned. `ADDSKILL@0x1f1`, `CHECKSKILL@0x200`, `CHMODE3@0x1b6f` and `@0x2563` (the links) |
| `G[0x9eefed]` | 1 |  | 26 | 7106..9087, 17 distinct | keyed by action: a story flag it needs, set when `G[0x929 + flag − 1]` is 1. `ADDSKILL@0x1a2`, `CHECKSKILL@0x1b1`, `CHMODE3@0x1b20` |

`GAMESTART@0xca7` and `GAMECLEAR@0x216` show `copy-to-global (global-int 0x9e53ad) 0x3c`, but the loops after them
index a 60-cell local array (`GAMESTART` local `0x408` at `@0xd0a`, `GAMECLEAR` local 4): the listing names a local as
this global. They are not readers of `SKINIT`.

### SSINIT (skills)

Leader `G[0x9f3d71]` (the kind), capacity 600, ids 1 to 524. 123 skills: 120 in the base data and 3 in `$3$` (439
師弟の絆, 457 神秘の力, 458 先史技術). A skill has up to five levels, each with its own name: 貫通Ⅰ to 貫通Ⅴ. The
current leader `G[0x9f3411]` (the sort key) is unwritten on 8 records (310 女神化Ⅰ, 379 飛竜使い, 422, 424, 425, 427,
428 and 433), so the tool counts 115 records; with the kind as leader it finds all 123 and puts the level arrays on
the grid. The arrays lie back to back from `G[0x9f31b9]` to `G[0xa26f69]`; RTINIT starts at `G[0xa26f6c]`. The two
cells before the table, `G[0x9f31b7]` and `G[0x9f31b8]`, are the skill and the level (0 to 4) that `SSMES` describes
(`SSMES@0x0`, `CHMODE2@0x3552`).

A character's base level of skill s is `G[0x12195a + 600 × char + s]` (saved): `SETSS@0xb` sets it from
`G[0x269b2c]` and `G[0x269b2d]`. `CALCREVISE` builds the effective level `G[0xc0bc11 + 600 × char + s]` from the base
level, a class's skills (`G[0xaadbad + 600 × G[0x1218f6 + char] + s]`) and the equipped items' ITINIT `G[0x9e0209]`
(skill) and `G[0x9e2919]` (level) (`CALCREVISE@0x175`–`0x1ac`), clamps it to 0..5, lowers it to the highest level that
has a name (`CALCREVISE@0x2a5`, `SETCH@0xc2b`), and sets it to 0 when the player switched the skill off
(`G[0x1303ba + 600 × char + s]`, saved) and the skill can be switched off (`CALCREVISE@0x302`–`0x344`,
`SETCH@0xca3`–`0xce5`). In battle a unit's levels are `G[0xc1ca4e + 600 × unit + s]`, and `CHECKSS` keeps the skills
that trigger this action in `G[0x276f00 + 600 × side + s]`.

The **kind** says when a skill acts. Kind 1 (14 skills) are the attacker's: `CHECKSS@0x95` drops them for the
defender, and `CHECKSS4@0x65` clears them when the defender triggered 101 反射, 102 見切り, 103 カウンター or 106
神秘の防護. Kind 2 (6) are the defender's (`CHECKSS@0xbd`). Kind 4 (54) are passive: `CALCREVISE@0x347`–`0x3af`,
`SETCH@0xce8`–`0xd50` and `SETEN@0xe8a` add the skill's condition row to the unit's lasting conditions; 358 即死無効
to 372 全無効 are kind 4 and block a condition (`CALCILL@0x2bf`, `ADDILL@0x7d`). Kind 6 (17) are support skills that
the characters in `G[0x36f9fd]` (two cells after the five fighters of `G[0x36f9f8]`; inferred to be the reserve) lend
the party: `BTL@0x83b4`–`0x845b` keeps the
party's highest level of each in `G[0x27b550]`, and `BTL@0x2353`–`0x2625` runs them on the battle clock. Kinds 3 (5:
先手, 戦闘指揮, 豹変, 俊足転移, 好奇心) and 5 (27: 嫌い, 絆, 魔剣解放, 魔導鎧 …) are not confirmed by a consumer here.

A triggered skill pays its costs: `CHECKSS@0x30d`–`0x362` keeps it only when HP after the HP cost stays at least 1
and MP and TP plus the costs stay at least 0; `CHECKSS@0x4c3`–`0x51c` takes a defender's costs at once and sums an
attacker's in `G[0x276e44..46]`, which `COMMITSS@0xd4`–`0xf4` takes. `COMMITSS@0x86`–`0x13f` and `@0x1ed`–`0x2a4`
take back an attack-only skill and refund it when the action did not land (`G[0x276e92]` 0) or the action is 4033 or
4126. When a skill with an effect triggers, `BTL@0x3e47`–`0x3e98` (side 1) and `COMMITSS@0x355`–`0x3bf` (side 2) put
the effect in `G[0xc259fc + unit]`, and `SETEFFECT@0x9b`–`0xae` hands it to the BTANINIT chain `G[0xc78410]` as
`G[0xc25962]`. `INFO6@0x7a5`–`0x88a` lists the skills in sort order and shows one only when its first scene is 0 or
one of its scenes is set in `G[0x25febb]`, the shared profile's 10000 seen-scene cells (`INFO6@0x86b` loads it;
`$1$SC0630@0x610`–`0x623` sets one and stores the profile).

Saved state: `CALCREVISE@0x15`–`0x2c` zeroes the character's lasting-condition row `G[0x11e75a + 60 × char]` (saved)
and rebuilds it from the kind-4 skills' condition rows and the items' `G[0x904669]`. A changed table therefore reaches
an old save the next time `CALCREVISE` runs. No other SSINIT value is copied below `0x268b19`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x9f31b9]` | 1 |  | 3 | 3..3, 1 distinct 3×3 | the append whose name-plate sheet holds the skill; written before the record's first field. `BTL@0x91cf`–`0x9238` copies a kind-1 skill's 154 × 18 plate at ((id − 1) / 20 × 154, (id − 1) % 20 × 18) from surface `0xac` + this value, which holds the SO036 sheet of that slot of `G[0xc781f0]` |
| `G[0x9f3411]` | 1 |  | 115 | 10..5170, 115 distinct | a sort number, in steps of ten. `sort-indices-by-key-sum` at `CHECKSS@0x274`, `COMMITSS@0x2a`, `INFO6@0x7a5`, `BTL@0x3e15` |
| `G[0x9f3669]` | 3 | 0 | 25 | 81..8124, 16 distinct | up to three scenes (scene id + 1) that unlock the skill's entry in `INFO6` (`@0x7e5`–`0x88a`); only column 0 is written |
| `G[0x9f3d71]` | 1 |  | 123 | 1..6, 6 distinct 1×14, 2×6, 3×5, 4×54, 5×27, 6×17 | the kind: 1 the attacker's, 2 the defender's, 4 passive, 6 support (above); 3 and 5 not read here |
| `G[0x9f3fc9]` | 1 |  | 18 | 1..1, 1 distinct 1×18 | 1: the skill acts only on an attack, an action whose ITINIT `G[0x647b79]` is 1 or 2 (`CHECKSS@0xe2`), and is refunded when it does not land (`COMMITSS@0x86`) |
| `G[0x9f4221]` | 1 |  | 18 | 501..1208, 11 distinct | a BTANINIT effect id played when the skill triggers (`BTL@0x3e47`, `COMMITSS@0x355`, `SETEFFECT@0x9b`) |
| `G[0x9f4479]` | 1 |  | 20 | 1..2, 2 distinct 1×10, 2×10 | whose unit shows the effect: 1 the attacker (`BTL@0x3e47`), 2 the defender (`COMMITSS@0x355`) |
| `G[0x9f46d1]` | 5 |  | 0 |  | the HP cost of each level (`CHECKSS@0x30d`, `COMMITSS@0xd4`); no record writes it |
| `G[0x9f5289]` | 5 | 0..4 | 3 | -34..-6, 12 distinct | the MP cost of each level, negative (即死 −10 to −30). `CHECKSS@0x334`, `CHMODE2@0x2c72`, `COMMITSS@0xf4` |
| `G[0x9f5e41]` | 5 | 0..4 | 5 | -30..-5, 11 distinct | the TP cost of each level, negative (貫通 −10 to −30). `CHECKSS@0x362`, `CHMODE2@0x2cc4` |
| `G[0x9f69f9]` | 5 | 0..4 | 17 | 3..100, 19 distinct | the trigger chance in percent: `CHECKSS@0x148`–`0x1b5` drops the skill when random(100) is at least this, unless `G[0xc0ba06]` is set. Also `BTL@0x427` |
| `G[0x9f75b1]` | 5 | 0..4 | 55 | -5..1000, 36 distinct | the skill's amount, each skill's own: 貫通 50; 祈り the percent of maximum HP it heals (`BTL@0x2413`); 応援 what it adds to `G[0xc1a960]` (`BTL@0x24d2`); 魔術結界 the percent of magic damage a party unit takes (`SETSSDAMAGE@0x1e7`); a support action's power (`BTL@0x25b6`). Read in more places than read here (`BTL` ×15, `SETSSBONUS@0x24`, `CALCDAMAGE@0x5a0`, `CHECKACT@0x173`, `SETILLREVISE@0x5dd` …) |
| `G[0x9f8169]` | 5 | 0..2 | 17 | 1..2, 2 distinct 1×27, 2×20 | a support action's mode (kind 6): 1 an attack whose power comes from the supporter's `G[0x1196e2]` and the amount (`BTL@0x2575`–`0x2605`), 2 the amount negated (a heal) (`BTL@0x260b`) |
| `G[0x9f8d21]` | 5 | 0..4 | 33 | 1..1, 1 distinct 1×75 | 1: the player cannot switch the skill off. `CHMODE2@0x1310` refuses the toggle; `CALCREVISE@0x302` and `SETCH@0xcb5` ignore the switch |
| `G[0x9f98d9]` | 300 | 1..279 | 5 | 1..5, 5 distinct | the condition levels a level gives, 60 ILINIT conditions a level: cell 300 × id + 60 × level + condition. Read for kind 4 only (`CALCREVISE@0x392`, `SETCH@0xd33`, `SETEN@0xe8a`): ＨＰ再生 再生 1 to 5, ＭＰ再生 魔再, ＴＰ再生 技再, 努力家 経験. 即死Ⅰ's row (即死 1) is kind 1 and never read |
| `G[0xa257f9]` | 5 | 0..2 | 9 | 9980..9991, 9 distinct | the ITINIT action a support skill performs (水弾支援 9991, 自動回復 9983): `BTL@0x2527`–`0x2570` puts it in `G[0x276e55]` |
| `G[0xa263b1]` | 5 | 0..2 | 11 | 10..100, 9 distinct | a support skill's period: it acts when the battle clock `G[0x276e4d]` is a multiple of it (`BTL@0x2393`–`0x23c5`) |
| `GS[0x811c]` | 5 | 0..4 | 123 | 貫通Ⅰ / 貫通Ⅱ / 貫通Ⅲ | the name of each level; a level with no name does not exist (`CALCREVISE@0x2a5`). Also `CHMODE2`, `CHMODE4`, `DRAWCHP`, `DRAWENP`, `INFO4`, `INFO6`, `SETEN` |
| `GS[0x8cd4]` | 35 | 0..29 | 123 | 【<貫通,かんつう>Ⅰ】　攻撃時に確率で発動し… | the description of each level, 7 lines a level: 35 × id + 7 × level + line. No script reads it: `SSMES` prints its own copy of the text, chosen by `G[0x9f31b7]` and `G[0x9f31b8]`, and `DRAWSSMES` runs the SSMES chain `G[0xc78210]` |

The tool's layout differs in five places. Its leader misses 8 records (above). It puts three level arrays 4 or 8
cells early (`G[0x9f69f5]`, `G[0x9f75a9]`, `G[0x9f8d19]` †): their bases are `G[0x9f69f9]`, `G[0x9f75b1]` and
`G[0x9f8d21]`, which the scripts index. Its `G[0xa0cb8f]` (stride 40 †) and the `G` cells it leaves outside
(`G[0x9f9b32]` …, 即死Ⅰ's condition 1) are the condition array `G[0x9f98d9]`, 300 cells an id. Its `GS[0x8cde]`,
`GS[0x9080]` and `GS[0xb0d0]` and the `GS` cells it leaves outside are one description array, `GS[0x8cd4]`, 35 cells
an id (`GS[0x811c]` + 3000). With these every image cell is on the grid.

### ILINIT (conditions)

Leader `G[0x9f3103]` (the polarity), capacity 60, ids 1 to 45, one layer (base). 45 conditions, five levels each, with
a name for each level (呪い1 to 呪い5; 即死, 喪意 and 石化 have one). 40 records write fields; 4 HP吸, 5 MP吸, 6 TP吸,
35 蘇生 and 36 経験 write only names. The arrays lie back to back: 22 arrays of 5 cells an id from `G[0x9f16ff]`, then
4 of one cell from `G[0x9f30c7]`, ending at `G[0x9f31b7]`, where SSINIT's argument cells begin. The two cells before
the table are its arguments: `G[0x9f16fd]` a condition and `G[0x9f16fe]` the levels to add (negative to take away).
A map event sets them and calls `ADDILL` (`MP06F0101@0x181`: 13 猛暑, −5, for the whole party).

A character's condition levels are `G[0x11cfea + 60 × char + condition]` (saved) and their durations
`G[0x11feca + 60 × char + condition]` (saved). Lasting levels from skills and items are the floor
`G[0x11e75a + 60 × char + condition]` (saved, rebuilt by `CALCREVISE`). `ADDILL@0x6f`–`0x2d4` refuses a positive
amount when the character's effective level of a blocking skill is above 0 (372 全無効 for all, 358 即死無効 for 1 …
371 恐怖無効 for 19; 361 吸収無効 for 4 to 6). `ADDILL@0x2d5`–`0x43a` adds the amount, keeps the level between the
floor and 5, and sets the duration: 0 at level 0, −1 at the floor, else the level's duration (on a cut, no more than
it). Taking 呪い away also unequips the items whose ITINIT `G[0x85bf19]` is set (`ADDILL@0x43f`–`0x540`). In battle
the same happens on the unit's `G[0xc1adb6]` (level), `G[0xc1b4be]` (floor) and `G[0xc1bbc6]` (duration)
(`CALCILL@0x9b9`–`0xb35`, the duration indexed by the level above the floor).

An attack inflicts the levels in its action's ITINIT row `G[0x871ea9 + 60 × action + condition]`
(`CALCILL@0xa7`–`0xc9`). `CALCILL@0x2ac`–`0x75a` checks the defender's blocking skills, then rolls: unless the
action's ITINIT `G[0x860d39]` is 1 (a sure hit), a condition with the boss-proof mark fails on a unit whose
`G[0x37989f + G[0xc1a8ca + unit]]` is set, and the chance is (the action's stat, `G[0x9af4c9]` for a mental condition or `G[0x9acdb9]` for
a physical one, + the attacker's mental or physical resistance + random(100) + 1) × the defender's affinity for the
condition's column / 100 − the defender's resistance. The affinity row is AFINIT's `G[0xc0b56d + 20 × element +
column]`, the element `G[0xc1ac8a + unit]`, which 321 地の守護者 to 332 創造の守護者 set (`SETILLREVISE@0x408`).
`CALCILL@0x75d`–`0x9b5` then acts on some conditions at once: 即死 sets HP to 0, 喪意 sets `G[0xc1a960]` to 0, 石化
stops a party unit and kills an enemy, 魅了 stops the unit (`CANCELACT`), 混乱 breaks an action of a school above 40
and 沈黙 one of schools 21 to 40 (`BREAKACT`), and 蘇生 revives with its level fifths of the maximum HP. 破術 (skill
7) takes one level of every beneficial condition away (`CALCILL@0x34`–`0x82`; `CHECKSS2@0xe3`–`0x155` triggers it
only on such a condition).

`SETILLREVISE@0x5a`–`0x3c3` sums each modifier array over the unit's conditions at their levels and applies the sums
to the battle stats: the percent arrays scale them, the others add. The stat each array changes is named here by the
condition that writes only that array (命中 writes only the hit modifier), so the stat names are inferred
(confidence med). `FIELD@0x169c`–`0x1797` adds the HP, MP and TP changes to the saved `G[0x11980e]`, `G[0x1198d6]` and
`G[0x11999e]` on every call (a step on the map), counts the duration down and drops the level to the floor at 0.

Saved state: `ADDILL@0x3ed`–`0x43a` copies the level's duration into the saved `G[0x11feca]`. A changed duration
reaches an old save only when the condition is next applied. No other ILINIT value is copied below `0x268b19`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x9f16ff]` | 5 | 0..4 | 40 | 3..4, 2 distinct 3×67, 4×121 | which resistance a level is rolled against: 3 mental (`G[0xc1ab22]`), 4 physical (`G[0xc1ab04]`); any other value is not rolled and not kept (`CALCILL@0x523`–`0x54e`, `@0x630`–`0x733`) |
| `G[0x9f182b]` | 5 | 0..4 | 40 | 11..17, 7 distinct | the column of the defender's AFINIT affinity row that scales the chance (`CALCILL@0x5ec`–`0x6cf`) |
| `G[0x9f1957]` | 5 | 0..4 | 38 | -1..300, 10 distinct | the duration of each level, counted down by `FIELD@0x1756` on the map; −1 lasts until cured. `ADDILL@0x3ed`, `CALCILL@0xae3`, `BTL@0x583` ×14, `DRAWENLIST@0x891` |
| `G[0x9f1a83]` | 5 | 0..4 | 3 | -30..30, 11 distinct | hit (命中, 命低, 呪い): added to `G[0xc1acc6]` (`SETILLREVISE@0x198`) |
| `G[0x9f1baf]` | 5 | 0..4 | 4 | -50..30, 13 distinct | evasion (回避, 回低, 麻痺, 呪い): added to `G[0xc1ace4]` (`SETILLREVISE@0x1af`) |
| `G[0x9f1cdb]` | 5 | 0..4 | 4 | -50..30, 10 distinct | physical attack in percent (物攻, 攻低, 高揚, 衰弱): scales `G[0xc1aa50]` (`SETILLREVISE@0x9b`, `@0x22d`) |
| `G[0x9f1e07]` | 5 | 0..4 | 4 | -100..30, 16 distinct | physical defence in percent (物防, 防低, 高揚, 衰弱): scales `G[0xc1aa6e]` (`SETILLREVISE@0xb2`) |
| `G[0x9f1f33]` | 5 | 0..4 | 1 | 1..5, 5 distinct | attack count (攻回): added to `G[0xc1aac8]` (`SETILLREVISE@0xf7`) |
| `G[0x9f205f]` | 5 | 0..4 | 1 | 1..5, 5 distinct | defence count (防回): added to `G[0xc1aae6]` (`SETILLREVISE@0x10e`) |
| `G[0x9f218b]` | 5 | 0..4 | 4 | -50..30, 10 distinct | magic attack in percent (魔攻, 攻低, 高揚, 衰弱): scales `G[0xc1aa8c]` (`SETILLREVISE@0xc9`) |
| `G[0x9f22b7]` | 5 | 0..4 | 4 | -100..30, 16 distinct | magic defence in percent (魔防, 防低, 高揚, 衰弱): scales `G[0xc1aaaa]` (`SETILLREVISE@0xe0`) |
| `G[0x9f23e3]` | 5 | 0..4 | 1 | 1..5, 5 distinct | magic power (魔威): added to `G[0xc1ad20]` (`SETILLREVISE@0x1dd`) |
| `G[0x9f250f]` | 5 | 0..4 | 1 | 10..30, 5 distinct | physical resistance (肉抵): added to `G[0xc1ab04]` (`SETILLREVISE@0x125`) |
| `G[0x9f263b]` | 5 | 0..4 | 1 | 10..30, 5 distinct | mental resistance (精抵): added to `G[0xc1ab22]` (`SETILLREVISE@0x13c`) |
| `G[0x9f2767]` | 5 | 0..4 | 4 | -20..10, 13 distinct | physical speed (肉速, 速低, 束縛, 呪い): added to `G[0xc1ab40]` (`SETILLREVISE@0x153`) |
| `G[0x9f2893]` | 5 | 0..4 | 4 | -20..10, 13 distinct | mental speed (精速, 速低, 束縛, 呪い): added to `G[0xc1ab5e]` (`SETILLREVISE@0x16a`) |
| `G[0x9f29bf]` | 5 | 0..4 | 1 | 2..10, 5 distinct | luck (運): `SETILLREVISE@0x181` sums it but applies the sum to nothing |
| `G[0x9f2aeb]` | 5 | 0..4 | 3 | -10..10, 10 distinct | critical hits (痛打, 高揚, 呪い): added to `G[0xc1ad02]` (`SETILLREVISE@0x1c6`) |
| `G[0x9f2c17]` | 5 | 0..4 | 7 | -16..5, 17 distinct | HP each turn (猛毒 −1 to −12, 再生 +1 to +5 …): added to `G[0xc1ad3e]` in battle (`SETILLREVISE@0x1f4`), to the saved HP on the map (`FIELD@0x16dd`) |
| `G[0x9f2d43]` | 5 | 0..4 | 2 | -3..5, 8 distinct | MP each turn (魔再, 衰弱): `G[0xc1ad5c]` (`SETILLREVISE@0x206`), the saved MP (`FIELD@0x16fd`) |
| `G[0x9f2e6f]` | 5 | 0..4 | 2 | -3..5, 8 distinct | TP each turn (技再, 衰弱): `G[0xc1ad7a]` (`SETILLREVISE@0x218`), the saved TP (`FIELD@0x171d`) |
| `G[0x9f2f9b]` | 5 | 0..4 | 3 | -10..-1, 8 distinct | what 強酸, 恐怖 and 衰弱 take from the unit's `G[0xc1a960]` every 10 clock ticks, to no less than 1 (`BTL@0x211c`–`0x218a`) |
| `G[0x9f30c7]` | 1 |  | 38 | 3..30, 16 distinct | the condition's icon; conditions that share one are drawn once (`DRAWENLIST@0x7d`–`0xd5`) |
| `G[0x9f3103]` | 1 |  | 40 | 1..2, 2 distinct 1×19, 2×21 | the polarity: 1 beneficial, which 破術 removes (`CALCILL@0x63`, `CHECKSS2@0x117`); 2 an ailment, which marks the character on the map (`DRAWMAPF@0xd1`) |
| `G[0x9f313f]` | 1 |  | 14 | 1..1, 1 distinct 1×14 | 1: fails on a unit whose `G[0x37989f]` is set (`CALCILL@0x5cf`); inferred to be boss-proof (confidence med) |
| `G[0x9f317b]` | 1 |  | 35 | 2..3, 2 distinct 2×21, 3×14 | which cures clear it, as bits: bit 1 on a knock-out (`BTL@0x5d41`) and by `RECOVER` (`@0x6b`); bit 0 by `RECOVER` when `G[0x36e5]` is set, which clears only those |
| `GS[0x7d98]` | 5 | 0..4 | 45 | 即死 / 喪意 / 石化 | the name of each level (呪い1 to 呪い5), read by `DRAWCARD@0x36a`, `DRAWCHP@0x1719`, `DRAWENLIST@0x9e8`, `DRAWENP@0x618` |

The tool finds 22 arrays and leaves 25 cells outside. The 25 are five arrays of one record each, which it does not
form: `G[0x9f1f33]` (攻回), `G[0x9f205f]` (防回), `G[0x9f23e3]` (魔威), `G[0x9f250f]` (肉抵) and `G[0x9f263b]` (精抵).
Its three † arrays are three more: `G[0x9f2a32]` (stride 32) is `G[0x9f29bf]` (運), and `G[0x9f2ddb]` and
`G[0x9f2ee2]` are `G[0x9f2d43]` and `G[0x9f2e6f]`. All eight are 5 cells an id on the grid, and `SETILLREVISE` and
`FIELD` index them at these bases. With them every image cell is on the grid. The leader is right.

### AFINIT (elements, affinities, classes and formations)

AFINIT holds constants in four parts, each with its own capacity, so no leader fits the whole table:

- **The element names.** `GS[0xf64c + e]` names the attack elements 1..17, and `GS[0xf660 + e]` names the defense
  elements 1..15, 20 cells each. The two lists name 1..9 alike, and the matrix pairs them (火炎, 冷却, 電撃 and 地脈
  take -100 of their own attack element), so attack element n is element n up to 9, the registry's table `element`.
  The attack elements above 9 are no element: 10 戦意 is the attack element of every 戦意 attack (ITINIT damage kind
  5, 36 items), and 11..17, 抵抗1..7, are the columns a condition's chance is scaled by, 抵抗3 being also the attack
  element of seven mind checks (damage kind 3).
- **Row 0 and column 0** are 100 throughout, and the game names neither. Row 0 is used: a battle unit's defense
  element is its armour's, else its unit's (`SETCH@0xa82`, `SETEN@0xbfb`), and 131 units have none of their own, nor
  does the armour they start with give one (ハイシェラ and シュリ among the party, soldiers among the enemies); such a
  unit takes 100 of every column, where 無属 takes 50 of 戦意 and 抵抗1..5. Column 0 is not: CALCDAMAGE takes the
  action's attack element, else the attacker's `G[0xc1ac4e]` (`CALCDAMAGE@0x1a2`), which only SETCH and SETEN write,
  the weapon's element or 2 物理 without a weapon (`SETCH@0x9d9`, `SETEN@0xb52`), and every weapon has one. The
  registry names column 0 `undefined`, and the element kind titles row 0 "Undefined". The game draws nothing for an
  element of 0: the status screen (`DRAWCHP@0xd69` for the attack element, `0xdf8` for the defense element), the
  battle panel (`DRAWENP@0x45a`) and the bestiary (`INFO4@0x108b`) skip it, and the item tooltip, which shows an
  item's attack element, else its defense element, writes "－－" for an empty name (`DRAWTIP@0x5a7`–`0x5eb`).
- **The icons.** The status screen draws an element's 12 × 14 icon beside its name from SO011.AGF (resource
  0x3335, which CHMODE, INFO and the other menus load into slot 0x7b): defense element n at 12 × ((n − 1) mod 9),
  84 + 14 × ((n − 1) div 9) (`DRAWCHP@0xe06`–`0xe37`), attack element n at 12 × (n − 1), 84 (`DRAWCHP@0xd77`–
  `0xd85`). The row at y 84 holds 無属 to 暗黒 and 戦意's icon; 抵抗1..7 have none.
- **The affinity matrix.** `G[0xc0b56d + 20·def + atk]` is 20 × 20. AFINIT@0xa0 writes rows 0..15 with
  `copy-inline-int-array`, and `G[0xc0b6fd]` follows at +400.
- **The classes**, 1..55 at capacity 60. Six arrays lie 60 apart; `G[0xc0b7b1]` takes 7 cells a class, and
  `G[0xc0b955]` follows it at +420.
- **The formations**, 1..15 at capacity 16: `GS[0xf728]`, and `GS[0xf738]` at 14 cells a formation.

The base data has 38 classes. `$2$AFINIT` adds 51 聖剣 and 52 斬撃, `$3$AFINIT` adds 53 接技, 54 体術 and 55 奥飛, and
`$1$AFINIT` is empty (530 integer and 189 string cells in the base data; 20 and 4 in `$2$`; 29 and 6 in `$3$`).

The tool's leaderless view merges arrays that lie back to back. It shows `GS[0xf674]`, `GS[0xf6b0]` and `GS[0xf6ec]`
as one 94-id array, and `G[0xc0b6fd]` with `G[0xc0b739]` as one 86-id array; the table below splits them.
`Leader("GS", 0xf674, 60)` splits the class arrays right (43 classes), but it cuts the matrix and the element names
into † fragments, so the table keeps no leader.

The affinity is a percentage of the damage, row the defender's element and column the attacker's. CALCDAMAGE@0x1d2
reads it. CALCDAMAGE@0x179..0x20c adds 20 for each level the attack element has over the defense element. A
negative percentage makes the damage negative (CALCDAMAGE@0x9e9): fire armour against fire is −100 (inferred: it
heals). CALCILL@0x618 and @0x6c4 read the same matrix for a condition, its column taken from `G[0x9f182b]`. BTL,
CHECKACT@0x3f2, INFO5@0xc31 and thirteen RTN scripts read it too. The matrix:

| defense \ attack | 無属 | 物理 | 万能 | 火炎 | 冷却 | 電撃 | 地脈 | 神聖 | 暗黒 | 戦意 | 抵抗1 | 抵抗2 | 抵抗3 | 抵抗4 | 抵抗5 | 抵抗6 | 抵抗7 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 無属 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 2 物理 | 100 | 100 | 120 | 110 | 125 | 100 | 100 | 110 | 135 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 3 万能 | 100 | 70 | 100 | 90 | 100 | 90 | 110 | 90 | 100 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 4 火炎 | 100 | 65 | 100 | -100 | 150 | 100 | 100 | 100 | 105 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 5 冷却 | 100 | 65 | 100 | 150 | -100 | 100 | 100 | 100 | 105 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 6 電撃 | 100 | 65 | 100 | 100 | 100 | -100 | 150 | 85 | 85 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 7 地脈 | 100 | 65 | 100 | 100 | 100 | 150 | -100 | 85 | 100 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 8 神聖 | 100 | 90 | 100 | 60 | 50 | 30 | 100 | 60 | 100 | 25 | 25 | 25 | 25 | 25 | 25 | 75 | 100 |
| 9 暗黒 | 100 | 90 | 100 | 65 | 100 | 75 | 100 | 100 | 30 | 25 | 25 | 25 | 25 | 25 | 25 | 100 | 100 |
| 10 神格 | 100 | 60 | 95 | 75 | 75 | 75 | 75 | 75 | 90 | 0 | 25 | 0 | 0 | 25 | 25 | 75 | 100 |
| 11 魔神 | 100 | 50 | 90 | 50 | 50 | 50 | 50 | 100 | 100 | 0 | 25 | 0 | 0 | 25 | 25 | 100 | 100 |
| 12 反万 | 100 | 100 | 1 | 100 | 100 | 100 | 100 | 100 | 50 | 50 | 50 | 50 | 50 | 50 | 50 | 100 | 100 |
| 13 霊体 | 50 | 1 | 35 | 70 | 50 | 85 | 1 | 200 | -100 | 0 | 0 | 0 | 50 | 0 | 0 | 0 | 100 |
| 14 不死 | 50 | 25 | 60 | 125 | 25 | 50 | 25 | 150 | -100 | 0 | 0 | 0 | 0 | 0 | 50 | 100 | 100 |
| 15 創造 | 50 | 100 | 100 | 25 | 1 | 150 | 1 | 50 | 50 | 0 | 0 | 0 | 0 | 0 | 50 | 100 | 100 |

A character's rank in a class, `G[0x119eb2 + 60·char + class]` (1 to 7, E to M), and the class exp,
`G[0x11b622 + 60·char + class]`, are saved cells. BTL@0x4894..0x4902 adds a battle's exp to the class exp
(`5·G[0x1196e2 + char]` plus a random 0..99) and compares it with `G[0xc0b7b1 + 7·class + rank − 1]`. CHMODE2@0x18b2
and CHMODE3@0x1e06 show the next threshold. A changed threshold reaches an old save; a rank already reached stays.

The formation of the current party is the saved `G[0x266374 + G[0x363c]]`. CHMODE4@0x257e prints its name, and
CHMODE4@0x2714 prints its fourteen effect lines in two columns of seven. AFINIT holds only the text. This reading did
not trace where the battle applies a formation's numbers.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc0b56d]` | 20 | 0..17 | 16 | -100..200, 22 distinct | the affinity matrix above: row = defense element, column = attack element, a percentage of the damage (CALCDAMAGE@0x1d2, CALCILL@0x618) |
| `G[0xc0b6fd]` | 1 |  | 43 | 3..60, 38 distinct | the class's icon, given as an ITINIT category (重要 26, 小型 3, 治癒 36). DRAWCHP@0xf99 draws it from the category icon grid, ((v−1)%10·12, (v−1)/10·14); CHMODE2@0x280f and RESULT@0x13ff read it too |
| `G[0xc0b739]` | 1 |  | 43 | 10..990, 43 distinct | the class's place in the class list, in steps of ten (CHMODE3@0xf9c copies all 60 for its list) |
| `G[0xc0b775]` | 1 |  | 10 | 64..524288, 8 distinct | the weapons an art school requires: one bit per ITINIT category (剣 0x40, bit 6; 弓 0x1000, bit 12; 魔装 0x80000, bit 19). CHECKACT@0x920 tests the unit's weapon's category against it, and CHMODE3@0x1891 shows it. 一撃 and 破撃 hold 0xe00 (双剣, 大剣, 槍), where their note says 「連接」「双剣」「大剣」 |
| `G[0xc0b7b1]` | 7 | 0..5 | 30 | 10000..400000, 39 distinct | the class exp a character needs to pass each rank, by rank − 1 (column 0 E to D … column 5 S to M; BTL@0x48d2, CHMODE2@0x18b2, CHMODE3@0x1e06) |
| `G[0xc0b955]` | 1 |  | 5 | 2..3, 2 distinct 2×2, 3×3 | the append that adds the class (51..55). CHMODE3@0x1ff7 lists such a class only while `G[0xc781d0 + append]` is set (inferred: that append's slot of a hook chain, filled when it is installed) |
| `G[0xc0b991]` | 1 |  | 10 | 1..3, 3 distinct 1×3, 2×5, 3×2 | a weapon category's flags, indexed by ITINIT category 1..12. Bit 0, on 連接, 双剣, 大剣, 槍 and 弓: equipping the weapon first takes off the item in the character's `G[0x11cebe]` slot (CHMODE1@0x1083 and its sub at 0x1e61, which refuses a cursed one, "は外すことが出来ない…"; inferred: two-handed, that slot the shield). Bit 1, on 杖, 短剣, 突剣, 剣, 拳, 双剣 and 大剣: CALCDAMAGE@0x8e2 halves the damage when another unit stands between attacker and target (inferred: a melee weapon) |
| `GS[0xf64c]` | 1 |  | 17 | 無属 / 物理 / 万能 | the attack element names, 1..17 (DRAWTIP@0xb1, DRAWCHP@0xdcf) |
| `GS[0xf660]` | 1 |  | 15 | 無属 / 物理 / 万能 | the defense element names, 1..15 (DRAWTIP@0xd3, DRAWCHP@0xe81, DRAWENP@0x489, INFO4@0x10ba) |
| `GS[0xf674]` | 1 |  | 43 | 重要アイテム / 通常アイテム / 非戦闘時アイテム | the class's name (小型武器, 魔術・治癒, 必殺・飛燕; CHMODE2@0x28b6, CHMODE3@0x1fe9) |
| `GS[0xf6b0]` | 1 |  | 43 | 重要 / 物品 / 非戦 | its short name; no script reads it |
| `GS[0xf6ec]` | 1 |  | 8 | 「盾」装備不可 / 「剣」装備必須 / 「連接」「双剣」「大剣」装備必須 | its equipment note (「盾」装備不可 on 大型, the weapon an art school needs, 「魔法剣」スキル装備必須 on 魔剣); no script reads it |
| `GS[0xf728]` | 1 |  | 15 | 基本陣形 / 攻撃陣形 / 防御陣形 | the formation names (1..8, then 9..15 ？？陣形; CHMODE4@0x257e) |
| `GS[0xf738]` | 14 | 0..8 | 15 | 効果なし / 物理攻撃up / 物理防御up | a formation's effect lines, two columns of seven (CHMODE4@0x2714 draws line n at x 0x31 + (n / 7)·0xf7) |

### TRINIT (treasure contents)

`TRINIT` holds what a treasure gives: up to five slots, each an item and a count, or money. It has no leader. The
scripts index two arrays of 5 cells a treasure, `G[0x6253fc]` and `G[0x63174c]`, 50000 cells apart, which is 10000
treasures of 5 slots; the selector `G[0x6253fb]` is the cell before the first. The tool's bases are the ones the
scripts index. There are 630 treasures: 574 from `TRINIT.BIN` (ids 1 to 3216, in the hundreds 0 to 17 and 30 to 32),
20 from `$1$` (ids 5001 to 5032), none from `$2$`, and 36 from `$3$` (ids 5050 to 5125). They have 1 slot (524
treasures), 2 (70), 3 (30), 4 (4) or 5 (2). Of the 780 filled slots, 724 give an item and 56 give money, in slot 0
(49) or slot 2 (7).

381 map event scripts call `GETTREASURE`. It takes the treasure id from the map object in play
(`G[0x6253fb] = G[0x5ed0b9 + G[0x5dda5b]]`, `GETTREASURE@0x00000`), whose array the map scripts fill (`MP03B0201`
gives ids 121 to 123). A treasure of id 0, or one already opened (`G[0x109a43 + id] == 1`), gives 何も見つからなかった.
Otherwise `GETTREASURE` marks it opened (`GETTREASURE@0x00048`) and calls `GETITEM`. `GETITEM` collects the five
slots (`GETITEM@0x0001d`..`0x0007c`), shows the items and the money, adds each item with `ADDITEM`
(`GETITEM@0x00109`..`0x00129`), adds the money to `G[0x118f0d + G[0x363c]]` up to 9,999,999
(`GETITEM@0x0012f`..`0x00186`) and clears the selector (`GETITEM@0x0018b`).

No field is copied into saved state. The save keeps the opened flags `G[0x109a43 + id]`, keyed by the treasure id, so
a changed treasure reaches an old save unless it was already opened there.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x6253fc]` | 5 | 0..4 | 581 | 3..2987, 280 distinct | the slot's item (an `ITINIT` id), 0 for money (`GETITEM@0x0001d`) |
| `G[0x63174c]` | 5 | 0..4 | 630 | 1..96000, 44 distinct | the slot's count (1 to 9), or the money where the item is 0 (500 to 96000) (`GETITEM@0x0001d`..`0x0007c`) |

### EX1INIT (the fishing game)

`EX1INIT` holds the data of the fishing game, `EXTRA1` (its comment reads 釣りゲーム). It has no leader and no
appends: mostly `copy-inline-int-array` writes, 1526 integer cells from `G[0xc77b38]` to `G[0xc7812d]` and 88
strings. Its arrays are indexed by four id spaces, each an offset into `ITINIT`: rod k (0 to 3) is item `46 + k`
(`EXTRA1@0x000cb`..`0x00119`), bait k (0 to 7) is item `601 + k` (spent at `EXTRA1@0x0209b`..`0x020b0`), fish k (0 to
14) is item `611 + k` (added at `EXTRA1@0x02072`), and spot k (0 to 23, 9 written) is a fishing place. The game's own
debug comments name some arrays: Ex1HitProbTbl (`EXTRA1@0x06dc2`), Ex1FishExist, CastPointNum, BaitNum and AllMapNo.

Eight map event scripts start the game: they set `G[0x0] = 9` and `G[0xc77b2d] = 1` (`MPEV06F0401@0x001b4`..`0x001b9`;
also `MPEV01B0101`, `MPEV10B0105`, `MPEV11B0101`, `MPEV19F0105`, `MPEV22B0106`, `MPEV24B0102`, `MPEV27F0101`). In
mode 9 `SYSTEM4` calls the script in `G[0xc77b2e + G[0xc77b2d]]` (`SYSTEM4@0x00719`), and `INIT2` puts `EXTRA1` in
slot 1 (`INIT2@0x00114`). `EXTRA1` picks the spot whose map is `G[0x363f]` (`EXTRA1@0x00256`).

A cast stops a marker on a gauge of five cast points. The rod limits the points it reaches (`EXTRA1@0x06ce5`..
`0x06d1e`). The bait and the point pick a row of the bite weights (`EXTRA1@0x06d5b`), and the bite draws a fish at
random, each weighted by its row value times its presence at the spot (`EXTRA1@0x06d8d`..`0x06e26`). The reel then
moves a marker on a second gauge: the fish is caught when the marker lies inside the fish's zone
(`EXTRA1@0x01d1c`..`0x01d4b`), and escapes otherwise. A catch adds the fish's points, at most 99999 in all
(`EXTRA1@0x02048`); an escape takes away its penalty, to no less than 0 (`EXTRA1@0x0201b`). The points give the rank.

No field is copied into saved state. `EXTRA1` loads the fishing points from `G[0x26636c]` and the rod in use from
`G[0x26636d]` (`EXTRA1@0x0033a`, `0x0033f`) and stores both back on leaving (`EXTRA1@0x07c39`, `0x07c3e`); it sets
`G[0x266370]` once it has set them up (`EXTRA1@0x0030f`..`0x00335`). The ever-obtained flags of the fish items,
`G[0x10f227 + 611 + k]`, hide the fish not yet caught from the list (`EXTRA1@0x001fe`). The rod and fish names here
are the game's own, apart from `ITINIT`'s (ミスリルロッド here, ミスリルの釣竿 there).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc77b38]` | 3 | 0..2 | 4 | 1..15, 9 distinct | a rod: the nearest and the farthest cast point it reaches (1 to 5; a cast outside them fails, `EXTRA1@0x06ce5`, and the gauge marks only those between, `EXTRA1@0x03916`), and the speed of the cast marker (`EXTRA1@0x06bb5`, `0x06e5d`). [1,3,15], [2,4,13], [3,5,10], [1,5,6] |
| `G[0xc77b44]` | 5 | 0..4 | 8 | 0..39, 40 distinct | a bait: the row of `G[0xc77b6c]` for each cast point 1 to 5 (`EXTRA1@0x06d5b`); bait k has rows 5k to 5k + 4 |
| `G[0xc77b6c]` | 15 | 0..14 | 40 | 1..20, 12 distinct | Ex1HitProbTbl: the bite weight of each fish for a bait and a cast point (`EXTRA1@0x06d8d`) |
| `G[0xc77dc4]` | 4 | 0..3 | 15 | 0..370, 30 distinct | a fish: the points a catch gives (`EXTRA1@0x02048`), the points an escape takes (`EXTRA1@0x0201b`), and the x and width of its catch zone on the reel gauge (`EXTRA1@0x03051`, `0x03083`). 腐った水草 [0,0,300,60]; 虹色の福寿魚 [10,10,370,7] |
| `G[0xc77e00]` | 1 |  | 7 | 0..300, 7 distinct | the points each rank needs: 0, 30, 60, 100, 150, 210, 300 (`EXTRA1@0x07553`) |
| `G[0xc77e07]` | 1 |  | 15 | 400..400, 1 distinct 400×15 | a value per fish that goes into a cell of a picture element (`EXTRA1@0x049b2`) |
| `G[0xc77e16]` | 15 | 0..14 | 24 | 0..1, 2 distinct 0×68, 1×292 | Ex1FishExist: 1 where the fish lives at the spot (`EXTRA1@0x06d8d`). Rows 9 to 23, no spot's, are all 1 |
| `G[0xc77f7e]` | 1 |  | 24 | 0..50, 6 distinct 0×15, 4×1, 16×4, 17×2, 20×1, 50×1 | the spot's music (`EXTRA1@0x0081c`) |
| `G[0xc77f96]` | 1 |  | 24 | 0..14228, 3 distinct 0×21, 10360×2, 14228×1 | the spot's looping sound, A0628.WAV or B0503B.WAV, 0 for none (`EXTRA1@0x00828`..`0x00861`) |
| `G[0xc77fae]` | 1 |  | 24 | 0..14237, 10 distinct | the spot's background, SO027E, SO027A and the rest (`EXTRA1@0x00885`) |
| `G[0xc77fc6]` | 1 |  | 24 | 0..14244, 8 distinct | a second picture over the background, 0 for none (`EXTRA1@0x0089a`) |
| `G[0xc77fde]` | 1 |  | 24 | 0..1, 2 distinct 0×21, 1×3 | which of two pictures of the fisher the spot shows, column 0 or 1 of `G[0xbfbb6d + 2 * G[0x363b]]` (`EXTRA1@0x0029b`..`0x002ff`) |
| `G[0xc77ff6]` | 12 | 0..11 | 24 | -1..420, 17 distinct | six ranges on the cast gauge, a start and a width each, -1 for none. The gauge draws them (`EXTRA1@0x037a0`), and a cast that stops in one ends in state 2 (`EXTRA1@0x01ada`..`0x01b39`); what state 2 means is not read |
| `G[0xc78116]` | 1 |  | 24 | 0..28, 10 distinct | AllMapNo: the map of the spot, matched with `G[0x363f]` (`EXTRA1@0x00256`): 10, 1, 6, 11, 19, 22, 24, 27, 28 |
| `GS[0x10e9a]` | 4 | 0..2 | 4 | 丈夫な木の枝 / ミスリルロッド / 神木 | a rod's name and up to three lines of its description (`EXTRA1@0x07a58`) |
| `GS[0x10eaa]` | 4 | 0..2 | 8 | 荒地ミミズ / イワノコムシ / 地虫玉 | a bait's name and its description (`EXTRA1@0x079e9`) |
| `GS[0x10eca]` | 1 |  | 7 | 釣り初心者 / 釣り好き / 釣りマニア | the rank names, 釣り初心者 to 釣りの神様 (`EXTRA1@0x0758d`) |
| `GS[0x10ed1]` | 4 | 0..3 | 15 | 腐った水草 / オショロコマ / 突進イワシ | a fish's name and three lines of its description (`EXTRA1@0x075ca`..`0x07630`, `0x02865`) |

### SHINIT (the shop in play)

`SHINIT` is not in the boot image. It fills the goods of one shop each time `SHOP` asks for them. A scene script sets
the shop id `G[0x42ab0d]` and calls `SHOP` (`SC0600@0x00d6a`..`0x00d70`); scene scripts SC0300 to SC6390, and
`DEBUGADV`, open the shops this way, and `SHOP` clears the id on leaving (`SHOP@0x04310`). `SHOP` runs the hook chain
`G[0xc782f0]` (`SHOP@0x01f34`..`0x01f66`): `INIT2` puts `SHINIT` in slot 0 (`INIT2@0x00092`), and the three appends
put their `$n$SHINIT` in slots 1 to 3 (`$1$AUTORUN@0x00086` and the same offset in the others). The appends' scripts
only exit. `SHOP` runs the chain when it opens (`SHOP@0x0040f`) and again after each settlement (`SHOP@0x01563`).

`SHINIT@0x00000` clears the pictures and both 40-cell arrays, then branches on the shop id. There are 22 shops, ids 1
to 18 and 20 to 23. Each array is 2 pages of 20 slots (`SHOP` reads `G[0x42ab0f + 20 * page + slot]`). The clears
give the bases and the page size, so the layout is not inferred. Slot 0 of each page is never written. Page 0 holds
goods without a stock limit (329 entries), page 1 goods with one (271 of 274 entries).

Two kinds of gate follow an item's write. A story-flag test sets the item to 0, which removes it: 326 tests remove it
unless a flag is set, and 14 remove it once a flag is set. A chapter test negates the item while `G[0x363a]` is below a
value (128 entries; the scene scripts `SC0000` to `SC0180` set `G[0x363a]` to 0 to 10, `SC0020@0x00902`). One entry is
negated by a flag instead: item 17 (ラクダ) of shop 1, once flag 9018 says it is held (`SHINIT@0x0019d`). `SHOP`
shows a negated item by its absolute value (`SHOP@0x02988`) and refuses to sell it, playing sound `0x333f`
(`SHOP@0x00ba0`..`0x00bc4`).

`SHOP` builds each page's list from the slots whose item is not 0 and whose stock is not 0 (`SHOP@0x02031`..
`0x0208f`). A slot's stock is its limit minus what has been bought there, `G[0x25fa0b + 40 * shop + 20 * page + slot]`,
no less than 0, or unlimited where the limit is 0 (`SHOP@0x01fab`..`0x0202a`). A purchase adds the item with
`ADDITEM` and its count to that cell (`SHOP@0x0145a`..`0x014b2`), takes the bill from the money in play (`SHOP@0x014c3`), and adds it
to `G[0x266377 + party]` (`SHOP@0x0154e`..`0x0155c`). Those two arrays are saved. The save keys the bought counts by
shop, page and slot, so a change that moves a limited good to another slot leaves its count on the old slot.

| array | cells per slot | shops | slots | values | holds |
|---|---|---|---|---|---|
| `G[0x42ab0e]` | 1 | 22 | none | 0..6, 7 distinct 0×3, 1×12, 2×1, 3×1, 4×1, 5×3, 6×1 | which 141×141 cell of the shop's sign sheet (texture `0x7c`) `SHOP` draws, by the x and y lists it loads at `SHOP@0x00038` (`SHOP@0x02559`..`0x02567`) |
| `G[0x42ab0f]` | 40 (2 pages × 20) | 22 | 1..19, 21..39 | ±17..2975, 603 entries | the item sold in the slot (an `ITINIT` id); 0 for none, negated while it cannot be bought (`SHOP@0x00b90`, `0x0145a`) |
| `G[0x42ab37]` | 40 (2 pages × 20) | 22 | 21..39 | 1..20, 7 distinct 1, 3, 4, 5, 6, 10, 20 | the slot's stock limit, 0 for unlimited (`SHOP@0x01fab`) |
| `G[0x42ab5f]` | 1 | 20 | none | CS057AA, CS060AA, CS061AA, CS067AA, CS072AA | the shopkeeper's picture, drawn at the bottom right (`SHOP@0x02496`, `0x0251e`..`0x02541`) |
| `G[0x42ab60]` | 1 | 19 | none | BG002A, BG024A, BG029A, BG029B | the shop's background, drawn over the whole screen (`SHOP@0x02488`, `0x024de`) |

### CSINIT (scripted battle parties)

`CSINIT` is not in the boot image. It sets the party of a scripted battle. `BTL` runs the hook chain `G[0xc78390]`
when the scripted case `G[0x63dcda]` is not 0 (`BTL@0x000ab`..`0x000e4`): `INIT2` puts `CSINIT` in slot 0
(`INIT2@0x000ab`), and the appends put theirs in slots 1 to 3 (`$1$AUTORUN@0x0009f` and the same offset in the
others). Map events, scene scripts and `ARENA` set the case before the battle (`MPEV07B0602`, `SC0690`, …); `RESULT`,
`SLG` and `BTLMENU` read it too. `$1$CSINIT` and `$2$CSINIT` only exit; `$3$CSINIT` adds case 25.

`CSINIT@0x00000`..`0x00014` clears five arrays of 7 slots, then branches on the case (cases 1 to 24, and 25 from
`$3$`). The clears give the bases. A case writes the character and the position of 1 to 4 slots; a case of one
character puts it at column 3, row 3. The cases and their slots, as (character, column, row):

| case | slots | case | slots |
|---|---|---|---|
| 1 | (1,3,3) (2,1,2) (4,5,1) | 14 | (63,3,3) |
| 2 | (11,2,3) (12,4,3) | 15 | (13,3,3) |
| 3 | (11,3,3) | 16 | (4,3,3) (19,3,1) |
| 4 | (4,3,3) | 17 | (12,3,3) (16,3,1) |
| 5 | (2,3,3) | 18 | (11,3,3) (12,1,3) (43,5,3) (19,3,1) |
| 6 | (54,3,3) | 19 | (52,3,3) |
| 7 | (51,3,3) | 20 | (17,3,3) |
| 8 | (52,3,3) | 21 | (18,3,3) |
| 9 | (51,4,1) (52,3,3) (54,2,1) | 22 | (1,3,3) |
| 10 | (1,3,3) (2,4,1) (3,2,1) | 23 | (11,2,3) (63,4,3) |
| 11 | (1,4,3) (2,3,1) (4,2,3) | 24 | (1,3,3) (3,3,1) |
| 12 | (12,3,3) | 25 | (28,3,3) |
| 13 | (16,3,3) |  |  |

`BTL` keeps a slot only when the character's state `G[0x118fd8 + character]` has bit 2 set (`BTL@0x0821e`) and the
slot's two flags pass (`BTL@0x00107`..`0x00126`). It copies slots 0 to 4 to the battle party `G[0x36f9f8]` and slots 5
and 6 to `G[0x36f9fd]` (`BTL@0x00130`). Without a case the party comes from the party in play, `G[0x118ed0 + 5 *
party]` and `G[0x118eee + 2 * party]` (`BTL@0x00141`), and the positions from the formation `G[0x118edf]`. The battle
party lies above the saved prefix, so nothing of `CSINIT` reaches a save.

| array | cells per slot | cases | slots | values | holds |
|---|---|---|---|---|---|
| `G[0x63dcdb]` | 1 | 25 | 0..3 | 1..63, 17 distinct | the character in the slot (a `CHINIT` id), 0 for none (`BTL@0x00107`, `0x00130`) |
| `G[0x63dce2]` | 1 | 25 | 0..3 | 1..5 | the character's start column: `SETENP` places it at x = column - 1 (`SETENP@0x0018f`) |
| `G[0x63dce9]` | 1 | 25 | 0..3 | 1..3 | the character's start row: `SETENP` places it at y = row - 4 (`SETENP@0x001a4`) |
| `G[0x63dcf0]` | 1 | 0 | none | never written | a story flag that must be set for the slot to stay (`BTL@0x08247`) |
| `G[0x63dcf7]` | 1 | 0 | none | never written | a story flag that drops the slot when set (`BTL@0x08279`) |

## Battle, enemies and routines

A battle on the field draws on these tables in turn. On a map, `FIELD` loads the map's blocks (`RMINIT`) and the
block's encounter set (`ENINIT`). As the party walks, `FIELD` rolls the chance in `EEINIT`. When a battle starts,
`SETENP` draws one entry of the set and places its formations (`ESINIT`), and `SETEN` sets up each enemy from `EBINIT`.
In battle, `BTL` runs `RTN` for each battler under routine control. `RTN` follows the unit's routine in `RTINIT`.
An action's animation is a `BTANINIT2` record. `SETEFFECT` builds that record's effects from `BTANINIT` and `ANIME` plays
them. On the strategy map, `SLG` runs `MVRTN` for each map unit. `MVRTN` follows the unit's routine in `SRINIT`.

`RTINIT`, `SRINIT`, `BTANINIT2` and `EEINIT` are boot tables. `RMINIT`, `ENINIT`, `ESINIT` and `BTANINIT` are not in the
boot image. Each runs through a hook chain (below) when the game needs it.

### Hook chains of this domain

The chains are described under "Hook chains" above; this domain's four, with when each runs (a runner calls every
nonzero slot in order, `FIELD@0x84`..`0xaa`, except `BTANINIT`'s):

| chain | slot 0 | run by | when |
|---|---|---|---|
| `G[0xc78330]` | `RMINIT` | `FIELD@0x92`; `INIT2@0x1e5` | `FIELD` runs it when it starts on a map. `INIT2` runs it when the saved `G[0x36e5]` is set. `SYSTEM4` runs `INIT2` (`SYSTEM4@0x2d1`) right before it restores a save (`SYSTEM4@0x2d4`), so this rebuilds the map in play after a load (inferred from that order) |
| `G[0xc78350]` | `ENINIT` | `FIELD@0x63a`; `MP01B0101`, `MP13B0101`, `MP13B0404`, `MPEV01B0203` | after `G[0x63da9c]` is set from the block (`FIELD@0x618`) or by the map script (`MP01B0101@0x4e9`) |
| `G[0xc78370]` | `ESINIT` | `SETENP@0x414` | once for each of the drawn entry's two formations, with `G[0x63dc91]` set to it (`SETENP@0x3f2`) |
| `G[0xc78410]` | `BTANINIT` | `SETEFFECT@0xf0`..`0x105` | for each effect of the animation in play. Unlike the others, it stops at the first layer that defines the effect: it walks the chain while the slot's picture is still 0 (`SETEFFECT@0xcd`..`0xe9`) |

The appends add: `$1$RMINIT` maps 26 and 42; `$3$RMINIT` maps 44 and 45; `$1$ENINIT` sets 76 and 78; `$3$ENINIT` sets
97 to 101; `$1$ESINIT` 13 formations (71 to 1015); `$3$ESINIT` 76 formations (1270 to 1410); `$2$BTANINIT` effects 661
to 666 and 1134; `$3$BTANINIT` effects 667 to 676. The other `$n$` scripts of these tables have no branches.

### RTINIT (battle routines)

A battle routine is a list of up to 50 steps. Each step names a rule (which provider script runs it), the chance it
runs, and up to four arguments. The leader `G[0xa26f6c]` has capacity 50000 with 50 slots a row: a row is a routine and
a slot is a step. The tool's leader is right. Ten banks follow, 50000 apart. Banks 7 and 8 (`G[0xa7c69c]`,
`G[0xa889ec]`) are written by no layer, so the tool does not list them. Cells at the offsets of banks 10 and 11 belong
to `TIINIT`.

There are 364 routines, ids 1 to 747. By layer, the base has 316 routines (1 to 708), `$1$` adds 5, `$2$` adds 2 and
`$3$` adds 41. Step records by layer: 2876, 73, 64 and 333, 3346 in all.

How the game reads it: `BTL@0x31c6`..`0x3206`. When the battler's state has bit `0x100` set, `BTL` takes the routine
from `EBINIT` (`G[0x41be47 + unit]`, the unit being `G[0xc1a8ca + battler]`) into `G[0xa26f69]` and runs `RTN`. A result
of 2 sets the battler's state bit 4 (the 防御 stance). `RTN` first copies the unit's six actions (`EBINIT`'s
`G[0x41c22f]`, six a unit) into `G[0x36fa00]` (`RTN@0x9d`). If action 0 is empty, it uses the battler's
`G[0xc1ab9a]`, or 9999 (素手). For a party battler (1 to 5), tactic 4 of the character (防御, column 1 of
`G[0x16ad3a + 6 × character]`) runs `RTN202` at once (`RTN@0xe4`..`0x121`). Otherwise `RTN` walks the steps 0 to 49
(`RTN@0x124`..`0x280`):

- A step whose rule is 0, or a rule with no script, ends the routine (`RTN@0x141`..`0x174`).
- Bank 8, when set, is a story flag that must be on: `G[0x929 + flag - 1]` (`RTN@0x181`..`0x1ac`).
- Bank 9, when set, is how many times the step may run in this battle. `G[0xc1c436 + 50 × battler + step]` counts its
  runs (`RTN@0x1b1`..`0x1e7`), and `RTN` adds one when the step acts (`RTN@0x25e`..`0x274`).
- For a party battler, bank 2 is a mask of tactics: a set bit for the character's tactic skips the step
  (`RTN@0x1ec`..`0x21e`, `check-bit`).
- Bank 1 is the percent chance (`RTN@0x22a`..`0x241`). Then the rule's script runs with the step in `G[0xa26f6a]`
  (`RTN@0x248`..`0x254`).
- The first script that sets `G[0xa26f6b]` ends the walk. If none does, `RTN000` uses action 0 on a target the battle
  picks (`RTN@0x283`..`0x291`).

The rules (`RTN@0x0`..`0x8c` maps rule r to the script in local `7 + r`):

| rule | script | what it does | steps |
|---|---|---|---|
| 1 | `RTN001` | one of the six actions, drawn among those `CHECKACT` allows (`RTN001@0x1d`..`0x7d`) | 32 |
| 2 | `RTN002` | the step's action on the nearest target. The key is `abs(dy) << 16 + abs(dx) << 8 + a draw` (`RTN002@0x1c2`..`0x217`), sorted from the smallest | 682 |
| 3 | `RTN003` | the step's action on a target the battle picks (target -1, `RTN003@0x3c`) | 699 |
| 4 | `RTN004` | on the target whose gauge named by bank 4 is lowest (1 HP, 2 MP, 3 TP, 4 the gauge at `G[0xc1a960]`; `RTN004@0x1d9`..`0x2a5`) | 0 |
| 5 | `RTN005` | on a target drawn among those whose bank-4 gauge is at or above bank 5 percent (`RTN005@0x1ee`..`0x2de`) | 4 |
| 6 | `RTN006` | the same, at or below (`RTN006@0x2d7`) | 0 |
| 7 | `RTN007` | on a battler whose unit is bank 4 (`RTN007@0x1d9`) | 3 |
| 8 | `RTN008` | on the target it suits most: `AFINIT`'s rate for the target's kind and the action (`G[0xc0b56d]`), plus 20 times the lead of the actor's `G[0xc1ac6c]` over the target's `G[0xc1aca8]`, the largest first (`RTN008@0x1c2`..`0x24f`; what the two cells are is not read) | 39 |
| 9 | `RTN009` | an action with an area, aimed where it covers the most targets (inferred from its reads of `ITINIT`'s ranges `G[0x865b59]` and `G[0x863449]`, `RTN009@0x4c`..`0x14e`); it reads bank 4 too | 159 |
| 10 | `RTN010` | the step's action when the actor's own bank-4 gauge is at or above bank 5 percent (`RTN010@0x37`..`0x125`) | 2 |
| 11 | `RTN011` | the same, at or below (`RTN011@0x125`) | 457 |
| 12 | `RTN012` | the step's action once the battle clock `100 × G[0x276e4e] + G[0x276e4d]` has reached bank 4 (`RTN012@0x20`..`0x35`) | 217 |
| 13 | `RTN013` | on a target drawn at random, not itself (`RTN013@0x15e`) | 2 |
| 51 | `RTN051` | a party member: one of the character's learned skills (`G[0x13ee1a]`) whose kind (`ITINIT`'s `G[0x645469]`) is bank 3 and which attacks (`G[0x860d39]` = 1). Under チャージ全力攻撃 or 全力攻撃 it takes the strongest (`ITINIT`'s `G[0x640649]`), otherwise one at random (`RTN051@0x5a`..`0xea`) | 143 |
| 52 | `RTN052` | the same over every learned attacking skill (`RTN052@0x43`..`0xbe`) | 31 |
| 53 | `RTN053` | a learned skill of kind bank 3 whose class (`G[0x860d39]`) is 2 or 3 (`RTN053@0x86`..`0xb0`) | 59 |
| 101 | `RTN101` | the step's action on itself when its own bank-4 gauge is at or below bank 5 percent (`RTN101@0x125`..`0x16b`) | 0 |
| 102 | `RTN102` | the action when some target's bank-4 gauge is at or below bank 5 percent (`RTN102@0x23c`) | 10 |
| 103 | `RTN103` | the same when at least bank 6 targets are (`RTN103@0x284`..`0x2a3`) | 78 |
| 201 | `RTN201` | action 9998 (逃亡) on itself (`RTN201@0x0`) | 3 |
| 202 | `RTN202` | stand: result 2 (`RTN202@0x0`), which `BTL` turns into the 防御 stance | 0 |
| 203 | `RTN203` | action 9979 (盗む) (`RTN203@0x0`) | 5 |
| 301 to 306 | `RTN301`..`RTN306` | the item versions: 301 as 3, 302 as 9, 303 as 12, 304 as 101, 305 as 102, 306 as 103. Bank 3 is an item id, which `CHECKACT` takes directly (`RTN301@0x10`, `RTN304@0x37`) | 6, 0, 1, 247, 291, 175 |

The names are `ITINIT`'s (逃亡 is `GS[0x2f78 + 9998]`).

Data: the items are 101 to 123 for 304 and 305 (治癒の水・小 … 魔力石・大), 106 to 113 for 306, and 153 to 216 for 301.
Rule 103's actions include 3001 to 3008 (癒しの息吹Ⅰ … 大いなる癒しの風). Routine 370's step 1 has rule 30, which has no
script, so the routine ends there; it may be a slip for 3.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xa26f6c]` | 50 | 0..49 | 364 | 1..306, 24 distinct | each step's rule, 1 to 13, 51 to 53, 101 to 103, 201 to 203 and 301 to 306 for `RTN001` to `RTN306` (the table above). 0 ends the routine (`RTN@0x141`) |
| `G[0xa332bc]` | 50 | 0..49 | 364 | 5..100, 19 distinct | the percent chance the step runs (`RTN@0x22a`, a draw of 0 to 99) |
| `G[0xa3f60c]` | 50 | 20..49 | 30 | 11..14, 4 distinct 11×51, 12×114, 13×53, 14×17 | for a party battler, the tactics that skip the step: bit t for tactic t of column 1 of `G[0x16ad3a]` (0 チャージ全力攻撃, 1 全力攻撃, 2 ランダム攻撃, 3 基本攻撃で攻撃, 4 防御; `CHMODE2`'s strings). 11 runs only under ランダム攻撃, 12 under the two 全力攻撃, 13 under 全力攻撃, 14 under チャージ全力攻撃 (`RTN@0x1fa`) |
| `G[0xa4b95c]` | 50 | 0..48 | 358 | 1..7015, 80 distinct | the step's action. Below 6 it is a slot of the unit's six actions (`G[0x36fa00]`), otherwise an `ITINIT` id (`RTN003@0x0`..`0x2d`). For rules 51 and 53 it is a skill kind instead (`RTN051@0x86`). For 301 to 306 it is an item |
| `G[0xa57cac]` | 50 | 0..32 | 303 | 1..800, 28 distinct | the rule's first argument: the gauge (1 HP, 2 MP, 3 TP, 4 `G[0xc1a960]`) for 4 to 6, 10, 11, 101 to 103 and 304 to 306; a unit for 7; a battle-clock time for 12 and 303 (`RTN012@0x10`) |
| `G[0xa63ffc]` | 50 | 0..32 | 268 | 5..100, 16 distinct | a percent of that gauge, the threshold of 5, 6, 10, 11, 101 to 103 and 304 to 306 (`RTN010@0x125`) |
| `G[0xa7034c]` | 50 | 0..17 | 54 | 1..3, 3 distinct 1×55, 2×202, 3×4 | for 103 and 306, how many targets must meet the threshold (`RTN103@0x29c`) |
| `G[0xa7c69c]` | 50 |  | 0 | never written | no script reads it |
| `G[0xa889ec]` | 50 |  | 0 | never written | a story flag the step needs, `G[0x929 + flag - 1]` (`RTN@0x181`); no routine sets one |
| `G[0xa94d3c]` | 50 | 0..8 | 146 | 1..10, 8 distinct | how many times the step may run in a battle (`RTN@0x1b1`) |

Saved state: the use counts `G[0xc1c436]` and every cell `RTN` writes are battle work, above the saved prefix. No script
copies a routine cell below `0x268b19`.

### SRINIT (map-unit routines)

`SRINIT` is the routines of the units on the strategy map. It works like `RTINIT`, with 20 steps a routine. It has no
leader in the tool. Its arrays line up 20000 apart from `G[0xc4bbdb]`, in nine banks. The right leader is:

```
Leader("G", 0xc4bbdb, 20000, group=20)
```

With it, the tool finds 54 step records in the base layer. They make 11 routines, 1 to 11, of 1 to 9 steps. The
appends' `$n$SRINIT` add none. Banks 4 to 7 are never written. The tool lists bank 8's one cell (`G[0xc72da5]`, routine
10 step 2) as outside the table.

How the game reads it: `SLG@0x1dee` runs `MVRTN` for the map unit `G[0x262f35]`. `MVRTN` takes the unit's routine from
`G[0x2659ea + unit]` (a saved cell that `STINIT` fills) into `G[0xa26f69]` (`MVRTN@0x87`). It then walks steps 0 to 19
(`MVRTN@0x99`..`0x1c5`):

- Rule 0 ends the routine. A rule with no script is skipped (`MVRTN@0xb6`..`0xdb`).
- Bank 7 is a story flag that must be on (`MVRTN@0xe7`..`0x12a`).
- Bank 8 limits the step's uses against `G[0x265a08 + 20 × unit + step]` (`MVRTN@0x12f`..`0x16c`).
- Bank 1 is the chance (`MVRTN@0x178`). Then the rule's script runs (`MVRTN@0x19d`).
- The first script that sets `G[0xa26f6b]` ends the walk. If none does, `MVRTN` sets it to 1 (`MVRTN@0x1c8`..`0x1d6`).

A script that acts adds one to the step's count in the saved `G[0x265a08]` (`RTN_M001@0x0`..`0x16`).

The rules (`MVRTN@0x0`..`0x7d` maps rule r to the script in local `6 + r`):

| rule | script | what it does | steps |
|---|---|---|---|
| 1 | `RTN_M001` | nothing; the unit's turn ends (`RTN_M001@0x1d`) | 1 |
| 2 | `RTN_M002` | the same, only while bit 0 of `G[0x2630f8 + arg1]` is set (`RTN_M002@0x10`..`0x1e`) | 6 |
| 3 | `RTN_M003` | the same, only while it is clear (`RTN_M003@0x1e`) | 0 |
| 11 | `RTN_M011` | moves toward cell (arg1, arg2) of the 18 × 10 map. `MVSEEK` finds where it can go and `SETROUTE` walks the route. A unit already on the cell ends its turn with result 2 (`RTN_M011@0x0`..`0x1e3`) | 2 |
| 12 | `RTN_M012` | the same with `MVSEEK`'s mode `G[0xc4abe8]` 2 instead of 1 (the only difference from `RTN_M011`) | 0 |
| 13 | `RTN_M013` | moves toward map unit arg1's cell (`G[0x262fea + u]`, `G[0x263008 + u]`; `RTN_M013@0x0`..`0x23`) | 4 |
| 14 | `RTN_M014` | moves toward the unit's own `G[0x266312 + unit]`, `G[0x266330 + unit]` (`RTN_M014@0x0`..`0x13`) | 2 |
| 21, 22 | `RTN_M021`, `RTN_M022` | call `MVSEEK` and `SETROUTE`; not read further | 2, 0 |
| 31 to 35 | `RTN_M031`..`RTN_M035` | call `SETROUTE`; not read further | 2, 5, 0, 2, 1 |
| 41 to 45 | `RTN_M041`..`RTN_M045` | not read | 3, 5, 0, 4, 0 |
| 51 to 55 | `RTN_M051`..`RTN_M055` | not read (`RTN_M052` calls `SETDRCT`) | 2, 4, 1, 3, 2 |
| 61, 62 | `RTN_M061`, `RTN_M062` | 61 calls `MVSEEK`, `SETROUTE` and `ATSEEK`; 62 calls `SETROUTE`; not read further | 2, 1 |

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4bbdb]` | 20 | 0..8 | 11 | 1..62, 20 distinct | each step's rule, `RTN_M001` to `RTN_M062` (the table above); 0 ends the routine (`MVRTN@0xc6`) |
| `G[0xc509fb]` | 20 | 0..8 | 11 | 100..100, 1 distinct 100×54 | the percent chance the step runs (`MVRTN@0x17d`) |
| `G[0xc5581b]` | 20 | 0..6 | 5 | 1..17, 10 distinct | the rule's first argument: a switch for 2 and 3, a cell's x for 11 and 12, a map unit for 13 (`RTN_M002@0x0`, `RTN_M011@0x0`, `RTN_M013@0x0`) |
| `G[0xc5a63b]` | 20 | 2..5 | 2 | 6..6, 1 distinct 6×2 | the second argument: a cell's y for 11 and 12 (`RTN_M011@0x10`) |
| `G[0xc5f45b]`, `G[0xc6427b]`, `G[0xc6909b]` | 20 |  | 0 | never written | no script reads them |
| `G[0xc6debb]` | 20 |  | 0 | never written | a story flag the step needs (`MVRTN@0xe7`) |
| `G[0xc72cdb]` | 20 | 2 | 1 | 1..1, 1 distinct 1×1 | how many times the step may run, against the saved count `G[0x265a08]` (`MVRTN@0x12f`) |

Saved state: the routine a unit follows (`G[0x2659ea]`) and its use counts (`G[0x265a08]`) are saved. The routine cells
are not copied: an edited `SRINIT` reaches an old save at once.

### BTANINIT2 (battle animations) and BTANINIT (their effects)

An action's animation is a record of `BTANINIT2`: six effect slots, each with the effect it plays and when the effect
starts, and a total length. The tool has no leader for it. The arrays line up 10000 records apart, with 6 cells a
record. The right leader is:

```
Leader("G", 0xc25a1a, 10000, stride=6)
```

With it, the tool finds 926 records, ids 101 to 9999. By layer: 906 in the base, then 1, 7 and 12.

How the game reads it: `BTL` takes the action's animation from `ITINIT`'s `G[0x9d65c9]` into `G[0xc25964]` and runs
`ANIME` (`BTL@0x1b89`..`0x1b9a`, and eight more sites). `ANIME` runs `SETEFFECT` (`ANIME@0x11`). When the option
`G[0x3637]` is set (`CONFIG@0xfc9`), it first sets the animation to 0 (`ANIME@0x0`..`0xc`). `SETEFFECT` clears the
effect work arrays. For slots 0 to 5 it builds the effect `G[0xc25a1a + 6 × animation + slot]`. For slots 6 to 35 it
builds the effect `G[0xc259fc + slot - 6]` (`SETEFFECT@0x4b`..`0xb1`). Those 30 cells are the battlers' condition effects:
`BTL` copies `SSINIT`'s `G[0x9f4221]` for the condition into `G[0xc259fc + battler]` (`BTL@0x3e8a`, `COMMITSS@0x3b1`).
`ANIME` then starts each slot at its start time, less the earliest (`ANIME@0x29a`..`0x2ec`, `0x158a`).

The slots are used by role. Slot 0 holds one of 7 lead-in effects (125 to 127, 726, 744 to 746), always at 100 ms. Slot
2 holds the main effect (399 distinct). Slot 5 holds one of 9 closing effects. Slots 1, 3 and 4 are rare (1, 76 and 46
records).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc25a1a]` | 6 | 0..5 | 926 | 1..4123, 415 distinct | the effect each slot plays (a `BTANINIT` effect id). `SETEFFECT@0x68` reads it, 0 for none. Animation 7102's slot 2 names effect 4123, which no layer of `BTANINIT` defines, so that slot shows nothing |
| `G[0xc3447a]` | 6 | 0..5 | 926 | 100..1800, 24 distinct | when the slot's effect starts, in ms: slot 0 always 100, slot 2 mostly 400, 800 or 1000, slot 5 500 to 1800 (`ANIME@0x158a`; its sound too, `ANIME@0x18d5`) |
| `G[0xc42eda]` | 1 |  | 923 | 600..2200, 10 distinct 1200×182, 1300×275, 1500×330, 1900×100 … | the animation's length in ms (`ANIME@0x29a`); a voice is scheduled against it (`ANIME@0x2143`) |

`BTANINIT` defines the effects: 439 in the base (ids 1 to 2113), 7 in `$2$` and 10 in `$3$`. Each branch on the effect
`G[0xc25962]` writes the effect into the slot `G[0xc25963]` of 15 work arrays of 36 cells, which `SETEFFECT` clears
(`SETEFFECT@0x0`..`0x46`). 423 of the 439 base effects play the picture `MVB<id>.AGF`. Effect 3 is the one still sheet:
`AMB999.AGF`, 300 × 300, a 5 × 5 sheet of 24 frames over 1000 ms.

| array | cells per slot | effects | values | holds |
|---|---|---|---|---|
| `G[0xc455ea]` | 1 | 456 | 13080..13682 base; `$2$`, `$3$` packed ids | the effect's picture (`MVB110.AGF` …). `ANIME` plays it as a movie into a surface (`ANIME@0x1642`), or sets it as a texture for a sheet (`ANIME@0x1602`) |
| `G[0xc4560e]` | 1 | 456 | 0..1, 0×455, 1×1 | 0 for a movie, nonzero for a still picture (`ANIME@0x15ca`). With 1 the texture is set with colour key `0xff00`, otherwise with -1 (`ANIME@0x15ed`..`0x161a`). The picture is set only while bit 0 of `G[0xc25965]` is set, which `BTL` sets (`BTL@0x3eed`) |
| `G[0xc45632]` | 1 | 456 | 1..3, 1×435, 3×21 | the blend mode `gfx-draw-color` gets (`ANIME@0x22a1`): 1 additive glow, 3 the subtractive path |
| `G[0xc45656]` | 1 | 456 | 112..512, 17 distinct, 160 most | the surface's width (`ANIME@0x1632`). The effect is drawn centred on x (`ANIME@0x221f`) |
| `G[0xc4567a]` | 1 | 456 | 96..368, 14 distinct | the surface's height. The effect is drawn with its foot on y (`ANIME@0x2234`) |
| `G[0xc4569e]` | 1 | 1 | 5 | a sheet's columns (`ANIME@0x22c6`, `animate-gfx-srcrect-target`) |
| `G[0xc456c2]` | 1 | 1 | 5 | not read; a sheet's rows (inferred from effect 3's 5 × 5 sheet) |
| `G[0xc456e6]` | 1 | 1 | 24 | a sheet's frames (`ANIME@0x22bf`) |
| `G[0xc4570a]` | 1 | 1 | 1000 | the effect's length in ms. For a movie `ANIME` writes the movie's own length into it (`ANIME@0x1652`); `ANIME@0x1657` takes the longest |
| `G[0xc4572e]` | 1 | 456 | 1..1, 1×456 | 1: the effect is placed from a battler's cell (the target's, inferred); 0 would place it at (512, 576) (`ANIME@0x215d`..`0x221a`) |
| `G[0xc45752]` | 1 | 456 | -50..115, 13 distinct, 0×420 | an x offset (`ANIME@0x2247`) |
| `G[0xc45776]` | 1 | 456 | -30..60, 19 distinct, 0×336 | a y offset (`ANIME@0x225a`) |
| `G[0xc4579a]` | 1 | 0 | never written | a sound for the slot, played at its start plus the next array (`ANIME@0x18ad`..`0x18f5`); no effect has one |
| `G[0xc457be]` | 1 | 0 | never written | that sound's delay in ms (`ANIME@0x18e7`) |
| `G[0xc457e2]` | 3 | 4 | column 0: 100 | up to three moments, in ms after the slot starts, at which `ANIME` sets a value that falls from 5 over 100 ms (`ANIME@0x1afc`..`0x1b91`, its local `0x29`); what that value moves is not read |

Saved state: none of these cells is saved, and no script copies one below `0x268b19`.

### EEINIT (encounter chances)

`EEINIT` is one inline grid of 5 rows by 10 columns at `G[0x63dcfe]` (one layer, 50 cells). It holds the percent chance
of a battle at each roll on the field. It needs no leader. `FIELD` rolls each time the distance walked, `G[0x36e3]`,
passes another 20 (`FIELD@0x15a4`..`0x161b`). It rolls only while an encounter set is in play (`G[0x63da9c]` > 0) and
four switches allow it (`FIELD@0x15c0`..`0x1614`).

- The row is the distance since the last battle in units of 20 (`G[0x36e3] / 20`): below 10, below 20, below 40, below 70,
  or more (`FIELD@0x19a7`..`0x19ff`).
- The column is the encounter level: `LAINIT`'s `G[0x5f232f + G[0x36df]]` where that is set, otherwise the block's
  `RMINIT` level `G[0x5dd60b]` (`FIELD@0x1a13`..`0x1a3b`). `G[0x36ec]` is added and the result is held to 0..9. It is
  0 while `G[0x26637f]` is set (`FIELD@0x1a04`).
- The cell is the chance (`FIELD@0x1a70`). A party condition raises it by `SSINIT`'s `G[0x9f75b1]` for condition `0x193`
  (`FIELD@0x1ab4`..`0x1b17`).
- A draw of 0 to 99 below it starts a battle (`FIELD@0x1b1e`..`0x1b41`).

| row (distance / 20) | level 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
|---|---|---|---|---|---|---|---|---|---|---|
| below 10 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 2 |
| below 20 | 0 | 0 | 0 | 0 | 0 | 1 | 1 | 2 | 3 | 4 |
| below 40 | 0 | 1 | 1 | 2 | 3 | 3 | 4 | 6 | 8 | 10 |
| below 70 | 0 | 1 | 3 | 4 | 6 | 6 | 8 | 12 | 16 | 20 |
| 70 or more | 0 | 2 | 5 | 6 | 9 | 9 | 12 | 18 | 32 | 40 |

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x63dcfe]` | 10 | 0..9 | 5 | 0..40, 16 distinct | the percent chance of a battle at a roll, by distance band (row) and encounter level (column) (`FIELD@0x1a70`) |

### RMINIT (map blocks)

A map (`G[0x363f]`, a saved cell 124 scripts set) is a grid of blocks, 30 to a row: row `G[0x36d1]`, block `G[0x36d2]`.
`RMINIT` loads one map each time it runs. It clears 17 arrays of 750 cells, or 2 or 3 cells a block (`RMINIT@0x0`..`0x50`),
and fills them from the branch for `G[0x363f]`. The base has 41 maps, ids 1 to 47. Scripts index the arrays as
`lookup-array-2d (base) (G[0x36d1]) 30 (G[0x36d2])`, so a block's record is `30 × row + block`. A block's script and
pictures are resource ids; the values below name some of them.

How the game reads it:

- `SETMAPDATA` calls the block's script (`SETMAPDATA@0xce`) and copies three numbers (`SETMAPDATA@0x9e`..`0xc9`).
- `SETMAPTEX` loads the block's pictures (`SETMAPTEX@0x207`..`0x2c1`).
- `FIELD` calls the block's event script (`FIELD@0x106b`), plays its music (`FIELD@0x66f`) and takes its encounter set
  (`FIELD@0x60d`).
- `SETEN` takes its enemy level (`SETEN@0xbe`..`0x148`) and `BTL` its battle backdrop (`BTL@0x8fb6`).

| array | cells per slot | maps | blocks | values | holds |
|---|---|---|---|---|---|
| `G[0x5d9b73]` | 1 | 40 | rows 1..20, blocks 1..17 | 632 distinct (`MP01B0101.BIN` …) | the block's map script, called when the block is set up (`SETMAPDATA@0xce`) |
| `G[0x5d9e61]` | 1 | 40 | rows 1..20, blocks 1..17 | 632 distinct (`MPEV01B0101.BIN` …) | the block's event script (`FIELD@0x106b`; `$3$SC5970@0x13d6` calls it too) |
| `G[0x5da14f]` | 1 | 40 | rows 1..20, blocks 1..17 | 632 distinct (`MP01B0101A.AGF` …) | the block's picture, loaded into surface `0x71` (`SETMAPTEX@0x232`) |
| `G[0x5da43d]` | 1 | 38 | rows 1..14, blocks 1..17 | 425 distinct (`MP01B0101B.AGF` …) | a second picture, surface `0x72` (`SETMAPTEX@0x252`); 0 for none |
| `G[0x5da72b]` | 1 | 12 | rows 1..20, blocks 1..13 | `SO026A.AGF`×140 | a picture set on `0x73` (`SETMAPTEX@0x27d`) |
| `G[0x5daa19]` | 1 | 40 | rows 1..20, blocks 1..17 | 630 distinct (`MPS01B0101A.AGF` …) | the minimap picture (`DRAWMINIMAP@0x12`) |
| `G[0x5dad07]` | 1 | 40 | rows 1..20, blocks 1..17 | 100 distinct (`MPB007A.AGF`×67 …) | the battle backdrop (`BTL@0x8fb6`, set on texture `0x2e`) |
| `G[0x5daff5]` | 1 | 8 | rows 1..20, blocks 1..7 | 15 distinct (`MP43F0000D.AGF` …) | a picture loaded into surface `0x70` (`SETMAPTEX@0x207`) |
| `G[0x5db2e3]` | 1 | 0 | none | never written | no script reads it |
| `G[0x5db5d1]` † | 1 | 1 | 4 blocks | 40 | not cleared by `RMINIT` but by `FIELD@0x70` and `INIT2@0x1c3`; copied into `G[0x5dda69]` (`SETMAPDATA@0x9e`), the loop time `DRAWMAP` gives an animated layer (`DRAWMAP@0x4e0`, inferred) |
| `G[0x5db8bf]` | 1 | 8 | rows 1..20, blocks 1..7 | 4..8, 4×57, 8×1 | copied into `G[0x5dda6a]` (`SETMAPDATA@0xae`), a divisor of `DRAWMAP`'s (`DRAWMAP@0x61a`); not read further |
| `G[0x5dbbad]` | 1 | 28 | rows 1..9, blocks 1..17 | 100..280, 9 distinct, 250×140, 280×62 | copied into `G[0x5dda68]` (`SETMAPDATA@0xbe`). Where set, `SETMAPTEX` sets picture `0x5124` (`SO031.AGF`) on `0x74` (`SETMAPTEX@0x2a8`); not read further |
| `G[0x5dbe9b]` | 1 | 0 | none | never written | no script reads it |
| `G[0x5dc189]` | 3 | 40 | rows 1..20, blocks 1..17 | 4..51, 19 distinct | the block's music, one number for each of three variants `G[0x36ea]` (`FIELD@0x66f`..`0x68c`, `play-bgm`); 0 fades the music out |
| `G[0x5dca53]` | 2 | 38 | rows 1..20, blocks 1..17 | 1..95, 81 distinct | the block's encounter set (an `ENINIT` id) for each of two variants `G[0x36eb]` (`FIELD@0x60d`). Map scripts set the variant (`MP01B0101@0x4cb`) |
| `G[0x5dd02f]` | 1 | 39 | rows 1..20, blocks 1..17 | 15..450, 88 distinct | the enemies' base level on the block. `SETEN` raises it by part of `G[0x2756d3]`'s lead over it, through `EBINIT`'s `G[0x413977]` (`SETEN@0xbe`..`0x106`; the level reading is inferred) |
| `G[0x5dd31d]` | 1 | 39 | rows 1..20, blocks 1..17 | 1..7, 7 distinct | the spread of that level: `SETEN` draws 0 to 2v and takes v off (`SETEN@0x10d`..`0x148`) |
| `G[0x5dd60b]` | 1 | 38 | rows 1..20, blocks 1..17 | 1..9, 9 distinct | the block's encounter level, `EEINIT`'s column (`FIELD@0x1a30`) |

† Not one of `RMINIT`'s cleared arrays. It is written at 4 blocks (`RMINIT@0x7f0`, `@0xaf0f`).

Saved state: `FIELD` copies the block's music into the saved `G[0x2662d5]` (`FIELD@0x6f2`..`0x6fd`; 35 map scripts do the
same). The copy is rewritten on every block, so an edited value takes effect when the party next enters the block. No
other `RMINIT` cell is copied below `0x268b19`.

### ENINIT (encounter sets)

An encounter set is the list of what may be met on a block. `ENINIT` loads the set `G[0x63da9c]` (from `RMINIT`'s
`G[0x5dca53]`). It clears four arrays of 100 entries (`ENINIT@0x0`..), and the base has 58 sets, ids 0 to 95. `SETENP`
draws one entry per battle (`BTL@0x178` runs `SETENP`). An entry may be drawn when its window holds the field gauge
`G[0x36e1]` (−15 to 15; `FIELD` lowers it as the party walks, `FIELD@0x1591`, and `USESKILL` changes it, `USESKILL@0x15`).
The draw is weighted (`SETENP@0x2b9`..`0x393`). The entry's two formations go to `G[0x276e47]` and `G[0x276e48]`
(`SETENP@0x39b`..`0x3b6`).

| array | cells per slot | sets | entries | values | holds |
|---|---|---|---|---|---|
| `G[0x63da9d]` | 1 | 57 | 0..51 | 5..100, 14 distinct; 50×427, 100×223, 30×209 | the entry's weight in the draw (`SETENP@0x314`..`0x393`); an entry of weight 0 is skipped (`SETENP@0x2c0`) |
| `G[0x63db01]` | 1 | 57 | 0..51 | 1..27, 5 distinct; 1, 6, 12, 21, 27 | the low end of the entry's window, plus 16: drawn when `G[0x36e1]` ≥ value − 16; 0 for always (`SETENP@0x2c7`..`0x2e3`) |
| `G[0x63db65]` | 1 | 57 | 0..51 | 5..31, 5 distinct; 5, 11, 20, 26, 31 | the high end, plus 16: `G[0x36e1]` ≤ value − 16 (`SETENP@0x2ea`..`0x2f8`) |
| `G[0x63dbc9]` | 2 | 57 | 0..51 | 202..1266, 320 distinct | two formations (`ESINIT` ids); the second, set on 322 entries, is placed five rows further (`SETENP@0x840`..`0x84e`) |

### ESINIT (formations)

A formation places up to eight enemies on a 5 × 5 field. `ESINIT` loads the formation `G[0x63dc91]` into nine arrays of 8
slots, which it clears; the base has 709 formations, ids 1 to 1266. `SETENP` places each slot in order
(`SETENP@0x439`..`0x467`, `0x4c6`..`0x855`). A cell outside the slot's range costs 100 a cell in the placement key, so the
range is preferred, not forced (`SETENP@0x6c5`..`0x788`). The unit's size is `EBINIT`'s `G[0x379c87]` by
`G[0x37a06f]`. `SETEN` then sets the enemy up (`SETENP@0x855`).

| array | cells per slot | formations | slots | values | holds |
|---|---|---|---|---|---|
| `G[0x63dc92]` | 1 | 669 | 0..7 | 12..695, 336 distinct | the slot's enemy, an `EBINIT` unit (into `G[0x370816]`, `SETENP@0x553`); 0 for an empty slot (`SETENP@0x456`) |
| `G[0x63dc9a]` | 1 | 342 | 0..7 | 1..5 | the first column the enemy prefers, 1-based; 0 for any (`SETENP@0x55f`..`0x5ca`) |
| `G[0x63dca2]` | 1 | 342 | 0..7 | 1..5 | the last column; below the first, the first alone |
| `G[0x63dcaa]` | 1 | 361 | 0..7 | 1..5 | the first row it prefers, 1-based; 0 for any (`SETENP@0x5cf`..`0x63a`) |
| `G[0x63dcb2]` | 1 | 361 | 0..7 | 1..5 | the last row |
| `G[0x63dcba]` | 1 | 281 | 0..7 | 5..90, 16 distinct; 50×265, 10×154 | the percent chance the slot appears; 0 for always (`SETENP@0x4c6`..`0x4ee`) |
| `G[0x63dcc2]` | 1 | 208 | 0..7 | 31..8102, 25 distinct | a story flag that must be on, `G[0x929 + flag - 1]` (`SETENP@0x4ef`..`0x520`) |
| `G[0x63dcca]` | 1 | 42 | 0..7 | 31..8102, 13 distinct | a story flag that must be off (`SETENP@0x521`..`0x552`) |
| `G[0x63dcd2]` | 1 | 19 | 1..7 | 1..7, 1×48 | the slot (1-based) whose battler this one is linked to: `G[0xc1ca30 + battler]` gets that slot's battler (`SETENP@0x487`..`0x4b8`) |

Saved state: none of `ENINIT`'s or `ESINIT`'s cells is saved, and no script copies one below `0x268b19`.

## The world: places, the field, objects and the war game

The player is always in one place, `G[0x363f]`. The same number is an area of the world map (`AMINIT2`), a field
map (`MPINIT`) and a town (`SIINIT`'s branches): `ALLMAP` sets `G[0]` to 3 (`FIELD`) or 5 (`TOWN`) by the area's kind
(`ALLMAP@0xe11`..`@0xe43`), and `FIELD` and `TOWN` read their tables by `G[0x363f]`. `SYSTEM4` runs the mode in
`G[0]` (`SYSTEM4@0x67f`..`@0x720`): 2 `ALLMAP`, 3 `FIELD`, 4 `BTL`, 5 `TOWN`, 6 `ARENA`, 7 `INN`, 8 `SLG`.

Inside a field map the player stands on a floor `G[0x36d1]` and a section `G[0x36d2]` (up to 25 floors of 30
sections). The section's script, pictures and events come from `RMINIT`, which `FIELD` runs through its chain
`G[0xc78330]` (`FIELD@0x92`) on entry; see "How a map names its scripts and resources" below.

Four of these tables are not in the boot image. `AMINIT`, `SIINIT`, `PZINIT` and `STINIT` run only through their
hook chains, each time the place or battle that needs them opens, and write the cells of the place in play.

### AMINIT2 (world-map areas)

Leader `G[0x42a588]`, capacity 100: 89 records, ids 1 to 99, by layer {0: 87, 1: 2}. An area is a point of a world
map: a dungeon (kind 1, which enters `FIELD`) or a town (kind 2, `TOWN`). `ALLMAP` draws each shown area at its x and
y and moves the cursor between areas with the four neighbour arrays: joy actions 0 to 3 (up, right, down, left)
follow `G[0x42a718]`, `G[0x42a844]`, `G[0x42a77c]` and `G[0x42a7e0]` (`ALLMAP@0x1251`, `@0x1338`, `@0x129e`,
`@0x12eb`), and skip an area whose state bit 0 is clear. The area's state is the saved cell `G[0x3640 + area]`, which
`AMINIT` rebuilds: bit 0 shows the area (`ALLMAP@0xed6`), bit 1 lets the player enter it; otherwise a buzzer plays
(`ALLMAP@0xdf3`).

On entering, `FIELD@0x2b9` and `TOWN@0x11` copy the area's name into `GS[0x1a6]`, the saved name of the current
place (`FIELD` then overwrites it with the floor's name, see `MPINIT`). `FIELD@0x1c7` and `TOWN@0x70` copy the area's
world into the saved `G[0x118ef7 + G[0x363c]]` and the area into `G[0x118efa + G[0x363c]]`, one pair for each value
of `G[0x363c]`. A changed world number reaches an old save only at the next entry.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x42a4c0]` | 2 | 1 | 87 | 13182..16777389, 79 distinct | column 1: the area's icon on the world map (`ALT001A.AGF`: a small mark and a larger one with its label), `ALLMAP@0x14d4`, `SHOWALLMAP@0x58`. Column 0 is never written; `ALLMAP@0x19e2` and `SHOWALLMAP@0xc7` read it for the area under the cursor (surfaces `0x48` and `0x8a`) |
| `G[0x42a588]` | 1 |  | 89 | 1..2, 2 distinct 1×78, 2×11 | the world map that holds the area: 1, or 2 for the other world (始まりの集落, ゲテニア, 異界討伐令 …). `ALLMAP@0x22b` sets the world in play `G[0x36a4]` from it, which `AMINIT` branches on |
| `G[0x42a5ec]` | 1 |  | 89 | 1..2, 2 distinct 1×41, 2×48 | what entering does: 1 `FIELD` (`G[0]` = 3), 2 `TOWN` (`G[0]` = 5) (`ALLMAP@0xe11`..`@0xe43`) |
| `G[0x42a650]` | 1 |  | 87 | 120..2196, 59 distinct | x on the world map (`ALLMAP@0x57`, `@0xe4c`, `@0xeeb`) |
| `G[0x42a6b4]` | 1 |  | 87 | 90..2208, 64 distinct | y on the world map (`ALLMAP@0x63`) |
| `G[0x42a718]` | 1 |  | 40 | 1..96, 39 distinct | the area the cursor moves to on up (joy 0, `ALLMAP@0x1251`) |
| `G[0x42a77c]` | 1 |  | 40 | 1..96, 39 distinct | on down (joy 2, `ALLMAP@0x129e`) |
| `G[0x42a7e0]` | 1 |  | 44 | 1..99, 42 distinct | on left (joy 3, `ALLMAP@0x12eb`) |
| `G[0x42a844]` | 1 |  | 43 | 5..99, 41 distinct | on right (joy 1, `ALLMAP@0x1338`) |
| `G[0x42a8a8]` | 1 |  | 5 | 53..59, 5 distinct | on five areas, the area the cursor stands on when the world map opens in this one's place (集落の裏口 → 北華鏡の集落), `ALLMAP@0x27a` |
| `GS[0x2244]` | 1 |  | 89 | 日陰の遺跡 / フェミリンス神殿 / 粛鎖の岩塩坑 | the area's name, copied into `GS[0x1a6]` (`FIELD@0x2b9`, `TOWN@0x11`) |

### AMINIT (the world map's open areas, on demand)

Chain `G[0xc782b0]`, run by `ALLMAP@0x30` and `ALLMAP@0x254` each time the world map opens. It clears the 100 area
states at `G[0x3640]` and branches on the world in play `G[0x36a4]`: it sets the world map's music `G[0x36a8]`
(`0x2b`, `0x31`) and its six pictures `G[0x36a9]`..`G[0x36ae]` (`AL001.AGF` …), then sets bits 0 and 1 of
`G[0x3640 + area]` by story flags and by `G[0x363c]`/`G[0x363d]`. Worlds 1 and 2 have branches. `$1$AMINIT` adds to
world 1 and tests worlds 2 to 9 with empty branches; `$2$` and `$3$` are such empty tests too. The states lie in the
saved prefix, but `AMINIT` rebuilds them every time, so a changed condition reaches an old save at once.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x3640]` | 1 |  | areas 1..99 | 1 and bit 1 | the area's state: bit 0 shown, bit 1 enterable (`ALLMAP@0xed6`, `@0xdf3`) |
| `G[0x36a8]` | scalar |  | 2 worlds | `0x2b`, `0x31` | the world map's music |
| `G[0x36a9]` | 6 | 0..5 | 2 worlds | `AL001.AGF` … 11 distinct | the world map's six pictures |

### MPINIT (field maps)

Leader `G[0x5d901f]`, capacity 50: 44 records, ids 1 to 47, by layer {0: 40, 1: 2, 3: 2}. The id is the place
`G[0x363f]`, the same id as `AMINIT2`'s dungeon areas. A map is up to 20 floors; each floor has a name, a name plate
and a number. `FIELD` reads the start of the map when the player first enters (`FIELD@0xf5`..`@0x125`, when
`G[0x36e5]` = 0, `G[0xc0ba06]` = 0 and `G[0x5dda60]` ≠ 2) and copies it into the saved position: floor `G[0x36d1]`,
section `G[0x36d2]`, x `G[0x36da]`, y `G[0x36dc]` and facing `G[0x36de]`. `FIELD@0x2cb` copies the floor's name into
the saved place name `GS[0x1a6]` (`BTL@0x8fe4` reads it too). A changed start reaches an old save only on the next
first entry; a changed floor name at the next floor change.

`SETMAPTEX@0x132` loads the map's object sheet as surface `0x66`, from which `DRAWMAP@0x970` draws every map object
(`OBINIT`). `SETMAPTEX@0x195` loads the floor's name plate as surface `0x69`, which `DRAWMAPH` draws.

The tool's base for the floor numbers is one cell off: the scripts' grid puts them at `G[0x5d9691]` (the name
plates' base plus 1250), columns 1 to 20 like the names and plates; the tool's `G[0x5d9692]` reads them as columns 0
to 19. The array ends at `G[0x5d9b73]`, where `RMINIT`'s arrays start.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x5d901f]` | 1 |  | 44 | 13047..50331745, 14 distinct | the map's object sprite sheet (`MO701.AGF`: chests closed and open, bags, switches, doors, a glow ring), loaded as surface `0x66` (`SETMAPTEX@0x132`) |
| `G[0x5d9051]` | 1 |  | 44 | 1..3, 3 distinct 1×39, 2×2, 3×3 | the start floor, into `G[0x36d1]` (`FIELD@0xf5`) |
| `G[0x5d9083]` | 1 |  | 44 | 1..6, 4 distinct 1×39, 2×3, 5×1, 6×1 | the start section, into `G[0x36d2]` (`FIELD@0x101`) |
| `G[0x5d90b5]` | 1 |  | 44 | 150..1810, 41 distinct | the start x, into `G[0x36da]` (`FIELD@0x10d`) |
| `G[0x5d90e7]` | 1 |  | 44 | 295..1960, 40 distinct | the start y, into `G[0x36dc]` (`FIELD@0x119`) |
| `G[0x5d9119]` | 1 |  | 38 | 1..7, 6 distinct 1×1, 2×7, 3×4, 4×12, 6×8, 7×6 | the start facing, into `G[0x36de]` (`FIELD@0x125`); the walking sheets have eight rows, one a facing (inferred) |
| `G[0x5d914b]` | 1 |  | 44 | 150..500, 7 distinct | how fast the encounter level falls: the field's level `G[0x36e1]` (−15 to 15, which `USESKILL` raises) drops by 1 each time the distance walked `G[0x36e2]` reaches ten times this value (`FIELD@0x1575`) (inferred from the arithmetic) |
| `G[0x5d917d]` | 1 |  | 29 | 200..350, 7 distinct | the divisor of the stray-demon (はぐれ魔神) encounter rate; 0, on 15 maps, means none (`FIELD@0x28df`, `@0x296d`; the debug line "はぐれ魔神率=" at `FIELD@0x2982`) |
| `G[0x5d91af]` | 25 | 1..20 | 44 | 14253..50331748, 214 distinct | each floor's name plate (`MPN0101.AGF`, a title banner), surface `0x69` (`SETMAPTEX@0x195`) |
| `G[0x5d9691]` | 25 | 1..20 | 44 | -99..20, 50 distinct | each floor's number as its name shows it, negative below ground (粛鎖の岩塩坑 B1F is −1; 野望の間B99F is −99). No script reads it |
| `GS[0x2a96]` | 25 | 1..20 | 44 | 日陰の遺跡～崩れた柱の麓～ / 日陰の遺跡～魔力の柱回廊～ / フェミリンス神殿～水湧の聖堂～ | each floor's name, copied into `GS[0x1a6]` (`FIELD@0x2cb`) |

#### How a map names its scripts and resources

`MPINIT` holds no script ids. `RMINIT` (another section's table; chain `G[0xc78330]`, run at `FIELD@0x92`) clears
arrays of 750 cells, 25 floors of 30 sections, and fills the branch for `G[0x363f]`. Cell `30 × floor + section` of:
`G[0x5d9b73]` is the section's script (`MP01B0101.BIN`), which `SETMAPDATA@0xce` calls; `G[0x5d9e61]` its event
script (`MPEV01B0101.BIN`); `G[0x5da14f]`, `G[0x5da43d]`, `G[0x5daff5]` and `G[0x5da72b]` its pictures
(`MP01B0101A.AGF` …, `SETMAPTEX@0x207`..`@0x2a1`); `G[0x5daa19]` a shadow-like layer (`MPS01B0101A.AGF`) and
`G[0x5dad07]` a background (`MPB001A.AGF`); `G[0x5db5d1]`, `G[0x5db8bf]` and `G[0x5dbbad]` three numbers that
`SETMAPDATA@0x9e`..`@0xc9` copies into `G[0x5dda69]`, `G[0x5dda6a]` and `G[0x5dda68]`. So the file names follow the
map, floor and section (`MPmm` `B`/`F` `ff` `ss`), and a mod that adds a section adds its row in `RMINIT`'s branch.
`FIELD` finds a section's saved slot in `G[0x36f3]` by the code `map × 10000 + floor × 100 + section` and keeps the
slot in `G[0x36d4]` (`FIELD@0x302`..`@0x38b`).

### LAINIT (land types)

Leader `G[0x5f2073]`, capacity 100: 29 records, ids 1 to 91, all in the base layer. A land type is what a field
section's ground is made of under the player. `FIELD@0x1b60` asks `AGERC`'s `_GetLand` (slot 2) for the land
polygon under the player and writes its land index into `G[0x36df]`; every array is indexed by it. A land type says
which walking sheet the leader wears, the footstep sound, the walking speed, a drift that pushes the player, and an
encounter class.

The tool reads 5 arrays and 8 cells outside them, because 15 records write no leader cell: ids 4 and 7 to 14 write
only a drift, 2 and 27 a speed, 19 and 28 an encounter class. The scripts index nine arrays of 100 from
`G[0x5f200f]`, back to back up to `OBINIT`'s first array at `G[0x5f2393]`; the tool finds the same seven written
ones with `Leader("G", 0x5f2073, 100, also=(0x5f200f, 0x5f20d7, 0x5f213b, 0x5f219f, 0x5f232f))`. `G[0x5f2267]` and
`G[0x5f22cb]` are neither written nor read.

Ids 7 to 14 are a current in eight directions, 50 straight and 35 diagonal: 7 down, 8 down-left, 9 left, 10
up-left, 11 up, 12 up-right, 13 right, 14 down-right (y grows downward). `FIELD@0x1f0a`..`@0x1f8b` adds
`drift × speed / 100` to the player's step each frame; where `G[0x5f2203]` is 0 it first drops the part of the
player's own step that goes against the drift.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x5f200f]` | 1 |  | 15 | 68..14252, 9 distinct | the footstep loop (`WALKB.WAV` … `WALKE.WAV`, `F0009.WAV`, `B0016.WAV`); 0 plays `WALKA.WAV` (`FIELD@0x1bf7`, `@0x28b0`, `DRAWWALK@0x1c4`) |
| `G[0x5f2073]` | 1 |  | 14 | 1..9, 9 distinct | the column of the leader's walking sheets to wear: `G[0xbe2cfd + 25 × G[0x363b] + column]` (`TIINIT`'s table) is loaded as surface `0x67` when the land changes (`FIELD@0x1bb8`..`@0x1be9`, `DRAWCH@0x37`, `CHANGETITLE@0x4a`, `DRAWJUMP@0x18f`). Column 0 is the plain sheet (`CPM001AA.AGF`); 1 wades in water (`CPM001AB.AGF`), 9 rides a mine cart (`CPM001AJ.AGF`) |
| `G[0x5f20d7]` | 1 |  | 12 | -20..-15, 2 distinct -20×11, -15×1 | the walking speed's change in percent: the step is `max(1, 50 × (100 + value) / 1000)` (`FIELD@0x1b6d`, `DRAWWALK@0x153`) |
| `G[0x5f213b]` | 1 |  | 8 | -50..67, 5 distinct -50×1, -35×3, 35×2, 50×1, 67×1 | the drift's x (`FIELD@0x1f1f`, `@0x1f61`, `@0x27ac`) |
| `G[0x5f219f]` | 1 |  | 8 | -50..50, 4 distinct -50×1, -35×2, 35×4, 50×1 | the drift's y (`FIELD@0x1f40`, `@0x1f7d`, `@0x27cd`) |
| `G[0x5f2203]` | 1 |  | 9 | 1..1, 1 distinct 1×9 | 1: the player may walk against the drift; 0 drops the step's part against it (`FIELD@0x1f0a`, `@0x2797`) |
| `G[0x5f2267]` | 1 |  | 0 | never written | not read |
| `G[0x5f22cb]` | 1 |  | 0 | never written | not read |
| `G[0x5f232f]` | 1 |  | 4 | 7..9, 2 distinct 7×1, 9×3 | an encounter class that replaces the section's own (`RMINIT`'s `G[0x5dd60b]`) as the column, clamped to 0..9, of `EEINIT`'s ten-column `G[0x63dcfe]` that `FIELD@0x1a70` reads (`FIELD@0x1a13`) (what the column means is not read) |

### OBINIT and OBINIT2 (map object kinds)

`OBINIT` (leader `G[0x5f2b63]`) and `OBINIT2` (leader `G[0x5f371b]`) are two halves of one table: capacity 1000,
102 records each, ids 1 to 260, by layer {0: 91, 3: 11}, the same ids in both. The id is an object kind. Their
arrays lie on one grid of eleven, `OBINIT2`'s four inside `OBINIT`'s: `OBINIT`'s first five (`G[0x5f2393]` to
`G[0x5f3333]`), `OBINIT2`'s four (`G[0x5f371b]` to `G[0x60c58b]`, two of them 100 cells an id), then `OBINIT`'s last
two (`G[0x624c2b]`, `G[0x625013]`). `OBINIT` says how a kind looks and what it turns into; `OBINIT2` its depth and its
wall.

A section's placed objects live in saved cells, not here: object `k` of section slot `s` has its kind in
`G[0xbb843 + 100 × s + k]`, its x in `G[0xd3ee3 + …]`, its y in `G[0xec583 + …]` and flags in `G[0xa31a3 + …]` (bit
16); `G[0x5f200d]` counts them. `DRAWMAP@0x930`..`@0x988` draws the kind's rectangle of the map's object sheet
(surface `0x66`, `MPINIT`) at the object. `FIELD@0x119f`..`@0x1237` adds the object's position to its kind's wall
points and writes them into the wall polygon tables that go to `AGERC`'s `_SetMapWall*` (points `G[0x5ddcf9]`, x
`G[0x5dddc1]`, y `G[0x5e2be1]`, `Cr` `G[0x5e7a01]` = 1, at polygon `G[0x5f200b] + k`). `EVOLVEOBJ` turns an object
whose switch `G[0x109a43 + G[0x5ecba5 + k]]` is on into the kind's next kind, which it writes into the saved
`G[0xbb843 …]`; a changed next kind reaches an old save only for objects not yet turned.

In records 2 and later the sprite x is written before the leader.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x5f2393]` | 1 |  | 92 | 60..960, 29 distinct | `OBINIT`: x of the kind's rectangle in the sheet (`DRAWMAP@0x930`) |
| `G[0x5f277b]` | 1 |  | 85 | 60..850, 10 distinct | `OBINIT`: its y (`DRAWMAP@0x937`) |
| `G[0x5f2b63]` | 1 |  | 102 | 60..360, 11 distinct | `OBINIT`: its width (`DRAWMAP@0x93e`) |
| `G[0x5f2f4b]` | 1 |  | 102 | 60..200, 8 distinct | `OBINIT`: its height (`DRAWMAP@0x945`) |
| `G[0x5f3333]` | 1 |  | 10 | 1..1, 1 distinct 1×10 | `OBINIT`: the blend mode, 1 additive (the glow rings) (`DRAWMAP@0x988`) |
| `G[0x5f371b]` | 1 |  | 102 | -9999..9999, 8 distinct | `OBINIT2`: added to the object's y to sort it among the sprites; ±9999 puts it under or over everything (`DRAWMAP@0xf5`, `@0x28a`) |
| `G[0x5f3b03]` | 1 |  | 85 | 5..7, 2 distinct 5×53, 7×32 | `OBINIT2`: the number of wall polygon points (`FIELD@0x1191`) |
| `G[0x5f3eeb]` | 100 | 0..6 | 85 | -10..120, 14 distinct | `OBINIT2`: the points' x, from the object's position (`FIELD@0x11e8`) |
| `G[0x60c58b]` | 100 | 0..6 | 85 | 30..160, 15 distinct | `OBINIT2`: the points' y (`FIELD@0x1210`) |
| `G[0x624c2b]` | 1 |  | 54 | -1..251, 41 distinct | `OBINIT`: the kind the object becomes when its switch is on; −1 removes it (`EVOLVEOBJ@0x55`; 117 `MPEV` scripts read it) |
| `G[0x625013]` | 1 |  | 42 | 14467..20712, 6 distinct | `OBINIT`: the sound that change plays (`SE001.WAV` …) (`EVOLVEOBJ@0xba`, `@0xde`) |

### PZINIT (box puzzles, on demand)

Chain `G[0xc783b0]`, run by `SETMAPDATA@0x113` when a section names a puzzle in `G[0x5f2005]` (non-zero); it runs the
chain until `G[0xc7812f]` is set. Twelve puzzles in the base layer, 13 and 14 in `$3$`. `CARRY` (called by
`FIELD@0xe43`) plays them: the player pushes blocks onto goal cells. Rows and columns count from 1.

`SETMAPDATA@0x174`..`@0x1d4` copies each block's start into the saved `G[0x10c4d7 + 8 × puzzle + k]` (x) and
`G[0x10c7f7 + 8 × puzzle + k]` (y) and clears `G[0x10c1b7 + 8 × puzzle …]` when bit 0 of `G[0x10c153 + puzzle]` is
clear, that is the first time. `CARRY` then plays from the saved positions, so a changed start reaches an old save
only for a puzzle never entered.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x63dd30]` | scalar |  | 14 puzzles | 3..10 | the grid's width (`CARRY@0x790`) |
| `G[0x63dd31]` | scalar |  | 14 puzzles | 3..12 | its height (`CARRY@0x773`) |
| `G[0x63dd32]` | 22 | from 1 | 14 puzzles | inline rows | the grid, `22 × row + column`, cleared 22 × 22: 0 no cell, 1 floor, `10 + k` a goal for a block of kind `k` (10 for any) (`CARRY@0xe2`, `@0xf8`, `@0x158`) |
| `G[0x63df16]` | scalar |  | 14 puzzles | 1..8 | the number of blocks |
| `G[0x63df17]` | 8 | 0..7 | 14 puzzles | 1..7 | each block's start column |
| `G[0x63df1f]` | 8 | 0..7 | 14 puzzles | 1..12 | its start row |
| `G[0x63df27]` | 8 | 0..7 | 5 puzzles | 1 | its kind, which a goal `10 + kind` takes (`CARRY@0x158`) |
| `G[0x63df2f]` | 8 | 0..7 | 10 puzzles | 1 | a flag `CARRY@0x47b` tests for each push direction (not read further) |

### SIINIT (town sites, on demand)

Chain `G[0xc782d0]`, run by `TOWN@0x1af` on each visit. It branches on the place `G[0x363f]` for the towns 50 to 63
(58's branch is empty): 13 towns, 81 sites in all. It clears the town's background, `G[0x36b1]` (30 cells), each
site array (30) and the neighbours (120), then writes the town's sites, numbered from 1. The appends only open sites
(`$1$` towns 50, 51 and 59; `$2$` town 51; `$3$` nothing). `TOWN` keeps the current site in `G[0x36b0]`; a site is a
hot rectangle at its x and y, and the four neighbour cells say where each direction key goes (`TOWN@0x704`..`@0x75b`
follows them to the next open site). The site states lie in the saved prefix, but `SIINIT` rebuilds them on every
visit.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x42a90c]` | scalar |  | 13 towns | `MT050A.AGF` … | the town's background, surface `0x40` (`TOWN@0x785`) |
| `G[0x42a90d]` | scalar |  | 13 towns | 3, 4, 0x11, 0x2e | the town's music (`TOWN@0x344`) |
| `G[0x42a90e]` | 1 |  | 81 sites | 44 distinct | the site's picture (`MTS050A.AGF` …), surface `0x48 + n` (`TOWN@0x7b5`) |
| `G[0x42a92c]` | 1 |  | 78 sites | 62 distinct | the site's x (`TOWN@0x2fd`) |
| `G[0x42a94a]` | 1 |  | 81 sites | 56 distinct | its y (`TOWN@0x310`) |
| `G[0x42a968]` | 1 |  | 81 sites | 70 | the hot area's left (`TOWN@0x275`) |
| `G[0x42a986]` | 1 |  | 0 | never written | the hot area's top (`TOWN@0x2c0`) |
| `G[0x42a9a4]` | 1 |  | 81 sites | 72 | the hot area's width (`TOWN@0x29d`) |
| `G[0x42a9c2]` | 1 |  | 81 sites | 52, 72 | the hot area's height (`TOWN@0x2e8`) |
| `G[0x42a9e0]` | 4 | 0..3 | 81 sites | 1..15 | the site each of four directions leads to (`TOWN@0x704`..`@0x75b`) |
| `G[0x36b1]` | 1 |  | 81 sites | bit 0 | the site is open (`TOWN@0x1f6`) |
| `GS[0x22a8]` | 1 |  | 81 sites | 街を出る / ラギールの店 / 砂猫亭（酒場兼宿屋） | the site's name; no script indexes this base |

### ARINIT (arena matches)

Leader `GS[0x22c6]`, capacity 100: 55 records, ids 1 to 87, by layer {0: 45, 3: 10}. A record is one match of the
arena (`ARENA`, `G[0]` = 6): kind 1 a team match (団体戦), 2 a single fighter's (個人戦), 3 a special match (特殊戦:
the レンスト tournament and the inn's challenges), each a chain of rounds in which a round opens once the round before
it is won. `$3$` adds ten matches (69 to 78, 【Ver2.0追加】).

`ARENA` lists the matches from `G[0x5d7aa9]`, seven rows of 13 ids (`ARENA@0x9ef`), and skips an id whose name is
empty (`ARENA@0xa06`). It keeps the chosen match in `G[0x36cf]`. A match can be entered when the match before it is
won (`ARENA@0xb0a`..`@0xb34`), each of its three conditions is a story flag that is set and each of its three bars is
one that is not (`ARENA@0xb52`, `@0xbac`: number `n` names the flag `G[0x929 + n − 1]`), each of its five required
characters has bit 1 of `G[0x11967e + character]` (`ARENA@0xc11`), a single fighter's match has one party member
(`ARENA@0x822`..`@0x88b`), and the player has the fee (`ARENA@0x88c`); otherwise a buzzer plays. The fee comes off
the saved money `G[0x118f0d + G[0x363c]]` (`ARENA@0x8b1`..`@0x8c6`). `ARENA@0xdbe`..`@0xdf4` then sets the battle:
`G[0x63dcda]` the battle mode, `G[0x276e47]` and `G[0x276e48]` the battle, and `G[0]` = 4 (`BTL`); `SETEN@0xaf` takes
the enemies' level from the match while `G[0x1167bd]` (set by `ARENA@0xe`) is 1.

`RESULT@0x380`..`@0x42a` pays a won match only the first time: when the saved `G[0x116759 + match]` is 0 it sets it to
1, adds the prize to the battle's money and the reward items to its dropped items. A changed prize or reward reaches an
old save for any match not yet won.

The tool's view is off the grid in two places. The seven inline arrays are one array, `G[0x5d7aa9]`, 91 cells (7 ×
13) right before the first record array, not `G[0x5d7a90]` and `G[0x5d7ab4]`. The 50 cells outside are the prize
array `G[0x5d8338]`, which `RESULT@0x3a1` and `ARENA@0x14d0` index. `G[0x5d7bcc]` (5 an id) is read and never
written.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x5d7aa9]` | 13 | 0..12 | 7 rows | 1..91, 91 distinct | the order of the match list: row `r`, column `c` (`ARENA@0x9ef`, `@0xcf6`, `@0x10a3`) |
| `G[0x5d7b04]` | 1 |  | 55 | 1..3, 3 distinct 1×30, 2×16, 3×9 | the kind: 1 団体戦, 2 個人戦, 3 特殊戦 (`ARENA@0x1138`..`@0x11b4`, `@0x13cb`); 2 needs exactly one party member (`ARENA@0x822`) |
| `G[0x5d7b68]` | 1 |  | 48 | 1..85, 48 distinct | the match that must be won first (`ARENA@0xb0a`) |
| `G[0x5d7bcc]` | 5 |  | 0 | never written | five characters who must have bit 1 of `G[0x11967e + character]` (`ARENA@0xc11`); a non-zero column 0 marks the list entry (`ARENA@0xa72`) |
| `G[0x5d7dc0]` | 1 |  | 36 | 1000..10000, 10 distinct | the entry fee (`ARENA@0x893`, `@0x8bf`; drawn at `ARENA@0x14b7`) |
| `G[0x5d7e24]` | 3 | 0..2 | 55 | 1201..9043, 12 distinct | three story flags that must be set: `n` is `G[0x929 + n − 1]` (`ARENA@0xb52`) |
| `G[0x5d7f50]` | 3 | 0 | 39 | 6636..7081, 4 distinct 6636×1, 6638×1, 6641×1, 7081×36 | three story flags that close the match while set (`ARENA@0xbac`); 7081 is `G[0x24d1]`, which `SC1200@0x8fe` sets |
| `G[0x5d807c]` | 1 |  | 9 | 2..3, 2 distinct 2×5, 3×4 | the battle mode, into `G[0x63dcda]` (`ARENA@0xdbe`), on which `CSINIT` branches |
| `G[0x5d80e0]` | 1 |  | 55 | 160..500, 31 distinct | the enemies' level (`SETEN@0xaf`) |
| `G[0x5d8144]` | 2 | 0 | 55 | 901..1410, 55 distinct | the battle, into `G[0x276e47]` (column 0) and `G[0x276e48]` (column 1, never written) (`ARENA@0xdd4`, `@0xde4`) |
| `G[0x5d820c]` | 3 | 0..2 | 55 | 1201..6640, 11 distinct | scenes: column 0 runs before the battle (`ARENA@0x8d0`: `G[0]` = 1, `G[0x269b30]`), column 1 after it when `G[0x276e99]` is 1, column 2 when it is 0 (`BTL@0xa20a`, `@0xa261`); column 1 is `SC1800` on all but one, so 1 reads as a loss and 0 a win (inferred) |
| `G[0x5d8338]` | 1 |  | 50 | 3000..500000, 26 distinct | the prize money, paid on the first win (`RESULT@0x3a1`; 賞金, `ARENA@0x14d0`) |
| `G[0x5d839c]` | 2 | 0..1 | 49 | 23..2970, 47 distinct | two reward items (賞品), added to the battle's dropped items on the first win (`RESULT@0x3ca`, `@0x3fc`; `ARENA@0x1506`) |
| `G[0x5d8464]` | 2 | 0..1 | 49 | 1..5, 3 distinct 1×49, 2×2, 5×16 | how many of each (`RESULT@0x413`, `ARENA@0x1518`) |
| `GS[0x22c6]` | 1 |  | 55 | 団体戦A　１回戦 / 団体戦A　２回戦 / 団体戦A　３回戦 | the match's name (`ARENA@0x9ff`, `@0x142e`) |
| `GS[0x232a]` | 5 | 0..4 | 55 | 勝ち抜きバトル団体戦　セリカ勢力１回戦 / 自分の実力を試す、または名声を得る為に是非 / なし | five lines of description (`ARENA@0x1441`..`@0x149a`) |

### The war game: SOINIT, TPINIT, CAINIT, RUINIT, GRINIT and STINIT

`SLG` (`G[0]` = 8) is the war game: armies of troops, each led by up to four commanders, on a grid of terrain. A
scene sets the stage `G[0x262e7d]` and enters `SLG`: `SC2400` stage 1, `SC1350` and `DEBUGSLG` stage 2
(`DEBUGSLG@0x27`, `@0x2c`), `SC5000` stage 3, `SC2350` stages 4 and 5. `SLG@0x2df` runs `STINIT` through its chain
`G[0xc783d0]`. The boot tables give the kinds: terrain (`SOINIT`), troops (`TPINIT`), commanders (`CAINIT`), tactics
(`RUINIT`) and ranks (`GRINIT`). `TACTICS`, which `BTL@0x2bf7` calls, reads none of them. Their arrays run on
without a gap, `SOINIT`'s into `TPINIT`'s into `CAINIT`'s into `RUINIT`'s, and so do their strings.

At a battle's start `SLG@0x3ba`..`@0x4f2` copies the tables into the saved unit state: a troop's soldiers into
`G[0x263044]`/`G[0x263062]`, its morale into `G[0x263080]`, its four numbers plus the rank's bonus into
`G[0x266385]`, its tactic and its commanders' tactics into `G[0x263116 + 5 × unit + …]`. A changed table reaches a
save made outside a battle; one made during a battle keeps the old numbers until it ends.

#### SOINIT (terrain)

Leader `GS[0x10cb0]`, capacity 50: 10 records, ids 0 to 9 (壁 平野 森 浅瀬 川 山岳 崖 瓦礫 床 橋).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4b25d]` | 4 | 0..3 | 10 | 0..3, 4 distinct 0×10, 1×21, 2×7, 3×2 | the move cost for each move type (`TPINIT`'s `G[0xc4b361]`: foot, cavalry, water dragon, flying); 0 cannot enter (`MVSEEK@0x204`, `SETROUTE`, the `RTN_M031`..`035` scripts) |
| `GS[0x10cb0]` | 1 |  | 10 | 壁 / 平野 / 森 | the terrain's name (`SLG@0x302e`) |

#### TPINIT (troops)

Leader `GS[0x10ce2]`, capacity 30: 17 records, ids 1 to 13, 21 to 23 and 29 (セリカ). A stage's unit names its troop.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4b325]` | 2 | 0..1 | 16 | 18737..18768, 32 distinct | the troop's map sheet for side 1 (`TP001A.AGF`) and side 2 (`TP001B.AGF`) (`SLG@0x2116`) |
| `G[0xc4b361]` | 1 |  | 17 | 0..3, 4 distinct 0×12, 1×1, 2×1, 3×3 | the move type, a column of `SOINIT`'s costs: 0 foot, 1 cavalry, 2 water dragon, 3 flying (`MVSEEK@0x4c`) |
| `G[0xc4b37f]` | 4 | 0..3 | 17 | 0..10, 10 distinct | move (the range is the value plus 1, `MVSEEK@0x33`), attack, defence and magic; `SLG@0x428` copies them into the unit, `CALCRUSH` reads them |
| `G[0xc4b3f7]` | 1 |  | 16 | 1..33, 6 distinct | the troop's own tactic, a `RUINIT` id (`SLG@0x48e`) |
| `G[0xc4b415]` | 1 |  | 16 | 400..1000, 5 distinct | its soldiers (`SLG@0x3cd`, into `G[0x263044]` and `G[0x263062]`) |
| `G[0xc4b433]` | 1 |  | 17 | 50..100, 4 distinct 50×3, 80×2, 90×1, 100×11 | its starting morale (`SLG@0x3f3`, into `G[0x263080]`); `CALCRUSH` multiplies attack by `(100 + morale) / 100` |
| `G[0xc4b451]` | 1 |  | 17 | 30..100, 7 distinct | the gauge points its rout is worth, when its morale falls below 0 (`SLG@0x1aa1`) (inferred) |
| `GS[0x10ce2]` | 1 |  | 17 | 歩兵隊 / 騎兵隊 / 弓兵隊 | the troop's name (`SELACT`, `SLG`) |

#### CAINIT (commanders)

Leader `GS[0x10d00]`, capacity 200: 19 records, ids 1 to 157. The tool counts one cell outside, `G[0xc4b72c]`: it is
record 101's cell of `G[0xc4b6c7]`, an array of its own on the grid, which only that record writes.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4b46f]` | 1 |  | 19 | 1..10, 9 distinct | leadership: it divides the unit's morale loss (`SLG@0x19e1`; drawn at `SLG@0x3323`) |
| `G[0xc4b537]` | 1 |  | 19 | 4..10, 7 distinct | the attack of the commander's tactics (`CALCRUSH@0x78`; drawn at `SLG@0x3350`, `SELACT@0xb9d`) |
| `G[0xc4b5ff]` | 1 |  | 13 | 11..37, 9 distinct | the commander's tactic, a `RUINIT` id (`SLG@0x4eb`, into `G[0x263116 + 5 × unit + 1 + k]`) |
| `G[0xc4b6c7]` | 1 |  | 1 | 319 | the 一騎討ち battle: `SLG@0x15fe` puts it in `G[0x276e47]` and enters `BTL` (`G[0]` = 4). Only commander 101 has one |
| `G[0xc4b78f]` | 2 | 0..1 | 17 | 13683..13716, 34 distinct | column 0 the commander's map sprite (`CPS001A.AGF`, `SLG@0x20dc`), column 1 a larger sheet (`CPS001B.AGF`, `SLG@0x3958`) |
| `GS[0x10d00]` | 1 |  | 19 | リウイ / カーリアン / ファーミシルス | the commander's name |

#### RUINIT (tactics)

Leader `GS[0x10dc8]`, capacity 100: 16 records, ids 1 to 3, 11, 21 to 25 and 31 to 37.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4b91f]` | 1 |  | 16 | 0..3, 4 distinct 0×3, 1×5, 2×7, 3×1 | the kind: 0 a troop's basic attack, 1 a commander's attack, 2 magic, 3 a duel. Magic uses the stat column 3, the others column 1; at 3 `CALCRUSH` returns (`CALCRUSH@0x21`) |
| `G[0xc4b983]` | 1 |  | 16 | 0..4, 4 distinct | the least range (`CALCSCOPE`; drawn "b-c" at `SLG@0x3441`) |
| `G[0xc4b9e7]` | 1 |  | 16 | 0..5, 6 distinct | the greatest range (`CALCSCOPE`; `SLG@0x345e`) |
| `G[0xc4ba4b]` | 1 |  | 15 | 10..15, 4 distinct | the power multiplier (`CALCRUSH@0xbf`) |
| `G[0xc4baaf]` | 1 |  | 16 | 1..3, 3 distinct 1×6, 2×7, 3×3 | the cooldown: 1 always usable, `n` unusable for `n − 1` turns after use (`SELACT@0xb20`, `SLG@0x1891`) |
| `G[0xc4bb13]` | 1 |  | 15 | 1..3, 2 distinct 1×13, 3×2 | the effect's blend mode: 1 additive, 3 subtractive (dark magic) (`SLG@0x3b85`) |
| `G[0xc4bb77]` | 1 |  | 15 | 17522..17536, 15 distinct | the effect's animation (`MVR001.AGF` …), played as a movie (`SLG@0x3b5d`) |
| `GS[0x10dc8]` | 1 |  | 16 | 突撃 / 射撃 / 砲撃 | the tactic's name |
| `GS[0x10e2c]` | 1 |  | 16 | 汎用基本攻撃 / 弓兵用基本攻撃 / 砲兵用基本攻撃 | its description (`SELACT@0xb10`) |

#### GRINIT (ranks)

Leader `GS[0x10e90]`, capacity 10: 5 records, ids 1 to 5 (勇士 豪将 英傑 覇王 戦神). `SLG@0x231` gives the player
the highest rank whose points are at most the saved war points `G[0x26637e]`. The array after it, `G[0xc77b2d]`, is
not `GRINIT`'s: it is the mode-9 script selector of `SYSTEM4`.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc77afb]` | 1 |  | 4 | 1500..20000, 4 distinct | the points the rank needs (rank 1 none) (`SLG@0x231`) |
| `G[0xc77b05]` | 4 | 0..3 | 5 | 0..3, 4 distinct | bonuses to the four numbers of the player's units (`SLG@0x463`) |
| `GS[0x10e90]` | 1 |  | 5 | 勇士 / 豪将 / 英傑 | the rank's name |

#### STINIT (war stages, on demand)

Chain `G[0xc783d0]`, run at `SLG@0x2df`. It branches on the stage `G[0x262e7d]`: five stages, 56 units (11, 20, 13,
6, 6); the appends are empty. Every cell it writes lies in the saved prefix, so a save keeps the stage as it was set
up, and a changed `STINIT` reaches an old save at the next battle's start. A unit is a slot 1 to 29.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x262f34]` | scalar |  | 5 stages | `ST001.AGF` … `ST005.AGF` | the battlefield's picture |
| `G[0x262e7e]` | 18 | 0..17 | 5 stages | `SOINIT` ids | the terrain, 10 rows of 18 (`18 × row + column`), copied from inline rows |
| `G[0x26637d]` | scalar |  | 5 stages | 500, 800, 1000, 2000 | the war gauge's start; `SLG@0x2fa` copies it into `G[0x266380]`, which is bounded at twice it (`SLG@0x1ae1`) |
| `GS[0x1a8]` | scalar |  | 5 stages | メンフィル軍 / メンフィル連合軍 / メンフィル赤軍 | the player's army's name (a saved string) |
| `GS[0x1a9]` | scalar |  | 5 stages | ザミルザーニィ軍 / エディカーヌ軍 / レウィニア軍 | the enemy's army's name (a saved string) |
| `G[0x262f36]` | 1 |  | 56 units | 1, 2 | the unit's side |
| `G[0x262f54]` | 1 |  | 56 units | 15 distinct | the unit's troop, a `TPINIT` id (`SLG@0x3ba`) |
| `G[0x262f72]` | 4 | 0..3 | 56 units | 19 distinct | up to four commanders, `CAINIT` ids |
| `G[0x266312]` | 1 |  | 56 units | 0..16 | the unit's start column (`SLG@0x338`) |
| `G[0x266330]` | 1 |  | 56 units | 0..8 | its start row (`SLG@0x34d`) |
| `G[0x26634e]` | 1 |  | 56 units | 0, 2, 3 | its facing (`SLG@0x3ae`, `SETRELIEF@0xfa`) |
| `G[0x2662d6]` | 1 |  | 11 units | 1 | a mark on two or three units a stage: `DRAWUN@0x17c` draws an emblem on the unit and `SLG@0x1d72` counts it in the check whether a side is beaten (what the mark decides is not read) |
| `G[0x2662f4]` | 1 |  | 1 unit | 1 | the unit is not placed at the start (`SLG@0x32a` places only units with 0 here and a position); `SETRELIEF` brings units in later (inferred) |
| `G[0x2659ea]` | 1 |  | 29 units | 2..11 | the enemy unit's routine: `MVRTN@0x87` reads `G[0xc4bbdb + 20 × routine + step]` |

## Information screens

### FAINIT (objectives)

Leader `GS[0xf860]`, capacity 2000; 120 ids, 119 by the base script and one by `$1$` (id 19, 第三章 リウイ、廃滅の
地にて…). Each record writes its sort key first, ahead of the leader (`FAINIT@0x0`, then `@0x5`).
`INFO` calls `INFO1`, the objectives screen: it sorts ids 0 to 199 by the key (`sort-indices-by-key-sum`,
`INFO1@0x812`) and lists each that has text and whose flags allow it. No chain carries its text.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc4584e]` | 1 |  | 120 | 10..990, 103 distinct | the sort key (`INFO1@0x812`) |
| `G[0xc4601e]` | 3 | 0..1 | 120 | 1..8115, 110 distinct | flags, each a flag number + 1, that must all be set (`G[0x929 + value − 1]`, `INFO1@0x8a1`) |
| `G[0xc4778e]` | 3 | 0 | 9 | 109..3206, 7 distinct | flags, each a flag number + 1, none of which may be set (`INFO1@0x8fb`): the objective is done |
| `GS[0xf860]` | 1 |  | 120 | 序章 / 第一章 / 第一章 | the chapter heading (`INFO1@0xd0f`) |
| `GS[0x10030]` | 1 |  | 120 | セリカ、レスペレント地方で始動 / リウイ、フェミリンス神殿の再調査を開 / ディアーネ、エクリアの監視に派遣され | the objective; an empty one is not listed (`INFO1@0x849`, drawn at `@0xd26`) |

### VIINIT (glossary)

Leader `GS[0x108c8]`, capacity 1000; 107 ids, 1 to 107, all by the base script. `INFO2` is the glossary screen.
It lists an entry whose title is not empty (`INFO2@0x99c`) when its first unlock scene is 0, when the shared
profile has seen any of its scenes (`G[0x25febb + value − 1]`, `INFO2@0xa0c` to `0xa56`), or when `G[0xc0ba06]`
is set (`INFO2@0x9c7`), and shows the share of entries seen. The entry's text comes through the `VIMES` chain:
`INFO2@0xfce` puts the id in `G[0xc49477]`, the cell before the scenes, and `VIMES` branches on it.

The tool's leader, `G[0xc49478]` with stride 3, finds 81 records: 26 entries write no unlock scene. The title
starts every record: `Leader("GS", 0x108c8, 1000)` finds all 107, with the same arrays.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0xc49478]` | 3 | 0..2 | 81 | 1..6073, 59 distinct | scenes, each a scene id + 1, any of which unlocks the entry once seen (`INFO2@0xa0c`) |
| `G[0xc4a030]` | 1 |  | 107 | 18769..18822, 54 distinct | the entry's picture, `VI000` to `VI081` (`INFO2@0xfe1`) |
| `G[0xc4a418]` | 1 |  | 107 | 50..50, 1 distinct 50×107 | the picture's x offset from the centred position (`INFO2@0x1027`) |
| `G[0xc4a800]` | 1 |  | 50 | 33..33, 1 distinct 33×50 | the picture's y offset (`INFO2@0x103a`) |
| `GS[0x108c8]` | 1 |  | 107 | 【用語】二つの回廊の終わり / 【用語】三神戦争 / 【用語】先史文明期 | the entry's title (【用語】 a term, 【神名】 a god); empty means not listed (`INFO2@0x99c`) |

## Quests

### QUINIT (quests, on demand)

`QUINIT` is not a boot table. The `INN` walks its chain `G[0xc78310]` on entry (`INN@0x8a`), and `INIT2@0x24e`
walks it on a load in an inn, so its arrays are not in the boot image; this section reads the listings. It
clears its arrays and writes 20 quests for every inn (ids 1 to 72), then branches on the inn `G[0x118ecf]`, 1 to 9
(`QUINIT@0x5e4`, `@0x785`, `@0xbc9`, `@0xe7d`, `@0xf9c`, `@0x108e`, `@0x1270`, `@0x1353`, `@0x1422`), which
writes the inn's own, ids 101 to 126: 7, 18, 12, 6, 4, 8, 4, 4 and 3. `$1$QUINIT` adds quest 127,
【地元】廃滅の地の薬品不足, to inn 2; `$2$` and `$3$` are empty. Each branch also sets the inn's two pictures,
`G[0xc0ba08]` (`BG001A` or `BG029A`, the background, texture `0x7e`) and `G[0xc0ba07]` (`CS082AA` or `CS081AA`,
texture `0x7f`; `INN@0xba2`).

A quest's id is its row and its slot: ids 1 to 99 are row 0, shared by every inn, and ids 100 to 199 are the
current inn's row (`INN@0x845`: row = id / 100 × inn). Its state is the saved `G[0x1167be + 100 × row + id % 100]`:
bit 0 taken (`INN@0x728`), bit 17 + party the party that took it, or bit 20 where `G[0x363d]` is set
(`INN@0x73f`, `@0x759`), bit 1 cleared (`QUESTCLEAR@0x1e6`). `CHECKCONTRACT` asks whether the party in play holds
the quest `G[0x5d852d]` and answers in `G[0x5d852e]`: bit 20 always counts, and so do bits 17 to 19 where
`G[0x363d]` is set, or bit 17 + `G[0x363c]` where it is not (`CHECKCONTRACT@0x21` to `0x5c`). The board (`INN@0x81a`) goes through ids 0 to 199: one with a difficulty lists if it is cleared, or
taken and held by this party, or open (`INN@0x93f`: its prerequisite cleared, its party right, its flags right).
`QUESTCLEAR` pays the reward and sets bit 1.

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x5d852f]` | 1 |  | 39 | 1..66, 19 distinct | the quest that must be cleared first, a slot of the same row (`INN@0x944` to `0x980`) |
| `G[0x5d85f7]` | 1 |  | 87 | 1..2, 2 distinct | the party it is for, party + 1: while the parties are apart (`G[0x363d]` 0) only that party sees it; 4 would show it only while they are together (`INN@0x988` to `0x9e7`) |
| `G[0x5d86bf]` | 3 | 0..2 | 66 | 11..9003, 42 distinct | flags, each a flag number + 1, that must all be set (`INN@0xa05` to `0xa3e`) |
| `G[0x5d8917]` | 3 | 0..1 | 27 | 51..8113, 26 distinct | flags, each a flag number + 1, none of which may be set (`INN@0xa5f` to `0xa98`) |
| `G[0x5d8b6f]` | 1 |  | 87 | 1..5, 5 distinct | the difficulty, drawn as that many ★ (`INN@0xf11` to `0xf34`); a slot without one is no quest (`INN@0x837`) |
| `G[0x5d8c37]` | 1 |  | 74 | 1000..81000, 15 distinct | the money reward, 賞金 (`INN@0x1204`); `QUESTCLEAR@0x10e` adds it to the party's money `G[0x118f0d + G[0x363c]]`, up to 9,999,999 |
| `G[0x5d8cff]` | 2 | 0..1 | 24 | 106..2968, 28 distinct | the item rewards, 賞品, item ids of `ITINIT` (`INN@0x1285` names them from `GS[0x2f78 + item]`: 114 勇壮の水・小); `QUESTCLEAR@0xe8` gives them through `ADDITEM` |
| `G[0x5d8e8f]` | 2 | 0..1 | 24 | 1..5, 4 distinct | how many of each item (`INN@0x12ba`, `QUESTCLEAR@0xf8`) |
| `GS[0x251e]` | 1 |  | 87 | 【組合】日向の薬草探し / 【組合】蒼涼の粘液探し / 【組合】天使の涙探し | the quest's name (`INN@0xef2`, `@0x1155`; `QUESTCLEAR@0x2d` logs 依頼達成：) |
| `GS[0x25e6]` | 6 | 0..5 | 87 | 『日向の薬草』を探しています。 / 旅の途中に見付けたらぜひ提供してください。 / 【依頼主】　修道女 | 0 to 2 the description, its last line the client (【依頼主】); 3 to 5 the three hints, 情報① to ③ (`INN@0x1168` to `0x11e7`) |

The ids count records over every branch (87 records, 40 distinct ids). The leader would be
`Leader("GS", 0x251e, 200)`; `init_layout.py` does not lay out on-demand tables.

## Galleries

`ROOM` runs `SETMEMOIR` (`ROOM@0xb9`), which builds the three galleries' pages from `CGINIT`, `SPINIT` and
`MUINIT` into cells above the saved prefix, and computes each one's share unlocked from the engine's catalog
(`is-catalog-resource-unlocked`): `G[0x26d9b2]`, `G[0x275269]` and `G[0x275366]`.

### CGINIT (CG gallery)

Leader `G[0x269b32]` with stride 2, capacity 2000; 1035 ids, 1 to 1035. Its arrays line up 2000 apart. An append's
record writes its number first, ahead of the leader (`$1$CGINIT@0x0` writes `G[0x26d54a]`, then the leader at
`@0x5`). Records by layer: the base script 4037 writes, `$1$` 105, `$2$` 73, `$3$` 378. 970 records have a place
in the gallery; 65, from id 892 (`CSE022AA`), have only pictures.

`SETMEMOIR` puts each placed CG at `G[0x26d9b5 + 3000 × (tab − 1) + 100 × (slot − 1) + (variant − 1)]`, its id where
its picture is unlocked and −1 where not, counts each slot's CGs into `G[0x275011]` and unlocked ones into
`G[0x274ee5]` (`[10][30]` each), and closes up the empty slots of tab 7 (`SETMEMOIR@0x12b`, row 6). `CGMODE` draws a
tab's page as six columns and five rows of thumbnails (`CGMODE@0x14f6` to `0x1520`); a slot's variants are its CGs
in order (inferred from `G[0x26d9b5]`'s shape).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x269b32]` | 2 | 0..1 | 1035 | 98..50332414, 1173 distinct | 0 the CG's picture (`CGMODE@0xb74`; `EV010BB`); 1 on 546, a 204 × 115 picture (`EVM010BB`) that `SAVE@0xdb5` and `ALLMAP@0x3ff` draw as the save slot's thumbnail when the screen shows picture 0 (`SC0040` and `SC4300` do the same) |
| `G[0x26aad2]` | 1 |  | 970 | 1..7, 7 distinct | the gallery's tab (`SETMEMOIR@0x5e`) |
| `G[0x26b2a2]` | 1 |  | 970 | 1..30, 30 distinct | the slot on the tab's page (`SETMEMOIR@0x6c`; `CGMODE@0x14e8` places its thumbnail) |
| `G[0x26ba72]` | 1 |  | 970 | 1..18, 18 distinct | the variant within the slot; 0 means not in the gallery (`SETMEMOIR@0x50`, `@0x7a`) |
| `G[0x26ca12]` | 1 |  | 3 | 261..432, 2 distinct 261×2, 432×1 | the y scroll of a picture larger than the screen, negated (`CGMODE@0xbd4`). `G[0x26c242]`, the x scroll, is read (`CGMODE@0xbc1`) and never written |
| `G[0x26d1e2]` | 1 |  | 99 | 1..3, 3 distinct 1×20, 2×13, 3×66 | the append that adds the CG; its thumbnail is on that append's sheet, texture `0x5e + value` (`CGMODE@0x14da`; the sheets come from chain `G[0xc78130]`) |

### SPINIT (scene replay)

No leader is set. The table is one array of 10 rows of 15, `G[0x27513d + 15 × row + column]`, and
`Leader("G", 0x27513d, 150, group=15)` lays it out by cell. The base script fills rows 0 to 4 (67 scripts);
`$1$`, `$2$` and `$3$` fill row 5's columns 0, 1 and 2 to 7. `HMODE` is the replay screen: it shows six pages
where an append is mounted and five otherwise (`HMODE@0x19`), and runs the chosen cell's script
(`HMODE@0xbc1`). `SETMEMOIR` marks each cell `column + 1` where its script is unlocked and −1 where not, in
`G[0x27526c]` (`[10][15]`, `SETMEMOIR@0x266` to `0x2bd`), and closes up row 5 (`SETMEMOIR@0x2ca`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x27513d]` | 15 | 0..14 | 6 | 80..50332248, 75 distinct | a page's scenes to replay, packed script ids (`SP2712.BIN`, …; row 5 `$1$SP5903.BIN`, `$2$SP5931.BIN`, `$3$SP5941.BIN` …), run by `HMODE@0xbcc` |
| `G[0x2751d3]` | 15 | 0..7 | 1 | 1..3, 3 distinct 1×1, 2×1, 3×6 | row 5 only: the append that adds the scene; its thumbnail is on texture `0x5e + value` (`HMODE@0x1020`; the sheets come from chain `G[0xc78150]`) |

### MUINIT (music room)

Leader `G[0x275334]`, capacity 50; 46 ids, 0 to 45, all by the base script. `MMODE` is the music room
(鑑賞するＢＧＭを選択してください): it plays a track where `SETMEMOIR` marked it unlocked (`G[0x275369 + id]`,
`SETMEMOIR@0x393`; `MMODE@0x4e7` to `0x511`), and shows the counts `G[0x275367]`, `G[0x275368]` and the share
`G[0x275366]` (`MMODE@0x5d6`).

| array | cells per id | columns written | ids | values | holds |
|---|---|---|---|---|---|
| `G[0x275302]` | 1 |  | 46 | 2..1531, 46 distinct | the catalog resource that unlocks the track: the track's own file (`BGM041.OGG` for track 41), or for the theme songs the script that plays them (`OP.BIN` for id 0, `ED2.BIN` and `ED.BIN`) (`SETMEMOIR@0x38c`) |
| `G[0x275334]` | 1 |  | 46 | 2..52, 46 distinct | the BGM number to play (`MMODE@0x50a`, `play-bgm`; 41 plays `BGM041.OGG`) |

## Saved state

A save keeps the integer cells below `0x268b19` and the string cells below `0x1b6` (the profile's
`persistence.bankDimensions`; [`sys4-format-notes.md`](sys4-format-notes.md), "Saved prefix"). Every cell the 37
boot tables write lies above both, and so do the cells of the on-demand tables except `STINIT`'s, `SIINIT`'s site
states and `AMINIT`'s area states, which lie in the prefix and are written afresh each time the table runs. So boot,
or the screen that runs a table, rebuilds the table, and a changed table reaches an old save as well as a new game.

The other direction is whether the game copies a table's value into a saved cell and reads the copy afterwards.
Each table's section ends with what its readers found: the copies, and the keys a save holds state by. In short:

- **A value copied once.** A joining character's equipment, money, drop items, base level and starting title
  (`UNITECH`, from `EBINIT`, `CHINIT` and `CBINIT`'s seven parameters); a character's stats, rewritten from
  `EBINIT`'s growth at each level up (`CALCLVUP`); a title's 特技 (`ADDTITLE`); a condition's duration
  (`ADDILL`); a map block's music (`FIELD`, `RMINIT`); an area's or map's start (`FIELD`, `TOWN`, from `AMINIT2` and
  `MPINIT`); a box puzzle's start (`SETMAPDATA`, from `PZINIT`); a war stage's set-up (`SLG` and `STINIT`); a
  turned object's next kind (`OBINIT`); and the money of a match, quest, recipe or drop when it is paid. A changed
  value reaches these in an old save only where the copy is made again: at the next level, entry, battle or payment,
  never for what the save has had.
- **Rebuilt from the tables.** A character's lasting conditions (`CALCREVISE`, from `SSINIT` and `ITINIT`, also on
  every load: "INIT2" above), the world map's open areas (`AMINIT`) and the town's sites (`SIINIT`).
- **State kept by id or slot.** The save keeps item counts, obtained marks, growth and equipment by item id; titles
  earned and the current title by title id; opened treasures by treasure id; quest states, skill-tree bits and
  proficiency by **slot**; arena wins by match. A mod that renumbers an id or moves an entry to another slot meets the
  old one's state in an old save.
- **Story and progress.** The flags `G[0x929 + n]`, the chapter, the area, the parties and the scene queue
  ("Scene dispatch"), and the boot values of "INIT" are saved; `INIT2`'s constants are not. `SYSTEM4@0x366` copies a
  scene's script id (`SCINIT`) into the saved `G[0x90f]`, so a save made in a scene keeps the script it had
  (inferred: a changed `G[0x4256a0]` does not reach that scene). The seen cells `G[0x25febb + scene]` lie below the
  prefix too, but `SYSTEM4` stores them in the shared profile.
- **Galleries.** No table value is copied: `SETMEMOIR` rebuilds the pages each time `ROOM` runs, and what is
  unlocked is the engine's catalog and the shared profile, not the save.

The search behind these notes looked, table by table, for a write into a cell below `0x268b19` whose value reads a
table cell, in the listings of the scripts that read the table. A copy that passes through another global cell or a
local of another script is not ruled out.

## What is not decoded

- **Arrays with no reader, or no writer.** No script reads `EBINIT`'s full name `GS[0x12a4]`, `ITINIT`'s
  `G[0x9c7b69]` and the authors' notes `GS[0x5688]`, `SSINIT`'s level descriptions `GS[0x8cd4]` (`SSMES` prints its
  own copy), `AFINIT`'s short names and equipment notes, `MPINIT`'s floor numbers, `LAINIT`'s `G[0x5f2267]` and
  `G[0x5f22cb]`, `RTINIT`'s bank 7, `SRINIT`'s banks 4 to 6, `BTANINIT`'s `G[0xc456c2]`, and `RMINIT`'s
  `G[0x5db2e3]` and `G[0x5dbe9b]`. Some arrays are read but no record writes them: `SSINIT`'s HP cost, `CSINIT`'s two
  story-flag tests, `ARINIT`'s required characters, `SIINIT`'s hot area top, `RTINIT`'s and `SRINIT`'s story-flag
  banks, `BTANINIT`'s sounds, and `CGINIT`'s x scroll.
- **Fields read but not understood.** `RMINIT`'s `G[0x5db8bf]` and `G[0x5dbbad]` are copied, and nothing is read of
  what they drive; `BTANINIT`'s cue `G[0xc457e2]`; `RTINIT`'s bank 4 for rules 9 and 302; the war-unit mark
  `G[0x2662d6]`; the battle gauge `G[0xc1a960]` and rule 8's `G[0xc1ac6c]` and `G[0xc1aca8]`; `SETEN`'s party level
  `G[0x2756d3]`; `CBINIT`'s parameters' names; most of `INITCONFIG`'s options, whose labels are pictures.
- **Scripts not read.** `RTN_M021` to `RTN_M062` (the war game's routine rules), and `BTLMENU`, `CALCDAMAGE`,
  `CALCSCOPE`, `SELTARGET` and `USESKILL` beyond the cells they share with the tables.
- **Scene dispatch.** What `G[0x363d]` is, the 25 tags of `G[0x427db0]` that differ from the context naming the
  scene, and the five values of a location's scene. `tools/scjump_decode.py` was written for Himegari and was not
  run on Verita.
- **The six tables without a leader** (AFINIT, SKINIT, IMINIT, EEINIT, TRINIT, EX1INIT) are shown by the arrays the
  scripts index, which can lump an array no script indexes with the one before it; their sections give the arrays
  read from the listings instead.
- **Three oddities in the data.** Animation 7102's slot 2 names effect 4123, which no layer defines; routine 370's
  step 1 uses rule 30, which has no script; and unit 179 取り憑かれた大鼠's first action is item 5423, which no layer of
  `ITINIT` writes (the game data export's one missing reference).
