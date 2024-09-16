using System;
using UnityEngine.Events;

/// <summary>
/// Triggers when an action is selected for a unit (but not yet committed.)
/// Passed data:
/// 1. Unit
/// 2. Coordinates where unit will perform action
/// </summary>
[Serializable]
public class UnitActionEvent : UnityEvent<UnitAction> {}