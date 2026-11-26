-- The bg0.tables format's handlers (data patches spec §6, docs/superpowers/specs/2026-11-17-data-patches-design.md).
-- Hand-written. A document names records by table, decimal id and field, by the registry's names, as OMS does:
-- item["101"].sell_price is item[101].sell_price. A table the record map keys by a scope is named through it:
-- dungeon["3"].block["31"].music is map 3's block 31's music, town["50"].site["2"].name a town's site, and
-- world["1"].area["10"].name an area of the first story part's world map. ome-records checks the whole document
-- against the record map before its first write, and writes through this base mod's own accessors. What builds each
-- table, its scope, and which fields a save holds come from lua/generated/tables.lua.
local records = require("ome-records")

local g = maid.names.g

local applier = records.applier {
    map = require("generated.tables"),
    table = maid.names.table,
    labels = { "boot", "world_map", "town", "map" },
    -- The event each label is applied at: a document with a part at an event this install does not raise is refused
    -- whole, rather than applied in part (spec §6.2).
    events = { boot = "bg0.data.layered", world_map = "bg0.world.loaded", town = "bg0.town.loaded",
               map = "bg0.map.loaded" },
    -- A file that names a table or a field by an old name gets one line for each old name in lua.log.
    log = maid.log.warn,
}

-- The dungeon scope's ids (the registry's [[scope]] dungeon): current_place_id names a field map only from 1 to 49.
local FIRST_MAP, LAST_MAP = 1, 49
-- system_mode while SYSTEM4 plays a scene.
local SCENE = 1

--- At bg0.data.layered: a contribution's boot tables, after every layer and before the boot uses them.
function apply_boot(event, contribution)
    return applier:apply(contribution, { built_by = "boot" })
end

--- At bg0.world.loaded: a contribution's part for the world map whose areas AMINIT has just written, each time.
function apply_world(event, contribution)
    return applier:apply(contribution, { built_by = "world_map", scope = { world = event.world } })
end

--- At bg0.town.loaded: a contribution's part for the town whose sites SIINIT has just written, each time.
function apply_town(event, contribution)
    return applier:apply(contribution, { built_by = "town", scope = { town = event.town } })
end

--- At bg0.map.loaded: a contribution's part for the field map whose block loader has just run, each time.
function apply_map(event, contribution)
    return applier:apply(contribution, { built_by = "map", scope = { dungeon = event.map } })
end

--- At bg0.data.ready: after a load of a save made in a scene after a field map, CALLBACK_LOAD has run the map's block
--- loader again (where system_mode is 1 and map_entered is set) without raising bg0.map.loaded, so the map's part is
--- written here.
function apply_loaded_map(event, contribution)
    local map = g.current_place_id
    if g.system_mode ~= SCENE or g.map_entered == 0 or map < FIRST_MAP or map > LAST_MAP then return 0 end
    return applier:apply(contribution, { built_by = "map", scope = { dungeon = map } })
end
