using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] List<CrouchingActions> crouchActions;
    [SerializeField] ParkourAction RunningJumpAction;
    [SerializeField] ParkourAction JumpDownAction;
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    Animator animator;
    bool isAction = false;
    bool freeRun;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        freeRun = Input.GetKey(KeyCode.LeftShift);
        var hitData1 = environmentScanner.ObstackleCheck();
        var hitData2 = environmentScanner.BarrierCheck();
        if (Input.GetKeyDown(KeyCode.Space) && freeRun && !isAction)
        {
            if (!playerController.isGrounded)
            {
                return;
            }
            StartCoroutine(DoParkourAction2(RunningJumpAction));
        }
        if ((Input.GetKeyDown(KeyCode.Space) || freeRun) && !isAction)
        {
            if (!playerController.isGrounded)
            {
                return;
            }
            foreach (var action in parkourActions)
            {
                if (action.CheckIfPossible(hitData1, transform))
                {
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }
        if ((Input.GetKeyDown(KeyCode.LeftAlt) || freeRun) && !isAction)
        {
            if (!playerController.isGrounded)
            {
                return;
            }
            foreach (var action in crouchActions)
            {
                if (action.CheckIfPossible2(hitData2, transform))
                {
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }
        if (playerController.IsOnLedge && !isAction && !hitData1.forwardHitFound && (Input.GetKeyDown(KeyCode.Space) || freeRun))
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
        if (!animState.IsName(action.AnimName))
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
    IEnumerator DoParkourAction2(ParkourAction action)
    {
        isAction = true;
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        yield return new WaitForSeconds(animState.length);
        isAction = false;
    }
    IEnumerator DoParkourAction(CrouchingActions action)
    {
        isAction = true;
        playerController.SetControl(false);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
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
            if (animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        playerController.SetControl(true);
        isAction = false;
    }
}