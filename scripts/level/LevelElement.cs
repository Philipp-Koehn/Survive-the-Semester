using Godot;

// Basis für interaktive Elemente im Level (Tür, Kiste, Automat, Spinde, Pickups ...).
// Jedes Element meldet Ereignisse über das Signal Triggered, z. B. "tuer_auf" oder "kiste_auf".
// Der SoundManager (Tim) kann sich mit einer Zeile an alle Elemente hängen:
//   else if (node is LevelElement element) element.Triggered += PlayWennDa;
public partial class LevelElement : Node2D
{
	[Signal] public delegate void TriggeredEventHandler(string eventName);

	// Gemeinsame Zeichenebenen (absolut): über dem Boden (-10), unter Spieler und Gegnern (0)
	public const int RoomFloorZ = -9;
	public const int ElementZ = -4;
	public const int TextZ = 20;

	protected void Trigger(string eventName)
	{
		EmitSignal(SignalName.Triggered, eventName);
	}

	protected Node2D FindPlayer()
	{
		return GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	// Kurzer Text, der nach oben schwebt und ausblendet (z. B. "+50 XP")
	public static void ShowFloatingText(Node parent, Vector2 globalPosition, string text, Color color)
	{
		var label = new Label
		{
			Text = text,
			Modulate = color,
			ZIndex = TextZ,
			ZAsRelative = false,
		};
		label.AddThemeFontSizeOverride("font_size", 22);
		label.AddThemeConstantOverride("outline_size", 6);
		label.AddThemeColorOverride("font_outline_color", Colors.Black);
		parent.AddChild(label);
		label.GlobalPosition = globalPosition + new Vector2(-30, -40);

		Tween tween = label.CreateTween().SetParallel();
		tween.TweenProperty(label, "position:y", label.Position.Y - 45, 0.9);
		tween.TweenProperty(label, "modulate:a", 0.0, 0.9).SetDelay(0.3);
		tween.Chain().TweenCallback(Callable.From(label.QueueFree));
	}
}
