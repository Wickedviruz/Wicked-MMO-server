using MoonSharp.Interpreter;
using GameCore.Core;
using GameCore.Scripting.Events;

namespace GameCore.Scripting;

public sealed class LuaEnvironment
{
    public Script Vm { get; } = new Script(CoreModules.Preset_SoftSandbox);

    public CreatureEventRegistry CreatureEvents { get; } = new();
    public TalkActionRegistry    TalkActions    { get; } = new();
    public GlobalEventRegistry   GlobalEvents   { get; } = new();

    public void RegisterApis()
    {
        UserData.RegisterType<CreatureEventRegistry>();
        UserData.RegisterType<TalkActionRegistry>();
        UserData.RegisterType<GlobalEventRegistry>();

        // Lua print() -> Logger.Info
        Vm.Globals["print"] = (Action<DynValue>)(val =>
            Logger.Log(LogLevel.Info, "LUA", val.ToPrintString()));
    }
}