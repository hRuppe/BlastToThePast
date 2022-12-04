using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour
{
    [Header("----- Arrow Components -----")]
    [SerializeField] Rigidbody rb;

    [Header ("----- Arrow Stats -----")]
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletdamage;
    [SerializeField] int bulletTimer;
    [SerializeField] int bulletVertOffset; // Controls the vertical aspect of the initial velocity for the arrow.


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = (transform.forward * bulletSpeed) + new Vector3(0, bulletVertOffset, 0); // Use instead of trans.forward when game manager is set up with player (playerRef.transform.position - transform.position)
        Destroy(gameObject, bulletTimer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.isTrigger)
        {
            // Ignore player and other triggers
            return;
        }

        if (other.gameObject.GetComponent<IDamage>() != null)
        {
            other.gameObject.GetComponent<IDamage>().TakeDamage(bulletdamage);
        }

        // Stick arrow into wall
        //transform.position += -transform.forward;

        // Parents the arrow to the other object so it moves with them
        transform.parent = other.gameObject.transform;

        // Disables the collider so it doesn't collide again
        GetComponent<Collider>().enabled = false;

        rb.velocity = Vector3.zero;
        rb.useGravity = false;
    }
    
}
