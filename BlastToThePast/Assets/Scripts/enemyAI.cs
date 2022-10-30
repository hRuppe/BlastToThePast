using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("---- Components ----")]
    [SerializeField] SkinnedMeshRenderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject tempPlayer; // Use until player & game manager are ready
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnPos; 

    [Header("---- Enemy Stats ----")]
    [SerializeField] int HP;
    [SerializeField] float dmgFlashDelay;

    [Header("---- Gun Stats ----")]
    [SerializeField] int shootDelay;

    bool isShooting;
    bool playerInRange; 

    void Start()
    {
        
    }

    void Update()
    {
        agent.SetDestination(tempPlayer.transform.position);

        if (playerInRange)
        {
            if (!isShooting)
            {
                StartCoroutine(Shoot());
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        StartCoroutine(FlashDamage()); 

        if (HP <= 0)
        {
            Destroy(gameObject); 
        }
    }

    IEnumerator FlashDamage()
    {
        model.material.color = Color.red; 
        yield return new WaitForSeconds(.1f);
        model.material.color = Color.white; 
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        Instantiate(bullet, bulletSpawnPos.position, transform.rotation);
        yield return new WaitForSeconds(shootDelay);
        isShooting = false; 
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = false;
        }
    }
}
