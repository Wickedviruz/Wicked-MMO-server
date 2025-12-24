using MoonSharp.Interpreter;

namespace GameCore.Scripting;

/// <summary>
/// Represents a single script hook defined in XML.
/// Example: Creature:onThink -> creature/test.lua
/// </summary>
public sealed class ScriptEvent
{
    public string Class { get; init; } = "";
    public string Method { get; init; } = "";
    public string ScriptPath { get; init; } = "";
    public bool Enabled { get; init; } = true;

    // Loaded Lua VM
    public Script Script { get; set; } = null!;
}
