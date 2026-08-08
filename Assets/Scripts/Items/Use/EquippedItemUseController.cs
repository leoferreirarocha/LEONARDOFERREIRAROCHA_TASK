using System.Collections.Generic;
using LeonardoTask.Inventory;
using UnityEngine;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Resolves gameplay behavior for the item currently equipped
    /// in the player's Hand slot.
    ///
    /// Item-specific behaviors are discovered automatically from
    /// child objects, allowing new equipped-item behaviors to be
    /// added without modifying this controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EquippedItemUseController :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private InventoryController inventory;

        private readonly Dictionary
            <ItemDefinition, EquippedItemUseBehaviour>
            behavioursByItem = new();

        private void Awake()
        {
            BuildBehaviourLookup();
        }

        /// <summary>
        /// Attempts to use whichever item is currently equipped
        /// in the player's Hand.
        /// </summary>
        public bool TryUseEquippedItem()
        {
            if (inventory == null ||
                inventory.HandItem == null)
            {
                return false;
            }

            return TryUseEquippedItem(
                inventory.HandItem
            );
        }

        /// <summary>
        /// Attempts to use the equipped item only when it matches
        /// the requested ItemDefinition.
        ///
        /// Contextual interactions use this overload when a specific
        /// equipped item is required.
        /// </summary>
        public bool TryUseEquippedItem(
            ItemDefinition expectedItem
        )
        {
            if (inventory == null ||
                expectedItem == null ||
                inventory.HandItem != expectedItem)
            {
                return false;
            }

            if (!behavioursByItem.TryGetValue(
                    expectedItem,
                    out EquippedItemUseBehaviour behaviour
                ))
            {
                return false;
            }

            if (behaviour == null ||
                !behaviour.isActiveAndEnabled)
            {
                return false;
            }

            return behaviour.Use();
        }

        /// <summary>
        /// Rebuilds the runtime lookup using equipped-item behaviors
        /// attached to this object or any of its children.
        /// </summary>
        private void BuildBehaviourLookup()
        {
            behavioursByItem.Clear();

            EquippedItemUseBehaviour[] behaviours =
                GetComponentsInChildren
                    <EquippedItemUseBehaviour>(
                        true
                    );

            foreach (
                EquippedItemUseBehaviour behaviour
                in behaviours
            )
            {
                if (behaviour == null ||
                    behaviour.Item == null)
                {
                    continue;
                }

                if (behavioursByItem.ContainsKey(
                        behaviour.Item
                    ))
                {
                    Debug.LogWarning(
                        $"Multiple equipped-item behaviors are configured for '{behaviour.Item.DisplayName}'. " +
                        "Only the first behavior will be used.",
                        this
                    );

                    continue;
                }

                behavioursByItem.Add(
                    behaviour.Item,
                    behaviour
                );
            }
        }

        private void OnValidate()
        {
            if (inventory == null)
            {
                inventory =
                    GetComponent<InventoryController>();
            }
        }
    }
}