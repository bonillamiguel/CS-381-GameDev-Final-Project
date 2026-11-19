using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public enum UpgradePool
    {
        Tech,        // normal rooms, free
        Imagination  // fragment rooms only, raises suspicion
    }

    public enum UpgradeRarity
    {
        Common,
        Rare,
        Epic
    }

    /// <summary>
    /// Special behaviors that aren't just stat changes. The owning system checks for these
    /// (e.g. the dash script checks for DashSlowTime). Add new entries at the END only.
    /// </summary>
    public enum UpgradeEffect
    {
        None = 0,
        DashSlowTime = 1,          // Sandwichvistan
        LastSecondDodgeSlowTime = 2, // Kerenzik-off
        ChainHit = 3,              // Mono-Wi-Fi
        BuildBleed = 4,            // Creeks of Ketchup
        HealOnCrit = 5,            // Assassin's Crimson Spork
        DashHidden = 6,            // Bloodhound's Shuffle
        SpecialFlipSlam = 7,       // Ash of Warranty: Lion's Paw
        DamageUntilLevelEnd = 8,   // Ruin Arc
        RevealFloor = 9,           // Hand-Drawn Map
        BossSummonHelper = 10,     // The Helper in the Jar Hat
        FloorWarnings = 11,        // Floor Message: "Try Dashing"
        EarlierTelegraphs = 12,    // Fan Wiki Page
        NoReloadAtRoomStart = 13,  // Fan-Made Mod: Bottomless Mag
        ReviveOnce = 14,           // 200-Hour Save File
        DoodleCompanion = 15,      // Doodled Sidekick
    }

    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Unprompted/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description; // effect text plus lore
        [Tooltip("Tech: the line of code shown, e.g. maxHP += 15. Imagination: a short human line.")]
        [SerializeField] private string codeLine;
        [SerializeField] private Sprite icon;

        [Header("Pool")]
        [SerializeField] private UpgradePool pool = UpgradePool.Tech;
        [SerializeField] private UpgradeRarity rarity = UpgradeRarity.Common;
        [Tooltip("How much suspicion taking this adds. Only used for Imagination upgrades.")]
        [SerializeField, Min(0)] private int suspicionCost = 1;

        [Header("Effect")]
        [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();
        [SerializeField] private UpgradeEffect effect = UpgradeEffect.None;
        [Tooltip("Meaning depends on the effect, e.g. slow-time strength or chain count.")]
        [SerializeField] private float effectValue;
        [Tooltip("Meaning depends on the effect, e.g. duration in seconds.")]
        [SerializeField] private float effectDuration;

        public string DisplayName => displayName;
        public string Description => description;
        public string CodeLine => codeLine;
        public Sprite Icon => icon;
        public UpgradePool Pool => pool;
        public UpgradeRarity Rarity => rarity;
        public int SuspicionCost => pool == UpgradePool.Imagination ? suspicionCost : 0;
        public IReadOnlyList<StatModifier> Modifiers => modifiers;
        public UpgradeEffect Effect => effect;
        public float EffectValue => effectValue;
        public float EffectDuration => effectDuration;
    }
}
