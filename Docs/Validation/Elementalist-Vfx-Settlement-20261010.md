# Elementalist effect preview and settlement navigation — 2026-10-10

Scope: iOS working tree. Ice/fire/lightning impact bodies now use an explicit elementalist opt-in, a smaller interior light, larger separated crown silhouettes, staged vertical motion, and stronger front/back shading. The shared presentation of other classes remains on the prior path. Damage, cooldowns and hit schedules are unchanged. Chain lightning's existing connecting arc and contact identity are retained in this first preview.

Completed-dungeon settlement and chest-result pages have three bottom actions: 再次挑战, 挑战下一阶, 返回营地. Replays and next tier require committed rewards; a saved reveal receipt is acknowledged on the user's click. Failed acknowledgement stops before navigation and preserves the page. Next tier retains chapter admission and maximum-tier checks. The dedicated death recap retains its original recovery controls.

Validation:

- Unity 6000.6.4f1 with Metal rendered 144 Camera.Render frames through the actual production effect component, with the same gameplay camera projection and deterministic sampled ages. `ArtSource/Review/Elementalist-20261010/Comparison.gif` shows previous and new paths. Direct impact replay is not a full skill-cast interaction check.
- FilledVfxPoolProductionTests: 4645 assertions plus three compiled negative controls passed. Includes sculpted low-tier limits, pause, retirement and no cross-class pooled-state leakage. Updated missing presentation boundary and Unity constant doubles in the existing fixture.
- SettlementNavigationProductionTests: 36 actual extracted-method assertions passed, including footer bounds, pending choice/reward gating, max tier, failed save, failed travel, accepted travel and receipt retry. GUI/save/navigation boundaries are managed doubles.
- Unity simulator export and Xcode Release build succeeded. Installed and launched `com.h644782259.emberfall.ios` on iPhone 16 Pro simulator `36D0E0CE-37F5-42DC-B3BF-39DFAE115331`. Startup screenshot confirms title screen and the existing elementalist save is visible.
- Existing RoomGenerationRecapLayoutTests fails because it expects `data.MechanismEvidence` in DrawRecapCards. That member is also absent from HEAD and DrawRecapCards is byte-identical to HEAD; this is a pre-existing stale source assertion. It was not changed in this task.

No Windows synchronization, Git commit/push, real-device GPU/FPS measurement, or simulator button-by-button interaction acceptance is claimed. Native computer-use binding returned Invalid app for Simulator and its bundle ID, so user interaction remains to be checked.
