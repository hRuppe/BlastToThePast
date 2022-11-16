using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundDetection : MonoBehaviour
{
    [SerializeField] int minimumHearingDistance;
    [SerializeField] float soundThreshold;
    [SerializeField] wallStats thinWall;
    [SerializeField] wallStats mediumWall;
    [SerializeField] wallStats thickWall;
    [SerializeField] enemyAI parent;

    bool canBeHeard;
    float perceivedSound;
    
    void Update()
    {
        if (canBeHeard)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

            perceivedSound = (minimumHearingDistance / (Mathf.Clamp(distanceToPlayer, 10, 100) / gameManager.instance.playerScript.playerSoundLevel));

            RaycastHit[] hits = Physics.RaycastAll(transform.position, gameManager.instance.player.transform.position - transform.position, distanceToPlayer);
            
            // Draw ray indicating sound detection
            //Debug.DrawRay(transform.position, gameManager.instance.player.transform.position - transform.position);

            float totalDampening = 0;

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("ThinWall"))
                    totalDampening += thinWall.soundDampening;
                else if (hit.transform.CompareTag("MediumWall"))
                    totalDampening += mediumWall.soundDampening;
                else if (hit.transform.CompareTag("ThickWall"))
                    totalDampening += thickWall.soundDampening;
            }

            perceivedSound -= totalDampening;

            perceivedSound = Mathf.Clamp(perceivedSound, 0, 1);

            if (perceivedSound >= soundThreshold)
            {
                parent.GotoPlayer();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canBeHeard = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canBeHeard = false;
    }
}
