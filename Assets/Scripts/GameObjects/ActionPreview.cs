using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionPreview : MonoBehaviour {
    public GameObject actionPosition;
    public Canvas canvas;
    public Button cancelCommandButton;
    [DoNotSerialize] public Unit unit;
    [DoNotSerialize] public Tile tile;
}
