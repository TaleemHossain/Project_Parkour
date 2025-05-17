using System.Collections;
using System.Collections.Generic;
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
    public bool IsOnLedge { get; set; }
    public EnvironmentScanner.LedgeData LedgeData { get; set; }
    float ySpeed = 0f;
    Vector3 desiredMoveDir;
    Vector3 moveDir;
    Vector3 velocity = Vector3.zero;
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
        if (environmentScanner == null) Debug.LogError("EnvironmentScanner component is missing on " + gameObject.name);
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float moveAmount = Mathf.Clamp(Mathf.Abs(horizontal) + Mathf.Abs(vertical), 0, 1);

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        desiredMoveDir = cameraController.PlanarRotation * direction;
        moveDir = desiredMoveDir;

        if (!hasControl) return;

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (!isGrounded)
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = transform.forward * moveSpeed / 2;
        }
        else
        {
            velocity = desiredMoveDir * moveSpeed;
            ySpeed = -0.1f;
            IsOnLedge = environmentScanner.LedgeCheck(desiredMoveDir, out EnvironmentScanner.LedgeData ledgeData);
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }
            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.1f, Time.deltaTime);
        }
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0 && moveDir.magnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }
    void LedgeMovement()
    {
        float angle = Vector3.Angle(LedgeData.surfaceHitInfo.normal, desiredMoveDir);
        if (angle < 90f)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
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