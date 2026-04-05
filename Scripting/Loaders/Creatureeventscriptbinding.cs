using MoonSharp.Interpreter;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

/// <summary>
/// Injected into Lua as 'creatureEvent' while a script loads.
///
/// Lua pattern:
///   local evt = creatureEvent
///   function evt.onDeath(creature, killer) ... end
///   evt:register()
/// </summary>
[MoonSharpUserData]
public sealed class CreatureEventScriptBinding
{
    private readonly Script               _vm;
    private readonly CreatureEventRegistry _registry;
    private readonly string               _eventType;
    private readonly string               _name;

    public CreatureEventScriptBinding(Script vm, CreatureEventRegistry registry, string eventType, string name)
    {
        _vm        = vm;
        _registry  = registry;
        _eventType = eventType;
        _name      = name;
    }

    /// <summary>Called from Lua: evt:register(function(creature, killer) ... end)</summary>
    public void register(DynValue fn)
    {
        if (fn.Type != DataType.Function)
            throw new ScriptRuntimeException(
                $"creatureEvent:register expects a function, got {fn.Type}");

        _registry.Register(_eventType, _name, fn);
    }
}