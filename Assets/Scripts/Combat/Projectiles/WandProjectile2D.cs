using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Represents a fast physical projectile fired by the player's Wand.
    ///
    /// The projectile uses continuous Rigidbody2D collision detection,
    /// ignores every collider belonging to its owner, applies damage
    /// to compatible targets, and disappears after impact or timeout.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WandProjectile2D :
        MonoBehaviour
    {
        [Header("Movement")]

        [SerializeField, Min(0.01f)]
        private float speed = 20f;

        [SerializeField, Min(0.1f)]
        private float lifetime = 0.8f;

        [Header("Damage")]

        [SerializeField, Min(1)]
        private int damage = 1;

        private Rigidbody2D body;
        private Collider2D projectileCollider;

        private Transform owner;
        private bool initialized;
        private bool consumed;

        private void Awake()
        {
            body =
                GetComponent<Rigidbody2D>();

            projectileCollider =
                GetComponent<Collider2D>();

            body.gravityScale =
                0f;

            body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            body.constraints |=
                RigidbodyConstraints2D.FreezeRotation;

            if (projectileCollider.isTrigger)
            {
                Debug.LogWarning(
                    $"{nameof(WandProjectile2D)} on '{name}' expects Is Trigger to be disabled. " +
                    "The collider was corrected automatically at runtime.",
                    this
                );

                projectileCollider.isTrigger =
                    false;
            }
        }

        /// <summary>
        /// Initializes projectile movement and prevents collisions
        /// between the projectile and its owner.
        /// </summary>
        public void Initialize(
            Vector2 direction,
            Transform projectileOwner
        )
        {
            if (initialized)
            {
                return;
            }

            initialized =
                true;

            owner =
                projectileOwner;

            IgnoreOwnerColliders();

            if (direction.sqrMagnitude <=
                Mathf.Epsilon)
            {
                direction =
                    Vector2.right;
            }

            body.linearVelocity =
                direction.normalized *
                speed;

            Destroy(
                gameObject,
                lifetime
            );
        }

        private void OnCollisionEnter2D(
            Collision2D collision
        )
        {
            if (!initialized ||
                consumed ||
                collision == null)
            {
                return;
            }

            Collider2D other =
                collision.collider;

            if (other == null ||
                IsOwnedCollider(other))
            {
                return;
            }

            consumed =
                true;

            DamageableHealth2D health =
                other.GetComponentInParent
                    <DamageableHealth2D>();

            if (health != null)
            {
                health.TakeDamage(
                    damage
                );
            }

            Destroy(
                gameObject
            );
        }

        /// <summary>
        /// Prevents the projectile from physically interacting with
        /// any collider belonging to its firing character.
        /// </summary>
        private void IgnoreOwnerColliders()
        {
            if (owner == null ||
                projectileCollider == null)
            {
                return;
            }

            Collider2D[] ownerColliders =
                owner.GetComponentsInChildren
                    <Collider2D>();

            foreach (
                Collider2D ownerCollider
                in ownerColliders
            )
            {
                if (ownerCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    projectileCollider,
                    ownerCollider,
                    true
                );
            }
        }

        private bool IsOwnedCollider(
            Collider2D other
        )
        {
            if (owner == null)
            {
                return false;
            }

            Transform otherTransform =
                other.transform;

            return otherTransform == owner ||
                   otherTransform.IsChildOf(owner);
        }

        private void OnValidate()
        {
            speed =
                Mathf.Max(
                    0.01f,
                    speed
                );

            lifetime =
                Mathf.Max(
                    0.1f,
                    lifetime
                );

            damage =
                Mathf.Max(
                    1,
                    damage
                );
        }
    }
}