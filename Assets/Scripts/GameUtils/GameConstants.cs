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

class TestMapConstants {
    public static int TotalColumns { get; private set; } = 3;
    public static int TotalRows { get; private set; } = 3;
    public static (int, int, int, int)[] Doors { get; private set; } = {
        (0, 0, 1, 0),
        (2, 1, 2, 2)
    };
    public static int[,] TileRoomIds = {
        { 0, 1, 1 },
        { 0, 1, 1 },
        { 2, 2, 2 }
    };
}

class RealMapConstants {
    // Named N for formatting purposes
    private static readonly int? N = null;
    public static int TotalColumns { get; private set; } = 10;
    public static int TotalRows { get; private set; } = 5;

    public static (int, int, int, int)[] Doors { get; private set; } = {
        (7, 0, 8, 0),
        (0, 1, 1, 1),
        (1, 1, 2, 1),
        (2, 1, 3, 1),
        (5, 1, 6, 1),
        (6, 1, 7, 1),
        (0, 2, 1, 2),
        (1, 2, 2, 2),
        (6, 2, 7, 2),
        (2, 3, 3, 3),
        (3, 3, 4, 3),
        (7, 3, 8, 3),
        (3, 1, 3, 2),
        (5, 1, 5, 2),
        (5, 2, 5, 3),
        (5, 3, 5, 4),
        (6, 3, 6, 4),
        (7, 1, 7, 2),
        (8, 1, 8, 2),
        (9, 0, 9, 1)
    };

    public static int?[,] TileRoomIds = {
        { N, 1, 1, 2, 2, 2, 3, 3, 4, 5 },
        { 6, 1, 7, 2, 2, 2, 8, 3, 4, 4 },
        { 6, 9, 10,11,12,13,14,15,16,N },
        { N, 9, 9, 11,12,12,15,15,16,N },
        { N, N, N, N, N, 17,17,N, N, N }
    };
}