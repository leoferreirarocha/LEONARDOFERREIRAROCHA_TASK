using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Handles continuous Trumpet playback while the item is equipped.
    ///
    /// Pressing the use input starts the Trumpet from the beginning.
    /// Releasing the input stops playback immediately.
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
            "Sound played while the equipped Trumpet is being used."
        )]
        [SerializeField]
        private AudioClip trumpetClip;

        [Tooltip(
            "Repeats the Trumpet clip for as long as the use input remains held."
        )]
        [SerializeField]
        private bool loopWhileHeld = true;

        [SerializeField, Range(0f, 1f)]
        private float volume = 1f;

        [Header("Events")]

        [Tooltip(
            "Optional event invoked whenever a new Trumpet use begins."
        )]
        [SerializeField]
        private UnityEvent onUseStarted;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource =
                    GetComponent<AudioSource>();
            }

            ConfigureAudioSource();
        }

        /// <summary>
        /// Restarts Trumpet playback from the beginning.
        ///
        /// Gameplay use still succeeds when no AudioClip has been assigned
        /// so progression can remain independent from presentation assets.
        /// </summary>
        public override bool BeginUse()
        {
            if (audioSource == null)
            {
                return false;
            }

            // Stop first so repeated uses always begin from the start
            // instead of continuing or overlapping previous playback.
            audioSource.Stop();

            if (trumpetClip != null)
            {
                audioSource.clip =
                    trumpetClip;

                audioSource.loop =
                    loopWhileHeld;

                audioSource.volume =
                    volume;

                audioSource.Play();
            }

            onUseStarted?.Invoke();

            return true;
        }

        /// <summary>
        /// Stops Trumpet playback immediately when use input is released.
        /// </summary>
        public override void EndUse()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
        }

        private void OnDisable()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void ConfigureAudioSource()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake =
                false;

            audioSource.loop =
                loopWhileHeld;

            audioSource.volume =
                volume;
        }

        private void Reset()
        {
            audioSource =
                GetComponent<AudioSource>();

            ConfigureAudioSource();
        }

        private void OnValidate()
        {
            volume =
                Mathf.Clamp01(
                    volume
                );

            if (audioSource != null)
            {
                ConfigureAudioSource();
            }
        }
    }
}