using UnityEngine;

public class WaterLogic : MonoBehaviour
{
    [Header("Damage Settings")]
    public float boilingHeatDamage = 30f; 
    
    [Header("Shade Sensitivity")]
    [Range(0f, 1f)]
    public float requiredCoverage = 0.75f;

    [Header("Visual Indicators")]
    public SpriteRenderer waterRenderer;
    public ParticleSystem boilingParticles; 
    public Color coolColor = new Color(0.2f, 0.6f, 1f, 0.6f); 
    public Color boilingColor = new Color(1f, 0.3f, 0.2f, 0.8f); 

    [Header("State (Diagnostic)")]
    [SerializeField] private bool isBoiling = true;
    [SerializeField] private float currentCoveragePercentage = 0f;

    private Collider2D waterCollider;
    private int shadowLayerMask;

    void Start()
    {
        if (waterRenderer == null) waterRenderer = GetComponent<SpriteRenderer>();
        waterCollider = GetComponent<Collider2D>();
        
        // Get the layer integer for your Umbrella/Shadow layer
        shadowLayerMask = LayerMask.GetMask("Umbrella");
        
        UpdateWaterState();
    }

    void Update()
    {
        CheckPreciseShadowCoverage();

        // Smoothly transition water color depending on state
        Color targetColor = isBoiling ? boilingColor : coolColor;
        waterRenderer.color = Color.Lerp(waterRenderer.color, targetColor, Time.deltaTime * 5f);
    }

    private void CheckPreciseShadowCoverage()
    {
        Bounds bounds = waterCollider.bounds;
        int samplePoints = 4; // Check 4 points across the puddle surface
        int coveredPoints = 0;

        // Loop from the left edge of the puddle to the right edge
        for (int i = 0; i < samplePoints; i++)
        {
            // Calculate a point along the top/surface of the water box
            float t = (float)i / (samplePoints - 1);
            float sampleX = Mathf.Lerp(bounds.min.x, bounds.max.x, t);
            Vector2 samplePosition = new Vector2(sampleX, bounds.max.y - 0.05f); // Slightly below the absolute top surface

            // Ask Unity's physics engine if a shadow collider covers this specific point
            Collider2D shadowHit = Physics2D.OverlapPoint(samplePosition, shadowLayerMask);
            
            if (shadowHit != null)
            {
                coveredPoints++;
            }
        }

        // Calculate actual coverage percentage based on points hit
        currentCoveragePercentage = (float)coveredPoints / samplePoints;

        // Check against your threshold requirement (e.g., 0.75 means 3 out of 4 points must be shaded)
        bool meetsThreshold = currentCoveragePercentage >= requiredCoverage;

        if (isBoiling && meetsThreshold)
        {
            isBoiling = false;
            UpdateWaterState();
        }
        else if (!isBoiling && !meetsThreshold)
        {
            isBoiling = true;
            UpdateWaterState();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Apply thermometer damage if Bro stands in it while boiling
        if (other.CompareTag("Player") && isBoiling)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.timeRemaining -= boilingHeatDamage * Time.deltaTime;
            }
        }
    }

    private void UpdateWaterState()
    {
        if (boilingParticles != null)
        {
            if (isBoiling)
            {
                if (!boilingParticles.isPlaying) boilingParticles.Play();
            }
            else
            {
                if (boilingParticles.isPlaying) boilingParticles.Stop();
            }
        }
    }

    // Visual helper so you can see the check points in your scene view!
    private void OnDrawGizmosSelected()
    {
        if (waterCollider == null) waterCollider = GetComponent<Collider2D>();
        Bounds bounds = waterCollider.bounds;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 4; i++)
        {
            float t = (float)i / 3;
            float sampleX = Mathf.Lerp(bounds.min.x, bounds.max.x, t);
            Vector2 samplePosition = new Vector2(sampleX, bounds.max.y - 0.05f);
            Gizmos.DrawSphere(samplePosition, 0.05f);
        }
    }
}