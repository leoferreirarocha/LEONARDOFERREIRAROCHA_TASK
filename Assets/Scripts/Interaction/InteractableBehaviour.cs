using UnityEngine;

namespace LeonardoTask.Interaction
{
    /// <summary>
    /// Base MonoBehaviour for reusable world interactions.
    ///
    /// Specific gameplay objects inherit from this class and implement
    /// only the behavior that should occur when the player interacts.
    /// </summary>
    public abstract class InteractableBehaviour :
        MonoBehaviour,
        IInteractable
    {
        [Header("Interaction")]

        [Tooltip(
            "Short action label displayed beside the interaction key."
        )]
        [SerializeField]
        private string interactionLabel = "Interact";

        /// <summary>
        /// Gets the action label displayed by the interaction prompt.
        /// </summary>
        public string InteractionLabel =>
            interactionLabel;

        /// <summary>
        /// Gets whether this interaction is currently available.
        /// </summary>
        public virtual bool CanInteract =>
            isActiveAndEnabled;

        /// <summary>
        /// Performs the interaction behavior.
        /// </summary>
        public abstract void Interact(
            PlayerInteractor2D interactor
        );
    }
}