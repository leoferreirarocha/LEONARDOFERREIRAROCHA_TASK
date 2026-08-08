using System.Collections;
using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Controls the stationary ranged attack behavior of the enemy.
    ///
    /// While a player remains inside the vision trigger, the enemy fires
    /// three-shot bursts toward the player's current position and waits
    /// before beginning the next burst.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyRangedAttack2D :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private DamageableHealth2D health;

        [SerializeField]
        private EnemyProjectile2D projectilePrefab;

        [SerializeField]
        private Transform firePoint;

        [Header("Burst")]

        [SerializeField, Min(1)]
        private int shotsPerBurst = 3;

        [SerializeField, Min(0f)]
        private float initialShotDelay = 0.35f;

        [SerializeField, Min(0.01f)]
        private float shotInterval = 0.18f;

        [SerializeField, Min(0.01f)]
        private float burstDelay = 1.1f;

        [Header("Aim")]

        [Tooltip(
            "Small world-space offset added to the player's current position."
        )]
        [SerializeField]
        private Vector2 targetAimOffset =
            new Vector2(0f, 0.25f);

        private Transform currentTarget;
        private Coroutine attackRoutine;

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died +=
                    HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -=
                    HandleDeath;
            }

            StopAttackRoutine();
        }

        /// <summary>
        /// Begins attacking the specified player while the target
        /// remains inside the enemy vision zone.
        /// </summary>
        public void SetTarget(
            Transform target
        )
        {
            if (target == null ||
                health == null ||
                health.IsDead)
            {
                return;
            }

            currentTarget =
                target;

            if (attackRoutine == null)
            {
                attackRoutine =
                    StartCoroutine(
                        AttackRoutine()
                    );
            }
        }

        /// <summary>
        /// Stops attacking when the current player leaves vision.
        /// </summary>
        public void ClearTarget(
            Transform target
        )
        {
            if (currentTarget != target)
            {
                return;
            }

            currentTarget =
                null;

            StopAttackRoutine();
        }

        private IEnumerator AttackRoutine()
        {
            if (initialShotDelay > 0f)
            {
                yield return new WaitForSeconds(
                    initialShotDelay
                );
            }

            while (currentTarget != null &&
                   health != null &&
                   !health.IsDead)
            {
                for (int i = 0;
                     i < shotsPerBurst;
                     i++)
                {
                    if (currentTarget == null ||
                        health.IsDead)
                    {
                        break;
                    }

                    FireProjectile();

                    if (i <
                        shotsPerBurst - 1)
                    {
                        yield return new WaitForSeconds(
                            shotInterval
                        );
                    }
                }

                if (currentTarget != null &&
                    !health.IsDead)
                {
                    yield return new WaitForSeconds(
                        burstDelay
                    );
                }
            }

            attackRoutine =
                null;
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null ||
                firePoint == null ||
                currentTarget == null)
            {
                return;
            }

            Vector2 targetPosition =
                (Vector2)currentTarget.position +
                targetAimOffset;

            Vector2 direction =
                targetPosition -
                (Vector2)firePoint.position;

            EnemyProjectile2D projectile =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );

            projectile.Initialize(
                direction,
                transform
            );
        }

        private void HandleDeath()
        {
            currentTarget =
                null;

            StopAttackRoutine();
        }

        private void StopAttackRoutine()
        {
            if (attackRoutine == null)
            {
                return;
            }

            StopCoroutine(
                attackRoutine
            );

            attackRoutine =
                null;
        }

        private void OnValidate()
        {
            shotsPerBurst =
                Mathf.Max(
                    1,
                    shotsPerBurst
                );

            initialShotDelay =
                Mathf.Max(
                    0f,
                    initialShotDelay
                );

            shotInterval =
                Mathf.Max(
                    0.01f,
                    shotInterval
                );

            burstDelay =
                Mathf.Max(
                    0.01f,
                    burstDelay
                );
        }
    }
}