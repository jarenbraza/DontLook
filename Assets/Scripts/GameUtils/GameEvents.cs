using System;
using UnityEngine.Events;

[Serializable]
public class SelectUnitEvent : UnityEvent<Unit> {}

[Serializable]
public class StageMoveActionEvent : UnityEvent<Unit, Tile> {}

[Serializable]
public class CommitMoveActionEvent : UnityEvent<Unit, Tile> {}

[Serializable]
public class CancelActionEvent : UnityEvent<Unit, Tile> {}
