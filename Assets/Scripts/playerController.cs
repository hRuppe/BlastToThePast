using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject arrow; // This will eventually be determined by the weapon the player is holding
    [SerializeField] Transform shootPos;

    [Header("----- Player Stats -----")]
    [Range(0, 50)] [SerializeField] int playerHealth;
    [Range(1, 10)] [SerializeField] float playerBaseSpeed; 
    [Range(1.5f, 5)] [SerializeField] float playerSprintMod;
    [Range(8, 20)] [SerializeField] float jumpHeight;
    [Range(0, 35)] [SerializeField] float gravityValue;
    [Range(1, 3)] [SerializeField] int jumpMax;

    [Header("----- Weapon Stats -----")]
    [SerializeField] float shootRate; // Value represents bullets per second
    [SerializeField] float shootRange;
    [SerializeField] int shootDamage;
    [SerializeField] List<gunStats> gunStatList;
    [SerializeField] GameObject hitEffect;
    [SerializeField] GameObject gunModel;


    private Vector3 move;
    private Vector3 playerVelocity;

    [SerializeField] private int jumpTimes;
    private int origJumpsMax; // Wall jump
    private int OrigHP;
    private int selectedGun;
    private float playerCurrentSpeed;

    private bool isShooting = false;

    private void Start()
    {
        playerCurrentSpeed = playerBaseSpeed;
        OrigHP = playerHealth;
        origJumpsMax = jumpMax; // Wall jump
        respawn();
    }

    void Update()
    {
        PlayerMove();
        PlayerSprint();
        StartCoroutine(Shoot());
        GunSelect();
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

        }
        else if (Input.GetButtonUp("Sprint"))
        {
            playerCurrentSpeed = playerBaseSpeed;
        }
    }

    IEnumerator Shoot()
    {
        if (gunStatList.Count > 0 && !isShooting && Input.GetButton("Shoot"))
        {
            isShooting = true;
            Debug.Log("Shot fired");

            // GameObject newArrow = Instantiate(arrow, shootPos.position, transform.rotation);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootRange))
            {
                if (hit.collider.gameObject.GetComponent<IDamage>() != null)
                {
                    hit.collider.gameObject.GetComponent<IDamage>().TakeDamage(shootDamage);
                }
            }

            yield return new WaitForSeconds(1.0f / shootRate);
            isShooting = false;
        }
    }

    public void damage(int damageValue)
    {
        playerHealth -= damageValue;

        StartCoroutine(gameManager.instance.playerDamageFlash()); 

        if(playerHealth <= 0)
        {
            gameManager.instance.playerDeadMenu.SetActive(true);
            gameManager.instance.pause();
        }    
    }

    public void GunPickup(gunStats gunStat)
    {
        gunStatList.Add(gunStat);

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
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
            {
                selectedGun--;
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

    public void respawn()
    {
        controller.enabled = false;
        playerHealth = OrigHP;
        transform.position = gameManager.instance.spawnPos.transform.position;
        gameManager.instance.playerDeadMenu.SetActive(false);
        controller.enabled = true;
    }

    public void OnTriggerEnter(Collider other)
    { 
        AddWallJump(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TakeWallJumpAway(other);
    }

    private void AddWallJump(Collider other)
    {
        if (other.tag == "Wall")
        {
            jumpTimes--;
        }
    }

    private void TakeWallJumpAway(Collider other)
    {
        if (other.tag == "Wall")
        {
            if (jumpMax > origJumpsMax)
            {
                jumpTimes++;
            }
        }
    }
}