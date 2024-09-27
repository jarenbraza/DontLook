using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    private enum State {
        WaitingForPlayersToCommit,
        WaitingForPlayersToAnimate,
        EndOfTurn,
        GoodVictory,
        EvilVictory
    }

    [SerializeField] private CommitCommandButton commitCommandButton;

    public UnityEvent OnTurnStart { get; private set; }

    /// <summary> Players still in the process of committing commands. </summary>
    private HashSet<string> remainingPlayers;

    private int remainingTurns;
    private State state;

    private void Awake() {
        Instance = this;
        OnTurnStart ??= new();
        remainingPlayers = new();
        remainingTurns = 10;
        state = State.WaitingForPlayersToCommit;
    }

    private void Start() {
        commitCommandButton.OnClick.AddListener(CommitCommandButton_OnClick);
        remainingPlayers.Add(Player.Instance.ID); // TODO: Update this with RPC from all players
    }

    private void Update() {
        switch (state) {
            case State.WaitingForPlayersToCommit:
                if (remainingPlayers.Count == 0) {
                    // TODO: Resolve all commands (overwrites, duplicates, etc.)
                    // TODO: Broadcast resolved commands for players to animate
                    state = State.WaitingForPlayersToAnimate;
                }
                break;
            case State.WaitingForPlayersToAnimate:
                // TODO: Trigger player animations
                state = State.EndOfTurn;
                break;
            case State.EndOfTurn:
                if (IsEvilVictory()) {
                    state = State.EvilVictory;
                    // TODO: Invoke evil victory event
                }
                else if (remainingTurns == 0) {
                    state = State.GoodVictory;
                    // TODO: Invoke good victory event
                }
                else {
                    state = State.WaitingForPlayersToCommit;
                    remainingPlayers.Add(Player.Instance.ID);
                    OnTurnStart.Invoke();
                }
                break;
        }
    }

    private void OnDestroy() {
        OnTurnStart.RemoveAllListeners();
        remainingPlayers = null;
        Instance = null;
    }

    // TODO: Should get the player that clicked the commit button
    private void CommitCommandButton_OnClick(IEnumerable<StagedCommand> stagedCommands) {
        foreach (var stagedCommand in stagedCommands)
            GameState.Instance.AddStagedCommand(Player.Instance.ID, stagedCommand);

        remainingPlayers.Remove(Player.Instance.ID);
    }

    // TODO: Impl. Should check if killer has weapon and is in same room as target.
    private bool IsEvilVictory() {
        return false;
    }
}
