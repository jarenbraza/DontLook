using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains logic for two primary goals:
/// 1. Instantiating, transforming, and destroying GameObjects.
/// 2. Updating GameObject components with game-relevant data.
/// </summary>
public class Game : MonoBehaviour {
    public CancelCommandEvent CancelCommandEvent { get; private set; }
    public StageCommandMoveEvent StageCommandMoveEvent { get; private set; }

    [SerializeField] private GameObject tileOriginal;
    [SerializeField] private GameObject unitOriginal;
    [SerializeField] private GameObject doorOriginal;
    [SerializeField] private GameObject wallOriginal;
    [SerializeField] private GameObject reachableTileHighlightOriginal;
    [SerializeField] private GameObject actionPreviewOriginal;

    [SerializeField] [Tooltip("Container to hold non-essential environment details, like walls and doors")]
    private GameObject environmentContainer;

    [SerializeField] [Tooltip("Container to hold interactable game-related objects")]
    private GameObject gameContainer;

    private Player player;

    // Game-related fields. To be used frequently by game logic.
    public Tile[,] Tiles { get; private set; }
    public Unit[] Units { get; private set; }

    // Fields generated and deleted during runtime.
    private IEnumerable<GameObject> renderedReachableTiles = Enumerable.Empty<GameObject>();
    private GameObject renderedActionPreview;

    void Awake() {
        CancelCommandEvent ??= new();
        StageCommandMoveEvent ??= new();

        RenderTiles();
        RenderUnits();
        RenderDoors();
        RenderWalls();
    }

    void Start() {
        player = gameObject.GetComponent<Player>();
        AddEventListeners();
    }

    void RenderTiles() {
        Tiles = new Tile[RealMapConstants.TotalRows, RealMapConstants.TotalColumns];

        for (var row = 0; row < RealMapConstants.TotalRows; row++) {
            for (var col = 0; col < RealMapConstants.TotalColumns; col++) {
                var tileRoomID = RealMapConstants.TileRoomIds[row, col];

                if (tileRoomID == null) {
                    Tiles[row, col] = null;
                    continue;
                }

                var renderedTile = Instantiate(tileOriginal, ComputeTilePosition(row, col), Quaternion.identity, gameContainer.transform);
                renderedTile.name = $"{tileOriginal.name}_{row}_{col}";

                var tile = renderedTile.GetComponent<Tile>();
                (tile.Row, tile.Col, tile.RoomId) = (row, col, tileRoomID.ThrowIfNull());
                Tiles[row, col] = tile;
            }
        }

        foreach (var (c1, r1, c2, r2) in RealMapConstants.Doors) {
            if (c1 < c2) {
                Tiles[r1, c1].hasDoor.Add(Direction.Right);
                Tiles[r2, c2].hasDoor.Add(Direction.Left);
            } else if (c1 > c2) {
                Tiles[r1, c1].hasDoor.Add(Direction.Left);
                Tiles[r2, c2].hasDoor.Add(Direction.Right);
            }
            if (r1 < r2) {
                Tiles[r1, c1].hasDoor.Add(Direction.Up);
                Tiles[r2, c2].hasDoor.Add(Direction.Down);
            } else if (r1 > r2) {
                Tiles[r1, c1].hasDoor.Add(Direction.Down);
                Tiles[r2, c2].hasDoor.Add(Direction.Up);
            }
        }

        foreach (var t in Tiles) {
            if (t == null) continue;

            if (t.Row == 1 && t.Col == 1) {
                var s = $"({t.Row},{t.Col}):";
                if (t.hasDoor.Contains(Direction.Right)) s += "R";
                if (t.hasDoor.Contains(Direction.Left)) s += "L";
                if (t.hasDoor.Contains(Direction.Up)) s += "U";
                if (t.hasDoor.Contains(Direction.Down)) s += "D";
                Debug.Log(s);
            }
        }
    }

    void RenderUnits() {
        var (startingRow, startingCol) = (0, 3);
        Units = new Unit[GameConstants.UnitCount];

        for (var id = 0; id < GameConstants.UnitCount; id++) {
            var renderedUnit = Instantiate(unitOriginal, ComputeUnitPosition(startingRow, startingCol, id), Quaternion.Euler(90, 0, -180), gameObject.transform);
            renderedUnit.name = $"{unitOriginal.name}_{id}";

            Units[id] = renderedUnit.GetComponent<Unit>();
            (Units[id].Id, Units[id].Row, Units[id].Col, Units[id].Movement) = (id, startingRow, startingCol, 3);
        }
    }

    void RenderDoors() {
        foreach (var (x1, y1, x2, y2) in RealMapConstants.Doors)
            RenderDoor(x1, y1, x2, y2);
    }

    void RenderDoor(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1)
            throw new Exception("Expected door coordinates to be adjacent");

        var tileScale = tileOriginal.transform.localScale;
        var (x, y) = (Math.Min(x1, x2), Math.Min(y1, y2));

        if (Math.Abs(x1 - x2) == 1) {
            var renderedDoor = Instantiate(
                doorOriginal,
                new Vector3(tileScale.x * x + tileScale.x / 2, tileScale.y * y, -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentContainer.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var renderedDoor = Instantiate(
                doorOriginal,
                new Vector3(tileScale.x * x, tileScale.y * y + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentContainer.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }
    }

    // For each edge across all tiles, if it is not connected to another tile or leads out-of-bounds, render a wall.
    void RenderWalls() {
        var uniqueWallCoordinates = new HashSet<(int, int, int, int)>();
        for (var row = 0; row < RealMapConstants.TotalRows; row++) {
            for (var col = 0; col < RealMapConstants.TotalColumns; col++) {
                if (RealMapConstants.TileRoomIds[row, col] == null)
                    continue;

                var adjacentCoordinates = new List<(int, int)>() { (row - 1, col), (row + 1, col), (row, col - 1), (row, col + 1) };
                var coordinatesToDrawWalls = adjacentCoordinates.Except(Tiles[row, col].GetConnectedTiles(Tiles).Select(t => (t.Row, t.Col)));

                foreach (var (coordinateRow, coordinateCol) in coordinatesToDrawWalls)
                    uniqueWallCoordinates.Add((
                        Math.Min(col, coordinateCol),
                        Math.Min(row, coordinateRow),
                        Math.Max(col, coordinateCol),
                        Math.Max(row, coordinateRow)
                    ));
            }
        }

        foreach (var (x1, y1, x2, y2) in uniqueWallCoordinates)
            RenderWall(x1, y1, x2, y2);
    }

    void RenderWall(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1)
            throw new Exception("Expected wall coordinates to be adjacent");

        var tileScale = tileOriginal.transform.localScale;

        if (Math.Abs(x1 - x2) == 1) {
            var renderedWall = Instantiate(
                wallOriginal,
                new Vector3(tileScale.x * Math.Min(x1, x2) + tileScale.x / 2, tileScale.y * Math.Min(y1, y2), -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentContainer.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var renderedWall = Instantiate(
                wallOriginal,
                new Vector3(tileScale.x * Math.Min(x1, x2), tileScale.y * Math.Min(y1, y2) + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentContainer.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }
    }

    IEnumerable<GameObject> RenderReachableTiles(Unit unit) {
        var reachableTiles = unit.GetReachableTiles(Tiles);
        List<GameObject> renderedReachableTiles = new();

        foreach (var tile in reachableTiles) {
            var renderedReachableTile = Instantiate(
                reachableTileHighlightOriginal,
                ComputeTilePosition(tile.Row, tile.Col) - new Vector3(0, 0, 1.51f), // Place reachable highlight highlight just above the tile
                Quaternion.Euler(0, 90, -90),
                tile.transform
            );
            renderedReachableTile.transform.localScale = new Vector3(0.8f, 1, 0.8f);
            tile.IsSelectable = true;
            renderedReachableTiles.Add(renderedReachableTile);
        }

        return renderedReachableTiles;
    }

    void DestroyReachableTiles() {
        foreach (var renderedReachableTile in renderedReachableTiles) {
            renderedReachableTile.GetComponentInParent<Tile>().IsSelectable = false;
            Destroy(renderedReachableTile);
        }

        renderedReachableTiles = Enumerable.Empty<GameObject>();
    }

    // Get the absolute position in the world of a tile at (x, y).
    Vector3 ComputeTilePosition(int row, int col) {
        return Vector3.Scale(tileOriginal.transform.localScale, new Vector3(col, row, 0));
    }

    // Get the absolute position in the world of a unit at (x, y).
    // TODO: Make unit position rely on how many units are in the room
    Vector3 ComputeUnitPosition(int row, int col, int unitId) {
        var tilePosition = ComputeTilePosition(row, col);
        var tileScale = tileOriginal.transform.localScale;
        var unitScale = unitOriginal.transform.localScale;

        return new Vector3(
            tilePosition.x + unitId % Convert.ToInt32(tileScale.x) - unitScale.x,
            tilePosition.y + unitId / Convert.ToInt32(tileScale.y) - unitScale.y,
            -tileScale.z / 2
        );
    }

    void AddEventListeners() {
        foreach (var tile in Tiles)
            if (tile != null)
                tile.ReachableTileClickEvent.AddListener(Tile_ReachableTileClickEvent);

        foreach (var unit in Units)
            unit.SelectUnitEvent.AddListener(Unit_SelectUnitEvent);

        gameContainer.GetComponent<Player>().CommitMoveActionEvent.AddListener(Player_CommitMoveEvent);
    }

    void CancelCommandButton_OnClick(ActionPreview actionPreview) {
        CancelCommandEvent.Invoke(actionPreview.unit, actionPreview.tile);
        Destroy(renderedActionPreview);
    }

    void Player_CommitMoveEvent(Unit unit, Tile tile) {
        unit.gameObject.transform.position = ComputeUnitPosition(tile.Row, tile.Col, unit.Id);
        Destroy(renderedActionPreview);
    }

    void Unit_SelectUnitEvent(Unit unit) {
        if (unit == null)
            DestroyReachableTiles();
        else
            renderedReachableTiles = RenderReachableTiles(unit);
    }

    // TODO: Instead, we would provide a pop-up UI on what action should be performed. For now, we always assume it's to move.
    void Tile_ReachableTileClickEvent(Tile tile) {
        var unit = player.SelectedUnit;
        renderedActionPreview = Instantiate(actionPreviewOriginal, unit.transform);

        var actionPreview = renderedActionPreview.GetComponent<ActionPreview>();

        actionPreview.actionPosition.transform.SetPositionAndRotation(
            new Vector3(ComputeTilePosition(tile.Row, tile.Col).x, ComputeTilePosition(tile.Row, tile.Col).y, -1.5f),
            Quaternion.Euler(0, 90, -90)
        );

        (actionPreview.unit, actionPreview.tile) = (unit, tile);
        actionPreview.cancelCommandButton.onClick.AddListener(() => CancelCommandButton_OnClick(actionPreview));

        DestroyReachableTiles();

        StageCommandMoveEvent.Invoke(unit, tile);
    }
}
