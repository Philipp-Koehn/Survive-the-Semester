using Godot;

// Bereich, in dem Gegner langsamer sind (Ruhezone in der Bibliothek, nasses Gras am Rasensprenger).
// Arbeitet nur mit den Signalen der Area2D (betreten/verlassen), fragt also nicht jeden Frame ab.
// Der Spieler wird nicht gebremst (dafür bräuchte es ApplySlow von Philipp).
// Bosse sind standardmäßig ausgenommen, damit Bosskämpfe nicht verzerrt werden.
public partial class SlowZone : LevelElement
{
	public enum ZoneShape { Circle, Rectangle }

	[Export] public ZoneShape Shape = ZoneShape.Circle;
	[Export] public float Radius = 220f;                     // für Circle
	[Export] public Vector2 Size = new(800, 600);            // für Rectangle
	[Export(PropertyHint.Range, "0.1,1,0.05")] public float EnemySpeedFactor = 0.5f;
	[Export] public bool AffectBosses = false;
	[Export] public Color Color = new(0.4f, 0.6f, 1f, 0.18f);
	[Export] public bool PulseBorder = false;                // Rand pulsiert dezent (Ruhezone)
	[Export] public string Caption = "";                     // Beschriftung, z. B. "Ruhezone"

	private readonly System.Collections.Generic.HashSet<Enemy> _inside = new();

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = RoomFloorZ;
		AddToGroup("slow_zones");

		var area = new Area2D { Monitorable = false };
		Shape2D shape = Shape == ZoneShape.Circle
			? new CircleShape2D { Radius = Radius }
			: new RectangleShape2D { Size = Size };
		area.AddChild(new CollisionShape2D { Shape = shape });
		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;
		AddChild(area);

		if (Caption != "")
		{
			var label = new Label { Text = Caption, Modulate = new Color(Color, 0.9f) };
			label.AddThemeFontSizeOverride("font_size", 28);
			AddChild(label);
			label.Position = Shape == ZoneShape.Circle
				? new Vector2(-40, -Radius - 40)
				: new Vector2(-Size.X / 2f + 12, -Size.Y / 2f + 8);
		}

		if (PulseBorder)
		{
			Tween pulse = CreateTween().SetLoops();
			pulse.TweenProperty(this, "modulate:a", 0.55, 1.5).SetTrans(Tween.TransitionType.Sine);
			pulse.TweenProperty(this, "modulate:a", 1.0, 1.5).SetTrans(Tween.TransitionType.Sine);
		}
	}

	public bool ContainsPoint(Vector2 globalPoint)
	{
		Vector2 local = globalPoint - GlobalPosition;
		return Shape == ZoneShape.Circle
			? local.Length() <= Radius
			: new Rect2(-Size / 2f, Size).HasPoint(local);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Enemy enemy || (!AffectBosses && enemy is Boss)) return;
		enemy.SetSpeedModifier(this, EnemySpeedFactor);
		_inside.Add(enemy);
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is not Enemy enemy) return;
		enemy.ClearSpeedModifier(this);
		_inside.Remove(enemy);
	}

	public override void _ExitTree()
	{
		// Beim Ortswechsel: Bremse von überlebenden Gegnern (z. B. Bossen) wieder lösen
		foreach (Enemy enemy in _inside)
			if (IsInstanceValid(enemy)) enemy.ClearSpeedModifier(this);
		_inside.Clear();
	}

	public override void _Draw()
	{
		Color border = new(Color, Mathf.Min(1f, Color.A * 4f));
		if (Shape == ZoneShape.Circle)
		{
			DrawCircle(Vector2.Zero, Radius, Color);
			DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 48, border, 3f);
		}
		else
		{
			var rect = new Rect2(-Size / 2f, Size);
			DrawRect(rect, Color);
			DrawRect(rect, border, false, 3f);
		}
	}
}
