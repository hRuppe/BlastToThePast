using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bomb : MonoBehaviour
{
    [Header("----- Bopmb Components -----")]
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject fire;

    [Header("----- Bomb Stats -----")]
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletdamage;
    [SerializeField] int bulletTimer;
    [SerializeField] int bulletVertOffset; // Controls the vertical aspect of the initial velocity for the arrow.

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = (transform.forward * bulletSpeed) + new Vector3(0, bulletVertOffset, 0); // Use instead of trans.forward when game manager is set up with player (playerRef.transform.position - transform.position)
        Destroy(gameObject, bulletTimer);
    }

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(fire, transform.position, fire.transform.rotation);

        if (other.tag == "Player")
        {
            gameManager.instance.playerScript.damage(bulletdamage);
        }

        Destroy(gameObject);
    }
}
