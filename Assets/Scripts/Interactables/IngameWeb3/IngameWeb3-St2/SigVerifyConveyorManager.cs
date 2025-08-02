using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple manager to stop conveyor when invalid crates are present
/// </summary>
public class SigVerifyConveyorManager : MonoBehaviour
{
    [Header("Conveyor Control")]
    [Tooltip("Conveyor to control when invalid crates are detected")]
    public Conveyor targetConveyor;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    // Private tracking
    private int invalidCratesCount = 0;
    private bool conveyorWasRunning = true;
    
    void Start()
    {
        // Validate setup
        if (targetConveyor == null)
        {
            Debug.LogError("SigVerifyConveyorManager: No target conveyor assigned!");
        }
        else
        {
            conveyorWasRunning = targetConveyor.isConveyorRunning;
            if (showDebug)
            {
                Debug.Log($"SigVerifyConveyorManager: Monitoring conveyor '{targetConveyor.name}'");
            }
        }
    }
    
    /// <summary>
    /// Called when an invalid crate enters the area
    /// </summary>
    public void OnInvalidCrateEnter(GameObject invalidCrate)
    {
        invalidCratesCount++;
        
        if (showDebug)
        {
            Debug.Log($"SigVerifyConveyorManager: Invalid crate '{invalidCrate.name}' entered. Count: {invalidCratesCount}");
        }
        
        // Stop conveyor when first invalid crate enters
        if (invalidCratesCount == 1 && targetConveyor != null)
        {
            conveyorWasRunning = targetConveyor.isConveyorRunning;
            targetConveyor.isConveyorRunning = false;
            
            if (showDebug)
            {
                Debug.Log("SigVerifyConveyorManager: Conveyor stopped due to invalid crate");
            }
        }
    }
    
    /// <summary>
    /// Called when an invalid crate exits the area
    /// </summary>
    public void OnInvalidCrateExit(GameObject invalidCrate)
    {
        invalidCratesCount = Mathf.Max(0, invalidCratesCount - 1);
        
        if (showDebug)
        {
            Debug.Log($"SigVerifyConveyorManager: Invalid crate '{invalidCrate.name}' exited. Count: {invalidCratesCount}");
        }
        
        // Resume conveyor when no invalid crates remain
        if (invalidCratesCount == 0 && targetConveyor != null && conveyorWasRunning)
        {
            targetConveyor.isConveyorRunning = true;
            
            if (showDebug)
            {
                Debug.Log("SigVerifyConveyorManager: Conveyor resumed - no invalid crates remaining");
            }
        }
    }
    
    /// <summary>
    /// Get current count of invalid crates in area
    /// </summary>
    public int GetInvalidCrateCount()
    {
        return invalidCratesCount;
    }
    
    /// <summary>
    /// Check if conveyor is stopped due to invalid crates
    /// </summary>
    public bool IsConveyorStoppedByInvalidCrates()
    {
        return invalidCratesCount > 0;
    }
}
