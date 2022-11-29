using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyMelee : MonoBehaviour
{
    [SerializeField] int weaponDmg;
    [SerializeField] meleeSwordsmanAI meleeSwordsmanAIScript;
    
    public AudioSource audSource; 
    public bool playerHit; 

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(PlayerHit(other)); 
    }

    IEnumerator PlayerHit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            playerHit = true;
            
            if (gameManager.instance.playerScript.blockTime > .5f || !gameManager.instance.playerScript.isBlocking && meleeSwordsmanAIScript.isSwinging)
            {
                gameManager.instance.playerScript.damage(weaponDmg);
            }
            yield return new WaitForSeconds(1); 
            playerHit = false;
        }
    }
}
