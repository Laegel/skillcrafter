using System.Collections.Generic;
using System.Linq;
using Godot;



class TileMapTerrain
{

    public Dictionary<string, Vector2I> tiles = new()
{
    {"0000", new Vector2I(2, 2)}, {"0001", new Vector2I(2, 0)}, {"0010", new Vector2I(3, 0)}, {"0011", new Vector2I(3, 3)},
    {"0100", new Vector2I(1, 1)}, {"0101", new Vector2I(0, 0)}, {"0110", new Vector2I(3, 2)}, {"0111", new Vector2I(1, 3)},
    {"1000", new Vector2I(0, 1)}, {"1001", new Vector2I(0, 3)}, {"1010", new Vector2I(1, 0)}, {"1011", new Vector2I(2, 3)},
    {"1100", new Vector2I(0, 2)}, {"1101", new Vector2I(3, 1)}, {"1110", new Vector2I(2, 1)}, {"1111", new Vector2I(1, 2)}
};

    public TileMapLayer tileMapLayer;
    public TileMapTerrain(TileMapLayer tileMapLayer)
    {
        this.tileMapLayer = tileMapLayer;
    }

    public void PlaceTile(List<Vector2I> tilemap, Vector2I position)
    {
        Vector2I? top = tilemap.Contains(position + new Vector2I(0, -1)) ? FindTile(tilemap, position + new Vector2I(0, -1)) : null;
        Vector2I? right = tilemap.Contains(position + new Vector2I(1, 0)) ? FindTile(tilemap, position + new Vector2I(1, 0)) : null;
        Vector2I? bottom = tilemap.Contains(position + new Vector2I(0, 1)) ? FindTile(tilemap, position + new Vector2I(0, 1)) : null;
        Vector2I? left = tilemap.Contains(position + new Vector2I(-1, 0)) ? FindTile(tilemap, position + new Vector2I(-1, 0)) : null;

        string neighbors = "" + (left.HasValue ? "1" : "0") +
                                (top.HasValue ? "1" : "0") +
                             (right.HasValue ? "1" : "0") +
                             (bottom.HasValue ? "1" : "0")
                             ;

        if (tiles.TryGetValue(neighbors, out Vector2I tile))
        {
            tileMapLayer.SetCell(position, 17, tile);
        }
        else
            GD.Print("Not supposed to happen");
    }

    public Vector2I FindTile(List<Vector2I> tilemap, Vector2I position)
    {
        return tilemap.FirstOrDefault(t => t == position);
    }
}