using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] List<CrouchingActions> crouchActions;
    [SerializeField] ParkourAction RunningJumpAction;
    [SerializeField] ParkourAction JumpDownAction;
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    ClimbController climbController;
    Animator animator;
    public bool InAction = false;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        climbController = GetComponent<ClimbController>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (playerController.isHanging) return;
        if (InAction) return;
        if (!playerController.isGrounded) return;  
        if (playerController.isCrouched) return;

        var hitData1 = environmentScanner.ObstackleCheck();
        var hitData2 = environmentScanner.BarrierCheck();

        if (Input.GetKeyDown(KeyCode.Space) && playerController.freeRun)
        {
            StartCoroutine(DoParkourAction2(RunningJumpAction));
        }

        if (Input.GetKeyDown(KeyCode.Space) || playerController.freeRun)
        {
            foreach (var action in parkourActions)
            {
                if (action.CheckIfPossible(hitData1, transform))
                {
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || playerController.freeRun)
        {
            foreach (var action in crouchActions)
            {
                if (action.CheckIfPossible2(hitData2, transform))
                {
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }
        
        if (playerController.IsOnLedge && !hitData1.forwardHitFound && (Input.GetKeyDown(KeyCode.Space) || playerController.freeRun))
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
        InAction = true;
        playerController.SetControl(false);
        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
        {
            Debug.Log("Wrong Animation name");
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
        InAction = false;
    }
    IEnumerator DoParkourAction2(ParkourAction action)
    {
        InAction = true;
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            if ((Input.GetKeyDown(KeyCode.Space) || playerController.freeRun) && environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit) && !playerController.isHanging)
            {
                playerController.SetControl(false);
                StartCoroutine(climbController.JumpToLedge("JumpToHang", ledgeHit.transform, 0.10f, 0.75f));
            }
            yield return null;
        }
        InAction = false;
    }
    IEnumerator DoParkourAction(CrouchingActions action)
    {
        InAction = true;
        playerController.SetControl(false);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
        {
            Debug.Log("Wrong Animation name");
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
        InAction = false;
    }
}