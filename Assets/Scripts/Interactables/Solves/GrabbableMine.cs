using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabbableMine : MonoBehaviour
{
    public InputActionReference grabAction; // Input action for grabbing/ungrabbing
    public bool isGrabbed = false;          // To check if the mine is currently grabbed
    public Transform playerHand;            // Reference to the player's hand or holding point
    private Rigidbody rb;                   // Rigidbody of the mine
    public float grabSpeed = 5f;            // Speed for smooth grab movement

    [Header("Player Speeds")]
    public float updatedPlayerSpeed = 1.5f;
    public float updatedPlayerSprintSpeed = 4.8f;
    public float updatedJumpHeight = 0.25f;

    private ThirdPersonController tpController;
    private float initialPlayerSpeed = 2f;
    private float initialPlayerSprintSpeed = 5.335f;
    private float initialJumpHeight = 1.2f;

    private MineRespawn mineRespawn;        // Reference to MineRespawn component

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mineRespawn = GetComponent<MineRespawn>(); // Get the MineRespawn component

        if (rb == null)
        {
            Debug.LogWarning("Rigidbody not found on GrabbableMine.");
        }

        if (mineRespawn == null)
        {
            Debug.LogWarning("MineRespawn component not found on GrabbableMine.");
        }

        // Start coroutine to delay execution of player speed setup
        StartCoroutine(InitializePlayerSpeedAfterDelay(4f));
    }

    private IEnumerator InitializePlayerSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        tpController = FindObjectOfType<ThirdPersonController>();
        if (tpController != null)
        {
            initialPlayerSpeed = tpController.MoveSpeed;
            initialPlayerSprintSpeed = tpController.SprintSpeed;
            initialJumpHeight = tpController.JumpHeight;
        }
    }

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
        grabAction.action.Enable();
        grabAction.action.performed += ToggleGrab;
    }

    private void OnDisable()
    {
        grabAction.action.performed -= ToggleGrab;
        grabAction.action.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player Hands"))
        {
            playerHand = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player Hands") && !isGrabbed)
        {
            playerHand = null;
        }
    }

    private void ToggleGrab(InputAction.CallbackContext context)
    {
        if (playerHand != null)
        {
            isGrabbed = !isGrabbed;

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
        if (isGrabbed && playerHand != null)
        {
            Vector3 targetPosition = playerHand.position;
            rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * grabSpeed));
        }
    }

    private void Grab()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            //            rb.constraints = RigidbodyConstraints.FreezeRotation;
            UpdatePlayerSpeed(updatedPlayerSpeed, updatedPlayerSprintSpeed, updatedJumpHeight);
        }

        if (mineRespawn != null)
        {
            mineRespawn.allowInputRespawn = false; // Disable input respawn while grabbed
        }
    }

    public void Release()
    {
        if (rb != null)
        {
            rb.useGravity = true;
            //            rb.constraints = RigidbodyConstraints.None;
            UpdatePlayerSpeed(initialPlayerSpeed, initialPlayerSprintSpeed, initialJumpHeight);
        }

        if (mineRespawn != null)
        {
            mineRespawn.allowInputRespawn = true; // Re-enable input respawn when released
        }
    }
}
