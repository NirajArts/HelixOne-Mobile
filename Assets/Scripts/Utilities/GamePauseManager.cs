using UnityEngine;
using UnityEngine.UI;
using System;

public class GamePauseManager : MonoBehaviour
{
    [Header("Pause State")]
    private bool isGamePaused = false;
    private bool isPlayerDead = false;
    public bool pauseAtStart = false;

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button[] additionalPauseButtons; // For multiple pause buttons
    [SerializeField] private Button[] additionalResumeButtons; // For multiple resume buttons

    // Events for external systems to listen to pause state changes
    public static event Action<bool> OnGamePausedChanged;

    private void Start()
    {
        SetupUIReferences();
        
        if (pauseAtStart)
        {
            PauseGame();
        }
    }

    private void SetupUIReferences()
    {
        // Setup pause button listeners
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
            
        if (additionalPauseButtons != null)
        {
            foreach (Button button in additionalPauseButtons)
            {
                if (button != null)
                    button.onClick.AddListener(PauseGame);
            }
        }

        // Setup resume button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (additionalResumeButtons != null)
        {
            foreach (Button button in additionalResumeButtons)
            {
                if (button != null)
                    button.onClick.AddListener(ResumeGame);
            }
        }
    }

    public void PauseGame() // Can be called by UI buttons or external scripts
    {
        if (!isPlayerDead && !isGamePaused)
        {
            // Update UI state
            UpdatePauseUI(true);
            
            // Pause the game
            Time.timeScale = 0f;
            isGamePaused = true;
            
            // Notify other systems
            OnGamePausedChanged?.Invoke(true);
        }
    }

    public void ResumeGame() // Can be called by UI buttons or external scripts
    {
        if (!isPlayerDead && isGamePaused)
        {
            // Update UI state
            UpdatePauseUI(false);
            
            // Resume the game
            Time.timeScale = 1f;
            isGamePaused = false;
            
            // Notify other systems
            OnGamePausedChanged?.Invoke(false);
        }
    }

    private void UpdatePauseUI(bool isPaused)
    {
        // Show/hide pause panel
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
            
        // Enable/disable pause button
        if (pauseButton != null)
            pauseButton.interactable = !isPaused;
            
        // Handle additional pause buttons
        if (additionalPauseButtons != null)
        {
            foreach (Button button in additionalPauseButtons)
            {
                if (button != null)
                    button.interactable = !isPaused;
            }
        }
    }

    // Public properties for external access
    public bool IsGamePaused => isGamePaused;
    public bool IsPlayerDead 
    { 
        get => isPlayerDead; 
        set => isPlayerDead = value; 
    }

    // Toggle pause state
    public void TogglePause()
    {
        if (isGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(PauseGame);
            
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);
            
        if (additionalPauseButtons != null)
        {
            foreach (Button button in additionalPauseButtons)
            {
                if (button != null)
                    button.onClick.RemoveListener(PauseGame);
            }
        }
        
        if (additionalResumeButtons != null)
        {
            foreach (Button button in additionalResumeButtons)
            {
                if (button != null)
                    button.onClick.RemoveListener(ResumeGame);
            }
        }
    }
}
