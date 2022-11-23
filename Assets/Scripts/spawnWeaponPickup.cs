using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnWeaponPickup : MonoBehaviour
{
    [SerializeField] GameObject weaponPickup; // Weapon that will spawn near portal

    void Start()
    {
        // Instatn
        Instantiate(weaponPickup, transform.position, weaponPickup.transform.rotation); 
    }
}
