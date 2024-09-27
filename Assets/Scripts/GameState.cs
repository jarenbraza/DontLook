using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all data relevant for game logic.
/// </summary>
public class GameState : MonoBehaviour {
    public static GameState Instance { get; private set; }

    [SerializeField] private GameObject unitContainer;
    [SerializeField] private GameObject tileContainer;

    /// <summary> All units associated with the game. </summary>
    public List<Unit> Units { get; private set; }

    /// <summary> All tiles associated with the game. </summary>
    public List<Tile> Tiles { get; private set; }

    /// <summary> Key: Player ID, Value: Staged commands for the corresponding player. </summary>
    private Dictionary<string, List<StagedCommand>> stagedCommands;

    private void Awake() {
        Instance = this;

        Units = new(unitContainer.GetComponentsInChildren<Unit>());
        Tiles = new(tileContainer.GetComponentsInChildren<Tile>());
        stagedCommands = new();
    }

    private void OnDestroy() {
        Instance = null;
    }

    public void AddStagedCommand(string playerID, StagedCommand stagedCommand) {
        if (stagedCommands.TryGetValue(playerID, out var stagedCommandsForPlayer)) {
            stagedCommandsForPlayer.Add(stagedCommand);
        } else {
            stagedCommands.Add(playerID, new() { stagedCommand });
        }
    }

    public void RemoveStagedCommand(string playerID, StagedCommand stagedCommand) {
        if (stagedCommands.TryGetValue(playerID, out var stagedCommandsForPlayer)) {
            if (!stagedCommandsForPlayer.Remove(stagedCommand)) {
                Debug.Log("Unable to find staged command for player " + playerID);
            }
        } else {
            Debug.Log("Player " + playerID + " had no staged commands");
        }
    }
}
