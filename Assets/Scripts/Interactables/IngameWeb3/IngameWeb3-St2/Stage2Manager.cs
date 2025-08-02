using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stage2Manager : MonoBehaviour
{
    [Header("UI Controls")]
    public GameObject grabBoxButton;
    public GameObject dropBoxButton;
    public GrabbableObject pickBoxGrabbableObject; // Reference to the GrabbableObject prefab for the pick box
    
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
            Debug.Log("Stage2Manager initialized successfully");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PickOrPlaceBox()
    {
        if (pickBoxGrabbableObject == null)
            return;

        pickBoxGrabbableObject.GrabWithUIButton();
    }

    public void GrabBoxButton(bool enable)
    {
        if (grabBoxButton != null)
        {
            grabBoxButton.SetActive(enable);
        }
        else
        {
            Debug.LogWarning("grabBoxButton is not assigned in Stage2Manager!");
        }
    }

    public void DropBoxButton(bool enable)
    {
        if (dropBoxButton != null)
        {
            dropBoxButton.SetActive(enable);
        }
        else
        {
            Debug.LogWarning("dropBoxButton is not assigned in Stage2Manager!");
        }
    }

    public void GrabDropBoxButton(bool enable)
    {
        GrabBoxButton(enable);
        DropBoxButton(enable);
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
            Debug.Log("Stage 2 completed successfully!");
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
            Debug.Log("Stage 2 failed!");
        }
    }
    
    /// <summary>
    /// Called when the slot timer expires (timeout event)
    /// </summary>
    private void OnTimerExpired()
    {
        if (showDebug)
        {
            Debug.Log("Timer expired - calling LevelFailed");
        }
        
        LevelFailed();
    }
    
    /// <summary>
    /// Get current game state
    /// </summary>
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
    
    /// <summary>
    /// Set game state manually (for debugging or special cases)
    /// </summary>
    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
        
        if (showDebug)
        {
            Debug.Log($"Game state changed to: {newState}");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (slotTimer != null)
        {
            slotTimer.OnTimerTimeout.RemoveListener(OnTimerExpired);
        }
    }
}
