using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeonardoTask.Inventory.UI
{
    /// <summary>
    /// Displays information about the currently selected inventory item.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryDetailsView : MonoBehaviour
    {
        [Header("Item Information")]

        [SerializeField]
        private Image itemIcon;

        [SerializeField]
        private TMP_Text itemNameText;

        [SerializeField]
        private TMP_Text itemTypeText;

        [SerializeField]
        private TMP_Text itemDescriptionText;

        [Header("Action")]

        [SerializeField]
        private Button actionButton;

        [SerializeField]
        private TMP_Text actionButtonLabel;

        private Action currentAction;

        private void Awake()
        {
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(
                    HandleActionButtonClicked
                );
            }

            Clear();
        }

        private void OnDestroy()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(
                    HandleActionButtonClicked
                );
            }
        }

        /// <summary>
        /// Displays item information and an optional contextual action.
        /// </summary>
        public void Show(
            ItemDefinition item,
            string actionLabel,
            Action action
        )
        {
            if (item == null)
            {
                Clear();
                return;
            }

            if (itemIcon != null)
            {
                itemIcon.sprite =
                    item.Icon;

                itemIcon.enabled =
                    item.Icon != null;

                itemIcon.preserveAspect = true;
            }

            if (itemNameText != null)
            {
                itemNameText.text =
                    item.DisplayName;
            }

            if (itemTypeText != null)
            {
                itemTypeText.text =
                    item.Type
                        .ToString()
                        .ToUpperInvariant();
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text =
                    item.Description;
            }

            currentAction =
                action;

            bool hasAction =
                currentAction != null;

            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(
                    hasAction
                );
            }

            if (actionButtonLabel != null)
            {
                actionButtonLabel.text =
                    actionLabel ?? string.Empty;
            }
        }

        /// <summary>
        /// Clears the details panel.
        /// </summary>
        public void Clear()
        {
            currentAction = null;

            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (itemNameText != null)
            {
                itemNameText.text =
                    "No Item Selected";
            }

            if (itemTypeText != null)
            {
                itemTypeText.text =
                    string.Empty;
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text =
                    "Select an inventory slot to inspect its contents.";
            }

            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(
                    false
                );
            }
        }

        private void HandleActionButtonClicked()
        {
            currentAction?.Invoke();
        }
    }
}