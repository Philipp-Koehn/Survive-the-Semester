using Godot;

// Kiste (z. B. im Seminarraum): pulsiert leicht, solange sie zu ist, und öffnet sich bei
// Berührung durch den Spieler. Inhalt: XP, Leben und/oder Schild (über PlayerBridge).
// Öffnet einmal pro Ortsbesuch (die Arena wird beim Ortswechsel neu geladen).
// Mit RespawnCooldown > 0 schließt sie sich nach dieser Zeit wieder und kann erneut geöffnet werden.
public partial class LootChest : LevelElement
{
	[Export] public int XpReward = 50;
	[Export] public int HealAmount = 15;
	[Export] public int ShieldAmount = 0;
	[Export] public double RespawnCooldown = 0;   // 0 = nur einmal
	[Export] public Vector2 Size = new(56, 40);
	[Export] public Color ColorClosed = new(0.75f, 0.55f, 0.2f);
	[Export] public Color ColorOpen = new(0.45f, 0.35f, 0.2f);

	public bool IsOpen { get; private set; }

	private float _lid;          // 0 = Deckel zu, 1 = aufgeklappt
	private Tween _pulse;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = ElementZ;

		var area = new Area2D { Monitorable = false };
		area.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = Size.X * 0.8f } });
		area.BodyEntered += OnBodyEntered;
		AddChild(area);

		StartPulse();
	}

	private void StartPulse()
	{
		_pulse = CreateTween().SetLoops();
		_pulse.TweenProperty(this, "scale", new Vector2(1.07f, 1.07f), 0.6).SetTrans(Tween.TransitionType.Sine);
		_pulse.TweenProperty(this, "scale", Vector2.One, 0.6).SetTrans(Tween.TransitionType.Sine);
	}

	public override void _Draw()
	{
		Vector2 h = Size / 2f;
		Color body = IsOpen ? ColorOpen : ColorClosed;
		DrawRect(new Rect2(-h, Size), body);
		DrawRect(new Rect2(-h, Size), body.Darkened(0.5f), false, 3f);

		// Deckel: oberes Drittel, klappt beim Öffnen nach oben und wird dünner
		float lidHeight = Size.Y * 0.35f * (1f - 0.6f * _lid);
		var lid = new Rect2(-h.X, -h.Y - _lid * 18f, Size.X, lidHeight);
		DrawRect(lid, body.Lightened(0.25f));
		DrawRect(lid, body.Darkened(0.5f), false, 2f);

		if (!IsOpen) // Schloss
			DrawRect(new Rect2(-5, -h.Y + lidHeight - 6, 10, 10), new Color(1f, 0.9f, 0.4f));
	}

	private void SetLid(float value)
	{
		_lid = value;
		QueueRedraw();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (IsOpen || !body.IsInGroup("player")) return;
		IsOpen = true;

		_pulse?.Kill();
		Scale = new Vector2(1.25f, 1.25f);
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(this, "scale", Vector2.One, 0.3).SetTrans(Tween.TransitionType.Back);
		tween.TweenMethod(Callable.From<float>(SetLid), 0f, 1f, 0.25);

		// Belohnung
		Vector2 pos = GlobalPosition;
		float offset = 0;
		if (XpReward > 0 && PlayerBridge.AddXp(body, XpReward))
			ShowFloatingText(GetParent(), pos + new Vector2(0, offset -= 0), $"+{XpReward} XP", Pickup.ColorOf(Pickup.Kind.Xp));
		if (HealAmount > 0 && PlayerBridge.Heal(body, HealAmount))
			ShowFloatingText(GetParent(), pos + new Vector2(0, offset -= 26), $"+{HealAmount} Leben", Pickup.ColorOf(Pickup.Kind.Heal));
		if (ShieldAmount > 0 && PlayerBridge.RestoreShield(body, ShieldAmount))
			ShowFloatingText(GetParent(), pos + new Vector2(0, offset -= 26), $"+{ShieldAmount} Schild", Pickup.ColorOf(Pickup.Kind.Shield));

		GD.Print("Kiste geöffnet!");
		Trigger("kiste_auf");

		if (RespawnCooldown > 0)
			GetTree().CreateTimer(RespawnCooldown, false).Timeout += CloseAgain;
	}

	private void CloseAgain()
	{
		if (!IsInstanceValid(this) || !IsInsideTree()) return;
		IsOpen = false;
		SetLid(0f);
		StartPulse();
	}
}
