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
    
    [Header("Visual Control")]
    [Tooltip("Mesh renderers for conveyor visuals")]
    public MeshRenderer[] conveyorRenderers;
    
    [Tooltip("Color to show when invalid crates are detected")]
    public Color invalidCrateColor = Color.red;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    // Private tracking
    private int invalidCratesCount = 0;
    private bool conveyorWasRunning = true;
    private Color[] originalEmissiveColors;
    
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
        
        // Store original emissive colors
        StoreOriginalEmissiveColors();
    }
    
    /// <summary>
    /// Store the original emissive colors of all conveyor renderers
    /// </summary>
    private void StoreOriginalEmissiveColors()
    {
        if (conveyorRenderers != null && conveyorRenderers.Length > 0)
        {
            originalEmissiveColors = new Color[conveyorRenderers.Length];
            
            for (int i = 0; i < conveyorRenderers.Length; i++)
            {
                if (conveyorRenderers[i] != null && conveyorRenderers[i].material != null)
                {
                    originalEmissiveColors[i] = conveyorRenderers[i].material.GetColor("_EmissionColor");
                    
                    if (showDebug)
                    {
                        Debug.Log($"SigVerifyConveyorManager: Stored original emissive color for '{conveyorRenderers[i].name}': {originalEmissiveColors[i]}");
                    }
                }
            }
        }
        else if (showDebug)
        {
            Debug.LogWarning("SigVerifyConveyorManager: No conveyor renderers assigned for visual feedback");
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
            
            // Change conveyor visual to invalid color
            SetConveyorEmissiveColor(invalidCrateColor);
            
            if (showDebug)
            {
                Debug.Log("SigVerifyConveyorManager: Conveyor stopped and turned red due to invalid crate");
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
            
            // Restore original conveyor visual
            RestoreOriginalEmissiveColors();
            
            if (showDebug)
            {
                Debug.Log("SigVerifyConveyorManager: Conveyor resumed and color restored - no invalid crates remaining");
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
    
    /// <summary>
    /// Set the emissive color of all conveyor renderers
    /// </summary>
    private void SetConveyorEmissiveColor(Color color)
    {
        if (conveyorRenderers != null)
        {
            foreach (MeshRenderer renderer in conveyorRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetColor("_EmissionColor", color);
                    renderer.material.EnableKeyword("_EMISSION");
                    
                    if (showDebug)
                    {
                        Debug.Log($"SigVerifyConveyorManager: Set emissive color to {color} for '{renderer.name}'");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Restore the original emissive colors of all conveyor renderers
    /// </summary>
    private void RestoreOriginalEmissiveColors()
    {
        if (conveyorRenderers != null && originalEmissiveColors != null)
        {
            for (int i = 0; i < conveyorRenderers.Length && i < originalEmissiveColors.Length; i++)
            {
                if (conveyorRenderers[i] != null && conveyorRenderers[i].material != null)
                {
                    conveyorRenderers[i].material.SetColor("_EmissionColor", originalEmissiveColors[i]);
                    
                    if (showDebug)
                    {
                        Debug.Log($"SigVerifyConveyorManager: Restored original emissive color for '{conveyorRenderers[i].name}': {originalEmissiveColors[i]}");
                    }
                }
            }
        }
    }
}
