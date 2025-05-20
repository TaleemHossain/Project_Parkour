using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    ClimbPoint currentPoint = null;
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
        if (parkourController.InAction) return;
        if (Input.GetKeyDown(KeyCode.Space) && environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit) && !playerController.isHanging)
        {
            currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
            if (currentPoint == null)
            {
                Debug.Log("Current point is null");
            }
            playerController.SetControl(false);
            StartCoroutine(JumpToLedge("IdleToHanging", ledgeHit.transform, 0.41f, 0.54f));
        }
        else
        {
            if (currentPoint == null) return;
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            var inputDir = new Vector3(-h, v, 0).normalized;

            if (currentPoint.GetNeighbour(inputDir, maxAngleAllowed) == null) return;
            var nextPoint = currentPoint.GetNeighbour(inputDir, maxAngleAllowed);
            if(nextPoint != null)
            {
                if (nextPoint.connectionType == ConnectionType.Jump && Input.GetKeyDown(KeyCode.Space))
                {
                    currentPoint = nextPoint.point;
                    if (Mathf.Abs(nextPoint.direction.y) <= 0.33f)
                    {
                        if (nextPoint.direction.x > 0) // +x of object there is left of object
                        {
                            StartCoroutine(JumpToLedge("HopLeft", currentPoint.transform, 0.2f, 0.43f));
                        }
                        else
                        {
                            StartCoroutine(JumpToLedge("HopRight", currentPoint.transform, 0.2f, 0.5f));
                        }
                    }
                    else
                    {
                        if (nextPoint.direction.y > 0)
                        {
                            StartCoroutine(JumpToLedge("HopUp", currentPoint.transform, 0.34f, 0.7f));
                        }
                        else
                        {
                            StartCoroutine(JumpToLedge("HopDown", currentPoint.transform, 0.4f, 0.69f));
                        }
                    }
                }
            }
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
