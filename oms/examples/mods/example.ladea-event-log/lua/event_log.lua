-- Every event ladea-base raises, with its fields, in the order raised. The driver's console reads the list:
--   lua mod=example.ladea-event-log return table.concat(seen, "; ")
-- At each ladea.data.ready it also gives the party one 治癒の水・小 (item 201) through the base mod's helper, which runs
-- the game's own ADDITEM, and logs the count it returns.
local game = require("ladea-base")

local FIELDS = {
  ["ladea.data.layered"] = {},
  ["ladea.data.ready"] = {},
  ["ladea.floor.loaded"] = {"floor"},
  ["ladea.town.place_loaded"] = {"place"},
  ["ladea.experience.award"] = {"character", "amount", "level"},
}

seen = {}

function on_event(event)
  local parts = { event.event }
  for _, name in ipairs(FIELDS[event.event] or {}) do
    parts[#parts + 1] = name .. "=" .. tostring(event[name])
  end
  if event.event == "ladea.data.ready" then
    parts[#parts + 1] = "item201=" .. tostring((game.give_item(201)))
  end
  local line = table.concat(parts, " ")
  seen[#seen + 1] = line
  maid.log.info(line)
end
