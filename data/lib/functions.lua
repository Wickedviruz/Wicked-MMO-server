-- data/lib/functions.lua
-- Shared helper functions available to ALL scripts.
-- Loaded first so everything else can use these.

--- Clamp a number between min and max.
function math.clamp(val, min, max)
    if val < min then return min end
    if val > max then return max end
    return val
end

--- Returns true if a random roll hits (0-100 chance).
function rollChance(chance)
    return math.random(0, 100) <= chance
end

--- Log a formatted message with a tag.
function logInfo(tag, msg)
    print("[" .. tag .. "] " .. tostring(msg))
end