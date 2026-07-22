using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [Header("Jumping")]
    [SerializeField] Vector2 GroundedRaycastOffset = new Vector2(0, -1f);
    [SerializeField] float maxJumpForce = 7f;
    [SerializeField] float jumpChargeTime = 1.5f;
    [Header("Movement Time")]
    [SerializeField] TextMeshProUGUI LeftText;
    [SerializeField] TextMeshProUGUI RightText;
    [SerializeField] TextMeshProUGUI JumpText;

    public float LeftMovementTimeLeft = 0f;
    public float RightMovementTimeLeft = 0f;
    public float JumpMovementTimeLeft = 0f;
    Rigidbody2D refRB;

    float timeChargingJump = 0;

    private void Start()
    {
        refRB = GetComponent<Rigidbody2D>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + GroundedRaycastOffset, (Vector2)transform.position + GroundedRaycastOffset + Vector2.down * 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        UpdateTimeLeft();
    }

    /// <summary>
    /// Handles all movement on the 2d plane for the character, including jumping and moving
    /// </summary>
    void HandleMovement()
    {
        // Horizontal movement
        Vector2 velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.D) && RightMovementTimeLeft > 0)
        {
            velocity += Vector2.right;
            RightMovementTimeLeft -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) && LeftMovementTimeLeft > 0)
        {
            velocity += Vector2.left;
            LeftMovementTimeLeft -= Time.deltaTime;
        }
        // Only change velocity if the player is moving or grounded
        if (velocity != Vector2.zero || IsGrounded())
            refRB.linearVelocityX = velocity.x * speed;

        // Jumping

        // If input down while grounded
        if (Input.GetKey(KeyCode.Space) && JumpMovementTimeLeft > 0
            && IsGrounded() && timeChargingJump < jumpChargeTime)
        {
            timeChargingJump += Time.deltaTime;
            // only decut jump time if charging
            if (timeChargingJump > jumpChargeTime)
                timeChargingJump = jumpChargeTime;
            JumpMovementTimeLeft -= Time.deltaTime;
        }
        if ((Input.GetKeyUp(KeyCode.Space) && IsGrounded())
            || (JumpMovementTimeLeft <= 0 && timeChargingJump > 0))
        {
            float jumpForce = Mathf.Lerp(0, maxJumpForce, timeChargingJump / jumpChargeTime);
            refRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            timeChargingJump = 0;
        }
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
