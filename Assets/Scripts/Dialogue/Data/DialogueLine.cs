using System;
using UnityEngine;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Represents one dialogue line, including its speaker,
    /// text content, and optional visual variant.
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        [Tooltip(
            "Name displayed as the speaker of this dialogue line."
        )]
        [SerializeField]
        private string speakerName;

        [TextArea(2, 5)]
        [SerializeField]
        private string text;

        [Tooltip(
            "Visual variant displayed while this line is active. " +
            "Use -1 to preserve the current visual state."
        )]
        [SerializeField]
        private int visualVariantIndex = -1;

        /// <summary>
        /// Gets the speaker displayed for this line.
        /// </summary>
        public string SpeakerName =>
            speakerName;

        /// <summary>
        /// Gets the dialogue text.
        /// </summary>
        public string Text =>
            text;

        /// <summary>
        /// Gets the optional visual variant index.
        /// </summary>
        public int VisualVariantIndex =>
            visualVariantIndex;
    }
}