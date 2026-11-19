extends SceneTree
# Headless-Tests für Gegner und Leveldesign (nicht Teil des Spiels).
# Aufruf aus dem Projektordner:
#   Godot --headless --path . -s res://tests/headless/level_test.gd -- soak <woche> <sekunden>
#   Godot --headless --path . -s res://tests/headless/level_test.gd -- gaenge
# Achtung: Läufe können über MetaProgress Credits in den lokalen Spielstand schreiben.
# run_tests.sh sichert den Spielstand vorher und stellt ihn danach wieder her.

var main: Node
var player: Node2D
var mode := ""
var t := 0.0
var duration := 60.0
var next_log := 0.0
var samples := []
var triggers := {}
var step := 0
var fails := 0

# Gänge-Test
var room: Node2D
var door: Node
var chest: Node
var machine: Node
var spawned := []

func _initialize():
	var args := OS.get_cmdline_user_args()
	mode = args[0] if args.size() > 0 else "soak"
	var week := int(args[1]) if args.size() > 1 else 1
	if args.size() > 2:
		duration = float(args[2])

	node_added.connect(_on_node_added)
	main = load("res://main.tscn").instantiate()
	main.get_node("PlayerController/PlayerStats").set("MaxHealth", 1000000) # Test-Spieler stirbt nicht
	var wm = main.get_node("WaveManager")
	if mode == "gaenge":
		wm.set("TestStartWeek", 1)
		wm.set("WeekDuration", 100000.0)
	else:
		wm.set("TestStartWeek", week)
	root.add_child(main)
	current_scene = main
	player = main.get_node("PlayerController")
	if mode == "gaenge":
		main.get_node("EnemySpawner").process_mode = Node.PROCESS_MODE_DISABLED
		player.get_node("WeaponInventory").process_mode = Node.PROCESS_MODE_DISABLED
	print("[TEST] Modus ", mode, " Woche ", week, " Dauer ", duration)

func _on_node_added(node: Node):
	if node.has_signal("Triggered") and not node.is_connected("Triggered", _on_triggered):
		node.connect("Triggered", _on_triggered)
	if mode == "gaenge" and node.get_class() == "StaticBody2D" and node.get("MinInterval") != null:
		node.set("MinInterval", 1.0)   # Spinde im Test schnell aufspringen lassen
		node.set("MaxInterval", 2.0)

func _on_triggered(name: String):
	triggers[name] = triggers.get(name, 0) + 1

func count(group: String) -> int:
	return get_nodes_in_group(group).size()

func count_class(group: String, script_name: String) -> int:
	var n := 0
	for node in get_nodes_in_group(group):
		var s = node.get_script()
		if s and s.resource_path.ends_with(script_name):
			n += 1
	return n

func check(ok: bool, text: String):
	print("[OK] " if ok else "[FAIL] ", text)
	if not ok:
		fails += 1

func _process(delta):
	t += delta
	# Level-Up-Auswahl pausiert das Spiel: automatisch die erste Option wählen
	var up = player.get_node("UpgradeManager")
	if up.get("IsChoosing"):
		up.call("Choose", 0)

	if mode == "gaenge":
		return gaenge_test()
	return soak_test()

func soak_test() -> bool:
	if t >= next_log:
		next_log += 5.0
		var sample = {
			"t": int(t),
			"gegner": count("enemies"),
			"bosse": count("bosses"),
			"geschosse": count_class("enemy_attacks", "EnemyProjectile.cs"),
			"boden": count_class("enemy_attacks", "GroundAttack.cs"),
			"pickups": count("pickups"),
			"nodes": int(Performance.get_monitor(Performance.OBJECT_NODE_COUNT)),
		}
		samples.append(sample)
		print("[COUNT] ", sample)
	if t >= duration:
		# Wachstum: letztes Drittel gegen mittleres Drittel vergleichen
		var n := samples.size()
		var mid = samples[n / 3]["nodes"]
		var last = samples[n - 1]["nodes"]
		var peak := 0
		for s in samples:
			peak = max(peak, s["nodes"])
		print("[RESULT] Nodes Mitte ", mid, " Ende ", last, " Maximum ", peak, " Trigger ", triggers)
		return true
	return false

func in_room(pos: Vector2) -> bool:
	var size: Vector2 = room.get("InnerSize")
	return Rect2(room.global_position - size / 2, size).has_point(pos)

func gaenge_test() -> bool:
	match step:
		0:
			if t < 0.5: return false
			var arena = get_first_node_in_group("arena")
			room = arena.get_node("Seminarraum")
			door = room.get("Door")
			chest = room.get_node("Kiste")
			machine = arena.get_node("EnergyDrinkAutomat")
			player.global_position = Vector2(100, -560) # am Automaten, Schild ist noch voll
			step = 1
		1:
			if t < 1.5: return false
			check(machine.get("IsReady") == true and triggers.get("energydrink", 0) == 0, "Automat wird bei vollem Schild nicht verbraucht")
			player.global_position = Vector2(-1550, 0) # vor der Tür
			step = 2
		2:
			if t < 2.5: return false
			check(door.get("IsOpen") == true and triggers.get("tuer_auf", 0) >= 1, "Tür geht auf, wenn der Spieler davor steht")
			player.global_position = Vector2(-1850, 250) # im Raum
			step = 3
		3:
			if t < 3.0: return false
			check(door.get("IsOpen") == true, "Tür bleibt offen, solange der Spieler im Raum ist")
			for p in [Vector2(-900, -200), Vector2(-700, 0), Vector2(-900, 250), Vector2(-500, 100)]:
				var e = load("res://scenes/enemies/enemy.tscn").instantiate()
				main.add_child(e)
				e.global_position = p
				spawned.append(e)
			step = 4
		4:
			if t < 13.0: return false
			var inside := 0
			for e in spawned:
				if is_instance_valid(e) and in_room(e.global_position): inside += 1
			check(inside >= 2, "Gegner finden durch die Tür in den Raum (%d/4 drin)" % inside)
			player.global_position = Vector2(900, 0) # weit weg im Gang
			step = 5
		5:
			if t < 25.0: return false
			var inside := 0
			for e in spawned:
				if is_instance_valid(e) and in_room(e.global_position): inside += 1
			check(inside == 0, "Gegner verlassen den Raum wieder (%d noch drin)" % inside)
			for e in spawned:
				if is_instance_valid(e): e.call("TakeDamage", 100000)
			step = 6
		6:
			if t < 29.0: return false
			check(door.get("IsOpen") == false and triggers.get("tuer_zu", 0) >= 1, "Tür schließt, wenn niemand mehr da ist")
			player.global_position = Vector2(-1850, 250)
			step = 7
		7:
			if t < 30.0: return false
			player.global_position = chest.global_position # Kiste berühren
			step = 8
		8:
			if t < 31.0: return false
			check(chest.get("IsOpen") == true and triggers.get("kiste_auf", 0) == 1, "Kiste öffnet bei Berührung")
			player.global_position = Vector2(-1850, -250)
			step = 9
		9:
			if t < 31.5: return false
			player.global_position = chest.global_position # zweites Mal
			step = 10
		10:
			if t < 32.5: return false
			check(triggers.get("kiste_auf", 0) == 1, "Kiste öffnet nur einmal")
			player.global_position = Vector2(100, -560)
			step = 11
		11:
			if t < 33.5: return false
			player.global_position = Vector2(100, -300)
			player.get_node("PlayerStats").call("TakeDamage", 10) # Schild 25 -> 15
			step = 12
		12:
			if t < 34.0: return false
			player.global_position = Vector2(100, -560)
			step = 13
		13:
			if t < 35.0: return false
			check(machine.get("IsReady") == false and triggers.get("energydrink", 0) == 1, "Automat löst bei Berührung aus und geht in Cooldown")
			check(triggers.get("spind_auf", 0) >= 3, "Spinde springen auf (%d×)" % triggers.get("spind_auf", 0))
			var pickups = get_nodes_in_group("pickups")
			check(pickups.size() >= 1, "Spinde lassen Loot fallen (%d liegen)" % pickups.size())
			if pickups.size() > 0:
				player.global_position = pickups[0].global_position
			step = 14
		14:
			if t < 36.0: return false
			check(triggers.get("pickup", 0) >= 1, "Loot wird bei Berührung eingesammelt")
			# Spawn-Test: Spieler im Raum, 40 Gegner über den Spawner erzeugen
			player.global_position = Vector2(-1850, 250)
			step = 15
		15:
			if t < 36.5: return false
			var spawner = main.get_node("EnemySpawner")
			var bad := 0
			var scene = load("res://scenes/enemies/enemy.tscn")
			for i in 40:
				var e = spawner.call("SpawnScene", scene)
				var pos: Vector2 = e.global_position
				for z in get_nodes_in_group("no_spawn"):
					if z.call("Contains", pos): bad += 1
				e.queue_free()
			check(bad == 0, "Kein Spawn im Seminarraum (%d von 40 im Raum)" % bad)
			step = 16
		16:
			if t < 37.0: return false
			# Pickups verschwinden nach ihrer Lebenszeit: alle Spinde jetzt lange zu lassen
			for n in get_nodes_in_group("obstacles"):
				if n.get("MinInterval") != null:
					n.set("MinInterval", 10000.0); n.set("MaxInterval", 10001.0)
			player.global_position = Vector2(900, 0)
			step = 17
		17:
			if t < 62.0: return false
			check(count("pickups") == 0, "Loot verschwindet nach 20 s (%d übrig)" % count("pickups"))
			print("[RESULT] Fehlgeschlagen: ", fails, " Trigger ", triggers)
			return true
	return false
