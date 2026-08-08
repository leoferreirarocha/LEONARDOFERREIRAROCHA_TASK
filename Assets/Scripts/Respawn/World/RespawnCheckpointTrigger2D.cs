using UnityEngine;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Updates the player's current respawn location when entered.
    ///
    /// The checkpoint only needs to activate once during the current
    /// gameplay session.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class RespawnCheckpointTrigger2D :
        MonoBehaviour
    {
        [Header("Checkpoint")]

        [Tooltip(
            "Exact world position used by future player respawns."
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

            activated =
                true;

            player.SetRespawnPoint(
                checkpointPoint
            );

            triggerCollider.enabled =
                false;
        }
    }
}