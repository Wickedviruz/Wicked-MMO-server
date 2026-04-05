namespace GameCore.Scripting.Loaders;

/// <summary>
/// Parsed definition from one &lt;globalevent&gt; XML element.
/// </summary>
public sealed record GlobalEventDef(
    string Name,
    string Type,
    string Script,
    int    IntervalMs
);