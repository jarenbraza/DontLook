using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains core logic for running the game.
/// </summary>
public class Game : MonoBehaviour {
    [SerializeField] private Button commitCommandButton;

    private Unit[] units;
    private Affiliation affiliation;
    private List<UnitAction> unitActions;

    public static bool CanSelectUnits { get; private set; }
    public static Unit SelectedUnit { get; private set; }
    public static Tile[,] Tiles { get; private set; }

    public CommitMoveActionEvent CommitMoveActionEvent { get; private set; }

    void Awake() {
        CommitMoveActionEvent ??= new();
    }

    // Render the game. Update all GameObject components with data to be used later.
    void Start() {
        commitCommandButton.onClick.AddListener(CommitCommandHandler);

        var objectRenderer = gameObject.GetComponent<Render>();
        objectRenderer.CancelCommandEvent.AddListener(CancelCommandEventHandler);

        UpdateTiles(objectRenderer.RenderTiles());
        UpdateUnits(objectRenderer.RenderUnits());
        objectRenderer.RenderDoors();
        objectRenderer.RenderWalls(Tiles);

        // TODO: For now, assume all players are good :) Eventually, we want to be able to choose teams or randomize across everyone.
        affiliation = Affiliation.Good;
        unitActions = new();
        SelectedUnit = null;
        CanSelectUnits = true;
    }

    void UpdateTiles(GameObject[,] tileGameObjects) {
        Tiles = new Tile[MapConstants.MapSizeX, MapConstants.MapSizeY];

        for (var x = 0; x < MapConstants.MapSizeX; x++) {
            for (var y = 0; y < MapConstants.MapSizeY; y++) {
                var tile = tileGameObjects[x, y].GetComponent<Tile>();
                (tile.X, tile.Y, tile.RoomId) = (x, y, MapConstants.TileRoomIds[x, y]);
                Tiles[x, y] = tile;
            }
        }

        foreach (var (x1, y1, x2, y2) in MapConstants.Doors) {
            Tiles[x1, y1].hasDoor.Add(x1 < x2 ? Direction.Right : Direction.Left);
            Tiles[x2, y2].hasDoor.Add(x1 < x2 ? Direction.Left : Direction.Right);
            Tiles[x1, y1].hasDoor.Add(y1 < y2 ? Direction.Up : Direction.Down);
            Tiles[x2, y2].hasDoor.Add(y1 < y2 ? Direction.Down : Direction.Up);
        }

        foreach (var tile in Tiles)
            tile.StageMoveActionEvent.AddListener(StageMoveActionEventHandler);
    }

    void UpdateUnits(GameObject[] unitGameObjects) {
        units = new Unit[GameConstants.UnitCount];

        for (var id = 0; id < GameConstants.UnitCount; id++) {
            var unit = unitGameObjects[id].GetComponent<Unit>();
            (unit.Id, unit.X, unit.Y, unit.Movement) = (id, 0, 0, 3);
            units[id] = unit;
        }

        foreach (var unit in units)
            unit.SelectUnitEvent.AddListener(SelectUnitEventHandler);
    }

    void CancelCommandEventHandler(Unit unit, Tile tile) {
        unitActions = unitActions.Where((unitAction) => unitAction.Unit != unit && unitAction.Tile != tile).ToList();
        CanSelectUnits = true;
    }

    void CommitCommandHandler() {
        foreach (var unitAction in unitActions) {
            unitAction.Unit.Move(unitAction.Tile);
            CommitMoveActionEvent.Invoke(unitAction.Unit, unitAction.Tile);
        }

        unitActions.Clear();
        CanSelectUnits = true;
    }

    void StageMoveActionEventHandler(Unit unit, Tile tile) {
        if (SelectedUnit.ReachableTiles.Contains(tile)) {
            SelectedUnit.Move(tile);
            unitActions.Add(new UnitAction(unit, tile));
            CanSelectUnits = affiliation == Affiliation.Good ? unitActions.Count < 1 : unitActions.Count < 2;
        }
    }

    void SelectUnitEventHandler(Unit unit) {
        if (SelectedUnit != null)
            SelectedUnit.Unhighlight();

        SelectedUnit = unit;
    }
}
