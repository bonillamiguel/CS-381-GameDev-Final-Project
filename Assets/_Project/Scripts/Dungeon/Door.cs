using UnityEngine;

namespace Game.Dungeon
{
    public enum DoorDirection
    {
        North,
        South,
        East,
        West
    }

    /// <summary>
    /// One doorway of a room. When locked, a solid blocker collider fills the gap and the
    /// locked visual shows; when unlocked, the blocker is off and the unlocked visual shows.
    /// Doors start unlocked. The owning Room locks and unlocks them.
    ///
    /// The floor generator (#16) links the matching doors of two neighbouring rooms with Connect().
    /// </summary>
    [DisallowMultipleComponent]
    public class Door : MonoBehaviour
    {
        private const float EntryGizmoRadius = 0.25f;

        [SerializeField] private DoorDirection direction = DoorDirection.North;
        [Tooltip("Solid (non-trigger) collider that fills the doorway. Enabled only while locked.")]
        [SerializeField] private Collider2D blocker;
        [Tooltip("Shown while locked. Optional.")]
        [SerializeField] private GameObject lockedVisual;
        [Tooltip("Shown while unlocked. Optional.")]
        [SerializeField] private GameObject unlockedVisual;
        [Tooltip("Where a player arriving through this door is placed, just inside the room. Defaults to the door itself.")]
        [SerializeField] private Transform entryPoint;
        [Tooltip("The matching door in the neighbouring room. Set by the floor generator through Connect().")]
        [SerializeField] private Door connectedDoor;

        private bool isLocked;
        private Room room;

        public DoorDirection Direction => direction;
        public bool IsLocked => isLocked;
        public Transform EntryPoint => entryPoint != null ? entryPoint : transform;
        public Door ConnectedDoor => connectedDoor;
        public bool IsConnected => connectedDoor != null;
        public Room ConnectedRoom => connectedDoor != null ? connectedDoor.Room : null;

        /// <summary>The room this door belongs to.</summary>
        public Room Room
        {
            get
            {
                if (room == null)
                    room = GetComponentInParent<Room>(true);
                return room;
            }
        }

        private void Awake()
        {
            if (blocker == null)
                Debug.LogWarning($"Door '{name}' has no blocker collider, so it can't stop the player when locked.", this);
            ApplyState();
        }

        public void Lock() => SetLocked(true);
        public void Unlock() => SetLocked(false);

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            ApplyState();
        }

        /// <summary>Links this door and the other one to each other, dropping any old links. Null disconnects.</summary>
        public void Connect(Door other)
        {
            if (other == this)
            {
                Debug.LogWarning($"Door '{name}' can't connect to itself.", this);
                return;
            }
            if (other == connectedDoor)
                return;

            Disconnect();
            if (other == null)
                return;

            other.Disconnect();
            connectedDoor = other;
            other.connectedDoor = this;
        }

        /// <summary>Removes the link on both sides.</summary>
        public void Disconnect()
        {
            if (connectedDoor == null)
                return;
            Door old = connectedDoor;
            connectedDoor = null;
            if (old.connectedDoor == this)
                old.connectedDoor = null;
        }

        public static DoorDirection Opposite(DoorDirection direction)
        {
            switch (direction)
            {
                case DoorDirection.North: return DoorDirection.South;
                case DoorDirection.South: return DoorDirection.North;
                case DoorDirection.East: return DoorDirection.West;
                default: return DoorDirection.East;
            }
        }

        private void ApplyState()
        {
            if (blocker != null)
                blocker.enabled = isLocked;
            if (lockedVisual != null)
                lockedVisual.SetActive(isLocked);
            if (unlockedVisual != null)
                unlockedVisual.SetActive(!isLocked);
        }

        private void OnDrawGizmos()
        {
            if (entryPoint == null)
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, entryPoint.position);
            Gizmos.DrawWireSphere(entryPoint.position, EntryGizmoRadius);
        }
    }
}
