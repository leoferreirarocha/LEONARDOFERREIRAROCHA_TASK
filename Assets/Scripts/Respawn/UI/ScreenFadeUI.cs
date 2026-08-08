using System.Collections;
using UnityEngine;

namespace LeonardoTask.Respawn
{
    /// <summary>
    /// Controls a full-screen CanvasGroup used for simple screen fades.
    ///
    /// The overlay remains active at all times and becomes invisible
    /// by setting its CanvasGroup alpha to zero.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFadeUI : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Gets the current fade opacity.
        /// </summary>
        public float Alpha =>
            canvasGroup != null
                ? canvasGroup.alpha
                : 0f;

        private void Awake()
        {
            canvasGroup =
                GetComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            SetAlpha(0f);
        }

        /// <summary>
        /// Immediately sets the overlay opacity.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha =
                Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// Smoothly fades the overlay toward the requested opacity.
        ///
        /// Unscaled time keeps the fade independent from gameplay
        /// time scale changes.
        /// </summary>
        public IEnumerator FadeTo(
            float targetAlpha,
            float duration
        )
        {
            targetAlpha =
                Mathf.Clamp01(targetAlpha);

            if (canvasGroup == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float startingAlpha =
                canvasGroup.alpha;

            float distance =
                Mathf.Abs(
                    targetAlpha -
                    startingAlpha
                );

            if (distance <= Mathf.Epsilon)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float fadeSpeed =
                distance /
                duration;

            while (!Mathf.Approximately(
                       canvasGroup.alpha,
                       targetAlpha
                   ))
            {
                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        targetAlpha,
                        fadeSpeed *
                        Time.unscaledDeltaTime
                    );

                yield return null;
            }

            SetAlpha(targetAlpha);
        }
    }
}