using LeonardoTask.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Handles Wand usage while the Wand is equipped in the player's Hand.
    ///
    /// Each successful input press creates one projectile traveling
    /// horizontally in the direction the player is currently facing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WandItemUseBehaviour :
        EquippedItemUseBehaviour
    {
        [Header("Projectile")]

        [SerializeField]
        private WandProjectile2D projectilePrefab;

        [Tooltip(
            "World position from which Wand projectiles are created."
        )]
        [SerializeField]
        private Transform firePoint;

        [Tooltip(
            "Transform whose X scale indicates the player's current facing direction."
        )]
        [SerializeField]
        private Transform facingReference;

        [Tooltip(
            "Root transform ignored by projectiles so they cannot damage their owner."
        )]
        [SerializeField]
        private Transform ownerRoot;

        [Header("Fire Rate")]

        [SerializeField, Min(0f)]
        private float fireCooldown = 0.2f;

        [Header("Events")]

        [SerializeField]
        private UnityEvent onFired;

        private float nextAllowedFireTime;

        /// <summary>
        /// Creates one Wand projectile when the cooldown allows it.
        /// </summary>
        public override bool BeginUse()
        {
            if (projectilePrefab == null ||
                firePoint == null ||
                facingReference == null ||
                ownerRoot == null)
            {
                return false;
            }

            if (Time.time <
                nextAllowedFireTime)
            {
                return false;
            }

            nextAllowedFireTime =
                Time.time +
                fireCooldown;

            Vector2 direction =
                GetFacingDirection();

            WandProjectile2D projectile =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );

            projectile.Initialize(
                direction,
                ownerRoot
            );

            onFired?.Invoke();

            return true;
        }

        /// <summary>
        /// Determines horizontal firing direction from the same visual
        /// X-scale flip already used by the player movement system.
        /// </summary>
        private Vector2 GetFacingDirection()
        {
            float horizontalSign =
                facingReference.lossyScale.x >= 0f
                    ? 1f
                    : -1f;

            return new Vector2(
                horizontalSign,
                0f
            );
        }

        private void OnValidate()
        {
            fireCooldown =
                Mathf.Max(
                    0f,
                    fireCooldown
                );

            if (ownerRoot == null)
            {
                EquippedItemUseController controller =
                    GetComponentInParent
                        <EquippedItemUseController>();

                if (controller != null)
                {
                    ownerRoot =
                        controller.transform;
                }
            }
        }
    }
}