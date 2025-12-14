namespace GameCore.Core;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}
public static class Logger
{
    private static readonly object _lock = new();

    public static LogLevel MinimumLevel = LogLevel.Trace;

    public static void Log(
        LogLevel level,
        string station,
        string message)
    {
        if (level < MinimumLevel)
            return;

        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var line = $"[{time}] [{level}] [{station}] {message}";

        lock (_lock)
        {
            Console.WriteLine(line);
            WriteToFile(line);
        }
    }

    private static void WriteToFile(string line)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var path = Path.Combine("logs", $"server-{date}.log");

        Directory.CreateDirectory("logs");
        File.AppendAllText(path, line + Environment.NewLine);
    }
}