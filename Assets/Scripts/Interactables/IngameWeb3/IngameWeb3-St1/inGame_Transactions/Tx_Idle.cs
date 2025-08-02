using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tx_Idle : MonoBehaviour
{
    [Header("Scale Pulse Animation")]
    [Tooltip("Speed of the scale pulsing animation")]
    public float scaleSpeed = 2f;
    
    [Tooltip("Amount of scale variation (0.1 = 10% size change)")]
    [Range(0.01f, 0.5f)]
    public float scaleAmount = 0.1f;
    
    [Header("Vertical Movement Animation")]
    [Tooltip("Speed of the up-down movement")]
    public float verticalSpeed = 1.5f;
    
    [Tooltip("Length/distance of the up-down movement")]
    [Range(0.1f, 2f)]
    public float verticalLength = 0.5f;
    
    [Header("Animation Settings")]
    [Tooltip("Enable/disable the idle animation")]
    public bool enableAnimation = true;
    
    [Tooltip("Random offset to prevent synchronized animations")]
    public bool useRandomOffset = true;
    
    // Private variables to store initial values
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private float timeOffset;
    
    void Start()
    {
        // Store the initial position and scale
        initialPosition = transform.position;
        initialScale = transform.localScale;
        
        // Add random offset if enabled to prevent all objects animating in sync
        if (useRandomOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
        else
        {
            timeOffset = 0f;
        }
    }

    void Update()
    {
        if (!enableAnimation) return;
        
        // Calculate time with offset for varied animations
        float time = Time.time + timeOffset;
        
        // Scale pulsing animation
        float scaleMultiplier = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmount;
        transform.localScale = initialScale * scaleMultiplier;
        
        // Vertical movement animation
        float verticalOffset = Mathf.Sin(time * verticalSpeed) * verticalLength;
        Vector3 newPosition = initialPosition;
        newPosition.y += verticalOffset;
        transform.position = newPosition;
    }
    
    /// <summary>
    /// Enable or disable the idle animation
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public void SetAnimationEnabled(bool enable)
    {
        enableAnimation = enable;
        
        if (!enable)
        {
            // Reset to initial values when disabled
            transform.position = initialPosition;
            transform.localScale = initialScale;
        }
    }
    
    /// <summary>
    /// Reset the animation to its initial state
    /// </summary>
    public void ResetAnimation()
    {
        transform.position = initialPosition;
        transform.localScale = initialScale;
        
        // Generate new random offset
        if (useRandomOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    /// <summary>
    /// Update the initial position (useful if the object is moved)
    /// </summary>
    public void UpdateInitialPosition()
    {
        initialPosition = transform.position;
    }
    
    /// <summary>
    /// Update the initial scale (useful if the object is resized)
    /// </summary>
    public void UpdateInitialScale()
    {
        initialScale = transform.localScale;
    }
}
