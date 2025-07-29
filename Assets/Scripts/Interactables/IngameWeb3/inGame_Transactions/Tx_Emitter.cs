using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tx_Emitter : MonoBehaviour
{
    [Header("Transaction Prefabs")]
    [Tooltip("Prefab for valid transactions")]
    public GameObject validTransactionPrefab;
    
    [Tooltip("Prefab for invalid transactions")]
    public GameObject invalidTransactionPrefab;
    
    [Tooltip("Prefab for fake transactions")]
    public GameObject fakeTransactionPrefab;
    
    [Header("Spawn Probabilities")]
    [Tooltip("Probability of spawning valid transactions (0-1)")]
    [Range(0f, 1f)]
    public float validTxProbability = 0.5f;
    
    [Tooltip("Probability of spawning invalid transactions (0-1)")]
    [Range(0f, 1f)]
    public float invalidTxProbability = 0.3f;
    
    [Tooltip("Probability of spawning fake transactions (0-1)")]
    [Range(0f, 1f)]
    public float fakeTxProbability = 0.2f;
    
    [Header("Spawn Settings")]
    [Tooltip("Number of transactions to spawn")]
    [Range(1, 100)]
    public int spawnCount = 10;
    
    [Tooltip("Minimum distance between spawned transactions")]
    [Range(0.1f, 5f)]
    public float minDistance = 1f;
    
    [Tooltip("Maximum attempts to find a valid spawn position")]
    [Range(10, 1000)]
    public int maxSpawnAttempts = 100;
    
    [Tooltip("Offset distance from mesh surface")]
    [Range(0f, 2f)]
    public float surfaceOffset = 0.1f;
    
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance for raycast from emitter surface")]
    [Range(1f, 100f)]
    public float maxRaycastDistance = 50f;
    
    [Tooltip("Layers to raycast against (ground, objects, etc.)")]
    public LayerMask raycastLayers = -1;
    
    [Tooltip("Offset above the hit surface")]
    [Range(0f, 2f)]
    public float groundOffset = 0.1f;
    
    [Header("Auto Spawn Settings")]
    [Tooltip("Automatically spawn transactions on start")]
    public bool autoSpawnOnStart = true;
    
    [Tooltip("Enable continuous spawning over time")]
    public bool continuousSpawning = false;
    
    [Tooltip("Interval between spawn cycles (seconds)")]
    [Range(1f, 60f)]
    public float spawnInterval = 5f;
    
    [Header("Performance Optimization")]
    [Tooltip("Pre-calculate raycast positions at start for better performance")]
    public bool preCalculatePositions = true;
    
    [Tooltip("Number of positions to pre-calculate")]
    [Range(50, 1000)]
    public int preCalculatedPositionsCount = 200;
    
    [Header("Debug")]
    [Tooltip("Show debug information and gizmos")]
    public bool showDebug = false;
    
    // Private variables
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private List<GameObject> spawnedTransactions = new List<GameObject>();
    private List<Vector3> preCalculatedPositions = new List<Vector3>();
    private List<Vector3> preCalculatedNormals = new List<Vector3>();
    private Coroutine continuousSpawnCoroutine;
    
    void Start()
    {
        InitializeMesh();
        
        if (preCalculatePositions)
        {
            StartCoroutine(PreCalculateSpawnPositions());
        }
        
        if (autoSpawnOnStart)
        {
            if (preCalculatePositions)
            {
                StartCoroutine(SpawnWhenReady());
            }
            else
            {
                SpawnTransactions();
            }
        }
        
        if (continuousSpawning)
        {
            StartContinuousSpawning();
        }
    }
    
    /// <summary>
    /// Wait for pre-calculation to complete before spawning
    /// </summary>
    private IEnumerator SpawnWhenReady()
    {
        while (preCalculatedPositions.Count == 0)
        {
            yield return null; // Wait one frame
        }
        SpawnTransactions();
    }
    
    /// <summary>
    /// Pre-calculate spawn positions for better performance
    /// </summary>
    private IEnumerator PreCalculateSpawnPositions()
    {
        if (showDebug)
        {
            Debug.Log("Starting pre-calculation of spawn positions...");
        }
        
        preCalculatedPositions.Clear();
        preCalculatedNormals.Clear();
        
        int calculatedCount = 0;
        int maxAttempts = preCalculatedPositionsCount * 5; // Allow more attempts for pre-calculation
        
        for (int attempt = 0; attempt < maxAttempts && calculatedCount < preCalculatedPositionsCount; attempt++)
        {
            Vector3 position, normal;
            if (GetRandomSpawnPositionImmediate(out position, out normal))
            {
                // Check if this position is far enough from already calculated positions
                bool validPosition = true;
                foreach (Vector3 existingPos in preCalculatedPositions)
                {
                    if (Vector3.Distance(position, existingPos) < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (validPosition)
                {
                    preCalculatedPositions.Add(position);
                    preCalculatedNormals.Add(normal);
                    calculatedCount++;
                }
            }
            
            // Yield every few calculations to prevent frame drops
            if (attempt % 10 == 0)
            {
                yield return null;
            }
        }
        
        if (showDebug)
        {
            Debug.Log($"Pre-calculated {calculatedCount} spawn positions out of {preCalculatedPositionsCount} requested");
        }
    }
    
    /// <summary>
    /// Initialize mesh data for spawning
    /// </summary>
    private void InitializeMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        if (meshFilter == null)
        {
            Debug.LogError($"Tx_Emitter on {gameObject.name} requires a MeshFilter component!");
            return;
        }
        
        mesh = meshFilter.mesh;
        if (mesh == null)
        {
            Debug.LogError($"Tx_Emitter on {gameObject.name}: MeshFilter has no mesh!");
            return;
        }
        
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        
        if (showDebug)
        {
            Debug.Log($"Tx_Emitter initialized with {vertices.Length} vertices and {triangles.Length / 3} triangles");
        }
    }
    
    /// <summary>
    /// Spawn transactions on the mesh surface
    /// </summary>
    public void SpawnTransactions()
    {
        if (mesh == null)
        {
            Debug.LogError("Cannot spawn transactions: mesh not initialized!");
            return;
        }
        
        spawnedPositions.Clear();
        spawnedTransactions.Clear();
        int successfulSpawns = 0;
        
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition;
            Vector3 spawnNormal;
            
            if (GetRandomSpawnPosition(out spawnPosition, out spawnNormal))
            {
                GameObject transactionPrefab = SelectTransactionPrefab();
                
                if (transactionPrefab != null)
                {
                    // Use the original prefab rotation instead of aligning with surface normal
                    Quaternion spawnRotation = transactionPrefab.transform.rotation;
                    
                    // Instantiate the transaction without setting as child
                    GameObject spawnedTx = Instantiate(transactionPrefab, spawnPosition, spawnRotation);
                    
                    spawnedPositions.Add(spawnPosition);
                    spawnedTransactions.Add(spawnedTx);
                    successfulSpawns++;
                    
                    if (showDebug)
                    {
                        Debug.Log($"Spawned {transactionPrefab.name} at {spawnPosition}");
                    }
                }
            }
        }
        
        if (showDebug)
        {
            Debug.Log($"Successfully spawned {successfulSpawns} out of {spawnCount} transactions");
        }
        
        // Update the InGame_TxManager with the number of spawned transactions
        if (InGame_TxManager.Instance != null)
        {
            InGame_TxManager.Instance.AddSpawnedTransactions(successfulSpawns);
        }
    }
    
    /// <summary>
    /// Get a random valid spawn position on the mesh surface - immediate version for pre-calculation
    /// </summary>
    private bool GetRandomSpawnPositionImmediate(out Vector3 worldPosition, out Vector3 worldNormal)
    {
        worldPosition = Vector3.zero;
        worldNormal = Vector3.up;
        
        if (vertices == null || triangles == null || vertices.Length == 0 || triangles.Length == 0)
        {
            return false;
        }
        
        // Select a random triangle
        int triangleIndex = Random.Range(0, triangles.Length / 3) * 3;
        
        // Get triangle vertices
        Vector3 v0 = vertices[triangles[triangleIndex]];
        Vector3 v1 = vertices[triangles[triangleIndex + 1]];
        Vector3 v2 = vertices[triangles[triangleIndex + 2]];
        
        // Generate random barycentric coordinates
        float r1 = Random.Range(0f, 1f);
        float r2 = Random.Range(0f, 1f);
        
        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }
        
        // Calculate position on triangle
        Vector3 localPosition = v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
        Vector3 emitterSurfacePos = transform.TransformPoint(localPosition);
        
        // Calculate surface normal for raycast direction
        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        Vector3 rayDirection = transform.TransformDirection(normal);
        
        // Perform raycast from emitter surface downward
        RaycastHit hit;
        Vector3 rayStart = emitterSurfacePos + rayDirection * surfaceOffset;
        
        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRaycastDistance, raycastLayers))
        {
            // Found a surface to land on
            worldPosition = hit.point + hit.normal * groundOffset;
            worldNormal = hit.normal;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get a random valid spawn position on the mesh surface
    /// </summary>
    private bool GetRandomSpawnPosition(out Vector3 worldPosition, out Vector3 worldNormal)
    {
        // Use pre-calculated positions if available
        if (preCalculatePositions && preCalculatedPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, preCalculatedPositions.Count);
            worldPosition = preCalculatedPositions[randomIndex];
            worldNormal = preCalculatedNormals[randomIndex];
            
            // Check minimum distance from other spawned positions
            if (IsValidSpawnPosition(worldPosition))
            {
                return true;
            }
        }
        
        // Fallback to real-time calculation with distance checking
        worldPosition = Vector3.zero;
        worldNormal = Vector3.up;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Select a random triangle
            int triangleIndex = Random.Range(0, triangles.Length / 3) * 3;
            
            // Get triangle vertices
            Vector3 v0 = vertices[triangles[triangleIndex]];
            Vector3 v1 = vertices[triangles[triangleIndex + 1]];
            Vector3 v2 = vertices[triangles[triangleIndex + 2]];
            
            // Generate random barycentric coordinates
            float r1 = Random.Range(0f, 1f);
            float r2 = Random.Range(0f, 1f);
            
            if (r1 + r2 > 1f)
            {
                r1 = 1f - r1;
                r2 = 1f - r2;
            }
            
            // Calculate position on triangle
            Vector3 localPosition = v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
            Vector3 emitterSurfacePos = transform.TransformPoint(localPosition);
            
            // Calculate surface normal for raycast direction
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            Vector3 rayDirection = transform.TransformDirection(normal);
            
            // Perform raycast from emitter surface downward
            RaycastHit hit;
            Vector3 rayStart = emitterSurfacePos + rayDirection * surfaceOffset;
            
            if (Physics.Raycast(rayStart, rayDirection, out hit, maxRaycastDistance, raycastLayers))
            {
                // Found a surface to land on
                worldPosition = hit.point + hit.normal * groundOffset;
                worldNormal = hit.normal;
                
                // Check minimum distance from other spawned positions
                if (IsValidSpawnPosition(worldPosition))
                {
                    if (showDebug)
                    {
                        Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.green, 2f);
                        Debug.Log($"Raycast hit: {hit.collider.name} at {hit.point}");
                    }
                    return true;
                }
            }
            else if (showDebug)
            {
                Debug.DrawRay(rayStart, rayDirection * maxRaycastDistance, Color.red, 1f);
            }
        }
        
        if (showDebug)
        {
            Debug.LogWarning($"Failed to find valid spawn position after {maxSpawnAttempts} attempts");
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a position is valid (maintains minimum distance from other spawns)
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Select a transaction prefab based on probabilities
    /// </summary>
    private GameObject SelectTransactionPrefab()
    {
        // Normalize probabilities
        float totalProbability = validTxProbability + invalidTxProbability + fakeTxProbability;
        
        if (totalProbability <= 0f)
        {
            Debug.LogWarning("All transaction probabilities are zero!");
            return null;
        }
        
        float randomValue = Random.Range(0f, totalProbability);
        
        if (randomValue < validTxProbability)
        {
            return validTransactionPrefab;
        }
        else if (randomValue < validTxProbability + invalidTxProbability)
        {
            return invalidTransactionPrefab;
        }
        else
        {
            return fakeTransactionPrefab;
        }
    }
    
    /// <summary>
    /// Start continuous spawning coroutine
    /// </summary>
    public void StartContinuousSpawning()
    {
        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
        }
        
        continuousSpawnCoroutine = StartCoroutine(ContinuousSpawnRoutine());
    }
    
    /// <summary>
    /// Stop continuous spawning
    /// </summary>
    public void StopContinuousSpawning()
    {
        if (continuousSpawnCoroutine != null)
        {
            StopCoroutine(continuousSpawnCoroutine);
            continuousSpawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// Continuous spawning coroutine
    /// </summary>
    private IEnumerator ContinuousSpawnRoutine()
    {
        while (continuousSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnTransactions();
        }
    }
    
    /// <summary>
    /// Clear all spawned transactions
    /// </summary>
    public void ClearSpawnedTransactions()
    {
        // Destroy all tracked spawned transaction objects
        for (int i = spawnedTransactions.Count - 1; i >= 0; i--)
        {
            if (spawnedTransactions[i] != null)
            {
                DestroyImmediate(spawnedTransactions[i]);
            }
        }
        
        spawnedPositions.Clear();
        spawnedTransactions.Clear();
        
        if (showDebug)
        {
            Debug.Log("Cleared all spawned transactions");
        }
    }
    
    /// <summary>
    /// Validate probability settings
    /// </summary>
    private void OnValidate()
    {
        // Ensure probabilities don't exceed reasonable limits
        float total = validTxProbability + invalidTxProbability + fakeTxProbability;
        if (total > 3f)
        {
            Debug.LogWarning("Total probability exceeds 3.0, consider adjusting values for better balance");
        }
    }
    
    /// <summary>
    /// Draw debug gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebug) return;
        
        // Draw spawn positions
        Gizmos.color = Color.yellow;
        foreach (Vector3 pos in spawnedPositions)
        {
            Gizmos.DrawWireSphere(pos, minDistance * 0.5f);
        }
        
        // Draw mesh bounds
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, meshFilter.mesh.bounds.size);
        }
    }
}
