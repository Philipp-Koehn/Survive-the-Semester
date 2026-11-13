# Game-Brief: Survive the Semester

> Written for: Teammitglieder und Dozent, die das Spiel in einer Seite verstehen wollen.

## Konzept

2D-Top-down-Arena-Survival, inspiriert von Vampire Survivors. Die Spielerin bzw. der Spieler überlebt 14 Semesterwochen voller Deadlines, Hausaufgaben und Müdigkeit, bis die Prüfungs-Bosse warten.

## Säulen

- **Wellen folgen dem Semesterkalender:** Eine Woche ist eine Welle.
- **Direkte Verfolgung statt Pathfinding:** Die Arena ist offen, Gegner laufen direkt auf den Spieler zu.
- **Bosse sind Prüfungen:** Die Zwischenprüfung beschwört Helfer (Deadlines).

## Gegner (Verantwortung: Mert)

- Deadline, Hausaufgaben-Stapel (Basis-Gegner, `enemy.tscn` mit Vererbung).
- Zwischenprüfung: Boss, 150 HP, Kontaktschaden 15, beschwört alle 4 s drei Deadlines.
- Klausur-Boss: geplant.
- Abschlussprüfung: Szene `final_exam.tscn` vorhanden.

## Spieler (Verantwortung: Philipp)

Offen, wird gemeinsam festgelegt:

- Waffen: Die Waffen sind Schulsachen (z. B. Stifte, Taschenrechner, Laptop). Neue Startwaffen werden über das Credit-System freigeschaltet. Credits sind zugleich Skillpunkte.
- Waffen-Slots im Run: Man kann bis zu 4 Waffen gleichzeitig haben. Bei jedem Level-Aufstieg werden zufällig Auswahlen angeboten, die aus Upgrades für vorhandene Waffen oder neuen Waffen bestehen (z. B. drei neue Waffen, oder Upgrades plus eine neue Waffe). Sind alle 4 Slots belegt, werden keine neuen Waffen mehr angeboten.
- Waffenwechsel: Im Spiel kann man zwischen den Waffen wechseln. Waffentypen unterscheiden sich, z. B. Einzelziel (Single Target) und Flächenschaden (AoE).
- Upgrades: Jede Waffe hat maximal 4 Level. Danach gibt es ein ultimatives Upgrade (Mechanik wie bei Vampire Survivors).
- Spieler-Werte: Leben, Schaden, Krit-Chance, Bewegungstempo (Movement), Schild. Weitere Werte können ergänzt werden.
- Spielende: Was passiert bei null Leben, und gibt es danach einen Neustart? amn stirbt und kommt in das main menü zurück

## Fortschritt

- 14 Wochen, gesteuert vom `WaveManager`.
- Zwischenprüfung ab Woche 7 (geplant).
- Nach Woche 14 beginnt die Prüfungsphase: Die Endbosse müssen besiegt werden, dann ist das Spiel gewonnen.

## MVP (erste spielbare Version)

1. Spielerbewegung und Kamera (Top-down).
2. Startwaffe (ein Schulsachen-Gegenstand) mit Grundschaden.
3. Waffen-Slots (bis 4) mit Wechsel im Spiel.
4. Upgrade-Auswahl pro Level (bis Level 4 pro Waffe, dann ultimatives Upgrade).
5. Gegnerkontakt reduziert Leben; Schild und Krit-Chance wirken.
6. Tod: Spielende, Rückkehr ins Hauptmenü.
7. Zugriff auf Wochen-Fortschritt und Zwischenprüfung (Mert, bereits vorhanden).

## Zeitplan (laut README)

| Termin | Meilenstein |
|--------|-------------|
| 28.10.2026 | Zwischenpräsentation |
| Mitte Nov. | Kern-Prototyp |
| Mitte Dez. | Inhalte (Gegner, Waffen, erster Boss) |
| ca. 20.01.2027 | Feinschliff |
| 27.01.2027 | Abschlusspräsentation |
| 10.02.2027 | Abgabe |

## Offene Punkte

- Arena-Grenze: aktuell nur optisch, Wände fehlen.
- Arena-Boden ist ein Platzhalter (Grafik liegt bei Fabian).
