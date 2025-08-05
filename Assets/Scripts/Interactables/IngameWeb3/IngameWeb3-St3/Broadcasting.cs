using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broadcasting : MonoBehaviour
{
    [Header("Broadcasting Grid Settings")]
    [Tooltip("Array of Transform positions where crates will be arranged")]
    public Transform[] broadcastingSlots;
    
    [Header("Broadcasting State")]
    [Tooltip("Current number of crates in broadcasting area")]
    public int cratesInBroadcasting = 0;
    
    [Header("Timing Settings")]
    [Tooltip("Delay before adding crate to grid (in seconds)")]
    public float addToGridDelay = 2f;
    
    [Tooltip("Speed at which crates move to broadcasting slots")]
    public float moveToSlotSpeed = 2f;
    
    [Header("Level Completion")]
    [Tooltip("Reference to the SlotTimer to stop when all crates collected")]
    public SlotTimer slotTimer;
    
    [Tooltip("Array of doors to open when all crates are collected")]
    public SigVerifyDoor[] doorsToOpen;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebug = false;
    
    // Track crates waiting to be added to grid
    private List<GameObject> pendingCrates = new List<GameObject>();
    
    // Track crates currently moving to slots
    private List<GameObject> movingCrates = new List<GameObject>();
    
    // Track total pushable crates in scene
    private int totalPushableCrates = 0;
    private bool levelCompleted = false;
    
    void Start()
    {
        // Initialize broadcasting system
        if (showDebug)
        {
            Debug.Log($"Broadcasting: Initialized with {broadcastingSlots.Length} slots available");
        }
        
        // Count total pushable crates in the scene
        CountTotalPushableCrates();
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Count all objects with "Pushable" tag in the scene
    /// </summary>
    private void CountTotalPushableCrates()
    {
        GameObject[] pushableCrates = GameObject.FindGameObjectsWithTag("Pushable");
        totalPushableCrates = pushableCrates.Length;
        
        if (showDebug)
        {
            Debug.Log($"Broadcasting: Found {totalPushableCrates} total pushable crates in scene");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            GameObject crate = other.gameObject;
            
            if (showDebug)
            {
                Debug.Log($"Broadcasting: Crate '{crate.name}' entered broadcasting area");
            }
            
            // Start coroutine to add to grid after delay
            StartCoroutine(AddToGridAfterDelay(crate));
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            GameObject crate = other.gameObject;
            
            // Remove from pending list if it was waiting
            if (pendingCrates.Contains(crate))
            {
                pendingCrates.Remove(crate);
                
                if (showDebug)
                {
                    Debug.Log($"Broadcasting: Crate '{crate.name}' removed from pending list");
                }
            }
            // Don't remove from grid if crate is moving to slot - let it complete the movement
            else if (!movingCrates.Contains(crate))
            {
                // Remove from grid if it was already placed and not moving
                RemoveFromGrid(crate);
            }
        }
    }
    
    /// <summary>
    /// Coroutine to add crate to grid after specified delay
    /// </summary>
    private IEnumerator AddToGridAfterDelay(GameObject crate)
    {
        // Add to pending list
        pendingCrates.Add(crate);
        
        if (showDebug)
        {
            Debug.Log($"Broadcasting: Crate '{crate.name}' waiting {addToGridDelay} seconds before adding to grid");
        }
        
        // Wait for specified delay
        yield return new WaitForSeconds(addToGridDelay);
        
        // Check if crate is still in pending list (wasn't removed during wait)
        if (pendingCrates.Contains(crate) && crate != null)
        {
            // Remove from pending and add to grid
            pendingCrates.Remove(crate);
            AddCrateToBroadcasting(crate);
        }
    }
    
    /// <summary>
    /// Add crate to the broadcasting grid with smooth movement
    /// </summary>
    private void AddCrateToBroadcasting(GameObject crate)
    {
        if (cratesInBroadcasting < broadcastingSlots.Length)
        {
            // Find next available slot
            Transform targetSlot = broadcastingSlots[cratesInBroadcasting];
            
            // Increment count immediately
            cratesInBroadcasting++;
            
            // Start smooth movement to slot
            StartCoroutine(MoveCrateToSlot(crate, targetSlot));
            
            if (showDebug)
            {
                Debug.Log($"Broadcasting: Crate '{crate.name}' starting movement to slot {cratesInBroadcasting - 1}. Total crates: {cratesInBroadcasting}");
            }
            
            // Check if all crates have been collected
            CheckLevelCompletion();
        }
        else
        {
            if (showDebug)
            {
                Debug.LogWarning($"Broadcasting: No available slots for crate '{crate.name}'. All {broadcastingSlots.Length} slots occupied.");
            }
        }
    }
    
    /// <summary>
    /// Check if all pushable crates have been collected and complete level
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (levelCompleted) return; // Already completed
        
        // Check if all crates are collected
        if (cratesInBroadcasting >= totalPushableCrates)
        {
            levelCompleted = true;
            
            // Stop the timer
            if (slotTimer != null)
            {
                slotTimer.StopTimer();
                
                if (showDebug)
                {
                    Debug.Log("Broadcasting: SlotTimer stopped - all crates collected!");
                }
            }
            
            // Open all doors
            if (doorsToOpen != null && doorsToOpen.Length > 0)
            {
                foreach (SigVerifyDoor door in doorsToOpen)
                {
                    if (door != null)
                    {
                        door.ForceDoorState(true);
                    }
                }
                
                if (showDebug)
                {
                    Debug.Log($"Broadcasting: Opened {doorsToOpen.Length} doors - level completed!");
                }
            }
            
            if (showDebug)
            {
                Debug.Log($"Broadcasting: LEVEL COMPLETED! All {totalPushableCrates} crates collected in broadcasting slots!");
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.Log($"Broadcasting: Progress - {cratesInBroadcasting}/{totalPushableCrates} crates collected");
            }
        }
    }
    
    /// <summary>
    /// Smoothly move crate to the target slot
    /// </summary>
    private IEnumerator MoveCrateToSlot(GameObject crate, Transform targetSlot)
    {
        if (crate == null) yield break;
        
        // Add to moving list
        movingCrates.Add(crate);
        
        Vector3 startPosition = crate.transform.position;
        Quaternion startRotation = crate.transform.rotation;
        Vector3 targetPosition = targetSlot.position;
        Quaternion targetRotation = targetSlot.rotation;
        
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = journeyLength / moveToSlotSpeed;
        float elapsedTime = 0;
        
        if (showDebug)
        {
            Debug.Log($"Broadcasting: Crate '{crate.name}' moving to slot. Journey time: {journeyTime:F2} seconds");
        }
        
        // Smooth movement
        while (elapsedTime < journeyTime && crate != null)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            
            // Smooth interpolation
            crate.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            crate.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, fractionOfJourney);
            
            yield return null;
        }
        
        // Ensure final position is exact
        if (crate != null)
        {
            crate.transform.position = targetPosition;
            crate.transform.rotation = targetRotation;
            
            if (showDebug)
            {
                Debug.Log($"Broadcasting: Crate '{crate.name}' reached target slot");
            }
        }
        
        // Remove from moving list
        movingCrates.Remove(crate);
    }
    
    /// <summary>
    /// Remove crate from the broadcasting grid
    /// </summary>
    private void RemoveFromGrid(GameObject crate)
    {
        if (cratesInBroadcasting > 0)
        {
            cratesInBroadcasting--;
            
            if (showDebug)
            {
                Debug.Log($"Broadcasting: Crate '{crate.name}' removed from grid. Remaining crates: {cratesInBroadcasting}");
            }
        }
    }
    
    /// <summary>
    /// Get current number of crates in broadcasting
    /// </summary>
    public int GetCratesCount()
    {
        return cratesInBroadcasting;
    }
    
    /// <summary>
    /// Check if broadcasting area is full
    /// </summary>
    public bool IsBroadcastingFull()
    {
        return cratesInBroadcasting >= broadcastingSlots.Length;
    }
}
