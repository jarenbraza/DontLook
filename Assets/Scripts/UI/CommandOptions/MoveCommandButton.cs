using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MoveCommandButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public UnityEvent<StagedCommand> OnClick { get; private set; }

    [SerializeField] private StagedCommandSO stagedCommandSO;
    [SerializeField] private Transform moveCommandButtonTransform;
    [SerializeField] private LineRenderer lineRenderer;

    private void Awake() {
        OnClick ??= new();
    }

    private void OnDisable() {
        OnClick.RemoveAllListeners();
    }

    public void OnPointerClick(PointerEventData eventData) {
        var stagedCommand = StagedCommand.SpawnStagedCommand(stagedCommandSO);
        OnClick.Invoke(stagedCommand);
    }

    /// <summary> Render line from center of button (estimated) to the top of the currently selected tile. </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        var buttonCenter = Util.EstimateScreenPointToWorld(moveCommandButtonTransform.position);
        var tileCenter = Player.Instance.CurrentTile.TileTop.position;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new[] { buttonCenter, tileCenter });
    }

    /// <summary> Clear all rendered lines. </summary>
    public void OnPointerExit(PointerEventData eventData) {
        lineRenderer.positionCount = 0;
    }
}
