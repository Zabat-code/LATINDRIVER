using Godot;

// ThrownObject.cs
// Proyectil que el jugador lanza hacia atras (izquierda) para derribar perros.
// Es un Area2D en grupo "thrown"; al solaparse con un perro (Dog) lo elimina.
public partial class ThrownObject : Area2D
{
	[Export] public float Speed = 600f;   // px/s hacia la izquierda
	[Export] public float DespawnX = -300f;

	private float _playerX;

	public void SetOrigin(float playerX) { _playerX = playerX; }

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Position = new Vector2(Position.X - Speed * dt, Position.Y);

		// ¿Toca un perro? (grupo "dog")
		foreach (Node body in GetOverlappingBodies())
		{
			if (body.IsInGroup("dog"))
			{
				body.QueueFree();           // derriba el perro
				QueueFree();
				return;
			}
		}
		foreach (Node area in GetOverlappingAreas())
		{
			if (area.IsInGroup("dog"))
			{
				area.QueueFree();
				QueueFree();
				return;
			}
		}

		if (Position.X < _playerX + DespawnX) QueueFree();
	}
}
