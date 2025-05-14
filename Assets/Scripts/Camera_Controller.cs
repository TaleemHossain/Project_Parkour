using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float mouseSensitivity = 5f;
    [SerializeField] float distance = 5f;
    [SerializeField] float minVerticalAngle = -20f;
    [SerializeField] float maxVerticalAngle = 45f;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] bool invertY = false;
    [SerializeField] bool invertX = false;
    float rotationY;
    float rotationX;
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (invertY == true)
        {
            rotationY += -1 * Input.GetAxis("Mouse X") * mouseSensitivity;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        }
        if (invertX == true)
        {
            rotationX += -1 * Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        else
        {
            rotationX += Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
        // transform.LookAt(focusPosition.position);
    }

}
