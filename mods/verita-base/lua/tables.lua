-- The verita.tables format's handlers (data patches spec §6, docs/superpowers/specs/2026-11-17-data-patches-design.md).
-- Hand-written. A document names records by table, decimal id and field, by the registry's names, as OMS does:
-- item["1001"].price is item[1001].price. A table the record map keys by a scope is named through it:
-- dungeon["3"].block["31"].music is map 3's block 31's music. ome-records checks the whole document against the record
-- map before its first write, and writes through this base mod's own accessors. What builds each table, its scope,
-- and which fields a save holds come from lua/generated/tables.lua.
local records = require("ome-records")

local g = maid.names.g

local applier = records.applier {
    map = require("generated.tables"),
    table = maid.names.table,
    labels = { "boot", "map" },
    -- The event each label is applied at: a document with a part at an event this install does not raise is refused
    -- whole, rather than applied in part (spec §6.2).
    events = { boot = "verita.data.layered", map = "verita.map.loaded" },
    -- A file that names a table or a field by an old name gets one line for each old name in lua.log.
    log = maid.log.warn,
}

-- The dungeon scope's ids (the registry's [[scope]] dungeon): current_place_id names a dungeon map only from 1 to 49.
local FIRST_MAP, LAST_MAP = 1, 49

--- At verita.data.layered: a contribution's boot tables, after every layer and before the boot uses them.
function apply_boot(event, contribution)
    return applier:apply(contribution, { built_by = "boot" })
end

--- At verita.map.loaded: a contribution's part for the dungeon map whose block loaders have just run, each time.
function apply_dungeon(event, contribution)
    return applier:apply(contribution, { built_by = "map", scope = { dungeon = event.map } })
end

--- At verita.data.ready: after a load of a save made on a dungeon map the boot has run that map's block loaders
--- again (INIT2, where on_dungeon_map is set) without raising verita.map.loaded, so the map's part is written here.
function apply_loaded_dungeon(event, contribution)
    local map = g.current_place_id
    if g.on_dungeon_map == 0 or map < FIRST_MAP or map > LAST_MAP then return 0 end
    return applier:apply(contribution, { built_by = "map", scope = { dungeon = map } })
end
