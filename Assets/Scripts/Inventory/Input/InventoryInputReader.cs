using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Centralizes inventory-specific input through the Unity Input System.
    ///
    /// This component translates quick-slot Input Actions into zero-based
    /// pocket slot indices without containing inventory or UI logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryInputReader : MonoBehaviour
    {
        [Header("Quick Slot Actions")]

        [Tooltip("Input Actions associated with pocket slots 1 through 6.")]
        [SerializeField]
        private InputActionReference[] quickSlotActions;

        /// <summary>
        /// Raised when a quick-slot action is pressed.
        ///
        /// The provided value is a zero-based pocket index:
        /// key 1 produces index 0, key 2 produces index 1, and so on.
        /// </summary>
        public event Action<int> QuickSlotPressed;

        private bool[] actionsEnabledByThisComponent;

        private void Awake()
        {
            actionsEnabledByThisComponent =
                new bool[quickSlotActions?.Length ?? 0];
        }

        private void OnEnable()
        {
            if (quickSlotActions == null)
            {
                return;
            }

            for (int i = 0;
                 i < quickSlotActions.Length;
                 i++)
            {
                InputActionReference actionReference =
                    quickSlotActions[i];

                if (!HasValidAction(actionReference))
                {
                    continue;
                }

                InputAction action =
                    actionReference.action;

                action.performed +=
                    HandleQuickSlotPerformed;

                if (!action.enabled)
                {
                    action.Enable();

                    actionsEnabledByThisComponent[i] =
                        true;
                }
            }
        }

        private void OnDisable()
        {
            if (quickSlotActions == null)
            {
                return;
            }

            for (int i = 0;
                 i < quickSlotActions.Length;
                 i++)
            {
                InputActionReference actionReference =
                    quickSlotActions[i];

                if (!HasValidAction(actionReference))
                {
                    continue;
                }

                InputAction action =
                    actionReference.action;

                action.performed -=
                    HandleQuickSlotPerformed;

                if (i < actionsEnabledByThisComponent.Length &&
                    actionsEnabledByThisComponent[i])
                {
                    action.Disable();

                    actionsEnabledByThisComponent[i] =
                        false;
                }
            }
        }

        /// <summary>
        /// Converts the performed Input Action back into its corresponding
        /// pocket slot index.
        /// </summary>
        private void HandleQuickSlotPerformed(
            InputAction.CallbackContext context
        )
        {
            for (int i = 0;
                 i < quickSlotActions.Length;
                 i++)
            {
                InputActionReference actionReference =
                    quickSlotActions[i];

                if (!HasValidAction(actionReference))
                {
                    continue;
                }

                if (actionReference.action !=
                    context.action)
                {
                    continue;
                }

                QuickSlotPressed?.Invoke(i);

                return;
            }
        }

        private static bool HasValidAction(
            InputActionReference actionReference
        )
        {
            return actionReference != null &&
                   actionReference.action != null;
        }

        private void OnValidate()
        {
            if (quickSlotActions == null)
            {
                return;
            }

            if (quickSlotActions.Length != 6)
            {
                Debug.LogWarning(
                    $"{nameof(InventoryInputReader)} expects exactly six quick-slot actions.",
                    this
                );
            }
        }
    }
}