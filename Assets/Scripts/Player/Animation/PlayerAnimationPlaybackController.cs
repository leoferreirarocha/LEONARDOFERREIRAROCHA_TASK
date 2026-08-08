using UnityEngine;

namespace LeonardoTask.Player
{
    /// <summary>
    /// Controls the playback of the player's existing movement animation.
    ///
    /// The animation plays while the player provides movement-related
    /// input and pauses immediately while the player is idle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationPlaybackController :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private PlayerInputReader input;

        [SerializeField]
        private PlayerMovement2D movement;

        [SerializeField]
        private Animator animator;

        [Header("Input")]

        [SerializeField, Min(0f)]
        private float horizontalInputThreshold = 0.01f;

        private void Awake()
        {
            if (input == null)
            {
                input =
                    GetComponent<PlayerInputReader>();
            }

            if (movement == null)
            {
                movement =
                    GetComponent<PlayerMovement2D>();
            }

            if (animator == null)
            {
                animator =
                    GetComponentInChildren<Animator>();
            }

            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            // The player starts visually idle.
            animator.speed = 0f;
        }

        private void Update()
        {
            bool shouldPlayAnimation =
                ShouldPlayAnimation();

            animator.speed =
                shouldPlayAnimation
                    ? 1f
                    : 0f;
        }

        /// <summary>
        /// Returns whether the existing movement animation should advance.
        ///
        /// Horizontal movement represents A/D input, while JumpHeld and
        /// RunHeld represent the Space and Shift actions respectively.
        /// </summary>
        private bool ShouldPlayAnimation()
        {
            if (!movement.MovementEnabled)
            {
                return false;
            }

            bool hasHorizontalInput =
                Mathf.Abs(
                    input.Move.x
                ) >
                horizontalInputThreshold;

            return hasHorizontalInput ||
                   input.JumpHeld ||
                   input.RunHeld;
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerAnimationPlaybackController)} on '{name}' requires PlayerInputReader.",
                    this
                );

                valid = false;
            }

            if (movement == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerAnimationPlaybackController)} on '{name}' requires PlayerMovement2D.",
                    this
                );

                valid = false;
            }

            if (animator == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerAnimationPlaybackController)} on '{name}' requires an Animator reference.",
                    this
                );

                valid = false;
            }

            return valid;
        }
    }
}