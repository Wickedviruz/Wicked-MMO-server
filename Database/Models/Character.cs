namespace GameCore.Database.Models;

// character model

public class Character
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = "";
    public string Class { get; set; } = "Warrior";
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    
    // Stats
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 50;
    public int MaxMana { get; set; } = 50;
    
    // Position
    public int PositionX { get; set; } = 0;
    public int PositionY { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? DeletedAt { get; set; }
}