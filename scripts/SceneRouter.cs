using Godot;

// SceneRouter.cs
// Autoload para navegación limpia entre escenas.
// Recomendación gpt-6.1-sol: separar navegación de GameManager.

public partial class SceneRouter : Node
{
    public static SceneRouter Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void GoTo(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    public void GoToMenu()
    {
        GoTo("res://scenes/MainMenu.tscn");
    }

    public void GoToLevel()
    {
        GoTo("res://scenes/Level.tscn");
    }

    public void GoToGameOver(RunResult result)
    {
        RunSession.Instance.LastResult = result;
        GoTo("res://scenes/GameOver.tscn");
    }
}

// RunResult.cs
// Datos de una partida terminada
public class RunResult
{
    public GameModeType Mode { get; set; }
    public int MapIndex { get; set; }
    public float Distance { get; set; }
    public float Time { get; set; }
    public string LoseReason { get; set; } = "";
}

// RunSession.cs
// Autoload para datos persistentes entre escenas
public partial class RunSession : Node
{
    public static RunSession Instance { get; private set; }

    public RunResult LastResult { get; set; }
    public GameModeType SelectedMode { get; set; } = GameModeType.TimeAttack;
    public int SelectedMap { get; set; } = 1;
    public TurnType SelectedTurn { get; set; } = TurnType.Tarde;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }
}
