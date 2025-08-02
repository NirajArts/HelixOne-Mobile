using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Independent Level Manager for handling scene transitions, player states, and level management
/// Uses events to communicate with UI systems - no direct UI dependencies
/// Can be used in any scene without static dependencies
/// </summary>
public class GameLevelManager : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Time delay before transitioning to next level")]
    public float transitionDelay = 2.3f;
    
    [Tooltip("Animator for fade screen transitions")]
    public Animator fadeScreen;
    
    [Tooltip("Allow immediate scene loading without UI delays")]
    public bool allowDirectLoading = false;
    
    [Tooltip("Default fade duration for scene transitions")]
    public float fadeDuration = 1.5f;
    
    [Header("Level Management")]
    [Tooltip("Name of the next level scene")]
    public string nextLevelName = "";
    
    [Tooltip("Current level name (for restart functionality)")]
    public string currentLevelName = "";
    
    [Header("Player State")]
    [Tooltip("Is player currently in teleportation state")]
    public bool isTeleported = false;
    
    [Tooltip("Is player currently dead")]
    public bool isPlayerDead = false;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;
    
    // Events for UI communication - no direct UI dependencies
    public static event Action OnLevelCompleted;
    public static event Action OnLevelFailed;
    public static event Action<string> OnLevelTransitionStarted;
    public static event Action<float> OnTeleportationProgress;
    
    // Private variables
    private float teleportTimer;
    private bool isTransitioning = false;
    
    #region Unity Lifecycle
    
    void Start()
    {
        InitializeManager();
    }
    
    void Update()
    {
        HandleTeleportTimer();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize the level manager
    /// </summary>
    private void InitializeManager()
    {
        // Set current level name if not manually assigned
        if (string.IsNullOrEmpty(currentLevelName))
        {
            currentLevelName = SceneManager.GetActiveScene().name;
        }
        
        // Reset states
        isTeleported = false;
        isPlayerDead = false;
        isTransitioning = false;
        teleportTimer = transitionDelay;
        
        if (showDebugMessages)
        {
            Debug.Log($"GameLevelManager initialized for level: {currentLevelName}");
        }
    }
    
    #endregion
    
    #region Timer Management
    
    /// <summary>
    /// Handle teleportation timer countdown
    /// </summary>
    private void HandleTeleportTimer()
    {
        if (!isTeleported || isTransitioning)
            return;
            
        teleportTimer -= Time.deltaTime;
        
        // Broadcast teleportation progress
        float progress = 1f - (teleportTimer / transitionDelay);
        OnTeleportationProgress?.Invoke(progress);
        
        if (teleportTimer <= 0f)
        {
            LoadNextLevel();
        }
    }
    
    #endregion
    
    #region Public Level Control Methods
    
    /// <summary>
    /// Trigger teleportation state (usually called by triggers or game events)
    /// </summary>
    public void TriggerTeleportation()
    {
        if (isTransitioning || isPlayerDead)
            return;
            
        isTeleported = true;
        teleportTimer = transitionDelay;
        
        if (showDebugMessages)
        {
            Debug.Log($"Teleportation triggered. Transitioning in {transitionDelay} seconds.");
        }
    }
    
    /// <summary>
    /// Load the next level (defined in nextLevelName)
    /// </summary>
    public void LoadNextLevel()
    {
        if (isTransitioning)
            return;
            
        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogWarning("Next level name is not set! Cannot load next level.");
            return;
        }
        
        StartLevelTransition(nextLevelName, true);
    }
    
    /// <summary>
    /// Load a specific level by name (for UI button events)
    /// </summary>
    /// <param name="levelName">Name of the scene to load</param>
    public void LoadLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning("Level name is empty! Cannot load level.");
            return;
        }
        
        StartLevelTransition(levelName, false);
    }
    
    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        if (string.IsNullOrEmpty(currentLevelName))
        {
            // Fallback to current scene if name not set
            currentLevelName = SceneManager.GetActiveScene().name;
        }
        
        StartLevelTransition(currentLevelName, false);
    }
    
    /// <summary>
    /// Load level by build index (for UI button events)
    /// </summary>
    /// <param name="buildIndex">Build index of the scene</param>
    public void LoadLevelByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Invalid build index: {buildIndex}");
            return;
        }
        
        StartCoroutine(TransitionToSceneByIndex(buildIndex));
    }
    
    #endregion
    
    #region Player State Management
    
    /// <summary>
    /// Handle player death
    /// </summary>
    public void OnPlayerDeath()
    {
        if (isPlayerDead)
            return;
            
        isPlayerDead = true;
        isTeleported = false; // Stop any ongoing teleportation
        
        // Broadcast level failed event for UI systems to handle
        OnLevelFailed?.Invoke();
        
        if (showDebugMessages)
        {
            Debug.Log("Player death triggered - Level failed event broadcasted");
        }
    }
    
    /// <summary>
    /// Reset player state (useful for respawning)
    /// </summary>
    public void ResetPlayerState()
    {
        isPlayerDead = false;
        isTeleported = false;
        isTransitioning = false;
        teleportTimer = transitionDelay;
        
        if (showDebugMessages)
        {
            Debug.Log("Player state reset");
        }
    }
    
    #endregion
    
    #region Private Transition Methods
    
    /// <summary>
    /// Start level transition with fade effects
    /// </summary>
    /// <param name="levelName">Target level name</param>
    /// <param name="showWinPanel">Whether to broadcast level completion event</param>
    private void StartLevelTransition(string levelName, bool showWinPanel)
    {
        if (isTransitioning)
            return;
            
        isTransitioning = true;
        
        // Broadcast transition started event
        OnLevelTransitionStarted?.Invoke(levelName);
        
        // Trigger fade animation
        if (fadeScreen != null)
        {
            fadeScreen.SetTrigger("FadeIn");
        }
        
        // Broadcast level completion event if requested
        if (showWinPanel)
        {
            OnLevelCompleted?.Invoke();
        }
        
        // Start transition
        if (allowDirectLoading)
        {
            StartCoroutine(TransitionToScene(levelName));
        }
        else
        {
            StartCoroutine(TransitionToScene(levelName));
        }
        
        if (showDebugMessages)
        {
            Debug.Log($"Starting transition to level: {levelName}");
        }
    }
    
    /// <summary>
    /// Coroutine to handle scene transition with delay
    /// </summary>
    /// <param name="levelName">Target scene name</param>
    /// <returns></returns>
    private IEnumerator TransitionToScene(string levelName)
    {
        yield return new WaitForSeconds(fadeDuration);
        
        try
        {
            SceneManager.LoadScene(levelName);
            
            if (showDebugMessages)
            {
                Debug.Log($"Successfully loaded scene: {levelName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{levelName}': {e.Message}");
            isTransitioning = false;
        }
    }
    
    /// <summary>
    /// Coroutine to handle scene transition by build index
    /// </summary>
    /// <param name="buildIndex">Target scene build index</param>
    /// <returns></returns>
    private IEnumerator TransitionToSceneByIndex(int buildIndex)
    {
        isTransitioning = true;
        
        // Trigger fade animation
        if (fadeScreen != null)
        {
            fadeScreen.SetTrigger("FadeIn");
        }
        
        yield return new WaitForSeconds(fadeDuration);
        
        try
        {
            SceneManager.LoadScene(buildIndex);
            
            if (showDebugMessages)
            {
                Debug.Log($"Successfully loaded scene with index: {buildIndex}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene with index {buildIndex}: {e.Message}");
            isTransitioning = false;
        }
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Check if manager is currently transitioning
    /// </summary>
    /// <returns>True if transitioning</returns>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    /// <summary>
    /// Get current level name
    /// </summary>
    /// <returns>Current level name</returns>
    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }
    
    /// <summary>
    /// Set next level name
    /// </summary>
    /// <param name="levelName">Next level name</param>
    public void SetNextLevel(string levelName)
    {
        nextLevelName = levelName;
        
        if (showDebugMessages)
        {
            Debug.Log($"Next level set to: {levelName}");
        }
    }
    
    #endregion
    
    #region Legacy Support (for backward compatibility)
    
    /// <summary>
    /// Legacy method - use TriggerTeleportation() instead
    /// </summary>
    [System.Obsolete("Use TriggerTeleportation() instead")]
    public void Teleported()
    {
        TriggerTeleportation();
    }
    
    /// <summary>
    /// Legacy method - use OnPlayerDeath() instead
    /// </summary>
    [System.Obsolete("Use OnPlayerDeath() instead")]
    public void PlayerIsDead()
    {
        OnPlayerDeath();
    }
    
    /// <summary>
    /// Legacy method - use LoadLevelByIndex() instead
    /// </summary>
    /// <param name="index">Scene build index</param>
    [System.Obsolete("Use LoadLevelByIndex() instead")]
    public void LoadCustomSceneWithIndex(int index)
    {
        LoadLevelByIndex(index);
    }
    
    #endregion
}
