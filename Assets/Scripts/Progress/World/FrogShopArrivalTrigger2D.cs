using LeonardoTask.Interaction;
using UnityEngine;

namespace LeonardoTask.Progress
{
    /// <summary>
    /// Records the player's first successful arrival at the Frog Shop.
    ///
    /// The trigger permanently unlocks the shop shortcut and enables
    /// the next progression step.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class FrogShopArrivalTrigger2D :
        MonoBehaviour
    {
        [Header("Progress")]

        [SerializeField]
        private GameProgressController progress;

        private Collider2D triggerCollider;

        private void Awake()
        {
            triggerCollider =
                GetComponent<Collider2D>();

            if (!triggerCollider.isTrigger)
            {
                Debug.LogError(
                    $"{nameof(FrogShopArrivalTrigger2D)} on '{name}' requires Is Trigger to be enabled.",
                    this
                );

                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed +=
                    RefreshState;
            }
        }

        private void Start()
        {
            RefreshState();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.Changed -=
                    RefreshState;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            if (progress == null ||
                progress.FrogShopReached)
            {
                return;
            }

            PlayerInteractor2D player =
                other.GetComponentInParent<PlayerInteractor2D>();

            if (player == null)
            {
                return;
            }

            progress.ReachFrogShop();
        }

        private void RefreshState()
        {
            if (progress == null)
            {
                return;
            }

            if (progress.FrogShopReached)
            {
                gameObject.SetActive(
                    false
                );
            }
        }

        private void Reset()
        {
            Collider2D collider =
                GetComponent<Collider2D>();

            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
    }
}