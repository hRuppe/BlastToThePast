using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthPickup : MonoBehaviour
{
    [Header("---- Health ----")]
    [SerializeField] float healthToGive;
    
    [Header("---- Pickup Movement ----")]
    [SerializeField] float moveIncrement;
    [SerializeField] float hoverMax;
    [SerializeField] float rotationSpeed;
    
    [Header("---- Components ----")]
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip potionSound;
    [SerializeField] GameObject vfx; 

    float rotation = 180; 
    private Vector3 originalPosition;
    private float yOffset;
    private bool movingDown;
    private bool isRotating;

    private void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (!isRotating)
            RotatePickup();

        Hover();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager.instance.playerScript.GetCurrentHealth() < gameManager.instance.playerScript.GetOriginalHealth())
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameManager.instance.playerScript.GiveHealth(healthToGive);
            source.PlayOneShot(potionSound);
            Instantiate(vfx, transform.position, vfx.transform.rotation);
            Destroy(gameObject, potionSound.length); 
        }
    }

    private void Hover()
    {
        // Theres probably a better way to do this. Adds motion to the pickup
        if (yOffset <= 0 || !movingDown)
        {
            yOffset += moveIncrement * Time.deltaTime;
            movingDown = false;
        }

        if (yOffset >= hoverMax || movingDown)
        {
            yOffset -= moveIncrement * Time.deltaTime;
            movingDown = true;
        }

        transform.position = originalPosition + new Vector3(0, yOffset, 0);
    }

    void RotatePickup()
    {
        isRotating = true; 
        transform.Rotate(0, 0, rotation * Time.deltaTime * rotationSpeed);
        isRotating = false; 
    }
}
