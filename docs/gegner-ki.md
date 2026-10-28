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