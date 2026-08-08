using LeonardoTask.Inventory;
using LeonardoTask.Progress;
using LeonardoTask.Respawn;
using UnityEngine;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Automatically starts a dialogue when the player enters
    /// a 2D trigger area.
    ///
    /// The trigger can optionally require and consume an inventory item,
    /// allowing the same component to support both simple narrative
    /// moments and lightweight progression gates.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class AutomaticDialogueTrigger2D :
        MonoBehaviour
    {
        [Header("Dialogue")]

        [SerializeField]
        private DialogueController dialogueController;

        [SerializeField]
        private DialogueLine[] lines;

        [Header("Optional Item Requirement")]

        [Tooltip(
            "Optional item required before this trigger can activate."
        )]
        [SerializeField]
        private ItemDefinition requiredItem;

        [SerializeField]
        private InventoryController inventory;

        [Tooltip(
            "Removes one instance of the required item when the dialogue starts."
        )]
        [SerializeField]
        private bool consumeRequiredItem;

        [Header("Optional Progression")]

        [SerializeField]
        private GameProgressController progress;

        [Tooltip(
            "Marks the castle progression as completed when this trigger activates."
        )]
        [SerializeField]
        private bool markCastleDoorOpened;

        private Collider2D triggerCollider;
        private bool triggered;

        private void Awake()
        {
            triggerCollider =
                GetComponent<Collider2D>();

            triggerCollider.isTrigger =
                true;
        }

        private void Start()
        {
            if (markCastleDoorOpened &&
                progress != null &&
                progress.CastleDoorOpened)
            {
                triggered = true;
                triggerCollider.enabled = false;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            if (triggered)
            {
                return;
            }

            PlayerRespawnController player =
                other.GetComponentInParent
                    <PlayerRespawnController>();

            if (player == null)
            {
                return;
            }

            if (!CanSatisfyItemRequirement())
            {
                return;
            }

            if (dialogueController == null ||
                lines == null ||
                lines.Length == 0)
            {
                return;
            }

            bool dialogueStarted =
                dialogueController.BeginDialogue(
                    lines,
                    null,
                    null
                );

            if (!dialogueStarted)
            {
                return;
            }

            triggered = true;

            ConsumeRequiredItem();

            if (markCastleDoorOpened &&
                progress != null)
            {
                progress.MarkCastleDoorOpened();
            }

            triggerCollider.enabled =
                false;
        }

        private bool CanSatisfyItemRequirement()
        {
            if (requiredItem == null)
            {
                return true;
            }

            if (inventory == null)
            {
                return false;
            }

            return inventory.Contains(
                       requiredItem
                   ) ||
                   inventory.IsEquipped(
                       requiredItem
                   );
        }

        private void ConsumeRequiredItem()
        {
            if (!consumeRequiredItem ||
                requiredItem == null ||
                inventory == null)
            {
                return;
            }

            bool removed =
                inventory.TryRemoveItem(
                    requiredItem,
                    1
                );

            if (!removed)
            {
                Debug.LogWarning(
                    $"{nameof(AutomaticDialogueTrigger2D)} could not consume '{requiredItem.DisplayName}'.",
                    this
                );
            }
        }
    }
}