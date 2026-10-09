#!/bin/bash
# Double-click in Finder, or run from any directory with bash.
set -euo pipefail
repo="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd -P)"
mode="${1:-all}"
case "$mode" in
  all|device|simulator) ;;
  --help|-h) echo 'Usage: bash Tools/Build-Xcode/Build.command [all|device|simulator]'; exit 0 ;;
  *) echo "Unknown mode: $mode" >&2; exit 2 ;;
esac
[[ $# -le 1 ]] || { echo 'Only one mode argument is supported.' >&2; exit 2; }
mkdir -p "$repo/Logs"
lock="$repo/Logs/xcode-export.lock"
mkdir "$lock" 2>/dev/null || { echo "Another export is running. If it was interrupted, remove $lock after confirming Unity has stopped." >&2; exit 1; }
snapshot=""
cleanup() {
  status=$?
  trap - EXIT
  if [[ -n "$snapshot" && -f "$snapshot" ]]; then
    cp "$snapshot" "$repo/ProjectSettings/ProjectSettings.asset"
    rm -f "$snapshot"
  fi
  rmdir "$lock"
  exit "$status"
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
snapshot="$(mktemp "${TMPDIR:-/tmp}/emberfall-player-settings.XXXXXX")"
cp "$repo/ProjectSettings/ProjectSettings.asset" "$snapshot"
echo 'Close Unity and Xcode before exporting; do not edit Player Settings during this operation.'
echo 'Exports current local source, including uncommitted changes. No Git pull or push is performed.'
if [[ "$mode" == all || "$mode" == simulator ]]; then
  bash "$repo/Tools/Export-iOS.sh" --sdk simulator
fi
if [[ "$mode" == all || "$mode" == device ]]; then
  bash "$repo/Tools/Export-iOS.sh" --sdk device
fi
echo 'Export complete. Xcode compilation, signing and installation are separate steps.'
