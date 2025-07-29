using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreUIManager : MonoBehaviour
{
    public TMP_Text levelTimer;
    public TMP_Text levelText;


    private void Start()
    {

        // Set the level text to the current level's name
        levelText.text = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {

    }

    /*
    private void ConvertToHHMMSS()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(00);
        levelTimer.text = "Time " + string.Format("{0:D2}:{1:D2}:{2:D2}",
                                        timeSpan.Hours,
                                        timeSpan.Minutes,
                                        timeSpan.Seconds);
    }
    */
}
