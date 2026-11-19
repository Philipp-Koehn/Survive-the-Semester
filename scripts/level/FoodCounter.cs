using Godot;

// Essensausgabe (Mensa): einmal pro Semesterwoche eine kleine Heilung.
// Ist sie bereit, steht ein Tablett auf der Theke, das leicht pulsiert. Berührt der Spieler die
// Theke, bekommt er HealAmount Leben (über PlayerBridge.Heal). Danach ist sie bis zur nächsten
// Woche leer (WaveManager.WeekChanged). Bei vollem Leben wird nichts verbraucht ("Satt").
// Feste Theke (Hindernis), die Berührung wird über einen etwas größeren Bereich erkannt.
public partial class FoodCounter : LevelElement
{
	[Export] public int HealAmount = 20;
	[Export] public Vector2 Size = new(320, 64);
	[Export] public float TriggerMargin = 60f;
	[Export] public Color CounterColor = new(0.6f, 0.62f, 0.66f);
	[Export] public Color TrayColor = new(1f, 0.75f, 0.35f);

	public bool IsReady { get; private set; } = true;

	private Tween _pulse;
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
		area.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = Size + new Vector2(TriggerMargin, TriggerMargin) * 2f } });
		area.BodyEntered += b => { if (b.IsInGroup("player")) { _playerInZone = true; _player = b; TryServe(); } };
		area.BodyExited += b => { if (b.IsInGroup("player")) _playerInZone = false; };
		AddChild(area);

		// Neue Woche = neues Essen. Die Arena wird beim Ortswechsel neu erzeugt, dann ist sie sofort bereit.
		if (GetTree().CurrentScene?.GetNodeOrNull<WaveManager>("WaveManager") is WaveManager waves)
			waves.WeekChanged += OnWeekChanged;

		StartPulse();
	}

	private void OnWeekChanged(int week)
	{
		if (IsReady) return;
		IsReady = true;
		QueueRedraw();
		StartPulse();
		Trigger("essen_bereit");
		if (_playerInZone) TryServe();
	}

	private void StartPulse()
	{
		_pulse?.Kill();
		_pulse = CreateTween().SetLoops();
		_pulse.TweenProperty(this, "modulate", new Color(1.25f, 1.25f, 1.25f), 0.7).SetTrans(Tween.TransitionType.Sine);
		_pulse.TweenProperty(this, "modulate", Colors.White, 0.7).SetTrans(Tween.TransitionType.Sine);
	}

	private void TryServe()
	{
		if (!IsReady || _player == null) return;

		if (_player is PlayerController pc && pc.Stats.Health >= pc.Stats.MaxHealth)
		{
			ShowFloatingText(GetParent(), GlobalPosition, "Satt", CounterColor.Lightened(0.4f));
			return;
		}

		if (PlayerBridge.Heal(_player, HealAmount))
			ShowFloatingText(GetParent(), GlobalPosition, $"+{HealAmount} Leben", Pickup.ColorOf(Pickup.Kind.Heal));

		IsReady = false;
		_pulse?.Kill();
		Modulate = Colors.White;
		QueueRedraw();
		Trigger("essen");
	}

	public override void _Draw()
	{
		Vector2 h = Size / 2f;
		DrawRect(new Rect2(-h, Size), CounterColor);
		DrawRect(new Rect2(-h, Size), CounterColor.Darkened(0.5f), false, 3f);
		// Glasscheibe der Theke
		DrawRect(new Rect2(-h.X + 8, -h.Y + 6, Size.X - 16, 10), new Color(0.75f, 0.9f, 1f, 0.6f));

		if (IsReady) // Tablett mit Teller
		{
			var tray = new Rect2(-40, -12, 80, 34);
			DrawRect(tray, TrayColor);
			DrawRect(tray, TrayColor.Darkened(0.4f), false, 2f);
			DrawCircle(new Vector2(-12, 5), 11, Colors.White);
			DrawCircle(new Vector2(18, 5), 7, new Color(0.9f, 0.3f, 0.2f));
		}
	}
}
