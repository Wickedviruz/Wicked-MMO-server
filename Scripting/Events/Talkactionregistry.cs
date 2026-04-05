using MoonSharp.Interpreter;
using GameCore.Core;

namespace GameCore.Scripting.Events;

public sealed class TalkActionRegistry
{
    private sealed record Entry(string Separator, DynValue Fn);
    private readonly Dictionary<string, Entry> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string words, string separator, DynValue fn)
    {
        _handlers[words] = new Entry(separator, fn);
        Logger.Log(LogLevel.Debug, "TALKACTION", $"Registered: {words}");
    }

    public bool TryInvoke(Script vm, object player, string message)
    {
        var matched = _handlers.Keys
            .Where(cmd => message.StartsWith(cmd, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(cmd => cmd.Length)
            .FirstOrDefault();

        if (matched is null) return false;

        var entry = _handlers[matched];
        var param = message.Length > matched.Length
            ? message[(matched.Length + entry.Separator.Length)..]
            : "";

        try
        {
            vm.Call(entry.Fn, player, matched, param);
            return true;
        }
        catch (ScriptRuntimeException ex)
        {
            Logger.Log(LogLevel.Error, "TALKACTION", $"'{matched}': {ex.DecoratedMessage}");
            return false;
        }
    }
}