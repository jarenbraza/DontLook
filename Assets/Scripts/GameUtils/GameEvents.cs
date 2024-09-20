using UnityEngine.Events;

public class SelectUnitEvent : UnityEvent<Unit> {}
public class CancelCommandEvent : UnityEvent<Unit, Tile> {}
public class TileClickEvent : UnityEvent<Tile> {}

// Command staging events. Occur after a player selects a user, tile, and then a corresponding action.
public class StageCommandMoveEvent : UnityEvent<Unit, Tile> {}
public class StageCommandSearchEvent : UnityEvent {}

// Command committing events. Occur after a player clicks the "Commit Command" button. 
public class CommitCommandMoveEvent : UnityEvent<Unit, Tile> {}

/// <summary>
/// Generic event to indicate that any command was staged by a player.
/// Useful for UI or camera, which will do the same behavior every time any command is staged.
/// </summary>
public class StageCommandGenericEvent : UnityEvent {}
