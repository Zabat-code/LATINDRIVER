using Godot;

// FuelMode.cs
// Modo "Gasolina" (Fase 2): un tanque que baja constantemente con la distancia
// recorrida. Hay bidones de gasolina en el camino que lo rellenan. Si llega a 0,
// la moto se detiene y pierdes. Por ahora la velocidad del jugador es fija, así
// que el consumo es proporcional al tiempo (delta) para mantener jugabilidad.
// Ver PROJECT_CONTEXT.md (modo 3: gasolina) y IGameMode.cs.

public partial class FuelMode : Node, IGameMode
{
	[Export] public float StartingFuel = 100f;
	[Export] public float FuelDrainPerSec = 4f;     // consumo base por segundo
	[Export] public float FuelPerPickup = 35f;      // recarga por bidón
	[Export] public float GoalPositionX = 6000f;    // meta por distancia

	private float _fuel;
	private Node2D _playerRef;

	public float TimeRemaining => _fuel; // el HUD muestra el tanque como "tiempo"

	public void SetPlayerReference(Node2D player)
	{
		_playerRef = player;
	}

	public void OnStart()
	{
		_fuel = StartingFuel;
	}

	public void OnUpdate(float delta)
	{
		_fuel -= FuelDrainPerSec * delta;
		if (_fuel < 0f) _fuel = 0f;
	}

	public bool CheckLoseCondition()
	{
		return _fuel <= 0f;
	}

	public bool CheckWinCondition()
	{
		if (_playerRef == null) return false;
		return _playerRef.GlobalPosition.X >= GoalPositionX;
	}

	public void OnPlayerCollectItem(string itemType)
	{
		if (itemType == "fuel")
		{
			_fuel = Mathf.Min(StartingFuel, _fuel + FuelPerPickup);
		}
	}

	public void OnPlayerCollected(string itemType)
	{
		OnPlayerCollectItem(itemType);
	}
}
