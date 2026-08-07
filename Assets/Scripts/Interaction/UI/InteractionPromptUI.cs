using TMPro;
using UnityEngine;

namespace LeonardoTask.Interaction.UI
{
    /// <summary>
    /// Displays the currently available interaction to the player.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]

        [Tooltip(
            "Visual content enabled while an interaction is available."
        )]
        [SerializeField]
        private GameObject contentRoot;

        [SerializeField]
        private TMP_Text keyText;

        [SerializeField]
        private TMP_Text actionText;

        [Header("Presentation")]

        [SerializeField]
        private string interactionKeyLabel = "E";

        private void Awake()
        {
            Hide();
        }

        /// <summary>
        /// Displays the interaction key and contextual action label.
        /// </summary>
        public void Show(
            string actionLabel
        )
        {
            if (keyText != null)
            {
                keyText.text =
                    interactionKeyLabel;
            }

            if (actionText != null)
            {
                actionText.text =
                    actionLabel;
            }

            if (contentRoot != null)
            {
                contentRoot.SetActive(
                    true
                );
            }
        }

        /// <summary>
        /// Hides the interaction prompt.
        /// </summary>
        public void Hide()
        {
            if (contentRoot != null)
            {
                contentRoot.SetActive(
                    false
                );
            }
        }
    }
}