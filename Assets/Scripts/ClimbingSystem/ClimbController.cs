using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    public ClimbPoint currentPoint = null;
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    ParkourController parkourController;
    [SerializeField] float maxAngleAllowed = 15f;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        parkourController = GetComponent<ParkourController>();
    }
    private void Update()
    {
        if (parkourController.InAction)
        {
            return;
        }
        bool hitFound = environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit);
        bool dropLedgeFound = environmentScanner.DropLedgeCheck(out RaycastHit dropLedgeHit);
        if (Input.GetKeyDown(KeyCode.Space) && hitFound && !playerController.isHanging)
        {
            currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
            if (currentPoint == null && hitFound)
            {
                if (ledgeHit.transform.GetComponent<ClimbPointContainer>() != null)
                {
                    currentPoint = ledgeHit.transform.GetComponent<ClimbPointContainer>().GetClimbPoint(transform.position);
                }
            }
            if (currentPoint == null)
            {
                return;
            }
            playerController.SetControl(false);
            StartCoroutine(JumpToLedge("IdleToHanging", currentPoint.transform, 0.41f, 0.54f));
        }
        else if (Input.GetKeyDown(KeyCode.LeftAlt) && dropLedgeFound && !playerController.isHanging && playerController.isGrounded)
        {
            Debug.Log("Drop ledge found : " + dropLedgeFound);
            currentPoint = dropLedgeHit.transform.GetComponent<ClimbPoint>();
            if (currentPoint == null && dropLedgeFound)
            {
                Debug.Log("Iterating through climbpoints of " + dropLedgeHit.transform);
                if (dropLedgeHit.transform.GetComponent<ClimbPointContainer>() != null)
                {
                    currentPoint = dropLedgeHit.transform.GetComponent<ClimbPointContainer>().GetClimbPoint(dropLedgeHit.point);
                    Debug.Log("Best climbpoint Found : " + currentPoint.transform);
                }
            }
            if (currentPoint == null)
            {
                return;
            }
            playerController.SetControl(false);
            StartCoroutine(JumpToLedge("DropToLedge", currentPoint.transform, 0.3f, 0.45f, AvatarTarget.RightHand, new Vector3(0f, -0.25f, 0.02f)));
            // transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            if (currentPoint == null) return;
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            var inputDir = new Vector3(-h, v, 0).normalized;

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                StartCoroutine(DropActions("HangToDrop"));
                currentPoint = null;
                return;
            }
            if (inputDir.sqrMagnitude <= 0.1f)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(DropActions("JumpFromHang"));
                    currentPoint = null;
                    return;
                }
                return;
            }
            if (currentPoint.MountPoint && inputDir.y > 0.5f && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(HangToClimb());
                currentPoint = null;
                return;
            }
            if (currentPoint.GetNeighbour(inputDir, maxAngleAllowed) == null) return;
            var nextPoint = currentPoint.GetNeighbour(inputDir, maxAngleAllowed);
            if (nextPoint != null)
            {
                if (nextPoint.connectionType == ConnectionType.Jump && Input.GetKeyDown(KeyCode.Space))
                {
                    currentPoint = nextPoint.point;
                    if (Mathf.Abs(nextPoint.direction.y) <= 0.33f)
                    {
                        if (nextPoint.direction.x > 0) // +x of object there is left of object
                        {
                            StartCoroutine(JumpToLedge("HopLeft", currentPoint.transform, 0.2f, 0.43f, AvatarTarget.RightHand, new Vector3(0.01f, -0.05f, 0.125f)));
                        }
                        else
                        {
                            StartCoroutine(JumpToLedge("HopRight", currentPoint.transform, 0.2f, 0.43f, AvatarTarget.LeftHand, new Vector3(0.01f, -0.05f, -0.625f)));
                        }
                    }
                    else
                    {
                        if (nextPoint.direction.y > 0)
                        {
                            StartCoroutine(JumpToLedge("HopUp", currentPoint.transform, 0.34f, 0.7f, AvatarTarget.RightHand, new Vector3(0f, 0f, 0f)));
                        }
                        else
                        {
                            StartCoroutine(JumpToLedge("HopDown", currentPoint.transform, 0.26f, 0.58f, AvatarTarget.LeftHand, new Vector3(0f, -0.07f, -0.60f)));
                        }
                    }
                }
                else if (nextPoint.connectionType == ConnectionType.Move)
                {
                    currentPoint = nextPoint.point;
                    if (nextPoint.direction.x < 0)
                    {
                        StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.4f, AvatarTarget.LeftHand, new Vector3(0.03f, -0.04f, -1.11f)));
                    }
                    else if (nextPoint.direction.x > 0)
                    {
                        StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.4f, AvatarTarget.RightHand, new Vector3(0.03f, -0.04f, 0.54f)));
                    }
                }
            }
        }
    }
    public IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand = AvatarTarget.RightHand, Vector3? Offset = null)
    {
        Vector3 offset = Offset ?? Vector3.zero;
        var matchTargetParameters = new PlayerController.MatchTargetParameters()
        {
            pos = ledge.position + ledge.forward * 0.05f - ledge.up * 0.07f - ledge.right * 0.275f,
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };
        if (offset != Vector3.zero)
        {
            matchTargetParameters.pos += ledge.forward * offset.x - ledge.up * offset.y - ledge.right * offset.z;
        }
        playerController.isHanging = true;
        yield return playerController.DoAction(anim, Quaternion.LookRotation(-ledge.forward), matchTargetParameters, true);
        playerController.freeRun = false;
        playerController.isCrouched = false;
    }
    IEnumerator DropActions(string animName)
    {

        playerController.isHanging = false;
        yield return playerController.DoAction(animName);
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }
    IEnumerator HangToClimb()
    {
        playerController.isHanging = false;
        yield return playerController.DoAction("HangToClimb");
        playerController.GetComponent<CharacterController>().enabled = true;
        playerController.ResetTargetRotation();
        yield return new WaitForSeconds(0.5f);
        playerController.SetControl(true);
    }
}
