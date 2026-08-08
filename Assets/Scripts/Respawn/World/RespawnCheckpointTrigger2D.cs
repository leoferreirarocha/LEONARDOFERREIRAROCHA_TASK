using UnityEngine;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Activates the persistent checkpoint immediately before
    /// the enemy encounter.
    ///
    /// Reaching this trigger is enough to persist the checkpoint.
    /// Enemy state has no effect on checkpoint activation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class RespawnCheckpointTrigger2D :
        MonoBehaviour
    {
        [Header("Checkpoint")]

        [Tooltip(
            "Exact world position used for future player respawns."
        )]
        [SerializeField]
        private Transform checkpointPoint;

        private Collider2D triggerCollider;
        private bool activated;

        private void Awake()
        {
            triggerCollider =
                GetComponent<Collider2D>();

            triggerCollider.isTrigger =
                true;

            if (checkpointPoint == null)
            {
                checkpointPoint =
                    transform;
            }
        }

        private void Start()
        {
            // The checkpoint has already fulfilled its purpose
            // when it was persisted during a previous session.
            if (PlayerRespawnController
                .HasPersistentEnemyCheckpoint)
            {
                activated = true;

                triggerCollider.enabled =
                    false;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            if (activated)
            {
                return;
            }

            PlayerRespawnController player =
                other.GetComponentInParent
                    <PlayerRespawnController>();

            if (player == null)
            {
                return;
            }

            activated = true;

            player.ActivatePersistentEnemyCheckpoint(
                checkpointPoint
            );

            triggerCollider.enabled =
                false;
        }
    }
}