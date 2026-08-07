using System;
using UnityEngine;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Provides the Unity-facing entry point for the player's inventory.
    ///
    /// This component owns the runtime InventoryModel and exposes
    /// inventory operations to gameplay, UI, and persistence systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryController : MonoBehaviour
    {
        [Header("Pocket Inventory")]

        [Tooltip("Number of available slots in the player's pocket inventory.")]
        [SerializeField, Min(1)]
        private int pocketSlotCount = 6;

        private InventoryModel pocket;

        /// <summary>
        /// Raised whenever the pocket inventory successfully changes.
        /// </summary>
        public event Action InventoryChanged;

        /// <summary>
        /// Gets the runtime pocket inventory model.
        /// </summary>
        public InventoryModel Pocket => pocket;

        /// <summary>
        /// Gets the number of available pocket slots.
        /// </summary>
        public int PocketSlotCount =>
            pocket != null
                ? pocket.SlotCount
                : pocketSlotCount;

        private void Awake()
        {
            InitializeInventory();
        }

        private void OnDestroy()
        {
            if (pocket != null)
            {
                pocket.Changed -= HandleInventoryChanged;
            }
        }

        /// <summary>
        /// Attempts to add an item to the pocket inventory.
        /// </summary>
        public bool TryAddItem(
            ItemDefinition item,
            int quantity = 1
        )
        {
            return pocket.TryAddItem(
                item,
                quantity
            );
        }

        /// <summary>
        /// Attempts to remove an item from the pocket inventory.
        /// </summary>
        public bool TryRemoveItem(
            ItemDefinition item,
            int quantity = 1
        )
        {
            return pocket.TryRemoveItem(
                item,
                quantity
            );
        }

        /// <summary>
        /// Determines whether the pocket contains the requested item quantity.
        /// </summary>
        public bool Contains(
            ItemDefinition item,
            int quantity = 1
        )
        {
            return pocket.Contains(
                item,
                quantity
            );
        }

        /// <summary>
        /// Attempts to move contents between pocket slots.
        /// </summary>
        public bool TryMoveItem(
            int sourceIndex,
            int targetIndex
        )
        {
            return pocket.TryMoveItem(
                sourceIndex,
                targetIndex
            );
        }

        /// <summary>
        /// Attempts to swap two pocket slots.
        /// </summary>
        public bool TrySwapItems(
            int firstIndex,
            int secondIndex
        )
        {
            return pocket.TrySwapItems(
                firstIndex,
                secondIndex
            );
        }

        /// <summary>
        /// Attempts to move or swap two pocket slots.
        /// Intended for future drag-and-drop interactions.
        /// </summary>
        public bool TryMoveOrSwap(
            int sourceIndex,
            int targetIndex
        )
        {
            return pocket.TryMoveOrSwap(
                sourceIndex,
                targetIndex
            );
        }

        /// <summary>
        /// Returns the runtime contents of a pocket slot.
        /// </summary>
        public InventorySlot GetPocketSlot(int index)
        {
            return pocket.GetSlot(index);
        }

        private void InitializeInventory()
        {
            pocket = new InventoryModel(
                pocketSlotCount
            );

            pocket.Changed += HandleInventoryChanged;
        }

        private void HandleInventoryChanged()
        {
            InventoryChanged?.Invoke();
        }

        private void OnValidate()
        {
            pocketSlotCount = Mathf.Max(
                1,
                pocketSlotCount
            );
        }
    }
}