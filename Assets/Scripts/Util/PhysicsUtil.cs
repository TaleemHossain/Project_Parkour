using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    public static bool ThreeRayCasts(Vector3 origin, Vector3 direction, float spacing, Transform transform, out List<RaycastHit> hits, float distance, LayerMask layer)
    {
        bool centerHitFound = Physics.Raycast(origin, Vector3.down, out RaycastHit centerHit, distance, layer);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, Vector3.down, out RaycastHit leftHit, distance, layer);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, Vector3.down, out RaycastHit rightHit, distance, layer);
        hits = new List<RaycastHit>() { centerHit, leftHit, rightHit };
        bool hitFound = centerHitFound || leftHitFound || rightHitFound;
        return hitFound;
    }
}
