using MoonSharp.Interpreter;
using GameCore.Core;

namespace GameCore.Scripting.Events;

public sealed class GlobalEventRegistry
{
    public static readonly string[] ValidEvents =
        ["serverSave", "serverStart", "serverShutdown"];

    private sealed record HookEntry(string Name, DynValue Fn);
    private sealed record TimerEntry(string Name, int IntervalMs, DynValue Fn);

    private readonly Dictionary<string, List<HookEntry>> _hooks   = new();
    private readonly List<TimerEntry>                    _timers  = [];
    private readonly List<System.Threading.Timer>        _running = [];

    public void Register(string eventType, string name, DynValue fn)
    {
        if (!ValidEvents.Contains(eventType))
            throw new ScriptRuntimeException(
                $"GlobalEvent: unknown type '{eventType}'. Valid: {string.Join(", ", ValidEvents)}");

        if (!_hooks.ContainsKey(eventType))
            _hooks[eventType] = [];

        _hooks[eventType].Add(new HookEntry(name, fn));
        Logger.Log(LogLevel.Debug, "GLOBALEVENT", $"Registered: {eventType} '{name}'");
    }

    public void Invoke(Script vm, string eventType)
    {
        if (!_hooks.TryGetValue(eventType, out var entries)) return;

        foreach (var entry in entries)
        {
            try   { vm.Call(entry.Fn); }
            catch (ScriptRuntimeException ex)
            {
                Logger.Log(LogLevel.Error, "GLOBALEVENT", $"'{entry.Name}' ({eventType}): {ex.DecoratedMessage}");
            }
        }
    }

    public void RegisterTimer(string name, int intervalMs, DynValue fn)
    {
        _timers.Add(new TimerEntry(name, intervalMs, fn));
        Logger.Log(LogLevel.Debug, "GLOBALEVENT", $"Timer registered: '{name}' every {intervalMs}ms");
    }

    public void StartTimers(Script vm)
    {
        StopTimers();
        foreach (var entry in _timers)
        {
            var captured = entry;
            var timer = new System.Threading.Timer(_ =>
            {
                try   { vm.Call(captured.Fn); }
                catch (ScriptRuntimeException ex)
                {
                    Logger.Log(LogLevel.Error, "GLOBALEVENT", $"Timer '{captured.Name}': {ex.DecoratedMessage}");
                }
            }, null, captured.IntervalMs, captured.IntervalMs);
            _running.Add(timer);
        }
    }

    public void StopTimers()
    {
        foreach (var t in _running) t.Dispose();
        _running.Clear();
    }
}