using System.Collections;
using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Provides immediate visual feedback whenever an enemy receives damage.
    ///
    /// The SpriteRenderer temporarily changes to the configured flash
    /// color and automatically restores its original color afterward.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHitFlash2D :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private DamageableHealth2D health;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [Header("Flash")]

        [SerializeField]
        private Color flashColor =
            Color.white;

        [SerializeField, Min(0.01f)]
        private float flashDuration = 0.08f;

        private Color originalColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (spriteRenderer != null)
            {
                originalColor =
                    spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged +=
                    HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -=
                    HandleDamaged;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(
                    flashRoutine
                );

                flashRoutine =
                    null;
            }

            RestoreColor();
        }

        private void HandleDamaged(
            int currentHealth,
            int maximumHealth
        )
        {
            if (flashRoutine != null)
            {
                StopCoroutine(
                    flashRoutine
                );
            }

            flashRoutine =
                StartCoroutine(
                    FlashRoutine()
                );
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer == null)
            {
                yield break;
            }

            spriteRenderer.color =
                flashColor;

            yield return new WaitForSeconds(
                flashDuration
            );

            RestoreColor();

            flashRoutine =
                null;
        }

        private void RestoreColor()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color =
                    originalColor;
            }
        }

        private void OnValidate()
        {
            flashDuration =
                Mathf.Max(
                    0.01f,
                    flashDuration
                );
        }
    }
}