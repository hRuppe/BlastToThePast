using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public float shooteRate;
    public int shootDistance;
    public int shootDamage;
    public int ammoCount;
    public GameObject gunModel;
    public GameObject hitEffect;
    public GameObject muzzleFlash;
    public AudioClip gunSound;
}
