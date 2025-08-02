using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stage 1 specific timer controller that handles transaction completion logic.
/// This script connects SlotTimer with InGame_TxManager for Stage 1 gameplay.
/// </summary>
public class Stage1TimerController : BaseStageTimerController
{
    [Header("Stage 1 Specific References")]
    [Tooltip("Reference to the transaction manager")]
    public InGame_TxManager txManager;
    
    protected override void Start()
    {
        // Auto-find InGame_TxManager if enabled
        if (autoFindReferences && txManager == null)
        {
            txManager = InGame_TxManager.Instance;
        }
        
        // Validate transaction manager reference
        if (txManager == null)
        {
            Debug.LogError("Stage1TimerController: InGame_TxManager not found! Make sure it exists in the scene.");
            return;
        }
        
        // Call base Start method
        base.Start();
    }
    
    /// <summary>
    /// Configure the SlotTimer for Stage 1 transaction collection logic
    /// </summary>
    protected override void SetupStageLogic()
    {
        // Set the completion condition: all transactions collected
        slotTimer.SetCompletionCondition(CheckStageCompletionCondition);
        
        // Set the timer complete behavior: always trigger level failed on timeout
        slotTimer.SetTimerCompleteBehavior(HandleStageTimerCompletion);
        
        if (showDebug)
        {
            Debug.Log("Stage 1 logic configured: Timer will complete when all transactions are collected");
        }
    }
    
    /// <summary>
    /// Check if all transactions have been collected (Stage 1 specific logic)
    /// </summary>
    /// <returns>True if all transactions are collected</returns>
    protected override bool CheckStageCompletionCondition()
    {
        if (txManager == null) return false;
        
        // Check if total collected equals total available
        bool allCollected = txManager.totalTxCount >= txManager.totalTxToCollect && txManager.totalTxToCollect > 0;
        
        if (allCollected && showDebug)
        {
            Debug.Log($"Stage1TimerController: All transactions collected: {txManager.totalTxCount}/{txManager.totalTxToCollect}");
        }
        
        return allCollected;
    }
    
    /// <summary>
    /// Handle what happens when timer completes for Stage 1
    /// </summary>
    /// <returns>True to trigger level failed (Stage 1 always fails on timeout)</returns>
    protected override bool HandleStageTimerCompletion()
    {
        // Stage 1: Always trigger level failed when timer expires
        return true;
    }
    
    /// <summary>
    /// Get current transaction progress for Stage 1
    /// </summary>
    /// <returns>Progress from 0 to 1</returns>
    public override float GetStageProgress()
    {
        if (txManager == null || txManager.totalTxToCollect <= 0) return 0f;
        
        return (float)txManager.totalTxCount / txManager.totalTxToCollect;
    }
    
    /// <summary>
    /// Reset Stage 1 data
    /// </summary>
    public override void ResetStageData()
    {
        if (txManager != null)
        {
            txManager.ResetAllCounters();
        }
        
        if (showDebug)
        {
            Debug.Log("Stage1TimerController: Stage data reset");
        }
    }
    
    /// <summary>
    /// Get remaining transactions to collect
    /// </summary>
    /// <returns>Number of transactions remaining</returns>
    public int GetRemainingTransactions()
    {
        if (txManager == null) return 0;
        
        return Mathf.Max(0, txManager.totalTxToCollect - txManager.totalTxCount);
    }
}
