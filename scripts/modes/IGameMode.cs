using Godot;

// IGameMode.cs
// Interfaz que deben implementar todos los modos de juego (TimeAttackMode,
// ChaseMode, FuelMode). El GameManager delega en la implementación activa
// para decidir victoria/derrota, sin conocer los detalles de cada modo.
// Ver PROJECT_CONTEXT.md, sección "Arquitectura de modos de juego".

public interface IGameMode
{
	// Referencia al jugador, asignada por GameManager antes de OnStart(),
	// para que los modos puedan leer su posición (CheckWinCondition).
	void SetPlayerReference(Node2D player);

	// Se llama una vez al iniciar el nivel con este modo activo.
	void OnStart();

	// Se llama cada frame de física (o el intervalo que decida GameManager).
	void OnUpdate(float delta);

	// Debe retornar true cuando se cumple la condición de derrota del modo.
	bool CheckLoseCondition();

	// Debe retornar true cuando se cumple la condición de victoria del modo
	// (ej. el jugador llegó al final del nivel).
	bool CheckWinCondition();

	// Tiempo restante (para el HUD). Los modos sin tiempo pueden retornar 0.
	float TimeRemaining { get; }

	// Se llama cuando el jugador recoge un ítem relevante para el modo
	// (ej. un bidón de gasolina en FuelMode). itemType identifica el tipo.
	void OnPlayerCollectItem(string itemType);

	// Alias usado por GameManager al reenviar la señal PlayerCollected del jugador.
	void OnPlayerCollected(string itemType);
}
