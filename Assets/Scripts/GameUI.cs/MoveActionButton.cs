using UnityEngine;
using UnityEngine.EventSystems;

public class MoveCommandButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private LineRenderer lineRenderer;
    public Tile Tile { get; set; }

    void Start() {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        Utility.UpdateLineRenderer(lineRenderer);
    }

    /// <summary>
    /// Draw a line from this MoveCommandButton to the tile.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if (Tile == null) {
            Debug.Log($"No tile associated with MoveCommandButton on object {gameObject}");
        } else {
            var screenPoint = Utility.EstimateScreenPointToWorld(gameObject.transform.position);
            var topOfTilePoint = Tile.transform.position - new Vector3(0, 0, Tile.transform.localScale.z / 2);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[] { screenPoint, topOfTilePoint });
        }
    }

    /// <summary>
    /// Removes the drawn line created in OnPointerEnter.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        lineRenderer.positionCount = 0;
    }
}
