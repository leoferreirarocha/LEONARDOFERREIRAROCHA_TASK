using UnityEngine;

namespace LeonardoTask.Player
{
    /// <summary>
    /// Controls responsive movement for a fast-paced 2D platformer.
    ///
    /// Rigidbody2D remains responsible for gravity and collision resolution,
    /// while this component controls velocity to provide predictable,
    /// responsive, and satisfying character movement.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [Header("References")]

        [Tooltip("Transform used as the origin of the ground detection area.")]
        [SerializeField]
        private Transform groundCheck;

        [Tooltip(
            "Visual object that is flipped when the player changes direction. " +
            "Assign a visual child object instead of the Player root."
        )]
        [SerializeField]
        private Transform visualRoot;

        [Header("Horizontal Movement")]

        [Tooltip("Maximum horizontal speed during normal movement.")]
        [SerializeField, Min(0f)]
        private float walkSpeed = 7f;

        [Tooltip("Maximum horizontal speed while the run action is held.")]
        [SerializeField, Min(0f)]
        private float runSpeed = 10f;

        [Tooltip("Rate at which the player reaches the target speed while grounded.")]
        [SerializeField, Min(0f)]
        private float groundAcceleration = 85f;

        [Tooltip("Rate at which the player slows down after releasing movement input.")]
        [SerializeField, Min(0f)]
        private float groundDeceleration = 35f;

        [Tooltip("Acceleration applied when reversing direction while grounded.")]
        [SerializeField, Min(0f)]
        private float groundTurnAcceleration = 150f;

        [Header("Air Movement")]

        [Tooltip("Horizontal acceleration applied while airborne.")]
        [SerializeField, Min(0f)]
        private float airAcceleration = 35f;

        [Tooltip(
            "Horizontal deceleration applied while airborne. " +
            "Lower values preserve more momentum during jumps."
        )]
        [SerializeField, Min(0f)]
        private float airDeceleration = 4f;

        [Tooltip("Acceleration applied when reversing direction while airborne.")]
        [SerializeField, Min(0f)]
        private float airTurnAcceleration = 55f;

        [Header("Jump")]

        [Tooltip("Initial upward velocity applied when a jump begins.")]
        [SerializeField, Min(0f)]
        private float jumpVelocity = 15f;

        [Tooltip(
            "Percentage of upward velocity preserved when the jump button is released."
        )]
        [SerializeField, Range(0.05f, 1f)]
        private float jumpCutMultiplier = 0.45f;

        [Tooltip(
            "Time window in which the player can still jump after leaving a platform."
        )]
        [SerializeField, Min(0f)]
        private float coyoteTime = 0.12f;

        [Tooltip(
            "Time window in which a jump input remains buffered before landing."
        )]
        [SerializeField, Min(0f)]
        private float jumpBufferTime = 0.12f;

        [Header("Gravity")]

        [Tooltip("Default gravity scale used by the player.")]
        [SerializeField, Min(0f)]
        private float baseGravityScale = 3.3f;

        [Tooltip("Gravity multiplier applied while the player is falling.")]
        [SerializeField, Min(1f)]
        private float fallGravityMultiplier = 2f;

        [Tooltip(
            "Gravity multiplier applied near the highest point of a jump."
        )]
        [SerializeField, Range(0.1f, 1f)]
        private float apexGravityMultiplier = 0.5f;

        [Tooltip(
            "Absolute vertical velocity considered close to the jump apex."
        )]
        [SerializeField, Min(0f)]
        private float apexVelocityThreshold = 1.4f;

        [Tooltip("Maximum downward velocity allowed during a fall.")]
        [SerializeField, Min(0f)]
        private float maximumFallSpeed = 22f;

        [Header("Ground Detection")]

        [Tooltip("Physics layers that should be considered valid ground.")]
        [SerializeField]
        private LayerMask groundLayer;

        [Tooltip("Size of the ground detection box positioned beneath the player.")]
        [SerializeField]
        private Vector2 groundCheckSize = new Vector2(0.55f, 0.12f);

        [Tooltip(
            "Maximum upward velocity at which the player may still be considered grounded."
        )]
        [SerializeField, Min(0f)]
        private float groundedVerticalTolerance = 0.5f;

        [Header("Input")]

        [Tooltip("Ignores small analog input values below this threshold.")]
        [SerializeField, Range(0f, 0.5f)]
        private float inputDeadZone = 0.1f;

        private Rigidbody2D body;
        private PlayerInputReader input;

        private float horizontalInput;
        private float jumpBufferCounter;
        private float coyoteCounter;

        private bool runHeld;
        private bool jumpCutRequested;
        private bool isGrounded;
        private bool facingRight = true;

        /// <summary>
        /// Gets whether the player is currently standing on valid ground.
        /// </summary>
        public bool IsGrounded => isGrounded;

        /// <summary>
        /// Gets whether the player is currently providing movement input
        /// while holding the run action.
        /// </summary>
        public bool IsRunning =>
            runHeld &&
            Mathf.Abs(horizontalInput) > inputDeadZone;

        /// <summary>
        /// Gets the current horizontal Rigidbody2D velocity.
        /// </summary>
        public float HorizontalVelocity =>
            body != null ? body.linearVelocity.x : 0f;

        /// <summary>
        /// Gets the current vertical Rigidbody2D velocity.
        /// </summary>
        public float VerticalVelocity =>
            body != null ? body.linearVelocity.y : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();

            // Prevents the character from rotating after colliding with corners.
            body.freezeRotation = true;

            // Horizontal deceleration is controlled explicitly by this component.
            body.linearDamping = 0f;

            // Smooths visual movement between physics updates.
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Reduces the chance of passing through colliders at high speeds.
            body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            body.gravityScale = baseGravityScale;

            if (visualRoot != null)
            {
                facingRight = visualRoot.localScale.x >= 0f;
            }
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateJumpInput();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
            UpdateCoyoteTimer();

            ApplyHorizontalMovement();
            TryPerformJump();
            ApplyJumpCut();

            UpdateGravity();
            LimitFallSpeed();
        }

        /// <summary>
        /// Reads horizontal input and filters small analog values
        /// according to the configured dead zone.
        /// </summary>
        private void ReadMovementInput()
        {
            float rawHorizontalInput = input.Move.x;

            if (Mathf.Abs(rawHorizontalInput) < inputDeadZone)
            {
                horizontalInput = 0f;
            }
            else
            {
                horizontalInput = Mathf.Clamp(
                    rawHorizontalInput,
                    -1f,
                    1f
                );
            }

            runHeld = input.RunHeld;
        }

        /// <summary>
        /// Stores jump input for a short period of time.
        ///
        /// This allows a jump pressed shortly before landing to be performed
        /// immediately after the player reaches the ground.
        /// </summary>
        private void UpdateJumpInput()
        {
            if (input.JumpPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter = Mathf.Max(
                    jumpBufferCounter - Time.deltaTime,
                    0f
                );
            }

            if (input.JumpReleasedThisFrame)
            {
                // Preserve the request until it can be processed
                // during the next physics update.
                jumpCutRequested = true;
            }
        }

        /// <summary>
        /// Checks whether a collider on a valid ground layer
        /// overlaps the detection area beneath the player.
        /// </summary>
        private void UpdateGroundedState()
        {
            if (groundCheck == null)
            {
                isGrounded = false;
                return;
            }

            Collider2D groundCollider = Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            );

            bool hasGroundBelow = groundCollider != null;

            // Prevents the player from being considered grounded
            // while moving upward immediately after jumping.
            bool isNotMovingUpward =
                body.linearVelocity.y <= groundedVerticalTolerance;

            isGrounded =
                hasGroundBelow &&
                isNotMovingUpward;
        }

        /// <summary>
        /// Maintains a short jump window after the player leaves the ground.
        /// </summary>
        private void UpdateCoyoteTimer()
        {
            if (isGrounded)
            {
                coyoteCounter = coyoteTime;
                return;
            }

            coyoteCounter = Mathf.Max(
                coyoteCounter - Time.fixedDeltaTime,
                0f
            );
        }

        /// <summary>
        /// Moves the current horizontal velocity toward a target velocity.
        ///
        /// Different acceleration rates are used when starting movement,
        /// stopping, reversing direction, or moving while airborne.
        /// </summary>
        private void ApplyHorizontalMovement()
        {
            float currentSpeed = body.linearVelocity.x;

            float maximumSpeed = runHeld
                ? runSpeed
                : walkSpeed;

            float targetSpeed =
                horizontalInput * maximumSpeed;

            bool hasDirectionalInput =
                Mathf.Abs(horizontalInput) > inputDeadZone;

            bool isChangingDirection =
                hasDirectionalInput &&
                Mathf.Abs(currentSpeed) > 0.05f &&
                Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed);

            bool isAboveTargetSpeed =
                hasDirectionalInput &&
                Mathf.Sign(targetSpeed) == Mathf.Sign(currentSpeed) &&
                Mathf.Abs(currentSpeed) > Mathf.Abs(targetSpeed);

            float accelerationRate;

            if (!hasDirectionalInput || isAboveTargetSpeed)
            {
                accelerationRate = isGrounded
                    ? groundDeceleration
                    : airDeceleration;
            }
            else if (isChangingDirection)
            {
                accelerationRate = isGrounded
                    ? groundTurnAcceleration
                    : airTurnAcceleration;
            }
            else
            {
                accelerationRate = isGrounded
                    ? groundAcceleration
                    : airAcceleration;
            }

            float newHorizontalSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                accelerationRate * Time.fixedDeltaTime
            );

            body.linearVelocity = new Vector2(
                newHorizontalSpeed,
                body.linearVelocity.y
            );
        }

        /// <summary>
        /// Performs a jump when a buffered jump input exists and
        /// the player is grounded or within the coyote-time window.
        /// </summary>
        private void TryPerformJump()
        {
            bool hasBufferedJump =
                jumpBufferCounter > 0f;

            bool canUseGroundJump =
                coyoteCounter > 0f;

            if (!hasBufferedJump || !canUseGroundJump)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;

            // Replace only vertical velocity to preserve horizontal momentum.
            velocity.y = jumpVelocity;

            body.linearVelocity = velocity;

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            isGrounded = false;
        }

        /// <summary>
        /// Reduces upward velocity when the jump action is released
        /// before the player reaches the jump apex.
        ///
        /// A quick press produces a shorter jump, while holding
        /// the action produces a full-height jump.
        /// </summary>
        private void ApplyJumpCut()
        {
            if (!jumpCutRequested)
            {
                return;
            }

            if (body.linearVelocity.y > 0f)
            {
                Vector2 velocity = body.linearVelocity;

                velocity.y *= jumpCutMultiplier;

                body.linearVelocity = velocity;
            }

            jumpCutRequested = false;
        }

        /// <summary>
        /// Adjusts gravity according to the current phase of the jump.
        ///
        /// Normal gravity is used while rising, reduced gravity is used
        /// near the apex, and increased gravity is used while falling.
        /// </summary>
        private void UpdateGravity()
        {
            if (isGrounded)
            {
                body.gravityScale = baseGravityScale;
                return;
            }

            float verticalSpeed = body.linearVelocity.y;

            bool isNearJumpApex =
                Mathf.Abs(verticalSpeed) <= apexVelocityThreshold;

            if (isNearJumpApex)
            {
                body.gravityScale =
                    baseGravityScale * apexGravityMultiplier;

                return;
            }

            if (verticalSpeed < 0f)
            {
                body.gravityScale =
                    baseGravityScale * fallGravityMultiplier;

                return;
            }

            body.gravityScale = baseGravityScale;
        }

        /// <summary>
        /// Prevents the player from reaching excessive downward velocity
        /// after falling for an extended period.
        /// </summary>
        private void LimitFallSpeed()
        {
            if (body.linearVelocity.y >= -maximumFallSpeed)
            {
                return;
            }

            body.linearVelocity = new Vector2(
                body.linearVelocity.x,
                -maximumFallSpeed
            );
        }

        /// <summary>
        /// Flips only the visual child object when movement direction changes.
        ///
        /// The Player root, colliders, and GroundCheck remain unchanged.
        /// </summary>
        private void UpdateFacingDirection()
        {
            if (visualRoot == null)
            {
                return;
            }

            if (Mathf.Abs(horizontalInput) <= inputDeadZone)
            {
                return;
            }

            bool shouldFaceRight = horizontalInput > 0f;

            if (shouldFaceRight == facingRight)
            {
                return;
            }

            facingRight = shouldFaceRight;

            Vector3 visualScale = visualRoot.localScale;

            visualScale.x =
                Mathf.Abs(visualScale.x) *
                (facingRight ? 1f : -1f);

            visualRoot.localScale = visualScale;
        }

        private void OnValidate()
        {
            runSpeed = Mathf.Max(runSpeed, walkSpeed);

            groundCheckSize.x = Mathf.Max(
                groundCheckSize.x,
                0.01f
            );

            groundCheckSize.y = Mathf.Max(
                groundCheckSize.y,
                0.01f
            );
        }

        /// <summary>
        /// Draws the ground detection area in the Scene view
        /// while the Player object is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.DrawWireCube(
                groundCheck.position,
                groundCheckSize
            );
        }
    }
}