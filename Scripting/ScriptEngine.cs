using MoonSharp.Interpreter;

namespace GameCore.Scripting;

/// <summary>
/// Creates and configures Lua runtimes.
/// One engine = factory, NOT a global VM.
/// </summary>
public sealed class ScriptEngine
{
    public Script Create()
    {
        var script = new Script(CoreModules.Preset_Complete);

        // ===== FUTURE =====
        // UserData.RegisterType<Creature>();
        // UserData.RegisterType<Player>();
        // UserData.RegisterType<Item>();
        // script.Globals["Game"] = new LuaGameApi();

        return script;
    }
}
