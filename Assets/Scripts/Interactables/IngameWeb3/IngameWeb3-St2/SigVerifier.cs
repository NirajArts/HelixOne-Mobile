using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Signature Verifier System
/// Tracks transaction crates entering verification area and arranges them
/// Controls door opening when all valid crates are verified
/// </summary>
public class SigVerifier : MonoBehaviour
{
    [Header("Verification Slots")]
    [Tooltip("Transform positions where verified crates will be arranged")]
    public Transform[] verificationSlots;
    
    [Header("Door Control")]
    [Tooltip("Doors to open when all crates are verified")]
    public SigVerifyDoor[] doorsToOpen;
    
    [Header("Crate Spawner Reference")]
    [Tooltip("Reference to get total valid crate count")]
    public St2TransactionCrateSpawner crateSpawner;
    
    [Header("Timer Control")]
    [Tooltip("Slot timer to stop when verification is complete")]
    public SlotTimer slotTimer;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    // Private tracking variables
    private int totalValidCrates = 0;
    private int verifiedCratesCount = 0;
    private List<GameObject> verifiedCrates = new List<GameObject>();
    private bool allCratesVerified = false;
    
    void Start()
    {
        // Get total valid crates from spawner
        if (crateSpawner != null)
        {
            totalValidCrates = GetValidCrateCountFromSpawner();
            if (showDebug)
            {
                Debug.Log($"SigVerifier: Expecting {totalValidCrates} valid crates for verification");
            }
        }
        else
        {
            Debug.LogError("SigVerifier: No crate spawner reference assigned!");
        }
        
        // Validate setup
        if (verificationSlots == null || verificationSlots.Length == 0)
        {
            Debug.LogError("SigVerifier: No verification slots assigned!");
        }
        
        if (doorsToOpen == null || doorsToOpen.Length == 0)
        {
            Debug.LogWarning("SigVerifier: No doors assigned to open on verification completion");
        }
        
        // Auto-find SlotTimer if not assigned
        if (slotTimer == null)
        {
            slotTimer = FindObjectOfType<SlotTimer>();
        }
        
        if (showDebug && slotTimer != null)
        {
            Debug.Log($"SigVerifier: Timer reference found: '{slotTimer.name}'");
        }
    }
    
    /// <summary>
    /// Get the valid crate count from the spawner
    /// </summary>
    private int GetValidCrateCountFromSpawner()
    {
        if (crateSpawner == null) return 0;
        
        // Access the spawner's calculation method indirectly
        int validTransactions = crateSpawner.useTestingMode ? 
            crateSpawner.testValidTransactions : 
            PlayerPrefs.GetInt("ValidTransactionCount", 0);
            
        // Use same calculation as spawner
        return Mathf.CeilToInt((float)validTransactions / crateSpawner.transactionsPerCrate);
    }
    
    /// <summary>
    /// Detect when transaction crates enter the verification area
    /// NOTE: This is now handled by TxCrate_Valid scripts detecting the SigVerifier
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // This method is kept for backwards compatibility but functionality moved to TxCrate_Valid
        if (showDebug)
        {
            Debug.Log($"SigVerifier: Trigger detected for '{other.name}' - functionality handled by TxCrate_Valid");
        }
    }
    
    /// <summary>
    /// Detect when crates exit the verification area
    /// NOTE: This is now handled by TxCrate_Valid scripts
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        // This method is kept for backwards compatibility but functionality moved to TxCrate_Valid
        if (showDebug)
        {
            Debug.Log($"SigVerifier: Trigger exit detected for '{other.name}' - functionality handled by TxCrate_Valid");
        }
    }
    
    /// <summary>
    /// Check if the object is a valid transaction crate
    /// </summary>
    private bool IsValidTransactionCrate(GameObject crate)
    {
        // Check for Pushable tag and TxScrate_Valid component
        if (!crate.CompareTag("Pushable")) return false;
        
        // Check if TxScrate_Valid component exists using reflection to avoid compile errors
        return crate.GetComponent<TxCrate_Valid>() != null;
    }
    
    /// <summary>
    /// Add a crate to verification and arrange it in the next available slot
    /// Called by TxCrate_Valid when crates enter the verification area
    /// </summary>
    public void AddCrateToVerification(GameObject crate)
    {
        if (verifiedCrates.Contains(crate)) return;
        
        if (verifiedCratesCount >= verificationSlots.Length)
        {
            if (showDebug)
            {
                Debug.LogWarning($"SigVerifier: No more verification slots available!");
            }
            return;
        }
        
        if (verifiedCratesCount >= totalValidCrates)
        {
            if (showDebug)
            {
                Debug.LogWarning($"SigVerifier: Already have all {totalValidCrates} expected crates!");
            }
            return;
        }
        
        // Add to tracking
        verifiedCrates.Add(crate);
        
        // Position the crate at the next verification slot
        Transform targetSlot = verificationSlots[verifiedCratesCount];
        crate.transform.position = targetSlot.position;
        crate.transform.rotation = targetSlot.rotation;
        
        verifiedCratesCount++;
        
        if (showDebug)
        {
            Debug.Log($"SigVerifier: Added '{crate.name}' to slot {verifiedCratesCount}. Progress: {verifiedCratesCount}/{totalValidCrates}");
        }
        
        // Check if all crates are verified
        CheckVerificationComplete();
    }
    
    /// <summary>
    /// Check if verification is complete and open doors
    /// </summary>
    private void CheckVerificationComplete()
    {
        if (verifiedCratesCount >= totalValidCrates && !allCratesVerified)
        {
            allCratesVerified = true;
            
            if (showDebug)
            {
                Debug.Log($"SigVerifier: All {totalValidCrates} crates verified! Opening doors...");
            }
            
            // Stop the timer when verification is complete
            if (slotTimer != null && slotTimer.IsRunning())
            {
                slotTimer.StopTimer();
                if (showDebug)
                {
                    Debug.Log("SigVerifier: Timer stopped - verification complete!");
                }
            }
            
            OpenDoors();
        }
    }

    /// <summary>
    /// Open all assigned doors
    /// </summary>
    private void OpenDoors()
    {
        foreach (SigVerifyDoor door in doorsToOpen)
        {
            if (door != null)
            {
                door.ForceDoorState(true);
                if (showDebug)
                {
                    Debug.Log($"SigVerifier: Opened door '{door.name}'");
                }
            }
        }
        
    }
    
    /// <summary>
    /// Close all assigned doors
    /// </summary>
    private void CloseDoors()
    {
        foreach (SigVerifyDoor door in doorsToOpen)
        {
            if (door != null)
            {
                door.ForceDoorState(false);
            }
        }
    }
    
    /// <summary>
    /// Get current verification progress
    /// </summary>
    public float GetVerificationProgress()
    {
        if (totalValidCrates == 0) return 0f;
        return (float)verifiedCratesCount / totalValidCrates;
    }
    
    /// <summary>
    /// Check if all crates are verified
    /// </summary>
    public bool IsVerificationComplete()
    {
        return allCratesVerified;
    }
    
    /// <summary>
    /// Manually reset verification state
    /// </summary>
    public void ResetVerification()
    {
        verifiedCrates.Clear();
        verifiedCratesCount = 0;
        allCratesVerified = false;
        CloseDoors();
        
        if (showDebug)
        {
            Debug.Log("SigVerifier: Verification reset");
        }
    }
}
