using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SunDamage : MonoBehaviour
{
    [Header("Sun Settings")]
    public Transform sunSource; // Drag your Freeform Light 2D GameObject here!
    public LayerMask umbrellaLayer;
    public float raycastMaxDistance = 50f; 

    [Header("Death Settings")]
    public float delayBeforeReset = 2f; 
    
    [HideInInspector] public bool isDead = false; 

    void Update()
    {
        if (isDead) return;

        if (sunSource != null)
        {
            CheckSunExposure();
        }
        else
        {
            Debug.LogWarning("Please assign the Sun Source Transform in the Inspector!");
        }
    }

    void CheckSunExposure()
    {
        // 1. Calculate the direction from the Sun to the Player
        Vector2 rayOrigin = sunSource.position;
        Vector2 playerPos = transform.position;
        Vector2 sunDirection = (playerPos - rayOrigin).normalized;

        // 2. Cast the ray from the sun directly towards the player
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, sunDirection, raycastMaxDistance, umbrellaLayer);

        // 3. Let's draw it so you can see it perfectly in Scene View
        // If it hits the umbrella before reaching the distance to the player, you are safe!
        float distanceToPlayer = Vector2.Distance(rayOrigin, playerPos);

        if (hit.collider != null && hit.distance < distanceToPlayer)
        {
            // The ray hit the umbrella *before* hitting the player! Safe!
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
            return; 
        }
        else
        {
            // The ray reached the player without being intercepted by the umbrella layer. Burn!
            Debug.DrawRay(rayOrigin, sunDirection * raycastMaxDistance, Color.red);
            StartCoroutine(HandlePlayerDeath());
        }
    }

    IEnumerator HandlePlayerDeath()
    {
        isDead = true;
        
        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null) movementScript.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        yield return new WaitForSeconds(delayBeforeReset);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}