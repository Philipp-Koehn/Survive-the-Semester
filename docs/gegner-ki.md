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
- **Aktualisierung 19.11.2026:** Die Deadline hat jetzt eine eigene Hitbox (Radius 18 statt 25),
  weil die geerbte Hitbox größer war als das verkleinerte Sprite. Normale Gegner werden außerdem
  pro Woche zäher (Abschnitt 13). Neue Typen: Trittbrettfahrer (Abschnitt 14), Müdigkeit (Abschnitt 15).
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
- **Aktualisierung 19.11.2026:** Die Gegnerliste pro Woche, die Bosswochen und die Prüfungsphase
  nach Woche 14 stehen jetzt in Abschnitt 20.

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
- **Aktualisierung 19.11.2026:** Erledigt bzw. geändert. Die Zwischenprüfung heißt jetzt
  „Zwischenprüfung Mathe“, hat 500 HP (Richtwert aus Philipps Balancing) und kommt in Woche 5.
  Helfer entstehen nur noch innerhalb der Arena. Ohne `MinionScene` ruft ein Boss keine Helfer.
  Siehe Abschnitte 17 und 20.

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

### Aktualisierung 19.11.2026
- Die Endklausur (`final_exam.tscn`) wurde durch den Endboss „Der Professor“ ersetzt und gelöscht.
  Vor dem Löschen wurde geprüft, dass keine Szene und kein Skript mehr darauf verweist.
- `MidtermBoss`/`MidtermWeek`/`FinalBoss` gibt es nicht mehr. Stattdessen `MidtermBosses`/`MidtermWeeks`
  (Zwischenprüfungen in Woche 5 und 10) und `ExamBosses` (Prüfungsphase *nach* Woche 14,
  wie in der Präsentation angekündigt). Siehe Abschnitt 20.

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

## 12. Hindernisse (Leveldesign)

**Datum:** 16.11.2026
**Dateien:** `scripts/level/Obstacle.cs`, `scenes/level/obstacle.tscn`,
`scenes/level/arena_gaenge.tscn`, `scripts/enemies/EnemySpawner.cs`

### Umsetzung
- `Obstacle` ist ein `StaticBody2D` mit einstellbarer Größe in Kacheln (`SizeInTiles`)
  und Farbe. Es zeichnet ein Platzhalter-Rechteck und legt beim Spielstart eine
  passende rechteckige Kollision an. Alle Hindernisse sind in der Group `"obstacles"`.
- Durch `[Tool]` ist das Rechteck schon im Editor sichtbar, damit man es platzieren kann.
  Die Kollision wird nur im Spiel angelegt, damit sie nicht in der Szene mitgespeichert wird.
- Der `EnemySpawner` prüft vor dem Spawnen per Punktabfrage (`IntersectPoint`),
  ob an der Stelle ein Hindernis steht, und sucht sonst einen anderen Punkt.
- Gänge (4000 × 1400) bestückt mit:

| Hindernis | Größe (Kacheln) | Anzahl | Platzierung |
|---|---|---|---|
| Spindreihe | 3 × 2 | 8 | oben und unten an der Wand, versetzt |
| Schwarzes Brett | 2 × 1 | 1 | an der oberen Wand |
| Mülleimer | 1 × 1 | 3 | frei im Gang |

### Designentscheidungen
- **Ein Baustein für alle Hindernisse:** Ein Skript, beliebig oft in jede Arena gesetzt.
  Später wird nur das Rechteck durch Fabians Grafik ersetzt.
- **Versetzte Spindreihen:** erzeugen Engstellen, durch die man sich bewegen muss.
- **Mitte bleibt frei:** Der Spieler wird beim Ortswechsel in die Mitte gesetzt.
- **Hindernisse vor allem am Rand:** Die Gegner nutzen keine Wegfindung. Freistehende
  Hindernisse sind deshalb klein, damit Gegner nicht dauerhaft hängen bleiben.

### Test
- Spieler kann nicht durch Hindernisse laufen, Gegner spawnen nicht in Hindernissen.
- **Aktualisierung 20.11.2026:** Die acht Spindreihen sind jetzt `LockerRow` (erbt von `Obstacle`)
  und springen zufällig auf (Abschnitt 23).
## 13. Normale Gegner fertiggestellt: Erweiterbarer Basis-Gegner und Leben pro Woche

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/Enemy.cs`, `scripts/enemies/EnemySpawner.cs`,
`scripts/enemies/WaveManager.cs`, `scenes/enemies/deadline.tscn`

### Umsetzung
- `Enemy` hat drei überschreibbare Methoden, damit neue Typen nur ihr eigenes Verhalten ergänzen:
  - `GetMoveDirection()`: wohin der Gegner läuft (Standard: direkt zum Spieler),
  - `GetMoveSpeed()`: wie schnell (Standard: `Speed`),
  - `OnPlayerContact(Node player)`: was bei Berührung passiert (Standard: `TakeDamage(ContactDamage)`),
    höchstens alle `DamageCooldown` Sekunden.
- Das Verhalten von Standard-Gegner, Deadline und Hausaufgaben-Stapel ist dadurch unverändert.
- **Leben pro Woche:** Der WaveManager setzt zu Beginn jeder Woche
  `EnemySpawner.HealthMultiplier = 1 + (Woche − 1) × HealthGrowthPerWeek` (Standard 0,06).
  Jeder normale Gegner multipliziert beim Entstehen (`_Ready`) seine `MaxHealth` damit.
  Das gilt auch für Helfer, die ein Boss ruft. Bosse selbst wachsen nicht (`ScalesWithWeek = false`).
- Die Deadline hat eine eigene Hitbox mit Radius 18, passend zum Sprite (Scale 0,35).

| Woche | 1 | 5 | 10 | 14 |
|---|---|---|---|---|
| Faktor | ×1,00 | ×1,24 | ×1,54 | ×1,78 |
| Standard-Gegner (10 HP) | 10 | 12 | 15 | 18 |
| Hausaufgaben-Stapel (40 HP) | 40 | 50 | 62 | 71 |

### Verhalten
- **Standard-Gegner** (rot): läuft mit 120 direkt auf den Spieler zu, 5 Kontaktschaden pro Sekunde.
- **Deadline** (orange, klein): schnell (220), wenig Leben (5), 3 Schaden. Kommt in Massen.
- **Hausaufgaben-Stapel** (lila, groß): langsam (60), viel Leben (40), 10 Schaden. Blockiert Wege.

### Designentscheidungen
- **Faktor über den Spawner statt in jeder Szene:** Die Grundwerte bleiben pro Typ im Inspektor,
  die Steigerung über das Semester steht an einer Stelle (WaveManager). Vorschlag aus
  Philipps Balancing (`design/balance-waffen.md`, Abschnitt 8).
- **Gruppe `"enemy_spawner"`:** Gegner finden den Faktor ohne feste Verbindung zum Spawner.
  Fehlt der Spawner (z. B. in einer Testszene), bleibt der Faktor 1.

### Test
- Konsole zeigt pro Woche „Gegner-Leben ×…“, Werte wie in der Tabelle.
- Build ohne Fehler, Spiel startet headless ohne Fehlermeldungen.

## 14. Trittbrettfahrer

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/FreeRider.cs`, `scenes/enemies/free_rider.tscn`, `WaveManager.cs`, `main.tscn`

### Umsetzung
- Geerbte Szene von `enemy.tscn` mit eigenem Skript `FreeRider : Enemy`.
- Werte: Tempo 170, 15 HP, **kein** Kontaktschaden, grün, Scale 0,4, Hitbox-Radius 20.
  Erscheint ab Woche 6 (`FreeRiderWeek`).

### Verhalten
1. Läuft wie ein normaler Gegner direkt auf den Spieler zu.
2. Beim ersten Kontakt klaut er `StealAmount` XP (Standard 25), färbt sich hellgrün
   (`LootColor`, „trägt Beute“) und flieht mit `FleeSpeed` (260) direkt vom Spieler weg.
   Er klaut nur einmal.
3. **Entkommt** er (nach `FleeDuration` = 8 s Flucht oder bei `EscapeDistance` = 1000 px Abstand),
   verschwindet er **ohne** `Died`-Signal: Die geklaute XP ist weg, es gibt keine Kill-XP und keinen Kill-Sound.
4. **Wird er besiegt**, bekommt der Spieler die geklaute XP zurück, zusätzlich zur normalen Kill-XP.

### Designentscheidungen
- **Klauen beim Kontakt statt XP-Drops wegschnappen:** Es gibt (noch) keine XP-Drops,
  Philipps `ExperienceSystem` bucht die XP direkt beim `Died`-Signal.
- **Fluchttempo 260 < Spielertempo 300:** Wer sofort hinterherläuft, kann ihn einholen.
  Das erzeugt eine bewusste Entscheidung: Beute zurückholen oder Position halten.
- **15 HP:** Mit weniger würde der Stift (10 Schaden) ihn schon vor dem Kontakt mit einem Schuss erledigen.
- **Schnittstelle abgesichert:** Der Trittbrettfahrer ruft `StealXp`/`AddXp` am Node
  `ExperienceSystem` des Spielers nur auf, wenn es die Methoden gibt (`HasMethod`, dann `Call`,
  wie bei `TakeDamage`). Sonst kommt einmal pro Spielstart eine Warnung und er flieht mit 0 XP.

### Benötigte Schnittstelle (mit Philipp abzusprechen)
```csharp
// in ExperienceSystem
public int StealXp(int amount); // zieht bis zu amount vom Fortschritt im aktuellen Level ab
                                // (nie unter 0, kein Level-Verlust), löst XpChanged aus,
                                // gibt die wirklich abgezogene Menge zurück
public void AddXp(int amount);  // fügt XP hinzu, inklusive Level-Ups und XpChanged
```

### Test
- Headless: Trittbrettfahrer neben den Spieler gesetzt (Waffen aus). Er berührt den Spieler,
  „Trittbrettfahrer klaut 0 XP und flieht!“, nach 8 s „…entkommen“. Ein zweiter, der auf der Flucht
  besiegt wird, gibt normale Kill-XP. Die Warnung zur fehlenden Schnittstelle kommt nur einmal.

### Offene Punkte
- Die XP wird erst wirklich abgezogen bzw. zurückgegeben, wenn Philipp `StealXp`/`AddXp` eingebaut hat.

## 15. Müdigkeit

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/Tiredness.cs`, `scenes/enemies/tiredness.tscn`, `WaveManager.cs`, `main.tscn`

### Umsetzung
- Geerbte Szene von `enemy.tscn` mit Skript `Tiredness : Enemy`.
- Werte: Tempo 90, 15 HP, 2 Kontaktschaden, graublau, Scale 0,55, Hitbox-Radius 28.
  Erscheint ab Woche 4 (`TirednessWeek`).
- Verlangsamung: `SlowFactor` 0,5 (halbes Tempo), `SlowDuration` 2 s.

### Verhalten
- Verfolgt den Spieler direkt wie ein normaler Gegner.
- Bei jeder Berührung (höchstens 1× pro Sekunde): 2 Schaden **und** der Spieler wird
  2 Sekunden lang auf halbes Tempo verlangsamt. Bleibt die Müdigkeit dran, wird die Verlangsamung
  immer wieder erneuert.

### Designentscheidungen
- **Wenig Schaden, starker Effekt:** Die Müdigkeit ist selbst kaum gefährlich, macht aber die
  anderen Gegner gefährlich, weil man ihnen langsamer ausweicht (laut Präsentation
  „verlangsamt bei Berührung“).
- **Schnittstelle abgesichert** wie beim Trittbrettfahrer: `ApplySlow` wird am Spieler
  (Group `"player"`) nur aufgerufen, wenn es die Methode gibt. Sonst einmalige Warnung,
  der Kontaktschaden wirkt trotzdem.

### Benötigte Schnittstelle (mit Philipp abzusprechen)
```csharp
// in PlayerController
public void ApplySlow(float factor, double duration);
// factor: Anteil des normalen Tempos (0,5 = halb so schnell), duration: Sekunden.
// Mehrere Aufrufe stapeln sich nicht: stärkster Faktor gilt, Dauer wird erneuert.
```

### Test
- Headless: Müdigkeit neben den Spieler gesetzt, „Spieler getroffen! Schaden: 2“ einmal pro Sekunde,
  Warnung zu `ApplySlow` einmal.

## 16. Boss-Bausteine: Geschosse und Bodenangriffe

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/EnemyProjectile.cs`, `scenes/enemies/enemy_projectile.tscn`,
`scripts/enemies/GroundAttack.cs`, `scenes/enemies/ground_attack.tscn`, `WaveManager.cs`

### Umsetzung
- **Geschoss (`EnemyProjectile`, `Area2D`):** fliegt geradeaus (`Direction`, `Speed`), trifft den
  Spieler über `TakeDamage(Damage)` und verschwindet dabei, an Wänden und Hindernissen
  (`StaticBody2D`) oder nach `Lifetime` (5 s). Andere Gegner werden durchflogen.
  Platzhalter-Grafik: farbiger Kreis mit schwarzem Rand (`_Draw`).
- **Bodenangriff (`GroundAttack`):** zeichnet `WarningTime` Sekunden lang eine **Warnmarkierung**:
  ein durchsichtiger Kreis mit kräftigem Rand, dessen innere Füllung bis zum Einschlag wächst.
  Dann wird geprüft, ob der Spieler den Kreis berührt (`Radius` + 20 px), und einmal `Damage`
  ausgeteilt. Der Einschlag ist 0,2 s als voller Kreis zu sehen, dann verschwindet er.
- Beide sind in der Group `"enemy_attacks"` und werden beim Ortswechsel mit entfernt.

### Designentscheidungen
- **Nicht in der Group `"enemies"`:** Sonst würden Philipps Waffen auf Geschosse zielen,
  und jedes Geschoss würde beim Verschwinden XP geben.
- **Füllender Kreis als Countdown:** Man sieht nicht nur *wo*, sondern auch *wann* es einschlägt.
  Wer rechtzeitig aus dem Kreis läuft, bekommt keinen Schaden.
- **Werte kommen vom Boss:** Radius, Vorwarnzeit, Schaden und Farbe setzt der jeweilige Boss.
  Dieselben Bausteine dienen für Bomben (orange, 2 s) und Bodenausbrüche (rot, 1 s).
- **Über dem Boden, unter den Figuren** (`ZIndex = -5`): Die Markierung verdeckt weder
  Spieler noch Gegner.

### Test
- Über die Bosse in Abschnitt 18 und 19 getestet: Geschosse und Bodenangriffe verursachen
  Schaden („Spieler getroffen! Schaden: 8/10/18/20“).

## 17. Zwischenprüfungen und gemeinsames Boss-Skript `ExamBoss`

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/ExamBoss.cs`, `scripts/enemies/Boss.cs`,
`scenes/enemies/midterm_exam.tscn`, `scenes/enemies/midterm_programming.tscn`

### Umsetzung
- `ExamBoss : Boss` ergänzt schaltbare Angriffsmuster. Jeder Angriff ist aus, solange sein
  Intervall 0 ist. Alle Werte stehen per `[Export]` (in Gruppen) in der Boss-Szene:
  - **Bewegung:** `PreferredDistance` (0 = direkt verfolgen, sonst Abstand halten und
    seitlich um den Spieler kreisen, Kreisrichtung wechselt alle 4 s), `AttackRange`
    (greift nur an, wenn der Spieler näher ist, also im Bild).
  - **Gezielte Salve:** `AimedShots` Geschosse im Fächer (`AimedSpreadDegrees`) auf den Spieler.
  - **Ring:** `RingCount` Geschosse in alle Richtungen, jeder Ring um eine halbe Lücke gedreht.
  - **Bodenangriff:** Marker unter dem Spieler (`GroundAtPlayer`), zufällig um ihn
    (`GroundScatterCount`, `GroundScatterRadius`) und in einer Linie vom Boss zum Spieler
    (`GroundLineCount`), die nacheinander einschlägt (`GroundLineStagger`).
  - **Sturmangriff:** `DashWindup` Sekunden stehen bleiben und weiß blinken, dann `DashDuration`
    Sekunden mit `DashSpeed` geradeaus.
  - **Phase 2:** ab `Phase2HealthFraction` der Leben werden alle Intervalle mit
    `Phase2IntervalFactor` multipliziert, `Phase2ExtraScatter` Bodenmarker mehr, der Boss wird
    rötlich, optional ruft er erst jetzt Helfer (`SummonOnlyInPhase2`). Signal `PhaseChanged(int phase)`.
- `Boss` ruft nur Helfer, wenn `MinionScene` gesetzt ist, und setzt sie in die Arena (nicht hinter die Wand).

### Verhalten
- **Zwischenprüfung Mathe** (`midterm_exam.tscn`, gelb, Woche 5): 500 HP, Tempo 70,
  15 Kontaktschaden. Verfolgt den Spieler direkt und ruft alle 4 s drei Deadlines im Kreis um sich.
- **Zwischenprüfung Programmieren** (`midterm_programming.tscn`, türkis, Woche 10): 500 HP,
  Tempo 75, 15 Kontaktschaden. Verfolgt den Spieler. Alle 5 s **Sturmangriff**: 0,8 s stehen
  und blinken, dann 0,6 s mit Tempo 520 geradeaus in die Richtung, in der der Spieler am Ende
  des Blinkens steht. Alle 7 s ruft sie zwei Hausaufgaben-Stapel.

### Designentscheidungen
- **Ein Skript für alle neuen Bosse:** Neue Bosse oder Balancing brauchen keinen Code, nur Werte
  in der Szene. Die Grundlogik (Leben, Beschwörung, `HealthChanged`, Group `"bosses"`) bleibt in `Boss`.
  Dadurch funktionieren Boss-XP, Credits (Philipp) und Boss-Sounds (Tim) automatisch.
- **Richtung des Sturmangriffs erst am Ende des Blinkens:** Das Blinken ist die Warnung.
  Wer dann zur Seite geht, weicht aus.
- **500 HP:** Richtwert aus `design/balance-waffen.md` (Stift L4 brauchte für 150 HP nur 12,6 s).

### Test
- Headless (Spieler ohne Waffen): beide erscheinen, rufen Helfer, „Zwischenprüfung …: 0/500 HP“,
  „… bestanden!“, Boss-Zuschlag 100 XP wird gutgeschrieben.

### Probleme & Lösungen
- Im Test starb der still stehende Test-Spieler durch Sturmangriff und Stapel, danach hing das
  Testskript am gelöschten Boss. → Kein Spielfehler, im Testskript Spieler mit sehr vielen Leben.

## 18. Klausur-Bosse (Prüfungsphase)

**Datum:** 19.11.2026
**Dateien:** `scenes/enemies/exam_math.tscn`, `scenes/enemies/exam_programming.tscn`

### Verhalten
- **Klausur Mathe 1 – „Formel-Regen“** (hellblau, Scale 1,4): 1000 HP, Tempo 60, 20 Kontaktschaden.
  - Hält etwa 350 px Abstand und kreist seitlich um den Spieler.
  - Alle 2,2 s gezielter Fächer aus 3 Formel-Geschossen (24°, Tempo 230, je 8 Schaden).
  - Alle 3,5 s ein Ring aus 14 Geschossen, jeder Ring leicht gedreht, damit man nicht stehen bleiben kann.
  - Phase 2 ab 50 % HP: alle Angriffe 30 % häufiger.
- **Klausur Programmieren – „Pop-Quiz-Bombe“** (dunkelblau, Scale 1,4): 1200 HP, Tempo 70, 20 Kontaktschaden.
  - Hält etwa 300 px Abstand.
  - Alle 3 s drei Bomben: eine genau unter dem Spieler, zwei im Umkreis von 260 px.
    Jede zeigt 2 s einen orangen Warnkreis (Radius 110) und explodiert dann (20 Schaden).
  - Alle 6 s ruft sie vier Deadlines („Bugs“).
  - Phase 2 ab 50 % HP: Bomben 30 % häufiger, je zwei Bomben mehr.

### Designentscheidungen
- **Eigene Muster pro Klausur:** Mathe testet Ausweichen zwischen Geschossen (in Bewegung bleiben),
  Programmieren testet Positionierung (aus Flächen herauslaufen, während Helfer drängen).
  Das entspricht den Namen aus der Präsentation.
- **Stärker als Zwischenprüfungen:** doppelte bzw. mehr Leben, mehr Kontaktschaden, Fernangriffe
  und eine zweite Phase.
- **Werte:** HP nach Philipps Richtwert (Endklausur ca. 1500), auf zwei Klausuren und den Endboss verteilt.

### Test
- Headless: Mathe trifft mit Geschossen (8 Schaden), bis zu 35 Geschosse gleichzeitig.
  Programmieren legt Bomben (20 Schaden) und ruft Deadlines. Bei 50 % erscheint „Phase 2!“.

## 19. Endboss „Der Professor“

**Datum:** 19.11.2026
**Dateien:** `scenes/enemies/professor.tscn`

### Verhalten
- Dunkelgrau, groß (Scale 1,8, Hitbox 112), 1800 HP, Tempo 65, 25 Kontaktschaden.
  Erfundene Karikatur, keine echte Person.
- Hält etwa 400 px Abstand und kreist um den Spieler, greift bis 1000 px Entfernung an.
- **Geschosse:**
  - alle 1,6 s gezielter Fächer aus 5 Geschossen (32°, Tempo 320, je 10 Schaden),
  - alle 4,5 s ein Ring aus 16 Geschossen.
- **Bodenangriffe** alle 3,5 s, jeweils mit **1 s Warnmarkierung** (roter Kreis, Radius 80, 18 Schaden):
  - einer genau unter dem Spieler,
  - zwei zufällig daneben (bis 240 px),
  - eine Linie aus 5 Markern vom Professor zum Spieler, die nacheinander einschlägt
    (je 0,15 s später, wie eine Welle aus dem Boden).
- **Phase 2** ab 50 % HP: alle Angriffe 40 % häufiger, 3 zusätzliche Bodenmarker, rötliche Färbung,
  ruft ab jetzt alle 6 s drei Deadlines.
- Sieg über den Professor beendet das Semester (`SemesterFinished`).

### Designentscheidungen
- **Kombiniert alle Muster:** Geschosse wie Mathe, Bodenflächen wie Programmieren, dazu die Linie.
  Damit ist der Endboss eine Abschlussprüfung über alles, was man vorher gelernt hat.
- **Kurze Vorwarnung (1 s) statt 2 s wie bei den Bomben:** Der Endboss soll fordern, die Markierung
  reicht aber zum Herauslaufen (Spielertempo 300 px/s, Radius 80).
- **Helfer erst in Phase 2:** Am Anfang soll man die Muster lernen können.

### Test
- Headless: Geschosse (10) und Bodenausbrüche (18) treffen, bis zu 40 Angriffe gleichzeitig,
  in Phase 2 erscheinen Deadlines, nach dem Sieg „Der Professor bestanden!“.

## 20. Semesterablauf mit allen Gegnern und Bossen

**Datum:** 19.11.2026
**Dateien:** `scripts/enemies/WaveManager.cs`, `main.tscn`

### Umsetzung
- **Gegnerliste pro Woche** (mehrfach eingetragen = häufiger):

| Woche | Gegner in der Liste |
|---|---|
| 1–2 | 2× Standard |
| 3 | 2× Standard, 1× Deadline |
| 4 | + 1× Müdigkeit |
| 5 | + 1× Hausaufgaben-Stapel |
| 6–8 | + 1× Trittbrettfahrer |
| 9–14 | + 1× Deadline, + 1× Stapel (2× Standard, 2× Deadline, 1× Müdigkeit, 2× Stapel, 1× Trittbrettfahrer) |

- **Bosse:**

| Wann | Boss | Ort |
|---|---|---|
| Woche 5 | Zwischenprüfung Mathe | Outdoor-Campus |
| Woche 10 | Zwischenprüfung Programmieren | Mensa |
| nach Woche 14 | Klausur Mathe 1 → Klausur Programmieren → Der Professor | Hörsaal |

- Zwischenprüfungen erscheinen zu Beginn ihrer Woche. `BossWarningTime` (5 s) vorher kommt
  „Achtung: … in 5 s!“ und das Signal `BossWarning(string bossName, double secondsUntilSpawn)`.
- **Prüfungsphase:** Nach Ablauf von Woche 14 kommt „Prüfungsphase beginnt!“ und das Signal
  `ExamPhaseStarted`. Normale Gegner spawnen weiter, aber nur alle 0,8 s (`ExamPhaseSpawnInterval`),
  damit die Bosskämpfe spielbar bleiben. Die Klausur-Bosse aus `ExamBosses` kommen einzeln
  nacheinander, jeweils nach `ExamBossDelay` (5 s) mit Warnung. Erst wenn einer besiegt ist,
  beginnt die Pause vor dem nächsten. Nach dem letzten (Professor): `SemesterFinished`.
- `CurrentWeek` bleibt in der Prüfungsphase 14 (XP pro Kill und Credits wie in Woche 14).
  Neue Eigenschaft `InExamPhase`.
- Leere Einträge in den Listen werden übersprungen. Fehlt eine Boss-Szene, geht es mit der nächsten weiter.

### Designentscheidungen
- **Prüfungsphase nach Woche 14:** so in Präsentation und Game-Brief angekündigt.
- **Bosse nacheinander statt gleichzeitig:** Mit zwei Bossen gleichzeitig wurde es schon in
  Abschnitt 8 zu voll. Mit Fernangriffen wäre es unfair.
- **Zwischenprüfungen in Woche 5 und 10:** gleichmäßig über das Semester verteilt, jeweils
  in einem anderen Ort.
- **Warn-Signal für die UI:** In der Präsentation (Folie 7) ist eine „Warnung vor dem nächsten
  Prüfungs-Boss“ eingeplant. Fabian kann `BossWarning` dafür nutzen.

### Test
- Headless mit `WeekDuration = 2` und `ExamBossDelay = 1` (nur im Testskript, Szene unverändert),
  Bosse im Test sofort besiegt: Ortswechsel, Warnungen, Zwischenprüfungen in Woche 5 und 10,
  Prüfungsphase mit drei Bossen in der richtigen Reihenfolge, danach „Semester geschafft!“.
  Credits „Bosse 5“ wurden korrekt gezählt. Keine Fehlermeldungen.

### Probleme & Lösungen
- Im ersten Durchlauf blieb der Test in Woche 8 stehen: Die Level-Up-Auswahl pausiert das Spiel.
  → Im Testskript wird automatisch die erste Option gewählt.
- Die Testläufe haben über `MetaProgress` echte Credits im lokalen Spielstand gutgeschrieben.
  → Spielstand danach auf den Wert vor den Tests zurückgesetzt. Bei künftigen Tests beachten.

## 21. Werteübersicht aller Gegner

**Datum:** 19.11.2026

Alle Werte sind `[Export]` und im Inspektor der jeweiligen Szene in `scenes/enemies/` einstellbar.
Die Steigerung über das Semester (Spawn-Tempo, Leben ×1,06 pro Woche, Bosswochen) steht im `WaveManager`.
Die XP pro Kill legt Philipps `ExperienceSystem` fest (nach Woche, Bosse +100), nicht der Gegner.

| Typ | Szene | Tempo | Leben | Kontakt­schaden | Besonderes | Farbe / Scale |
|---|---|---|---|---|---|---|
| Standard | `enemy.tscn` | 120 | 10* | 5 | – | rot / 0,5 |
| Deadline | `deadline.tscn` | 220 | 5* | 3 | – | orange / 0,35 |
| Hausaufgaben-Stapel | `homework_stack.tscn` | 60 | 40* | 10 | – | lila / 0,8 |
| Müdigkeit | `tiredness.tscn` | 90 | 15* | 2 | verlangsamt auf 50 % für 2 s | graublau / 0,55 |
| Trittbrettfahrer | `free_rider.tscn` | 170 (Flucht 260) | 15* | 0 | klaut 25 XP, flieht 8 s | grün / 0,4 |
| Zwischenprüfung Mathe | `midterm_exam.tscn` | 70 | 500 | 15 | 3 Deadlines alle 4 s | gelb / 1,2 |
| Zwischenprüfung Programmieren | `midterm_programming.tscn` | 75 | 500 | 15 | Sturm alle 5 s, 2 Stapel alle 7 s | türkis / 1,2 |
| Klausur Mathe 1 | `exam_math.tscn` | 60 | 1000 | 20 | Fächer 3×8, Ring 14×8 | hellblau / 1,4 |
| Klausur Programmieren | `exam_programming.tscn` | 70 | 1200 | 20 | 3 Bomben (20) alle 3 s, 4 Deadlines | dunkelblau / 1,4 |
| Der Professor | `professor.tscn` | 65 | 1800 | 25 | Fächer 5×10, Ring 16×10, Boden 18 | dunkelgrau / 1,8 |

\* wächst pro Woche um 6 % (Abschnitt 13). Alle Bosse: Phase 2 ab 50 % HP (außer Zwischenprüfungen).

### Offene Punkte (Absprachen mit anderen)
- **Philipp:** `ExperienceSystem.StealXp(int)` und `AddXp(int)` für den Trittbrettfahrer
  (Abschnitt 14), `PlayerController.ApplySlow(float, double)` für die Müdigkeit (Abschnitt 15).
- **Philipp (optional):** XP pro Gegnertyp. Bisher gibt jeder Kill gleich viel XP.
  Möglich wäre ein Wert am Gegner, den das `ExperienceSystem` ausliest.
- **Fabian:** neue Signale für die UI: `WaveManager.BossWarning`, `ExamPhaseStarted`,
  `ExamBoss.PhaseChanged`. Boss-Lebensleiste weiterhin über `BossSpawned` + `HealthChanged`.
- **Tim:** Geschosse und Bodenangriffe haben noch keine Sounds. Sie sind in der Group
  `"enemy_attacks"`, falls er dafür Sounds anhängen will.
- Nach „Semester geschafft!“ bleiben die noch lebenden Gegner stehen. Das betrifft den
  Sieg-Bildschirm (Fabian).
- Balancing der Bossangriffe mit echtem Spieler und Waffen steht noch aus.

## 22. Schnittstellen zum Spieler (`PlayerBridge`)

**Datum:** 20.11.2026
**Dateien:** `scripts/level/PlayerBridge.cs`, `scripts/level/LevelElement.cs`,
`scripts/enemies/FreeRider.cs`, `scripts/enemies/Tiredness.cs`

### Umsetzung
- `PlayerBridge` bündelt alle Aufrufe an Philipps Code an einer Stelle: `AddXp`, `StealXp`,
  `Heal`, `RestoreShield`, `ApplySlow`. Jede Methode wird nur aufgerufen, wenn es sie gibt
  (`HasMethod`, dann `Call`). Fehlt sie, kommt pro Spielstart **eine** Warnung
  „Schnittstelle fehlt: …“, und der Effekt wird übersprungen (Rückgabe `false` bzw. 0).
- Trittbrettfahrer und Müdigkeit nutzen jetzt ebenfalls `PlayerBridge` (Verhalten unverändert).
- `LevelElement` ist die Basis aller interaktiven Elemente. Sie hat das Signal
  `Triggered(string eventName)` für Sounds und einen schwebenden Text (z. B. „+50 XP“).
  Gemeinsame Zeichenebenen: Raumboden −9, Elemente −4 (unter Spieler und Gegnern), Text 20.

### Benötigte Methoden (mit Philipp abzusprechen)

| Klasse | Signatur | Wofür |
|---|---|---|
| `ExperienceSystem` | `int StealXp(int amount)` | Trittbrettfahrer |
| `ExperienceSystem` | `void AddXp(int amount)` | Trittbrettfahrer (zurückgeben), Kiste, XP-Loot |
| `PlayerStats` | `void Heal(int amount)` | Kiste, später Essensausgabe (Mensa) |
| `PlayerStats` | `void RestoreShield(int amount)` | Energy-Drink-Automat, Schild-Loot |
| `PlayerController` | `void ApplySlow(float factor, double duration)` | Müdigkeit |

### Ereignisse für Sounds (Tim)
Alle Elemente senden `Triggered` mit diesen Namen: `tuer_auf`, `tuer_zu`, `kiste_auf`,
`energydrink`, `automat_bereit`, `spind_auf`, `pickup`. Im `SoundManager` reicht eine Zeile
in `OnNodeAdded`: `else if (node is LevelElement element) element.Triggered += PlayWennDa;`.
`LockerRow` ist ein Hindernis (`StaticBody2D`) und hat deshalb ein eigenes, gleichnamiges Signal,
dafür eine zweite Zeile: `else if (node is LockerRow lockers) lockers.Triggered += PlayWennDa;`.

## 23. Leveldesign Gänge: Seminarraum, Energy-Drink-Automat, Spinde

**Datum:** 20.11.2026
**Dateien:** `scripts/level/SideRoom.cs`, `AutoDoor.cs`, `LootChest.cs`, `Pickup.cs`,
`EnergyDrinkMachine.cs`, `LockerRow.cs`, `NoSpawnZone.cs`, `scenes/level/locker_row.tscn`,
`scenes/level/arena_gaenge.tscn`, `scripts/enemies/Enemy.cs`, `scripts/enemies/EnemySpawner.cs`

### Umsetzung und Verhalten
- **Seminarraum** (`SideRoom`, am linken Ende der Gänge, Innenmaß 300 × 760, Tür rechts):
  Wände und Boden werden per Code erzeugt. Mit `[Tool]` ist der Raum schon im Editor sichtbar.
  Im Raum und direkt an seinen Wänden spawnen keine Gegner (`NoSpawnZone`).
- **Automatische Tür** (`AutoDoor`, 192 px breit): Zwei Flügel gleiten per Tween in 0,3 s
  auseinander, sobald der Spieler näher als 200 px ist. Sie schließt frühestens 1 s nach dem
  Weggehen und nur, wenn niemand in der Tür steht und der Raum leer ist. Läuft beim Schließen
  jemand hinein, geht sie wieder auf.
- **Wegpunkte durch die Tür:** Sind Spieler und Gegner in verschiedenen Räumen, setzt der Raum
  alle 0,25 s einen Wegpunkt (`Enemy.Waypoint`). Ist der Spieler drinnen, laufen die Gegner
  draußen erst vor die Tür und dann hinein. Ist der Spieler draußen, laufen die Gegner drinnen
  erst zur Tür und dann hinaus. Bosse und der fliehende Trittbrettfahrer folgen keinen Wegpunkten.
- **Kiste** (`LootChest`, im Seminarraum): pulsiert leicht, solange sie zu ist. Bei Berührung
  klappt der Deckel hoch, sie ploppt kurz auf und gibt **50 XP und 15 Leben**. Sie öffnet nur
  einmal pro Ortsbesuch (`RespawnCooldown = 0`).
- **Energy-Drink-Automat** (`EnergyDrinkMachine`, an der oberen Wand bei x = 100): fester
  Körper. Bei Berührung gibt es **+25 Schild**, danach **45 s Cooldown**. In der Zeit ist er grau,
  und ein grüner Ring füllt sich. Ist der Schild schon voll, erscheint „Schild voll“ und es wird
  nichts verbraucht.
- **Spinde** (`LockerRow`, alle acht Spindreihen): in Spinde unterteilt (1 Kachel = 1 Spind).
  Jede Reihe springt alle 60–120 s auf (die erste Öffnung zufällig früher), insgesamt etwa
  alle 10 s irgendwo. Die Tür klappt in den Gang, die Reihe blitzt auf, und ein Loot springt
  heraus. 60 % sind **10 XP** (blauer Kristall), sonst **10 Schild** (türkiser Kreis).
  Nach 2 s geht der Spind wieder zu.
- **Loot** (`Pickup`): schwebt leicht und wird bei Berührung eingesammelt („+10 XP“). Er blinkt
  ab 17 s und verschwindet nach 20 s.

### Designentscheidungen
- **Wegpunkte statt Pathfinding:** Eine Tür ist die einzige Engstelle. Ein Wegpunkt pro Gegner
  kostet fast nichts und verhindert, dass Gegner an der Raumwand festhängen.
- **Tür schließt nie auf jemandem:** Sie schließt erst, wenn die Tür frei und der Raum leer ist.
  Kein Gegner wird eingesperrt, niemand eingeklemmt, und Spieler oder Gegner werden nicht von der Kollision weggeschoben.
- **Raum am Rand, Tür zum Gang:** Der Gang bleibt frei. Ober- und unterhalb des Raums bleiben
  mindestens 160 px Durchgang. Keine Bosse in den Gängen, also kein Problem mit großen Hitboxen.
- **Benutzen durch Berühren statt Taste:** Die Eingaben in `project.godot` sind mit allen geteilt.
- **Energy-Drink statt Kaffee:** laut Präsentation Energy-Drink = Schild, Kaffee = Tempo-Schub.
- **Performance:** Tür und Raum prüfen nur alle 0,1 bzw. 0,25 s. Der Automat läuft nur während
  des Cooldowns und zeichnet dann 5× pro Sekunde neu. Tweens laufen nur beim Animieren,
  das Pulsieren nur, solange Kiste bzw. Loot da sind. Keine Partikel, keine Shader.

### Test
Automatisch headless mit `tests/headless/level_test.gd` (Modus `gaenge`, Spawner und Waffen aus),
alle 14 Prüfungen bestanden:
- Automat bleibt bei vollem Schild bereit. Nach Schaden löst er aus und geht in Cooldown.
- Tür geht auf, wenn der Spieler davor steht, und bleibt offen, solange er im Raum ist.
- 4 Gegner im Gang, Spieler im Raum: Gegner kommen durch die Tür herein. Spieler wieder im Gang:
  nach 12 s ist kein Gegner mehr im Raum. Danach schließt die Tür.
- Kiste öffnet bei Berührung, ein zweites Mal nicht.
- Spinde springen auf (im Test mit 1–2 s Abstand), Loot liegt da, wird eingesammelt und
  verschwindet nach 20 s.
- Spieler im Raum, 40 Gegner über den Spawner erzeugt: keiner im Raum.

### Probleme & Lösungen
- Der erste Test „Automat bei vollem Schild“ schlug fehl. Die Test-Gegner hatten den Spieler vorher
  getroffen, der Automat hat also richtig aufgefüllt. → Test vor das Erzeugen der Gegner verschoben.

### Offene Punkte
- XP, Leben und Schild aus Kiste, Automat und Loot wirken erst, wenn Philipp `AddXp`, `Heal` und
  `RestoreShield` eingebaut hat. Bis dahin öffnen und animieren die Elemente, und es kommt je eine
  Warnung. Der „+…“-Text erscheint nur, wenn der Effekt wirklich angewendet wurde.

## 24. Test-Option `TestStartWeek` und Headless-Tests

**Datum:** 20.11.2026
**Dateien:** `scripts/enemies/WaveManager.cs`, `tests/headless/level_test.gd`, `tests/headless/run_tests.sh`

### Umsetzung
- `WaveManager.TestStartWeek` (Inspektor, Standard 1 = normales Spiel): Das Spiel startet direkt
  in dieser Woche, also auch am passenden Ort und mit ihrer Zwischenprüfung. Ein Wert größer
  als `TotalWeeks` (z. B. 15) startet direkt die Prüfungsphase.

| Ort testen | TestStartWeek |
|---|---|
| Gänge | 1 |
| Outdoor-Campus (+ Zwischenprüfung Mathe) | 3 (5) |
| Bibliothek | 6 |
| Mensa (+ Zwischenprüfung Programmieren) | 9 (10) |
| Hörsaal | 12 |
| Prüfungsphase | 15 |

- `bash tests/headless/run_tests.sh [Ordner]` baut das Projekt und startet:
  - den Gänge-Test,
  - einen 60-s-Dauerlauf für die Wochen 1, 3, 5, 6, 9, 10, 12 und 15 (Spieler mit sehr vielen
    Leben, Waffen an, Level-Ups werden automatisch gewählt). Dabei werden alle 5 s Gegner,
    Bosse, Geschosse, Bodenangriffe, Pickups und alle Nodes gezählt.
- Das Skript sichert vorher den lokalen Spielstand (`meta_progress.json`) und stellt ihn danach
  wieder her, weil Testläufe über `MetaProgress` Credits schreiben können.
- Die Tests liegen in einem eigenen Ordner und sind in keiner Spielszene eingebunden.

### Test (20.11.2026)
- Build ohne Fehler, in keinem Lauf eine `ERROR`-Meldung. Warnungen nur „Schnittstelle fehlt“,
  jede einmal pro Lauf.
- Node-Anzahl bleibt begrenzt (zusätzlich 180-s-Läufe für Woche 5, 10 und 15):
  - Normale Gegner: höchstens 50 (`MaxEnemies`).
  - Mit Boss: bis 64, weil Boss-Helfer nicht unter die Grenze fallen.
  - Geschosse der Klausur Mathe pendeln zwischen 18 und 33 (Lebensdauer 5 s).
  - Insgesamt höchstens etwa 260 Nodes. Beim Ortswechsel fällt die Zahl wieder.
- Spielstand nach den Tests unverändert.

### Offene Punkte
- ~~Boss-Helfer zählen nicht zur Obergrenze `MaxEnemies`.~~ Erledigt am 20.11.2026: neue Obergrenze
  `Boss.MaxMinionsAlive` (Standard 12). Ein Boss ruft nur so viele Helfer, bis 12 eigene Helfer
  leben. Besiegte oder beim Ortswechsel entfernte Helfer werden vor jeder Beschwörung ausgetragen.
  Test (180 s): Woche 5 höchstens 57 Gegner mit zwei Bossen (vorher 64), Woche 10 höchstens 53.
- Nach einem fertigen 180-s-Lauf brach Godot einmal beim Beenden ab
  („mutex lock failed“, nach der Ergebnisausgabe). Der Test war da schon vollständig. Das ist ein
  Problem beim Herunterfahren im Headless-Skriptmodus, kein Spielfehler. Im vollständigen Testlauf
  danach (8 Dauerläufe und der Gänge-Test, 20.11.2026) trat es nicht mehr auf. Weiter beobachten.

## 25. Übersicht: Orte und ihre Elemente

| Ort (Wochen) | Hindernisse | Interaktive Elemente | Stand |
|---|---|---|---|
| Gänge (1–2) | Spindreihen, Schwarzes Brett, Mülleimer | Seminarraum mit automatischer Tür und Kiste, Energy-Drink-Automat, aufspringende Spinde mit Loot | fertig (20.11.2026) |
| Outdoor-Campus (3–5) | – | Rasensprenger (bremst Gegner), Radweg mit Fahrrad | geplant |
| Bibliothek (6–8) | Bücherregale | Ruhezone (Gegner halb so schnell) | geplant |
| Mensa (9–11) | Tische | Essensausgabe (1× pro Woche Heilung), Energy-Drink-Automat | geplant |
| Hörsaal (12–14, Prüfungsphase) | Sitzreihen am Rand | Tafel reagiert auf Boss-Warnung | geplant |
