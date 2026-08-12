using Godot;

// Obstacle.cs
// Obstaculo que se desplaza hacia la izquierda (el mundo avanza). Es un
// StaticBody2D en grupo "obstacle"; el Player (CharacterBody2D) lo detecta por
// colision deslizante (GetSlideCollision) y emite PlayerHit, que GameManager
// usa para terminar la partida. Se auto-elimina al salir de pantalla.
public partial class Obstacle : StaticBody2D
{
	// Velocidad horizontal impuesta por GameManager (px/s, negativa = hacia la izquierda).
	public float Speed { get; set; } = -200f;

	// X a partir de la cual el obstaculo se considera fuera de pantalla y se libera.
	public float DespawnX { get; set; } = -100f;

	public override void _PhysicsProcess(double delta)
	{
		Position = new Vector2(Position.X + Speed * (float)delta, Position.Y);
		if (Position.X < DespawnX)
		{
			QueueFree();
		}
	}
}
