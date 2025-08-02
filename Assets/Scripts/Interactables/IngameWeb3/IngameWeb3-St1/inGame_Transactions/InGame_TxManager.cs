using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGame_TxManager : MonoBehaviour
{
    [Header("Transaction Counters")]
    [Tooltip("Number of valid transactions collected")]
    public int validTxCount = 0;
    
    [Tooltip("Number of fake transactions collected")]
    public int fakeTxCount = 0;
    
    [Tooltip("Number of invalid transactions collected")]
    public int invalidTxCount = 0;
    
    [Header("Total Transaction Info")]
    [Tooltip("Total number of transactions collected")]
    public int totalTxCount = 0;
    
    [Tooltip("Total number of transactions available to collect (spawned in the world)")]
    public int totalTxToCollect = 0;
    
    [Header("Transaction Events")]
    [Tooltip("Event triggered when a valid transaction is collected")]
    public UnityEvent<int> OnValidTxCollected;
    
    [Tooltip("Event triggered when a fake transaction is collected")]
    public UnityEvent<int> OnFakeTxCollected;
    
    [Tooltip("Event triggered when an invalid transaction is collected")]
    public UnityEvent<int> OnInvalidTxCollected;
    
    [Tooltip("Event triggered when any transaction is collected")]
    public UnityEvent<int> OnTotalTxUpdated;
    
    [Header("Debug Info")]
    [Tooltip("Enable debug logging for transaction collection")]
    public bool enableDebugLogs = true;
    
    [Header("Save Settings")]
    [Tooltip("Automatically save transaction counts to PlayerPrefs")]
    public bool autoSave = true;
    
    [Tooltip("Automatically load transaction counts from PlayerPrefs on start")]
    public bool autoLoad = true;
    
    [Tooltip("Automatically delete transaction data at start")]
    public bool autoDeleteAtStart = true;
    
    // PlayerPrefs keys for saving transaction data
    private const string VALID_TX_KEY = "ValidTxCount";
    private const string FAKE_TX_KEY = "FakeTxCount";
    private const string INVALID_TX_KEY = "InvalidTxCount";
    private const string TOTAL_TX_KEY = "TotalTxCount";
    private const string TOTAL_TX_TO_COLLECT_KEY = "TotalTxToCollect";
    
    // Singleton pattern for easy access (scene-specific)
    private static InGame_TxManager _instance;
    public static InGame_TxManager Instance
    {
        get
        {
            // Check if instance exists and is not destroyed
            if (_instance == null || _instance.gameObject == null)
            {
                _instance = FindObjectOfType<InGame_TxManager>();
                if (_instance == null)
                {
                    GameObject txManagerObj = new GameObject("InGame_TxManager");
                    _instance = txManagerObj.AddComponent<InGame_TxManager>();
                    Debug.LogWarning("InGame_TxManager was created automatically. Consider adding it to the scene manually.");
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Ensure singleton pattern for current scene only
        if (_instance == null)
        {
            _instance = this;
            // Removed DontDestroyOnLoad - this manager should be scene-specific
            
            // Clear PlayerPrefs immediately in Awake if autoDeleteAtStart is enabled
            if (autoDeleteAtStart)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("InGame_TxManager Awake: Clearing saved data due to autoDeleteAtStart=true");
                }
                ClearSavedData();
            }
        }
        else if (_instance != this)
        {
            if (enableDebugLogs)
            {
                Debug.Log("InGame_TxManager: Duplicate instance detected, destroying this one");
            }
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"InGame_TxManager Start: autoLoad={autoLoad}, autoDeleteAtStart={autoDeleteAtStart}");
            Debug.Log($"InGame_TxManager Start: Current counts before any operations - {GetTransactionSummary()}");
        }
        
        // Only load if we didn't already clear in Awake
        if (autoLoad && !autoDeleteAtStart)
        {
            LoadTransactionCounts();
        }
        else if (autoLoad && autoDeleteAtStart)
        {
            if (enableDebugLogs)
            {
                Debug.Log("InGame_TxManager: Skipping autoLoad because autoDeleteAtStart=true (data was cleared in Awake)");
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("InGame_TxManager initialized. Ready to track transactions.");
            Debug.Log($"InGame_TxManager Start: Final counts after initialization - {GetTransactionSummary()}");
        }
    }
    
    /// <summary>
    /// Add a valid transaction to the counter
    /// </summary>
    public void AddValidTransaction()
    {
        validTxCount++;
        totalTxCount++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Valid transaction collected! Total: {validTxCount}");
        }
        
        // Trigger events
        OnValidTxCollected?.Invoke(validTxCount);
        OnTotalTxUpdated?.Invoke(totalTxCount);
        
        // Auto-save if enabled
        if (autoSave)
        {
            SaveTransactionCounts();
        }
    }
    
    /// <summary>
    /// Add a fake transaction to the counter
    /// </summary>
    public void AddFakeTransaction()
    {
        fakeTxCount++;
        totalTxCount++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Fake transaction collected! Total: {fakeTxCount}");
        }
        
        // Trigger events
        OnFakeTxCollected?.Invoke(fakeTxCount);
        OnTotalTxUpdated?.Invoke(totalTxCount);
        
        // Auto-save if enabled
        if (autoSave)
        {
            SaveTransactionCounts();
        }
    }
    
    /// <summary>
    /// Add an invalid transaction to the counter
    /// </summary>
    public void AddInvalidTransaction()
    {
        invalidTxCount++;
        totalTxCount++;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Invalid transaction collected! Total: {invalidTxCount}");
        }
        
        // Trigger events
        OnInvalidTxCollected?.Invoke(invalidTxCount);
        OnTotalTxUpdated?.Invoke(totalTxCount);
        
        // Auto-save if enabled
        if (autoSave)
        {
            SaveTransactionCounts();
        }
    }
    
    /// <summary>
    /// Add spawned transactions to the total available to collect
    /// </summary>
    /// <param name="count">Number of transactions spawned</param>
    public void AddSpawnedTransactions(int count)
    {
        totalTxToCollect += count;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Added {count} spawned transactions. Total available to collect: {totalTxToCollect}");
        }
        
        // Auto-save if enabled
        if (autoSave)
        {
            SaveTransactionCounts();
        }
    }
    
    /// <summary>
    /// Reset all transaction counters to zero
    /// </summary>
    public void ResetAllCounters()
    {
        validTxCount = 0;
        fakeTxCount = 0;
        invalidTxCount = 0;
        totalTxCount = 0;
        totalTxToCollect = 0;
        
        if (enableDebugLogs)
        {
            Debug.Log("All transaction counters have been reset.");
        }
        
        // Trigger events with zero values
        OnValidTxCollected?.Invoke(validTxCount);
        OnFakeTxCollected?.Invoke(fakeTxCount);
        OnInvalidTxCollected?.Invoke(invalidTxCount);
        OnTotalTxUpdated?.Invoke(totalTxCount);
        
        // Auto-save the reset state if enabled
        if (autoSave)
        {
            SaveTransactionCounts();
        }
    }
    
    /// <summary>
    /// Get the total number of all transactions collected
    /// </summary>
    /// <returns>Total transaction count</returns>
    public int GetTotalTransactions()
    {
        return totalTxCount;
    }
    
    /// <summary>
    /// Get transaction counts as a formatted string
    /// </summary>
    /// <returns>Formatted string with all transaction counts</returns>
    public string GetTransactionSummary()
    {
        return $"Transactions - Collected: {totalTxCount} (Valid: {validTxCount}, Fake: {fakeTxCount}, Invalid: {invalidTxCount}), Available: {totalTxToCollect}";
    }
    
    /// <summary>
    /// Check if a specific transaction count has been reached
    /// </summary>
    /// <param name="targetCount">Target count to check</param>
    /// <param name="transactionType">Type of transaction to check ("valid", "fake", "invalid", "total")</param>
    /// <returns>True if target has been reached</returns>
    public bool HasReachedTarget(int targetCount, string transactionType)
    {
        switch (transactionType.ToLower())
        {
            case "valid":
                return validTxCount >= targetCount;
            case "fake":
                return fakeTxCount >= targetCount;
            case "invalid":
                return invalidTxCount >= targetCount;
            case "total":
                return totalTxCount >= targetCount;
            default:
                Debug.LogWarning($"Unknown transaction type: {transactionType}");
                return false;
        }
    }
    
    /// <summary>
    /// Save all transaction counts to PlayerPrefs
    /// </summary>
    public void SaveTransactionCounts()
    {
        PlayerPrefs.SetInt(VALID_TX_KEY, validTxCount);
        PlayerPrefs.SetInt(FAKE_TX_KEY, fakeTxCount);
        PlayerPrefs.SetInt(INVALID_TX_KEY, invalidTxCount);
        PlayerPrefs.SetInt(TOTAL_TX_KEY, totalTxCount);
        PlayerPrefs.SetInt(TOTAL_TX_TO_COLLECT_KEY, totalTxToCollect);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
        {
            Debug.Log($"Transaction counts saved to PlayerPrefs: {GetTransactionSummary()}");
        }
    }
    
    /// <summary>
    /// Load all transaction counts from PlayerPrefs
    /// </summary>
    public void LoadTransactionCounts()
    {
        validTxCount = PlayerPrefs.GetInt(VALID_TX_KEY, 0);
        fakeTxCount = PlayerPrefs.GetInt(FAKE_TX_KEY, 0);
        invalidTxCount = PlayerPrefs.GetInt(INVALID_TX_KEY, 0);
        totalTxCount = PlayerPrefs.GetInt(TOTAL_TX_KEY, 0);
        totalTxToCollect = PlayerPrefs.GetInt(TOTAL_TX_TO_COLLECT_KEY, 0);
        
        // Validate total count matches sum of individual counts
        int calculatedTotal = validTxCount + fakeTxCount + invalidTxCount;
        if (totalTxCount != calculatedTotal)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"Total transaction count mismatch. Correcting from {totalTxCount} to {calculatedTotal}");
            }
            totalTxCount = calculatedTotal;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"Transaction counts loaded from PlayerPrefs: {GetTransactionSummary()}");
        }
    }
    
    /// <summary>
    /// Clear all saved transaction data from PlayerPrefs
    /// </summary>
    public void ClearSavedData()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"InGame_TxManager: Clearing saved data. Current PlayerPrefs state:");
            Debug.Log($"  ValidTx: {PlayerPrefs.GetInt(VALID_TX_KEY, -1)}");
            Debug.Log($"  FakeTx: {PlayerPrefs.GetInt(FAKE_TX_KEY, -1)}");
            Debug.Log($"  InvalidTx: {PlayerPrefs.GetInt(INVALID_TX_KEY, -1)}");
            Debug.Log($"  TotalTx: {PlayerPrefs.GetInt(TOTAL_TX_KEY, -1)}");
            Debug.Log($"  TotalToCollect: {PlayerPrefs.GetInt(TOTAL_TX_TO_COLLECT_KEY, -1)}");
        }
        
        PlayerPrefs.DeleteKey(VALID_TX_KEY);
        PlayerPrefs.DeleteKey(FAKE_TX_KEY);
        PlayerPrefs.DeleteKey(INVALID_TX_KEY);
        PlayerPrefs.DeleteKey(TOTAL_TX_KEY);
        PlayerPrefs.DeleteKey(TOTAL_TX_TO_COLLECT_KEY);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
        {
            Debug.Log("InGame_TxManager: All saved transaction data cleared from PlayerPrefs and saved");
            Debug.Log($"  ValidTx after clear: {PlayerPrefs.GetInt(VALID_TX_KEY, -1)}");
            Debug.Log($"  TotalTx after clear: {PlayerPrefs.GetInt(TOTAL_TX_KEY, -1)}");
        }
    }
    
    /// <summary>
    /// Reset counters and also clear saved data
    /// </summary>
    public void ResetAndClearSavedData()
    {
        ResetAllCounters();
        ClearSavedData();
    }
    
    /// <summary>
    /// Check if there is any saved transaction data
    /// </summary>
    /// <returns>True if saved data exists</returns>
    public bool HasSavedData()
    {
        return PlayerPrefs.HasKey(VALID_TX_KEY) || 
               PlayerPrefs.HasKey(FAKE_TX_KEY) || 
               PlayerPrefs.HasKey(INVALID_TX_KEY) || 
               PlayerPrefs.HasKey(TOTAL_TX_KEY) ||
               PlayerPrefs.HasKey(TOTAL_TX_TO_COLLECT_KEY);
    }
    
    /// <summary>
    /// Clean up singleton reference when destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Clear the static reference if this instance is being destroyed
        if (_instance == this)
        {
            if (enableDebugLogs)
            {
                Debug.Log("InGame_TxManager: Instance being destroyed, clearing static reference");
            }
            _instance = null;
        }
    }
    
    /// <summary>
    /// Force complete reset - clear both memory and PlayerPrefs (for debugging)
    /// </summary>
    [ContextMenu("Force Complete Reset")]
    public void ForceCompleteReset()
    {
        if (enableDebugLogs)
        {
            Debug.Log("InGame_TxManager: Force complete reset triggered");
        }
        
        // Reset memory counters
        ResetAllCounters();
        
        // Clear PlayerPrefs
        ClearSavedData();
        
        if (enableDebugLogs)
        {
            Debug.Log($"InGame_TxManager: Force reset complete. Current state: {GetTransactionSummary()}");
        }
    }
}
