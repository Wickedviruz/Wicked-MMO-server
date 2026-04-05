-- data/globalevents/scripts/healtick.lua
-- Timer event – runs every 60 seconds.

local evt = globalEvent

evt:register(function()
    print("HealTick: regenerating creature HP...")
    -- TODO: iterate world creatures via binding once available
end)