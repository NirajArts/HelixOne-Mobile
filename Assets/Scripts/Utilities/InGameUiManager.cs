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
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (pausePanel != null && !allowPausingAtStartUI)
            pausePanel.SetActive(false);

        levelManager = gameObject.GetComponent<GameLevelManager>();
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
        StartCoroutine(levelManager.ChangeScenewithDelay(SceneManager.GetActiveScene().buildIndex, 0.5f));
        fadePanel.SetActive(true);
    }

    public void LoadNextLevel()
    {
        StartCoroutine(ChangeScenewithDelay(SceneManager.GetActiveScene().buildIndex + 1, 1.5f));
    }
    public IEnumerator ChangeScenewithDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);

        yield return null;
    }
    IEnumerator TurnOnScreenAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);

        yield return null;
    }

}
