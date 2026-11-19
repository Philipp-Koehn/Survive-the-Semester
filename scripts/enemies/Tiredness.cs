using Godot;

// Müdigkeit: verfolgt den Spieler wie ein normaler Gegner, macht wenig Schaden,
// verlangsamt ihn aber bei jeder Berührung für kurze Zeit.
//
// Schnittstelle zum Spieler (Philipp), wird nur aufgerufen, wenn sie existiert:
//   void PlayerController.ApplySlow(float factor, double duration)
//   factor = Anteil des normalen Tempos (0,5 = halb so schnell), duration in Sekunden.
public partial class Tiredness : Enemy
{
	[Export(PropertyHint.Range, "0.1,1,0.05")] public float SlowFactor = 0.5f;
	[Export] public double SlowDuration = 2.0;

	protected override void OnPlayerContact(Node player)
	{
		base.OnPlayerContact(player); // normaler Kontaktschaden

		PlayerBridge.ApplySlow(player, SlowFactor, SlowDuration); // fehlt sie: einmalige Warnung
	}
}
