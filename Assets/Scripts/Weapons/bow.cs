using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bow : weapon
{
    [SerializeField] float chargeDecaySpeed;

    [SerializeField] GameObject arrow;

    [SerializeField] gunStats gunStats;

    private float currentLoadTime;

    private bool isLoaded;

    private Animator anim;

    void Start()
    {
        anim = gameManager.instance.player.GetComponent<Animator>();
    }

    void Update()
    {
        PrimaryFire();
        AltFire();
    }

    public override void PrimaryFire()
    {
        // Charge bow if held until fire
        if (Input.GetButton("Shoot"))
        {
            // Charges the bow. If you release before fully charged, it starts to uncharge it
            currentLoadTime += Time.deltaTime;
        }
        else
        {
            // If the bow is loaded, shoot, otherwise, uncharge the bow
            if (isLoaded)
            {
                GameObject newArrow = Instantiate(arrow, gameManager.instance.playerScript.shootPos.position, gameManager.instance.playerScript.cam.transform.rotation);
                isLoaded = false;
                currentLoadTime = 0;
            }
            else
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

    public override void AltFire()
    {
        if (Input.GetButton("Alt Fire"))
        {
            if (isLoaded)
            {
                currentLoadTime = 0;
            }
        }
    }
}
