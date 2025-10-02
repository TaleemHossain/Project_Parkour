using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 30f;
    [SerializeField] float lifetime = 2f;
    [SerializeField] float damage = 10f;
    [SerializeField] GameObject hitEffect;
    void Start()
    {
        // transform.Rotate(90f, 0f, 0f, Space.Self);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        Ray ray = new (transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, moveDistance))
        {
            OnHit(hit.collider, hit.point, hit.normal);
        }
        else
        {
            transform.Translate(Vector3.forward * moveDistance);
        }
    }
    void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        }
        if (other.CompareTag("Target"))
        {
            other.GetComponent<Target>().TakeDamage(damage);
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage);
        }
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
