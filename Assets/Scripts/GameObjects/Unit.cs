using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour {
    public SelectUnitEvent SelectUnitEvent { get; private set; }

    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Movement { get; set; }
    public HashSet<Tile> ReachableTiles { get; private set; }

    private Outline outline;

    void Awake() {
        SelectUnitEvent ??= new();
        ReachableTiles = new();

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineColor = Color.red;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = 0f;
    }

    public void Move(Tile tile) {
        (X, Y) = (tile.X, tile.Y);

        ClearUnitSelection();
    }

    /// <summary>
    /// When the unit is clicked, clear selections and then select this unit.
    /// </summary>
    void OnMouseUp() {
        if (!Game.CanSelectUnits)
            return;

        var isAlreadySelected = this == Game.SelectedUnit;

        ClearUnitSelection();

        if (isAlreadySelected != this) {
            Highlight();
            SetReachableTiles();
            SelectUnitEvent.Invoke(this);
        }
    }

    void OnMouseEnter() {
        if (!Game.CanSelectUnits)
            return;

        if (this != Game.SelectedUnit) {
            outline.OutlineWidth = GameConstants.UnitHoverWidth;
            outline.OutlineColor = GameConstants.UnitHoverColor;
        }
    }

    void OnMouseExit() {
        if (!Game.CanSelectUnits)
            return;

        if (this != Game.SelectedUnit)
            Unhighlight();
    }

    void Highlight() {
        outline.OutlineWidth = GameConstants.UnitSelectedWidth;
        outline.OutlineColor = GameConstants.UnitSelectedColor;
    }

    public void Unhighlight() {
        outline.OutlineWidth = 0f;
    }

    void ClearUnitSelection() {
        SelectUnitEvent.Invoke(null);
        ReachableTiles.Clear();
    }

    void SetReachableTiles() {
        var q = new Queue<(Tile, int)>(new[] { (Game.Tiles[X, Y], 0) });
        ReachableTiles = new HashSet<Tile>() { Game.Tiles[X, Y] };

        while (q.Count > 0) {
            var (currentTile, distanceTraveled) = q.Dequeue();

            if (distanceTraveled == Movement)
                continue;

            foreach (var tileToVisit in currentTile.GetConnectedTiles(Game.Tiles).Except(ReachableTiles)) {
                q.Enqueue((tileToVisit, distanceTraveled + 1));
                ReachableTiles.Add(tileToVisit);
            }
        }
    }
}
