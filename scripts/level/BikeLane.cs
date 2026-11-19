using Godot;

// Radweg (Outdoor-Campus): eine Spur quer durch die Arena (entlang der lokalen x-Achse,
// für senkrechte Spuren den Knoten um 90° drehen). In zufälligen Abständen:
// 1. Warnung: Die Spur blinkt WarningTime Sekunden lang ("Klingeln").
// 2. Ein Fahrrad fährt mit BikeSpeed durch, abwechselnd in beide Richtungen.
//    Wen es trifft, nimmt einmal pro Durchfahrt Schaden: Gegner viel, Spieler wenig.
// Das Fahrrad ist kein festes Hindernis, damit es niemanden einklemmt oder wegschiebt.
public partial class BikeLane : LevelElement
{
	[Export] public float Length = 5000f;
	[Export] public float Width = 96f;
	[Export] public double MinInterval = 8.0;
	[Export] public double MaxInterval = 14.0;
	[Export] public double WarningTime = 1.2;
	[Export] public float BikeSpeed = 900f;
	[Export] public int EnemyDamage = 20;
	[Export] public int PlayerDamage = 5;
	[Export] public bool HitsBosses = false;
	[Export] public Color LaneColor = new(0.55f, 0.55f, 0.58f, 0.35f);
	[Export] public Color WarningColor = new(1f, 0.85f, 0.2f, 0.45f);
	[Export] public Color BikeColor = new(0.9f, 0.2f, 0.2f);

	public bool IsActive => _state != State.Idle;

	private enum State { Idle, Warning, Riding }

	private readonly RandomNumberGenerator _rng = new();
	private State _state = State.Idle;
	private double _nextPass;
	private float _flash;          // 0..1, Stärke der Warnfarbe
	private int _direction = 1;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = RoomFloorZ;
		AddToGroup("bike_lanes");
		_rng.Randomize();
		_nextPass = _rng.RandfRange((float)MinInterval, (float)MaxInterval);
	}

	public override void _Process(double delta)
	{
		if (_state != State.Idle) return;
		_nextPass -= delta;
		if (_nextPass <= 0) StartPass();
	}

	// Startet sofort eine Durchfahrt (mit Warnung). Öffentlich für Tests.
	public void StartPass()
	{
		if (_state != State.Idle) return;
		_state = State.Warning;
		Trigger("fahrrad_klingel");

		// Spur blinkt dreimal
		Tween warn = CreateTween();
		int blinks = 3;
		double step = WarningTime / (blinks * 2);
		for (int i = 0; i < blinks; i++)
		{
			warn.TweenMethod(Callable.From<float>(SetFlash), 0f, 1f, step);
			warn.TweenMethod(Callable.From<float>(SetFlash), 1f, 0f, step);
		}
		warn.TweenCallback(Callable.From(Ride));
	}

	private void SetFlash(float value)
	{
		_flash = value;
		QueueRedraw();
	}

	private void Ride()
	{
		_state = State.Riding;
		var hit = new System.Collections.Generic.HashSet<ulong>();

		var bike = new Area2D { Monitorable = false };
		bike.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(64, 30) } });
		bike.AddChild(new Polygon2D
		{
			Color = BikeColor,
			Polygon = new[] { new Vector2(-32, -15), new Vector2(32, -15), new Vector2(32, 15), new Vector2(-32, 15) },
		});
		bike.BodyEntered += body =>
		{
			if (!hit.Add(body.GetInstanceId())) return; // einmal pro Durchfahrt
			if (body.IsInGroup("player"))
			{
				body.Call("TakeDamage", PlayerDamage);
				Trigger("fahrrad_treffer");
			}
			else if (body is Enemy enemy && (HitsBosses || enemy is not Boss))
			{
				enemy.TakeDamage(EnemyDamage);
			}
		};
		AddChild(bike);

		float half = Length / 2f + 60f;
		bike.Position = new Vector2(-half * _direction, 0);
		Tween ride = CreateTween();
		ride.TweenProperty(bike, "position", new Vector2(half * _direction, 0), 2 * half / BikeSpeed);
		ride.TweenCallback(Callable.From(() =>
		{
			bike.QueueFree();
			_direction = -_direction;
			_state = State.Idle;
			_nextPass = _rng.RandfRange((float)MinInterval, (float)MaxInterval);
		}));
	}

	public override void _Draw()
	{
		var lane = new Rect2(-Length / 2f, -Width / 2f, Length, Width);
		DrawRect(lane, LaneColor);
		// gestrichelte Mittellinie
		for (float x = -Length / 2f; x < Length / 2f; x += 80f)
			DrawLine(new Vector2(x, 0), new Vector2(x + 40f, 0), new Color(1, 1, 1, 0.35f), 3f);
		if (_flash > 0)
			DrawRect(lane, new Color(WarningColor, WarningColor.A * _flash));
	}
}
