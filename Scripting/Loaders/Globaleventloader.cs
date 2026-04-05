using System.Xml.Linq;
using MoonSharp.Interpreter;
using GameCore.Core;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

public sealed class GlobalEventLoader
{
    private readonly string              _basePath;
    private readonly Script              _vm;
    private readonly GlobalEventRegistry _registry;

    public GlobalEventLoader(string basePath, Script vm, GlobalEventRegistry registry)
    {
        _basePath = basePath;
        _vm       = vm;
        _registry = registry;
    }

    public void Load()
    {
        LoadLib();

        var xmlPath = Path.Combine(_basePath, "globalevents.xml");
        if (!File.Exists(xmlPath))
        {
            Logger.Log(LogLevel.Warn, "GLOBALEVENT", $"No globalevents.xml at {xmlPath}");
            return;
        }

        var defs = ParseXml(xmlPath);
        foreach (var def in defs)
            LoadScript(def);

        Logger.Log(LogLevel.Debug, "GLOBALEVENT", $"Loaded {defs.Count} event(s).");
    }

    private void LoadLib()
    {
        var libPath = Path.Combine(_basePath, "lib");
        if (!Directory.Exists(libPath)) return;

        foreach (var file in Directory.GetFiles(libPath, "*.lua").OrderBy(f => f))
        {
            try   { _vm.DoFile(file); }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "GLOBALEVENT", $"lib/{Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }

    private List<GlobalEventDef> ParseXml(string xmlPath)
    {
        var defs = new List<GlobalEventDef>();
        try
        {
            var doc = XDocument.Load(xmlPath);
            foreach (var el in doc.Root?.Elements("globalevent") ?? [])
            {
                var name     = el.Attribute("name")?.Value     ?? "";
                var type     = el.Attribute("type")?.Value     ?? "";
                var script   = el.Attribute("script")?.Value   ?? "";
                var interval = el.Attribute("interval")?.Value;

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(script))
                {
                    Logger.Log(LogLevel.Warn, "GLOBALEVENT", "<globalevent> missing 'type' or 'script', skipping.");
                    continue;
                }

                int intervalMs = 0;
                if (type == "timer")
                {
                    if (!int.TryParse(interval, out intervalMs) || intervalMs < 1000)
                    {
                        Logger.Log(LogLevel.Warn, "GLOBALEVENT", $"Timer '{name}' has invalid interval '{interval}', skipping.");
                        continue;
                    }
                }

                defs.Add(new GlobalEventDef(name, type, script, intervalMs));
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "GLOBALEVENT", $"Failed to parse globalevents.xml: {ex.Message}");
        }
        return defs;
    }

    private void LoadScript(GlobalEventDef def)
    {
        var scriptPath = Path.Combine(_basePath, "scripts", def.Script);
        if (!File.Exists(scriptPath))
        {
            Logger.Log(LogLevel.Error, "GLOBALEVENT", $"Script not found: scripts/{def.Script}");
            return;
        }

        try
        {
            UserData.RegisterType<GlobalEventScriptBinding>();
            _vm.Globals["globalEvent"] = new GlobalEventScriptBinding(_vm, _registry, def);
            _vm.DoFile(scriptPath);
            _vm.Globals["globalEvent"] = DynValue.Nil;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "GLOBALEVENT", $"scripts/{def.Script}: {ex.Message}");
        }
    }
}