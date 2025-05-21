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
    public bool isHanging = false;
    bool shift;
    bool hasControl = true;
    public bool IsOnLedge { get; set; }
    public EnvironmentScanner.LedgeData LedgeData { get; set; }
    float ySpeed = 0f;
    public Vector3 desiredMoveDir;
    Vector3 moveDir;
    Vector3 velocity = Vector3.zero;
    Camera_Controller cameraController;
    Animator animator;
    CharacterController characterController;
    ParkourController parkourController;
    EnvironmentScanner environmentScanner;
    Quaternion targetRotation;
    public class MatchTargetParameters
    {
        public Vector3 pos;
        public AvatarTarget bodyPart;
        public Vector3 posWeight;
        public float startTime;
        public float targetTime;
    }
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
        parkourController = GetComponent<ParkourController>();
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

        if (isHanging)
        {
            cameraController.distance = 5f;
            return;
        }
        else
        {
            cameraController.distance = 4f;
        }

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);

        if (!isGrounded)
        {
            parkourController.GrabLedgeMidAir();
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = transform.forward * moveSpeed / 2;
        }
        else
        {
            velocity = desiredMoveDir * moveSpeed;
            ySpeed = -0.1f;

            if (Input.GetKeyDown(KeyCode.C))
            {
                UpdateCrouchMode(!isCrouched);
            }

            shift = Input.GetKey(KeyCode.LeftShift);
            UpdateFreeRun();

            IsOnLedge = environmentScanner.PlatformLedgeCheck(desiredMoveDir, out EnvironmentScanner.LedgeData ledgeData);
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }
            if (!isCrouched)
            {
                animator.SetFloat("moveAmount", velocity.magnitude * (freeRun ? SprintMult : 1f) / (moveSpeed * SprintMult), 0.1f, Time.deltaTime);
                animator.SetFloat("crouchMoveAmount", 0f);
            }
            else
            {
                animator.SetFloat("crouchMoveAmount", velocity.magnitude / moveSpeed, 0.1f, Time.deltaTime);
                animator.SetFloat("moveAmount", 0f);
            }
        }
        velocity.y = ySpeed;

        float speedMultiplier;
        if (freeRun)
        {
            cameraController.distance = 5f;
            speedMultiplier = SprintMult;
        }
        else if (isCrouched)
        {
            cameraController.distance = 3f;
            speedMultiplier = CrouchMult;
        }
        else
        {
            cameraController.distance = 4f;
            speedMultiplier = 1f;
        }
        if (characterController.enabled)
        {
            characterController.Move(speedMultiplier * velocity * Time.deltaTime);
        }

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
            animator.SetFloat("crouchMoveAmount", 0f);
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
                UpdateCrouchMode(false);
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
    void UpdateCrouchMode(bool state)
    {
        if (isCrouched)
        {
            freeRun = false;
        }
        if (!state && environmentScanner.RoofCheck().IsThereShortRoof)
            return;
        isCrouched = state;
        if (isCrouched)
        {
            animator.CrossFade("CrouchLocomotion", 0.2f);
            freeRun = false;
        }
        animator.SetBool("isCrouched", isCrouched);
        characterController.height = isCrouched ? 1.26f : 1.66f;
        characterController.center = isCrouched ? new Vector3(0.12f, 0.66f, 0.2f) : new Vector3(0f, 0.86f, 0.1f);
    }
    public IEnumerator DoAction(string animName, Quaternion TargetRotation = new Quaternion(), MatchTargetParameters matchTarget = null, bool rotate = false, float postActionDelay = 0f, bool mirror = false)
    {
        parkourController.InAction = true;
        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        float timer = 0f;
        while (timer <= animState.length)
        {
            float rotateStartTime = (matchTarget == null) ? 0f : matchTarget.startTime;
            float normalizedTime = timer / animState.length;
            timer += Time.deltaTime;
            if (rotate && normalizedTime > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetRotation, rotationSpeed * Time.deltaTime);
            }
            if (matchTarget != null)
            {
                if (!animator.isMatchingTarget)
                {
                    animator.MatchTarget(matchTarget.pos, transform.rotation, matchTarget.bodyPart, new MatchTargetWeightMask(matchTarget.posWeight, 0), matchTarget.startTime, matchTarget.targetTime);
                }
            }
            if (animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(postActionDelay);
        parkourController.InAction = false;
    }
    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }
}