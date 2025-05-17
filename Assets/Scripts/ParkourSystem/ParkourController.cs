using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                    // Debug.Log("Parkour action possible");
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
                else
                {
                    Debug.Log(action.AnimName + "Parkour action not possible");
                }
            }
        }
    }
    IEnumerator DoParkourAction(ParkourAction action)
    {
        isAction = true;
        playerController.SetControl(false);
        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (animState.IsName(action.AnimName))
        {
            // Debug.Log("Parkour action in progress");
        }
        else
        {
            Debug.LogError("Wrong Animation name");
        }
        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            if (action.RotateToObstackle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.rotationSpeed * Time.deltaTime);
            }
            if (action.EnableTargetMatching)
            {
                if (!animator.isMatchingTarget)
                {
                    animator.MatchTarget(action.MatchPosition, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
                }
            }
            if (animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(action.PostActionDelay);
        playerController.SetControl(true);
        isAction = false;
    }
}