using UnityEngine.Events;

public class SelectUnitEvent : UnityEvent<Unit> {}
public class StageCommandMoveEvent : UnityEvent<Unit, Tile> {}
public class CommitMoveActionEvent : UnityEvent<Unit, Tile> {}
public class CancelCommandEvent : UnityEvent<Unit, Tile> {}
public class ReachableTileClickEvent : UnityEvent<Tile> {}
public class ActionOptionSelectedEvent : UnityEvent {}
