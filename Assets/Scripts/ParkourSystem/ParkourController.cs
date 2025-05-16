using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    Animator animator;
    bool isAction = false;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAction)
        {
            var hitData = environmentScanner.ObstackleCheck();
            if (hitData.forwardHitFound)
            {
                StartCoroutine(DoParkourAction());
            }
        }
    }
    IEnumerator DoParkourAction()
    {
        isAction = true;
        playerController.SetControl(false);
        animator.CrossFade("SteppingUp", 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        yield return new WaitForSeconds(animState.length);
        playerController.SetControl(true);
        isAction = false;
    }
}
