using Godot;

// MainMenu.cs
// Pantalla principal (menú) de LATINDRIVER. Permite elegir modo de juego antes de
// entrar a la escena de nivel:
//   1 -> Contra Reloj (TimeAttack)
//   2 -> Perros (Chase)
//   3 -> Gasolina (Fuel)
// La selección se guarda en GameConfig.SelectedMode para que GameManager la lea al
// cargar Level.tscn. También hay un botón "Jugar" que arranca el modo actual.

public partial class MainMenu : Node2D
{
	private Button _playButton;
	private Button _mode1;
	private Button _mode2;
	private Button _mode3;
	private Label _modeLabel;

	public override void _Ready()
	{
		_playButton = NodeUtils.GetChildByName<Button>(this, "PlayButton");
		_mode1 = NodeUtils.GetChildByName<Button>(this, "Mode1Button");
		_mode2 = NodeUtils.GetChildByName<Button>(this, "Mode2Button");
		_mode3 = NodeUtils.GetChildByName<Button>(this, "Mode3Button");
		_modeLabel = NodeUtils.GetChildByName<Label>(this, "ModeLabel");

		_playButton.Pressed += OnPlayPressed;
		_mode1.Pressed += () => SelectMode(GameModeType.TimeAttack);
		_mode2.Pressed += () => SelectMode(GameModeType.Chase);
		_mode3.Pressed += () => SelectMode(GameModeType.Fuel);

		SelectMode(GameConfig.SelectedMode);
	}

	private void SelectMode(GameModeType mode)
	{
		GameConfig.SelectedMode = mode;
		string name = mode == GameModeType.TimeAttack ? "Contra Reloj"
			: mode == GameModeType.Chase ? "Perros" : "Gasolina";
		_modeLabel.Text = "Modo: " + name;
	}

	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Level.tscn");
	}
}
