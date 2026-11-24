using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Owns the player's PlayerStats so every player script (movement, dash, weapons) reads the same numbers.
    /// Tune base values on this component in the Inspector. Upgrades add modifiers to Stats.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStatsHolder : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats = new PlayerStats();

        public PlayerStats Stats => stats;

        // Start, not Awake, so listeners (camera, HUD, RunManager) have subscribed in their OnEnable first.
        private void Start() => GameEvents.RaisePlayerSpawned(gameObject);
    }
}
