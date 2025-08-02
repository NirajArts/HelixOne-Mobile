using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TxCrate_Fake : MonoBehaviour
{
    public void DestroyThisObject()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SigVerifyConveyorManager"))
        {

            DestroyThisObject();
        }
        
        if (other.CompareTag("Trash"))
        {
            DestroyThisObject();
        }
        
    }
}
