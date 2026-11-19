using Godot;

// Basis-Gegner: läuft direkt auf den Spieler zu und macht Kontaktschaden.
// Alle Gegnertypen (Deadline, Hausaufgaben-Stapel, Trittbrettfahrer, Müdigkeit, Bosse) bauen hierauf auf.
// Untertypen ändern das Verhalten über GetMoveDirection, GetMoveSpeed und OnPlayerContact.
public partial class Enemy : CharacterBody2D
{
	// Wird ausgelöst, wenn der Gegner stirbt (z. B. für Bosse, XP, Statistik)
	[Signal] public delegate void DiedEventHandler(Enemy enemy);

	[Export] public float Speed = 120f;
	[Export] public int MaxHealth = 10;
	[Export] public int ContactDamage = 5;
	[Export] public double DamageCooldown = 1.0;

	public int Health { get; private set; }

	// Zwischenziel, z. B. eine Tür, wenn Spieler und Gegner in verschiedenen Räumen sind.
	// Wird von SideRoom gesetzt und gelöscht. null = direkt zum Spieler.
	public Vector2? Waypoint { get; set; }

	protected Node2D Player;
	private double _cooldownLeft;

	// Darf der Gegner gerade einem Wegpunkt folgen? (Der fliehende Trittbrettfahrer z. B. nicht)
	protected virtual bool FollowsWaypoints => true;

	// Normale Gegner werden mit jeder Woche zäher (Faktor kommt vom EnemySpawner), Bosse nicht
	protected virtual bool ScalesWithWeek => true;

	public override void _Ready()
	{
		if (ScalesWithWeek && GetTree().GetFirstNodeInGroup("enemy_spawner") is EnemySpawner spawner)
			MaxHealth = Mathf.Max(1, Mathf.RoundToInt(MaxHealth * spawner.HealthMultiplier));

		Health = MaxHealth;
		AddToGroup("enemies");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Player == null || !IsInstanceValid(Player))
		{
			Player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (Player == null) return;
		}

		// Verfolgungs-KI: Standard ist direkt in Richtung Spieler
		Vector2 direction = Waypoint is Vector2 waypoint && FollowsWaypoints
			? GlobalPosition.DirectionTo(waypoint)
			: GetMoveDirection();
		Velocity = direction * GetMoveSpeed();
		MoveAndSlide();

		// Kontakt mit Abklingzeit
		_cooldownLeft -= delta;
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			if (GetSlideCollision(i).GetCollider() is Node hit
				&& hit.IsInGroup("player")
				&& _cooldownLeft <= 0)
			{
				OnPlayerContact(hit);
				_cooldownLeft = DamageCooldown;
			}
		}
	}

	// Richtung, in die der Gegner läuft (Länge 1 oder 0)
	protected virtual Vector2 GetMoveDirection()
	{
		return GlobalPosition.DirectionTo(Player.GlobalPosition);
	}

	protected virtual float GetMoveSpeed()
	{
		return Speed;
	}

	// Was beim Berühren des Spielers passiert (höchstens alle DamageCooldown Sekunden)
	protected virtual void OnPlayerContact(Node player)
	{
		player.Call("TakeDamage", ContactDamage);
	}

	public virtual void TakeDamage(int amount)
	{
		if (Health <= 0) return; // schon tot, nicht doppelt sterben
		Health -= amount;
		if (Health <= 0)
			Die();
	}

	protected virtual void Die()
	{
		EmitSignal(SignalName.Died, this);
		QueueFree();
	}
}
