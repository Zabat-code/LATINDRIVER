using Godot;

// Obstacle.cs
// Obstáculo que se desplaza hacia la izquierda (el mundo avanza). Cuando su
// Area2D se solapa con el jugador (grupo "player") emite HitPlayer, que
// GameManager usa para terminar la partida. Se auto-elimina al salir de pantalla.

public partial class Obstacle : Area2D
{
	[Signal] public delegate void HitPlayerEventHandler();

	// Velocidad horizontal impuesta por GameManager (px/s, negativa = hacia la izquierda).
	public float Speed { get; set; } = -200f;

	// X a partir de la cual el obstáculo se considera fuera de pantalla y se libera.
	public float DespawnX { get; set; } = -100f;

	public override void _PhysicsProcess(double delta)
	{
		Position = new Vector2(Position.X + Speed * (float)delta, Position.Y);
		if (Position.X < DespawnX)
		{
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("player"))
		{
			EmitSignal(SignalName.HitPlayer);
		}
	}
}
