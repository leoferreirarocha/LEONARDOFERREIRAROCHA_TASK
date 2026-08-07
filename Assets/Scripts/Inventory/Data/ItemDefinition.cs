using UnityEngine;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Defines immutable authoring data shared by every instance
    /// of a specific inventory item.
    ///
    /// Item definitions are stored as ScriptableObject assets and should
    /// not contain runtime state such as ownership, slot position, or quantity.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewItemDefinition",
        menuName = "Leonardo Task/Inventory/Item Definition",
        order = 0
    )]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]

        [Tooltip(
            "Stable identifier used by runtime systems and save data. " +
            "Changing this value after creating save files can invalidate references."
        )]
        [SerializeField]
        private string id;

        [Tooltip("Player-facing name displayed by the inventory UI.")]
        [SerializeField]
        private string displayName;

        [Tooltip("Player-facing description displayed in item details.")]
        [TextArea(2, 5)]
        [SerializeField]
        private string description;

        [Tooltip("Sprite displayed by inventory slots and item details.")]
        [SerializeField]
        private Sprite icon;

        [Header("Inventory Behavior")]

        [Tooltip("High-level gameplay category of this item.")]
        [SerializeField]
        private ItemType itemType = ItemType.Tool;

        [Tooltip(
            "Determines whether this item can become the active item in the player's hand."
        )]
        [SerializeField]
        private bool equippable = true;

        [Tooltip(
            "Maximum quantity that can occupy a single inventory slot."
        )]
        [SerializeField, Min(1)]
        private int maximumStackSize = 1;

        /// <summary>
        /// Gets the stable identifier used by runtime and persistence systems.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Gets the player-facing item name.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Gets the player-facing item description.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the icon displayed by inventory interfaces.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Gets the gameplay category of the item.
        /// </summary>
        public ItemType Type => itemType;

        /// <summary>
        /// Gets whether the item can be equipped in the player's hand.
        /// </summary>
        public bool IsEquippable => equippable;

        /// <summary>
        /// Gets the maximum quantity allowed in a single inventory slot.
        /// </summary>
        public int MaximumStackSize => maximumStackSize;

        private void OnValidate()
        {
            maximumStackSize = Mathf.Max(
                1,
                maximumStackSize
            );
        }
    }
}