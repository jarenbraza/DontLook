using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains logic for instantiating and positioning GameObjects into the world.
/// </summary>
public class Render : MonoBehaviour {
    [SerializeField] private GameObject tileGameObject;
    [SerializeField] private GameObject unitGameObject;
    [SerializeField] private GameObject doorGameObject;
    [SerializeField] private GameObject wallGameObject;
    [SerializeField] private GameObject reachableTileGameObject;

    private List<GameObject> renderedReachableTiles = new();

    public GameObject[,] RenderTiles() {
        var renderedObjects = new GameObject[MapConstants.MapSizeX, MapConstants.MapSizeY];

        for (var x = 0; x < MapConstants.MapSizeX; x++)
            for (var y = 0; y < MapConstants.MapSizeY; y++)
                renderedObjects[x, y] = Instantiate(tileGameObject, ComputeTilePosition(x, y), Quaternion.identity);

        return renderedObjects;
    }

    public GameObject[] RenderUnits() {
        var renderedObjects = new GameObject[GameConstants.UnitCount];

        for (var id = 0; id < GameConstants.UnitCount; id++)
            renderedObjects[id] = Instantiate(unitGameObject, ComputeUnitPosition(0, 0, id), Quaternion.Euler(90, 0, -180));
        
        return renderedObjects;
    }

    public void RenderDoors() {
        foreach (var (x1, y1, x2, y2) in MapConstants.Doors)
            RenderDoor(x1, y1, x2, y2);
    }

    private void RenderDoor(int x1, int y1, int x2, int y2) {

        Debug.Log($"Rendering door between ({x1},{y1}) ({x2},{y2})");
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1) {
            throw new Exception("Expected door coordinates to be adjacent");
        }

        var tileScale = tileGameObject.transform.localScale;
        var x = Math.Min(x1, x2);
        var y = Math.Min(y1, y2);

        if (Math.Abs(x1 - x2) == 1) {
            var door = Instantiate(
                doorGameObject,
                new Vector3(tileScale.x * x + tileScale.x / 2, tileScale.y * y, -1.5f),
                Quaternion.Euler(90, 90, -90)
            );
            door.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var door = Instantiate(
                doorGameObject,
                new Vector3(tileScale.x * x, tileScale.y * y + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90)
            );
            door.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }
    }

    // For each edge across all tiles, if it is not connected to another tile or leads out-of-bounds, render a wall.
    public void RenderWalls(Tile[,] tiles) {
        var uniqueWallCoordinates = new HashSet<(int, int, int, int)>();
        for (var x = 0; x < MapConstants.MapSizeX; x++) {
            for (var y = 0; y < MapConstants.MapSizeY; y++) {
                var adjacentCoordinates = new List<(int, int)>() { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
                var coordinatesToDrawWalls = adjacentCoordinates.Except(tiles[x, y].GetConnectedTiles(tiles).Select(t => (t.X, t.Y)));

                foreach (var (cx, cy) in coordinatesToDrawWalls)
                    uniqueWallCoordinates.Add((Math.Min(x, cx), Math.Min(y, cy), Math.Max(x, cx), Math.Max(y, cy)));
            }
        }

        foreach (var (x1, y1, x2, y2) in uniqueWallCoordinates)
            RenderWall(x1, y1, x2, y2);
    }

    private void RenderWall(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1) {
            throw new Exception("Expected wall coordinates to be adjacent");
        }

        var tileScale = tileGameObject.transform.localScale;

        if (Math.Abs(x1 - x2) == 1) {
            var wall = Instantiate(
                wallGameObject,
                new Vector3(tileScale.x * Math.Min(x1, x2) + tileScale.x / 2, tileScale.y * Math.Min(y1, y2), -1.5f),
                Quaternion.Euler(90, 90, -90)
            );
            wall.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var wall = Instantiate(
                wallGameObject,
                new Vector3(tileScale.x * Math.Min(x1, x2), tileScale.y * Math.Min(y1, y2) + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90)
            );
            wall.transform.localScale = new Vector3(1, tileScale.x, tileScale.y);
        }
    }

    public void RenderReachableTiles(IEnumerable<Tile> tiles) {
        renderedReachableTiles = new();

        foreach (var tile in tiles) {
            var reachableTilePosition = ComputeTilePosition(tile.X, tile.Y);
            reachableTilePosition.z = -1.7f; // Place reachable tile highlight just above the floor
            var gameObject = Instantiate(reachableTileGameObject, reachableTilePosition, Quaternion.Euler(0, 90, -90));
            gameObject.transform.localScale = tileGameObject.transform.localScale;
            renderedReachableTiles.Add(gameObject);
        }
    }

    public void DestroyReachableTiles() {
        foreach (var gameObject in renderedReachableTiles)
            Destroy(gameObject);

        renderedReachableTiles.Clear();
    }

    // Get the absolute position in the world of a tile at (x, y).
    public Vector3 ComputeTilePosition(int x, int y) {
        return Vector3.Scale(tileGameObject.transform.localScale, new Vector3(x, y, 0));
    }

    // Get the absolute position in the world of a unit at (x, y).
    // TODO: Make unit position rely on how many units are in the room
    public Vector3 ComputeUnitPosition(int x, int y, int unitId) {
        var tilePosition = ComputeTilePosition(x, y);
        var tileScale = tileGameObject.transform.localScale;
        var unitScale = unitGameObject.transform.localScale;

        return new Vector3(
            tilePosition.x + unitId % Convert.ToInt32(tileScale.x) - unitScale.x,
            tilePosition.y + unitId / Convert.ToInt32(tileScale.y) - unitScale.y,
            unitScale.z / 2 - tileScale.z
        );
    }
}