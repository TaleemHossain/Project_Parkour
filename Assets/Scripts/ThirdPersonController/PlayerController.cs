using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Stamina settings")]
    [SerializeField] float maxStamina = 15f;
    [Header("Gun Settings")]
    [SerializeField] GameObject GunHolder;
    [SerializeField] float ReloadTime = 3f;
    [SerializeField] float bulltetTimeGap = 0.1f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform crfirePoint;
    [SerializeField] float bulletSpeed = 30f;
    [SerializeField] float spread = 0.05f;
    [SerializeField] GameObject muzzleFlash;
    [Header("Boolean Variables")]
    bool isResting = false;
    public bool isGrounded;
    public bool isCrouched = false;
    public bool freeRun = false;
    public bool isHanging = false;
    public bool isAiming = false;
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
    int bulletCount = 0;
    float reloadTimer = 0f;
    float fireCooldown = 0f;
    float TotalTime;
    public Vector3 desiredMoveDir;
    public GameObject GameOverScene;
    public GameObject levelCompleteScene;
    public List<GameObject> Targets;
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
        if (dead || completed || !hasControl || !isGrounded || isHanging)
        {
            PauseSound();
        }
        if (dead) return;
        if (completed) return;
        CheckDeath();
        if (dead) return;
        CheckIfWon();
        if (completed) return;
        TotalTime += Time.deltaTime;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float moveAmount = Mathf.Clamp(Mathf.Abs(horizontal) + Mathf.Abs(vertical), 0, 1);

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        desiredMoveDir = cameraController.PlanarRotation * direction;
        moveDir = desiredMoveDir;


        if (isHanging)
        {
            cameraController.distance = 5f;
            return;
        }
        else
        {
            cameraController.distance = 4f;
        }

        if (!hasControl) return;

        bool aiming = Input.GetMouseButton(1);
        UpdateAim(aiming);
        UpdateFire();

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
            if (!isAiming)
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
        velocity.y = ySpeed;

        float speedMultiplier;
        if (freeRun)
        {
            cameraController.distance = 5f;
            speedMultiplier = SprintMult;
        }
        else if (isCrouched || isAiming)
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
        if (isCrouched && !isAiming)
        {
            animator.CrossFade("CrouchLocomotion", 0.2f);
        }
        if (isCrouched && isAiming)
        {
            animator.CrossFade("Aiming Actions.Crouching", 0.2f);
        }
        animator.SetBool("isCrouched", isCrouched);
        characterController.height = isCrouched ? 1.26f : 1.8f;
        characterController.center = isCrouched ? new Vector3(0.12f, 0.66f, 0.2f) : new Vector3(0f, 0.925f, 0.1f);
    }
    void UpdateAim(bool state)
    {
        if (state == true)
        {
            if (isAiming == true)
            {
                return;
            }
            else
            {
                isAiming = true;
                parkourController.InAction = true;
                freeRun = false;
                animator.SetBool("isAiming", true);
                if (isCrouched)
                {
                    animator.CrossFade("Aiming Actions.Crouching", 0.2f);
                }
                else
                {
                    animator.CrossFade("Aiming Actions.Standing", 0.2f);
                }
                GunHolder.SetActive(true);
            }
        }
        else
        {
            if (isAiming == false)
            {
                return;
            }
            else
            {
                isAiming = false;
                parkourController.InAction = false;
                animator.SetBool("isAiming", false);
                GunHolder.SetActive(false);
            }
        }
    }
    void UpdateFire()
    {
        reloadTimer += Time.deltaTime;
        fireCooldown += Time.deltaTime;
        if (isAiming == false) return;
        else
        {
            bool fireButton = Input.GetMouseButton(0);
            if (bulletCount < 30)
            {
                if (fireButton && fireCooldown >= bulltetTimeGap)
                {
                    Rigidbody rb;
                    Vector3 shootDirection;
                    if (isCrouched == false)
                    {
                        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                        GameObject effect = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation * Quaternion.Euler(0f, 180f, 0f));
                        Destroy(effect, 0.1f);
                        rb = bullet.GetComponent<Rigidbody>();

                        shootDirection = firePoint.forward;
                    }
                    else
                    {
                        GameObject bullet = Instantiate(bulletPrefab, crfirePoint.position, crfirePoint.rotation);
                        GameObject effect = Instantiate(muzzleFlash, crfirePoint.position, crfirePoint.rotation * Quaternion.Euler(0f, 180f, 0f));
                        Destroy(effect, 0.1f);
                        rb = bullet.GetComponent<Rigidbody>();

                        shootDirection = crfirePoint.forward;
                    }
                    shootDirection += new Vector3(
                        Random.Range(-spread, spread),
                        Random.Range(-spread, spread),
                        Random.Range(-spread, spread)
                    );
                    shootDirection.Normalize();

                    rb.linearVelocity = shootDirection * bulletSpeed;
                    bulletCount++;
                    fireCooldown = 0f;
                    reloadTimer = 0f;
                }
            }
            else
            {
                if (reloadTimer <= 0.5f)
                {
                    FindFirstObjectByType<AudioManager>().PlaySound("Reload 1");
                }
                reloadTimer += Time.deltaTime;
                if (reloadTimer >= ReloadTime)
                {
                    reloadTimer = 0f;
                    bulletCount = 0;
                    FindFirstObjectByType<AudioManager>().PauseSound("Reload 1");
                }
            }
        }
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
        if (HitPoint <= 0f)
        {
            StartCoroutine(PlayerDeath());
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