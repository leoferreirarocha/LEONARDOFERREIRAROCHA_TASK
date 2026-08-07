using System;
using LeonardoTask.Interaction;
using LeonardoTask.Player;
using TMPro;
using UnityEngine;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Coordinates dialogue presentation, interaction input,
    /// optional visual variants, and temporary player control locking.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DialogueController :
        MonoBehaviour
    {
        [Header("Player References")]

        [SerializeField]
        private PlayerInputReader input;

        [SerializeField]
        private PlayerMovement2D playerMovement;

        [SerializeField]
        private PlayerInteractor2D playerInteractor;

        [Header("UI References")]

        [SerializeField]
        private GameObject dialoguePanel;

        [SerializeField]
        private TMP_Text speakerNameText;

        [SerializeField]
        private TMP_Text dialogueText;

        [SerializeField]
        private TMP_Text continueHintText;

        [Header("Presentation")]

        [SerializeField]
        private string continueHint = "E";

        private DialogueLine[] activeLines;

        private DialogueVisualVariantSwitcher
            activeVisualSwitcher;

        private Action completionCallback;

        private int currentLineIndex;

        public bool IsDialogueActive
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(
                    false
                );
            }
        }

        private void OnDisable()
        {
            if (IsDialogueActive)
            {
                CancelDialogue();
            }
        }

        /// <summary>
        /// Begins a dialogue sequence and temporarily transfers ownership
        /// of the Interact input from world interaction to dialogue.
        /// </summary>
        public bool BeginDialogue(
            DialogueLine[] lines,
            DialogueVisualVariantSwitcher visualSwitcher,
            Action onCompleted
        )
        {
            if (IsDialogueActive ||
                lines == null ||
                lines.Length == 0 ||
                input == null)
            {
                return false;
            }

            activeLines =
                lines;

            activeVisualSwitcher =
                visualSwitcher;

            completionCallback =
                onCompleted;

            currentLineIndex =
                0;

            IsDialogueActive =
                true;

            playerMovement?.SetMovementEnabled(
                false
            );

            playerInteractor?.SetInteractionEnabled(
                false
            );

            // World interaction has released the input at this point,
            // so dialogue can safely own the same action.
            input.InteractPressed +=
                HandleInteractPressed;

            if (continueHintText != null)
            {
                continueHintText.text =
                    continueHint;
            }

            dialoguePanel?.SetActive(
                true
            );

            DisplayCurrentLine();

            return true;
        }

        private void HandleInteractPressed()
        {
            if (!IsDialogueActive)
            {
                return;
            }

            AdvanceDialogue();
        }

        private void AdvanceDialogue()
        {
            currentLineIndex++;

            if (currentLineIndex >=
                activeLines.Length)
            {
                FinishDialogue();

                return;
            }

            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            DialogueLine line =
                activeLines[currentLineIndex];

            if (speakerNameText != null)
            {
                speakerNameText.text =
                    line.SpeakerName;
            }

            if (dialogueText != null)
            {
                dialogueText.text =
                    line.Text;
            }

            if (line.VisualVariantIndex >= 0)
            {
                activeVisualSwitcher?.ShowVariant(
                    line.VisualVariantIndex
                );
            }
        }

        private void FinishDialogue()
        {
            Action callback =
                completionCallback;

            StopListeningForInput();

            ClearRuntimeState();

            ReleasePlayerControl();

            callback?.Invoke();

            playerInteractor?.RefreshInteraction();
        }

        private void CancelDialogue()
        {
            StopListeningForInput();

            ClearRuntimeState();

            ReleasePlayerControl();
        }

        private void StopListeningForInput()
        {
            if (input != null)
            {
                input.InteractPressed -=
                    HandleInteractPressed;
            }
        }

        private void ClearRuntimeState()
        {
            IsDialogueActive =
                false;

            activeLines =
                null;

            activeVisualSwitcher =
                null;

            completionCallback =
                null;

            currentLineIndex =
                0;

            dialoguePanel?.SetActive(
                false
            );
        }

        private void ReleasePlayerControl()
        {
            playerMovement?.SetMovementEnabled(
                true
            );

            playerInteractor?.SetInteractionEnabled(
                true
            );
        }
    }
}