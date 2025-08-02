using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUiManager : MonoBehaviour
{
    public GameObject deathPanel;
    public GameObject pausePanel;
    public GameObject fadePanel;
    public GameObject winPanel;

    private GameLevelManager levelManager;

    public bool allowPausingAtStartUI = false;
    void Start()
    {
        // Initialize UI panels
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (pausePanel != null && !allowPausingAtStartUI)
            pausePanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);

        // Find GameLevelManager reference
        levelManager = gameObject.GetComponent<GameLevelManager>();
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<GameLevelManager>();
        }
        
        if (levelManager == null)
        {
            Debug.LogWarning("InGameUiManager: No GameLevelManager found. Some functionality may not work properly.");
        }
    }

    public void InitiateDeathScreen()
    {
        Debug.Log("Death screen initiated successfuly.");
        StartCoroutine(TurnOnScreenAfterDelay(deathPanel, 1f));
    }

    public void GamePaused(bool isPaused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }

    public void EnableWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
        else
        {
            Debug.LogWarning("InGameUiManager: Cannot restart - no GameLevelManager found.");
        }
        
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
        }
    }

    public void LoadNextLevel()
    {
        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }
        else
        {
            Debug.LogWarning("InGameUiManager: Cannot load next level - no GameLevelManager found.");
        }
    }
    IEnumerator TurnOnScreenAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);

        yield return null;
    }

    /// <summary>
    /// Close all UI panels
    /// </summary>
    public void CloseAllPanels()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (fadePanel != null) fadePanel.SetActive(false);
    }

    /// <summary>
    /// Show a specific panel
    /// </summary>
    /// <param name="panelName">Name of the panel to show</param>
    public void ShowPanel(string panelName)
    {
        switch (panelName.ToLower())
        {
            case "death":
                InitiateDeathScreen();
                break;
            case "pause":
                GamePaused(true);
                break;
            case "win":
                EnableWinPanel();
                break;
            case "fade":
                if (fadePanel != null) fadePanel.SetActive(true);
                break;
            default:
                Debug.LogWarning($"InGameUiManager: Unknown panel name '{panelName}'");
                break;
        }
    }

    /// <summary>
    /// Hide a specific panel
    /// </summary>
    /// <param name="panelName">Name of the panel to hide</param>
    public void HidePanel(string panelName)
    {
        GameObject panel = null;
        
        switch (panelName.ToLower())
        {
            case "death":
                panel = deathPanel;
                break;
            case "pause":
                panel = pausePanel;
                break;
            case "win":
                panel = winPanel;
                break;
            case "fade":
                panel = fadePanel;
                break;
            default:
                Debug.LogWarning($"InGameUiManager: Unknown panel name '{panelName}'");
                return;
        }
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

}
