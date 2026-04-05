-- data/talkactions/scripts/reload.lua

local function onSay(player, words, param)
    print("Player used /reload")
    -- Hot-reload sker automatiskt via FileSystemWatcher
end

talkAction:register(onSay)