using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay = 10f;

    void Start()
    {
        StartCoroutine(RemoveAfterDelay());
    }
    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // Destroy the gameObject after the delay
    }
}
