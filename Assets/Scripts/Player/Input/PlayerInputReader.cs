using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeonardoTask.Player
{
    /// <summary>
    /// Centralizes player input reading through the Unity Input System.
    ///
    /// This component does not control player behavior directly.
    /// It translates Input Actions into values and events that other
    /// gameplay systems can consume.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]

        [SerializeField]
        private InputActionReference moveAction;

        [SerializeField]
        private InputActionReference jumpAction;

        [SerializeField]
        private InputActionReference runAction;

        [SerializeField]
        private InputActionReference interactAction;

        private bool moveEnabledByThisComponent;
        private bool jumpEnabledByThisComponent;
        private bool runEnabledByThisComponent;
        private bool interactEnabledByThisComponent;

        private bool interactCallbackRegistered;

        /// <summary>
        /// Raised when the player performs the interaction action.
        /// </summary>
        public event Action InteractPressed;

        /// <summary>
        /// Gets the current movement direction.
        ///
        /// Horizontal values:
        /// -1 represents left.
        ///  0 represents no horizontal input.
        ///  1 represents right.
        ///
        /// The vertical axis remains available for future actions.
        /// </summary>
        public Vector2 Move
        {
            get
            {
                if (!HasValidAction(moveAction))
                {
                    return Vector2.zero;
                }

                return moveAction.action.ReadValue<Vector2>();
            }
        }

        /// <summary>
        /// Gets whether the jump action was pressed during the current frame.
        /// </summary>
        public bool JumpPressedThisFrame =>
            HasValidAction(jumpAction) &&
            jumpAction.action.WasPressedThisFrame();

        /// <summary>
        /// Gets whether the jump action was released during the current frame.
        /// </summary>
        public bool JumpReleasedThisFrame =>
            HasValidAction(jumpAction) &&
            jumpAction.action.WasReleasedThisFrame();

        /// <summary>
        /// Gets whether the jump action is currently being held.
        /// </summary>
        public bool JumpHeld =>
            HasValidAction(jumpAction) &&
            jumpAction.action.IsPressed();

        /// <summary>
        /// Gets whether the run action is currently being held.
        /// </summary>
        public bool RunHeld =>
            HasValidAction(runAction) &&
            runAction.action.IsPressed();

        private void OnEnable()
        {
            EnableActionIfNecessary(
                moveAction,
                ref moveEnabledByThisComponent
            );

            EnableActionIfNecessary(
                jumpAction,
                ref jumpEnabledByThisComponent
            );

            EnableActionIfNecessary(
                runAction,
                ref runEnabledByThisComponent
            );

            EnableActionIfNecessary(
                interactAction,
                ref interactEnabledByThisComponent
            );

            RegisterInteractCallback();
        }

        private void OnDisable()
        {
            UnregisterInteractCallback();

            DisableActionIfOwned(
                moveAction,
                moveEnabledByThisComponent
            );

            DisableActionIfOwned(
                jumpAction,
                jumpEnabledByThisComponent
            );

            DisableActionIfOwned(
                runAction,
                runEnabledByThisComponent
            );

            DisableActionIfOwned(
                interactAction,
                interactEnabledByThisComponent
            );
        }

        private void RegisterInteractCallback()
        {
            if (!HasValidAction(interactAction) ||
                interactCallbackRegistered)
            {
                return;
            }

            interactAction.action.performed +=
                HandleInteractPerformed;

            interactCallbackRegistered = true;
        }

        private void UnregisterInteractCallback()
        {
            if (!HasValidAction(interactAction) ||
                !interactCallbackRegistered)
            {
                return;
            }

            interactAction.action.performed -=
                HandleInteractPerformed;

            interactCallbackRegistered = false;
        }

        private void HandleInteractPerformed(
            InputAction.CallbackContext context
        )
        {
            InteractPressed?.Invoke();
        }

        private static bool HasValidAction(
            InputActionReference actionReference
        )
        {
            return actionReference != null &&
                   actionReference.action != null;
        }

        /// <summary>
        /// Enables an action only when it has not already been enabled.
        ///
        /// This prevents the component from taking ownership of an action
        /// that is already managed by another system.
        /// </summary>
        private static void EnableActionIfNecessary(
            InputActionReference actionReference,
            ref bool enabledByThisComponent
        )
        {
            enabledByThisComponent = false;

            if (!HasValidAction(actionReference))
            {
                return;
            }

            if (actionReference.action.enabled)
            {
                return;
            }

            actionReference.action.Enable();

            enabledByThisComponent = true;
        }

        /// <summary>
        /// Disables an action only when it was enabled by this component.
        /// </summary>
        private static void DisableActionIfOwned(
            InputActionReference actionReference,
            bool enabledByThisComponent
        )
        {
            if (!enabledByThisComponent ||
                !HasValidAction(actionReference))
            {
                return;
            }

            actionReference.action.Disable();
        }
    }
}