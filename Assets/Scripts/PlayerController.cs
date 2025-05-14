using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 720f;
    Camera_Controller cameraController;
    Quaternion targetRotation;
    private void Awake()
    {
        cameraController = Camera.main.GetComponent<Camera_Controller>();
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float moveAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

        Vector3 direction = (new Vector3(horizontal, 0, vertical)).normalized;
        var moveDir = cameraController.PlanarRotation * direction;

        if (moveAmount > 0)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
