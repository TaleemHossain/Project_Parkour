using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 720f;
    [SerializeField] float SprintMult = 1.5f;
    [SerializeField] float CrouchMult = 0.75f;

    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Stamina settings")]
    [SerializeField] float maxStamina = 15f;
    private float currentStamina;
    bool isResting = false;
    public bool isGrounded;
    public bool isCrouched = false;
    public bool freeRun = false;
    bool shift;
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
        currentStamina = maxStamina;
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouched = !isCrouched;
            if (isCrouched == true)
            {
                freeRun = false;
            }
            animator.SetBool("isCrouched", isCrouched);
            characterController.height = isCrouched ? 1.25f : 1.7f;
            characterController.center = isCrouched ? new Vector3(0.1f, 0.67f, 0.15f) : new Vector3(0f, 0.88f, 0.15f);
        }

        shift = Input.GetKey(KeyCode.LeftShift);

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

        UpdateFreeRun();
        velocity.y = ySpeed;
        float speedMultiplier = freeRun ? SprintMult : 1f;
        speedMultiplier *= isCrouched ? CrouchMult : 1f;
        characterController.Move(speedMultiplier * velocity * Time.deltaTime);

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
        if (LedgeData.isCorner)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
            return;
        }
        Vector3 ledgeNormal = LedgeData.surfaceHitInfo.normal;
        Vector3 moveDirProjected = Vector3.ProjectOnPlane(desiredMoveDir, ledgeNormal);
        velocity = moveDirProjected * moveSpeed;
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
    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }
    private void UpdateFreeRun()
    {
        if (shift)
        {
            if (currentStamina >= 0f && !isResting)
            {
                freeRun = true;
                isCrouched = false;
                animator.SetBool("isCrouched", isCrouched);
            }
            if (freeRun)
            {
                currentStamina -= Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }
            if (currentStamina <= 0f)
            {
                freeRun = false;
                isResting = true;
            }
        }
        else
        {
            freeRun = false;
            currentStamina += Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            if (currentStamina >= 5f)
            {
                isResting = false;
            }
        }
    }
}