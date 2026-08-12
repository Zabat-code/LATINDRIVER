using Godot;

// Player.cs
// Corredor: la moto avanza "hacia la derecha" pero en pantalla se queda en su X
// y el mundo (obstáculos) se desplaza hacia la izquierda. Aquí solo manejamos
// salto, agachado/deslizamiento y la gravedad. La colisión con obstáculos la
// detecta el Obstacle (Area2D) y emite HitPlayer, que escucha GameManager.

public partial class Player : CharacterBody2D
{
	// Gravedad y salto (px/s). El "avance" lo define GameManager vía WorldSpeed.
	[Export] public float JumpForce = -400f;
	[Export] public float Gravity = 900f;
	[Export] public float CrouchScaleY = 0.5f;

	[Signal] public delegate void PlayerHitEventHandler();
	[Signal] public delegate void PlayerCollectedEventHandler(string itemType);

	private CollisionShape2D _collisionShape;
	private Vector2 _originalCollisionSize;
	private bool _isCrouching = false;

	public override void _Ready()
	{
		AddToGroup("player");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape.Shape is RectangleShape2D rectShape)
		{
			_originalCollisionSize = rectShape.Size;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// Gravedad / suelo
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * dt;
		}
		else
		{
			velocity.Y = 0;
		}

		// Salto (solo en suelo)
		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
		{
			velocity.Y = JumpForce;
		}

		// Agachar / deslizar
		bool wantsCrouch = Input.IsActionPressed("ui_down");
		if (wantsCrouch != _isCrouching)
		{
			SetCrouching(wantsCrouch);
		}

		// Sin avance horizontal: el mundo se mueve hacia el jugador.
		velocity.X = 0;
		Velocity = velocity;
		MoveAndSlide();

		// Detectar ítems "pickup" (bidones de gasolina) tras el movimiento físico.
		int count = GetSlideCollisionCount();
		for (int i = 0; i < count; i++)
		{
			KinematicCollision2D col = GetSlideCollision(i);
			if (col != null && col.GetCollider() is Node body && body.IsInGroup("pickup"))
			{
				string itemType = "fuel";
				if (body is Pickup p) itemType = p.GetItemType();
				EmitSignal(SignalName.PlayerCollected, itemType);
				body.QueueFree();   // consumir el bidón
			}
		}
	}

	private void SetCrouching(bool crouching)
	{
		_isCrouching = crouching;
		if (_collisionShape.Shape is RectangleShape2D rectShape)
		{
			rectShape.Size = crouching
				? new Vector2(_originalCollisionSize.X, _originalCollisionSize.Y * CrouchScaleY)
				: _originalCollisionSize;
		}
		Scale = crouching ? new Vector2(1, CrouchScaleY) : new Vector2(1, 1);
	}
}
