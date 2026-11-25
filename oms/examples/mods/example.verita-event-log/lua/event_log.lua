-- Every event verita-base raises, with its fields, in the order raised. The driver's console reads the list:
--   lua mod=example.verita-event-log return table.concat(seen, "; ")
-- At each verita.data.ready it also gives the party one 治癒の水・小 (item 101) through the base mod's helper, which runs
-- the game's own ADDITEM, and logs the count it returns.
local game = require("verita-base")

local FIELDS = {
  ["verita.data.layered"] = {},
  ["verita.data.ready"] = {},
  ["verita.map.loaded"] = {"map"},
  ["verita.experience.award"] = {"character", "amount", "level"},
}

seen = {}

function on_event(event)
  local parts = { event.event }
  for _, name in ipairs(FIELDS[event.event] or {}) do
    parts[#parts + 1] = name .. "=" .. tostring(event[name])
  end
  if event.event == "verita.data.ready" then
    parts[#parts + 1] = "item101=" .. tostring(game.give_item(101))
  end
  local line = table.concat(parts, " ")
  seen[#seen + 1] = line
  maid.log.info(line)
end
