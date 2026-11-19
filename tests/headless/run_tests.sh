#!/bin/bash
# Führt alle Headless-Tests aus. Sichert vorher den lokalen Spielstand (MetaProgress)
# und stellt ihn danach wieder her, weil Testläufe Credits gutschreiben können.
# Aufruf aus dem Projektordner: bash tests/headless/run_tests.sh [Ausgabeordner]
# Nur einzelne Orte/Wochen: LOCATIONS="campus" WEEKS="3 5" bash tests/headless/run_tests.sh
GODOT="${GODOT:-/Applications/Godot_mono.app/Contents/MacOS/Godot}"
OUT="${1:-/tmp/sts-tests}"
SAVE="$HOME/Library/Application Support/Godot/app_userdata/Survive The Semester/meta_progress.json"
mkdir -p "$OUT"
[ -f "$SAVE" ] && cp "$SAVE" "$OUT/meta_progress.backup.json"

dotnet build > "$OUT/build.log" 2>&1 || { echo "Build fehlgeschlagen, siehe $OUT/build.log"; exit 1; }

run() { "$GODOT" --headless --path . --quit-after 20000 -s res://tests/headless/level_test.gd -- "$@" > "$OUT/$(echo "$@" | tr ' ' '_').log" 2>&1; }

# Wochen: 1 Gänge, 3 Campus, 5 Campus + Zwischenprüfung, 6 Bibliothek, 9 Mensa,
# 10 Mensa + Zwischenprüfung, 12 Hörsaal, 15 Prüfungsphase
LOCATIONS="${LOCATIONS:-gaenge campus bibliothek mensa hoersaal}"
WEEKS="${WEEKS:-1 3 5 6 9 10 12 15}"
for l in $LOCATIONS; do run "$l" & done
wait
n=0
for w in $WEEKS; do
  run soak "$w" 60 &
  n=$((n + 1))
  [ $((n % 4)) -eq 0 ] && wait   # höchstens 4 Läufe gleichzeitig
done
wait

[ -f "$OUT/meta_progress.backup.json" ] && cp "$OUT/meta_progress.backup.json" "$SAVE"
echo "Fertig. Logs in $OUT"
