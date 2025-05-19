using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Obstacle Scanning")]
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float forwardRayLength = 0.5f;
    [SerializeField] LayerMask obstacleLayerMask;
    [SerializeField] float heightRayLength = 5f;
    [Header("Barrier Scanning")]
    [SerializeField] Vector3 barrierRayOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] float barrierforwardRayLength = 1f;
    [SerializeField] float barrierRayLength = 2f;
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
    public struct BarrierHitData
    {
        public bool forwardHitFound;
        public bool heightHitFound;
        public RaycastHit forwardHitInfo;
        public RaycastHit heightHitInfo;
    }
    public struct RoofHitData
    {
        public bool HitFound;
        public RaycastHit HitInfo;
        public bool IsThereShortRoof;
    }
    public struct LedgeData
    {
        public float height;
        public float angle;
        public RaycastHit ledgeHitInfo;
        public RaycastHit surfaceHitInfo;
        public bool isCorner;
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
    public BarrierHitData BarrierCheck()
    {
        var hitData = new BarrierHitData();
        var ForwardRayOrigin = transform.position + barrierRayOffset;
        hitData.forwardHitFound = Physics.Raycast(ForwardRayOrigin, transform.forward, out hitData.forwardHitInfo, barrierforwardRayLength, obstacleLayerMask);
        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHitInfo.point - Vector3.up * barrierRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.up, out hitData.heightHitInfo, heightRayLength, obstacleLayerMask);
        }
        return hitData;
    }
    public RoofHitData RoofCheck()
    {
        var hitData = new RoofHitData();
        hitData.IsThereShortRoof = false;
        hitData.HitFound = Physics.Raycast(transform.position, transform.up, out hitData.HitInfo, 2f, obstacleLayerMask);
        if (hitData.HitFound)
        {
            float heightOfHit = hitData.HitInfo.point.y - transform.position.y;
            Debug.Log("Height = " + heightOfHit);
            if (heightOfHit <= 1.75)
            {
                hitData.IsThereShortRoof = true;
            }
            else
            {
                hitData.IsThereShortRoof = false;
            }
        }
        else
        {
            hitData.IsThereShortRoof = false;
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
            bool LedgeCheck = Physics.Raycast(origin, Vector3.down, out RaycastHit ledgehit, ledgeCheckRayLength, obstacleLayerMask);
            if (PhysicsUtil.ThreeRayCasts(origin, Vector3.down, ledgeCheckRayOffset, transform, out List<RaycastHit> hits, ledgeCheckRayLength, obstacleLayerMask))
            {
                ledgeData.ledgeHitInfo = ledgehit;
                var SurfaceRayOrigin = transform.position + moveDir - new Vector3(0, 0.1f, 0);
                if (Physics.Raycast(SurfaceRayOrigin, -moveDir, out RaycastHit surfaceHit, 2f, obstacleLayerMask))
                {
                    float height = transform.position.y - ledgehit.point.y;
                    if (height > minimumHeightForLedge)
                    {
                        ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                        ledgeData.height = height;
                        ledgeData.surfaceHitInfo = surfaceHit;
                        float sideRayDistance = ledgeCheckRayOffset;
                        Vector3 perpendicularDir = new Vector3(-moveDir.z, 0, moveDir.x).normalized;
                        Vector3 leftOrigin = transform.position - perpendicularDir * sideRayDistance + Vector3.up;
                        Vector3 rightOrigin = transform.position + perpendicularDir * sideRayDistance + Vector3.up;
                        bool leftHit = Physics.Raycast(leftOrigin, Vector3.down, out RaycastHit leftHitInfo, ledgeCheckRayLength, obstacleLayerMask);
                        bool rightHit = Physics.Raycast(rightOrigin, Vector3.down, out RaycastHit rightHitInfo, ledgeCheckRayLength, obstacleLayerMask);
                        ledgeData.isCorner = ((leftHit && transform.position.y - leftHitInfo.point.y > minimumHeightForLedge) ||
                                             (rightHit && transform.position.y - rightHitInfo.point.y > minimumHeightForLedge)) &&
                                             (LedgeCheck && transform.position.y - ledgehit.point.y > minimumHeightForLedge);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
