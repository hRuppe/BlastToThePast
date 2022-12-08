using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemyMelee : MonoBehaviour
{
    [Header("---- Components ----")]
    [SerializeField] meleeSwordsmanAI meleeSwordsmanAIScript;
    public AudioSource audSource; 
    
    [HideInInspector]public bool perfectBlock; 

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitCheck(other);  
    }

    void PlayerHitCheck(Collider collider)
    {
        // Make sure it's the player
        if (collider.tag == "Player")
        {
            // Check if canBlock & isBlocking is true & that the blockTime is less than the perfect block time limit
            if (meleeSwordsmanAIScript.canBlock && gameManager.instance.playerScript.isBlocking &&
                gameManager.instance.playerScript.blockTime < meleeSwordsmanAIScript.perfectBlockTimeLimit)
            {
                perfectBlock = true; 
            }
            else
            {
                gameManager.instance.playerScript.damage(meleeSwordsmanAIScript.swordDamage);
            }
        }
    }
}
