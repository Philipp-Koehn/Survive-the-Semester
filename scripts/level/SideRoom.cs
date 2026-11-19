using Godot;

// Nebenraum (z. B. Seminarraum): vier Wände mit einer automatischen Tür an einer Seite.
// - Wände und Boden werden per Code erzeugt und gezeichnet ([Tool]: im Editor sichtbar).
// - Im Raum spawnen keine Gegner (NoSpawnZone).
// - Gegner laufen direkt auf den Spieler zu und würden an den Raumwänden hängen bleiben.
//   Deshalb bekommen sie einen Wegpunkt an der Tür, solange Spieler und Gegner in
//   verschiedenen Räumen sind (alle WaypointUpdateInterval Sekunden aktualisiert).
// - Die Tür bleibt offen, solange der Spieler oder ein Gegner im Raum ist.
// Weitere Inhalte (z. B. eine Kiste) werden als Kinder in der Arena-Szene hineingesetzt.
[Tool]
public partial class SideRoom : Node2D
{
	public enum Side { Left, Right, Top, Bottom }

	private Vector2 _innerSize = new(300, 760);
	private Side _doorSide = Side.Right;

	[Export] public Vector2 InnerSize { get => _innerSize; set { _innerSize = value; QueueRedraw(); } }
	[Export] public Side DoorSide { get => _doorSide; set { _doorSide = value; QueueRedraw(); } }
	[Export] public float WallThickness = 32f;
	[Export] public float DoorWidth = 192f;
	[Export] public Color WallColor = new(0.2f, 0.22f, 0.26f);
	[Export] public Color FloorColor = new(0.36f, 0.33f, 0.3f);
	[Export] public float DoorOpenDistance = 200f;
	[Export] public double DoorCloseDelay = 1.0;
	[Export] public double WaypointUpdateInterval = 0.25;

	public AutoDoor Door { get; private set; }
	public bool PlayerInside { get; private set; }

	private Rect2 Inner => new(-InnerSize / 2f, InnerSize);
	private double _updateLeft;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = LevelElement.RoomFloorZ;
		QueueRedraw();
		if (Engine.IsEditorHint()) return;

		// Wände
		var walls = new StaticBody2D { Name = "Walls" };
		walls.AddToGroup("obstacles");
		foreach (Rect2 r in GetWallRects())
			walls.AddChild(new CollisionShape2D { Position = r.GetCenter(), Shape = new RectangleShape2D { Size = r.Size } });
		AddChild(walls);

		// Tür in der Lücke
		bool vertical = DoorSide == Side.Left || DoorSide == Side.Right;
		Door = new AutoDoor
		{
			Name = "Door",
			Size = vertical ? new Vector2(WallThickness, DoorWidth) : new Vector2(DoorWidth, WallThickness),
			OpenDistance = DoorOpenDistance,
			CloseDelay = DoorCloseDelay,
			Position = DoorCenterLocal(),
		};
		AddChild(Door);

		// Kein Spawn im Raum und direkt an seinen Wänden
		AddChild(new NoSpawnZone { Size = Inner.Grow(WallThickness + 60f).Size });
	}

	private Vector2 Outward => DoorSide switch
	{
		Side.Left => Vector2.Left,
		Side.Right => Vector2.Right,
		Side.Top => Vector2.Up,
		_ => Vector2.Down,
	};

	private Vector2 DoorCenterLocal()
	{
		Vector2 half = InnerSize / 2f + new Vector2(WallThickness / 2f, WallThickness / 2f);
		return new Vector2(Outward.X * half.X, Outward.Y * half.Y);
	}

	// Vier Wände, die Türseite mit Lücke in der Mitte (lokal)
	private Rect2[] GetWallRects()
	{
		Rect2 inner = Inner;
		Rect2 outer = inner.Grow(WallThickness);
		float t = WallThickness;
		var top = new Rect2(outer.Position.X, outer.Position.Y, outer.Size.X, t);
		var bottom = new Rect2(outer.Position.X, inner.End.Y, outer.Size.X, t);
		var left = new Rect2(outer.Position.X, inner.Position.Y, t, inner.Size.Y);
		var right = new Rect2(inner.End.X, inner.Position.Y, t, inner.Size.Y);

		var list = new System.Collections.Generic.List<Rect2>();
		foreach (var (rect, side) in new[] { (top, Side.Top), (bottom, Side.Bottom), (left, Side.Left), (right, Side.Right) })
		{
			if (side == DoorSide) list.AddRange(SplitForDoor(rect));
			else list.Add(rect);
		}
		return list.ToArray();
	}

	private Rect2[] SplitForDoor(Rect2 wall)
	{
		float gap = DoorWidth / 2f;
		if (wall.Size.X > wall.Size.Y) // waagerechte Wand, Lücke bei x = 0
		{
			return new[]
			{
				new Rect2(wall.Position.X, wall.Position.Y, -gap - wall.Position.X, wall.Size.Y),
				new Rect2(gap, wall.Position.Y, wall.End.X - gap, wall.Size.Y),
			};
		}
		return new[] // senkrechte Wand, Lücke bei y = 0
		{
			new Rect2(wall.Position.X, wall.Position.Y, wall.Size.X, -gap - wall.Position.Y),
			new Rect2(wall.Position.X, gap, wall.Size.X, wall.End.Y - gap),
		};
	}

	public override void _Draw()
	{
		DrawRect(Inner, FloorColor);
		foreach (Rect2 r in GetWallRects())
			DrawRect(r, WallColor);
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint()) return;

		_updateLeft -= delta;
		if (_updateLeft > 0) return;
		_updateLeft = WaypointUpdateInterval;
		UpdateWaypoints();
	}

	private void UpdateWaypoints()
	{
		if (GetTree().GetFirstNodeInGroup("player") is not Node2D player) return;

		var inner = new Rect2(GlobalPosition + Inner.Position, InnerSize);
		PlayerInside = inner.HasPoint(player.GlobalPosition);

		// Punkte vor und hinter der Tür und der "Kanal" durch die Tür
		Vector2 door = Door.GlobalPosition;
		float half = WallThickness / 2f;
		Vector2 insidePoint = door - Outward * (half + 90f);
		Vector2 outsidePoint = door + Outward * (half + 100f);
		// Kanal: von 110 px innen bis 120 px außen, quer dazu fast so breit wie die Tür
		Vector2 across = Outward.Orthogonal() * (DoorWidth / 2f - 20f);
		Rect2 channel = new Rect2(door - Outward * (half + 110f) - across, Vector2.Zero)
			.Expand(door + Outward * (half + 120f) + across);

		int enemiesInside = 0;
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is not Enemy enemy || enemy is Boss) continue;

			bool inside = inner.HasPoint(enemy.GlobalPosition);
			if (inside) enemiesInside++;

			if (inside == PlayerInside)
				enemy.Waypoint = null; // gleicher Raum: direkt verfolgen
			else if (PlayerInside)
				enemy.Waypoint = channel.HasPoint(enemy.GlobalPosition) ? insidePoint : outsidePoint; // hinein
			else
				enemy.Waypoint = channel.HasPoint(enemy.GlobalPosition) ? outsidePoint : insidePoint; // hinaus
		}

		Door.KeepOpen = PlayerInside || enemiesInside > 0;
	}

	public override void _ExitTree()
	{
		if (Engine.IsEditorHint()) return;
		// Wegpunkte nicht zurücklassen (z. B. falls ein Boss den Ortswechsel überlebt)
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
			if (node is Enemy enemy) enemy.Waypoint = null;
	}
}
