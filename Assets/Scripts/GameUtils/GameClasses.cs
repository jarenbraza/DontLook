public class UnitAction {
    public Unit Unit { get; private set; }
    public Tile Tile { get; private set; }

    public UnitAction(Unit unit, Tile tile) {
        Unit = unit;
        Tile = tile;
    }
}
