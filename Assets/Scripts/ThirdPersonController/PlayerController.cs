using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 720f;

    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;
    float ySpeed = 0f;
    Camera_Controller cameraController;
    Animator animator;
    CharacterController characterController;
    Quaternion targetRotation;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component is missing on " + gameObject.name);
        cameraController = Camera.main.GetComponent<Camera_Controller>();
        if (cameraController == null) Debug.LogError("Camera_Controller component is missing on Main Camera");
        characterController = GetComponent<CharacterController>();
        if (characterController == null) Debug.LogError("CharacterController component is missing on " + gameObject.name);
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float moveAmount = Mathf.Clamp(Mathf.Abs(horizontal) + Mathf.Abs(vertical), 0, 1);

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        var moveDir = cameraController.PlanarRotation * direction;

        GroundCheck();
        if(!isGrounded)
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            ySpeed = -0.1f;
        }
        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.1f, Time.deltaTime);
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = new Color(0, 1, 0, 0.5f);
    //     Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    // }
}