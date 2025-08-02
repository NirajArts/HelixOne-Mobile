using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stage1Manager : MonoBehaviour
{
    [Header("Game References")]
    [Tooltip("Reference to the Game UI Manager")]
    public Game_UI_Manager gameUIManager;
    
    [Tooltip("Reference to the Slot Timer")]
    public SlotTimer slotTimer;
    
    [Header("Game State")]
    [Tooltip("Current game state")]
    public GameState currentGameState = GameState.Playing;
    
    [Header("Game Events")]
    [Tooltip("Event triggered when level is completed")]
    public UnityEvent OnLevelComplete;
    
    [Tooltip("Event triggered when level is failed")]
    public UnityEvent OnLevelFailed;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebug = false;
    
    // Game states enum
    public enum GameState
    {
        Playing,
        Completed,
        Failed,
        Paused
    }
    
    // Scene-specific instance (no singleton pattern)
    private static Stage1Manager _instance;
    public static Stage1Manager Instance
    {
        get
        {
            // Only find in current scene, don't create if not found
            if (_instance == null || _instance.gameObject == null)
            {
                _instance = FindObjectOfType<Stage1Manager>();
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Scene-specific instance - no singleton enforcement
        if (_instance == null)
        {
            _instance = this;
            // Removed DontDestroyOnLoad - this manager should be scene-specific
        }
        else if (_instance != this)
        {
            if (showDebug)
            {
                Debug.Log("GameManager: Duplicate instance detected, destroying this one");
            }
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // Auto-find references if not assigned
        if (gameUIManager == null)
        {
            gameUIManager = FindObjectOfType<Game_UI_Manager>();
        }
        
        if (slotTimer == null)
        {
            slotTimer = FindObjectOfType<SlotTimer>();
        }
        
        // Subscribe to slot timer timeout event (only for actual timeout, not early completion)
        if (slotTimer != null)
        {
            slotTimer.OnTimerTimeout.AddListener(OnTimerExpired);
        }
        
        if (showDebug)
        {
            Debug.Log("GameManager initialized successfully");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Additional game logic can be added here if needed
    }
    
    /// <summary>
    /// Call this method when the player completes the level (from trigger)
    /// </summary>
    public void LevelComplete()
    {
        if (currentGameState != GameState.Playing)
        {
            if (showDebug)
            {
                Debug.LogWarning("Level complete called but game is not in playing state");
            }
            return;
        }
        
        currentGameState = GameState.Completed;
        
        // Stop the timer if it's running
        if (slotTimer != null && slotTimer.IsRunning())
        {
            slotTimer.StopTimer();
        }
        
        // Update level complete UI
        if (gameUIManager != null)
        {
            gameUIManager.UpdateLevelCompleteUI();
        }
        
        // Trigger level complete event
        OnLevelComplete?.Invoke();
        
        if (showDebug)
        {
            Debug.Log("Level completed successfully!");
        }
    }
    
    /// <summary>
    /// Call this method when the level fails (timer expired or other conditions)
    /// </summary>
    public void LevelFailed()
    {
        if (currentGameState != GameState.Playing)
        {
            if (showDebug)
            {
                Debug.LogWarning("Level failed called but game is not in playing state");
            }
            return;
        }
        
        currentGameState = GameState.Failed;
        
        // Update level failed UI
        if (gameUIManager != null)
        {
            gameUIManager.UpdateLevelFailedUI();
        }
        
        // Trigger level failed event
        OnLevelFailed?.Invoke();
        
        if (showDebug)
        {
            Debug.Log("Level failed!");
        }
    }
    
    /// <summary>
    /// Called when the slot timer expires
    /// </summary>
    private void OnTimerExpired()
    {
        if (showDebug)
        {
            Debug.Log("Timer expired - triggering level failed");
        }
        
        LevelFailed();
    }
    
    /// <summary>
    /// Reset the game to initial state
    /// </summary>
    public void ResetGame()
    {
        currentGameState = GameState.Playing;
        
        // Reset UI if available
        if (gameUIManager != null)
        {
            gameUIManager.ResetUI();
        }
        
        // Reset timer if available
        if (slotTimer != null)
        {
            slotTimer.ResetTimer();
        }
        
        // Reset transaction manager if available
        if (InGame_TxManager.Instance != null)
        {
            InGame_TxManager.Instance.ResetAllCounters();
        }
        
        if (showDebug)
        {
            Debug.Log("Game reset to initial state");
        }
    }
    
    /// <summary>
    /// Check if the game is currently active/playing
    /// </summary>
    /// <returns>True if game is in playing state</returns>
    public bool IsGameActive()
    {
        return currentGameState == GameState.Playing;
    }
    
    /// <summary>
    /// Get current game state
    /// </summary>
    /// <returns>Current game state</returns>
    public GameState GetGameState()
    {
        return currentGameState;
    }
    
    /// <summary>
    /// Set game state (useful for external systems)
    /// </summary>
    /// <param name="newState">New game state</param>
    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
        
        if (showDebug)
        {
            Debug.Log($"Game state changed to: {newState}");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (slotTimer != null)
        {
            slotTimer.OnTimerTimeout.RemoveListener(OnTimerExpired);
        }
    }
}
