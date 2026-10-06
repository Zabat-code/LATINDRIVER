using Godot;

// LibreMode.cs
// Modo "Libre" (modalidad 3): sin límite de tiempo ni gasolina.
// Solo esquivar obstáculos con dificultad progresiva.
// Pierdes si chocas con un obstáculo o un perro.

public partial class LibreMode : Node, IGameMode
{
    [Export] public float StartingDifficulty = 1.0f;
    [Export] public float DifficultyRamp = 0.05f; // incremento por spawn
    [Export] public float MinSpawnInterval = 0.4f;

    private float _difficulty;
    private Node2D _playerRef;

    public float TimeRemaining => 0f; // sin límite de tiempo

    public void SetPlayerReference(Node2D player)
    {
        _playerRef = player;
    }

    public void OnStart()
    {
        _difficulty = StartingDifficulty;
    }

    public void OnUpdate(float delta)
    {
        // La dificultad aumenta con el tiempo
        _difficulty += DifficultyRamp * delta;
    }

    public bool CheckLoseCondition()
    {
        // En modo Libre, la pérdida se detecta por colisión (PlayerHit)
        // GameManager maneja esto. Aquí devolvemos false.
        return false;
    }

    public bool CheckWinCondition()
    {
        return false; // modo endless: no hay victoria
    }

    public void OnPlayerCollectItem(string itemType)
    {
        // No hay ítems en modo Libre
    }

    public void OnPlayerCollected(string itemType)
    {
        OnPlayerCollectItem(itemType);
    }

    // Helpers usados por GameManager/LevelGenerator
    public float CurrentDifficulty => _difficulty;
}
