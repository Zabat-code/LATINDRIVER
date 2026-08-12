using Godot;

// MainMenu.cs
// Pantalla principal (menú) de LATINDRIVER. Permite elegir:
//   - Modo de juego (1=Contra Reloj, 2=Perros, 3=Gasolina)
//   - Mapa (Pueblito/Ciudad Colonial/Megaciudad/Ciudad Marginada/Avenida) con 4/6
//   - Turno (Día/Tarde/Noche) con 7/8/9
// La selección se guarda en GameConfig para que GameManager la lea al cargar Level.
// Botón "Jugar" arranca el modo/mapa/turno elegido.

public partial class MainMenu : Node2D
{
    private Button _playButton;
    private Button _mode1, _mode2, _mode3;
    private Button _map4, _map5, _map6, _map7, _map8;
    private Button _turn7, _turn8, _turn9;
    private Label _modeLabel, _mapLabel, _turnLabel;

    public override void _Ready()
    {
        _playButton = NodeUtils.GetChildByName<Button>(this, "PlayButton");
        _mode1 = NodeUtils.GetChildByName<Button>(this, "Mode1Button");
        _mode2 = NodeUtils.GetChildByName<Button>(this, "Mode2Button");
        _mode3 = NodeUtils.GetChildByName<Button>(this, "Mode3Button");
        _map4 = NodeUtils.GetChildByName<Button>(this, "Map1Button");
        _map5 = NodeUtils.GetChildByName<Button>(this, "Map2Button");
        _map6 = NodeUtils.GetChildByName<Button>(this, "Map3Button");
        _map7 = NodeUtils.GetChildByName<Button>(this, "Map4Button");
        _map8 = NodeUtils.GetChildByName<Button>(this, "Map5Button");
        _turn7 = NodeUtils.GetChildByName<Button>(this, "Turn1Button");
        _turn8 = NodeUtils.GetChildByName<Button>(this, "Turn2Button");
        _turn9 = NodeUtils.GetChildByName<Button>(this, "Turn3Button");
        _modeLabel = NodeUtils.GetChildByName<Label>(this, "ModeLabel");
        _mapLabel = NodeUtils.GetChildByName<Label>(this, "MapLabel");
        _turnLabel = NodeUtils.GetChildByName<Label>(this, "TurnLabel");

        _playButton.Pressed += OnPlayPressed;
        _mode1.Pressed += () => SelectMode(GameModeType.TimeAttack);
        _mode2.Pressed += () => SelectMode(GameModeType.Chase);
        _mode3.Pressed += () => SelectMode(GameModeType.Fuel);
        _map4.Pressed += () => SelectMap(0);
        _map5.Pressed += () => SelectMap(1);
        _map6.Pressed += () => SelectMap(2);
        _map7.Pressed += () => SelectMap(3);
        _map8.Pressed += () => SelectMap(4);
        _turn7.Pressed += () => SelectTurn(TurnType.Dia);
        _turn8.Pressed += () => SelectTurn(TurnType.Tarde);
        _turn9.Pressed += () => SelectTurn(TurnType.Noche);

        SelectMode(GameConfig.SelectedMode);
        SelectMap(GameConfig.SelectedMap);
        SelectTurn(GameConfig.SelectedTurn);
    }

    private void SelectMode(GameModeType mode)
    {
        GameConfig.SelectedMode = mode;
        _modeLabel.Text = "Modo: " + (mode == GameModeType.TimeAttack ? "Contra Reloj" : mode == GameModeType.Chase ? "Perros" : "Gasolina");
    }

    private void SelectMap(int idx)
    {
        GameConfig.SelectedMap = idx;
        _mapLabel.Text = "Mapa: " + MapConfig.Maps[idx].Nombre;
    }

    private void SelectTurn(TurnType turn)
    {
        GameConfig.SelectedTurn = turn;
        _turnLabel.Text = "Turno: " + (turn == TurnType.Dia ? "Día" : turn == TurnType.Tarde ? "Tarde" : "Noche");
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/Level.tscn");
    }
}
