using UnityEngine;

public enum Direction {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}

public enum Affiliation {
    Good = 0,
    MainBad = 1,
    AssistantBad = 2
}

class GameConstants {
    public static float UnitSelectedWidth { get; private set; } = 3f;
    public static float UnitHoverWidth { get; private set; } = 1f;
    public static Color UnitSelectedColor { get; private set; } = Color.red;
    public static Color UnitHoverColor { get; private set; } = Color.yellow;
    public static int UnitCount { get; private set; } = 6;
}

class MapConstants {
    public static int MapSizeX { get; private set; } = 3;
    public static int MapSizeY { get; private set; } = 3;
    public static (int x1, int y1, int x2, int y2)[] Doors { get; private set; } = {
        (0, 0, 1, 0),
        (2, 1, 2, 2)
    };
    public static int[,] TileRoomIds = {
        { 0, 0, 2},
        { 1, 1, 2},
        { 1, 1, 2}
    };
}