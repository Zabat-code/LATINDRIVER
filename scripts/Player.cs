using Godot;

// Player.cs
// Controla el movimiento automático hacia adelante de la moto, el salto,
// el agachado/deslizamiento, y detecta colisiones con obstáculos.
// No contiene lógica de ningún modo de juego específico (ver PROJECT_CONTEXT.md,
// sección "Arquitectura de modos de juego").

public partial class Player : CharacterBody2D
{
	// Velocidad de avance automático horizontal (px/s)
	[Export] public float Speed = 200f;

	// Fuerza del impulso de salto (valor negativo = hacia arriba en Godot)
	[Export] public float JumpForce = -400f;

	// Gravedad aplicada mientras el jugador está en el aire
	[Export] public float Gravity = 900f;

	// Señal emitida cuando el jugador choca con un obstáculo.
	// El GameManager / modo de juego activo debe escuchar esta señal
	// y decidir qué hacer (perder vida, game over, etc.)
	[Signal] public delegate void PlayerHitEventHandler();

	private CollisionShape2D _collisionShape;
	private Vector2 _originalCollisionSize;
	private bool _isCrouching = false;

	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		// Guardamos el tamaño original de la colisión para poder restaurarlo
		// al salir del agachado. Asume una RectangleShape2D; ajustar si se usa otra forma.
		if (_collisionShape.Shape is RectangleShape2D rectShape)
		{
			_originalCollisionSize = rectShape.Size;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// Avance automático constante
		velocity.X = Speed;

		// Gravedad
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * dt;
		}
		else
		{
			velocity.Y = 0;
		}

		// Input de salto (solo si está en el suelo)
		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
		{
			velocity.Y = JumpForce;
		}

		// Input de agachar/deslizar
		bool wantsCrouch = Input.IsActionPressed("ui_down");
		if (wantsCrouch != _isCrouching)
		{
			SetCrouching(wantsCrouch);
		}

		Velocity = velocity;
		MoveAndSlide();

		// Revisar colisiones con obstáculos
		CheckObstacleCollisions();
	}

	private void SetCrouching(bool crouching)
	{
		_isCrouching = crouching;

		if (_collisionShape.Shape is RectangleShape2D rectShape)
		{
			if (crouching)
			{
				// Reduce la altura de la colisión a la mitad para pasar bajo obstáculos altos
				rectShape.Size = new Vector2(_originalCollisionSize.X, _originalCollisionSize.Y / 2f);
			}
			else
			{
				rectShape.Size = _originalCollisionSize;
			}
		}
	}

	private void CheckObstacleCollisions()
	{
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);
			if (collision.GetCollider() is Node2D collider && collider.IsInGroup("obstacle"))
			{
				EmitSignal(SignalName.PlayerHit);
			}
		}
	}
}
