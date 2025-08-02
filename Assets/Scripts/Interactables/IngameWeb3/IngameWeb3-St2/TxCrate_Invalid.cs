using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Invalid Transaction Crate Component
/// Handles detection and registration with SigVerifyConveyorManager to stop conveyors
/// </summary>
public class TxCrate_Invalid : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebug = false;
    
    // Private variables
    private SigVerifyConveyorManager currentManager = null;
    private bool isRegistered = false;

    /// <summary>
    /// Detect when entering a SigVerifyConveyorManager area
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SigVerifyConveyorManager"))
        {
            SigVerifyConveyorManager manager = other.GetComponent<SigVerifyConveyorManager>();
            if (manager != null && !isRegistered)
            {
                RegisterWithManager(manager);
            }
        }

        if (other.CompareTag("Trash"))
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Detect when leaving a SigVerifyConveyorManager area
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SigVerifyConveyorManager"))
        {
            SigVerifyConveyorManager manager = other.GetComponent<SigVerifyConveyorManager>();
            if (manager != null && manager == currentManager)
            {
                UnregisterFromManager();
            }
        }
    }
    
    /// <summary>
    /// Register this invalid crate with a SigVerifyConveyorManager
    /// </summary>
    private void RegisterWithManager(SigVerifyConveyorManager manager)
    {
        if (currentManager != null)
        {
            // Already registered with another manager, unregister first
            UnregisterFromManager();
        }
        
        currentManager = manager;
        manager.OnInvalidCrateEnter(gameObject);
        isRegistered = true;
        
        if (showDebug)
        {
            Debug.Log($"TxCrate_Invalid: '{gameObject.name}' registered with manager '{manager.name}'");
        }
    }
    
    /// <summary>
    /// Unregister this invalid crate from the current SigVerifyConveyorManager
    /// </summary>
    private void UnregisterFromManager()
    {
        if (currentManager != null)
        {
            currentManager.OnInvalidCrateExit(gameObject);
            
            if (showDebug)
            {
                Debug.Log($"TxCrate_Invalid: '{gameObject.name}' unregistered from manager '{currentManager.name}'");
            }
            
            currentManager = null;
            isRegistered = false;
        }
    }
    
    /// <summary>
    /// Get the current manager this crate is registered with
    /// </summary>
    public SigVerifyConveyorManager GetCurrentManager()
    {
        return currentManager;
    }
    
    /// <summary>
    /// Check if this crate is currently registered with a manager
    /// </summary>
    public bool IsRegistered()
    {
        return isRegistered && currentManager != null;
    }
    
    /// <summary>
    /// Force unregister from any manager (useful for cleanup)
    /// </summary>
    public void ForceUnregister()
    {
        UnregisterFromManager();
    }
    
    void OnDestroy()
    {
        // Clean up registration when object is destroyed
        ForceUnregister();
    }
}
