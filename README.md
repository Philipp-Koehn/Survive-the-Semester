# Survive the Semester

Ein 2D-Arena-Survival-Spiel (Top-down) über das Studentenleben, inspiriert von Vampire Survivors.
Überlebe 14 Semesterwochen voller Deadlines, Hausaufgaben und Müdigkeit, bis die Prüfungs-Bosse warten.

Projekt im Modul Spieleprogrammierung, WS 2026/27.

## Team

| Name | Bereich |
|------|---------|
| Philipp Köhn | Spieler & Waffen |
| Mert Erbasaran | Gegner & KI, Wellen, Bosse, Leveldesign |
| Fabian Boci | UI & Fortschritt, Grafik |
| Tim Wacht | Sound, Build & Test, Doku, Projektleitung |

## Setup

1. **Godot 4.7.2 .NET** installieren (die .NET-Version, sonst kein C#)
2. **.NET 10 SDK** installieren
3. Repo klonen:
```bash
   git clone https://github.com/Philipp-Koehn/Survive-the-Semester.git
```
4. In Godot: **Importieren** → `project.godot` auswählen
5. Einmal auf **Build** (Hammer) klicken, dann mit **Play** starten

Renderer: Compatibility (läuft auch auf schwächeren Laborrechnern).

## Projektstruktur

- `scenes/`: Szenen (.tscn), Unterordner pro Bereich
- `scripts/`: C#-Skripte, Unterordner pro Bereich
- `assets/`: Sprites, Sounds, Fonts
- `docs/`: Präsentationen und Dokumentation

## Workflow

- Vor dem Arbeiten: `git pull`
- Danach: `git add .` → `git commit -m "Was gemacht"` → `git push`
- Nicht gleichzeitig an derselben `.tscn` arbeiten (Merge-Konflikte)

## Zeitplan

| Termin | Meilenstein |
|--------|-------------|
| 28.10.2026 | Zwischenpräsentation |
| Mitte Nov. | Kern-Prototyp |
| Mitte Dez. | Inhalte (Gegner, Waffen, erster Boss) |
| ca. 20.01.2027 | Feinschliff |
| 27.01.2027 | Abschlusspräsentation |
| 10.02.2027 | Abgabe |