using System.Collections.Generic;
using Godot;

/// <summary>What a level-up option does when chosen.</summary>
public enum UpgradeKind
{
    /// <summary>Raise a weapon by one level.</summary>
    WeaponLevel,

    /// <summary>Apply the ultimate upgrade to a maxed weapon.</summary>
    WeaponUltimate,

    /// <summary>Add a new weapon into a free slot.</summary>
    NewWeapon,
}

/// <summary>One choice offered on a level-up.</summary>
public sealed class UpgradeOption
{
    /// <summary>What the option does.</summary>
    public UpgradeKind Kind { get; init; }

    /// <summary>The weapon to upgrade (null for a new weapon).</summary>
    public Weapon Weapon { get; init; }

    /// <summary>The scene of the new weapon (null for an upgrade).</summary>
    public PackedScene Scene { get; init; }

    /// <summary>Text for the choice list.</summary>
    public string Label { get; init; }
}

/// <summary>
/// Offers random upgrade options on a level-up and applies the chosen one.
/// The game is paused during the choice. The choice is shown in the console until the UI exists.
/// </summary>
/// <example>
/// Call <see cref="TriggerLevelUp"/> when the player levels up; the choice is made with keys 1-3.
/// </example>
public partial class UpgradeManager : Node
{
    /// <summary>Path to the <see cref="WeaponInventory"/> this manager upgrades.</summary>
    [Export]
    public NodePath InventoryPath { get; set; } = "../WeaponInventory";

    /// <summary>Weapon scenes that can be offered as new weapons.</summary>
    [Export]
    public PackedScene[] WeaponPool { get; set; } = System.Array.Empty<PackedScene>();

    /// <summary>How many options a level-up offers at most.</summary>
    [Export]
    public int OptionCount { get; set; } = 3;

    private readonly List<UpgradeOption> _offered = new();
    private readonly RandomNumberGenerator _rng = new();
    private WeaponInventory _inventory;

    /// <summary>True while the player is choosing and the game is paused.</summary>
    public bool IsChoosing => _offered.Count > 0;

    /// <summary>The options currently on offer.</summary>
    public IReadOnlyList<UpgradeOption> Offered => _offered;

    /// <inheritdoc/>
    public override void _Ready()
    {
        // Must keep running while the tree is paused, or the choice could never be made.
        ProcessMode = ProcessModeEnum.Always;
        _rng.Randomize();
        _inventory = GetNode<WeaponInventory>(InventoryPath);
    }


    /// <summary>Starts a level-up: offers options and pauses. Does nothing when no option exists.</summary>
    public void TriggerLevelUp()
    {
        // No level-up while choosing, and none after death: the run is over and the game is paused.
        if (IsChoosing || GetNode<PlayerStats>("../PlayerStats").IsDead)
        {
            return;
        }

        _offered.AddRange(BuildOptions());
        if (_offered.Count == 0)
        {
            GD.Print("Level-Up: keine Optionen, übersprungen");
            return;
        }

        GetTree().Paused = true;
        GD.Print("Level-Up! Wähle mit 1-" + _offered.Count + ":");
        for (int i = 0; i < _offered.Count; i++)
        {
            GD.Print($"  {i + 1}) {_offered[i].Label}");
        }
    }

    /// <summary>Applies option <paramref name="index"/> (0-based) and resumes the game. Ignored when out of range.</summary>
    public void Choose(int index)
    {
        if (!IsChoosing || index < 0 || index >= _offered.Count)
        {
            return;
        }

        UpgradeOption option = _offered[index];
        switch (option.Kind)
        {
            case UpgradeKind.WeaponLevel:
                option.Weapon.LevelUp();
                break;
            case UpgradeKind.WeaponUltimate:
                option.Weapon.ApplyUltimate();
                break;
            case UpgradeKind.NewWeapon:
                _inventory.TryAddWeapon(option.Scene);
                break;
        }

        GD.Print($"Gewählt: {option.Label}");
        _offered.Clear();
        GetTree().Paused = false;
    }

    private List<UpgradeOption> BuildOptions()
    {
        var candidates = new List<UpgradeOption>();
        var ownedScenes = new HashSet<string>();

        foreach (Weapon weapon in _inventory.Weapons)
        {
            ownedScenes.Add(weapon.SceneFilePath);

            if (!weapon.IsMaxLevel)
            {
                candidates.Add(new UpgradeOption
                {
                    Kind = UpgradeKind.WeaponLevel,
                    Weapon = weapon,
                    Label = $"{weapon.WeaponName}: Level {weapon.Level} -> {weapon.Level + 1}",
                });
            }
            else if (!weapon.HasUltimate)
            {
                candidates.Add(new UpgradeOption
                {
                    Kind = UpgradeKind.WeaponUltimate,
                    Weapon = weapon,
                    Label = $"{weapon.WeaponName}: ULTIMATIVES Upgrade",
                });
            }
        }

        if (!_inventory.IsFull)
        {
            foreach (PackedScene scene in WeaponPool)
            {
                if (scene == null || ownedScenes.Contains(scene.ResourcePath))
                {
                    continue;
                }

                candidates.Add(new UpgradeOption
                {
                    Kind = UpgradeKind.NewWeapon,
                    Scene = scene,
                    Label = $"Neue Waffe: {WeaponNameOf(scene)}",
                });
            }
        }

        Shuffle(candidates);
        int count = Mathf.Min(OptionCount, candidates.Count);
        return candidates.GetRange(0, count);
    }

    private static string WeaponNameOf(PackedScene scene)
    {
        var instance = scene.Instantiate<Weapon>();
        string name = instance.WeaponName;
        instance.Free();
        return name;
    }

    private void Shuffle(List<UpgradeOption> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (IsChoosing)
        {
            if (@event.IsActionPressed("upgrade_pick_1")) Choose(0);
            else if (@event.IsActionPressed("upgrade_pick_2")) Choose(1);
            else if (@event.IsActionPressed("upgrade_pick_3")) Choose(2);

            // Consume the input so the weapon slots do not also switch (e.g. key 2 -> slot 2).
            // Choose() unpauses the tree, so without this the WeaponInventory would receive the same key.
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("debug_level_up"))
        {
            // TEMPORARY test trigger (key 9) until a real XP/level system exists.
            TriggerLevelUp();
        }
    }
}
