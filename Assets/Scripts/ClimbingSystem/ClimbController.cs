using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    ParkourController parkourController;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        parkourController = GetComponent<ParkourController>();
    }
    private void Update()
    {
        if (parkourController.InAction) return;
        if (Input.GetKeyDown(KeyCode.Space) && environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit) && !playerController.isHanging)
        {
            playerController.SetControl(false);
            StartCoroutine(JumpToLedge("IdleToHanging", ledgeHit.transform, 0.41f, 0.54f));
        }
    }
    public IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime)
    {
        var matchTargetParameters = new PlayerController.MatchTargetParameters()
        {
            pos = ledge.position + ledge.forward * 0.05f - ledge.up * 0.07f - ledge.right * 0.28f,
            bodyPart = AvatarTarget.RightHand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };
        yield return playerController.DoAction(anim, Quaternion.LookRotation(-ledge.forward), matchTargetParameters, true);
        playerController.isHanging = true;
        playerController.freeRun = false;
        playerController.isCrouched = false;
    }
}
