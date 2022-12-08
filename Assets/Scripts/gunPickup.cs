using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunPickup : MonoBehaviour
{

    [SerializeField] gunStats gunStat;
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip pickupSound; 
    [SerializeField] float moveIncrement;
    [SerializeField] float hoverMax;
    [SerializeField] float rotationSpeed;

    private Vector3 originalPosition;
    private float yOffset;
    private bool movingDown;
    private bool isRotating;

    private void Start()
    {
        originalPosition = transform.position;    
    }

    private void Update()
    {
        if (!isRotating)
            RotatePickup();

        Hover();
     }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerScript.GunPickup(gunStat, weapon);
            gameManager.instance.playerScript.audioSource.PlayOneShot(pickupSound);
            gameObject.GetComponent<MeshRenderer>().enabled = false; 
            Destroy(gameObject, pickupSound.length);
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

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.right), rotationSpeed * Time.deltaTime);

        isRotating = false;
    }
}
