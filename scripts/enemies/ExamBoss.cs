using Godot;

// Prüfungs-Boss mit eigenen Angriffsmustern. Erbt von Boss (Leben, Beschwörung, HealthChanged,
// Group "bosses") und ergänzt schaltbare Angriffe. Jeder Angriff ist aus, solange sein Intervall 0 ist.
// So entstehen alle Bosse (Zwischenprüfung Programmieren, Klausuren, Professor) aus demselben Skript,
// nur die Werte in der jeweiligen Szene unterscheiden sich.
//
// Angriffe:
// - Gezielte Salve: AimedShots Geschosse im Fächer auf den Spieler
// - Ring: RingCount Geschosse gleichmäßig in alle Richtungen, jede Salve leicht gedreht
// - Bodenangriff: Warnmarkierungen unter dem Spieler, verstreut um ihn und in einer Linie vom Boss zu ihm
// - Sturmangriff: Boss bleibt stehen und blinkt (Warnung), dann stürmt er geradeaus
// - Phase 2: ab Phase2HealthFraction der Leben werden alle Angriffe schneller, mehr Bodenmarker
public partial class ExamBoss : Boss
{
	// z. B. für die UI (Fabian): 2 = zweite Phase hat begonnen
	[Signal] public delegate void PhaseChangedEventHandler(int phase);

	[ExportGroup("Bewegung")]
	[Export] public float PreferredDistance = 0f; // 0 = direkt verfolgen, sonst Abstand halten und seitlich kreisen
	[Export] public float AttackRange = 900f;     // greift nur an, wenn der Spieler so nah ist (im Bild)

	[ExportGroup("Geschosse")]
	[Export] public PackedScene ProjectileScene;  // leer = enemy_projectile.tscn
	[Export] public float ProjectileSpeed = 300f;
	[Export] public int ProjectileDamage = 8;
	[Export] public Color ProjectileColor = new(1f, 0.85f, 0.2f);
	[Export] public double AimedInterval = 0;     // gezielte Salve, 0 = aus
	[Export] public int AimedShots = 3;
	[Export] public float AimedSpreadDegrees = 20f; // Winkel zwischen äußerem linken und rechten Geschoss
	[Export] public double RingInterval = 0;      // Ring, 0 = aus
	[Export] public int RingCount = 12;

	[ExportGroup("Bodenangriff")]
	[Export] public PackedScene GroundAttackScene; // leer = ground_attack.tscn
	[Export] public double GroundInterval = 0;    // 0 = aus
	[Export] public bool GroundAtPlayer = true;   // eine Markierung genau unter dem Spieler
	[Export] public int GroundScatterCount = 0;   // weitere zufällig um den Spieler
	[Export] public float GroundScatterRadius = 220f;
	[Export] public int GroundLineCount = 0;      // Markierungen in einer Linie vom Boss zum Spieler
	[Export] public double GroundLineStagger = 0.15; // jede weitere in der Linie schlägt so viel später ein
	[Export] public float GroundRadius = 80f;
	[Export] public double GroundWarningTime = 1.0;
	[Export] public int GroundDamage = 15;
	[Export] public Color GroundColor = new(1f, 0.25f, 0.15f);

	[ExportGroup("Sturmangriff")]
	[Export] public double DashInterval = 0;      // 0 = aus
	[Export] public double DashWindup = 0.8;      // so lange steht er still und blinkt vorher
	[Export] public float DashSpeed = 520f;
	[Export] public double DashDuration = 0.6;
	[Export] public Color DashWarningColor = new(1f, 1f, 1f);

	[ExportGroup("Phase 2")]
	[Export(PropertyHint.Range, "0,1,0.05")] public float Phase2HealthFraction = 0f; // 0 = keine Phase 2
	[Export] public float Phase2IntervalFactor = 0.7f; // Intervalle werden damit multipliziert
	[Export] public int Phase2ExtraScatter = 0;        // zusätzliche Bodenmarker
	[Export] public bool SummonOnlyInPhase2 = false;   // Helfer erst in Phase 2

	public bool InPhase2 { get; private set; }

	private enum DashState { None, Windup, Dashing }

	private DashState _dash = DashState.None;
	private double _dashTime;
	private Vector2 _dashDirection;
	private double _aimedLeft, _ringLeft, _groundLeft, _dashLeft, _strafeLeft;
	private float _ringRotation;
	private float _strafeSign = 1f;
	private Sprite2D _sprite;
	private Color _baseColor = Colors.White;
	private readonly RandomNumberGenerator _rng = new();

	protected override bool CanSummon => !SummonOnlyInPhase2 || InPhase2;

	public override void _Ready()
	{
		base._Ready();
		_rng.Randomize();
		ProjectileScene ??= GD.Load<PackedScene>("res://scenes/enemies/enemy_projectile.tscn");
		GroundAttackScene ??= GD.Load<PackedScene>("res://scenes/enemies/ground_attack.tscn");

		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (_sprite != null) _baseColor = _sprite.Modulate;

		// Erster Angriff nicht sofort, sondern nach einem vollen Intervall (versetzt)
		_aimedLeft = AimedInterval;
		_ringLeft = RingInterval * 1.3;
		_groundLeft = GroundInterval * 0.8;
		_dashLeft = DashInterval;
		_strafeLeft = 4.0;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta); // Bewegung (über GetMoveDirection), Kontaktschaden, Beschwörung
		if (Player == null || !IsInstanceValid(Player) || Health <= 0) return;

		UpdateDash(delta);

		_strafeLeft -= delta;
		if (_strafeLeft <= 0)
		{
			_strafeLeft = 4.0;
			_strafeSign = -_strafeSign; // Kreisrichtung wechseln
		}

		// Keine Angriffe außerhalb des Bildes und nicht während des Sturmangriffs
		if (_dash != DashState.None) return;
		if (GlobalPosition.DistanceTo(Player.GlobalPosition) > AttackRange) return;

		if (Tick(ref _aimedLeft, AimedInterval, delta)) FireAimed();
		if (Tick(ref _ringLeft, RingInterval, delta)) FireRing();
		if (Tick(ref _groundLeft, GroundInterval, delta)) SpawnGroundAttacks();
		if (Tick(ref _dashLeft, DashInterval, delta)) StartDash();
	}

	// Zählt einen Angriffs-Timer herunter. true = Angriff jetzt auslösen.
	private bool Tick(ref double left, double interval, double delta)
	{
		if (interval <= 0) return false;
		left -= delta;
		if (left > 0) return false;
		left = InPhase2 ? interval * Phase2IntervalFactor : interval;
		return true;
	}

	// ---------- Bewegung ----------

	protected override Vector2 GetMoveDirection()
	{
		switch (_dash)
		{
			case DashState.Windup: return Vector2.Zero;
			case DashState.Dashing: return _dashDirection;
		}

		Vector2 toPlayer = GlobalPosition.DirectionTo(Player.GlobalPosition);
		if (PreferredDistance <= 0) return toPlayer;

		// Abstand halten: zu weit weg = näher, zu nah = zurück, sonst seitlich um den Spieler kreisen
		float distance = GlobalPosition.DistanceTo(Player.GlobalPosition);
		if (distance > PreferredDistance + 60f) return toPlayer;
		if (distance < PreferredDistance - 60f) return -toPlayer;
		return toPlayer.Orthogonal() * _strafeSign;
	}

	protected override float GetMoveSpeed()
	{
		return _dash == DashState.Dashing ? DashSpeed : Speed;
	}

	// ---------- Sturmangriff ----------

	private void StartDash()
	{
		_dash = DashState.Windup;
		_dashTime = DashWindup;
	}

	private void UpdateDash(double delta)
	{
		if (_dash == DashState.None) return;
		_dashTime -= delta;

		if (_dash == DashState.Windup)
		{
			// Warnung: schnelles Blinken
			if (_sprite != null)
				_sprite.Modulate = ((int)(_dashTime * 10) % 2 == 0) ? DashWarningColor : _baseColor;

			if (_dashTime <= 0)
			{
				// Richtung erst jetzt festlegen: Wer rechtzeitig zur Seite geht, weicht aus
				_dashDirection = GlobalPosition.DirectionTo(Player.GlobalPosition);
				_dash = DashState.Dashing;
				_dashTime = DashDuration;
				if (_sprite != null) _sprite.Modulate = _baseColor;
			}
		}
		else if (_dashTime <= 0)
		{
			_dash = DashState.None;
		}
	}

	// ---------- Geschosse ----------

	private void FireAimed()
	{
		float baseAngle = GlobalPosition.DirectionTo(Player.GlobalPosition).Angle();
		float spread = Mathf.DegToRad(AimedSpreadDegrees);
		for (int i = 0; i < AimedShots; i++)
		{
			float offset = AimedShots == 1 ? 0f : -spread / 2f + spread * i / (AimedShots - 1);
			SpawnProjectile(Vector2.FromAngle(baseAngle + offset));
		}
	}

	private void FireRing()
	{
		for (int i = 0; i < RingCount; i++)
			SpawnProjectile(Vector2.FromAngle(_ringRotation + Mathf.Tau * i / RingCount));

		// nächste Salve um eine halbe Lücke gedreht, damit man nicht an derselben Stelle stehen bleiben kann
		_ringRotation += Mathf.Pi / RingCount;
	}

	private void SpawnProjectile(Vector2 direction)
	{
		if (ProjectileScene?.Instantiate() is not EnemyProjectile projectile) return;
		projectile.Direction = direction;
		projectile.Speed = ProjectileSpeed;
		projectile.Damage = ProjectileDamage;
		projectile.Color = ProjectileColor;
		GetParent().AddChild(projectile);
		projectile.GlobalPosition = GlobalPosition;
	}

	// ---------- Bodenangriffe ----------

	private void SpawnGroundAttacks()
	{
		Vector2 target = Player.GlobalPosition;

		if (GroundAtPlayer)
			SpawnGroundAttack(target, GroundWarningTime);

		int scatter = GroundScatterCount + (InPhase2 ? Phase2ExtraScatter : 0);
		for (int i = 0; i < scatter; i++)
		{
			Vector2 offset = Vector2.FromAngle(_rng.RandfRange(0f, Mathf.Tau))
						   * _rng.RandfRange(GroundRadius, GroundScatterRadius);
			SpawnGroundAttack(target + offset, GroundWarningTime);
		}

		// Linie vom Boss zum Spieler, schlägt nacheinander ein (wie eine Welle aus dem Boden)
		for (int i = 1; i <= GroundLineCount; i++)
		{
			Vector2 pos = GlobalPosition.Lerp(target, (float)i / (GroundLineCount + 1));
			SpawnGroundAttack(pos, GroundWarningTime + i * GroundLineStagger);
		}
	}

	private void SpawnGroundAttack(Vector2 position, double warningTime)
	{
		if (GroundAttackScene?.Instantiate() is not GroundAttack attack) return;
		attack.Radius = GroundRadius;
		attack.WarningTime = warningTime;
		attack.Damage = GroundDamage;
		attack.Color = GroundColor;
		GetParent().AddChild(attack);

		var arena = GetTree().GetFirstNodeInGroup("arena") as ArenaFloor;
		attack.GlobalPosition = arena == null ? position : arena.ClampInside(position, GroundRadius * 0.5f);
	}

	// ---------- Phase 2 ----------

	public override void TakeDamage(int amount)
	{
		base.TakeDamage(amount);

		if (!InPhase2 && Phase2HealthFraction > 0 && Health > 0
			&& Health <= MaxHealth * Phase2HealthFraction)
		{
			InPhase2 = true;
			_baseColor = _baseColor.Lerp(Colors.Red, 0.4f); // Platzhalter: rötlicher = wütend
			if (_sprite != null && _dash == DashState.None) _sprite.Modulate = _baseColor;
			GD.Print($"{BossName}: Phase 2!");
			EmitSignal(SignalName.PhaseChanged, 2);
		}
	}
}
