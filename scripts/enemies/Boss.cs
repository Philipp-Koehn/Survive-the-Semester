using Godot;

// Prüfungs-Boss: verfolgt den Spieler wie ein normaler Gegner,
// ruft aber regelmäßig kleinere Gegner ("Prüfungsfragen") herbei.
// Ohne MinionScene ruft der Boss niemanden. Bosse mit eigenen Angriffen erben davon (ExamBoss).
public partial class Boss : Enemy
{
	// Für eine Boss-Lebensleiste in der UI (Fabian)
	[Signal] public delegate void HealthChangedEventHandler(int health, int maxHealth);

	[Export] public string BossName = "Zwischenprüfung";
	[Export] public PackedScene MinionScene;      // welcher Gegner herbeigerufen wird
	[Export] public double SummonInterval = 4.0;  // Sekunden zwischen zwei Beschwörungen
	[Export] public int MinionsPerSummon = 3;
	[Export] public float SummonRadius = 150f;    // Abstand der Helfer zum Boss
	[Export] public int MaxMinionsAlive = 12;     // höchstens so viele eigene Helfer gleichzeitig (zusätzlich zu MaxEnemies)

	private double _summonLeft;
	private readonly System.Collections.Generic.List<Node> _minions = new();

	// Bosse haben feste Leben und wachsen nicht mit der Woche
	protected override bool ScalesWithWeek => false;

	// Darf der Boss gerade Helfer rufen? (ExamBoss z. B. erst in Phase 2)
	protected virtual bool CanSummon => true;

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("bosses");
		_summonLeft = SummonInterval;
		GD.Print($"{BossName} erscheint! ({MaxHealth} HP)");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta); // normale Verfolgung und Kontaktschaden

		if (MinionScene == null || !CanSummon) return;

		_summonLeft -= delta;
		if (_summonLeft <= 0)
		{
			_summonLeft = SummonInterval;
			SummonMinions();
		}
	}

	private void SummonMinions()
	{
		// Besiegte oder entfernte Helfer austragen, dann nur bis zur Obergrenze auffüllen
		_minions.RemoveAll(m => !IsInstanceValid(m) || m.IsQueuedForDeletion());
		int count = Mathf.Min(MinionsPerSummon, MaxMinionsAlive - _minions.Count);
		if (count <= 0) return;

		var arena = GetTree().GetFirstNodeInGroup("arena") as ArenaFloor;

		for (int i = 0; i < count; i++)
		{
			var minion = MinionScene.Instantiate<Node2D>();
			GetParent().AddChild(minion);
			_minions.Add(minion);

			// gleichmäßig im Kreis um den Boss verteilen
			float angle = Mathf.Tau * i / MinionsPerSummon;
			Vector2 pos = GlobalPosition + Vector2.FromAngle(angle) * SummonRadius;
			// nicht hinter der Wand entstehen lassen
			minion.GlobalPosition = arena == null ? pos : arena.ClampInside(pos, 60f);
		}
	}

	public override void TakeDamage(int amount)
	{
		if (Health <= 0) return;

		// Erst melden, dann Schaden nehmen, damit UI und Ausgabe
		// den Treffer vor dem Tod mitbekommen
		int hp = Mathf.Max(Health - amount, 0);
		EmitSignal(SignalName.HealthChanged, hp, MaxHealth);
		GD.Print($"{BossName}: {hp}/{MaxHealth} HP");
		if (hp == 0)
			GD.Print($"{BossName} bestanden!");

		base.TakeDamage(amount);
	}
}
