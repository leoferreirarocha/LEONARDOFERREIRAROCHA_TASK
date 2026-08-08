using System.Collections.Generic;
using LeonardoTask.Inventory;
using LeonardoTask.Player;
using UnityEngine;

namespace LeonardoTask.Items
{
    /// <summary>
    /// Resolves gameplay behavior for the item currently equipped
    /// in the player's Hand slot.
    ///
    /// The controller supports both momentary and continuous item use.
    /// Continuous use is automatically ended when the Interact input
    /// is released or when the equipped item changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EquippedItemUseController :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private InventoryController inventory;

        [SerializeField]
        private PlayerInputReader input;

        private readonly Dictionary
            <ItemDefinition, EquippedItemUseBehaviour>
            behavioursByItem = new();

        private EquippedItemUseBehaviour
            activeBehaviour;

        private void Awake()
        {
            BuildBehaviourLookup();
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.InteractReleased +=
                    HandleInteractReleased;
            }

            if (inventory != null)
            {
                inventory.EquipmentChanged +=
                    HandleEquipmentChanged;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.InteractReleased -=
                    HandleInteractReleased;
            }

            if (inventory != null)
            {
                inventory.EquipmentChanged -=
                    HandleEquipmentChanged;
            }

            EndActiveUse();
        }

        /// <summary>
        /// Attempts to begin using whichever item is currently equipped
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
        /// Attempts to begin using the equipped item only when it matches
        /// the requested ItemDefinition.
        ///
        /// Contextual interactions such as the Frog awakening sequence
        /// use this overload when a specific equipped item is required.
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

            // A new use always replaces any previous active use.
            // This prevents continuous behaviors from overlapping.
            EndActiveUse();

            bool started =
                behaviour.BeginUse();

            if (!started)
            {
                return false;
            }

            activeBehaviour =
                behaviour;

            return true;
        }

        /// <summary>
        /// Ends any currently active equipped-item behavior.
        /// </summary>
        public void EndActiveUse()
        {
            if (activeBehaviour == null)
            {
                return;
            }

            activeBehaviour.EndUse();

            activeBehaviour =
                null;
        }

        private void HandleInteractReleased()
        {
            EndActiveUse();
        }

        private void HandleEquipmentChanged()
        {
            if (activeBehaviour == null)
            {
                return;
            }

            // Stop a continuous behavior immediately when its item
            // is no longer the item equipped in the Hand.
            if (inventory.HandItem !=
                activeBehaviour.Item)
            {
                EndActiveUse();
            }
        }

        /// <summary>
        /// Builds the runtime item-to-behavior lookup from child objects.
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
            if (input == null)
            {
                input =
                    GetComponent<PlayerInputReader>();
            }
        }
    }
}