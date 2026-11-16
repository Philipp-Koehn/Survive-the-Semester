using Godot;

// Erzeugt in regelmäßigen Abständen Gegner auf einem Kreis um den Spieler,
// außerhalb des sichtbaren Bereichs, aber immer innerhalb der Arena und nicht in Hindernissen.
// Der Gegnertyp wird zufällig aus einer Liste gewählt.
public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene[] EnemyScenes = new PackedScene[0]; // mögliche Gegnertypen
	[Export] public double SpawnInterval = 1.5;   // Sekunden zwischen zwei Spawns
	[Export] public float SpawnDistance = 700f;   // Abstand zum Spieler (außerhalb des Bildschirms)
	[Export] public int MaxEnemies = 50;          // Obergrenze gleichzeitig lebender Gegner

	private double _timeLeft;
	private Node2D _player;
	private readonly RandomNumberGenerator _rng = new();

	public override void _Ready()
	{
		_rng.Randomize();
		_timeLeft = SpawnInterval;
	}

	public override void _Process(double delta)
	{
		if (!EnsurePlayer()) return;

		_timeLeft -= delta;
		if (_timeLeft > 0) return;
		_timeLeft = SpawnInterval;

		if (GetTree().GetNodesInGroup("enemies").Count >= MaxEnemies) return;

		if (EnemyScenes.Length == 0)
		{
			GD.PushWarning("EnemySpawner: Keine Gegnertypen in EnemyScenes eingetragen!");
			return;
		}

		PackedScene scene = EnemyScenes[_rng.RandiRange(0, EnemyScenes.Length - 1)];
		SpawnScene(scene);
	}

	// Erzeugt einen Gegner der angegebenen Szene außerhalb des Bildschirms.
	// Wird auch vom WaveManager für Bosse genutzt (ohne MaxEnemies-Grenze).
	public Node2D SpawnScene(PackedScene scene)
	{
		if (scene == null || !EnsurePlayer()) return null;

		var enemy = scene.Instantiate<Node2D>();
		GetParent().AddChild(enemy);
		enemy.GlobalPosition = FindSpawnPosition();
		return enemy;
	}

	// Sucht einen Punkt im Abstand SpawnDistance zum Spieler, der in der Arena liegt
	// und nicht in einem Hindernis
	private Vector2 FindSpawnPosition()
	{
		var arena = GetTree().GetFirstNodeInGroup("arena") as ArenaFloor;
		const float margin = 100f; // Abstand zu den Wänden

		for (int i = 0; i < 20; i++)
		{
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			Vector2 pos = _player.GlobalPosition + Vector2.FromAngle(angle) * SpawnDistance;
			bool inArena = arena == null || arena.IsInside(pos, margin);
			if (inArena && !IsBlocked(pos))
				return pos;
		}

		// Kein passender Punkt gefunden (Spieler steht in einer Ecke): in die Arena zurückschieben
		Vector2 fallback = _player.GlobalPosition
						 + Vector2.FromAngle(_rng.RandfRange(0f, Mathf.Tau)) * SpawnDistance;
		return arena == null ? fallback : arena.ClampInside(fallback, margin);
	}

	// Prüft, ob an dieser Stelle ein Hindernis steht
	private bool IsBlocked(Vector2 pos)
	{
		var query = new PhysicsPointQueryParameters2D
		{
			Position = pos,
			CollideWithBodies = true,
			CollideWithAreas = false
		};
		foreach (var hit in GetWorld2D().DirectSpaceState.IntersectPoint(query))
		{
			if (hit["collider"].As<Node>() is Node node && node.IsInGroup("obstacles"))
				return true;
		}
		return false;
	}

	// Sucht den Spieler, falls er noch nicht bekannt ist
	private bool EnsurePlayer()
	{
		if (_player == null || !IsInstanceValid(_player))
			_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		return _player != null;
	}
}
