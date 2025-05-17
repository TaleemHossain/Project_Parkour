using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction JumpDownAction;
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
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }
        if (playerController.IsOnLedge && !isAction && !hitData.forwardHitFound && Input.GetKeyDown(KeyCode.Space))
        {
            if (playerController.LedgeData.angle <= 50f)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(JumpDownAction));
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