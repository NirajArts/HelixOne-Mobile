using UnityEngine;
using UnityEngine.InputSystem;

public class GamePauseManager : MonoBehaviour
{
    public InputActionReference pauseAction; // Public InputActionReference for the pause input
    private bool isGamePaused = false;
    private bool isPlayerDead = false; // Assuming you have a way to check this condition
    private InGameUiManager uiManager; // Assuming you have a UIManager to handle UI changes

    public bool pauseAtStart = false;

    private void Start()
    {
        uiManager = GetComponent<InGameUiManager>();
        if (pauseAtStart)
        {
            PauseGame();
        }
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            // Enable the input action
            pauseAction.action.Enable();
            // Assign the TogglePause function to be called when the pause action is performed
            pauseAction.action.performed += TogglePause;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            // Unsubscribe from the pause action and disable it
            pauseAction.action.performed -= TogglePause;
            pauseAction.action.Disable();
        }
    }

    public void PauseGame() // Executed by third-party UI pauseButton
    {
        if (!isPlayerDead && !isGamePaused)
        {
            if (uiManager != null)
                uiManager.GamePaused(true);
            Time.timeScale = 0f;
            isGamePaused = true;
        }
    }

    public void ResumeGame() // Executed by third-party UI resumeButton
    {
        if (!isPlayerDead && isGamePaused)
        {
            if (uiManager != null)
                uiManager.GamePaused(false);
            Time.timeScale = 1f;
            isGamePaused = false;
        }
    }

    private void TogglePause(InputAction.CallbackContext context) // Toggled with input action
    {
        if (!isPlayerDead)
        {
            if (isGamePaused)
            {
                ResumeGame(); // Resume the game if it's currently paused
            }
            else
            {
                PauseGame(); // Pause the game if it's not paused
            }
        }
    }
}
