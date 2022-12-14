using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bow : weapon
{
    [SerializeField] float chargeDecaySpeed;

    [SerializeField] GameObject arrow;

    [SerializeField] gunStats gunStats;

    [SerializeField] AudioClip bowDraw;
    [SerializeField] AudioClip bowFire;

    private float currentLoadTime;

    private bool isLoaded;
    private bool playedDrawSound;
    private bool drawBow;
    private bool bowReset; // Used to track when the player manually unloaded bow with alt fire

    private Animator anim;

    void Start()
    {
        anim = gameManager.instance.player.GetComponent<Animator>();
    }

    void Update()
    {
        if (!anim.GetBool("Dead"))
        {
            PrimaryFire();
            AltFire();
        }
        
        anim.SetBool("Draw Bow", drawBow);
    }

    public override void PrimaryFire()
    {
        if (!base.CanFire()) return;

        // Allows the player to draw the bow again after resetting with alt fire
        if (Input.GetButtonUp("Shoot"))
            bowReset = false;

        // Charge bow if held until fire
        if (Input.GetButton("Shoot") && !bowReset)
        {
            drawBow = true;

            // Charges the bow. If you release before fully charged, it starts to uncharge it
            currentLoadTime += Time.deltaTime;

            if (!playedDrawSound)
            {
                gameManager.instance.playerScript.audioSource.PlayOneShot(bowDraw, 1);
                playedDrawSound = true;
            }
        }
        else
        {
            drawBow = false;
            playedDrawSound = false;

            // If the bow is loaded, shoot, otherwise, uncharge the bow
            if (isLoaded)
            {
                GameObject newArrow = Instantiate(arrow, gameManager.instance.playerScript.shootPos.position, gameManager.instance.playerScript.cam.transform.rotation);
                isLoaded = false;
                currentLoadTime = 0;
                gameManager.instance.playerScript.audioSource.PlayOneShot(bowFire, 0.25f);
                playedDrawSound = false;
            }
            else if (currentLoadTime > 0)
            {
                currentLoadTime -= chargeDecaySpeed * Time.deltaTime;
            }
        }

        // Change isLoaded based on currentLoadTime
        if (currentLoadTime >= gunStats.reloadSpeed)
        {
            isLoaded = true;
        }
        else
        {
            isLoaded = false;
        }

        currentLoadTime = Mathf.Clamp(currentLoadTime, 0, gunStats.reloadSpeed);
    }

    // Un-notches the arrow from bow if the player already charged the bow
    public override void AltFire()
    {
        if (!base.CanFire()) return;

        if (Input.GetButton("Alt Fire"))
        {
            isLoaded = false;
            currentLoadTime = 0;

            bowReset = true;
            drawBow = false;
        }
    }
}
