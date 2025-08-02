using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Game_UI_Manager : MonoBehaviour
{
    [Header("Transaction Count UI")]
    [Tooltip("Text component to display valid transaction count")]
    public TMP_Text validTxCountText;
    
    [Tooltip("Text component to display fake transaction count")]
    public TMP_Text fakeTxCountText;
    
    [Tooltip("Text component to display invalid transaction count")]
    public TMP_Text invalidTxCountText;
    
    [Tooltip("Text component to display total transaction count")]
    public TMP_Text totalTxCountText;
    
    [Tooltip("Text component to display total transactions available")]
    public TMP_Text totalTxAvailableText;
    
    [Header("Level Complete Screen UI")]
    [Tooltip("Text component to display valid transaction count on level complete")]
    public TMP_Text levelCompleteValidTxText;
    
    [Tooltip("Text component to display fake transaction count on level complete")]
    public TMP_Text levelCompleteFakeTxText;
    
    [Tooltip("Text component to display invalid transaction count on level complete")]
    public TMP_Text levelCompleteInvalidTxText;
    
    [Header("Level Failed Screen UI")]
    [Tooltip("Text component to display valid transaction count on level failed")]
    public TMP_Text levelFailedValidTxText;
    
    [Tooltip("Text component to display fake transaction count on level failed")]
    public TMP_Text levelFailedFakeTxText;
    
    [Tooltip("Text component to display invalid transaction count on level failed")]
    public TMP_Text levelFailedInvalidTxText;
    
    [Tooltip("Text component to display remaining transactions on level failed")]
    public TMP_Text levelFailedRemainingTxText;
    
    [Header("Timer UI")]
    [Tooltip("Text component to display slot timer")]
    public TMP_Text slotTimerText;
    
    [Header("Progress UI")]
    [Tooltip("Progress bar for transaction collection")]
    public Slider txProgressBar;
    
    [Tooltip("Text component to display collection progress percentage")]
    public TMP_Text progressPercentageText;
    
    [Header("UI Settings")]
    [Tooltip("Update UI every frame (disable for performance)")]
    public bool updateEveryFrame = true;
    
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    // Reference to GameLevelManager for subscribing to events
    private GameLevelManager gameLevelManager;
    
    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to transaction manager events if available
        if (InGame_TxManager.Instance != null)
        {
            InGame_TxManager.Instance.OnValidTxCollected.AddListener(OnValidTxCollected);
            InGame_TxManager.Instance.OnFakeTxCollected.AddListener(OnFakeTxCollected);
            InGame_TxManager.Instance.OnInvalidTxCollected.AddListener(OnInvalidTxCollected);
            InGame_TxManager.Instance.OnTotalTxUpdated.AddListener(OnTotalTxUpdated);
        }
        
        // Find and subscribe to GameLevelManager events
        SubscribeToLevelManagerEvents();
        
        // Initialize UI with current values
        UpdateAllTransactionUI();
        
        if (showDebugInfo)
        {
            Debug.Log("Game_UI_Manager initialized and subscribed to transaction events");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (updateEveryFrame)
        {
            UpdateAllTransactionUI();
            UpdateProgressUI();
        }
    }
    
    /// <summary>
    /// Update all transaction-related UI elements
    /// </summary>
    public void UpdateAllTransactionUI()
    {
        if (InGame_TxManager.Instance != null)
        {
            UpdateValidTxUI(InGame_TxManager.Instance.validTxCount);
            UpdateFakeTxUI(InGame_TxManager.Instance.fakeTxCount);
            UpdateInvalidTxUI(InGame_TxManager.Instance.invalidTxCount);
            UpdateTotalTxUI(InGame_TxManager.Instance.totalTxCount);
            UpdateTotalAvailableUI(InGame_TxManager.Instance.totalTxToCollect);
        }
    }
    
    /// <summary>
    /// Update progress bar and percentage
    /// </summary>
    public void UpdateProgressUI()
    {
        if (InGame_TxManager.Instance != null)
        {
            int collected = InGame_TxManager.Instance.totalTxCount;
            int available = InGame_TxManager.Instance.totalTxToCollect;
            
            float progress = available > 0 ? (float)collected / available : 0f;
            
            // Update progress bar
            if (txProgressBar != null)
            {
                txProgressBar.value = progress;
            }
            
            // Update percentage text
            if (progressPercentageText != null)
            {
                progressPercentageText.text = $"{progress * 100:F0}%";
            }
        }
    }
    
    /// <summary>
    /// Update valid transaction count display
    /// </summary>
    /// <param name="count">Valid transaction count</param>
    public void UpdateValidTxUI(int count)
    {
        if (validTxCountText != null)
        {
            validTxCountText.text = count.ToString();
        }
    }
    
    /// <summary>
    /// Update fake transaction count display
    /// </summary>
    /// <param name="count">Fake transaction count</param>
    public void UpdateFakeTxUI(int count)
    {
        if (fakeTxCountText != null)
        {
            fakeTxCountText.text = count.ToString();
        }
    }
    
    /// <summary>
    /// Update invalid transaction count display
    /// </summary>
    /// <param name="count">Invalid transaction count</param>
    public void UpdateInvalidTxUI(int count)
    {
        if (invalidTxCountText != null)
        {
            invalidTxCountText.text = count.ToString();
        }
    }
    
    /// <summary>
    /// Update total transaction count display
    /// </summary>
    /// <param name="count">Total transaction count</param>
    public void UpdateTotalTxUI(int count)
    {
        if (totalTxCountText != null)
        {
            totalTxCountText.text = count.ToString();
        }
    }
    
    /// <summary>
    /// Update total available transaction count display
    /// </summary>
    /// <param name="count">Total available transaction count</param>
    public void UpdateTotalAvailableUI(int count)
    {
        if (totalTxAvailableText != null)
        {
            totalTxAvailableText.text = count.ToString();
        }
    }
    
    /// <summary>
    /// Update slot timer display
    /// </summary>
    /// <param name="timerText">Timer text to display</param>
    public void UpdateSlotTimer(string timerText)
    {
        if (slotTimerText != null)
        {
            slotTimerText.text = timerText;
        }
    }
    
    /// <summary>
    /// Update slot timer with formatted time
    /// </summary>
    /// <param name="timeValue">Time value</param>
    /// <param name="format">Format string (default: "F0 ms")</param>
    public void UpdateSlotTimer(float timeValue, string format = "F0")
    {
        if (slotTimerText != null)
        {
            slotTimerText.text = $"{timeValue.ToString(format)} ms";
        }
    }
    
    /// <summary>
    /// Update level complete screen transaction counts
    /// </summary>
    public void UpdateLevelCompleteUI()
    {
        if (InGame_TxManager.Instance != null)
        {
            if (levelCompleteValidTxText != null)
                levelCompleteValidTxText.text = InGame_TxManager.Instance.validTxCount.ToString();
            
            if (levelCompleteFakeTxText != null)
                levelCompleteFakeTxText.text = InGame_TxManager.Instance.fakeTxCount.ToString();
            
            if (levelCompleteInvalidTxText != null)
                levelCompleteInvalidTxText.text = InGame_TxManager.Instance.invalidTxCount.ToString();
        }
    }
    
    /// <summary>
    /// Update level failed screen transaction counts
    /// </summary>
    public void UpdateLevelFailedUI()
    {
        if (InGame_TxManager.Instance != null)
        {
            if (levelFailedValidTxText != null)
                levelFailedValidTxText.text = InGame_TxManager.Instance.validTxCount.ToString();
            
            if (levelFailedFakeTxText != null)
                levelFailedFakeTxText.text = InGame_TxManager.Instance.fakeTxCount.ToString();
            
            if (levelFailedInvalidTxText != null)
                levelFailedInvalidTxText.text = InGame_TxManager.Instance.invalidTxCount.ToString();
            
            // Calculate remaining transactions
            int collected = InGame_TxManager.Instance.totalTxCount;
            int available = InGame_TxManager.Instance.totalTxToCollect;
            int remaining = available - collected;
            
            if (levelFailedRemainingTxText != null)
                levelFailedRemainingTxText.text = "of " + remaining.ToString();
        }
    }
    
    // Event handlers for transaction collection
    private void OnValidTxCollected(int count)
    {
        UpdateValidTxUI(count);
        UpdateProgressUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"UI Updated: Valid transactions = {count}");
        }
    }
    
    private void OnFakeTxCollected(int count)
    {
        UpdateFakeTxUI(count);
        UpdateProgressUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"UI Updated: Fake transactions = {count}");
        }
    }
    
    private void OnInvalidTxCollected(int count)
    {
        UpdateInvalidTxUI(count);
        UpdateProgressUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"UI Updated: Invalid transactions = {count}");
        }
    }
    
    private void OnTotalTxUpdated(int count)
    {
        UpdateTotalTxUI(count);
        UpdateProgressUI();
        
        if (showDebugInfo)
        {
            Debug.Log($"UI Updated: Total transactions = {count}");
        }
    }
    
    /// <summary>
    /// Show/hide UI elements
    /// </summary>
    /// <param name="show">Whether to show or hide UI</param>
    public void SetUIVisibility(bool show)
    {
        gameObject.SetActive(show);
    }
    
    /// <summary>
    /// Reset all UI to initial state
    /// </summary>
    public void ResetUI()
    {
        UpdateValidTxUI(0);
        UpdateFakeTxUI(0);
        UpdateInvalidTxUI(0);
        UpdateTotalTxUI(0);
        UpdateTotalAvailableUI(0);
        UpdateSlotTimer("0 ms");
        
        // Reset level complete UI
        if (levelCompleteValidTxText != null)
            levelCompleteValidTxText.text = "0";
        if (levelCompleteFakeTxText != null)
            levelCompleteFakeTxText.text = "0";
        if (levelCompleteInvalidTxText != null)
            levelCompleteInvalidTxText.text = "0";
        
        // Reset level failed UI
        if (levelFailedValidTxText != null)
            levelFailedValidTxText.text = "0";
        if (levelFailedFakeTxText != null)
            levelFailedFakeTxText.text = "0";
        if (levelFailedInvalidTxText != null)
            levelFailedInvalidTxText.text = "0";
        if (levelFailedRemainingTxText != null)
            levelFailedRemainingTxText.text = "of 0";
        
        if (txProgressBar != null)
        {
            txProgressBar.value = 0f;
        }
        
        if (progressPercentageText != null)
        {
            progressPercentageText.text = "0%";
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Game UI reset to initial state");
        }
    }
    
    #region GameLevelManager Event Management
    
    /// <summary>
    /// Subscribe to GameLevelManager events
    /// </summary>
    private void SubscribeToLevelManagerEvents()
    {
        // Find GameLevelManager in the scene
        gameLevelManager = GetComponent<GameLevelManager>();
        if (gameLevelManager == null)
        {
            gameLevelManager = FindObjectOfType<GameLevelManager>();
        }
        
        // Subscribe to static events
        GameLevelManager.OnLevelCompleted += OnLevelCompleted;
        GameLevelManager.OnLevelFailed += OnLevelFailed;
        GameLevelManager.OnLevelTransitionStarted += OnLevelTransitionStarted;
        GameLevelManager.OnTeleportationProgress += OnTeleportationProgress;
        
        if (showDebugInfo)
        {
            Debug.Log("Game_UI_Manager subscribed to GameLevelManager events");
        }
    }
    
    /// <summary>
    /// Unsubscribe from GameLevelManager events
    /// </summary>
    private void UnsubscribeFromLevelManagerEvents()
    {
        GameLevelManager.OnLevelCompleted -= OnLevelCompleted;
        GameLevelManager.OnLevelFailed -= OnLevelFailed;
        GameLevelManager.OnLevelTransitionStarted -= OnLevelTransitionStarted;
        GameLevelManager.OnTeleportationProgress -= OnTeleportationProgress;
    }
    
    /// <summary>
    /// Handle level completion event from GameLevelManager
    /// </summary>
    private void OnLevelCompleted()
    {
        UpdateLevelCompleteUI();
        
        if (showDebugInfo)
        {
            Debug.Log("Level completed event received - updating UI");
        }
    }
    
    /// <summary>
    /// Handle level failed event from GameLevelManager
    /// </summary>
    private void OnLevelFailed()
    {
        UpdateLevelFailedUI();
        
        if (showDebugInfo)
        {
            Debug.Log("Level failed event received - updating UI");
        }
    }
    
    /// <summary>
    /// Handle level transition started event from GameLevelManager
    /// </summary>
    /// <param name="levelName">Name of the level being loaded</param>
    private void OnLevelTransitionStarted(string levelName)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Level transition started to: {levelName}");
        }
        
        // You can add transition UI effects here if needed
        // For example: show loading screen, disable input, etc.
    }
    
    /// <summary>
    /// Handle teleportation progress updates from GameLevelManager
    /// </summary>
    /// <param name="progress">Progress value from 0 to 1</param>
    private void OnTeleportationProgress(float progress)
    {
        // You can add teleportation progress UI here if needed
        // For example: update a countdown timer, progress bar, etc.
        
        if (showDebugInfo && progress % 0.1f < 0.02f) // Log every 10%
        {
            Debug.Log($"Teleportation progress: {progress * 100:F0}%");
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Unsubscribe from transaction manager events to prevent memory leaks
        if (InGame_TxManager.Instance != null)
        {
            InGame_TxManager.Instance.OnValidTxCollected.RemoveListener(OnValidTxCollected);
            InGame_TxManager.Instance.OnFakeTxCollected.RemoveListener(OnFakeTxCollected);
            InGame_TxManager.Instance.OnInvalidTxCollected.RemoveListener(OnInvalidTxCollected);
            InGame_TxManager.Instance.OnTotalTxUpdated.RemoveListener(OnTotalTxUpdated);
        }
        
        // Unsubscribe from GameLevelManager events to prevent memory leaks
        UnsubscribeFromLevelManagerEvents();
    }
}
