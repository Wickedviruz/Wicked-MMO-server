using GameCore.World;

namespace GameCore.Entities;

public class Creature
{
    // identitet
    public Guid Id{get;}
    public string Name {get; set;}

    // position
    public Position CurrentPosition{get; private set;}

    // stats
    public int Health {get; private set;}
    public int MaxHealth {get;}
    public int Speed {get;}

    public Creature(string name, Position startPos, int maxHealth = 100,int speed = 100)
    {
        Id = Guid.NewGuid();
        Name = name;
        CurrentPosition = startPos;
        MaxHealth = maxHealth;
        Health = maxHealth;
        Speed = speed;
    }

    //TODO: movemnet method
    public bool MoveTo(Position newPos, Map map)
    {
        Tile? currentTile = map.GetTile(CurrentPosition);
        Tile? newTile = map.GetTile(newPos);
        if (newTile == null || currentTile == null)
            return false;

        if(newTile.IsWalkable() && !newTile.IsBlocked())
        {
            currentTile.RemoveCreature(this);
            newTile.AddCreature(this);
            CurrentPosition = newPos;
            return true;
        }

        return false;
    }

    public void TakeDamage(int damageAmount)
    {
        Health = Math.Max(0, Health - damageAmount);
    }

    public void Attack(Creature target)
    {
        if (target == this)
        {
            Console.WriteLine("You cannot attack yourself!");
            return;
        }
        if (target.IsDead)
        {
            // ge text till spelaren att target is dead
            Console.WriteLine("Target is already dead");
            return;
        }

        int dmg = Random.Shared.Next(10, 30);
        target.TakeDamage(dmg);
    }

    public void Healing(int healingAmount)
    {
        Health = Math.Min(MaxHealth, Health + healingAmount);
    }

    public bool IsDead => Health<= 0;
}