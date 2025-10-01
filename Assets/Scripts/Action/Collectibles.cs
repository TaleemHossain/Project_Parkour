using System.Collections;
using UnityEngine;

public class Collectibles : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // play SFX

        // de-spawn VFX

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        StartCoroutine(TargetDeath());

    }
    IEnumerator TargetDeath()
    {
        animator.Play("Death");
        yield return new WaitForSeconds(1f);
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}
