using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class retrieveItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameManager.instance.hasItem = true; 
        gameObject.SetActive(false);
    }
}
