using System;
using UnityEngine;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Represents the runtime contents of a single inventory slot.
    ///
    /// A slot contains an item definition and its current quantity.
    /// Empty slots contain no item and always report a quantity of zero.
    /// </summary>
    [Serializable]
    public sealed class InventorySlot
    {
        [SerializeField]
        private ItemDefinition item;

        [SerializeField, Min(0)]
        private int quantity;

        /// <summary>
        /// Gets the item currently stored in this slot.
        /// </summary>
        public ItemDefinition Item => item;

        /// <summary>
        /// Gets the current item quantity.
        /// Empty slots always return zero.
        /// </summary>
        public int Quantity =>
            IsEmpty
                ? 0
                : quantity;

        /// <summary>
        /// Gets whether this slot currently contains no valid item.
        /// </summary>
        public bool IsEmpty =>
            item == null ||
            quantity <= 0;

        /// <summary>
        /// Gets whether the provided item can be stacked into this slot.
        /// </summary>
        public bool CanStack(ItemDefinition candidate)
        {
            if (candidate == null || IsEmpty)
            {
                return false;
            }

            return item == candidate &&
                   quantity < item.MaximumStackSize;
        }

        /// <summary>
        /// Gets the amount of additional items that can fit in this slot.
        /// </summary>
        public int GetRemainingCapacity(ItemDefinition candidate)
        {
            if (candidate == null)
            {
                return 0;
            }

            if (IsEmpty)
            {
                return candidate.MaximumStackSize;
            }

            if (item != candidate)
            {
                return 0;
            }

            return Mathf.Max(
                0,
                item.MaximumStackSize - quantity
            );
        }

        /// <summary>
        /// Replaces the current slot contents.
        /// </summary>
        internal void Set(
            ItemDefinition newItem,
            int newQuantity
        )
        {
            if (newItem == null || newQuantity <= 0)
            {
                Clear();
                return;
            }

            item = newItem;

            quantity = Mathf.Clamp(
                newQuantity,
                1,
                newItem.MaximumStackSize
            );
        }

        /// <summary>
        /// Adds as much of the requested quantity as possible
        /// and returns the amount that could not be added.
        /// </summary>
        internal int Add(int amount)
        {
            if (IsEmpty || amount <= 0)
            {
                return amount;
            }

            int availableSpace =
                item.MaximumStackSize -
                quantity;

            int amountToAdd = Mathf.Min(
                amount,
                availableSpace
            );

            quantity += amountToAdd;

            return amount - amountToAdd;
        }

        /// <summary>
        /// Removes up to the requested quantity and returns
        /// the number of items that were actually removed.
        /// </summary>
        internal int Remove(int amount)
        {
            if (IsEmpty || amount <= 0)
            {
                return 0;
            }

            int removedAmount = Mathf.Min(
                amount,
                quantity
            );

            quantity -= removedAmount;

            if (quantity <= 0)
            {
                Clear();
            }

            return removedAmount;
        }

        /// <summary>
        /// Removes all contents from this slot.
        /// </summary>
        internal void Clear()
        {
            item = null;
            quantity = 0;
        }
    }
}