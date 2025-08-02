using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Valid Transaction Crate Component
/// Handles detection and registration with SigVerifier systems
/// </summary>
public class TxCrate_Valid : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebug = false;
    
    // Private variables
    private SigVerifier currentVerifier = null;
    private bool isRegistered = false;

    /// <summary>
    /// Detect when entering a SigVerifier area
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SigVerifier"))
        {
            SigVerifier verifier = other.GetComponent<SigVerifier>();
            if (verifier != null && !isRegistered)
            {
                RegisterWithVerifier(verifier);
            }
        }

        if (other.CompareTag("Trash"))
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Detect when leaving a SigVerifier area
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        // No action needed - crates verify once only
        if (showDebug && other.CompareTag("SigVerifier"))
        {
            Debug.Log($"TxCrate_Valid: '{gameObject.name}' left verifier area (no unregister needed)");
        }
    }
    
    /// <summary>
    /// Register this crate with a SigVerifier
    /// </summary>
    private void RegisterWithVerifier(SigVerifier verifier)
    {
        // Only register if not already registered
        if (isRegistered)
        {
            if (showDebug)
            {
                Debug.Log($"TxCrate_Valid: '{gameObject.name}' already registered, ignoring new verifier");
            }
            return;
        }
        
        currentVerifier = verifier;
        verifier.AddCrateToVerification(gameObject);
        isRegistered = true;
        
        if (showDebug)
        {
            Debug.Log($"TxCrate_Valid: '{gameObject.name}' registered with verifier '{verifier.name}'");
        }
    }
    
    /// <summary>
    /// Get the current verifier this crate is registered with
    /// </summary>
    public SigVerifier GetCurrentVerifier()
    {
        return currentVerifier;
    }
    
    /// <summary>
    /// Check if this crate is currently registered with a verifier
    /// </summary>
    public bool IsRegistered()
    {
        return isRegistered && currentVerifier != null;
    }
}
