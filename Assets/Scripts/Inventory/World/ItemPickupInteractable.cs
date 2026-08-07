using System.Collections;
using LeonardoTask.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Represents an inventory item that can be collected from the world
    /// through the generic interaction system.
    /// </summary>
    public sealed class ItemPickupInteractable :
        InteractableBehaviour
    {
        [Header("Inventory")]

        [SerializeField]
        private InventoryController inventory;

        [SerializeField]
        private ItemDefinition item;

        [SerializeField, Min(1)]
        private int quantity = 1;

        [Header("Ownership")]

        [Tooltip(
            "Prevents collecting another copy when this item is already owned."
        )]
        [SerializeField]
        private bool preventDuplicateOwnership = true;

        [Header("Events")]

        [SerializeField]
        private UnityEvent onCollected;

        public override bool CanInteract
        {
            get
            {
                if (!base.CanInteract ||
                    inventory == null ||
                    item == null)
                {
                    return false;
                }

                if (preventDuplicateOwnership &&
                    IsAlreadyOwned())
                {
                    return false;
                }

                return inventory.Pocket
                    .GetAvailableCapacity(item) >=
                    quantity;
            }
        }

        private IEnumerator Start()
        {
            // Wait until the next frame so persistent inventory data
            // has already been restored before checking ownership.
            yield return null;

            if (preventDuplicateOwnership &&
                IsAlreadyOwned())
            {
                gameObject.SetActive(
                    false
                );
            }
        }

        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                interactor.RefreshInteraction();
                return;
            }

            bool collected =
                inventory.TryAddItem(
                    item,
                    quantity
                );

            if (!collected)
            {
                return;
            }

            onCollected?.Invoke();

            interactor.UnregisterInteractable(
                this
            );

            gameObject.SetActive(
                false
            );
        }

        private bool IsAlreadyOwned()
        {
            return inventory.Contains(item) ||
                   inventory.IsEquipped(item);
        }
    }
}