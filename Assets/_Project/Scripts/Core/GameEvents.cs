using System;
using UnityEngine;

namespace Game.Core
{
    public enum RunResult
    {
        Died,
        Won,
        Quit
    }

    /// <summary>
    /// Global events so systems never need references to each other.
    ///
    /// Listen:  void OnEnable()  { GameEvents.RoomCleared += HandleRoomCleared; }
    ///          void OnDisable() { GameEvents.RoomCleared -= HandleRoomCleared; }
    /// Raise:   GameEvents.RaiseRoomCleared(gameObject);
    ///
    /// Always unsubscribe in OnDisable, or a destroyed object keeps getting called.
    /// </summary>
    public static class GameEvents
    {
        // Player
        public static event Action<GameObject> PlayerSpawned;             // player root; RunManager re-applies upgrades here
        public static event Action<DamageInfo, float, float> PlayerDamaged; // hit, current HP, max HP
        public static event Action<float, float> PlayerHealthChanged;     // current HP, max HP (damage, heals, max changes)
        public static event Action PlayerDied;
        public static event Action<int, int, float> PlayerDashCooldownChanged; // charges ready, max charges, next charge progress 0..1 (1 = full)
        public static event Action<int, int, bool> PlayerAmmoChanged;     // rounds in magazine, magazine size (0 = infinite), is reloading

        // Combat
        public static event Action<GameObject, EnemyData> EnemyKilled;    // enemy object (about to be destroyed), its data

        // Dungeon
        public static event Action<GameObject> RoomEntered;               // room root
        public static event Action<GameObject> RoomCleared;               // room root
        public static event Action<FloorData> FloorStarted;
        public static event Action<FloorData> FloorCompleted;             // the level's boss is dead

        // Upgrades and suspicion
        public static event Action<UpgradeData> UpgradePicked;            // tech or imagination
        public static event Action<UpgradeData> FragmentCollected;        // an imagination fragment was taken
        public static event Action<int> SuspicionChanged;                 // new suspicion level for this run

        // Run
        public static event Action<RunResult> RunEnded;

        public static void RaisePlayerSpawned(GameObject player) => PlayerSpawned?.Invoke(player);
        public static void RaisePlayerDamaged(DamageInfo info, float current, float max) => PlayerDamaged?.Invoke(info, current, max);
        public static void RaisePlayerHealthChanged(float current, float max) => PlayerHealthChanged?.Invoke(current, max);
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaisePlayerDashCooldownChanged(int ready, int max, float progress) => PlayerDashCooldownChanged?.Invoke(ready, max, progress);
        public static void RaisePlayerAmmoChanged(int current, int magazineSize, bool reloading) => PlayerAmmoChanged?.Invoke(current, magazineSize, reloading);
        public static void RaiseEnemyKilled(GameObject enemy, EnemyData data) => EnemyKilled?.Invoke(enemy, data);
        public static void RaiseRoomEntered(GameObject room) => RoomEntered?.Invoke(room);
        public static void RaiseRoomCleared(GameObject room) => RoomCleared?.Invoke(room);
        public static void RaiseFloorStarted(FloorData floor) => FloorStarted?.Invoke(floor);
        public static void RaiseFloorCompleted(FloorData floor) => FloorCompleted?.Invoke(floor);
        public static void RaiseUpgradePicked(UpgradeData upgrade) => UpgradePicked?.Invoke(upgrade);
        public static void RaiseFragmentCollected(UpgradeData upgrade) => FragmentCollected?.Invoke(upgrade);
        public static void RaiseSuspicionChanged(int level) => SuspicionChanged?.Invoke(level);
        public static void RaiseRunEnded(RunResult result) => RunEnded?.Invoke(result);

        /// <summary>
        /// Clears every listener when Play starts. Static events survive between Play sessions when
        /// "Enter Play Mode Options" skips the domain reload, which causes calls on destroyed objects.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearAll()
        {
            PlayerSpawned = null;
            PlayerDamaged = null;
            PlayerHealthChanged = null;
            PlayerDied = null;
            PlayerDashCooldownChanged = null;
            PlayerAmmoChanged = null;
            EnemyKilled = null;
            RoomEntered = null;
            RoomCleared = null;
            FloorStarted = null;
            FloorCompleted = null;
            UpgradePicked = null;
            FragmentCollected = null;
            SuspicionChanged = null;
            RunEnded = null;
        }
    }
}
