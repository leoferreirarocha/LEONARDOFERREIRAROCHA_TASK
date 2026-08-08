using System.Collections;
using LeonardoTask.CameraSystem;
using LeonardoTask.Interaction;
using LeonardoTask.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Coordinates player death, persistent checkpoint restoration,
    /// and local respawning without reloading the scene.
    ///
    /// Inventory and world progression remain untouched because
    /// respawning only changes the player's physical state and position.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerRespawnController :
        MonoBehaviour
    {
        private const string EnemyCheckpointPreferenceKey =
            "respawn.enemy-checkpoint.reached";

        [Header("Player References")]

        [SerializeField]
        private PlayerMovement2D movement;

        [SerializeField]
        private PlayerInteractor2D interactor;

        [Tooltip(
            "Visual child rotated during the death presentation. " +
            "Do not assign the Player root."
        )]
        [SerializeField]
        private Transform visualRoot;

        [Header("Respawn Points")]

        [FormerlySerializedAs("respawnPoint")]
        [SerializeField]
        private Transform initialRespawnPoint;

        [Tooltip(
            "Persistent checkpoint used before the enemy encounter."
        )]
        [SerializeField]
        private Transform enemyCheckpointPoint;

        [SerializeField]
        private PlayerCameraFollow2D cameraFollow;

        [Header("Screen Fade")]

        [SerializeField]
        private ScreenFadeUI screenFade;

        [Header("Death Presentation")]

        [SerializeField]
        private float deathVisualRotationZ = 90f;

        [SerializeField, Min(0f)]
        private float fadeToBlackDuration = 0.3f;

        [SerializeField, Min(0f)]
        private float blackScreenHoldDuration = 0.15f;

        [SerializeField, Min(0f)]
        private float fadeFromBlackDuration = 0.3f;

        private Rigidbody2D body;

        private Transform currentRespawnPoint;

        private Quaternion initialVisualLocalRotation;

        private bool isDead;

        /// <summary>
        /// Gets whether a death sequence is currently active.
        /// </summary>
        public bool IsDead =>
            isDead;

        /// <summary>
        /// Gets the respawn point currently used by the player.
        /// </summary>
        public Transform CurrentRespawnPoint =>
            currentRespawnPoint;

        /// <summary>
        /// Gets whether the enemy checkpoint was reached in
        /// a previous or current game session.
        /// </summary>
        public static bool HasPersistentEnemyCheckpoint =>
            PlayerPrefs.GetInt(
                EnemyCheckpointPreferenceKey,
                0
            ) == 1;

        private void Awake()
        {
            body =
                GetComponent<Rigidbody2D>();

            if (movement == null)
            {
                movement =
                    GetComponent<PlayerMovement2D>();
            }

            if (interactor == null)
            {
                interactor =
                    GetComponent<PlayerInteractor2D>();
            }

            currentRespawnPoint =
                initialRespawnPoint;

            if (visualRoot != null)
            {
                initialVisualLocalRotation =
                    visualRoot.localRotation;
            }

            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        private void Start()
        {
            RestorePersistentCheckpoint();
        }

        /// <summary>
        /// Starts the player death sequence.
        ///
        /// Repeated death requests are ignored until the current
        /// respawn sequence has completely finished.
        /// </summary>
        public void Kill()
        {
            if (isDead ||
                !isActiveAndEnabled)
            {
                return;
            }

            StartCoroutine(
                DeathSequence()
            );
        }

        /// <summary>
        /// Changes the active respawn location for the current session.
        /// </summary>
        public void SetRespawnPoint(
            Transform newRespawnPoint
        )
        {
            if (newRespawnPoint == null)
            {
                return;
            }

            currentRespawnPoint =
                newRespawnPoint;
        }

        /// <summary>
        /// Activates and immediately persists the enemy checkpoint.
        ///
        /// This checkpoint is independent from enemy defeat.
        /// Reaching it is enough to make it the player's future
        /// starting and respawn location.
        /// </summary>
        public void ActivatePersistentEnemyCheckpoint(
            Transform checkpointPoint
        )
        {
            if (checkpointPoint == null)
            {
                return;
            }

            currentRespawnPoint =
                checkpointPoint;

            PlayerPrefs.SetInt(
                EnemyCheckpointPreferenceKey,
                1
            );

            // This checkpoint is activated only once, so forcing
            // an immediate write here is acceptable and guarantees
            // persistence even if the game is closed shortly afterward.
            PlayerPrefs.Save();
        }

        private IEnumerator DeathSequence()
        {
            isDead = true;

            LockPlayer();

            ApplyDeathVisual();

            yield return StartCoroutine(
                screenFade.FadeTo(
                    1f,
                    fadeToBlackDuration
                )
            );

            if (blackScreenHoldDuration > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        blackScreenHoldDuration
                    );
            }

            Respawn();

            yield return StartCoroutine(
                screenFade.FadeTo(
                    0f,
                    fadeFromBlackDuration
                )
            );

            ReleasePlayer();

            isDead = false;
        }

        /// <summary>
        /// Restores the saved enemy checkpoint when starting the game.
        ///
        /// The enemy does not need to be defeated for this to happen.
        /// </summary>
        private void RestorePersistentCheckpoint()
        {
            if (!HasPersistentEnemyCheckpoint)
            {
                currentRespawnPoint =
                    initialRespawnPoint;

                return;
            }

            if (enemyCheckpointPoint == null)
            {
                Debug.LogWarning(
                    $"{nameof(PlayerRespawnController)} found a saved enemy checkpoint, " +
                    "but no Enemy Checkpoint Point is assigned.",
                    this
                );

                currentRespawnPoint =
                    initialRespawnPoint;

                return;
            }

            currentRespawnPoint =
                enemyCheckpointPoint;

            PlacePlayerAt(
                enemyCheckpointPoint
            );
        }

        private void LockPlayer()
        {
            movement.SetMovementEnabled(
                false
            );

            interactor.SetInteractionEnabled(
                false
            );

            body.linearVelocity =
                Vector2.zero;

            body.angularVelocity =
                0f;
        }

        private void ApplyDeathVisual()
        {
            visualRoot.localRotation =
                initialVisualLocalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    deathVisualRotationZ
                );
        }

        private void Respawn()
        {
            Transform targetPoint =
                currentRespawnPoint != null
                    ? currentRespawnPoint
                    : initialRespawnPoint;

            PlacePlayerAt(
                targetPoint
            );
        }

        /// <summary>
        /// Immediately places the player at the requested world position.
        /// </summary>
        private void PlacePlayerAt(
            Transform targetPoint
        )
        {
            if (targetPoint == null)
            {
                return;
            }

            interactor.ClearNearbyInteractables();

            body.linearVelocity =
                Vector2.zero;

            body.angularVelocity =
                0f;

            body.position =
                targetPoint.position;

            visualRoot.localRotation =
                initialVisualLocalRotation;

            cameraFollow.SnapToTarget();
        }

        private void ReleasePlayer()
        {
            body.linearVelocity =
                Vector2.zero;

            movement.SetMovementEnabled(
                true
            );

            interactor.SetInteractionEnabled(
                true
            );
        }

        /// <summary>
        /// Development utility used to clear the persisted enemy
        /// checkpoint without affecting inventory or game progression.
        /// </summary>
        [ContextMenu("Reset Saved Enemy Checkpoint")]
        private void ResetSavedEnemyCheckpoint()
        {
            PlayerPrefs.DeleteKey(
                EnemyCheckpointPreferenceKey
            );

            PlayerPrefs.Save();

            currentRespawnPoint =
                initialRespawnPoint;

            Debug.Log(
                "Saved enemy checkpoint was reset.",
                this
            );
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (movement == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires PlayerMovement2D.",
                    this
                );

                valid = false;
            }

            if (interactor == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires PlayerInteractor2D.",
                    this
                );

                valid = false;
            }

            if (visualRoot == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a visual root.",
                    this
                );

                valid = false;
            }

            if (initialRespawnPoint == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires an initial respawn point.",
                    this
                );

                valid = false;
            }

            if (enemyCheckpointPoint == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires an enemy checkpoint point.",
                    this
                );

                valid = false;
            }

            if (cameraFollow == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires PlayerCameraFollow2D.",
                    this
                );

                valid = false;
            }

            if (screenFade == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires ScreenFadeUI.",
                    this
                );

                valid = false;
            }

            return valid;
        }

        private void OnValidate()
        {
            fadeToBlackDuration =
                Mathf.Max(
                    0f,
                    fadeToBlackDuration
                );

            blackScreenHoldDuration =
                Mathf.Max(
                    0f,
                    blackScreenHoldDuration
                );

            fadeFromBlackDuration =
                Mathf.Max(
                    0f,
                    fadeFromBlackDuration
                );
        }
    }
}