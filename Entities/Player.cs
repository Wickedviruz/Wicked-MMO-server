using GameCore.World;

namespace GameCore.Entities;


public class Player : Creature
{
    public string AccountName {get; init;}

    // level
    public int Level {get; set;} =1;
    public long Experience {get;set;}=0;

    // mana
    public int Mana {get; set;} =100;
    public int MaxMana {get; set;}=100;

    //TODO: add skills

    public Player(string name, Position startPos, string accountName)
        : base(name,startPos)
    {
        AccountName = accountName;
    }
}