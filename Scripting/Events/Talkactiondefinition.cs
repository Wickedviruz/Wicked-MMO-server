namespace GameCore.Scripting.Events;

/// <summary>
/// Represents one &lt;talkaction&gt; entry from talkactions.xml.
/// </summary>
public sealed class TalkActionDefinition
{
    /// <summary>Trigger word, e.g. "/info"</summary>
    public string Words { get; init; } = "";

    /// <summary>Separator between command and param, default " "</summary>
    public string Separator { get; init; } = " ";

    /// <summary>Script filename inside scripts/, e.g. "info.lua"</summary>
    public string Script { get; init; } = "";
}