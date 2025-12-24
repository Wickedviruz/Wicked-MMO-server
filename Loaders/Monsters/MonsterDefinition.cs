using GameCore.Core;
namespace GameCore.Loaders.Monsters;

public sealed class MonsterDefinition
{
    
    // identity
    public string Name { get; init; } ="";
    public string Description { get; init; } ="";
    public string Race { get; init; } ="";

    // stats
    public int Experience { get; init; }
    public int Speed { get; init; }
    public int MaxHealth { get; init; }

    // combat
    public List<Attack> Attacks {get; init; }= new();
    public Defense Defenses { get; init; } = new();

    // loot
    public List<Loot> LootTable {get; init; }= new();

    // AI
    public AiSettings Ai { get; init; } = new();


    public sealed class Attack
    {
        public string Name {get; init;}="";
        public int IntervalMs { get; init;}
        public int MinDamage {get; init;}
        public int MaxDamage {get; init;}
    }

        public sealed class Defense
    {
        public int Armor {get; init;}
        public int DefenseValue {get; init;}
    }

    public sealed class Loot
    {
        public string Item {get; init; } = "";
        public int Chance { get; init; } // 0-10000
        public int CountMax { get; init; } = 1;
    }

    public sealed class AiSettings
    {
        public AiType Type { get; init; } = AiType.Passive;
        public int Speed { get; init; } = 1;
    }
}