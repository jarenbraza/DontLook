using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour {
    public static Unit SelectedUnit { get; private set; }
    public static UnitActionEvent UnitActionEvent { get; set; }

    private Outline outline;
    private Render render;

    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Movement { get; set; }
    public Tile[,] Tiles { get; set; }
    public HashSet<Tile> ReachableTiles { get; private set; } = new();

    void Awake() {
        UnitActionEvent ??= new();

        outline = gameObject.AddComponent<Outline>();
        render = gameObject.GetComponent<Render>();
        outline.OutlineColor = Color.red;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = 0f;
    }

    void OnMouseUp() {
        if (SelectedUnit == this) {
            UnselectUnit();
        } else {
            UnselectUnit();
            SelectedUnit = this;
            Highlight();
            SetReachableTiles();
            render.RenderReachableTiles(ReachableTiles);
        }
    }

    void OnMouseEnter() {
        if (this != SelectedUnit) {
            outline.OutlineWidth = GameConstants.UnitHoverWidth;
            outline.OutlineColor = GameConstants.UnitHoverColor;
        }
    }

    void OnMouseExit() {
        if (this != SelectedUnit)
            Unhighlight();
    }

    public void Move(int destinationX, int destinationY) {
        UnitActionEvent.Invoke(new UnitAction(this, (destinationX, destinationY)));
        (X, Y) = (destinationX, destinationY);
        UnselectUnit();
    }

    void Highlight() {
        outline.OutlineWidth = GameConstants.UnitSelectedWidth;
        outline.OutlineColor = GameConstants.UnitSelectedColor;
    }

    void Unhighlight() {
        outline.OutlineWidth = 0f;
    }

    void UnselectUnit() {
        if (SelectedUnit != null) {
            SelectedUnit.Unhighlight();
            SelectedUnit = null;
            ReachableTiles.Clear();
            render.DestroyReachableTiles();
        }
    }

    void SetReachableTiles() {
        var q = new Queue<(Tile, int)>(new[] { (Tiles[X, Y], 0) });
        ReachableTiles = new HashSet<Tile>() { Tiles[X, Y] };

        while (q.Count > 0) {
            var (currentTile, distanceTraveled) = q.Dequeue();

            if (distanceTraveled == Movement)
                continue;

            foreach (var tileToVisit in currentTile.GetConnectedTiles(Tiles).Except(ReachableTiles)) {
                q.Enqueue((tileToVisit, distanceTraveled + 1));
                ReachableTiles.Add(tileToVisit);
            }
        }
    }
}
