using LeonardoTask.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Reusable world interaction that starts an NPC dialogue sequence.
    /// </summary>
    public sealed class NPCDialogueInteractable :
        InteractableBehaviour
    {
        [Header("Dialogue")]

        [SerializeField]
        private DialogueController dialogueController;

        [SerializeField]
        private string speakerName = "NPC";

        [SerializeField]
        private DialogueLine[] lines;

        [Header("Optional Visual Switching")]

        [SerializeField]
        private DialogueVisualVariantSwitcher visualSwitcher;

        [Header("Events")]

        [SerializeField]
        private UnityEvent onDialogueCompleted;

        public override bool CanInteract =>
            base.CanInteract &&
            dialogueController != null &&
            !dialogueController.IsDialogueActive &&
            lines != null &&
            lines.Length > 0;

        public override void Interact(
            PlayerInteractor2D interactor
        )
        {
            if (!CanInteract)
            {
                return;
            }

            dialogueController.BeginDialogue(
                speakerName,
                lines,
                visualSwitcher,
                HandleDialogueCompleted
            );
        }

        private void HandleDialogueCompleted()
        {
            onDialogueCompleted?.Invoke();
        }
    }
}