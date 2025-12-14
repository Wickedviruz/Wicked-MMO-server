using GameCore.Entities;

namespace GameCore.Core;

public class CreatureManager
{
    private Dictionary<Guid, Creature> _creatures = new();

    public void AddCreature(Creature creature)
    {
        ArgumentNullException.ThrowIfNull(creature);
        _creatures[creature.Id] = creature;
    }

    public void RemoveCreature(Creature creature)
    {
        //TODO: add funktion
    }

    public void GetCreatureById(Guid id)
    {
        //TODO: add funktion
    }

    public void GetAllCreatures()
    {
        //TODO: implementera senare.
    }
}