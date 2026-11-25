using System;
using System.Collections;
using Game.Combat;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Fires the equipped ranged WeaponData: hold Attack to shoot, Reload to reload (also automatic on empty).
    /// Spread blooms by spreadPerShot each shot up to maxSpread and recovers once you stop firing.
    /// Player stats scale it: Damage x RangedDamage, FireRate, ReloadTime, CritChance/CritMultiplier, Knockback.
    /// Ammo changes go out on GameEvents.PlayerAmmoChanged.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponData startingWeapon;
        [Tooltip("Where projectiles spawn. Make it a child of AimPivot, just in front of the player.")]
        [SerializeField] private Transform muzzle;
        [Tooltip("Player/Attack from the project-wide InputSystem_Actions asset.")]
        [SerializeField] private InputActionReference attackAction;
        [Tooltip("Player/Reload from the project-wide InputSystem_Actions asset.")]
        [SerializeField] private InputActionReference reloadAction;

        private PlayerAim aim;
        private PlayerStatsHolder statsHolder;
        private Health health;

        private WeaponData weapon;
        private int ammo;
        private float nextFireTime;
        private float lastShotTime = float.NegativeInfinity;
        private float currentSpread;
        private bool firingBurst;
        private Coroutine reloadRoutine;
        private float reloadStartTime;
        private float reloadDuration;

        /// <summary>Raised for every projectile fired, with its direction. For muzzle flash, sound, recoil.</summary>
        public event Action<Vector2> Fired;
        public event Action ReloadStarted;
        public event Action ReloadFinished;

        public WeaponData Weapon => weapon;
        public int Ammo => ammo;
        public bool IsReloading => reloadRoutine != null;
        public float CurrentSpread => currentSpread;
        public float ReloadProgress => IsReloading ? Mathf.Clamp01((Time.time - reloadStartTime) / reloadDuration) : 0f;
        private bool InfiniteAmmo => weapon != null && weapon.MagazineSize <= 0;

        private void Awake()
        {
            aim = GetComponent<PlayerAim>();
            statsHolder = GetComponent<PlayerStatsHolder>();
            health = GetComponent<Health>();
            if (muzzle == null)
                muzzle = transform;
        }

        private void Start()
        {
            SetWeapon(startingWeapon);
        }

        private void OnEnable()
        {
            EnableAction(attackAction);
            EnableAction(reloadAction);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            reloadRoutine = null;
            firingBurst = false;
        }

        /// <summary>Equips a weapon with a full magazine. Melee weapons are handled by another component.</summary>
        public void SetWeapon(WeaponData newWeapon)
        {
            StopAllCoroutines();
            reloadRoutine = null;
            firingBurst = false;
            weapon = newWeapon;
            if (weapon != null && weapon.Kind != WeaponKind.Ranged)
                Debug.LogWarning($"PlayerWeapon only fires ranged weapons; {weapon.name} is {weapon.Kind}.", this);
            ammo = weapon != null ? weapon.MagazineSize : 0;
            currentSpread = weapon != null ? weapon.BaseSpread : 0f;
            nextFireTime = 0f;
            SendAmmo();
        }

        private void Update()
        {
            if (weapon == null || weapon.Kind != WeaponKind.Ranged || (health != null && health.IsDead))
                return;

            RecoverSpread();

            if (Pressed(reloadAction))
                TryReload();

            if (Held(attackAction))
                TryFire();
        }

        /// <summary>Fires one shot (or burst) if able. Starts a reload when the magazine is empty.</summary>
        public bool TryFire()
        {
            if (firingBurst || IsReloading || Time.time < nextFireTime)
                return false;
            if (!InfiniteAmmo && ammo <= 0)
            {
                TryReload();
                return false;
            }

            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, weapon.AttacksPerSecond * Stat(StatType.FireRate, 1f));
            if (weapon.BurstCount > 1)
                StartCoroutine(FireBurst());
            else
                FireOne();
            return true;
        }

        public bool TryReload()
        {
            if (InfiniteAmmo || IsReloading || firingBurst || ammo >= weapon.MagazineSize)
                return false;
            reloadRoutine = StartCoroutine(Reload());
            return true;
        }

        private IEnumerator FireBurst()
        {
            firingBurst = true;
            for (int i = 0; i < weapon.BurstCount; i++)
            {
                if (!InfiniteAmmo && ammo <= 0)
                    break;
                FireOne();
                if (i < weapon.BurstCount - 1)
                    yield return new WaitForSeconds(weapon.BurstInterval);
            }
            firingBurst = false;
            if (!InfiniteAmmo && ammo <= 0)
                TryReload();
        }

        private IEnumerator Reload()
        {
            reloadStartTime = Time.time;
            reloadDuration = Mathf.Max(0.01f, weapon.ReloadTime * Stat(StatType.ReloadTime, 1f));
            ReloadStarted?.Invoke();
            SendAmmo();
            yield return new WaitForSeconds(reloadDuration);
            ammo = weapon.MagazineSize;
            reloadRoutine = null;
            ReloadFinished?.Invoke();
            SendAmmo();
        }

        private void FireOne()
        {
            if (weapon.ProjectilePrefab == null)
            {
                Debug.LogWarning($"{weapon.name} has no Projectile Prefab.", this);
                return;
            }

            Vector2 aimDir = aim != null ? aim.AimDirection : (Vector2)muzzle.right;
            float offset = UnityEngine.Random.Range(-currentSpread, currentSpread) * 0.5f;
            Vector2 dir = Quaternion.Euler(0f, 0f, offset) * aimDir;

            bool crit = UnityEngine.Random.value < Stat(StatType.CritChance, 0f);
            float damage = weapon.Damage * Stat(StatType.Damage, 1f) * Stat(StatType.RangedDamage, 1f);
            if (crit)
                damage *= Stat(StatType.CritMultiplier, 2f);
            var hit = new DamageInfo(damage, Team.Player, gameObject, dir,
                                     weapon.Knockback * Stat(StatType.Knockback, 1f), crit);

            GameObject shot = Instantiate(weapon.ProjectilePrefab, muzzle.position, Quaternion.identity);
            if (shot.TryGetComponent(out Projectile projectile))
                projectile.Launch(hit, dir * weapon.ProjectileSpeed, weapon.ProjectileLifetime);
            else
                Debug.LogWarning($"{weapon.ProjectilePrefab.name} has no Projectile component.", this);

            if (!InfiniteAmmo)
                ammo--;
            lastShotTime = Time.time;
            currentSpread = Mathf.Min(weapon.MaxSpread, currentSpread + weapon.SpreadPerShot);
            Fired?.Invoke(dir);
            SendAmmo();

            if (!InfiniteAmmo && ammo <= 0 && !firingBurst)
                TryReload();
        }

        /// <summary>Spread only recovers once you stop firing (longer than one shot interval since the last shot).</summary>
        private void RecoverSpread()
        {
            float interval = 1f / Mathf.Max(0.01f, weapon.AttacksPerSecond * Stat(StatType.FireRate, 1f));
            if (Time.time - lastShotTime > interval)
                currentSpread = Mathf.MoveTowards(currentSpread, weapon.BaseSpread, weapon.SpreadRecovery * Time.deltaTime);
        }

        private float Stat(StatType stat, float fallback) =>
            statsHolder != null ? statsHolder.Stats.Get(stat) : fallback;

        private void SendAmmo() =>
            GameEvents.RaisePlayerAmmoChanged(ammo, weapon != null ? weapon.MagazineSize : 0, IsReloading);

        private static void EnableAction(InputActionReference reference)
        {
            if (reference != null && reference.action != null && !reference.action.enabled)
                reference.action.Enable();
        }

        private static bool Pressed(InputActionReference reference) =>
            reference != null && reference.action != null && reference.action.WasPressedThisFrame();

        private static bool Held(InputActionReference reference) =>
            reference != null && reference.action != null && reference.action.IsPressed();
    }
}
