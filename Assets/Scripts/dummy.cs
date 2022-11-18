using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dummy : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    public void TakeDamage(int dmg)
    {
        StartCoroutine(FlashDamage());
    }

    IEnumerator FlashDamage()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.15f);
        model.material.color = Color.white;
    }
}
