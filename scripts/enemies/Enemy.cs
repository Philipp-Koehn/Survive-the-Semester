using Godot;

// Basis-Gegner: läuft direkt auf den Spieler zu und macht Kontaktschaden.
// Alle Gegnertypen (Deadline, Hausaufgaben-Stapel, ...) bauen hierauf auf.
public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 120f;
	[Export] public int MaxHealth = 10;
	[Export] public int ContactDamage = 5;
	[Export] public double DamageCooldown = 1.0;

	private int _health;
	private double _cooldownLeft;
	private Node2D _player;

	public override void _Ready()
	{
		_health = MaxHealth;
		AddToGroup("enemies");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null || !IsInstanceValid(_player))
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (_player == null) return;
		}

		// Verfolgungs-KI: direkt in Richtung Spieler
		Velocity = GlobalPosition.DirectionTo(_player.GlobalPosition) * Speed;
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

	public void TakeDamage(int amount)
	{
		_health -= amount;
		if (_health <= 0)
			QueueFree(); // später: XP-Drop
	}
}
