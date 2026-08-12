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
	private float _worldSpeedMult = 1.0f;

	public void SetPlayerReference(Player player) { _player = player; }
	public void SetGameManager(GameManager gm) { _gameManager = gm; }

	// Llamado por GameManager tras crear el modo activo.
	public void ConfigureForMode(GameModeType mode)
	{
		_mode = mode;
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

	// Aplica mapa + turno: ajusta la frecuencia (SpawnInterval) y velocidad de
	// obstáculos según la densidad/velocidad del mapa y el turno.
	public void ConfigureMap(int mapIndex, TurnType turn)
	{
		float densidad = MapConfig.Maps[mapIndex].DensidadBase * MapConfig.TurnDensidad(turn);
		float velocidad = MapConfig.Maps[mapIndex].VelocidadBase * MapConfig.TurnVelocidad(turn);
		// densidad alta => spawn más frecuente (intervalo menor).
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

		// Siempre puede haber un obstáculo, en un carril aleatorio (alto/medio/bajo)
		// para que el cambio de carril sirva de esquive.
		if (ObstacleScene != null)
		{
			float[] lanes = new float[] { 440f, 510f, 580f };
			float laneY = lanes[(int)(GD.Randf() * lanes.Length)];
			Obstacle obs = (Obstacle)ObstacleScene.Instantiate();
			obs.Position = new Vector2(spawnX, laneY);
			obs.Speed = -200f * _worldSpeedMult;   // base ajustada por mapa/turno
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
