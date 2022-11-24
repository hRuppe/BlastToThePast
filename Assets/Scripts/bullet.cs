using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [Header("---- Components ----")]
    [SerializeField] Rigidbody rb;

    [Header("---- Bullet Stats ----")]
    [Range(1, 20)][SerializeField] float bulletSpeed;
    [Range(1, 100)][SerializeField] int bulletDmg;
    [Range(5, 25)][SerializeField] float bulletTimer; 

    void Start()
    {
        rb.velocity = (gameManager.instance.playerScript.torsoPos.transform.position - transform.position) * bulletSpeed;  
        Destroy(gameObject, bulletTimer); 
    }

    void Update()
    {
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            gameManager.instance.playerScript.damage(bulletDmg); 
        }

        Destroy(gameObject);
    }
}
