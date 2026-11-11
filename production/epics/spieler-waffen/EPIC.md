# Epic: spieler-waffen

> **Ziel**: Spielerbewegung, Waffen, Upgrades und Spieler-Werte als spielbare Basis (Kern-Prototyp).
> **Quelle**: `design/game-brief.md` (MVP-Punkte 1-6; Punkt 7, Wochen-Fortschritt, liegt bei Mert und existiert).
> **Bereich**: Philipp (Spieler, Steuerung, Waffen, Upgrades, Waffen-Balancing).

## Scope

Bewegung und Kamera, Startwaffe, bis zu 4 Waffen-Slots mit Wechsel, Upgrade-Auswahl pro Level (Level 4, dann Ultimate), Spieler-Werte (Leben, Schaden, Krit, Schild), Tod mit Rückkehr ins Hauptmenü.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | Spielerbewegung und Kamera | Visual/Feel | Ready | N/A |
| 002 | Startwaffe mit Grundschaden | Logic | Ready | N/A |
| 003 | Waffen-Slots und Waffenwechsel | Logic | Ready | N/A |
| 004 | Upgrade-Auswahl pro Level | Logic | Ready | N/A |
| 005 | Spieler-Werte: Leben, Schaden, Schild, Krit | Logic | Ready | N/A |
| 006 | Tod und Rückkehr ins Hauptmenü | Integration | Ready | N/A |

## Schnittstellen zu Kollegen (nur Vorschläge)

- Upgrade-Auswahl-Anzeige und HUD: Fabian (UI).
- Hauptmenü-Szene: Fabian.
- Gegner treffen über Group `"player"` und `TakeDamage(int)`: siehe `docs/gegner-ki.md`.
