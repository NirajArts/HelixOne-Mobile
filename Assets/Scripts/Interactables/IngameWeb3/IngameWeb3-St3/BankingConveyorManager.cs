using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankingConveyorManager : MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>

    [Header("Banking Conveyor references")]
    public BankingConveyor bankingConveyor1; // To check which account crate and is it read or write, on conveyor 1
    public BankingConveyor bankingConveyor2; // To check which account crate and is it read or write, on conveyor 2

    [Header("Reference to Button Component")]
    public ConveyorButton conveyorButton;
    
    void Update()
    {
        // Only check for stopping conveyors automatically
        // Don't start them automatically - player must press button
        CheckAndStopIfNeeded();
    }
    
    /// <summary>
    /// Automatically stop conveyors if requirements are no longer met
    /// </summary>
    private void CheckAndStopIfNeeded()
    {
        // Only check if conveyors are currently running
        if (bankingConveyor1.isConveyorRunning || bankingConveyor2.isConveyorRunning)
        {
            bool requirementsMet = CheckConveyorRequirements();
            
            if (!requirementsMet)
            {
                StopConveyor();
                Debug.Log("Conveyors auto-stopped - requirements no longer met");
            }
        }
    }

    private void StartConveyor()    // Start conveyor when requirements are met
    {
        bankingConveyor1.isConveyorRunning = true;
        bankingConveyor2.isConveyorRunning = true;
        conveyorButton.PressButton(); // Visual feedback - button pressed
    }

    private void StopConveyor()     // Stop the conveyor when requirements don't meet.
    {
        bankingConveyor1.isConveyorRunning = false;
        bankingConveyor2.isConveyorRunning = false;
        conveyorButton.ReleaseButton(); // Visual feedback - button released
    }

    public void ToggleConveyor()
    {
        // Check if requirements meet for Solana account access rules
        bool canStartConveyor = CheckConveyorRequirements();
        
        if (canStartConveyor)
        {
            // Only start if not already running
            if (!bankingConveyor1.isConveyorRunning && !bankingConveyor2.isConveyorRunning)
            {
                StartConveyor();
                Debug.Log("Conveyors manually started - no conflicts detected");
            }
            else
            {
                Debug.Log("Conveyors already running");
            }
        }
        else
        {
            StopConveyor();
            Debug.Log("Cannot start conveyors - requirements not met");
        }
    }
    
    /// <summary>
    /// Check Solana account access conflict rules:
    /// - Both conveyors must have exactly one crate each
    /// - If crates have different accountNames (A vs B) = no conflict
    /// - If crates have same accountName + both are read (isRead=true) = no conflict  
    /// - If crates have same accountName + any write (isRead=false) = CONFLICT
    /// Rule: same account + any write = conflict (Solana can't process read+write or write+write on same account in parallel)
    /// </summary>
    private bool CheckConveyorRequirements()
    {
        // Check if both conveyors have exactly one crate (no null, no multiple crates)
        if (bankingConveyor1.bankingCrate == null || bankingConveyor2.bankingCrate == null)
        {
            Debug.Log("Requirement failed: One or both conveyors missing crates");
            return false;
        }
        
        if (bankingConveyor1.moreThanOneCrate || bankingConveyor2.moreThanOneCrate)
        {
            Debug.Log("Requirement failed: Multiple crates detected on conveyor(s)");
            return false;
        }
        
        // Get account information from both crates
        string account1 = bankingConveyor1.accountName;
        string account2 = bankingConveyor2.accountName;
        bool isRead1 = bankingConveyor1.isRead;
        bool isRead2 = bankingConveyor2.isRead;
        
        // Check for Solana account access conflicts
        if (account1 == account2) // Same account
        {
            // If both are read operations, no conflict
            if (isRead1 && isRead2)
            {
                Debug.Log($"Same account ({account1}) but both are READ operations - no conflict");
                return true;
            }
            else
            {
                // At least one is write operation - CONFLICT!
                Debug.Log($"CONFLICT: Same account ({account1}) with write operation detected! " +
                         $"Conveyor1: {(isRead1 ? "READ" : "WRITE")}, Conveyor2: {(isRead2 ? "READ" : "WRITE")}");
                return false;
            }
        }
        else
        {
            // Different accounts - no conflict possible
            Debug.Log($"Different accounts ({account1} vs {account2}) - no conflict");
            return true;
        }
    }
}
