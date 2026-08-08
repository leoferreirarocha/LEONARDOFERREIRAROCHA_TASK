using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Handles the gameplay behavior of using the Trumpet
    /// while it is equipped in the player's Hand.
    ///
    /// The audio clip is optional during development. Once assigned,
    /// every successful use plays the configured one-shot sound.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class TrumpetItemUseBehaviour :
        EquippedItemUseBehaviour
    {
        [Header("Audio")]

        [SerializeField]
        private AudioSource audioSource;

        [Tooltip(
            "Sound played whenever the equipped Trumpet is used. " +
            "This field may remain empty until the final audio clip is available."
        )]
        [SerializeField]
        private AudioClip trumpetClip;

        [SerializeField, Range(0f, 1f)]
        private float volumeScale = 1f;

        [Header("Events")]

        [Tooltip(
            "Optional event invoked whenever the Trumpet is successfully used."
        )]
        [SerializeField]
        private UnityEvent onUsed;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource =
                    GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Plays the Trumpet sound when one has been assigned and
        /// invokes optional gameplay or presentation events.
        ///
        /// The use still succeeds when no AudioClip is assigned so
        /// gameplay can be developed before final audio is available.
        /// </summary>
        public override bool Use()
        {
            if (audioSource == null)
            {
                return false;
            }

            if (trumpetClip != null)
            {
                audioSource.PlayOneShot(
                    trumpetClip,
                    volumeScale
                );
            }

            onUsed?.Invoke();

            return true;
        }

        private void Reset()
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }
}