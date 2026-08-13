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

	// Velocidad de avance del mundo (px/s). El jugador avanza en X para que la
	// camara (que lo sigue) lo mantenga centrado en pantalla, pero su X global
	// crece: asi el fondo se desplaza, la distancia aumenta y los modos pueden
	// usar GlobalPosition.X como posicion de carrera.
	[Export] public float RunSpeed = 200f;

	// Multiplicador de aceleracion (1.0 normal, hasta 1.5 impuesto por
	// GameManager). La velocidad de scroll y de obstaculos escala con esto,
	// asi la camara (que sigue al jugador) nunca se adelanta.
	public float AccelMultiplier { get; set; } = 1f;

	[Signal] public delegate void PlayerHitEventHandler();
	[Signal] public delegate void PlayerCollectedEventHandler(string itemType);

	private CollisionShape2D _collisionShape;
	private Vector2 _originalCollisionSize;
	private bool _isCrouching = false;
	private int _lane = 1;                 // 0,1,2 -> LaneY1/2/3
	private float _targetY;
	private float _worldX = 0f;            // X de carrera (crece con RunSpeed)
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
		_worldX = Position.X;              // arranca donde lo puso la escena
		Position = new Vector2(_worldX, _targetY);

		// Usar el sprite de moto generado (moto1.png) en vez del rider placeholder.
		var spr = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (spr != null)
		{
			var tex = GD.Load<Texture2D>("res://assets/sprites/moto1.png");
			if (tex != null) { spr.Texture = tex; spr.Scale = new Vector2(1f, 1f); }
		}

		// Hitbox de colisión: un Area2D hijo del Player detecta por entrada de
		// cuerpos (obstacle / pickup) en vez de MoveAndSlide, porque el jugador
		// se posiciona manualmente (velocidad 0) y MoveAndSlide no reportaría
		// colisiones. El Area2D con mask incluyendo la capa 2 (obstáculos y
		// pickups) dispara body_entered de forma edge-triggered (una sola vez).
				var hitbox = GetNodeOrNull<Area2D>("Hitbox");
		if (hitbox != null)
		{
			hitbox.BodyEntered += OnBodyEntered;
		}
		GD.Print("[PLY] _Ready hitbox=", hitbox != null, " childCount=", GetChildCount());
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

						// El avance horizontal lo aporta _worldX (abajo) y la colisión se detecta
		// vía el Area2D Hitbox (ver OnBodyEntered en _Ready). No se usa MoveAndSlide
		// porque el jugador no se desplaza por física (se posiciona manualmente) y
		// MoveAndSlide con velocidad 0 no reportaría colisiones.

		// Avance de la carrera: la X global crece (la camara lo mantiene centrado
		// en pantalla). Esto mueve el fondo, sube la distancia y alimenta los
		// modos (meta por X) y el spawn de obstaculos/perros. Escala con la
		// aceleracion (cap 1.5x) impuesta por GameManager.
		_worldX += RunSpeed * AccelMultiplier * dt;

				// Posicion final: carril (lerp suave en Y) + altura de salto, X de carrera.
		float newY = Mathf.Lerp(Position.Y, _targetY, LaneLerp * dt) - _jumpHeight;
		Position = new Vector2(_worldX, newY);

		// DEBUG: contar cuerpos solapados en el Hitbox cada ~1s.
		if (Engine.GetFramesDrawn() % 60 == 0)
		{
			var hb = GetNodeOrNull<Area2D>("Hitbox");
			int overlap = 0; Vector2? nearest = null;
						if (hb != null)
			{
				foreach (Node body in hb.GetOverlappingBodies())
				{
					if (body is Node2D b2)
					{
						overlap++;
						nearest = b2.GlobalPosition;
					}
				}
			}
			GD.Print("[PLY_DBG] pos=", Position, " worldX=", _worldX, " hitbox_overlap=", overlap, " nearest=", nearest);
		}

				// La colisión con obstáculos y pickups se detecta vía el Area2D Hitbox
		// (método OnBodyEntered), no vía colisiones de slide.
	}

	// Colisión de cuerpos contra la Hitbox (obstáculos -> game over; pickups ->
	// recolección). Edge-triggered: se dispara al entrar, no cada frame, por lo
	// que PlayerHit / PlayerCollected se emiten una sola vez por contacto.
		private void OnBodyEntered(Node body)
	{
		if (body.IsInGroup("obstacle"))
		{
			GD.Print("[HIT] obstacle -> PlayerHit");
			EmitSignal(SignalName.PlayerHit);
		}
		else if (body.IsInGroup("pickup"))
		{
			string itemType = "fuel";
			if (body is Pickup p) itemType = p.GetItemType();
			GD.Print("[HIT] pickup -> PlayerCollected ", itemType);
			EmitSignal(SignalName.PlayerCollected, itemType);
			body.QueueFree();   // consumir el bidón
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
