using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Represents a projectile fired by the player's Wand.
    ///
    /// The projectile travels using Rigidbody2D linear velocity,
    /// damages compatible targets, ignores its owner, and destroys
    /// itself after impact or after exceeding its lifetime.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WandProjectile2D :
        MonoBehaviour
    {
        [Header("Movement")]

        [SerializeField, Min(0.01f)]
        private float speed = 12f;

        [SerializeField, Min(0.1f)]
        private float lifetime = 3f;

        [Header("Damage")]

        [SerializeField, Min(1)]
        private int damage = 1;

        [Header("Collision")]

        [Tooltip(
            "Layers that should destroy the projectile even when " +
            "they do not contain a DamageableHealth2D component."
        )]
        [SerializeField]
        private LayerMask obstacleLayers;

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

            if (!projectileCollider.isTrigger)
            {
                Debug.LogWarning(
                    $"{nameof(WandProjectile2D)} on '{name}' expects its Collider2D to use Is Trigger.",
                    this
                );
            }
        }

        /// <summary>
        /// Initializes projectile movement and ownership immediately
        /// after the prefab is instantiated.
        /// </summary>
        public void Initialize(
            Vector2 direction,
            Transform projectileOwner
        )
        {
            if (direction.sqrMagnitude <=
                Mathf.Epsilon)
            {
                direction =
                    Vector2.right;
            }

            owner =
                projectileOwner;

            body.linearVelocity =
                direction.normalized *
                speed;

            initialized =
                true;

            Destroy(
                gameObject,
                lifetime
            );
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            if (!initialized ||
                consumed ||
                other == null)
            {
                return;
            }

            if (IsOwnedCollider(other))
            {
                return;
            }

            DamageableHealth2D health =
                other.GetComponentInParent
                    <DamageableHealth2D>();

            if (health != null)
            {
                consumed =
                    true;

                health.TakeDamage(
                    damage
                );

                Destroy(
                    gameObject
                );

                return;
            }

            if (IsObstacleLayer(
                    other.gameObject.layer
                ))
            {
                consumed =
                    true;

                Destroy(
                    gameObject
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

        private bool IsObstacleLayer(
            int layer
        )
        {
            return (
                obstacleLayers.value &
                (1 << layer)
            ) != 0;
        }

        private void OnValidate()
        {
            speed =
                Mathf.Max(
                    speed,
                    0.01f
                );

            lifetime =
                Mathf.Max(
                    lifetime,
                    0.1f
                );

            damage =
                Mathf.Max(
                    damage,
                    1
                );
        }
    }
}