using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    public HashSet<Direction> hasDoor = new();
    public List<Unit> units = new();
    public GameMap tileMap;

    public int X { get; set; }
    public int Y { get; set; }
    public int RoomId { get; set; }

    void OnMouseUp() {
        if (tileMap.IsVisitableForSelectedUnit(X, Y))
            tileMap.MoveSelectedUnit(X, Y);

        tileMap.UnselectUnit();
    }

    public List<Tile> GetConnectedTiles(Tile[,] tiles) {
        var connectedTiles = new List<Tile>();

        if (Y > 0 && IsConnected(tiles[X, Y - 1]))
            connectedTiles.Add(tiles[X, Y - 1]);

        if (Y < tiles.GetLength(0) - 1 && IsConnected(tiles[X, Y + 1]))
            connectedTiles.Add(tiles[X, Y + 1]);

        if (X > 0 && IsConnected(tiles[X - 1, Y]))
            connectedTiles.Add(tiles[X - 1, Y]);

        if (X < tiles.GetLength(1) - 1 && IsConnected(tiles[X + 1, Y]))
            connectedTiles.Add(tiles[X + 1, Y]);

        return connectedTiles;
    }

    // Two tiles are connected if they are part of the same room or if there is a door between them
    public bool IsConnected(Tile tile) {
        if (RoomId == tile.RoomId)
            return true;

        if (Y + 1 == tile.Y && hasDoor.Contains(Direction.Up))
            return true;

        if (Y - 1 == tile.Y && hasDoor.Contains(Direction.Down))
            return true;

        if (X + 1 == tile.X && hasDoor.Contains(Direction.Right))
            return true;

        if (X - 1 == tile.X && hasDoor.Contains(Direction.Left))
            return true;

        return false;
    }
}
