using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    public Stage2Manager stage2Manager;

    public SlotTimer slotTimer;

    public GameObject WinScreen;
    public GameObject failedScreen;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            if (stage2Manager != null) stage2Manager.LevelComplete();
            WinScreen.SetActive(true);
        }
    }

    public void LevelFailed()
    {
        if (stage2Manager != null) stage2Manager.LevelFailed();
        failedScreen.SetActive(true);
    }
    
}
