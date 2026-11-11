# Story 001: Spielerbewegung und Kamera

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Core
> **Type**: Visual/Feel
> **Estimate**: S
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-11

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Der Spieler bewegt sich in 8 Richtungen über `move_*`-Eingaben; Diagonalen sind nicht schneller.
- [ ] `MoveSpeed` ist per `[Export]` im Inspektor einstellbar.
- [ ] Eine Kamera folgt dem Spieler; Bewegung ist auf dem Arena-Boden erkennbar.
- [ ] Spieler startet bei (0, 0) und ist in der Group `"player"`.
- [ ] Spieler stoppt im selben Frame, in dem keine Taste gedrückt ist.

---

## Implementation Notes

- `scripts/Player/PlayerController.cs` existiert (Bewegung); Kamera und Group-Zuweisung in `scenes/Player/PlayerController.tscn` prüfen/ergänzen.
- Motion Mode Floating (siehe `docs/gegner-ki.md`).

---

## Out of Scope

- Story 002: Waffen.
- Story 005: Leben und Schaden.

---

## Files

- `scripts/Player/PlayerController.cs`
- `scenes/Player/PlayerController.tscn`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory), but a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Visual/Feel
**Required evidence**: retained screenshot in `production/qa/evidence/` + sign-off in `production/qa/evidence/story-001-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: `production/epics/spieler-waffen/story-002-startwaffe.md`, `production/epics/spieler-waffen/story-005-spieler-werte.md`

---

## Completion Notes
**Completed**: 2026-11-11
**Criteria**: 5/5 passing (keine deferred)
**Deviations**: `main.tscn` und `project.godot` (Input-Map) wurden mit der Story committet, stehen aber nicht unter `## Files` (mit Mert abgestimmt). In `story-001-evidence.md` sind die Punkte unter „Noch offen“ nicht abgehakt, der Sign-off ist eingetragen.
**Test Evidence**: `production/qa/evidence/story-001-spieler-arena.png` und `story-001-evidence.md`
**Reviewed by**: self, no independent review
