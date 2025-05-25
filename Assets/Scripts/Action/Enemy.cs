using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Animator animator;
    CapsuleCollider CapsuleCollider;
    [SerializeField] float HitPoint = 100f;
    [Header("Ground Check settings")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Detect Player")]
    [SerializeField] private Transform player;
    [SerializeField] public float visionRange = 15f;
    [SerializeField] public float visionAngle = 75f;
    [SerializeField] public LayerMask playerLayer;
    [Header("Firing Settings")]
    [SerializeField] public Transform firePoint;
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] public GameObject muzzleFlash;
    [SerializeField] public float fireRate = 5f;
    [SerializeField] private float fireCooldown = 0f;
    [SerializeField] private float spread = 0.1f;
    bool dead = false;
    void Awake()
    {
        CapsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void LateUpdate()
    {
        if (dead) return;
        if (player == null) return;

        if (HitPoint <= 0f)
        {
            StartCoroutine(Death());
        }
        fireCooldown += Time.deltaTime;

        if (CanSeePlayer())
        {
            if (fireCooldown >= 1f / fireRate)
            {
                animator.SetBool("isFiring", true);
                Shoot();
                fireCooldown = 0f;
            }
        }
        else
        {
            animator.SetBool("isFiring", false);
        }
    }
    public void TakeDamage(float damage)
    {
        HitPoint -= damage;
        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 10800f * Time.deltaTime);
    }
    IEnumerator Death()
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Death 1");
        dead = true;
        animator.Play("Death");
        yield return new WaitForSeconds(3f);
        CapsuleCollider.enabled = false;
        Destroy(gameObject);
    }
    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        if (dirToPlayer.y > 1.0f || dirToPlayer.y < -0.1f)
        {
            return false;
        }
        float distance = dirToPlayer.magnitude;

        if (distance > visionRange)
        {
            return false;
        }

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > visionAngle)
        {
            return false;
        }

        if (Physics.Raycast(transform.position + Vector3.up * 1f, dirToPlayer.normalized, out RaycastHit hit, visionRange, playerLayer))
        {
            if (hit.transform.CompareTag("Player"))
            {
                dirToPlayer.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 1080f * Time.deltaTime);
                return true;
            }
        }
        return false;
    }
    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        GameObject effect = Instantiate(muzzleFlash, firePoint.position, transform.rotation * Quaternion.Euler(0f, 180f, 0f));
        Destroy(effect, 0.1f);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        Vector3 shootDirection = transform.forward;

        shootDirection += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        shootDirection.Normalize();

        rb.linearVelocity = shootDirection * 30f;
    }
}
