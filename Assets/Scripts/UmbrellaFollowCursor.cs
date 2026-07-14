using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class UmbrellaFollow : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;

    [Header("References")]
    public SunDamage playerSunDamage;

    [Header("Physics Settings")]
    [Tooltip("How fast the umbrella rushes to catch up to the cursor. High values make it snappy.")]
    public float followSpeed = 25f; 

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>(); 

        // CRITICAL Rigidbody Settings for physical collision:
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;                   // Stop it from falling down!
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Prevents fast phasing
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;         // Keeps movement silky smooth
        
        // Hide default cursor
        Cursor.visible = false; 
    }

    void FixedUpdate() // Changed to FixedUpdate for physical velocity updates!
    {
        if (playerSunDamage != null && playerSunDamage.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Mouse.current != null)
        {
            // 1. Get raw mouse position in world space
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0f;

            // 2. Calculate direction and distance from umbrella to the mouse
            Vector2 difference = (Vector2)mouseWorldPosition - rb.position;

            // 3. Apply physical velocity to slide toward the cursor
            // The further away the mouse is, the faster it catches up (clamped to prevent crazy speeds)
            rb.linearVelocity = difference * followSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}