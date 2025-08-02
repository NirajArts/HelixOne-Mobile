using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransactionDoor : MonoBehaviour
{
    [Tooltip("Reference to the button press component you want to use to trigger open the door")]
    public ButtonPress buttonPressInput;
    
    [Header("Transaction Collection Door")]
    [Tooltip("Open door when all transactions are collected (overrides button input)")]
    public bool openOnAllTransactionsCollected = true;
    
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool shouldOpenDoor = false;
        
        // Check if door should open based on transaction collection
        if (openOnAllTransactionsCollected && InGame_TxManager.Instance != null)
        {
            int totalToCollect = InGame_TxManager.Instance.totalTxToCollect;
            int totalCollected = InGame_TxManager.Instance.totalTxCount;
            
            // Open door if all transactions are collected and there are transactions to collect
            if (totalToCollect > 0 && totalCollected >= totalToCollect)
            {
                shouldOpenDoor = true;
            }
        }
        
        // Check button input if transaction collection doesn't trigger the door
        if (!shouldOpenDoor && buttonPressInput != null)
        {
            shouldOpenDoor = buttonPressInput.isPressed;
        }
        
        // Update animator
        animator.SetBool("Door", shouldOpenDoor);
    }
}
