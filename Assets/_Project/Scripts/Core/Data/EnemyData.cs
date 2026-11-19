using UnityEngine;

namespace Game.Core
{
    public enum EnemyRole
    {
        Rusher,
        Ranged,
        Tank,
        Summoner,
        Exploder,
        Boss
    }

    public enum EliteTrait
    {
        None,
        Shielded,
        Faster,
        SplitsOnDeath
    }

    /// <summary>
    /// Stats for one enemy type. Per-level reskins (prefab variants) point at their own EnemyData
    /// so the same AI script can have different numbers in each level.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Unprompted/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName;
        [SerializeField] private EnemyRole role = EnemyRole.Rusher;
        [SerializeField] private EliteTrait eliteTrait = EliteTrait.None;

        [Header("Stats")]
        [SerializeField, Min(1f)] private float maxHealth = 30f;
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [Tooltip("0 = full knockback, 1 = immune to knockback.")]
        [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float contactDamage = 10f;
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0f)] private float attackRange = 1.5f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.5f;
        [Tooltip("Seconds the red telegraph shows before the attack lands.")]
        [SerializeField, Min(0f)] private float telegraphTime = 0.6f;
        [Tooltip("Ranged and summoners: distance they try to keep from the player.")]
        [SerializeField, Min(0f)] private float preferredDistance = 0f;

        [Header("Rewards")]
        [SerializeField, Min(0)] private int currencyMin = 1;
        [SerializeField, Min(0)] private int currencyMax = 3;

        public string DisplayName => displayName;
        public EnemyRole Role => role;
        public EliteTrait EliteTrait => eliteTrait;
        public bool IsElite => eliteTrait != EliteTrait.None;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float KnockbackResistance => knockbackResistance;
        public float ContactDamage => contactDamage;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float TelegraphTime => telegraphTime;
        public float PreferredDistance => preferredDistance;
        public int CurrencyMin => currencyMin;
        public int CurrencyMax => Mathf.Max(currencyMin, currencyMax);

        public int RollCurrency() => Random.Range(currencyMin, CurrencyMax + 1);
    }
}
