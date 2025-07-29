using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame_tx : MonoBehaviour
{
    [Header("Transaction Type")]
    [Tooltip("This transaction is valid and should be collected")]
    public bool isValidTx = false;
    
    [Tooltip("This transaction is fake and may have negative consequences")]
    public bool isFakeTx = false;
    
    [Tooltip("This transaction is invalid and should be avoided")]
    public bool isInvalidTx = false;

    [Header("Collection Settings")]
    [Tooltip("Should this transaction be destroyed after collection?")]
    public bool destroyOnCollection = true;
    
    [Tooltip("Delay before destroying the object (in seconds)")]
    public float destroyDelay = 0.1f;
    
    [Header("Visual Feedback")]
    [Tooltip("GameObject to disable when collected (optional)")]
    public GameObject visualObject;
    
    [Tooltip("Effect to spawn when collected (optional)")]
    public GameObject collectionEffect;
    
    [Header("Haptic Feedback")]
    [Tooltip("Haptic intensity when transaction is collected")]
    public float hapticIntensity = 0.2f;
    
    // Reference to the transaction manager
    private InGame_TxManager txManager;
    private MobileHaptics mobileHaptics;
    private bool isCollected = false;

    void Start()
    {
        // Find the transaction manager in the scene
        txManager = InGame_TxManager.Instance;
        mobileHaptics = MobileHaptics.Instance;
        
        if (txManager == null)
        {
            Debug.LogError("InGame_TxManager not found in the scene! Please add one to track transactions.");
        }
        
        // Ensure only one transaction type is set
        ValidateTransactionType();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            Debug.Log("Player triggered a transaction.");
            
            // Handle transaction collection based on type
            if (isValidTx)
            {
                HandleValidTransaction();
            }
            else if (isFakeTx)
            {
                HandleFakeTransaction();
            }
            else if (isInvalidTx)
            {
                HandleInvalidTransaction();
            }
            else
            {
                Debug.LogWarning("Transaction has no type set! Please set one of the transaction type booleans.");
                return;
            }
            
            // Handle visual feedback and cleanup
            HandleCollection();
        }
    }
    
    /// <summary>
    /// Handle valid transaction collection
    /// </summary>
    private void HandleValidTransaction()
    {
        Debug.Log("Valid transaction collected!");
        
        if (txManager != null)
        {
            txManager.AddValidTransaction();
        }
        
        // Add any specific valid transaction logic here
        // For example: play positive sound, show green effect, add score, etc.
    }
    
    /// <summary>
    /// Handle fake transaction collection
    /// </summary>
    private void HandleFakeTransaction()
    {
        Debug.Log("Fake transaction collected!");
        
        if (txManager != null)
        {
            txManager.AddFakeTransaction();
        }
        
        // Add any specific fake transaction logic here
        // For example: play warning sound, show red effect, reduce score, etc.
    }
    
    /// <summary>
    /// Handle invalid transaction collection
    /// </summary>
    private void HandleInvalidTransaction()
    {
        Debug.Log("Invalid transaction collected!");
        
        if (txManager != null)
        {
            txManager.AddInvalidTransaction();
        }
        
        // Add any specific invalid transaction logic here
        // For example: play negative sound, show warning effect, penalty, etc.
    }
    
    /// <summary>
    /// Handle visual feedback and object cleanup after collection
    /// </summary>
    private void HandleCollection()
    {
        // Trigger haptic feedback
        if (mobileHaptics != null)
        {
            mobileHaptics.TriggerHaptic(hapticIntensity);
        }
        
        // Disable visual object if specified
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        // Spawn collection effect if specified
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, transform.rotation);
        }
        
        // Destroy the transaction object if specified
        if (destroyOnCollection)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>
    /// Validate that only one transaction type is set
    /// </summary>
    private void ValidateTransactionType()
    {
        int typeCount = 0;
        if (isValidTx) typeCount++;
        if (isFakeTx) typeCount++;
        if (isInvalidTx) typeCount++;
        
        if (typeCount == 0)
        {
            Debug.LogWarning($"{gameObject.name}: No transaction type is set! Please set one of the transaction type booleans.");
        }
        else if (typeCount > 1)
        {
            Debug.LogWarning($"{gameObject.name}: Multiple transaction types are set! Only one should be active.");
        }
    }
    
    /// <summary>
    /// Manually trigger transaction collection (for testing or special cases)
    /// </summary>
    public void ForceCollection()
    {
        if (!isCollected)
        {
            isCollected = true;
            
            if (isValidTx)
                HandleValidTransaction();
            else if (isFakeTx)
                HandleFakeTransaction();
            else if (isInvalidTx)
                HandleInvalidTransaction();
            
            HandleCollection();
        }
    }
}
