using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
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
        var hitData = environmentScanner.ObstackleCheck();
        if (Input.GetKeyDown(KeyCode.Space) && !isAction)
        {
            if (!playerController.isGrounded)
            {
                return;
            }
            foreach (var action in parkourActions)
            {
                if (action.CheckIfPossible(hitData, transform))
                {
                    Debug.Log("Parkour action possible");
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
                else
                {
                    Debug.Log("Parkour action not possible");
                }
            }
        }
    }
    IEnumerator DoParkourAction(ParkourAction action)
    {
        isAction = true;
        playerController.SetControl(false);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (animState.IsName(action.AnimName))
        {
            Debug.Log("Parkour action in progress");
        }
        else
        {
            Debug.LogError("Wrong Animation name");
        }
        yield return new WaitForSeconds(animState.length);
        playerController.SetControl(true);
        isAction = false;
    }
}
