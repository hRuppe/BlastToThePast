using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyMelee : MonoBehaviour
{
    [SerializeField] int weaponDmg; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            gameManager.instance.playerScript.damage(weaponDmg); 
        }
    }
}
