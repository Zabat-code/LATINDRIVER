using Godot;

// Player.cs
// Personaje principal: motociclista en scroll infinito.
// 4 carriles (0=arriba, 3=abajo), salto, agacharse, colisiones.
// Métrica base: sprite 64x64 px. Carriles separados 70px.
// Velocidad base: 200 px/s (el mundo se mueve hacia la izquierda).

public partial class Player : CharacterBody2D
{
    // Salto (altura propia, px). Impulso hacia arriba, luego gravedad de salto.
    [Export] public float JumpImpulse = 260f;
    [Export] public float JumpGravity = 900f;

    // Agachado / desliz.
    [Export] public float CrouchScaleY = 0.5f;

    // 4 carriles: Y positions (separados 70px)
    // Carril 0: 440 (alto), 1: 510 (medio-alto, default), 2: 580 (medio-bajo), 3: 650 (abajo)
    [Export] public float LaneY1 = 440f;
    [Export] public float LaneY2 = 510f;
    [Export] public float LaneY3 = 580f;
    [Export] public float LaneY4 = 650f;
    [Export] public float LaneLerp = 14f;

    // Velocidad de avance del mundo (px/s).
    [Export] public float RunSpeed = 200f;
    public float AccelMultiplier { get; set; } = 1f;

    [Signal] public delegate void PlayerHitEventHandler();
    [Signal] public delegate void PlayerCollectedEventHandler(string itemType);

    private CollisionShape2D _collisionShape;
    private Vector2 _originalCollisionSize;
    private bool _isCrouching = false;
    private int _lane = 1;                 // 0,1,2,3 -> LaneY1/2/3/4
    private float _targetY;
    private float _worldX = 0f;
    private float _jumpHeight = 0f;
    private float _jumpVelocity = 0f;

    // Métricas públicas
    public float DistanceTraveled => _worldX;
    public float ElapsedTime { get; private set; } = 0f;
    public int CurrentLane => _lane;
    public bool IsCrouching => _isCrouching;

    public override void _Ready()
    {
        AddToGroup("player");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        if (_collisionShape.Shape is RectangleShape2D rectShape)
        {
            _originalCollisionSize = rectShape.Size;
        }
        _lane = 1;
        _targetY = LaneY2;
        _worldX = Position.X;
        Position = new Vector2(_worldX, _targetY);

        // Sprite de moto
        var spr = GetNodeOrNull<Sprite2D>("Sprite2D");
        if (spr != null)
        {
            var tex = GD.Load<Texture2D>("res://assets/sprites/moto1.png");
            if (tex != null) { spr.Texture = tex; spr.Scale = new Vector2(1f, 1f); }
        }

        // Hitbox de colisión
        var hitbox = GetNodeOrNull<Area2D>("Hitbox");
        if (hitbox != null)
        {
            hitbox.BodyEntered += OnBodyEntered;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        ElapsedTime += dt;

        // --- Cambio de carril (4 carriles: 0-3) ---
        if (Input.IsActionJustPressed("ui_left") && _lane > 0) _lane--;
        if (Input.IsActionJustPressed("ui_right") && _lane < 3) _lane++;
        _targetY = _lane switch
        {
            0 => LaneY1,
            1 => LaneY2,
            2 => LaneY3,
            3 => LaneY4,
            _ => LaneY2
        };

        // --- Salto ---
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

        // --- Agachar / deslizar ---
        bool wantsCrouch = Input.IsActionPressed("ui_down") && onGround;
        if (wantsCrouch != _isCrouching)
        {
            SetCrouching(wantsCrouch);
        }

        // Avance de la carrera
        _worldX += RunSpeed * AccelMultiplier * dt;

        // Posición final
        float newY = Mathf.Lerp(Position.Y, _targetY, LaneLerp * dt) - _jumpHeight;
        Position = new Vector2(_worldX, newY);
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("obstacle"))
        {
            EmitSignal(SignalName.PlayerHit);
        }
        else if (body.IsInGroup("dog"))
        {
            EmitSignal(SignalName.PlayerHit);
        }
        else if (body.IsInGroup("pickup"))
        {
            string itemType = "fuel";
            if (body is Pickup p) itemType = p.GetItemType();
            EmitSignal(SignalName.PlayerCollected, itemType);
            body.QueueFree();
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
