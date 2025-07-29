using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryPlatform : MonoBehaviour
{
    public Animator platformAnimator;
    public float timeToDeletePlatform = 0.75f;
    private bool isPlayerOn = false;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        if (isPlayerOn)
        {
            timeToDeletePlatform -= Time.deltaTime;

            if(timeToDeletePlatform < 0)
            {
                platformAnimator.gameObject.SetActive(false);

            }
            if(timeToDeletePlatform < -1)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Pushable"))
        {
            isPlayerOn = true;
            audioSource.enabled = true;
            platformAnimator.SetTrigger("Dissolve");
        }
    }
}
