using LeonardoTask.Respawn;
using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Represents a projectile fired by the ranged enemy.
    ///
    /// The projectile travels toward the player's position captured
    /// when fired and immediately triggers the existing death system
    /// if it collides with the player.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyProjectile2D :
        MonoBehaviour
    {
        [Header("Movement")]

        [SerializeField, Min(0.01f)]
        private float speed = 18f;

        [SerializeField, Min(0.1f)]
        private float lifetime = 1.5f;

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

            projectileCollider.isTrigger =
                false;
        }

        /// <summary>
        /// Launches the projectile in the requested direction and ignores
        /// every collider belonging to the firing enemy.
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
                    Vector2.left;
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

            PlayerRespawnController player =
                other.GetComponentInParent
                    <PlayerRespawnController>();

            if (player != null)
            {
                player.Kill();
            }

            Destroy(
                gameObject
            );
        }

        private void IgnoreOwnerColliders()
        {
            if (owner == null)
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
    }
}