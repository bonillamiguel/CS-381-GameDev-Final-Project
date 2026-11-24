using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// 8-direction top-down movement. Reads the Move action (WASD / left stick), clamps it to length 1 so
    /// diagonals aren't faster, and sets the Rigidbody2D velocity in FixedUpdate.
    /// Speed comes from PlayerStats (MoveSpeed) when a PlayerStatsHolder is present, else from fallbackSpeed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Player/Move from the project-wide InputSystem_Actions asset.")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Used only if there is no PlayerStatsHolder on this object.")]
        [SerializeField, Min(0f)] private float fallbackSpeed = 6f;

        private Rigidbody2D body;
        private PlayerStatsHolder statsHolder;
        private Vector2 moveInput;

        /// <summary>Last movement input, length 0..1. Other scripts (dash, animation) can read it.</summary>
        public Vector2 MoveInput => moveInput;

        /// <summary>Set false to let another script (e.g. dash, knockback) drive the Rigidbody2D.</summary>
        public bool InputEnabled { get; set; } = true;

        public float Speed => statsHolder != null ? statsHolder.Stats.Get(StatType.MoveSpeed) : fallbackSpeed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            statsHolder = GetComponent<PlayerStatsHolder>();
        }

        private void OnEnable()
        {
            // Project-wide actions are enabled automatically; this covers a reference to another asset.
            if (moveAction != null && moveAction.action != null && !moveAction.action.enabled)
                moveAction.action.Enable();
        }

        private void Update()
        {
            Vector2 raw = moveAction != null && moveAction.action != null
                ? moveAction.action.ReadValue<Vector2>()
                : Vector2.zero;
            moveInput = Vector2.ClampMagnitude(raw, 1f);
        }

        private void FixedUpdate()
        {
            if (!InputEnabled)
                return;
            body.linearVelocity = moveInput * Speed;
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }
    }
}
