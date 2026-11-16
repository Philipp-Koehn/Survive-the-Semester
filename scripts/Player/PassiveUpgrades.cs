using System;
using Godot;

/// <summary>The permanent upgrades that can be bought with credits.</summary>
public enum PassiveKind
{
    /// <summary>More maximum health.</summary>
    Health,

    /// <summary>Higher damage multiplier.</summary>
    Damage,

    /// <summary>Higher crit chance.</summary>
    Crit,
}

/// <summary>
/// Data of the credit shop (story 008, design/game-brief.md "Spieler"): the starter weapons with their prices
/// and the passive upgrades with their effect per level and their cost curve.
/// The arrays of the passive upgrades are indexed by <see cref="PassiveKind"/>.
/// Stored as a resource file so balancing needs no code change.
/// </summary>
/// <example>
/// <c>int cost = catalog.GetCost(PassiveKind.Health, currentLevel);</c>
/// </example>
[GlobalClass]
public partial class PassiveUpgrades : Resource
{
    /// <summary>Starter weapons that can be chosen. Entry 0 is the pen and is always free.</summary>
    [Export]
    public PackedScene[] StarterWeapons { get; set; } = Array.Empty<PackedScene>();

    /// <summary>Credit price of each entry of <see cref="StarterWeapons"/> (entry 0 is ignored, the pen is free).</summary>
    [Export]
    public int[] StarterWeaponPrices { get; set; } = Array.Empty<int>();

    /// <summary>Display name of each passive upgrade.</summary>
    [Export]
    public string[] PassiveNames { get; set; } = { "Mehr Leben", "Mehr Schaden", "Krit-Chance" };

    /// <summary>Effect of one level: extra max health, extra damage multiplier, extra crit chance.</summary>
    [Export]
    public float[] ValuePerLevel { get; set; } = { 10f, 0.05f, 0.02f };

    /// <summary>Cost of the first level of each passive upgrade.</summary>
    [Export]
    public int[] BaseCost { get; set; } = { 20, 25, 30 };

    /// <summary>Each further level costs this much more than the one before.</summary>
    [Export]
    public int[] CostGrowth { get; set; } = { 10, 12, 15 };

    /// <summary>Highest level of each passive upgrade.</summary>
    [Export]
    public int[] MaxLevel { get; set; } = { 10, 10, 10 };

    /// <summary>Credits for buying the next level of <paramref name="kind"/> when it is at <paramref name="currentLevel"/>.</summary>
    public int GetCost(PassiveKind kind, int currentLevel)
    {
        return BaseCost[(int)kind] + CostGrowth[(int)kind] * currentLevel;
    }

    /// <summary>
    /// Applies the bought levels to the stats. Meant to be called once at the start of a run.
    /// </summary>
    /// <param name="stats">The stats of the new run.</param>
    /// <param name="levels">Bought level of each passive upgrade, indexed by <see cref="PassiveKind"/>.</param>
    public void ApplyTo(PlayerStats stats, int[] levels)
    {
        int health = Mathf.RoundToInt(ValuePerLevel[(int)PassiveKind.Health] * levels[(int)PassiveKind.Health]);
        float damage = ValuePerLevel[(int)PassiveKind.Damage] * levels[(int)PassiveKind.Damage];
        float crit = ValuePerLevel[(int)PassiveKind.Crit] * levels[(int)PassiveKind.Crit];
        stats.ApplyPassiveBonuses(health, damage, crit);
    }
}
