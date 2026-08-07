using UnityEngine;

namespace LeonardoTask.Dialogue
{
    /// <summary>
    /// Controls mutually exclusive visual variants used during dialogue.
    ///
    /// This can create lightweight dialogue animation by switching
    /// between alternate mouth, eye, or complete character visuals.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DialogueVisualVariantSwitcher :
        MonoBehaviour
    {
        [Header("Visual Variants")]

        [SerializeField]
        private GameObject[] variants;

        [SerializeField, Min(0)]
        private int defaultVariantIndex;

        private void Awake()
        {
            ShowVariant(
                defaultVariantIndex
            );
        }

        /// <summary>
        /// Activates one visual variant and disables every other variant.
        /// </summary>
        public void ShowVariant(
            int index
        )
        {
            if (index < 0)
            {
                return;
            }

            if (variants == null ||
                index >= variants.Length)
            {
                Debug.LogWarning(
                    $"Dialogue visual variant index {index} is invalid on '{name}'.",
                    this
                );

                return;
            }

            for (int i = 0;
                 i < variants.Length;
                 i++)
            {
                if (variants[i] == null)
                {
                    continue;
                }

                variants[i].SetActive(
                    i == index
                );
            }
        }
    }
}