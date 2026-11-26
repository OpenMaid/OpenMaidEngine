-- Every event koukaku-base raises, with its fields, in the order raised. The driver's console reads the list:
--   lua mod=example.koukaku-event-log return table.concat(seen, "; ")
-- At each koukaku.data.ready it also gives the player's nation one 転移の石 (item 101) through the base mod's helper,
-- which runs the game's own ADDITEM, and logs the count it returns.
local game = require("koukaku-base")

local FIELDS = {
  ["koukaku.data.layered"] = {},
  ["koukaku.data.ready"] = {},
  ["koukaku.battlefield.loaded"] = {"map"},
  ["koukaku.turn.week_ends"] = {"week", "player"},
  ["koukaku.experience.award"] = {"character", "amount", "level"},
}

seen = {}

function on_event(event)
  local parts = { event.event }
  for _, name in ipairs(FIELDS[event.event] or {}) do
    parts[#parts + 1] = name .. "=" .. tostring(event[name])
  end
  if event.event == "koukaku.data.ready" then
    parts[#parts + 1] = "item101=" .. tostring(game.give_item(101))
  end
  local line = table.concat(parts, " ")
  seen[#seen + 1] = line
  maid.log.info(line)
end
