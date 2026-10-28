using Godot;

// Erzeugt in regelmäßigen Abständen Gegner auf einem Kreis um den Spieler,
// außerhalb des sichtbaren Bereichs. Grundlage für das spätere Wellensystem.
public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene;
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
		// Spieler suchen, falls er noch fehlt (gleiches Prinzip wie beim Gegner)
		if (_player == null || !IsInstanceValid(_player))
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (_player == null) return;
		}

		_timeLeft -= delta;
		if (_timeLeft > 0) return;
		_timeLeft = SpawnInterval;

		// Nicht unendlich viele Gegner erzeugen (Performance)
		if (GetTree().GetNodesInGroup("enemies").Count >= MaxEnemies) return;

		SpawnEnemy();
	}

	private void SpawnEnemy()
	{
		if (EnemyScene == null)
		{
			GD.PushWarning("EnemySpawner: Keine EnemyScene im Inspektor gesetzt!");
			return;
		}

		var enemy = EnemyScene.Instantiate<Node2D>();
		GetParent().AddChild(enemy);

		// Zufälliger Punkt auf einem Kreis um den Spieler
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		enemy.GlobalPosition = _player.GlobalPosition + Vector2.FromAngle(angle) * SpawnDistance;
	}
}
