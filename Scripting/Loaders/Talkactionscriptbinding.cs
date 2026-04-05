using MoonSharp.Interpreter;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

/// <summary>
/// Injected into Lua as 'talkAction' while a script is loading.
/// The script calls talkAction:register(fn) to bind its handler.
///
/// Lua script pattern:
///   local action = talkAction   -- already injected by loader
///   function action.onSay(player, words, param)
///       player:sendMessage("pong!")
///   end
///   action:register(action.onSay)
/// </summary>
[MoonSharpUserData]
public sealed class TalkActionScriptBinding
{
    private readonly Script              _vm;
    private readonly TalkActionRegistry  _registry;
    private readonly TalkActionDefinition _def;

    public TalkActionScriptBinding(Script vm, TalkActionRegistry registry, TalkActionDefinition def)
    {
        _vm       = vm;
        _registry = registry;
        _def      = def;
    }

    /// <summary>Called from Lua: talkAction:register(function(player, words, param) ... end)</summary>
    public void register(DynValue fn)
    {
        if (fn.Type != DataType.Function)
            throw new ScriptRuntimeException(
                $"talkAction:register expects a function, got {fn.Type}");

        _registry.Register(_def.Words, _def.Separator, fn);
    }
}