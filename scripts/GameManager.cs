using Godot;
using System;

// GameManager.cs (Fase 3) — orquestador del endless runner con modos de juego.
// - Lee GameConfig.SelectedMode y crea el IGameMode activo.
// - Colision ROBUSTA por distancia (no depende de capas de fisica): al tocar
//   un obstaculo o que un perro lo alcance -> GAME OVER. Esto arregla el bug de
//   que el personaje "no hacia nada" al chocar.
// - Aceleracion gradual CAP 1.5x: la velocidad de scroll y de obstaculos escala
//   con el mismo multiplicador, asi la camara nunca se adelanta al jugador.
// - Lanzar objetos hacia atras (tecla) para derribar perros.
// - Pantalla de GAME OVER (imagen de caricatura + Reintentar/Menu).
// - Musica en bucle + control de volumen.

public partial class GameManager : Node
{
    [Export] public NodePath PlayerPath;
    [Export] public NodePath HudPath;
    [Export] public NodePath LevelGeneratorPath;
    [Export] public float WorldSpeed = 200f;
    [Export] public float SpeedRamp = 4f;
    [Export] public float DespawnX = -200f;
    [Export] public float HitHalfWidth = 30f;    // tolerancia de colision en X
    [Export] public float HitHalfHeight = 30f;    // tolerancia de colision en Y
    [Export] public float ThrowCooldown = 0.6f;

    private Player _player;
    private HUD _hud;
    private LevelGenerator _levelGenerator;
    private IGameMode _mode;
    private GameModeType _modeType;

    private bool _gameOver = false;
    private bool _won = false;
    private float _startX;
    private float _accelMult = 1f;
    private float _throwTimer = 0f;
    private MusicManager _music;

    // Tolerancia de captura de perro (debe coincidir con ChaseMode.CatchDistance).
    private float _catchDist = 40f;

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
        switch (_modeType)
        {
            case GameModeType.TimeAttack: _mode = new TimeAttackMode(); break;
            case GameModeType.Fuel:       _mode = new FuelMode(); break;
            case GameModeType.Chase:      _mode = new ChaseMode(); break;
            case GameModeType.Libre:      _mode = new LibreMode(); break;
            default:                       _mode = new TimeAttackMode(); break;
        }

        _player.PlayerHit += OnObstacleHitPlayer;
        _player.PlayerCollected += OnPlayerCollected;

        _mode.SetPlayerReference(_player);
        _levelGenerator?.ConfigureForMode(_modeType);
        _levelGenerator?.SetPlayerReference(_player);
        _levelGenerator?.SetGameManager(this);
        _levelGenerator?.ConfigureMap(GameConfig.SelectedMap, GameConfig.SelectedTurn);
        ApplyMapTint(GameConfig.SelectedMap, GameConfig.SelectedTurn);

        // Acciones de entrada (por si el project.godot no las tiene).
        RegisterAction("accelerate", Key.Shift);
        RegisterAction("throw", Key.X);

        // Musica + volumen por defecto.
        _music = new MusicManager();
        AddChild(_music);
        _music.SetVolume(0.6f);

        _mode.OnStart();
        _startX = _player != null ? _player.GlobalPosition.X : 0f;

        string modeName = _modeType == GameModeType.TimeAttack ? "Contra Reloj"
                        : _modeType == GameModeType.Chase ? "Perros"
                        : _modeType == GameModeType.Fuel ? "Gasolina" : "Libre";
        _hud?.SetStatus("Modo: " + modeName);
    }

    private void RegisterAction(string name, Key key)
    {
        if (!InputMap.HasAction(name))
        {
            InputEventKey ev = new InputEventKey();
            ev.Keycode = key;
            InputMap.AddAction(name);
            InputMap.ActionAddEvent(name, ev);
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        if (_gameOver || _won || _player == null) return;

        // --- Aceleracion gradual CAP 1.5x ---
        bool accel = Input.IsActionPressed("accelerate");
        _accelMult = accel ? 1.5f : 1.0f;
        // el jugador avanza mas rapido y los obstaculos tambien -> la camara
        // (que sigue al jugador) nunca se adelanta.
        _player.AccelMultiplier = _accelMult;

        WorldSpeed += SpeedRamp * dt;

        if (_levelGenerator != null)
        {
            foreach (Node child in _levelGenerator.GetChildren())
            {
                if (child is Obstacle obs)
                {
                    obs.Speed = -WorldSpeed * _accelMult;
                    obs.DespawnX = DespawnX;
                }
            }
        }

        _mode.OnUpdate(dt);

        // --- Colision ROBUSTA por distancia (obstaculos) ---
        float px = _player.GlobalPosition.X;
        float py = _player.GlobalPosition.Y - 16f; // centro del hitbox del jugador
        if (_levelGenerator != null)
        {
            foreach (Node child in _levelGenerator.GetChildren())
            {
                if (child is Obstacle obs)
                {
                    if (Mathf.Abs(obs.GlobalPosition.X - px) <= HitHalfWidth &&
                        Mathf.Abs(obs.GlobalPosition.Y - py) <= HitHalfHeight)
                    {
                        TriggerGameOver();
                        return;
                    }
                }
            }
        }

        float dist = (_player.GlobalPosition.X - _startX) / 32f;
        _hud?.SetDistance(dist * 32f);
        if (_modeType == GameModeType.Fuel)
            _hud?.SetTime(_mode.TimeRemaining);
        else if (_modeType == GameModeType.TimeAttack)
            _hud?.SetTime(_mode.TimeRemaining);
        else if (_modeType == GameModeType.Libre && _mode is LibreMode libre)
            _hud?.SetTime(libre.CurrentDifficulty); // muestra dificultad como "nivel"
        else
            _hud?.SetTime(0f);

        // --- Perros (modo Chase): mover y detectar captura ---
        if (_modeType == GameModeType.Chase && _mode is ChaseMode chase && _levelGenerator != null)
        {
            foreach (Node child in _levelGenerator.GetChildren())
            {
                if (child is Dog dog)
                {
                    dog.Speed = chase.CurrentDogSpeed;
                    if (dog.HasCaught(_catchDist))
                    {
                        TriggerGameOver();
                        return;
                    }
                }
            }
        }

        // --- Lanzar objeto hacia atras ---
        _throwTimer -= dt;
        if (Input.IsActionJustPressed("throw") && _throwTimer <= 0f)
        {
            _throwTimer = ThrowCooldown;
            SpawnThrown();
        }

        if (_mode.CheckLoseCondition()) { TriggerGameOver(); return; }
        if (_mode.CheckWinCondition())
        {
            _won = true;
            _hud?.SetStatus("¡META ALCANZADA!");
            return;
        }
    }

    private void SpawnThrown()
    {
        var scene = GD.Load<PackedScene>("res://scenes/obstacles/ThrownObject.tscn");
        if (scene == null || _player == null) return;
        ThrownObject t = (ThrownObject)scene.Instantiate();
        t.Position = new Vector2(_player.GlobalPosition.X - 20f, _player.GlobalPosition.Y - 16f);
        t.SetOrigin(_player.GlobalPosition.X);
        GetTree().Root.AddChild(t);
        GD.Print("[THROW] objeto lanzado");
    }

    private void OnPlayerCollected(string itemType)
    {
        if (_gameOver || _won) return;
        _mode.OnPlayerCollected(itemType);
    }

    private void ApplyMapTint(int mapIndex, TurnType turn)
    {
        Node root = GetTree().Root;
        var bg = NodeUtils.FindInTree<Sprite2D>(root, "Background");
        if (bg != null) bg.Modulate = MapConfig.TinteMapa(mapIndex, turn);
    }

    public void OnObstacleHitPlayer()
    {
        if (!_gameOver && !_won) TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        if (_gameOver || _won) return;
        _gameOver = true;
        _hud?.SetStatus("GAME OVER");
        if (_player != null) _player.Visible = false;

        var goScene = GD.Load<PackedScene>("res://scenes/GameOver.tscn");
        if (goScene != null)
        {
            var go = goScene.Instantiate();
            GetTree().Root.AddChild(go);
        }
        GD.Print("[GAME OVER] el jugador choco");
    }

    // Control de volumen desde fuera (p.ej. un slider en el HUD/pausa).
    public void SetMusicVolume(float linear)
    {
        _music?.SetVolume(linear);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if ((_gameOver || _won) && (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R))
        {
            GetTree().ReloadCurrentScene();
        }
        if (@event is InputEventKey esc && esc.Pressed && esc.Keycode == Key.Escape)
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        }
    }
}
