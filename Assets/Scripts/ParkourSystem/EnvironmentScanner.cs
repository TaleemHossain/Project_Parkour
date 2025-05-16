using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float forwardRayLength = 1f;
    [SerializeField] LayerMask obstacleLayerMask;
    [SerializeField] float heightRayLength = 4f;
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
        // Debug.DrawRay(ForwardRayOrigin, transform.forward * forwardRayLength, hitData.forwardHitFound ? Color.red : Color.green);
        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHitInfo.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHitInfo, heightRayLength, obstacleLayerMask);
            // Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, hitData.heightHitFound ? Color.red : Color.green);
        }
        return hitData;
    }
}
