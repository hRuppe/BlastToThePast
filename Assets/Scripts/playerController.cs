using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject arrow; // This will eventually be determined by the weapon the player is holding
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject mine;
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioSource audioSource;

    [Header("----- Player Stats -----")]
    [Range(0, 50)] [SerializeField] int playerHealth;
    [Range(1, 10)] [SerializeField] float playerBaseSpeed; 
    [Range(1.5f, 5)] [SerializeField] float playerSprintMod;
    [Range(8, 20)] [SerializeField] float jumpHeight;
    [Range(0, 35)] [SerializeField] float gravityValue;
    [Range(1, 3)] [SerializeField] public int jumpMax;

    [Header("----- Weapon Stats -----")]
    [SerializeField] float shootRate; // Value represents bullets per second
    [SerializeField] float shootRange;
    [SerializeField] int shootDamage;
    [SerializeField] List<gunStats> gunStatList;
    [Range(1, 10)][SerializeField] int minePlaceDistance;
    [Range(0, 10)][SerializeField] int mineCount;
    [Range(1, 10)][SerializeField] float placeMineTimer;

    [Header("----- Audio -----")]
    [SerializeField] AudioClip[] audioJump;
    [SerializeField] AudioClip[] audioHurt;
    [SerializeField] AudioClip audioGunSwap;
    [Range(0, 1)] [SerializeField] float audioJumpVolume;
    [Range(0, 1)] [SerializeField] float audioHurtVolume;
    [Range(0, 1)] [SerializeField] float audioGunshotVolume;

    public float playerSoundLevel;

    private Vector3 move;
    private Vector3 playerVelocity;

    [HideInInspector] public int jumpTimes;
    [HideInInspector] public int origJumpsMax;

    private int OrigHP;
    private int selectedGun;

    private float playerCurrentSpeed;

    private bool isShooting;
    private bool isSprinting;
    private bool isSneaking;
    private bool trackShootSound;
    private bool isPlacingMine;


    private void Start()
    {
        // Store original values
        OrigHP = playerHealth;
        origJumpsMax = jumpMax;

        playerCurrentSpeed = playerBaseSpeed;
        Respawn();
    }

    void Update()
    {
        PlayerMove();
        PlayerSprint();
        PlayerSneak();
        StartCoroutine(Shoot());
        StartCoroutine(PlaceMine()); 
        GunSelect();
        CalculateSound();
    }

    void PlayerMove()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            jumpTimes = 0;
            playerVelocity.y = 0f;
        }

        move = transform.right * Input.GetAxis("Horizontal") +
               transform.forward * Input.GetAxis("Vertical");

        controller.Move(move * Time.deltaTime * playerCurrentSpeed);

        if (Input.GetButtonDown("Jump") && jumpTimes < jumpMax)
        {
            audioSource.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], audioJumpVolume);
            playerVelocity.y = jumpHeight;
            jumpTimes++;
        }

        playerVelocity.y -= gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void PlayerSprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            playerCurrentSpeed = playerBaseSpeed * playerSprintMod;
            isSprinting = true;

        }
        else if (Input.GetButtonUp("Sprint"))
        {
            playerCurrentSpeed = playerBaseSpeed;
            isSprinting = false;
        }
    }

    void PlayerSneak()
    {
        if (Input.GetButtonDown("Sneak"))
        {
            playerCurrentSpeed = playerBaseSpeed / 2;
            isSneaking = true;

        }
        else if (Input.GetButtonUp("Sneak"))
        {
            playerCurrentSpeed = playerBaseSpeed;
            isSneaking = false;
        }
    }

    IEnumerator Shoot()
    {
        if (gunStatList.Count > 0 && !isShooting && Input.GetButton("Shoot"))
        {
            isShooting = true;
            audioSource.PlayOneShot(gunStatList[selectedGun].gunSound, audioGunshotVolume);
            trackShootSound = true;

            // GameObject newArrow = Instantiate(arrow, shootPos.position, transform.rotation);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootRange))
            {
                if (hit.collider.gameObject.GetComponent<IDamage>() != null)
                {
                    hit.collider.gameObject.GetComponent<IDamage>().TakeDamage(shootDamage);
                }
                Instantiate(hitEffect, hit.point, hitEffect.transform.rotation);
            }

            yield return new WaitForSeconds(1.0f / shootRate);
            isShooting = false;
        }
    }

    IEnumerator PlaceMine()
    {
        if (Input.GetButtonDown("Place Trap") && !isPlacingMine && mineCount > 0)
        {
            isPlacingMine = true;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(.5f, .5f)), out hit, minePlaceDistance))
            {
                Instantiate(mine, hit.point, mine.transform.rotation);
                mineCount--;
            }
            yield return new WaitForSeconds(placeMineTimer);
            isPlacingMine = false;
        }
    }

    void CalculateSound()
    {
        playerSoundLevel = 0;

        // Sound is on a scale from 0 - 1
        if (isSprinting)
            playerSoundLevel += 0.3f;

        if ((move.x > 0 || move.x < 0) || (move.y > 0 || move.y < 0))
            playerSoundLevel += 0.2f;

        if (isSneaking)
            playerSoundLevel /= 2f;

        // Shooting is tracked last since sneaking shouldn't influence the sound level
        if (trackShootSound)
        {
            playerSoundLevel += 0.5f;
            trackShootSound = false;
        }
        gameManager.instance.soundBar.fillAmount = playerSoundLevel;
    }

    public void damage(int damageValue)
    {
        playerHealth -= damageValue;
        StartCoroutine(gameManager.instance.playerDamageFlash());
        audioSource.PlayOneShot(audioHurt[Random.Range(0, audioJump.Length)], audioJumpVolume);
        UpdatePlayerHPBar();

        if (playerHealth <= 0)
        {
            gameManager.instance.playerDeadMenu.SetActive(true);
            gameManager.instance.pause();
        }    
    }

    public void GunPickup(gunStats gunStat)
    {
        gunStatList.Add(gunStat);
        audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);

        // Clone gun stats onto player
        shootRate = gunStat.shooteRate;
        shootRange = gunStat.shootDistance;
        shootDamage = gunStat.shootDamage;
        hitEffect = gunStat.hitEffect;

        // Transfer model to player weapon model
        gunModel.GetComponent<MeshFilter>().sharedMesh = gunStat.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunStat.gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void GunSelect()
    {
        if (gunStatList.Count > 1)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < gunStatList.Count - 1)
            {
                selectedGun++;
                audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);

            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
            {
                selectedGun--;
                audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);
            }

            ChangeGuns();
        }
    }

    public void ChangeGuns()
    {
        // Clone gun stats onto player
        shootRate = gunStatList[selectedGun].shooteRate;
        shootRange = gunStatList[selectedGun].shootDistance;
        shootDamage = gunStatList[selectedGun].shootDamage;
        hitEffect = gunStatList[selectedGun].hitEffect;

        // Transfer model to player weapon model
        gunModel.GetComponent<MeshFilter>().sharedMesh = gunStatList[selectedGun].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunStatList[selectedGun].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void Respawn()
    {
        controller.enabled = false;
        playerHealth = OrigHP;
        UpdatePlayerHPBar();
        transform.position = gameManager.instance.spawnPos.transform.position;
        gameManager.instance.playerDeadMenu.SetActive(false);
        controller.enabled = true;
    }

    public void UpdatePlayerHPBar()
    {
        gameManager.instance.healthBar.fillAmount = (float)playerHealth / (float)OrigHP; 
    }
}