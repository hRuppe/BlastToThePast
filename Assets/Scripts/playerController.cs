using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] public Camera cam;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] public Animator anim;
    [SerializeField] GameObject arrow; // This will eventually be determined by the weapon the player is holding
    [SerializeField] public Transform shootPos;
    [SerializeField] public Transform torsoPos; 
    //[SerializeField] GameObject mine;
    [SerializeField] GameObject rightHandWeaponContainer;
    [SerializeField] GameObject leftHandWeaponContainer;
    [SerializeField] GameObject hitEffect;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] BoxCollider meleeCollider;

    [Header("----- Player Stats -----")]
    [Range(0, 50)] [SerializeField] float playerHealth;
    [Range(1, 10)] [SerializeField] float playerBaseSpeed; 
    [Range(1.5f, 5)] [SerializeField] float playerSprintMod;
    [Range(8, 20)] [SerializeField] float jumpHeight;
    [Range(0, 35)] [SerializeField] float gravityValue;
    [Range(1, 3)] [SerializeField] public int jumpMax;
    [SerializeField] int animLerpSpeed;
    [SerializeField] int playerRotateSpeed;
    [SerializeField] float adsSpeed;
    [SerializeField] float adsFov;
    [SerializeField] float deathDuration;

    [Header("----- Weapon Stats -----")]
    [SerializeField] float shootRate; // Value represents bullets per second
    [SerializeField] float shootRange;
    [SerializeField] public int shootDamage;
    [SerializeField] List<gunStats> gunStatList;
    //[Range(1, 10)][SerializeField] int minePlaceDistance;
    //[Range(0, 10)][SerializeField] int mineCount;
    //[Range(1, 10)][SerializeField] float placeMineTimer;

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

    private weapon weaponScript;

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

    private bool isJumping;
    private bool isDodging; 
    private bool isAds;
    private bool isShooting;
    private bool isSprinting;
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

        freeLook.transform.rotation = new Quaternion(0, 0, 0, 0);

        Respawn();
    }

    void Update()
    {
        // These values are so we can manipulate the axis values later
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        // Divides the axis total by two if we aren't sprinting, causing the blend tree to use the walking animation.
        if (!isSprinting && !isSneaking)
        {
            horizontalAxis /= 2;
            verticalAxis /= 2;
        }

        // Lerp the values so the animations transition smoothly
        anim.SetFloat("Horizontal", Mathf.Lerp(anim.GetFloat("Horizontal"), horizontalAxis, Time.deltaTime * animLerpSpeed));
        anim.SetFloat("Vertical", Mathf.Lerp(anim.GetFloat("Vertical"), verticalAxis, Time.deltaTime * animLerpSpeed));

        shootPos.transform.rotation = cam.transform.rotation;

        // If the player is dead, don't run any of this stuff
        if (!anim.GetBool("Dead"))
        {
            PlayerMove();
            PlayerSprint();
            PlayerSneak();
            //AltFire();
            //StartCoroutine(Shoot());
            //StartCoroutine(PlaceMine());
            GunSelect();
            CalculateSound();
            StartBlockTimer();
            anim.SetBool("Jump", isJumping);
        }
    }

    void PlayerMove()
    {
        // Jump reset
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            isJumping = false;
            jumpTimes = 0;
            playerVelocity.y = 0f;
        }

        move = transform.right * Input.GetAxis("Horizontal") +
               transform.forward * Input.GetAxis("Vertical");

        // Makes the player rotate with the camera. 
        // Ignoring the x-axis because that is the axis that rotates the player towards the ground
        transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

        controller.Move(move * Time.deltaTime * playerCurrentSpeed);

        if (Input.GetButtonDown("Jump") && jumpTimes < jumpMax)
        {
            isJumping = true;
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
        // Don't sprint while sneaking
        if (isSneaking) return;

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

    // This is a coroutine so the death animation plays before pausing the game and showing the death screen
    public IEnumerator PlayerDead()
    {
        anim.SetBool("Dead", true);

        yield return new WaitForSeconds(deathDuration);

        gameManager.instance.playerDeadMenu.SetActive(true);
        gameManager.instance.pause();
    }

    public void TurnOffController()
    {
        controller.enabled = false;
    }

    void PlayerSneak()
    {
        if (Input.GetButtonDown("Sneak"))
        {
            anim.SetBool("Is Crouching", true);
            playerCurrentSpeed = playerBaseSpeed / 2;
            isSneaking = true;

        }
        else if (Input.GetButtonUp("Sneak"))
        {
            anim.SetBool("Is Crouching", false);
            playerCurrentSpeed = playerBaseSpeed;
            isSneaking = false;
        }
    }

    // Function handles genric right click actions based on the type of weapon equipped
   /* void AltFire()
    {
        if (gunStatList.Count <= 0) return;

        if (Input.GetButtonDown("Alt Fire"))
        {
            switch (gunStatList[selectedGun].weaponType)
            {
                // Block
                case enums.WeaponType.Melee:
                     
                    break;
                
                // Aim down sight
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
        
        // Resets all values changed when using right click
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

        // Calculations for lerping the aim down sight zoom
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
   */

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
        else
        {
            blockTime = 0; 
        }
    }

    /*IEnumerator Shoot()
    {
        // Makes sure the player has a weapon and is trying to shoot
        if (gunStatList.Count > 0 && !isShooting && Input.GetButton("Shoot"))
        {
            isShooting = true;

            if (gunStatList.Count > 0)
                audioSource.PlayOneShot(gunStatList[selectedGun].gunSound, audioGunshotVolume);

            // Tracks shoot sound for use in sound detection. Will probably get changed.
            trackShootSound = true;
            RaycastHit hit;

            if (gunStatList[selectedGun].weaponType == enums.WeaponType.Melee)
            {

                

            } else if (gunStatList[selectedGun].weaponType == enums.WeaponType.Projectile)
            {
                
            } else if (gunStatList[selectedGun].weaponType == enums.WeaponType.Hitscan)
            {
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

            if (gunStatList[selectedGun].isLeftHanded)
                leftHandWeaponContainer.GetComponent<BoxCollider>().enabled = false;
            else
                rightHandWeaponContainer.GetComponent<BoxCollider>().enabled = false;

            isShooting = false;
        }
    }
    */

    // ---- DON'T NEED MINE PLACEMENT FOR NOW ----

    //IEnumerator PlaceMine()
    //{
    //    if (Input.GetButtonDown("Place Trap") && !isPlacingMine && mineCount > 0)
    //    {
    //        isPlacingMine = true;
    //        RaycastHit hit;
    //        if (Physics.Raycast(cam.ViewportPointToRay(new Vector2(.5f, .5f)), out hit, minePlaceDistance))
    //        {
    //            Instantiate(mine, hit.point, mine.transform.rotation);
    //            mineCount--;
    //        }
    //        yield return new WaitForSeconds(placeMineTimer);
    //        isPlacingMine = false;
    //    }
    //}

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
            StartCoroutine(PlayerDead());
        }    
    }

    public void GunPickup(gunStats gunStat, GameObject newWeapon)
    {
        gunStatList.Add(gunStat);
        
        if (audioGunSwap.length > 0)
            audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);

        // Clone gun stats onto player
        shootRate = gunStat.shooteRate;
        shootRange = gunStat.shootDistance;
        shootDamage = gunStat.shootDamage;
        hitEffect = gunStat.hitEffect;

        // Using prefabs for weapons, so we just instantiate the new weapon
        if (gunStat.isLeftHanded)
        {
            if (leftHandWeaponContainer.transform.childCount == 0)
            {
                Instantiate(newWeapon, leftHandWeaponContainer.transform.position, leftHandWeaponContainer.transform.rotation, leftHandWeaponContainer.transform);
            }
        } else
        {
            if (rightHandWeaponContainer.transform.childCount == 0)
            {
                Instantiate(newWeapon, rightHandWeaponContainer.transform.position, rightHandWeaponContainer.transform.rotation, rightHandWeaponContainer.transform);
            }
        }

        ChangeGuns();
    }

    void GunSelect()
    {
        if (gunStatList.Count > 1)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < gunStatList.Count - 1)
            {
                selectedGun++;
                
                audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);
                ChangeGuns();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
            {
                selectedGun--;

                audioSource.PlayOneShot(audioGunSwap, audioGunshotVolume);
                ChangeGuns();
            }
        }
    }

    public void ChangeGuns()
    {
        // Clone gun stats onto player
        shootRate = gunStatList[selectedGun].shooteRate;
        shootRange = gunStatList[selectedGun].shootDistance;
        shootDamage = gunStatList[selectedGun].shootDamage;
        hitEffect = gunStatList[selectedGun].hitEffect;

        // Destroy both hand's weapons if they exist. We reinstantiate them below
        if (leftHandWeaponContainer.transform.childCount != 0)
            Destroy(leftHandWeaponContainer.transform.GetChild(0).gameObject);

        if (rightHandWeaponContainer.transform.childCount != 0)
            Destroy(rightHandWeaponContainer.transform.GetChild(0).gameObject);

        // Instantiate weapon prefab in appropriate hand
        if (gunStatList[selectedGun].isLeftHanded)
        {
            Instantiate(gunStatList[selectedGun].gunModel, leftHandWeaponContainer.transform.position, leftHandWeaponContainer.transform.rotation, leftHandWeaponContainer.transform);
        } else
        {
            Instantiate(gunStatList[selectedGun].gunModel, rightHandWeaponContainer.transform.position, rightHandWeaponContainer.transform.rotation, rightHandWeaponContainer.transform);
        }
    }

    public void Respawn()
    {
        anim.SetBool("Dead", false);
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