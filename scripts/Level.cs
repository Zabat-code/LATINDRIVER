using Godot;

// Level.cs
// Script de la escena Level (raíz Node2D) del prototipo LATINDRIVER.
// Responsabilidades acotadas (no contiene lógica de modo de juego):
//  - Mantener el fondo (Background) cubriendo siempre el viewport del jugador.
//  - Limpiar obstáculos que quedaron atrás del jugador (QueueFree).
//  - Cámara: sigue al jugador en X, pero con Y FIJO (centrada en la carretera),
//    para que el cambio de carril (que mueve Y) no desplace la vista ni oculte
//    el sprite del jugador.
// La lógica de modo, spawn y colisiones vive en GameManager / LevelGenerator / Player.

public partial class Level : Node2D
{
	private Sprite2D _background;
	private Player _player;
	private Camera2D _camera;
	private const float ViewportHalfWidth = 640f; // 1280/2
	private const float CameraY = 400f;           // Y fija centrada en la carretera

	public override void _Ready()
	{
		CallDeferred("Setup");
	}

	private void Setup()
	{
		_background = NodeUtils.GetChildByName<Sprite2D>(this, "Background");
		_player = NodeUtils.GetChildByName<Player>(this, "Player");
		_camera = NodeUtils.GetChildByName<Camera2D>(this, "Camera2D");
		if (_camera != null) _camera.Position = new Vector2(0, CameraY);
	}

	public override void _Process(double delta)
	{
		if (_player == null) return;

		// Fondo: anclar su borde izquierdo al viewport de la cámara (sigue en X).
		if (_background != null)
		{
			_background.Position = new Vector2(
				_player.GlobalPosition.X - ViewportHalfWidth,
				_background.Position.Y);
		}

		// Cámara: sigue en X, Y fija (evita que el carril oculte el sprite).
		if (_camera != null)
		{
			_camera.Position = new Vector2(_player.GlobalPosition.X, CameraY);
		}

		// Limpia obstáculos atrás del jugador.
		float cleanupX = _player.GlobalPosition.X - 800f;
		foreach (Node child in GetChildren())
		{
			if (child is Obstacle obs && obs.GlobalPosition.X < cleanupX)
			{
				obs.QueueFree();
			}
		}
	}
}
