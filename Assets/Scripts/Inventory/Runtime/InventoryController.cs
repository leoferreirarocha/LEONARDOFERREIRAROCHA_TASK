using System;
using UnityEngine;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Provides the Unity-facing entry point for the player's
    /// inventory and equipment systems.
    ///
    /// This component owns the runtime pocket and equipment models
    /// and exposes inventory operations to gameplay, UI, and
    /// persistence systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryController : MonoBehaviour
    {
        [Header("Pocket Inventory")]

        [Tooltip("Number of available slots in the player's pocket inventory.")]
        [SerializeField, Min(1)]
        private int pocketSlotCount = 6;

        private InventoryModel pocket;
        private EquipmentModel equipment;

        /// <summary>
        /// Raised whenever the pocket inventory successfully changes.
        /// </summary>
        public event Action InventoryChanged;

        /// <summary>
        /// Raised whenever the active hand item changes.
        /// </summary>
        public event Action EquipmentChanged;

        /// <summary>
        /// Gets the runtime pocket inventory model.
        /// </summary>
        public InventoryModel Pocket => pocket;

        /// <summary>
        /// Gets the runtime equipment model.
        /// </summary>
        public EquipmentModel Equipment => equipment;

        /// <summary>
        /// Gets the item currently equipped in the player's hand.
        /// </summary>
        public ItemDefinition HandItem =>
            equipment != null
                ? equipment.HandItem
                : null;

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

            if (equipment != null)
            {
                equipment.Changed -= HandleEquipmentChanged;
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
        /// Determines whether the pocket contains the requested
        /// item quantity.
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
        ///
        /// This operation will later be used directly by
        /// inventory drag-and-drop interactions.
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

        /// <summary>
        /// Attempts to equip the item contained in a pocket slot.
        ///
        /// When the hand is empty, the pocket item is transferred
        /// directly into the hand.
        ///
        /// When another item is already equipped, the two items
        /// exchange positions.
        /// </summary>
        public bool TryEquipFromPocket(int pocketIndex)
        {
            if (!IsValidPocketIndex(pocketIndex))
            {
                return false;
            }

            InventorySlot sourceSlot =
                pocket.GetSlot(pocketIndex);

            if (sourceSlot.IsEmpty)
            {
                return false;
            }

            ItemDefinition candidate =
                sourceSlot.Item;

            if (!candidate.IsEquippable)
            {
                return false;
            }

            // Equippable definitions are intentionally limited
            // to one unit per slot.
            if (sourceSlot.Quantity != 1)
            {
                return false;
            }

            ItemDefinition previousHandItem =
                equipment.HandItem;

            if (previousHandItem == null)
            {
                sourceSlot.Clear();
            }
            else
            {
                // Swap the currently equipped item back into
                // the exact pocket slot used for the new item.
                sourceSlot.Set(
                    previousHandItem,
                    1
                );
            }

            equipment.SetHandItemWithoutNotification(
                candidate
            );

            // Both models are already in their final state before
            // listeners are informed about the transaction.
            pocket.NotifyChanged();
            equipment.NotifyChanged();

            return true;
        }

        /// <summary>
        /// Attempts to move the currently equipped hand item
        /// back into the first available pocket slot.
        /// </summary>
        public bool TryUnequipToPocket()
        {
            if (!equipment.HasHandItem)
            {
                return false;
            }

            int emptySlotIndex =
                pocket.FindFirstEmptySlotIndex();

            if (emptySlotIndex < 0)
            {
                return false;
            }

            ItemDefinition equippedItem =
                equipment.HandItem;

            InventorySlot targetSlot =
                pocket.GetSlot(emptySlotIndex);

            targetSlot.Set(
                equippedItem,
                1
            );

            equipment.ClearHandItemWithoutNotification();

            // Notify only after the complete transaction finishes.
            pocket.NotifyChanged();
            equipment.NotifyChanged();

            return true;
        }

        /// <summary>
        /// Determines whether the provided item is currently
        /// equipped in the player's hand.
        /// </summary>
        public bool IsEquipped(ItemDefinition item)
        {
            return equipment.IsEquipped(item);
        }

        private void InitializeInventory()
        {
            pocket = new InventoryModel(
                pocketSlotCount
            );

            equipment = new EquipmentModel();

            pocket.Changed += HandleInventoryChanged;
            equipment.Changed += HandleEquipmentChanged;
        }

        private bool IsValidPocketIndex(int index)
        {
            return index >= 0 &&
                   index < PocketSlotCount;
        }

        private void HandleInventoryChanged()
        {
            InventoryChanged?.Invoke();
        }

        private void HandleEquipmentChanged()
        {
            EquipmentChanged?.Invoke();
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