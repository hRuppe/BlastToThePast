using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magicDamage : MonoBehaviour
{
    [SerializeField] SphereCollider dmgCollider; 

    [SerializeField] int damage;
    [SerializeField] float dmgDelay;
    [SerializeField] float reactTime; 
    [SerializeField] float lifeTime;

    bool isDamaging; 

    private void Start()
    {
        // Wait before enabling the collider to give player reaction time
        dmgCollider.enabled = false; 
        StartCoroutine(EnableCollider()); 

        // Destroy after set amount of time
        Destroy(gameObject, lifeTime); 
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isDamaging && other.tag == "Player")
        {
            StartCoroutine(MagicDamage());
        }       
    }

    IEnumerator MagicDamage()
    {
        isDamaging = true; 
        gameManager.instance.playerScript.damage(damage);
        yield return new WaitForSeconds(dmgDelay);
        isDamaging = false; 
    }

    IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(reactTime);
        dmgCollider.enabled = true; 
    }
}
