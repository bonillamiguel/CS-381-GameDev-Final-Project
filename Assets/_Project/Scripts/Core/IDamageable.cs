using UnityEngine;

namespace Game.Core
{
    /// <summary>Which side something is on. Damage never hurts its own team.</summary>
    public enum Team
    {
        Player,
        Enemy,
        Neutral // hurts both sides (e.g. hazards)
    }

    /// <summary>Everything about a single hit, passed from attacker to target.</summary>
    public struct DamageInfo
    {
        public float Amount;
        public Team SourceTeam;
        public GameObject Source;   // who dealt it (can be null)
        public Vector2 Direction;   // normalized direction of the hit, used for knockback
        public float Knockback;     // knockback force; the target decides how to apply it
        public bool IsCritical;

        public DamageInfo(float amount, Team sourceTeam, GameObject source = null,
                          Vector2 direction = default, float knockback = 0f, bool isCritical = false)
        {
            Amount = amount;
            SourceTeam = sourceTeam;
            Source = source;
            Direction = direction;
            Knockback = knockback;
            IsCritical = isCritical;
        }
    }

    /// <summary>
    /// Anything that can be hit. Attackers look this up on whatever they hit:
    ///   var target = other.GetComponentInParent&lt;IDamageable&gt;();
    ///   target?.TakeDamage(new DamageInfo(10f, Team.Player, gameObject, dir, 5f));
    /// </summary>
    public interface IDamageable
    {
        Team Team { get; }
        bool IsDead { get; }

        /// <summary>Returns true if the damage was applied (false if dead, invincible, or same team).</summary>
        bool TakeDamage(DamageInfo info);
    }
}
