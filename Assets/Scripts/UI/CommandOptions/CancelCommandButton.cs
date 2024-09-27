using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CancelCommandButton : MonoBehaviour, IPointerClickHandler {
    public UnityEvent OnClick { get; private set; }

    private void Awake() {
        OnClick ??= new();
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnClick.Invoke();
    }
}