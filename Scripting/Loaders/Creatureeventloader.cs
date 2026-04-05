using System.Xml.Linq;
using MoonSharp.Interpreter;
using GameCore.Core;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

public sealed class CreatureEventLoader
{
    private readonly string               _basePath;
    private readonly Script               _vm;
    private readonly CreatureEventRegistry _registry;

    public CreatureEventLoader(string basePath, Script vm, CreatureEventRegistry registry)
    {
        _basePath = basePath;
        _vm       = vm;
        _registry = registry;
    }

    public void Load()
    {
        LoadLib();

        var xmlPath = Path.Combine(_basePath, "creaturescripts.xml");
        if (!File.Exists(xmlPath))
        {
            Logger.Log(LogLevel.Warn, "CREATURE", $"No creaturescripts.xml at {xmlPath}");
            return;
        }

        var defs = ParseXml(xmlPath);
        foreach (var def in defs)
            LoadScript(def);

        Logger.Log(LogLevel.Debug, "CREATURE", $"Loaded {defs.Count} script(s).");
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
                Logger.Log(LogLevel.Error, "CREATURE", $"lib/{Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }

    private List<(string Type, string Name, string Script)> ParseXml(string xmlPath)
    {
        var defs = new List<(string, string, string)>();
        try
        {
            var doc = XDocument.Load(xmlPath);
            foreach (var el in doc.Root?.Elements("event") ?? [])
            {
                var type   = el.Attribute("type")?.Value   ?? "";
                var name   = el.Attribute("name")?.Value   ?? "";
                var script = el.Attribute("script")?.Value ?? "";

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(script))
                {
                    Logger.Log(LogLevel.Warn, "CREATURE", "<event> missing 'type' or 'script', skipping.");
                    continue;
                }
                defs.Add((type, name, script));
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "CREATURE", $"Failed to parse creaturescripts.xml: {ex.Message}");
        }
        return defs;
    }

    private void LoadScript((string Type, string Name, string Script) def)
    {
        var scriptPath = Path.Combine(_basePath, "scripts", def.Script);
        if (!File.Exists(scriptPath))
        {
            Logger.Log(LogLevel.Error, "CREATURE", $"Script not found: scripts/{def.Script}");
            return;
        }

        try
        {
            UserData.RegisterType<CreatureEventScriptBinding>();
            _vm.Globals["creatureEvent"] = new CreatureEventScriptBinding(_vm, _registry, def.Type, def.Name);
            _vm.DoFile(scriptPath);
            _vm.Globals["creatureEvent"] = DynValue.Nil;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "CREATURE", $"scripts/{def.Script}: {ex.Message}");
        }
    }
}