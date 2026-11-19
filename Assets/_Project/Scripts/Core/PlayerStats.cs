using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The player's base stats plus the modifiers that upgrades add.
    /// A plain class (not a MonoBehaviour) so the player can hold it as a serialized field,
    /// tune base values in the Inspector, and it can be unit-tested without a scene.
    ///
    ///   [SerializeField] private PlayerStats stats = new PlayerStats();
    ///   float speed = stats.Get(StatType.MoveSpeed);
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        [Serializable]
        public struct BaseValue
        {
            public StatType stat;
            public float value;

            public BaseValue(StatType stat, float value)
            {
                this.stat = stat;
                this.value = value;
            }
        }

        [SerializeField] private List<BaseValue> baseValues = DefaultBaseValues();

        private readonly List<StatModifier> modifiers = new List<StatModifier>();

        /// <summary>Raised whenever a modifier is added or removed. Listeners re-read the stats they care about.</summary>
        public event Action Changed;

        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        public float GetBase(StatType stat)
        {
            foreach (var b in baseValues)
                if (b.stat == stat)
                    return b.value;
            return 0f;
        }

        public float Get(StatType stat)
        {
            float flat = 0f, percentAdd = 0f, mult = 1f;
            foreach (var m in modifiers)
            {
                if (m.stat != stat)
                    continue;
                switch (m.type)
                {
                    case ModifierType.Flat: flat += m.value; break;
                    case ModifierType.PercentAdd: percentAdd += m.value; break;
                    case ModifierType.PercentMult: mult *= 1f + m.value; break;
                }
            }
            return (GetBase(stat) + flat) * (1f + percentAdd) * mult;
        }

        public int GetInt(StatType stat) => Mathf.RoundToInt(Get(stat));

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            Changed?.Invoke();
        }

        /// <summary>Adds all of an upgrade's stat modifiers, tagged with the upgrade so they can be removed later.</summary>
        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
                return;
            foreach (var m in upgrade.Modifiers)
                modifiers.Add(new StatModifier(m.stat, m.type, m.value, upgrade));
            Changed?.Invoke();
        }

        public int RemoveModifiersFromSource(object source)
        {
            int removed = modifiers.RemoveAll(m => ReferenceEquals(m.source, source));
            if (removed > 0)
                Changed?.Invoke();
            return removed;
        }

        public void ClearModifiers()
        {
            if (modifiers.Count == 0)
                return;
            modifiers.Clear();
            Changed?.Invoke();
        }

        /// <summary>Starting numbers; tune them in the Inspector rather than here.</summary>
        public static List<BaseValue> DefaultBaseValues() => new List<BaseValue>
        {
            new BaseValue(StatType.MaxHealth, 100f),
            new BaseValue(StatType.MoveSpeed, 6f),
            new BaseValue(StatType.Damage, 1f),
            new BaseValue(StatType.MeleeDamage, 1f),
            new BaseValue(StatType.RangedDamage, 1f),
            new BaseValue(StatType.SpecialDamage, 1f),
            new BaseValue(StatType.CritChance, 0.05f),
            new BaseValue(StatType.CritMultiplier, 2f),
            new BaseValue(StatType.FireRate, 1f),
            new BaseValue(StatType.ReloadTime, 1f),
            new BaseValue(StatType.DashCharges, 1f),
            new BaseValue(StatType.DashCooldown, 1f),
            new BaseValue(StatType.DashDistance, 4f),
            new BaseValue(StatType.ProjectileHoming, 0f),
            new BaseValue(StatType.Knockback, 1f),
            new BaseValue(StatType.HealCharges, 0f),
        };
    }
}
