using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    public bool isTeleported = false;
    public float timeToLevelTransition = 2.3f;
    public Animator fadeScreen;

    public bool allowDirectLoading = false;

    [Header("Player Death")]
    public bool isPlayerDead = false;

    InGameUiManager uiManager;
    void Start()
    {
        uiManager = GetComponent<InGameUiManager>();
    }

    void Update()
    {
        if (!isTeleported)
            return;

        if (isTeleported)
        {
            timeToLevelTransition -= Time.deltaTime;
            if (timeToLevelTransition < 0)
                LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        fadeScreen.SetTrigger("FadeIn");
        uiManager.EnableWinPanel();
        if (allowDirectLoading)
        {
            TransitionTheScene();
        }
    }

    void TransitionTheScene()
    {
        fadeScreen.SetTrigger("FadeIn");
        StartCoroutine(ChangeScenewithDelay(SceneManager.GetActiveScene().buildIndex + 1, 1.5f));
        Debug.Log("transitioning to next scene");
    }

    public void LoadCustomSceneWithIndex(int index)
    {
        StartCoroutine(ChangeScenewithDelay(index, 1.5f));
        Debug.Log("Loading custom scene with index: " + index);
    }

    public IEnumerator ChangeScenewithDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);
        Debug.Log("Scene loaded with index: " + index);

        yield return null;
    }

    public void Teleported()
    {
        isTeleported = true;
    }

    public void PlayerIsDead()
    {
        isPlayerDead = true;
        uiManager.InitiateDeathScreen();
    }

}
