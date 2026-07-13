using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    
    [Header("Climbing")]
    public float climbSpeed = 4f;
    private bool isNearLadder;
    private bool isClimbing;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private SunDamage sunDamageScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (sunDamageScript != null && sunDamageScript.isDead)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            return; 
        }
        // --- NEW INPUT SYSTEM REPLACEMENTS ---

        // Replaces: Input.GetAxisRaw("Horizontal")
        horizontalInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;

        // Replaces: Input.GetAxisRaw("Vertical")
        verticalInput = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput = -1f;

        // Replaces: Input.GetButtonDown("Jump")
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- END OF NEW INPUT ---

        // Toggle Climbing state
        if (isNearLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }
    }

    void FixedUpdate()
{
    // 2. CRUCIAL: Freeze physical forces if dead
    if (sunDamageScript != null && sunDamageScript.isDead)
    {
        rb.linearVelocity = Vector2.zero; // Strip all remaining speed/momentum
        rb.gravityScale = 0f;       // Optional: Stop him from falling if he dies mid-air
        return;                     // Skip the rest of the movement physics
    }

    // Ground Check
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    // Handle Horizontal Movement
    rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

    // Handle Ladder Climbing
    if (isClimbing)
    {
        rb.gravityScale = 0f; 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
    }
    else
    {
        rb.gravityScale = 2f; 
    }
}

    // Detect Ladders
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isNearLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isNearLadder = false;
            isClimbing = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}