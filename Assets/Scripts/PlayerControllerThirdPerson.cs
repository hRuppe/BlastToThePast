using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerThirdPerson : MonoBehaviour
{
    [SerializeField] int playerBaseSpeed; 
    [SerializeField] CharacterController controller;
    [SerializeField] int maxJumps;
    [SerializeField] int jumpHeight;
    [SerializeField] float gravityValue;
    [SerializeField] Camera cam;
    [SerializeField] int playerRotateSpeed;
    [SerializeField] Animator anim;
    [SerializeField] int animLerpSpeed;
    [SerializeField] float playerSprintMod;
    
    Vector3 move; 
    Vector3 playerVelocity;
    int jumpTimes;
    float playerCurrentSpeed;
    bool isSprinting; 

    // Start is called before the first frame update
    void Start()
    {
        playerCurrentSpeed = 0; 
    }

    // Update is called once per frame
    void Update()
    {        
        // Animation speed is not set up correctly 
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), controller.velocity.normalized.magnitude, Time.deltaTime * animLerpSpeed));

        PlayerMove();
        PlayerSprint(); 
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

        // Makes the player rotate with the camera. Has a bug where the player rotates on all axis
        transform.rotation = Quaternion.Lerp(transform.rotation, cam.transform.rotation, Time.deltaTime * playerRotateSpeed);

        controller.Move(move * Time.deltaTime * playerCurrentSpeed);

        if (Input.GetButtonDown("Jump") && jumpTimes < maxJumps)
        {
            //audioSource.PlayOneShot(audioJump[Random.Range(0, audioJump.Length)], audioJumpVolume);
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


}
