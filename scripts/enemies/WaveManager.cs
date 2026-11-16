using Godot;
using System.Collections.Generic;

// Steuert den Ablauf des Semesters: Alle WeekDuration Sekunden beginnt eine neue Woche.
// Mit jeder Woche spawnen Gegner schneller, neue Gegnertypen kommen dazu,
// in bestimmten Wochen erscheinen Prüfungs-Bosse, und der Ort (Arena) wechselt nach Plan.
public partial class WaveManager : Node
{
	// Signale, z. B. für die UI (Fabian)
	[Signal] public delegate void WeekChangedEventHandler(int week);
	[Signal] public delegate void LocationChangedEventHandler(int index, string locationName);
	[Signal] public delegate void BossSpawnedEventHandler(Boss boss);
	[Signal] public delegate void SemesterFinishedEventHandler();

	[Export] public EnemySpawner Spawner;
	[Export] public PackedScene BasicEnemy;
	[Export] public PackedScene Deadline;
	[Export] public PackedScene HomeworkStack;
	[Export] public PackedScene MidtermBoss;      // Zwischenprüfung
	[Export] public PackedScene FinalBoss;        // Endklausur (letzte Woche)
	[Export] public int MidtermWeek = 7;

	[Export] public double WeekDuration = 30.0;   // Sekunden pro Woche
	[Export] public int TotalWeeks = 14;
	[Export] public double StartInterval = 1.5;   // Spawn-Intervall in Woche 1
	[Export] public double IntervalStep = 0.1;    // pro Woche so viel schneller
	[Export] public double MinInterval = 0.3;     // schneller geht es nicht

	// Orte im Semester: gleicher Index = gleicher Ort
	[Export] public string[] LocationNames = { "Gänge", "Outdoor-Campus", "Bibliothek", "Mensa", "Hörsaal" };
	[Export] public int[] LocationStartWeeks = { 1, 3, 6, 9, 12 };
	[Export] public string[] LocationArenaPaths =
	{
		"res://scenes/level/arena_gaenge.tscn",
		"res://scenes/level/arena_campus.tscn",
		"res://scenes/level/arena_bibliothek.tscn",
		"res://scenes/level/arena_mensa.tscn",
		"res://scenes/level/arena_hoersaal.tscn",
	};

	public int CurrentWeek { get; private set; }
	public int CurrentLocation { get; private set; } = -1;

	private double _timeLeft;
	private bool _finished;
	private bool _finalBossAlive;

	public override void _Ready()
	{
		if (Spawner == null)
		{
			GD.PushWarning("WaveManager: Kein Spawner im Inspektor zugewiesen!");
			_finished = true;
			return;
		}
		// Woche 1 startet im ersten Frame, wenn alle Nodes bereit sind
		CurrentWeek = 0;
		_timeLeft = 0;
	}

	public override void _Process(double delta)
	{
		if (_finished) return;

		_timeLeft -= delta;
		if (_timeLeft > 0) return;

		if (CurrentWeek < TotalWeeks)
		{
			StartWeek(CurrentWeek + 1);
		}
		else if (!_finalBossAlive)
		{
			// Nur falls keine Endklausur eingetragen ist: nach Ablauf der letzten Woche fertig
			FinishSemester();
		}
		// Sonst: warten, bis die Endklausur besiegt ist
	}

	private void StartWeek(int week)
	{
		CurrentWeek = week;
		_timeLeft = WeekDuration;

		// Erst den Ort prüfen, damit Gegner und Bosse schon in der neuen Arena entstehen
		int location = LocationForWeek(week);
		if (location != CurrentLocation)
			ChangeLocation(location);

		Spawner.SpawnInterval = Mathf.Max(MinInterval, StartInterval - (week - 1) * IntervalStep);
		Spawner.EnemyScenes = BuildEnemyList(week);

		GD.Print($"Woche {week} beginnt (Spawn alle {Spawner.SpawnInterval:0.00} s)");
		EmitSignal(SignalName.WeekChanged, week);

		if (week == MidtermWeek)
			SpawnBoss(MidtermBoss, false);
		if (week == TotalWeeks)
			SpawnBoss(FinalBoss, true);
	}

	// Letzter Ort, dessen Startwoche erreicht ist
	private int LocationForWeek(int week)
	{
		int result = 0;
		for (int i = 0; i < LocationStartWeeks.Length; i++)
		{
			if (week >= LocationStartWeeks[i])
				result = i;
		}
		return result;
	}

	// Ersetzt die aktuelle Arena durch die des neuen Ortes
	private void ChangeLocation(int index)
	{
		if (index < 0 || index >= LocationArenaPaths.Length) return;

		var scene = GD.Load<PackedScene>(LocationArenaPaths[index]);
		if (scene == null)
		{
			GD.PushWarning($"WaveManager: Arena nicht gefunden: {LocationArenaPaths[index]}");
			return;
		}

		// Alte Arena entfernen (sofort aus der Group, damit der Spawner sie nicht mehr findet)
		foreach (Node old in GetTree().GetNodesInGroup("arena"))
		{
			old.RemoveFromGroup("arena");
			old.QueueFree();
		}

		// Neue Arena ganz nach hinten in die Hauptszene setzen
		var arena = scene.Instantiate<ArenaFloor>();
		Node parent = GetParent();
		parent.AddChild(arena);
		parent.MoveChild(arena, 0);
		arena.Position = Vector2.Zero;

		// Spieler in die Mitte setzen
		if (GetTree().GetFirstNodeInGroup("player") is Node2D player)
			player.GlobalPosition = arena.GlobalPosition;

		// Normale Gegner entfernen, Bosse in die neue Arena schieben
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node.IsInGroup("bosses") && node is Node2D boss)
				boss.GlobalPosition = arena.ClampInside(boss.GlobalPosition, 150f);
			else
				node.QueueFree();
		}

		CurrentLocation = index;
		string locationName = index < LocationNames.Length ? LocationNames[index] : $"Ort {index + 1}";
		GD.Print($"Neuer Ort: {locationName}");
		EmitSignal(SignalName.LocationChanged, index, locationName);
	}

	// Welche Gegner in welcher Woche vorkommen. Mehrfach eingetragen = häufiger.
	private PackedScene[] BuildEnemyList(int week)
	{
		var list = new List<PackedScene> { BasicEnemy, BasicEnemy };
		if (week >= 3) list.Add(Deadline);
		if (week >= 5) list.Add(HomeworkStack);
		if (week >= 9)
		{
			list.Add(Deadline);
			list.Add(HomeworkStack);
		}
		return list.ToArray();
	}

	private void SpawnBoss(PackedScene scene, bool isFinal)
	{
		if (scene == null) return;

		var boss = Spawner.SpawnScene(scene) as Boss;
		if (boss == null) return;

		EmitSignal(SignalName.BossSpawned, boss);

		if (isFinal)
		{
			_finalBossAlive = true;
			boss.Died += OnFinalBossDied;
		}
	}

	private void OnFinalBossDied(Enemy enemy)
	{
		_finalBossAlive = false;
		FinishSemester();
	}

	private void FinishSemester()
	{
		if (_finished) return;
		_finished = true;
		Spawner.SetProcess(false); // keine neuen Gegner mehr
		GD.Print("Semester geschafft!");
		EmitSignal(SignalName.SemesterFinished);
	}
}
