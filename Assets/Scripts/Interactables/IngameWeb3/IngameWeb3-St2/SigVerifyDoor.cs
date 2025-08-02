using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Stage 2 Door Controller for Signature Verification
/// Opens door when forced open and stays open
/// </summary>
public class SigVerifyDoor : MonoBehaviour
{
    [Header("Door Animation")]
    [Tooltip("Animation trigger name for door state (default: 'Door')")]
    public string doorAnimationParameter = "Door";
    
    [Header("Audio & Effects")]
    [Tooltip("Sound to play when door opens")]
    public AudioClip doorOpenSound;
    
    [Tooltip("Particle effect when door opens")]
    public ParticleSystem doorOpenEffect;
    
    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    // Private components
    private Animator animator;
    private AudioSource audioSource;
    
    // State tracking
    private bool isDoorOpen = false;
    
    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (showDebugInfo)
        {
            Debug.Log("SigVerifyDoor initialized - Simple mode");
        }
    }
    
    /// <summary>
    /// Force door open/close (called by SigVerifier)
    /// </summary>
    /// <param name="forceOpen">True to force open, false to close</param>
    public void ForceDoorState(bool forceOpen)
    {
        if (isDoorOpen == forceOpen) return; // Already in desired state
        
        isDoorOpen = forceOpen;
        
        // Update animator if available
        if (animator != null)
        {
            animator.SetBool(doorAnimationParameter, forceOpen);
        }
        
        if (forceOpen)
        {
            OnDoorOpened();
        }
        else
        {
            OnDoorClosed();
        }
    }
    
    /// <summary>
    /// Handle door opening effects and audio
    /// </summary>
    private void OnDoorOpened()
    {
        // Play door open sound
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Trigger particle effect
        if (doorOpenEffect != null)
        {
            doorOpenEffect.Play();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("SigVerifyDoor opened!");
        }
    }
    
    /// <summary>
    /// Handle door closing effects
    /// </summary>
    private void OnDoorClosed()
    {
        // Stop particle effect if running
        if (doorOpenEffect != null && doorOpenEffect.isPlaying)
        {
            doorOpenEffect.Stop();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("SigVerifyDoor closed.");
        }
    }
    
    /// <summary>
    /// Check if door is currently open
    /// </summary>
    /// <returns>True if door is open</returns>
    public bool IsDoorOpen()
    {
        return isDoorOpen;
    }
}
