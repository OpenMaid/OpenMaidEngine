<!-- DO NOT EDIT -- generated from vm-map/patch-points-koukaku.toml by tools/patch_points_build.py --build -->
# Named patch points — koukaku (generated)

Stable site names for OMS fragments (`before site <name> { … }`; mod loader spec §5,
`docs/superpowers/specs/2026-11-03-mod-loader-design.md`). Each point lists its site per script revision;
a fragment compiles only against a revision its point lists.

## `koukaku.battlefield.after_map_loaders_checkreg`

Script: `CHECKREG.BIN`

A region's map is written as the region opens: CHECKREG has set battlefield_map_id to the map of region_arg_id, G[0x8e0330 + region], and walked the MPINIT chain (CHECKREG@0xa4 to 0xde). It then runs SETFORT (CHECKREG@0xe1), the region's first setup, which places the map's preset objects in the region's saved layers.

| Revision | Site |
|---|---|
| `sha256:1dad0c0177425e096db79a6058262577df4a7682e42ab2dcc88d9d6fedf67c8a` | `0xe1` |

## `koukaku.battlefield.after_map_loaders_delcn`

Script: `DELCN.BIN`

A region's map is written as a nation is removed: DELCN has found the region not yet set up (bit 1 of region_flags clear), set battlefield_map_id to its map, G[0x8e0330 + region], and walked the MPINIT chain (DELCN@0x9a to 0xd4). It then runs SETFORT (DELCN@0xd7).

| Revision | Site |
|---|---|
| `sha256:e30b1a4c4f80b9f3815f08cf475c25295d98bf5867da5d7729ca67a0318b8d7a` | `0xd7` |

## `koukaku.battlefield.after_map_loaders_field`

Script: `FIELD.BIN`

A battle's map is written: FIELD has set battlefield_map_id to the battle's map (G[0xc07c], a story battle's from DUINIT or the region's G[0x8e0330 + region]) and walked the MPINIT chain (FIELD@0x806 to 0x840). FIELD then marks the entries (FIELD@0x843 to 0x924) and runs SETFIELD (FIELD@0x946), which copies the map's size, bounds and picture and builds the land grid from it, and, on the region's first setup, SETFORT, which places the map's preset objects in the region's saved layers. FIELD skips the walk while G[0x8858] is 1, and this point with it.

| Revision | Site |
|---|---|
| `sha256:196a9b17596b7c766d57926cd700e2bd6ae542d181a7192aa61fcabf418b0c9f` | `0x843` |

## `koukaku.battlefield.after_map_loaders_occupy`

Script: `OCCUPY.BIN`

A captured region's map is written: OCCUPY has found the region not yet set up (bit 1 of region_flags clear), set battlefield_map_id to its map, G[0x8e0330 + region], and walked the MPINIT chain (OCCUPY@0x193 to 0x1cd). It then runs SETFORT (OCCUPY@0x1d0).

| Revision | Site |
|---|---|
| `sha256:8e54c960c6c4280545eee73a6eca666058ba62db74d4533317c5991945f705f7` | `0x1d0` |

## `koukaku.battlefield.after_map_loaders_palace`

Script: `PALACE.BIN`

The palace's map is written: PALACE has set battlefield_map_id to 1 and walked the MPINIT chain (PALACE@0x25 to 0x5f). It then copies the map's columns and rows (PALACE@0x62, @0x67).

| Revision | Site |
|---|---|
| `sha256:18f440c1c8b494e70f2f194b6080995aa963538e7f555d827bf5b3559317d080` | `0x62` |

## `koukaku.battlefield.after_map_loaders_reign`

Script: `REIGN.BIN`

The region map's battlefield is written: REIGN has set battlefield_map_id to the map of the region in play, G[0x8e0330 + G[0x19a329]], and walked the MPINIT chain (REIGN@0x145 to 0x17f). While G[0x8858] is 0, REIGN then runs SETFIELD for the region (REIGN@0x190, @0x195).

| Revision | Site |
|---|---|
| `sha256:0ee6bd2acf660961b6a22a16418c997d28339810b5b0793842a2f6cee4ca5f02` | `0x182` |

## `koukaku.boot.data_layered`

Script: `INIT2.BIN`

The instruction after op 0x143 (INIT2@0x33b). Every layer has written its data: the 37 base *INIT scripts, INIT2's hook chains and constants (INIT2@0x6f to 0x335), then the append's $1$AUTORUN, which runs its 36 $1$*INIT layers and fills slot 1 of the chains. INIT2 goes on to give each of CVINIT's colours to the shared profile where it left none (INIT2@0x33b to 0x39f), and, on a load where G[0x0] and G[0x38a5] are 1 and G[0x2] is 2 or 4, to rebuild the screen in play from the saved state (READMAP, or READFIELD, READREIGN and READCH; INIT2@0x3a2 to 0x3eb), which reads the tables. The boot tables lie above the saved prefix (G[0x211c08], GS[0x1b1]), so a save holds none of them: SYSTEM4 runs INIT2 on every boot and again while a load restores (SYSTEM4@0x2aa, before op 0xae at SYSTEM4@0x2ee), so control reaches here again then, with the save's cells back.

| Revision | Site |
|---|---|
| `sha256:e23fed1be20881df5ae8cd16691944060bef5136cf20fe64a88c6cddc99a99f7` | `0x33b` |

## `koukaku.boot.data_ready`

Script: `INIT2.BIN`

INIT2's exit. Every data table is built: the 37 base *INIT scripts, INIT2's hook chains and constants, the append's $1$AUTORUN and its $1$*INIT layers (op 0x143), and INIT2's own work after them (koukaku.boot.data_layered): the colours, and on such a load the rebuilt screen. An edit made here comes after that work. SYSTEM4 runs INIT2 on every boot and again while a load restores, so control reaches here again then.

| Revision | Site |
|---|---|
| `sha256:e23fed1be20881df5ae8cd16691944060bef5136cf20fe64a88c6cddc99a99f7` | `0x3ee` |

## `koukaku.experience.before_add`

Script: `RESULT.BIN`

One character's experience from a battle, before RESULT adds it: the add character_experience[c] += G[0xc0cd + c] (RESULT@0xbb6), its third operand the character's experience from the battle, which FIELD clears as the battle starts (FIELD@0x32e), its attacks and skills add to (FIELD@0x8225, EXECSKILL), and RESULT raises by 5 or 2 and caps at 999 (RESULT@0xb29 to 0xb80). The character, RESULT's local 0x275, is the commander of one of the player's units (unit_commanders[nation_player][slot], RESULT@0xb19), and the add runs only while its level is below its cap (character_levels against character_level_caps, RESULT@0xb85 to 0xb9a). After it, each 100 of experience is a level, up to the cap (RESULT@0xbbd to 0xc43).

| Revision | Site |
|---|---|
| `sha256:3d48a533ba577307650fc0d76e9988f327dfa20d369a0a0bc877d7c52506b92f` | `0xbb6` |

## `koukaku.turn.before_week_advance`

Script: `ALLMAP.BIN`

A week ends: ALLMAP has raised the weeks since each played scene (G[0x38a6 + scene] for each set scene_played_flags cell, ALLMAP@0x244c to 0x248c), and is about to raise turn_week by 1 (ALLMAP@0x248f). Then it opens the new week's first phase (ALLMAP@0x2496, which a nation's phase also reaches, ALLMAP@0x1022).

| Revision | Site |
|---|---|
| `sha256:a9396e66ee5bd22c39239efd7841786188b6af2efe33671d736666a418ef962d` | `0x248f` |
