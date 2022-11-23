using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerThirdPerson : MonoBehaviour
{
    [SerializeField] int playerBaseSpeed;
    [SerializeField] int maxJumps;
    [SerializeField] int jumpHeight;
    [SerializeField] int animLerpSpeed;
    [SerializeField] int playerRotateSpeed;
    [SerializeField] float gravityValue;
    [SerializeField] float playerSprintMod;
    [SerializeField] CharacterController controller;
    [SerializeField] Camera cam;
    [SerializeField] Animator anim;
    
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
        // I used the absolute value of the horizontal and vertical axis clamped to the values of 0 - 1 for the animation speed.
        // This is more consistent and achieves the same outcome
        float horizontalAxis = Mathf.Abs(Input.GetAxis("Horizontal"));
        float verticalAxis = Mathf.Abs(Input.GetAxis("Vertical"));
        float axisTotal = Mathf.Clamp(horizontalAxis + verticalAxis, 0, 1);

        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), axisTotal, Time.deltaTime * animLerpSpeed));

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
        // Ignoring the x-axis because that is the axis that rotates the player towards the ground
        transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

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
