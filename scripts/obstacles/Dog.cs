using Godot;

// Dog.cs
// Perro perseguidor del modo "Perros" (ChaseMode). Es una entidad visual muda:
// se mueve hacia el jugador cada frame (su velocidad la controla el modo para
// generar tensión). No está en el grupo "obstacle", así que no causa Game Over
// por colisión; es el ChaseMode quien detecta la captura y pierde la partida.
// Placeholder (rectángulo de color); en Fase 2 se reemplaza por pixel art.

public partial class Dog : Node2D
{
	[Export] public float Speed = 180f;

	private Player _target;

	public void SetTarget(Player player)
	{
		_target = player;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null) return;
		Vector2 toTarget = _target.GlobalPosition - GlobalPosition;
		float dist = toTarget.Length();
		if (dist > 1f)
		{
			GlobalPosition += toTarget.Normalized() * Speed * (float)delta;
		}
	}

	// ¿Atrapó al jugador? (distancia menor al umbral de captura)
	public bool HasCaught(float catchDistance)
	{
		if (_target == null) return false;
		return GlobalPosition.DistanceTo(_target.GlobalPosition) <= catchDistance;
	}
}
