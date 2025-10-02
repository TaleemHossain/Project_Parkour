using UnityEngine;
using Unity.Cinemachine;

public class AimController : MonoBehaviour
{
    PlayerController playerController;
    Animator animator;
    ParkourController parkourController;
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
    public float weaponRotationSpeed = 360f;
    [Header("Crosshair UI")]
    [SerializeField] GameObject crossHair;
    [Header("Aim Settings")]
    [SerializeField] Transform aimTarget;
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
        crossHair.SetActive(false);
    }
    public void UpdateAim(bool state)
    {
        if (state == true)
        {
            if (isAiming == false)
            {
                crossHair.SetActive(true);
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
                crossHair.SetActive(false);
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
        
    }
    public void UpdateFire()
    {
        reloadTimer += Time.deltaTime;
        fireCooldown += Time.deltaTime;
        // Debug.Log("isAiming: " + isAiming);
        if (isAiming == false) return;
        else
        {
            // Debug.Log("Aiming");
            bool fireButton = Input.GetMouseButton(0);
            if (bulletCount < 30)
            {
                if (fireButton && fireCooldown >= bulltetTimeGap)
                {
                    // Debug.Log("Firing");
                    Rigidbody rb;
                    Vector3 shootOrigin, shootDirection;
                    Vector3 aimPoint  = aimTarget.position;
                    if (playerController.isCrouched == false)
                    {
                        shootOrigin = firePoint.position;
                        shootDirection = (aimPoint - shootOrigin).normalized;
                        shootOrigin += shootDirection * 0.05f;
                        GameObject bullet = Instantiate(bulletPrefab, shootOrigin, firePoint.rotation);
                        GameObject effect = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation * Quaternion.Euler(0f, 180f, 0f));
                        Destroy(effect, 0.1f);
                        rb = bullet.GetComponent<Rigidbody>();
                    }
                    else
                    {
                        shootOrigin = crfirePoint.position;
                        shootDirection = (aimPoint - shootOrigin).normalized;
                        shootOrigin += shootDirection * 0.05f;
                        GameObject bullet = Instantiate(bulletPrefab, shootOrigin, crfirePoint.rotation);
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
