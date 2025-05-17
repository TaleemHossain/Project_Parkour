using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Obstacle Scanning")]
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float forwardRayLength = 1f;
    [SerializeField] LayerMask obstacleLayerMask;
    [SerializeField] float heightRayLength = 4f;
    [Header("Ledge Scanning")]
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
    public struct LedgeData
    {
        public float height;
        public float angle;
        public RaycastHit ledgeHitInfo;
        public RaycastHit surfaceHitInfo;
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
    public bool LedgeCheck(Vector3 moveDir, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();
        if (moveDir == Vector3.zero)
        {
            return false;
        }
        else
        {
            var origin = transform.position + moveDir * ledgeCheckRayOffset + Vector3.up;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeCheckRayLength, obstacleLayerMask))
            {
                ledgeData.ledgeHitInfo = hit;
                var SurfaceRayOrigin = transform.position + moveDir - new Vector3(0, 0.1f, 0);
                if (Physics.Raycast(SurfaceRayOrigin, -moveDir, out RaycastHit surfaceHit, 2f, obstacleLayerMask))
                {
                    float height = transform.position.y - hit.point.y;
                    if (height > minimumHeightForLedge)
                    {
                        ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                        ledgeData.height = height;
                        ledgeData.surfaceHitInfo = surfaceHit;
                        return true;
                    }
                }
            }
            return false;
        }
    } 
}
