# Story 005: Spieler-Werte: Leben, Schaden, Schild, Krit

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-11

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 5

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Der Spieler hat Leben, Schaden, Krit-Chance, Bewegungstempo und Schild als Werte, per `[Export]` oder Daten-Ressource einstellbar.
- [ ] `TakeDamage(int)` reduziert zuerst das Schild, dann das Leben (Group `"player"`).
- [ ] Krit-Chance lässt Treffer mit erhöhtem Schaden zu; der Krit-Multiplikator ist einstellbar.
- [ ] Gegner-Kontaktschaden (Cooldown 1 s) wirkt auf den Spieler; Leben fällt nicht unter 0.
- [ ] Bei Leben 0 wird ein `Died`-Signal ausgelöst (verarbeitet in Story 006).
- [ ] Werte können später durch Upgrades verändert werden (Story 004).

---

## Implementation Notes

- Werte in einer eigenen Komponente `PlayerStats`, damit Waffen und Upgrades darauf zugreifen.
- Weitere Werte lassen sich ergänzen (Brief: „weitere Werte können ergänzt werden“).

---

## Out of Scope

- Story 006: Tod/Menü.
- HUD für Leben/Schild: Fabian (Vorschlag).

---

## Files

- `scripts/Player/PlayerStats.cs`
- `scripts/Player/PlayerController.cs` (TakeDamage leitet an PlayerStats, Stats-Property)
- `scripts/Player/Weapons/Weapon.cs` (Schaden mit Multiplikator und Krit)
- `scenes/Player/PlayerController.tscn` (PlayerStats-Knoten)

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/player_stats_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-001-spielerbewegung-kamera.md`
- Unlocks: `production/epics/spieler-waffen/story-006-tod-hauptmenue.md`

---

## Completion Notes
**Completed**: 2026-11-11
**Criteria**: 6/6 passing (keine deferred)
**Deviations**:
- Das Schild lädt sich nicht wieder auf (späterer Balancing-Punkt).
- Konsolenausgabe bei Treffer und Tod in `PlayerStats.cs` ist temporär, bis es eine Lebensanzeige (UI, Fabian) gibt.
- Bewegungstempo bleibt als `MoveSpeed` am `PlayerController`.
**Test Evidence**: waived at qa.level minimal
**Reviewed by**: self, no independent review
