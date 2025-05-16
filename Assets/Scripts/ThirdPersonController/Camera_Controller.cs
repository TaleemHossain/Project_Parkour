using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float mouseSensitivityX = 5f;
    [SerializeField] float mouseSensitivityY = 5f;
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
    private void Awake()
    {
        if (invertX == true)
        {
            mouseSensitivityY *= -1;
        }
        if (invertY == true)
        {
            mouseSensitivityX *= -1;
        }
    }
    private void Update()
    {
        if (followTarget == null)
        {
            Debug.LogError("Follow target is not assigned in Camera_Controller");
            return;
        }
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivityY;
        rotationX += Input.GetAxis("Mouse Y") * mouseSensitivityX;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
        // transform.LookAt(focusPosition.position);
    }
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}
