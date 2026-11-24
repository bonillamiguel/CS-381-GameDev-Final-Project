using System;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Dash in the movement direction (or the aim direction when standing still), with invincibility frames.
    /// Distance, charges and cooldown come from PlayerStats (DashDistance, DashCharges, DashCooldown) so
    /// upgrades change them. Charges refill one at a time; progress goes out on
    /// GameEvents.PlayerDashCooldownChanged for the HUD.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement))]
    public class PlayerDash : MonoBehaviour
    {
        [Tooltip("Player/Dash from the project-wide InputSystem_Actions asset.")]
        [SerializeField] private InputActionReference dashAction;
        [Tooltip("Seconds the dash lasts. Speed = distance / duration.")]
        [SerializeField, Min(0.01f)] private float duration = 0.15f;
        [Tooltip("Seconds of invincibility from the start of the dash. Usually a bit longer than the dash.")]
        [SerializeField, Min(0f)] private float invincibilityTime = 0.2f;

        [Header("Used only if there is no PlayerStatsHolder")]
        [SerializeField, Min(0f)] private float fallbackDistance = 4f;
        [SerializeField, Min(0.01f)] private float fallbackCooldown = 1f;
        [SerializeField, Min(1)] private int fallbackCharges = 1;

        private Rigidbody2D body;
        private PlayerMovement movement;
        private PlayerAim aim;
        private Health health;
        private PlayerStatsHolder statsHolder;

        private int chargesReady;
        private float rechargeTimer;
        private float dashTimer;
        private Vector2 dashVelocity;
        private Vector2 lastMoveDirection = Vector2.right;

        private int lastSentReady = -1, lastSentMax = -1;
        private float lastSentProgress = -1f;

        /// <summary>Raised when a dash starts, with its direction. For local effects (sound, trail, animation).</summary>
        public event Action<Vector2> Dashed;

        public bool IsDashing => dashTimer > 0f;
        public int ChargesReady => chargesReady;
        public int MaxCharges => statsHolder != null ? Mathf.Max(1, statsHolder.Stats.GetInt(StatType.DashCharges)) : fallbackCharges;
        public float Cooldown => Mathf.Max(0.01f, statsHolder != null ? statsHolder.Stats.Get(StatType.DashCooldown) : fallbackCooldown);
        public float Distance => statsHolder != null ? statsHolder.Stats.Get(StatType.DashDistance) : fallbackDistance;

        /// <summary>Progress of the charge currently refilling, 0..1. 1 when all charges are ready.</summary>
        public float RechargeProgress => chargesReady >= MaxCharges ? 1f : Mathf.Clamp01(rechargeTimer / Cooldown);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            movement = GetComponent<PlayerMovement>();
            aim = GetComponent<PlayerAim>();
            health = GetComponent<Health>();
            statsHolder = GetComponent<PlayerStatsHolder>();
        }

        private void Start()
        {
            chargesReady = MaxCharges;
            SendCooldown(force: true);
        }

        private void OnEnable()
        {
            if (dashAction != null && dashAction.action != null && !dashAction.action.enabled)
                dashAction.action.Enable();
        }

        private void OnDisable()
        {
            if (IsDashing)
                EndDash();
        }

        private void Update()
        {
            if (movement.MoveInput.sqrMagnitude > 0.01f)
                lastMoveDirection = movement.MoveInput.normalized;

            Recharge();

            if (dashAction != null && dashAction.action != null && dashAction.action.WasPressedThisFrame())
                TryDash();

            SendCooldown(force: false);
        }

        private void FixedUpdate()
        {
            if (!IsDashing)
                return;
            body.linearVelocity = dashVelocity;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        /// <summary>Starts a dash if a charge is ready. Returns false during a dash, with no charges, or when dead.</summary>
        public bool TryDash()
        {
            if (IsDashing || chargesReady <= 0 || (health != null && health.IsDead))
                return false;

            Vector2 direction = DashDirection();
            chargesReady--;
            dashTimer = duration;
            dashVelocity = direction * (Distance / duration);
            movement.InputEnabled = false;
            if (health != null && invincibilityTime > 0f)
                health.SetInvincibleFor(invincibilityTime);

            Dashed?.Invoke(direction);
            SendCooldown(force: true);
            return true;
        }

        private Vector2 DashDirection()
        {
            if (movement.MoveInput.sqrMagnitude > 0.01f)
                return movement.MoveInput.normalized;
            if (aim != null)
                return aim.AimDirection;
            return lastMoveDirection;
        }

        private void EndDash()
        {
            dashTimer = 0f;
            body.linearVelocity = Vector2.zero;
            movement.InputEnabled = true;
        }

        private void Recharge()
        {
            int max = MaxCharges;
            if (chargesReady > max)
                chargesReady = max; // lost a charge upgrade
            if (chargesReady >= max)
            {
                rechargeTimer = 0f;
                return;
            }

            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= Cooldown)
            {
                rechargeTimer -= Cooldown;
                chargesReady++;
                if (chargesReady >= max)
                    rechargeTimer = 0f;
            }
        }

        /// <summary>Raises the HUD event only when something visible changed.</summary>
        private void SendCooldown(bool force)
        {
            int max = MaxCharges;
            float progress = RechargeProgress;
            if (!force && chargesReady == lastSentReady && max == lastSentMax && Mathf.Abs(progress - lastSentProgress) < 0.01f)
                return;
            lastSentReady = chargesReady;
            lastSentMax = max;
            lastSentProgress = progress;
            GameEvents.RaisePlayerDashCooldownChanged(chargesReady, max, progress);
        }
    }
}
