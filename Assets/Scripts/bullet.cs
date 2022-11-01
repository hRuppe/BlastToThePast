using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [Header("---- Components ----")]
    [SerializeField] Rigidbody rb;

    [Header("---- Bullet Stats ----")]
    [Range(5, 50)][SerializeField] float bulletSpeed;
    [Range(1, 100)][SerializeField] int bulletDmg;
    [Range(5, 25)][SerializeField] float bulletTimer; 

    void Start()
    {
        rb.velocity =  transform.forward * bulletSpeed; // Use instead of trans.forward when game manager is set up with player: (playerRef.transform.position - transform.position)
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

        Destroy(gameObject);
    }
}
