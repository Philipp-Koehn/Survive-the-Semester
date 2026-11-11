# Story 003: Waffen-Slots und Waffenwechsel

> **Epic**: spieler-waffen
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: —

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 3

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Der Spieler hat maximal 4 Waffen-Slots; die Startwaffe belegt Slot 1.
- [ ] Eine neue Waffe wird im nächsten freien Slot eingefügt; bei vollen Slots wird keine neue Waffe hinzugefügt.
- [ ] Der Spieler kann im Spiel zwischen den Waffen wechseln (eigene Eingabeaktion, z. B. Zahlentasten und Mausrad).
- [ ] Es gibt mindestens zwei Waffentypen: Einzelziel (Single Target) und Flächenschaden (AoE).
- [ ] Die aktive Waffe ist erkennbar (Debug-Ausgabe oder Marker; HUD-Anzeige ist Fabian vorgeschlagen).
- [ ] Der Waffenwechsel hat keinen unerwünschten Cooldown-Reset (Abklingzeit bleibt pro Waffe erhalten).

---

## Implementation Notes

- `WeaponInventory`-Komponente am Spieler; Slot-Limit per `[Export]` (Standard 4).
- AoE-Waffe als zweite Basisvariante (z. B. Taschenrechner) für den Test des Wechsels.

---

## Out of Scope

- Story 004: Wie neue Waffen angeboten werden (Zufallsauswahl).
- HUD-Anzeige der Slots: Fabian (Vorschlag).

---

## Files

- `scripts/Player/WeaponInventory.cs`
- `scripts/Player/Weapons/` (AoE-Waffe)
- `tests/unit/spieler/weapon_inventory_test.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/weapon_inventory_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-002-startwaffe.md`
- Unlocks: `production/epics/spieler-waffen/story-004-upgrade-auswahl.md`
