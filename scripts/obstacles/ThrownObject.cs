using Godot;

// ThrownObject.cs
// Proyectil que el jugador lanza hacia atras o hacia adelante.
// Es un Area2D en grupo "thrown"; al solaparse con un perro (Dog) lo elimina.
public partial class ThrownObject : Area2D
{
    [Export] public float Speed = 600f;
    [Export] public float DespawnX = -300f;

    private float _playerX;
    private float _direction = -1f;

    public void SetOrigin(float playerX) { _playerX = playerX; }
    public void SetDirection(float direction) { _direction = Mathf.Sign(direction); }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Position = new Vector2(Position.X + _direction * Speed * dt, Position.Y);

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

        float distanceFromOrigin = Position.X - _playerX;
        if ((_direction < 0f && distanceFromOrigin < DespawnX)
            || (_direction > 0f && distanceFromOrigin > 1200f))
        {
            QueueFree();
        }
	}
}
