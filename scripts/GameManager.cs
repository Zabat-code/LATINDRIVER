using Godot;
using System;

// GameManager.cs
// Orquesta el modo de juego activo (TimeAttackMode por defecto), conecta la
// señal PlayerHit del jugador para disparar Game Over, y muestra un HUD básico
// con el tiempo restante. Es el pegamento que faltaba en el estado de Cline.

public partial class GameManager : Node
{
    [Export] public NodePath PlayerPath;
    [Export] public NodePath ModePath;

    private Player _player;
    private IGameMode _mode;
    private Label _hud;
    private bool _gameOver = false;

    public override void _Ready()
    {
        _player = GetNode<Player>(PlayerPath);
        _mode = GetNode<TimeAttackMode>(ModePath);

        // HUD: creamos un Label simple arriba a la izquierda.
        _hud = new Label();
        _hud.Position = new Vector2(12, 12);
        _hud.Scale = new Vector2(2, 2);
        AddChild(_hud);

        if (_player != null)
        {
            _player.PlayerHit += OnPlayerHit;
        }

        _mode?.SetPlayerReference(_player);
        _mode?.OnStart();
    }

    public override void _Process(double delta)
    {
        if (_gameOver || _mode == null) return;

        _mode.OnUpdate((float)delta);

        _hud.Text = $"Tiempo: {_mode.TimeRemaining:F1}s";

        if (_mode.CheckLoseCondition())
        {
            EndGame("GAME OVER");
        }
        else if (_mode.CheckWinCondition())
        {
            EndGame("VICTORIA");
        }
    }

    private void OnPlayerHit()
    {
        // En endless runner, chocar con un obstáculo = game over inmediato.
        EndGame("GAME OVER");
    }

    private void EndGame(string message)
    {
        if (_gameOver) return;
        _gameOver = true;
        _hud.Text = message;
        // Detenemos el avance del jugador.
        if (_player != null)
        {
            _player.Speed = 0f;
        }
        GD.Print(message);
    }
}
