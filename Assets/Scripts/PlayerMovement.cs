using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The max speed of the player")]
        [SerializeField] float maxSpeed = 5f;
    [Tooltip("The acceleration of the player")]
        [SerializeField] float acceleration = 10f;
    [Tooltip("The dampening of the acceleration when in mid air (0.5 is half the speed)")]
        [SerializeField] float midAirAccelerationDampen = 0.5f;
    [Tooltip("The deceleration of the player when no input is given")]
        [SerializeField] float deceleration = 10f;

    [Header("Jumping")]
    // TODO: ADD A SECOND RAYCAST
    [Tooltip("Where the raycast for being grounded is")]
        [SerializeField] Vector2 GroundedRaycastOffset = new Vector2(0, -1f);
    [Tooltip("The max jump force of the player. An instnat jump uses half this jump height")]
        [SerializeField] float maxJumpForce = 7f;
    [Tooltip("The base cost of a jump (this is deducted instantly on button press)")]
        [SerializeField] float baseJumpCost = 0.5f;
    [Tooltip("The time it takes to charge a jump (real time). Note: half of this will be used instantly on button press")]
        [SerializeField] float jumpChargeTime = 1.5f;
    [Tooltip("Should the player automatically jump when the jump time runs out")]
        [SerializeField] bool AutoJump = true;
    [Tooltip("The time it takes to start charging the jump after pressing the jump button")]
        [SerializeField] float timeUntilChargeJump = 0.1f;
    [Tooltip("The line renderer for the jump charge")]
        [SerializeField] LineRenderer jumpChargeLine;
    [SerializeField] float jumpLineMaxLength = 1.5f;

    [Header("Glide")]
    [Tooltip("The speed the player should glide at (should be positive)")]
        [SerializeField] float glideFallSpeed = 1f;

    [Header("Movement Time")]
    [SerializeField] TextMeshProUGUI LeftText;
    [SerializeField] TextMeshProUGUI RightText;
    [SerializeField] TextMeshProUGUI JumpText;
    public float LeftMovementTimeLeft = 0f;
    public float RightMovementTimeLeft = 0f;
    public float JumpMovementTimeLeft = 0f;
    Rigidbody2D refRB;

    float timeUntilStartChargeJump = 0;
    float timeSpentChargingJump = 0;
    float jumpChargeTimeLeftWhenStartingJump = 0;

    private void Start()
    {
        refRB = GetComponent<Rigidbody2D>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + GroundedRaycastOffset, 
            (Vector2)transform.position + GroundedRaycastOffset + Vector2.down * 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        HandleHorizontalMovement();
        HandleVerticalMovement();
        UpdateTimeLeft();
    }

    /// <summary>
    /// Handles all movement on the x axis for the player, including acceleration and deceleration
    /// </summary>
    void HandleHorizontalMovement()
    {
        // Horizontal movement
        Vector2 velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.D) && RightMovementTimeLeft > 0)
        {
            float currAccel = IsGrounded() ? acceleration : acceleration * midAirAccelerationDampen;
            velocity += Vector2.right * currAccel;
            RightMovementTimeLeft -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) && LeftMovementTimeLeft > 0)
        {
            float currAccel = IsGrounded() ? acceleration : acceleration * midAirAccelerationDampen;
            velocity += Vector2.left * currAccel;
            LeftMovementTimeLeft -= Time.deltaTime;
        }
        // Accelerate the player
        if (velocity != Vector2.zero)
        {
            refRB.linearVelocity = new Vector2(Mathf.Clamp(refRB.linearVelocity.x + velocity.x * Time.deltaTime, -maxSpeed, maxSpeed), refRB.linearVelocity.y);
        }
        else if (IsGrounded())
        {
            // Decelerate the player when no input is given
            if (refRB.linearVelocity.x > 0)
            {
                refRB.linearVelocity = new Vector2(Mathf.Max(0, refRB.linearVelocity.x - deceleration * Time.deltaTime), refRB.linearVelocity.y);
            }
            else if (refRB.linearVelocity.x < 0)
            {
                refRB.linearVelocity = new Vector2(Mathf.Min(0, refRB.linearVelocity.x + deceleration * Time.deltaTime), refRB.linearVelocity.y);
            }
        }
    }

    /// <summary>
    /// Handles all movement on the y axis for the player, including jumping and gliding
    /// </summary>
    void HandleVerticalMovement()
    {
        // If input down while grounded
        if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0
            && IsGrounded() && timeSpentChargingJump < jumpChargeTime)
        { 
            if (timeSpentChargingJump == 0)
            {
                // Deduct jump movement time if this is initial jump
                if (timeUntilStartChargeJump == 0)
                {
                    JumpMovementTimeLeft -= baseJumpCost;
                }
                // Start counting until starting the charge jump
                timeUntilStartChargeJump += Time.deltaTime;
                if (timeUntilStartChargeJump >= timeUntilChargeJump)
                {
                    // This is for rounding errors because deltatime sucks
                    jumpChargeTimeLeftWhenStartingJump = JumpMovementTimeLeft;
                    // Start charging the jump
                    HandleChargeJumpLogic();
                }
            }
            else
            {
                HandleChargeJumpLogic();
            }
        }
        // Launch the jump if input up or time runs out
        if ((Input.GetKeyUp(KeyCode.Space) && IsGrounded())
            || (JumpMovementTimeLeft <= 0 && timeSpentChargingJump > 0 && AutoJump))
        {
            HandleJumpLogic();
        }
        // Reset charge if falling off a ledge
        if (!IsGrounded())
        {
            timeSpentChargingJump = 0;
            timeUntilStartChargeJump = 0;
            jumpChargeLine.enabled = false;
        }

        // If in air, glide
        if (Input.GetKey(KeyCode.Space) && !IsGrounded() && refRB.linearVelocityY <= -glideFallSpeed)
        {
            refRB.linearVelocityY = -glideFallSpeed;
            JumpMovementTimeLeft -= Time.deltaTime;
        }
    }

    void HandleChargeJumpLogic()
    {
        // Charge the jump
        timeSpentChargingJump += Time.deltaTime;
        // Reduce time for jump
        if (timeSpentChargingJump > jumpChargeTime)
        {
            // Reset to max charge time
            timeSpentChargingJump = jumpChargeTime;
            // Adjust jumpmovementtimeleft to account for deltatime differences
            JumpMovementTimeLeft = jumpChargeTimeLeftWhenStartingJump - jumpChargeTime;
        }
        else
        {
            // Straight reduce otherwise
            JumpMovementTimeLeft -= Time.deltaTime;
        }
        // Update the line renderer for the jump
        jumpChargeLine.enabled = true;
        jumpChargeLine.SetPosition(1, new(0, Mathf.Lerp(0, jumpLineMaxLength, timeSpentChargingJump / jumpChargeTime)));
    }

    void HandleJumpLogic()
    {
        // Lerp the jump force based on how long the player charged the jump
        float jumpForce = Mathf.Lerp(0, maxJumpForce/2, timeSpentChargingJump / jumpChargeTime);
        // Add half of the max jump force to the jump force to ensure a minimum jump height
        jumpForce += maxJumpForce / 2;
        // Apply the force
        refRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        // Reset variables
        timeSpentChargingJump = 0;
        jumpChargeLine.enabled = false;
    }
    
    void UpdateTimeLeft()
    {
        LeftText.text = Mathf.Max(0, LeftMovementTimeLeft).ToString("F2");
        RightText.text = Mathf.Max(0, RightMovementTimeLeft).ToString("F2");
        JumpText.text = Mathf.Max(0, JumpMovementTimeLeft).ToString("F2");
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + GroundedRaycastOffset, Vector2.down, 0.1f);
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            return true;
        }
        return false;
    }
}
