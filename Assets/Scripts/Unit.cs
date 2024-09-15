using UnityEngine;

public class Unit : MonoBehaviour {
    private Outline outline;

    public GameObject unitGameObject;
    public GameMap tileMap;

    private readonly float UnitSelectedWidth = 5f;
    private readonly Color UnitSelectedColor = Color.red;
    private readonly float UnitHoverWidth = 1f;
    private readonly Color UnitHoverColor = Color.yellow;

    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Movement { get; set; }

    void Start() {
        outline = unitGameObject.AddComponent<Outline>();
        outline.OutlineColor = Color.red;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = 0f;
    }

    void OnMouseUp() {
        
        Debug.Log("Clicked");
        if (this == tileMap.SelectedUnit) {
            tileMap.UnselectUnit();
        } else {
            tileMap.UnselectUnit();
            tileMap.SelectedUnit = this;
            Highlight();
        }
    }

    void OnMouseEnter() {
        Debug.Log("Entered");
        if (this != tileMap.SelectedUnit) {
            outline.OutlineWidth = UnitHoverWidth;
            outline.OutlineColor = UnitHoverColor;
        }
    }

    void OnMouseExit() {
        Debug.Log("Exited");
        if (this != tileMap.SelectedUnit)
            Unhighlight();
    }

    public void Highlight() {
        outline.OutlineWidth = UnitSelectedWidth;
        outline.OutlineColor = UnitSelectedColor;
    }

    public void Unhighlight() {
        outline.OutlineWidth = 0f;
    }
}
