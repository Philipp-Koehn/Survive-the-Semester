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