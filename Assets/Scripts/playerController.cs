using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] Camera cam;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] Animator anim;
    [SerializeField] GameObject arrow; // This will eventually be determined by the weapon the player is holding
    [SerializeField] Transform shootPos;
    [SerializeField] public Transform torsoPos; 
    [SerializeField] GameObject mine;
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioSource audioSource;
    [SerializeField] BoxCollider meleeCollider;

    [Header("----- Player Stats -----")]
    [Range(0, 50)] [SerializeField] float playerHealth;
    [Range(1, 10)] [SerializeField] float playerBaseSpeed; 
    [Range(1.5f, 5)] [SerializeField] float playerSprintMod;
    [Range(8, 20)] [SerializeField] float jumpHeight;
    [Range(1, 10)][SerializeField] float dashDelay; // 
    [Range(0, 3)][SerializeField] float dashTime; // 
    [Range(1, 40)][SerializeField] int dashSpeed;
    [Range(0, 35)] [SerializeField] float gravityValue;
    [Range(1, 3)] [SerializeField] public int jumpMax;
    [SerializeField] int animLerpSpeed;
    [SerializeField] int playerRotateSpeed;
    [SerializeField] float adsSpeed;
    [SerializeField] float adsFov;

    [Header("----- Weapon Stats -----")]
    [SerializeField] float shootRate; // Value represents bullets per second
    [SerializeField] float shootRange;
    [SerializeField] public int shootDamage;
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
    public float blockTime;

    private Vector3 move;
    private Vector3 playerVelocity;

    [HideInInspector] public int jumpTimes;
    [HideInInspector] public int origJumpsMax;

    private int selectedGun;

    private float originalFov;
    private float OrigHP;
    private float playerCurrentSpeed;
    private float timeSinceAdsStart;
    private float originalHorizontalSens;
    private float originalVerticalSens;

    public bool isBlocking;

    private bool isAds;
    private bool isShooting;
    private bool isSprinting;
    private bool isDashing; 
    private bool isSneaking;
    private bool isPlacingMine;
    private bool trackShootSound;
    private bool canCombo;



    private void Start()
    {
        // Store original values
        originalFov = freeLook.m_Lens.FieldOfView;
        OrigHP = playerHealth;
        origJumpsMax = jumpMax;
        originalHorizontalSens = freeLook.m_XAxis.m_MaxSpeed;
        originalVerticalSens = freeLook.m_YAxis.m_MaxSpeed;

        playerCurrentSpeed = playerBaseSpeed;
        Respawn();
    }

    void Update()
    {
        // I used the absolute value of the horizontal and vertical axis clamped to the values of 0 - 1 for the animation speed.
        // This is more consistent and achieves the same outcome
        float horizontalAxis = Mathf.Abs(Input.GetAxis("Horizontal"));
        float verticalAxis = Mathf.Abs(Input.GetAxis("Vertical"));
        float axisTotal = Mathf.Clamp(horizontalAxis + verticalAxis, 0, 1);

        // Divides the axis total by two if we aren't sprinting, causing the blend tree to use the walking animation.
        if (!isSprinting)
            axisTotal /= 2;

        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), axisTotal, Time.deltaTime * animLerpSpeed));

        shootPos.transform.rotation = cam.transform.rotation;

        PlayerMove();
        PlayerSprint();
        PlayerSneak();
        AltFire();
        StartCoroutine(Shoot());
        StartCoroutine(PlaceMine()); 
        GunSelect();
        CalculateSound();
        StartBlockTimer(); 
    }

    void PlayerMove()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            jumpTimes = 0;
            playerVelocity.y = 0f;
        }

        // Removing the horizontal axis makes the camera movement feel better but takes awa
        move = transform.right * Input.GetAxis("Horizontal") +
               transform.forward * Input.GetAxis("Vertical");

        // Makes the player rotate with the camera. 
        // Ignoring the x-axis because that is the axis that rotates the player towards the ground
        transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

        controller.Move(move * Time.deltaTime * playerCurrentSpeed);

        if (Input.GetButtonDown("Jump") && jumpTimes < jumpMax)
        {
            if (audioJump.Length > 0)
                audioSource.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], audioJumpVolume);
            
            anim.SetTrigger("Jump");
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

    //IEnumerator PlayerDash()
    //{
    //    if (Input.GetButtonDown("Dash") && !isDashing)
    //    {
    //        isDashing = true;

    //        float startTime = Time.time; 
    //        while (Time.time < startTime + dashTime)
    //        {
    //            playerCurrentSpeed = playerBaseSpeed * dashSpeed; 
    //        }
    //        playerCurrentSpeed = playerBaseSpeed; 
    //    }
    //    yield return new WaitForSeconds(dashDelay);
    //    isDashing = false; 
    //}

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

    void AltFire()
    {
        if (gunStatList.Count <= 0) return;

        if (Input.GetButtonDown("Alt Fire"))
        {

            switch (gunStatList[selectedGun].weaponType)
            {
                case enums.WeaponType.Melee:
                    isBlocking = true;
                    anim.SetBool("Block", true); 
                    break;
                
                case enums.WeaponType.Hitscan:
                    isAds = true;
                    timeSinceAdsStart = 0;
                    freeLook.m_XAxis.m_MaxSpeed = originalHorizontalSens / 2;
                    freeLook.m_YAxis.m_MaxSpeed = originalVerticalSens / 2;
                    break;
                
                case enums.WeaponType.Projectile:

                    break;
            }
        } 
        
        else if (Input.GetButtonUp("Alt Fire"))
        {
            switch (gunStatList[selectedGun].weaponType)
            {
                case enums.WeaponType.Melee:
                    blockTime = 0; 
                    isBlocking = false;
                    anim.SetBool("Block", false);
                    break;
                case enums.WeaponType.Hitscan:
                    isAds = false;
                    freeLook.m_XAxis.m_MaxSpeed = originalHorizontalSens;
                    freeLook.m_YAxis.m_MaxSpeed = originalVerticalSens;
                    break;
                case enums.WeaponType.Projectile:
                    break;
            }
        }

        if (isAds)
        {
            timeSinceAdsStart += Time.deltaTime;

            freeLook.m_Lens.FieldOfView = Mathf.Lerp(originalFov, adsFov, Mathf.Clamp(timeSinceAdsStart / adsSpeed, 0, 1));
        } else if (!isAds)
        {
            timeSinceAdsStart -= Time.deltaTime;

            freeLook.m_Lens.FieldOfView = Mathf.Lerp(originalFov, adsFov, Mathf.Clamp(timeSinceAdsStart / adsSpeed, 0, 1));
        }

        timeSinceAdsStart = Mathf.Clamp(timeSinceAdsStart, 0, adsSpeed);

    }

    // Resets canCombo, which determines if the player can combo sword swings. Called by an animation event.
    void ResetCombo()
    {
        anim.SetBool("CanCombo", false);
    }

    // Checks a collider in front of the player and damages enemy. This function is called by an animation event.
    void DamageMelee()
    {
        anim.SetBool("CanCombo", true);

        Collider[] hits = Physics.OverlapBox(meleeCollider.transform.position, meleeCollider.size / 2);
        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<IDamage>() != null)
            {
                hit.GetComponent<IDamage>().TakeDamage(shootDamage);
            }
        }
    }

    // Starts a timer to see how long the player has been blocking
    void StartBlockTimer()
    {
        if (isBlocking)
        {
            blockTime += Time.deltaTime; 
        }
    }

    IEnumerator Shoot()
    {
        if (gunStatList.Count > 0 && !isShooting && Input.GetButton("Shoot"))
        {
            isShooting = true;

            if (gunStatList.Count > 0)
                audioSource.PlayOneShot(gunStatList[selectedGun].gunSound, audioGunshotVolume);
            trackShootSound = true;
            RaycastHit hit;

            if (gunStatList[selectedGun].weaponType == enums.WeaponType.Melee)
            {

                anim.SetTrigger("SwordCombo");

            } else
            {
                // GameObject newArrow = Instantiate(arrow, shootPos.position, transform.rotation);


                if (Physics.Raycast(cam.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootRange, 3))
                {
                    if (hit.collider.gameObject.GetComponent<IDamage>() != null)
                    {
                        hit.collider.gameObject.GetComponent<IDamage>().TakeDamage(shootDamage);
                        Instantiate(hitEffect, hit.point, hitEffect.transform.rotation);
                    }
                }
            }

            

            yield return new WaitForSeconds(1.0f / shootRate);

            gunModel.GetComponent<BoxCollider>().enabled = false;
            isShooting = false;
        }
    }

    IEnumerator PlaceMine()
    {
        if (Input.GetButtonDown("Place Trap") && !isPlacingMine && mineCount > 0)
        {
            isPlacingMine = true;
            RaycastHit hit;
            if (Physics.Raycast(cam.ViewportPointToRay(new Vector2(.5f, .5f)), out hit, minePlaceDistance))
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

    public void damage(float damageValue)
    {
        if (isBlocking)
            playerHealth -= damageValue / 2;
        else
            playerHealth -= damageValue;

        StartCoroutine(gameManager.instance.playerDamageFlash());

        if (audioHurt.Length > 0)
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
        
        if (audioGunSwap.length > 0)
            audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);

        // Clone gun stats onto player
        shootRate = gunStat.shooteRate;
        shootRange = gunStat.shootDistance;
        shootDamage = gunStat.shootDamage;
        hitEffect = gunStat.hitEffect;

        // Transfer model to player weapon model
        gunModel.GetComponent<MeshFilter>().sharedMesh = gunStat.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunStat.gunModel.GetComponent<MeshRenderer>().sharedMaterial;
        gunModel.transform.localScale = gunStat.handScale;
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
        gunModel.transform.localScale = gunStatList[selectedGun].handScale;
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