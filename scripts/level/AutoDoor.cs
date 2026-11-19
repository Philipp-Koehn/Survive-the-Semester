using Godot;

// Automatische Schiebetür: Zwei Flügel gleiten auseinander, sobald der Spieler näher als
// OpenDistance ist. Sie schließt erst, wenn der Spieler weg ist, niemand mehr in der Tür steht
// und der Raum sie nicht offen hält (KeepOpen). So wird niemand eingeklemmt oder eingesperrt.
// Wird normalerweise von SideRoom erzeugt.
public partial class AutoDoor : LevelElement
{
	[Export] public Vector2 Size = new(32, 192);   // Türöffnung (Dicke × Breite oder Breite × Dicke)
	[Export] public float OpenDistance = 200f;
	[Export] public double CloseDelay = 1.0;       // so lange bleibt sie nach dem Weggehen noch offen
	[Export] public double AnimationTime = 0.3;
	[Export] public Color Color = new(0.55f, 0.7f, 0.8f);

	// Vom Raum gesetzt: Spieler oder Gegner sind im Raum, Tür nicht schließen
	public bool KeepOpen { get; set; }
	public bool IsOpen => _state != State.Closed;

	private enum State { Closed, Opening, Open, Closing }

	private const double CheckInterval = 0.1;      // nicht jeden Frame prüfen
	private State _state = State.Closed;
	private double _checkLeft;
	private double _closeLeft;
	private int _bodiesInDoorway;
	private CollisionShape2D _shape;
	private Polygon2D _panelA, _panelB;
	private Vector2 _slideAxis;                    // Richtung, in die Flügel A aufgeht
	private Tween _tween;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = ElementZ;

		bool vertical = Size.Y >= Size.X;          // Flügel gleiten entlang der langen Seite
		_slideAxis = vertical ? Vector2.Down : Vector2.Right;
		Vector2 panelSize = vertical ? new Vector2(Size.X, Size.Y / 2f) : new Vector2(Size.X / 2f, Size.Y);

		// Kollision der geschlossenen Tür
		var body = new StaticBody2D();
		_shape = new CollisionShape2D { Shape = new RectangleShape2D { Size = Size } };
		body.AddChild(_shape);
		AddChild(body);

		// Türbereich: zählt Spieler und Gegner, die gerade in der Tür stehen
		var doorway = new Area2D { Monitorable = false };
		Vector2 grow = vertical ? new Vector2(80, 0) : new Vector2(0, 80);
		doorway.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = Size + grow } });
		doorway.BodyEntered += b => { if (b is CharacterBody2D) _bodiesInDoorway++; };
		doorway.BodyExited += b => { if (b is CharacterBody2D) _bodiesInDoorway--; };
		AddChild(doorway);

		_panelA = CreatePanel(panelSize);
		_panelB = CreatePanel(panelSize);
		_panelA.Position = ClosedOffset(+1);
		_panelB.Position = ClosedOffset(-1);
	}

	private Polygon2D CreatePanel(Vector2 size)
	{
		Vector2 h = size / 2f;
		var panel = new Polygon2D
		{
			Color = Color,
			Polygon = new[] { new Vector2(-h.X, -h.Y), new Vector2(h.X, -h.Y), new Vector2(h.X, h.Y), new Vector2(-h.X, h.Y) },
		};
		AddChild(panel);
		return panel;
	}

	// Flügelmitte geschlossen: ein Viertel der Breite von der Mitte
	private Vector2 ClosedOffset(int sign) => _slideAxis * sign * LongSide() / 4f;

	// Flügelmitte offen: in die Wand geschoben
	private Vector2 OpenOffset(int sign) => _slideAxis * sign * LongSide() * 0.75f;

	private float LongSide() => Mathf.Max(Size.X, Size.Y);

	public override void _Process(double delta)
	{
		_checkLeft -= delta;
		if (_checkLeft > 0) return;
		_checkLeft = CheckInterval;

		Node2D player = FindPlayer();
		bool playerNear = player != null && player.GlobalPosition.DistanceTo(GlobalPosition) < OpenDistance;
		bool wantOpen = playerNear || KeepOpen || _bodiesInDoorway > 0;

		if (wantOpen)
		{
			_closeLeft = CloseDelay;
			if (_state == State.Closed || _state == State.Closing)
				Open();
		}
		else if (_state == State.Open)
		{
			_closeLeft -= CheckInterval;
			if (_closeLeft <= 0)
				Close();
		}
	}

	private void Open()
	{
		bool wasClosed = _state == State.Closed;
		_state = State.Opening;
		_shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		AnimatePanels(OpenOffset(+1), OpenOffset(-1), () => _state = State.Open);
		if (wasClosed) Trigger("tuer_auf");
	}

	private void Close()
	{
		_state = State.Closing;
		AnimatePanels(ClosedOffset(+1), ClosedOffset(-1), () =>
		{
			if (_bodiesInDoorway > 0) // jemand ist in letzter Sekunde hineingelaufen
			{
				Open();
				return;
			}
			_state = State.Closed;
			_shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
			Trigger("tuer_zu");
		});
	}

	private void AnimatePanels(Vector2 a, Vector2 b, System.Action done)
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_panelA, "position", a, AnimationTime);
		_tween.TweenProperty(_panelB, "position", b, AnimationTime);
		_tween.Chain().TweenCallback(Callable.From(done));
	}
}
