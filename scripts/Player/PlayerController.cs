using Godot;

/// <summary>
/// Top-down player body. Moves in 8 directions from the move_* input actions at a constant speed.
/// Diagonal input is normalized, and the body stops the same frame no key is held.
/// </summary>
/// <example>
/// Instance PlayerController.tscn in a scene; tune <see cref="MoveSpeed"/> in the Inspector.
/// </example>
public partial class PlayerController : CharacterBody2D
{
    /// <summary>Movement speed in pixels per second.</summary>
    [Export]
    public float MoveSpeed { get; set; } = 300.0f;

    /// <summary>Health, shield, damage and crit values of the player.</summary>
    public PlayerStats Stats { get; private set; }

    /// <inheritdoc/>
    public override void _Ready()
    {
        // Gegner finden den Spieler über diese Group (siehe docs/gegner-ki.md).
        AddToGroup("player");
        Stats = GetNode<PlayerStats>("PlayerStats");
    }

    /// <summary>Enemies call this on contact (see docs/gegner-ki.md). Forwards to <see cref="PlayerStats.TakeDamage"/>.</summary>
    /// <param name="amount">Damage the enemy deals.</param>
    public void TakeDamage(int amount)
    {
        Stats.TakeDamage(amount);
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        // GetVector returns a vector of length <= 1, so diagonals are not faster.
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }
}
