using System;

namespace LeonardoTask.SaveSystem
{
    /// <summary>
    /// Represents the persistent state of a single pocket inventory slot.
    ///
    /// Item references are stored as stable string identifiers rather than
    /// ScriptableObject references so the data can be serialized to JSON.
    /// </summary>
    [Serializable]
    public sealed class InventorySlotSaveData
    {
        /// <summary>
        /// Stable identifier of the item stored in this slot.
        /// An empty string represents an empty inventory slot.
        /// </summary>
        public string itemId = string.Empty;

        /// <summary>
        /// Quantity stored in this slot.
        /// Empty slots always use zero.
        /// </summary>
        public int quantity;

        /// <summary>
        /// Creates an empty slot save entry.
        /// </summary>
        public InventorySlotSaveData()
        {
        }

        /// <summary>
        /// Creates a slot save entry with the provided item state.
        /// </summary>
        public InventorySlotSaveData(
            string itemId,
            int quantity
        )
        {
            this.itemId = itemId ?? string.Empty;
            this.quantity = quantity;
        }
    }
}