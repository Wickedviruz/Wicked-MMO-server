using System.Net.NetworkInformation;
using GameCore.Core;

namespace GameCore.World;

public sealed class Map
{
    private readonly Tile[,] _tiles;

    public int Width {get;}
    public int Height {get;}

    // create map with given size
    // all tiles default to grass

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        // skapa alla tiles
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = new Position(x, y, 0);
                _tiles[x,y] = new Tile(position, TileType.Grass);
            }
        }
    }

    // get tile at position
    public Tile? GetTile(Position pos)
    {
        // kolla om bounds först
        if (!IsInBounds(pos))
            return null;

        //Return tile
        return _tiles[pos.X, pos.Y];

    }

    // set tile type at position
    public void SetTileType(Position pos, TileType type)
    {
        var tile = GetTile(pos);
        if (tile != null)
        {
            tile.Type = type;
        }
    }

    // Check if positio is within map bounds
    public bool IsInBounds(Position pos)
    {
        return pos.X >= 0 && pos.X < Width &&
                pos.Y >= 0 && pos.Y < Height;
    }

}