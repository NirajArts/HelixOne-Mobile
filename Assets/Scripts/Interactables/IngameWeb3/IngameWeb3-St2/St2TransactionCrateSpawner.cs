using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stage 2 Transaction Crate Spawner
/// Spawns crates based on previous stage transaction collection results
/// Uses PlayerPrefs data from Stage 1 to determine crate quantities
/// </summary>
public class St2TransactionCrateSpawner : MonoBehaviour
{
    [Header("Crate Prefabs")]
    [Tooltip("Prefab for valid transaction crates")]
    public GameObject validCratePrefab;
    
    [Tooltip("Prefab for invalid transaction crates")]
    public GameObject invalidCratePrefab;
    
    [Tooltip("Prefab for fake transaction crates")]
    public GameObject fakeCratePrefab;
    
    [Header("Spawning Settings")]
    [Tooltip("Number of transactions required to spawn one crate")]
    [Range(1, 50)]
    public int transactionsPerCrate = 20;
    
    [Tooltip("Spawn crates immediately on Start")]
    public bool spawnOnStart = true;
    
    [Header("Grid Layout Settings")]
    [Tooltip("Distance between crates on X axis")]
    public float gridSpacingX = 2.0f;
    
    [Tooltip("Distance between crates on Z axis")]
    public float gridSpacingZ = 2.0f;
    
    [Tooltip("Maximum crates per row before wrapping to next row")]
    [Range(1, 20)]
    public int maxCratesPerRow = 5;
    
    [Tooltip("Height offset for spawned crates")]
    public float spawnHeight = 0.5f;
    
    [Header("Testing Mode")]
    [Tooltip("Use testing values instead of PlayerPrefs data")]
    public bool useTestingMode = false;
    
    [Space(10)]
    [Tooltip("Test value for valid transactions (testing mode only)")]
    public int testValidTransactions = 42;
    
    [Tooltip("Test value for invalid transactions (testing mode only)")]
    public int testInvalidTransactions = 18;
    
    [Tooltip("Test value for fake transactions (testing mode only)")]
    public int testFakeTransactions = 25;
    
    [Header("PlayerPrefs Keys")]
    [Tooltip("PlayerPrefs key for valid transaction count")]
    public string validTxKey = "ValidTxCount";
    
    [Tooltip("PlayerPrefs key for invalid transaction count")]
    public string invalidTxKey = "InvalidTxCount";
    
    [Tooltip("PlayerPrefs key for fake transaction count")]
    public string fakeTxKey = "FakeTxCount";
    
    [Header("Spawn Tracking")]
    [Tooltip("Show debug information about spawning")]
    public bool showDebugInfo = true;
    
    // Public read-only properties for spawned crate counts
    [Header("Spawned Crate Counts (Read Only)")]
    [SerializeField] private int spawnedValidCrates = 0;
    [SerializeField] private int spawnedInvalidCrates = 0;
    [SerializeField] private int spawnedFakeCrates = 0;
    [SerializeField] private int totalSpawnedCrates = 0;
    
    // Private variables for spawning
    private List<GameObject> spawnedCrateObjects = new List<GameObject>();
    private Vector3 spawnerPosition;
    
    // Transaction data
    private int validTransactionCount = 0;
    private int invalidTransactionCount = 0;
    private int fakeTransactionCount = 0;
    
    void Start()
    {
        Initialize();
        
        if (spawnOnStart)
        {
            SpawnAllCrates();
        }
    }
    
    /// <summary>
    /// Initialize the spawner and load transaction data
    /// </summary>
    private void Initialize()
    {
        spawnerPosition = transform.position;
        LoadTransactionData();
        
        if (showDebugInfo)
        {
            Debug.Log($"St2TransactionCrateSpawner initialized at {spawnerPosition}");
            Debug.Log($"Transaction data loaded - Valid: {validTransactionCount}, Invalid: {invalidTransactionCount}, Fake: {fakeTransactionCount}");
            Debug.Log($"Transactions per crate: {transactionsPerCrate}");
        }
    }
    
    /// <summary>
    /// Load transaction data from PlayerPrefs or testing values
    /// </summary>
    private void LoadTransactionData()
    {
        if (useTestingMode)
        {
            validTransactionCount = testValidTransactions;
            invalidTransactionCount = testInvalidTransactions;
            fakeTransactionCount = testFakeTransactions;
            
            if (showDebugInfo)
            {
                Debug.Log("Using testing mode transaction values");
            }
        }
        else
        {
            validTransactionCount = PlayerPrefs.GetInt(validTxKey, 0);
            invalidTransactionCount = PlayerPrefs.GetInt(invalidTxKey, 0);
            fakeTransactionCount = PlayerPrefs.GetInt(fakeTxKey, 0);
            
            if (showDebugInfo)
            {
                Debug.Log("Loaded transaction data from PlayerPrefs");
            }
        }
    }
    
    /// <summary>
    /// Calculate how many crates should be spawned for a transaction type
    /// Uses ceiling division - even 1 extra transaction requires a new crate (like logistics)
    /// </summary>
    /// <param name="transactionCount">Number of transactions collected</param>
    /// <returns>Number of crates to spawn</returns>
    private int CalculateCrateCount(int transactionCount)
    {
        if (transactionCount <= 0)
            return 0;
            
        // Use ceiling division: even 1 extra transaction needs a new crate
        // Example: 86 transactions ÷ 20 per crate = 4.3 → rounds UP to 5 crates
        return Mathf.CeilToInt((float)transactionCount / transactionsPerCrate);
    }
    
    /// <summary>
    /// Spawn all crates based on loaded transaction data
    /// </summary>
    public void SpawnAllCrates()
    {
        // Clear existing crates first
        ClearAllCrates();
        
        // Calculate crate counts
        int validCrates = CalculateCrateCount(validTransactionCount);
        int invalidCrates = CalculateCrateCount(invalidTransactionCount);
        int fakeCrates = CalculateCrateCount(fakeTransactionCount);
        
        if (showDebugInfo)
        {
            Debug.Log($"Spawning crates - Valid: {validCrates}, Invalid: {invalidCrates}, Fake: {fakeCrates}");
        }
        
        // Spawn crates in order: Valid, Invalid, Fake
        int currentGridPosition = 0;
        
        // Spawn valid crates
        currentGridPosition = SpawnCratesOfType(validCratePrefab, validCrates, currentGridPosition, "Valid");
        spawnedValidCrates = validCrates;
        
        // Spawn invalid crates
        currentGridPosition = SpawnCratesOfType(invalidCratePrefab, invalidCrates, currentGridPosition, "Invalid");
        spawnedInvalidCrates = invalidCrates;
        
        // Spawn fake crates
        currentGridPosition = SpawnCratesOfType(fakeCratePrefab, fakeCrates, currentGridPosition, "Fake");
        spawnedFakeCrates = fakeCrates;
        
        // Update total count
        totalSpawnedCrates = spawnedValidCrates + spawnedInvalidCrates + spawnedFakeCrates;
        
        if (showDebugInfo)
        {
            Debug.Log($"Total crates spawned: {totalSpawnedCrates}");
        }
    }
    
    /// <summary>
    /// Spawn crates of a specific type in grid formation
    /// </summary>
    /// <param name="cratePrefab">Prefab to spawn</param>
    /// <param name="count">Number of crates to spawn</param>
    /// <param name="startGridPosition">Starting grid position</param>
    /// <param name="crateType">Type name for debugging</param>
    /// <returns>Next available grid position</returns>
    private int SpawnCratesOfType(GameObject cratePrefab, int count, int startGridPosition, string crateType)
    {
        if (cratePrefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {crateType} crates!");
            return startGridPosition;
        }
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = CalculateGridPosition(startGridPosition + i);
            GameObject spawnedCrate = Instantiate(cratePrefab, spawnPosition, Quaternion.identity, transform);
            
            // Name the spawned crate for organization
            spawnedCrate.name = $"{crateType}Crate_{i + 1}";
            
            // Add to spawned objects list
            spawnedCrateObjects.Add(spawnedCrate);
            
            if (showDebugInfo)
            {
                Debug.Log($"Spawned {crateType} crate {i + 1}/{count} at {spawnPosition}");
            }
        }
        
        return startGridPosition + count;
    }
    
    /// <summary>
    /// Calculate world position based on grid index
    /// </summary>
    /// <param name="gridIndex">Index in the grid</param>
    /// <returns>World position for spawning</returns>
    private Vector3 CalculateGridPosition(int gridIndex)
    {
        int row = gridIndex / maxCratesPerRow;
        int col = gridIndex % maxCratesPerRow;
        
        float x = spawnerPosition.x + (col * gridSpacingX);
        float y = spawnerPosition.y + spawnHeight;
        float z = spawnerPosition.z + (row * gridSpacingZ);
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// Clear all spawned crates
    /// </summary>
    public void ClearAllCrates()
    {
        foreach (GameObject crate in spawnedCrateObjects)
        {
            if (crate != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(crate);
                }
                else
                {
                    DestroyImmediate(crate);
                }
            }
        }
        
        spawnedCrateObjects.Clear();
        spawnedValidCrates = 0;
        spawnedInvalidCrates = 0;
        spawnedFakeCrates = 0;
        totalSpawnedCrates = 0;
        
        if (showDebugInfo)
        {
            Debug.Log("All crates cleared");
        }
    }
    
    /// <summary>
    /// Reload transaction data and respawn crates
    /// </summary>
    public void RefreshCrates()
    {
        LoadTransactionData();
        SpawnAllCrates();
        
        if (showDebugInfo)
        {
            Debug.Log("Crates refreshed with updated transaction data");
        }
    }
    
    #region Public API for External Access
    
    /// <summary>
    /// Get the number of spawned valid crates
    /// </summary>
    /// <returns>Number of valid crates spawned</returns>
    public int GetSpawnedValidCrates() => spawnedValidCrates;
    
    /// <summary>
    /// Get the number of spawned invalid crates
    /// </summary>
    /// <returns>Number of invalid crates spawned</returns>
    public int GetSpawnedInvalidCrates() => spawnedInvalidCrates;
    
    /// <summary>
    /// Get the number of spawned fake crates
    /// </summary>
    /// <returns>Number of fake crates spawned</returns>
    public int GetSpawnedFakeCrates() => spawnedFakeCrates;
    
    /// <summary>
    /// Get the total number of spawned crates
    /// </summary>
    /// <returns>Total number of crates spawned</returns>
    public int GetTotalSpawnedCrates() => totalSpawnedCrates;
    
    /// <summary>
    /// Get all spawned crate GameObjects
    /// </summary>
    /// <returns>List of spawned crate GameObjects</returns>
    public List<GameObject> GetSpawnedCrateObjects() => new List<GameObject>(spawnedCrateObjects);
    
    /// <summary>
    /// Get current transaction data being used
    /// </summary>
    /// <returns>Array containing [valid, invalid, fake] transaction counts</returns>
    public int[] GetCurrentTransactionData()
    {
        return new int[] { validTransactionCount, invalidTransactionCount, fakeTransactionCount };
    }
    
    /// <summary>
    /// Get spawned crate counts
    /// </summary>
    /// <returns>Array containing [valid, invalid, fake] crate counts</returns>
    public int[] GetSpawnedCrateCounts()
    {
        return new int[] { spawnedValidCrates, spawnedInvalidCrates, spawnedFakeCrates };
    }
    
    /// <summary>
    /// Set custom transaction data (overrides PlayerPrefs and testing mode temporarily)
    /// </summary>
    /// <param name="valid">Valid transaction count</param>
    /// <param name="invalid">Invalid transaction count</param>
    /// <param name="fake">Fake transaction count</param>
    public void SetCustomTransactionData(int valid, int invalid, int fake)
    {
        validTransactionCount = Mathf.Max(0, valid);
        invalidTransactionCount = Mathf.Max(0, invalid);
        fakeTransactionCount = Mathf.Max(0, fake);
        
        if (showDebugInfo)
        {
            Debug.Log($"Custom transaction data set - Valid: {valid}, Invalid: {invalid}, Fake: {fake}");
        }
    }
    
    #endregion
    
    #region Editor Utilities
    
    #if UNITY_EDITOR
    [System.Serializable]
    public class EditorControls
    {
        [Header("Editor Controls")]
        public bool spawnCrates = false;
        public bool clearCrates = false;
        public bool refreshCrates = false;
        public bool testGridLayout = false;
    }
    
    [Header("Editor Controls")]
    public EditorControls editorControls;
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            // Handle editor controls during runtime
            if (editorControls.spawnCrates)
            {
                editorControls.spawnCrates = false;
                SpawnAllCrates();
            }
            
            if (editorControls.clearCrates)
            {
                editorControls.clearCrates = false;
                ClearAllCrates();
            }
            
            if (editorControls.refreshCrates)
            {
                editorControls.refreshCrates = false;
                RefreshCrates();
            }
            
            if (editorControls.testGridLayout)
            {
                editorControls.testGridLayout = false;
                TestGridLayout();
            }
        }
        
        // Ensure valid values
        transactionsPerCrate = Mathf.Max(1, transactionsPerCrate);
        maxCratesPerRow = Mathf.Max(1, maxCratesPerRow);
        gridSpacingX = Mathf.Max(0.1f, gridSpacingX);
        gridSpacingZ = Mathf.Max(0.1f, gridSpacingZ);
    }
    
    /// <summary>
    /// Test grid layout with placeholder cubes (Editor only)
    /// </summary>
    private void TestGridLayout()
    {
        if (showDebugInfo)
        {
            Debug.Log("Testing grid layout with current settings...");
        }
        
        // This could spawn temporary cubes to visualize the grid layout
        // Implementation can be added if needed for testing
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw grid visualization in Scene view
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        
        // Draw spawner center
        Gizmos.DrawWireCube(center + Vector3.up * spawnHeight, Vector3.one * 0.5f);
        
        // Draw grid positions for visualization
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++) // Show first 20 positions
        {
            Vector3 gridPos = CalculateGridPosition(i);
            Gizmos.DrawWireCube(gridPos, Vector3.one * 0.3f);
        }
    }
    
    #endif
    
    #endregion
}
