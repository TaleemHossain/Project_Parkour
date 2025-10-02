using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float HitPoint = 200f;
    [Header("Speed Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 720f;
    [SerializeField] float SprintMult = 1.5f;
    [SerializeField] float CrouchMult = 0.75f;
    [SerializeField] float inputDeadzone = 0.1f;

    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Stamina settings")]
    [SerializeField] float maxStamina = 15f;
    [Header("Boolean Variables")]
    bool isResting = false;
    public bool isGrounded;
    public bool isCrouched = false;
    public bool freeRun = false;
    public bool isHanging = false;
    public bool CanPause = true;
    int playStarted = 0; // 0 for false, 1 for walking, 2 for running, 3 for crouching
    int cuurrentMode = 0;
    bool dead = false;
    bool completed = false;
    private float currentStamina;
    bool shift;
    bool hasControl = true;
    public bool IsOnLedge { get; set; }
    public EnvironmentScanner.LedgeData LedgeData { get; set; }
    float ySpeed = 0f;
    float TotalTime;
    public Vector3 desiredMoveDir;
    public GameObject GameOverScene;
    public GameObject levelCompleteScene;
    public List<GameObject> Targets;
    Vector3 moveDir;
    Vector3 velocity = Vector3.zero;
    [Header("Cameras")]
    [SerializeField] public Transform MainCamera;
    [SerializeField] Transform FreeLookCamera;
    CinemachineCamera FreeLookCam;
    Animator animator;
    CharacterController characterController;
    ParkourController parkourController;
    EnvironmentScanner environmentScanner;
    Quaternion targetRotation;
    AimController aimController;
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentStamina = maxStamina;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        parkourController = GetComponent<ParkourController>();
        aimController = GetComponent<AimController>();
        FreeLookCam = FreeLookCamera.GetComponent<CinemachineCamera>();
    }
    private void Update()
    {
        if (dead || completed || !hasControl || !isGrounded || isHanging) PauseSound();
        CheckDeath();
        CheckIfWon();
        if (dead) return;
        if (completed) return;
        if(isHanging) return;
        if (!hasControl) return;
        TotalTime += Time.deltaTime;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 rawInput = new Vector2(horizontal, vertical);
        if (rawInput.magnitude < inputDeadzone)
        {
            horizontal = 0f;
            vertical = 0f;
        }
        float moveAmount = Mathf.Clamp(Mathf.Abs(horizontal) + Mathf.Abs(vertical), 0, 1);

        Vector3 direction = new (horizontal, 0f, vertical);
        if (direction.magnitude > 0.01f) direction.Normalize();

        desiredMoveDir = MainCamera.TransformDirection(direction);
        desiredMoveDir.y = 0f;
        if (desiredMoveDir.magnitude > 0.01f) desiredMoveDir.Normalize();
        else desiredMoveDir = Vector3.zero;

        moveDir = desiredMoveDir;

        bool aiming = Input.GetMouseButton(1);
        aimController.UpdateAim(aiming);
        aimController.UpdateFire();

        GroundCheck();
        velocity.y = ySpeed;

        float speedMultiplier = 1f;
        if (freeRun) speedMultiplier = SprintMult;
        if (isCrouched) speedMultiplier = CrouchMult;

        if (freeRun) FreeLookCam.Lens.FieldOfView = 50f;
        else if (isCrouched) FreeLookCam.Lens.FieldOfView = 30f;
        else FreeLookCam.Lens.FieldOfView = 40f;

        if (characterController.enabled)
            characterController.Move(speedMultiplier * Time.deltaTime * velocity);

        bool isMoving = moveAmount > 0.01f && moveDir.magnitude > 0.01f;

        Vector3 flatMoveDir = Vector3.ProjectOnPlane(moveDir, Vector3.up);

        if (isMoving && flatMoveDir.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(flatMoveDir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            targetRotation = transform.rotation;
        }
        Sfx(moveAmount);
    }
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
        if (!isGrounded)
        {
            parkourController.GrabLedgeMidAir();
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = desiredMoveDir * (moveSpeed / 2f);
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
            if (!aimController.isAiming)
            {
                UpdateFreeRun();
            }

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
    void Sfx(float moveAmount)
    {
        if (isCrouched) cuurrentMode = 3;
        else if (freeRun) cuurrentMode = 2;
        else cuurrentMode = 1;
        if (moveAmount > 0.1f && (playStarted == 0 || cuurrentMode != playStarted))
        {
            PauseSound();
            PlayOnMove();
        }
        if (moveAmount <= 0.1f)
        {
            PauseSound();
        }
    }
    private void PlayOnMove()
    {
        if (!isGrounded) return;
        else if (isHanging) return;
        else if (isCrouched)
        {
            FindFirstObjectByType<AudioManager>().PlaySound("Crouching");
            playStarted = 3;
        }
        else if (freeRun)
        {
            FindFirstObjectByType<AudioManager>().PlaySound("Running");
            playStarted = 2;
        }
        else
        {
            FindFirstObjectByType<AudioManager>().PlaySound("Walking");
            playStarted = 1;
        }
    }
    private void PauseSound()
    {
        FindFirstObjectByType<AudioManager>().PauseSound("Crouching");
        FindFirstObjectByType<AudioManager>().PauseSound("Walking");
        FindFirstObjectByType<AudioManager>().PauseSound("Running");
        playStarted = 0;
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
    public void UpdateCrouchMode(bool state)
    {
        if (isCrouched)
        {
            freeRun = false;
        }
        if (!state && environmentScanner.RoofCheck().IsThereShortRoof)
            return;
        isCrouched = state;
        if (isCrouched && !aimController.isAiming)
        {
            animator.CrossFade("CrouchLocomotion", 0.2f);
        }
        if (isCrouched && aimController.isAiming)
        {
            animator.CrossFade("Aiming Actions.Crouching", 0.2f);
        }
        animator.SetBool("isCrouched", isCrouched);
        characterController.height = isCrouched ? 1.28f : 1.8f;
        characterController.center = isCrouched ? new Vector3(0.12f, 0.665f, 0.2f) : new Vector3(0f, 0.925f, 0.1f);
        aimController.FollowCam.VerticalArmLength = isCrouched ? 1f : 1.5f;
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
    public void TakeDamage(float damage)
    {
        HitPoint -= damage;
    }
    void CheckDeath()
    {
        if (HitPoint <= 0f && !dead)
        {
            StartCoroutine(PlayerDeath());
            dead = true;
        }
    }
    IEnumerator PlayerDeath()
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Disconnection");
        dead = true;
        SetControl(false);
        animator.Play("Death");
        parkourController.InAction = true;
        characterController.enabled = false;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CanPause = false;
        GameOverScene.SetActive(true);
    }
    void CheckIfWon()
    {
        if (completed) return;
        foreach (var target in Targets)
        {
            if (target != null)
            {
                return;
            }
        }
        completed = true;
        CanPause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        levelCompleteScene.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Congrats! You completed this level in " + TotalTime + " seconds";
        levelCompleteScene.SetActive(true);
    }
}