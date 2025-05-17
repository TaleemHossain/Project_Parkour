using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float forwardRayLength = 1f;
    [SerializeField] LayerMask obstacleLayerMask;
    [SerializeField] float heightRayLength = 4f;
    [SerializeField] float ledgeCheckRayOffset = 1f;
    [SerializeField] float ledgeCheckRayLength = 10f;
    [SerializeField] float minimumHeightForLedge = 0.75f;
    public struct ObstackleHitData
    {
        public bool forwardHitFound;
        public bool heightHitFound;
        public RaycastHit forwardHitInfo;
        public RaycastHit heightHitInfo;
    }

    public ObstackleHitData ObstackleCheck()
    {
        var hitData = new ObstackleHitData();
        var ForwardRayOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(ForwardRayOrigin, transform.forward, out hitData.forwardHitInfo, forwardRayLength, obstacleLayerMask);
        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHitInfo.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHitInfo, heightRayLength, obstacleLayerMask);
        }
        return hitData;
    }
    public bool LedgeCheck(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero)
        {
            // return false;
            var origin = transform.position + moveDir * ledgeCheckRayOffset + Vector3.up;
            Debug.DrawRay(origin, Vector3.down * ledgeCheckRayLength, Color.green);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeCheckRayLength, obstacleLayerMask))
            {
                float height = transform.position.y - hit.point.y;
                if (height > minimumHeightForLedge)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            var origin = transform.position + moveDir * ledgeCheckRayOffset + Vector3.up;
            Debug.DrawRay(origin, Vector3.down * ledgeCheckRayLength, Color.green);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeCheckRayLength, obstacleLayerMask))
            {
                float height = transform.position.y - hit.point.y;
                if (height > minimumHeightForLedge)
                {
                    return true;
                }
            }
            return false;
        }
    } 
}
