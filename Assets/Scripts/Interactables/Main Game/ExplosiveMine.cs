using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMine : MonoBehaviour
{
    [Tooltip("Reference to the button press component you want to use to trigger the explosion")]
    private ButtonPress buttonPressInput;
    private bool startExplosion = false;
    private bool hasExploded = false; // To prevent multiple explosions

    public float timeToExplode = 2f; // Time before explosion occurs
    public float timeToRemoveResidues = 3f; // Time before removing residues after explosion
    public GameObject explosionGameObject; // GameObject to activate upon explosion
    public GameObject explosionAnticipationSound;

    Collider[] colliders;

    public GameObject explosionDecal;

    private MeshRenderer meshRenderer;
    public MeshRenderer buttonMeshRenderer;

    private ExplosionImpact explosionImpact;  // Reference to ExplosionImpact script

    [Header("Caryable Mine Parents optional")]
    public bool isThisCarryable = false;
    public MeshRenderer OptionalTableRenderer;
    public Collider OptionalCollider;
    public Rigidbody TableRigidBody;

    [Header("Rumble (Vibration) Parameters")]       // Vibrating the gamepad
    public float rumbleMultiplier = 5f;
    public float rumbleDuration = 0.5f;
    private ControllerRumble controllerRumble;
    private void Start()
    {
        buttonPressInput = GetComponent<ButtonPress>();
        meshRenderer = GetComponent<MeshRenderer>();
        explosionImpact = GetComponent<ExplosionImpact>();  // Get the ExplosionImpact component
        colliders = GetComponents<Collider>(); // Get all colliders attached to this GameObject
        controllerRumble = FindObjectOfType<ControllerRumble>();
    }

    private void Update() // Use Update for checking button press
    {
        if (!hasExploded && buttonPressInput != null && buttonPressInput.isPressed)
        {
            startExplosion = true;
            if (explosionAnticipationSound != null)
            {
                explosionAnticipationSound.SetActive(true);
            }
        }

        if (startExplosion && !hasExploded)
        {
            timeToExplode -= Time.deltaTime;
            if (timeToExplode <= 0)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        if (hasExploded) return;  // Prevent multiple explosions

        hasExploded = true;  // Mark the explosion as occurred
        startExplosion = false;  // Stop explosion timer

        //Remove Table if this is carryable mine
        if (isThisCarryable)
            RemoveTable();

        // Disable the mine's visuals
        meshRenderer.enabled = false;
        if (buttonMeshRenderer != null)
        {
            buttonMeshRenderer.enabled = false;
        }

        // Disable colliders
        foreach (Collider collider in colliders)
        {
            if (collider != null)
                collider.enabled = false;
        }

        // Enable explosion effect
        if (explosionGameObject != null)
        {
            explosionGameObject.SetActive(true);
        }

        // Trigger explosion impact
        if (explosionImpact != null)
        {
            explosionImpact.ApplyExplosion(transform.position);  // Pass the position of the mine
        }

        // Spawn explosion decal
        if (explosionDecal != null)
        {
            Instantiate(explosionDecal, transform.position, Quaternion.identity);
        }

        // Start the coroutine to handle explosion timing
        StartCoroutine(RemoveAfterDelay());

        //Rumble the Gamepad
        controllerRumble.RumbleTheController(rumbleDuration, 0.1f * rumbleMultiplier, 0.1f * rumbleMultiplier);

        // Deactivate the anticipation sound
        if (explosionAnticipationSound != null)
        {
            explosionAnticipationSound.SetActive(false);
        }
    }

    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(timeToRemoveResidues);
        Destroy(gameObject); // Destroy the mine after the delay
    }

    private void RemoveTable()
    {
        if (OptionalCollider != null)
        {
            OptionalCollider.enabled = false;
        }
        if (OptionalTableRenderer != null)
        {
            OptionalTableRenderer.enabled = false;
        }
        if(TableRigidBody != null)
        {
            TableRigidBody.isKinematic = false;
        }
    }

}
