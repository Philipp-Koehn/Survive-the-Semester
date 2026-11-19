using Godot;
using System.Collections.Generic;

// Abgesicherte Aufrufe an Spieler-Code (Philipp). Jede Methode wird nur aufgerufen,
// wenn sie existiert (HasMethod, dann Call, wie bei TakeDamage). Fehlt sie, kommt pro
// Spielstart genau eine Warnung, und der Aufruf gibt false bzw. 0 zurück.
// Die benötigten Methoden stehen in docs/gegner-ki.md (Abschnitt 22).
public static class PlayerBridge
{
	private static readonly HashSet<string> _warned = new();

	// ExperienceSystem.AddXp(int): XP hinzufügen
	public static bool AddXp(Node player, int amount)
	{
		return CallIfExists(player?.GetNodeOrNull("ExperienceSystem"), "ExperienceSystem", "AddXp", amount);
	}

	// ExperienceSystem.StealXp(int): XP abziehen, gibt die wirklich abgezogene Menge zurück
	public static int StealXp(Node player, int amount)
	{
		Node xp = player?.GetNodeOrNull("ExperienceSystem");
		if (!Has(xp, "ExperienceSystem", "StealXp")) return 0;
		return xp.Call("StealXp", amount).AsInt32();
	}

	// PlayerStats.Heal(int): Leben auffüllen (höchstens bis MaxHealth)
	public static bool Heal(Node player, int amount)
	{
		return CallIfExists(player?.GetNodeOrNull("PlayerStats"), "PlayerStats", "Heal", amount);
	}

	// PlayerStats.RestoreShield(int): Schild auffüllen (höchstens bis MaxShield)
	public static bool RestoreShield(Node player, int amount)
	{
		return CallIfExists(player?.GetNodeOrNull("PlayerStats"), "PlayerStats", "RestoreShield", amount);
	}

	// PlayerController.ApplySlow(float, double): Spieler kurz verlangsamen
	public static bool ApplySlow(Node player, float factor, double duration)
	{
		if (!Has(player, "PlayerController", "ApplySlow")) return false;
		player.Call("ApplySlow", factor, duration);
		return true;
	}

	private static bool CallIfExists(Node target, string className, string method, int amount)
	{
		if (!Has(target, className, method)) return false;
		target.Call(method, amount);
		return true;
	}

	private static bool Has(Node target, string className, string method)
	{
		if (target != null && target.HasMethod(method)) return true;

		if (_warned.Add(method))
			GD.PushWarning($"Schnittstelle fehlt: {className}.{method} (Absprache mit Philipp, " +
						   "siehe docs/gegner-ki.md). Der Effekt wird übersprungen.");
		return false;
	}
}
