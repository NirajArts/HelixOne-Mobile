using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject playerGameObject;   // Reference to the player prefab
    public GameObject spawnEffect;        // Effect to play when spawning
    public float timeToSpawnPlayer = 2f;  // Delay before spawning player
    public AudioClip spawnSound;          // Sound to play on spawn

    private bool isSpawned = false;
    private AudioSource audioSource;      // Audio source for playing spawn sound
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Optionally start spawning immediately
        Invoke(nameof(SpawnPlayer), timeToSpawnPlayer);
        SpawnEffect();

        // Play spawn sound
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Add any additional logic if needed
    }

    // Method to spawn the player
    void SpawnPlayer()
    {
        if (!isSpawned)
        {
            // Spawn the player at the current location and rotation
            if (playerGameObject != null)
            {
                playerGameObject.SetActive(true);
            }

            // Mark the player as spawned
            isSpawned = true;
        }
    }

    void SpawnEffect()
    {
        // Play spawn effect
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, transform.position, Quaternion.identity);
        }
    }


}
