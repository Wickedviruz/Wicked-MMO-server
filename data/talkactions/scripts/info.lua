-- data/talkactions/scripts/info.lua

local function onSay(player, words, param)
    print("Player used /info")
    -- TODO: player:sendMessage("Server info") när PlayerBinding finns
end

talkAction:register(onSay)