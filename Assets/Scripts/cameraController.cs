using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] public int horizontalSens;
    [SerializeField] public int verticalSens;

    [SerializeField] int verticalConstraintMin;
    [SerializeField] int verticalConstraintMax;

    [SerializeField] bool invertY;

    public int originalHorizontalSense;
    public int originalVerticalSens;

    float xRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHorizontalSense = horizontalSens;
        originalVerticalSens = verticalSens;
    }

    // Update is called once per frame
    void Update()
    {
        // Get Cursor Inptu
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * horizontalSens;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * verticalSens;

        if (invertY)
        {
            xRotation += mouseY;
        }
        else
        {
            xRotation -= mouseY;
        }

        // Clamp Camera Rotation Value to Constraints
        xRotation = Mathf.Clamp(xRotation, verticalConstraintMin, verticalConstraintMax);

        // Rotate camera on the x-axis
        gameObject.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Rotate the player
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
