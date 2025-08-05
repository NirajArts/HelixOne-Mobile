using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SlotTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Stylized duration displayed in UI (milliseconds)")]
    public float slotDuration = 400; // Stylized duration for each slot in milliseconds, actual of 2 minutes. This is the duration that will be displayed in the UI.
    
    [Tooltip("Real-time duration the timer actually runs (seconds)")]
    public float realTimeDuration = 150f; // Real-time duration for each slot in seconds, the game should run.
    
    [Header("UI References")]
    [Tooltip("Reference to Game_UI_Manager for timer display")]
    public Game_UI_Manager gameUIManager;
    
    [Tooltip("UI text component to display the timer (fallback if no Game_UI_Manager)")]
    public TMP_Text timerText; // Reference to the UI text component to display the slotDuration as "400 ms"
    
    [Header("Timer Completion")]
    [Tooltip("Enable external control for timer completion conditions")]
    public bool allowExternalCompletion = true;
    
    [Header("Timer Control")]
    [Tooltip("Whether the timer has started")]
    public bool timeStarted = false;
    
    [Tooltip("Auto-start timer on Start()")]
    public bool autoStart = false;
    
    [Tooltip("Start timer when player enters trigger zone")]
    public bool startOnPlayerTrigger = true;
    
    [Header("Timer Events")]
    [Tooltip("Event triggered when timer completes (early or timeout)")]
    public UnityEvent OnTimerComplete;
    
    [Tooltip("Event triggered when timer times out (only on timeout, not early completion)")]
    public UnityEvent OnTimerTimeout;
    
    [Tooltip("Event triggered when timer completes early (only on early completion)")]
    public UnityEvent OnTimerEarlyComplete;
    
    [Tooltip("Event triggered when timer starts")]
    public UnityEvent OnTimerStart;
    
    [Tooltip("Event triggered every second (for UI updates)")]
    public UnityEvent<float> OnTimerUpdate;
    
    [Header("External Control Events")]
    [Tooltip("Function that checks if timer should complete early (return true to complete)")]
    public System.Func<bool> CheckCompletionCondition;
    
    [Tooltip("Function that determines what happens when timer completes (return true to trigger level failed)")]
    public System.Func<bool> OnTimerCompleteBehavior;
    
    // Private variables
    private float currentRealTime;
    private float currentDisplayTime;
    private bool timerCompleted = false;

    public Collider[] timeTriggerColliders;

    // Add validation in editor
    private void OnValidate()
    {
        // Ensure values are positive
        if (slotDuration <= 0) slotDuration = 10f;
        if (realTimeDuration <= 0) realTimeDuration = 10f;

        Debug.Log($"OnValidate - slotDuration={slotDuration}, realTimeDuration={realTimeDuration}");

        // Force update current values even during play mode
        if (Application.isPlaying && timeStarted)
        {
            // If timer is running, update the current values proportionally
            float progress = GetProgress();
            currentRealTime = realTimeDuration * (1f - progress);
            currentDisplayTime = slotDuration * (1f - progress);
            Debug.Log($"OnValidate during play - Updated values: currentRealTime={currentRealTime}, currentDisplayTime={currentDisplayTime}");
        }
        else
        {
            currentRealTime = realTimeDuration;
            currentDisplayTime = slotDuration;
        }
    }
    void Start()
    {
        // Debug the current inspector values
        Debug.Log($"SlotTimer Start() - Inspector values: slotDuration={slotDuration}, realTimeDuration={realTimeDuration}");
        
        // Initialize timer values
        currentRealTime = realTimeDuration;
        currentDisplayTime = slotDuration;
        timerCompleted = false;
        
        // Update UI initially
        UpdateTimerDisplay();
        
        // Auto-start if enabled
        if (autoStart)
        {
            StartTimer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeStarted && !timerCompleted)
        {
            // Check external completion condition if available
            if (allowExternalCompletion && CheckCompletionCondition != null && CheckCompletionCondition())
            {
                Debug.Log("SlotTimer: External completion condition met - stopping timer");
                CompleteTimerEarly();
                return;
            }
            
            // Update real time
            currentRealTime -= Time.deltaTime;
            
            // Calculate display time proportionally (should decrease as real time decreases)
            float timeProgress = 1f - (currentRealTime / realTimeDuration);
            currentDisplayTime = slotDuration * (currentRealTime / realTimeDuration);
            
            // Update UI
            UpdateTimerDisplay();
            
            // Trigger update event
            OnTimerUpdate?.Invoke(timeProgress);
            
            // Check if timer completed
            if (currentRealTime <= 0f)
            {
                CompleteTimer();
            }
        }
    }
    
    /// <summary>
    /// Force refresh timer values from inspector (useful for debugging)
    /// </summary>
    [ContextMenu("Force Refresh Values")]
    public void ForceRefreshValues()
    {
        Debug.Log($"Force Refresh - slotDuration={slotDuration}, realTimeDuration={realTimeDuration}");
        
        if (!timeStarted)
        {
            currentRealTime = realTimeDuration;
            currentDisplayTime = slotDuration;
            UpdateTimerDisplay();
            Debug.Log($"Values refreshed - currentRealTime={currentRealTime}, currentDisplayTime={currentDisplayTime}");
        }
        else
        {
            Debug.Log("Cannot refresh while timer is running. Stop timer first.");
        }
    }

    /// <summary>
    /// Start the timer
    /// </summary>
    public void StartTimer()
    {
        Debug.Log($"StartTimer called - Before assignment: slotDuration={slotDuration}, realTimeDuration={realTimeDuration}");

        if (!timeStarted && !timerCompleted)
        {
            timeStarted = true;
            currentRealTime = realTimeDuration;
            currentDisplayTime = slotDuration;

            Debug.Log($"StartTimer - After assignment: currentRealTime={currentRealTime}, currentDisplayTime={currentDisplayTime}");

            UpdateTimerDisplay();
            OnTimerStart?.Invoke();

            Debug.Log($"SlotTimer started - Real duration: {realTimeDuration}s, Display duration: {slotDuration}ms");
        }

        foreach (Collider col in timeTriggerColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Stop the timer
    /// </summary>
    public void StopTimer()
    {
        timeStarted = false;
        Debug.Log("SlotTimer stopped");
    }
    
    /// <summary>
    /// Reset the timer to initial values
    /// </summary>
    public void ResetTimer()
    {
        timeStarted = false;
        timerCompleted = false;
        currentRealTime = realTimeDuration;
        currentDisplayTime = slotDuration;
        
        UpdateTimerDisplay();
        Debug.Log("SlotTimer reset");
    }
    
    /// <summary>
    /// Complete the timer early (called when external conditions are met)
    /// </summary>
    public void CompleteTimerEarly()
    {
        timeStarted = false;
        timerCompleted = true;
        
        UpdateTimerDisplay();
        
        // Trigger specific events for early completion
        OnTimerComplete?.Invoke();
        OnTimerEarlyComplete?.Invoke();
        
        Debug.Log("SlotTimer completed early due to external condition!");
    }
    
    /// <summary>
    /// Complete the timer (when time expires)
    /// </summary>
    private void CompleteTimer()
    {
        timeStarted = false;
        timerCompleted = true;
        currentRealTime = 0f;
        currentDisplayTime = 0f;
        
        UpdateTimerDisplay();
        
        // Trigger events for timeout
        OnTimerComplete?.Invoke();
        OnTimerTimeout?.Invoke();
        
        // Check if external behavior is defined for timer completion
        bool shouldTriggerLevelFailed = true;
        if (OnTimerCompleteBehavior != null)
        {
            shouldTriggerLevelFailed = OnTimerCompleteBehavior();
        }
        
        // Default behavior: trigger level failed when timer reaches 0
        if (shouldTriggerLevelFailed && Stage1Manager.Instance != null)
        {
            Debug.Log("SlotTimer completed - triggering level failed!");
            Stage1Manager.Instance.LevelFailed();
        }
        else if (shouldTriggerLevelFailed)
        {
            Debug.LogError("SlotTimer: Stage1Manager.Instance not found! Cannot trigger level failed.");
        }
        
        Debug.Log("SlotTimer completed!");
    }
    
    /// <summary>
    /// Update the timer display text
    /// </summary>
    private void UpdateTimerDisplay()
    {
        string timerDisplayText = $"{currentDisplayTime:F0} ms";
        
        Debug.Log($"UpdateTimerDisplay - currentDisplayTime={currentDisplayTime}, text='{timerDisplayText}'");
        
        // Use Game_UI_Manager if available
        if (gameUIManager != null)
        {
            gameUIManager.UpdateSlotTimer(timerDisplayText);
        }
        
        // Fallback to direct timerText component
        if (timerText != null)
        {
            timerText.text = timerDisplayText;
        }
    }
    
    /// <summary>
    /// Get the current progress as a percentage (0-1)
    /// </summary>
    /// <returns>Progress from 0 to 1</returns>
    public float GetProgress()
    {
        if (realTimeDuration <= 0f) return 1f;
        return 1f - (currentRealTime / realTimeDuration);
    }
    
    /// <summary>
    /// Get remaining real time in seconds
    /// </summary>
    /// <returns>Remaining time in seconds</returns>
    public float GetRemainingRealTime()
    {
        return currentRealTime;
    }
    
    /// <summary>
    /// Get remaining display time in milliseconds
    /// </summary>
    /// <returns>Remaining display time in milliseconds</returns>
    public float GetRemainingDisplayTime()
    {
        return currentDisplayTime;
    }
    
    /// <summary>
    /// Check if timer is currently running
    /// </summary>
    /// <returns>True if timer is running</returns>
    public bool IsRunning()
    {
        return timeStarted && !timerCompleted;
    }
    
    /// <summary>
    /// Check if timer has completed
    /// </summary>
    /// <returns>True if timer has completed</returns>
    public bool IsCompleted()
    {
        return timerCompleted;
    }
    
    /// <summary>
    /// Set external completion condition function
    /// </summary>
    /// <param name="condition">Function that returns true when timer should complete early</param>
    public void SetCompletionCondition(System.Func<bool> condition)
    {
        CheckCompletionCondition = condition;
    }
    
    /// <summary>
    /// Set external behavior for timer completion
    /// </summary>
    /// <param name="behavior">Function that returns true if level should fail when timer completes</param>
    public void SetTimerCompleteBehavior(System.Func<bool> behavior)
    {
        OnTimerCompleteBehavior = behavior;
    }
    
    /// <summary>
    /// Manually complete timer from external source
    /// </summary>
    public void ForceCompleteTimer()
    {
        if (timeStarted && !timerCompleted)
        {
            CompleteTimerEarly();
        }
    }
    
    /// <summary>
    /// Handle player entering trigger zone
    /// </summary>
    /// <param name="other">The collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player and trigger start is enabled
        if (startOnPlayerTrigger && other.CompareTag("Player"))
        {
            if (!timeStarted && !timerCompleted)
            {
                StartTimer();
                Debug.Log($"SlotTimer started by player trigger: {other.name}");
            }
            else if (timerCompleted)
            {
                Debug.Log("SlotTimer already completed, player entered trigger zone");
            }
            else if (timeStarted)
            {
                Debug.Log("SlotTimer already running, player entered trigger zone");
            }
        }
    }
}
