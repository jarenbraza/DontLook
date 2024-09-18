using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour {
    public SelectUnitEvent SelectUnitEvent { get; private set; }

    public int Id { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public int Movement { get; set; }

    private Outline outline;
    private Game game;
    private Player player;

    void Awake() {
        SelectUnitEvent ??= new();

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineColor = Color.red;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = 0f;
    }

    void Start() {
        player = GameObject.Find("GameContainer").GetComponent<Player>();
        game = GameObject.Find("GameContainer").GetComponent<Game>();

        game.CancelCommandEvent.AddListener((unit, _) => { unit.Unhighlight(); });
        game.StageCommandMoveEvent.AddListener((unit, _) => { unit.Unhighlight(); });
    }

    void OnMouseUp() {
        if (!player.CanSelectUnit)
            return;

        var isAlreadySelected = this == player.SelectedUnit;

        ClearUnitSelection();

        if (isAlreadySelected != this) {
            Highlight();
            SelectUnitEvent.Invoke(this);
        }
    }

    void OnMouseEnter() {
        if (player.CanSelectUnit && this != player.SelectedUnit) {
            outline.OutlineWidth = GameConstants.UnitHoverWidth;
            outline.OutlineColor = GameConstants.UnitHoverColor;
        }
    }

    void OnMouseExit() {
        if (player.CanSelectUnit && this != player.SelectedUnit)
            Unhighlight();
    }

    public void Move(Tile tile) {
        (Row, Col) = (tile.Row, tile.Col);
        ClearUnitSelection();
    }

    public ISet<Tile> GetReachableTiles(Tile[,] tiles) {
        var startingTile = tiles[Row, Col];
        var tilesToVisit = new Queue<(Tile, int)>(new[] { (startingTile, 0) });
        var reachableTiles = new HashSet<Tile>() { startingTile };

        while (tilesToVisit.Count > 0) {
            var (currentTile, distanceTraveled) = tilesToVisit.Dequeue();

            if (distanceTraveled == Movement)
                continue;

            foreach (var tileToVisit in currentTile.GetConnectedTiles(tiles).Except(reachableTiles)) {
                tilesToVisit.Enqueue((tileToVisit, distanceTraveled + 1));
                reachableTiles.Add(tileToVisit);
            }
        }

        return reachableTiles;
    }

    public void Unhighlight() {
        outline.OutlineWidth = 0f;
    }

    void Highlight() {
        outline.OutlineWidth = GameConstants.UnitSelectedWidth;
        outline.OutlineColor = GameConstants.UnitSelectedColor;
    }

    void ClearUnitSelection() {
        if (player.SelectedUnit != null)
            player.SelectedUnit.Unhighlight();

        SelectUnitEvent.Invoke(null);
    }
}
