using LeonardoTask.Progress;
using UnityEngine;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Updates the player's active respawn point and records the
    /// encounter checkpoint in persistent game progression.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class RespawnCheckpointTrigger2D :
        MonoBehaviour
    {
        [Header("Checkpoint")]

        [SerializeField]
        private Transform checkpointPoint;

        [SerializeField]
        private GameProgressController progress;

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
            if (progress != null &&
                progress.EnemyCheckpointReached)
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

            player.SetRespawnPoint(
                checkpointPoint
            );

            progress?.MarkEnemyCheckpointReached();

            triggerCollider.enabled =
                false;
        }
    }
}