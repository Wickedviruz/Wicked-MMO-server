using MoonSharp.Interpreter;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

/// <summary>
/// Injected into Lua as 'globalEvent' while a script loads.
/// Exposes the event type so scripts can route to the right handler.
///
/// Lua pattern:
///   local evt = globalEvent
///   print(evt.type)  --> "serverSave"
///
///   function evt.onSave() ... end
///   evt:register(evt.onSave)
/// </summary>
[MoonSharpUserData]
public sealed class GlobalEventScriptBinding
{
    private readonly Script              _vm;
    private readonly GlobalEventRegistry _registry;
    private readonly GlobalEventDef      _def;

    // Exposed to Lua so scripts can branch on event type
    public string type => _def.Type;
    public string name => _def.Name;

    public GlobalEventScriptBinding(Script vm, GlobalEventRegistry registry, GlobalEventDef def)
    {
        _vm       = vm;
        _registry = registry;
        _def      = def;
    }

    /// <summary>Called from Lua: evt:register(function() ... end)</summary>
    public void register(DynValue fn)
    {
        if (fn.Type != DataType.Function)
            throw new ScriptRuntimeException(
                $"globalEvent:register expects a function, got {fn.Type}");

        if (_def.Type == "timer")
            _registry.RegisterTimer(_def.Name, _def.IntervalMs, fn);
        else
            _registry.Register(_def.Type, _def.Name, fn);
    }
}