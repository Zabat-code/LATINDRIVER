using Godot;
using System;

// GameManager.cs (Fase 2) — orquestador del endless runner con modos de juego.
// - Lee GameConfig.SelectedMode y crea el IGameMode activo.
// - Configura LevelGenerator para el modo (obstáculos / perros / bidones).
// - Mueve obstáculos, actualiza el modo, refleja estado en el HUD.
// - Game Over al golpear obstáculo o al cumplirse la condición de derrota del modo.
// - Victoria al cumplirse la condición de victoria del modo.
// - R reinicia la escena.

public partial class GameManager : Node
{
    [Export] public NodePath PlayerPath;
    [Export] public NodePath HudPath;
    [Export] public NodePath LevelGeneratorPath;
    [Export] public float WorldSpeed = 200f;
    [Export] public float SpeedRamp = 4f;
    [Export] public float DespawnX = -200f;

    private Player _player;
    private HUD _hud;
    private LevelGenerator _levelGenerator;
    private IGameMode _mode;
    private GameModeType _modeType;

    private bool _gameOver = false;
    private bool _won = false;
    private float _startX;

    public override void _Ready()
    {
        CallDeferred("Setup");
    }

    private void Setup()
    {
        Node root = GetTree().Root;
        _player = NodeUtils.FindInTree<Player>(root, "Player");
        _hud = NodeUtils.FindInTree<HUD>(root, "HUD");
        _levelGenerator = NodeUtils.FindInTree<LevelGenerator>(root, "LevelGenerator");

        _modeType = GameConfig.SelectedMode;

        // Instanciar el modo seleccionado.
        switch (_modeType)
        {
            case GameModeType.TimeAttack: _mode = new TimeAttackMode(); break;
            case GameModeType.Fuel:       _mode = new FuelMode(); break;
            case GameModeType.Chase:      _mode = new ChaseMode(); break;
            default:                       _mode = new TimeAttackMode(); break;
        }

        // Conectar recolección de ítems (Player emite PlayerCollected al tocar "pickup").
        _player.PlayerCollected += OnPlayerCollected;

        // Dar referencia del jugador al modo y al generador.
        _mode.SetPlayerReference(_player);
        _levelGenerator?.ConfigureForMode(_modeType);
        _levelGenerator?.SetPlayerReference(_player);
        _levelGenerator?.SetGameManager(this);

        _mode.OnStart();
        _startX = _player != null ? _player.GlobalPosition.X : 0f;

        string modeName = _modeType == GameModeType.TimeAttack ? "Contra Reloj"
                        : _modeType == GameModeType.Chase ? "Perros" : "Gasolina";
        _hud?.SetStatus("Modo: " + modeName);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        if (_gameOver || _won || _player == null) return;

        WorldSpeed += SpeedRamp * dt;

        // Mover obstáculos/pickups hacia la izquierda y limpiar fuera de pantalla.
        foreach (Node child in GetChildren())
        {
            if (child is Obstacle obs)
            {
                obs.Speed = -WorldSpeed;
                obs.DespawnX = DespawnX;
            }
        }

        // Actualizar modo y comprobar condiciones.
        _mode.OnUpdate(dt);

        float dist = (_player.GlobalPosition.X - _startX) / 32f;

        // HUD: distancia + medidor del modo (tiempo o combustible) + estado.
        _hud?.SetDistance(dist * 32f);
        if (_modeType == GameModeType.Fuel)
            _hud?.SetTime(_mode.TimeRemaining);   // tanque de gasolina
        else if (_modeType == GameModeType.TimeAttack)
            _hud?.SetTime(_mode.TimeRemaining);   // segundos
        else
            _hud?.SetTime(0f);                     // Chase: sin tiempo

        // Modo Chase: mover perros y detectar captura.
        if (_modeType == GameModeType.Chase && _mode is ChaseMode chase)
        {
            foreach (Node child in GetChildren())
            {
                if (child is Dog dog)
                {
                    dog.Speed = chase.CurrentDogSpeed;
                    if (dog.HasCaught(chase.CatchRadius))
                    {
                        TriggerGameOver();
                        return;
                    }
                }
            }
        }

        // Pérdida por condición del modo (tiempo 0 / gasolina 0).
        if (_mode.CheckLoseCondition())
        {
            TriggerGameOver();
            return;
        }

        // Victoria por condición del modo (llegar a meta X).
        if (_mode.CheckWinCondition())
        {
            _won = true;
            _hud?.SetStatus("¡META ALCANZADA!");
            return;
        }
    }

    private void OnPlayerCollected(string itemType)
    {
        if (_gameOver || _won) return;
        _mode.OnPlayerCollected(itemType);
    }

    // Llamado por LevelGenerator cuando un Obstacle golpea al jugador.
    public void OnObstacleHitPlayer()
    {
        if (!_gameOver && !_won) TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        _gameOver = true;
        _hud?.SetStatus("GAME OVER - Pulsa R");
        if (_player != null) _player.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if ((_gameOver || _won) && (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
