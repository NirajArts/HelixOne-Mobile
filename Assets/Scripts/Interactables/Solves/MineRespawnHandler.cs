using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MineRespawnHandler : MonoBehaviour
{
    public InputActionReference respawnAction;

    public MineRespawn mineRespawn; // Reference to the MineRespawn script
    public float timeToDeleteReference = 5f; // Time before clearing reference to Mine
    private float _timeToDeleteRef;

    private void Start()
    {
        _timeToDeleteRef = timeToDeleteReference;
    }

    private void Update()
    {
        _timeToDeleteRef -= Time.deltaTime;
        if (_timeToDeleteRef < 0)
            mineRespawn = null;
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
        // Execute StartDissolve on the mine when respawn action is triggered
        if (mineRespawn != null && mineRespawn.allowInputRespawn)
        {
            mineRespawn.StartDissolve();
            mineRespawn = null;
        }
    }
}
