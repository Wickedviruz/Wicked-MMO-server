using MoonSharp.Interpreter;
using GameCore.Core;

namespace GameCore.Scripting.Events;

public sealed class CreatureEventRegistry
{
    public static readonly string[] ValidEvents =
        ["login", "logout", "death", "kill", "think", "extendedopcode"];

    private sealed record Entry(string Name, DynValue Fn);
    private readonly Dictionary<string, List<Entry>> _hooks = new();

    public void Register(string eventType, string name, DynValue fn)
    {
        if (!ValidEvents.Contains(eventType))
            throw new ScriptRuntimeException(
                $"CreatureEvent: unknown type '{eventType}'. Valid: {string.Join(", ", ValidEvents)}");

        if (!_hooks.ContainsKey(eventType))
            _hooks[eventType] = [];

        _hooks[eventType].Add(new Entry(name, fn));
        Logger.Log(LogLevel.Debug, "CREATURE", $"Registered: {eventType} '{name}'");
    }

    public void Invoke(Script vm, string eventType, params object[] args)
    {
        if (!_hooks.TryGetValue(eventType, out var entries)) return;

        foreach (var entry in entries)
        {
            try   { vm.Call(entry.Fn, args); }
            catch (ScriptRuntimeException ex)
            {
                Logger.Log(LogLevel.Error, "CREATURE", $"'{entry.Name}' ({eventType}): {ex.DecoratedMessage}");
            }
        }
    }
}