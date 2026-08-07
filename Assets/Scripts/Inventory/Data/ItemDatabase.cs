using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Provides centralized access to every item definition available
    /// in the game.
    ///
    /// The database allows persistence systems to resolve stable item IDs
    /// back into their corresponding ScriptableObject definitions.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemDatabase",
        menuName = "Leonardo Task/Inventory/Item Database",
        order = 1
    )]
    public sealed class ItemDatabase : ScriptableObject
    {
        [Tooltip(
            "All item definitions that may be referenced by runtime or save systems."
        )]
        [SerializeField]
        private List<ItemDefinition> items = new();

        private Dictionary<string, ItemDefinition> itemLookup;

        /// <summary>
        /// Gets all item definitions registered in the database.
        /// </summary>
        public IReadOnlyList<ItemDefinition> Items => items;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        /// <summary>
        /// Attempts to resolve an item definition from its stable identifier.
        /// </summary>
        public bool TryGetItem(
            string itemId,
            out ItemDefinition item
        )
        {
            item = null;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            EnsureLookup();

            return itemLookup.TryGetValue(
                itemId,
                out item
            );
        }

        /// <summary>
        /// Returns an item definition for the provided identifier,
        /// or null when no matching definition exists.
        /// </summary>
        public ItemDefinition GetItemOrNull(string itemId)
        {
            return TryGetItem(
                itemId,
                out ItemDefinition item
            )
                ? item
                : null;
        }

        private void EnsureLookup()
        {
            if (itemLookup == null)
            {
                RebuildLookup();
            }
        }

        /// <summary>
        /// Rebuilds the runtime ID lookup and reports invalid database entries.
        /// </summary>
        private void RebuildLookup()
        {
            itemLookup = new Dictionary<string, ItemDefinition>(
                StringComparer.Ordinal
            );

            foreach (ItemDefinition item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    Debug.LogError(
                        $"Item definition '{item.name}' has an empty ID.",
                        item
                    );

                    continue;
                }

                if (!itemLookup.TryAdd(item.Id, item))
                {
                    Debug.LogError(
                        $"Duplicate item ID '{item.Id}' detected in {name}.",
                        this
                    );
                }
            }
        }
    }
}