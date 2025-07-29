using UnityEngine;
using StarterAssets;

public class JoystickSprintController : MonoBehaviour
{
    [Tooltip("Reference to the StarterAssetsInputs component")]
    public StarterAssetsInputs starterAssetsInputs;
    
    [Tooltip("Threshold beyond which sprint is activated")]
    public float sprintThreshold = 1.5f;

    void Update()
    {
        // Check the magnitude of the move input
        Vector2 moveInput = starterAssetsInputs.move;
        
        // Check if X or Y component exceeds threshold (positive or negative)
        bool shouldSprint = Mathf.Abs(moveInput.x) > sprintThreshold || 
                            Mathf.Abs(moveInput.y) > sprintThreshold;
        
        // Update the sprint state
        starterAssetsInputs.SprintInput(shouldSprint);
    }
}
