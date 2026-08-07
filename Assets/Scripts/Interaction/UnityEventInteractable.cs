using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Interaction
{
    /// <summary>
    /// Generic interaction that invokes configurable UnityEvents.
    ///
    /// Useful for simple world interactions that do not require
    /// dedicated gameplay logic.
    /// </summary>
    public sealed class UnityEventInteractable :
        InteractableBehaviour
    {
        [Header("Behavior")]

        [SerializeField]
        private bool interactOnce;

        [SerializeField]
        private UnityEvent onInteract;

        private bool consumed;

        public override bool CanInteract =>
            base.CanInteract &&
            !consumed;

        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                return;
            }

            if (interactOnce)
            {
                consumed = true;
            }

            onInteract?.Invoke();

            interactor.RefreshInteraction();
        }
    }
}