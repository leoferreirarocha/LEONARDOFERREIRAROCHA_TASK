using LeonardoTask.Interaction;
using LeonardoTask.Inventory;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Controls the persistent Castle Door interaction.
    ///
    /// The door can only be unlocked while the player owns the
    /// Castle Key. Unlocking consumes the key and permanently records
    /// the open state in game progression.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CastleDoorInteractable :
        InteractableBehaviour
    {
        [Header("Runtime References")]

        [SerializeField]
        private GameProgressController progress;

        [SerializeField]
        private InventoryController inventory;

        [Header("Required Item")]

        [SerializeField]
        private ItemDefinition castleKeyItem;

        [Header("World Objects")]

        [Tooltip(
            "Visual and/or collider object representing the closed door."
        )]
        [SerializeField]
        private GameObject closedDoorObject;

        [Tooltip(
            "Trigger area used to interact with the closed door."
        )]
        [SerializeField]
        private GameObject interactionZoneObject;

        /// <summary>
        /// Gets whether the player can currently unlock the Castle Door.
        /// </summary>
        public override bool CanInteract =>
            base.CanInteract &&
            progress != null &&
            inventory != null &&
            castleKeyItem != null &&
            !progress.CastleDoorOpened &&
            OwnsCastleKey();

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed +=
                    ApplyPersistentState;
            }
        }

        private void Start()
        {
            ApplyPersistentState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.Changed -=
                    ApplyPersistentState;
            }
        }

        /// <summary>
        /// Consumes the Castle Key and permanently opens the door.
        /// </summary>
        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                return;
            }

            bool keyRemoved =
                inventory.TryRemoveItem(
                    castleKeyItem,
                    1
                );

            if (!keyRemoved)
            {
                return;
            }

            progress.MarkCastleDoorOpened();

            ApplyPersistentState();

            interactor?.RefreshInteraction();
        }

        private bool OwnsCastleKey()
        {
            return inventory.Contains(
                castleKeyItem
            ) ||
            inventory.IsEquipped(
                castleKeyItem
            );
        }

        /// <summary>
        /// Reconstructs the Castle Door from persistent progression.
        /// </summary>
        private void ApplyPersistentState()
        {
            if (progress == null)
            {
                return;
            }

            bool doorOpened =
                progress.CastleDoorOpened;

            if (closedDoorObject != null)
            {
                closedDoorObject.SetActive(
                    !doorOpened
                );
            }

            if (interactionZoneObject != null)
            {
                interactionZoneObject.SetActive(
                    !doorOpened
                );
            }
        }
    }
}