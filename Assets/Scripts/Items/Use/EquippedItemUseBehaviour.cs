using LeonardoTask.Inventory;
using UnityEngine;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Base component for gameplay behavior associated with an item
    /// while that item is equipped in the player's Hand slot.
    ///
    /// Specific item behaviors only need to define what happens when
    /// their corresponding item is used.
    /// </summary>
    public abstract class EquippedItemUseBehaviour : MonoBehaviour
    {
        [Header("Item")]

        [Tooltip(
            "ItemDefinition associated with this equipped-item behavior."
        )]
        [SerializeField]
        private ItemDefinition item;

        /// <summary>
        /// Gets the item associated with this use behavior.
        /// </summary>
        public ItemDefinition Item =>
            item;

        /// <summary>
        /// Performs the gameplay behavior associated with this item.
        ///
        /// Returns true when the input was successfully handled.
        /// </summary>
        public abstract bool Use();
    }
}