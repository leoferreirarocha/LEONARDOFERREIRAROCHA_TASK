using System;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Owns the runtime state of the player's active hand item.
    ///
    /// The equipment model intentionally has no dependency on UI,
    /// input, GameObjects, or scene-specific gameplay systems.
    /// </summary>
    public sealed class EquipmentModel
    {
        private ItemDefinition handItem;

        /// <summary>
        /// Raised whenever the equipped hand item changes.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Gets the item currently equipped in the player's hand.
        /// </summary>
        public ItemDefinition HandItem => handItem;

        /// <summary>
        /// Gets whether the player currently has an active hand item.
        /// </summary>
        public bool HasHandItem => handItem != null;

        /// <summary>
        /// Determines whether the provided item is currently equipped.
        /// </summary>
        public bool IsEquipped(ItemDefinition item)
        {
            return item != null &&
                   handItem == item;
        }

        /// <summary>
        /// Replaces the current hand item without notifying listeners.
        ///
        /// InventoryController uses this method to perform atomic
        /// pocket-to-hand transactions before broadcasting changes.
        /// </summary>
        internal void SetHandItemWithoutNotification(
            ItemDefinition item
        )
        {
            handItem = item;
        }

        /// <summary>
        /// Clears the current hand item without notifying listeners.
        /// </summary>
        internal void ClearHandItemWithoutNotification()
        {
            handItem = null;
        }

        /// <summary>
        /// Notifies listeners that the equipment state has changed.
        /// </summary>
        internal void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}