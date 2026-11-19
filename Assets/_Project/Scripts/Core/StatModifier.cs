using System;

namespace Game.Core
{
    /// <summary>
    /// Every number upgrades can change. Damage-type stats are multipliers (base 1) applied on top of WeaponData values.
    /// Add new entries at the END only: values are saved by number in assets, so inserting shifts every existing upgrade.
    /// </summary>
    public enum StatType
    {
        MaxHealth = 0,
        MoveSpeed = 1,
        Damage = 2,          // multiplier on all damage
        MeleeDamage = 3,     // multiplier on melee damage
        RangedDamage = 4,    // multiplier on ranged damage
        SpecialDamage = 5,   // multiplier on right-click special damage
        CritChance = 6,      // 0..1
        CritMultiplier = 7,
        FireRate = 8,        // multiplier
        ReloadTime = 9,      // multiplier, lower is faster
        DashCharges = 10,
        DashCooldown = 11,   // seconds
        DashDistance = 12,   // world units
        ProjectileHoming = 13, // 0 = none, 1 = strong
        Knockback = 14,      // multiplier
        HealCharges = 15,
    }

    public enum ModifierType
    {
        Flat,        // base + value          e.g. maxHP += 15
        PercentAdd,  // summed, then applied  e.g. melee.damage *= 1.3 -> 0.3
        PercentMult  // multiplied separately e.g. 0.5 -> x1.5
    }

    /// <summary>
    /// One change to one stat. Final value = (base + all Flat) * (1 + sum of PercentAdd) * each (1 + PercentMult).
    /// </summary>
    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public ModifierType type;
        public float value;

        /// <summary>What added it (usually an UpgradeData), so it can be removed later. Not saved.</summary>
        [NonSerialized] public object source;

        public StatModifier(StatType stat, ModifierType type, float value, object source = null)
        {
            this.stat = stat;
            this.type = type;
            this.value = value;
            this.source = source;
        }
    }
}
