using UnityEngine;

public class MobileHaptics : MonoBehaviour
{
    [Header("Haptic Settings")]
    [Tooltip("Enable/disable haptic feedback")]
    public bool enableHaptics = true;
    
    [Header("Default Vibration Patterns")]
    [Tooltip("Duration for light haptic feedback (in seconds)")]
    public float lightVibrationDuration = 0.1f;
    
    [Tooltip("Duration for medium haptic feedback (in seconds)")]
    public float mediumVibrationDuration = 0.2f;
    
    [Tooltip("Duration for strong haptic feedback (in seconds)")]
    public float strongVibrationDuration = 0.4f;

    private static MobileHaptics _instance;
    public static MobileHaptics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MobileHaptics>();
                if (_instance == null)
                {
                    GameObject hapticObj = new GameObject("MobileHaptics");
                    _instance = hapticObj.AddComponent<MobileHaptics>();
                    DontDestroyOnLoad(hapticObj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Trigger haptic feedback with specified duration
    /// </summary>
    /// <param name="duration">Duration of vibration in seconds</param>
    public void TriggerHaptic(float duration)
    {
        if (!enableHaptics) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        TriggerAndroidVibration(duration);
#elif UNITY_IOS && !UNITY_EDITOR
        TriggerIOSHaptic(duration);
#else
        // For editor testing
        Debug.Log($"Haptic feedback triggered: {duration}s");
#endif
    }

    /// <summary>
    /// Trigger light haptic feedback
    /// </summary>
    public void TriggerLightHaptic()
    {
        TriggerHaptic(lightVibrationDuration);
    }

    /// <summary>
    /// Trigger medium haptic feedback
    /// </summary>
    public void TriggerMediumHaptic()
    {
        TriggerHaptic(mediumVibrationDuration);
    }

    /// <summary>
    /// Trigger strong haptic feedback
    /// </summary>
    public void TriggerStrongHaptic()
    {
        TriggerHaptic(strongVibrationDuration);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void TriggerAndroidVibration(float duration)
    {
        try
        {
            // Convert duration to milliseconds
            long milliseconds = (long)(duration * 1000);
            
            // Use Unity's built-in Handheld.Vibrate() for simple vibration
            if (milliseconds <= 100)
            {
                Handheld.Vibrate(); // Short vibration
            }
            else
            {
                // For longer vibrations, we use Android's native vibration
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator.Call<bool>("hasVibrator"))
                    {
                        // Check Android API level for different vibration methods
                        using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
                        {
                            int apiLevel = buildVersion.GetStatic<int>("SDK_INT");
                            
                            if (apiLevel >= 26) // Android 8.0+
                            {
                                // Use VibrationEffect for newer Android versions
                                using (AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                                {
                                    AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
                                    vibrator.Call("vibrate", effect);
                                }
                            }
                            else
                            {
                                // Fallback for older Android versions
                                vibrator.Call("vibrate", milliseconds);
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Android vibration failed: {e.Message}");
            // Fallback to simple vibration
            Handheld.Vibrate();
        }
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private void TriggerIOSHaptic(float duration)
    {
        try
        {
            // iOS haptic feedback using different intensities based on duration
            if (duration <= 0.1f)
            {
                // Light haptic
                iOSHapticFeedback(0);
            }
            else if (duration <= 0.25f)
            {
                // Medium haptic
                iOSHapticFeedback(1);
            }
            else
            {
                // Heavy haptic
                iOSHapticFeedback(2);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"iOS haptic failed: {e.Message}");
            // Fallback to simple vibration
            Handheld.Vibrate();
        }
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void iOSHapticFeedback(int type);
#endif

    /// <summary>
    /// Check if haptic feedback is available on the current device
    /// </summary>
    /// <returns>True if haptics are supported</returns>
    public bool IsHapticSupported()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                return vibrator.Call<bool>("hasVibrator");
            }
        }
        catch
        {
            return false;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        return true; // iOS devices generally support haptics
#else
        return false; // Not supported in editor or other platforms
#endif
    }
}
