using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;  // Assuming your custom inputs are in the StarterAssets namespace

public class PlayerMotor : MonoBehaviour
{
    public CharacterController controller; // The player's CharacterController
    public StarterAssetsInputs input;      // Input system (StarterAssetsInputs)
    public Animator animator;              // Animator for controlling animations
    public Transform playerModel;          // Model or object that rotates with movement
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 720f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.0f;
    public Transform groundCheck;          // For ground checking
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    // Animation parameter hashes
    private int speedHash = Animator.StringToHash("Speed");
    private int jumpHash = Animator.StringToHash("Jump");
    private int groundedHash = Animator.StringToHash("Grounded");
    private int freeFallHash = Animator.StringToHash("FreeFall");
    private int motionSpeedHash = Animator.StringToHash("MotionSpeed");

    private void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Set grounded state in animator
        animator.SetBool(groundedHash, isGrounded);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Reset y velocity when grounded
        }

        // Movement input
        Vector2 moveInput = input.move;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // Move the player based on sprint or walk
        float targetSpeed = input.sprint ? runSpeed : walkSpeed;
        Vector3 move = moveDirection * targetSpeed;

        controller.Move(move * Time.deltaTime);

        // Calculate speed and pass to animator
        float speedPercent = move.magnitude / runSpeed;
        animator.SetFloat(speedHash, move.magnitude); // Controls transition from idle to walk to run
        animator.SetFloat(motionSpeedHash, speedPercent); // Additional speed control

        // Rotation
        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(playerModel.eulerAngles.y, targetAngle, ref velocity.y, rotationSpeed * Time.deltaTime);
            playerModel.rotation = Quaternion.Euler(0, angle, 0);
        }

        // Jumping
        if (input.jump && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool(jumpHash, true);  // Trigger jump animation
        }

        // Falling and Gravity
        if (!isGrounded)
        {
            animator.SetBool(freeFallHash, true); // Trigger in-air animation
        }
        else
        {
            animator.SetBool(freeFallHash, false); // Reset in-air state when grounded
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Reset jump state when grounded
        if (isGrounded)
        {
            animator.SetBool(jumpHash, false); // Reset jump animation state
        }
    }
}
