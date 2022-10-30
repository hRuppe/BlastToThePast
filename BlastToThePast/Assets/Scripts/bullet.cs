using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [Header("---- Components ----")]
    [SerializeField] Rigidbody rb;

    [Header("---- Bullet Stats ----")]
    [SerializeField] float bulletSpeed;
    [SerializeField] int bulletDmg;
    [SerializeField] float bulletTimer; 

    void Start()
    {
        rb.velocity =  transform.forward * bulletSpeed; // Use instead of trans.forward when game manager is set up with player (playerRef.transform.position - transform.position)
        Destroy(gameObject, bulletTimer); 
    }

    void Update()
    {
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // Do dmg through game manager
        }

        // Sticks the arrow into the object
        transform.position = transform.position + (transform.forward * 1.25f);

        // Parents the arrow to the other object so it moves with them
        gameObject.transform.parent = other.gameObject.transform;

        // Disables the collider so it doesn't collide again
        GetComponent<CapsuleCollider>().enabled = false;
    }
}
