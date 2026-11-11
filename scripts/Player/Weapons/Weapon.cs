using Godot;

/// <summary>Weapon behaviour. More kinds (area damage) follow with the weapon slots in story 003.</summary>
public enum WeaponKind
{
    /// <summary>Hits the nearest enemy in range.</summary>
    SingleTarget,
}

/// <summary>
/// Base weapon: fires automatically at the nearest enemy in range and respects its cooldown.
/// All values are exported so balancing needs no code change.
/// </summary>
/// <example>
/// Instance Pen.tscn as a child of the player; tune <see cref="Damage"/> in the Inspector.
/// </example>
public partial class Weapon : Node2D
{
    private const double ShotFlashSeconds = 0.08;

    /// <summary>Display name of the weapon.</summary>
    [Export]
    public string WeaponName { get; set; } = "Stift";

    /// <summary>Damage dealt per hit.</summary>
    [Export]
    public int Damage { get; set; } = 5;

    /// <summary>Seconds between two shots.</summary>
    [Export]
    public double Cooldown { get; set; } = 0.6;

    /// <summary>Maximum distance to a target in pixels.</summary>
    [Export]
    public float Range { get; set; } = 250.0f;

    /// <summary>How the weapon picks and hits its targets.</summary>
    [Export]
    public WeaponKind Kind { get; set; } = WeaponKind.SingleTarget;

    private double _cooldownLeft;
    private double _flashLeft;
    private Vector2 _lastTargetGlobal;

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        _cooldownLeft -= delta;

        if (_flashLeft > 0)
        {
            _flashLeft -= delta;
            if (_flashLeft <= 0)
            {
                QueueRedraw();
            }
        }

        if (_cooldownLeft > 0)
        {
            return;
        }

        Node2D target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        Fire(target);
        _cooldownLeft = Cooldown;
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_flashLeft > 0)
        {
            DrawLine(Vector2.Zero, ToLocal(_lastTargetGlobal), Colors.Yellow, 3.0f);
        }
    }

    private Node2D FindNearestEnemy()
    {
        Node2D nearest = null;
        float nearestDistance = Range;

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Node2D enemy || !IsInstanceValid(enemy) || enemy.IsQueuedForDeletion())
            {
                continue;
            }

            float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (distance <= nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void Fire(Node2D target)
    {
        // Enemies expose TakeDamage(int); see docs/gegner-ki.md.
        if (target.HasMethod("TakeDamage"))
        {
            target.Call("TakeDamage", Damage);
        }

        _lastTargetGlobal = target.GlobalPosition;
        _flashLeft = ShotFlashSeconds;
        QueueRedraw();
    }
}
