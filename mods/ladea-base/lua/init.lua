-- ladea-base's helpers (Lua scripting spec §9.3): changes made the way the game makes them, by calling its own scripts
-- (spec §10). A helper's writes are this base mod's, so a mod that uses one lists none of them under [lua] writes; the
-- trace names the mod whose handler called the helper. A helper that calls a script runs in a handler the interpreter
-- invoked (a hook, a watch or an event), not in init.lua, ome.session.start or the console.

local g = maid.names.g
local items = maid.names.table("item_counts")

local base = {}

local function whole(value, what, low, high)
    if type(value) ~= "number" or value ~= math.floor(value) or value < low or value > high then
        error(string.format("bad argument %s (an integer from %d to %d expected, got %s)", what, low, high,
                            tostring(value)), 3)
    end
    return value
end

---Give the party `count` of item `item`, or take them away with a negative count, as the game does: ADDITEM adds the
---change to item_counts, kept within 0 and 999. A stone the party has had before comes as its crystal
---(item_duplicate_crystals) instead, and on a stone's first copy ADDITEM keeps one and adds the rest as crystals. A key
---item (kind 1) sets its story flag 9001 + item while one is held. ADDITEM marks the item obtained and seen
---(item_obtained_flags, and item_seen_flags in the shared profile). An item of kind 0 changes nothing. The argument
---cells ADDITEM reads are put back as they were.
---@param item integer an item id, 1 to 19999
---@param count? integer the change, -999 to 999; 1 when nil
---@return integer count the count of `item` held after the change
---@return integer added the item ADDITEM changed last: `item`, or the crystal it gave in its place
function base.give_item(item, count)
    whole(item, "#1 to 'give_item'", 1, 19999)
    count = whole(count == nil and 1 or count, "#2 to 'give_item'", -999, 999)
    local item_before, count_before = g.current_item, g.item_count_change
    g.current_item, g.item_count_change = item, count
    local ok, err = pcall(maid.vm.call, "ADDITEM")
    local added = g.current_item
    g.current_item, g.item_count_change = item_before, count_before
    if not ok then error(err, 0) end
    return items[item], added
end

return base
