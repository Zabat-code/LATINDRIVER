using Godot;

// TimeAttackMode.cs
// Modo "Contra reloj": el jugador pierde si el temporizador llega a 0
// antes de alcanzar la posición X objetivo (fin del nivel).
// Ver PROJECT_CONTEXT.md y TASKS.md, sección 4.

public partial class TimeAttackMode : Node, IGameMode
{
	// Tiempo inicial en segundos (valor sugerido en TASKS.md: 60)
	[Export] public float StartingTime = 60f;

	// Posición X que el jugador debe alcanzar para ganar el nivel.
	// Debe configurarse según el largo real del nivel/mapa.
	[Export] public float GoalPositionX = 3000f;

	private float _timeRemaining;
	private Node2D _playerRef;

	public float TimeRemaining => _timeRemaining;

	// Referencia al jugador debe asignarse desde GameManager antes de OnStart(),
	// para poder leer su posición X en CheckWinCondition().
	public void SetPlayerReference(Node2D player)
	{
		_playerRef = player;
	}

	public void OnStart()
	{
		_timeRemaining = StartingTime;
	}

	public void OnUpdate(float delta)
	{
		_timeRemaining -= delta;
		if (_timeRemaining < 0)
		{
			_timeRemaining = 0;
		}
	}

	public bool CheckLoseCondition()
	{
		return _timeRemaining <= 0;
	}

	public bool CheckWinCondition()
	{
		if (_playerRef == null) return false;
		return _playerRef.GlobalPosition.X >= GoalPositionX;
	}

	public void OnPlayerCollectItem(string itemType)
	{
		// Los checkpoints suman tiempo (ver PROJECT_CONTEXT.md).
		// Ajustar el valor de bonus según diseño posterior.
		if (itemType == "checkpoint")
		{
			_timeRemaining += 10f;
		}
	}

	public void OnPlayerCollected(string itemType)
	{
		OnPlayerCollectItem(itemType);
	}
}
