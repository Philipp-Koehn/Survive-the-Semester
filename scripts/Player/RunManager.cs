using Godot;

/// <summary>
/// Ends the run when the player dies: pauses the game, waits a moment, then leaves for the main menu.
/// Pausing stops input, weapons, enemies and the enemy spawner at once.
/// A new run starts when the game scene is loaded again, so everything begins from its start values.
/// </summary>
/// <example>
/// Set <see cref="MainMenuScenePath"/> in the Inspector once the menu scene exists.
/// Until then the run restarts after a warning.
/// </example>
public partial class RunManager : Node
{
    /// <summary>Scene to load after death. Empty until the main menu exists.</summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string MainMenuScenePath { get; set; } = "";

    /// <summary>Seconds between the death and leaving the scene.</summary>
    [Export]
    public double LeaveDelay { get; set; } = 2.0;

    /// <summary>Credits per level reached at the end of the run (story 008).</summary>
    [Export]
    public int CreditsPerLevel { get; set; } = 5;

    /// <summary>Credits per week reached at the end of the run.</summary>
    [Export]
    public int CreditsPerWeek { get; set; } = 10;

    /// <summary>Credits per defeated boss.</summary>
    [Export]
    public int CreditsPerBoss { get; set; } = 25;

    private ExperienceSystem _experience;
    private WaveManager _waves;
    private int _bossesDefeated;
    private bool _awarded;
    private bool _warnedNoWaveManager;

    /// <inheritdoc/>
    public override void _Ready()
    {
        // The pause must not stop this node, or the delay could never finish.
        ProcessMode = ProcessModeEnum.Always;
        var stats = GetNode<PlayerStats>("../PlayerStats");
        stats.Died += OnPlayerDied;
        _experience = GetNode<ExperienceSystem>("../ExperienceSystem");

        // Run start: the bought passive upgrades go onto the stats. The starter weapon is placed by the WeaponInventory.
        MetaProgress.Instance.ApplyPassivesTo(stats);

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
            {
                Connect(enemy);
            }
        }

        GetTree().NodeAdded += OnNodeAdded;

        // The WaveManager is a sibling of the player and may not be ready yet.
        CallDeferred(MethodName.ConnectWaveManager);
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnNodeAdded;
    }

    private void OnNodeAdded(Node node)
    {
        if (node is Enemy enemy)
        {
            Connect(enemy);
        }
    }

    private void Connect(Enemy enemy)
    {
        Callable callable = Callable.From<Enemy>(OnEnemyDied);
        if (!enemy.IsConnected(Enemy.SignalName.Died, callable))
        {
            enemy.Died += OnEnemyDied;
        }
    }

    private void ConnectWaveManager()
    {
        _waves = GetTree().CurrentScene?.GetNodeOrNull<WaveManager>("WaveManager");
        if (_waves == null)
        {
            if (!_warnedNoWaveManager)
            {
                _warnedNoWaveManager = true;
                GD.PushWarning("RunManager: kein WaveManager gefunden, Woche zählt als 0.");
            }

            return;
        }

        _waves.SemesterFinished += AwardCredits;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (!_awarded && enemy.IsInGroup("bosses"))
        {
            _bossesDefeated++;
        }
    }

    /// <summary>Gives the credits for level, week and bosses once per run and saves them.</summary>
    private void AwardCredits()
    {
        if (_awarded)
        {
            return;
        }

        _awarded = true;
        int week = _waves?.CurrentWeek ?? 0;
        int credits = _experience.Level * CreditsPerLevel + week * CreditsPerWeek + _bossesDefeated * CreditsPerBoss;
        MetaProgress.Instance.AddCredits(credits);
        GD.Print($"[Meta] Run-Ende: +{credits} Credits (Level {_experience.Level}, Woche {week}, Bosse {_bossesDefeated}), gesamt {MetaProgress.Instance.Credits}");
    }

    private async void OnPlayerDied()
    {
        AwardCredits();
        GD.Print($"Run beendet. Zurück zum Hauptmenü in {LeaveDelay:0.#} s.");
        GetTree().Paused = true;

        await ToSignal(GetTree().CreateTimer(LeaveDelay, processAlways: true), SceneTreeTimer.SignalName.Timeout);

        // The pause outlives a scene change, so it must be lifted first.
        GetTree().Paused = false;

        if (MainMenuScenePath != "" && ResourceLoader.Exists(MainMenuScenePath))
        {
            Error error = GetTree().ChangeSceneToFile(MainMenuScenePath);
            if (error == Error.Ok)
            {
                return;
            }

            GD.PushWarning($"Hauptmenü '{MainMenuScenePath}' konnte nicht geladen werden ({error}). Der Run startet neu.");
        }
        else
        {
            GD.PushWarning($"Keine Hauptmenü-Szene gefunden (Pfad: '{MainMenuScenePath}'). Setze MainMenuScenePath am RunManager. Der Run startet neu.");
        }

        GetTree().ReloadCurrentScene();
    }
}
