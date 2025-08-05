using UnityEngine;
using UnityEngine.UI;

public class ConveyorButton : MonoBehaviour
{
    public GameObject button;  // Reference to the child button object
    public float pressDistance = 0.1f;  // How far the button gets pressed
    public bool isPressed = false;  // Boolean to track if the button is pressed
    public MeshRenderer meshRenderer;  // Reference to the MeshRenderer for the button's material

    private Vector3 initialPosition;
    private Vector3 pressedPosition;
    private Material buttonMaterial;  // Store the button's material

    public AudioClip buttonOnClip;
    public AudioClip buttonOffClip;
    private AudioSource buttonSource;

    public BankingConveyorManager conveyorManager;

    [Header("UI Button")]
    public Button toggleButton;
    private void Start()
    {
        // Store the initial position of the button
        initialPosition = button.transform.localPosition;
        pressedPosition = initialPosition - new Vector3(0, pressDistance, 0);  // Adjust for press direction (downwards)

        // Get the material from the MeshRenderer
        if (meshRenderer != null)
        {
            buttonMaterial = meshRenderer.material;  // Clone the material to avoid modifying the original
        }

        buttonSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Unhide toggle button UI Button component
            if (toggleButton != null)
                toggleButton.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide toggle button UI Button component
            if (toggleButton != null)
                toggleButton.gameObject.SetActive(false);
        }
    }

    public void CheckRequirements()
    {
        // Check if the Accounts and read/write requirements meet in BankingConveyorMananger
    }

    // Call it in BankingConveyorMananger if requirements meet
    public void PressButton()
    {
        if (!isPressed)
        {
            // Move button down
            button.transform.localPosition = pressedPosition;
            isPressed = true;
            // Enable emission on the button material
            EnableEmission(true);
            PlayButtonAudio(buttonOnClip);
            Debug.Log("Button Pressed!");
        }
    }

    // Call it in BankingConveyorMananger if requirements doesnt meet or conveyor completed the cycle.
    public void ReleaseButton()
    {
        if (isPressed)
        {
            // Move button back to initial position
            button.transform.localPosition = initialPosition;
            isPressed = false;
            // Disable emission on the button material
            EnableEmission(false);
            PlayButtonAudio(buttonOffClip);
            Debug.Log("Button Released!");
        }
    }

    private void EnableEmission(bool isEnabled)
    {
        if (buttonMaterial != null)
        {
            if (isEnabled)
            {
                buttonMaterial.EnableKeyword("_EMISSION");  // Enable emission
            }
            else
            {
                buttonMaterial.DisableKeyword("_EMISSION"); // Disable emission
            }
        }
    }

    void PlayButtonAudio(AudioClip audioClip)
    {
        if (audioClip != null)
            buttonSource.PlayOneShot(audioClip);
    }

    public void ToggleConveyor()
    {
        conveyorManager.ToggleConveyor();
    }
}
