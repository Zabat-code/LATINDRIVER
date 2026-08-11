using Godot;

// IGameMode.cs
// Interfaz que deben implementar todos los modos de juego (TimeAttackMode,
// ChaseMode, FuelMode). El GameManager delega en la implementación activa
// para decidir victoria/derrota, sin conocer los detalles de cada modo.
// Ver PROJECT_CONTEXT.md, sección "Arquitectura de modos de juego".

public interface IGameMode
{
	// Se llama una vez al iniciar el nivel con este modo activo.
	void OnStart();

	// Se llama cada frame de física (o el intervalo que decida GameManager).
	void OnUpdate(float delta);

	// Debe retornar true cuando se cumple la condición de derrota del modo.
	bool CheckLoseCondition();

	// Debe retornar true cuando se cumple la condición de victoria del modo
	// (ej. el jugador llegó al final del nivel).
	bool CheckWinCondition();

	// Se llama cuando el jugador recoge un ítem relevante para el modo
	// (ej. un bidón de gasolina en FuelMode). itemType identifica el tipo.
	void OnPlayerCollectItem(string itemType);
}
