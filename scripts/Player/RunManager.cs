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

    /// <inheritdoc/>
    public override void _Ready()
    {
        // The pause must not stop this node, or the delay could never finish.
        ProcessMode = ProcessModeEnum.Always;
        GetNode<PlayerStats>("../PlayerStats").Died += OnPlayerDied;
    }

    private async void OnPlayerDied()
    {
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
