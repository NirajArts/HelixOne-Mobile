using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveStats : MonoBehaviour
{
    public int damageToDealt = 110;
    public PlayerStats playerStats;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player")) 
        {
            playerStats.TakeDamage(damageToDealt);
            Debug.Log("Damage Dealt.");

        }
    }
}
