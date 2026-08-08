using LeonardoTask.Dialogue;
using LeonardoTask.Interaction;
using LeonardoTask.Inventory;
using LeonardoTask.Items;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Coordinates the Frog's progression-specific interaction.
    ///
    /// While asleep, the Frog uses the normal sleeping dialogue.
    /// Using the equipped Trumpet near the Frog wakes it permanently,
    /// starts the awakening conversation, and grants the Wand when
    /// the conversation is completed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrogProgressionInteractable :
        InteractableBehaviour
    {
        [Header("Runtime References")]

        [SerializeField]
        private GameProgressController progress;

        [SerializeField]
        private InventoryController inventory;

        [SerializeField]
        private EquippedItemUseController
            equippedItemUseController;

        [SerializeField]
        private DialogueController dialogueController;

        [Header("Progression Items")]

        [SerializeField]
        private ItemDefinition trumpetItem;

        [SerializeField]
        private ItemDefinition wandItem;

        [Header("Sleeping Dialogue")]

        [Tooltip(
            "Dialogue shown while the Frog is asleep and the Trumpet is not equipped."
        )]
        [SerializeField]
        private DialogueLine[] sleepingLines;

        [Header("Awakening Dialogue")]

        [Tooltip(
            "Dialogue shown after the equipped Trumpet wakes the Frog. " +
            "Completing this sequence grants the Wand."
        )]
        [SerializeField]
        private DialogueLine[] awakeningLines;

        [Header("Post-Reward Dialogue")]

        [Tooltip(
            "Optional dialogue shown when talking to the Frog after the Wand has already been received."
        )]
        [SerializeField]
        private DialogueLine[] postRewardLines;

        [Header("Visual Switching")]

        [SerializeField]
        private DialogueVisualVariantSwitcher
            visualSwitcher;

        [Tooltip(
            "Visual variant used when the Frog is sleeping outside dialogue."
        )]
        [SerializeField, Min(0)]
        private int sleepingIdleVariantIndex = 0;

        [Tooltip(
            "Visual variant used when the Frog is awake outside dialogue."
        )]
        [SerializeField, Min(0)]
        private int awakeIdleVariantIndex = 1;

        /// <summary>
        /// Gets whether the Frog currently has a valid dialogue
        /// or progression interaction available.
        /// </summary>
        public override bool CanInteract
        {
            get
            {
                if (!base.CanInteract ||
                    progress == null ||
                    inventory == null ||
                    dialogueController == null ||
                    dialogueController.IsDialogueActive)
                {
                    return false;
                }

                if (!progress.FrogAwake)
                {
                    return IsTrumpetEquipped()
                        ? HasLines(awakeningLines)
                        : HasLines(sleepingLines);
                }

                // If the game was closed after waking the Frog but before
                // the Wand reward was completed, allow the awakening
                // conversation to be resumed safely.
                if (!progress.WandReceived)
                {
                    return HasLines(
                        awakeningLines
                    );
                }

                return HasLines(
                    postRewardLines
                );
            }
        }

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed +=
                    HandleProgressChanged;
            }
        }

        private void Start()
        {
            ApplyPersistentVisualState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.Changed -=
                    HandleProgressChanged;
            }
        }

        /// <summary>
        /// Selects the appropriate Frog behavior from persistent
        /// progression and the currently equipped Hand item.
        /// </summary>
        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                return;
            }

            if (!progress.FrogAwake)
            {
                if (IsTrumpetEquipped())
                {
                    WakeFrogWithTrumpet();
                    return;
                }

                StartSleepingDialogue();
                return;
            }

            if (!progress.WandReceived)
            {
                StartAwakeningDialogue();
                return;
            }

            StartPostRewardDialogue();
        }

        /// <summary>
        /// Plays the equipped Trumpet and begins the permanent
        /// Frog awakening sequence.
        /// </summary>
        private void WakeFrogWithTrumpet()
        {
            bool trumpetUsed =
                equippedItemUseController
                    .TryUseEquippedItem(
                        trumpetItem
                    );

            if (!trumpetUsed)
            {
                Debug.LogWarning(
                    "The Frog detected the equipped Trumpet, but no valid Trumpet use behavior could be executed.",
                    this
                );
            }

            bool dialogueStarted =
                dialogueController.BeginDialogue(
                    awakeningLines,
                    visualSwitcher,
                    HandleAwakeningDialogueCompleted
                );

            if (!dialogueStarted)
            {
                return;
            }

            // Persist the awakening only after the dialogue sequence
            // has successfully started.
            progress.MarkFrogAwake();
        }

        /// <summary>
        /// Starts the normal dialogue used while the Frog remains asleep.
        /// </summary>
        private void StartSleepingDialogue()
        {
            dialogueController.BeginDialogue(
                sleepingLines,
                visualSwitcher,
                null
            );
        }

        /// <summary>
        /// Starts or resumes the awakening dialogue.
        ///
        /// This also supports recovering from a session that ended
        /// after the Frog woke but before the Wand was granted.
        /// </summary>
        private void StartAwakeningDialogue()
        {
            dialogueController.BeginDialogue(
                awakeningLines,
                visualSwitcher,
                HandleAwakeningDialogueCompleted
            );
        }

        /// <summary>
        /// Starts optional repeat dialogue after the Frog's reward
        /// has already been collected.
        /// </summary>
        private void StartPostRewardDialogue()
        {
            dialogueController.BeginDialogue(
                postRewardLines,
                visualSwitcher,
                null
            );
        }

        /// <summary>
        /// Grants the Wand after the awakening conversation completes.
        /// </summary>
        private void HandleAwakeningDialogueCompleted()
        {
            TryGrantWand();

            ApplyPersistentVisualState();
        }

        /// <summary>
        /// Adds the Wand to the player's Pocket exactly once.
        ///
        /// Existing ownership is treated as a valid reward state,
        /// preventing duplicate Wand instances after save recovery.
        /// </summary>
        private bool TryGrantWand()
        {
            if (progress.WandReceived)
            {
                return true;
            }

            bool alreadyOwnsWand =
                inventory.Contains(
                    wandItem
                ) ||
                inventory.IsEquipped(
                    wandItem
                );

            if (!alreadyOwnsWand)
            {
                bool added =
                    inventory.TryAddItem(
                        wandItem,
                        1
                    );

                if (!added)
                {
                    Debug.LogWarning(
                        "The Frog could not grant the Wand because the inventory has no available capacity.",
                        this
                    );

                    return false;
                }
            }

            progress.MarkWandReceived();

            return true;
        }

        private bool IsTrumpetEquipped()
        {
            return trumpetItem != null &&
                   inventory.IsEquipped(
                       trumpetItem
                   );
        }

        private void HandleProgressChanged()
        {
            // Dialogue lines temporarily control Frog visuals.
            // Do not overwrite those variants while a conversation
            // is currently being presented.
            if (dialogueController != null &&
                dialogueController.IsDialogueActive)
            {
                return;
            }

            ApplyPersistentVisualState();
        }

        /// <summary>
        /// Reconstructs the Frog's idle appearance from persistent
        /// progression after loading or completing dialogue.
        /// </summary>
        private void ApplyPersistentVisualState()
        {
            if (progress == null ||
                visualSwitcher == null)
            {
                return;
            }

            visualSwitcher.ShowVariant(
                progress.FrogAwake
                    ? awakeIdleVariantIndex
                    : sleepingIdleVariantIndex
            );
        }

        private static bool HasLines(
            DialogueLine[] lines
        )
        {
            return lines != null &&
                   lines.Length > 0;
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if (progress == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires a GameProgressController reference.",
                    this
                );

                valid = false;
            }

            if (inventory == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires an InventoryController reference.",
                    this
                );

                valid = false;
            }

            if (equippedItemUseController == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires an EquippedItemUseController reference.",
                    this
                );

                valid = false;
            }

            if (dialogueController == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires a DialogueController reference.",
                    this
                );

                valid = false;
            }

            if (trumpetItem == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires the Trumpet ItemDefinition.",
                    this
                );

                valid = false;
            }

            if (wandItem == null)
            {
                Debug.LogError(
                    $"{nameof(FrogProgressionInteractable)} on '{name}' requires the Wand ItemDefinition.",
                    this
                );

                valid = false;
            }

            return valid;
        }
    }
}