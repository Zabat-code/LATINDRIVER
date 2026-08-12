using Godot;

// Player.cs
// Corredor de LATINDRIVER. La moto avanza automaticamente (el mundo se desplaza
// hacia la izquierda, controlado por GameManager via WorldSpeed). Aqui solo
// manejamos: salto, agachado/desliz y cambio de carril lateral.
//
// Diseno de fisica (side-view runner):
//  - El personaje NO usa la gravedad global del mundo. En su lugar mantiene una
//    Y fija por carril y el salto es una "altura" propia (offset) que sube y
//    baja con su propia gravedad de salto. Asi el carril queda estable y visible
//    y el salto no pelea con el suelo.
//  - MoveAndSlide se usa solo para detectar colisiones deslizantes (obstaculos /
//    pickups via GetSlideCollision) sin gravedad global.
//  - La colision con obstaculos (grupo "obstacle") la detecta este script y emite
//    PlayerHit, que escucha GameManager para terminar la partida.

public partial class Player : CharacterBody2D
{
	// Salto (altura propia, px). Impulso hacia arriba, luego gravedad de salto.
	[Export] public float JumpImpulse = 260f;
	[Export] public float JumpGravity = 900f;

	// Agachado / desliz.
	[Export] public float CrouchScaleY = 0.5f;

	// Carriles laterales: 3 posiciones Y fijas sobre la carretera, bien separadas
	// para que el cambio de carril sea claramente visible.
	[Export] public float LaneY1 = 440f;   // carril alto
	[Export] public float LaneY2 = 510f;   // carril medio (default)
	[Export] public float LaneY3 = 580f;   // carril bajo
	[Export] public float LaneLerp = 14f;  // velocidad de interpolacion del carril

	[Signal] public delegate void PlayerHitEventHandler();
	[Signal] public delegate void PlayerCollectedEventHandler(string itemType);

	private CollisionShape2D _collisionShape;
	private Vector2 _originalCollisionSize;
	private bool _isCrouching = false;
	private int _lane = 1;                 // 0,1,2 -> LaneY1/2/3
	private float _targetY;
	private float _jumpHeight = 0f;       // altura de salto actual (>=0)
	private float _jumpVelocity = 0f;     // velocidad vertical del salto

	public override void _Ready()
	{
		AddToGroup("player");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape.Shape is RectangleShape2D rectShape)
		{
			_originalCollisionSize = rectShape.Size;
		}
		_lane = 1;
		_targetY = LaneY2;                 // carril central por defecto
		Position = new Vector2(Position.X, _targetY);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// --- Cambio de carril (flechas IZQ/DER) -> ajusta el Y objetivo. ---
		if (Input.IsActionJustPressed("ui_left") && _lane > 0) _lane--;
		if (Input.IsActionJustPressed("ui_right") && _lane < 2) _lane++;
		_targetY = _lane == 0 ? LaneY1 : (_lane == 1 ? LaneY2 : LaneY3);

		// --- Salto (solo si no esta en el aire). ---
		bool onGround = _jumpHeight <= 0.01f;
		if (Input.IsActionJustPressed("ui_up") && onGround)
		{
			_jumpVelocity = JumpImpulse;
		}
		if (_jumpVelocity > 0f || _jumpHeight > 0f)
		{
			_jumpVelocity -= JumpGravity * dt;
			_jumpHeight += _jumpVelocity * dt;
			if (_jumpHeight <= 0f)
			{
				_jumpHeight = 0f;
				_jumpVelocity = 0f;
			}
		}

		// --- Agachar / deslizar (tecla mantenida). ---
		bool wantsCrouch = Input.IsActionPressed("ui_down") && onGround;
		if (wantsCrouch != _isCrouching)
		{
			SetCrouching(wantsCrouch);
		}

		// Sin gravedad global ni avance horizontal: el mundo se mueve hacia el
		// jugador. MoveAndSlide solo para detectar colisiones deslizantes.
		velocity.X = 0f;
		velocity.Y = 0f;
		Velocity = velocity;
		MoveAndSlide();

		// Posicion final: carril (lerp suave en Y) + altura de salto.
		float newY = Mathf.Lerp(Position.Y, _targetY, LaneLerp * dt) - _jumpHeight;
		Position = new Vector2(Position.X, newY);

		// Detectar colisiones deslizantes: obstaculos (grupo "obstacle") y pickups
		// (grupo "pickup").
		int count = GetSlideCollisionCount();
		for (int i = 0; i < count; i++)
		{
			KinematicCollision2D col = GetSlideCollision(i);
			if (col == null) continue;
			if (col.GetCollider() is Node body)
			{
				if (body.IsInGroup("obstacle"))
				{
					EmitSignal(SignalName.PlayerHit);
				}
				else if (body.IsInGroup("pickup"))
				{
					string itemType = "fuel";
					if (body is Pickup p) itemType = p.GetItemType();
					EmitSignal(SignalName.PlayerCollected, itemType);
					body.QueueFree();   // consumir el bidon
				}
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
