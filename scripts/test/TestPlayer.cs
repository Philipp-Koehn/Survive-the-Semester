using Godot;

// Platzhalter-Spieler zum Testen der Gegner-KI.
// Wird später durch Philipps echten Spieler ersetzt.
public partial class TestPlayer : CharacterBody2D
{
	[Export] public float Speed = 250f;
	[Export] public int MaxHealth = 100;

	private int _health;

	public override void _Ready()
	{
		_health = MaxHealth;
		AddToGroup("player");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = direction * Speed;
		MoveAndSlide();
	}

	public void TakeDamage(int amount)
	{
		_health -= amount;
		GD.Print($"Spieler getroffen! HP: {_health}");
	}
}
