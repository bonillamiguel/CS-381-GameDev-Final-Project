using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Smoothly follows a target on X/Y and keeps the camera's own Z. Put it on the camera.
    /// If target is empty it picks up the player from GameEvents.PlayerSpawned.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Seconds to roughly catch up with the target. 0 = locked on, higher = floatier.")]
        [SerializeField, Min(0f)] private float damping = 0.15f;
        [Tooltip("Offset from the target in world units (X/Y only).")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [Tooltip("Optional: lead the camera toward the cursor by this fraction of the aim distance.")]
        [SerializeField, Range(0f, 0.5f)] private float aimLookAhead = 0f;
        [SerializeField] private PlayerAim aimSource;

        private Vector3 velocity;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                aimSource = value != null ? value.GetComponentInChildren<PlayerAim>() : null;
                SnapToTarget();
            }
        }

        private void OnEnable() => GameEvents.PlayerSpawned += HandlePlayerSpawned;
        private void OnDisable() => GameEvents.PlayerSpawned -= HandlePlayerSpawned;

        private void Start() => SnapToTarget();

        private void HandlePlayerSpawned(GameObject player)
        {
            if (player != null)
                Target = player.transform;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;
            transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref velocity, damping);
        }

        /// <summary>Jumps straight to the target, e.g. after a room transition.</summary>
        public void SnapToTarget()
        {
            if (target == null)
                return;
            transform.position = DesiredPosition();
            velocity = Vector3.zero;
        }

        private Vector3 DesiredPosition()
        {
            Vector2 point = (Vector2)target.position + offset;
            if (aimSource != null && aimLookAhead > 0f)
                point += (aimSource.AimPoint - (Vector2)target.position) * aimLookAhead;
            return new Vector3(point.x, point.y, transform.position.z);
        }
    }
}
