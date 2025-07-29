using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ControllerRumble : MonoBehaviour
{
    // Public function to control the gamepad rumble
    // duration: How long the rumble should last (in seconds)
    // lowFrequency: Low-frequency (left motor) rumble intensity (0.0 to 1.0)
    // highFrequency: High-frequency (right motor) rumble intensity (0.0 to 1.0)
    public void RumbleTheController(float duration, float lowFrequency, float highFrequency)
    {
        // Check platform and call the appropriate method
#if UNITY_WEBGL
        // Call JavaScript rumble function if running in WebGL
        RumbleWebGLController(duration, lowFrequency, highFrequency);
#else
            // Default rumble for PC (using Unity Input System)
            if (Gamepad.current != null)
            {
                StartCoroutine(RumbleRoutine(duration, lowFrequency, highFrequency));
            }
            else
            {
                Debug.LogWarning("No gamepad connected.");
            }
#endif
    }

    // Routine for rumble when on PC
    private IEnumerator RumbleRoutine(float duration, float lowFrequency, float highFrequency)
    {
        // Set initial rumble values
        Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Stop rumble after the duration ends
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    // Function to call JavaScript for WebGL rumble
    private void RumbleWebGLController(float duration, float lowFrequency, float highFrequency)
    {
        // Call the JavaScript function for WebGL
        Application.ExternalCall("RumbleGamepad", duration, lowFrequency, highFrequency);
    }
}
