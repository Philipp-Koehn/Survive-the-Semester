using Godot;

/// <summary>How a weapon picks and hits its targets.</summary>
public enum WeaponKind
{
    /// <summary>Hits the nearest enemy in range.</summary>
    SingleTarget,

    /// <summary>Hits every enemy within range of the player at once.</summary>
    Area,
}

/// <summary>
/// Base weapon: fires automatically while it is the active weapon, and respects its cooldown.
/// The cooldown runs even while the weapon is inactive, so switching never resets it.
/// All values are exported so balancing needs no code change.
/// </summary>
/// <example>
/// Add a weapon scene under <see cref="WeaponInventory"/>; the inventory sets <see cref="IsActive"/>.
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

    /// <summary>True while this weapon is the one the player currently uses. Set by <see cref="WeaponInventory"/>.</summary>
    public bool IsActive { get; set; }

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

        if (!IsActive || _cooldownLeft > 0)
        {
            return;
        }

        if (Kind == WeaponKind.Area)
        {
            if (FindNearestEnemy() == null)
            {
                return;
            }

            FireArea();
        }
        else
        {
            Node2D target = FindNearestEnemy();
            if (target == null)
            {
                return;
            }

            FireSingle(target);
        }

        _cooldownLeft = Cooldown;
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_flashLeft <= 0)
        {
            return;
        }

        if (Kind == WeaponKind.Area)
        {
            DrawCircle(Vector2.Zero, Range, new Color(1.0f, 1.0f, 0.0f, 0.2f));
        }
        else
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

    private void FireSingle(Node2D target)
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

    private void FireArea()
    {
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Node2D enemy
                && IsInstanceValid(enemy)
                && !enemy.IsQueuedForDeletion()
                && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= Range
                && enemy.HasMethod("TakeDamage"))
            {
                enemy.Call("TakeDamage", Damage);
            }
        }

        _flashLeft = ShotFlashSeconds;
        QueueRedraw();
    }
}
