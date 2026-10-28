using Godot;

// Erzeugt in regelmäßigen Abständen Gegner auf einem Kreis um den Spieler,
// außerhalb des sichtbaren Bereichs. Der Gegnertyp wird zufällig aus einer Liste gewählt.
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
		if (_player == null || !IsInstanceValid(_player))
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (_player == null) return;
		}

		_timeLeft -= delta;
		if (_timeLeft > 0) return;
		_timeLeft = SpawnInterval;

		if (GetTree().GetNodesInGroup("enemies").Count >= MaxEnemies) return;

		SpawnEnemy();
	}

	private void SpawnEnemy()
	{
		if (EnemyScenes.Length == 0)
		{
			GD.PushWarning("EnemySpawner: Keine Gegnertypen in EnemyScenes eingetragen!");
			return;
		}

		// Zufälligen Gegnertyp aus der Liste wählen
		PackedScene scene = EnemyScenes[_rng.RandiRange(0, EnemyScenes.Length - 1)];
		if (scene == null) return;

		var enemy = scene.Instantiate<Node2D>();
		GetParent().AddChild(enemy);

		float angle = _rng.RandfRange(0f, Mathf.Tau);
		enemy.GlobalPosition = _player.GlobalPosition + Vector2.FromAngle(angle) * SpawnDistance;
	}
}
