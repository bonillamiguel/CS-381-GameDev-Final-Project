using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Everything that makes one level different. Levels 2 and 3 should need a new FloorData asset
    /// plus art and room prefabs, never new scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "Floor_", menuName = "Unprompted/Floor")]
    public class FloorData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName;          // "Cyberjunk 2177"
        [SerializeField, Min(1)] private int levelNumber = 1;
        [Tooltip("Exact scene name, as listed in Build Profiles.")]
        [SerializeField] private string sceneName;
        [SerializeField] private string levelCardText;        // "Welcome to Night-ish City"
        [SerializeField] private string currencyName = "Teddies";
        [SerializeField] private Sprite currencySprite;

        [Header("Rooms")]
        [SerializeField] private GameObject startRoom;
        [SerializeField] private List<GameObject> roomTemplates = new List<GameObject>();
        [SerializeField] private GameObject fragmentRoom;
        [SerializeField] private GameObject bossRoom;
        [SerializeField, Min(1)] private int minRooms = 6;
        [SerializeField, Min(1)] private int maxRooms = 8;

        [Header("Enemies")]
        [SerializeField] private List<GameObject> enemyPool = new List<GameObject>();
        [Tooltip("Only spawned when suspicion is high enough.")]
        [SerializeField] private List<GameObject> eliteEnemyPool = new List<GameObject>();
        [SerializeField] private GameObject bossPrefab;

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeData> techUpgrades = new List<UpgradeData>();
        [Tooltip("Imagination upgrades this level's fragment rooms can offer.")]
        [SerializeField] private List<UpgradeData> fragmentUpgrades = new List<UpgradeData>();

        [Header("Audio")]
        [SerializeField] private AudioClip music;
        [SerializeField] private AudioClip bossMusic;

        [Header("Progression")]
        [SerializeField] private FloorData nextFloor;         // null on the last level

        public string DisplayName => displayName;
        public int LevelNumber => levelNumber;
        public string SceneName => sceneName;
        public string LevelCardText => levelCardText;
        public string CurrencyName => currencyName;
        public Sprite CurrencySprite => currencySprite;
        public GameObject StartRoom => startRoom;
        public IReadOnlyList<GameObject> RoomTemplates => roomTemplates;
        public GameObject FragmentRoom => fragmentRoom;
        public GameObject BossRoom => bossRoom;
        public int MinRooms => minRooms;
        public int MaxRooms => Mathf.Max(minRooms, maxRooms);
        public IReadOnlyList<GameObject> EnemyPool => enemyPool;
        public IReadOnlyList<GameObject> EliteEnemyPool => eliteEnemyPool;
        public GameObject BossPrefab => bossPrefab;
        public IReadOnlyList<UpgradeData> TechUpgrades => techUpgrades;
        public IReadOnlyList<UpgradeData> FragmentUpgrades => fragmentUpgrades;
        public AudioClip Music => music;
        public AudioClip BossMusic => bossMusic;
        public FloorData NextFloor => nextFloor;
        public bool IsLastFloor => nextFloor == null;
    }
}
