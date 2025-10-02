using UnityEngine;
using Unity.Cinemachine;
using UnityEditor.Experimental.GraphView;

public class AimController : MonoBehaviour
{
    PlayerController playerController;
    Animator animator;
    ParkourController parkourController;
    [SerializeField] GameObject playerAvatar;
    [SerializeField] Transform TargetFollowCamera;
    public CinemachineThirdPersonFollow FollowCam;
    [Header("Gun Settings")]
    [SerializeField] GameObject GunHolder;
    [SerializeField] float ReloadTime = 3f;
    [SerializeField] float bulltetTimeGap = 0.1f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform crfirePoint;
    [SerializeField] float bulletSpeed = 30f;
    [SerializeField] float spread = 0f;
    [SerializeField] GameObject muzzleFlash;
    [Header("Weapon")]
    [SerializeField] Transform Weapon;
    public float weaponRotationSpeed = 360f;
    [Header("Aim Settings")]
    [SerializeField] Transform aimTarget;
    [SerializeField] Transform cameraFollow;
    [SerializeField] Vector3 aimOffset = new(0f, 0f, 0f);
    [SerializeField] float fixedAimDistance = 60f;
    [SerializeField] float bodyYawSpeed = 300f;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] float minPitch = -20f;
    [SerializeField] float maxPitch = 60f;
    [SerializeField] GameObject crosshair;
    float yaw = 0f;
    float pitch = 10f;
    int bulletCount = 0;
    float reloadTimer = 0f;
    float fireCooldown = 0f;
    public bool isAiming;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        parkourController = GetComponent<ParkourController>();
        FollowCam = TargetFollowCamera.GetComponent<CinemachineThirdPersonFollow>();
        isAiming = false;
        GunHolder.SetActive(false);
        crosshair.SetActive(false);
    }
    public void UpdateAim(bool state)
    {
        if (state == true)
        {
            if (isAiming == false)
            {
                crosshair.SetActive(true);
                playerAvatar.transform.localRotation = Quaternion.Euler(0f, 22.5f, 0f);
                playerController.rotationSpeed = 60f;
                isAiming = true;
                parkourController.InAction = true;
                playerController.freeRun = false;
                animator.SetBool("isAiming", true);
                if (playerController.isCrouched)
                {
                    animator.CrossFade("Aiming Actions.Crouching", 0.2f);
                    FollowCam.VerticalArmLength = 1f;
                }
                else
                {
                    animator.CrossFade("Aiming Actions.Standing", 0.2f);
                    FollowCam.VerticalArmLength = 1.5f;
                }
                GunHolder.SetActive(true);
            }
            AimMovement();
        }
        else
        {
            if (isAiming == false)
            {
                if (playerController.rotationSpeed < 360f)
                {
                    playerController.rotationSpeed += Time.deltaTime * 100f;
                    playerController.rotationSpeed = Mathf.Clamp(playerController.rotationSpeed, 60f, 360f);
                }
                return;
            }
            else
            {
                crosshair.SetActive(false);
                playerAvatar.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                playerController.rotationSpeed += Time.deltaTime * 100f;
                isAiming = false;
                parkourController.InAction = false;
                animator.SetBool("isAiming", false);
                GunHolder.SetActive(false);
            }
        }
    }
    void AimMovement()
    {
        if (cameraFollow == null || Camera.main == null) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraFollow.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Quaternion wantPlayerYaw = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, wantPlayerYaw, bodyYawSpeed * Time.deltaTime);

        Vector2 screenCenter = new (Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 crossOffset = aimOffset;
        Vector2 screenPoint = screenCenter + crossOffset;
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Vector3 aimPoint = ray.origin + ray.direction.normalized * fixedAimDistance;

        if (GunHolder != null)
        {
            Vector3 toTarget = aimPoint - GunHolder.transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion wantWeapon = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                GunHolder.transform.rotation = Quaternion.RotateTowards(GunHolder.transform.rotation, wantWeapon, weaponRotationSpeed * Time.deltaTime);
            }
        }
    }
    public void UpdateFire()
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
                    Vector3 shootOrigin, shootDirection;
                    Vector3 aimPoint = aimTarget.position + aimOffset;
                    if (playerController.isCrouched == false)
                    {
                        shootOrigin = firePoint.position;
                        shootDirection = (aimPoint - shootOrigin).normalized;
                        shootOrigin += shootDirection * 0.05f;
                        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);
                        GameObject bullet = Instantiate(bulletPrefab, shootOrigin, bulletRotation);
                        GameObject effect = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation * Quaternion.Euler(0f, 180f, 0f));
                        Destroy(effect, 0.1f);
                        rb = bullet.GetComponent<Rigidbody>();
                    }
                    else
                    {
                        shootOrigin = crfirePoint.position;
                        shootDirection = (aimPoint - shootOrigin).normalized;
                        shootOrigin += shootDirection * 0.05f;
                        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);
                        GameObject bullet = Instantiate(bulletPrefab, shootOrigin, bulletRotation);
                        GameObject effect = Instantiate(muzzleFlash, crfirePoint.position, crfirePoint.rotation * Quaternion.Euler(0f, 180f, 0f));
                        Destroy(effect, 0.1f);
                        rb = bullet.GetComponent<Rigidbody>();
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
}
