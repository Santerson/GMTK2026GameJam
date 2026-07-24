using System;
using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("The physics the player gets while dead")]
        [SerializeField] PhysicsMaterial2D deadPhysicsMaterial;

    [Header("Win")]
    [Tooltip("Time unitl the level changes when reaching the goal")]
        [SerializeField] float timeUntilLevelChangeOnWin = 1f;

    [Header("Grabbing")]
    [Tooltip("The left offset of the object")]
        [SerializeField] Vector2 leftGrabOffset = new Vector2(-1f, 0f);
    [Tooltip("The right offset of the object")]
        [SerializeField] Vector2 rightGrabOffset = new Vector2(1f, 0f);

    [Header("Audio - Timer")]
    [Tooltip("The percentage of time left for a movement type that will trigger the low time tick sound")]
    [SerializeField][Range(0, 1)] float lowTimeThreshold = 0.2f;
    [Tooltip("The interval between tick sounds")]
    [SerializeField] float tickInterval = 1f;
    [Tooltip("The time the tick sound will stop for when the player runs out of a time. This does not override other tick ends.")]
    [SerializeField] float runOutTickStop = 0.5f;
    [SerializeField] AudioSource SFX_NormalTick;
    [SerializeField] AudioSource SFX_LowTick;
    [SerializeField] AudioSource SFX_RunOutTick;

    [Header("Audio - Movement and Death and Such")]
    [Tooltip("How long it takes a repeating sound to play again, these are walk, swim idle, and swim up")]
    [SerializeField] float RepeatTime = 0.5f;
    [SerializeField] AudioSource SFX_Jump;
    [SerializeField] AudioSource SFX_Land;
    [SerializeField] AudioSource SFX_Death;
    [SerializeField] AudioSource SFX_Win;
    [SerializeField] AudioSource SFX_SwimUp;
    [SerializeField] AudioSource SFX_SwimIdle;
    [SerializeField] AudioSource SFX_Walk;
    [SerializeField] AudioSource SFX_Sleep;

    [Header("Movement Time")]
    [SerializeField] TextMeshProUGUI LeftText;
    [SerializeField] TextMeshProUGUI RightText;
    [SerializeField] TextMeshProUGUI JumpText;
    public float LeftMovementTimeLeft = 0f;
    public float RightMovementTimeLeft = 0f;
    public float JumpMovementTimeLeft = 0f;
    float L_timeUntilNextTickSound = 0f;
    float R_timeUntilNextTickSound = 0f;
    float J_timeUntilNextTickSound = 0f;
    float walkingTimeUntilNextSound = 0f;
    float swimmingTimeUntilNextSound = 0f;
    float swimmingUpTimeUntilNextSound = 0f;

    [HideInInspector] public float MaxLeftMovementTime = 0f;
    [HideInInspector] public float MaxRightMovementTime = 0f;
    [HideInInspector] public float MaxJumpMovementTime = 0f;

    Rigidbody2D refRB;
    SpriteRenderer refRenderer;
    GameObject heldObject;
    Animator refAnimator;

    // float timeSpentChargingJump = 0;
    [SerializeField] float currentSwimSpeedCap = 3f;
    float timeSpentSwimming = 0f;
    float tickStoppedTime = 0f;

    public bool canMove = true;
    public bool IsHoldingObject { get; private set; } = false;

    enum AnimState
    {
        Idle,
        Walking,
        Swimming,
        Landing,
        Dying,
        Sleeping,
        EpicDub
    }
    AnimState currentState = AnimState.Idle;

    enum MoveType
    {
        None,
        Left,
        Right,
        Jump
    }
    MoveType AudioPriority = MoveType.None;

    private void Start()
    {
        refRB = GetComponent<Rigidbody2D>();
        refRenderer = GetComponent<SpriteRenderer>();
        refAnimator = GetComponent<Animator>();
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
        if (walkingTimeUntilNextSound > 0) walkingTimeUntilNextSound -= Time.deltaTime;
        if (swimmingTimeUntilNextSound > 0) swimmingTimeUntilNextSound -= Time.deltaTime;
        if (swimmingUpTimeUntilNextSound > 0) swimmingUpTimeUntilNextSound -= Time.deltaTime;
        if (canMove)
        {
            HandleHorizontalMovement();
            // HandleVerticalMovement();
            HandleSwimLogic();
            CheckForOutOfTime();
            CheckDropItem();
            CheckPlayTickSounds();
            // Check for r key to reset
            if (Input.GetKeyDown(KeyCode.R))
            {
                Skissue(true);
            }
        }
        UpdateTimeLeft();
        UpdateAnimState();
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

    void UpdateAnimState()
    {
        if (currentState == AnimState.Dying || currentState == AnimState.Sleeping || currentState == AnimState.EpicDub)
        {
            refAnimator.SetInteger("PlayerState", (int)currentState);
            return;
        }
        if (IsGrounded())
        {
            if (Mathf.Abs(refRB.linearVelocity.x) > 0.1f)
            {
                currentState = AnimState.Walking;
            }
            else
            {
                currentState = AnimState.Idle;
            }
        }
        else
        {
            if (refRB.linearVelocity.y > 0.1f)
            {
                currentState = AnimState.Swimming;
            }
            else
            {
                currentState = AnimState.Landing;
            }
        }
        refAnimator.SetInteger("PlayerState", (int)currentState);
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
            R_timeUntilNextTickSound -= Time.deltaTime;
            if (AudioPriority == MoveType.None)
            {
                AudioPriority = MoveType.Right;
            }
        }
        if (Input.GetKey(KeyCode.A) && LeftMovementTimeLeft > 0)
        {
            float currAccel = IsGrounded() ? acceleration : acceleration * midAirAccelerationDampen;
            velocity += Vector2.left * currAccel;
            LeftMovementTimeLeft -= Time.deltaTime;
            L_timeUntilNextTickSound -= Time.deltaTime;
            if (AudioPriority == MoveType.None)
            {
                AudioPriority = MoveType.Left;
            }
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

        // Play walk sound if moving and grounded
        if (IsGrounded() && Mathf.Abs(refRB.linearVelocity.x) > 0.1f)
        {
            
            if (!SFX_Walk.isPlaying && walkingTimeUntilNextSound <= 0)
            { 
                SFX_Walk.Play();
                walkingTimeUntilNextSound = RepeatTime;
            }
        }
    }

    /// <summary>
    /// Handles logif for swiming
    /// </summary>
    void HandleSwimLogic()
    {
        if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0)
        {
            // Check if grounded and give a big impulse instead of swimming if so
            if (IsGrounded())
            {
                if (!SFX_Jump.isPlaying) SFX_Jump.Play();
                timeSpentSwimming = 0;
                refRB.linearVelocity = new(refRB.linearVelocity.x, Mathf.Max(refRB.linearVelocityY + swimImpulse * Time.deltaTime, swimImpulse));
            }
            else
            {
                if (!SFX_SwimUp.isPlaying && swimmingUpTimeUntilNextSound <= 0)
                {
                    SFX_SwimUp.Play();
                    swimmingUpTimeUntilNextSound = RepeatTime;
                }
                // Calculate the current swim speed cap
                timeSpentSwimming += Time.deltaTime;
                currentSwimSpeedCap  = Mathf.Lerp(swimImpulse, maxSwimSpeed, timeSpentSwimming / SwimSpeedChangeTime);
                // Apply swim acceleration
                refRB.linearVelocity = new(refRB.linearVelocity.x, Mathf.Min(refRB.linearVelocity.y + swimAcceleration * Time.deltaTime, currentSwimSpeedCap));
            }
            // Reduce time for jump
            JumpMovementTimeLeft -= Time.deltaTime;
            J_timeUntilNextTickSound -= Time.deltaTime;
            // Adjust audio priority
            if (AudioPriority == MoveType.None)
            {
                AudioPriority = MoveType.Jump;
            }
        }
        if (IsGrounded() && refRB.linearVelocityY < -0.1f)
        {
            if (!SFX_Land.isPlaying) SFX_Land.Play();
        }
        if (!IsGrounded() && !Input.GetKey(KeyCode.Space))
        {
            if (!SFX_SwimIdle.isPlaying && swimmingTimeUntilNextSound <= 0)
            {
                SFX_SwimIdle.Play();
                swimmingTimeUntilNextSound = RepeatTime;
            }
        }
    }

    /// <summary>
    /// Plays the ticks sounds depending on the time left for each movement type and the input given. Also plays the run out sound if the player is out of time and still holding the key.
    /// </summary>
    void CheckPlayTickSounds()
    {
        //Priority:
        // Lowest tick if it is in the threshold
        // otherwise, most old press

        // See if any movement type is in the low time threshold and set the priority to that if so
        MoveType PlayAudioPriority = AudioPriority;
        if (LeftMovementTimeLeft <= MaxLeftMovementTime * lowTimeThreshold && LeftMovementTimeLeft > 0 && Input.GetKey(KeyCode.A))
        {
            PlayAudioPriority = MoveType.Left;
        }
        else if (RightMovementTimeLeft <= MaxRightMovementTime * lowTimeThreshold && RightMovementTimeLeft > 0 && Input.GetKey(KeyCode.D))
        {
            PlayAudioPriority = MoveType.Right;
        }
        else if (JumpMovementTimeLeft <= MaxJumpMovementTime * lowTimeThreshold && JumpMovementTimeLeft > 0 && Input.GetKey(KeyCode.Space))
        {
            PlayAudioPriority = MoveType.Jump;
        }
        else
        {
            PlayAudioPriority = AudioPriority;
        }

        // Play the tick sounds
        if (LeftMovementTimeLeft > 0 && Input.GetKey(KeyCode.A) && L_timeUntilNextTickSound <= 0 && PlayAudioPriority == MoveType.Left
            && tickStoppedTime <= 0)
        {
            if (LeftMovementTimeLeft <= MaxLeftMovementTime * lowTimeThreshold)
            {
                SFX_LowTick.Play();
            }
            else
            {
                SFX_NormalTick.Play();
            }
            L_timeUntilNextTickSound = tickInterval;
        }
        if (RightMovementTimeLeft > 0 && Input.GetKey(KeyCode.D) && R_timeUntilNextTickSound <= 0 && PlayAudioPriority == MoveType.Right 
            && tickStoppedTime <= 0)
        {
            if (RightMovementTimeLeft <= MaxRightMovementTime * lowTimeThreshold)
            {
                SFX_LowTick.Play();
            }
            else
            {
                SFX_NormalTick.Play();
            }
            R_timeUntilNextTickSound = tickInterval;
        }
        if (JumpMovementTimeLeft > 0 && Input.GetKey(KeyCode.Space) && J_timeUntilNextTickSound <= 0 && PlayAudioPriority == MoveType.Jump
            && tickStoppedTime <= 0)
        {
            if (JumpMovementTimeLeft <= MaxJumpMovementTime * lowTimeThreshold)
            {
                SFX_LowTick.Play();
            }
            else
            {
                SFX_NormalTick.Play();
            }
            J_timeUntilNextTickSound = tickInterval;
        }
        // Reset the tick timers if the player is not holding the key
        if (!Input.GetKey(KeyCode.A))
        {
            L_timeUntilNextTickSound = 0;
        }
        if (!Input.GetKey(KeyCode.D))
        {
            R_timeUntilNextTickSound = 0;
        }
        if (!Input.GetKey(KeyCode.Space))
        {
            J_timeUntilNextTickSound = 0;
        }
        // Check for timers out of time
        if (LeftMovementTimeLeft - Time.deltaTime <= 0 && Input.GetKey(KeyCode.A) && LeftMovementTimeLeft > 0)
        {
            SFX_RunOutTick.Stop();
            SFX_RunOutTick.Play();
            tickStoppedTime = runOutTickStop;
        }
        if (RightMovementTimeLeft - Time.deltaTime <= 0 && Input.GetKey(KeyCode.D) && RightMovementTimeLeft > 0)
        {
            SFX_RunOutTick.Stop();
            SFX_RunOutTick.Play();
            tickStoppedTime = runOutTickStop;
        }
        if (JumpMovementTimeLeft - Time.deltaTime <= 0 && Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0)
        {
            SFX_RunOutTick.Stop();
            SFX_RunOutTick.Play();
            tickStoppedTime = runOutTickStop;
        }
        // Check for keyups for priority
        if (Input.GetKeyUp(KeyCode.A) && AudioPriority == MoveType.Left)
        {
            AudioPriority = MoveType.None;
        }
        if (Input.GetKeyUp(KeyCode.D) && AudioPriority == MoveType.Right)
        {
            AudioPriority = MoveType.None;
        }
        if (Input.GetKeyUp(KeyCode.Space) && AudioPriority == MoveType.Jump)
        {
            AudioPriority = MoveType.None;
        }

        // Subtrack tick stop time
        if (tickStoppedTime > 0)
        {
            tickStoppedTime -= Time.deltaTime;
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
        // if already dead, skip this stuff and just reset and restart the coroutine
        if (currentState != AnimState.Dying && currentState != AnimState.Sleeping)
        {
            // Stop the player from moving
            canMove = false;
            currentState = AnimState.Dying;
            // Apply dead physics
            refRB.sharedMaterial = deadPhysicsMaterial;
            // Do some death animation call here, for now we flip the sprite vertically
            refRenderer.flipY = true;
            // Stop the player's velocity
            if (stopPlayer) refRB.linearVelocity = Vector2.zero;
            // Play death sound
            if (SFX_Death != null)
            {
                SFX_Death.Play();
            }
        }
        else
        {
            StopAllCoroutines();
        }
        // Wait for a few seconds and then respawn the player
        StartCoroutine(WaitOnRespawn());
    }

    void Sleep()
    {
        // Stop the player from moving
        canMove = false;
        // Do some death animation call here, for now we flip the sprite vertically
        refRenderer.flipY = true;
        // Set the state to sleeping
        currentState = AnimState.Sleeping;
        // Stop the player's velocity
        refRB.linearVelocity = Vector2.zero;
        // Play death sound
        if (SFX_Sleep != null)
        {
            SFX_Sleep.Play();
        }
        // Wait for a few seconds and then respawn the player
        StartCoroutine(WaitOnRespawn());
    }

    /// <summary>
    /// Waits for the respawn time and then resets the level
    /// </summary>
    IEnumerator WaitOnRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        // wait while the player has speeeeed
        if (refRB.linearVelocity.magnitude > 0.1f)
        {
            yield return new WaitUntil(() => refRB.linearVelocity.magnitude < 0.1f);
            yield return new WaitForSeconds(0.5f);
        }
        
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
        if (LeftMovementTimeLeft <= 0 && RightMovementTimeLeft <= 0 && JumpMovementTimeLeft <= 0 && refRB.linearVelocity.magnitude < 0.1f && IsGrounded())
        {
            Sleep();
        }
    }

    public void GamerWin()
    {
        // win if player dead
        StopAllCoroutines();
        // Track the camera on the player again
        FindFirstObjectByType<CameraMovement>().SetCamTracking(true);
        // Wait a bit
        canMove = false;
        refRB.linearVelocity = Vector2.zero;
        refRenderer.flipY = true;
        StartCoroutine(LevelChange());
        // drop any held object
        if (IsHoldingObject)
        {
            DropHeldItem();
        }
        // Play win sound
        if (SFX_Win != null)
        {
            SFX_Win.Play();
        }
    }

    void CheckDropItem()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsHoldingObject)
        {
            DropHeldItem();
        }
    }

    private void DropHeldItem()
    {
        // Drop the object
        Transform heldObject = this.heldObject.transform;
        heldObject.GetComponent<GrabbableCrate>().PlayDropSound();
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
