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
        [SerializeField] Vector2 LeftGroundedRaycastOffset = new Vector2(0, -1f);
    [Tooltip("Where the raycast for being grounded is")]
        [SerializeField] Vector2 RightGroundedRaycastOffset = new Vector2(0, -1f);
    [Tooltip("The max jump force of the player. An instnat jump uses half this jump height")]
        [SerializeField] float maxJumpForce = 7f;
    [Tooltip("The minimum jump height")]
        [SerializeField] float minJumpForce = 2f;
    [Tooltip("The time it takes to charge a jump (real time). Note: half of this will be used instantly on button press")]
        [SerializeField] float jumpChargeTime = 1.5f;
    [Tooltip("Should the player automatically jump when the jump time runs out")]
        [SerializeField] bool AutoJump = true;
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

    float timeSpentChargingJump = 0;

    public bool canMove = true;

    private void Start()
    {
        refRB = GetComponent<Rigidbody2D>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + LeftGroundedRaycastOffset, 
            (Vector2)transform.position + LeftGroundedRaycastOffset + Vector2.down * 0.1f);
        Gizmos.DrawLine((Vector2)transform.position + RightGroundedRaycastOffset,
            (Vector2)transform.position + RightGroundedRaycastOffset + Vector2.down * 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            HandleHorizontalMovement();
            HandleVerticalMovement();
        }
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
        // Deduct timer if holding space
        if (Input.GetKey(KeyCode.Space))
        {
            JumpMovementTimeLeft -= Time.deltaTime;
        }

        // If input down while grounded
        if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0
            && IsGrounded() && timeSpentChargingJump < jumpChargeTime)
        {
            HandleChargeJumpLogic();
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
            jumpChargeLine.enabled = false;
        }

        // If in air, glide
        if (Input.GetKey(KeyCode.Space) && !IsGrounded() && refRB.linearVelocityY <= -glideFallSpeed)
        {
            refRB.linearVelocityY = -glideFallSpeed;
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
        }
       
        // Update the line renderer for the jump
        jumpChargeLine.enabled = true;
        jumpChargeLine.SetPosition(1, new(0, Mathf.Lerp(0, jumpLineMaxLength, timeSpentChargingJump / jumpChargeTime)));
    }

    void HandleJumpLogic()
    {
        // Lerp the jump force based on how long the player charged the jump
        float jumpForce = Mathf.Lerp(0, maxJumpForce, timeSpentChargingJump / jumpChargeTime);

        jumpForce = Mathf.Max(jumpForce, minJumpForce); // Ensure the jump force is at least the minimum jump force
        // Apply the force
        refRB.linearVelocityY = jumpForce;
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
        RaycastHit2D lHit = Physics2D.Raycast((Vector2)transform.position + LeftGroundedRaycastOffset, Vector2.down, 0.1f);
        RaycastHit2D rHit = Physics2D.Raycast((Vector2)transform.position + RightGroundedRaycastOffset, Vector2.down, 0.1f);
        if (lHit.collider != null && lHit.collider.CompareTag("Ground"))
        {
            return true;
        }
        if (rHit.collider != null && rHit.collider.CompareTag("Ground"))
        {
            return true;
        }
        return false;
    }
}
