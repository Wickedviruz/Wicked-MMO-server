-- data/talkactions/lib/talkactions.lua
-- Helpers available to ALL talkaction scripts.

local logFormat = "[%s] %s %s\n"

--- Log a player command to their personal log file.
function logCommand(player, words, param)
    local dir = "data/logs/"
    local filename = dir .. player:getName() .. " commands.log"
    local file = io.open(filename, "a")
    if not file then
        return
    end
    file:write(logFormat:format(os.date("%d/%m/%Y %H:%M"), words, param))
    file:close()
end

--- Trim whitespace from a string.
function string.trim(s)
    return s:match("^%s*(.-)%s*$")
end

--- Split a string by delimiter.
function string.split(s, sep)
    local result = {}
    for part in s:gmatch("([^" .. sep .. "]+)") do
        result[#result + 1] = part
    end
    return result
end