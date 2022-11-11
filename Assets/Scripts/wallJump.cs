using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallJump : MonoBehaviour
{
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
            gameManager.instance.playerScript.jumpTimes--;
        }
    }

    private void TakeWallJumpAway(Collider other)
    {
        if (other.tag == "Wall")
        {
            if (gameManager.instance.playerScript.jumpMax > gameManager.instance.playerScript.origJumpsMax)
            {
                gameManager.instance.playerScript.jumpTimes++;
            }
        }
    }
}
