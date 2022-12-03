using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class interactiveButtons : MonoBehaviour
{
    [SerializeField] Color hoverColor;

    bool hovered = false;

    Color originalColor;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name); 
            }
        }
    }

    private void Start()
    {
        originalColor = gameObject.GetComponentInChildren<TMP_Text>().color; 
    }

    private void OnMouseEnter()
    {
        Debug.Log("Entered");
        gameObject.GetComponentInChildren<TMP_Text>().color = hoverColor; 
    }

    private void OnMouseExit()
    {
        gameObject.GetComponentInChildren<TMP_Text>().color = originalColor;
    }
}
