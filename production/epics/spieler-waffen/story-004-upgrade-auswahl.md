# Story 004: Upgrade-Auswahl pro Level

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-11

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
- `scripts/Player/Weapons/Weapon.cs` (Level, Upgrade, Ultimate)
- `scripts/Player/WeaponInventory.cs` (IsFull, Weapons; Test-Hook von Story 003 entfernt)
- `scenes/Player/PlayerController.tscn` (UpgradeManager mit Waffen-Pool)
- `scenes/Player/Weapons/Laptop.tscn` (dritte Waffe)
- `project.godot` (upgrade_pick_1 bis 3, debug_level_up)

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

---

## Completion Notes
**Completed**: 2026-11-11
**Criteria**: 8/8 passing (keine deferred)
**Deviations**:
- Level 1 bis 4 heißt: Start auf Level 1, drei Aufwertungen, dann das Ultimate (mit dir abgestimmt).
- Die Regel „volle Slots bieten keine neue Waffe“ ist nur über einen temporären Selbsttest belegt, nicht von Hand (der Waffen-Pool hat nur 3 Waffen).
- Taste 9 ist ein temporärer Level-Up-Auslöser, bis es ein XP-System gibt.
- Die Auswahl erscheint nur in der Konsole, die Anzeige gehört Fabian.
**Test Evidence**: waived at qa.level minimal
**Reviewed by**: self, no independent review
