using UnityEngine;

/// <summary>
/// Contains core logic for running the game.
/// </summary>
public class Game : MonoBehaviour {
    private Tile[,] tiles;
    private Unit[] units;

    // Render the game. Update all GameObject components with data to be used later.
    void Start() {
        var objectRenderer = gameObject.GetComponent<Render>();

        UpdateTiles(objectRenderer.RenderTiles());
        UpdateUnits(objectRenderer.RenderUnits());
        UpdateDoors();
        objectRenderer.RenderDoors();
        objectRenderer.RenderWalls(tiles);
    }

    void UpdateTiles(GameObject[,] tileGameObjects) {
        tiles = new Tile[MapConstants.MapSizeX, MapConstants.MapSizeY];

        for (var x = 0; x < MapConstants.MapSizeX; x++) {
            for (var y = 0; y < MapConstants.MapSizeY; y++) {
                var tile = tileGameObjects[x, y].GetComponent<Tile>();
                (tile.X, tile.Y, tile.RoomId) = (x, y, MapConstants.TileRoomIds[x, y]);
                tiles[x, y] = tile;
            }
        }
    }

    void UpdateUnits(GameObject[] unitGameObjects) {
        units = new Unit[GameConstants.UnitCount];

        for (var id = 0; id < GameConstants.UnitCount; id++) {
            var unit = unitGameObjects[id].GetComponent<Unit>();
            (unit.Id, unit.X, unit.Y, unit.Movement, unit.Tiles) = (id, 0, 0, 3, tiles);
            units[id] = unit;
        }
    }

    void UpdateDoors() {
        foreach (var (x1, y1, x2, y2) in MapConstants.Doors) {
            tiles[x1, y1].hasDoor.Add(x1 < x2 ? Direction.Right : Direction.Left);
            tiles[x2, y2].hasDoor.Add(x1 < x2 ? Direction.Left : Direction.Right);
            tiles[x1, y1].hasDoor.Add(y1 < y2 ? Direction.Up : Direction.Down);
            tiles[x2, y2].hasDoor.Add(y1 < y2 ? Direction.Down : Direction.Up);
        }
    }
}
