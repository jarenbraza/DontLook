using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {
    public enum Affiliation {
        Good,
        Bad
    }

    public static Player Instance { get; private set; }

    public UnityEvent OnCommandStaged { get; private set; }

    [SerializeField] private MoveCommandButton moveCommandButton;

    public Tile CurrentTile { get; private set; }
    public Unit CurrentUnit { get; private set; }
    public string ID { get; private set; } = Guid.NewGuid().ToString(); // TODO: Get this from lobby/relay
    public bool CanStageCommands { get => remainingCommands > 0; }

    private Affiliation affiliation = Affiliation.Good; // TODO: Get this from lobby/relay
    private int remainingCommands;

    private void Awake() {
        Instance = this;

        OnCommandStaged ??= new();
        CurrentTile = null;
        CurrentUnit = null;
        ResetRemainingCommands();
    }

    private void Start() {
        GameState.Instance.Units.ForEach(unit => unit.OnSelect.AddListener(Unit_OnSelect));
        GameState.Instance.Tiles.ForEach(tile => tile.OnSelect.AddListener(Tile_OnSelect));
        GameManager.Instance.OnTurnStart.AddListener(GameManager_OnTurnStart);

        moveCommandButton.OnClick.AddListener(MoveCommandButton_OnClick);
    }

    private void OnDestroy() {
        OnCommandStaged.RemoveAllListeners();

        Instance = null;
    }

    private void Unit_OnSelect(Unit unit) {
        CurrentUnit = unit;
    }

    private void Tile_OnSelect(Tile tile) {
        CurrentTile = tile;
    }

    private void MoveCommandButton_OnClick(StagedCommand stagedCommand) {
        remainingCommands -= 1;
        stagedCommand.OnCancel.AddListener(StagedCommand_OnCancel);
        OnCommandStaged.Invoke();
    }

    private void StagedCommand_OnCancel(StagedCommand stagedCommand) {
        remainingCommands += 1;
        stagedCommand.OnCancel.RemoveListener(StagedCommand_OnCancel);
    }

    private void GameManager_OnTurnStart() {
        ResetRemainingCommands();
    }

    private void ResetRemainingCommands() {
        remainingCommands = affiliation == Affiliation.Good ? 2 : 1;
    }
}
