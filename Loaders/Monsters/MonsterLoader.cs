using System.Security.Permissions;
using System.Xml.Linq;
using GameCore.Core;

namespace GameCore.Loaders.Monsters;

public static class MonsterLoader
{
    public static void Load(string monsterPath)
    {
        var indexFile = Path.Combine(monsterPath,"monsters.xml");

        if(!File.Exists(indexFile))
        {
            Logger.Log(LogLevel.Error, "MONSTER", "monsters.xml is missing");
            return;
        }

        var index = XDocument.Load(indexFile);

        foreach (var entry in index.Root!.Elements("monster"))
        {
            var file = entry.Attribute("file")?.Value;
            if(file == null)
                continue;
            
            var fullPath = Path.Combine(monsterPath, file);

            try
            {
                var monster = LoadMonsterFile(fullPath);
                MonsterRegistry.Register(monster);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MONSTER", $"Failed loading {file}: {ex.Message}");
            }
        }
    }

        private static MonsterDefinition LoadMonsterFile(string file)
        {
            var doc = XDocument.Load(file);
            var root = doc.Element("monster")
                ?? throw new Exception("Missing <monster>");

            var health = root.Element("health")
                ?? throw new Exception("Missing <health>");

            var monster = new MonsterDefinition
            {
                Name = root.Attribute("name")!.Value,
                Description = root.Attribute("nameDescription")?.Value ?? "",
                Race = root.Attribute("race")?.Value ?? "",
                Experience = int.Parse(root.Attribute("experience")?.Value ?? "0"),
                Speed = int.Parse(root.Attribute("speed")?.Value ?? "100"),
                MaxHealth = int.Parse(health.Attribute("max")!.Value),

                Defenses = new MonsterDefinition.Defense
                {
                    Armor = int.Parse(
                        root.Element("defenses")?.Attribute("armor")?.Value ?? "0"),
                    DefenseValue = int.Parse(
                        root.Element("defenses")?.Attribute("defense")?.Value ?? "0")
                },

                Ai = new MonsterDefinition.AiSettings
                {
                    Type = ParseAiType(
                        root.Element("ai")?.Attribute("type")?.Value),
                    Speed = int.Parse(
                        root.Element("ai")?.Attribute("speed")?.Value ?? "1")
                }
            };

            // Attacks
            foreach (var atk in root.Element("attacks")?.Elements("attack") ?? [])
            {
                monster.Attacks.Add(new MonsterDefinition.Attack
                {
                    Name = atk.Attribute("name")?.Value ?? "melee",
                    IntervalMs = int.Parse(atk.Attribute("interval")?.Value ?? "2000"),
                    MinDamage = int.Parse(atk.Attribute("min")?.Value ?? "0"),
                    MaxDamage = int.Parse(atk.Attribute("max")?.Value ?? "0"),
                });
            }

            // Loot
            foreach (var item in root.Element("loot")?.Elements("item") ?? [])
            {
                monster.LootTable.Add(new MonsterDefinition.Loot
                {
                    Item = item.Attribute("name")?.Value
                        ?? item.Attribute("id")?.Value
                        ?? "unknown",
                    Chance = int.Parse(item.Attribute("chance")?.Value ?? "0"),
                    CountMax = int.Parse(item.Attribute("countmax")?.Value ?? "1")
                });
            }

            return monster;
        }

        private static Core.AiType ParseAiType(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "aggressive" => Core.AiType.Aggressive,
                "defensive" => Core.AiType.Defensive,
                _ => Core.AiType.Passive
            };
        }


    public static class MonsterRegistry
    {
        private static readonly Dictionary<string, MonsterDefinition> _monsters = new();

        public static IReadOnlyDictionary<string, MonsterDefinition> All => _monsters;

        public static void Register(MonsterDefinition monster)
        {
            _monsters[monster.Name] = monster;
        }

        public static MonsterDefinition? Get(string name)
            => _monsters.TryGetValue(name, out var m) ? m : null;

        public static int Count => _monsters.Count;
    }
}