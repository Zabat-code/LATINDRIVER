using Godot;
using System;

// GameManager.cs
// Bucle de endless runner: spawnea obstáculos desde la derecha que se desplazan
// a la izquierda (WorldSpeed), detecta colisión vía la señal HitPlayer del
// obstáculo, y permite reiniciar con R. Es el pegamento que faltaba en Cline.

public partial class GameManager : Node
{
	[Export] public NodePath PlayerPath;
	[Export] public PackedScene ObstacleScene;

	// Velocidad del mundo (px/s). Sube ligeramente con el tiempo para dificultad.
	[Export] public float WorldSpeed = 200f;
	[Export] public float SpeedRamp = 4f;          // px/s ganados por segundo
	[Export] public float SpawnInterval = 1.4f;     // segundos entre obstáculos
	[Export] public float SpawnX = 1320f;          // borde derecho (ventana 1280)
	[Export] public float ObstacleY = 512f;        // suelo (~560) menos media altura

	private Player _player;
	private Label _hud;
	private Godot.Timer _spawnTimer;
	private bool _gameOver = false;
	private float _elapsed = 0f;

	public override void _Ready()
	{
		_player = GetNode<Player>(PlayerPath);

		_hud = new Label();
		_hud.Position = new Vector2(12, 12);
		_hud.Scale = new Vector2(2, 2);
		AddChild(_hud);

		_spawnTimer = new Godot.Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Timeout += OnSpawnTimeout;
		AddChild(_spawnTimer);
		_spawnTimer.Start();

		if (_player != null)
		{
			// No necesitamos PlayerHit (ese era el diseño viejo); el obstáculo
			// emite su propia señal. Lo dejamos por compatibilidad si existe.
		}
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		if (_gameOver)
		{
			_hud.Text = "GAME OVER\nPulsa R para reiniciar";
			if (Input.IsActionJustPressed("ui_accept") || Input.IsKeyPressed(Key.R))
			{
				GetTree().ReloadCurrentScene();
			}
			return;
		}

		_elapsed += dt;
		WorldSpeed += SpeedRamp * dt;

		// Mover obstáculos existentes hacia la izquierda.
		foreach (Node child in GetChildren())
		{
			if (child is Obstacle obs)
			{
				obs.Speed = -WorldSpeed;
				obs.DespawnX = -100f;
			}
		}

		_hud.Text = $"Tiempo: {_elapsed:F1}s   Vel: {WorldSpeed:F0}";
	}

	private void OnSpawnTimeout()
	{
		if (_gameOver || ObstacleScene == null) return;

		Obstacle obs = (Obstacle)ObstacleScene.Instantiate();
		obs.Position = new Vector2(SpawnX, ObstacleY);
		obs.Speed = -WorldSpeed;
		obs.DespawnX = -100f;
		obs.HitPlayer += OnObstacleHitPlayer;
		AddChild(obs);
	}

	private void OnObstacleHitPlayer()
	{
		_gameOver = true;
		_spawnTimer.Stop();
		if (_player != null) _player.Visible = false;
		GD.Print("GAME OVER");
	}
}
