using Godot;
using System.Collections.Generic;

// Pool.cs
// Pool de objetos para optimizar spawn de obstáculos, perros y pickups.
// Recomendación gpt-6.1-sol: capacidad = frecuencia máxima × tiempo en pantalla + 25%

public partial class Pool : Node
{
    [Export] public PackedScene Scene;
    [Export] public int InitialCapacity = 20;

    private Queue<Node2D> _available = new Queue<Node2D>();
    private List<Node2D> _active = new List<Node2D>();

    public override void _Ready()
    {
        // Pre-instanciar objetos
        for (int i = 0; i < InitialCapacity; i++)
        {
            Node2D obj = (Node2D)Scene.Instantiate();
            obj.ProcessMode = ProcessModeEnum.Disabled;
            AddChild(obj);
            _available.Enqueue(obj);
        }
    }

    public Node2D Acquire(Vector2 position, float speed = 0f)
    {
        Node2D obj;
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else
        {
            // Si no hay disponibles, crear uno nuevo
            obj = (Node2D)Scene.Instantiate();
            AddChild(obj);
        }

        obj.Position = position;
        obj.ProcessMode = ProcessModeEnum.Inherit;
        obj.Visible = true;

        // Configurar velocidad si es un obstáculo
        if (obj is Obstacle obs)
        {
            obs.Speed = speed;
        }

        _active.Add(obj);
        return obj;
    }

    public void Release(Node2D obj)
    {
        if (!_active.Contains(obj)) return;

        _active.Remove(obj);
        obj.ProcessMode = ProcessModeEnum.Disabled;
        obj.Visible = false;
        _available.Enqueue(obj);
    }

    public void ReleaseAll()
    {
        foreach (var obj in _active)
        {
            obj.ProcessMode = ProcessModeEnum.Disabled;
            obj.Visible = false;
            _available.Enqueue(obj);
        }
        _active.Clear();
    }

    public int ActiveCount => _active.Count;
    public int AvailableCount => _available.Count;
}
