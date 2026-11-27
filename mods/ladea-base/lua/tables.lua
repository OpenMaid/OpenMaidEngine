-- The ladea.tables format's handlers (data patches spec §6, docs/superpowers/specs/2026-11-17-data-patches-design.md).
-- Hand-written. A document names records by table, decimal id and field, by the registry's names, as OMS does:
-- item["101"].hp_change is item[101].hp_change. A table the record map keys by a scope is named through it:
-- dungeon_floor["3"].object["2"].start_kind is floor 3's object slot 2, and place["5"].spot["1"].x place 5's spot 1.
-- ome-records checks the whole document against the record map before its first write, and writes through this base
-- mod's own accessors. What builds each table, its scope, and which fields a save holds come from
-- lua/generated/tables.lua.
local records = require("ome-records")

local applier = records.applier {
    map = require("generated.tables"),
    table = maid.names.table,
    labels = { "boot", "floor", "town" },
    -- The event each label is applied at: a document with a part at an event this install does not raise is refused
    -- whole, rather than applied in part (spec §6.2).
    events = { boot = "ladea.data.layered", floor = "ladea.floor.loaded", town = "ladea.town.place_loaded" },
    -- A file that names a table or a field by an old name gets one line for each old name in lua.log.
    log = maid.log.warn,
}

--- At ladea.data.layered: a contribution's boot tables, after every layer and before INIT2's last work.
function apply_boot(event, contribution)
    return applier:apply(contribution, { built_by = "boot" })
end

--- At ladea.floor.loaded: a contribution's part for the floor FLINIT has just written, on each map-block entry.
function apply_floor(event, contribution)
    return applier:apply(contribution, { built_by = "floor", scope = { dungeon_floor = event.floor } })
end

--- At ladea.town.place_loaded: a contribution's part for the place ORINIT has just written, each time SETORNDATA
--- builds it.
function apply_place(event, contribution)
    return applier:apply(contribution, { built_by = "town", scope = { place = event.place } })
end
