using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeonardoTask.Inventory.UI
{
    /// <summary>
    /// Represents a single interactive inventory slot in the UI.
    ///
    /// The view is responsible only for presenting slot state and
    /// forwarding pointer interactions to InventoryHUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventorySlotView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        public enum SlotType
        {
            Pocket,
            Hand
        }

        [Header("Slot Configuration")]

        [SerializeField]
        private SlotType slotType = SlotType.Pocket;

        [SerializeField, Min(0)]
        private int pocketIndex;

        [Header("References")]

        [SerializeField]
        private InventoryHUD hud;

        [SerializeField]
        private Image itemIcon;

        [SerializeField]
        private Image selectionFrame;

        [SerializeField]
        private TMP_Text quantityText;

        private bool dragStarted;

        /// <summary>
        /// Gets whether this view represents a pocket or hand slot.
        /// </summary>
        public SlotType Type => slotType;

        /// <summary>
        /// Gets the pocket index represented by this view.
        /// This value is ignored for Hand slots.
        /// </summary>
        public int PocketIndex => pocketIndex;

        /// <summary>
        /// Refreshes the visual state of the slot.
        /// </summary>
        public void Refresh(
            ItemDefinition item,
            int quantity,
            bool selected
        )
        {
            bool hasItem =
                item != null;

            if (itemIcon != null)
            {
                itemIcon.sprite =
                    hasItem
                        ? item.Icon
                        : null;

                itemIcon.enabled =
                    hasItem &&
                    item.Icon != null;

                itemIcon.preserveAspect = true;
            }

            if (selectionFrame != null)
            {
                selectionFrame.gameObject.SetActive(
                    selected
                );
            }

            if (quantityText != null)
            {
                bool showQuantity =
                    hasItem &&
                    quantity > 1;

                quantityText.gameObject.SetActive(
                    showQuantity
                );

                if (showQuantity)
                {
                    quantityText.text =
                        quantity.ToString();
                }
            }
        }

        public void OnPointerClick(
            PointerEventData eventData
        )
        {
            if (eventData.button !=
                PointerEventData.InputButton.Left)
            {
                return;
            }

            hud.SelectSlot(
                this
            );
        }

        public void OnBeginDrag(
            PointerEventData eventData
        )
        {
            if (eventData.button !=
                PointerEventData.InputButton.Left)
            {
                return;
            }

            dragStarted =
                hud.TryBeginDrag(
                    this,
                    eventData.position
                );
        }

        public void OnDrag(
            PointerEventData eventData
        )
        {
            if (!dragStarted)
            {
                return;
            }

            hud.UpdateDragPosition(
                eventData.position
            );
        }

        public void OnEndDrag(
            PointerEventData eventData
        )
        {
            if (!dragStarted)
            {
                return;
            }

            dragStarted = false;

            hud.EndDrag();
        }

        public void OnDrop(
            PointerEventData eventData
        )
        {
            hud.HandleDrop(
                this
            );
        }
    }
}