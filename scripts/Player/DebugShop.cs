using Godot;

/// <summary>
/// Debug-only stand-in for the shop until the shop UI exists (story 008).
/// Key 8 buys the next passive upgrade (health, damage, crit in turn), key 0 unlocks and selects the next starter weapon
/// (calculator, laptop, pen, then again from the start; the pen is always free).
/// Bought upgrades and the chosen weapon apply at the start of the next run.
/// Every action prints its result to the console.
/// </summary>
public partial class DebugShop : Node
{
    private int _nextPassive;
    private int _nextStarter = 1;

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_buy_passive"))
        {
            BuyPassive();
        }
        else if (@event.IsActionPressed("debug_unlock_starter"))
        {
            UnlockStarter();
        }
    }

    private static void Log(string text) => GD.Print($"[Shop] {text}");

    private void BuyPassive()
    {
        MetaProgress meta = MetaProgress.Instance;
        var kind = (PassiveKind)(_nextPassive % 3);
        int before = meta.Credits;
        int levelBefore = meta.GetPassiveLevel(kind);
        bool bought = meta.TryBuyPassive(kind);
        Log(bought
            ? $"{kind} gekauft: Stufe {levelBefore} -> {meta.GetPassiveLevel(kind)}, Credits {before} -> {meta.Credits}. Wirkt ab dem naechsten Run."
            : $"{kind} NICHT gekauft (zu wenig Credits oder Maximalstufe). Credits {meta.Credits}, Stufe {levelBefore}.");
        _nextPassive++;
    }

    private void UnlockStarter()
    {
        MetaProgress meta = MetaProgress.Instance;
        int count = meta.Catalog.StarterWeapons.Length;
        if (count <= 1)
        {
            Log("Keine weiteren Startwaffen im Katalog.");
            return;
        }

        int index = _nextStarter;
        int before = meta.Credits;
        bool owned = meta.IsStarterUnlocked(index);
        bool unlocked = owned || meta.TryUnlockStarter(index);
        if (unlocked && meta.SelectStarter(index))
        {
            Log($"Startwaffe {index} gewaehlt{(owned ? "" : " und freigeschaltet")}, Credits {before} -> {meta.Credits}. Wirkt ab dem naechsten Run.");
        }
        else
        {
            Log($"Startwaffe {index} NICHT freigeschaltet (zu wenig Credits). Credits {meta.Credits}.");
        }

        // Cycle through every starter weapon, the pen (index 0) included.
        _nextStarter = (_nextStarter + 1) % count;
    }
}
