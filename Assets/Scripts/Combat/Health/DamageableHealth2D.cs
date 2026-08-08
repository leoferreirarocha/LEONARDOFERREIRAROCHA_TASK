using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Provides a simple reusable health component for damageable
    /// gameplay objects.
    ///
    /// Enemy behavior remains independent from health and damage rules.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageableHealth2D :
        MonoBehaviour
    {
        [Header("Health")]

        [SerializeField, Min(1)]
        private int maximumHealth = 3;

        [Header("Death")]

        [Tooltip(
            "Automatically destroys this GameObject when health reaches zero. " +
            "Disable this when another gameplay system owns death behavior."
        )]
        [SerializeField]
        private bool destroyOnDeath;

        [Header("Events")]

        [SerializeField]
        private UnityEvent onDamaged;

        [SerializeField]
        private UnityEvent onDeath;

        private int currentHealth;
        private bool isDead;

        /// <summary>
        /// Gets the object's current health.
        /// </summary>
        public int CurrentHealth =>
            currentHealth;

        /// <summary>
        /// Gets the maximum health configured for this object.
        /// </summary>
        public int MaximumHealth =>
            maximumHealth;

        /// <summary>
        /// Gets whether this object has already reached zero health.
        /// </summary>
        public bool IsDead =>
            isDead;

        private void Awake()
        {
            currentHealth =
                maximumHealth;
        }

        /// <summary>
        /// Applies positive damage to this object.
        ///
        /// Damage requests are ignored after death.
        /// </summary>
        public void TakeDamage(
            int amount
        )
        {
            if (isDead ||
                amount <= 0)
            {
                return;
            }

            currentHealth =
                Mathf.Max(
                    0,
                    currentHealth - amount
                );

            onDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Restores this health component to its initial state.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth =
                maximumHealth;

            isDead =
                false;
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead =
                true;

            onDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(
                    gameObject
                );
            }
        }

        private void OnValidate()
        {
            maximumHealth =
                Mathf.Max(
                    1,
                    maximumHealth
                );
        }
    }
}