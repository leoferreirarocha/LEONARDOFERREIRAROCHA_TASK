using LeonardoTask.Interaction;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Represents the one-time lever that permanently unlocks
    /// the Frog Shop shortcut and the next progression step.
    ///
    /// The lever uses the existing world interaction system and
    /// synchronizes its visual state with persistent game progress.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrogShopLeverInteractable :
        InteractableBehaviour
    {
        [Header("Progress")]

        [Tooltip(
            "Persistent progression state modified by this lever."
        )]
        [SerializeField]
        private GameProgressController progress;

        [Header("Visual State")]

        [Tooltip(
            "Visual displayed before the lever has been activated."
        )]
        [SerializeField]
        private GameObject leverUpVisual;

        [Tooltip(
            "Visual displayed after the lever has been activated."
        )]
        [SerializeField]
        private GameObject leverDownVisual;

        /// <summary>
        /// Gets whether the lever can still be activated.
        ///
        /// The persistent shortcut state guarantees that this interaction
        /// can succeed only once.
        /// </summary>
        public override bool CanInteract =>
            base.CanInteract &&
            progress != null &&
            !progress.ShortcutUnlocked;

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed +=
                    RefreshVisualState;
            }
        }

        private void Start()
        {
            RefreshVisualState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.Changed -=
                    RefreshVisualState;
            }
        }

        /// <summary>
        /// Activates the lever and permanently unlocks the Frog Shop shortcut.
        /// </summary>
        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                interactor?.RefreshInteraction();
                return;
            }

            progress.ActivateFrogShopLever();

            RefreshVisualState();

            // Re-evaluate the interaction immediately so the prompt
            // disappears after this one-time interaction is consumed.
            interactor?.RefreshInteraction();
        }

        /// <summary>
        /// Synchronizes both lever visuals with persistent progression.
        /// </summary>
        private void RefreshVisualState()
        {
            if (progress == null)
            {
                return;
            }

            bool leverActivated =
                progress.ShortcutUnlocked;

            if (leverUpVisual != null)
            {
                leverUpVisual.SetActive(
                    !leverActivated
                );
            }

            if (leverDownVisual != null)
            {
                leverDownVisual.SetActive(
                    leverActivated
                );
            }
        }
    }
}