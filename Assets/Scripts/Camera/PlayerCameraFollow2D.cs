using UnityEngine;
using UnityEngine.Serialization;

namespace LeonardoTask.CameraSystem
{
    /// <summary>
    /// Smoothly follows a target in a 2D platforming environment.
    ///
    /// The camera applies independent horizontal and vertical damping,
    /// preserves its initial vertical framing, and respects configurable
    /// world-space boundaries.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class PlayerCameraFollow2D : MonoBehaviour
    {
        [Header("Target")]

        [Tooltip(
            "Root transform followed by the camera. " +
            "Assign the Player root instead of the visual child."
        )]
        [SerializeField]
        private Transform target;

        [FormerlySerializedAs("preserveInitialOffset")]
        [Tooltip(
            "Preserves the initial vertical distance between the camera " +
            "and the target when the scene begins."
        )]
        [SerializeField]
        private bool preserveInitialVerticalOffset = true;

        [FormerlySerializedAs("manualOffset")]
        [Tooltip(
            "Offset applied while following the target. " +
            "The horizontal value is always applied. The vertical value is used " +
            "only when Preserve Initial Vertical Offset is disabled."
        )]
        [SerializeField]
        private Vector2 followOffset = Vector2.zero;

        [Header("Follow Smoothing")]

        [Tooltip(
            "Approximate time required for the camera to catch up " +
            "with horizontal target movement."
        )]
        [SerializeField, Min(0.01f)]
        private float horizontalSmoothTime = 0.18f;

        [Tooltip(
            "Approximate time required for the camera to catch up " +
            "with vertical target movement."
        )]
        [SerializeField, Min(0.01f)]
        private float verticalSmoothTime = 0.25f;

        [Tooltip(
            "Maximum speed at which the camera can move toward the target."
        )]
        [SerializeField, Min(0.01f)]
        private float maximumFollowSpeed = 40f;

        [Tooltip(
            "Maximum horizontal distance allowed between the camera's " +
            "desired position and its current position. " +
            "Set this value to zero to disable the limit."
        )]
        [SerializeField, Min(0f)]
        private float maximumHorizontalLag = 2.25f;

        [Header("Camera Bounds")]

        [Tooltip(
            "Lowest world-space X position the camera is allowed to reach."
        )]
        [SerializeField]
        private float minimumX = 0f;

        [Tooltip(
            "Lowest world-space Y position the camera is allowed to reach."
        )]
        [SerializeField]
        private float minimumY = -2f;

        private Vector2 runtimeOffset;

        private float horizontalVelocity;
        private float verticalVelocity;
        private float cameraZ;

        private bool isInitialized;

        private void Awake()
        {
            cameraZ = transform.position.z;
        }

        private void Start()
        {
            if (target == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerCameraFollow2D)} on {name} requires a target.",
                    this
                );

                enabled = false;
                return;
            }

            CalculateRuntimeOffset();
            ClampInitialPosition();

            isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!isInitialized || target == null)
            {
                return;
            }

            Vector2 desiredPosition = CalculateDesiredPosition();
            Vector3 currentPosition = transform.position;

            float nextX = CalculateHorizontalPosition(
                currentPosition.x,
                desiredPosition.x
            );

            float nextY = CalculateVerticalPosition(
                currentPosition.y,
                desiredPosition.y
            );

            transform.position = new Vector3(
                nextX,
                nextY,
                cameraZ
            );
        }

        /// <summary>
        /// Calculates the offset used while following the target.
        ///
        /// The horizontal offset is explicitly configured so that the
        /// initial left-side composition is not permanently preserved.
        /// The vertical offset may preserve the framing created in the editor.
        /// </summary>
        private void CalculateRuntimeOffset()
        {
            float verticalOffset = preserveInitialVerticalOffset
                ? transform.position.y - target.position.y
                : followOffset.y;

            runtimeOffset = new Vector2(
                followOffset.x,
                verticalOffset
            );
        }

        /// <summary>
        /// Ensures that the camera begins inside the configured boundaries.
        /// </summary>
        private void ClampInitialPosition()
        {
            Vector3 clampedPosition = transform.position;

            clampedPosition.x = Mathf.Max(
                clampedPosition.x,
                minimumX
            );

            clampedPosition.y = Mathf.Max(
                clampedPosition.y,
                minimumY
            );

            clampedPosition.z = cameraZ;

            transform.position = clampedPosition;
        }

        /// <summary>
        /// Calculates the desired follow position while respecting
        /// the configured horizontal and vertical boundaries.
        /// </summary>
        private Vector2 CalculateDesiredPosition()
        {
            float desiredX =
                target.position.x +
                runtimeOffset.x;

            float desiredY =
                target.position.y +
                runtimeOffset.y;

            desiredX = Mathf.Max(
                desiredX,
                minimumX
            );

            desiredY = Mathf.Max(
                desiredY,
                minimumY
            );

            return new Vector2(
                desiredX,
                desiredY
            );
        }

        /// <summary>
        /// Smoothly follows horizontal movement while limiting both
        /// camera lag and movement beyond the minimum horizontal boundary.
        /// </summary>
        private float CalculateHorizontalPosition(
            float currentX,
            float desiredX
        )
        {
            float smoothedX = Mathf.SmoothDamp(
                currentX,
                desiredX,
                ref horizontalVelocity,
                horizontalSmoothTime,
                maximumFollowSpeed,
                Time.deltaTime
            );

            float lagLimitedX = smoothedX;

            if (maximumHorizontalLag > 0f)
            {
                float minimumAllowedX =
                    desiredX -
                    maximumHorizontalLag;

                float maximumAllowedX =
                    desiredX +
                    maximumHorizontalLag;

                lagLimitedX = Mathf.Clamp(
                    smoothedX,
                    minimumAllowedX,
                    maximumAllowedX
                );
            }

            float boundedX = Mathf.Max(
                lagLimitedX,
                minimumX
            );

            bool isStoppedAtMinimumBoundary =
                Mathf.Approximately(boundedX, minimumX) &&
                desiredX <= minimumX;

            if (isStoppedAtMinimumBoundary)
            {
                horizontalVelocity = 0f;
            }

            return boundedX;
        }

        /// <summary>
        /// Smoothly follows vertical movement while preventing the camera
        /// from moving below the level baseline.
        /// </summary>
        private float CalculateVerticalPosition(
            float currentY,
            float desiredY
        )
        {
            float smoothedY = Mathf.SmoothDamp(
                currentY,
                desiredY,
                ref verticalVelocity,
                verticalSmoothTime,
                maximumFollowSpeed,
                Time.deltaTime
            );

            float boundedY = Mathf.Max(
                smoothedY,
                minimumY
            );

            bool isStoppedAtMinimumBoundary =
                Mathf.Approximately(boundedY, minimumY) &&
                desiredY <= minimumY;

            if (isStoppedAtMinimumBoundary)
            {
                verticalVelocity = 0f;
            }

            return boundedY;
        }

        /// <summary>
        /// Immediately moves the camera to its desired follow position.
        ///
        /// This can be used after respawning, teleporting, or loading
        /// the player at a distant location.
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Vector2 desiredPosition = CalculateDesiredPosition();

            horizontalVelocity = 0f;
            verticalVelocity = 0f;

            transform.position = new Vector3(
                desiredPosition.x,
                desiredPosition.y,
                cameraZ
            );
        }

        private void OnValidate()
        {
            horizontalSmoothTime = Mathf.Max(
                horizontalSmoothTime,
                0.01f
            );

            verticalSmoothTime = Mathf.Max(
                verticalSmoothTime,
                0.01f
            );

            maximumFollowSpeed = Mathf.Max(
                maximumFollowSpeed,
                0.01f
            );

            maximumHorizontalLag = Mathf.Max(
                maximumHorizontalLag,
                0f
            );
        }
    }
}