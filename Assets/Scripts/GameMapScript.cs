using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMap : MonoBehaviour {
    [SerializeField] private GameObject tileVisualPrefab;
    [SerializeField] private GameObject unitVisualPrefab;
    [SerializeField] private GameObject doorVisualPrefab;
    [SerializeField] private GameObject wallVisualPrefab;
    [SerializeField] private GameObject selectableTileVisualPrefab;

    private const int tileMapSizeX = 3;
    private const int tileMapSizeY = 3;
    private const int unitCount = 6;
    private Unit[] units;
    private Tile[,] tiles;

    public Unit SelectedUnit { get; set; }

    void Start() {
        GenerateTiles();
        GenerateUnits();

        // TODO: For all of the below, would be better to have some sort of file to parse this from to support multiple maps cleanly

        // Manually set doors.
        GenerateDoor(tiles[0, 0], tiles[1, 0]);
        GenerateDoor(tiles[2, 1], tiles[2, 2]);

        // Manually set rooms.
        tiles[0, 0].RoomId = 0;
        tiles[1, 0].RoomId = 1;
        tiles[2, 0].RoomId = 1;
        tiles[0, 1].RoomId = 0;
        tiles[1, 1].RoomId = 1;
        tiles[2, 1].RoomId = 1;
        tiles[0, 2].RoomId = 2;
        tiles[1, 2].RoomId = 2;
        tiles[2, 2].RoomId = 2;

        // Manually set walls. Walls should be placed between (1) two unconnected tiles or (2) a tile and out-of-bounds.
        var uniqueWallCoordinates = new HashSet<(int, int, int, int)>();
        for (var x = 0; x < tileMapSizeX; x++) {
            for (var y = 0; y < tileMapSizeY; y++) {
                var tile = tiles[x, y];
                var adjacentCoordinates = new List<(int, int)>() { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
                var coordinatesToDrawWalls = adjacentCoordinates.Except(tile.GetConnectedTiles(tiles).Select(t => (t.X, t.Y)));
                foreach (var (cx, cy) in coordinatesToDrawWalls)
                    uniqueWallCoordinates.Add((Math.Min(tile.X, cx), Math.Min(tile.Y, cy), Math.Max(tile.X, cx), Math.Max(tile.Y, cy)));
            }
        }
        foreach (var c in uniqueWallCoordinates)
            GenerateWall(c.Item1, c.Item2, c.Item3, c.Item4);
    }

    void GenerateTiles() {
        tiles = new Tile[tileMapSizeX, tileMapSizeY];

        for (var x = 0; x < tileMapSizeX; x++) {
            for (var y = 0; y < tileMapSizeY; y++) {
                var tile = Instantiate(tileVisualPrefab, GetTilePosition(x, y), Quaternion.identity).GetComponent<Tile>();
                tile.X = x;
                tile.Y = y;
                tile.tileMap = this;
                tiles[x, y] = tile;
            }
        }
    }

    private void GenerateDoor(Tile t1, Tile t2) {
        if (Math.Abs(t1.X - t2.X) + Math.Abs(t1.Y - t2.Y) != 1) {
            throw new Exception("Expected tiles with door in-between to be adjacent");
        }

        var tileScale = tileVisualPrefab.transform.localScale;

        if (Math.Abs(t1.X - t2.X) == 1) {
            var door = Instantiate(
                doorVisualPrefab,
                new Vector3(tileScale.x * t1.X + tileScale.x / 2, tileScale.y * t1.Y, -1.5f),
                Quaternion.Euler(90, 90, -90)
            );
            door.transform.localScale = new Vector3(1, 3, 3);

            if (t1.X < t2.X) {
                t1.hasDoor.Add(Direction.Right);
                t2.hasDoor.Add(Direction.Left);
            } else {
                t1.hasDoor.Add(Direction.Left);
                t2.hasDoor.Add(Direction.Right);
            }
        }

        if (Math.Abs(t1.Y - t2.Y) == 1) {
            var door = Instantiate(
                doorVisualPrefab,
                new Vector3(tileScale.x * t1.X, tileScale.y * t1.Y + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90)
            );
            door.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);

            if (t1.Y < t2.Y) {
                t1.hasDoor.Add(Direction.Up);
                t2.hasDoor.Add(Direction.Down);
            } else {
                t1.hasDoor.Add(Direction.Down);
                t2.hasDoor.Add(Direction.Up);
            }
        }
    }

    private void GenerateWall(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1) {
            throw new Exception("Expected wall coordinates to be adjacent");
        }

        var tileScale = tileVisualPrefab.transform.localScale;

        if (Math.Abs(x1 - x2) == 1) {
            var wall = Instantiate(
                wallVisualPrefab,
                new Vector3(tileScale.x * Math.Min(x1, x2) + tileScale.x / 2, tileScale.y * Math.Min(y1, y2), -1.5f),
                Quaternion.Euler(90, 90, -90)
            );
            wall.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var wall = Instantiate(
                wallVisualPrefab,
                new Vector3(tileScale.x * Math.Min(x1, x2), tileScale.y * Math.Min(y1, y2) + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90)
            );
            wall.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }
    }

    void GenerateUnits() {
        units = new Unit[unitCount];

        for (var id = 0; id < unitCount; id++) {
            // TODO: Update so units are randomly spawned across the initial room.
            var startingX = 0;
            var startingY = 0;
            var unitGameObject = Instantiate(unitVisualPrefab, GetUnitPosition(startingX, startingY, id), Quaternion.Euler(90, 0, -180));
            var unit = unitGameObject.GetComponent<Unit>();
            unit.unitGameObject = unitGameObject;
            unit.Id = id;
            unit.X = startingX;
            unit.Y = startingY;
            unit.Movement = 3;
            unit.tileMap = this;

            tiles[startingX, startingY].units.Add(unit);

            units[id] = unit;
        }
    }

    public void UnselectUnit() {
        if (SelectedUnit != null) {
            SelectedUnit.Unhighlight();
            SelectedUnit = null;
        }
    }

    public void MoveSelectedUnit(int x, int y) {
        if (!tiles[SelectedUnit.X, SelectedUnit.Y].units.Remove(SelectedUnit)) {
            Debug.Log($"Unable to find unit {SelectedUnit.Id} in tiles");
        }
        tiles[x, y].units.Add(SelectedUnit);
        SelectedUnit.transform.position = GetUnitPosition(x, y, SelectedUnit.Id);
        SelectedUnit.X = x;
        SelectedUnit.Y = y;
    }

    // Check if the unit can make it to (x, y). Accounts for tile connections.
    public bool IsVisitableForSelectedUnit(int x, int y) {
        if (SelectedUnit == null)
            return false;

        // BFS with distance tracked.
        var startingTile = tiles[SelectedUnit.X, SelectedUnit.Y];
        var q = new Queue<(Tile, int)>(new[] { (startingTile, 0) });
        var visitedTiles = new HashSet<Tile> { startingTile };

        while (q.Count > 0) {
            var (currentTile, distanceTraveled) = q.Dequeue();

            if (currentTile.X == x && currentTile.Y == y)
                return true;

            if (distanceTraveled == SelectedUnit.Movement)
                continue;

            foreach (var tileToVisit in currentTile.GetConnectedTiles(tiles).Except(visitedTiles)) {
                q.Enqueue((tileToVisit, distanceTraveled + 1));
                visitedTiles.Add(tileToVisit);
            }
        }

        return false;
    }

    // Get the absolute position in Unity space of a tile at (x, y).
    private Vector3 GetTilePosition(int x, int y) {
        return Vector3.Scale(tileVisualPrefab.transform.localScale, new Vector3(x, y, 0));
    }

    // Get the absolute position in Unity space of a unit at (x, y).
    private Vector3 GetUnitPosition(int x, int y, int unitId) {
        var tilePosition = GetTilePosition(x, y);
        var tileScale = tileVisualPrefab.transform.localScale;
        var unitScale = unitVisualPrefab.transform.localScale;

        return new Vector3(
            tilePosition.x + unitId % Convert.ToInt32(tileScale.x) - unitScale.x,
            tilePosition.y + unitId / Convert.ToInt32(tileScale.y) - unitScale.y,
            unitScale.z / 2 - tileScale.z
        );
    }
}
