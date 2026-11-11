# Evidenz: Story 001 – Spielerbewegung und Kamera

> Written for: Team (Abnahme von Story 001).

**Story:** `production/epics/spieler-waffen/story-001-spielerbewegung-kamera.md`
**Typ:** Visual/Feel
**Datum:** 2026-11-11
**Godot:** 4.7.2 Mono (Build und Start headless geprüft, ohne Fehler)

## Screenshot

`production/qa/evidence/story-001-spieler-arena.png`: Spieler (Godot-Icon) in der Mitte des Schachbrett-Bodens, Kamera folgt dem Spieler.

## Geprüft

- [x] Build der C#-Lösung: 0 Warnungen, 0 Fehler.
- [x] Hauptszene headless gestartet: keine Fehler mehr (vorher: `move_*`-Aktionen fehlten, behoben durch Input-Map in `project.godot`).
- [x] Spieler in Group `"player"` (Code-Änderung in `PlayerController.cs`).
- [x] Kamera folgt dem Spieler (Screenshot).

## Noch offen (Sichtprüfung durch Mensch)

- [ ] Bewegung in 8 Richtungen mit WASD und Pfeiltasten; Diagonalen nicht schneller.
- [ ] Der Spieler stoppt, sobald keine Taste gedrückt ist.
- [ ] Gegner laufen auf den Spieler zu (Kontakt in der Konsole sichtbar).

## Sign-off

Freigabe durch: philipp,11.11.2026______ (Name, Datum)
