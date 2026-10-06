using Godot;

// LevelGenerator.cs
// Genera contenido por delante del jugador mientras este avanza automaticamente.
// 4 carriles (0-3): Y positions separadas 70px.
// Según el modo (GameConfig.SelectedMode) genera:
//  - TimeAttack: solo obstáculos.
//  - Chase: obstáculos + perros (Dog) que persiguen desde atrás.
//  - Fuel: obstáculos + bidones de gasolina (Pickup) para recargar.
//  - Libre: obstáculos con dificultad progresiva (sin tiempo ni gasolina).

public partial class LevelGenerator : Node
{
    [Export] public PackedScene ObstacleScene;
    [Export] public PackedScene DogScene;
    [Export] public PackedScene PickupScene;
    [Export] public NodePath PlayerPath;
    [Export] public float SpawnInterval = 2.0f;
    [Export] public float SpawnAhead = 700f;
    [Export] public float ObstacleY = 512f;

    private Player _player;
    private GameManager _gameManager;
    private Godot.Timer _timer;
    private bool _spawning = true;
    private GameModeType _mode = GameModeType.TimeAttack;
    private float _worldSpeedMult = 1.0f;
    private float _difficultyMult = 1.0f;

    // 4 carriles: Y positions (separados 70px)
    private readonly float[] Lanes = { 440f, 510f, 580f, 650f };

    public void SetPlayerReference(Player player) { _player = player; }
    public void SetGameManager(GameManager gm) { _gameManager = gm; }

    public void ConfigureForMode(GameModeType mode)
    {
        _mode = mode;
        if (mode == GameModeType.Libre)
        {
            _difficultyMult = 1.0f;
        }
    }

    public override void _Ready()
    {
        CallDeferred("Setup");
    }

    private void Setup()
    {
        Node root = GetTree().Root;
        _player = NodeUtils.FindInTree<Player>(root, "Player");
        _timer = new Godot.Timer();
        _timer.WaitTime = SpawnInterval;
        _timer.Timeout += OnSpawnTimeout;
        AddChild(_timer);
        _timer.Start();
    }

    public void ConfigureMap(int mapIndex, TurnType turn)
    {
        float densidad = MapConfig.Maps[mapIndex].DensidadBase * MapConfig.TurnDensidad(turn);
        float velocidad = MapConfig.Maps[mapIndex].VelocidadBase * MapConfig.TurnVelocidad(turn);
        SpawnInterval = Mathf.Clamp(2.2f / densidad, 0.6f, 4.0f);
        _worldSpeedMult = velocidad;
        if (_timer != null) _timer.WaitTime = SpawnInterval;
    }

    public void StopSpawning()
    {
        _spawning = false;
        _timer?.Stop();
    }

    private void OnSpawnTimeout()
    {
        if (!_spawning || _player == null) return;

        float spawnX = _player.GlobalPosition.X + SpawnAhead;

        // Obstáculo en carril aleatorio (0-3)
        if (ObstacleScene != null)
        {
            int lane = (int)(GD.Randf() * Lanes.Length);
            float laneY = Lanes[lane];
            Obstacle obs = (Obstacle)ObstacleScene.Instantiate();
            obs.Position = new Vector2(spawnX, laneY);
            obs.Speed = -200f * _worldSpeedMult * _difficultyMult;
            AddChild(obs);
        }

        // Perros (modo Chase)
        if (_mode == GameModeType.Chase && DogScene != null)
        {
            Dog dog = (Dog)DogScene.Instantiate();
            dog.Position = new Vector2(_player.GlobalPosition.X - 450f, ObstacleY);
            dog.SetTarget(_player);
            AddChild(dog);
        }

        // Bidones de gasolina (modo Fuel)
        if (_mode == GameModeType.Fuel && PickupScene != null)
        {
            Pickup fuel = (Pickup)PickupScene.Instantiate();
            int lane = (int)(GD.Randf() * Lanes.Length);
            float laneY = Lanes[lane];
            fuel.Position = new Vector2(spawnX + 250f, laneY);
            AddChild(fuel);
        }

        // Modo Libre: aumentar dificultad progresivamente
        if (_mode == GameModeType.Libre)
        {
            _difficultyMult += 0.05f;
            SpawnInterval = Mathf.Max(0.4f, SpawnInterval * 0.98f);
            if (_timer != null) _timer.WaitTime = SpawnInterval;
        }
    }
}
