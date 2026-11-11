using Godot;

/// <summary>
/// The player's health, shield, damage and crit values.
/// Incoming damage is taken from the shield first, then from the health.
/// All values are exported so balancing needs no code change, and upgrades can change them at runtime.
/// </summary>
/// <example>
/// Enemies call <c>PlayerController.TakeDamage</c>, which forwards to <see cref="TakeDamage"/>.
/// Weapons call <see cref="RollDamage"/> to get their hit damage.
/// </example>
public partial class PlayerStats : Node
{
    /// <summary>Raised once when health reaches 0.</summary>
    [Signal]
    public delegate void DiedEventHandler();

    /// <summary>Health at the start of a run.</summary>
    [Export]
    public int MaxHealth { get; set; } = 100;

    /// <summary>Shield at the start of a run. It absorbs damage before the health does.</summary>
    [Export]
    public int MaxShield { get; set; } = 25;

    /// <summary>Multiplies the damage of every weapon. 1.0 means no change.</summary>
    [Export]
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>Chance of a critical hit, from 0 (never) to 1 (always).</summary>
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float CritChance { get; set; } = 0.1f;

    /// <summary>Damage multiplier of a critical hit.</summary>
    [Export]
    public float CritMultiplier { get; set; } = 2.0f;

    /// <summary>Current health, never below 0.</summary>
    public int Health { get; private set; }

    /// <summary>Current shield, never below 0.</summary>
    public int Shield { get; private set; }

    /// <summary>True once health has reached 0.</summary>
    public bool IsDead => Health <= 0;

    /// <inheritdoc/>
    public override void _Ready()
    {
        Health = MaxHealth;
        Shield = MaxShield;
    }

    /// <summary>
    /// Takes damage from the shield first, then from the health. Emits <see cref="Died"/> once
    /// when the health reaches 0. Zero or negative damage and damage to a dead player are ignored.
    /// </summary>
    /// <param name="amount">Damage to take.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        int absorbed = Mathf.Min(Shield, amount);
        Shield -= absorbed;
        Health = Mathf.Max(0, Health - (amount - absorbed));

        // Console output until the HUD (UI) shows health and shield.
        GD.Print($"Spieler getroffen! Schaden: {amount}, Schild: {Shield}/{MaxShield}, Leben: {Health}/{MaxHealth}");

        if (IsDead)
        {
            GD.Print("Spieler ist gestorben.");
            EmitSignal(SignalName.Died);
        }
    }

    /// <summary>Returns the damage of one hit for <paramref name="baseDamage"/>, with the damage multiplier and a random crit.</summary>
    /// <param name="baseDamage">Damage of the weapon.</param>
    public int RollDamage(int baseDamage)
    {
        return ResolveDamage(baseDamage, GD.Randf());
    }

    /// <summary>
    /// Same as <see cref="RollDamage"/> with the crit roll given: a roll below <see cref="CritChance"/> is a crit.
    /// </summary>
    /// <param name="baseDamage">Damage of the weapon.</param>
    /// <param name="critRoll">A random value from 0 to 1.</param>
    public int ResolveDamage(int baseDamage, float critRoll)
    {
        float damage = baseDamage * DamageMultiplier;
        if (critRoll < CritChance)
        {
            damage *= CritMultiplier;
        }

        return Mathf.RoundToInt(damage);
    }
}
