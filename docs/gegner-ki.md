# Gegner & KI – Dokumentation (Mert Erbasaran)

## 1. Basis-Gegner und Verfolgungs-KI

**Datum:** 28.10.2026
**Dateien:** `scenes/enemies/enemy.tscn`, `scripts/enemies/Enemy.cs`,
`scenes/test/test_player.tscn`, `scripts/test/TestPlayer.cs`

### Umsetzung
- Der Gegner ist ein `CharacterBody2D` mit Sprite (rot eingefärbt, halbe Größe)
  und kreisförmiger Kollision.
- Verfolgungs-KI: Jeden Physik-Frame wird die Richtung zum Spieler berechnet
  (`DirectionTo`), und der Gegner bewegt sich mit `MoveAndSlide()` dorthin.
- Kontaktschaden: Bei Kollision mit dem Spieler wird `TakeDamage()` aufgerufen,
  mit 1 Sekunde Abklingzeit, damit nicht jeden Frame Schaden entsteht.
- Werte (Geschwindigkeit, Leben, Schaden, Abklingzeit) sind per `[Export]`
  im Inspektor einstellbar, Balancing geht also ohne Code-Änderung.

### Designentscheidungen
- **Direkte Verfolgung statt Pathfinding:** Die Arena ist offen und hat kaum Hindernisse.
  Pathfinding wäre bei vielen Gegnern deutlich teurer und bringt im
  Vampire-Survivors-Stil keinen Vorteil.
- **Schnittstelle über Groups:** Spieler in `"player"`, Gegner in `"enemies"`, beide mit
  `TakeDamage(int)`. Spieler- und Gegnercode sind dadurch unabhängig und können
  parallel entwickelt werden.
- **Test-Spieler:** einfacher Platzhalter mit Kamera, damit die Gegner-KI getestet werden
  kann, bevor der echte Spieler fertig ist.
- **Skalierung nur am Sprite:** Der `CharacterBody2D` selbst bleibt auf Scale 1,
  weil skalierte Physik-Körper in Godot zu Kollisionsfehlern führen können.

### Test
- Drei Gegner und der Test-Spieler in `main.tscn` platziert.
- Ergebnis: Die Gegner verfolgen den Spieler, in der Ausgabe erscheint bei
  Kontakt jeweils „Spieler getroffen! HP: …“ im Abstand von 1 Sekunde pro Gegner.

### Probleme & Lösungen
- Der Spieler wurde beim Start nicht immer gefunden, weil die Reihenfolge der
  `_Ready()`-Aufrufe von der Szenenstruktur abhängt.
  → Lösung: Der Spieler wird in `_PhysicsProcess` gesucht, falls er noch fehlt.
- Der Test-Spieler hat keine Todeslogik, seine HP laufen ins Minus.
  Das ist gewollt: Tod und Game Over gehören zum echten Spieler (Philipp).

## 2. Gegner-Spawner

**Datum:** 28.10.2026
**Dateien:** `scripts/enemies/EnemySpawner.cs`, `main.tscn`,
`scenes/test/test_player.tscn`, `scenes/enemies/enemy.tscn` (Motion Mode)

### Umsetzung
- `EnemySpawner` ist ein `Node2D` in der Hauptszene. Er zählt jeden Frame
  einen Timer herunter und erzeugt nach Ablauf (`SpawnInterval`, Standard 1,5 s)
  einen Gegner.
- Der Gegnertyp wird als `PackedScene` im Inspektor zugewiesen, derselbe
  Spawner kann also später beliebige Gegnertypen erzeugen.
- Spawn-Position: zufälliger Winkel auf einem Kreis mit Radius `SpawnDistance`
  (700 px) um den Spieler. Der Kreis liegt außerhalb des sichtbaren Bereichs,
  die Gegner laufen also von außen ins Bild.
- `MaxEnemies` (50) begrenzt die Anzahl gleichzeitig lebender Gegner
  (Abfrage über die Group `"enemies"`).
- Die von Hand platzierten Test-Gegner wurden aus `main.tscn` entfernt.

### Designentscheidungen
- **Spawn außerhalb des Bildschirms:** Gegner tauchen nicht plötzlich neben dem
  Spieler auf. Das wirkt fairer und entspricht dem Vampire-Survivors-Vorbild.
- **Obergrenze statt unbegrenztem Spawn:** schützt die Performance auf
  schwächeren Rechnern.
- **Timer im Code statt Timer-Node:** weniger Abhängigkeiten in der Szene, und
  das Intervall kann später vom Wellensystem direkt geändert werden.
- **Motion Mode „Floating“:** Spieler und Gegner nutzen den Floating-Modus statt
  „Grounded“, weil es in der Top-Down-Ansicht keinen Boden und keine Schwerkraft gibt.

### Test
- Ergebnis: Gegner erscheinen regelmäßig außerhalb des Bildes, laufen auf den
  Spieler zu und sammeln sich um ihn.

### Probleme & Lösungen
- Die Spawn-Position wird erst nach `AddChild()` gesetzt, weil `GlobalPosition`
  nur zuverlässig funktioniert, wenn der Node bereits im Szenenbaum hängt.
- Die neuen `[Export]`-Felder erschienen erst nach einem Build (Hammer) im Inspektor.
- Die Bewegung des Spielers war nicht erkennbar, weil die Kamera ihm folgt und der
  Hintergrund leer ist. Ein Test mit deaktivierter Kamera bestätigte, dass die
  Steuerung funktioniert. Wird durch den Arena-Boden (Leveldesign) gelöst.

## 3. Platzhalter-Boden der Arena (Leveldesign)

**Datum:** 28.10.2026
**Dateien:** `scripts/level/ArenaFloor.cs`, `main.tscn`

### Umsetzung
- `ArenaFloor` ist ein `Node2D` in der Hauptszene, der in `_Draw()` ein
  Schachbrettmuster (Felder à 64 px) und einen roten Rand zeichnet.
- Die Arena ist 4000 × 4000 px groß und um den Nullpunkt zentriert.
  Der Spieler startet deshalb bei Position (0, 0).
- `ZIndex = -10` sorgt dafür, dass der Boden immer hinter Spieler und Gegnern liegt.
- Größe, Feldgröße und Farben sind per `[Export]` im Inspektor einstellbar.

### Designentscheidungen
- **Per Code gezeichnet statt Grafik:** Für den Prototyp werden keine Assets
  benötigt. Der Boden ist ein Platzhalter und wird später durch die richtige
  Arena-Grafik (Thema Hochschule) ersetzt.
- **Schachbrettmuster:** Durch den Kontrast ist die Bewegung des Spielers trotz
  mitlaufender Kamera klar erkennbar. Das löst das Problem aus Abschnitt 2.

### Test
- Ergebnis: Das Schachbrett ist im Spiel sichtbar und bewegt sich beim Laufen
  unter dem Spieler. Der rote Rand markiert die Grenze der Arena.

### Offene Punkte
- Der Rand ist bisher nur optisch, der Spieler kann noch hinauslaufen.
  Begrenzung durch Wände folgt.
- Der Boden ist nur im laufenden Spiel sichtbar, nicht im Editor.

## 4. Gegnertypen

**Datum:** 28.10.2026
**Dateien:** `scenes/enemies/deadline.tscn`, `scenes/enemies/homework_stack.tscn`,
`scripts/enemies/EnemySpawner.cs` (Liste statt einzelner Szene), `main.tscn`

### Umsetzung
- Neue Gegnertypen sind **geerbte Szenen** von `enemy.tscn`. Sie übernehmen
  Verhalten und Aufbau des Basis-Gegners, nur Werte und Aussehen werden überschrieben.

| Typ | Rolle | Speed | Leben | Schaden | Aussehen |
|---|---|---|---|---|---|
| Enemy | Standard | 120 | 10 | 5 | rot, Scale 0.5 |
| Deadline | schnell, schwach | 220 | 5 | 3 | orange, Scale 0.35 |
| Hausaufgaben-Stapel | langsam, zäh | 60 | 40 | 10 | lila, Scale 0.8, Hitbox-Radius 50 |

- Der Spawner hat statt einer einzelnen Szene jetzt eine Liste `EnemyScenes`
  und wählt bei jedem Spawn zufällig einen Typ daraus.

### Designentscheidungen
- **Vererbung statt neuer Skripte:** Kein doppelter Code. Änderungen am
  Basis-Gegner (z. B. an der KI) wirken automatisch auf alle Typen.
- **Unterschiedliche Rollen:** Deadlines erzeugen Druck durch Tempo, Stapel
  blockieren Wege und halten viel aus. So muss der Spieler sich bewegen und
  priorisieren, statt stehen zu bleiben.
- **Gewichtung über die Liste:** Ein Typ, der mehrfach eingetragen ist, erscheint
  entsprechend häufiger. Das ist einfach und reicht für den Prototyp.

### Test
- Ergebnis: Alle drei Typen erscheinen gemischt und unterscheiden sich
  sichtbar in Größe, Farbe und Geschwindigkeit.

### Probleme & Lösungen
- Die Kreisform der Kollision wird zwischen Basis-Szene und geerbten Szenen
  geteilt. Eine Radius-Änderung beim Stapel hätte auch den Basis-Gegner vergrößert.
  → Lösung: Shape beim Stapel über „Einzigartig machen“ als eigene Kopie angelegt.
- Die Farben wirken dunkler als eingestellt, weil `Modulate` mit dem blauen
  Platzhalter-Sprite multipliziert wird. Wird mit eigenen Grafiken behoben.

## 5. Wellensystem (Semester-Wochen)

**Datum:** 28.10.2026
**Dateien:** `scripts/enemies/WaveManager.cs`, `main.tscn`

### Umsetzung
- Der `WaveManager` (Node in der Hauptszene) teilt das Spiel in **14 Wochen**
  à 30 Sekunden ein (`WeekDuration`, `TotalWeeks`).
- Zu Beginn jeder Woche passt er den `EnemySpawner` an:
  - **Spawn-Intervall:** startet bei 1,5 s und wird pro Woche 0,1 s kürzer,
    mindestens 0,3 s.
  - **Gegnerliste:** bestimmt, welche Typen vorkommen und wie häufig.

| Woche | Gegner in der Liste |
|---|---|
| 1–2 | 2× Standard |
| 3–4 | 2× Standard, 1× Deadline |
| 5–8 | 2× Standard, 1× Deadline, 1× Stapel |
| 9–14 | 2× Standard, 2× Deadline, 2× Stapel |

- Nach Woche 14 wird der Spawner gestoppt und „Semester geschafft!“ ausgegeben.
- Signale `WeekChanged(int week)` und `SemesterFinished` werden ausgelöst,
  damit andere Teile des Spiels darauf reagieren können.

### Designentscheidungen
- **Wochen statt abstrakter Wellen:** passt zum Thema „Survive the Semester“
  und gibt dem Spieler eine verständliche Fortschrittsanzeige.
- **Getrennte Verantwortung:** Der Spawner weiß nur, *wie* er spawnt.
  Der WaveManager entscheidet, *was* und *wie schnell*. So bleiben beide
  Skripte einfach und unabhängig testbar.
- **Signale als Schnittstelle zur UI:** Fabian kann `WeekChanged` für die
  Wochenanzeige und `SemesterFinished` für den Sieg-Bildschirm nutzen,
  ohne den Gegnercode zu kennen.
- **Alle Werte im Inspektor einstellbar:** Wochenlänge, Anzahl der Wochen und
  Tempo-Steigerung lassen sich beim Balancing ohne Code-Änderung anpassen.

### Test
- Mit `WeekDuration = 5` getestet: Die Wochen 1 bis 14 laufen nacheinander durch,
  neue Gegnertypen erscheinen ab Woche 3 bzw. 5, danach erscheint
  „Semester geschafft!“ und es spawnen keine neuen Gegner mehr.
- Anschließend wieder auf 30 Sekunden zurückgestellt.

### Offene Punkte
- Prüfungs-Bosse (Zwischenprüfung, Klausur) werden als Nächstes ergänzt.
- Die Wochenanzeige auf dem Bildschirm übernimmt die UI (Fabian) über das
  Signal `WeekChanged`.

## 6. Gegner besiegen: Died-Signal und Test-Angriff

**Datum:** 28.10.2026
**Dateien:** `scripts/enemies/Enemy.cs`, `scripts/test/TestPlayer.cs`

### Umsetzung
- `Enemy` hat jetzt ein Signal `Died(Enemy enemy)`, das beim Tod ausgelöst wird,
  und eine öffentliche Eigenschaft `Health`.
- `TakeDamage()` und `Die()` sind `virtual`, damit spezielle Gegner (Bosse)
  eigenes Verhalten ergänzen können. Doppeltes Sterben wird verhindert.
- Der Test-Spieler hat einen Platzhalter-Angriff auf der Leertaste: Alle Gegner
  im Umkreis von 150 px erhalten 5 Schaden.

### Designentscheidungen
- **Test-Angriff statt auf die Waffe zu warten:** Bosse und Semester-Ende
  setzen voraus, dass Gegner besiegt werden können. Der Angriff nutzt dieselbe
  Schnittstelle (`TakeDamage`), die später auch die echte Waffe (Philipp) aufruft.
  Er wird zusammen mit dem Test-Spieler entfernt.
- **Signal statt direkter Aufrufe:** Andere Systeme (Bosse, XP, Statistik) können
  auf `Died` reagieren, ohne dass der Gegner sie kennen muss.

### Test
- Deadline stirbt nach 1 Treffer, Standard-Gegner nach 2, Hausaufgaben-Stapel nach 8.

## 7. Prüfungs-Boss „Zwischenprüfung“

**Datum:** 28.10.2026
**Dateien:** `scripts/enemies/Boss.cs`, `scenes/enemies/midterm_exam.tscn`

### Umsetzung
- `Boss` erbt von `Enemy` und übernimmt Verfolgung und Kontaktschaden.
- Zusätzlich ruft der Boss alle 4 Sekunden 3 Deadlines („Prüfungsfragen“)
  gleichmäßig im Kreis um sich herum herbei (`MinionScene`, `SummonInterval`,
  `MinionsPerSummon`, `SummonRadius`).
- Signal `HealthChanged(int health, int maxHealth)` bei jedem Treffer,
  z. B. für eine Boss-Lebensleiste in der UI (Fabian).
- Bosse sind zusätzlich in der Group `"bosses"`.
- `midterm_exam.tscn` ist eine geerbte Szene von `enemy.tscn` mit `Boss.cs`
  als Skript: 150 HP, Speed 70, Kontaktschaden 15, Scale 1.2, eigene Hitbox (Radius 75).

### Designentscheidungen
- **Vererbung im Code (`Boss : Enemy`):** Die Grundlogik existiert nur einmal.
  Der Boss überschreibt nur, was anders ist (`_PhysicsProcess`, `TakeDamage`).
- **Herbeigerufene Gegner statt komplexer Angriffe:** einfach umzusetzen, passt
  zum Thema (eine Prüfung wirft viele Fragen auf) und erhöht den Druck,
  ohne neue Mechaniken zu brauchen.
- **Name als Export:** Derselbe Code dient für verschiedene Prüfungen.

### Test
- Boss testweise in `main.tscn` platziert, Test-Angriff auf 25 Schaden erhöht.
- Ergebnis: „Zwischenprüfung erscheint! (150 HP)“, pro Treffer −25 HP,
  nach 6 Treffern „Zwischenprüfung bestanden!“. Test-Boss danach wieder entfernt.

### Probleme & Lösungen
- „bestanden!“ wurde vor der letzten HP-Meldung ausgegeben, weil der Boss
  schon beim Schaden stirbt. → Meldung in `TakeDamage` nach der HP-Ausgabe verschoben.

### Offene Punkte
- Boss automatisch über den WaveManager spawnen (Woche 7).
- Endklausur als zweiter Boss in Woche 14, Semester endet erst nach ihrem Sieg.

## 8. Bosse im Semesterablauf

**Datum:** 11.11.2026
**Dateien:** `scripts/enemies/WaveManager.cs`, `scripts/enemies/EnemySpawner.cs`,
`scripts/enemies/Boss.cs`, `scenes/enemies/final_exam.tscn`, `main.tscn`

### Umsetzung
- Der `WaveManager` spawnt in Woche 7 (`MidtermWeek`) die **Zwischenprüfung**
  und in der letzten Woche die **Endklausur** (`MidtermBoss`, `FinalBoss`).
- Das Semester endet erst, wenn die Endklausur besiegt ist. Dafür verbindet der
  WaveManager sich mit ihrem `Died`-Signal.
- Neues Signal `BossSpawned(Boss boss)`, damit die UI z. B. eine Boss-Lebensleiste
  einblenden und mit `HealthChanged` verbinden kann.
- `EnemySpawner` hat eine öffentliche Methode `SpawnScene(PackedScene)`, über die
  der WaveManager Bosse außerhalb des Bildschirms erzeugt (ohne `MaxEnemies`-Grenze).
- Woche 1 startet jetzt im ersten Frame statt in `_Ready()`, damit beim Spawnen
  alle Nodes bereit sind.
- `final_exam.tscn` ist eine geerbte Szene der Zwischenprüfung: 400 HP, Speed 80,
  Kontaktschaden 20, ruft alle 5 s vier Standard-Gegner, Scale 1.5, Hitbox-Radius 95.

### Designentscheidungen
- **Sieg an den Endboss gekoppelt:** Das Semester ist erst „geschafft“, wenn die
  Klausur bestanden ist. Zeit überleben allein reicht nicht.
- **Endklausur als Platzhalter:** Später wird sie durch einen eigenen Endboss
  (Professor mit Projektilen) ersetzt. Im WaveManager muss dann nur die Szene
  bei `FinalBoss` getauscht werden.
- **Testwerte im Inspektor statt im Code:** Für den Test wurden `WeekDuration = 5`,
  `TotalWeeks = 3` und `MidtermWeek = 2` gesetzt und danach zurückgestellt.

### Test
- Ergebnis: Woche 2 Zwischenprüfung, Woche 3 Endklausur, nach deren Sieg
  „Endklausur: 0/400 HP“, „Endklausur bestanden!“ und „Semester geschafft!“
  in dieser Reihenfolge, danach keine neuen Gegner mehr.
- Beobachtung: Mit beiden Bossen gleichzeitig wird es sehr voll. Im echten Ablauf
  liegen sieben Wochen dazwischen. Feinabstimmung folgt mit dem echten Spieler.

### Probleme & Lösungen
- Bosse erschienen zunächst nicht, weil `MidtermBoss`/`FinalBoss` im Inspektor
  leer waren und `TotalWeeks` nicht übernommen wurde.
  → Felder zugewiesen, Werte mit Enter bestätigt und gespeichert.
- „Semester geschafft!“ erschien vor der letzten HP-Meldung des Bosses.
  → `Boss.TakeDamage` meldet jetzt zuerst die neuen HP und nimmt erst danach Schaden.
- Build-Fehler „} erwartet“ nach dem Ersetzen der Methode: Die schließende
  Klammer der Klasse fehlte. → Klammer ergänzt.

## 9. Arena-Begrenzung (Leveldesign)

**Datum:** 11.11.2026
**Dateien:** `scripts/level/ArenaFloor.cs`, `scripts/enemies/EnemySpawner.cs`

### Umsetzung
- `ArenaFloor` erzeugt beim Start einen `StaticBody2D` „Walls“ mit vier
  rechteckigen Kollisionsformen direkt außerhalb der Arena (Dicke `WallThickness`, 64 px).
- Kollision und Zeichnung nutzen dieselben Rechtecke (`GetWallRects`), damit
  sichtbare und tatsächliche Wand immer übereinstimmen.
- Die Arena ist in der Group `"arena"` und bietet `Bounds`, `IsInside()` und
  `ClampInside()` für andere Systeme an.
- Der `EnemySpawner` sucht jetzt bis zu 10 zufällige Punkte im Abstand
  `SpawnDistance` und nimmt den ersten, der mindestens 100 px innerhalb der Arena liegt.
  Findet er keinen (Spieler in einer Ecke), wird der Punkt in die Arena zurückgeschoben.

### Designentscheidungen
- **Wände per Code statt von Hand platziert:** Die Wände passen sich automatisch an
  `ArenaSize` an. Für jeden Ort kann die Arenagröße im Inspektor geändert werden.
- **Spawn nur innerhalb der Arena:** Ohne diese Prüfung wären Gegner hinter der Wand
  entstanden und dort hängen geblieben.

### Test
- Spieler kann die Arena nicht verlassen. In der Ecke stehend spawnen alle Gegner
  innerhalb der Arena.

### Probleme & Lösungen
- Beim Ersetzen von `SpawnScene` wurde versehentlich die ganze Datei überschrieben
  (Build-Fehler „public-Modifizierer nicht gültig“). → Datei komplett neu eingefügt.
- Grauer Streifen zwischen Boden und Wand: 4000 px ergeben 62,5 Kacheln (nur 62
  gezeichnet), und der Rahmen wurde mittig auf der Linie gezeichnet.
  → Kachelanzahl aufrunden und am Rand abschneiden, Wände als gefüllte Rechtecke
  an der Position der Kollision zeichnen.

## 10. Umstellung auf den echten Spieler

**Datum:** 16.11.2026

- Philipps `PlayerController` ersetzt in `main.tscn` den Test-Spieler. Er ist in der
  Group `"player"` und hat `TakeDamage(int)`. Die Gegner funktionieren ohne Änderung.
- Philipps Waffen treffen die Gegner über `TakeDamage`, sein XP-System nutzt das
  `Died`-Signal der Gegner.
- Test: Gegner verursachen Schaden (erst Schild, dann Leben), der Spieler stirbt bei 0,
  besiegte Gegner geben XP, der Semesterablauf startet nach dem Neustart wieder in Woche 1.
- `TestPlayer` und `test_player.tscn` wurden gelöscht, da sie nicht mehr benötigt werden.
- Ungültige UID für `icon.svg` in `enemy.tscn` behoben (Textur neu zugewiesen).

## 11. Orte im Semester (Leveldesign)

**Datum:** 16.11.2026
**Dateien:** `scenes/level/arena.tscn`, `scenes/level/arena_gaenge.tscn`,
`arena_campus.tscn`, `arena_bibliothek.tscn`, `arena_mensa.tscn`, `arena_hoersaal.tscn`,
`scripts/enemies/WaveManager.cs`, `main.tscn`

### Umsetzung
- Die Arena ist eine eigene Szene (`arena.tscn`). Jeder Ort ist eine geerbte Szene
  davon mit eigener Größe und eigenen Farben:

| Wochen | Ort | Arena-Größe (px) |
|---|---|---|
| 1–2 | Gänge | 4000 × 1400 (lang und schmal) |
| 3–5 | Outdoor-Campus | 5000 × 5000 (weitläufig) |
| 6–8 | Bibliothek | 3000 × 3000 |
| 9–11 | Mensa | 3500 × 3500 |
| 12–14 | Hörsaal | 3500 × 2500 |

- Der `WaveManager` bestimmt zu Beginn jeder Woche den Ort (`LocationStartWeeks`).
  Wechselt er, wird die alte Arena entfernt und die neue geladen
  (`LocationArenaPaths`). Der Spieler wird in die Mitte gesetzt, normale Gegner
  werden entfernt, Bosse werden in die neue Arena verschoben.
- Neues Signal `LocationChanged(int index, string locationName)`, z. B. für eine
  Ortsanzeige in der UI (Fabian).
- Philipps alte Test-Arena `scenes/Arena.tscn` wurde nach Absprache gelöscht.

### Designentscheidungen
- **Geerbte Szenen pro Ort:** Größe, Farben und später Grafiken und Hindernisse
  können pro Ort im Inspektor geändert werden, Wände und Spawn-Logik bleiben gemeinsam.
- **Unterschiedliche Größen als Leveldesign:** Enge Gänge erzeugen Druck,
  der weite Campus erlaubt Ausweichen.
- **Platzhalter-Farben:** Die Böden werden später durch Pixel-Art von Fabian
  ersetzt (Kachelgröße 16 oder 32 px, noch offen).
- **Gegner beim Wechsel entfernen:** Sonst stünden sie außerhalb der neuen,
  eventuell kleineren Arena.

### Test
- Mit `WeekDuration = 5` getestet: Die Orte wechseln in den vorgesehenen Wochen
  (Ausgabe „Neuer Ort: …“), die Zwischenprüfung erscheint in Woche 7 in der Bibliothek,
  XP des Spielers laufen über den Wechsel weiter. Danach auf 30 s zurückgestellt.