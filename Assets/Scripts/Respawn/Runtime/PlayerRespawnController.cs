using System.Collections;
using LeonardoTask.CameraSystem;
using LeonardoTask.Interaction;
using LeonardoTask.Player;
using UnityEngine;

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

        [SerializeField]
        private Transform respawnPoint;

        [SerializeField]
        private PlayerCameraFollow2D cameraFollow;

        [Header("Screen Fade")]

        [SerializeField]
        private ScreenFadeUI screenFade;

        [Header("Death Presentation")]

        [Tooltip(
            "Additional local Z rotation applied to the visual while dead."
        )]
        [SerializeField]
        private float deathVisualRotationZ = 90f;

        [Tooltip(
            "Time required for the screen to become fully black."
        )]
        [SerializeField, Min(0f)]
        private float fadeToBlackDuration = 0.35f;

        [Tooltip(
            "Time spent on a fully black screen before fading back."
        )]
        [SerializeField, Min(0f)]
        private float blackScreenHoldDuration = 0.15f;

        [Tooltip(
            "Time required for the screen to return from black."
        )]
        [SerializeField, Min(0f)]
        private float fadeFromBlackDuration = 0.35f;

        private Rigidbody2D body;

        private Quaternion initialVisualLocalRotation;

        private bool isDead;

        /// <summary>
        /// Gets whether a death and respawn sequence is currently active.
        /// </summary>
        public bool IsDead =>
            isDead;

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
                yield return new WaitForSecondsRealtime(
                    blackScreenHoldDuration
                );
            }

            RespawnAtHome();

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
        /// Prevents movement and world interaction while the
        /// death sequence is active.
        /// </summary>
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

        /// <summary>
        /// Applies the lightweight visual death pose without rotating
        /// the Rigidbody2D or player collision geometry.
        /// </summary>
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

        /// <summary>
        /// Restores the player to the configured home spawn while the
        /// screen is fully black.
        /// </summary>
        private void RespawnAtHome()
        {
            interactor.ClearNearbyInteractables();

            body.linearVelocity =
                Vector2.zero;

            body.angularVelocity =
                0f;

            body.position =
                respawnPoint.position;

            visualRoot.localRotation =
                initialVisualLocalRotation;

            cameraFollow.SnapToTarget();
        }

        /// <summary>
        /// Restores player control after the fade has completely cleared.
        /// </summary>
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
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerMovement2D reference.",
                    this
                );

                valid = false;
            }

            if (interactor == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerInteractor2D reference.",
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

            if (respawnPoint == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a respawn point.",
                    this
                );

                valid = false;
            }

            if (cameraFollow == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a PlayerCameraFollow2D reference.",
                    this
                );

                valid = false;
            }

            if (screenFade == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerRespawnController)} on '{name}' requires a ScreenFadeUI reference.",
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