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
    [Header("---- UI ----")]
    [SerializeField] Image playerSeenImage;
    [SerializeField] Image investigateImage;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject UI;

    [Header("---- Components ----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] GameObject headPos;
    [SerializeField] enemyMelee meleeScript;
    [SerializeField] MeshCollider swordCollider;
    [SerializeField] GameObject perfectBlockVFX;
    [SerializeField] Transform perfectBlockVFXPos;
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

    [Header("---- Combat Stats ----")]
    [Range(1, 8)][SerializeField] float stunnedTime; // How long enemy is stunned on a perfect block 
    [Range(1, 10)][SerializeField] public float swordDamage; 
    [Range(.1f, 1)][SerializeField] public float perfectBlockTimeLimit; // How quick the player has to block for it to perfect block (the lower the harder)

    int currentCheckpoint;

    Vector3 playerDir;
    Vector3 startingPos;

    [HideInInspector]public bool isSwinging;
    [HideInInspector]public bool canBlock;
    [HideInInspector]public bool canSeePlayer;

    bool playerInRange;
    bool inPursuit;
    bool isRoaming;
    bool isTakingDmg; 
    bool goingToLocation;
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
        followRoute = true;
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
                if (inPursuit)
                {
                    FacePlayer(); 
                }
            }
            if (!canSeePlayer)
            {
                if (followRoute)
                {
                    NextCheckpoint();
                }
                    
                else if (!followRoute && agent.remainingDistance < 0.25f && !isAtCheckpoint)
                {
                    investigatingSound = false;
                    StartCoroutine(WaitAtCheckpoint());
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
        } else
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

               
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    FacePlayer();
                }

                if (!isSwinging && agent.remainingDistance <= agent.stoppingDistance && !anim.GetBool("Stun"))
                {
                    //StartCoroutine(SwingSword()); 
                    SwingSword(); 
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
            agent.stoppingDistance = 0;
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
        investigatingSound = false;
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
            StopCoroutine(Pursuit());
            StartCoroutine(Pursuit());
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
        // Sneak Attack
        if (!canSeePlayer && !inPursuit && gameManager.instance.playerScript.selectedWeaponType == enums.WeaponType.Melee)
        {
            dmg = HP;
        }

        // Play animation & hit sound
        anim.SetTrigger("GetHit");
        source.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
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

    public IEnumerator StunEnemy()
    {
        if (agent.enabled && meleeScript.perfectBlock)
        {
            // Play sword clashing audio & spawn vfx
            meleeScript.audSource.Play();
            Instantiate(perfectBlockVFX, perfectBlockVFXPos.position, perfectBlockVFX.transform.rotation);

            // Start stun animation, reset swing trigger so it does not swing when stunned, and stop movement
            anim.SetBool("Stun", true);
            anim.ResetTrigger("Swing");
            agent.isStopped = true; 

            // Wait before stopping stun animation & make sure agent is still alive before resuming movement
            yield return new WaitForSeconds(stunnedTime);
            anim.SetBool("Stun", false);
            if (agent.enabled)
            {
                agent.isStopped = false;
            }
            meleeScript.perfectBlock = false; 
        }
    }

    void SwingSword()
    {
        if (!anim.GetBool("Stun") && agent.enabled && !isTakingDmg && !gameManager.instance.playerScript.anim.GetBool("Dead"))
        {
            anim.SetTrigger("Swing");
            StartCoroutine(StunEnemy());
        }
    }

    // Enables collider on sword
    void EnableSwordCollider()
    {
        swordCollider.enabled = true;
        isSwinging = true; 
    }

    // Disables collider on sword
    void DisableSwordCollider()
    {
        swordCollider.enabled = false;
        isSwinging = false; 
    }

    // Called during attack animation starting the blocking window
    void CanBlock()
    {
        canBlock = true;
    }

    // Called during attack animation after blocking window
    void CannotBlock()
    {
        canBlock = false; 
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
