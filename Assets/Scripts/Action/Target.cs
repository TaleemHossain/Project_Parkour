using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    Animator animator;
    [SerializeField] float HitPoint = 30f;
    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }
    void Update()
    {
        if (HitPoint <= 0f)
        {
            StartCoroutine(TargetDeath());
        }
    }
    public void TakeDamage(float damage)
    {
        HitPoint -= damage;
    }
    IEnumerator TargetDeath()
    {
        animator.Play("Death");
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
