# Story 007: XP- und Level-System

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-16

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief „Spieler“ (XP-/Level-System, vom Nutzer ergänzt; kein MVP-Punkt)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Der Spieler startet auf Level 1 mit 0 XP. Level und XP sind lesbar und ändern sich über das Signal `XpChanged`.
- [ ] Jeder besiegte Gegner gibt XP. Die Menge hängt von der aktuellen Woche ab: je später im Semester, desto mehr XP (Kurve als `[Export]`).
- [ ] Bosse geben über die Group `"bosses"` einen einstellbaren Zuschlag.
- [ ] Der XP-Bedarf steigt pro Level: Level 2 braucht die Basis-Menge, jedes weitere Level braucht mehr (Kurve als `[Export]`).
- [ ] Bei genug XP steigt das Level, `LeveledUp` wird ausgelöst und `UpgradeManager.TriggerLevelUp()` startet die Auswahl.
- [ ] Überschüssige XP bleiben erhalten und zählen für das nächste Level.
- [ ] Steigt der Spieler durch viele XP auf mehrere Level auf einmal, kommen die Auswahlen nacheinander, nicht gleichzeitig.
- [ ] Nach dem Tod gibt es keine XP und kein Level-Up mehr. Ein neuer Run beginnt wieder bei Level 1 mit 0 XP.
- [ ] Taste 9 bleibt als Debug-Auslöser erhalten.

---

## Implementation Notes

- Die Wochenzahl kommt aus dem Signal `WeekChanged(int week)` des `WaveManager` (Mert). Diese Story liest nur das Signal und ändert keine Gegner- oder Wave-Dateien.
- Die XP-Kurven (XP pro Kill nach Woche, Bedarf pro Level, Boss-Zuschlag) sind `[Export]`-Werte und werden in Story 009 abgestimmt.
- Die Anzeige von Level und XP ist UI (Fabian). Hier gibt es nur Signale und Abfragen.

---

## Out of Scope

- HUD-Balken für XP und Level: Fabian.
- Credits aus XP oder Level: Story 008.

---

## Files

- `Survive-the-Semester/scripts/Player/ExperienceSystem.cs` — created (XP, Level, Kurven als [Export], Signale XpChanged/LeveledUp, Level-Up-Warteschlange)
- `Survive-the-Semester/scenes/Player/PlayerController.tscn` — modified (ExperienceSystem-Knoten unter PlayerController)

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/xp_level_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-004-upgrade-auswahl.md` (Level-Up-Auswahl)
- Unlocks: `production/epics/spieler-waffen/story-008-credit-system.md`, `production/epics/spieler-waffen/story-009-balancing-durchgang.md`

---

## Completion Notes

**Completed**: 2026-11-16
**Criteria**: 9/9 passing (keine zurückgestellt)
**Deviations**: Out of scope, nicht unter `## Files` gelistet: `scripts/Player/UpgradeManager.cs` wurde geändert (`SetInputAsHandled()` nach der Auswahl, damit Taste 2 nicht zugleich Waffenslot 2 schaltet). Das `GD.Print`-Debug-Log in `ExperienceSystem.cs` ist noch enthalten.
**Test Evidence**: waived at qa.level minimal
**Reviewed by**: self, no independent review
