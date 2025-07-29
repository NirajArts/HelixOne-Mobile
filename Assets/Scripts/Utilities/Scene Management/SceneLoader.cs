using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject fadeScreen;
    public float timeToWaitBeforeLoadingLevel = 1f;
    public bool allowTestLoad = false;
    void Start()
    {
        if (allowTestLoad)
        {
            LoadSceneWithIndex();
        }
    }

    public void LoadScene(string sceneName)
    {
        if (fadeScreen != null)
        {
            fadeScreen.SetActive(true);
        }
        StartCoroutine(LoadSceneInGivenDuration(sceneName));
    }

    IEnumerator LoadSceneInGivenDuration(string sceneName)
    {
        yield return new WaitForSeconds(timeToWaitBeforeLoadingLevel);

        SceneManager.LoadScene(sceneName);

        yield return null;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadSceneWithIndex()
    {
        if (fadeScreen != null)
        {
            fadeScreen.SetActive(true);
        }
        StartCoroutine(LoadSceneInGivenDuration("InitialLoaderWallet"));
    }

}
