using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler {
    public ReachableTileClickEvent ReachableTileClickEvent { get; private set; }

    public int Row { get; set; }
    public int Col { get; set; }
    public int RoomId { get; set; }
    public bool IsSelectable { get; set; }

    public HashSet<Direction> Doors { get; private set; }
    public List<Item> Items { get; private set; }

    void Awake() {
        ReachableTileClickEvent ??= new();
        Doors = new();
        Items = new();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (IsSelectable)
            ReachableTileClickEvent.Invoke(this);
    }

    public List<Tile> GetConnectedTiles(Tile[,] tiles) {
        var connectedTiles = new List<Tile>();

        if (Col > 0 && IsConnected(tiles[Row, Col - 1]))
            connectedTiles.Add(tiles[Row, Col - 1]);

        if (Col < tiles.GetLength(1) - 1 && IsConnected(tiles[Row, Col + 1]))
            connectedTiles.Add(tiles[Row, Col + 1]);

        if (Row > 0 && IsConnected(tiles[Row - 1, Col]))
            connectedTiles.Add(tiles[Row - 1, Col]);

        if (Row < tiles.GetLength(0) - 1 && IsConnected(tiles[Row + 1, Col]))
            connectedTiles.Add(tiles[Row + 1, Col]);

        return connectedTiles;
    }

    // Two tiles are connected if they are part of the same room or if there is a door between them
    public bool IsConnected(Tile tile) {
        if (tile == null)
            return false;

        if (RoomId == tile.RoomId)
            return true;

        if (Col + 1 == tile.Col && Doors.Contains(Direction.Right))
            return true;

        if (Col - 1 == tile.Col && Doors.Contains(Direction.Left))
            return true;

        if (Row + 1 == tile.Row && Doors.Contains(Direction.Up))
            return true;

        if (Row - 1 == tile.Row && Doors.Contains(Direction.Down))
            return true;

        return false;
    }
}
