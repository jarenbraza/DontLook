using UnityEngine;

public class CommandOptions : MonoBehaviour {
    public static CommandOptions Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private MoveCommandButton moveCommandButton;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (var tile in GameState.Instance.Tiles)
            tile.OnSelect.AddListener(Tile_OnSelect);

        moveCommandButton.OnClick.AddListener(MoveCommandButton_OnClick);

        Unrender();
    }

    private void OnDestroy() {
        Instance = null;
    }

    private void Render() {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Unrender() {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Tile_OnSelect(Tile tile) {
        if (tile != null)
            Render();
    }

    private void MoveCommandButton_OnClick(StagedCommand stagedCommand) {
        Unrender();
    }
}
