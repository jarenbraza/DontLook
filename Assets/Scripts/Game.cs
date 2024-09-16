using UnityEngine;

/// <summary>
/// Contains core logic for running the game.
/// </summary>
public class Game : MonoBehaviour {
    private Tile[,] tiles;
    private Unit[] units;

    public Affiliation Affiliation { get; set; }
    public UnitAction[] UnitActions { get; private set; }

    // Render the game. Update all GameObject components with data to be used later.
    void Start() {
        var objectRenderer = gameObject.GetComponent<Render>();

        UpdateTiles(objectRenderer.RenderTiles());
        UpdateUnits(objectRenderer.RenderUnits());
        UpdateDoors();
        objectRenderer.RenderDoors();
        objectRenderer.RenderWalls(tiles);

        // TODO: For now, assume all players are good :) Eventually, we want to be able to choose teams or randomize across everyone.
        Affiliation = Affiliation.Good;
        UnitActions = Affiliation == Affiliation.Good ? new UnitAction[1] : new UnitAction[2];

        // Must be at the end. All components should be in the scene now, so it is safe to add event listeners.
        AddListeners();
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

    public void CommitActions() {
        // TODO: Basically, for each action made by the player, perform game (logic) updates and rendering (visual) updates
        // TODO: Rendered visual updates should be a prefab.
        // TODO: For now, we're doing action right away (since moves are rendered instantly).
    }

    void AddListeners() {
        Unit.UnitActionEvent.AddListener(HandleUnitActionEvent);
    }

    void HandleUnitActionEvent(UnitAction unitAction) {
        UnitActions[0] = unitAction;
        Debug.Log($"Stored unit action for unit {unitAction.Unit.Id} to move to {unitAction.Position.Item1},{unitAction.Position.Item2}");
    }
}
