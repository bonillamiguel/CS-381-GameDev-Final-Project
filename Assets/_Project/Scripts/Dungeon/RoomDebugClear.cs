using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Dungeon
{
    /// <summary>
    /// Editor-only test helper: press the key to raise RoomCleared for the room the player is in,
    /// so doors can be tested without enemies. Put one in a test scene. In builds it removes itself.
    /// </summary>
    public class RoomDebugClear : MonoBehaviour
    {
#pragma warning disable CS0414 // only read in the Editor
        [SerializeField] private Key clearKey = Key.F9;
        [Tooltip("Log RoomEntered and RoomCleared to the Console.")]
        [SerializeField] private bool logEvents = true;
#pragma warning restore CS0414

#if UNITY_EDITOR
        private GameObject currentRoom;

        private void OnEnable()
        {
            GameEvents.RoomEntered += HandleRoomEntered;
            GameEvents.RoomCleared += HandleRoomCleared;
        }

        private void OnDisable()
        {
            GameEvents.RoomEntered -= HandleRoomEntered;
            GameEvents.RoomCleared -= HandleRoomCleared;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[clearKey].wasPressedThisFrame)
                return;

            if (currentRoom == null)
            {
                Debug.Log("[RoomDebugClear] No current room to clear.", this);
                return;
            }
            GameEvents.RaiseRoomCleared(currentRoom);
        }

        private void HandleRoomEntered(GameObject room)
        {
            currentRoom = room;
            if (logEvents)
                Debug.Log($"[RoomDebugClear] RoomEntered: {room.name}", room);
        }

        private void HandleRoomCleared(GameObject room)
        {
            if (logEvents)
                Debug.Log($"[RoomDebugClear] RoomCleared: {room.name}", room);
        }
#else
        private void Awake()
        {
            Destroy(this);
        }
#endif
    }
}
