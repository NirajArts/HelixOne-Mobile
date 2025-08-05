using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEndTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Only trigger once per game session")]
    public bool triggerOnce = true;
    
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    [Tooltip("Delay before triggering level complete (seconds)")]
    public float triggerDelay = 0f;
    
    // Private variables
    private bool hasTriggered = false;

    public bool isThisBanking = false;
    Stage2Manager stage2Manager;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure this GameObject has a Collider with IsTrigger enabled
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"StageEndTrigger on {gameObject.name} requires a Collider component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"StageEndTrigger on {gameObject.name} requires the Collider to have 'Is Trigger' enabled!");
        }

        if (showDebug)
        {
            Debug.Log($"StageEndTrigger initialized on {gameObject.name}");
        }
        stage2Manager = FindObjectOfType<Stage2Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Called when another collider enters the trigger zone
    /// </summary>
    /// <param name="other">The collider that entered</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // Prevent multiple triggers if triggerOnce is enabled
            if (triggerOnce && hasTriggered)
            {
                if (showDebug)
                {
                    Debug.Log($"StageEndTrigger: Player entered but already triggered once");
                }
                return;
            }
            
            if (showDebug)
            {
                Debug.Log($"StageEndTrigger: Player {other.name} entered trigger zone");
            }
            
            // Mark as triggered
            hasTriggered = true;
            
            // Trigger level complete with optional delay
            if (triggerDelay > 0f)
            {
                StartCoroutine(TriggerLevelCompleteDelayed());
            }
            else
            {
                TriggerLevelComplete();
            }
        }
    }
    
    /// <summary>
    /// Trigger level complete with delay
    /// </summary>
    /// <returns></returns>
    private IEnumerator TriggerLevelCompleteDelayed()
    {
        if (showDebug)
        {
            Debug.Log($"StageEndTrigger: Waiting {triggerDelay} seconds before completing level");
        }
        
        yield return new WaitForSeconds(triggerDelay);
        TriggerLevelComplete();
    }

    /// <summary>
    /// Call GameManager to complete the level
    /// </summary>
    private void TriggerLevelComplete()
    {
        // Check if GameManager exists
        if (Stage1Manager.Instance != null)
        {
            if (showDebug)
            {
                Debug.Log("StageEndTrigger: Triggering level complete!");
            }

            Stage1Manager.Instance.LevelComplete();
            if (stage2Manager != null) stage2Manager.LevelComplete(); // Ensure Stage2Manager is also notified
        }
        else
        {
            Debug.LogError("StageEndTrigger: Stage1Manager.Instance not found! Cannot trigger level complete.");
        }

        if (isThisBanking && stage2Manager != null)
        {
            stage2Manager.LevelComplete();
        }
    }
    
    /// <summary>
    /// Reset the trigger state (useful for testing)
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        
        if (showDebug)
        {
            Debug.Log("StageEndTrigger: Reset trigger state");
        }
    }
}
