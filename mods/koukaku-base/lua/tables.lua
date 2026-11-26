-- The koukaku.tables format's handlers (data patches spec §6, docs/superpowers/specs/2026-11-17-data-patches-design.md).
-- Hand-written. A document names records by table, decimal id and field, by the registry's names, as OMS does:
-- item["101"].name is item[101].name. A table the record map keys by a scope is named through it:
-- battlefield["2"].cell["21"].land is battlefield map 2's land at row 1, column 1. ome-records checks the whole
-- document against the record map before its first write, and writes through this base mod's own accessors. What
-- builds each table, its scope, and which fields a save holds come from lua/generated/tables.lua.
local records = require("ome-records")

local applier = records.applier {
    map = require("generated.tables"),
    table = maid.names.table,
    labels = { "boot", "battlefield" },
    -- The event each label is applied at: a document with a part at an event this install does not raise is refused
    -- whole, rather than applied in part (spec §6.2).
    events = { boot = "koukaku.data.layered", battlefield = "koukaku.battlefield.loaded" },
    -- A file that names a table or a field by an old name gets one line for each old name in lua.log.
    log = maid.log.warn,
}

--- At koukaku.data.layered: a contribution's boot tables, after every layer and before INIT2's last work.
function apply_boot(event, contribution)
    return applier:apply(contribution, { built_by = "boot" })
end

--- At koukaku.battlefield.loaded: a contribution's part for the battlefield map MPINIT has just written, each time
--- one of the six walkers loads it.
function apply_battlefield(event, contribution)
    return applier:apply(contribution, { built_by = "battlefield", scope = { battlefield = event.map } })
end
