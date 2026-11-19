using Godot;

// Rechteckiger Bereich, in dem der EnemySpawner keine Gegner erzeugt (z. B. Nebenräume).
// Unsichtbar, kostet zur Laufzeit nichts: Der Spawner fragt nur beim Spawnen nach.
public partial class NoSpawnZone : Node2D
{
	[Export] public Vector2 Size = new(256, 256);

	public Rect2 GlobalRect => new(GlobalPosition - Size / 2f, Size);

	public override void _Ready()
	{
		AddToGroup("no_spawn");
	}

	public bool Contains(Vector2 point)
	{
		return GlobalRect.HasPoint(point);
	}
}
