using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 5f;
    [Header("Jumping")]
    [SerializeField] Vector2 GroundedRaycastOffset = new Vector2(0, -1f);
    Rigidbody2D refRB;

    public float LeftMovementTimeLeft = 0f;
    public float RightMovementTimeRight = 0f;
    public float JumpMovementTimeLeft = 0f;
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
    }

    /// <summary>
    /// Handles all movement on the 2d plane for the character, including jumping and moving
    /// </summary>
    void HandleMovement()
    {
        // Horizontal movement
        Vector2 velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.D) && LeftMovementTimeLeft > 0)
        {
            velocity += Vector2.right;
            LeftMovementTimeLeft -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += Vector2.left;
            RightMovementTimeRight -= Time.deltaTime;
        }
        refRB.linearVelocityX = velocity.x * speed;

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            refRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
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
