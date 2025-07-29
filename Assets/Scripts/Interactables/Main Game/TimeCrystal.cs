using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCrystal : MonoBehaviour
{
    private DestroyAfterDelay destroyAfterDelay;
    private AudioSource audioSource;
    private Collider collider;

    public GameObject[] crystalPieces;
    public GameObject idleparticleEffects;
    public GameObject collectEffect;

    void Start()
    {
        destroyAfterDelay = GetComponent<DestroyAfterDelay>();
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            destroyAfterDelay.enabled = true;
            if(audioSource != null)
            {
                audioSource.Play();
            }
            foreach (GameObject go in crystalPieces)
            {
                if(go != null)
                {
                    go.SetActive(false);
                }
            }
            if(idleparticleEffects != null)
            {
                idleparticleEffects.SetActive(false);
            }

            if (collectEffect != null)
            {
                collectEffect.SetActive(true);
            }
            if(collider != null)
            {
                collider.enabled = false;
            }

        }

    }

}

