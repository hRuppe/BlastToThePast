using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    private Collider trigger;

    private void Awake()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
        Destroy(gameObject, 0.5f);
    }

    public void SetTriggerSize(int newSize)
    {
        trigger.transform.localScale = new Vector3(newSize, newSize, newSize);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !other.CompareTag("Enemy")) return;

        if (other.gameObject.GetComponent<meleeSwordsmanAI>() != null)
        {
            meleeSwordsmanAI enemy = other.gameObject.GetComponent<meleeSwordsmanAI>();
            enemy.InvestigateSound(transform.position);
        }
        else if (other.gameObject.GetComponent<rangedEnemyAI>() != null)
        {
            rangedEnemyAI enemy = other.gameObject.GetComponent<rangedEnemyAI>();
            enemy.InvestigateSound(transform.position);
        }
        else if (other.gameObject.GetComponent<bossAI>() != null)
        {
            bossAI enemy = other.gameObject.GetComponent<bossAI>();
            enemy.InvestigateSound(transform.position);
        }
    }
}
