using Godot;

// Basis-Gegner: läuft direkt auf den Spieler zu und macht Kontaktschaden.
// Alle Gegnertypen (Deadline, Hausaufgaben-Stapel, Bosse) bauen hierauf auf.
public partial class Enemy : CharacterBody2D
{
	// Wird ausgelöst, wenn der Gegner stirbt (z. B. für Bosse, XP-Drops, Statistik)
	[Signal] public delegate void DiedEventHandler(Enemy enemy);

	[Export] public float Speed = 120f;
	[Export] public int MaxHealth = 10;
	[Export] public int ContactDamage = 5;
	[Export] public double DamageCooldown = 1.0;

	public int Health { get; private set; }

	protected Node2D Player;
	private double _cooldownLeft;

	public override void _Ready()
	{
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

		// Verfolgungs-KI: direkt in Richtung Spieler
		Velocity = GlobalPosition.DirectionTo(Player.GlobalPosition) * Speed;
		MoveAndSlide();

		// Kontaktschaden mit Abklingzeit
		_cooldownLeft -= delta;
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			if (GetSlideCollision(i).GetCollider() is Node hit
				&& hit.IsInGroup("player")
				&& _cooldownLeft <= 0)
			{
				hit.Call("TakeDamage", ContactDamage);
				_cooldownLeft = DamageCooldown;
			}
		}
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
		QueueFree(); // später: XP-Drop
	}
}
