using MoonSharp.Interpreter;

namespace GameCore.Scripting;

/// <summary>
/// Manages all Lua script events.
/// Dispatches C# events into Lua.
/// </summary>
public sealed class ScriptEventManager
{
    private readonly Dictionary<string, ScriptEvent> _events = new();
    private readonly ScriptEngine _engine = new();

    private static string MakeKey(string @class, string method)
        => $"{@class}:{method}";

    public void Register(ScriptEvent ev)
    {
        if (!ev.Enabled)
            return;

        var script = _engine.Create();
        script.DoFile(ev.ScriptPath);

        ev.Script = script;

        var key = MakeKey(ev.Class, ev.Method);
        _events[key] = ev;
    }

    public void Invoke(string @class, string method, params object[] args)
    {
        var key = MakeKey(@class, method);

        if (!_events.TryGetValue(key, out var ev))
            return;

        var fn = ev.Script.Globals.Get(method);
        if (fn.IsNil())
            return;

        ev.Script.Call(fn, args);
    }
}
