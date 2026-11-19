using Godot;

// Kleiner Loot zum Einsammeln (aus Spinden): schwebt leicht auf und ab und wird bei Berührung
// eingesammelt. Blinkt in den letzten 3 Sekunden und verschwindet nach Lifetime.
// XP = blauer Kristall (wie in der Präsentation), Leben = rotes Kreuz, Schild = türkiser Kreis.
public partial class Pickup : LevelElement
{
	public enum Kind { Xp, Heal, Shield }

	[Export] public Kind Type = Kind.Xp;
	[Export] public int Amount = 10;
	[Export] public double Lifetime = 20.0;
	[Export] public float Radius = 12f;

	private double _age;
	private bool _blinking;
	private bool _collected;

	public static Color ColorOf(Kind kind) => kind switch
	{
		Kind.Xp => new Color(0.3f, 0.55f, 1f),
		Kind.Heal => new Color(1f, 0.3f, 0.3f),
		_ => new Color(0.3f, 0.95f, 0.9f),
	};

	// Erzeugt ein Pickup bei "from" und lässt es zu "to" springen
	public static Pickup Spawn(Node parent, Vector2 from, Vector2 to, Kind kind, int amount)
	{
		var pickup = new Pickup { Type = kind, Amount = amount };
		parent.AddChild(pickup);
		pickup.GlobalPosition = from;
		pickup.CreateTween().TweenProperty(pickup, "global_position", to, 0.25)
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		return pickup;
	}

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = ElementZ;
		AddToGroup("pickups");

		var area = new Area2D { Monitorable = false };
		area.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = Radius + 14f } });
		area.BodyEntered += OnBodyEntered;
		AddChild(area);

		// leichtes Schweben (nur die Zeichnung, nicht die Position)
		Tween bob = CreateTween().SetLoops();
		bob.TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.5).SetTrans(Tween.TransitionType.Sine);
		bob.TweenProperty(this, "scale", Vector2.One, 0.5).SetTrans(Tween.TransitionType.Sine);
	}

	public override void _Process(double delta)
	{
		_age += delta;
		if (!_blinking && _age >= Lifetime - 3.0)
		{
			_blinking = true;
			Tween blink = CreateTween().SetLoops();
			blink.TweenProperty(this, "modulate:a", 0.25, 0.15);
			blink.TweenProperty(this, "modulate:a", 1.0, 0.15);
		}
		if (_age >= Lifetime)
			QueueFree();
	}

	public override void _Draw()
	{
		Color c = ColorOf(Type);
		float r = Radius;
		switch (Type)
		{
			case Kind.Xp: // Kristall
				DrawColoredPolygon(new[] { new Vector2(0, -r * 1.3f), new Vector2(r, 0), new Vector2(0, r * 1.3f), new Vector2(-r, 0) }, c);
				break;
			case Kind.Heal: // Kreuz
				DrawRect(new Rect2(-r * 0.35f, -r, r * 0.7f, r * 2f), c);
				DrawRect(new Rect2(-r, -r * 0.35f, r * 2f, r * 0.7f), c);
				break;
			default: // Schild
				DrawCircle(Vector2.Zero, r, c);
				DrawArc(Vector2.Zero, r * 0.6f, 0, Mathf.Tau, 16, Colors.White, 2f);
				break;
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_collected || !body.IsInGroup("player")) return;
		_collected = true;

		bool applied = Type switch
		{
			Kind.Xp => PlayerBridge.AddXp(body, Amount),
			Kind.Heal => PlayerBridge.Heal(body, Amount),
			_ => PlayerBridge.RestoreShield(body, Amount),
		};
		if (applied)
		{
			string unit = Type switch { Kind.Xp => "XP", Kind.Heal => "Leben", _ => "Schild" };
			ShowFloatingText(GetParent(), GlobalPosition, $"+{Amount} {unit}", ColorOf(Type));
		}

		Trigger("pickup");
		QueueFree();
	}
}
