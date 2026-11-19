using Godot;

// Geschoss der Bosse: fliegt geradeaus, trifft den Spieler über TakeDamage
// und verschwindet an Wänden und Hindernissen oder nach Lifetime Sekunden.
// Bewusst NICHT in der Group "enemies": Waffen zielen nicht darauf, und es gibt keine XP.
public partial class EnemyProjectile : Area2D
{
	[Export] public float Speed = 300f;
	[Export] public int Damage = 8;
	[Export] public double Lifetime = 5.0;
	[Export] public float Radius = 10f;               // nur Zeichnung, die Hitbox steht in der Szene
	[Export] public Color Color = new(1f, 0.85f, 0.2f);

	public Vector2 Direction { get; set; } = Vector2.Right;

	public override void _Ready()
	{
		AddToGroup("enemy_attacks"); // wird beim Ortswechsel mit entfernt
		ZIndex = 5;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Direction * Speed * (float)delta;
		Lifetime -= delta;
		if (Lifetime <= 0)
			QueueFree();
	}

	public override void _Draw()
	{
		// Platzhalter: Kreis mit dunklem Rand
		DrawCircle(Vector2.Zero, Radius, Color);
		DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 16, Colors.Black, 2f);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			body.Call("TakeDamage", Damage);
			QueueFree();
		}
		else if (body is StaticBody2D) // Wände und Hindernisse
		{
			QueueFree();
		}
		// Andere Gegner werden durchflogen
	}
}
