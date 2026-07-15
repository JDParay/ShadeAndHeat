using UnityEngine;

public class SunDamage : MonoBehaviour
{
    [Header("Sun Burn Settings")]
    [Tooltip("How many extra seconds are drained from the level timer per second Bro spends in the direct sun.")]
    public float sunBurnIntensity = 20f; 

    [Header("State")]
    public bool isExposedToSun = true;
    public bool isDead = false; // Kept for animation/state checks if needed

    private int umbrellaCollidersOverlapping = 0;

    void Update()
    {
        // If the level is already over or Bro is locked, do nothing
        if (LevelManager.Instance != null && LevelManager.Instance.isLevelOver) return;

        // Determine if Bro is safely in the shade or burning
        isExposedToSun = (umbrellaCollidersOverlapping == 0);

        // If exposed to the blazing sun, rapidly drain the LevelManager's timer!
        if (isExposedToSun && LevelManager.Instance != null)
        {
            // Time.deltaTime * sunBurnIntensity means we lose extra seconds every real second
            LevelManager.Instance.timeRemaining -= sunBurnIntensity * Time.deltaTime;
        }
    }

    // Trigger zones look for your custom shadow projection meshes!
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Umbrella"))
        {
            umbrellaCollidersOverlapping++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Umbrella"))
        {
            umbrellaCollidersOverlapping = Mathf.Max(0, umbrellaCollidersOverlapping - 1);
        }
    }
}