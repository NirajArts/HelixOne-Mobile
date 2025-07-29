using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public bool[] powerUps; // Boolean array for power-ups
    public string[] strings; // String array containing "True" or "False"

    public void FetchPowerUps()
    {
        // Ensure the powerUps array has the same size as strings
        if (strings.Length != powerUps.Length)
        {
            Debug.LogWarning("PowerUps and strings arrays are not the same length. Resizing powerUps array.");
            powerUps = new bool[strings.Length];
        }

        // Convert string values to boolean and assign them to powerUps
        for (int i = 0; i < strings.Length; i++)
        {
            powerUps[i] = strings[i].Equals("True", System.StringComparison.OrdinalIgnoreCase);
        }

        Debug.Log("Power-ups updated successfully!");
    }

    void Start()
    {
        FetchPowerUps();
    }
}
