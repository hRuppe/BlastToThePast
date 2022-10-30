using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [Header("---- Components ----")]
    [SerializeField] Rigidbody rb;
    //[SerializeField] GameObject playerRef; Use when game manager is set up

    [Header("---- Bullet Stats ----")]
    [SerializeField] float bulletSpeed;
    [SerializeField] int bulletDmg;
    [SerializeField] float bulletTimer; 

    void Start()
    {
        rb.velocity =  transform.forward * bulletSpeed; // Use instead of trans.forward when game manager is set up with player (playerRef.transform.position - transform.position)
    }

    void Update()
    {
        StartCoroutine(DestroyBullet()); 
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(bulletTimer);
        Destroy(gameObject); 
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
