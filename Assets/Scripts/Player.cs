using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains logic for a single player.
/// </summary>
public class Player : MonoBehaviour {
    [SerializeField] private Button commitCommandButton;
    public CommitMoveActionEvent CommitMoveActionEvent { get; private set; }

    private Game game;
    private Affiliation affiliation;
    private List<UnitAction> unitActions;

    public bool CanSelectUnit {
        get => (unitActions == null) || unitActions.Count < (affiliation == Affiliation.Good ? 1 : 2);
    }
    public Unit SelectedUnit { get; private set; }

    void Awake() {
        CommitMoveActionEvent ??= new();
    }

    void Start() {
        game = gameObject.GetComponent<Game>();

        // TODO: For now, assume all players are good :) Eventually, we want to be able to choose teams in the Lobby.
        affiliation = Affiliation.Good;
        unitActions = new();
        SelectedUnit = null;

        AddEventListeners();
    }

    void AddEventListeners() {
        foreach (var unit in game.Units)
            unit.SelectUnitEvent.AddListener(Unit_OnSelectUnit);

        commitCommandButton.onClick.AddListener(CommitCommandButton_OnCommitCommand);
        game.StageCommandMoveEvent.AddListener(Game_OnStageCommand);
        game.CancelCommandEvent.AddListener(Game_OnCancelCommand);
    }

    void Unit_OnSelectUnit(Unit unit) {
        SelectedUnit = unit;
    }

    void CommitCommandButton_OnCommitCommand() {
        foreach (var unitAction in unitActions) {
            unitAction.Unit.Move(unitAction.Tile);
            CommitMoveActionEvent.Invoke(unitAction.Unit, unitAction.Tile);
        }

        unitActions.Clear();
    }

    void Game_OnStageCommand(Unit unit, Tile tile) {
        unitActions.Add(new UnitAction(unit, tile));
        SelectedUnit = null;
    }

    void Game_OnCancelCommand(Unit unit, Tile tile) {
        unitActions = unitActions.Where((unitAction) => unitAction.Unit != unit && unitAction.Tile != tile).ToList();
    }
}
