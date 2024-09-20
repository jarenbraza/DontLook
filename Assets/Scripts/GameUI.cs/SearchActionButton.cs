using UnityEngine;
using UnityEngine.EventSystems;

public class SearchCommandButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private LineRenderer lineRenderer;
    public Item Item { get; set; }

    void Start() {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        Utility.UpdateLineRenderer(lineRenderer);
    }

    /// <summary>
    /// Draw a line from this SearchCommandButton to the item.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if (Item == null) {
            Debug.Log($"No item associated with SearchCommandButton on object {gameObject}");
        } else {
            var screenPoint = Utility.EstimateScreenPointToWorld(gameObject.transform.position);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[] { screenPoint, Item.transform.position });
        }
    }

    /// <summary>
    /// Removes the drawn line created in OnPointerEnter.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        lineRenderer.positionCount = 0;
    }
}
