using UnityEngine;
using UnityEngine.InputSystem;

public class UmbrellaFollow : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    public SunDamage playerSunDamage;
    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        
        // Hide the default Windows cursor
        Cursor.visible = false; 
    }

    void Update()
    {
        if (playerSunDamage != null && playerSunDamage.isDead)
    {
        return;
    }
        if (Mouse.current != null)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0f;

            // Instead of transform.position, we use the Rigidbody to move it physically
            rb.MovePosition(mouseWorldPosition);
        }
    }
}