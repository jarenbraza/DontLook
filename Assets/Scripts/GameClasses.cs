public class UnitAction {
    public Unit Unit { get; private set; }
    public (int, int) Position { get; private set; }

    public UnitAction(Unit unit, (int, int) position) {
        Unit = unit;
        Position = position;
    }
}