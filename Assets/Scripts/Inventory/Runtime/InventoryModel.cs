using System;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Owns the runtime state and business rules of the player's
    /// slot-based pocket inventory.
    ///
    /// This model intentionally has no dependency on UI, input,
    /// GameObjects, or scene objects.
    /// </summary>
    public sealed class InventoryModel
    {
        private readonly InventorySlot[] slots;

        /// <summary>
        /// Raised whenever the inventory contents change successfully.
        /// UI and persistence systems can subscribe to this event.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Gets the number of available pocket slots.
        /// </summary>
        public int SlotCount => slots.Length;

        /// <summary>
        /// Creates an empty inventory with the requested capacity.
        /// </summary>
        public InventoryModel(int slotCount)
        {
            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotCount),
                    "Inventory slot count must be greater than zero."
                );
            }

            slots = new InventorySlot[slotCount];

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// Returns the slot located at the requested index.
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            ValidateSlotIndex(index);

            return slots[index];
        }

        /// <summary>
        /// Determines whether the inventory contains at least
        /// the requested quantity of an item.
        /// </summary>
        public bool Contains(
            ItemDefinition item,
            int quantity = 1
        )
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            return CountItem(item) >= quantity;
        }

        /// <summary>
        /// Counts the total quantity of an item across every pocket slot.
        /// </summary>
        public int CountItem(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int total = 0;

            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty &&
                    slot.Item == item)
                {
                    total += slot.Quantity;
                }
            }

            return total;
        }

        /// <summary>
        /// Attempts to add the complete requested quantity.
        ///
        /// The operation fails without modifying the inventory when
        /// there is not enough available space.
        /// </summary>
        public bool TryAddItem(
            ItemDefinition item,
            int quantity = 1
        )
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            if (GetAvailableCapacity(item) < quantity)
            {
                return false;
            }

            int remaining = quantity;

            // Fill existing stacks before consuming empty slots.
            foreach (InventorySlot slot in slots)
            {
                if (!slot.CanStack(item))
                {
                    continue;
                }

                remaining = slot.Add(remaining);

                if (remaining <= 0)
                {
                    Changed?.Invoke();
                    return true;
                }
            }

            // Create new stacks only after existing ones are full.
            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    continue;
                }

                int amountToPlace = Math.Min(
                    remaining,
                    item.MaximumStackSize
                );

                slot.Set(
                    item,
                    amountToPlace
                );

                remaining -= amountToPlace;

                if (remaining <= 0)
                {
                    Changed?.Invoke();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove the requested quantity of an item.
        ///
        /// The operation fails without modifying the inventory when
        /// the requested quantity is not fully available.
        /// </summary>
        public bool TryRemoveItem(
            ItemDefinition item,
            int quantity = 1
        )
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            if (!Contains(item, quantity))
            {
                return false;
            }

            int remaining = quantity;

            for (int i = slots.Length - 1; i >= 0; i--)
            {
                InventorySlot slot = slots[i];

                if (slot.IsEmpty ||
                    slot.Item != item)
                {
                    continue;
                }

                int removed = slot.Remove(remaining);

                remaining -= removed;

                if (remaining <= 0)
                {
                    Changed?.Invoke();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves a slot into an empty target slot, or merges matching
        /// stackable items when space is available.
        ///
        /// Occupied slots containing different items are not swapped
        /// by this operation.
        /// </summary>
        public bool TryMoveItem(
            int sourceIndex,
            int targetIndex
        )
        {
            if (!AreDistinctValidIndices(
                    sourceIndex,
                    targetIndex
                ))
            {
                return false;
            }

            InventorySlot source = slots[sourceIndex];
            InventorySlot target = slots[targetIndex];

            if (source.IsEmpty)
            {
                return false;
            }

            if (target.IsEmpty)
            {
                target.Set(
                    source.Item,
                    source.Quantity
                );

                source.Clear();

                Changed?.Invoke();
                return true;
            }

            if (target.Item != source.Item)
            {
                return false;
            }

            int targetCapacity =
                target.GetRemainingCapacity(source.Item);

            if (targetCapacity <= 0)
            {
                return false;
            }

            int amountToMove = Math.Min(
                source.Quantity,
                targetCapacity
            );

            int remainder =
                target.Add(amountToMove);

            int successfullyMoved =
                amountToMove -
                remainder;

            source.Remove(successfullyMoved);

            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Swaps the complete contents of two inventory slots.
        /// </summary>
        public bool TrySwapItems(
            int firstIndex,
            int secondIndex
        )
        {
            if (!AreDistinctValidIndices(
                    firstIndex,
                    secondIndex
                ))
            {
                return false;
            }

            InventorySlot first = slots[firstIndex];
            InventorySlot second = slots[secondIndex];

            ItemDefinition firstItem = first.Item;
            int firstQuantity = first.Quantity;

            ItemDefinition secondItem = second.Item;
            int secondQuantity = second.Quantity;

            first.Set(
                secondItem,
                secondQuantity
            );

            second.Set(
                firstItem,
                firstQuantity
            );

            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Moves an item when possible and otherwise swaps the two slots.
        ///
        /// This method is intended to support future drag-and-drop behavior.
        /// </summary>
        public bool TryMoveOrSwap(
            int sourceIndex,
            int targetIndex
        )
        {
            if (!AreDistinctValidIndices(
                    sourceIndex,
                    targetIndex
                ))
            {
                return false;
            }

            InventorySlot source = slots[sourceIndex];
            InventorySlot target = slots[targetIndex];

            if (source.IsEmpty)
            {
                return false;
            }

            if (target.IsEmpty ||
                target.Item == source.Item)
            {
                return TryMoveItem(
                    sourceIndex,
                    targetIndex
                );
            }

            return TrySwapItems(
                sourceIndex,
                targetIndex
            );
        }

        /// <summary>
        /// Calculates how many additional copies of an item
        /// can currently fit in the inventory.
        /// </summary>
        public int GetAvailableCapacity(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int capacity = 0;

            foreach (InventorySlot slot in slots)
            {
                capacity += slot.GetRemainingCapacity(item);
            }

            return capacity;
        }

        /// <summary>
        /// Returns the index of the first empty pocket slot,
        /// or -1 when the inventory has no available empty slots.
        /// </summary>
        public int FindFirstEmptySlotIndex()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Clears every pocket slot without broadcasting a state change.
        ///
        /// This is used while restoring a complete saved state so
        /// listeners never observe a partially loaded inventory.
        /// </summary>
        internal void ClearWithoutNotification()
        {
            foreach (InventorySlot slot in slots)
            {
                slot.Clear();
            }
        }

        /// <summary>
        /// Restores a specific pocket slot without broadcasting
        /// an individual state change.
        /// </summary>
        internal void SetSlotWithoutNotification(
            int index,
            ItemDefinition item,
            int quantity
        )
        {
            ValidateSlotIndex(index);

            slots[index].Set(
                item,
                quantity
            );
        }

        /// <summary>
        /// Manually broadcasts that the inventory state has changed.
        ///
        /// Higher-level systems use this after completing an atomic
        /// transaction or restoring a complete saved state.
        /// </summary>
        internal void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private bool AreDistinctValidIndices(
            int firstIndex,
            int secondIndex
        )
        {
            return firstIndex != secondIndex &&
                   IsValidIndex(firstIndex) &&
                   IsValidIndex(secondIndex);
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 &&
                   index < slots.Length;
        }

        private void ValidateSlotIndex(int index)
        {
            if (!IsValidIndex(index))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Inventory slot index must be between 0 and {slots.Length - 1}."
                );
            }
        }
    }
}