using UnityEngine;
using UnityEngine.UI; // Required for using standard UI Images

public class LevelUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image thermometerFillBar;   // Drag your 'ThermometerFill' Image here
    public GameObject winScreenPanel;  // Drag your "Level Cleared" popup menu panel here

    [Header("Thermometer Colors")]
    public Color coldColor = new Color(1f, 0.6f, 0f); // Warm Orange at start
    public Color hotColor = Color.red;                // Burning Red at the end

    private float maxTime = 120f; // Stores the level time cap

    void Start()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        if (LevelManager.Instance != null)
        {
            maxTime = LevelManager.Instance.timeRemaining;
        }
    }

    // This replaces the old UpdateTimerText method entirely
    public void UpdateTimerText(float timeRemaining)
    {
        if (thermometerFillBar == null) return;

        float timeElapsed = maxTime - timeRemaining;

        float fillPercentage = Mathf.Clamp01(timeElapsed / maxTime);

        thermometerFillBar.fillAmount = fillPercentage;

        thermometerFillBar.color = Color.Lerp(coldColor, hotColor, fillPercentage);
    }

    public void ShowWinScreen()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
            Cursor.visible = true; 
        }
    }

    public void OnNextLevelButtonPressed()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}