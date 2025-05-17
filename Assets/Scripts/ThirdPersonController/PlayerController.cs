using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 720f;

    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    public bool isGrounded;
    bool hasControl = true;
    public bool isOnLedge { get; set; }
    float ySpeed = 0f;
    Camera_Controller cameraController;
    Animator animator;
    CharacterController characterController;
    EnvironmentScanner environmentScanner;
    Quaternion targetRotation;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component is missing on " + gameObject.name);
        cameraController = Camera.main.GetComponent<Camera_Controller>();
        if (cameraController == null) Debug.LogError("Camera_Controller component is missing on Main Camera");
        characterController = GetComponent<CharacterController>();
        if (characterController == null) Debug.LogError("CharacterController component is missing on " + gameObject.name);
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float moveAmount = Mathf.Clamp(Mathf.Abs(horizontal) + Mathf.Abs(vertical), 0, 1);

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        var moveDir = cameraController.PlanarRotation * direction;
        if (!hasControl) return;
        GroundCheck();
        if (!isGrounded)
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            ySpeed = -0.1f;
            isOnLedge = environmentScanner.LedgeCheck(moveDir);
            if (isOnLedge)
            {
                Debug.Log("On Ledge");
            }
            else
            {
                Debug.Log("Not on Ledge");
            }
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
    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;
        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }
}