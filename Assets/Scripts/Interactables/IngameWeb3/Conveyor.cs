using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Conveyor Belt Physics System
/// Handles movement of rigidbody objects - no visuals, pure physics
/// Attach this to an invisible collider GameObject for conveyor functionality
/// </summary>
public class Conveyor : MonoBehaviour
{
    public float speed = 1f;
    public bool isConveyorRunning = true;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!isConveyorRunning) return;
        
        Vector3 pos = rb.position;
        rb.position += speed * Time.fixedDeltaTime * Vector3.left;
        rb.MovePosition(pos);
    }
}
