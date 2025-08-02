using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Texture Scroller System
/// Handles scrolling texture animation for conveyor belt visuals
/// </summary>
public class TextureScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Speed of texture scrolling")]
    public float scrollSpeed = 1f;
    
    [Header("Scroll Direction")]
    [Tooltip("Scroll texture on X axis")]
    public bool scrollX = false;
    
    [Tooltip("Scroll texture on Y axis")]
    public bool scrollY = true;
    
    [Tooltip("Scroll texture on Z axis (rarely used)")]
    public bool scrollZ = false;
    
    [Header("Control")]
    [Tooltip("Enable/disable texture scrolling")]
    public bool isScrolling = true;
    
    [Header("Conveyor Dependency")]
    [Tooltip("Reference to the conveyor script to sync with")]
    public Conveyor conveyorReference;
    
    [Tooltip("Auto-find conveyor in parent or current GameObject")]
    public bool autoFindConveyor = false;
    
    // Private variables
    private Renderer objectRenderer;
    private Material material;
    private Vector2 textureOffset;
    
    void Start()
    {
        // Get the renderer component
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"TextureScroller on '{gameObject.name}' requires a Renderer component!");
            enabled = false;
            return;
        }
        
        // Get the material (create instance to avoid modifying shared material)
        material = objectRenderer.material;
        if (material == null)
        {
            Debug.LogError($"TextureScroller on '{gameObject.name}' requires a material!");
            enabled = false;
            return;
        }
        
        // Initialize texture offset
        textureOffset = material.mainTextureOffset;
        
        // Auto-find conveyor if enabled
        if (autoFindConveyor && conveyorReference == null)
        {
            FindConveyorReference();
        }
    }
    
    /// <summary>
    /// Automatically find a conveyor script to reference
    /// </summary>
    private void FindConveyorReference()
    {
        // Check current GameObject first
        conveyorReference = GetComponent<Conveyor>();
        
        // If not found, check parent objects
        if (conveyorReference == null)
        {
            conveyorReference = GetComponentInParent<Conveyor>();
        }
        
        // If still not found, check child objects
        if (conveyorReference == null)
        {
            conveyorReference = GetComponentInChildren<Conveyor>();
        }
        
        if (conveyorReference != null)
        {
            Debug.Log($"TextureScroller: Auto-found conveyor reference on '{conveyorReference.gameObject.name}'");
        }
        else
        {
            Debug.LogWarning($"TextureScroller: No conveyor reference found for '{gameObject.name}'. Texture will scroll independently.");
        }
    }
    
    void Update()
    {
        if (!ShouldScroll() || material == null)
            return;
            
        ScrollTexture();
    }
    
    /// <summary>
    /// Check if texture should be scrolling based on conveyor state and local settings
    /// </summary>
    private bool ShouldScroll()
    {
        // Check local scrolling toggle
        if (!isScrolling) return false;
        
        // Check conveyor dependency if available
        if (conveyorReference != null)
        {
            return conveyorReference.isConveyorRunning;
        }
        
        // If no conveyor reference, use local setting only
        return true;
    }
    
    /// <summary>
    /// Handle texture scrolling based on enabled axes
    /// </summary>
    private void ScrollTexture()
    {
        float deltaTime = Time.deltaTime * scrollSpeed;
        
        // Calculate offset changes
        float offsetX = scrollX ? deltaTime : 0f;
        float offsetY = scrollY ? deltaTime : 0f;
        // Note: Z scrolling would require a different approach (like using a custom shader)
        
        // Update texture offset
        textureOffset.x += offsetX;
        textureOffset.y += offsetY;
        
        // Apply the offset to the material
        material.mainTextureOffset = textureOffset;
    }
    
    /// <summary>
    /// Reset texture offset to zero
    /// </summary>
    public void ResetTextureOffset()
    {
        textureOffset = Vector2.zero;
        if (material != null)
        {
            material.mainTextureOffset = textureOffset;
        }
    }
    
    /// <summary>
    /// Set scroll speed at runtime
    /// </summary>
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
    
    /// <summary>
    /// Toggle scrolling on/off
    /// </summary>
    public void ToggleScrolling()
    {
        isScrolling = !isScrolling;
    }
    
    /// <summary>
    /// Set the conveyor reference manually
    /// </summary>
    public void SetConveyorReference(Conveyor conveyor)
    {
        conveyorReference = conveyor;
    }
    
    /// <summary>
    /// Get the current conveyor reference
    /// </summary>
    public Conveyor GetConveyorReference()
    {
        return conveyorReference;
    }
    
    void OnDestroy()
    {
        // Clean up material instance
        if (material != null && objectRenderer != null)
        {
            DestroyImmediate(material);
        }
    }
}
