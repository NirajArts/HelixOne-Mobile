using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The object the camera follows
    public Vector3 offset;   // Offset distance between the camera and the target
    public float smoothSpeed = 0.125f; // Adjust this for smoothness
    private Vector3 velocity = Vector3.zero; // For SmoothDamp

    public Camera mainCamera;
    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // Target position with offset
            Vector3 targetPosition = target.position + offset;

            // Smoothly move the camera towards the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
        }
    }
}
