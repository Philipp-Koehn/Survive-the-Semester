# Spieler-Schnittstellen für UI und Menü (Stand: 11.11.2026)

> Written for: Fabian (UI, Menü) und seine KI. Beschreibt, was der Spieler-Bereich (Philipp) anbietet und was von dir gebraucht wird.

## Frage an Fabian (bitte beantworten)

1. **Wie heißt die Hauptmenü-Szene, und wo liegt sie?** Beispiel: `res://scenes/ui/main_menu.tscn`.
   Wenn der Spieler stirbt, soll das Spiel dorthin wechseln. Der Pfad steht im Inspektor am Knoten `RunManager` in `scenes/Player/PlayerController.tscn` (`MainMenuScenePath`), im Moment leer. Du kannst ihn selbst eintragen oder Philipp den Pfad sagen.
2. **Welche Anzeigen brauchst du und welche Signale dafür?** Siehe unten, ich (Philipp) baue gern Signale, wenn die Anzeige sie braucht.

## Tod und neuer Run

- Beim Tod (Leben 0) gibt `PlayerStats` das Signal `Died` aus. Der `RunManager` (Kind des Spielers) pausiert das Spiel, wartet `LeaveDelay` Sekunden (Standard 2 s) und lädt dann die Szene aus `MainMenuScenePath`.
- Ist der Pfad leer oder die Szene fehlt, gibt es eine Warnung in der Konsole und der Run startet neu. Es gibt keinen Absturz.
- Die Pause wird vor dem Szenenwechsel aufgehoben. Das Menü läuft also normal.
- **Neuer Run:** Der „Start“-Button im Menü soll `res://main.tscn` laden (`GetTree().ChangeSceneToFile("res://main.tscn")`). Dadurch beginnt alles von vorn (Leben, Schild, nur die Startwaffe).

## Daten für Lebens- und Schildanzeige

```csharp
var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
PlayerStats stats = player.Stats;
// stats.Health, stats.MaxHealth, stats.Shield, stats.MaxShield, stats.IsDead
```

- Signal `Died` existiert. Ein Signal bei jeder Änderung von Leben oder Schild gibt es noch nicht, die Anzeige müsste sonst jeden Frame lesen. Wenn du eines willst, sag Bescheid.
- Die Konsolenzeilen „Spieler getroffen! …“ sind nur ein Platzhalter, bis es eine Anzeige gibt.

## Level-Up-Auswahl

Die Auswahl hat bisher nur eine Konsolenausgabe und die Tasten 1 bis 3 (`UpgradeManager`, Kind des Spielers).

- `IsChoosing`: true, solange gewählt wird. Das Spiel ist dabei pausiert.
- `Offered`: Liste der Optionen, jede mit `Label` (Text), `Kind` (Waffen-Level, Ultimate oder neue Waffe), `Weapon` und `Scene`.
- `Choose(int index)`: wendet die Option an (0-basiert) und setzt das Spiel fort.
- `TriggerLevelUp()`: startet die Auswahl. Bis es ein XP-System gibt, löst die Taste 9 das aus (Test).
- Ein Signal „Optionen wurden angeboten“ fehlt noch. Wenn du ein UI-Fenster bauen willst, sag das, dann ergänze ich es.

## Waffen-Slots

- `WeaponInventory` (Kind des Spielers): `Weapons` (Liste), `ActiveIndex`, `Count`, `MaxSlots` (4).
- Jede `Weapon`: `WeaponName`, `Level`, `HasUltimate`.
- Wechsel im Spiel: Tasten 1 bis 4 und Mausrad. Nur die aktive Waffe schießt.

## Eingaben des Spielers

| Aktion | Taste |
|---|---|
| `move_left`, `move_right`, `move_up`, `move_down` | WASD und Pfeiltasten |
| `weapon_1` bis `weapon_4` | Zahlentasten 1 bis 4 |
| `weapon_next`, `weapon_prev` | Mausrad |
| `upgrade_pick_1` bis `upgrade_pick_3` | Zahlentasten 1 bis 3, nur während der Auswahl |
| `debug_level_up` | Taste 9, nur zum Testen |

Die Aktionen stehen in `project.godot` im Abschnitt `[input]`. Bitte nicht dieselben Tasten für Menüs belegen, ohne vorher Bescheid zu sagen.
