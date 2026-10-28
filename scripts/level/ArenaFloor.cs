using Godot;

// Platzhalter-Boden für die Arena: zeichnet ein Schachbrettmuster mit Rand.
// Wird später durch die richtige Arena-Grafik ersetzt.
public partial class ArenaFloor : Node2D
{
	[Export] public Vector2I ArenaSize = new(4000, 4000); // Größe der Arena in Pixeln
	[Export] public int TileSize = 64;                    // Größe eines Feldes
	[Export] public Color ColorA = new(0.22f, 0.24f, 0.28f);
	[Export] public Color ColorB = new(0.27f, 0.29f, 0.33f);
	[Export] public Color BorderColor = new(0.8f, 0.3f, 0.3f);

	public override void _Ready()
	{
		ZIndex = -10; // immer hinter Spieler und Gegnern
		QueueRedraw();
	}

	public override void _Draw()
	{
		Vector2 size = ArenaSize;
		Vector2 origin = -size / 2f; // Arena ist um den Nullpunkt zentriert

		int cols = ArenaSize.X / TileSize;
		int rows = ArenaSize.Y / TileSize;

		for (int x = 0; x < cols; x++)
		{
			for (int y = 0; y < rows; y++)
			{
				Color color = (x + y) % 2 == 0 ? ColorA : ColorB;
				var tile = new Rect2(origin + new Vector2(x * TileSize, y * TileSize),
									 new Vector2(TileSize, TileSize));
				DrawRect(tile, color);
			}
		}

		// Rand der Arena
		DrawRect(new Rect2(origin, size), BorderColor, false, 8f);
	}
}
