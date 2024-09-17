using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains logic for instantiating and positioning GameObjects into the world.
/// </summary>
public class Render : MonoBehaviour {
    public CancelActionEvent CancelCommandEvent { get; private set; }

    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject unit;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject reachableTileHighlight;
    [SerializeField] private GameObject actionPreview;

    [SerializeField] [Tooltip("Container to hold non-essential environment details, like walls and doors")]
    private GameObject environmentModels;

    [SerializeField] [Tooltip("Container to hold interactable game-related objects")]
    private GameObject game;

    // The fields below are generated and deleted during runtime.
    private IEnumerable<GameObject> renderedReachableTileHighlights = Enumerable.Empty<GameObject>();
    private GameObject renderedActionPreview;

    void Awake() {
        CancelCommandEvent ??= new();
    }

    void Start() {
        game.GetComponent<Game>().CommitMoveActionEvent.AddListener(CommitMoveActionEventHandler);
    }

    public GameObject[,] RenderTiles() {
        var renderedTiles = new GameObject[MapConstants.MapSizeX, MapConstants.MapSizeY];

        for (var x = 0; x < MapConstants.MapSizeX; x++) {
            for (var y = 0; y < MapConstants.MapSizeY; y++) {
                renderedTiles[x, y] = Instantiate(tile, ComputeTilePosition(x, y), Quaternion.identity, game.transform);
                renderedTiles[x, y].name = $"{tile.name}_{x}_{y}";
            }
        }

        foreach (var renderedTile in renderedTiles)
            renderedTile.GetComponent<Tile>().StageMoveActionEvent.AddListener(StageMoveActionEventHandler);

        return renderedTiles;
    }

    public GameObject[] RenderUnits() {
        var renderedUnits = new GameObject[GameConstants.UnitCount];

        for (var id = 0; id < GameConstants.UnitCount; id++) {
            renderedUnits[id] = Instantiate(unit, ComputeUnitPosition(0, 0, id), Quaternion.Euler(90, 0, -180), gameObject.transform);
            renderedUnits[id].name = $"{unit.name}_{id}";
        }

        foreach (var renderedUnit in renderedUnits)
            renderedUnit.GetComponent<Unit>().SelectUnitEvent.AddListener(SelectUnitEventHandler);

        return renderedUnits;
    }

    public void RenderDoors() {
        foreach (var (x1, y1, x2, y2) in MapConstants.Doors)
            RenderDoor(x1, y1, x2, y2);
    }

    void RenderDoor(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1)
            throw new Exception("Expected door coordinates to be adjacent");

        var tileScale = tile.transform.localScale;
        var x = Math.Min(x1, x2);
        var y = Math.Min(y1, y2);

        if (Math.Abs(x1 - x2) == 1) {
            var renderedDoor = Instantiate(
                door,
                new Vector3(tileScale.x * x + tileScale.x / 2, tileScale.y * y, -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentModels.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var renderedDoor = Instantiate(
                door,
                new Vector3(tileScale.x * x, tileScale.y * y + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentModels.transform
            );
            renderedDoor.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
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

    void RenderWall(int x1, int y1, int x2, int y2) {
        if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1)
            throw new Exception("Expected wall coordinates to be adjacent");

        var tileScale = tile.transform.localScale;

        if (Math.Abs(x1 - x2) == 1) {
            var renderedWall = Instantiate(
                wall,
                new Vector3(tileScale.x * Math.Min(x1, x2) + tileScale.x / 2, tileScale.y * Math.Min(y1, y2), -1.5f),
                Quaternion.Euler(90, 90, -90),
                environmentModels.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }

        if (Math.Abs(y1 - y2) == 1) {
            var renderedWall = Instantiate(
                wall,
                new Vector3(tileScale.x * Math.Min(x1, x2), tileScale.y * Math.Min(y1, y2) + tileScale.y / 2, -1.5f),
                Quaternion.Euler(0, 90, -90),
                environmentModels.transform
            );
            renderedWall.transform.localScale = new Vector3(1, tileScale.x * 2 / 3, tileScale.y);
        }
    }

    public IEnumerable<GameObject> RenderReachableTiles(IEnumerable<Tile> tiles) {
        List<GameObject> renderedReachableTiles = new();

        foreach (var tile in tiles) {
            var renderedHighlight = Instantiate(
                reachableTileHighlight,
                ComputeTilePosition(tile.X, tile.Y) - new Vector3(0, 0, 1.7f), // Place reachable highlight highlight just above the tile
                Quaternion.Euler(0, 90, -90),
                tile.transform
            );
            renderedHighlight.transform.localScale = new Vector3(0.8f, 1, 0.8f);
            renderedReachableTiles.Add(renderedHighlight);
        }

        return renderedReachableTiles;
    }

    public void DestroyReachableTiles() {
        foreach (var gameObject in renderedReachableTileHighlights)
            Destroy(gameObject);

        renderedReachableTileHighlights = Enumerable.Empty<GameObject>();
    }

    // Get the absolute position in the world of a tile at (x, y).
    public Vector3 ComputeTilePosition(int x, int y) {
        return Vector3.Scale(tile.transform.localScale, new Vector3(x, y, 0));
    }

    // Get the absolute position in the world of a unit at (x, y).
    // TODO: Make unit position rely on how many units are in the room
    public Vector3 ComputeUnitPosition(int x, int y, int unitId) {
        var tilePosition = ComputeTilePosition(x, y);
        var tileScale = tile.transform.localScale;
        var unitScale = unit.transform.localScale;

        return new Vector3(
            tilePosition.x + unitId % Convert.ToInt32(tileScale.x) - unitScale.x,
            tilePosition.y + unitId / Convert.ToInt32(tileScale.y) - unitScale.y,
            -tileScale.z / 2
        );
    }

    void CommitMoveActionEventHandler(Unit unit, Tile tile) {
        unit.gameObject.transform.position = ComputeUnitPosition(tile.X, tile.Y, unit.Id);
        Destroy(renderedActionPreview);
    }

    void StageMoveActionEventHandler(Unit unit, Tile tile) {
        renderedActionPreview = Instantiate(actionPreview, unit.transform);

        var actionPreviewComponent = renderedActionPreview.GetComponent<ActionPreview>();

        actionPreviewComponent.actionPosition.transform.SetPositionAndRotation(
            new Vector3(ComputeTilePosition(tile.X, tile.Y).x, ComputeTilePosition(tile.X, tile.Y).y, -1.5f),
            Quaternion.Euler(0, 90, -90)
        );

        actionPreviewComponent.unit = unit;
        actionPreviewComponent.tile = tile;
        actionPreviewComponent.cancelCommandButton.onClick.AddListener(() => CancelActionEventHandler(actionPreviewComponent));
    }

    void CancelActionEventHandler(ActionPreview actionPreview) {
        CancelCommandEvent.Invoke(actionPreview.unit, actionPreview.tile);
        Destroy(renderedActionPreview);
    }

    void SelectUnitEventHandler(Unit unit) {
        if (unit == null)
            DestroyReachableTiles();
        else
            renderedReachableTileHighlights = RenderReachableTiles(unit.ReachableTiles);
    }
}
