using System.Collections.Generic;
using LeonardoTask.Interaction.UI;
using LeonardoTask.Player;
using UnityEngine;

namespace LeonardoTask.Interaction
{
    /// <summary>
    /// Coordinates player interaction with nearby world objects.
    ///
    /// Nearby interactions are registered by trigger events rather than
    /// discovered through repeated physics queries.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor2D : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private PlayerInputReader input;

        [SerializeField]
        private InteractionPromptUI promptUI;

        private readonly List<InteractableBehaviour>
            nearbyInteractables = new();

        private InteractableBehaviour currentInteractable;

        private bool interactionEnabled = true;

        private bool inputCallbackRegistered;

        private void OnEnable()
        {
            RegisterInputCallback();
        }

        private void OnDisable()
        {
            UnregisterInputCallback();

            promptUI?.Hide();
        }

        /// <summary>
        /// Registers an interaction that has entered the player's range.
        /// </summary>
        public void RegisterInteractable(
            InteractableBehaviour interactable
        )
        {
            if (interactable == null ||
                nearbyInteractables.Contains(interactable))
            {
                return;
            }

            nearbyInteractables.Add(
                interactable
            );

            RefreshInteraction();
        }

        /// <summary>
        /// Removes an interaction that is no longer inside the player's range.
        /// </summary>
        public void UnregisterInteractable(
            InteractableBehaviour interactable
        )
        {
            if (interactable == null)
            {
                return;
            }

            nearbyInteractables.Remove(
                interactable
            );

            RefreshInteraction();
        }
        /// <summary>
        /// Clears every currently registered world interaction.
        ///
        /// Teleporting systems use this to prevent stale proximity references
        /// after moving the player instantly to another world position.
        /// </summary>
        public void ClearNearbyInteractables()
        {
            nearbyInteractables.Clear();

            currentInteractable =
                null;

            promptUI?.Hide();
        }

        /// <summary>
        /// Enables or disables world interaction.
        ///
        /// Disabling interaction also releases ownership of the Interact input,
        /// allowing modal systems such as dialogue to use the same action safely.
        /// </summary>
        public void SetInteractionEnabled(
            bool enabled
        )
        {
            if (interactionEnabled == enabled)
            {
                return;
            }

            interactionEnabled =
                enabled;

            if (!interactionEnabled)
            {
                UnregisterInputCallback();

                currentInteractable = null;

                promptUI?.Hide();

                return;
            }

            RegisterInputCallback();

            RefreshInteraction();
        }

        /// <summary>
        /// Re-evaluates the best currently available interaction.
        /// </summary>
        public void RefreshInteraction()
        {
            RemoveInvalidInteractables();

            if (!interactionEnabled)
            {
                currentInteractable = null;

                promptUI?.Hide();

                return;
            }

            currentInteractable =
                FindNearestInteractable();

            if (currentInteractable == null)
            {
                promptUI?.Hide();

                return;
            }

            promptUI?.Show(
                currentInteractable.InteractionLabel
            );
        }

        private void HandleInteractPressed()
        {
            if (!interactionEnabled)
            {
                return;
            }

            // Re-evaluate only when interaction is requested.
            // No continuous physics query is required.
            RefreshInteraction();

            if (currentInteractable == null ||
                !currentInteractable.CanInteract)
            {
                return;
            }

            InteractableBehaviour interactable =
                currentInteractable;

            interactable.Interact(
                this
            );

            RefreshInteraction();
        }

        private InteractableBehaviour
            FindNearestInteractable()
        {
            InteractableBehaviour nearest =
                null;

            float nearestDistanceSquared =
                float.PositiveInfinity;

            foreach (
                InteractableBehaviour interactable
                in nearbyInteractables
            )
            {
                if (interactable == null ||
                    !interactable.CanInteract)
                {
                    continue;
                }

                float distanceSquared =
                    (
                        interactable.transform.position -
                        transform.position
                    ).sqrMagnitude;

                if (distanceSquared >=
                    nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared =
                    distanceSquared;

                nearest =
                    interactable;
            }

            return nearest;
        }

        private void RemoveInvalidInteractables()
        {
            for (
                int i = nearbyInteractables.Count - 1;
                i >= 0;
                i--
            )
            {
                if (nearbyInteractables[i] == null)
                {
                    nearbyInteractables.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Registers the interaction input callback when world interaction
        /// is currently enabled.
        /// </summary>
        private void RegisterInputCallback()
        {
            if (input == null ||
                inputCallbackRegistered ||
                !interactionEnabled)
            {
                return;
            }

            input.InteractPressed +=
                HandleInteractPressed;

            inputCallbackRegistered = true;
        }

        /// <summary>
        /// Removes the world interaction input callback.
        /// </summary>
        private void UnregisterInputCallback()
        {
            if (input == null ||
                !inputCallbackRegistered)
            {
                return;
            }

            input.InteractPressed -=
                HandleInteractPressed;

            inputCallbackRegistered = false;
        }
    }
}