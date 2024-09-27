using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tile : MonoBehaviour {
    public UnityEvent<Tile> OnSelect { get; private set; }

    [SerializeField] private GameObject tileVisual;
    [SerializeField] private GameObject tileHighlightHoveredVisual;
    [SerializeField] private GameObject tileHighlightUnhoveredVisual;
    [SerializeField] private List<Tile> connectedTiles;

    [field: SerializeField] public int Row { get; private set; }
    [field: SerializeField] public int Column { get; private set; }
    [field: SerializeField] public Transform TileTop { get; private set; }

    private bool isSelectable;

    private bool IsSelected { get => Player.Instance.CurrentTile == this; }

    private void Awake() {
        OnSelect ??= new();
        (Row, Column) = Util.GetRowCol(transform);
        isSelectable = false;
        MakeUnselectable();
    }

    private void Start() {
        foreach (var unit in GameState.Instance.Units)
            unit.OnSelect.AddListener(Unit_OnSelect);
    }

    private void OnDisable() {
        OnSelect.RemoveAllListeners();
    }

    private void OnMouseDown() {
        if (isSelectable) {
            foreach (var tile in GameState.Instance.Tiles)
                if (tile != this)
                    tile.MakeUnselectable();   

            tileHighlightHoveredVisual.SetActive(true);
            tileHighlightUnhoveredVisual.SetActive(false);

            Player.Instance.OnCommandStaged.AddListener(Player_OnCommandStaged);
            OnSelect.Invoke(this);
        }
    }

    private void OnMouseEnter() {
        if (isSelectable && !IsSelected) {
            tileHighlightHoveredVisual.SetActive(true);
            tileHighlightUnhoveredVisual.SetActive(false);
        }
    }

    private void OnMouseExit() {
        if (isSelectable && !IsSelected) {
            tileHighlightHoveredVisual.SetActive(false);
            tileHighlightUnhoveredVisual.SetActive(true);
        }
    }

    private void MakeUnselectable() {
        isSelectable = false;
        tileHighlightHoveredVisual.SetActive(false);
        tileHighlightUnhoveredVisual.SetActive(false);
    }

    /// <summary>
    /// Performs a depth-limited BFS starting from this tile.
    /// Each visited tile will be made selectable.
    /// </summary>
    private void UpdateSelectableTiles(int maxDepth) {
        var tilesToVisit = new Queue<(Tile, int)>();
        var visitedTiles = new HashSet<Tile>();

        tilesToVisit.Enqueue((this, 0));
        visitedTiles.Add(this);

        while (tilesToVisit.Count > 0) {
            var (currentTile, currentDepth) = tilesToVisit.Dequeue();

            currentTile.isSelectable = true;
            currentTile.tileHighlightUnhoveredVisual.SetActive(true);

            if (currentDepth == maxDepth || currentTile.connectedTiles == null)
                continue;

            foreach (var tileToVisit in currentTile.connectedTiles) {
                if (!visitedTiles.Contains(tileToVisit)) {
                    tilesToVisit.Enqueue((tileToVisit, currentDepth + 1));
                    visitedTiles.Add(currentTile);
                }
            }
        }
    }

    private void Unit_OnSelect(Unit unit) {
        if (unit == null)
            MakeUnselectable();
        else if (Row == unit.Row && Column == unit.Column)
            UpdateSelectableTiles(unit.Movement);
    }

    private void Player_OnCommandStaged() {
        MakeUnselectable();
        Player.Instance.OnCommandStaged.RemoveListener(Player_OnCommandStaged);
        OnSelect.Invoke(null);
    }
}
