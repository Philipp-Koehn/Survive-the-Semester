# Balance: Waffen, Spielerwerte und Fortschritt

Story 009 · Stand 2026-11-16 · Werte aus den Szenen und Skripten gerechnet, nicht geschätzt.
Quellen: `Weapon.cs`, `PlayerStats.cs`, `Pen/Calculator/Laptop.tscn`, `ExperienceSystem.cs`,
`PassiveUpgrades.tres`, `RunManager.cs`, Gegner nur gelesen (`scenes/enemies/*.tscn`, `WaveManager.cs`).

## 1. Waffen-DPS (Level 1 bis 4 und Ultimate)

Formel: Schaden = Basis + `DamagePerLevel` × (L − 1). Abklingzeit = max(0,15; Basis − 0,05 × (L − 1)).
Ultimate (nur auf Level 4): Schaden × 2, Reichweite × 1,5. DPS = Schaden / Abklingzeit.
Krit (10 % × Faktor 2) erhöht den Erwartungswert um ×1,10, Schadens-Multiplikator 1,0.

| Waffe | L1 | L2 | L3 | L4 | Ultimate |
|---|---|---|---|---|---|
| Stift (10, +4 pro Level, 2,0 s) | 10 / 2,00 = 5,00 | 14 / 1,95 = 7,18 | 18 / 1,90 = 9,47 | 22 / 1,85 = 11,89 | 44 / 1,85 = 23,78 |
| Taschenrechner (5, +2, 1,2 s), pro Gegner im Radius 120 px | 5 / 1,20 = 4,17 | 7 / 1,15 = 6,09 | 9 / 1,10 = 8,18 | 11 / 1,05 = 10,48 | 22 / 1,05 = 20,95 |
| Laptop (12, +2, 1,6 s), Platzhalter | 7,50 | 9,03 | 10,67 | 12,41 | 24,83 |

- Rollen: Der Stift ist Einzelziel. Er tötet einen Standardgegner (10 HP) mit einem Treffer, einen Hausaufgaben-Stapel (40 HP) mit 4 (auf L4 mit 2). Er schafft aber nur 0,5 Kills pro Sekunde und liegt damit schon ab Woche 1 unter der Spawnrate (0,67 pro Sekunde). Stehenbleiben reicht also nicht, Flächenschaden wird früh gebraucht.
- Der Taschenrechner ist die Waffe gegen Schwärme: Standardgegner nach 2 Pulsen, Deadline nach 1 Puls. Er schlägt den Stift schon ab 2 Gegnern im Radius, auf jeder Stufe.
- Laptop ist ein Platzhalter und bekommt später Zusatzeffekte. Er wird deshalb hier nicht abgestimmt. Aktuell ist er auf jeder Stufe schwächer als der Stift und nur durch die Reichweite (350 px) interessant.

## 2. Spielerwerte gegen Gegnerschaden pro Woche

Effektives Leben: 100 Leben + 25 Schild = 125, keine Regeneration. Jeder Gegner trifft einmal pro Sekunde (`DamageCooldown` 1,0).

| Gegner | Kontaktschaden | Treffer bis zum Tod | Vorkommen |
|---|---|---|---|
| Standardgegner (10 HP) | 5 | 25 | ab Woche 1 |
| Deadline (5 HP) | 3 | 42 | ab Woche 3 |
| Hausaufgaben-Stapel (40 HP) | 10 | 13 | ab Woche 5 |
| Zwischenprüfung (150 HP) | 15 | 9 | Woche 7 |
| Endklausur (400 HP) | 20 | 7 | Woche 14 |

| Woche | Spawn-Intervall | Gegner-Mix (Ø Schaden pro Treffer) | Treffer bis zum Tod, Ø | Zeit bis zum Tod bei 3 Gegnern im Kontakt |
|---|---|---|---|---|
| 1–2 | 1,5–1,4 s | Standard (5,0) | 25 | 8,3 s |
| 3–4 | 1,3–1,2 s | Standard, Deadline (4,3) | 29 | 9,6 s |
| 5–8 | 1,1–0,8 s | + Hausaufgaben (5,8) | 22 | 7,2 s |
| 9–13 | 0,7–0,3 s | Hausaufgaben und Deadline häufiger (6,0) | 21 | 6,9 s |
| 14 | 0,3 s | wie 9–13, Endklausur 20 | 7 (Boss) | – |

Bewertung: Leben, Schild und Krit bleiben vorerst unverändert. Ob sie reichen, zeigt der Probelauf (Abschnitt 6). Stirbt der Spieler vor Woche 7, ist die erste Stellschraube `MaxShield` oder `MaxHealth`.

## 3. XP-Kurve

Bedarf für Level n → n+1: `XpForLevel2` + `XpGrowthPerLevel` × (n − 1). Annahme der Tabelle: alle gespawnten Gegner werden besiegt, Boss-Zuschlag 100 XP in Woche 7.

| Woche | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 14 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Level Ausgangswert (30, +15) | 3 | 4 | 6 | 7 | 9 | 10 | 12 | 14 | 16 | 18 | 31 |
| Level erste Runde (60, +12) | 2 | 3 | 5 | 6 | 8 | 9 | 12 | 13 | 16 | 18 | 32 |
| Level zweite Runde (80, +15) | 2 | 3 | 4 | 5 | 6 | 8 | 10 | 11 | 13 | 15 | 28 |
| Level aktuell (120, +20) | 1 | 2 | 3 | 4 | 5 | 6 | 8 | 9 | 11 | 13 | 23 |

Eine Waffe auf Level 4 braucht 3 Level-Ups auf dieselbe Waffe. Mit der aktuellen Kurve ist das frühestens in Woche 4 möglich (Spieler-Level 4), typisch eher in Woche 5–6, je nachdem, welche Upgrades angeboten werden. Die Tabelle nimmt an, dass alle Gegner besiegt werden. Im echten Spiel liegt das Level eher darunter.

## 4. Credits und Preise

Credits am Run-Ende: Level × 5 + Woche × 10 + besiegte Bosse × 25. Ein Run bis Woche 7 mit Level 8 (Kurve aus Runde 3) bringt etwa 110 Credits (+25 mit Zwischenprüfung).

- Taschenrechner 150: nach etwa 2 Runs.
- Laptop 300: nach etwa 3–4 Runs.
- Passive Upgrades unverändert (Basis 20/25/30, Wachstum 10/12/15, max. 10 Stufen): erste Käufe schon nach dem ersten Run.

## 5. Änderungen (alt → neu)

| Wert | Datei | alt → neu | Begründung |
|---|---|---|---|
Runde 1 (2026-11-16):

| Wert | Datei | alt → neu | Begründung |
|---|---|---|---|
| `Damage` Stift | `scenes/Player/Weapons/Pen.tscn` | 5 → 4 | Am Anfang war der Stift zu stark. |
| `DamagePerLevel` Stift | `Pen.tscn` | 2 (Standard) → 3 | Ab Level 2 ausgleichen, später stärker. |
| `XpForLevel2` | `scripts/Player/ExperienceSystem.cs` | 30 → 60 | Levelling am Anfang langsamer. |
| `XpGrowthPerLevel` | `ExperienceSystem.cs` | 15 → 12 | Flachere Kurve, ab Woche 7 wieder auf dem alten Niveau. |
| `StarterWeaponPrices` | `data/Player/PassiveUpgrades.tres` | 0/50/100 → 0/150/300 | Erste Freischaltung nach wenigen Runs statt nach dem ersten. |

Runde 2 (2026-11-16, nach Rückmeldung: Levelling noch zu schnell, Stift soll Einzelziel sein, Taschenrechner braucht einen Sinn):

| Wert | Datei | alt → neu | Begründung |
|---|---|---|---|
| `Cooldown` Stift | `Pen.tscn` | 0,6 → 1,2 s | Stift schießt viel langsamer und kann Schwärme nicht mehr allein halten. |
| `Damage` Stift | `Pen.tscn` | 4 → 10 | Harte Einzeltreffer: ein Standardgegner pro Schuss, Hausaufgaben-Stapel in 4 Schüssen. |
| `DamagePerLevel` Stift | `Pen.tscn` | 3 → 5 | Passt zur höheren Basis. L4 23,8 DPS, Ultimate 47,6 DPS. |
| `Damage` Taschenrechner | `scenes/Player/Weapons/Calculator.tscn` | 3 → 5 | Die Waffe gegen Schwärme: Standardgegner in 2 Pulsen, Deadline in 1. |
| `XpForLevel2` | `ExperienceSystem.cs` | 60 → 80 | Levelling insgesamt langsamer. |
| `XpGrowthPerLevel` | `ExperienceSystem.cs` | 12 → 15 | Woche 7 jetzt Level 10 statt 12. |

Runde 3 (2026-11-16, nach Lauf 1: bis Woche 7 ohne Bewegung überlebt, Level 4 schon in Woche 3):

| Wert | Datei | alt → neu | Begründung |
|---|---|---|---|
| `Cooldown` Stift | `Pen.tscn` | 1,2 → 2,0 s | Bei 0,83 Kills/s hielt der Stift die Spawnrate bis Woche 7 fast allein. Bei 0,5 Kills/s liegt er ab Woche 1 darunter. |
| `DamagePerLevel` Stift | `Pen.tscn` | 5 → 4 | Upgrades auf den Stift machen ihn langsamer stark. L4 11,9 statt 23,8 DPS. |
| `XpForLevel2` | `ExperienceSystem.cs` | 80 → 120 | Level 4 in Woche 3 war zu früh. Jetzt Level 3 in Woche 3. |
| `XpGrowthPerLevel` | `ExperienceSystem.cs` | 15 → 20 | Woche 7 jetzt Level 8 statt 10. |

Nicht geändert: Laptop (Platzhalter), Taschenrechner (seit Runde 2: 5 Schaden), Leben, Schild, Krit, Credit-Formel, passive Upgrades, Startwaffen-Preise aus Runde 1.

Zusätzlich (keine Balance-Änderung): Die Debug-Taste 0 (`scripts/Player/DebugShop.cs`) geht jetzt alle Startwaffen durch, den Stift eingeschlossen. Vorher ließ sie den Stift aus, deshalb war er nach dem ersten Kauf nicht mehr wählbar. Die Logik in `MetaProgress.SelectStarter(0)` war richtig.

## 6. Probelauf bis mindestens Woche 7

Noch offen. Eintragen, sobald gespielt:

| Lauf | Dauer | Woche erreicht | Level | Ausgang (Tod / Sieg) | Anmerkung |
|---|---|---|---|---|---|
| 1 (Werte aus Runde 2) | – | ≥ 7 | 4 in Woche 3 | überlebt bis Woche 7 | Ohne Bewegung möglich, nur Stift-Upgrades genommen, kaum Schaden erhalten. Führte zu Runde 3. |
| 2 (Werte aus Runde 3) | ca. 4,5–5 min | 10 | 10 | Tod in Woche 10 | Gegen Ende fast nur noch Taschenrechner, weil die Gegnermenge zu groß war. Stift ab etwa Woche 3 kaum noch nützlich. Gewollt so, Werte bleiben. Rückmeldung: „jetzt ist es gut“. |

Bewertung: Der Stift fällt ab Woche 3 deutlich ab, das ist bewusst so. Er wird später über neue Upgrades wieder gestärkt (Abschnitt 7). Die Werte aus Runde 3 sind der aktuelle Stand.

## 7. Später geplant (nicht Teil dieser Story)

- **Passive Upgrades beim Level-Up:** Neben Waffen-Upgrades und neuen Waffen kommen passive Upgrades in die Level-Up-Auswahl, zum Beispiel für Schaden oder Abklingzeit.
- **Passive Waffen:** Waffen, die nicht selbst schießen, sondern andere Waffen verstärken. Sie sollen vor allem den Stift im späteren Run wieder stark machen.
- **Ultimates mit Spezialeigenschaft:** Jede Ultimate bekommt zusätzlich zu Schaden × 2 und Reichweite × 1,5 eine eigene Spezialeigenschaft.
- **Laptop:** Er bleibt Platzhalter und bekommt eigene Zusatzeffekte, damit er sich vom Stift unterscheidet.

Wenn diese Upgrades da sind, wird die Balance aus diesem Dokument erneut geprüft, besonders Stift-DPS ab Woche 3.

## 8. Vorschläge an Mert (Gegnerwerte, nicht von mir geändert)

- Die Gegner-HP wachsen nicht mit der Woche: Ein Standardgegner hat das ganze Spiel über 10 HP. Vorschlag: HP pro Woche leicht steigern.
- Zwischenprüfung (150 HP) fällt mit Stift L4 in etwa 12,6 s, mit Ultimate in 6,3 s. Die Endklausur (400 HP) fällt in etwa 34 s bzw. 17 s. Mit dem Laptop L4 dauern sie 12 s bzw. 32 s. Vorschlag als Richtwert: Zwischenprüfung etwa 500 HP, Endklausur etwa 1500 HP.
- Ab Woche 13 spawnt alle 0,3 s ein Gegner (Obergrenze 50). Das ist spielbar nur mit Flächenschaden (Taschenrechner). Wenn das nicht gewollt ist, `MinInterval` anheben.
