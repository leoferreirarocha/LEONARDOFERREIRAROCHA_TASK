using UnityEngine;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Marks a 2D collider as lethal to the player.
    ///
    /// The same component supports trigger-based hazards such as pits
    /// and collision-based hazards when required by level geometry.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class DeathZone2D :
        MonoBehaviour
    {
        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            TryKillPlayer(
                other
            );
        }

        private void OnCollisionEnter2D(
            Collision2D collision
        )
        {
            TryKillPlayer(
                collision.collider
            );
        }

        private static void TryKillPlayer(
            Collider2D collider
        )
        {
            if (collider == null)
            {
                return;
            }

            PlayerRespawnController player =
                collider.GetComponentInParent
                    <PlayerRespawnController>();

            if (player == null)
            {
                return;
            }

            player.Kill();
        }
    }
}