-- data/globalevents/scripts/server.lua
-- Loaded once per <globalevent> entry that points to server.lua.
-- 'globalEvent' is injected with .type and .name set.

local evt = globalEvent

local handlers = {}

function handlers.serverStart()
    print("Server started. All scripts loaded successfully.")
end

function handlers.serverSave()
    print("Server save triggered. Persisting world state...")
end

function handlers.serverShutdown()
    print("Server shutting down. Cleanup complete.")
end

local fn = handlers[evt.type]
if fn then
    evt:register(fn)
else
    print("[WARNING] server.lua: no handler for type '" .. evt.type .. "'")
end