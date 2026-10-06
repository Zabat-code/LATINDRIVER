using Godot;

// Dog.cs
// Perro perseguidor del modo "Perros" (ChaseMode).
// Area2D con collider para detectar obstáculos.
// Estados: Chasing -> Stunned (al chocar) -> Chasing/Retreat
// Recomendación gpt-6.1-sol: retraso 0.25-0.40s al cambio de carril, 0.6-1s stunned.

public partial class Dog : Area2D
{
    [Export] public float Speed = 180f;
    [Export] public float CatchDistance = 40f;
    [Export] public float StunDuration = 0.8f;
    [Export] public float LaneChangeDelay = 0.3f;

    private Player _target;
    private float _stunTimer = 0f;
    private float _laneChangeTimer = 0f;
    private int _lastPlayerLane = 1;
    private bool _isActive = true;

    public enum DogState { Chasing, Stunned, Retreat }
    private DogState _state = DogState.Chasing;

    public override void _Ready()
    {
        AddToGroup("dog");
        AreaEntered += OnAreaEntered;
    }

    public void SetTarget(Player player)
    {
        _target = player;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !_isActive) return;

        float dt = (float)delta;

        // Detectar cambio de carril del jugador
        if (_target.CurrentLane != _lastPlayerLane)
        {
            _lastPlayerLane = _target.CurrentLane;
            _laneChangeTimer = LaneChangeDelay;
        }

        if (_laneChangeTimer > 0f)
        {
            _laneChangeTimer -= dt;
            return; // No perseguir durante el retraso
        }

        switch (_state)
        {
            case DogState.Chasing:
                ChasePlayer(dt);
                break;
            case DogState.Stunned:
                _stunTimer -= dt;
                if (_stunTimer <= 0f)
                {
                    _state = DogState.Chasing;
                }
                break;
            case DogState.Retreat:
                Position = new Vector2(Position.X - 100f * dt, Position.Y);
                if (Position.X < -100f)
                {
                    QueueFree();
                }
                break;
        }
    }

    private void ChasePlayer(float dt)
    {
        if (_target == null) return;

        // Perseguir la posición de suelo del jugador (sin salto)
        Vector2 targetPos = _target.GlobalPosition;
        targetPos.Y = _target.GetFloorPosition();

        Vector2 toTarget = targetPos - GlobalPosition;
        float dist = toTarget.Length();

        if (dist > 1f)
        {
            GlobalPosition += toTarget.Normalized() * Speed * dt;
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("obstacle") && _state == DogState.Chasing)
        {
            _state = DogState.Stunned;
            _stunTimer = StunDuration;
        }
    }

    public bool HasCaught(float catchDistance)
    {
        if (_target == null || _state != DogState.Chasing) return false;
        return GlobalPosition.DistanceTo(_target.GlobalPosition) <= catchDistance;
    }

    public void Deactivate()
    {
        _isActive = false;
        SetPhysicsProcess(false);
    }

    public DogState CurrentState => _state;
}
