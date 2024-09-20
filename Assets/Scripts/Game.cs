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
    public StageCommandSearchEvent StageCommandSearchEvent { get; private set; }
    public StageCommandGenericEvent StageCommandGenericEvent { get; private set; }

    // GameObjects to instantiate during runtime
    [SerializeField] private GameObject tileOriginal;
    [SerializeField] private GameObject unitOriginal;
    [SerializeField] private GameObject doorOriginal;
    [SerializeField] private GameObject wallOriginal;
    [SerializeField] private GameObject itemOriginal;
    [SerializeField] private GameObject tileHighlightOriginal;
    [SerializeField] private GameObject commandPreviewOriginal;

    [SerializeField] [Tooltip("Container to hold non-essential environment details, like walls and doors")]
    private GameObject environmentContainer;

    [SerializeField] [Tooltip("Container to hold interactable game-related objects")]
    private GameObject gameContainer;

    private Player player;

    // Game-related fields. To be used frequently by game logic.
    public Tile[,] Tiles { get; private set; }
    public Unit[] Units { get; private set; }
    public Tile SelectedTile { get; private set; }

    // Fields generated and deleted during runtime.
    private List<GameObject> renderedTileHighlights = new();
    private GameObject renderedCommandPreview;

    void Awake() {
        CancelCommandEvent ??= new();
        StageCommandMoveEvent ??= new();
        StageCommandSearchEvent ??= new();
        StageCommandGenericEvent ??= new();

        RenderTiles();
        RenderUnits();
        RenderDoors();
        RenderWalls();
        RenderItems();
    }

    void Start() {
        player = gameObject.GetComponent<Player>();
        AddEventListeners();
    }

    void RenderTiles() {
        Tiles = new Tile[MapConstants.TotalRows, MapConstants.TotalColumns];

        for (var row = 0; row < MapConstants.TotalRows; row++) {
            for (var col = 0; col < MapConstants.TotalColumns; col++) {
                var tileRoomID = MapConstants.TileRoomIds[row, col];

                if (tileRoomID == null) {
                    Tiles[row, col] = null;
                    continue;
                }

                var renderedTile = Instantiate(tileOriginal, ComputeTilePosition(row, col), Quaternion.identity, gameContainer.transform);
                renderedTile.name = $"{tileOriginal.name}_r{row}_c{col}";

                var tile = renderedTile.GetComponent<Tile>();
                (tile.Row, tile.Col, tile.RoomId) = (row, col, tileRoomID.ThrowIfNull());
                Tiles[row, col] = tile;
            }
        }

        foreach (var (c1, r1, c2, r2) in MapConstants.Doors) {
            if (c1 < c2) {
                Tiles[r1, c1].Doors.Add(Direction.Right);
                Tiles[r2, c2].Doors.Add(Direction.Left);
            } else if (c1 > c2) {
                Tiles[r1, c1].Doors.Add(Direction.Left);
                Tiles[r2, c2].Doors.Add(Direction.Right);
            }
            if (r1 < r2) {
                Tiles[r1, c1].Doors.Add(Direction.Up);
                Tiles[r2, c2].Doors.Add(Direction.Down);
            } else if (r1 > r2) {
                Tiles[r1, c1].Doors.Add(Direction.Down);
                Tiles[r2, c2].Doors.Add(Direction.Up);
            }
        }
    }

    void RenderUnits() {
        var (startingRow, startingCol) = (0, 3);
        Units = new Unit[GameConstants.UnitCount];

        var (targetId, killerId) = Utility.GetTwoUniqueInRange(GameConstants.UnitCount);

        for (var id = 0; id < GameConstants.UnitCount; id++) {
            var renderedUnit = Instantiate(unitOriginal, ComputeUnitPosition(startingRow, startingCol, id), Quaternion.Euler(90, 0, -180), gameObject.transform);
            renderedUnit.name = $"{unitOriginal.name}_{id}{(id == killerId ? "_killer" : "")}{(id == targetId ? "_target" : "")}";

            var unit = renderedUnit.GetComponent<Unit>();
            (unit.Id, unit.Row, unit.Col, unit.Movement) = (id, startingRow, startingCol, 3);
            unit.IsTarget = id == targetId;
            unit.IsKiller = id == killerId;
            
            Units[id] = unit;
        }
    }

    void RenderDoors() {
        foreach (var (c1, r1, c2, r2) in MapConstants.Doors)
            RenderDoor(c1, r1, c2, r2);
    }

    void RenderDoor(int c1, int r1, int c2, int r2) {
        if (Math.Abs(c1 - c2) + Math.Abs(r1 - r2) != 1)
            throw new Exception("Expected door coordinates to be adjacent");

        var tileScale = tileOriginal.transform.localScale;
        var (col, row) = (Math.Min(c1, c2), Math.Min(r1, r2));

        if (Math.Abs(c1 - c2) == 1) {
            var renderedDoor = Instantiate(
                doorOriginal,
                new Vector3(tileScale.x * col + tileScale.x / 2, tileScale.y * row, -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentContainer.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(r1 - r2) == 1) {
            var renderedDoor = Instantiate(
                doorOriginal,
                new Vector3(tileScale.x * col, tileScale.y * row + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentContainer.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }
    }

    // For each edge across all tiles, if it is not connected to another tile or leads out-of-bounds, render a wall.
    void RenderWalls() {
        var uniqueWallCoordinates = new HashSet<(int, int, int, int)>();
        for (var row = 0; row < MapConstants.TotalRows; row++) {
            for (var col = 0; col < MapConstants.TotalColumns; col++) {
                if (MapConstants.TileRoomIds[row, col] == null)
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

        foreach (var (c1, r1, c2, r2) in uniqueWallCoordinates)
            RenderWall(c1, r1, c2, r2);
    }

    void RenderWall(int c1, int r1, int c2, int r2) {
        if (Math.Abs(c1 - c2) + Math.Abs(r1 - r2) != 1)
            throw new Exception("Expected wall coordinates to be adjacent");

        var tileScale = tileOriginal.transform.localScale;

        if (Math.Abs(c1 - c2) == 1) {
            var renderedWall = Instantiate(
                wallOriginal,
                new Vector3(tileScale.x * Math.Min(c1, c2) + tileScale.x / 2, tileScale.y * Math.Min(r1, r2), -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentContainer.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(r1 - r2) == 1) {
            var renderedWall = Instantiate(
                wallOriginal,
                new Vector3(tileScale.x * Math.Min(c1, c2), tileScale.y * Math.Min(r1, r2) + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentContainer.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }
    }

    void RenderItems() {
        var (remainingFood, remainingWeapons, remainingTraps) = (MapConstants.TotalFood, MapConstants.TotalWeapons, MapConstants.TotalTraps);

        if (remainingFood + remainingWeapons + remainingTraps > MapConstants.TotalRows * MapConstants.TotalColumns * 2)
            throw new Exception("Too many items to place for map size and limitations");

        // Randomly place items throughout the map
        while (remainingFood > 0 || remainingWeapons > 0 || remainingTraps > 0) {
            var randomRow = UnityEngine.Random.Range(0, MapConstants.TotalRows);
            var randomCol = UnityEngine.Random.Range(0, MapConstants.TotalColumns);
            var randomTile = Tiles[randomRow, randomCol];

            if (randomTile == null || randomTile.Items.Count >= 2)
                continue;

            var renderedItem = Instantiate(
                itemOriginal,
                ComputeItemPosition(randomTile),
                Quaternion.Euler(0, 90, -90),
                randomTile.transform
            );
            var item = renderedItem.GetComponent<Item>();

            if (remainingFood > 0) {
                item.ItemType = ItemType.Food;
                remainingFood--;
            }
            else if (remainingWeapons > 0) {
                item.ItemType = ItemType.Weapon;
                remainingWeapons--;
            }
            else if (remainingTraps > 0) {
                item.ItemType = ItemType.Trap;
                remainingTraps--;
            }

            randomTile.Items.Add(item);
        }
    }

    void RenderTileHighlights(ISet<Tile> reachableTiles) {
        renderedTileHighlights = new List<GameObject>();

        foreach (var tile in reachableTiles) {
            var tileHighlight = Instantiate(
                tileHighlightOriginal,
                ComputeTilePosition(tile.Row, tile.Col) - new Vector3(0, 0, 1.51f), // Place reachable highlight highlight just above the tile
                Quaternion.Euler(0, 90, -90),
                tile.transform
            );
            tileHighlight.transform.localScale = new Vector3(0.8f, 1, 0.8f);
            renderedTileHighlights.Add(tileHighlight);
        }
    }

    void DestroyTileHighlights() {
        foreach (var renderedTileHighlight in renderedTileHighlights) {
            renderedTileHighlight.GetComponentInParent<Tile>().IsSelectable = false;
            Destroy(renderedTileHighlight);
        }

        renderedTileHighlights.Clear();
    }

    // Get the absolute position in the world of a tile at (col, row).
    Vector3 ComputeTilePosition(int row, int col) {
        return Vector3.Scale(tileOriginal.transform.localScale, new Vector3(col, row, 0));
    }

    // Get the absolute position in the world of a unit at (col, row).
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

    public Vector3 ComputeItemPosition(Tile tile) {
        var tilePosition = ComputeTilePosition(tile.Row, tile.Col);
        var tileScale = tileOriginal.transform.localScale;
        var itemScale = itemOriginal.transform.localScale;

        return new Vector3(
            tilePosition.x + tile.Items.Count - itemScale.x,
            tilePosition.y + 2 - itemScale.y,
            itemScale.z - tileScale.z
        );
    }

    void AddEventListeners() {
        foreach (var tile in Tiles)
            if (tile != null)
                tile.TileClickEvent.AddListener(Tile_OnTileClick);

        foreach (var unit in Units)
            unit.SelectUnitEvent.AddListener(Unit_OnSelectUnit);

        gameContainer.GetComponent<Player>().CommitMoveCommandEvent.AddListener(Player_OnCommitMove);

        var commandOptionsCanvas = GameObject.Find("CommandOptionsUI").GetComponent<CommandOptionCanvas>();

        commandOptionsCanvas.MoveCommandButton.onClick.AddListener(MoveCommandButton_OnClick);

        foreach (var searchCommandButton in commandOptionsCanvas.SearchCommandButtons)
            searchCommandButton.onClick.AddListener(SearchCommandButton_OnClick);
    }

    void CancelCommandButton_OnClick(CommandPreview commandPreview) {
        CancelCommandEvent.Invoke(commandPreview.unit, commandPreview.tile);
        Destroy(renderedCommandPreview);
    }

    void Player_OnCommitMove(Unit unit, Tile tile) {
        unit.gameObject.transform.position = ComputeUnitPosition(tile.Row, tile.Col, unit.Id);
        Destroy(renderedCommandPreview);
    }

    void Unit_OnSelectUnit(Unit unit) {
        if (unit == null)
            DestroyTileHighlights();
        else {
            var reachableTiles = unit.GetReachableTiles(Tiles);

            foreach (var tile in reachableTiles)
                tile.IsSelectable = true;

            RenderTileHighlights(reachableTiles);
        }
    }

    void SearchCommandButton_OnClick() {
        StageCommandSearchEvent.Invoke();

        StageCommandGenericEvent.Invoke();
        DestroyTileHighlights();
    }

    void MoveCommandButton_OnClick() {
        var unit = player.SelectedUnit;
        var tile = SelectedTile;
        renderedCommandPreview = Instantiate(commandPreviewOriginal, unit.transform);

        var commandPreview = renderedCommandPreview.GetComponent<CommandPreview>();

        commandPreview.position.transform.SetPositionAndRotation(
            new Vector3(ComputeTilePosition(tile.Row, tile.Col).x, ComputeTilePosition(tile.Row, tile.Col).y, -1.5f),
            Quaternion.Euler(0, 90, -90)
        );

        (commandPreview.unit, commandPreview.tile) = (unit, tile);
        commandPreview.cancelCommandButton.onClick.AddListener(() => CancelCommandButton_OnClick(commandPreview));

        StageCommandMoveEvent.Invoke(unit, tile);

        StageCommandGenericEvent.Invoke();
        DestroyTileHighlights();
    }

    void Tile_OnTileClick(Tile tile) {
        SelectedTile = tile;
        DestroyTileHighlights();
        RenderTileHighlights(new HashSet<Tile>{ tile });
    }
}
