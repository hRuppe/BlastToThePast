using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("---- Components ----")]
    [SerializeField] SkinnedMeshRenderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnPos; 

    [Header("---- Enemy Stats ----")]
    [Range(1, 100)][SerializeField] int HP;
    [Range(0, 5)][SerializeField] float playerFaceSpeed; 

    [Header("---- Gun Stats ----")]
    [Range(1, 5)][SerializeField] int shootDelay;

    bool isShooting;
    bool playerInRange;
    Vector3 playerDir; 

    void Start()
    {
        gameManager.instance.enemiesToKill++; //Increment the enemy count on spawn
        gameManager.instance.updateUI(); //Update the UI
    }

    void Update()
    {
        agent.SetDestination(gameManager.instance.player.transform.position); 

        if (playerInRange)
        {
            FacePlayer(); 
            if (!isShooting)
            {
                StartCoroutine(Shoot());
            }
        }
    }

    void FacePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position; 
        playerDir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(playerDir); 
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, playerFaceSpeed * Time.deltaTime); 
    }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        StartCoroutine(FlashDamage()); 

        if (HP <= 0)
        {
            gameManager.instance.updateEnemyNumber(); //Decrement enemy number on kill
            Destroy(gameObject); 
        }
    }

    IEnumerator FlashDamage()
    {
        model.material.color = Color.red; 
        yield return new WaitForSeconds(.15f);
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
        Console.WriteLine("Collided with: " + other.name);
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
