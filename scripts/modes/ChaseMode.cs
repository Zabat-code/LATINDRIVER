using Godot;

// ChaseMode.cs
// Modo "Perros" (Fase 2): una jauría persigue al jugador desde atrás. Pierdes
// si la distancia entre el jugador y el perro líder llega a 0 (te atrapan).
// La velocidad de los perros sube con el tiempo para generar tensión. No hay
// meta por X (es supervivencia): el modo termina solo por derrota (o si el
// arquitecto añade un tiempo objetivo).
// Ver PROJECT_CONTEXT.md (modo 2: persecución) y IGameMode.cs.

public partial class ChaseMode : Node, IGameMode
{
	[Export] public float DogStartSpeed = 160f;
	[Export] public float DogSpeedRamp = 6f;        // px/s ganados por segundo
	[Export] public float CatchDistance = 40f;       // distancia de captura
	[Export] public float SpawnBehind = 400f;       // px detrás del jugador al aparecer

	private Node2D _playerRef;
	private float _dogSpeed;

	public float TimeRemaining => 0f; // modo sin tiempo

	public void SetPlayerReference(Node2D player)
	{
		_playerRef = player;
	}

	public void OnStart()
	{
		_dogSpeed = DogStartSpeed;
	}

	public void OnUpdate(float delta)
	{
		_dogSpeed += DogSpeedRamp * delta;
	}

	public bool CheckLoseCondition()
	{
		// La detección real de captura la hace el GameManager (que tiene la
		// referencia a los perros instanciados). Aquí devolvemos false; el
		// GameManager llama a OnPlayerCollectItem/condición de captura aparte.
		// Mantenemos la interfaz: la pérdida se decide en GameManager.ChaseTick.
		return false;
	}

	public bool CheckWinCondition()
	{
		return false; // modo de supervivencia: no hay victoria por posición
	}

	public void OnPlayerCollectItem(string itemType)
	{
		// No aplica ítems en este modo.
	}

	public void OnPlayerCollected(string itemType)
	{
		OnPlayerCollectItem(itemType);
	}

	// --- Helpers usados por GameManager ---
	public float CurrentDogSpeed => _dogSpeed;
	public float SpawnBehindDistance => SpawnBehind;
	public float CatchRadius => CatchDistance;
}
