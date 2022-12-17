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
        if (!gameManager.instance.playerScript.anim.GetBool("Dead") && gameManager.instance.playerScript.controller.isGrounded)
        {
            PrimaryFire();
            AltFire();
            gameManager.instance.playerScript.anim.SetBool("Blocking", gameManager.instance.playerScript.isBlocking);
        }
    }

    public override void PrimaryFire()
    {
        if (!base.CanFire() || gameManager.instance.playerScript.isBlocking) return;

        if (Input.GetButton("Shoot") && canSwing)
        {
            gameManager.instance.playerScript.anim.SetTrigger("SwordCombo");
            StopCoroutine(Swing());
            StartCoroutine(Swing());
        }
    }

    public override void AltFire()
    {
        if (!base.CanFire()) return;

        if (Input.GetButton("Alt Fire"))
        {
            gameManager.instance.playerScript.isBlocking = true;
            
        } else
        {
            gameManager.instance.playerScript.isBlocking = false;
        }
    }

    IEnumerator Swing()
    {
        canSwing = false;
        
        yield return new WaitForSeconds(1.0f / gunStats.shooteRate);

        canSwing = true;
    }
}
