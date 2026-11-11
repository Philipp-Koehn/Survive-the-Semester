# Story 004: Upgrade-Auswahl pro Level

> **Epic**: spieler-waffen
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 4

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Beim Level-Aufstieg werden zufällig Optionen angeboten, jede ist ein Upgrade für eine vorhandene Waffe oder eine neue Waffe (z. B. drei neue Waffen, oder Upgrades plus eine neue Waffe).
- [ ] Sind alle 4 Slots belegt, werden keine neuen Waffen mehr angeboten.
- [ ] Der Spieler wählt genau eine Option; sie wird sofort angewendet.
- [ ] Jede Waffe hat maximal 4 Upgrade-Level; maximierte Waffen bieten keine weiteren normalen Upgrades.
- [ ] Nach Level 4 wird ein ultimatives Upgrade für diese Waffe angeboten (Mechanik wie bei Vampire Survivors).
- [ ] Das Spiel pausiert während der Auswahl.
- [ ] Upgrade-Werte und Optionen-Anzahl sind datengetrieben (Ressourcen oder `[Export]`).
- [ ] Ohne passende Optionen wird die Auswahl übersprungen (Edge Case: alle Waffen auf Maximum).

---

## Implementation Notes

- Level-Aufstieg-Quelle: XP/Level-System; in dieser Story als Signal `LeveledUp` am Spieler, Auslöser kann zunächst ein Test-Trigger sein.
- Anzeige der Auswahl ist UI (Fabian); hier nur Logik plus ein Platzhalter (Konsole/Debug).

---

## Out of Scope

- Auswahl-UI und Grafik: Fabian (Vorschlag).
- Credit-System und Meta-Progression (Zukunftsplan).

---

## Files

- `scripts/Player/UpgradeManager.cs`
- `scripts/Player/Weapons/` (Upgrade-Daten)
- `tests/unit/spieler/upgrade_manager_test.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/upgrade_manager_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-003-waffen-slots.md`
- Unlocks: None
