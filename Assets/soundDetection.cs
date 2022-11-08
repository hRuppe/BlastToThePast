using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundDetection : MonoBehaviour
{
    [SerializeField] float soundLevelSensitivity;
    [SerializeField] enemyAI detectorParent;

    private void OnTriggerStay(Collider other)
    {
        Listen();
    }

    private void Listen()
    {
        if (gameManager.instance.playerScript.soundEmitLevel > soundLevelSensitivity)
        {
            detectorParent.playerHeard = true;
        }
    }
}
