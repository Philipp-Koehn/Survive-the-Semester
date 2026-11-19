using Godot;

// Trittbrettfahrer: läuft zum Spieler, klaut beim ersten Kontakt XP und flieht.
// Entkommt er, ist die XP weg (er verschwindet ohne Died, gibt also auch keine Kill-XP).
// Wird er besiegt, bekommt der Spieler die geklaute XP zurück (zusätzlich zur normalen Kill-XP).
//
// Schnittstelle zum XP-System (Philipp), wird nur aufgerufen, wenn sie existiert:
//   int ExperienceSystem.StealXp(int amount)  zieht XP ab, gibt die wirklich abgezogene Menge zurück
//   void ExperienceSystem.AddXp(int amount)   gibt XP zurück
// Fehlt sie, warnt PlayerBridge einmal, und er flieht mit 0 XP.
public partial class FreeRider : Enemy
{
	[Export] public int StealAmount = 25;          // so viel XP will er klauen
	[Export] public float FleeSpeed = 260f;        // Tempo auf der Flucht
	[Export] public double FleeDuration = 8.0;     // nach so vielen Sekunden Flucht ist er weg
	[Export] public float EscapeDistance = 1000f;  // oder sobald er so weit weg ist
	[Export] public Color LootColor = new(0.75f, 1f, 0.75f); // Farbe, solange er Beute trägt

	public bool IsFleeing { get; private set; }
	public int StolenXp { get; private set; }

	private double _fleeTime;

	// Auf der Flucht läuft er vom Spieler weg, nicht zur Tür
	protected override bool FollowsWaypoints => !IsFleeing;

	protected override Vector2 GetMoveDirection()
	{
		Vector2 toPlayer = GlobalPosition.DirectionTo(Player.GlobalPosition);
		return IsFleeing ? -toPlayer : toPlayer;
	}

	protected override float GetMoveSpeed()
	{
		return IsFleeing ? FleeSpeed : Speed;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (!IsFleeing || Player == null || !IsInstanceValid(Player)) return;

		_fleeTime += delta;
		if (_fleeTime >= FleeDuration || GlobalPosition.DistanceTo(Player.GlobalPosition) >= EscapeDistance)
			Escape();
	}

	// Klaut nur einmal, danach berührt er den Spieler nicht mehr absichtlich
	protected override void OnPlayerContact(Node player)
	{
		if (IsFleeing) return;

		StolenXp = PlayerBridge.StealXp(player, StealAmount);

		IsFleeing = true;
		if (GetNodeOrNull<Sprite2D>("Sprite2D") is Sprite2D sprite)
			sprite.Modulate = LootColor;
		GD.Print($"Trittbrettfahrer klaut {StolenXp} XP und flieht!");
	}

	// Entkommen: XP bleibt weg, kein Died-Signal (also keine Kill-XP, kein Kill-Sound)
	private void Escape()
	{
		GD.Print($"Trittbrettfahrer ist mit {StolenXp} XP entkommen.");
		SetPhysicsProcess(false);
		QueueFree();
	}

	protected override void Die()
	{
		if (StolenXp > 0 && Player != null && IsInstanceValid(Player))
		{
			if (PlayerBridge.AddXp(Player, StolenXp))
				GD.Print($"Trittbrettfahrer gestellt: {StolenXp} XP zurück!");
		}
		base.Die();
	}
}
