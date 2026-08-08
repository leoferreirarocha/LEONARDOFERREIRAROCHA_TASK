using System.Collections;
using LeonardoTask.CameraSystem;
using LeonardoTask.Interaction;
using LeonardoTask.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Coordinates player death and local respawning without reloading
    /// the current scene.
    ///
    /// Inventory and persistent world progression remain untouched
    /// because only the player's physical state and position are reset.
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

        [Tooltip(
            "Visual child rotated during the death presentation. " +
            "Do not assign the Player root."
        )]
        [SerializeField]
        private Transform visualRoot;

        [Header("Respawn")]

        [FormerlySerializedAs("respawnPoint")]
        [SerializeField]
        private Transform initialRespawnPoint;

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
                enabled =
                    false;
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
        /// Changes the position used by future deaths in the current session.
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
            isDead =
                true;

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

            isDead =
                false;
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
            interactor.ClearNearbyInteractables();

            body.linearVelocity =
                Vector2.zero;

            body.angularVelocity =
                0f;

            Transform targetPoint =
                currentRespawnPoint != null
                    ? currentRespawnPoint
                    : initialRespawnPoint;

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
            bool valid =
                true;

            if (movement == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerMovement2D reference.",
                    this
                );

                valid =
                    false;
            }

            if (interactor == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerInteractor2D reference.",
                    this
                );

                valid =
                    false;
            }

            if (visualRoot == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a visual root.",
                    this
                );

                valid =
                    false;
            }

            if (initialRespawnPoint == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires an initial respawn point.",
                    this
                );

                valid =
                    false;
            }

            if (cameraFollow == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerCameraFollow2D reference.",
                    this
                );

                valid =
                    false;
            }

            if (screenFade == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a ScreenFadeUI reference.",
                    this
                );

                valid =
                    false;
            }

            return valid;
        }
    }
}