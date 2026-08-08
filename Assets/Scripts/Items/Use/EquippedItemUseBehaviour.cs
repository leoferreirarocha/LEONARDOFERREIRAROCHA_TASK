using LeonardoTask.Inventory;
using UnityEngine;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Base component for gameplay behavior associated with an item
    /// while that item is equipped in the player's Hand slot.
    ///
    /// Behaviors may implement either momentary use, such as firing
    /// a projectile, or continuous use that ends when input is released.
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
        /// Begins using the equipped item.
        ///
        /// Returns true when the input was successfully handled.
        /// </summary>
        public abstract bool BeginUse();

        /// <summary>
        /// Ends the current use operation.
        ///
        /// Momentary item behaviors do not need to override this method.
        /// </summary>
        public virtual void EndUse()
        {
        }
    }
}