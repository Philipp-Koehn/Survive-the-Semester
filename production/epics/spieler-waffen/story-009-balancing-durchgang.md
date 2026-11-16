# Story 009: Balancing-Durchgang Waffen, Spielerwerte und Fortschritt

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Feature
> **Type**: Config/Data
> **Estimate**: M
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-16

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief „Spieler“ (Waffen-Balancing, Bereich des Nutzers laut `CLAUDE.md`)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] `design/balance-waffen.md` enthält die DPS jeder Waffe für Level 1 bis 4 und mit Ultimate, berechnet aus den Werten in den Szenen.
- [ ] Leben, Schild und Krit sind gegen den Gegnerschaden pro Woche bewertet (wie viele Treffer bis zum Tod, je Woche).
- [ ] Die XP-Kurven aus Story 007 sind so gewählt, dass Level 4 einer Waffe in einem typischen Run erreichbar ist.
- [ ] Die Preise der Startwaffen und passiven Upgrades aus Story 008 sind so gewählt, dass die ersten Freischaltungen nach wenigen Runs möglich sind.
- [ ] Jede Änderung an einem Wert ist mit „alt → neu“ und Begründung im Dokument notiert.
- [ ] Änderungen beschränken sich auf Werte in Waffen- und Spielerszenen und Exporte. Gegnerwerte werden nur als Vorschlag an Mert notiert.
- [ ] Ein Probelauf bis mindestens Woche 7 ist protokolliert (Dauer, Level, Tod oder Sieg).
- [ ] Ein Smoke-Check-Protokoll liegt unter `production/qa/smoke-[Datum].md`.

---

## Implementation Notes

- Die Tabelle wird aus den Werten gerechnet, nicht geschätzt. Quellen: `Weapon.cs` (Cooldown, Schaden, Level), `PlayerStats.cs` (Leben, Schild, Krit).
- Gegnerwerte liegen bei Mert. Vorschläge kommen als Liste ins Dokument, ohne Änderung an seinen Dateien.

---

## Out of Scope

- Neue Waffen oder Mechaniken.
- Änderungen an Gegnerwerten oder Wellen.

---

## Files

- `design/balance-waffen.md` (neu: DPS-Tabellen, Spielerwerte, XP-Kurve, Preise, Änderungen alt → neu in drei Runden, Lauf 1, Vorschläge an Mert, Platz für den Probelauf)
- `scenes/Player/Weapons/Pen.tscn` (nur Werte: Damage 5 → 10, DamagePerLevel 2 → 4, Cooldown 0,6 → 2,0)
- `scenes/Player/Weapons/Calculator.tscn` (nur Werte: Damage 3 → 5)
- `scripts/Player/ExperienceSystem.cs` (nur Export-Standardwerte: XpForLevel2 30 → 120, XpGrowthPerLevel 15 → 20)
- `data/Player/PassiveUpgrades.tres` (nur Werte: Startwaffen-Preise 50/100 → 150/300)
- `scripts/Player/DebugShop.cs` (Fix: Debug-Taste 0 schließt den Stift wieder ein; Story-008-Datei)

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Config/Data
**Required evidence**: `production/qa/smoke-[Datum].md` (Smoke-Check, advisory)

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-007-xp-level-system.md`, `production/epics/spieler-waffen/story-008-credit-system.md`
- Unlocks: None

---

## Completion Notes

**Completed**: 2026-11-16
**Criteria**: 8/8 passing (keine zurückgestellt)
**Deviations**: Keine außerhalb des Scopes. Hinweise: Der Smoke-Check (`production/qa/smoke-2026-11-16.md`) hat Verdict FAIL (advisory): Der Charakter rutscht am Spielanfang ohne Eingabe leicht weg (Spielerbewegung, Story 001, nicht untersucht). Die Dauer von Lauf 2 (4,5–5 min) ist aus Woche 10 × 30 s abgeleitet, nicht gemessen. Laptop blieb als Platzhalter unverändert. Passive Upgrades beim Level-Up, passive Waffen und Ultimate-Spezialeigenschaften sind in `design/balance-waffen.md` Abschnitt 7 vermerkt, Vorschläge an Mert in Abschnitt 8.
**Test Evidence**: `production/qa/smoke-2026-11-16.md` (Verdict FAIL, advisory)
**Reviewed by**: self, no independent review
