using GameCore.Core;
using GameCore.Scripting.Events;
using GameCore.Scripting.Loaders;

namespace GameCore.Scripting;

public sealed class ScriptManager : IDisposable
{
    private readonly string    _dataPath;
    private LuaEnvironment     _env = new();
    private FileSystemWatcher? _watcher;

    private DateTime          _lastReload     = DateTime.MinValue;
    private readonly TimeSpan _reloadCooldown = TimeSpan.FromMilliseconds(500);

    public ScriptManager(string dataPath)
    {
        _dataPath = Path.GetFullPath(dataPath);
    }

    public CreatureEventRegistry CreatureEvents => _env.CreatureEvents;
    public TalkActionRegistry    TalkActions    => _env.TalkActions;
    public GlobalEventRegistry   GlobalEvents   => _env.GlobalEvents;
    public MoonSharp.Interpreter.Script Vm      => _env.Vm;

    public void Load()
    {
        _env = BuildEnvironment();
        Logger.Log(LogLevel.Info, "SCRIPTING", "All systems loaded.");
    }

    private LuaEnvironment BuildEnvironment()
    {
        var env = new LuaEnvironment();
        env.RegisterApis();

        new TalkActionLoader(Path.Combine(_dataPath, "talkactions"),     env.Vm, env.TalkActions).Load();
        new CreatureEventLoader(Path.Combine(_dataPath, "creaturescripts"), env.Vm, env.CreatureEvents).Load();
        new GlobalEventLoader(Path.Combine(_dataPath, "globalevents"),   env.Vm, env.GlobalEvents).Load();

        env.GlobalEvents.StartTimers(env.Vm);
        return env;
    }

    public void EnableHotReload()
    {
        if (!Directory.Exists(_dataPath))
        {
            Logger.Log(LogLevel.Warn, "SCRIPTING", $"Hot-reload skipped: {_dataPath} not found");
            return;
        }

        _watcher = new FileSystemWatcher(_dataPath)
        {
            Filter                = "*.*",
            IncludeSubdirectories = true,
            NotifyFilter          = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents   = true,
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Renamed += OnChanged;

        Logger.Log(LogLevel.Debug, "SCRIPTING", $"Hot-reload watching: {_dataPath}");
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.Name);
        if (ext is not (".lua" or ".xml")) return;

        var now = DateTime.UtcNow;
        if (now - _lastReload < _reloadCooldown) return;
        _lastReload = now;

        Logger.Log(LogLevel.Info, "SCRIPTING", $"Reloading scripts ({e.Name} changed)...");

        _env.GlobalEvents.StopTimers();

        try
        {
            _env = BuildEnvironment();
            Logger.Log(LogLevel.Info, "SCRIPTING", "Hot-reload complete.");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "SCRIPTING", $"Hot-reload failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _env.GlobalEvents.StopTimers();
    }
}