using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CommandPreview : MonoBehaviour {
    public GameObject position;
    public Canvas canvas;
    public Button cancelCommandButton;
    [DoNotSerialize] public Unit unit;
    [DoNotSerialize] public Tile tile;
}
