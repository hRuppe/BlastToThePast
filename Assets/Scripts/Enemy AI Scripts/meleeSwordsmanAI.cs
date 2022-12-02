using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Globalization;

public class meleeSwordsmanAI : MonoBehaviour, IDamage
{
    [Header("---- Components ----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] GameObject headPos;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject UI;
    [SerializeField] enemyMelee meleeScript;
    [SerializeField] MeshCollider swordCollider; 

    [Header("---- Enemy Stats ----")]
    [Range(1, 100)][SerializeField] int HP;
    [Range(0, 5)][SerializeField] float playerFaceSpeed; // Speed of the enemy turning to face player in range
    [Range(1, 10)][SerializeField] int animLerpSpeed; // How fast the animation transitions happen
    [Range(25, 75)][SerializeField] int sightAngle; // The angle that the player has to be under to be seen
    [Range(0, 50)][SerializeField] int roamDist; // How far the enemy can roam from orig position
    [Range(1, 15)][SerializeField] int playerPursuitStoppingDistance;
    [Range(1, 8)][SerializeField] float stunnedTime;

    [Header("---- Sword Stats ----")]
    [Range(1, 5)][SerializeField] int swingDelay; 

    Vector3 playerDir;
    Vector3 startingPos;

    public bool isSwinging;

    bool playerInRange;
    public bool canSeePlayer;
    bool inPursuit;
    bool isRoaming;
    bool isStunned;
    bool isTakingDmg; 

    float origStoppingDist;
    float origSpeed;
    float origHealth;
    float angleToPlayer;

    void Start()
    {
        // Store original values
        origStoppingDist = agent.stoppingDistance;
        origSpeed = agent.speed;
        startingPos = transform.position;
        origHealth = HP;
        UpdateHpBar();

        UI.gameObject.SetActive(false);
    }

    void Update()
    {
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animLerpSpeed));

        if (agent.enabled)
        {
            if (playerInRange)
            {
                CanSeePlayer();
                if (!canSeePlayer)
                {
                    if (!isRoaming && agent.destination != gameManager.instance.player.transform.position)
                    {
                        Roam();
                    }
                }
                if (inPursuit)
                {
                    FacePlayer(); 
                }
            }
            else if (!isRoaming && agent.destination != gameManager.instance.player.transform.position)
            {
                Roam();
            }
            if (agent.remainingDistance < .1f && agent.destination != gameManager.instance.player.transform.position)
            {
                isRoaming = false;
            }
        }
    }

    void CanSeePlayer()
    {
        playerDir = gameManager.instance.playerScript.torsoPos.transform.position - headPos.transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.transform.position, playerDir, out hit))
        {
            Debug.DrawRay(headPos.transform.position, playerDir);
            // Checks that player is not behind an object and within the enemy fov
            if (hit.collider.tag == "Player" && angleToPlayer <= sightAngle)
            {
                canSeePlayer = true;
                isRoaming = false;
                agent.stoppingDistance = playerPursuitStoppingDistance;
                agent.SetDestination(gameManager.instance.player.transform.position);
                 

                if (!inPursuit)
                {
                    StartCoroutine(Pursuit());
                }     

                // Prevents enemy from freezing up when you go the side of him in a fight - still happens if you go all the way behind him quickly
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    FacePlayer();
                }

                if (!isSwinging && agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartCoroutine(SwingSword()); 
                }
            }
            else
            { 
                canSeePlayer = false;
            }
        }
    }

    IEnumerator Pursuit()
    {
        inPursuit = true; 
        yield return new WaitForSeconds(5f);
        inPursuit = false;
    }

    void Roam()
    { 
        agent.stoppingDistance = 0;

        Vector3 randomPos = Random.insideUnitSphere * roamDist;
        randomPos += startingPos;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(new Vector3(randomPos.x, 0, randomPos.z), out hit, 1, 1))
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(hit.position, path);
            agent.SetPath(path);
            isRoaming = true;
        }
    }

    void FacePlayer()
    {
        playerDir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, playerFaceSpeed * Time.deltaTime);
    }

    public void GotoPlayer()
    {
        if (HP > 0)
        {
            agent.stoppingDistance = playerPursuitStoppingDistance;
            agent.SetDestination(gameManager.instance.player.transform.position);
        }
    }

    public void TakeDamage(int dmg)
    {
        anim.SetTrigger("GetHit"); 
        HP -= dmg;
        UI.gameObject.SetActive(true);
        UpdateHpBar();
        StartCoroutine(FlashDamage());
        // Turn stopping distance to 0 so enemy goes exactly where he was shot from
        agent.stoppingDistance = 0;
        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            gameManager.instance.updateEnemyNumber(); //Decrement enemy number on kill
            anim.SetBool("Dead", true);
            agent.enabled = false;
            UI.SetActive(false);
            GetComponent<Collider>().enabled = false;
        }
    }

    IEnumerator CheckForStun()
    {
        // Check that the player is blocking but not over 1/2 a second & that the player was hit
        if (gameManager.instance.playerScript.isBlocking && gameManager.instance.playerScript.blockTime < .5f 
            && meleeScript.playerHit && agent.enabled)
        {
            // Play sword clashing audio on perfect block
            meleeScript.audSource.Play(); 
            
            // Stop movement & stun animation to true
            isStunned = true;
            anim.SetBool("Stun", true);
            agent.isStopped = true; 

            // Wait before stopping stun animation & make sure agent is still alive before resuming movement
            yield return new WaitForSeconds(stunnedTime);
            anim.SetBool("Stun", false);
            if (agent.enabled)
            {
                agent.isStopped = false;
            } 
            isStunned = false; 
        }
    }

    IEnumerator SwingSword()
    {
        if (!isStunned && agent.enabled && !isTakingDmg)
        {
            swordCollider.enabled = true; 
            isSwinging = true;
            anim.SetTrigger("Swing");
            StartCoroutine(CheckForStun());
            yield return new WaitForSeconds(swingDelay);
            isSwinging = false;
            swordCollider.enabled = false; 
        }  
    }

    public void UpdateHpBar()
    {
        healthBar.fillAmount = HP / origHealth;
    }

    IEnumerator FlashDamage()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.15f);
        model.material.color = Color.white;
    }

    private void OnTriggerStay(Collider other)
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
            canSeePlayer = false;
        }
    }
}
