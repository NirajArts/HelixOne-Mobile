using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabbableObject : MonoBehaviour
{
    public InputActionReference grabAction; // Input action for grabbing/ungrabbing
    public bool isGrabbed = false;         // To check if the object is currently grabbed
    public Transform playerHand;           // Reference to the player's hand or holding point
    private Rigidbody rb;                   // Rigidbody of the object
    public float grabSpeed = 5f;            // Speed for smooth grab movement

    [Header("Player Speeds")]
    public float updatedPlayerSpeed = 1.5f;
    public float updatedPlayerSprintSpeed = 4.8f;
    public float updatedJumpHeight = 0.1f;

    private ThirdPersonController tpController;
    private float initialPlayerSpeed = 2f;
    private float initialPlayerSprintSpeed = 5.335f;
    private float initialJumpHeight = 1.2f;

    private BoxRespawn boxRespawn;          // Reference to BoxRespawn Component

    [Header("Haptic Feedback Parameters")]       // Mobile haptic vibration
    public float hapticIntensity = 0.25f;
    private MobileHaptics mobileHaptics;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxRespawn = GetComponent<BoxRespawn>();
        mobileHaptics = MobileHaptics.Instance;

        if (rb == null)
        {
            Debug.LogWarning("Rigidbody not found on GrabbableObject.");
        }

        if (boxRespawn == null)
        {
            Debug.LogWarning("BoxRespawn component not found on GrabbableObject.");
        }

        // Start coroutine to delay execution of player speed setup
        StartCoroutine(InitializePlayerSpeedAfterDelay(4f));
    }
    
    private IEnumerator InitializePlayerSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Execute this part after the delay
        tpController = FindObjectOfType<ThirdPersonController>();
        if (tpController != null)
        {
            initialPlayerSpeed = tpController.MoveSpeed;
            initialPlayerSprintSpeed = tpController.SprintSpeed;
            initialJumpHeight = tpController.JumpHeight;
        }
    }

    // Update Player attributes when Grabbed or Released the Box
    private void UpdatePlayerSpeed(float moveSpeed, float sprintSpeed, float jumpHeight)
    {
        if (tpController != null)
        {
            tpController.MoveSpeed = moveSpeed;
            tpController.SprintSpeed = sprintSpeed;
            tpController.JumpHeight = jumpHeight;
        }
    }

    private void OnEnable()
    {
        // Enable the grab action and listen for it to be performed
        grabAction.action.Enable();
        grabAction.action.performed += ToggleGrab;
    }

    private void OnDisable()
    {
        // Disable the grab action and unsubscribe
        grabAction.action.performed -= ToggleGrab;
        grabAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is triggered by the player's hands
        if (other.CompareTag("Player Hands"))
        {
            playerHand = other.transform; // Assume the "Player" has a Transform point to hold the object
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Clear the reference when the player leaves the trigger
        if (other.CompareTag("Player Hands") && !isGrabbed)
        {
            playerHand = null;
        }
    }

    private void ToggleGrab(InputAction.CallbackContext context)
    {
        if (playerHand != null) // Check if the player is in range
        {
            isGrabbed = !isGrabbed; // Toggle the grabbed state

            if (isGrabbed)
            {
                Grab();
            }
            else
            {
                Release();
            }
        }
    }

    private void FixedUpdate()
    {
        // If the object is grabbed, smoothly move towards the player's hand position and rotation
        if (isGrabbed && playerHand != null)
        {
            Vector3 targetPosition = playerHand.position;
            Quaternion targetRotation = playerHand.rotation;

            // Smoothly interpolate position and rotation
            rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * grabSpeed));
            rb.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * grabSpeed));
        }
    }

    private void Grab()
    {
        if (rb != null)
        {
            rb.useGravity = false;  // Optional: turn off gravity while grabbing
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Freeze rotation if needed
            UpdatePlayerSpeed(updatedPlayerSpeed, updatedPlayerSprintSpeed, updatedJumpHeight);
            
            // Trigger haptic feedback when grabbing
            if (mobileHaptics != null)
            {
                mobileHaptics.TriggerHaptic(hapticIntensity);
            }
        }

        if (boxRespawn != null)
        {
            boxRespawn.allowInputRespawn = false; // Disable input respawn while grabbed
        }
    }

    public void Release()
    {
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None; // Remove rotation constraints
            UpdatePlayerSpeed(initialPlayerSpeed, initialPlayerSprintSpeed, initialJumpHeight);
            
            // Trigger lighter haptic feedback when releasing
            if (mobileHaptics != null)
            {
                mobileHaptics.TriggerHaptic(hapticIntensity * 0.7f);
            }
        }

        if (boxRespawn != null)
        {
            boxRespawn.allowInputRespawn = true; // Re-enable input respawn when released
        }
    }
}
