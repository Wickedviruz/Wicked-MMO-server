using GameCore.Core;
using GameCore.Entities;

namespace GameCore.World;

    // Temporary placeholders (vi skapar riktiga senare)
    // ===========================================

    public class Item
    {
        public int ItemId { get; set; }
    }
    // ============================================

public sealed class Tile
{

    // position i världen
    public Position Position {get;}

    //typ av tile
    public TileType Type {get; set;}

    // creatures på denna tile
    private readonly List<Creature> _creatures = new();

    // items på denna tile (guld, vapen osv)
    private readonly List<Item> _items = new();

    // public readonly access 
    public IReadOnlyList<Creature> Creatures => _creatures;
    public IReadOnlyList<Item> Items => _items;

    public Tile(Position position, TileType tpye = TileType.Grass)
    {
        Position = position;
        Type = tpye;
    }

    //====================
    // creature management
    //====================

    // add creature to this tile
    public void AddCreature(Creature creature)
    {
        _creatures.Add(creature);
    } 

    // remove creature from tile
    public bool RemoveCreature(Creature creature)
    {
        return _creatures.Remove(creature);
    }

    // check if tile has creatures
    public bool HasCreatures() => _creatures.Count > 0;

    //====================
    // Tile properties
    //====================

    // check if tile i walkable
    public bool IsWalkable()
    {
        // TODO: implement
        return Type != TileType.Wall && Type != TileType.Water;
    }

    // chekc if tile blocks movement
    public bool IsBlocked()
    {
        // TODO implemnet item blockage aswell
        return HasCreatures();
    }

}