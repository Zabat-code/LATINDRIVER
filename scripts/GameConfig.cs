using Godot;

// GameConfig.cs
// Configuración global de selección de modo. El menú principal (MainMenu.cs)
// fija SelectedMode antes de cambiar a la escena Level, y GameManager.cs lo
// lee en Setup() para instanciar el modo correcto. Es un singleton estático
// simple (no autoload) porque la selección ocurre justo antes del cambio de
// escena y se consigue en el mismo proceso.

public enum GameModeType
{
	TimeAttack,
	Chase,
	Fuel
}

public static class GameConfig
{
	public static GameModeType SelectedMode = GameModeType.TimeAttack;
}
