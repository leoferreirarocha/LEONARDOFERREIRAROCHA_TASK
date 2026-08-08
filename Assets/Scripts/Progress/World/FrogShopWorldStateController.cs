using LeonardoTask.Inventory;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Synchronizes Frog Shop world objects with persistent progression.
    ///
    /// The shortcut remains blocked before the shop is reached.
    /// The Trumpet becomes available at home afterward until collected.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrogShopWorldStateController :
        MonoBehaviour
    {
        [Header("Runtime References")]

        [SerializeField]
        private GameProgressController progress;

        [SerializeField]
        private InventoryController inventory;

        [Header("Trumpet")]

        [SerializeField]
        private ItemDefinition trumpetItem;

        [SerializeField]
        private GameObject trumpetPickup;

        [Header("Shortcut")]

        [Tooltip(
            "World object that blocks the shortcut before it is unlocked."
        )]
        [SerializeField]
        private GameObject shortcutBlocker;

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed +=
                    RefreshWorldState;
            }

            if (inventory != null)
            {
                inventory.InventoryChanged +=
                    RefreshWorldState;

                inventory.EquipmentChanged +=
                    RefreshWorldState;
            }
        }

        private void Start()
        {
            RefreshWorldState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.Changed -=
                    RefreshWorldState;
            }

            if (inventory != null)
            {
                inventory.InventoryChanged -=
                    RefreshWorldState;

                inventory.EquipmentChanged -=
                    RefreshWorldState;
            }
        }

        /// <summary>
        /// Applies the current persistent progression to the Frog Shop
        /// and home Trumpet pickup.
        /// </summary>
        private void RefreshWorldState()
        {
            if (progress == null ||
                inventory == null ||
                trumpetItem == null)
            {
                return;
            }

            if (shortcutBlocker != null)
            {
                shortcutBlocker.SetActive(
                    !progress.ShortcutUnlocked
                );
            }

            bool ownsTrumpet =
                inventory.Contains(trumpetItem) ||
                inventory.IsEquipped(trumpetItem);

            bool trumpetShouldBeAvailable =
                progress.FrogShopReached &&
                !ownsTrumpet;

            if (trumpetPickup != null)
            {
                trumpetPickup.SetActive(
                    trumpetShouldBeAvailable
                );
            }
        }
    }
}