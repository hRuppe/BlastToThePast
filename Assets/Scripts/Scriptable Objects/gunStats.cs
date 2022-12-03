using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public float shooteRate;
    public float reloadSpeed;
    public int shootDistance;
    public int shootDamage;
    public int ammoCount;
    public bool isLeftHanded;
    public enums.WeaponType weaponType;
    public GameObject gunModel;
    public GameObject hitEffect;
    public GameObject muzzleFlash;
    public AudioClip gunSound;
    public Vector3 handScale;
}