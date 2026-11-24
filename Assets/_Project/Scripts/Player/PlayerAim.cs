using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Turns an aim pivot toward the mouse cursor in world space. Weapons should be children of the pivot
    /// and fire along its right (+X) axis, or read AimDirection / AimPoint directly.
    /// Rotates a child pivot instead of the player root so the body sprite stays upright.
    /// </summary>
    public class PlayerAim : MonoBehaviour
    {
        [Tooltip("Child transform that rotates to face the cursor. Its +X axis points at the mouse.")]
        [SerializeField] private Transform aimPivot;
        [Tooltip("Camera used to convert the cursor to world space. Empty = the MainCamera-tagged camera.")]
        [SerializeField] private Camera aimCamera;
        [Tooltip("Optional sprite to flip so the character faces the side the cursor is on.")]
        [SerializeField] private SpriteRenderer bodySprite;
        [Tooltip("Ignore the cursor when it is closer than this to the pivot, so aim doesn't jitter.")]
        [SerializeField, Min(0f)] private float deadZone = 0.1f;

        /// <summary>Normalized world-space direction from the pivot to the cursor.</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        /// <summary>Cursor position in world space.</summary>
        public Vector2 AimPoint { get; private set; }

        /// <summary>Aim angle in degrees, 0 = right, counter-clockwise.</summary>
        public float AimAngle { get; private set; }

        private void Awake()
        {
            if (aimCamera == null)
                aimCamera = Camera.main;
            if (aimPivot == null)
                aimPivot = transform;
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || aimCamera == null)
                return;

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = aimCamera.ScreenToWorldPoint(
                new Vector3(screen.x, screen.y, aimPivot.position.z - aimCamera.transform.position.z));
            AimPoint = world;

            Vector2 toCursor = AimPoint - (Vector2)aimPivot.position;
            if (toCursor.sqrMagnitude < deadZone * deadZone)
                return;

            AimDirection = toCursor.normalized;
            AimAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            if (aimPivot != transform)
                aimPivot.rotation = Quaternion.Euler(0f, 0f, AimAngle);

            if (bodySprite != null)
                bodySprite.flipX = AimDirection.x < 0f;
        }
    }
}
