using System.Collections.Generic;
using LeonardoTask.Respawn;
using UnityEngine;

namespace LeonardoTask.Combat
{
    /// <summary>
    /// Detects players entering or leaving the enemy's rectangular
    /// forward vision area.
    ///
    /// Detection is driven entirely through Collider2D trigger events
    /// and therefore requires no continuous physics queries.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyVisionTrigger2D :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private EnemyRangedAttack2D attack;

        private readonly Dictionary
            <PlayerRespawnController, int>
            overlapCounts = new();

        private BoxCollider2D visionCollider;

        private void Awake()
        {
            visionCollider =
                GetComponent<BoxCollider2D>();

            if (!visionCollider.isTrigger)
            {
                visionCollider.isTrigger =
                    true;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            PlayerRespawnController player =
                other.GetComponentInParent
                    <PlayerRespawnController>();

            if (player == null)
            {
                return;
            }

            if (overlapCounts.TryGetValue(
                    player,
                    out int count
                ))
            {
                overlapCounts[player] =
                    count + 1;

                return;
            }

            overlapCounts.Add(
                player,
                1
            );

            attack?.SetTarget(
                player.transform
            );
        }

        private void OnTriggerExit2D(
            Collider2D other
        )
        {
            PlayerRespawnController player =
                other.GetComponentInParent
                    <PlayerRespawnController>();

            if (player == null ||
                !overlapCounts.TryGetValue(
                    player,
                    out int count
                ))
            {
                return;
            }

            count--;

            if (count > 0)
            {
                overlapCounts[player] =
                    count;

                return;
            }

            overlapCounts.Remove(
                player
            );

            attack?.ClearTarget(
                player.transform
            );
        }

        private void OnDisable()
        {
            foreach (
                PlayerRespawnController player
                in overlapCounts.Keys
            )
            {
                if (player == null)
                {
                    continue;
                }

                attack?.ClearTarget(
                    player.transform
                );
            }

            overlapCounts.Clear();
        }
    }
}