extends SceneTree
# Headless-Tests für Gegner und Leveldesign (nicht Teil des Spiels).
# Aufruf aus dem Projektordner:
#   Godot --headless --path . -s res://tests/headless/level_test.gd -- soak <woche> <sekunden>
#   Godot --headless --path . -s res://tests/headless/level_test.gd -- gaenge
#   ... -- campus | bibliothek | mensa | hoersaal
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

const LOCATION_WEEKS = {"gaenge": 1, "campus": 3, "bibliothek": 6, "mensa": 9, "hoersaal": 12}

# Orts-Tests
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
	if LOCATION_WEEKS.has(mode):
		wm.set("TestStartWeek", LOCATION_WEEKS[mode])
		wm.set("WeekDuration", 100000.0)
	else:
		wm.set("TestStartWeek", week)
	root.add_child(main)
	current_scene = main
	player = main.get_node("PlayerController")
	if LOCATION_WEEKS.has(mode):
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

	match mode:
		"gaenge": return gaenge_test()
		"campus": return campus_test()
		"bibliothek": return bibliothek_test()
		"mensa": return mensa_test()
		"hoersaal": return hoersaal_test()
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

func spawn_enemy(path: String, pos: Vector2) -> Node:
	var e = load(path).instantiate()
	main.add_child(e)
	e.global_position = pos
	spawned.append(e)
	return e

func near(value: float, target: float, tolerance := 0.06) -> bool:
	return abs(value - target) <= target * tolerance

func finish() -> bool:
	print("[RESULT] Fehlgeschlagen: ", fails, " Trigger ", triggers)
	return true

var probe: Node
var probe_hp := 0
var zones := []
var lanes := []

func campus_test() -> bool:
	match step:
		0:
			if t < 0.5: return false
			zones = get_nodes_in_group("slow_zones")
			lanes = get_nodes_in_group("bike_lanes")
			check(zones.size() == 3 and lanes.size() == 2, "Campus hat 3 Rasensprenger und 2 Radwege (%d/%d)" % [zones.size(), lanes.size()])
			spawn_enemy("res://scenes/enemies/enemy.tscn", zones[0].global_position + Vector2(30, 0))
			spawn_enemy("res://scenes/enemies/enemy.tscn", Vector2(800, -300))
			spawn_enemy("res://scenes/enemies/midterm_exam.tscn", zones[1].global_position)
			step = 1
		1:
			if t < 1.0: return false
			var v_in: float = spawned[0].velocity.length()
			var v_out: float = spawned[1].velocity.length()
			var v_boss: float = spawned[2].velocity.length()
			check(near(v_in, 120.0 * 0.6), "Gegner im nassen Gras auf 60 %% gebremst (%.0f statt 120)" % v_in)
			check(near(v_out, 120.0), "Gegner außerhalb normal schnell (%.0f)" % v_out)
			check(near(v_boss, 70.0), "Boss im nassen Gras nicht gebremst (%.0f statt 70)" % v_boss)
			for e in spawned: e.queue_free()
			spawned.clear()
			step = 2
		2:
			if t < 1.5: return false
			player.global_position = Vector2(0, -1700) # auf dem Radweg
			probe = spawn_enemy("res://scenes/enemies/homework_stack.tscn", Vector2(250, -1700))
			step = 3
		3:
			if t < 1.7: return false
			probe_hp = probe.get("Health")
			lanes[0].call("StartPass")
			check(lanes[0].get("IsActive") == true and triggers.get("fahrrad_klingel", 0) == 1, "Radweg warnt vor der Durchfahrt (Klingeln)")
			step = 4
		4:
			if t < 10.5: return false
			var hp = probe.get("Health") if is_instance_valid(probe) else -1
			check(hp >= 0 and hp <= probe_hp - 20, "Fahrrad trifft Gegner (%d -> %d HP)" % [probe_hp, hp])
			check(triggers.get("fahrrad_treffer", 0) == 1, "Fahrrad trifft Spieler genau einmal")
			check(lanes[0].get("IsActive") == false, "Radweg nach der Durchfahrt wieder frei")
			return finish()
	return false

func blocked_by_obstacle(pos: Vector2) -> bool:
	var q := PhysicsPointQueryParameters2D.new()
	q.position = pos
	for hit in root.get_world_2d().direct_space_state.intersect_point(q):
		if hit["collider"].is_in_group("obstacles"):
			return true
	return false

func spawn_check(where: Vector2, label: String):
	player.global_position = where
	var spawner = main.get_node("EnemySpawner")
	var scene = load("res://scenes/enemies/enemy.tscn")
	var bad := 0
	for i in 40:
		var e = spawner.call("SpawnScene", scene)
		if blocked_by_obstacle(e.global_position): bad += 1
		e.queue_free()
	check(bad == 0, "%s: kein Spawn in Hindernissen (%d von 40)" % [label, bad])

func bibliothek_test() -> bool:
	match step:
		0:
			if t < 0.5: return false
			zones = get_nodes_in_group("slow_zones")
			check(zones.size() == 1, "Bibliothek hat eine Ruhezone (%d)" % zones.size())
			var shelves := 0
			for n in get_nodes_in_group("obstacles"):
				if n.name.begins_with("Regal"): shelves += 1
			check(shelves == 12, "12 Bücherregale (%d)" % shelves)
			player.global_position = Vector2(700, 700) # weit weg von der Zone
			spawn_enemy("res://scenes/enemies/enemy.tscn", zones[0].global_position)
			spawn_enemy("res://scenes/enemies/deadline.tscn", zones[0].global_position + Vector2(100, 100))
			spawn_enemy("res://scenes/enemies/enemy.tscn", Vector2(900, -900))
			spawn_enemy("res://scenes/enemies/midterm_programming.tscn", zones[0].global_position + Vector2(-200, 0))
			step = 1
		1:
			if t < 1.0: return false
			check(near(spawned[0].velocity.length(), 60.0), "Standard-Gegner in der Ruhezone halb so schnell (%.0f statt 120)" % spawned[0].velocity.length())
			check(near(spawned[1].velocity.length(), 110.0), "Deadline in der Ruhezone halb so schnell (%.0f statt 220)" % spawned[1].velocity.length())
			check(near(spawned[2].velocity.length(), 120.0), "Gegner außerhalb normal schnell (%.0f)" % spawned[2].velocity.length())
			check(near(spawned[3].velocity.length(), 75.0), "Boss in der Ruhezone nicht gebremst (%.0f statt 75)" % spawned[3].velocity.length())
			step = 2
		2:
			if t < 1.5: return false
			spawned[0].global_position = Vector2(900, 0) # aus der Zone heraus
			step = 3
		3:
			if t < 2.0: return false
			var e = spawned[0]
			var inside: bool = zones[0].call("ContainsPoint", e.global_position)
			check(not inside and near(e.velocity.length(), 120.0), "Nach Verlassen der Zone wieder normales Tempo (%.0f, drin: %s)" % [e.velocity.length(), inside])
			for x in spawned: if is_instance_valid(x): x.queue_free()
			spawned.clear()
			step = 4
		4:
			if t < 2.5: return false
			spawn_check(Vector2(-1100, 0), "Spieler am linken Rand")
			spawn_check(Vector2(0, 1100), "Spieler am unteren Rand")
			return finish()
	return false

var counter: Node

func mensa_test() -> bool:
	match step:
		0:
			if t < 0.5: return false
			var arena = get_first_node_in_group("arena")
			counter = arena.get_node("Essensausgabe")
			machine = arena.get_node("EnergyDrinkAutomat")
			var tables := 0
			for n in get_nodes_in_group("obstacles"):
				if n.name.begins_with("Tisch"): tables += 1
			check(tables == 7, "Mensa hat 7 Tische (%d)" % tables)
			player.global_position = counter.global_position + Vector2(0, 90) # vor der Theke, volles Leben
			step = 1
		1:
			if t < 1.5: return false
			check(counter.get("IsReady") == true and triggers.get("essen", 0) == 0, "Essensausgabe bei vollem Leben nicht verbraucht")
			player.global_position = Vector2(0, 0)
			player.get_node("PlayerStats").call("TakeDamage", 40) # Schild 25 weg, 15 Leben weg
			step = 2
		2:
			if t < 2.0: return false
			player.global_position = counter.global_position + Vector2(0, 90)
			step = 3
		3:
			if t < 3.0: return false
			check(counter.get("IsReady") == false and triggers.get("essen", 0) == 1, "Essensausgabe gibt Essen aus und ist danach leer")
			player.global_position = Vector2(0, 0)
			step = 4
		4:
			if t < 3.5: return false
			player.global_position = counter.global_position + Vector2(0, 90)
			step = 5
		5:
			if t < 4.5: return false
			check(triggers.get("essen", 0) == 1, "In derselben Woche kein zweites Essen")
			main.get_node("WaveManager").emit_signal("WeekChanged", 10) # neue Woche, Spieler steht noch davor
			step = 6
		6:
			if t < 5.0: return false
			check(triggers.get("essen_bereit", 0) == 1 and triggers.get("essen", 0) == 2, "Neue Woche: wieder bereit und sofort ausgegeben (Spieler steht davor)")
			player.global_position = machine.global_position + Vector2(0, 75)
			step = 7
		7:
			if t < 6.0: return false
			check(machine.get("IsReady") == false and triggers.get("energydrink", 0) == 1, "Energy-Drink-Automat in der Mensa löst aus (Schild war leer)")
			spawn_check(Vector2(-1300, 300), "Spieler neben einem Tisch")
			spawn_check(Vector2(0, -1400), "Spieler vor der Theke")
			return finish()
	return false

var board: Node
var boss: Node
var boss_start := Vector2.ZERO
var max_ground := 0
var max_proj := 0
var boss_path := 0.0
var boss_last := Vector2.ZERO

func hoersaal_test() -> bool:
	if is_instance_valid(boss):
		max_ground = max(max_ground, count_class("enemy_attacks", "GroundAttack.cs"))
		max_proj = max(max_proj, count_class("enemy_attacks", "EnemyProjectile.cs"))
		boss_path += boss.global_position.distance_to(boss_last)
		boss_last = boss.global_position
	match step:
		0:
			if t < 0.5: return false
			var arena = get_first_node_in_group("arena")
			board = arena.get_node("Tafel")
			var rows := 0
			for n in get_nodes_in_group("obstacles"):
				if n.name.begins_with("Sitzreihe"): rows += 1
			check(rows == 5, "Hörsaal hat 5 Sitzreihen (%d)" % rows)
			check(board.get("CurrentText") == "Vorlesung", "Tafel zeigt am Anfang \"Vorlesung\"")
			main.get_node("WaveManager").emit_signal("BossWarning", "Testklausur", 5.0)
			step = 1
		1:
			if t < 1.0: return false
			check(str(board.get("CurrentText")).contains("Testklausur") and triggers.get("tafel_warnung", 0) == 1, "Tafel zeigt Boss-Warnung (\"%s\")" % board.get("CurrentText"))
			main.get_node("WaveManager").emit_signal("ExamPhaseStarted")
			step = 2
		2:
			if t < 1.5: return false
			check(board.get("CurrentText") == "Prüfungsphase", "Tafel zeigt \"Prüfungsphase\"")
			# Endboss in den Hörsaal, Spieler bewegt sich im Kreis (weicht nicht aus, ist aber unverwundbar)
			boss = spawn_enemy("res://scenes/enemies/professor.tscn", Vector2(600, 0))
			boss_start = boss.global_position
			boss_last = boss_start
			step = 3
		3:
			player.global_position = Vector2(cos(t * 0.5), sin(t * 0.5)) * 500.0
			if t < 21.5: return false
			check(boss_path > 400.0, "Professor bewegt sich im Hörsaal (%.0f px in 20 s)" % boss_path)
			check(max_ground > 0, "Bodenangriffe im Hörsaal (bis %d gleichzeitig)" % max_ground)
			check(max_proj > 0, "Geschosse im Hörsaal (bis %d gleichzeitig)" % max_proj)
			var arena = get_first_node_in_group("arena")
			check(arena.call("IsInside", boss.global_position, 0.0), "Professor bleibt in der Arena")
			boss.call("TakeDamage", 1000000)
			step = 4
		4:
			if t < 22.5: return false
			spawn_check(Vector2(-1200, 900), "Spieler vor einer Sitzreihe")
			spawn_check(Vector2(0, -900), "Spieler vor der Tafel")
			return finish()
	return false
