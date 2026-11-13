using Godot;

// Arena: zeichnet den Boden (Platzhalter-Schachbrett) und erzeugt Wände am Rand,
// damit Spieler und Gegner die Arena nicht verlassen können.
public partial class ArenaFloor : Node2D
{
	[Export] public Vector2I ArenaSize = new(4000, 4000); // Größe der Arena in Pixeln
	[Export] public int TileSize = 64;                    // Größe eines Feldes
	[Export] public float WallThickness = 64f;            // Dicke der Wände
	[Export] public Color ColorA = new(0.22f, 0.24f, 0.28f);
	[Export] public Color ColorB = new(0.27f, 0.29f, 0.33f);
	[Export] public Color BorderColor = new(0.8f, 0.3f, 0.3f);

	// Bereich der Arena in Weltkoordinaten
	public Rect2 Bounds => new Rect2(GlobalPosition - (Vector2)ArenaSize / 2f, ArenaSize);

	public override void _Ready()
	{
		ZIndex = -10; // immer hinter Spieler und Gegnern
		AddToGroup("arena");
		CreateWalls();
		QueueRedraw();
	}

	// Prüft, ob ein Punkt (mit Abstand zum Rand) in der Arena liegt
	public bool IsInside(Vector2 point, float margin)
	{
		return Bounds.Grow(-margin).HasPoint(point);
	}

	// Schiebt einen Punkt in die Arena zurück
	public Vector2 ClampInside(Vector2 point, float margin)
	{
		Rect2 inner = Bounds.Grow(-margin);
		return point.Clamp(inner.Position, inner.End);
	}

	// Die vier Wand-Rechtecke (lokal), genutzt für Kollision UND Zeichnen
	private Rect2[] GetWallRects()
	{
		Vector2 size = ArenaSize;
		Vector2 o = -size / 2f;
		float t = WallThickness;
		return new[]
		{
			new Rect2(o.X - t, o.Y - t, size.X + 2 * t, t),        // oben
			new Rect2(o.X - t, o.Y + size.Y, size.X + 2 * t, t),   // unten
			new Rect2(o.X - t, o.Y, t, size.Y),                    // links
			new Rect2(o.X + size.X, o.Y, t, size.Y),               // rechts
		};
	}

	private void CreateWalls()
	{
		var walls = new StaticBody2D { Name = "Walls" };
		AddChild(walls);

		foreach (Rect2 r in GetWallRects())
		{
			walls.AddChild(new CollisionShape2D
			{
				Position = r.GetCenter(),
				Shape = new RectangleShape2D { Size = r.Size }
			});
		}
	}

	public override void _Draw()
	{
		Vector2 size = ArenaSize;
		Vector2 origin = -size / 2f; // Arena ist um den Nullpunkt zentriert
		var area = new Rect2(origin, size);

		// aufrunden, damit auch angeschnittene Kacheln am Rand gezeichnet werden
		int cols = Mathf.CeilToInt(size.X / TileSize);
		int rows = Mathf.CeilToInt(size.Y / TileSize);

		for (int x = 0; x < cols; x++)
		{
			for (int y = 0; y < rows; y++)
			{
				Color color = (x + y) % 2 == 0 ? ColorA : ColorB;
				var tile = new Rect2(origin + new Vector2(x * TileSize, y * TileSize),
									 new Vector2(TileSize, TileSize));
				DrawRect(tile.Intersection(area), color); // am Rand abschneiden
			}
		}

		// Wände genau dort zeichnen, wo die Kollision ist
		foreach (Rect2 r in GetWallRects())
			DrawRect(r, BorderColor);
	}
}
