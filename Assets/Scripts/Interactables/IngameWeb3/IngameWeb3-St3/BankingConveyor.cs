using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankingConveyor : MonoBehaviour
{

    [Header("Banking Conveyor Variables")]
    public BankingCrate bankingCrate;
    public Conveyor conveyor;
    public bool isConveyorRunning = false;
    public bool moreThanOneCrate = false;
    
    private int crateCount = 0; // Track number of crates on conveyor
    private List<BankingCrate> cratesOnConveyor = new List<BankingCrate>(); // Track all crates

    [Header("Account Details")]
    public string accountName;
    public bool isRead;

    private void HandleBankingConvayor()
    {
        conveyor.isConveyorRunning = isConveyorRunning;

    }

    void Update()
    {
        HandleBankingConvayor();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            Debug.Log("Pushbox Entered");
            crateCount++;
            moreThanOneCrate = crateCount > 1;
            
            BankingCrate newCrate = other.GetComponent<BankingCrate>();
            if (newCrate != null)
            {
                cratesOnConveyor.Add(newCrate);
                
                // Update current crate info (always use the latest one)
                bankingCrate = newCrate;
                accountName = newCrate.AccountName; // Use AccountName property, not GameObject name
                isRead = newCrate.isRead;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            Debug.Log("Pushbox Exited");
            crateCount--;
            moreThanOneCrate = crateCount > 1;
            
            BankingCrate exitingCrate = other.GetComponent<BankingCrate>();
            if (exitingCrate != null)
            {
                cratesOnConveyor.Remove(exitingCrate);
                
                // Update current crate info based on remaining crates
                if (cratesOnConveyor.Count > 0)
                {
                    // Set to the first remaining crate
                    bankingCrate = cratesOnConveyor[0];
                    accountName = cratesOnConveyor[0].AccountName; // Use AccountName property
                    isRead = cratesOnConveyor[0].isRead;
                }
                else
                {
                    // No crates left
                    bankingCrate = null;
                    accountName = null;
                }
            }
        }
    }

    public void StopConveyor()
    {
        if (moreThanOneCrate)
            isConveyorRunning = false;
    }
}
