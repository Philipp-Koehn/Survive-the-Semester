# Story 008: Credits, dauerhafte Freischaltung und passive Upgrades

> **Epic**: spieler-waffen
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: L
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-11-16

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief „Spieler“ (Credit-System, vom Nutzer ergänzt; Zukunftsplan „Meta-Progression“)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Godot 4.7 (C#) | **Risk**: HIGH
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal)

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

- [ ] Am Run-Ende (Tod oder `SemesterFinished`) werden Credits vergeben. Die Menge hängt von erreichtem Level, erreichter Woche und besiegten Bossen ab (Werte als `[Export]`).
- [ ] Credits und Freischaltungen bleiben über Runs hinweg erhalten, gespeichert unter `user://`.
- [ ] Fehlt die Datei oder ist sie beschädigt, startet das Spiel mit 0 Credits und ohne Freischaltungen, ohne Absturz. Der Stift ist immer frei.
- [ ] Eine Startwaffe kostet Credits. Ist sie einmal freigeschaltet, bleibt sie dauerhaft frei.
- [ ] Bei zu wenig Credits wird der Kauf abgelehnt, die Credits bleiben unverändert.
- [ ] Passive Upgrades werden mit Credits gekauft und gelten dauerhaft über alle Runs (z. B. Leben, Schaden, Krit-Chance).
- [ ] Die Kosten eines passiven Upgrades steigen pro Kauf (Kurve als `[Export]` bzw. Daten).
- [ ] Beim Run-Start werden die gekauften passiven Upgrades auf `PlayerStats` angewendet.
- [ ] Die gewählte Startwaffe wird beim Run-Start in den `WeaponInventory` gelegt. Ist keine Wahl getroffen, gilt der Stift.
- [ ] Preise, Vergabewerte und Upgrade-Werte sind datengetrieben (`[Export]` oder Ressource-Datei).

---

## Implementation Notes

- Speicher: eine Datei unter `user://` mit Credits, freigeschalteten Startwaffen und Stufen der passiven Upgrades. Lesen und Schreiben über try/catch, Fehler werden in der Konsole gemeldet.
- Passive Upgrades ändern `PlayerStats` (MaxHealth, DamageMultiplier, CritChance) beim Run-Start, nicht während des Runs.
- Der Shop (Startwaffe wählen, Upgrades kaufen) ist UI und liegt bei Fabian. Diese Story liefert Logik und Signale wie `CreditsChanged` und `UnlocksChanged`, dazu eine Abfrage der Kaufliste.

---

## Out of Scope

- Shop-Oberfläche und Startwaffen-Auswahl-Bildschirm: Fabian.
- Gegner-Werte oder Gegner-Vergabe: Mert.

---

## Files

- `scripts/Player/MetaProgress.cs` (neu: Credits, Freischaltungen, Speicher unter user://, Kaufliste)
- `scripts/Player/PassiveUpgrades.cs` (neu: Katalog-Resource, Upgrade-Definitionen und Anwendung)
- `data/Player/PassiveUpgrades.tres` (neu: Katalog-Daten mit Preisen der Startwaffen)
- `scripts/Player/RunManager.cs` (geändert: Credits am Run-Ende, Passive beim Run-Start)
- `scripts/Player/PlayerStats.cs` (geändert: ApplyPassiveBonuses; nicht in der ursprünglichen Liste)
- `scripts/Player/WeaponInventory.cs` (geändert: Startwaffe aus der Auswahl übernehmen)
- `scripts/Player/DebugShop.cs` (neu: Debug-Tasten 8 und 0 als Ersatz für den Shop, bis Fabians UI existiert)
- `project.godot` (geändert: Aktionen debug_buy_passive, debug_unlock_starter)
- `scenes/Player/PlayerController.tscn` (geändert: Knoten DebugShop)

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level`: at `qa.level: minimal` tests are waived (advisory).*

**Story Type**: Logic
**Required evidence**: `tests/unit/spieler/meta_progress_test.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/spieler-waffen/story-006-tod-hauptmenue.md` (Run-Ende), `production/epics/spieler-waffen/story-007-xp-level-system.md` (Level als Credit-Quelle)
- Unlocks: `production/epics/spieler-waffen/story-009-balancing-durchgang.md`

---

## Completion Notes

**Completed**: 2026-11-16
**Criteria**: 10/10 passing (keine zurückgestellt)
**Deviations**: Keine. Info: Die Credit-Formel (`Level×5 + Woche×10 + Bosse×25`) ist ein Vorschlag und wird in Story 009 abgestimmt. `DebugShop.cs` (Tasten 8 und 0) ist ein Debug-Ersatz für den Shop und muss vor einem Release raus. `RunManager.MainMenuScenePath` ist noch leer, bis Fabians Menü existiert.
**Test Evidence**: waived at qa.level minimal
**Reviewed by**: self, no independent review
