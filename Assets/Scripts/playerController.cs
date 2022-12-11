using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] GameObject rightHandWeaponContainer;
    [SerializeField] GameObject leftHandWeaponContainer;
    [SerializeField] GameObject hitEffect;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] BoxCollider meleeCollider;
    [SerializeField] GameObject soundTrigger;

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

    public enums.WeaponType selectedWeaponType;

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
        if (Input.GetAxis("Horizontal") != 0)
            anim.SetFloat("Horizontal", Mathf.Lerp(anim.GetFloat("Horizontal"), horizontalAxis, Time.deltaTime * animLerpSpeed));
        else
            anim.SetFloat("Horizontal", 0);

        if (Input.GetAxis("Vertical") != 0)
            anim.SetFloat("Vertical", Mathf.Lerp(anim.GetFloat("Vertical"), verticalAxis, Time.deltaTime * animLerpSpeed));
        else
            anim.SetFloat("Vertical", 0);

        shootPos.transform.rotation = cam.transform.rotation;

        // If the player is dead, don't run any of this stuff
        if (!anim.GetBool("Dead"))
        {
            PlayerMove();
            PlayerSprint();
            PlayerSneak();
            //StartCoroutine(PlaceMine());
            GunSelect();
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
                audioSource.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], 1);
            
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
            anim.SetBool("Stopped Crouching", false); 
            anim.SetBool("Is Crouching", true);
            playerCurrentSpeed = playerBaseSpeed / 2;
            isSneaking = true;
        }
        else if (Input.GetButtonUp("Sneak"))
        {
            anim.SetBool("Stopped Crouching", true); 
            anim.SetBool("Is Crouching", false);
            playerCurrentSpeed = playerBaseSpeed;
            isSneaking = false;
        }
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
        else
        {
            blockTime = 0; 
        }
    }

    public void damage(float damageValue)
    {
        if (isBlocking)
            playerHealth -= damageValue / 2;
        else
            playerHealth -= damageValue;

        if (!anim.GetBool("Dead"))
        {
            StartCoroutine(gameManager.instance.playerDamageFlash());

            if (audioHurt.Length > 0)
                audioSource.PlayOneShot(audioHurt[Random.Range(0, audioHurt.Length)], 1);
        }

        UpdatePlayerHPBar();

        if (playerHealth <= 0)
        {
            StartCoroutine(PlayerDead());
        }    
    }

    public void GunPickup(gunStats gunStat, GameObject newWeapon)
    {
        gunStatList.Add(gunStat);

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
                newWeapon.transform.localScale = gunStat.handScale; 
                Instantiate(newWeapon, leftHandWeaponContainer.transform.position, leftHandWeaponContainer.transform.rotation, leftHandWeaponContainer.transform);
            }
        } else
        {
            if (rightHandWeaponContainer.transform.childCount == 0)
            {
                newWeapon.transform.localScale = gunStat.handScale;
                Instantiate(newWeapon, rightHandWeaponContainer.transform.position, rightHandWeaponContainer.transform.rotation, rightHandWeaponContainer.transform);
            }
        }

        ChangeGuns();
    }

    // This is called by animation events
    void GenerateSoundTrigger(int size)
    {
        GameObject newTrigger = Instantiate(soundTrigger, transform.position, soundTrigger.transform.rotation);
        SoundTrigger triggerScript = newTrigger.GetComponent<SoundTrigger>();

        triggerScript.SetTriggerSize(size);
    }

    void GunSelect()
    {
        if (gunStatList.Count > 1)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < gunStatList.Count - 1)
            {
                selectedGun++;
                
                audioSource.PlayOneShot(audioGunSwap, 1);
                ChangeGuns();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
            {
                selectedGun--;

                audioSource.PlayOneShot(audioGunSwap, 1);
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
        selectedWeaponType = gunStatList[selectedGun].weaponType;

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

    public float GetCurrentHealth()
    {
        return playerHealth; 
    }

    public float GetOriginalHealth()
    {
        return OrigHP; 
    }

    public void GiveHealth(float healthToGive)
    {
        // Calculate what health would be with the health increase
        float postHealth = playerHealth + healthToGive;

        if (postHealth > OrigHP)
        {
            playerHealth = OrigHP;
        }
        else
        {
            playerHealth += healthToGive;
        }
        UpdatePlayerHPBar();

    }
}