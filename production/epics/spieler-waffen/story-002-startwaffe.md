# Story 002: Startwaffe mit Grundschaden

> **Epic**: spieler-waffen
> **Status**: In Progress
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-11

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 2

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Es gibt eine Waffen-Basisklasse mit Datenwerten (Name, Schaden, Abklingzeit, Reichweite, Typ), alle per `[Export]` oder Daten-Ressource einstellbar, nicht im Code fest.
- [ ] Eine Schulsachen-Startwaffe (z. B. Stift) existiert und feuert automatisch auf Gegner in Reichweite.
- [ ] Treffer rufen `TakeDamage(int)` an Gegnern der Group `"enemies"` auf.
- [ ] Die Waffe respektiert ihre Abklingzeit.
- [ ] Treffer-Schaden nutzt den Schadenswert; Krit wird in Story 005 ergänzt.

---

## Implementation Notes

- Waffe als eigenständige Szene, vom Spieler instanziert; Gegner werden über die Group `"enemies"` gefunden (Schnittstelle: `docs/gegner-ki.md`).
- Neue Waffen später als geerbte Szenen/Daten, damit Story 003/004 sie erweitern können.

---

## Out of Scope

- Story 003: Slots und Wechsel.
- Story 004: Upgrades.
- Credit-Freischaltung weiterer Startwaffen (Zukunftsplan).

---

## Files

- `scripts/Player/Weapons/Weapon.cs`
- `scenes/Player/Weapons/Pen.tscn`
- `scenes/Player/PlayerController.tscn`

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/weapon_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-001-spielerbewegung-kamera.md`
- Unlocks: `production/epics/spieler-waffen/story-003-waffen-slots.md`
