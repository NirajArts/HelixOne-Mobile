using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionImpact : MonoBehaviour
{
    public float explosionRadius = 5f;    // Radius within which objects are affected
    public float explosionForce = 700f;   // The force of the explosion
    public float upwardsModifier = 1f;    // Modifier for upwards force direction
    public LayerMask affectedLayers;      // Layer mask to specify what objects are affected by the explosion

    public void ApplyExplosion(Vector3 explosionPosition)
    {
        // Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius, affectedLayers);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            // If the object has a Rigidbody, apply an explosion force
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }
    }
}
