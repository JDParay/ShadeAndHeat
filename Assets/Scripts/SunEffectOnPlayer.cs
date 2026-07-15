using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SunEffectOnPlayer : MonoBehaviour
{
    private SpriteRenderer playerRenderer;
    private Color originalColor;

    [Header("Visual Tints")]
    [Tooltip("The slight bronze/tan color applied as the heat begins.")]
    public Color tanTint = new Color(0.9f, 0.75f, 0.6f); // Warm, light bronze

    [Tooltip("The final aggressive red tint when time is almost out.")]
    public Color severeBurnTint = new Color(1f, 0.4f, 0.3f); // Scorching red

    [Header("Heat Percentage (Diagnostic)")]
    [Range(0f, 1f)] public float heatProgress; // 0 = safe, 1 = max heat

    void Start()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        
        // Save the player's original sprite color so we can fade back to it later!
        originalColor = playerRenderer.color; 
    }

    void Update()
    {
        if (LevelManager.Instance == null) return;

        // 1. Calculate how much time has passed as a percentage (0-1)
        heatProgress = 1f - (LevelManager.Instance.timeRemaining / LevelManager.Instance.levelTimeLimit);
        heatProgress = Mathf.Clamp01(heatProgress);

        // 2. Linear interpolate (lerp) through the three stages
        Color finalTint;

        if (heatProgress < 0.5f)
        {
            // First 50%: Fade from the original sprite color to the slight 'tan' color
            float t = heatProgress * 2f; // Scale percentage from 0-0.5 up to 0-1
            finalTint = Color.Lerp(originalColor, tanTint, t);
        }
        else
        {
            // Final 50%: Fade from that 'tan' color into the aggressive 'severe burn' red
            float t = (heatProgress - 0.5f) * 2f; // Scale percentage from 0.5-1 down to 0-1
            finalTint = Color.Lerp(tanTint, severeBurnTint, t);
        }

        // 3. Apply the dynamic color tint directly to the main player sprite!
        playerRenderer.color = finalTint;
    }
}