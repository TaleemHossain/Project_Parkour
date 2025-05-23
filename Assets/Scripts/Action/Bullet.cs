using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed = 30f;
    float lifetime = 2f;
    float damage = 10f;
    [SerializeField] GameObject hitEffect;
    void Start()
    {
        // transform.Rotate(90f, 0f, 0f, Space.Self);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // transform.Translate(Vector3.forward * speed * Time.deltaTime);
        float moveDistance = speed * Time.deltaTime;
        Ray ray = new Ray(transform.position, transform.forward);

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

        Destroy(gameObject);
    }
}
