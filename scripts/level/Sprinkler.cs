using Godot;

// Rasensprenger (Outdoor-Campus): kleiner fester Sprengerkopf in der Mitte, um ihn ein Kreis aus
// nassem Gras (SlowZone, Gegner langsamer). Ein Wasserstrahl dreht sich langsam im Kreis.
// Animation nur über die Drehung eines Kind-Knotens (kein Neuzeichnen pro Frame).
public partial class Sprinkler : SlowZone
{
	[Export] public float RotationSpeed = 1.6f;   // Umdrehungen in Radiant pro Sekunde
	[Export] public Color WaterColor = new(0.6f, 0.85f, 1f, 0.8f);

	private Node2D _beam;

	public override void _Ready()
	{
		Shape = ZoneShape.Circle;
		base._Ready();

		// Sprengerkopf als kleines Hindernis
		var head = new StaticBody2D();
		head.AddToGroup("obstacles");
		head.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 14f } });
		AddChild(head);

		// Wasserstrahl: zwei gegenüberliegende Linien
		_beam = new Node2D();
		foreach (float sign in new[] { 1f, -1f })
		{
			_beam.AddChild(new Line2D
			{
				Points = new[] { new Vector2(sign * 16f, 0), new Vector2(sign * (Radius - 10f), 0) },
				Width = 6f,
				DefaultColor = WaterColor,
				BeginCapMode = Line2D.LineCapMode.Round,
				EndCapMode = Line2D.LineCapMode.Round,
			});
		}
		AddChild(_beam);
	}

	public override void _Process(double delta)
	{
		_beam.Rotation += RotationSpeed * (float)delta;
	}

	public override void _Draw()
	{
		base._Draw();
		DrawCircle(Vector2.Zero, 14f, new Color(0.35f, 0.35f, 0.38f));
		DrawCircle(Vector2.Zero, 6f, new Color(0.6f, 0.85f, 1f));
	}
}
