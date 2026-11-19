using UnityEngine;

namespace Game.Core
{
    public enum WeaponKind
    {
        Ranged,
        Melee
    }

    /// <summary>
    /// All tunable numbers for one weapon. Damage here is the base value;
    /// the player's Damage / RangedDamage / MeleeDamage stats multiply it.
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Unprompted/Weapon")]
    public class WeaponData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private WeaponKind kind = WeaponKind.Ranged;

        [Header("Primary attack (left click)")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [Tooltip("Attacks per second. For burst weapons, bursts per second.")]
        [SerializeField, Min(0.01f)] private float attacksPerSecond = 3f;
        [SerializeField, Min(0f)] private float knockback = 2f;

        [Header("Ranged")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField, Min(0f)] private float projectileSpeed = 18f;
        [SerializeField, Min(0f)] private float projectileLifetime = 1.5f;
        [SerializeField, Min(1)] private int burstCount = 1;
        [SerializeField, Min(0f)] private float burstInterval = 0.06f;
        [Tooltip("0 = infinite ammo, no reload.")]
        [SerializeField, Min(0)] private int magazineSize = 12;
        [SerializeField, Min(0f)] private float reloadTime = 1.2f;

        [Header("Spread bloom (degrees)")]
        [SerializeField, Min(0f)] private float baseSpread = 2f;
        [SerializeField, Min(0f)] private float spreadPerShot = 1.5f;
        [SerializeField, Min(0f)] private float maxSpread = 12f;
        [Tooltip("Degrees per second the spread recovers when not firing.")]
        [SerializeField, Min(0f)] private float spreadRecovery = 20f;

        [Header("Melee")]
        [SerializeField, Min(0f)] private float meleeRange = 1.5f;
        [SerializeField, Range(0f, 360f)] private float meleeArc = 120f;
        [SerializeField, Min(1)] private int comboHits = 1;
        [Tooltip("Time after the last swing before the combo resets.")]
        [SerializeField, Min(0f)] private float comboResetTime = 0.6f;

        [Header("Special (right click)")]
        [SerializeField] private string specialName;
        [SerializeField, Min(0f)] private float specialCooldown = 4f;
        [SerializeField, Min(0f)] private float specialDamage = 20f;

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public WeaponKind Kind => kind;
        public float Damage => damage;
        public float AttacksPerSecond => attacksPerSecond;
        public float Knockback => knockback;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int BurstCount => burstCount;
        public float BurstInterval => burstInterval;
        public int MagazineSize => magazineSize;
        public float ReloadTime => reloadTime;
        public float BaseSpread => baseSpread;
        public float SpreadPerShot => spreadPerShot;
        public float MaxSpread => maxSpread;
        public float SpreadRecovery => spreadRecovery;
        public float MeleeRange => meleeRange;
        public float MeleeArc => meleeArc;
        public int ComboHits => comboHits;
        public float ComboResetTime => comboResetTime;
        public string SpecialName => specialName;
        public float SpecialCooldown => specialCooldown;
        public float SpecialDamage => specialDamage;
    }
}
