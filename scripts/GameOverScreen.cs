using Godot;

// GameOverScreen.cs
// Pantalla de fin de partida: imagen de caricatura de accidente + dos botones
// (Reintentar / Menu). Se instancia desde GameManager al morir.
public partial class GameOverScreen : Control
{
	private Button _retry;
	private Button _menu;

	public override void _Ready()
	{
		_retry = NodeUtils.GetChildByName<Button>(this, "RetryButton");
		_menu  = NodeUtils.GetChildByName<Button>(this, "MenuButton");

		if (_retry != null) _retry.Pressed += OnRetry;
		if (_menu  != null) _menu.Pressed  += OnMenu;
	}

	private void OnRetry()
	{
		GetTree().ReloadCurrentScene();
	}

	private void OnMenu()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}
