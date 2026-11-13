using Godot;
using System.Collections.Generic;

// SoundManager (Tim) – als Autoload mit dem Namen "SoundManager" eingetragen.
//
// Läuft automatisch, ohne Code von anderen:
//   - Hover- und Klick-Sound bei jedem Button im Spiel
//   - Musik je nach Szene (siehe "WELCHE MUSIK IN WELCHER SZENE")
//   - Spiel-Ereignisse (siehe "AUTOMATISCHE SPIEL-SOUNDS")
//
// Von überall im Code benutzbar (kein Import nötig):
//   SoundManager.Play("kaffee");
public partial class SoundManager : Node
{
	// ===== LAUTSTÄRKE (in dB) =====
	// 0 = normal, kleinere Zahl = leiser (z. B. -25), größere Zahl = lauter (z. B. 3)
	private const float MusikLautstaerke = -20f;
	private const float EffektLautstaerke = -10f;
	// So viel leiser wird die Musik kurz bei Hover und Klick
	private const float MusikKurzLeiserUm = 10f;
	// ==============================

	// ===== AUTOMATISCHE SPIEL-SOUNDS =====
	// Diese Namen spielen von allein, sobald du sie unten bei "Eintragen" einträgst:
	//   "gegner_tot"          – ein normaler Gegner stirbt
	//   "boss_kommt"          – ein Prüfungs-Boss erscheint (auch die Endklausur)
	//   "boss_tot"            – ein Boss wird besiegt (Prüfung bestanden)
	//   "woche_neu"           – eine neue Semesterwoche beginnt
	//   "semester_geschafft"  – alle Wochen überlebt
	// Solange ein Name nicht eingetragen ist, passiert dort einfach nichts.
	// ======================================

	// Gleicher Sound höchstens alle so viele Millisekunden
	// (sonst knallt es, wenn 20 Gegner gleichzeitig sterben)
	private const ulong MindestAbstandMs = 50;

	private static SoundManager _instance;
	private Tween _musikTween;
	private readonly Dictionary<string, AudioStream> _sounds = new();
	private readonly Dictionary<string, string> _musikProSzene = new();
	private readonly Dictionary<string, ulong> _zuletztGespielt = new();
	private AudioStreamPlayer _musik;
	private Node _letzteSzene;

	public override void _Ready()
	{
		_instance = this;

		// Sounds laufen auch weiter, wenn das Spiel pausiert ist (z. B. bei der Level-Up-Auswahl)
		ProcessMode = ProcessModeEnum.Always;

		// ===== HIER TRÄGST DU ALLE SOUNDS EIN =====
		// Links der Name (frei wählbar), rechts der Pfad zur Datei.
		// Pfad bekommst du in Godot: Rechtsklick auf die Datei → "Pfad kopieren"
		Eintragen("hover", "res://assets/sounds/ui/hover.ogg");
		Eintragen("klick", "res://assets/sounds/ui/click.ogg");
		Eintragen("menu_musik", "res://assets/sounds/music/menu.mp3");
		// Eintragen("kaffee", "res://assets/sounds/weapons/kaffee.ogg");
		// Eintragen("gegner_tot", "res://assets/sounds/enemies/gegner_tot.ogg");
		// Eintragen("spiel_musik", "res://assets/sounds/music/spiel.ogg");

		// Gegner (spielen automatisch)
		// Eintragen("boss_kommt", "res://assets/sounds/enemies/boss_kommt.ogg");
		// Eintragen("boss_tot", "res://assets/sounds/enemies/boss_tot.ogg");

		// Semester (spielen automatisch)
		// Eintragen("woche_neu", "res://assets/sounds/music/woche_neu.ogg");
		// Eintragen("semester_geschafft", "res://assets/sounds/music/semester_geschafft.ogg");

		// Spieler (Philipp ruft sie in seinem Code mit SoundManager.Play("...") auf)
		// Eintragen("schaden", "res://assets/sounds/player/schaden.ogg");
		// Eintragen("xp", "res://assets/sounds/player/xp.ogg");
		// Eintragen("level_up", "res://assets/sounds/player/level_up.ogg");
		// ==========================================

		// ===== WELCHE MUSIK IN WELCHER SZENE =====
		// Links der Name der Szene (Dateiname ohne .tscn, egal in welchem Ordner),
		// rechts der Name der Musik von oben.
		// Szenen, die hier nicht stehen, haben keine Musik (Musik stoppt automatisch).
		MusikFuerSzene("sound_test", "menu_musik");
		// MusikFuerSzene("hauptmenue", "menu_musik");   // Fabians Menü
		// MusikFuerSzene("main", "spiel_musik");        // das eigentliche Spiel
		// ==========================================

		GD.Print($"SoundManager: {_sounds.Count} Sounds geladen: {string.Join(", ", _sounds.Keys)}");

		_musik = new AudioStreamPlayer();
		_musik.VolumeDb = MusikLautstaerke;
		AddChild(_musik);

		// Alles, was im Spiel neu auftaucht, wird geprüft (Buttons, Gegner, Bosse, Wellen)
		GetTree().NodeAdded += OnNodeAdded;

		// Was beim Spielstart schon da ist (z. B. das Menü oder die Arena als erste Szene)
		foreach (var node in GetTree().Root.FindChildren("*", "", true, false))
			OnNodeAdded(node);
	}

	private void Eintragen(string name, string pfad)
	{
		if (!ResourceLoader.Exists(pfad))
		{
			GD.PrintErr($"SoundManager: Datei '{pfad}' nicht gefunden (Sound '{name}')!");
			return;
		}
		_sounds[name] = GD.Load<AudioStream>(pfad);
	}

	private void MusikFuerSzene(string szenenName, string musikName)
	{
		_musikProSzene[szenenName.ToLower()] = musikName;
	}

	// Merkt automatisch, wenn eine neue Szene startet, und wechselt die Musik
	public override void _Process(double delta)
	{
		var szene = GetTree().CurrentScene;
		if (szene == null || szene == _letzteSzene)
			return;

		_letzteSzene = szene;
		var szenenName = szene.SceneFilePath.GetFile().GetBaseName().ToLower();

		if (_musikProSzene.TryGetValue(szenenName, out var musikName))
		{
			GD.Print($"SoundManager: Szene '{szenenName}' gestartet → Musik '{musikName}'");
			PlayMusic(musikName);
		}
		else
		{
			GD.Print($"SoundManager: Szene '{szenenName}' gestartet → keine Musik eingetragen");
			StopMusic();
		}
	}

	private void OnNodeAdded(Node node)
	{
		if (node.HasMeta("hat_sound"))
			return; // schon verbunden, nicht doppelt

		if (node is BaseButton button)
		{
			button.MouseEntered += () => Play("hover", musikKurzLeiser: true);
			button.Pressed += () => Play("klick", musikKurzLeiser: true);
		}
		else if (node is Boss boss)
		{
			PlayWennDa("boss_kommt");
			boss.Died += _ => PlayWennDa("boss_tot");
		}
		else if (node is Enemy enemy)
		{
			enemy.Died += _ => PlayWennDa("gegner_tot");
		}
		else if (node is WaveManager wellen)
		{
			wellen.WeekChanged += _ => PlayWennDa("woche_neu");
			wellen.SemesterFinished += () => PlayWennDa("semester_geschafft");
		}
		else
		{
			return;
		}

		node.SetMeta("hat_sound", true);
	}

	// Spielt einen Sound nur, wenn er eingetragen ist (für die automatischen Sounds)
	private static void PlayWennDa(string name)
	{
		if (_instance != null && _instance._sounds.ContainsKey(name))
			Play(name);
	}

	// Macht die Musik kurz leiser und dann sanft wieder normal laut
	private void MusikKurzLeiser()
	{
		if (!_musik.Playing)
			return;

		_musikTween?.Kill();
		_musikTween = CreateTween();
		_musikTween.TweenProperty(_musik, "volume_db", MusikLautstaerke - MusikKurzLeiserUm, 0.05);
		_musikTween.TweenInterval(0.4);
		_musikTween.TweenProperty(_musik, "volume_db", MusikLautstaerke, 0.6);
	}

	// Spielt einen kurzen Sound ab (mehrere gleichzeitig möglich)
	public static void Play(string name, bool musikKurzLeiser = false)
	{
		if (_instance == null || !_instance._sounds.TryGetValue(name, out var stream))
		{
			GD.PrintErr($"SoundManager: Sound '{name}' gibt es nicht!");
			return;
		}

		// Derselbe Sound nicht zigmal im selben Moment
		var jetzt = Time.GetTicksMsec();
		if (_instance._zuletztGespielt.TryGetValue(name, out var zuletzt) && jetzt - zuletzt < MindestAbstandMs)
			return;
		_instance._zuletztGespielt[name] = jetzt;

		if (musikKurzLeiser)
			_instance.MusikKurzLeiser();

		var player = new AudioStreamPlayer();
		player.Stream = stream;
		player.VolumeDb = EffektLautstaerke;
		player.PitchScale = (float)GD.RandRange(0.95, 1.05); // leicht variieren, damit es nicht nervt
		_instance.AddChild(player);
		player.Play();
		player.Finished += player.QueueFree;
	}

	// Startet Musik (läuft einfach weiter, wenn dieselbe Musik schon spielt)
	public static void PlayMusic(string name)
	{
		if (_instance == null || !_instance._sounds.TryGetValue(name, out var stream))
		{
			GD.PrintErr($"SoundManager: Musik '{name}' gibt es nicht!");
			return;
		}

		if (_instance._musik.Stream == stream && _instance._musik.Playing)
			return;

		_instance._musikTween?.Kill();
		_instance._musik.VolumeDb = MusikLautstaerke;
		_instance._musik.Stream = stream;
		_instance._musik.Play();
		GD.Print($"SoundManager: Musik '{name}' läuft (Lautstärke {MusikLautstaerke} dB)");
	}

	public static void StopMusic()
	{
		_instance?._musik.Stop();
	}
}
