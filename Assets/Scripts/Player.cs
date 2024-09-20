using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains logic for a single player.
/// </summary>
public class Player : MonoBehaviour {
    [SerializeField] private Button commitCommandButton;
    public CommitCommandMoveEvent CommitMoveCommandEvent { get; private set; }

    private Game game;
    private Affiliation affiliation;
    private List<Command> stagedCommands;

    public bool CanSelectUnit {
        get => (stagedCommands == null) || stagedCommands.Count < (affiliation == Affiliation.Good ? 1 : 2);
    }
    public Unit SelectedUnit { get; private set; }

    void Awake() {
        CommitMoveCommandEvent ??= new();
    }

    void Start() {
        game = gameObject.GetComponent<Game>();

        // TODO: For now, assume all players are good :) Eventually, we want to be able to choose teams in the Lobby.
        affiliation = Affiliation.Good;
        stagedCommands = new();
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
        // TODO: Update to allow for multiple different commands. Maybe command types?
        foreach (var command in stagedCommands) {
            command.Unit.Move(command.Tile);
            CommitMoveCommandEvent.Invoke(command.Unit, command.Tile);
        }

        stagedCommands.Clear();
    }

    void Game_OnStageCommand(Unit unit, Tile tile) {
        stagedCommands.Add(new Command(unit, tile));
        SelectedUnit = null;
    }

    void Game_OnCancelCommand(Unit unit, Tile tile) {
        // TODO: Update to allow for multiple different commands. Maybe command types?
        stagedCommands = stagedCommands.Where(command => command.Unit != unit && command.Tile != tile).ToList();
    }
}
