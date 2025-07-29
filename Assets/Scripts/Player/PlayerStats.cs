using StarterAssets;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerStats : MonoBehaviour
{
    public int health = 100;  // Starting health of the player
    private int _health;      // Internal variable for tracking current health

    private Animator _animator;

    [Header("Objects/Components to disable after death")]
    private ThirdPersonController _thirdPersonController; // Reference to player movement script
    private CharacterController _characterController;     // Reference to the CharacterController component
    private PlayerInput _playerInput;                     // Reference to player input controls

    [Header("Death Settings")]
    public bool isDead = false;   // To check if the player is dead

    [Header("Sending Messages")]
    private GameLevelManager _gameLevelManager;

    [Header("Haptic Feedback Parameters")]       // Mobile haptic vibration
    public float deathHapticIntensity = 0.5f;
    private MobileHaptics mobileHaptics;
    private void Start()
    {
        _health = health;

        // Initialize component references
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _gameLevelManager = FindObjectOfType<GameLevelManager>();
        mobileHaptics = MobileHaptics.Instance;

    }

    private void Update()
    {
        // Check if the player is dead
        if (_health <= 0 && !isDead)
        {
            PlayerDeath();
            _gameLevelManager.SendMessage("PlayerIsDead");
        }
    }

    // Call this function to reduce player's health
    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            _health -= damage;
            Debug.Log("Player took damage: " + damage + ". Current health: " + _health);

            if (_health <= 0)
            {
                _health = 0;
                PlayerDeath();
                _gameLevelManager.SendMessage("PlayerIsDead");
            }
        }
    }

    // Function that handles player death
    private void PlayerDeath()
    {
        isDead = true;
        Debug.Log("Player has died!");

        // Disable movement and controls
/*        if (_thirdPersonController != null)
            _thirdPersonController.enabled = false;

        if (_characterController != null)
            _characterController.enabled = false;
*/
        if (_playerInput != null)
            _playerInput.enabled = false;

        if (_animator != null)
            _animator.SetBool("isDead", true);

        // Trigger strong haptic feedback on death
        if (mobileHaptics != null)
        {
            mobileHaptics.TriggerHaptic(deathHapticIntensity);
        }
        
        // You can add other death-related behavior here, like playing a death animation or UI changes
    }

    // Optionally, you can add a method to revive the player, resetting health and re-enabling controls
    public void RevivePlayer()
    {
        if (isDead)
        {
            _health = health;  // Reset health to its original value
            isDead = false;

            // Re-enable movement and controls
/*            if (_thirdPersonController != null)
                _thirdPersonController.enabled = true;

            if (_characterController != null)
                _characterController.enabled = true;
*/
            if (_playerInput != null)
                _playerInput.enabled = true;

            if (_animator != null)
                _animator.SetBool("isDead", false);

            Debug.Log("Player revived with full health: " + _health);
        }
    }
}
