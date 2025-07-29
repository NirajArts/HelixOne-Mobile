using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxRespawnHandler : MonoBehaviour
{
    public InputActionReference respawnAction;

    public BoxRespawn boxRespawn;
    public float timeToDeleteReference = 5f;
    public float _timeToDeleteRef;

    private void Start()
    {
        _timeToDeleteRef = timeToDeleteReference;
    }

    private void Update()
    {
        _timeToDeleteRef -= Time.deltaTime;
        if( _timeToDeleteRef < 0 )
            boxRespawn = null;
    }

    private void OnEnable()
    {
        // Register the callback for respawn action
        respawnAction.action.performed += OnRespawnAction;
    }

    private void OnDisable()
    {
        // Unregister the callback for respawn action
        respawnAction.action.performed -= OnRespawnAction;
    }

    public void ResetDeleteRefTime()
    {
        _timeToDeleteRef = timeToDeleteReference;
    }

    private void OnRespawnAction(InputAction.CallbackContext context)
    {
        // Execute StartDissolve once when respawn action is triggered
        if (boxRespawn != null && boxRespawn.allowInputRespawn)
        {
            boxRespawn.StartDissolve();
            boxRespawn = null;
        }
    }
}
