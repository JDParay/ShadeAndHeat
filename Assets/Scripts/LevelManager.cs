using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    // Allows other scripts to easily communicate with the LevelManager
    public static LevelManager Instance { get; private set; }

    [Header("Timer Settings")]
    public float timeRemaining = 180f; // 2 minutes total
    private bool isTimerRunning = false;

    [Header("Game States")]
    public bool isLevelOver = false;

    // Script References
    private SunDamage playerSunDamage;
    private LevelUI levelUI;

    void Awake()
    {
        // Set up the Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Automatically find the UI manager and Sun damage logic in the level
        playerSunDamage = FindFirstObjectByType<SunDamage>();
        levelUI = FindFirstObjectByType<LevelUI>();

        isTimerRunning = true;
    }

    void Update()
    {
        if (isLevelOver) return;

        // Run the countdown
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                
                // Send the fresh time remaining numbers to your Thermometer UI script
                if (levelUI != null)
                {
                    levelUI.UpdateTimerText(timeRemaining);
                }
            }
            else
            {
                timeRemaining = 0;
                isTimerRunning = false;
                TriggerTimeOut();
            }
        }
    }

    // Called automatically by the Car script when Bro touches it
    // Inside LevelManager.cs

    public void LevelComplete()
    {
        if (isLevelOver) return;
        isLevelOver = true;
        isTimerRunning = false;

        Debug.Log("Bro reached the goal! Triggering cutscene walk...");

        // 1. Tell the player to start auto-walking to the right
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            movement.StartAutoWalk(1f); // 1f means right, -1f would mean left
        }

        // 2. Show the victory screen UI
        if (levelUI != null)
        {
            levelUI.ShowWinScreen();
        }
    }
    // Called when the thermometer fills up completely
    private void TriggerTimeOut()
    {
        isLevelOver = true;
        Debug.Log("Time ran out! Bro succumbed to the heat.");
        
        LockPlayerActions();

        if (playerSunDamage != null)
        {
            playerSunDamage.isDead = true; 
        }

        StartCoroutine(ResetLevelRoutine());
    }

    // Lock Bro down so he stands still during win/loss scenarios
    private void LockPlayerActions()
    {
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = FindFirstObjectByType<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    private IEnumerator ResetLevelRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Hooked up to the UI "Next Level" button click
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("All levels beaten! Back to Main Menu.");
            SceneManager.LoadScene(0); 
        }
    }
}