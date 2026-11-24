using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Dungeon
{
    /// <summary>Reward covers fragment, heal and shop rooms.</summary>
    public enum RoomType
    {
        Start,
        Combat,
        Boss,
        Reward
    }

    /// <summary>
    /// Root of a room prefab. The floor generator (#16) places rooms and links their doors;
    /// the wave spawner (#14) uses the spawn points and raises RoomCleared when the last enemy dies.
    ///
    /// Every time the player enters the room it raises GameEvents.RoomEntered (minimap, camera, music).
    /// The first entry into an uncleared Combat or Boss room locks every door first, so listeners
    /// already see IsLocked. GameEvents.RoomCleared for this room unlocks them for good.
    /// Start and Reward rooms count as cleared from the beginning and never lock.
    ///
    /// The room's centre is this object's position. See docs/rooms.md for how to build one.
    /// </summary>
    [DisallowMultipleComponent]
    public class Room : MonoBehaviour
    {
        private enum State
        {
            Unvisited,
            Locked,
            Cleared
        }

        private const float SpawnGizmoRadius = 0.3f;

        [SerializeField] private RoomType type = RoomType.Combat;
        [Tooltip("Outer size in world units, centred on this object. The floor generator uses it to place rooms.")]
        [SerializeField] private Vector2 size = new Vector2(16f, 10f);
        [Tooltip("Trigger covering the inside of the room. Keep it at least one tile away from every doorway.")]
        [SerializeField] private RoomEntryTrigger entryTrigger;
        [SerializeField] private List<Door> doors = new List<Door>();
        [Tooltip("Where the wave spawner (#14) may place enemies.")]
        [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();

        private State state = State.Unvisited;
        private bool isCurrent;

        public RoomType Type => type;
        public bool IsCleared => state == State.Cleared;
        public bool IsLocked => state == State.Locked;
        /// <summary>True while the player is in this room (it was the last room to raise RoomEntered).</summary>
        public bool IsCurrent => isCurrent;
        public bool LocksOnEntry => type == RoomType.Combat || type == RoomType.Boss;
        public Vector2 Size => size;
        public Bounds WorldBounds => new Bounds(transform.position, new Vector3(size.x, size.y, 0f));
        public IReadOnlyList<Door> Doors => doors;
        public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;

        /// <summary>The door on the given side, or null if this room has none there.</summary>
        public Door GetDoor(DoorDirection direction)
        {
            foreach (var door in doors)
                if (door != null && door.Direction == direction)
                    return door;
            return null;
        }

        private void Awake()
        {
            if (!LocksOnEntry)
                state = State.Cleared;
            if (entryTrigger == null)
                Debug.LogWarning($"Room '{name}' has no entry trigger, so it will never raise RoomEntered or lock.", this);
        }

        private void OnEnable()
        {
            if (entryTrigger != null)
                entryTrigger.PlayerEntered += HandlePlayerEntered;
            GameEvents.RoomEntered += HandleRoomEntered;
            GameEvents.RoomCleared += HandleRoomCleared;
        }

        private void OnDisable()
        {
            if (entryTrigger != null)
                entryTrigger.PlayerEntered -= HandlePlayerEntered;
            GameEvents.RoomEntered -= HandleRoomEntered;
            GameEvents.RoomCleared -= HandleRoomCleared;
        }

        private void HandlePlayerEntered()
        {
            // Stepping out of the trigger and back in without visiting another room is not a new entry.
            // This also covers players with several colliders and players inside a locked room.
            if (isCurrent)
                return;

            if (state == State.Unvisited && LocksOnEntry)
            {
                state = State.Locked;
                SetDoorsLocked(true);
            }

            isCurrent = true;
            GameEvents.RaiseRoomEntered(gameObject);
        }

        private void HandleRoomEntered(GameObject room)
        {
            isCurrent = room == gameObject;
        }

        private void HandleRoomCleared(GameObject room)
        {
            if (room != gameObject || state == State.Cleared)
                return;
            state = State.Cleared;
            SetDoorsLocked(false);
        }

        private void SetDoorsLocked(bool locked)
        {
            foreach (var door in doors)
                if (door != null)
                    door.SetLocked(locked);
        }

        // Runs when the component is added in the Editor (or on Reset), so the lists start filled in.
        private void Reset()
        {
            CollectFromChildren();
        }

        [ContextMenu("Collect Doors And Trigger From Children")]
        private void CollectFromChildren()
        {
            entryTrigger = GetComponentInChildren<RoomEntryTrigger>(true);
            doors = new List<Door>(GetComponentsInChildren<Door>(true));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));

            Gizmos.color = Color.red;
            foreach (var point in enemySpawnPoints)
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, SpawnGizmoRadius);
        }
    }
}
