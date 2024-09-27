using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour {
    public UnityEvent<Unit> OnSelect { get; private set; }

    [SerializeField] private GameObject unitVisual;
    [SerializeField] private Material outlineMaterial;

    /// <summary> ID for the unit, used for sorting ties when resolving commands. Range is [0, 5]. </summary>
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int Row { get; private set; }
    [field: SerializeField] public int Column { get; private set; }
    [field: SerializeField] public Transform UnitTop { get; private set; }

    public int Movement { get; private set; }
    private bool IsSelected { get => Player.Instance.CurrentUnit == this; }

    private void Awake() {
        (Row, Column) = Util.GetRowCol(transform);
        OnSelect ??= new();
        Movement = 3;
    }

    private void OnDisable() {
        OnSelect.RemoveAllListeners();
    }

    private void OnMouseDown() {
        if (!IsSelectable())
            return;

        if (IsSelected)
            Unselect();
        else {
            Select();
            Player.Instance.OnCommandStaged.AddListener(Player_OnCommandStaged);
        }
    }

    private void OnMouseEnter() {
        if (!IsSelectable())
            return;

        if (!IsSelected)
            AddOutline();
    }

    private void OnMouseExit() {
        if (!IsSelectable())
            return;

        if (!IsSelected)
            RemoveOutline();
    }

    private void Unselect() {
        RemoveOutline();
        OnSelect.Invoke(null);
    }

    private void Select() {
        AddOutline();
        OnSelect.Invoke(this);
    }

    private bool IsSelectable() {
        if (!Player.Instance.CanStageCommands)
            return false;
        
        // TODO: Hacky. Just serialize.
        var stagedCommands = GameObject.Find("StagedCommandsContainer").GetComponentsInChildren<StagedCommand>();

        foreach (var stagedCommand in stagedCommands)
            if (stagedCommand.Unit == this)
                return false;
        
        return true;
    }

    private void AddOutline() {
        var meshRenderer = unitVisual.GetComponent<MeshRenderer>();

        if (!meshRenderer.materials.Any(material => material.name.Contains(outlineMaterial.name)))
            meshRenderer.materials = meshRenderer.materials.Append(outlineMaterial).ToArray();
    }

    private void RemoveOutline() {
        var meshRenderer = unitVisual.GetComponent<MeshRenderer>();
        meshRenderer.materials = meshRenderer.materials.Where(material => !material.name.Contains(outlineMaterial.name)).ToArray();
    }

    private void Player_OnCommandStaged() {
        Unselect();
        Player.Instance.OnCommandStaged.RemoveListener(Player_OnCommandStaged);
    }
}
