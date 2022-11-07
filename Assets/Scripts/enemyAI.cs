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
    [SerializeField] int fieldOfView;

    [Header("---- Gun Stats ----")]
    [Range(1, 5)][SerializeField] int shootDelay;

    public bool playerCanBeSeen;

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
        if (CheckFOV())
            agent.SetDestination(gameManager.instance.player.transform.position); 

        if (playerInRange && CheckFOV())
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

    // Desc: Checks if the player is within the FOV and checks for obstacles
    // Returns: True if a player can be seen, false if player cannot be seen
    bool CheckFOV()
    {
        return CheckAngleToPlayer() && !CheckForObstacles();
    }

    // Desc: Checks if the angle between the enemy and the player player is within the enemy FOV value
    // Returns: True if a player is within the FOV and vision range, False if the player isn't
    bool CheckAngleToPlayer()
    {
        Vector3 playerDirection = Vector3.Normalize(gameManager.instance.player.transform.position - transform.position);
        Debug.Log(Vector3.Angle(transform.forward, playerDirection));
        return (Vector3.Angle(transform.forward, playerDirection) <= fieldOfView) && playerCanBeSeen;
    }

    // Desc: Uses a raycast to check for obstacles between the enemy and player
    // Returns: True if there is an obstacle, False if there isn't an obstacle
    bool CheckForObstacles()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gameManager.instance.player.transform.position - transform.position, out hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return false;
            }
        }

        return true;
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
