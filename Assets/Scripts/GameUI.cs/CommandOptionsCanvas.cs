using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class CommandOptionCanvas : MonoBehaviour {
    [field: SerializeField] private Canvas canvas;
    [field: SerializeField] public Button MoveCommandButton { get; private set; }
    [field: SerializeField] public List<Button> SearchCommandButtons { get; private set; }

    private Game game;

    void Start() {
        game = GameObject.Find("GameContainer").GetComponent<Game>();
        canvas.enabled = false;

        game.StageCommandMoveEvent.AddListener((_, _) => canvas.enabled = false);
        game.StageCommandSearchEvent.AddListener(() => canvas.enabled = false);
        
        foreach (var tile in game.Tiles)
            if (tile != null)
                tile.TileClickEvent.AddListener(Tile_OnTileClick);
    }

    void Tile_OnTileClick(Tile tile) {
        canvas.enabled = true;

        MoveCommandButton.GetComponent<MoveCommandButton>().Tile = tile;

        for (var i = 0; i < SearchCommandButtons.Count; i++) {
            var searchActionButton = SearchCommandButtons[i];

            if (i < tile.Items.Count) {
                searchActionButton.gameObject.SetActive(true);
                searchActionButton.GetComponent<SearchActionButton>().Item = tile.Items[i];
            }
            else {
                searchActionButton.gameObject.SetActive(false);
            }
        }
    }
}
