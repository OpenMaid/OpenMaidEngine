<!-- DO NOT EDIT -- generated from vm-map/patch-points-bg0.toml by tools/patch_points_build.py --build -->
# Named patch points — bg0 (generated)

Stable site names for OMS fragments (`before site <name> { … }`; mod loader spec §5,
`docs/superpowers/specs/2026-11-03-mod-loader-design.md`). Each point lists its site per script revision;
a fragment compiles only against a revision its point lists.

## `bg0.boot.data_layered`

Script: `INIT2.BIN`

The instruction after op 0x143 (INIT2@0x4b). Every layer has written its data: the 25 base *INIT scripts, then the append's $1$AUTORUN, which runs its $1$*INIT layers (and $1$AMINIT, $1$SIINIT and $1$SCINIT only while special_edition_mode is set, which it is not at a cold boot). The boot has not used the tables yet. INIT2 goes on to write its own constants, so an edit made here of scene_graphic_element_numbers, world_map_pictures, party_name_x, party_name_y, character_sort_keys or G[0x389180] is overwritten (INIT2@0x4c to 0x9c); to give a game with no formation the first one (formation_current, formation_learned_mask; INIT2@0xa1 to 0xb4); and to run CALCREVISE for each joined character (character_arg_id from 1 to 99, where bit 0 of character_flags is set; INIT2@0xb9 to 0xee), which reads an edit made here. The boot tables lie above the saved prefix (0x91003): SYSTEM4 runs INIT2 on every boot and again while a load restores (SYSTEM4@0x257, before op 0xae at SYSTEM4@0x25a), so control reaches here again then, with the save's cells back.

| Revision | Site |
|---|---|
| `sha256:45474e64ff9f89dac6000e2ff427c08b615e41d577d0d9ed74c76b53ba603632` | `0x4c` |

## `bg0.boot.data_ready`

Script: `INIT2.BIN`

INIT2's exit. Every data table is built: the 25 base *INIT scripts, the append's $1$AUTORUN and its $1$*INIT layers (op 0x143), and INIT2's own work after them, which reads and writes the tables (bg0.boot.data_layered): its constants, the first formation, and CALCREVISE for each joined character. An edit made here comes after that work. The boot tables lie above the saved prefix (0x91003): SYSTEM4 runs INIT2 on every boot and again while a load restores, so control reaches here again then. A load runs CALLBACK_LOAD before it restores SYSTEM4, so for a save made in a scene (system_mode 1) after a map (map_entered set) the map's block loader has run again by now (CALLBACK_LOAD@0xbc to 0xde): map_block holds the blocks of the map in play.

| Revision | Site |
|---|---|
| `sha256:45474e64ff9f89dac6000e2ff427c08b615e41d577d0d9ed74c76b53ba603632` | `0xf1` |

## `bg0.experience.before_add`

Script: `ADDEXP.BIN`

ADDEXP's read of the award: `mov L[0] script_arg_1`, inside the atomic block ADDEXP runs in. A caller has set character_arg_id to the character and script_arg_1 to the experience to add, and the character is in the party (bit 1 of character_flags; ADDEXP@0x1 to 0xf, and ADDEXP does nothing otherwise); nothing is added yet. A `before site` may rewrite script_arg_1. ADDEXP then adds it to character_experience (ADDEXP@0x35). セリカ, character 1, keeps it, capped at 999999. For any other character it is spent on levels: while it reaches the next level's need (G[0x38a9f7], from CALCNEXTEXP) below level 499, the need is taken off and the level rises, and at the need of level 499 the level becomes 500; then, if the level rose, ADDEXP passes CALCLVUP the new and the old level (script_arg_1 and script_arg_2) and runs CALCREVISE. RESULT calls ADDEXP for each character of a won battle (RESULT@0x696); FIELD, map events and scenes call it too.

| Revision | Site |
|---|---|
| `sha256:ec6872351ee7c7bbada965f1fe94a2747cb9ece28ca85febdd5deec245c31103` | `0x16` |

## `bg0.map.after_block_loaders`

Script: `FIELD.BIN`

After FIELD has run the map's block loader: RMINIT, or $1$RMINIT while special_edition_mode is set (FIELD@0x7b to 0x88). It has cleared the map blocks and filled them from the branch for the map in play (current_place_id, 1 to 49), so map_block holds that map's blocks, keyed by 30 * floor + section. FIELD then tests map_entered: when it is 0 the party has just entered the map, and FIELD sets it and takes the party's start from the map (MPINIT; FIELD@0x8b to 0xba). In both cases it then sets the map up from the blocks. FIELD reaches here each time it starts, which SYSTEM4 does for system_mode 3, and on a load of a save made on the map: the load restores FIELD from its entry up to op 0xae at FIELD@0x3ea, so FIELD runs the loader and comes here first. A load of a save made in a scene after the map (system_mode 1, map_entered set) does not come here: CALLBACK_LOAD runs the loader for it instead, before bg0.boot.data_ready.

| Revision | Site |
|---|---|
| `sha256:0a846cbf166eee7643bf0e24ce10b883c7761b67769d88d2c358dac70f555d17` | `0x8b` |

## `bg0.town.after_site_loaders`

Script: `TOWN.BIN`

After TOWN has run the town's loader: SIINIT, or $1$SIINIT while special_edition_mode is set (TOWN@0x5d to 0x6a). It has cleared the sites and filled them from the branch for the town in play (current_place_id, 50 to 99), so town_site holds that town's sites, keyed by site number from 1 (site 1 is the exit). TOWN has not drawn them or offered one yet. It reaches here each time it starts, which SYSTEM4 does for system_mode 5, and after a load of a save made in a town, which restores TOWN up to op 0xae at TOWN@0x59, before the loader.

| Revision | Site |
|---|---|
| `sha256:5ad0235d54917b24b1a50e6199b63a30a6dce8a3d223fe8da52ed9864959f88f` | `0x6d` |

## `bg0.world.after_area_loaders`

Script: `ALLMAP.BIN`

After ALLMAP has run the world map's loader: AMINIT, or $1$AMINIT while special_edition_mode is set (ALLMAP@0x5a to 0x67). It has cleared the areas and filled them from the branch for the story part in play (story_part), so area holds that world map's areas, keyed by place id (maps 1 to 49, towns from 50). ALLMAP has not drawn them or offered one yet. It reaches here each time it starts, which SYSTEM4 does for system_mode 2, and after a load of a save made on the world map, which restores ALLMAP up to op 0xae at ALLMAP@0x56, before the loader. SHOWALLMAP, which a scene calls to show a world map, runs AMINIT too (SHOWALLMAP@0x19) and does not come here: its point is bg0.world.after_shown_area_loaders.

| Revision | Site |
|---|---|
| `sha256:1a20098cb700d3c7e45e86473894cd020b62e6b704aaca02835964db0de39bb1` | `0x6a` |

## `bg0.world.after_shown_area_loaders`

Script: `SHOWALLMAP.BIN`

After SHOWALLMAP, which a scene calls to show a world map with one area marked, has run AMINIT (SHOWALLMAP@0x19). It has set story_part to the world its caller put in script_arg_1 and current_place_id to the area in script_arg_2 (SHOWALLMAP@0xf, 0x14), so area holds that world map's areas. SHOWALLMAP has not drawn them yet; it puts story_part and current_place_id back when it ends (SHOWALLMAP@0x375), and leaves area as AMINIT and this point made it, until ALLMAP runs the loader again. It runs the base AMINIT in either game mode.

| Revision | Site |
|---|---|
| `sha256:3407929246f19162749693f10e2bb2718ca50666a11d2ecdba397f332b73404a` | `0x1c` |
