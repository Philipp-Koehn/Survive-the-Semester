using Godot;

// Tafel (Hörsaal): Deko an der oberen Wand ohne Kollision, damit sie keinen Bosskampf stört.
// Zeigt, was im Semester passiert. Dafür hört sie auf die Signale des WaveManagers:
// - BossWarning:      "<Boss> in 5 s!" und kurzes Aufblinken
// - BossSpawned:      Name des Bosses
// - ExamPhaseStarted: "Prüfungsphase"
// - SemesterFinished: "Bestanden!"
public partial class Blackboard : LevelElement
{
	[Export] public Vector2 Size = new(640, 44);
	[Export] public string DefaultText = "Vorlesung";
	[Export] public Color BoardColor = new(0.12f, 0.28f, 0.2f);
	[Export] public Color FrameColor = new(0.45f, 0.3f, 0.18f);
	[Export] public Color ChalkColor = new(0.92f, 0.92f, 0.88f);

	public string CurrentText => _label?.Text ?? "";

	private Label _label;
	private Tween _flash;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = ElementZ;

		_label = new Label
		{
			Text = DefaultText,
			Modulate = ChalkColor,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Size = Size,
			Position = -Size / 2f,
		};
		_label.AddThemeFontSizeOverride("font_size", 24);
		AddChild(_label);

		if (GetTree().CurrentScene?.GetNodeOrNull<WaveManager>("WaveManager") is WaveManager waves)
		{
			waves.BossWarning += OnBossWarning;
			waves.BossSpawned += OnBossSpawned;
			waves.ExamPhaseStarted += OnExamPhaseStarted;
			waves.SemesterFinished += OnSemesterFinished;
		}
	}

	private void OnBossWarning(string bossName, double seconds)
	{
		_label.Text = $"{bossName} in {seconds:0} s!";
		Flash(3);
		Trigger("tafel_warnung");
	}

	private void OnBossSpawned(Boss boss) => _label.Text = boss.BossName;
	private void OnExamPhaseStarted() { _label.Text = "Prüfungsphase"; Flash(2); }
	private void OnSemesterFinished() => _label.Text = "Bestanden!";

	// Kurz rot aufblinken lassen, danach wieder Kreide-Farbe
	private void Flash(int times)
	{
		_flash?.Kill();
		_flash = CreateTween();
		for (int i = 0; i < times; i++)
		{
			_flash.TweenProperty(_label, "modulate", new Color(1f, 0.4f, 0.35f), 0.15);
			_flash.TweenProperty(_label, "modulate", ChalkColor, 0.15);
		}
	}

	public override void _Draw()
	{
		var board = new Rect2(-Size / 2f, Size);
		DrawRect(board.Grow(6f), FrameColor);
		DrawRect(board, BoardColor);
	}
}
