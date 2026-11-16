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

Stand 11.11.2026, umgesetzt im Epic `spieler-waffen`:

- **Bewegung:** WASD und Pfeiltasten, Tempo 300 (im Inspektor einstellbar), Kamera folgt dem Spieler.
- **Waffen:** Schulsachen mit eigenem Schaden und eigener Abklingzeit: Stift (Startwaffe, 5 Schaden, 0,6 s), Taschenrechner (3, 1,2 s), Laptop (12, 1,6 s). Zwei Typen: Einzelziel (Stift, Laptop: trifft den nächsten Gegner in Reichweite) und Fläche (Taschenrechner: trifft alle Gegner im Radius von 120 px).
- **Waffen-Slots:** bis zu 4 Waffen im Run. Wechsel mit den Tasten 1 bis 4 und dem Mausrad, nur die aktive Waffe schießt.
- **Level-Up:** Der `UpgradeManager` bietet bis zu 3 zufällige Optionen an (Waffen-Level, ultimatives Upgrade, neue Waffe). Neue Waffen kommen nur, solange ein Slot frei ist. Das Spiel pausiert während der Wahl.
- **Upgrades:** Jede Waffe hat bis zu 4 Level (u. a. kürzere Abklingzeit), danach ein ultimatives Upgrade.
- **Spieler-Werte:** Leben 100, Schild 25 (fängt Schaden zuerst ab), Schadens-Multiplikator, Krit-Chance 10 % mit Faktor 2. Alles im Inspektor einstellbar.
- **Spielende:** Bei 0 Leben pausiert das Spiel 2 s und wechselt ins Hauptmenü. Ist der Szenenpfad noch leer, startet der Run neu.
- **XP-/Level-System:** Gegner geben XP, ein Level-Up öffnet die Upgrade-Auswahl (Taste 9 bleibt als Debug-Test).
- **Credit-System:** Credits schalten Startwaffen frei und kaufen passive Spieler-Werte (Debug-Tasten 8 und 0).
- **Noch offen:** Anzeigen für Leben, Schild und Level-Up-Auswahl sowie die Hauptmenü-Szene (Fabian).

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
