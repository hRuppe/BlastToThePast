using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class retrieveItem : MonoBehaviour
{
    [SerializeField] AudioSource audSource;
    [SerializeField] AudioClip audPickup;
    [Range(0,1)][SerializeField] float audPickupVol;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(PickupItem());
        }   
    }

    IEnumerator PickupItem()
    {
        gameManager.instance.hasItem = true;
        audSource.PlayOneShot(audPickup, audPickupVol);
        yield return new WaitForSeconds(audPickup.length); 
        gameObject.SetActive(false);
    }
}
