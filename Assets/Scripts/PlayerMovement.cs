using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The max speed of the player")]
        [SerializeField] float maxSpeed = 5f;
    [Tooltip("The acceleration of the player")]
        [SerializeField] float acceleration = 10f;
    [Tooltip("The deceleration of the player when no input is given")]
        [SerializeField] float deceleration = 10f;
    [Tooltip("Whether or not to flip the player's sprite depending on which way they are going")]
        [SerializeField] bool flipSprite = true;

    [Header("Grounded Status")]
    [Tooltip("Where the raycast for being grounded is")]
    [SerializeField] Vector2 LeftGroundedRaycastOffset = new Vector2(0, -1f);
    [Tooltip("Where the raycast for being grounded is")]
    [SerializeField] Vector2 RightGroundedRaycastOffset = new Vector2(0, -1f);

    [Header("Swimming")]
    [Tooltip("The speed the player should swim at (should be positive)")]
        [SerializeField] float maxSwimSpeed = 2f;
    [Tooltip("The acceleration of the player while swimming")]
        [SerializeField] float swimAcceleration = 5f;
    [Tooltip("The veloicty the player gets when jumping off the ground")]
        [SerializeField] float swimImpulse = 10f;
    [Tooltip("The time it takes to interpolate to the max swim speed from the impulse")]
        [SerializeField] float SwimSpeedChangeTime = 0.5f;
    [Tooltip("The dampening of the acceleration when in mid air (0.5 is half the speed)")]
        [SerializeField] float midAirAccelerationDampen = 0.5f;
    [Tooltip("The dampening of the deceleration when in mid air (0.5 is half the speed)")]
        [SerializeField] float midAirDecelerationDampen = 0.5f;

    [Header("Death")]
    [Tooltip("The time it takes for the player to respawn after death")]
        [SerializeField] float respawnTime = 1f;

    [Header("Win")]
    [Tooltip("Time unitl the level changes when reaching the goal")]
        [SerializeField] float timeUntilLevelChangeOnWin = 1f;

    [Header("Grabbing")]
    [Tooltip("The left offset of the object")]
        [SerializeField] Vector2 leftGrabOffset = new Vector2(-1f, 0f);
    [Tooltip("The right offset of the object")]
        [SerializeField] Vector2 rightGrabOffset = new Vector2(1f, 0f);

    [Header("Movement Time")]
    [SerializeField] TextMeshProUGUI LeftText;
    [SerializeField] TextMeshProUGUI RightText;
    [SerializeField] TextMeshProUGUI JumpText;
    public float LeftMovementTimeLeft = 0f;
    public float RightMovementTimeLeft = 0f;
    public float JumpMovementTimeLeft = 0f;
    Rigidbody2D refRB;
    SpriteRenderer refRenderer;
    GameObject heldObject;

    // float timeSpentChargingJump = 0;
    [SerializeField] float currentSwimSpeedCap = 3f;
    float timeSpentSwimming = 0f;

    public bool canMove = true;
    public bool IsHoldingObject { get; private set; } = false;

    private void Start()
    {
        refRB = GetComponent<Rigidbody2D>();
        refRenderer = GetComponent<SpriteRenderer>();
        currentSwimSpeedCap = maxSwimSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + LeftGroundedRaycastOffset, 
            (Vector2)transform.position + LeftGroundedRaycastOffset + Vector2.down * 0.1f);
        Gizmos.DrawLine((Vector2)transform.position + RightGroundedRaycastOffset,
            (Vector2)transform.position + RightGroundedRaycastOffset + Vector2.down * 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine((Vector2) transform.position + leftGrabOffset, (Vector2)transform.position + leftGrabOffset + Vector2.up * 0.1f);
        Gizmos.DrawLine((Vector2)transform.position + rightGrabOffset, (Vector2)transform.position + rightGrabOffset + Vector2.up * 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            HandleHorizontalMovement();
            // HandleVerticalMovement();
            HandleSwimLogic();
            CheckForOutOfTime();
            CheckDropItem();
            // Check for r key to reset
            if (Input.GetKeyDown(KeyCode.R))
            {
                Skissue(true);
            }
        }
        UpdateTimeLeft();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            // Kill the player if they touch a hazard
            Skissue(true);
        }
        if (collision.gameObject.CompareTag("Bomb"))
        {
            Skissue(false);
        }
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
        else
        {
            float currDecel = IsGrounded() ? deceleration : deceleration * midAirDecelerationDampen;
            // Decelerate the player when no input is given
            if (refRB.linearVelocity.x > 0)
            {
                refRB.linearVelocity = new Vector2(Mathf.Max(0, refRB.linearVelocity.x - currDecel * Time.deltaTime), refRB.linearVelocity.y);
            }
            else if (refRB.linearVelocity.x < 0)
            {
                refRB.linearVelocity = new Vector2(Mathf.Min(0, refRB.linearVelocity.x + currDecel * Time.deltaTime), refRB.linearVelocity.y);
            }
        }

        // Rotate the player
        if (flipSprite)
        {
            if (refRB.linearVelocity.x > 0)
            {
                refRenderer.flipX = true;
                if (IsHoldingObject)
                {
                    heldObject.transform.localPosition = rightGrabOffset;
                }
            }
            else if (refRB.linearVelocity.x < 0)
            {
                refRenderer.flipX = false;
                if (IsHoldingObject)
                {
                    heldObject.transform.localPosition = leftGrabOffset;
                }
            }
        }
    }

    void HandleSwimLogic()
    {
        if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0)
        {


            // Check if grounded and give a big impulse instead of swimming if so
            if (IsGrounded())
            {
                timeSpentSwimming = 0;
                refRB.linearVelocity = new(refRB.linearVelocity.x, Mathf.Max(refRB.linearVelocityY + swimImpulse * Time.deltaTime, swimImpulse));
            }
            else
            {
                // Calculate the current swim speed cap
                timeSpentSwimming += Time.deltaTime;
                currentSwimSpeedCap  = Mathf.Lerp(swimImpulse, maxSwimSpeed, timeSpentSwimming / SwimSpeedChangeTime);
                // Apply swim acceleration
                refRB.linearVelocity = new(refRB.linearVelocity.x, Mathf.Min(refRB.linearVelocity.y + swimAcceleration * Time.deltaTime, currentSwimSpeedCap));
            }
            // Reduce time for jump
            JumpMovementTimeLeft -= Time.deltaTime;
        }
    }


    /// <summary>
    /// Updates the time left for each movement type and updates the UI text accordingly. This is temporary
    /// </summary>
    void UpdateTimeLeft()
    {
        LeftText.text = Mathf.Max(0, LeftMovementTimeLeft).ToString("F2");
        RightText.text = Mathf.Max(0, RightMovementTimeLeft).ToString("F2");
        JumpText.text = Mathf.Max(0, JumpMovementTimeLeft).ToString("F2");
    }

    /// <summary>
    /// Checks if the player is grounded by casting two raycasts downwards from the left and right offsets. If either raycast hits a collider with the "Ground" tag, the player is considered grounded.
    /// </summary>
    /// <returns>true if grounded, false otherwise</returns>
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

    /// <summary>
    /// Kills the player
    /// </summary>
    void Skissue(bool stopPlayer)
    {
        // Stop the player from moving
        canMove = false;
        // Do some death animation call here, for now we flip the sprite vertically
        refRenderer.flipY = true;
        // Stop the player's velocity
        if (stopPlayer) refRB.linearVelocity = Vector2.zero;
        // Wait for a few seconds and then respawn the player
        StartCoroutine(WaitOnRespawn());
    }

    /// <summary>
    /// Waits for the respawn time and then resets the level
    /// </summary>
    IEnumerator WaitOnRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        ResetLevel();
    }

    /// <summary>
    /// Resets the level
    /// </summary>
    void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Kills the player if they are out of time
    /// </summary>
    void CheckForOutOfTime()
    {
        if (LeftMovementTimeLeft <= 0 && RightMovementTimeLeft <= 0 && JumpMovementTimeLeft <= 0 && refRB.linearVelocity.magnitude < 0.1f)
        {
            Skissue(true);
        }
    }

    public void GamerWin()
    {
        // No win if player dead
        if (canMove == false)
            return;
        // Wait a bit
        canMove = false;
        refRB.linearVelocity = Vector2.zero;
        refRenderer.flipY = true;
        StartCoroutine(LevelChange());
    }

    void CheckDropItem()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsHoldingObject)
        {
            // Drop the object
            Transform heldObject = this.heldObject.transform;
            heldObject.SetParent(null);
            // Enable the object's collider
            Collider2D objCollider = heldObject.GetComponent<Collider2D>();
            if (objCollider != null)
                objCollider.enabled = true;
            // Enable the object's rigidbody
            Rigidbody2D objRB = heldObject.GetComponent<Rigidbody2D>();
            if (objRB != null)
                objRB.simulated = true;
            // Stop holding it
            IsHoldingObject = false;
            this.heldObject = null;
        }
    }

    public void GrabObject(GameObject obj)
    {
        if (IsHoldingObject)
            return;
        // Make the object a child of the player
        obj.transform.SetParent(transform);
        // Set the object's position based on the direction of the player
        if (refRenderer.flipX)
        {
            obj.transform.localPosition = rightGrabOffset;
        }
        else
        {
            obj.transform.localPosition = leftGrabOffset;
        }
        // Disable the object's collider so it doesn't interfere with the player
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        if (objCollider != null)
            objCollider.enabled = false;
        // Disable the object's rigidbody
        Rigidbody2D objRB = obj.GetComponent<Rigidbody2D>();
        if (objRB != null)
            objRB.simulated = false;
        // Hold it
        IsHoldingObject = true;
        heldObject = obj;
    }

    IEnumerator LevelChange()
    {
        yield return new WaitForSeconds(timeUntilLevelChangeOnWin);
        // Destroy the current time storage object
        AllocatedTimeStorage[] objs = FindObjectsByType<AllocatedTimeStorage>(FindObjectsSortMode.None);
        foreach (AllocatedTimeStorage obj in objs)
        {
            Destroy(obj.gameObject);
        }
        // Change the level here
        FindFirstObjectByType<GameSceneManager>().LoadLevel((int)FindFirstObjectByType<LevelGoal>().LevelIndex + 1);
    }
    
    // This is the old jump behavitor.
    ///// <summary>
    ///// Handles all movement on the y axis for the player, including jumping and gliding
    ///// </summary>
    //void HandleVerticalMovement()
    //{
    //    // Deduct timer if holding space
    //    if (Input.GetKey(KeyCode.Space))
    //    {
    //        JumpMovementTimeLeft -= Time.deltaTime;
    //    }

    //    // If input down while grounded
    //    if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0
    //        && IsGrounded() && timeSpentChargingJump < jumpChargeTime)
    //    {
    //        HandleChargeJumpLogic();
    //    }
    //    // Launch the jump if input up or time runs out
    //    if ((Input.GetKeyUp(KeyCode.Space) && IsGrounded() && JumpMovementTimeLeft >= 0)
    //        || (timeSpentChargingJump > 0 && AutoJump && JumpMovementTimeLeft <= 0))
    //    {
    //        HandleJumpLogic();
    //    }
    //    // Reset charge if falling off a ledge
    //    if (!IsGrounded())
    //    {
    //        timeSpentChargingJump = 0;
    //        jumpChargeLine.enabled = false;
    //    }

    //    // If in air, glide
    //    if (Input.GetKey(KeyCode.Space) && !IsGrounded() && refRB.linearVelocityY <= -glideFallSpeed && JumpMovementTimeLeft > 0)
    //    {
    //        refRB.linearVelocityY = -glideFallSpeed;
    //    }
    //}

    ///// <summary>
    ///// Handles logic for the charge jump system
    ///// </summary>
    //void HandleChargeJumpLogic()
    //{
    //    // Charge the jump
    //    timeSpentChargingJump += Time.deltaTime;
    //    // Reduce time for jump
    //    if (timeSpentChargingJump > jumpChargeTime)
    //    {
    //        // Reset to max charge time
    //        timeSpentChargingJump = jumpChargeTime;
    //    }
       
    //    // Update the line renderer for the jump
    //    jumpChargeLine.enabled = true;
    //    jumpChargeLine.SetPosition(1, new(0, Mathf.Lerp(0, jumpLineMaxLength, timeSpentChargingJump / jumpChargeTime)));
    //}

    ///// <summary>
    ///// Handles logic for jumping the player
    ///// </summary>
    //void HandleJumpLogic()
    //{
    //    // Lerp the jump force based on how long the player charged the jump
    //    float jumpForce = Mathf.Lerp(0, maxJumpForce, timeSpentChargingJump / jumpChargeTime);

    //    jumpForce = Mathf.Max(jumpForce, minJumpForce); // Ensure the jump force is at least the minimum jump force
    //    // Apply the force
    //    refRB.linearVelocityY = jumpForce;
    //    // Reset variables
    //    timeSpentChargingJump = 0;
    //    jumpChargeLine.enabled = false;
    //}
}
