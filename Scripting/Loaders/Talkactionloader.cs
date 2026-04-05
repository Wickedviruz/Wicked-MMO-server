using System.Xml.Linq;
using MoonSharp.Interpreter;
using GameCore.Core;
using GameCore.Scripting.Events;

namespace GameCore.Scripting.Loaders;

public sealed class TalkActionLoader
{
    private readonly string             _basePath;
    private readonly Script             _vm;
    private readonly TalkActionRegistry _registry;

    public TalkActionLoader(string basePath, Script vm, TalkActionRegistry registry)
    {
        _basePath = basePath;
        _vm       = vm;
        _registry = registry;
    }

    public void Load()
    {
        LoadLib();

        var xmlPath = Path.Combine(_basePath, "talkactions.xml");
        if (!File.Exists(xmlPath))
        {
            Logger.Log(LogLevel.Warn, "TALKACTION", $"No talkactions.xml found at {xmlPath}");
            return;
        }

        var defs = ParseXml(xmlPath);
        foreach (var def in defs)
            LoadScript(def);

        Logger.Log(LogLevel.Debug, "TALKACTION", $"Loaded {defs.Count} talkaction(s).");
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
                Logger.Log(LogLevel.Error, "TALKACTION", $"lib/{Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }

    private List<TalkActionDefinition> ParseXml(string xmlPath)
    {
        var defs = new List<TalkActionDefinition>();
        try
        {
            var doc = XDocument.Load(xmlPath);
            foreach (var el in doc.Root?.Elements("talkaction") ?? [])
            {
                var words = el.Attribute("words")?.Value ?? "";
                if (string.IsNullOrWhiteSpace(words))
                {
                    Logger.Log(LogLevel.Warn, "TALKACTION", "<talkaction> missing 'words', skipping.");
                    continue;
                }
                defs.Add(new TalkActionDefinition
                {
                    Words     = words,
                    Separator = el.Attribute("separator")?.Value ?? " ",
                    Script    = el.Attribute("script")?.Value    ?? "",
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "TALKACTION", $"Failed to parse talkactions.xml: {ex.Message}");
        }
        return defs;
    }

    private void LoadScript(TalkActionDefinition def)
    {
        if (string.IsNullOrWhiteSpace(def.Script))
        {
            Logger.Log(LogLevel.Warn, "TALKACTION", $"'{def.Words}' has no script defined, skipping.");
            return;
        }

        var scriptPath = Path.Combine(_basePath, "scripts", def.Script);
        if (!File.Exists(scriptPath))
        {
            Logger.Log(LogLevel.Error, "TALKACTION", $"Script not found: scripts/{def.Script}");
            return;
        }

        try
        {
            UserData.RegisterType<TalkActionScriptBinding>();
            _vm.Globals["talkAction"] = new TalkActionScriptBinding(_vm, _registry, def);
            _vm.DoFile(scriptPath);
            _vm.Globals["talkAction"] = DynValue.Nil;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "TALKACTION", $"scripts/{def.Script}: {ex.Message}");
        }
    }
}