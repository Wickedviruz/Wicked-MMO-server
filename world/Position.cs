using GameCore.Core;

namespace GameCore.World;

public readonly record struct Position(int X, int Y, int Z = 0)
{
    // kalkulera distans till annan position.
    public float DistanceTo(Position other)
    {
        // todo implement pythagoras
        int dx = other.X -X;
        int dy = other.Y -Y;

        return MathF.Sqrt(dx * dx + dy * dy);
    }

    //check if this pos is adjacent to another ( max one tile away)
    public bool IsAdjacentTo(Position other)
    {
        // Steg 1: Räkna absolut skillnad i X
        // Math.Abs = absolutvärde (alltid positivt)
        // Abs(-2) = 2, Abs(3) = 3
        int diffX = Math.Abs(X - other.X);
        
        // Steg 2: Räkna absolut skillnad i Y
        int diffY = Math.Abs(Y - other.Y);
        
        // Steg 3: Kolla om max 1 bort i X OCH Y OCH samma Z
        // && = OCH (alla måste vara sanna)
        return diffX <= 1 && diffY <= 1 && Z == other.Z;
    }

    // move to position in direction
    public static Position operator +(Position pos, Direction dir)
    {
        return dir switch
        {
            // north = y minskar
            Direction.North => new Position(pos.X, pos.Y - 1, pos.Z),
            // south = Y ökar
            Direction.South => new Position(pos.X, pos.Y + 1, pos.Z),
            // east = x ökar
            Direction.East => new Position(pos.X + 1, pos.Y, pos.Z),
            // west = x minskar
            Direction.West => new Position(pos.X - 1, pos.Y, pos.Z),

            // om annan dir, return samma
            _ => pos
        };
    }
}