# koukaku-base

The engine's base mod for 魔導巧殻 (Lua scripting spec §9,
`docs/superpowers/specs/2026-11-10-lua-scripting-design.md`). The engine ships it and mounts it only when an
enabled mod requires it. A Lua mod reads and writes the game's cells by name through it.

- `manifest.toml` is hand-written. Its major version is the version of the names the base mod exports.
- `layout/` is generated from `vm-map/globals-koukaku.toml` and `vm-map/patch-points-koukaku.toml` by
  `py -3.11 -X utf8 tools/base_mod_build.py --build`. Do not edit it.
- `oms/` is the game's OMS toolchain (modding from a release spec §5,
  `docs/superpowers/specs/2026-11-24-modding-from-a-release-design.md`), which the engine reads from its own copy of
  this base mod to compile fragments and new scripts. `oms/adv.oms` is the module `koukaku.adv`, the ADV prelude: the
  speakers (`enum speaker`, from CHINIT's names), the dialogue line template, the scene header's handlers and the
  library the scenes link. It was written by a generator from the scenes' core decompile on 2026-11-26 and is kept
  by hand from here on. `oms/globals.json` is the module `koukaku.globals`, generated with `layout/` from
  `vm-map/globals-koukaku.toml` by `tools/base_mod_build.py --build`, with the old names of renamed globals, tables
  and fields. Do not edit it.
- `events/` is hand-written: the game events the base mod declares, with each field's cell by an exported name:
  `boot.toml` (`koukaku.data.layered`, once the base and the append have written the data tables and before INIT2's
  last work, and `koukaku.data.ready`, once that work is done), `battlefield.toml` (`koukaku.battlefield.loaded`,
  when one of the six scripts that walk the MPINIT chain has loaded a battlefield map), `turn.toml`
  (`koukaku.turn.week_ends`, before ALLMAP raises the week) and `experience.toml` (`koukaku.experience.award`, one
  commander's experience from a battle before RESULT adds it). The patch points file says where each runs.
- `lua/init.lua` is hand-written: the helpers the base mod returns to a mod that requires it, which change the game
  through its own scripts (`give_item` through ADDITEM, `give_part` through ADDPART). `[lua] calls` in the manifest
  lists those scripts.
- `lua/tables.lua` is hand-written: the handlers of the `koukaku.tables` format (data patches spec §6,
  `docs/superpowers/specs/2026-11-17-data-patches-design.md`), which the manifest's `[[accepts]]` declares.
  `apply_boot`, bound at `koukaku.data.layered`, checks a data file's document against the record map and writes its
  boot tables; `apply_battlefield`, bound at `koukaku.battlefield.loaded`, writes its part for the battlefield map
  just loaded (`battlefield.<id>.cell`, `battlefield.<id>.entry`). Both go through `ome-records`, the library the
  manifest requires. The other on-demand tables (DUINIT's battle setups, SHINIT's offers) are not offered: the shape
  refuses a document that names one. The modder's guide to the format is `docs/modding/data-files.md`.
- `lua/generated/tables.lua` is the record map, generated with `layout/` by `tools/base_mod_build.py --build` from
  the registry's `[[table]]` and `[[scope]]` entries and the profile's saved prefix, with the old names of tables
  and fields. Do not edit it. `lua/tables.lua` hands the applier `maid.log.warn`, so a file that writes an old name
  gets a line for it in `lua.log`.
- `shapes/tables.schema.json` is the kind's shape (data patches spec §7), generated with the record map by
  `tools/base_mod_build.py --build`, and named by the manifest's `[[accepts]] shape`. The link checks each data file
  of `koukaku.tables` against it before any code runs; what it cannot say `lua/tables.lua` checks at the file's first
  delivery. Do not edit it. `omp lua-stubs` writes a copy for an editor.
- `layout/exports.txt` is the list of exported names, checked in: the globals, then the record tables
  (`table NAME`) and their fields (`field TABLE.FIELD`), then the scopes (`scope NAME`) and their tables' short
  names (`scope NAME.SHORT`), then the names `oms/*.oms` declares (`module MODULE.NAME`). A name in it stays
  exported, as a global's name or one of its `aliases`, a table's name or one of its `aliases`, or a field's name or
  an old name in its table's `[table.field_aliases]`, until the major version is raised; a scope, a short name and a
  module's name have no old-name form, so renaming a sub or a template of `oms/adv.oms` raises the major version.

The game's modding guide is `docs/modding/games/koukaku.md`.
