using Godot;

// Platzhalter-Hindernis: ein Rechteck mit Kollision, Größe in Kacheln.
// [Tool] sorgt dafür, dass man es schon im Editor sieht und platzieren kann.
// Später wird das Rechteck durch die Grafik von Fabian ersetzt.
[Tool]
public partial class Obstacle : StaticBody2D
{
	private Vector2I _sizeInTiles = new(1, 1);
	private Color _color = new(0.45f, 0.5f, 0.58f);

	[Export] public Vector2I SizeInTiles
	{
		get => _sizeInTiles;
		set { _sizeInTiles = value; QueueRedraw(); }
	}

	[Export] public int TileSize = 64;

	[Export] public Color Color
	{
		get => _color;
		set { _color = value; QueueRedraw(); }
	}

	public Vector2 PixelSize => (Vector2)SizeInTiles * TileSize;

	public override void _Ready()
	{
		QueueRedraw();

		// Im Editor nur zeichnen, keine Kollision anlegen (sonst würde sie mitgespeichert)
		if (Engine.IsEditorHint()) return;

		AddToGroup("obstacles");
		AddChild(new CollisionShape2D
		{
			Shape = new RectangleShape2D { Size = PixelSize }
		});
	}

	public override void _Draw()
	{
		var rect = new Rect2(-PixelSize / 2f, PixelSize);
		DrawRect(rect, Color);
		DrawRect(rect, Color.Darkened(0.4f), false, 3f); // dunkler Rand
	}
}
