# Frozen regression and bounded-fashion follow-up — 2026-10-08

Full snapshot: `1cc52597247b6b24a0ba42bee42e92a736ff751b`.
Full run: 298 checks; 296 passed; 2 failed. `sourceChangedDuringRun` is empty. Runtime compile passed against pinned Unity reference DLLs.

## Failures in that full snapshot

- `enemy-status-anchor`: introduced this round; corrected in follow-up below.
- `weapon-contact-production`: introduced this round; corrected in follow-up below.

## Follow-up scope

The full run exposed a duplicate EffectPreferences test boundary and a real >800-triangle loaded weapon budget. The follow-up removes only the duplicate declaration; uses faceted low-poly bow crests, spirit leaves and antlers; and replaces dense sword guard rings with pointed crystals. It preserves the 800-triangle cap, weapon anchors, combat properties and saved identities.

Formal follow-up: **9/9 passed**, with empty `sourceChangedDuringRun`: weaponfashionstructureproduction, enemy-status-anchor, equipment-composition-production, weapon-contact-production, integrated-actor-art-production, authored-actor-modules, actor-silhouette-f1-production, final-body-envelope-production, and the platform runtime compile (zero warnings/errors). Reports are in `20261008-Frozen/follow-up-final.json` and `follow-up-geometry.json`. These are targeted reruns after the full snapshot, not a second all-check run on the follow-up commit. The unified integration candidate still needs its own final validation.

## Execution limits

Managed production-code fixtures and pinned-reference compilation only. No Unity Editor, actual JsonUtility, rendering, physics, Windows build, Xcode build or device run occurred here. The parent coordinates Unity/Xcode and simulator installation against the unified SHA; main remains held for user acceptance.

CI queries on the full-snapshot SHAs returned no commit statuses and no PR-triggered workflow runs. The wrapper filters PR events and its first page; a separate all-Actions API query was denied. This is not a CI pass.
