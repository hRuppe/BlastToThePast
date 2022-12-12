using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Globalization;

public class bossAI : MonoBehaviour, IDamage
{
    [Header("---- UI ----")]
    [SerializeField] Image playerSeenImage;
    [SerializeField] Image investigateImage;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject UI;

    [Header("---- Components ----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject magicEffect;
    [SerializeField] GameObject summonEffect; 
    [SerializeField] Animator anim;
    [SerializeField] GameObject headPos;
    [SerializeField] GameObject enemyToSummon; 
    [SerializeField] Transform enemySummonPos1; 
    [SerializeField] Transform enemySummonPos2;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] hurtSounds; 

    [Header("---- Enemy Stats ----")]
    [Range(1, 100)][SerializeField] int HP;
    [Range(0, 5)][SerializeField] float playerFaceSpeed; // Speed of the enemy turning to face player in range
    [Range(1, 10)][SerializeField] int animLerpSpeed; // How fast the animation transitions happen
    [Range(25, 75)][SerializeField] int sightAngle; // The angle that the player has to be under to be seen
    [Range(0, 50)][SerializeField] int roamDist; // How far the enemy can roam from orig position
    [Range(1, 15)][SerializeField] int playerPursuitStoppingDistance;
    [SerializeField] int checkpointWaitTime; // the time the enemy waits at the checkpoint
    [SerializeField] List<Transform> routeCheckpoints;


    [Header("---- Attack Stats ----")]
    [Range(1, 20)][SerializeField] int magicAttackDelay;
    [Range(1, 15)][SerializeField] int enemySummonTimer;

    int currentCheckpoint;

    Vector3 playerDir;
    Vector3 startingPos;

    bool playerInRange;
    [HideInInspector] public bool canSeePlayer;
    bool usingMagic;
    bool isSummoning; 
    bool inPursuit;
    bool isRoaming;
    bool followRoute;
    bool isAtCheckpoint;
    bool investigatingSound;

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
        currentCheckpoint = 0;
        followRoute = false;
    }

    void Update()
    {
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animLerpSpeed));

        if (agent.enabled)
        {
            if (playerInRange)
            {
                CanSeePlayer();
                if (inPursuit)
                {
                    FacePlayer();
                }

                if (!canSeePlayer)
                {
                    if (followRoute)
                    {
                        NextCheckpoint();
                    }

                    else if (!followRoute && agent.remainingDistance < playerPursuitStoppingDistance && !isAtCheckpoint)
                    {
                        StartCoroutine(WaitAtCheckpoint());
                    }

                }
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (HP != origHealth)
            healthBar.enabled = true;
        else
            healthBar.enabled = false;

        if (investigatingSound)
        {
            investigateImage.enabled = true;
        }
        else
        {
            investigateImage.enabled = false;
        }

        if (inPursuit)
        {
            playerSeenImage.enabled = true;
        }
        else
        {
            playerSeenImage.enabled = false;
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

                if (!usingMagic && !gameManager.instance.playerScript.anim.GetBool("Dead"))
                {
                    StartCoroutine(MagicAttack());
                }

                if (!isSummoning && !gameManager.instance.playerScript.anim.GetBool("Dead"))
                {
                    StartCoroutine(SummonEnemies());
                }

            }
            else
            {
                canSeePlayer = false;
            }
        }
    }
    void NextCheckpoint()
    {
        if (currentCheckpoint == routeCheckpoints.Count)
        {
            currentCheckpoint = 0;
        }

        if (currentCheckpoint < routeCheckpoints.Count && followRoute)
        {
            agent.SetDestination(routeCheckpoints[currentCheckpoint].position);
            followRoute = false;
            currentCheckpoint++;
        }
    }

    IEnumerator WaitAtCheckpoint()
    {
        isAtCheckpoint = true;

        yield return new WaitForSeconds(checkpointWaitTime);

        isAtCheckpoint = false;
        followRoute = true;
    }

    IEnumerator Pursuit()
    {
        followRoute = false;
        inPursuit = true;
        yield return new WaitForSeconds(5f);
        inPursuit = false;
        followRoute = true;
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

    public void GotoLocation(Vector3 position)
    {
        if (HP > 0)
        {
            agent.stoppingDistance = playerPursuitStoppingDistance;
            agent.SetDestination(position);
        }
    }

    public void InvestigateSound(Vector3 position)
    {
        if (HP > 0 && !inPursuit)
        {
            investigatingSound = true;
            agent.stoppingDistance = playerPursuitStoppingDistance;
            agent.SetDestination(position);
        }
    }

    public void TakeDamage(int dmg)
    {
        source.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
        HP -= dmg;
        UI.gameObject.SetActive(true);
        UpdateHpBar();
        StartCoroutine(FlashDamage());

        if (HP <= 0)
        {
            gameManager.instance.updateEnemyNumber(); //Decrement enemy number on kill
            anim.SetBool("Dead", true);
            agent.enabled = false;
            UI.SetActive(false);
            GetComponent<Collider>().enabled = false;
        } else
        {
            // Turn stopping distance to 0 so enemy goes exactly where he was shot from
            agent.stoppingDistance = 0;
            InvestigateSound(gameManager.instance.player.transform.position);
        }
    }

    IEnumerator MagicAttack()
    {
        usingMagic = true;
        anim.SetTrigger("SummonMagic");

        Instantiate(magicEffect, gameManager.instance.player.transform.position, magicEffect.transform.rotation);

        yield return new WaitForSeconds(magicAttackDelay);
        usingMagic = false;
    }

    IEnumerator SummonEnemies()
    {
        // Set true & wait for timer
        isSummoning = true;
        yield return new WaitForSeconds(enemySummonTimer); 
        // Set false so it has to wait for timer before going back into it
        isSummoning = false;

        // Makes sure the boss doesn't try to summon enemies if he is dead.
        if (HP > 0)
        {
            agent.isStopped = true;
            anim.SetTrigger("SummonEnemies");
            Vector3 summonSpot1 = enemySummonPos1.position;
            Vector3 summonSpot2 = enemySummonPos2.position;
            GameObject effect1 = Instantiate(summonEffect, summonSpot1, summonEffect.transform.rotation);
            GameObject effect2 = Instantiate(summonEffect, summonSpot2, summonEffect.transform.rotation);
            yield return new WaitForSeconds(.5f);
            Instantiate(enemyToSummon, summonSpot1, gameObject.transform.rotation);
            Instantiate(enemyToSummon, summonSpot2, gameObject.transform.rotation);
            Destroy(effect1);
            Destroy(effect2);
            yield return new WaitForSeconds(1);
            agent.isStopped = false;
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

    public bool IsAlive()
    {
        bool result; 
        if (anim.GetBool("Dead"))
        {
            result = false; 
        }
        else
        {
            result = true; 
        }
        return result; 
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
