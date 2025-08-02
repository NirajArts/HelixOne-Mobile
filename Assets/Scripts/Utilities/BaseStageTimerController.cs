using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for stage-specific timer controllers.
/// Inherit from this to create controllers for different stages with varying completion rules.
/// </summary>
public abstract class BaseStageTimerController : MonoBehaviour
{
    [Header("Base Controller Settings")]
    [Tooltip("Reference to the SlotTimer to control")]
    public SlotTimer slotTimer;
    
    [Tooltip("Auto-find SlotTimer if not assigned")]
    public bool autoFindReferences = true;
    
    [Tooltip("Show debug information")]
    public bool showDebug = false;
    
    protected virtual void Start()
    {
        // Auto-find SlotTimer if enabled
        if (autoFindReferences && slotTimer == null)
        {
            slotTimer = FindObjectOfType<SlotTimer>();
        }
        
        // Validate SlotTimer reference
        if (slotTimer == null)
        {
            Debug.LogError($"{GetType().Name}: SlotTimer reference not found! Please assign it in the inspector.");
            return;
        }
        
        // Set up stage-specific logic
        SetupStageLogic();
        
        if (showDebug)
        {
            Debug.Log($"{GetType().Name} initialized successfully");
        }
    }
    
    /// <summary>
    /// Override this method to setup stage-specific timer logic
    /// </summary>
    protected abstract void SetupStageLogic();
    
    /// <summary>
    /// Override this method to define stage-specific completion conditions
    /// </summary>
    /// <returns>True if the stage completion condition is met</returns>
    protected abstract bool CheckStageCompletionCondition();
    
    /// <summary>
    /// Override this method to define what happens when timer completes
    /// </summary>
    /// <returns>True if level should fail when timer completes, false otherwise</returns>
    protected abstract bool HandleStageTimerCompletion();
    
    /// <summary>
    /// Get the current stage progress (override for stage-specific implementation)
    /// </summary>
    /// <returns>Progress from 0 to 1</returns>
    public abstract float GetStageProgress();
    
    /// <summary>
    /// Manually trigger timer completion (for testing or special cases)
    /// </summary>
    public virtual void ForceCompleteTimer()
    {
        if (slotTimer != null)
        {
            slotTimer.ForceCompleteTimer();
        }
    }
    
    /// <summary>
    /// Reset stage-specific data (override for stage-specific implementation)
    /// </summary>
    public abstract void ResetStageData();
    
    protected virtual void OnDestroy()
    {
        // Clean up references to prevent memory leaks
        if (slotTimer != null)
        {
            slotTimer.SetCompletionCondition(null);
            slotTimer.SetTimerCompleteBehavior(null);
        }
    }
}

/// <summary>
/// Interface for stage-specific timer controllers.
/// Implement this interface for consistent stage controller behavior.
/// </summary>
public interface IStageTimerController
{
    /// <summary>
    /// Get the current stage progress
    /// </summary>
    /// <returns>Progress from 0 to 1</returns>
    float GetStageProgress();
    
    /// <summary>
    /// Reset stage-specific data
    /// </summary>
    void ResetStageData();
    
    /// <summary>
    /// Manually trigger timer completion
    /// </summary>
    void ForceCompleteTimer();
}
