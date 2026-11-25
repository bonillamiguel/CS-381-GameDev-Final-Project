using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// A bullet. Whoever fires it calls Launch with the hit to deal, its velocity and lifetime.
    /// Damages the first IDamageable of another team it touches; stops on solid (non-trigger) colliders
    /// such as walls; passes through its own team and through other triggers.
    /// Prefab needs: Rigidbody2D (Dynamic, Gravity Scale 0) and a Collider2D with Is Trigger on.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Turn the sprite to face the direction of travel.")]
        [SerializeField] private bool rotateToVelocity = true;

        private Rigidbody2D body;
        private DamageInfo hit;
        private bool launched;
        private bool spent;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Starts the projectile. hit.Direction is overwritten with the travel direction.</summary>
        public void Launch(DamageInfo damage, Vector2 velocity, float lifetime)
        {
            hit = damage;
            hit.Direction = velocity.sqrMagnitude > 0f ? velocity.normalized : Vector2.right;
            body.linearVelocity = velocity;
            if (rotateToVelocity)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(hit.Direction.y, hit.Direction.x) * Mathf.Rad2Deg);
            launched = true;
            Destroy(gameObject, Mathf.Max(0.01f, lifetime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!launched || spent)
                return;
            if (hit.Source != null && other.transform.IsChildOf(hit.Source.transform))
                return; // never hit the shooter

            var target = other.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                if (target.IsDead)
                    return;
                if (target.Team == hit.SourceTeam && hit.SourceTeam != Team.Neutral)
                    return; // fly through allies
                target.TakeDamage(hit); // false when invincible: the shot is still used up
                Spend();
                return;
            }

            if (!other.isTrigger)
                Spend(); // wall or other solid
        }

        private void Spend()
        {
            spent = true;
            Destroy(gameObject);
        }
    }
}
