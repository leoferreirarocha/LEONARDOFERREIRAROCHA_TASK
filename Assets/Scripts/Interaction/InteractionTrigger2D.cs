using System.Collections.Generic;
using UnityEngine;

namespace LeonardoTask.Interaction
{
    /// <summary>
    /// Detects when a player enters or exits an interactable object's
    /// trigger area.
    ///
    /// The trigger registers its parent interaction with PlayerInteractor2D,
    /// allowing proximity detection to remain completely event-driven.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractionTrigger2D : MonoBehaviour
    {
        [Header("Interaction")]

        [Tooltip(
            "Interaction controlled by this trigger area."
        )]
        [SerializeField]
        private InteractableBehaviour interactable;

        private readonly Dictionary<PlayerInteractor2D, int>
            playerOverlapCounts = new();

        private Collider2D triggerCollider;

        private void Awake()
        {
            triggerCollider =
                GetComponent<Collider2D>();

            if (triggerCollider == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionTrigger2D)} on '{name}' requires a Collider2D.",
                    this
                );

                enabled = false;
                return;
            }

            if (!triggerCollider.isTrigger)
            {
                Debug.LogError(
                    $"{nameof(InteractionTrigger2D)} on '{name}' requires Is Trigger to be enabled.",
                    this
                );

                enabled = false;
                return;
            }

            if (interactable == null)
            {
                interactable =
                    GetComponentInParent<InteractableBehaviour>();
            }

            if (interactable == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionTrigger2D)} on '{name}' could not find an InteractableBehaviour.",
                    this
                );

                enabled = false;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            PlayerInteractor2D playerInteractor =
                other.GetComponentInParent<PlayerInteractor2D>();

            if (playerInteractor == null)
            {
                return;
            }

            if (playerOverlapCounts.TryGetValue(
                    playerInteractor,
                    out int overlapCount
                ))
            {
                playerOverlapCounts[playerInteractor] =
                    overlapCount + 1;

                return;
            }

            playerOverlapCounts.Add(
                playerInteractor,
                1
            );

            playerInteractor.RegisterInteractable(
                interactable
            );
        }

        private void OnTriggerExit2D(
            Collider2D other
        )
        {
            PlayerInteractor2D playerInteractor =
                other.GetComponentInParent<PlayerInteractor2D>();

            if (playerInteractor == null ||
                !playerOverlapCounts.TryGetValue(
                    playerInteractor,
                    out int overlapCount
                ))
            {
                return;
            }

            overlapCount--;

            if (overlapCount > 0)
            {
                playerOverlapCounts[playerInteractor] =
                    overlapCount;

                return;
            }

            playerOverlapCounts.Remove(
                playerInteractor
            );

            playerInteractor.UnregisterInteractable(
                interactable
            );
        }

        private void OnDisable()
        {
            foreach (
                PlayerInteractor2D playerInteractor
                in playerOverlapCounts.Keys
            )
            {
                if (playerInteractor != null)
                {
                    playerInteractor.UnregisterInteractable(
                        interactable
                    );
                }
            }

            playerOverlapCounts.Clear();
        }

        private void Reset()
        {
            interactable =
                GetComponentInParent<InteractableBehaviour>();

            Collider2D collider =
                GetComponent<Collider2D>();

            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            if (interactable == null)
            {
                interactable =
                    GetComponentInParent<InteractableBehaviour>();
            }
        }
    }
}