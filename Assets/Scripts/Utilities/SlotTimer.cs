using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SlotTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Stylized duration displayed in UI (milliseconds)")]
    public float slotDuration = 400f; // Stylized duration for each slot in milliseconds, actual of 2 minutes. This is the duration that will be displayed in the UI.
    
    [Tooltip("Real-time duration the timer actually runs (seconds)")]
    public float realTimeDuration = 120f; // Real-time duration for each slot in seconds, the game should run.
    
    [Header("UI References")]
    [Tooltip("Reference to Game_UI_Manager for timer display")]
    public Game_UI_Manager gameUIManager;
    
    [Tooltip("UI text component to display the timer (fallback if no Game_UI_Manager)")]
    public TMP_Text timerText; // Reference to the UI text component to display the slotDuration as "400 ms"
    
    [Header("Timer Control")]
    [Tooltip("Whether the timer has started")]
    public bool timeStarted = false;
    
    [Tooltip("Auto-start timer on Start()")]
    public bool autoStart = false;
    
    [Tooltip("Start timer when player enters trigger zone")]
    public bool startOnPlayerTrigger = true;
    
    [Header("Timer Events")]
    [Tooltip("Event triggered when timer completes")]
    public UnityEvent OnTimerComplete;
    
    [Tooltip("Event triggered when timer starts")]
    public UnityEvent OnTimerStart;
    
    [Tooltip("Event triggered every second (for UI updates)")]
    public UnityEvent<float> OnTimerUpdate;
    
    // Private variables
    private float currentRealTime;
    private float currentDisplayTime;
    private bool timerCompleted = false;
    void Start()
    {
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
            // Update real time
            currentRealTime -= Time.deltaTime;
            
            // Calculate display time proportionally
            float timeProgress = 1f - (currentRealTime / realTimeDuration);
            currentDisplayTime = slotDuration * (1f - timeProgress);
            
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
    /// Start the timer
    /// </summary>
    public void StartTimer()
    {
        if (!timeStarted && !timerCompleted)
        {
            timeStarted = true;
            currentRealTime = realTimeDuration;
            currentDisplayTime = slotDuration;
            
            UpdateTimerDisplay();
            OnTimerStart?.Invoke();
            
            Debug.Log($"SlotTimer started - Real duration: {realTimeDuration}s, Display duration: {slotDuration}ms");
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
    /// Complete the timer
    /// </summary>
    private void CompleteTimer()
    {
        timeStarted = false;
        timerCompleted = true;
        currentRealTime = 0f;
        currentDisplayTime = 0f;
        
        UpdateTimerDisplay();
        OnTimerComplete?.Invoke();
        
        Debug.Log("SlotTimer completed!");
    }
    
    /// <summary>
    /// Update the timer display text
    /// </summary>
    private void UpdateTimerDisplay()
    {
        string timerDisplayText = $"{currentDisplayTime:F0} ms";
        
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
