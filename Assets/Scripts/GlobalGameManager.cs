using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    [Header("Framerate Settings")]
    [Tooltip("Target framerate for the game (0 = unlimited)")]
    public int targetFrameRate = 120;
    
    [Header("Screen Orientation Settings")]
    [Tooltip("Screen orientation for the game")]
    public ScreenOrientation screenOrientation = ScreenOrientation.LandscapeLeft;
    
    // Start is called before the first frame update
    void Start()
    {
        SetCustomFramerate();
        SetCustomOrientation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Set custom framerate for the game
    /// </summary>
    public void SetCustomFramerate()
    {
        // Disable VSync for custom framerate control (especially important on Android)
        QualitySettings.vSyncCount = 0;
        
        // Set target framerate
        Application.targetFrameRate = targetFrameRate;
        
        Debug.Log($"GlobalGameManager: VSync disabled, Framerate set to {targetFrameRate} FPS");
    }
    
    /// <summary>
    /// Set custom screen orientation
    /// </summary>
    public void SetCustomOrientation()
    {
        Screen.orientation = screenOrientation;
        Debug.Log($"GlobalGameManager: Orientation set to {screenOrientation}");
    }
}
