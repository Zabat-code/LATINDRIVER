using Godot;

// LevelGenerator.cs
// Genera contenido por delante del jugador mientras este avanza automáticamente.
// El spawn usa un Godot.Timer y coloca cada elemento a una distancia fija por
// delante de la posición X del jugador.
// Según el modo (GameConfig.SelectedMode) genera:
//  - TimeAttack: solo obstáculos.
//  - Chase: obstáculos + perros (Dog) que persiguen desde atrás.
//  - Fuel: obstáculos + bidones de gasolina (Pickup) para recargar.
// Es un prototipo: tipos placeholder (rectángulos/imágenes). No se optimiza con
// pool todavía (ver TASKS.md fase 1/2).

public partial class LevelGenerator : Node
{
	[Export] public PackedScene ObstacleScene;
	[Export] public PackedScene DogScene;
	[Export] public PackedScene PickupScene;
	[Export] public NodePath PlayerPath;
	[Export] public float SpawnInterval = 2.0f;     // segundos entre oleadas
	[Export] public float SpawnAhead = 700f;        // px por delante del jugador
	[Export] public float ObstacleY = 512f;         // suelo (~560) menos media altura

	private Player _player;
	private GameManager _gameManager;
	private Godot.Timer _timer;
	private bool _spawning = true;
	private GameModeType _mode = GameModeType.TimeAttack;

	public void SetPlayerReference(Player player) { _player = player; }
	public void SetGameManager(GameManager gm) { _gameManager = gm; }

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

	// Llamado por GameManager tras crear el modo activo.
	public void ConfigureForMode(GameModeType mode)
	{
		_mode = mode;
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

		// Siempre puede haber un obstáculo.
		if (ObstacleScene != null)
		{
			Obstacle obs = (Obstacle)ObstacleScene.Instantiate();
			obs.Position = new Vector2(spawnX, ObstacleY);
			if (_gameManager != null) obs.HitPlayer += _gameManager.OnObstacleHitPlayer;
			AddChild(obs);
		}

		// Perros (modo Chase): aparecen detrás del jugador y lo persiguen.
		if (_mode == GameModeType.Chase && DogScene != null)
		{
			Dog dog = (Dog)DogScene.Instantiate();
			dog.Position = new Vector2(_player.GlobalPosition.X - 450f, ObstacleY);
			AddChild(dog);
		}

		// Bidones de gasolina (modo Fuel): aparecen por delante, en un carril.
		if (_mode == GameModeType.Fuel && PickupScene != null)
		{
			Pickup fuel = (Pickup)PickupScene.Instantiate();
			// Alterna carril para que no quede siempre alineado al jugador.
			float laneY = (GD.Randf() > 0.5f) ? 470f : 550f;
			fuel.Position = new Vector2(spawnX + 250f, laneY);
			AddChild(fuel);
		}
	}
}
