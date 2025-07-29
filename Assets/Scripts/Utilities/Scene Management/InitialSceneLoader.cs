using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialSceneLoader : MonoBehaviour
{
    int gameCount = 0;
    int incrementer;

    public string cinematicLevelName;
    public string mainMenuLevelName;

    SceneLoader sceneLoader;
    public bool allowLoadingSceneAtStartImidiately = false;
    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        gameCount = PlayerPrefs.GetInt("Game Launch Count", 0);
        incrementer = PlayerPrefs.GetInt("Game Launch Count", 0);
    }

    void Start()
    {
        if (allowLoadingSceneAtStartImidiately)
        {
            if(gameCount  < 1)
                sceneLoader.LoadScene(cinematicLevelName);
            else 
                sceneLoader.LoadScene(mainMenuLevelName);
        }
    }

    //Call this at cinematics end
    public void IncremetGameLaunchCount()
    {
        incrementer++;
        PlayerPrefs.SetInt("Game Launch Count", incrementer);
        Debug.Log("'IncremetGameLaunchCount' executed successfully!");
        PlayerPrefs.Save();
    }
    
}
