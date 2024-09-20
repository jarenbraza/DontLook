public class Command {
    public Unit Unit { get; private set; }
    public Tile Tile { get; private set; }

    public Command(Unit unit, Tile tile) {
        Unit = unit;
        Tile = tile;
    }
}
