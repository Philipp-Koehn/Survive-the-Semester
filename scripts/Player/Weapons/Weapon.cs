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

    /// <summary>Highest upgrade level. A weapon starts on level 1.</summary>
    [Export]
    public int MaxLevel { get; set; } = 4;

    /// <summary>Extra damage gained per upgrade level.</summary>
    [Export]
    public int DamagePerLevel { get; set; } = 2;

    /// <summary>Seconds taken off the cooldown per upgrade level.</summary>
    [Export]
    public double CooldownReductionPerLevel { get; set; } = 0.05;

    /// <summary>The cooldown never drops below this many seconds.</summary>
    [Export]
    public double MinCooldown { get; set; } = 0.15;

    /// <summary>Damage multiplier of the ultimate upgrade.</summary>
    [Export]
    public float UltimateDamageMultiplier { get; set; } = 2.0f;

    /// <summary>Range multiplier of the ultimate upgrade.</summary>
    [Export]
    public float UltimateRangeMultiplier { get; set; } = 1.5f;

    /// <summary>Current upgrade level, from 1 to <see cref="MaxLevel"/>.</summary>
    public int Level { get; private set; } = 1;

    /// <summary>True once the ultimate upgrade has been applied.</summary>
    public bool HasUltimate { get; private set; }

    /// <summary>True when no normal upgrade is left.</summary>
    public bool IsMaxLevel => Level >= MaxLevel;

    /// <summary>True while this weapon is the one the player currently uses. Set by <see cref="WeaponInventory"/>.</summary>
    public bool IsActive { get; set; }

    /// <summary>Raises the level by one: more damage, shorter cooldown. Does nothing on the maximum level.</summary>
    public void LevelUp()
    {
        if (IsMaxLevel)
        {
            return;
        }

        Level++;
        Damage += DamagePerLevel;
        Cooldown = Mathf.Max(MinCooldown, Cooldown - CooldownReductionPerLevel);
    }

    /// <summary>Applies the ultimate upgrade once the weapon is on its maximum level.</summary>
    public void ApplyUltimate()
    {
        if (!IsMaxLevel || HasUltimate)
        {
            return;
        }

        HasUltimate = true;
        Damage = Mathf.RoundToInt(Damage * UltimateDamageMultiplier);
        Range *= UltimateRangeMultiplier;
    }

    private PlayerStats _stats;
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

    // Hit damage with the player's damage multiplier and crit. Without a player it is the plain damage.
    private int RollHitDamage()
    {
        if (_stats == null || !IsInstanceValid(_stats))
        {
            _stats = (GetTree().GetFirstNodeInGroup("player") as PlayerController)?.Stats;
        }

        return _stats?.RollDamage(Damage) ?? Damage;
    }

    private void FireSingle(Node2D target)
    {
        // Enemies expose TakeDamage(int); see docs/gegner-ki.md.
        if (target.HasMethod("TakeDamage"))
        {
            target.Call("TakeDamage", RollHitDamage());
        }

        _lastTargetGlobal = target.GlobalPosition;
        _flashLeft = ShotFlashSeconds;
        QueueRedraw();
    }

    private void FireArea()
    {
        int hitDamage = RollHitDamage();

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Node2D enemy
                && IsInstanceValid(enemy)
                && !enemy.IsQueuedForDeletion()
                && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= Range
                && enemy.HasMethod("TakeDamage"))
            {
                enemy.Call("TakeDamage", hitDamage);
            }
        }

        _flashLeft = ShotFlashSeconds;
        QueueRedraw();
    }
}
