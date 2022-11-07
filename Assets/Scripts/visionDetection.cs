using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class visionDetection : MonoBehaviour
{
    enemyAI detectorParent;

    private void Start()
    {
        detectorParent = transform.parent.GetComponent<enemyAI>();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            detectorParent.playerCanBeSeen = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            detectorParent.playerCanBeSeen = false;
    }
}
