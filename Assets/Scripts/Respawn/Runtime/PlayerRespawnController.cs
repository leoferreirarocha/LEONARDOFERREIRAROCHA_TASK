using System.Collections;
using LeonardoTask.CameraSystem;
using LeonardoTask.Interaction;
using LeonardoTask.Player;
using LeonardoTask.Progress;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Coordinates player death, persistent checkpoint restoration,
    /// and local respawning without reloading the scene.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerRespawnController :
        MonoBehaviour
    {
        [Header("Player References")]

        [SerializeField]
        private PlayerMovement2D movement;

        [SerializeField]
        private PlayerInteractor2D interactor;

        [SerializeField]
        private Transform visualRoot;

        [Header("Progress")]

        [SerializeField]
        private GameProgressController progress;

        [Header("Respawn Points")]

        [FormerlySerializedAs("respawnPoint")]
        [SerializeField]
        private Transform initialRespawnPoint;

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

        public bool IsDead =>
            isDead;

        public Transform CurrentRespawnPoint =>
            currentRespawnPoint;

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

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.StateRestored +=
                    HandleProgressStateRestored;
            }
        }

        private void Start()
        {
            // This also covers cases where save restoration occurred
            // before this component reached Start.
            ApplyPersistentRespawnState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.StateRestored -=
                    HandleProgressStateRestored;
            }
        }

        /// <summary>
        /// Starts the player death sequence.
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
        /// Changes the active respawn point for the current runtime session.
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

        private void HandleProgressStateRestored()
        {
            ApplyPersistentRespawnState();
        }

        /// <summary>
        /// Reconstructs the active respawn location from persistent progress.
        ///
        /// Loading a save that has reached the enemy checkpoint also
        /// places the player directly at that checkpoint.
        /// </summary>
        private void ApplyPersistentRespawnState()
        {
            if (progress == null ||
                !progress.EnemyCheckpointReached)
            {
                currentRespawnPoint =
                    initialRespawnPoint;

                return;
            }

            if (enemyCheckpointPoint == null)
            {
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
        /// Immediately places the player at a world-space respawn point.
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

            if (progress == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires GameProgressController.",
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
    }
}