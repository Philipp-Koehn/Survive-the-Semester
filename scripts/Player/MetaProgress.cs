using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

/// <summary>One line of the shop: a starter weapon or a passive upgrade.</summary>
public sealed class PurchaseEntry
{
    /// <summary>True for a starter weapon, false for a passive upgrade.</summary>
    public bool IsStarterWeapon { get; init; }

    /// <summary>Index into <see cref="PassiveUpgrades.StarterWeapons"/> or the <see cref="PassiveKind"/> value.</summary>
    public int Index { get; init; }

    /// <summary>Display name.</summary>
    public string Name { get; init; }

    /// <summary>Credits for the next purchase. 0 when nothing can be bought any more.</summary>
    public int Price { get; init; }

    /// <summary>Starter weapon: true once unlocked. Passive upgrade: true once at the highest level.</summary>
    public bool Owned { get; init; }

    /// <summary>Passive upgrade: bought level. Starter weapon: 0.</summary>
    public int Level { get; init; }

    /// <summary>Passive upgrade: highest level. Starter weapon: 0.</summary>
    public int MaxLevel { get; init; }

    /// <summary>True when the entry is not owned and the credits suffice.</summary>
    public bool CanBuy { get; init; }

    /// <summary>Starter weapon: true when this is the chosen one.</summary>
    public bool IsSelected { get; init; }
}

/// <summary>
/// Credits, unlocked starter weapons and passive upgrade levels, kept across runs (story 008,
/// design/game-brief.md "Spieler"). Saved as JSON under <c>user://</c>. A missing or corrupt file means
/// 0 credits and no unlocks; only the pen is free. The shop UI (Fabian) uses <see cref="Instance"/>,
/// the signals and <see cref="GetPurchaseList"/>; it needs no node in any scene.
/// </summary>
/// <example>
/// <c>MetaProgress.Instance.TryBuyPassive(PassiveKind.Health);</c>
/// </example>
public partial class MetaProgress : RefCounted
{
    private const string SavePath = "user://meta_progress.json";
    private const string CatalogPath = "res://data/Player/PassiveUpgrades.tres";

    /// <summary>Raised whenever the credits changed.</summary>
    /// <param name="credits">The new credits.</param>
    [Signal]
    public delegate void CreditsChangedEventHandler(int credits);

    /// <summary>Raised when a starter weapon was unlocked or chosen, or a passive upgrade was bought.</summary>
    [Signal]
    public delegate void UnlocksChangedEventHandler();

    private sealed class SaveData
    {
        public int Credits { get; set; }
        public string[] UnlockedStarters { get; set; }
        public string SelectedStarter { get; set; }
        public int[] PassiveLevels { get; set; }
    }

    private static MetaProgress _instance;
    private readonly HashSet<string> _unlockedStarters = new();
    private int[] _passiveLevels;

    /// <summary>The one instance, loaded from the save file on first use.</summary>
    public static MetaProgress Instance => _instance ??= Create();

    /// <summary>Shop data: starter weapons, prices and passive upgrades.</summary>
    public PassiveUpgrades Catalog { get; private set; }

    /// <summary>Credits the player owns.</summary>
    public int Credits { get; private set; }

    /// <summary>Scene path of the chosen starter weapon. Empty means the pen.</summary>
    public string SelectedStarterPath { get; private set; } = "";

    private static MetaProgress Create()
    {
        var progress = new MetaProgress();
        progress.Catalog = ResourceLoader.Exists(CatalogPath) ? GD.Load<PassiveUpgrades>(CatalogPath) : null;
        if (progress.Catalog == null)
        {
            GD.PushWarning($"MetaProgress: Katalog '{CatalogPath}' nicht gefunden, es gibt nur den Stift.");
            progress.Catalog = new PassiveUpgrades();
        }

        progress._passiveLevels = new int[Enum.GetValues<PassiveKind>().Length];
        progress.Load();
        return progress;
    }

    /// <summary>Adds credits (for example the award at the end of a run) and saves. Zero or negative amounts are ignored.</summary>
    public void AddCredits(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Credits += amount;
        Save();
        EmitSignal(SignalName.CreditsChanged, Credits);
    }

    /// <summary>True when starter weapon <paramref name="index"/> may be used. The pen (index 0) always is.</summary>
    public bool IsStarterUnlocked(int index)
    {
        return index == 0 || _unlockedStarters.Contains(Catalog.StarterWeapons[index].ResourcePath);
    }

    /// <summary>
    /// Buys starter weapon <paramref name="index"/> for good. Returns false and changes nothing when the index is invalid,
    /// the weapon is already free or the credits are too few.
    /// </summary>
    public bool TryUnlockStarter(int index)
    {
        if (index <= 0 || index >= Catalog.StarterWeapons.Length || IsStarterUnlocked(index))
        {
            return false;
        }

        int price = Catalog.StarterWeaponPrices[index];
        if (Credits < price)
        {
            GD.Print($"[Meta] Zu wenig Credits ({Credits}/{price}).");
            return false;
        }

        Credits -= price;
        _unlockedStarters.Add(Catalog.StarterWeapons[index].ResourcePath);
        Save();
        EmitSignal(SignalName.CreditsChanged, Credits);
        EmitSignal(SignalName.UnlocksChanged);
        return true;
    }

    /// <summary>Chooses the starter weapon for the next runs. Returns false when the index is invalid or the weapon is not unlocked.</summary>
    public bool SelectStarter(int index)
    {
        if (index < 0 || index >= Catalog.StarterWeapons.Length || !IsStarterUnlocked(index))
        {
            return false;
        }

        SelectedStarterPath = Catalog.StarterWeapons[index].ResourcePath;
        Save();
        EmitSignal(SignalName.UnlocksChanged);
        return true;
    }

    /// <summary>Bought level of a passive upgrade.</summary>
    public int GetPassiveLevel(PassiveKind kind)
    {
        return _passiveLevels[(int)kind];
    }

    /// <summary>
    /// Buys the next level of a passive upgrade. Returns false and changes nothing when it is at the highest level
    /// or the credits are too few.
    /// </summary>
    public bool TryBuyPassive(PassiveKind kind)
    {
        int level = _passiveLevels[(int)kind];
        if (level >= Catalog.MaxLevel[(int)kind])
        {
            return false;
        }

        int price = Catalog.GetCost(kind, level);
        if (Credits < price)
        {
            GD.Print($"[Meta] Zu wenig Credits ({Credits}/{price}).");
            return false;
        }

        Credits -= price;
        _passiveLevels[(int)kind]++;
        Save();
        EmitSignal(SignalName.CreditsChanged, Credits);
        EmitSignal(SignalName.UnlocksChanged);
        return true;
    }

    /// <summary>Applies the bought passive upgrades to the stats. Call once at run start.</summary>
    public void ApplyPassivesTo(PlayerStats stats)
    {
        Catalog.ApplyTo(stats, _passiveLevels);
    }

    /// <summary>The shop contents: every starter weapon, then every passive upgrade, with price and state.</summary>
    public List<PurchaseEntry> GetPurchaseList()
    {
        var list = new List<PurchaseEntry>();

        for (int i = 0; i < Catalog.StarterWeapons.Length; i++)
        {
            PackedScene scene = Catalog.StarterWeapons[i];
            var weapon = scene.Instantiate<Weapon>();
            string name = weapon.WeaponName;
            weapon.Free();

            bool owned = IsStarterUnlocked(i);
            int price = i == 0 ? 0 : Catalog.StarterWeaponPrices[i];
            list.Add(new PurchaseEntry
            {
                IsStarterWeapon = true,
                Index = i,
                Name = name,
                Price = price,
                Owned = owned,
                CanBuy = !owned && Credits >= price,
                IsSelected = scene.ResourcePath == SelectedStarterPath || (i == 0 && SelectedStarterPath == ""),
            });
        }

        foreach (PassiveKind kind in Enum.GetValues<PassiveKind>())
        {
            int level = _passiveLevels[(int)kind];
            int max = Catalog.MaxLevel[(int)kind];
            bool maxed = level >= max;
            int price = maxed ? 0 : Catalog.GetCost(kind, level);
            list.Add(new PurchaseEntry
            {
                Index = (int)kind,
                Name = Catalog.PassiveNames[(int)kind],
                Price = price,
                Owned = maxed,
                Level = level,
                MaxLevel = max,
                CanBuy = !maxed && Credits >= price,
            });
        }

        return list;
    }

    private void Load()
    {
        try
        {
            string file = ProjectSettings.GlobalizePath(SavePath);
            if (!File.Exists(file))
            {
                return;
            }

            SaveData data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(file))
                ?? throw new JsonException("Speicherdatei ist leer.");

            // Validate everything before assigning, so a bad file leaves the clean start state.
            var unlocked = new HashSet<string>();
            foreach (string path in data.UnlockedStarters ?? Array.Empty<string>())
            {
                if (!string.IsNullOrEmpty(path))
                {
                    unlocked.Add(path);
                }
            }

            var levels = new int[_passiveLevels.Length];
            int[] saved = data.PassiveLevels ?? Array.Empty<int>();
            for (int i = 0; i < levels.Length && i < saved.Length; i++)
            {
                levels[i] = Math.Clamp(saved[i], 0, Catalog.MaxLevel[i]);
            }

            Credits = Math.Max(0, data.Credits);
            foreach (string path in unlocked)
            {
                _unlockedStarters.Add(path);
            }

            _passiveLevels = levels;
            string selected = data.SelectedStarter ?? "";
            bool selectedValid = selected == "" || unlocked.Contains(selected) || IsPen(selected);
            SelectedStarterPath = selectedValid ? selected : "";
        }
        catch (Exception e)
        {
            GD.PushError($"MetaProgress: Speicherdatei konnte nicht gelesen werden, starte mit 0 Credits: {e.Message}");
            Credits = 0;
            _unlockedStarters.Clear();
            _passiveLevels = new int[_passiveLevels.Length];
            SelectedStarterPath = "";
        }
    }

    private bool IsPen(string path)
    {
        return Catalog.StarterWeapons.Length > 0 && Catalog.StarterWeapons[0].ResourcePath == path;
    }

    private void Save()
    {
        try
        {
            var data = new SaveData
            {
                Credits = Credits,
                UnlockedStarters = new List<string>(_unlockedStarters).ToArray(),
                SelectedStarter = SelectedStarterPath,
                PassiveLevels = _passiveLevels,
            };

            string file = ProjectSettings.GlobalizePath(SavePath);
            string temp = file + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(data));
            File.Move(temp, file, true);
        }
        catch (Exception e)
        {
            GD.PushError($"MetaProgress: Speichern fehlgeschlagen: {e.Message}");
        }
    }
}
