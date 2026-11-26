-- koukaku-base's helpers (Lua scripting spec §9.3): changes made the way the game makes them, by calling its own scripts
-- (spec §10). A helper's writes are this base mod's, so a mod that uses one lists none of them under [lua] writes; the
-- trace names the mod whose handler called the helper. A helper that calls a script runs in a handler the interpreter
-- invoked (a hook, a watch or an event), not in init.lua, ome.session.start or the console.

local g = maid.names.g
local items = maid.names.table("item_counts")
local parts = maid.names.table("part_counts")

local base = {}

local function whole(value, what, low, high)
    if type(value) ~= "number" or value ~= math.floor(value) or value < low or value > high then
        error(string.format("bad argument %s (an integer from %d to %d expected, got %s)", what, low, high,
                            tostring(value)), 3)
    end
    return value
end

---Give a nation `count` of item `item`, or take them away with a negative count, as the game does: ADDITEM adds the
---change to the nation's item_counts, never below 0, and sells what goes above 999 at the item's selling price into
---the nation's money. For the player's nation it marks the item obtained and seen (item_obtained_flags, and
---item_seen_flags in the shared profile) and the item's skill known, and for a key item (kind 1) it sets the story
---flag 9001 + item. An item of kind 0 changes nothing. The argument cells ADDITEM reads are put back as they were.
---@param item integer an item id, 1 to 1999
---@param count? integer the change, -999 to 999; 1 when nil
---@param nation? integer the nation, 0 to 29; the player's (nation_player) when nil
---@return integer count the item's count in the nation's stock after the change
function base.give_item(item, count, nation)
    whole(item, "#1 to 'give_item'", 1, 1999)
    count = whole(count == nil and 1 or count, "#2 to 'give_item'", -999, 999)
    nation = whole(nation == nil and g.nation_player or nation, "#3 to 'give_item'", 0, 29)
    local item_before, count_before, nation_before = g.current_item, g.item_count_change, g.nation_arg_id
    g.current_item, g.item_count_change, g.nation_arg_id = item, count, nation
    local ok, err = pcall(maid.vm.call, "ADDITEM")
    g.current_item, g.item_count_change, g.nation_arg_id = item_before, count_before, nation_before
    if not ok then error(err, 0) end
    return items[nation][item]
end

---Give the player `count` of part `part`, or take them away with a negative count, as the game does: ADDPART adds the
---change to part_counts, at most 999, and marks the part obtained and seen (part_obtained_flags, and part_seen_flags
---in the shared profile) and its skill known. A costume (part kind 51) is added once, whatever the count: the first
---time, ADDPART marks its starting parts obtained and, if it has no level yet, sets its level, experience, parts and
---stats from COINIT. The argument cells ADDPART reads are put back as they were.
---@param part integer a part or costume id, 1 to 999
---@param count? integer the change, -999 to 999; 1 when nil
---@return integer count the part's count after the change
function base.give_part(part, count)
    whole(part, "#1 to 'give_part'", 1, 999)
    count = whole(count == nil and 1 or count, "#2 to 'give_part'", -999, 999)
    local part_before, count_before = g.current_part, g.part_count_change
    g.current_part, g.part_count_change = part, count
    local ok, err = pcall(maid.vm.call, "ADDPART")
    g.current_part, g.part_count_change = part_before, count_before
    if not ok then error(err, 0) end
    return parts[part]
end

return base
