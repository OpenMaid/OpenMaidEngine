<!-- DO NOT EDIT -- generated from vm-map/patch-points-ladea.toml by tools/patch_points_build.py --build -->
# Named patch points — ladea (generated)

Stable site names for OMS fragments (`before site <name> { … }`; mod loader spec §5,
`docs/superpowers/specs/2026-11-03-mod-loader-design.md`). Each point lists its site per script revision;
a fragment compiles only against a revision its point lists.

## `ladea.boot.data_layered`

Script: `INIT2.BIN`

The instruction after op 0x143 (INIT2@0x23e). Every layer has written its data: the 34 base *INIT scripts, INIT2's hook chains and constants (INIT2@0x66 to 0x236), then the appends' $1$AUTORUN and $2$AUTORUN, which run their $1$*INIT and $2$*INIT layers and fill slots 1 and 2 of the chains. INIT2 goes on to give each of CVINIT's colours to the shared profile where it left none (INIT2@0x23e to 0x2a2), to rebuild an event CG, or the field or the town under a scene, on a load inside one (INIT2@0x2a5 to 0x302), which reads the tables, to clear the negative item cells (INIT2@0x305 to 0x343), and to set G[0xcf2e7] (INIT2@0x346). The boot tables lie above the saved prefix (G[0xcf2e6], GS[0x34f]), so a save holds none of them: SYSTEM4 runs INIT2 on every boot and again while a load restores (SYSTEM4@0x286, before op 0xae at SYSTEM4@0x2ca), so control reaches here again then, with the save's cells back.

| Revision | Site |
|---|---|
| `sha256:e652dca336c075c7e88b68f00bc90dc0b2682c76fade34be8df2a38eec25e0df` | `0x23e` |

## `ladea.boot.data_ready`

Script: `INIT2.BIN`

INIT2's exit. Every data table is built: the 34 base *INIT scripts, INIT2's hook chains and constants, the appends' AUTORUNs and their layers (op 0x143), and INIT2's own work after them (ladea.boot.data_layered): the colours, on such a load the rebuilt CG, field or town, the item cells and G[0xcf2e7]. An edit made here comes after that work. SYSTEM4 runs INIT2 on every boot and again while a load restores, so control reaches here again then.

| Revision | Site |
|---|---|
| `sha256:e652dca336c075c7e88b68f00bc90dc0b2682c76fade34be8df2a38eec25e0df` | `0x34b` |

## `ladea.experience.before_add`

Script: `ADDEXP.BIN`

One character's experience, before ADDEXP adds it: the add character_experience[c] += G[0xe7a12] (ADDEXP@0x96), c being character_arg_id and G[0xe7a12] the amount its caller set just before the call. ADDEXP comes here only for a party member ((character_flags[c] & 0x10002) == 0x10002, ADDEXP@0x1c to 0x31) whose level is below the party's cap: セリカ's character_level_caps entry plus character_level_cap_bonus, raised to the character's own cap and at most 250 (ADDEXP@0x38 to 0x81). After the add, each 100 + 3L² of experience makes a level, up to the cap, and CALCREVISE runs when the level changed (ADDEXP@0xae to 0x177). Every award passes here: RESULT shares a battle's experience among the party (RESULT@0x6d6), FIELD gives 1 to each party character under condition 40 (経験) as the party walks (FIELD@0x2655 to 0x2671), and 30 scenes give a fixed amount (SC0010@0x6832 gives セリカ 200).

| Revision | Site |
|---|---|
| `sha256:607a9cd3cd6e13480db4861edb4da5bbe77da03e6ac31102b130c1fd62f0a6df` | `0x96` |

## `ladea.floor.after_object_loaders`

Script: `FIELD.BIN`

The objects of the floor in play are written: FIELD has copied the map block's floor into current_floor_id (FIELD@0x1c4) and walked the FLINIT chain, flinit_chain_script_ids (FIELD@0x37c to 0x3b1), whose base FLINIT, $1$FLINIT and $2$FLINIT branch on the floor and write floor_object's starting kinds, actions, arguments and the slot count. Nothing has read them yet: FIELD then applies the puzzles' reset rules (FIELD@0x422 to 0x492) and sets up each slot below the count (FIELD@0x545 to 0x5de), copying its starting kind into the saved floor_object_kinds unless the slot is used and present. FIELD reaches here on every map-block entry; when the scene dispatch of a new floor starts a scene first (FIELD@0x35d), FIELD leaves and reaches here when it runs again after the scene. INIT2 does not walk the chain after a load (docs/ladea-data-tables.md, FLINIT).

| Revision | Site |
|---|---|
| `sha256:bd40a0c8f33911409c573039ecadf36a4b293de93487d3a18f2fc8b171589d5e` | `0x3b4` |

## `ladea.town.after_spot_loaders`

Script: `SETORNDATA.BIN`

The spots of the place in play are written: SETORNDATA has zeroed the spot arrays (SETORNDATA@0x0 to 0x95) and walked the ORINIT chain, orinit_chain_script_ids (SETORNDATA@0x9a to 0xcf), whose base ORINIT, $1$ORINIT and $2$ORINIT branch on town_current_place_id and write town_spot's positions, pictures, conditions, characters, voices and actions. This is SETORNDATA's exit. TOWN runs it when it enters a place (TOWN@0x347, @0x10f3) and reads the spots after it, and, while it builds its list of places, once for each opened place, to read its spots (TOWN@0x1a48 to 0x1b0d); INIT2 runs it on a load inside a scene over the town (INIT2@0x2c5 to 0x302).

| Revision | Site |
|---|---|
| `sha256:6c61267394e52deeb0ea2c8663c42dc48d62abdbf314df0b258ed30004ef3860` | `0xd2` |
