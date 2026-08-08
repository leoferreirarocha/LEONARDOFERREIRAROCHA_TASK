using System;
using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Provides a lightweight reusable health system for
    /// damageable 2D gameplay objects.
    ///
    /// Combat behavior remains independent from health ownership,
    /// allowing enemies and future damageable objects to reuse it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageableHealth2D :
        MonoBehaviour
    {
        [Header("Health")]

        [SerializeField, Min(1)]
        private int maximumHealth = 10;

        [Header("Death")]

        [Tooltip(
            "Automatically destroys this GameObject at zero health. " +
            "Disable this when another gameplay system controls death."
        )]
        [SerializeField]
        private bool destroyOnDeath;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent onDamaged;

        [SerializeField]
        private UnityEvent onDeath;

        private int currentHealth;
        private bool isDead;

        /// <summary>
        /// Raised after valid damage is applied.
        ///
        /// Parameters contain current health followed by maximum health.
        /// </summary>
        public event Action<int, int> Damaged;

        /// <summary>
        /// Raised once when health reaches zero.
        /// </summary>
        public event Action Died;

        public int CurrentHealth =>
            currentHealth;

        public int MaximumHealth =>
            maximumHealth;

        public bool IsDead =>
            isDead;

        private void Awake()
        {
            ResetHealth();
        }

        /// <summary>
        /// Applies positive damage to this object.
        ///
        /// Returns true when damage was successfully applied.
        /// </summary>
        public bool TakeDamage(
            int amount
        )
        {
            if (isDead ||
                amount <= 0)
            {
                return false;
            }

            currentHealth =
                Mathf.Max(
                    0,
                    currentHealth - amount
                );

            Damaged?.Invoke(
                currentHealth,
                maximumHealth
            );

            onDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }

            return true;
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

            Died?.Invoke();

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