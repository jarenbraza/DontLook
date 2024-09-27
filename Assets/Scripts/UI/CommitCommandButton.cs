using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CommitCommandButton : MonoBehaviour, IPointerClickHandler {
    public UnityEvent<IEnumerable<StagedCommand>> OnClick { get; private set; }

    [SerializeField] private GameObject stagedCommandContainer;

    private void Awake() {
        OnClick ??= new();
    }

    private void OnDisable() {
        OnClick.RemoveAllListeners();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (Player.Instance.CanStageCommands) {
            var stagedCommands = stagedCommandContainer.GetComponentsInChildren<StagedCommand>();
            if (stagedCommands.Length == 0) {
                Debug.Log("No staged commands during commit for player " + Player.Instance.ID);
            }
            OnClick.Invoke(new List<StagedCommand>(stagedCommands));
        } else {
            Debug.Log("Player " + Player.Instance.ID + " tried to commit with remaining commands");
        }
    }
}
