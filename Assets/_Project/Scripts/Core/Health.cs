using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Generic health for the player, enemies, bosses and breakables.
    /// Local events (Damaged, Died, ...) are for scripts on the same object (hit flash, death anim).
    /// If Team is Player, it also raises the global GameEvents so the HUD never needs a reference.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private Team team = Team.Enemy;
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [Tooltip("Seconds of invincibility after taking a hit (0 = none). Usually only the player uses this.")]
        [SerializeField, Min(0f)] private float invincibilityAfterHit = 0f;
        [SerializeField] private bool destroyOnDeath = false;

        public Team Team => team;
        public float Current { get; private set; }
        public float Max => maxHealth;
        public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;
        public bool IsDead { get; private set; }
        public bool IsInvincible => invincibleTimer > 0f || invincibleLocks > 0;

        public event Action<DamageInfo> Damaged;
        public event Action<float> Healed;           // amount healed
        public event Action<float, float> Changed;   // current, max
        public event Action<DamageInfo> Died;

        private float invincibleTimer;
        private int invincibleLocks;

        private void Awake()
        {
            Current = maxHealth;
        }

        private void Start()
        {
            NotifyChanged(); // so the HUD shows the starting value
        }

        private void Update()
        {
            if (invincibleTimer > 0f)
                invincibleTimer -= Time.deltaTime;
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (IsDead || IsInvincible || info.Amount <= 0f)
                return false;
            if (info.SourceTeam == team && team != Team.Neutral)
                return false;

            Current = Mathf.Max(0f, Current - info.Amount);
            if (invincibilityAfterHit > 0f)
                invincibleTimer = invincibilityAfterHit;

            Damaged?.Invoke(info);
            if (team == Team.Player)
                GameEvents.RaisePlayerDamaged(info, Current, maxHealth);
            NotifyChanged();

            if (Current <= 0f)
                Die(info);
            return true;
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;
            float before = Current;
            Current = Mathf.Min(maxHealth, Current + amount);
            float healed = Current - before;
            if (healed <= 0f)
                return;
            Healed?.Invoke(healed);
            NotifyChanged();
        }

        /// <summary>Changes max HP (e.g. from a MaxHealth stat upgrade). Gaining max HP also heals by the difference.</summary>
        public void SetMaxHealth(float newMax, bool healByDifference = true)
        {
            newMax = Mathf.Max(1f, newMax);
            float diff = newMax - maxHealth;
            maxHealth = newMax;
            if (healByDifference && diff > 0f)
                Current += diff;
            Current = Mathf.Min(Current, maxHealth);
            NotifyChanged();
        }

        /// <summary>Invincible for a set time (e.g. dash i-frames). Doesn't shorten a longer timer already running.</summary>
        public void SetInvincibleFor(float seconds)
        {
            invincibleTimer = Mathf.Max(invincibleTimer, seconds);
        }

        /// <summary>Invincible until released (e.g. cutscenes). Every Add must be matched by a Release.</summary>
        public void AddInvincibilityLock() => invincibleLocks++;
        public void ReleaseInvincibilityLock() => invincibleLocks = Mathf.Max(0, invincibleLocks - 1);

        public void Kill(GameObject source = null)
        {
            if (IsDead)
                return;
            Current = 0f;
            NotifyChanged();
            Die(new DamageInfo(0f, Team.Neutral, source));
        }

        /// <summary>Brings back to full (or given) HP, e.g. the "200-Hour Save File" revive.</summary>
        public void Revive(float fraction = 1f)
        {
            IsDead = false;
            Current = Mathf.Clamp(maxHealth * fraction, 1f, maxHealth);
            NotifyChanged();
        }

        private void Die(DamageInfo killingBlow)
        {
            IsDead = true;
            Died?.Invoke(killingBlow);
            if (team == Team.Player)
                GameEvents.RaisePlayerDied();
            if (destroyOnDeath)
                Destroy(gameObject);
        }

        private void NotifyChanged()
        {
            Changed?.Invoke(Current, maxHealth);
            if (team == Team.Player)
                GameEvents.RaisePlayerHealthChanged(Current, maxHealth);
        }
    }
}
