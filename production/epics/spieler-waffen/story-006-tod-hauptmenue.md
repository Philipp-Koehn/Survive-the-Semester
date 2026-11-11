# Story 006: Tod und Rückkehr ins Hauptmenü

> **Epic**: spieler-waffen
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 6

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Bei `Died` des Spielers endet der Run: Eingabe, Waffen und Gegner-Spawn stoppen.
- [ ] Nach kurzer Verzögerung wechselt das Spiel zur Hauptmenü-Szene.
- [ ] Ein neuer Run startet mit zurückgesetzten Werten, Waffen und Level (nur Startwaffe).
- [ ] Der Szenenpfad des Hauptmenüs ist per `[Export]` einstellbar, bis die Menü-Szene existiert.
- [ ] Ohne Hauptmenü-Szene erscheint eine klare Warnung statt eines Absturzes.

---

## Implementation Notes

- Tod-Logik reagiert auf das `Died`-Signal aus Story 005; Szenenwechsel über `GetTree().ChangeSceneToFile`.
- Das Hauptmenü gehört Fabian: nur den Aufruf bauen, die Menü-Szene nicht selbst anlegen.

---

## Out of Scope

- Hauptmenü und Game-Over-Anzeige: Fabian (Vorschlag).
- Credits-Vergabe beim Tod (Zukunftsplan).

---

## Files

- `scripts/Player/PlayerController.cs`
- `scripts/Player/RunManager.cs`
- `tests/integration/spieler/death_flow_test.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Integration
**Required evidence**: `tests/integration/spieler/death_flow_test.cs` OR playtest doc

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-005-spieler-werte.md`
- Unlocks: None
