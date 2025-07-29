using UnityEngine;

public class BoxRespawn : MonoBehaviour
{
    public string triggerTag = "BoxRespawnTrigger";    // Tag that triggers the dissolve and respawn
    public bool isBoxDissolved = false;


    private Vector3 initialPosition;                // Original position of the box
    private Quaternion initialRotation;             // Original rotation of the box

    public Animator animator;                       // Reference to Animator for dissolve/spawn animations
    private Collider mainCollider;                  // Main collider of the box
    private Rigidbody boxRigidbody;                 // Rigidbody of the box
    public GameObject extraColliderObject;          // Additional collider object

    public bool allowInputRespawn = true;
    private GrabbableObject grabbableObject;

    [Header("Haptic Feedback Parameters")]       // Mobile haptic vibration
    public float hapticIntensity = 0.2f;
    private MobileHaptics mobileHaptics;
    
    void Start()
    {
        // Save the initial position and rotation of the box
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Get Collider, and Rigidbody components
        mainCollider = GetComponent<Collider>();
        boxRigidbody = GetComponent<Rigidbody>();
        grabbableObject = GetComponent<GrabbableObject>();
        mobileHaptics = MobileHaptics.Instance;

        // Ensure the Animator component exists
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on the box object.");
        }

        // Initially deactivate the extra collider object
        if (extraColliderObject != null)
        {
            extraColliderObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the box collides with the specified tag
        if (other.CompareTag(triggerTag) && !isBoxDissolved)
        {
            StartDissolve();
            if (grabbableObject != null)
            {
                grabbableObject.Release();
                grabbableObject.playerHand = null;
                grabbableObject.isGrabbed = false;
            }
        }
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Box Triggered with player");
            other.GetComponent<BoxRespawnHandler>().ResetDeleteRefTime();
            other.GetComponent<BoxRespawnHandler>().boxRespawn = this;
        }
    }


    public void StartDissolve()
    {
        isBoxDissolved = true;

        // Set Rigidbody to kinematic and disable the main collider
        if (boxRigidbody != null)
        {
            boxRigidbody.isKinematic = true;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        // Activate dissolve animation
        if (animator != null)
        {
            animator.SetTrigger("Dissolve"); // Ensure an animation trigger named "Dissolve" exists
        }

        // Deactivate extra collider object during dissolve
        if (extraColliderObject != null)
        {
            extraColliderObject.SetActive(false);
        }
        
        // Trigger haptic feedback when box dissolves
        if (mobileHaptics != null)
        {
            mobileHaptics.TriggerHaptic(hapticIntensity);
        }

        // Delay the respawn
        Invoke(nameof(RespawnBox), 1.0f); // Adjust delay as needed
    }

    private void RespawnBox()
    {
        // Reset position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Trigger spawn animation
        if (animator != null)
        {
            animator.SetTrigger("Spawn"); // Ensure an animation trigger named "Spawn" exists
        }

        // Re-enable the main collider and set Rigidbody back to non-kinematic
        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        if (boxRigidbody != null)
        {
            boxRigidbody.isKinematic = false;
        }
        
        // Trigger lighter haptic feedback when box respawns
        if (mobileHaptics != null)
        {
            mobileHaptics.TriggerHaptic(hapticIntensity * 0.5f);
        }
        
        // Activate the extra collider object with a delay, if applicable
        Invoke(nameof(EnableExtraCollider), 1.0f); // Adjust timing as needed

        isBoxDissolved = false;
    }

    private void EnableExtraCollider()
    {
        if (extraColliderObject != null)
        {
            extraColliderObject.SetActive(true);
        }
    }
}
