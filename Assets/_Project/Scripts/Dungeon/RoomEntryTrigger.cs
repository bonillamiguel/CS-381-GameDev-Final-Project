using System;
using UnityEngine;

namespace Game.Dungeon
{
    /// <summary>
    /// Goes on the trigger child of a room. Trigger callbacks only reach scripts on the collider's own
    /// object, so this passes "the player walked in" up to the Room.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomEntryTrigger : MonoBehaviour
    {
        [Tooltip("Tag on the player's collider or on the object that has its Rigidbody2D.")]
        [SerializeField] private string playerTag = "Player";

        public event Action PlayerEntered;

        private void Awake()
        {
            var trigger = GetComponent<Collider2D>();
            if (!trigger.isTrigger)
                Debug.LogWarning($"RoomEntryTrigger '{name}': the collider should have Is Trigger ticked.", this);
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other))
                PlayerEntered?.Invoke();
        }

        private bool IsPlayer(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                return true;
            var body = other.attachedRigidbody;
            return body != null && body.CompareTag(playerTag);
        }
    }
}
