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