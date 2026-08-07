using UnityEngine;
using UnityEngine.UI;

namespace LeonardoTask.Inventory.UI
{
    /// <summary>
    /// Coordinates the visual inventory interface with the runtime
    /// pocket and equipment models.
    ///
    /// The HUD reacts to inventory events instead of polling every frame,
    /// keeping presentation independent from inventory business logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryHUD : MonoBehaviour
    {
        private enum SelectionType
        {
            None,
            Pocket,
            Hand
        }

        [Header("Runtime References")]

        [SerializeField]
        private InventoryController inventory;

        [SerializeField]
        private InventoryInputReader input;

        [Header("Slot Views")]

        [SerializeField]
        private InventorySlotView[] pocketSlots;

        [SerializeField]
        private InventorySlotView handSlot;

        [Header("Details")]

        [SerializeField]
        private InventoryDetailsView detailsView;

        [Header("Drag Visualization")]

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private RectTransform dragLayer;

        [SerializeField]
        private Image dragIcon;

        private SelectionType selectionType =
            SelectionType.None;

        private int selectedPocketIndex = -1;

        private InventorySlotView dragSource;

        private bool isDragging;
        /// <summary>
        /// Selects the pocket slot associated with a keyboard quick-slot input.
        /// </summary>
        private void HandleQuickSlotPressed(
            int pocketIndex
        )
        {
            if (pocketIndex < 0 ||
                pocketIndex >= inventory.PocketSlotCount)
            {
                return;
            }

            InventorySlot slot =
                inventory.GetPocketSlot(
                    pocketIndex
                );

            // Empty slots clear the current selection instead of
            // displaying stale details from a previously selected item.
            if (slot.IsEmpty)
            {
                ClearSelection();
                return;
            }

            selectionType =
                SelectionType.Pocket;

            selectedPocketIndex =
                pocketIndex;

            RefreshAll();
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged +=
                    HandleInventoryChanged;

                inventory.EquipmentChanged +=
                    HandleInventoryChanged;
            }

            if (input != null)
            {
                input.QuickSlotPressed +=
                    HandleQuickSlotPressed;
            }
        }

        private void Start()
        {
            ValidateConfiguration();
            RefreshAll();
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged -=
                    HandleInventoryChanged;

                inventory.EquipmentChanged -=
                    HandleInventoryChanged;
            }

            if (input != null)
            {
                input.QuickSlotPressed -=
                    HandleQuickSlotPressed;
            }
        }

        /// <summary>
        /// Selects an inventory slot and updates the details panel.
        /// </summary>
        public void SelectSlot(
            InventorySlotView slotView
        )
        {
            if (slotView == null)
            {
                return;
            }

            if (slotView.Type ==
                InventorySlotView.SlotType.Pocket)
            {
                InventorySlot slot =
                    inventory.GetPocketSlot(
                        slotView.PocketIndex
                    );

                if (slot.IsEmpty)
                {
                    ClearSelection();
                    return;
                }

                selectionType =
                    SelectionType.Pocket;

                selectedPocketIndex =
                    slotView.PocketIndex;
            }
            else
            {
                if (inventory.HandItem == null)
                {
                    ClearSelection();
                    return;
                }

                selectionType =
                    SelectionType.Hand;

                selectedPocketIndex =
                    -1;
            }

            RefreshAll();
        }

        /// <summary>
        /// Begins dragging the item represented by a slot view.
        /// </summary>
        public bool TryBeginDrag(
            InventorySlotView source,
            Vector2 screenPosition
        )
        {
            if (source == null ||
                dragIcon == null)
            {
                return false;
            }

            ItemDefinition item =
                GetItemFromView(source);

            if (item == null ||
                item.Icon == null)
            {
                return false;
            }

            dragSource =
                source;

            isDragging =
                true;

            dragIcon.sprite =
                item.Icon;

            dragIcon.preserveAspect =
                true;

            dragIcon.gameObject.SetActive(
                true
            );

            UpdateDragPosition(
                screenPosition
            );

            return true;
        }

        /// <summary>
        /// Updates the visual drag icon position.
        /// </summary>
        public void UpdateDragPosition(
            Vector2 screenPosition
        )
        {
            if (!isDragging ||
                dragLayer == null ||
                dragIcon == null)
            {
                return;
            }

            Camera eventCamera =
                canvas != null &&
                canvas.renderMode !=
                    RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

            if (!RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        dragLayer,
                        screenPosition,
                        eventCamera,
                        out Vector2 localPosition
                    ))
            {
                return;
            }

            dragIcon.rectTransform.anchoredPosition =
                localPosition;
        }

        /// <summary>
        /// Handles dropping the currently dragged item onto another slot.
        /// </summary>
        public void HandleDrop(
            InventorySlotView target
        )
        {
            if (!isDragging ||
                dragSource == null ||
                target == null ||
                target == dragSource)
            {
                return;
            }

            bool changed =
                false;

            if (dragSource.Type ==
                    InventorySlotView.SlotType.Pocket &&
                target.Type ==
                    InventorySlotView.SlotType.Pocket)
            {
                changed =
                    inventory.TryMoveOrSwap(
                        dragSource.PocketIndex,
                        target.PocketIndex
                    );

                if (changed)
                {
                    selectionType =
                        SelectionType.Pocket;

                    selectedPocketIndex =
                        target.PocketIndex;
                }
            }
            else if (
                dragSource.Type ==
                    InventorySlotView.SlotType.Pocket &&
                target.Type ==
                    InventorySlotView.SlotType.Hand)
            {
                changed =
                    inventory.TryEquipFromPocket(
                        dragSource.PocketIndex
                    );

                if (changed)
                {
                    selectionType =
                        SelectionType.Hand;

                    selectedPocketIndex =
                        -1;
                }
            }
            else if (
                dragSource.Type ==
                    InventorySlotView.SlotType.Hand &&
                target.Type ==
                    InventorySlotView.SlotType.Pocket)
            {
                changed =
                    inventory.TryMoveHandToPocket(
                        target.PocketIndex
                    );

                if (changed)
                {
                    selectionType =
                        SelectionType.Pocket;

                    selectedPocketIndex =
                        target.PocketIndex;
                }
            }

            if (changed)
            {
                RefreshAll();
            }
        }

        /// <summary>
        /// Ends the current drag operation.
        /// </summary>
        public void EndDrag()
        {
            isDragging =
                false;

            dragSource =
                null;

            if (dragIcon != null)
            {
                dragIcon.sprite =
                    null;

                dragIcon.gameObject.SetActive(
                    false
                );
            }
        }

        private void RefreshAll()
        {
            RefreshPocketSlots();
            RefreshHandSlot();
            RefreshDetails();
        }

        private void RefreshPocketSlots()
        {
            if (pocketSlots == null)
            {
                return;
            }

            int count =
                Mathf.Min(
                    pocketSlots.Length,
                    inventory.PocketSlotCount
                );

            for (int i = 0;
                 i < count;
                 i++)
            {
                InventorySlot slot =
                    inventory.GetPocketSlot(i);

                bool selected =
                    selectionType ==
                        SelectionType.Pocket &&
                    selectedPocketIndex ==
                        i;

                pocketSlots[i].Refresh(
                    slot.Item,
                    slot.Quantity,
                    selected
                );
            }
        }

        private void RefreshHandSlot()
        {
            if (handSlot == null)
            {
                return;
            }

            bool selected =
                selectionType ==
                SelectionType.Hand;

            handSlot.Refresh(
                inventory.HandItem,
                inventory.HandItem == null
                    ? 0
                    : 1,
                selected
            );
        }

        private void RefreshDetails()
        {
            if (detailsView == null)
            {
                return;
            }

            switch (selectionType)
            {
                case SelectionType.Pocket:
                    ShowPocketDetails();
                    break;

                case SelectionType.Hand:
                    ShowHandDetails();
                    break;

                default:
                    detailsView.Clear();
                    break;
            }
        }

        private void ShowPocketDetails()
        {
            if (selectedPocketIndex < 0 ||
                selectedPocketIndex >=
                    inventory.PocketSlotCount)
            {
                ClearSelection();
                return;
            }

            InventorySlot slot =
                inventory.GetPocketSlot(
                    selectedPocketIndex
                );

            if (slot.IsEmpty)
            {
                ClearSelection();
                return;
            }

            ItemDefinition item =
                slot.Item;

            if (item.IsEquippable)
            {
                detailsView.Show(
                    item,
                    "EQUIP",
                    EquipSelectedPocketItem
                );

                return;
            }

            // Key items are inspected in the inventory but used
            // contextually by world interactions.
            detailsView.Show(
                item,
                string.Empty,
                null
            );
        }

        private void ShowHandDetails()
        {
            ItemDefinition item =
                inventory.HandItem;

            if (item == null)
            {
                ClearSelection();
                return;
            }

            detailsView.Show(
                item,
                "UNEQUIP",
                UnequipSelectedHandItem
            );
        }

        private void EquipSelectedPocketItem()
        {
            if (selectionType !=
                    SelectionType.Pocket)
            {
                return;
            }

            bool equipped =
                inventory.TryEquipFromPocket(
                    selectedPocketIndex
                );

            if (!equipped)
            {
                return;
            }

            selectionType =
                SelectionType.Hand;

            selectedPocketIndex =
                -1;

            RefreshAll();
        }

        private void UnequipSelectedHandItem()
        {
            bool unequipped =
                inventory.TryUnequipToPocket();

            if (!unequipped)
            {
                return;
            }

            ClearSelection();
        }

        private ItemDefinition GetItemFromView(
            InventorySlotView slotView
        )
        {
            if (slotView.Type ==
                InventorySlotView.SlotType.Hand)
            {
                return inventory.HandItem;
            }

            InventorySlot slot =
                inventory.GetPocketSlot(
                    slotView.PocketIndex
                );

            return slot.IsEmpty
                ? null
                : slot.Item;
        }

        private void HandleInventoryChanged()
        {
            RefreshAll();
        }

        private void ClearSelection()
        {
            selectionType =
                SelectionType.None;

            selectedPocketIndex =
                -1;

            RefreshAll();
        }

        private void ValidateConfiguration()
        {
            if (inventory == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryHUD)} requires an InventoryController reference.",
                    this
                );
            }

            if (pocketSlots == null ||
                pocketSlots.Length !=
                    inventory.PocketSlotCount)
            {
                Debug.LogError(
                    $"{nameof(InventoryHUD)} expected {inventory.PocketSlotCount} pocket slot views.",
                    this
                );
            }

            if (canvas == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryHUD)} requires a Canvas reference.",
                    this
                );
            }
        }
    }
}