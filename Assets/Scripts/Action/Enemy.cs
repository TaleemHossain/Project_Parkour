using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Animator animator;
    MeshCollider meshCollider;

    [SerializeField] float HitPoint = 100f;
    void Awake()
    {
        animator = GetComponent<Animator>();
        meshCollider = GetComponent<MeshCollider>();
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
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}
