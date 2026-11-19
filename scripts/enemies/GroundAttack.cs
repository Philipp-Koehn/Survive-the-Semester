using Godot;

// Bodenangriff der Bosse (Bodenausbruch, Pop-Quiz-Bombe):
// Zuerst wird WarningTime Sekunden lang eine Warnmarkierung gezeichnet, deren Füllung wächst.
// Ist sie voll, bekommt der Spieler Schaden, wenn er im Radius steht, danach kurzer Blitz.
// So kann der Spieler sehen, wo es gleich einschlägt, und ausweichen.
public partial class GroundAttack : Node2D
{
	[Export] public float Radius = 80f;
	[Export] public double WarningTime = 1.0;   // Vorwarnzeit in Sekunden
	[Export] public int Damage = 15;
	[Export] public Color Color = new(1f, 0.25f, 0.15f);
	[Export] public double FlashTime = 0.2;     // wie lange der Einschlag sichtbar bleibt

	private const float PlayerRadius = 20f;     // Spieler gilt als getroffen, wenn er den Kreis berührt
	private double _time;
	private bool _exploded;

	public override void _Ready()
	{
		AddToGroup("enemy_attacks"); // wird beim Ortswechsel mit entfernt
		ZIndex = -5;                 // über dem Boden, unter Spieler und Gegnern
	}

	public override void _Process(double delta)
	{
		_time += delta;
		if (!_exploded && _time >= WarningTime)
			Explode();
		else if (_exploded && _time >= WarningTime + FlashTime)
			QueueFree();
		QueueRedraw();
	}

	private void Explode()
	{
		_exploded = true;
		if (GetTree().GetFirstNodeInGroup("player") is Node2D player
			&& player.GlobalPosition.DistanceTo(GlobalPosition) <= Radius + PlayerRadius)
		{
			player.Call("TakeDamage", Damage);
		}
	}

	public override void _Draw()
	{
		if (_exploded)
		{
			// Einschlag: voller, heller Kreis
			DrawCircle(Vector2.Zero, Radius, new Color(Color, 0.85f));
			return;
		}

		// Warnung: Umriss plus wachsende Füllung (zeigt, wie viel Zeit bleibt)
		float progress = (float)Mathf.Clamp(_time / WarningTime, 0, 1);
		DrawCircle(Vector2.Zero, Radius, new Color(Color, 0.15f));
		DrawCircle(Vector2.Zero, Radius * progress, new Color(Color, 0.35f));
		DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 32, new Color(Color, 0.9f), 3f);
	}
}
