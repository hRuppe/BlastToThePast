using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class blowUp : MonoBehaviour
{
    [Header("--- Components ---")]
    [SerializeField] Animator anim;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Canvas UI;
    [SerializeField] GameObject blowUpBackpack;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AudioSource audioSource;
    [SerializeField] kamikazeAI enemyScript;

    [Header("--- Mine Stats ---")]
    [Range(1, 10)][SerializeField] int damage;
    [Range(1, 10)][SerializeField] int pushBackAmount;

    [Header("--- Audio ---")]
    [SerializeField] AudioClip audExplosion;
    [Range(0, 1)][SerializeField] float audExplosionVol;

    bool hasExploded;

    private void Update()
    {
        if (anim.GetBool("Dead"))
        {
            hasExploded = true; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && enemyScript.canSeePlayer && !hasExploded)
        {
            Instantiate(explosionEffect, blowUpBackpack.transform.position, transform.rotation);
            audioSource.PlayOneShot(audExplosion, audExplosionVol);
            gameManager.instance.playerScript.damage(damage); // Damage the player
            //gameManager.instance.playerScript.pushBack = (gameManager.instance.player.transform.position - transform.position).normalized * pushBackAmount;
            //transform.parent.GetComponent<IDamage>().TakeDamage(damage); // Damage the enemy
            anim.SetBool("Dead", true);
            agent.enabled = false;
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            UI.gameObject.SetActive(false);
            hasExploded = true; 
        }
    }
}
