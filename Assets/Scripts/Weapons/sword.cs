using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sword : weapon
{
    [SerializeField] gunStats gunStats;

    private bool canSwing;

    void Start()
    {
        canSwing = true;
    }

    
    void Update()
    {
        if (!gameManager.instance.playerScript.anim.GetBool("Dead"))
        {
            PrimaryFire();
            AltFire();
        }
    }

    public override void PrimaryFire()
    {
        if (Input.GetButton("Shoot") && canSwing)
        {
            gameManager.instance.playerScript.anim.SetTrigger("SwordCombo");
            StartCoroutine(Swing());
        }
    }

    public override void AltFire()
    {
        if (Input.GetButton("Alt Fire"))
        {
            gameManager.instance.playerScript.isBlocking = true;
            gameManager.instance.playerScript.anim.SetBool("Blocking", true);
        } else
        {
            gameManager.instance.playerScript.isBlocking = false;
            gameManager.instance.playerScript.anim.SetBool("Blocking", false);
        }
    }

    IEnumerator Swing()
    {
        gameManager.instance.playerScript.audioSource.PlayOneShot(gunStats.gunSound);
        canSwing = false;
        
        yield return new WaitForSeconds(1.0f / gunStats.shooteRate);

        canSwing = true;
    }
}
