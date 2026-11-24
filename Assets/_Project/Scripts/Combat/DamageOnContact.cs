using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Hurts any IDamageable that touches this trigger, then again every tickInterval while it stays inside.
    /// Use for hazards (spikes, lava) and enemy contact damage. Needs a Collider2D with Is Trigger on.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamageOnContact : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damage = 10f;
        [Tooltip("Neutral hurts both sides; Enemy hurts only the player; Player hurts only enemies.")]
        [SerializeField] private Team team = Team.Neutral;
        [Tooltip("Seconds between hits on the same target while it stays inside.")]
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField, Min(0f)] private float knockback = 0f;

        private readonly Dictionary<IDamageable, float> nextHitTime = new Dictionary<IDamageable, float>();

        private void OnTriggerStay2D(Collider2D other)
        {
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.IsDead)
                return;
            if (nextHitTime.TryGetValue(target, out float next) && Time.time < next)
                return;

            Vector2 direction = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            if (target.TakeDamage(new DamageInfo(damage, team, gameObject, direction, knockback)))
                nextHitTime[target] = Time.time + tickInterval;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var target = other.GetComponentInParent<IDamageable>();
            if (target != null)
                nextHitTime.Remove(target);
        }
    }
}
