using System;
using UnityEngine;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Represents one line of dialogue and its optional visual state.
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        [TextArea(2, 5)]
        [SerializeField]
        private string text;

        [Tooltip(
            "Visual variant displayed while this line is active. " +
            "Use -1 to preserve the current visual state."
        )]
        [SerializeField]
        private int visualVariantIndex = -1;

        public string Text =>
            text;

        public int VisualVariantIndex =>
            visualVariantIndex;
    }
}