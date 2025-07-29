using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    public GameObject button;  // Reference to the child button object
    public float pressDistance = 0.1f;  // How far the button gets pressed
    public bool isPressed = false;  // Boolean to track if the button is pressed
    public string[] pushableTags;  // Array of tags that can press the button
    public MeshRenderer meshRenderer;  // Reference to the MeshRenderer for the button's material

    private Vector3 initialPosition;
    private Vector3 pressedPosition;
    private Material buttonMaterial;  // Store the button's material

    public AudioClip buttonOnClip;
    public AudioClip buttonOffClip;
    private AudioSource buttonSource;

    private ControllerRumble controllerRumble;
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
        controllerRumble = FindObjectOfType<ControllerRumble>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other collider's tag matches any tag in the pushableTags array
        if (IsTagInList(other.tag))
        {
            PressButton();
            PlayButtonAudio(buttonOnClip);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTagInList(other.tag))
        {
            ReleaseButton();
            PlayButtonAudio(buttonOffClip);
        }
    }

    private bool IsTagInList(string tag)
    {
        foreach (string pushableTag in pushableTags)
        {
            if (tag == pushableTag)
            {
                return true;
            }
        }
        return false;
    }

    void PressButton()
    {
        if (!isPressed)
        {
            // Move button down
            button.transform.localPosition = pressedPosition;
            isPressed = true;
            controllerRumble.RumbleTheController(0.15f, 0.3f, 0f);
            // Enable emission on the button material
            EnableEmission(true);
            Debug.Log("Button Pressed!");
        }
    }

    void ReleaseButton()
    {
        if (isPressed)
        {
            // Move button back to initial position
            button.transform.localPosition = initialPosition;
            isPressed = false;
            controllerRumble.RumbleTheController(0.1f, 0.1f, 0.3f);
            // Disable emission on the button material
            EnableEmission(false);
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
}
