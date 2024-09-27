using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StagedCommand : MonoBehaviour {
    public UnityEvent<StagedCommand> OnCancel { get; private set; }

    [SerializeField] private CancelCommandButton cancelCommandButton;
    [SerializeField] private Transform commandPreview;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Tile Tile { get; private set; }
    public Unit Unit { get; private set; }

    private void Awake() {
        OnCancel ??= new();
    }

    private void Start() {
        // TODO: Not possible to serialize scene-level components in a prefab. See if there's anything better than a hardcoded lookup.
        var commitCommandButton = GameObject.Find("CommitCommandButton").GetComponent<CommitCommandButton>();
        commitCommandButton.OnClick.AddListener(CommitCommandButton_OnClick);
    }

    private void OnDestroy() {
        OnCancel.RemoveAllListeners();
    }

    public static StagedCommand SpawnStagedCommand(StagedCommandSO stagedCommandSO) {
        var stagedCommandContainer = GameObject.Find("StagedCommandsContainer").transform;

        Transform stagedCommandTransform = Instantiate(stagedCommandSO.prefab, stagedCommandContainer);

        var stagedCommand = stagedCommandTransform.GetComponent<StagedCommand>();

        stagedCommand.Tile = Player.Instance.CurrentTile;
        stagedCommand.Unit = Player.Instance.CurrentUnit;

        stagedCommand.cancelCommandButton.transform.position = Player.Instance.CurrentUnit.UnitTop.position;
        stagedCommand.cancelCommandButton.OnClick.AddListener(stagedCommand.CancelCommandButton_OnClick);

        stagedCommand.commandPreview.position = Player.Instance.CurrentTile.TileTop.position;

        stagedCommand.spriteRenderer.sprite = stagedCommandSO.sprite;

        return stagedCommand;
    }

    private void CancelCommandButton_OnClick() {
        OnCancel.Invoke(this);
        Destroy(gameObject);
    }

    private void CommitCommandButton_OnClick(IEnumerable<StagedCommand> stagedCommands) {
        Destroy(gameObject);
    }
}
