using Godot;

// GameConfig.cs
// Configuración global de selección (modo + mapa + turno). El menú principal
// fija estos valores antes de cambiar a Level; GameManager los lee en Setup().

public enum GameModeType
{
    TimeAttack,
    Chase,
    Fuel
}

public static class GameConfig
{
            public static GameModeType SelectedMode = GameModeType.TimeAttack;
    public static int SelectedMap = 1;            // índice en MapConfig.Maps (0..4)
    public static TurnType SelectedTurn = TurnType.Tarde;
}
