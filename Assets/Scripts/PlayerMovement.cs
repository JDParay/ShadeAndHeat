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

    [Header("Victory Cutscene")]
    private bool isAutoWalking = false;
    private float autoWalkDirection = 1f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private SunDamage sunDamageScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Automatically find the SunDamage component attached to Bro
        sunDamageScript = GetComponent<SunDamage>();
    }

    void Update()
    {
        if (isAutoWalking) return;

        // --- FIXED DEATH INPUT CHECK ---
        if (sunDamageScript != null && sunDamageScript.isDead)
        {
            // CRITICAL: Force inputs back to zero so momentum cannot stick!
            horizontalInput = 0f;
            verticalInput = 0f;

            // PLACEHOLDER FOR ANIMATION:
            // When you add animations later, this is where you force the death stance!
            // myAnimator.SetBool("IsDead", true);
            // myAnimator.SetFloat("Speed", 0f);

            return; 
        }

        // --- INPUT SYSTEM REPLACEMENTS ---
        horizontalInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;

        verticalInput = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput = -1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Toggle Climbing state
        if (isNearLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }
    }

    void FixedUpdate()
{
    // --- AIRTIGHT DEATH LOCK ---
    if (sunDamageScript != null && sunDamageScript.isDead)
    {
        horizontalInput = 0f;
        verticalInput = 0f;
        
        rb.linearVelocity = Vector2.zero; 
        
        // Switch to Static instead! This acts like an anchor, freezing his position 
        // completely in place mid-air, but still allows animations to play.
        rb.bodyType = RigidbodyType2D.Static; 
        return; 
    }

    if (isAutoWalking)
    {
        rb.linearVelocity = new Vector2(autoWalkDirection * (moveSpeed * 0.8f), rb.linearVelocity.y);
        return; 
    }

    // Ground Check
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    // Handle Ladder Climbing / Standard Movement
    if (isClimbing)
    {
        rb.gravityScale = 0f; 
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, verticalInput * climbSpeed);
    }
    else
    {
        rb.gravityScale = 2f; 
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}

    public void StartAutoWalk(float direction)
    {
        isAutoWalking = true;
        autoWalkDirection = direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder")) isNearLadder = true;
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