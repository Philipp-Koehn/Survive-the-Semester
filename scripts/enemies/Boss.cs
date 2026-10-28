using Godot;

// Prüfungs-Boss: verfolgt den Spieler wie ein normaler Gegner,
// ruft aber regelmäßig kleinere Gegner ("Prüfungsfragen") herbei.
public partial class Boss : Enemy
{
	// Für eine Boss-Lebensleiste in der UI (Fabian)
	[Signal] public delegate void HealthChangedEventHandler(int health, int maxHealth);

	[Export] public string BossName = "Zwischenprüfung";
	[Export] public PackedScene MinionScene;      // welcher Gegner herbeigerufen wird
	[Export] public double SummonInterval = 4.0;  // Sekunden zwischen zwei Beschwörungen
	[Export] public int MinionsPerSummon = 3;
	[Export] public float SummonRadius = 150f;    // Abstand der Helfer zum Boss

	private double _summonLeft;

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

		_summonLeft -= delta;
		if (_summonLeft <= 0)
		{
			_summonLeft = SummonInterval;
			SummonMinions();
		}
	}

	private void SummonMinions()
	{
		if (MinionScene == null) return;

		for (int i = 0; i < MinionsPerSummon; i++)
		{
			var minion = MinionScene.Instantiate<Node2D>();
			GetParent().AddChild(minion);

			// gleichmäßig im Kreis um den Boss verteilen
			float angle = Mathf.Tau * i / MinionsPerSummon;
			minion.GlobalPosition = GlobalPosition + Vector2.FromAngle(angle) * SummonRadius;
		}
	}

	public override void TakeDamage(int amount)
	{
		if (Health <= 0) return;
		base.TakeDamage(amount);

		int hp = Mathf.Max(Health, 0);
		EmitSignal(SignalName.HealthChanged, hp, MaxHealth);
		GD.Print($"{BossName}: {hp}/{MaxHealth} HP");

		if (hp == 0)
			GD.Print($"{BossName} bestanden!");
	}
}
