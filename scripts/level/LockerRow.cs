using Godot;

// Spindreihe (Gänge): Hindernis wie Obstacle, zusätzlich in Spinde unterteilt (eine Kachel = ein Spind).
// In zufälligen Abständen springt ein Spind auf: Die Tür klappt nach vorne, die Reihe blitzt kurz auf,
// und ein kleiner Loot (XP oder Schild) springt heraus. Danach schließt der Spind wieder.
// Gedacht für waagerechte Reihen an der oberen/unteren Wand. FrontDirection zeigt in den Gang.
[Tool]
public partial class LockerRow : Obstacle
{
	[Export] public Vector2 FrontDirection = Vector2.Down; // (0, 1) obere Wand, (0, -1) untere Wand
	[Export] public double MinInterval = 60.0;             // Sekunden bis zum nächsten Aufspringen
	[Export] public double MaxInterval = 120.0;
	[Export] public double OpenDuration = 2.0;
	[Export(PropertyHint.Range, "0,1,0.05")] public float XpChance = 0.6f; // sonst Schild
	[Export] public int XpAmount = 10;
	[Export] public int ShieldAmount = 10;

	// Für Sounds (Tim), wie bei LevelElement
	[Signal] public delegate void TriggeredEventHandler(string eventName);

	private readonly RandomNumberGenerator _rng = new();
	private double _nextOpen;
	private int _openTile = -1;
	private float _openAmount;      // 0 = zu, 1 = Tür ganz aufgeklappt

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint()) return;
		_rng.Randomize();
		_nextOpen = _rng.RandfRange((float)MinInterval * 0.3f, (float)MaxInterval); // nicht alle gleichzeitig
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint() || _openTile >= 0) return;

		_nextOpen -= delta;
		if (_nextOpen > 0) return;
		_nextOpen = _rng.RandfRange((float)MinInterval, (float)MaxInterval);
		PopOpen();
	}

	private void PopOpen()
	{
		int tiles = Mathf.Max(1, SizeInTiles.X);
		_openTile = _rng.RandiRange(0, tiles - 1);

		// Tür auf, kurz aufblitzen, nach OpenDuration wieder zu
		Tween tween = CreateTween();
		tween.TweenMethod(Callable.From<float>(SetOpen), 0f, 1f, 0.15).SetTrans(Tween.TransitionType.Back);
		tween.Parallel().TweenProperty(this, "modulate", new Color(1.5f, 1.5f, 1.5f), 0.08);
		tween.Chain().TweenProperty(this, "modulate", Colors.White, 0.2);
		tween.TweenInterval(OpenDuration);
		tween.TweenMethod(Callable.From<float>(SetOpen), 1f, 0f, 0.3);
		tween.TweenCallback(Callable.From(() => _openTile = -1));

		// Loot vor den Spind springen lassen
		Vector2 tileCenter = new(-PixelSize.X / 2f + TileSize * (_openTile + 0.5f), 0);
		Vector2 front = FrontDirection.Normalized() * (PixelSize.Y / 2f);
		Vector2 from = GlobalPosition + tileCenter + front;
		Vector2 to = from + FrontDirection.Normalized() * 60f;
		bool xp = _rng.Randf() < XpChance;
		Pickup.Spawn(GetParent(), from, to, xp ? Pickup.Kind.Xp : Pickup.Kind.Shield, xp ? XpAmount : ShieldAmount);

		EmitSignal(SignalName.Triggered, "spind_auf");
	}

	private void SetOpen(float value)
	{
		_openAmount = value;
		QueueRedraw();
	}

	public override void _Draw()
	{
		base._Draw();

		// Trennlinien zwischen den Spinden
		Vector2 size = PixelSize;
		for (int i = 1; i < SizeInTiles.X; i++)
		{
			float x = -size.X / 2f + i * TileSize;
			DrawLine(new Vector2(x, -size.Y / 2f), new Vector2(x, size.Y / 2f), Color.Darkened(0.4f), 2f);
		}

		if (_openTile < 0 || _openAmount <= 0) return;

		// Offener Spind: dunkles Inneres, Tür klappt nach vorne heraus
		float left = -size.X / 2f + _openTile * TileSize;
		var inside = new Rect2(left + 4, -size.Y / 2f + 4, TileSize - 8, size.Y - 8);
		DrawRect(inside, new Color(0.08f, 0.08f, 0.1f));

		float frontEdge = FrontDirection.Y >= 0 ? size.Y / 2f : -size.Y / 2f;
		float doorLength = 40f * _openAmount * (FrontDirection.Y >= 0 ? 1 : -1);
		var door = new Rect2(left, frontEdge, 8, doorLength).Abs();
		DrawRect(door, Color.Lightened(0.3f));
	}
}
