using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeonardoTask.Inventory
{
    /// <summary>
    /// Centralizes inventory-specific input through the Unity Input System.
    ///
    /// This component translates quick-slot Input Actions into inventory
    /// selection events without containing inventory or UI behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryInputReader : MonoBehaviour
    {
        [Header("Pocket Slot Actions")]

        [Tooltip(
            "Input Actions associated with pocket slots 1 through 6."
        )]
        [SerializeField]
        private InputActionReference[] quickSlotActions;

        [Header("Hand Slot Action")]

        [Tooltip(
            "Input Action used to select the active Hand slot."
        )]
        [SerializeField]
        private InputActionReference handSlotAction;

        private bool[] quickSlotActionsEnabledByThisComponent;
        private bool handSlotActionEnabledByThisComponent;

        /// <summary>
        /// Raised when a pocket quick-slot action is performed.
        ///
        /// The provided index is zero-based:
        /// key 1 produces index 0, key 2 produces index 1, and so on.
        /// </summary>
        public event Action<int> QuickSlotPressed;

        /// <summary>
        /// Raised when the Hand selection action is performed.
        /// </summary>
        public event Action HandSlotPressed;

        private void Awake()
        {
            quickSlotActionsEnabledByThisComponent =
                new bool[quickSlotActions?.Length ?? 0];
        }

        private void OnEnable()
        {
            EnableQuickSlotActions();
            EnableHandSlotAction();
        }

        private void OnDisable()
        {
            DisableQuickSlotActions();
            DisableHandSlotAction();
        }

        private void EnableQuickSlotActions()
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

                actionReference.action.performed +=
                    HandleQuickSlotPerformed;

                if (actionReference.action.enabled)
                {
                    continue;
                }

                actionReference.action.Enable();

                quickSlotActionsEnabledByThisComponent[i] =
                    true;
            }
        }

        private void DisableQuickSlotActions()
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

                actionReference.action.performed -=
                    HandleQuickSlotPerformed;

                if (i >=
                        quickSlotActionsEnabledByThisComponent.Length ||
                    !quickSlotActionsEnabledByThisComponent[i])
                {
                    continue;
                }

                actionReference.action.Disable();

                quickSlotActionsEnabledByThisComponent[i] =
                    false;
            }
        }

        private void EnableHandSlotAction()
        {
            if (!HasValidAction(handSlotAction))
            {
                return;
            }

            handSlotAction.action.performed +=
                HandleHandSlotPerformed;

            if (handSlotAction.action.enabled)
            {
                return;
            }

            handSlotAction.action.Enable();

            handSlotActionEnabledByThisComponent =
                true;
        }

        private void DisableHandSlotAction()
        {
            if (!HasValidAction(handSlotAction))
            {
                return;
            }

            handSlotAction.action.performed -=
                HandleHandSlotPerformed;

            if (!handSlotActionEnabledByThisComponent)
            {
                return;
            }

            handSlotAction.action.Disable();

            handSlotActionEnabledByThisComponent =
                false;
        }

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

        private void HandleHandSlotPerformed(
            InputAction.CallbackContext context
        )
        {
            HandSlotPressed?.Invoke();
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
                    $"{nameof(InventoryInputReader)} expects exactly six pocket quick-slot actions.",
                    this
                );
            }
        }
    }
}