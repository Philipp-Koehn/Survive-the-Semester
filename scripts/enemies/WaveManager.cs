using Godot;
using System.Collections.Generic;

// Steuert den Ablauf des Semesters: Alle WeekDuration Sekunden beginnt eine neue Woche.
// Mit jeder Woche spawnen Gegner schneller und werden zäher, neue Gegnertypen kommen dazu,
// in bestimmten Wochen erscheinen Zwischenprüfungen, und der Ort (Arena) wechselt nach Plan.
// Nach der letzten Woche beginnt die Prüfungsphase: Die Klausur-Bosse kommen nacheinander,
// der letzte ist der Endboss. Ist er besiegt, ist das Semester geschafft.
public partial class WaveManager : Node
{
	// Signale, z. B. für die UI (Fabian)
	[Signal] public delegate void WeekChangedEventHandler(int week);
	[Signal] public delegate void LocationChangedEventHandler(int index, string locationName);
	[Signal] public delegate void BossSpawnedEventHandler(Boss boss);
	[Signal] public delegate void BossWarningEventHandler(string bossName, double secondsUntilSpawn);
	[Signal] public delegate void ExamPhaseStartedEventHandler();
	[Signal] public delegate void SemesterFinishedEventHandler();

	[Export] public EnemySpawner Spawner;
	[Export] public PackedScene BasicEnemy;
	[Export] public PackedScene Deadline;
	[Export] public PackedScene HomeworkStack;
	[Export] public PackedScene FreeRider;        // Trittbrettfahrer
	[Export] public int FreeRiderWeek = 6;        // ab dieser Woche
	[Export] public PackedScene Tiredness;        // Müdigkeit
	[Export] public int TirednessWeek = 4;        // ab dieser Woche

	// Zwischenprüfungen: gleicher Index = gleiche Prüfung, erscheint zu Beginn der Woche
	[Export] public PackedScene[] MidtermBosses = new PackedScene[0];
	[Export] public int[] MidtermWeeks = { 5, 10 };

	// Prüfungsphase nach der letzten Woche: Bosse nacheinander, der letzte ist der Endboss
	[Export] public PackedScene[] ExamBosses = new PackedScene[0];
	[Export] public double ExamBossDelay = 5.0;          // Pause (mit Warnung) vor jedem Klausur-Boss
	[Export] public double ExamPhaseSpawnInterval = 0.8; // normale Gegner während der Prüfungsphase
	[Export] public double BossWarningTime = 5.0;        // so viele Sekunden vor einer Zwischenprüfung warnen

	// Test-Option: in dieser Woche starten (und damit am passenden Ort). 1 = normales Spiel.
	// Größer als TotalWeeks = direkt in die Prüfungsphase.
	[Export] public int TestStartWeek = 1;

	[Export] public double WeekDuration = 30.0;   // Sekunden pro Woche
	[Export] public int TotalWeeks = 14;
	[Export] public double StartInterval = 1.5;   // Spawn-Intervall in Woche 1
	[Export] public double IntervalStep = 0.1;    // pro Woche so viel schneller
	[Export] public double MinInterval = 0.3;     // schneller geht es nicht
	[Export] public float HealthGrowthPerWeek = 0.06f; // normale Gegner: +6 % Leben pro Woche

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
	public bool InExamPhase { get; private set; }

	private double _timeLeft;
	private bool _finished;
	private int _warnedWeek;       // für welche Woche schon vor dem Boss gewarnt wurde
	private int _examIndex = -1;   // aktueller Klausur-Boss
	private bool _waitingForExam;  // Pause vor dem nächsten Klausur-Boss läuft

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

		if (InExamPhase)
		{
			ProcessExamPhase(delta);
			return;
		}

		if (CurrentWeek == 0)
		{
			StartWeek(Mathf.Clamp(TestStartWeek, 1, TotalWeeks));
			if (TestStartWeek > 1)
				GD.Print($"[Test] Start in Woche {CurrentWeek} (TestStartWeek = {TestStartWeek})");
			if (TestStartWeek > TotalWeeks)
				StartExamPhase();
			return;
		}

		_timeLeft -= delta;
		CheckMidtermWarning();
		if (_timeLeft > 0) return;

		if (CurrentWeek < TotalWeeks)
			StartWeek(CurrentWeek + 1);
		else
			StartExamPhase();
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
		Spawner.HealthMultiplier = 1f + (week - 1) * HealthGrowthPerWeek;

		GD.Print($"Woche {week} beginnt (Spawn alle {Spawner.SpawnInterval:0.00} s, Gegner-Leben ×{Spawner.HealthMultiplier:0.00})");
		EmitSignal(SignalName.WeekChanged, week);

		PackedScene midterm = MidtermForWeek(week);
		if (midterm != null)
			SpawnBoss(midterm);
	}

	private PackedScene MidtermForWeek(int week)
	{
		for (int i = 0; i < MidtermWeeks.Length && i < MidtermBosses.Length; i++)
		{
			if (MidtermWeeks[i] == week)
				return MidtermBosses[i];
		}
		return null;
	}

	// Kurz vor einer Woche mit Zwischenprüfung warnen (z. B. für eine Anzeige in der UI)
	private void CheckMidtermWarning()
	{
		int next = CurrentWeek + 1;
		if (CurrentWeek < 1 || next > TotalWeeks || _warnedWeek == next || _timeLeft > BossWarningTime)
			return;

		_warnedWeek = next;
		PackedScene midterm = MidtermForWeek(next);
		if (midterm != null)
			WarnBoss(midterm, Mathf.Max(_timeLeft, 0));
	}

	// ---------- Prüfungsphase ----------

	private void StartExamPhase()
	{
		InExamPhase = true;
		Spawner.SpawnInterval = ExamPhaseSpawnInterval; // etwas Luft für die Bosskämpfe
		GD.Print("Prüfungsphase beginnt!");
		EmitSignal(SignalName.ExamPhaseStarted);
		QueueNextExam();
	}

	private void QueueNextExam()
	{
		_examIndex++;
		if (_examIndex >= ExamBosses.Length)
		{
			FinishSemester(); // alle Klausuren bestanden (oder keine eingetragen)
			return;
		}

		_waitingForExam = true;
		_timeLeft = ExamBossDelay;
		WarnBoss(ExamBosses[_examIndex], ExamBossDelay);
	}

	private void ProcessExamPhase(double delta)
	{
		if (!_waitingForExam) return; // ein Klausur-Boss lebt, warten auf sein Died

		_timeLeft -= delta;
		if (_timeLeft > 0) return;

		_waitingForExam = false;
		Boss boss = SpawnBoss(ExamBosses[_examIndex]);
		if (boss == null)
			QueueNextExam(); // Szene fehlt: überspringen statt hängen zu bleiben
		else
			boss.Died += _ => QueueNextExam();
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

		// Geschosse und Bodenangriffe der Bosse entfernen
		foreach (Node attack in GetTree().GetNodesInGroup("enemy_attacks"))
			attack.QueueFree();

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
		if (week >= TirednessWeek) list.Add(Tiredness);
		if (week >= 5) list.Add(HomeworkStack);
		if (week >= FreeRiderWeek) list.Add(FreeRider);
		if (week >= 9)
		{
			list.Add(Deadline);
			list.Add(HomeworkStack);
		}
		list.RemoveAll(scene => scene == null); // nicht zugewiesene Typen überspringen
		return list.ToArray();
	}

	private Boss SpawnBoss(PackedScene scene)
	{
		if (scene == null) return null;

		var boss = Spawner.SpawnScene(scene) as Boss;
		if (boss == null) return null;

		EmitSignal(SignalName.BossSpawned, boss);
		return boss;
	}

	private void WarnBoss(PackedScene scene, double seconds)
	{
		string name = BossNameOf(scene);
		GD.Print($"Achtung: {name} in {seconds:0} s!");
		EmitSignal(SignalName.BossWarning, name, seconds);
	}

	// Liest BossName aus der Szene, ohne sie zu erzeugen (auch aus geerbten Szenen)
	private static string BossNameOf(PackedScene scene)
	{
		while (scene != null)
		{
			SceneState state = scene.GetState();
			for (int i = 0; i < state.GetNodePropertyCount(0); i++)
			{
				if (state.GetNodePropertyName(0, i) == "BossName")
					return state.GetNodePropertyValue(0, i).AsString();
			}
			scene = state.GetNodeInstance(0);
		}
		return "Prüfung";
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
