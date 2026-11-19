using Godot;

// Energy-Drink-Automat (Gänge, Mensa): Wer ihn berührt, bekommt Schild aufgefüllt.
// Danach Cooldown: Der Automat ist grau, und ein Ring um ihn füllt sich, bis er wieder bereit ist.
// Ist der Schild schon voll, wird nichts verbraucht ("Schild voll").
// Fester Körper (Hindernis), die Berührung wird über einen etwas größeren Bereich erkannt.
public partial class EnergyDrinkMachine : LevelElement
{
	[Export] public int ShieldAmount = 25;
	[Export] public double Cooldown = 45.0;
	[Export] public Vector2 Size = new(64, 64);
	[Export] public float TriggerRadius = 75f;
	[Export] public Color ColorReady = new(0.15f, 0.85f, 0.55f);
	[Export] public Color ColorEmpty = new(0.4f, 0.4f, 0.42f);

	public bool IsReady => _cooldownLeft <= 0;

	private double _cooldownLeft;
	private double _redrawLeft;
	private bool _playerInZone;
	private Node2D _player;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = ElementZ;

		var body = new StaticBody2D();
		body.AddToGroup("obstacles");
		body.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = Size } });
		AddChild(body);

		var area = new Area2D { Monitorable = false };
		area.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = TriggerRadius } });
		area.BodyEntered += b => { if (b.IsInGroup("player")) { _playerInZone = true; _player = b; TryUse(); } };
		area.BodyExited += b => { if (b.IsInGroup("player")) _playerInZone = false; };
		AddChild(area);

		SetProcess(false); // läuft nur während des Cooldowns
	}

	public override void _Process(double delta)
	{
		_cooldownLeft -= delta;
		_redrawLeft -= delta;
		if (_redrawLeft <= 0) // Ring nur 5× pro Sekunde neu zeichnen
		{
			_redrawLeft = 0.2;
			QueueRedraw();
		}

		if (_cooldownLeft > 0) return;

		// wieder bereit
		SetProcess(false);
		QueueRedraw();
		Pop();
		Trigger("automat_bereit");
		if (_playerInZone) TryUse();
	}

	private void TryUse()
	{
		if (!IsReady || _player == null) return;

		if (_player is PlayerController pc && pc.Stats.Shield >= pc.Stats.MaxShield)
		{
			ShowFloatingText(GetParent(), GlobalPosition, "Schild voll", ColorEmpty.Lightened(0.4f));
			return;
		}

		if (PlayerBridge.RestoreShield(_player, ShieldAmount))
			ShowFloatingText(GetParent(), GlobalPosition, $"+{ShieldAmount} Schild", Pickup.ColorOf(Pickup.Kind.Shield));

		_cooldownLeft = Cooldown;
		_redrawLeft = 0;
		SetProcess(true);
		Pop();
		Trigger("energydrink");
	}

	private void Pop()
	{
		Scale = new Vector2(1.15f, 1.15f);
		CreateTween().TweenProperty(this, "scale", Vector2.One, 0.25).SetTrans(Tween.TransitionType.Back);
	}

	public override void _Draw()
	{
		Vector2 h = Size / 2f;
		Color c = IsReady ? ColorReady : ColorEmpty;
		DrawRect(new Rect2(-h, Size), c.Darkened(0.35f));
		DrawRect(new Rect2(-h.X + 8, -h.Y + 8, Size.X - 16, Size.Y * 0.45f), c);   // Sichtfenster
		DrawRect(new Rect2(-h.X + 12, h.Y - 16, Size.X - 24, 8), Colors.Black);      // Ausgabe
		DrawRect(new Rect2(-h, Size), Colors.Black, false, 2f);

		if (!IsReady)
		{
			// Cooldown-Ring: füllt sich im Uhrzeigersinn
			float progress = 1f - (float)(_cooldownLeft / Cooldown);
			float r = Size.X * 0.75f;
			DrawArc(Vector2.Zero, r, 0, Mathf.Tau, 32, new Color(0, 0, 0, 0.35f), 6f);
			DrawArc(Vector2.Zero, r, -Mathf.Pi / 2, -Mathf.Pi / 2 + Mathf.Tau * progress, 32, ColorReady, 6f);
		}
	}
}
