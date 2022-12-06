using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTextPopUp : MonoBehaviour
{
    [SerializeField] float popUptime; 

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowMission());  
    }

    IEnumerator ShowMission()
    {
        gameObject.SetActive(true); 
        yield return new WaitForSeconds(popUptime);
        gameObject.SetActive(false); 
    }
}
