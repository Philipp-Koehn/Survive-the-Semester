using System.Collections.Generic;
using Godot;

/// <summary>
/// The player's weapon slots. Holds up to <see cref="MaxSlots"/> weapons and selects which one is active.
/// Only the active weapon fires. Keys 1-4 select a slot; the mouse wheel steps through them.
/// </summary>
/// <example>
/// Add weapon scenes as children, or call <see cref="TryAddWeapon"/> with a weapon scene at runtime.
/// </example>
public partial class WeaponInventory : Node2D
{
    /// <summary>Maximum number of weapons the player can carry.</summary>
    [Export]
    public int MaxSlots { get; set; } = 4;

    private readonly List<Weapon> _slots = new();

    /// <summary>Index of the active weapon, or -1 when there is none.</summary>
    public int ActiveIndex { get; private set; } = -1;

    /// <summary>Number of weapons currently in the slots.</summary>
    public int Count => _slots.Count;

    /// <summary>True when every slot is taken.</summary>
    public bool IsFull => _slots.Count >= MaxSlots;

    /// <summary>The weapons in slot order.</summary>
    public IReadOnlyList<Weapon> Weapons => _slots;

    /// <inheritdoc/>
    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Weapon weapon && _slots.Count < MaxSlots)
            {
                _slots.Add(weapon);
            }
        }

        if (_slots.Count > 0)
        {
            Select(0);
        }

        ApplyStarterWeapon();
    }

    /// <summary>
    /// Story 008: puts the starter weapon chosen in the meta progress into the first slot at run start.
    /// Without a choice, or when the choice is the weapon already there (the pen), nothing changes.
    /// </summary>
    private void ApplyStarterWeapon()
    {
        string path = MetaProgress.Instance.SelectedStarterPath;
        if (path == "" || (_slots.Count > 0 && _slots[0].SceneFilePath == path))
        {
            return;
        }

        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"Startwaffe '{path}' nicht gefunden, der Stift bleibt.");
            return;
        }

        foreach (Weapon weapon in _slots)
        {
            RemoveChild(weapon);
            weapon.QueueFree();
        }

        _slots.Clear();
        ActiveIndex = -1;
        TryAddWeapon(GD.Load<PackedScene>(path));
    }

    /// <summary>
    /// Adds a weapon into the next free slot.
    /// Returns false, and adds nothing, when all slots are full.
    /// </summary>
    public bool TryAddWeapon(PackedScene weaponScene)
    {
        if (_slots.Count >= MaxSlots)
        {
            return false;
        }

        Weapon weapon = weaponScene.Instantiate<Weapon>();
        AddChild(weapon);
        _slots.Add(weapon);

        if (ActiveIndex < 0)
        {
            Select(0);
        }
        else
        {
            weapon.IsActive = false;
        }

        return true;
    }

    /// <summary>Makes the weapon at <paramref name="index"/> the active one. Out-of-range indices are ignored.</summary>
    public void Select(int index)
    {
        if (index < 0 || index >= _slots.Count)
        {
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].IsActive = i == index;
        }

        ActiveIndex = index;
        GD.Print($"Aktive Waffe: {_slots[index].WeaponName} (Slot {index + 1})");
    }

    /// <summary>Selects the next weapon, wrapping around to the first.</summary>
    public void SelectNext()
    {
        if (_slots.Count > 0)
        {
            Select((ActiveIndex + 1) % _slots.Count);
        }
    }

    /// <summary>Selects the previous weapon, wrapping around to the last.</summary>
    public void SelectPrevious()
    {
        if (_slots.Count > 0)
        {
            Select((ActiveIndex - 1 + _slots.Count) % _slots.Count);
        }
    }

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("weapon_1")) Select(0);
        else if (@event.IsActionPressed("weapon_2")) Select(1);
        else if (@event.IsActionPressed("weapon_3")) Select(2);
        else if (@event.IsActionPressed("weapon_4")) Select(3);
        else if (@event.IsActionPressed("weapon_next")) SelectNext();
        else if (@event.IsActionPressed("weapon_prev")) SelectPrevious();
    }
}
