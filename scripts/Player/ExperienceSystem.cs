using Godot;

/// <summary>
/// The player's XP and level (story 007, design/game-brief.md "Spieler").
/// Every defeated enemy gives XP that grows with the week of the semester, bosses give a bonus.
/// A level-up starts the upgrade choice; several level-ups at once are offered one after another.
/// Enemies are found by listening to <c>SceneTree.NodeAdded</c>, so no enemy file needs to know about this node.
/// A new run is a scene reload, so everything starts at level 1 with 0 XP.
/// </summary>
/// <example>
/// The HUD connects to <see cref="XpChanged"/> and <see cref="LeveledUp"/> and reads <see cref="Level"/>, <see cref="Xp"/>, <see cref="XpNeeded"/>.
/// </example>
public partial class ExperienceSystem : Node
{
    /// <summary>Raised whenever the XP or the level changed.</summary>
    /// <param name="xp">XP collected towards the next level.</param>
    /// <param name="xpNeeded">XP needed for the next level.</param>
    /// <param name="level">Current level.</param>
    [Signal]
    public delegate void XpChangedEventHandler(int xp, int xpNeeded, int level);

    /// <summary>Raised for every level gained, with the new level.</summary>
    [Signal]
    public delegate void LeveledUpEventHandler(int level);

    /// <summary>XP per defeated enemy by week (entry 0 = week 1). Weeks after the last entry use the last entry.</summary>
    [Export]
    public int[] XpPerKillByWeek { get; set; } = { 5, 5, 6, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

    /// <summary>Extra XP when the defeated enemy is in the group "bosses".</summary>
    [Export]
    public int BossBonusXp { get; set; } = 100;

    /// <summary>XP needed to go from level 1 to level 2.</summary>
    [Export]
    public int XpForLevel2 { get; set; } = 120;

    /// <summary>Each further level needs this much more XP than the one before.</summary>
    [Export]
    public int XpGrowthPerLevel { get; set; } = 20;

    /// <summary>Current level, starts at 1.</summary>
    public int Level { get; private set; } = 1;

    /// <summary>XP collected towards the next level.</summary>
    public int Xp { get; private set; }

    /// <summary>XP needed for the next level.</summary>
    public int XpNeeded => XpForLevel2 + XpGrowthPerLevel * (Level - 1);

    private int _week = 1;
    private int _pendingLevelUps;
    private bool _warnedNoWaveManager;
    private PlayerStats _stats;
    private UpgradeManager _upgrades;

    /// <inheritdoc/>
    public override void _Ready()
    {
        // Must keep dispatching while the tree is paused by the choice itself.
        ProcessMode = ProcessModeEnum.Always;
        _stats = GetNode<PlayerStats>("../PlayerStats");
        _upgrades = GetNode<UpgradeManager>("../UpgradeManager");

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

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        // One choice at a time: wait while choosing (the game is paused then), and never after death.
        if (_pendingLevelUps == 0 || _stats.IsDead || _upgrades.IsChoosing || GetTree().Paused)
        {
            return;
        }

        _pendingLevelUps--;
        _upgrades.TriggerLevelUp();
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
        var waves = GetTree().CurrentScene?.GetNodeOrNull<WaveManager>("WaveManager");
        if (waves == null)
        {
            if (!_warnedNoWaveManager)
            {
                _warnedNoWaveManager = true;
                GD.PushWarning("ExperienceSystem: kein WaveManager gefunden, Woche bleibt 1.");
            }

            return;
        }

        _week = Mathf.Max(1, waves.CurrentWeek);
        waves.WeekChanged += week => _week = week;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (_stats.IsDead)
        {
            return;
        }

        int index = Mathf.Clamp(_week - 1, 0, XpPerKillByWeek.Length - 1);
        int gained = XpPerKillByWeek[index];
        if (enemy.IsInGroup("bosses"))
        {
            gained += BossBonusXp;
        }

        Xp += gained;
        while (Xp >= XpNeeded)
        {
            Xp -= XpNeeded;
            Level++;
            _pendingLevelUps++;
            EmitSignal(SignalName.LeveledUp, Level);
            GD.Print($"[XP] LEVEL UP -> Level {Level}");
        }

        EmitSignal(SignalName.XpChanged, Xp, XpNeeded, Level);
        GD.Print($"[XP] +{gained} (Woche {_week}) -> Level {Level}, {Xp}/{XpNeeded} XP, wartende Level-Ups: {_pendingLevelUps}");
    }
}
